using System;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines plane shape for remote rendering.
  /// </summary>
  /// <remarks>
  /// The "plane" is represented by a small quadrilateral at the requested position and
  /// a representation of the plane normal.
  /// </remarks>
  public class Plane : Shape
  {
    /// <summary>
    /// The default plane orientation by normal.
    /// </summary>
    public static Vector3 DefaultNormal = Vector3.AxisZ;

    /// <summary>
    /// Construct a plane shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Defines the centre of the plane patch.</param>
    /// <param name="normal">The plane normal.</param>
    /// <param name="scale">Defines the size of the plane quad.</param>
    /// <param name="normalLength">Render length for the normal.</param>
    public Plane(uint id, Vector3 position, Vector3 normal, float scale = 1.0f, float normalLength = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Plane, id)
    {
      Position = position;
      Normal = normal;
      Scale = scale;
      NormalLength = normalLength;
    }

    /// <summary>
    /// Construct a plane shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Plane(uint id = 0u, ushort category = 0) : this(id, 0, Vector3.Zero, DefaultNormal) { }

    /// <summary>
    /// Construct a plane shape.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Defines the centre of the plane patch.</param>
    /// <param name="normal">The plane normal.</param>
    /// <param name="scale">Defines the size of the plane quad.</param>
    /// <param name="normalLength">Render length for the normal.</param>
    public Plane(uint id, ushort category, Vector3 position, Vector3 normal, float scale = 1.0f, float normalLength = 1.0f)
      : base((ushort)Tes.Net.ShapeID.Plane, id, category)
    {
      Position = position;
      Normal = normal;
      Scale = scale;
      NormalLength = normalLength;
    }

    /// <summary>
    /// Access the plane quad scale.
    /// </summary>
    public new float Scale
    {
      get { return ScaleX; }
      set { ScaleX = ScaleZ = value; }
    }

    /// <summary>
    /// Access the plane normal visualisation length.
    /// </summary>
    public float NormalLength
    {
      get { return ScaleY; }
      set { ScaleY = value; }
    }

    /// <summary>
    /// Access the plane normal.
    /// </summary>
    public Vector3 Normal
    {
      get
      {
        return Rotation * DefaultNormal;
      }

      set
      {
        Quaternion rot = new Quaternion();
        if (value.Dot(DefaultNormal) > -0.9998f)
        {
          rot.SetFromTo(DefaultNormal, value);
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
      Plane copy = new Plane();
      OnClone(copy);
      return copy;
    }
  }
}