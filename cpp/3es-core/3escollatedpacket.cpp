//
// Author Kazys Stepanas
#include "3escollatedpacket.h"

#include "3escrc.h"
#include "3esendian.h"
#include "3esmaths.h"
#include "3esmessages.h"

#include "shapes/3esshape.h"

#ifdef TES_ZLIB
#include <zlib.h>
#endif // TES_ZLIB

#include <algorithm>
#include <cstring>

using namespace tes;

namespace tes
{
  struct CollatedPacketZip
  {
#ifdef TES_ZLIB
    /// ZLib stream.
    z_stream stream;

    CollatedPacketZip()
    {
      memset(&stream, 0, sizeof(stream));
    }

    ~CollatedPacketZip()
    {
      // Ensure clean up
      if (stream.total_out)
      {
        deflate(&stream, Z_FINISH);
        deflateEnd(&stream);
      }
    }
#else  // TES_ZLIB
#endif // TES_ZLIB
  };
}

const size_t CollatedPacket::Overhead = sizeof(PacketHeader) + sizeof(CollatedPacketMessage) + sizeof(PacketWriter::CrcType);
const unsigned CollatedPacket::InitialCursorOffset = sizeof(PacketHeader) + sizeof(CollatedPacketMessage);
const uint16_t CollatedPacket::MaxPacketSize = (uint16_t)~0u;


CollatedPacket::CollatedPacket(bool compress, uint16_t bufferSize)
  : _zip(nullptr)
  , _buffer(nullptr)
  , _bufferSize(0)
  , _cursor(0)
  , _maxPacketSize(0)
  , _finalised(false)
  , _active(true)
{
  init(compress, bufferSize, MaxPacketSize);
}


CollatedPacket::CollatedPacket(unsigned bufferSize, unsigned maxPacketSize)
{
  init(false, bufferSize, maxPacketSize);
}


CollatedPacket::~CollatedPacket()
{
  delete[] _buffer;
  delete _zip;
}


void CollatedPacket::reset()
{
#ifdef TES_ZLIB
  // Call finalise to ensure compression buffers are flushed.
  if (!_finalised)
  {
    finalise();
  }
  if (_zip)
  {
    _zip->stream.zalloc = nullptr;
    _zip->stream.zfree = nullptr;
    _zip->stream.opaque = nullptr;
  }
#endif // TES_ZLIB
  _header->payloadSize = 0;
  _cursor = InitialCursorOffset;
  _finalised = false;
}


int CollatedPacket::add(const PacketWriter & packet)
{
  if (!_active)
  {
    return 0;
  }

  const uint8_t *packetBuffer = reinterpret_cast<const uint8_t *>(&packet.packet());
  uint16_t packetBytes = packet.packetSize();
  return add(packetBuffer, packetBytes);
}


int CollatedPacket::add(const uint8_t *buffer, uint16_t bufferSize)
{
  if (!_active)
  {
    return 0;
  }

  if (bufferSize <= 0)
  {
    return 0;
  }

  if (_finalised)
  {
    return -1;
  }

  // Check total size capacity.
  if (collatedBytes() + bufferSize + Overhead > _maxPacketSize)
  {
    // Too many bytes to collate.
    return -1;
  }

  if (_bufferSize < collatedBytes() + bufferSize + Overhead)
  {
    // Buffer too small.
    expand(_bufferSize);
  }

#ifdef TES_ZLIB
  if (_zip)
  {
    if (collatedBytes() == 0)
    {
      // Starting compression of a new block.
      // TODO: Investigate best compression level for speed/size compromise.
      const int windowBits = 15;
      const int GZipEncoding = 16;
      deflateInit2(&_zip->stream, Z_DEFAULT_COMPRESSION, Z_DEFLATED, windowBits | GZipEncoding, 8, Z_DEFAULT_STRATEGY);
      _zip->stream.next_out = (Bytef *)_buffer + InitialCursorOffset;
      _zip->stream.avail_out = (uInt)(_bufferSize - Overhead);
    }

    int zipRet;
    _zip->stream.avail_in = bufferSize;
    _zip->stream.next_in = (Bytef *)buffer;

    zipRet = deflate(&_zip->stream, Z_NO_FLUSH);
    if (_zip->stream.avail_in)
    {
      // Failed to write all incoming data. Output buffer not large enough. It should have been.
      // We can't do anything here.
      // TODO: consider resetting the output buffer to the previous state.
      // Need to know more about ZLib and see if we can do a simple cursor reset, then flush.
      return -1;
    }

    // Reflect the number of uncompressed bytes.
    _cursor += bufferSize;
  }
  else
#endif // TES_ZIP
  {
    memcpy(_buffer + _cursor, buffer, bufferSize);
    _cursor += bufferSize;
  }

  return bufferSize;
}


