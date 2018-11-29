using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines a sphere shape for remote rendering.
  /// </summary>
  public class Sphere : Shape
  {
    /// <summary>
    /// Create a transient, unit sphere at the origin.
    /// </summary>
    public Sphere() : this(0, 0) { }

    /// <summary>
    /// Create a sphere shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Sphere(uint id, ushort category = 0) : this(id, 0, Vector3.Zero) { }

    /// <summary>
    /// Create a sphere shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">The sphere centre.</param>
    /// <param name="radius">The sphere radius.</param>
    public Sphere(uint id, Vector3 position, float radius = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Sphere, id)
    {
      Position = position;
      Radius = radius;
    }

    /// <summary>
    /// Create a sphere shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">The sphere centre.</param>
    /// <param name="radius">The sphere radius.</param>
    public Sphere(uint id, ushort category, Vector3 position, float radius = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Sphere, id, category)
    {
      Position = position;
      Radius = radius;
    }

    /// <summary>
    /// Access the sphere radius.
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
      Sphere copy = new Sphere();
      OnClone(copy);
      return copy;
    }
  }
}