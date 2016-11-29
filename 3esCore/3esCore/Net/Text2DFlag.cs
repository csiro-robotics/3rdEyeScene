using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags for Text2D rendering
  /// </summary>
  [Flags]
  public enum Text2DFlag : ushort
  {
    /// <summary>
    /// Position is given in screen space and mapped to world space. Otherwise text is sceen spacewith (0, 0, z).
    /// </summary>
    WorldSpace = ObjectFlag.User
  }
}
