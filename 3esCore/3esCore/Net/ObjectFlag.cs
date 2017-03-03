using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags controlling object creation.
  /// </summary>
  [Flags]
  public enum ObjectFlag : ushort
  {
    /// <summary>
    /// Null flag.
    /// </summary>
    None = 0,
    /// <summary>
    /// Shape should be rendered using wireframe rendering.
    /// </summary>
    Wireframe = (1 << 0),
    /// <summary>
    /// Shape is transparent. Colour should include an appropriate alpha value.
    /// </summary>
    Transparent = (1 << 1),
    /// <summary>
    /// Shape used a two sided shader (triangle culling disabled).
    /// </summary>
    TwoSided = (1 << 2),
    /// <summary>
    /// Update attributes using only explicitly specified flags from the following.
    /// </summary>
    UpdateMode = (1 << 3),
    /// <summary>
    /// Update position data.
    /// </summary>
    Position = (1 << 4),
    /// <summary>
    /// Update rotation data.
    /// </summary>
    Rotation = (1 << 5),
    /// <summary>
    /// Update scale data.
    /// </summary>
    Scale = (1 << 6),
    /// <summary>
    /// Update colour data.
    /// </summary>
    Colour = (1 << 7),
    /// <summary>
    /// A spelling alias for colour.
    /// </summary>
    Color = Colour,
    /// <summary>
    /// User flags start here.
    /// </summary>
    User = (1 << 12)
  }
}
