using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines a box shape for remote rendering.
  /// </summary>
  /// <remarks>
  /// The box defaults to a unit cube. The scale represents the edge lengths.
  /// </remarks>
  public class Box : Shape
  {
    /// <summary>
    /// Construct a box shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The box edge lengths or extents.</param>
    /// <param name="rot">A quaternion rotation applied to the shape.</param>
    public Box(uint id, Vector3 pos, Vector3 scale, Quaternion rot)
      : base((ushort)Tes.Net.ShapeID.Box, id)
    {
      Position = pos;
      Rotation = rot;
      Scale = scale;
    }

    /// <summary>
    /// Construct a box shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The box edge lengths or extents.</param>
    public Box(uint id, Vector3 pos, Vector3 scale)
      : this(id, pos, scale, Quaternion.Identity)
    {
    }

    /// <summary>
    /// Construct a box shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Box(uint id = 0u, ushort category = 0)
      : this(id, category, Vector3.Zero, new Vector3(1, 1, 1), Quaternion.Identity) { }

    /// <summary>
    /// Construct a box shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The box edge lengths or extents.</param>
    /// <param name="rot">A quaternion rotation applied to the shape.</param>
    public Box(uint id, ushort category, Vector3 pos, Vector3 scale, Quaternion rot)
    : base((ushort)Tes.Net.ShapeID.Box, id, category)
    {
      Position = pos;
      Rotation = rot;
      Scale = scale;
    }

    /// <summary>
    /// Construct a box shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The box edge lengths or extents.</param>
    public Box(uint id, ushort category, Vector3 pos, Vector3 scale)
    : this(id, category, pos, scale, Quaternion.Identity)
    {
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      Box copy = new Box();
      OnClone(copy);
      return copy;
    }
  }
}
