using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines a pose (set of axes) shape for remote rendering.
  /// </summary>
  /// <remarks>
  /// A pose is represented by a set of axis arrows or lines, coloured RBG corresponding to XYZ.
  ///
  /// Setting the shape colour tints the axis colours.
  /// </remarks>
  public class Pose : Shape
  {
    /// <summary>
    /// Default constructor, creating a unit sized, transient pose.
    /// </summary>
    public Pose()
      : this(0, 0)
    {
    }

    /// <summary>
    /// Construct a pose shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The pose edge lengths or extents.</param>
    /// <param name="rot">A quaternion rotation applied to the shape.</param>
    public Pose(uint id, Vector3 pos, Vector3 scale, Quaternion rot)
      : base((ushort)Tes.Net.ShapeID.Pose, id)
    {
      Position = pos;
      Rotation = rot;
      Scale = scale;
    }

    /// <summary>
    /// Construct a pose shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Pose(uint id, ushort category = 0)
      : this(id, category, Vector3.Zero, new Vector3(1, 1, 1), Quaternion.Identity) { }

    /// <summary>
    /// Construct a pose shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The pose edge lengths or extents.</param>
    public Pose(uint id, Vector3 pos, Vector3 scale)
      : this(id, pos, scale, Quaternion.Identity)
    {
    }

    /// <summary>
    /// Construct a pose shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The pose edge lengths or extents.</param>
    public Pose(uint id, ushort category, Vector3 pos, Vector3 scale)
    : this(id, category, pos, scale, Quaternion.Identity)
    {
    }

    /// <summary>
    /// Construct a pose shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pos">Defines the centre of the shape.</param>
    /// <param name="scale">The pose edge lengths or extents.</param>
    /// <param name="rot">A quaternion rotation applied to the shape.</param>
    public Pose(uint id, ushort category, Vector3 pos, Vector3 scale, Quaternion rot)
    : base((ushort)Tes.Net.ShapeID.Pose, id, category)
    {
      Position = pos;
      Rotation = rot;
      Scale = scale;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      Pose copy = new Pose();
      OnClone(copy);
      return copy;
    }
  }
}
