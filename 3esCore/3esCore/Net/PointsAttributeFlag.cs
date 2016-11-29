using System;

namespace Tes
{
  /// <summary>
  /// Additional attribute flags for point data.
  /// </summary>
  [Flags]
  public enum PointsAttributeFlag : ushort
  {
    /// <summary>
    /// Zero flags.
    /// </summary>
    None = 0,
    /// <summary>
    /// Per point normals are present.
    /// </summary>
    Normals = (1 << 0),
    /// <summary>
    /// Per point colours (UInt32) are present.
    /// </summary>
    Colours = (1 << 1)
  }
}

