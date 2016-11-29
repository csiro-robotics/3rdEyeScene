//
// author: Kazys Stepanas
//
#ifndef _3ESCOORDINATEFRAME_H_
#define _3ESCOORDINATEFRAME_H_

#include "3es-core.h"

namespace tes
{
  /// Enumerates various coordinate frames. Each frame specifies the global axes in
  /// the order right, forward, up. The up axis may be negated, that is a positive
  /// value is down, in which case the 'Neg' suffix is added.
  ///
  /// Right handed coordinate frames come first with left handed frames being those
  /// greater than or equal to @c CFLeft.
  ///
  /// Examples:
  /// Label | Right | Forward | Up  | Notes
  /// ----- | ----- | ------- | --- | ----------------------------------------------
  /// XYZ   | X     | Y       | Z   | A common extension of 2D Catesian coordinates.
  /// XZY   | X     | Z       | Y   | The default in Unity 3D.
  /// XZYNeg| X     | Z       | -Y  | A common camera space system.
  enum CoordinateFrame
  {
    XYZ,
    XZYNeg,
    YXZNeg,
    YZX,
    ZXY,
    ZYXNeg,
    XYZNeg,
    XZY,
    YXZ,
    YZXNeg,
    ZXYNeg,
    ZYX,

    CFCount,
    CFLeft = XYZNeg
  };
}

#endif // _3ESCOORDINATEFRAME_H_