bool CollatedPacket::finalise()
{
  if (!_active)
  {
    return 0;
  }

  if (_finalised)
  {
    return false;
  }

  // Finalise compression.
  // Update the number of uncompressed bytes in the message.
  _message->uncompressedBytes = collatedBytes();
  networkEndianSwap(_message->uncompressedBytes);
#ifdef TES_ZLIB
  if (_zip && collatedBytes())
  {
    // Compressed data available. Flush the stream.
    _zip->stream.avail_in = 0;
    _zip->stream.next_in = nullptr;
    deflate(&_zip->stream, Z_FINISH);
    deflateEnd(&_zip->stream);
    // Update _cursor to reflect the number of bytes to write.
    _cursor = _zip->stream.total_out + InitialCursorOffset;
    _zip->stream.total_out = 0;
  }
#endif // TES_ZLIB

  // Set the packet buffer size.
  _header->payloadSize = collatedBytes() + (uint16_t)sizeof(CollatedPacketMessage);
  networkEndianSwap(_header->payloadSize);
  // Calculate the CRC
  PacketWriter::CrcType *crcPtr = reinterpret_cast<PacketWriter::CrcType *>(_buffer + _cursor);
  *crcPtr = crc16(_buffer, _cursor);
  networkEndianSwap(*crcPtr);
  _cursor += sizeof(*crcPtr);
  _finalised = true;
  return true;
}


const uint8_t *CollatedPacket::buffer(unsigned &byteCount) const
{
  byteCount = _cursor;
  return _buffer;
}


//-----------------------------------------------------------------------------
// Connection methods.
//-----------------------------------------------------------------------------
void CollatedPacket::close()
{
  // Not supported.
}


void CollatedPacket::setActive(bool enable)
{
  _active = enable;
}


bool CollatedPacket::active() const
{
  return _active;
}


const char *CollatedPacket::address() const
{
  return "CollatedPacket";
}


uint16_t CollatedPacket::port() const
{
  return 0;
}


bool CollatedPacket::isConnected() const
{
  return true;
}


