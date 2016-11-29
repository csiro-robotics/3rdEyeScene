//
// author: Kazys Stepanas
//
#include "3espacketreader.h"

#include "3escrc.h"
#include "3esendian.h"

#include <cstring>

using namespace tes;

PacketReader::PacketReader(const PacketHeader &packet)
: PacketStream<const PacketHeader>(packet)
{
  seek(0, Begin);
}


bool PacketReader::checkCrc()
{
  if (isCrcValid())
  {
    return true;
  }

  if ((_packet.flags & PF_NoCrc))
  {
    _status |= CrcValid;
    return true;
  }

  const CrcType packetCrc = crc();
  const CrcType crcVal = calculateCrc();
  if (crcVal == packetCrc)
  {
    _status |= CrcValid;
    return true;
  }
  return false;
}


PacketReader::CrcType PacketReader::calculateCrc() const
{
  const CrcType crcVal = crc16(reinterpret_cast<const uint8_t *>(&_packet), sizeof(PacketHeader)+payloadSize());
  return crcVal;
}


size_t PacketReader::readElement(uint8_t *bytes, size_t elementSize)
{
  if (bytesAvailable() >= elementSize)
  {
    memcpy(bytes, payload() + _payloadPosition, elementSize);
    networkEndianSwap(bytes, elementSize);
    _payloadPosition += uint16_t(elementSize);
    return elementSize;
  }

  return 0;
}


size_t PacketReader::readArray(uint8_t *bytes, size_t elementSize, size_t elementCount)
{
  size_t copyCount = bytesAvailable() / elementSize;
  if (copyCount > 0)
  {
    copyCount = (copyCount > elementCount) ? elementCount : copyCount;
    memcpy(bytes, payload() + _payloadPosition, copyCount * elementSize);
#if !TES_IS_NETWORK_ENDIAN
    uint8_t *fixBytes = bytes;
    for (unsigned i = 0; i < copyCount; ++i, fixBytes += elementSize)
    {
      networkEndianSwap(fixBytes, elementSize);
    }
#endif // !TES_IS_NETWORK_ENDIAN
    _payloadPosition += uint16_t(elementSize * copyCount);
    return copyCount;
  }

  return 0;
}


size_t PacketReader::readRaw(uint8_t *bytes, size_t byteCount)
{
  size_t copyCount = (byteCount <= bytesAvailable()) ? byteCount : bytesAvailable();
  memcpy(bytes, payload() + _payloadPosition, copyCount);
  _payloadPosition += uint16_t(copyCount);
  return copyCount;
}
