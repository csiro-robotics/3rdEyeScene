using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags for Text3D rendering
  /// </summary>
  [Flags]
  public enum Text3DFlag : ushort
  {
    /// <summary>
    /// Orient the text to face the screen
    /// </summary>
    SceenFacing = ObjectFlag.User,
  }
}