int CollatedPacket::create(const Shape &shape)
{
  if (!_active)
  {
    return 0;
  }

  // Start by trying to write directly into the packet.
  int written = 0;
  bool wroteMessage = false; // Create message written?
  bool expanded = false;
  unsigned initialCursor = _cursor;

  PacketWriter writer(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
  // Keep trying to write the packet while we don't have a fatal error.
  // Supports resizing the buffer.
  while (wroteMessage && written != -1)
  {
    wroteMessage = shape.writeCreate(writer);
    if (wroteMessage)
    {
      if (writer.finalise())
      {
        _cursor += writer.packetSize();
        written += writer.packetSize();
      }
      else
      {
        written = -1;
      }
    }
    else if (!expanded)
    {
      // Try resize.
      expand(1024u);
      expanded = true;
      writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
    }
    else
    {
      written = -1;
    }
  }

  if (wroteMessage && shape.isComplex())
  {
    // More to write. Support buffer expansion.
    bool complete = false;
    unsigned progress = 0;
    int res = 0;

    while (!complete && written != -1)
    {
      writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
      res = shape.writeData(writer, progress);

      if (res >= 0)
      {
        // Good write.
        if (writer.finalise())
        {
          // Good finalise.
          _cursor += writer.packetSize();
          written += writer.packetSize();
          writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
        }
        else
        {
          // Failed to finalise.
          written = -1;
        }

        complete = res == 0;
      }
      else
      {
        // Failed to write. Try resize.
        if (_bufferSize < maxPacketSize())
        {
          expand(1024u);
          writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
        }
        else
        {
          // Can't expand any more. Abort.
          written = -1;
        }
      }
    }
  }

  // Reset on error.
  if (written == -1)
  {
    _cursor = initialCursor;
  }

  return written;
}


int CollatedPacket::destroy(const Shape &shape)
{
  if (!_active)
  {
    return 0;
  }

  // Start by trying to write directly into the packet.
  int written = 0;
  bool wroteMessage = false; // Create message written?
  bool expanded = false;
  unsigned initialCursor = _cursor;

  PacketWriter writer(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
  // Keep trying to write the packet while we don't have a fatal error.
  // Supports resizing the buffer.
  while (wroteMessage && written != -1)
  {
    wroteMessage = shape.writeDestroy(writer);
    if (wroteMessage)
    {
      if (writer.finalise())
      {
        _cursor += writer.packetSize();
        written += writer.packetSize();
      }
      else
      {
        written = -1;
      }
    }
    else if (!expanded)
    {
      // Try resize.
      expand(1024u);
      expanded = true;
      writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
    }
    else
    {
      written = -1;
    }
  }

  // Reset on error.
  if (written == -1)
  {
    _cursor = initialCursor;
  }

  return written;
}


int CollatedPacket::update(const Shape &shape)
{
  if (!_active)
  {
    return 0;
  }

  // Start by trying to write directly into the packet.
  int written = 0;
  bool wroteMessage = false; // Create message written?
  bool expanded = false;
  unsigned initialCursor = _cursor;

  PacketWriter writer(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
  // Keep trying to write the packet while we don't have a fatal error.
  // Supports resizing the buffer.
  while (wroteMessage && written != -1)
  {
    wroteMessage = shape.writeUpdate(writer);
    if (wroteMessage)
    {
      if (writer.finalise())
      {
        _cursor += writer.packetSize();
        written += writer.packetSize();
      }
      else
      {
        written = -1;
      }
    }
    else if (!expanded)
    {
      // Try resize.
      expand(1024u);
      expanded = true;
      writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
    }
    else
    {
      written = -1;
    }
  }

  // Reset on error.
  if (written == -1)
  {
    _cursor = initialCursor;
  }

  return written;
}


int CollatedPacket::updateTransfers(unsigned /*byteLimit*/)
{
  return -1;
}


int CollatedPacket::updateFrame(float dt, bool flush)
{
  // Not supported
  return -1;
}


unsigned tes::CollatedPacket::referenceResource(const Resource *)
{
  return 0;
}


unsigned tes::CollatedPacket::releaseResource(const Resource *)
{
  return 0;
}


bool CollatedPacket::sendServerInfo(const ServerInfoMessage &info)
{
  if (!_active)
  {
    return false;
  }

  // Start by trying to write directly into the packet.
  int written = 0;
  bool wroteMessage = false; // Create message written?
  bool expanded = false;
  unsigned initialCursor = _cursor;

  PacketWriter writer(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
  writer.reset(MtServerInfo, 0);
  // Keep trying to write the packet while we don't have a fatal error.
  // Supports resizing the buffer.
  while (wroteMessage && written != -1)
  {
    wroteMessage = info.write(writer);
    if (wroteMessage)
    {
      if (writer.finalise())
      {
        _cursor += writer.packetSize();
        written += writer.packetSize();
      }
      else
      {
        written = -1;
      }
    }
    else if (!expanded)
    {
      // Try resize.
      expand(1024u);
      expanded = true;
      writer = PacketWriter(_buffer + _cursor, (uint16_t)std::min<size_t>(_bufferSize - _cursor - sizeof(PacketWriter::CrcType), 0xffffu));
    }
    else
    {
      written = -1;
    }
  }

  // Reset on error.
  if (written == -1)
  {
    _cursor = initialCursor;
  }

  return written != -1;
}


int CollatedPacket::send(const uint8_t *data, int byteCount)
{
  if (!_active)
  {
    return 0;
  }

  return add(data, byteCount);
}


//-----------------------------------------------------------------------------
// Private
//-----------------------------------------------------------------------------
void CollatedPacket::init(bool compress, unsigned bufferSize, unsigned maxPacketSize)
{
  _cursor = InitialCursorOffset;
  if (bufferSize == 0)
  {
    bufferSize = 1024;
  }
  _buffer = new uint8_t[bufferSize];
  _bufferSize = bufferSize;
  _maxPacketSize = maxPacketSize;
  _header = reinterpret_cast<PacketHeader *>(_buffer);
  memset(_header, 0, sizeof(PacketHeader));
  _message = reinterpret_cast<CollatedPacketMessage *>(_buffer + sizeof(PacketHeader));
  memset(_message, 0, sizeof(CollatedPacketMessage));
  // Keep header in network byte order.
  _header->marker = networkEndianSwapValue(PacketMarker);
  _header->versionMajor = networkEndianSwapValue(PacketVersionMajor);
  _header->versionMinor = networkEndianSwapValue(PacketVersionMinor);
  _header->routingId = MtCollatedPacket;
  networkEndianSwap(_header->routingId);
  _header->messageId = 0;

#ifdef TES_ZLIB
  if (compress)
  {
    _zip = new CollatedPacketZip;
    _message->flags |= CPFCompress;
    networkEndianSwap(_message->flags);
  }
#endif // TES_ZLIB
}


void CollatedPacket::expand(unsigned expandBy)
{
  // Buffer too small.
  unsigned newBufferSize = std::min(nextLog2(collatedBytes() + expandBy + Overhead), maxPacketSize());
  uint8_t *newBuffer = new uint8_t[newBufferSize];
#ifdef TES_ZLIB
  size_t zipOffset = InitialCursorOffset;
#endif // TES_ZLIB
  if (_buffer && collatedBytes())
  {
    memcpy(newBuffer, _buffer, _cursor);
#ifdef TES_ZLIB
    if (_zip)
    {
      zipOffset = _zip->stream.next_out - _buffer;
    }
#endif // TES_ZLIB
  }
  delete[] _buffer;
  _buffer = newBuffer;
  _bufferSize = newBufferSize;
  _header = reinterpret_cast<PacketHeader *>(_buffer);
  _message = reinterpret_cast<CollatedPacketMessage *>(_buffer + sizeof(PacketHeader));
#ifdef TES_ZLIB
  if (_zip)
  {
    _zip->stream.next_out = _buffer + zipOffset;
    _zip->stream.avail_out = (uInt)(newBufferSize - zipOffset - (Overhead - InitialCursorOffset));
  }
#endif // TES_ZLIB
}
