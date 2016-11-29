//
// author: Kazys Stepanas
//
#include "3espacketwriter.h"

#include "3escrc.h"
#include "3esendian.h"

#include <cstring>

using namespace tes;

PacketWriter::PacketWriter(PacketHeader &packet, uint16_t maxPayloadSize, uint16_t routingId, uint16_t messageId)
: PacketStream<PacketHeader>(packet)
, _bufferSize(maxPayloadSize + sizeof(PacketHeader))
{
  _packet.marker = PacketMarker;
  _packet.versionMajor = PacketVersionMajor;
  _packet.versionMinor = PacketVersionMinor;
  _packet.routingId = networkEndianSwapValue(routingId);
  _packet.messageId = networkEndianSwapValue(messageId);
  _packet.payloadSize = 0u;
  _packet.payloadOffset = 0u;
  _packet.flags = 0u;
}


PacketWriter::PacketWriter(uint8_t *buffer, uint16_t bufferSize, uint16_t routingId, uint16_t messageId)
: PacketStream<PacketHeader>(*reinterpret_cast<PacketHeader*>(buffer))
, _bufferSize(0)
{
  _bufferSize = bufferSize;
  if (bufferSize >= sizeof(PacketHeader) + sizeof(CrcType))
  {
    _packet.marker = networkEndianSwapValue(PacketMarker);
    _packet.versionMajor = networkEndianSwapValue(PacketVersionMajor);
    _packet.versionMinor = networkEndianSwapValue(PacketVersionMinor);
    _packet.routingId = networkEndianSwapValue(routingId);
    _packet.messageId = networkEndianSwapValue(messageId);;
    _packet.payloadSize = 0u;
    _packet.payloadOffset = 0u;
    _packet.flags = 0u;
  }
  else
  {
    _status |= Fail;
  }
}


PacketWriter::PacketWriter(const PacketWriter &other)
: PacketStream<PacketHeader>(*reinterpret_cast<PacketHeader*>(&other._packet))
, _bufferSize(other._bufferSize)
{
  _status = other._status;
  _payloadPosition = other._payloadPosition;
}


PacketWriter::~PacketWriter()
{
  finalise();
}


PacketWriter &PacketWriter::operator = (const PacketWriter &other)
{
  _packet = *reinterpret_cast<PacketHeader*>(&other._packet);
  _bufferSize = other._bufferSize;
  _status = other._status;
  _payloadPosition = other._payloadPosition;
  return *this;
}


void PacketWriter::reset(uint16_t routingId, uint16_t messageId)
{
  _status = Ok;
  if (_bufferSize >= sizeof(PacketHeader))
  {
    _packet.routingId = networkEndianSwapValue(routingId);
    _packet.messageId = networkEndianSwapValue(messageId);
    _packet.payloadSize = 0u;
    _packet.payloadOffset = 0u;
    _packet.flags = 0u;
    _payloadPosition = 0;
  }
  else
  {
    _status |= Fail;
  }
}


uint16_t PacketWriter::bytesRemaining() const
{
  return maxPayloadSize() - payloadSize();
}


uint16_t PacketWriter::maxPayloadSize() const
{
  return (!isFail()) ? _bufferSize - sizeof(PacketHeader) : 0u;
}


bool PacketWriter::finalise()
{
  if (!isFail())
  {
    calculateCrc();
  }
  return !isFail();
}


PacketWriter::CrcType PacketWriter::calculateCrc()
{
  if (isCrcValid())
  {
    return crc();
  }

  if (isFail())
  {
    return 0u;
  }

  if (packet().flags & PF_NoCrc)
  {
    // No CRC requested.
    _status |= CrcValid;
    return 0;
  }

  CrcType *crcPos = crcPtr();
  // Validate the CRC position for buffer overflow.
  const unsigned crcOffset = unsigned(reinterpret_cast<uint8_t*>(crcPos) - reinterpret_cast<uint8_t*>(&_packet));
  if (crcOffset > _bufferSize - sizeof(CrcType))
  {
    // CRC overruns the buffer. Cannot calculate.
    _status |= Fail;
    return 0;
  }

  CrcType crcVal = crc16(reinterpret_cast<const uint8_t *>(&_packet), sizeof(PacketHeader)+payloadSize());
  *crcPos = networkEndianSwapValue(crcVal);
  _status |= CrcValid;
  return *crcPos;
}


size_t PacketWriter::writeElement(const uint8_t *bytes, size_t elementSize)
{
  if (bytesRemaining() >= elementSize)
  {
    memcpy(payloadWritePtr(), bytes, elementSize);
    networkEndianSwap(payloadWritePtr(), elementSize);
    _payloadPosition += uint16_t(elementSize);
    incrementPayloadSize(elementSize);
    return elementSize;
  }

  return 0;
}


size_t PacketWriter::writeArray(const uint8_t *bytes, size_t elementSize, size_t elementCount)
{
  size_t copyCount = bytesRemaining() / elementSize;
  if (copyCount > 0)
  {
    copyCount = (copyCount > elementCount) ? elementCount : copyCount;
    memcpy(payloadWritePtr(), bytes, copyCount * elementSize);
#if !TES_IS_NETWORK_ENDIAN
    uint8_t *fixBytes = payloadWritePtr();
    for (unsigned i = 0; i < copyCount; ++i, fixBytes += elementSize)
    {
      networkEndianSwap(fixBytes, elementSize);
    }
#endif // !TES_IS_NETWORK_ENDIAN
    incrementPayloadSize(elementSize * copyCount);
    _payloadPosition += uint16_t(elementSize * copyCount);
    return copyCount;
  }

  return 0;
}


size_t PacketWriter::writeRaw(const uint8_t *bytes, size_t byteCount)
{
  size_t copyCount = (byteCount <= bytesRemaining()) ? byteCount : bytesRemaining();
  memcpy(payloadWritePtr(), bytes, copyCount);
  incrementPayloadSize(copyCount);
  _payloadPosition += uint16_t(copyCount);
  return copyCount;
}


void PacketWriter::incrementPayloadSize(size_t inc)
{
  _packet.payloadSize = uint16_t(payloadSize() + inc);
  networkEndianSwap(_packet.payloadSize);
  invalidateCrc();
}
