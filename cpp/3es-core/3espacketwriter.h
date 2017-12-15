//
// author: Kazys Stepanas
//
#ifndef _3ESPACKETWRITER_H_
#define _3ESPACKETWRITER_H_

#include "3es-core.h"

#include "3espacketstream.h"

namespace tes
{
  /// A utility class for writing payload data to a @c PacketHeader.
  ///
  /// This keeps the @c PacketHeader @c payloadSize member up to date and
  /// ensures the CRC is calculated, either via @c calculateCrc() explicitly
  /// on on destruction.
  ///
  /// The payload buffer size must be specified in the constructor, and data writes
  /// are limited by this value. The packet is assumed to be structured such that
  /// the packet header is located at the start of the buffer, followed immediately by
  /// space for the payload.
  ///
  /// Two construction options are available, one where the @c PacketHeader details are already
  /// initialised, except for the @c payloadSize and @c crc. The given packet is
  /// assumed to be the start of the data buffer. The second constructor accepts a raw
  /// byte pointer, which marks the start of the buffer, and the size of the buffer.
  /// The buffer size must be large enough for the @ PacketHeader. Remaining space is available
  /// for the payload.
  ///
  /// @bug Use the payloadOffset in various calculations herein. It was added after this
  /// class was written, but is currently only supported as being zero, so it's not an issue
  /// yet.
  class _3es_coreAPI PacketWriter : public PacketStream<PacketHeader>
  {
  public:
    /// Creates a @c PacketWriter to write to the given @p packet. This
    /// marks the start of the packet buffer.
    ///
    /// The packet members are initialised, but @c payloadSize and @c crc are
    /// left at zero to be calculated later. The @c routingId maybe given now
    /// or set with @c setRoutingId().
    ///
    /// @param packet The packet to write to.
    /// @param maxPayloadSize Specifies the space available for the payload (bytes).
    ///   This is in excess of the packet size, not the total buffer size.
    PacketWriter(PacketHeader &packet, uint16_t maxPayloadSize, uint16_t routingId = 0, uint16_t messageId = 0);

    /// Creates a @c PacketWriter to write to the given byte buffer.
    ///
    /// The buffer size must be at least @c sizeof(PacketHeader), larger if any
    /// payload is required. If not, then the @c isFail() will be true and
    /// all write operations will fail.
    ///
    /// The @c routingId maybe given now or set with @c setRoutingId().
    ///
    /// @param buffer The packet data buffer.
    /// @param bufferSize The total number of bytes available for the @c PacketHeader
    ///   and its paylaod. Must be at least @c sizeof(PacketHeader), or all writing
    ///   will fail.
    /// @param routingId Optionlly sets the @c routingId member of the packet.
    PacketWriter(uint8_t *buffer, uint16_t bufferSize, uint16_t routingId = 0, uint16_t messageId = 0);

    /// Copy constructor. Simple as neither writer owns the underlying memory.
    /// Both point to the same underlying memory, but only one should be used.
    /// @param other The packet to copy.
    PacketWriter(const PacketWriter &other);

    /// Destructor, ensuring the CRC is calculated.
    ~PacketWriter();

    /// Assignment operator. Simple as neither writer owns the underlying memory.
    /// Both point to the same underlying memory, but only one should be used.
    /// @param other The packet to copy.
    PacketWriter &operator = (const PacketWriter &other);

    /// Resets the packet, clearing out all variable data including the payload, crc and routing id.
    /// Allows preparation for writing new data to the same payload buffer.
    ///
    /// @param routingId Optional specification for the @c routingId after reset.
    void reset(uint16_t routingId, uint16_t messageId);

    /// @overload
    inline void reset() { reset(0, 0); }

    void setRoutingId(uint16_t routingId);
    PacketHeader &packet() const;

    const uint8_t *data() const;

    uint8_t *payload();

    inline void invalidateCrc() { _status &= ~CrcValid; }

    /// Returns the number of bytes remaining available in the payload.
    /// This is calculated as the @c maxPayloadSize() - @c payloadSize().
    /// @return Number of bytes remaining available for write.
    uint16_t bytesRemaining() const;

