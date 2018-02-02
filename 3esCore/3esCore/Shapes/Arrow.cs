using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines an arrow shape for remote rendering.
  /// </summary>
  public class Arrow : Shape
  {
    /// <summary>
    /// The default direction when rendering with an identity quaternion rotation.
    /// </summary>
    public static Vector3 DefaultDirection = Vector3.AxisZ;

    /// <summary>
    /// Default length when not otherwise specified.
    /// </summary>
    public const float DefaultLength = 1.0f;

    /// <summary>
    /// Default radius when not otherwise specified.
    /// </summary>
    public const float DefaultRadius = 0.025f;

    /// <summary>
    /// Default constructor, defining a transient object.
    /// </summary>
    public Arrow()
    : this(0, 0)
    {
    }

    /// <summary>
    /// Construct an arrow shape.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="origin">Position of the arrow base.</param>
    /// <param name="dir">Unit vector away from the <paramref name="origin"/>.</param>
    /// <param name="length">Length of the arrow along <paramref name="dir"/>.</param>
    /// <param name="radius">Arrow radius.</param>
    public Arrow(uint id, Vector3 origin, Vector3 dir, float length, float radius)
      : base((ushort)Tes.Net.ShapeID.Arrow, id)
    {
      Position = origin;
      Direction = dir;
      Length = length;
      Radius = radius;
    }


    /// <summary>
    /// Construct an arrow shape.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="origin">Position of the arrow base.</param>
    /// <param name="endPoint">Position of the arrow tip.</param>
    /// <param name="radius">Arrow radius.</param>
    public Arrow(uint id, Vector3 origin, Vector3 endPoint, float radius)
      : this(id, 0, origin, endPoint, radius)
    {
    }

    /// <summary>
    /// Construct an arrow shape at the origin.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Arrow(uint id, ushort category = 0)
      : this(id, category, Vector3.Zero, DefaultDirection, DefaultLength, DefaultRadius) { }

    /// <summary>
    /// Construct an arrow shape.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">Position of the arrow base.</param>
    /// <param name="dir">Unit vector away from the <paramref name="origin"/>.</param>
    /// <param name="length">Length of the arrow along <paramref name="dir"/>.</param>
    /// <param name="radius">Arrow radius.</param>
    public Arrow(uint id, ushort category, Vector3 origin, Vector3 dir, float length, float radius)
      : base((ushort)Tes.Net.ShapeID.Arrow, id, category)
    {
      Position = origin;
      Direction = dir;
      Length = length;
      Radius = radius;
    }

    /// <summary>
    /// Construct an arrow shape.
    /// </summary>
    /// <param name="id">ID of the shape. Zero for transient.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="origin">Position of the arrow base.</param>
    /// <param name="endPoint">Position of the arrow tip.</param>
    /// <param name="radius">Arrow radius.</param>
    public Arrow(uint id, ushort category, Vector3 origin, Vector3 endPoint, float radius)
      : base((ushort)Tes.Net.ShapeID.Arrow, id, category)
    {
      Vector3 axis = endPoint - origin;
      Position = origin;
      Length = axis.Magnitude;
      Direction = (Length > 1e-6f) ? axis / Length : DefaultDirection;
      Radius = radius;
    }

    /// <summary>
    /// Access the arrow cylinder radius. The arrow head widens by about 25%.
    /// </summary>
    public float Radius
    {
      get { return ScaleX; }
      set { ScaleX = ScaleY = value; }
    }

    /// <summary>
    /// Access the arrow length.
    /// </summary>
    public float Length
    {
      get { return ScaleZ; }
      set { ScaleZ = value; }
    }

    /// <summary>
    /// Access the arrow direction (unit vector).
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
      Arrow copy = new Arrow();
      OnClone(copy);
      return copy;
    }
  }
}