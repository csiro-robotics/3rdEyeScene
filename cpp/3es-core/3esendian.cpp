//
// author: Kazys Stepanas
//
#include "3esendian.h"

#include <cstring>
#include <cstdlib>

#ifdef WIN32
#include <malloc.h>
#endif // WIN32

namespace tes
{
  void endianSwap(uint8_t *data, size_t size)
  {
    switch (size)
    {
    case 0:
    case 1:
      return;
    case 2:
      return endianSwap2(data);;
    case 4:
      return endianSwap4(data);;
    case 8:
      return endianSwap8(data);;
    case 16:
      return endianSwap16(data);;
    default:
      break;
    }

    uint8_t *dataCopy = (uint8_t*)alloca(size);
    memcpy(dataCopy, data, size);
    for (size_t i = 0; i < size / 2; ++i)
    {
      data[i] = dataCopy[size - i - 1];
    }
  }
}
