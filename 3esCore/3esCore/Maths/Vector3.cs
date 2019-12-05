using System;

namespace Tes.Maths
{
  /// <summary>
  /// A basic vector3 implementation.
  /// </summary>
  public struct Vector3
  {
    /// <summary>
    /// Default epsilon value used in various calculations.
    /// </summary>
    public const float Epsilon = 1e-6f;
    /// <summary>
    /// A zero vector (0, 0, 0).
    /// </summary>
    public static Vector3 Zero = new Vector3 { X = 0, Y = 0, Z = 0 };
    /// <summary>
    /// A vector with all components one (1, 1, 1).
    /// </summary>
    public static Vector3 One = new Vector3 { X = 1, Y = 1, Z = 1 };
    /// <summary>
    /// A vector representing the X axis: (1, 0, 0).
    /// </summary>
    public static Vector3 AxisX = new Vector3 { X = 1, Y = 0, Z = 0 };
    /// <summary>
    /// A vector representing the Y axis: (0, 1, 0).
    /// </summary>
    public static Vector3 AxisY = new Vector3 { X = 0, Y = 1, Z = 0 };
    /// <summary>
    /// A vector representing the Z axis: (0, 0, 1).
    /// </summary>
    public static Vector3 AxisZ = new Vector3 { X = 0, Y = 0, Z = 1 };

    /// <summary>
    /// Creates a vector with all components set to <paramref name="scalar"/>.
    /// </summary>
    /// <param name="scalar">The value for all components.</param>
    /// <returns>The vector (scalar, scalar, scalar).</returns>
    public static Vector3 Scalar(float scalar) { return new Vector3 { X = scalar, Y = scalar, Z = scalar }; }

