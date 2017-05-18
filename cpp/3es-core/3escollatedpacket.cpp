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

namespace
{
  void writeMessageHeader(uint8_t *buffer, unsigned uncompressedSize, unsigned payloadSize, bool compressed)
  {
    PacketHeader *header = reinterpret_cast<PacketHeader *>(buffer);
    memset(header, 0, sizeof(PacketHeader));
    CollatedPacketMessage *message = reinterpret_cast<CollatedPacketMessage *>(buffer + sizeof(PacketHeader));
    memset(message, 0, sizeof(CollatedPacketMessage));

    // Keep header in network byte order.
    header->marker = networkEndianSwapValue(PacketMarker);
    header->versionMajor = networkEndianSwapValue(PacketVersionMajor);
    header->versionMinor = networkEndianSwapValue(PacketVersionMinor);
    header->routingId = MtCollatedPacket;
    networkEndianSwap(header->routingId);
    header->messageId = 0;
    header->payloadSize = payloadSize + sizeof(CollatedPacketMessage);
    networkEndianSwap(header->payloadSize);
    header->payloadOffset = 0;
    header->flags = 0;

    message->flags = (compressed) ? CPFCompress : 0;
    networkEndianSwap(message->flags);
    message->reserved = 0;
    message->uncompressedBytes = uncompressedSize;
    networkEndianSwap(message->uncompressedBytes);
  }
}

const size_t CollatedPacket::Overhead = sizeof(PacketHeader) + sizeof(CollatedPacketMessage) + sizeof(PacketWriter::CrcType);
const unsigned CollatedPacket::InitialCursorOffset = sizeof(PacketHeader) + sizeof(CollatedPacketMessage);
const uint16_t CollatedPacket::MaxPacketSize = (uint16_t)~0u;


CollatedPacket::CollatedPacket(bool compress, uint16_t bufferSize)
  : _zip(nullptr)
  , _buffer(nullptr)
  , _bufferSize(0)
  , _finalBuffer(nullptr)
  , _finalBufferSize(0)
  , _finalPacketCursor(0)
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
  delete[] _finalBuffer;
  delete _zip;
}


void CollatedPacket::reset()
{
  _cursor = _finalPacketCursor = 0;
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


int CollatedPacket::add(const uint8_t *buffer, uint16_t byteCount)
{
  if (!_active)
  {
    return 0;
  }

  if (byteCount <= 0)
  {
    return 0;
  }

  if (_finalised)
  {
    return -1;
  }

  // Check total size capacity.
  if (collatedBytes() + byteCount + Overhead > _maxPacketSize)
  {
    // Too many bytes to collate.
    return -1;
  }

  if (_bufferSize < collatedBytes() + byteCount + Overhead)
  {
    // Buffer too small.
    expand(_bufferSize, _buffer, _bufferSize, _cursor, _maxPacketSize);
  }

  memcpy(_buffer + _cursor, buffer, byteCount);
  _cursor += byteCount;

  return byteCount;
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

  if (collatedBytes() == 0)
  {
    _finalPacketCursor = 0;
    _finalised = true;
    return true;
  }

  if (_finalBufferSize < _bufferSize + Overhead)
  {
    expand(_bufferSize + Overhead - _finalBufferSize, _finalBuffer, _finalBufferSize, 0, _maxPacketSize);
  }

  // Finalise the packet. If possible, we try compress the buffer. If that is smaller then we use
  // the compressed result. Otherwise we use compressed data.
  bool compressedData = false;
#ifdef TES_ZLIB
  if (compressionEnabled() && collatedBytes())
  {
    unsigned compressedBytes = 0;

    const int windowBits = 15;
    const int GZipEncoding = 16;
    //Z_BEST_COMPRESSION
    // params: stream,level, method, window bits, memLevel, strategy
    deflateInit2(&_zip->stream, Z_BEST_COMPRESSION, Z_DEFLATED, windowBits | GZipEncoding, 8, Z_DEFAULT_STRATEGY);
    _zip->stream.next_out = (Bytef *)_finalBuffer + InitialCursorOffset;
    _zip->stream.avail_out = (uInt)(_finalBufferSize - Overhead);

    int zipRet;
    _zip->stream.avail_in = collatedBytes();
    _zip->stream.next_in = (Bytef *)_buffer;
    zipRet = deflate(&_zip->stream, Z_FINISH);
    deflateEnd(&_zip->stream);

    if (zipRet == Z_STREAM_END)
    {
      // Compressed ok. Check size.
      // Update _cursor to reflect the number of bytes to write.
      compressedBytes = _zip->stream.total_out;
      _zip->stream.total_out = 0;

      if (compressedBytes < collatedBytes())
      {
        // Compression is good. Smaller than uncompressed data.
        compressedData = true;
        // Write uncompressed header.
        writeMessageHeader(_finalBuffer, collatedBytes(), compressedBytes, true);
        _finalPacketCursor = InitialCursorOffset + compressedBytes;
      }
      //else
      //{
      //  std::cerr << "Compression failure. Collated " << collatedBytes() << " compressed to " << compressedBytes << std::endl;
      //}
    }
  }
#endif // TES_ZLIB

  if (!compressedData)
  {
    // No or failed compression. Write uncompressed.
    writeMessageHeader(_finalBuffer, collatedBytes(), collatedBytes(), false);
    memcpy(_finalBuffer + InitialCursorOffset, _buffer, collatedBytes());
    _finalPacketCursor = InitialCursorOffset + collatedBytes();
  }

  // Calculate the CRC
  PacketWriter::CrcType *crcPtr = reinterpret_cast<PacketWriter::CrcType *>(_finalBuffer + _finalPacketCursor);
  *crcPtr = crc16(_finalBuffer, _finalPacketCursor);
  networkEndianSwap(*crcPtr);
  _finalPacketCursor += sizeof(*crcPtr);
  _finalised = true;
  return true;
}


const uint8_t *CollatedPacket::buffer(unsigned &byteCount) const
{
  byteCount = _finalPacketCursor;
  return _finalBuffer;;
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
      expand(1024u, _buffer, _bufferSize, _cursor, _maxPacketSize);
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
          expand(1024u, _buffer, _bufferSize, _cursor, _maxPacketSize);
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
      expand(1024u, _buffer, _bufferSize, _cursor, _maxPacketSize);
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
      expand(1024u, _buffer, _bufferSize, _cursor, _maxPacketSize);
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
      expand(1024u, _buffer, _bufferSize, _cursor, _maxPacketSize);
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
  if (bufferSize == 0)
  {
    bufferSize = 16 * 1024;
  }
  _buffer = new uint8_t[bufferSize];
  _bufferSize = bufferSize;
  _finalBuffer = nullptr;
  _finalBufferSize = 0;
  _cursor = _finalPacketCursor = 0;
  _maxPacketSize = maxPacketSize;

#ifdef TES_ZLIB
  if (compress)
  {
    _zip = new CollatedPacketZip;
  }
#endif // TES_ZLIB
}


void CollatedPacket::expand(unsigned expandBy, uint8_t *&buffer, unsigned &bufferSize, unsigned currentDataCount, unsigned maxPacketSize)
{
  // Buffer too small.
  unsigned newBufferSize = std::min(nextLog2(bufferSize + expandBy + Overhead), maxPacketSize);
  uint8_t *newBuffer = new uint8_t[newBufferSize];
  if (buffer && currentDataCount)
  {
    memcpy(newBuffer, buffer, currentDataCount);
  }
  delete[] buffer;
  buffer = newBuffer;
  bufferSize = newBufferSize;
}
