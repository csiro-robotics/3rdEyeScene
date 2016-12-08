using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines a star shape to render. Looks like three crossed lines, one for each axis.
  /// </summary>
  public class Star : Shape
  {
    /// <summary>
    /// Create a star shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">The star centre.</param>
    /// <param name="radius">The star radius.</param>
    public Star(uint id, Vector3 position, float radius = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Star, id)
    {
      Position = position;
      Radius = radius;
    }

    /// <summary>
    /// Create a star shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Star(uint id = 0u, ushort category = 0) : this(id, 0, Vector3.Zero) { }

    /// <summary>
    /// Create a star shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">The star centre.</param>
    /// <param name="radius">The star radius.</param>
    public Star(uint id, ushort category, Vector3 position, float radius = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Star, id, category)
    {
      Position = position;
      Radius = radius;
    }

    /// <summary>
    /// Access the star radius.
    /// </summary>
    public float Radius
    {
      get { return ScaleX; }
      set { ScaleX = ScaleY = ScaleZ = value; }
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      Star copy = new Star();
      OnClone(copy);
      return copy;
    }
  }
}