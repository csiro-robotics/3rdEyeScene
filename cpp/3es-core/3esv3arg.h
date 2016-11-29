//
// author: Kazys Stepanas
//
#ifndef _3ESV3ARG_H_
#define _3ESV3ARG_H_

#include "3es-core.h"

#include "3esvector3.h"

namespace tes
{
  /// A helper structure used to convert from float or double pointers to @c Vector3f arguments.
  struct V3Arg
  {
    /// Single precision pointer constructor.
    /// @param q Vector 3 array.
    inline V3Arg(const float v[3]) : v3(v) {}
    /// Double precision pointer constructor.
    /// @param q Vector 3  array.
    inline V3Arg(const double v[3]) : v3(Vector3d(v)) {}
    /// Single precision vector constructor.
    /// @param q Vector 3 value.
    inline V3Arg(const Vector3f &v) : v3(v) {}
    /// Double precision vector constructor.
    /// @param q Vector 3 value.
    inline V3Arg(const Vector3d &v) : v3(v) {}

    /// Component wise constructor.
    /// @param x X value.
    /// @param y Y value.
    /// @param z Z value.
    inline V3Arg(float x, float y, float z) : v3(x, y, z) {}

    /// Convert to @c Vector3f.
    /// @return The single precision vector 3.
    inline operator Vector3f() const { return v3; }

    /// Indexing operator.
    /// @param i The element index [0, 2].
    /// @return The requested element
    inline float operator[](int i) const { return v3[i]; }

    /// Vector 3 value.
    Vector3f v3;
  };
}

#endif // _3ESV3ARG_H_
