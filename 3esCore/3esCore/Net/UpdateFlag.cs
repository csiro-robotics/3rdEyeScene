using System;

namespace Tes
{
  /// <summary>
  /// Flags controlling the creation and appearance of an object.
  /// </summary>
  [Flags]
  public enum UpdateFlag : ushort
  {
    /// <summary>
    /// Zero value.
    /// </summary>
    None = 0,
    /// <summary>
    /// Transition to the new position, colour, etc, is interpolated over the render frames.
    /// This is only used if the render frame rate of this application is higher than that of
    /// the incoming data.
    /// </summary>
    Interpolate = (1 << 0)
  }
}

