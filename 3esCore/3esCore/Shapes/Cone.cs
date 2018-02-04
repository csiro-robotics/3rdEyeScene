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
    /// Default angle when not otherwise specified.
    /// </summary>
    public const float DefaultAngle = 45.0f / 180.0f * (float)Math.PI;

    /// <summary>
    /// Default length when not otherwise specified.
    /// </summary>
    public const float DefaultLength = 1.0f;

    /// <summary>
    /// Default constructor, defining a transient object.
    /// </summary>
    public Cone() : this(0, 0) { }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Cone(uint id, ushort category = 0)
      : this(id, category, Vector3.Zero, DefaultDirection, DefaultLength, DefaultAngle) { }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The apex of the cone.</param>
    /// <param name="dir">The major axis.</param>
    /// <param name="angle">The cone angle (radians).</param>
    /// <param name="length">The length of the cone.</param>
    public Cone(uint id, Vector3 origin, Vector3 dir, float length, float angle)
      : this(id, 0, origin, dir, angle, length)
    {
    }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="origin">The apex of the cone.</param>
    /// <param name="basePoint">The position of the centre of the cone base.</param>
    /// <param name="radius">The cone radius at the base.</param>
    public Cone(uint id, Vector3 origin, Vector3 basePoint, float radius)
      : this(id, 0, origin, basePoint, radius)
    {
    }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The apex of the cone.</param>
    /// <param name="dir">The major axis.</param>
    /// <param name="angle">The cone angle (radians).</param>
    /// <param name="length">The length of the cone.</param>
    public Cone(uint id, ushort category, Vector3 origin, Vector3 dir, float length, float angle)
      : base((ushort)Tes.Net.ShapeID.Cone, id, category)
    {
      Position = origin;
      Direction = dir;
      // Must set length as setting the angle derives the radius.
      ScaleZ = length;
      Angle = angle;
    }

    /// <summary>
    /// Create a new cone.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">The apex of the cone.</param>
    /// <param name="basePoint">The position of the centre of the cone base.</param>
    /// <param name="radius">The cone radius at the base.</param>
    public Cone(uint id, ushort category, Vector3 origin, Vector3 basePoint, float radius)
      : base((ushort)Tes.Net.ShapeID.Cone, id, category)
    {
      Vector3 axis = basePoint - origin;
      Position = origin;
      ScaleZ = axis.Magnitude;
      ScaleX = ScaleY = radius;
      Direction = (Length > 1e-6f) ? axis / Length : DefaultDirection;
    }

    /// <summary>
    /// Access the cone angle (radians).
    /// </summary>
    public float Angle
    {
      get
      {
        // ScaleX/Y encode the radius of the cone base.
        // Convert to angle angle as:
        //   tan(theta) = radius / length
        //   theta = atan(radius / length)
        return (ScaleZ != 0.0f) ? (float)Math.Atan(ScaleX / ScaleZ) : 0.0f;
      }
      set
      {
        // ScaleX/Y encode the radius of the cone base.
        // Convert the given angle as:
        //   radius = length * tan(theta)
        ScaleX = ScaleY = ScaleZ * (float)Math.Tan(value);
      }
    }

    /// <summary>
    /// Access the cone length.
    /// </summary>
    public float Length
    {
      get { return ScaleZ; }
      set
      {
        // Changing the length requires maintaining the angle, so we must adjust the radius to suit.
        float angle = Angle;
        ScaleZ = value;
        Angle = angle;
      }
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