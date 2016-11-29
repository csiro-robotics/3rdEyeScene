using System;
using Tes.Maths;
using Tes.Shapes;

namespace Tes
{
  /// <summary>
  /// Defines an arrow shape for remote rendering.
  /// </summary>
  public class Pyramid : Shape
  {
    /// <summary>
    /// Construct a pyramid shape.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="position">Position of the shape.</param>
    /// <param name="scale">Shape scale.</param>
    /// <param name="rotation">Shape orientation</param>
    public Pyramid(uint id, Vector3 position, Vector3 scale, Quaternion rotation)
      : base((ushort)Tes.Net.RoutingID.UserIDStart, id)
    {
      Position = position;
      Scale = scale;
      Rotation = rotation;
    }

    /// <summary>
    /// Construct a pyramid shape.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Position of the shape.</param>
    /// <param name="scale">Shape scale.</param>
    /// <param name="rotation">Shape orientation</param>
    public Pyramid(uint id, ushort category, Vector3 position, Vector3 scale, Quaternion rotation)
      : base((ushort)Tes.Net.RoutingID.UserIDStart, id, category)
    {
      Position = position;
      Scale = scale;
      Rotation = rotation;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      Pyramid copy = new Pyramid(ID, Position, Scale, Rotation);
      OnClone(copy);
      return copy;
    }
  }
}