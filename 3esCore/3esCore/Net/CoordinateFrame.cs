using System;

namespace Tes.Net
{
  /// <summary>
  /// Defines the expected global coordinate frame. This is mapped to Unity's coordinate frame.
  /// </summary>
  /// <remarks>
  /// Each member of the enumeration identifies the right, forward and up axes
  /// in turn. That is, <see cref="XZY"/> defines the axes as X-right, Z-forward
  /// and Y-up. This is the default Unity coordinate frame.
  /// 
  /// Some enumeration memebers have the up axis prefixed with an underscore '_'.
  /// This is used in place of a '-' sign and identify that the axis is negated
  /// and defines a 'down' axis, not up.
  /// 
  /// The enumeration members are divided into two groups: right and left.
  /// All members before <see cref="XY_Z"/> are left handed, while every
  /// member thereafter, including <see cref="XY_Z"/> defines a left-handed
  /// coordinate frame.
  /// </remarks>
  public enum CoordinateFrame
  {
    // Disable Xml comment warnings.
#pragma warning disable 1591
    XYZ,
    XZ_Y,
    YX_Z,
    YZX,
    ZXY,
    ZY_X,
    XY_Z,
    XZY,
    YXZ,
    YZ_X,
    ZX_Y,
    ZYX,

    /// <summary>
    /// First left handed coordinate frame.
    /// </summary>
    LeftHanded = XY_Z
    // Restore Xml comment warnings.
#pragma warning restore 1591
  }
}
