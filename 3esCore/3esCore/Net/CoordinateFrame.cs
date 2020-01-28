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

  public static class CoordinateFrameUtil
  {
    static CoordinateFrameUtil()
    {
      _axisIndices = new int[(int)CoordinateFrame.ZYX + 1, 3];
      _flippedUpAxis = new bool[(int)CoordinateFrame.ZYX + 1];

      _axisIndices[(int)CoordinateFrame.XYZ, 0] = 0;
      _axisIndices[(int)CoordinateFrame.XYZ, 1] = 1;
      _axisIndices[(int)CoordinateFrame.XYZ, 2] = 2;
      _flippedUpAxis[(int)CoordinateFrame.XYZ] = false;

      _axisIndices[(int)CoordinateFrame.XZ_Y, 0] = 0;
      _axisIndices[(int)CoordinateFrame.XZ_Y, 1] = 2;
      _axisIndices[(int)CoordinateFrame.XZ_Y, 2] = 1;
      _flippedUpAxis[(int)CoordinateFrame.XZ_Y] = true;

      _axisIndices[(int)CoordinateFrame.YX_Z, 0] = 1;
      _axisIndices[(int)CoordinateFrame.YX_Z, 1] = 0;
      _axisIndices[(int)CoordinateFrame.YX_Z, 2] = 2;
      _flippedUpAxis[(int)CoordinateFrame.YX_Z] = true;

      _axisIndices[(int)CoordinateFrame.YZX, 0] = 1;
      _axisIndices[(int)CoordinateFrame.YZX, 1] = 2;
      _axisIndices[(int)CoordinateFrame.YZX, 2] = 0;
      _flippedUpAxis[(int)CoordinateFrame.YZX] = false;

      _axisIndices[(int)CoordinateFrame.ZXY, 0] = 2;
      _axisIndices[(int)CoordinateFrame.ZXY, 1] = 0;
      _axisIndices[(int)CoordinateFrame.ZXY, 2] = 1;
      _flippedUpAxis[(int)CoordinateFrame.ZXY] = false;

      _axisIndices[(int)CoordinateFrame.ZY_X, 0] = 2;
      _axisIndices[(int)CoordinateFrame.ZY_X, 1] = 1;
      _axisIndices[(int)CoordinateFrame.ZY_X, 2] = 0;
      _flippedUpAxis[(int)CoordinateFrame.ZY_X] = true;

      _axisIndices[(int)CoordinateFrame.XY_Z, 0] = 0;
      _axisIndices[(int)CoordinateFrame.XY_Z, 1] = 1;
      _axisIndices[(int)CoordinateFrame.XY_Z, 2] = 2;
      _flippedUpAxis[(int)CoordinateFrame.XY_Z] = true;

      _axisIndices[(int)CoordinateFrame.XZY, 0] = 0;
      _axisIndices[(int)CoordinateFrame.XZY, 1] = 2;
      _axisIndices[(int)CoordinateFrame.XZY, 2] = 1;
      _flippedUpAxis[(int)CoordinateFrame.XZY] = false;

      _axisIndices[(int)CoordinateFrame.YXZ, 0] = 1;
      _axisIndices[(int)CoordinateFrame.YXZ, 1] = 0;
      _axisIndices[(int)CoordinateFrame.YXZ, 2] = 2;
      _flippedUpAxis[(int)CoordinateFrame.YXZ] = false;

      _axisIndices[(int)CoordinateFrame.YZ_X, 0] = 1;
      _axisIndices[(int)CoordinateFrame.YZ_X, 1] = 2;
      _axisIndices[(int)CoordinateFrame.YZ_X, 2] = 0;
      _flippedUpAxis[(int)CoordinateFrame.YZ_X] = true;

      _axisIndices[(int)CoordinateFrame.ZX_Y, 0] = 2;
      _axisIndices[(int)CoordinateFrame.ZX_Y, 1] = 0;
      _axisIndices[(int)CoordinateFrame.ZX_Y, 2] = 1;
      _flippedUpAxis[(int)CoordinateFrame.ZX_Y] = true;

      _axisIndices[(int)CoordinateFrame.ZYX, 0] = 2;
      _axisIndices[(int)CoordinateFrame.ZYX, 1] = 1;
      _axisIndices[(int)CoordinateFrame.ZYX, 2] = 0;
      _flippedUpAxis[(int)CoordinateFrame.ZYX] = false;
    }

    /// <summary>
    /// Query the axis indices for a <see cref="CoordinateFrame"/>.
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    /// <remarks>
    /// The <paramref name="axis"/> value is interpreted as follows:
    ///
    /// <list type="table">
    /// <listheader><term>Axis</term><description>Meaning</description>
    /// <item><term>0</term><description>Right/left axis</description>
    /// <item><term>1</term><description>Forward</description>
    /// <item><term>2</term><description>Up</description>
    /// </list>
    ///
    /// The return values indicate which XYZ the is mapped to the descriptions above. For example, querying the
    /// AxisIndex with <paramref name="axis"/> set to 1 is a query for the Forward axis of the given
    /// <paramref name="frame"/>. This will return a value in the range [0, 2] mapping to XYZ respectively.
    /// </remarks>
    public int AxisIndex(CoordinateFrame frame, int axis)
    {
      return _axisIndices[(int)frame, axis];
    }

    public bool FlippedUpAxis(CoordinateFrame frame)
    {

    }

    public bool LeftHanded(CoordinateFrame frame)
    {
      return (int)frame >= (int)CoordinateFrame.LeftHanded;
    }

    private static int[,] _axisIndices;
    private static bool[] _flippedUpAxis;
  }
}
