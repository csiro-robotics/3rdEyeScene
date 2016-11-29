//
// author: Kazys Stepanas
//
#ifndef _3ESQUATERNIONARG_H_
#define _3ESQUATERNIONARG_H_

#include "3es-core.h"

#include "3esquaternion.h"

namespace tes
{
  /// A helper structure used to convert from float or double pointers to @c Quaternionf arguments.
  struct QuaternionArg
  {
    /// Single precision pointer constructor.
    /// @param q Quaternion array.
    inline QuaternionArg(const float q[4]) : q(q) {}
    /// Double precision pointer constructor.
    /// @param q Quaternion array.
    inline QuaternionArg(const double q[4]) : q(Quaterniond(q)) {}
    /// Single precision quaternion constructor.
    /// @param q Quaternion value.
    inline QuaternionArg(const Quaternionf &q) : q(q) {}
    /// Double precision quaternion constructor.
    /// @param q Quaternion value.
    inline QuaternionArg(const Quaterniond &q) : q(q) {}

    /// Component wise constructor.
    /// @param x X value.
    /// @param y Y value.
    /// @param z Z value.
    /// @param w W value.
    inline QuaternionArg(float x, float y, float z, float w) : q(x, y, z, w) {}

    /// Convert to @c Quaternionf.
    /// @return The single precision quaternion.
    inline operator Quaternionf() const { return q; }

    /// Indexing operator.
    /// @param i The element index [0, 3].
    /// @return The requested element
    inline float operator[](int i) const { return q[i]; }
    
    /// Quaternion value.
    Quaternionf q;
  };
}

#endif // _3ESQUATERNIONARG_H_
