using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines cylinder shape for remote rendering.
  /// </summary>
  public class Cylinder : Shape
  {
    /// <summary>
    /// The default major axis when rendering with an identity quaternion rotation.
    /// </summary>
    public static Vector3 DefaultUp = Vector3.AxisZ;

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The centre of the shape.</param>
    /// <param name="up">The major axis.</param>
    /// <param name="length">The length of the shape.</param>
    /// <param name="radius">The shape radius.</param>
    public Cylinder(uint id, Vector3 origin, Vector3 up, float length = 1.0f, float radius = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Cylinder, id)
    {
      Position = origin;
      Up = up;
      Length = length;
      Radius = radius;
    }

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The centre of the shape.</param>
    public Cylinder(uint id, Vector3 origin) : this(id, origin, DefaultUp) { }
    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public Cylinder(uint id = 0u) : this(id, Vector3.Zero, DefaultUp) { }

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The centre of the shape.</param>
    /// <param name="up">The major axis.</param>
    /// <param name="length">The length of the shape.</param>
    /// <param name="radius">The shape radius.</param>
    public Cylinder(uint id, ushort category, Vector3 origin, Vector3 up, float length = 1.0f, float radius = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Cylinder, id, category)
    {
      Position = origin;
      Up = up;
      Length = length;
      Radius = radius;
    }

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The centre of the shape.</param>
    public Cylinder(uint id, ushort category, Vector3 origin) : this(id, category, origin, DefaultUp) { }
    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Cylinder(uint id, ushort category) : this(id, category, Vector3.Zero, DefaultUp) { }

    /// <summary>
    /// Access the cylinder radius.
    /// </summary>
    public float Radius
    {
      get { return ScaleX; }
      set { ScaleX = ScaleY = value; }
    }

    /// <summary>
    /// Access the cylinder length.
    /// </summary>
    public float Length
    {
      get { return ScaleZ; }
      set { ScaleZ = value; }
    }

    /// <summary>
    /// Access the cylinder centre.
    /// </summary>
    public Vector3 Centre { get { return Position; } set { Position = value; } }

    /// <summary>
    /// Access the cylinder major axis.
    /// </summary>
    public Vector3 Up
    {
      get
      {
        return Rotation * DefaultUp;
      }

      set
      {
        Quaternion rot = new Quaternion();
        if (value.Dot(DefaultUp) > -0.9998f)
        {
          rot.SetFromTo(DefaultUp, value);
        }
        else
        {
          rot.SetAxisAngle(Vector3.AxisX, (float)Math.PI);
        }
        Rotation = rot;
      }
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      Cylinder copy = new Cylinder();
      OnClone(copy);
      return copy;
    }
  }
}