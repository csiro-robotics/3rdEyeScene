//
// author: Kazys Stepanas
//
#ifndef _3ESHEADERSTREAM_H_
#define _3ESHEADERSTREAM_H_

#include "3es-core.h"

#include "3esendian.h"
#include "3espacketheader.h"

namespace tes
{
  /// A utility class used for managing read/write operations to a @c PacketHeader payload.
  ///
  /// The template type is intended to be either a @c PacketReader or a @c const @c PacketHeader
  /// for use with @c PacketWriter and @c PacketReader respectively.
  template <class HEADER>
  class PacketStream
  {
  public:
    /// Defies the packet CRC type.
    typedef uint16_t CrcType;

    /// Control values for seeking.
    enum SeekPos
    {
      Begin,    ///< Seek from the beginning of the stream.
      Current,  ///< Seek from the current position.
      End       ///< Seek from the end of the stream.
    };

    /// Status bits.
    enum Status
    {
      /// No issues.
      Ok = 0,
      /// End at of packet/stream.
      EOP = (1 << 0),
      /// Set after an operation fails.
      Fail = (1 << 1),
      /// Read only stream?
      ReadOnly = (1 << 2),
      /// Is the CRC valid?
      CrcValid = (1 << 3),
    };

    /// Create a stream to read from beginning at @p packet.
    /// @param packet The beginning of the data packet.
    PacketStream(HEADER &packet);

    // PacketHeader member access. Ensures network endian swap as required.
    /// Fetch the marker bytes in local endian.
    /// @return The @c PacketHeader::marker bytes.
    uint32_t marker() const { return networkEndianSwapValue(_packet.marker); }
    /// Fetch the major version bytes in local endian.
    /// @return The @c PacketHeader::versionMajor bytes.
    uint16_t versionMajor() const { return networkEndianSwapValue(_packet.versionMajor); }
    /// Fetch the minor version bytes in local endian.
    /// @return The @c PacketHeader::versionMinor bytes.
    uint16_t versionMinor() const { return networkEndianSwapValue(_packet.versionMinor); }
    /// Fetch the payload size bytes in local endian.
    /// @return The @c PacketHeader::payloadSize bytes.
    uint16_t payloadSize() const { return networkEndianSwapValue(_packet.payloadSize); }
    /// Returns the size of the packet plus payload, giving the full data packet size including the CRC.
    /// @return PacketHeader data size (bytes).
    uint16_t packetSize() const { return sizeof(HEADER) + payloadSize() + (((packet().flags & PF_NoCrc) == 0) ? sizeof(CrcType) : 0); }
    /// Fetch the routing ID bytes in local endian.
    /// @return The @c PacketHeader::routingId bytes.
    uint16_t routingId() const { return networkEndianSwapValue(_packet.routingId); }
    /// Fetch the message ID bytes in local endian.
    /// @return The @c PacketHeader::messageId bytes.
    uint16_t messageId() const { return networkEndianSwapValue(_packet.messageId); }
    /// Fetch the CRC bytes in local endian.
    /// Invalid for packets with the @c PF_NoCrc flag set.
    /// @return The packet's CRC value.
    CrcType crc() const { return networkEndianSwapValue(*crcPtr()); }
    /// Fetch a pointer to the CRC bytes.
    /// Invalid for packets with the @c PF_NoCrc flag set.
    /// @return A pointer to the CRC location.
    CrcType *crcPtr();
    /// @overload
    const CrcType *crcPtr() const;

    /// Report the @c Status bits.
    /// @return The @c Status flags.
    inline uint16_t status() const;

    /// At end of packet/stream?
    /// @return True if at end of packet.
    inline bool isEop() const { return _status & EOP; }
    /// Status OK?
    /// @return True if OK
    inline bool isOk() const { return !isFail(); }
    /// Fail bit set?
    /// @return True if fail bit is set.
    inline bool isFail() const { return (_status & Fail) != 0; }
    /// Read only stream?
    /// @return True if read only.
    inline bool isReadOnly() const { return (_status & ReadOnly) != 0; }
    /// CRC validated?
    /// @return True if CRC has been validated.
    inline bool isCrcValid() const { return (_status & CrcValid) != 0; }

