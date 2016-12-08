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
    /// Default length when not otherwise specified.
    /// </summary>
    public const float DefaultLength = 1.0f;

    /// <summary>
    /// Default radius when not otherwise specified.
    /// </summary>
    public const float DefaultRadius = 0.5f;

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The centre of the shape.</param>
    /// <param name="up">The major axis.</param>
    /// <param name="length">The length of the shape.</param>
    /// <param name="radius">The shape radius.</param>
    public Cylinder(uint id, Vector3 origin, Vector3 up, float length, float radius)
      : this(id, 0, origin, up, length, radius)
    {
    }

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="startPoint">The first point of the primary axis.</param>
    /// <param name="endPoint">The second point of the primary axis.</param>
    /// <param name="radius">The shape radius.</param>
    public Cylinder(uint id, Vector3 startPoint, Vector3 endPoint, float radius)
      : this(id, 0, startPoint, endPoint, radius)
    {
    }

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Cylinder(uint id = 0u, ushort category = 0)
      : this(id, category, Vector3.Zero, DefaultUp, DefaultLength, DefaultRadius) { }

    /// <summary>
    /// Create a new cylinder.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The centre of the shape.</param>
    /// <param name="up">The major axis.</param>
    /// <param name="length">The length of the shape.</param>
    /// <param name="radius">The shape radius.</param>
    public Cylinder(uint id, ushort category, Vector3 origin, Vector3 up, float length, float radius)
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
    /// <param name="startPoint">The first point of the primary axis.</param>
    /// <param name="endPoint">The second point of the primary axis.</param>
    /// <param name="radius">The shape radius.</param>
    public Cylinder(uint id, ushort category, Vector3 startPoint, Vector3 endPoint, float radius)
      : base((ushort)Tes.Net.ShapeID.Cylinder, id, category)
    {
      Vector3 axis = (endPoint - startPoint);
      Length = axis.Magnitude;
      Position = 0.5f * axis;
      Up = (Length > 1e-6f) ? axis / Length : DefaultUp;
      Radius = radius;
    }

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