//
// author: Kazys Stepanas
//
#include "3escrc.h"

namespace tes
{
  // Crc code taken from http://www.barrgroup.com/Embedded-Systems/How-To/CRC-Calculation-C-Code
  template <typename CRC>
  class CrcCalc
  {
  public:
    CrcCalc(CRC initialRemainder, CRC finalXorValue, CRC polynomial);

    CRC crc(const uint8_t *message, size_t byteCount) const;

    inline CRC operator()(const uint8_t *message, size_t byteCount) const { return crc(message, byteCount); }

  private:
    CRC _initialRemainder;
    CRC _finalXorValue;
    CRC _crcTable[256];

    void initTable(CRC polynomial);

    const CRC Width = (8 * sizeof(CRC));
    const CRC TopBit = (1 << ((8 * sizeof(CRC)) - 1));
  };


  template <typename CRC>
  CrcCalc<CRC>::CrcCalc(CRC initialRemainder, CRC finalXorValue, CRC polynomial)
    : _initialRemainder(initialRemainder)
    , _finalXorValue(finalXorValue)
  {
    initTable(polynomial);
  }


  template <typename CRC>
  CRC CrcCalc<CRC>::crc(const uint8_t *message, size_t byteCount) const
  {
    uint8_t data;
    CRC remainder = _initialRemainder;

    // Divide the message by the polynomial, a byte at a time.
    for (size_t byte = 0u; byte < byteCount; ++byte)
    {
      data = message[byte] ^ (remainder >> (Width - 8));
      remainder = _crcTable[data] ^ (remainder << 8);
    }

    // The final remainder is the CRC.
    return remainder ^ _finalXorValue;
  }


  template <typename CRC>
  void CrcCalc<CRC>::initTable(CRC polynomial)
  {
    CRC remainder;

    // Compute the remainder of each possible dividend.
    for (int dividend = 0; dividend < 256; ++dividend)
    {
      // Start with the dividend followed by zeros.
      remainder = dividend << (Width - 8);

      // Perform modulo-2 division, a bit at a time.
      for (uint8_t bit = 8; bit > 0; --bit)
      {
        // Try to divide the current data bit.
        if (remainder & TopBit)
        {
          remainder = (remainder << 1) ^ polynomial;
        }
        else
        {
          remainder = (remainder << 1);
        }
      }

      // Store the result into the table.
      _crcTable[dividend] = remainder;
    }
  }


  static CrcCalc<uint8_t> Crc8(0xFFu, 0u, 0x21u);
  static CrcCalc<uint16_t> Crc16(0xFFFFu, 0u, 0x1021u);
  static CrcCalc<uint32_t> Crc32(0xFFFFFFFFu, 0xFFFFFFFFu, 0x04C11DB7u);


  uint8_t crc8(const uint8_t *message, size_t byteCount)
  {
    return Crc8(message, byteCount);
  }


  uint16_t crc16(const uint8_t *message, size_t byteCount)
  {
    return Crc16(message, byteCount);
  }


  uint32_t crc32(const uint8_t *message, size_t byteCount)
  {
    return Crc32(message, byteCount);
  }
}
