//
// author: Kazys Stepanas
//
#ifndef _3ESENDIAN_H_
#define _3ESENDIAN_H_

#include "3es-core.h"

#include <cinttypes>

namespace tes
{
  /// Perform an Endian swap on the given @p data buffer.
  /// This reverses the order of bytes in @p data.
  /// @param data The data buffer to reverse.
  /// @param size The number of bytes in @p data.
  void _3es_coreAPI endianSwap(uint8_t *data, size_t size);

  /// A 1-byte value Endian swap: noop.
  /// For completeness.
  /// @param data The 1-byte buffer.
  inline void endianSwap1(uint8_t *data) { return; } // No-op

  /// Perform a 2-byte value Endian swap on @p data.
  /// Switches the byte order.
  /// @param data The 2-byte buffer to Endian swap.
  inline void endianSwap2(uint8_t *data)
  {
    uint8_t temp = data[1];
    data[1] = data[0];
    data[0] = temp;
  }


  /// Perform a 4-byte value Endian swap on @p data.
  /// Switches the byte order.
  /// @param data The 4-byte buffer to Endian swap.
  inline void endianSwap4(uint8_t *data)
  {
    uint8_t temp = data[3];
    data[3] = data[0];
    data[0] = temp;
    temp = data[2];
    data[2] = data[1];
    data[1] = temp;
  }


  /// Perform an 8-byte value Endian swap on @p data.
  /// Switches the byte order.
  /// @param data The 8-byte buffer to Endian swap.
  inline void endianSwap8(uint8_t *data)
  {
    uint8_t temp = data[7];
    data[7] = data[0];
    data[0] = temp;
    temp = data[6];
    data[6] = data[1];
    data[1] = temp;
    temp = data[5];
    data[5] = data[2];
    data[2] = temp;
    temp = data[4];
    data[4] = data[3];
    data[3] = temp;
  }


  /// Perform a 16-byte value Endian swap on @p data.
  /// Switches the byte order.
  /// @param data The 16-byte buffer to Endian swap.
  inline void endianSwap16(uint8_t *data)
  {
    uint8_t temp = data[15];
    data[15] = data[0];
    data[0] = temp;
    temp = data[14];
    data[14] = data[1];
    data[1] = temp;
    temp = data[13];
    data[13] = data[2];
    data[2] = temp;
    temp = data[12];
    data[12] = data[3];
    data[3] = temp;

    temp = data[11];
    data[11] = data[4];
    data[4] = temp;
    temp = data[10];
    data[10] = data[5];
    data[5] = temp;
    temp = data[9];
    data[9] = data[6];
    data[6] = temp;
    temp = data[8];
    data[8] = data[7];
    data[7] = temp;
  }


  /// Single byte "Endian swap" for completeness: noop.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(uint8_t &data) {}
  /// Single byte "Endian swap" for completeness: noop.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(int8_t &data) {}

  /// Two byte integer Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(uint16_t &data) { endianSwap2(reinterpret_cast<uint8_t*>(&data)); }
  /// Two byte integer Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(int16_t &data) { endianSwap2(reinterpret_cast<uint8_t*>(&data)); }

  /// Four byte integer Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(uint32_t &data) { endianSwap4(reinterpret_cast<uint8_t*>(&data)); }
  /// Four byte integer Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(int32_t &data) { endianSwap4(reinterpret_cast<uint8_t*>(&data)); }

  /// Eight byte integer Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(uint64_t &data) { endianSwap8(reinterpret_cast<uint8_t*>(&data)); }
  /// Eight byte integer Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(int64_t &data) { endianSwap8(reinterpret_cast<uint8_t*>(&data)); }

  /// Four byte floating point Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(float &data) { endianSwap4(reinterpret_cast<uint8_t*>(&data)); }
  /// Eight byte floating point Endian swap.
  /// @param[in,out] data Value to swap the Endian of.
  inline void endianSwap(double &data) { endianSwap8(reinterpret_cast<uint8_t*>(&data)); }


  /// Return a copy of @p data with reversed byte order.
  /// @return The @p data value with reversed byte order.
  template <class T>
  inline T endianSwapValue(const T &data)
  {
    T val = data;
    endianSwap(val);
    return val;
  }


  /// Perform an @p endianSwap() on @p data to switch to/from network byte order (Big Endian).
  /// Does nothing on platforms which are already Big Endian.
  /// @param[in,out] data Buffer to reorder.
  /// @param size Number of bytes in @p data.
  inline void networkEndianSwap(uint8_t *data, size_t size)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap(data, size);
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// For completeness: noop.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(uint8_t &data) {}
  /// For completeness: noop.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(int8_t &data) {}

  /// Two byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(uint16_t &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap2(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Two byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(int16_t &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap2(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Four byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(uint32_t &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap4(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Four byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(int32_t &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap4(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Eight byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(uint64_t &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap8(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Eight byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(int64_t &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap8(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Four byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(float &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap4(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }

  /// Eight byte switch of @p data to/from network byte order.
  /// @param[in,out] data Value to potentially swap the Endian of.
  inline void networkEndianSwap(double &data)
  {
#if !TES_IS_NETWORK_ENDIAN
    endianSwap8(reinterpret_cast<uint8_t*>(&data));
#endif // !TES_IS_NETWORK_ENDIAN
  }


  /// Return a copy of @p data with byte order switched if host byte order is not network byte
  /// order.
  /// @return The @p data value potentially with reversed byte order.
  template <class T>
  inline T networkEndianSwapValue(const T &data)
  {
    T val = data;
    networkEndianSwap(val);
    return val;
  }
}

#endif // _3ESENDIAN_H_
