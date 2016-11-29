//
// author: Kazys Stepanas
//
#ifndef _3ESPACKETBUFFER_H_
#define _3ESPACKETBUFFER_H_

#include "3es-core.h"

#include <cinttypes>

namespace tes
{
  struct PacketHeader;

  /// This class accepts responsibility for collating incoming byte streams.
  /// Data is buffered until full packets have arrived, which must be extracted
  /// using @c extractPacket().
  class _3es_coreAPI PacketBuffer
  {
  public:
    /// Constructors 2Kb buffer.
    PacketBuffer();
    /// Destructor.
    ~PacketBuffer();

    /// Adds @c bytes to the buffer.
    ///
    /// Data are rejected if the marker is not present or, if present,
    /// data before the marker are rejected.
    ///
    /// @return the index of the first accepted byte or -1 if all are rejected.
    int addBytes(const uint8_t *bytes, size_t byteCount);

    /// Extract the first valid packet in the buffer. Additional packets
    /// may be available.
    ///
    /// @return A valid packet pointer if available, null if none available.
    /// The caller must later release the packet calling @c releasePacket().
    PacketHeader *extractPacket();

    /// Releases the memory for the given packet.
    /// @param packet The packet to release.
    void releasePacket(PacketHeader *packet);

  private:
    /// Grow the packet buffer with @p bytes.
    /// @param bytes Data to append.
    /// @param byteCount Number of bytes from @p bytes to append.
    void appendData(const uint8_t *bytes, size_t byteCount);

    /// Remove the first @p byteCount bytes from the packet buffer.
    /// @param byteCount The number of bytes to remove from the buffer.
    void removeData(size_t byteCount);

    uint8_t *_packetBuffer; ///< Buffers incoming packet data.
    size_t _byteCount;  ///< Number of data bytes currently stored in @c _packetBuffer;
    size_t _bufferSize; ///< Size of @c _packetBuffer in bytes;
    bool _markerFound;  ///< Has the @c PacketHeader marker been found?
  };
}

#endif // _3ESPACKETBUFFER_H_
