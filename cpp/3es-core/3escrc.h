// 
// author: Kazys Stepanas
// 
#ifndef _3ESCRC_H_ 
#define _3ESCRC_H_ 

#include "3es-core.h"

#include <cstdint>

namespace tes
{
  /// Calculate an 8-bit CRC value.
  /// @param message The buffer to operate on.
  /// @param byteCount The number of bytes in @p message.
  /// @return An 8-bit CRC for @c message.
  uint8_t _3es_coreAPI crc8(const uint8_t *message, size_t byteCount);

  /// Calculate an 16-bit CRC value.
  /// @param message The buffer to operate on.
  /// @param byteCount The number of bytes in @p message.
  /// @return An 16-bit CRC for @c message.
  uint16_t _3es_coreAPI crc16(const uint8_t *message, size_t byteCount);

  /// Calculate an 32-bit CRC value.
  /// @param message The buffer to operate on.
  /// @param byteCount The number of bytes in @p message.
  /// @return An 32-bit CRC for @c message.
  uint32_t _3es_coreAPI crc32(const uint8_t *message, size_t byteCount);
}

#endif // _3ESCRC_H_ 
