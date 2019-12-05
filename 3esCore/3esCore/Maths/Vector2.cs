using System;

namespace Tes.Maths
{
  /// <summary>
  /// A basic two component vector coordinate implementation.
  /// </summary>
  public struct Vector2
  {
    /// <summary>
    /// Default epsilon value used in various calculations.
    /// </summary>
    public const float Epsilon = 1e-6f;
    /// <summary>
    /// A zero vector (0, 0).
    /// </summary>
    public static Vector2 Zero = new Vector2 { X = 0, Y = 0 };
    /// <summary>
    /// A vector with all components one (1, 1).
    /// </summary>
    public static Vector2 One = new Vector2 { X = 1, Y = 1 };
    /// <summary>
    /// A vector representing the X axis: (1, 0).
    /// </summary>
    public static Vector2 AxisX = new Vector2 { X = 1, Y = 0 };
    /// <summary>
    /// A vector representing the Y axis: (0, 1).
    /// </summary>
    public static Vector2 AxisY = new Vector2 { X = 0, Y = 1 };

    /// <summary>
    /// Creates a vector with all components set to <paramref name="scalar"/>.
    /// </summary>
    /// <param name="scalar">The value for all components.</param>
    /// <returns>The vector (scalar, scalar).</returns>
    public static Vector2 Scalar(float scalar) { return new Vector2 { X = scalar, Y = scalar }; }

    /// <summary>
    /// The vector X component.
    /// </summary>
    public float X { get; set; }
    /// <summary>
    /// The vector Y component.
    /// </summary>
    public float Y { get; set; }
    /// <summary>
    /// Return the squared magnitude of this vector: <c>X * X + Y * Y</c>
    /// </summary>
    public float MagnitudeSquared { get { return X * X + Y * Y; } }
    /// <summary>
    /// Calculate the magnitude of this vector.
    /// </summary>
    public float Magnitude { get { return (float)Math.Sqrt(X * X + Y * Y); } }

    /// <summary>
    /// Indexing accessor. Indexes X, Y, Z, W across the range [0, 1].
    /// </summary>
    /// <param name="index">The component index [0, 1].</param>
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
        default: throw new IndexOutOfRangeException();
        }
      }
    }

    /// <summary>
    /// Construct a new vector with all components set to <paramref name="scalar"/>.
    /// </summary>
    /// <param name="scalar">The value to assign to each component.</param>
    public Vector2(float scalar)
    {
      X = Y = scalar;
    }

    /// <summary>
    /// Construct a vector with the given component values.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public Vector2(float x, float y)
    {
      X = x;
      Y = y;
    }

    /// <summary>
    /// Test if this vector is exactly zero in all components.
    /// </summary>
    public bool IsZero { get { return this == Zero; } }

    /// <summary>
    /// Return the negation of this vector. The sign of each component is flipped.
    /// </summary>
    public Vector2 Negated { get { return new Vector2 { X = -this.X, Y = -this.Y }; } }

    /// <summary>
    /// Negate each component of this vector.
    /// </summary>
    public void Negate() { X = -X; Y = -Y; }

    /// <summary>
    /// Calculate and return a normalised copy of this vector.
    /// </summary>
    /// <remarks>
    /// The vector is not normalised when the magnitude is less than or equal to <see cref="Epsilon"/>.
    /// </remarks>
    public Vector2 Normalised
    {
      get
      {
        Vector2 v = this;
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
      }
      return len;
    }

    //public float Dot(Vector2 other)
    //{
    //  return X * other.X + Y * other.Y;
    //}

    /// <summary>
    /// Compare two vectors for precise numeric equality.
    /// </summary>
    /// <param name="obj">The vector to compare to.</param>
    /// <returns>True if the vectors are precisely equal.</returns>
    /// <exception cref="InvalidCastException">When <paramref name="obj"/> is not a <see cref="Vector2"/>.</exception>
    public override bool Equals(object obj)
    {
      Vector2 other = (Vector2)obj;
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
      return hash;
    }

    /// <summary>
    /// Compare two quaternions for precise numeric equality.
    /// </summary>
    /// <param name="a">A quaternion to compare.</param>
    /// <param name="b">A quaternion to compare.</param>
    /// <returns>True if the quaternions are precisely equal.</returns>
    public static bool operator ==(Vector2 a, Vector2 b)
    {
      return a.X == b.X && a.Y == b.Y;
    }

    /// <summary>
    /// Compare two quaternions for inequality using precise numeric equality.
    /// </summary>
    /// <param name="a">A quaternion to compare.</param>
    /// <param name="b">A quaternion to compare.</param>
    /// <returns>True if the quaternions are not precisely equal.</returns>
    public static bool operator !=(Vector2 a, Vector2 b)
    {
      return a.X != b.X || a.Y != b.Y;
    }

    /// <summary>
    /// Sum two vectors and return the result.
    /// </summary>
    /// <param name="a">An operand.</param>
    /// <param name="b">An operand.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
      return new Vector2 { X = a.X + b.X, Y = a.Y + b.Y };
    }

    /// <summary>
    /// Subtract one vectors from another and return the result.
    /// </summary>
    /// <param name="a">An operand.</param>
    /// <param name="b">An operand.</param>
    /// <returns>The difference between <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
      return new Vector2 { X = a.X - b.X, Y = a.Y - b.Y };
    }

    /// <summary>
    /// Scale a vector by a scalar value.
    /// </summary>
    /// <param name="v">The vector value.</param>
    /// <param name="s">The scalar value.</param>
    /// <returns>The vector <paramref name="v"/> scaled by <paramref name="s"/>.</returns>
    public static Vector2 operator *(Vector2 v, float s)
    {
      return new Vector2 { X = v.X * s, Y = v.Y * s };
    }

    /// <summary>
    /// Scale a vector by a scalar value.
    /// </summary>
    /// <param name="s">The scalar value.</param>
    /// <param name="v">The vector value.</param>
    /// <returns>The vector <paramref name="v"/> scaled by <paramref name="s"/>.</returns>
    public static Vector2 operator *(float s, Vector2 v)
    {
      return new Vector2 { X = v.X * s, Y = v.Y * s };
    }

    /// <summary>
    /// Divide a vector by a scalar value.
    /// </summary>
    /// <param name="v">The vector value.</param>
    /// <param name="s">The scalar value.</param>
    /// <returns>The vector <paramref name="v"/> scaled by the inverse of <paramref name="s"/>.</returns>
    public static Vector2 operator /(Vector2 v, float s)
    {
      float sinv = 1.0f / s;
      return new Vector2 { X = v.X * sinv, Y = v.Y * sinv };
    }
  }
}
