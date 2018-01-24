//
// author: Kazys Stepanas
//
#ifndef _3ESPACKETREADER_H_
#define _3ESPACKETREADER_H_

#include "3es-core.h"

#include "3espacketstream.h"

namespace tes
{
  /// A utility class for dealing with reading packets.
  ///
  /// @bug Use the payloadOffset in various calculations herein. It was added after this
  /// class was written, but is currently only supported as being zero, so it's not an issue
  /// yet.
  class _3es_coreAPI PacketReader : public PacketStream<const PacketHeader>
  {
  public:
    /// Creates a new packet reader for the given packet and its CRC.
    PacketReader(const PacketHeader &packet);

    /// Calculates the CRC value, returning true if it matches. This also sets
    /// @c isCrcValid() on success.
    ///
    /// Returns true immediately when @c isCrcValid() is already set.
    /// @return True if the CRC is valid.
    bool checkCrc();

    /// Caluclates the CRC for the packet.
    CrcType calculateCrc() const;

    /// Returns the number of bytes available for writing in the payload.
    /// @return The number of bytes available for writing.
    uint16_t bytesAvailable() const;

    /// Reads a single data element from the current position. This assumes that
    /// a single data element of size @p elementSize is being read and may require
    /// an endian swap to the current platform endian.
    ///
    /// The reader position is advanced by @p elementSize. Does not set the
    /// @c Fail bit on failure.
    ///
    /// @param bytes Location to read into.
    /// @param elementSize Size of the data item being read at @p bytes.
    /// @return @p elementSize on success, 0 otherwise.
    size_t readElement(uint8_t *bytes, size_t elementSize);

    /// Reads an array of data items from the current position. This makes the
    /// same assumptions as @c readElement() and performs an endian swap per
    /// array element. Elements in the array are assumed to be contiguous in
    /// both source and destination locations.
    ///
    /// Up to @p elementCount elements will be read depending on availability.
    /// Less may be read, but on success the number of bytes read will be
    /// a multiple of @p elementSize.
    ///
    /// The reader position is advanced by the number of bytes read.
    /// Does not set the @c Fail bit on failure.
    ///
    /// @param bytes Location to read into.
    /// @param elementSize Size of a single array element to read.
    /// @param elementCount The number of elements to attempt to read.
    /// @return On success returns the number of whole elements read.
    size_t readArray(uint8_t *bytes, size_t elementSize, size_t elementCount);

    /// Reads raw bytes from the packet at the current position up to @p byteCount.
    /// No endian swap is performed on the data read.
    ///
    /// The reader position is advanced by @p byteCount.
    /// Does not set the @c Fail bit on failure.
    ///
    /// @param bytes Location to read into.
    /// @aparam byteCount Number of bytes to read.
    /// @return The number of bytes read. This may be less than @p byteCount if there
    ///   are insufficient data available.
    size_t readRaw(uint8_t *bytes, size_t byteCount);

    /// Peek @p byteCount bytes from the current position in the buffer. This does not affect the stream position.
    /// @param dst The memory to write to.
    /// @param byteCount Number of bytes to read.
    /// @param allowByteSwap @c true to allow the byte ordering to be modified in @p dst. Only performed when
    ///   the network endian does not match the platform endian.
    /// @return The number of bytes read. Must match @p byteCount for success.
    size_t peek(uint8_t *dst, size_t byteCount, bool allowByteSwap = true);

    /// Reads a single data item from the packet. This reads a number of bytes
    /// equal to @c sizeof(T) performing an endian swap if necessary.
    /// @param[out] element Set to the data read.
    /// @return @c sizeof(T) on success, zero on failure.
    template <typename T>
    size_t readElement(T &element);

    template <typename T>
    size_t readArray(T *elements, size_t elementCount);

    template <typename T>
    PacketReader &operator >> (T &val);
  };

  inline uint16_t PacketReader::bytesAvailable() const
  {
    return _packet.payloadSize - _payloadPosition;
  }

  template <typename T>
  inline size_t PacketReader::readElement(T &element)
  {
    return readElement(reinterpret_cast<uint8_t *>(&element), sizeof(T));
  }

  template <typename T>
  inline size_t PacketReader::readArray(T *elements, size_t elementCount)
  {
    return readArray(reinterpret_cast<uint8_t *>(elements), sizeof(T), elementCount);
  }

  template <typename T>
  inline PacketReader &PacketReader::operator >> (T &val)
  {
    int read = readElement(val);
    _status |= !(read == sizeof(T)) * Fail;
    return *this;
  }
}

#endif // _3ESPACKETREADER_H_