    /// Access the head of the packet buffer, for direct @p PacketHeader access.
    /// Note: values are in network Endian.
    /// @return A reference to the @c PacketHeader.
    HEADER &packet() const;

    /// Tell the current stream position.
    /// @return The current position.
    uint16_t tell() const;
    /// Seek to the indicated position.
    /// @param offset Seek offset from @p pos.
    /// @param pos The seek reference position.
    bool seek(int offset, SeekPos pos = Begin);
    /// Direct payload pointer access.
    /// @return The start of the payload bytes.
    const uint8_t *payload() const;

  protected:
    HEADER &_packet;    ///< Packet header and buffer start address.
    uint16_t _status;   ///< @c Status bits.
    uint16_t _payloadPosition;  ///< Payload cursor.

    /// Type traits: is @c T const?
    template <class T>
    struct IsConst
    {
      /// Check the traits.
      /// @return True if @p T is const.
      inline bool check() const { return false; }
    };

    /// Type traits: is @c T const?
    template <class T>
    struct IsConst<const T>
    {
      /// Check the traits.
      /// @return True if @p T is const.
      inline bool check() const { return true; }
    };
  };

  template class _3es_coreAPI PacketStream<PacketHeader>;
  template class _3es_coreAPI PacketStream<const PacketHeader>;

  template <class HEADER>
  PacketStream<HEADER>::PacketStream(HEADER &packet)
    : _packet(packet)
    , _status(Ok)
    , _payloadPosition(0u)
  {
    if (IsConst<HEADER>().check())
    {
      _status |= ReadOnly;
    }
  }


  template <class HEADER>
  bool PacketStream<HEADER>::seek(int offset, SeekPos pos)
  {
    switch (pos)
    {
    case Begin:
      if (offset <= _packet.payloadSize)
      {
        _payloadPosition = offset;
        return true;
      }
      break;

    case Current:
      if (offset >= 0 && offset + _payloadPosition <= _packet.payloadSize ||
        offset < 0 && _payloadPosition >= -offset)
      {
        _payloadPosition += offset;
        return true;
      }
      break;

    case End:
      if (offset < _packet.payloadSize)
      {
        _payloadPosition = _packet.payloadSize - 1 - offset;
        return true;
      }
      break;

    default:
      break;
    }

    return false;
  }


  template <class HEADER>
  typename PacketStream<HEADER>::CrcType *PacketStream<HEADER>::crcPtr()
  {
    // CRC appears after the payload.
    // TODO: fix the const correctness of this.
    uint8_t *pos = const_cast<uint8_t*>(reinterpret_cast<const uint8_t*>(&_packet)) + sizeof(HEADER) + payloadSize();
    return reinterpret_cast<CrcType*>(pos);
  }


  template <class HEADER>
  const typename PacketStream<HEADER>::CrcType *PacketStream<HEADER>::crcPtr() const
  {
    // CRC appears after the payload.
    const uint8_t *pos = reinterpret_cast<const uint8_t*>(&_packet) + sizeof(HEADER) + payloadSize();
    return reinterpret_cast<const CrcType*>(pos);
  }


  template <class HEADER>
  inline uint16_t PacketStream<HEADER>::status() const
  {
    return _status;
  }

  template <class HEADER>
  inline HEADER &PacketStream<HEADER>::packet() const
  {
    return _packet;
  }

  template <class HEADER>
  inline uint16_t PacketStream<HEADER>::tell() const
  {
    return _payloadPosition;
  }

  template <class HEADER>
  inline const uint8_t *PacketStream<HEADER>::payload() const
  {
    return reinterpret_cast<const uint8_t*>(&_packet) + sizeof(HEADER);
  }
}

#endif // _3ESHEADERSTREAM_H_
