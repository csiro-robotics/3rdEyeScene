//
// author: Kazys Stepanas
//
#ifndef _3ESMATHS_H_
#define _3ESMATHS_H_

#ifndef _USE_MATH_DEFINES
#define _USE_MATH_DEFINES
#endif // _USE_MATH_DEFINES
#include <cmath>

namespace tes
{
  /// Conversion from degrees to radians.
  /// @param angle The angle to convert (degrees).
  /// @return The equivalent angle in radians.
  template <typename T>
  inline T degToRad(const T &angle = T(1))
  {
    return angle / T(180) * T(M_PI);
  }


  /// Conversion from radians to degrees.
  /// @param angle The angle to convert (radians).
  /// @return The equivalent angle in degrees.
  template <typename T>
  inline T radToDeg(const T &angle = T(1))
  {
    return angle * T(180) / T(M_PI);
  }


  /// Round up to the next power of 2.
  ///
  /// From: https://graphics.stanford.edu/~seander/bithacks.html
  /// @param v The value to round up.
  /// @return The next power of 2 larger than v.
  inline unsigned nextLog2(unsigned v)
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

#endif // 3ESMATHS_H_
