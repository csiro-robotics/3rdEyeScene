using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags controlling the creation and appearance of an object.
  /// </summary>
  [Flags]
  public enum UpdateFlag : ushort
  {
    ///<summary>
    /// Update attributes using only explicitly specified flags from the following.
    /// </summary>
    UpdateMode = (ObjectFlag.User << 0),
    /// <summary>
    /// Update position data.
    /// </summary>
    Position = (ObjectFlag.User << 1),
    /// <summary>
    /// Update rotation data.
    /// </summary>
    Rotation = (ObjectFlag.User << 2),
    /// <summary>
    /// Update scale data.
    /// </summary>
    Scale = (ObjectFlag.User << 3),
    /// <summary>
    /// Update colour data.
    /// </summary>
    Colour = (ObjectFlag.User << 4),
    /// <summary>
    /// Spelling alias for colour.
    /// </summary>
    Color = UpdateFlag.Colour,
  }
}
