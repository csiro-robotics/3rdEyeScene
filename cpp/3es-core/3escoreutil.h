//
// author: Kazys Stepanas
//
#ifndef _3ESCOREUTIL_H_
#define _3ESCOREUTIL_H_

#include "3es-core.h"

#include "3escolour.h"
#include "3esvector4.h"

namespace tes
{
  /// A utility function for moving a pointer by a given byte stride.
  /// @param ptr The pointer to move.
  /// @param stride The stride in bytes to move the pointer on by.
  /// @return The address of ptr + stride (byte move).
  /// @tparam T The pointer type.
  /// @tparam ST The stride type. Must be an integer type.
  template <typename T, typename ST>
  inline T *moveByStride(T *ptr, ST stride)
  {
    char *mem = reinterpret_cast<char *>(ptr);
    mem += stride;
    return reinterpret_cast<T*>(mem);
  }


  /// @overload
  template <typename T, typename ST>
  inline const T *moveByStride(const T *ptr, ST stride)
  {
    const char *mem = reinterpret_cast<const char *>(ptr);
    mem += stride;
    return reinterpret_cast<const T *>(mem);
  }


  /// Convert a @c Colour to @c Vector4.
  ///
  /// Colour channels [R, G, B, A] line up with vertex channels [x, y, z, w].
  template <typename T>
  inline Vector4<T> toVector(const Colour &c)
  {
    return Vector4<T>(T(c.rf()), T(c.gf()), T(c.bf()), T(c.af()));
  }
  template Vector4<float> _3es_coreAPI toVector(const Colour &c);
  template Vector4<double> _3es_coreAPI toVector(const Colour &c);


  inline Vector4f _3es_coreAPI toVectorf(const Colour &c) { return toVector<float>(c); }
  inline Vector4d _3es_coreAPI toVectord(const Colour &c) { return toVector<double>(c); }


  /// Convert a @c Vector4 to a @c Colour. Some precision will be lost.
  ///
  /// Colour channels [R, G, B, A] line up with vertex channels [x, y, z, w].
  template <typename T>
  inline Colour toColour(const Vector4<T> &v)
  {
    Colour c;
    c.setRf(float(v.x));
    c.setGf(float(v.y));
    c.setBf(float(v.z));
    c.setAf(float(v.w));
    return c;
  }


  template Colour _3es_coreAPI toColour(const Vector4<float> &v);
  template Colour _3es_coreAPI toColour(const Vector4<double> &v);


  /// Calculate the next power of 2 equal to or greater than @p v.
  /// @param The base, integer value.
  template <typename T>
  inline T ceilPowerOf2(T v)
  {
    size_t next;
    bool isPow2;
    isPow2 = v && !(v & (v - 1));
    next = T(1) << (T(1) + T(std::floor(std::log2(float(v)))));
    return isPow2 ? v : next;
  }


  /// @overload
  template <>
  inline unsigned ceilPowerOf2(unsigned v)
  {
    v--;
    v |= v >> 1;
    v |= v >> 2;
    v |= v >> 4;
    v |= v >> 8;
    v |= v >> 16;
    v++;
    return v;
  }


  /// @overload
  template <>
  inline int ceilPowerOf2(int v)
  {
    v--;
    v |= v >> 1;
    v |= v >> 2;
    v |= v >> 4;
    v |= v >> 8;
    v |= v >> 16;
    v++;
    return v;
  }
}

#endif // _3ESCOREUTIL_H_