    /// <summary>
    /// The vector X component.
    /// </summary>
    public float X { get; set; }
    /// <summary>
    /// The vector Y component.
    /// </summary>
    public float Y { get; set; }
    /// <summary>
    /// The vector Z component.
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// Return the squared magnitude of this vector: <c>X * X + Y * Y + Z * Z</c>
    /// </summary>
    public float MagnitudeSquared { get { return X * X + Y * Y + Z * Z; } }
    /// <summary>
    /// Calculate the magnitude of this vector.
    /// </summary>
    public float Magnitude { get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); } }

    /// <summary>
    /// Indexing accessor. Indexes X, Y, Z, W across the range [0, 2].
    /// </summary>
    /// <param name="index">The component index [0, 2].</param>
    /// <returns>The requested component.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/>is out of range.</exception>
    public float this[int index]
    {
      get
      {
        switch (index)
        {
        case 0: return X;
        case 1: return Y;
        case 2: return Z;
        default: break;
        }
        throw new IndexOutOfRangeException();
      }

      set
      {
        switch (index)
        {
        case 0: X = value; break;
        case 1: Y = value; break;
        case 2: Z = value; break;
        default: throw new IndexOutOfRangeException();
        }
      }
    }

    /// <summary>
    /// Construct a new vector with all components set to <paramref name="scalar"/>.
    /// </summary>
    /// <param name="scalar">The value to assign to each component.</param>
    public Vector3(float scalar)
    {
      X = Y = Z = scalar;
    }

    /// <summary>
    /// Construct a vector with the given component values.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public Vector3(float x, float y, float z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    /// <summary>
    /// Test if this vector is exactly zero in all components.
    /// </summary>
    public bool IsZero { get { return this == Zero; } }

    /// <summary>
    /// Return the negation of this vector. The sign of each component is flipped.
    /// </summary>
    public Vector3 Negated { get { return new Vector3 { X = -this.X, Y = -this.Y, Z = -this.Z, }; } }

    /// <summary>
    /// Negate each component of this vector.
    /// </summary>
    /// <remarks>
    /// The vector is not normalised when the magnitude is less than or equal to <see cref="Epsilon"/>.
    /// </remarks>
    public void Negate() { X = -X;  Y = -Y; Z = -Z; }

    /// <summary>
    /// Calculate and return a normalised copy of this vector.
    /// </summary>
    public Vector3 Normalised
    {
      get
      {
        Vector3 v = this;
        v.Normalise();
        return v;
      }
    }

    /// <summary>
    /// Normalise this vector.
    /// </summary>
    /// <remarks>
    /// The vector is left as is if its magnitude is less than or equal to <paramref name="epsilon"/>.
    /// </remarks>
    /// <param name="epsilon">Prevents normalisation of small vectors with magnitudes less than or
    ///     equal to this value.</param>
    /// <returns>The vector magnitude before normalisation.</returns>
    public float Normalise(float epsilon = Epsilon)
    {
      float len = Magnitude;
      if (len > epsilon)
      {
        float lenInv = 1.0f / len;
        X *= lenInv;
        Y *= lenInv;
        Z *= lenInv;
      }
      return len;
    }

    /// <summary>
    /// Calculate the dot product of this vector and another.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The dot product of <c>this</c> and <paramref name="other"/>.</returns>
    public float Dot(Vector3 other)
    {
      return X * other.X + Y * other.Y + Z * other.Z;
    }

    /// <summary>
    /// Calculate the cross product of this vector and another.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The cross product of <c>this</c> and <paramref name="other"/>.</returns>
    public Vector3 Cross(Vector3 other)
    {
      Vector3 v = new Vector3 {
        X = this.Y * other.Z - this.Z * other.Y,
        Y = this.Z * other.X - this.X * other.Z,
        Z = this.X * other.Y - this.Y * other.Z
      };
      return v;
    }

    /// <summary>
    /// Compare two vectors for precise numeric equality.
    /// </summary>
    /// <param name="obj">The vector to compare to.</param>
    /// <returns>True if the vectors are precisely equal.</returns>
    /// <exception cref="InvalidCastException">When <paramref name="obj"/> is not a <see cref="Vector3"/>.</exception>
    public override bool Equals(object obj)
    {
      Vector3 other = (Vector3)obj;
      return this == other;
    }

    /// <summary>
    /// Generates a simple hash code for this vector.
    /// </summary>
    /// <returns>A 32-bit hash code.</returns>
    public override int GetHashCode()
    {
      int hash;
      hash = FloatUtil.ToInt32Bits(X);
      hash = 31 * hash + FloatUtil.ToInt32Bits(Y);
      hash = 31 * hash + FloatUtil.ToInt32Bits(Z);
      return hash;
    }

    /// <summary>
    /// Compare two quaternions for precise numeric equality.
    /// </summary>
    /// <param name="a">A quaternion to compare.</param>
    /// <param name="b">A quaternion to compare.</param>
    /// <returns>True if the quaternions are precisely equal.</returns>
    public static bool operator ==(Vector3 a, Vector3 b)
    {
      return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }

    /// <summary>
    /// Compare two quaternions for inequality using precise numeric equality.
    /// </summary>
    /// <param name="a">A quaternion to compare.</param>
    /// <param name="b">A quaternion to compare.</param>
    /// <returns>True if the quaternions are not precisely equal.</returns>
    public static bool operator !=(Vector3 a, Vector3 b)
    {
      return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
    }

    /// <summary>
    /// Sum two vectors and return the result.
    /// </summary>
    /// <param name="a">An operand.</param>
    /// <param name="b">An operand.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
      return new Vector3 { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
    }

    /// <summary>
    /// Subtract one vectors from another and return the result.
    /// </summary>
    /// <param name="a">An operand.</param>
    /// <param name="b">An operand.</param>
    /// <returns>The difference between <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
      return new Vector3 { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
    }

    /// <summary>
    /// Scale a vector by a scalar value.
    /// </summary>
    /// <param name="v">The vector value.</param>
    /// <param name="s">The scalar value.</param>
    /// <returns>The vector <paramref name="v"/> scaled by <paramref name="s"/>.</returns>
    public static Vector3 operator *(Vector3 v, float s)
    {
      return new Vector3 { X = v.X * s, Y = v.Y * s, Z = v.Z * s };
    }

    /// <summary>
    /// Scale a vector by a scalar value.
    /// </summary>
    /// <param name="s">The scalar value.</param>
    /// <param name="v">The vector value.</param>
    /// <returns>The vector <paramref name="v"/> scaled by <paramref name="s"/>.</returns>
    public static Vector3 operator *(float s, Vector3 v)
    {
      return new Vector3 { X = v.X * s, Y = v.Y * s, Z = v.Z * s };
    }

    /// <summary>
    /// Divide a vector by a scalar value.
    /// </summary>
    /// <param name="v">The vector value.</param>
    /// <param name="s">The scalar value.</param>
    /// <returns>The vector <paramref name="v"/> scaled by the inverse of <paramref name="s"/>.</returns>
    public static Vector3 operator /(Vector3 v, float s)
    {
      float sinv = 1.0f / s;
      return new Vector3 { X = v.X * sinv, Y = v.Y * sinv, Z = v.Z * sinv };
    }
  }
}