    /// Returns the size of the payload buffer. This is the maximum number of bytes
    /// which can be written to the payload.
    /// @return The payload buffer size (bytes).
    uint16_t maxPayloadSize() const;

    /// Finalises the packet for sending, calculating the CRC.
    /// @return True if the packet is valid and ready for sending.
    bool finalise();

    /// Calculates the CRC and writes it to the @c PacketHeader crc member.
    ///
    /// The current CRC value is returned when @c isCrcValid() is true.
    /// The CRC will not be calculate when @c isFail() is true and the
    /// result is undefined.
    /// @return The Calculated CRC, or undifined when @c isFail().
    CrcType calculateCrc();

    /// Writes a single data element from the current position. This assumes that
    /// a single data element of size @p elementSize is being write and may require
    /// an endian swap to the current platform endian.
    ///
    /// The writer position is advanced by @p elementSize. Does not set the
    /// @c Fail bit on failure.
    ///
    /// @param bytes Location to write from.
    /// @param elementSize Size of the data item being write at @p bytes.
    /// @return @p elementSize on success, 0 otherwise.
    size_t writeElement(const uint8_t *bytes, size_t elementSize);

    /// Writes an array of data items from the current position. This makes the
    /// same assumptions as @c writeElement() and performs an endian swap per
    /// array element. Elements in the array are assumed to be contiguous in
    /// both source and destination locations.
    ///
    /// The writer position is advanced by the number of bytes write.
    /// Does not set the @c Fail bit on failure.
    ///
    /// @param bytes Location to write from.
    /// @param elementSize Size of a single array element to write.
    /// @param elementCount The number of elements to attempt to write.
    /// @return On success returns the number of elements written, not bytes.
    size_t writeArray(const uint8_t *bytes, size_t elementSize, size_t elementCount);

    /// Writes raw bytes from the packet at the current position up to @p byteCount.
    /// No endian swap is performed on the data write.
    ///
    /// The writer position is advanced by @p byteCount.
    /// Does not set the @c Fail bit on failure.
    ///
    /// @param bytes Location to write into.
    /// @aparam byteCount Number of bytes to write.
    /// @return The number of bytes write. This may be less than @p byteCount if there
    ///   are insufficient data available.
    size_t writeRaw(const uint8_t *bytes, size_t byteCount);

    /// Writes a single data item from the packet. This writes a number of bytes
    /// equal to @c sizeof(T) performing an endian swap if necessary.
    /// @param[out] element Set to the data write.
    /// @return @c sizeof(T) on success, zero on failure.
    template <typename T>
    size_t writeElement(const T &element);

    template <typename T>
    size_t writeArray(const T *elements, size_t elementCount);

    template <typename T>
    PacketWriter &operator >> (T &val);

  protected:
    uint8_t *payloadWritePtr();
    void incrementPayloadSize(size_t inc);

    uint16_t _bufferSize;
  };

  inline void PacketWriter::setRoutingId(uint16_t routingId)
  {
    _packet.routingId = routingId;
  }

  inline PacketHeader &PacketWriter::packet() const
  {
    return _packet;
  }

  inline const uint8_t *PacketWriter::data() const
  {
    return reinterpret_cast<uint8_t*>(&_packet);
  }

  inline uint8_t *PacketWriter::payload()
  {
    return reinterpret_cast<uint8_t*>(&_packet) + sizeof(PacketHeader);
  }

  template <typename T>
  inline size_t PacketWriter::writeElement(const T &element)
  {
    return writeElement(reinterpret_cast<const uint8_t *>(&element), sizeof(T));
  }

  template <typename T>
  inline size_t PacketWriter::writeArray(const T *elements, size_t elementCount)
  {
    return writeArray(reinterpret_cast<const uint8_t *>(elements), sizeof(T), elementCount);
  }


  template <typename T>
  inline PacketWriter &PacketWriter::operator >> (T &val)
  {
    int written = writeElement(val);
    _status |= !(written == sizeof(T)) * Fail;
    return *this;
  }


  inline uint8_t *PacketWriter::payloadWritePtr()
  {
    return payload() + _payloadPosition;
  }
}

#endif // _3ESPACKETWRITER_H_
