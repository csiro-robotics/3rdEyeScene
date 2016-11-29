using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines cone shape for remote rendering.
  /// </summary>
  /// <remarks>
  /// For cones, the ScaleX component contains the cone angle.
  /// </remarks>
  public class Cone : Shape
  {
    /// <summary>
    /// The default major axis when rendering with an identity quaternion rotation.
    /// </summary>
    public static Vector3 DefaultDirection = Vector3.AxisZ;

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The apex of the cone.</param>
    /// <param name="dir">The major axis.</param>
    /// <param name="angle">The cone angle (radians).</param>
    /// <param name="length">The length of the cone.</param>
    public Cone(uint id, Vector3 origin, Vector3 dir, float angle = 45.0f / 180.0f * (float)Math.PI, float length = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Cone, id)
    {
      Position = origin;
      Direction = dir;
      Angle = angle;
      Length = length;
    }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The apex of the cone.</param>
    public Cone(uint id, Vector3 origin) : this(id, origin, DefaultDirection) { }
    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public Cone(uint id = 0u) : this(id, Vector3.Zero, DefaultDirection) { }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The apex of the cone.</param>
    /// <param name="dir">The major axis.</param>
    /// <param name="angle">The cone angle (radians).</param>
    /// <param name="length">The length of the cone.</param>
    public Cone(uint id, ushort category, Vector3 origin, Vector3 dir, float angle = 45.0f / 180.0f * (float)Math.PI, float length = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Cone, id, category)
    {
      Position = origin;
      Direction = dir;
      Angle = angle;
      Length = length;
    }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The apex of the cone.</param>
    public Cone(uint id, ushort category, Vector3 origin) : this(id, category, origin, DefaultDirection) { }
    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Cone(uint id, ushort category) : this(id, category, Vector3.Zero, DefaultDirection) { }

    /// <summary>
    /// Access the cone angle (radians).
    /// </summary>
    public float Angle
    {
      get
      {
        return ScaleX;
      }
      set
      {
        ScaleX = ScaleY = value;
      }
    }

    /// <summary>
    /// Access the cone length.
    /// </summary>
    public float Length
    {
      get { return ScaleZ; }
      set { ScaleZ = value; }
    }

    /// <summary>
    /// Access the cone apex.
    /// </summary>
    public Vector3 Point { get { return Position; } set { Position = value; } }

    /// <summary>
    /// Access the cone major axis.
    /// </summary>
    public Vector3 Direction
    {
      get
      {
        return Rotation * DefaultDirection;
      }

      set
      {
        Quaternion rot = new Quaternion();
        if (value.Dot(DefaultDirection) > -0.9998f)
        {
          rot.SetFromTo(DefaultDirection, value);
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
      Cone copy = new Cone();
      OnClone(copy);
      return copy;
    }
  }
}