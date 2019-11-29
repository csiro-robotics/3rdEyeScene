using System;

namespace Tes.Maths
{
  /// <summary>
  /// A quaternion implementation for applying 3D rotations.
  /// </summary>
  /// <remarks>
  /// See <a href="https://en.wikipedia.org/wiki/Quaternion">Wikipedia</a> or
  /// <a href="http://mathworld.wolfram.com/Quaternion.html">Wolfram MathWorld</a> for
  /// technical details on quaternions.
  ///
  /// Note that all angles expressed by this class are in radians.
  /// </remarks>
  public struct Quaternion
  {
    /// <summary>
    /// Default epsilon value used in various calculations.
    /// </summary>
    public const float Epsilon = 1e-6f;
    /// <summary>
    /// The identity quaternion (0, 0, 0, 1).
    /// </summary>
    public static Quaternion Identity = new Quaternion { X = 0, Y = 0, Z = 0, W = 1 };
    /// <summary>
    /// A zero quaternion (0, 0, 0, 0).
    /// </summary>
    public static Quaternion Zero = new Quaternion { X = 0, Y = 0, Z = 0, W = 0 };

    /// <summary>
    /// The quaternion X component.
    /// </summary>
    public float X { get; set; }
    /// <summary>
    /// The quaternion Y component.
    /// </summary>
    public float Y { get; set; }
    /// <summary>
    /// The quaternion Z component.
    /// </summary>
    public float Z { get; set; }
    /// <summary>
    /// The quaternion W component.
    /// </summary>
    public float W { get; set; }

    /// <summary>
    /// Initialise a quaternion with the given component values.
    /// </summary>
    /// <param name="x">X component value.</param>
    /// <param name="y">Y component value.</param>
    /// <param name="z">Z component value.</param>
    /// <param name="w">W component value.</param>
    public Quaternion(float x, float y, float z, float w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    /// <summary>
    /// Initialise a quaternion using an axis and an angle.
    /// </summary>
    /// <param name="axis">The quaternion rotation axis. Must be a unity vector for well defined behaviour.</param>
    /// <param name="angle">The rotation angle around <paramref name="axis"/> (radians).</param>
    public Quaternion(Vector3 axis, float angle)
    {
      X = Y = Z = 0; W = 1;
      SetAxisAngle(axis, angle);
    }

    /// <summary>
    /// Indexing accessor. Indexes X, Y, Z, W across the range [0, 3].
    /// </summary>
    /// <param name="index">The component index [0, 3].</param>
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
        case 3: return W;
        default:
          throw new IndexOutOfRangeException();
        }
      }

      set
      {
        switch (index)
        {
        case 0: X = value; break;
        case 1: Y = value; break;
        case 2: Z = value; break;
        case 3: W = value; break;
        default:
          throw new IndexOutOfRangeException();
        }
      }
    }

    /// <summary>
    /// Is this quaternion exactly zero (0, 0, 0, 0)?
    /// </summary>
    public bool IsZero { get { return this == Zero; } }
    /// <summary>
    /// Is this quaternion exactly identity (0, 0, 0, 1)?
    /// </summary>
    public bool IsIdentity { get { return this == Identity; } }

    /// Returns the magnitude of this quaternion.
    public float Magnitude { get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W); } }

    /// Returns the magnitude squared of this quaternion.
    public float MagnitudeSquared { get { return X * X + Y * Y + Z * Z + W * W; } }

    /// <summary>
    /// Initialise this quaternion as the rotation between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The vector to rotate from. Should ideally be normalised.</param>
    /// <param name="to">The vector to rotate to. Should ideally be normalised.</param>
    public void SetFromTo(Vector3 from, Vector3 to)
    {
      Vector3 half = from + to;
      half.Normalise();
      Vector3 v = from.Cross(half);
      X = v.X;
      Y = v.Y;
      Z = v.Z;
      W = from.Dot(half);
    }

    /// <summary>
    /// Static method for constructing a quaternion as a rotation between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The vector to rotate from. Should ideally be normalised.</param>
    /// <param name="to">The vector to rotate to. Should ideally be normalised.</param>
    /// <returns>The rotation quaternion between the given vectors.</returns>
    public static Quaternion FromTo(Vector3 from, Vector3 to)
    {
      Quaternion q = new Quaternion();
      q.SetFromTo(from, to);
      return q;
    }

    /// <summary>
    /// Converts this quaternion into a axis of rotation and the rotation angle around that axis (radians).
    /// </summary>
    /// <param name="angle"> Set to the rotation angle (radians). Zero if this quaternion is identity.</param>
    /// <param name="axis">Set to the axis of rotation. Set to (0, 0, 1) if this quaternion is identity or near zero length.</param>
    public void GetAxisAngle(ref Vector3 axis, ref float angle)
    {
      float mag = X * X + Y * Y + Z * Z;

      if (mag <= Vector3.Epsilon)
      {
        axis = Vector3.AxisZ;
        angle = 0;
        return;
      }

      float magInv = 1.0f / mag;
      axis = new Vector3 { X = X * magInv, Y = Y * magInv, Z = Z * magInv };
      angle = 2.0f * (float)Math.Acos(W);
    }

    /// <summary>
    /// Sets this quaternion from an axis of rotation and the angle of rotation about that axis (radians).
    /// </summary>
    /// <param name="axis">The axis of rotation. Must be a unit vector.</param>
    /// <param name="angle">The rotation angle around <paramref name="axis"/> (radians).</param>
    public void SetAxisAngle(Vector3 axis, float angle)
    {
      float sinHalfAngle = (float)Math.Sin(0.5f * angle);
      X = axis.X * sinHalfAngle;
      Y = axis.Y * sinHalfAngle;
      Z = axis.Z * sinHalfAngle;
      W = (float)Math.Cos(0.5f * angle);
      Normalise();
    }

    /// <summary>
    /// Creates a quaternion from an axis an angle. This provides a static alternative
    /// to <see cref="SetAxisAngle(Vector3, float)"/>.
    /// </summary>
    /// <param name="axis">The axis of rotation. Must be a unit vector.</param>
    /// <param name="angle">The rotation angle around <paramref name="axis"/> (radians).</param>
    /// <returns>The quaternion expressing the desired axis rotation.</returns>
    public static Quaternion AxisAngle(Vector3 axis, float angle)
    {
      Quaternion q = new Quaternion();
      q.SetAxisAngle(axis, angle);
      return q;
    }

    /// <summary>
    /// Inverts this quaternion so that it expresses the counter rotation to its current value.
    /// </summary>
    public void Invert()
    {
      float mag2 = MagnitudeSquared;
      Conjugate();
      this *= 1.0f / mag2;
    }

    /// <summary>
    /// Calculates and returns the inverse, or counter rotation, of this quaternion.
    /// </summary>
    /// <returns>
    /// The inverse of this quaternion.
    /// </returns>
    public Quaternion Inverse
    {
      get
      {
        Quaternion q = new Quaternion();
        q = this;
        q.Invert();
        return q;
      }
    }

    /// <summary>
    /// Sets this quaternion to its conjugate.
    /// </summary>
    /// <remarks>
    /// The conjugate is the same quaternion with x, y, z values negated, but
    /// w remains as is.
    /// </remarks>
    public void Conjugate()
    {
      X = -X;
      Y = -Y;
      Z = -Z;
    }

    /// <summary>
    /// Calculates and returns the conjugate of this quaternion.
    /// </summary>
    /// <remarks>
    /// The conjugate is the same quaternion with x, y, z values negated, but
    /// w remains as is.
    /// </remarks>
    /// <returns>This quaternion's conjugate.</returns>
    public Quaternion Conjugated
    {
      get
      {
        Quaternion q = new Quaternion();
        q = this;
        q.Conjugate();
        return q;
      }
    }

    /// <summary>
    /// Attempts to normalise this quaternion.
    /// </summary>
    /// <remarks>
    /// Normalisation fails if the length of this quaternion is less than or
    /// equal to <paramref name="epsilon"/>. In this case, the quaternion becomes identity.
    /// </remarks>
    /// <returns>The magnitude of this quaternion before normalisation or
    /// zero if normalisation failed.</returns>
    public float Normalise(float epsilon = Epsilon)
    {
      float mag = Magnitude;
      if (mag <= epsilon)
      {
        this = Identity;
        return 0;
      }

      float magInv = 1.0f / mag;
      X *= magInv;
      Y *= magInv;
      Z *= magInv;
      W *= magInv;

      return mag;
    }

    /// <summary>
    /// Returns a normalised copy of this quaternion.
    /// </summary>
    /// <remarks>
    /// Normalisation fails if the length of this quaternion is less than or
    /// equal to <see cref="Epsilon"/>.
    /// </remarks>
    /// <returns>A normalised copy of this quaternion, or a zero quaternion if
    /// if normalisation failed.</returns>
    public Quaternion normalised
    {
      get
      {
        Quaternion q = new Quaternion();
        q = this;
        q.Normalise();
        return q;
      }
    }

    /// <summary>
    /// Calculates the dot product of <code>this</code>and <paramref name="other"/>.
    /// </summary>
    /// <returns>The dot product.</returns>
    public float Dot(Quaternion other)
    {
      return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
    }

    /// <summary>
    /// Transforms <paramref name="v"/> by this quaternion rotation.
    /// </summary>
    /// <remarks>
    /// The transformation signifies a rotation of <paramref name="v"/> by this
    /// quaternion.
    /// </remarks>
    /// <returns>The transformed vector.</returns>
    public Vector3 Transform(Vector3 v)
    {
      float xx = X * X, xy = X * Y, xz = X * Z, xw = X * W;
      float yy = Y * Y, yz = Y * Z, yw = Y * W;
      float zz = Z * Z, zw = Z * W;

      Vector3 res = new Vector3();

      res.X = (1 - 2 * (yy + zz)) * v.X +
              (2 * (xy - zw)) * v.Y +
              (2 * (xz + yw)) * v.Z;

      res.Y = (2 * (xy + zw)) * v.X +
              (1 - 2 * (xx + zz)) * v.Y +
              (2 * (yz - xw)) * v.Z;

      res.Z = (2 * (xz - yw)) * v.X +
              (2 * (yz + xw)) * v.Y +
              (1 - 2 * (xx + yy)) * v.X;

      return res;
    }

    /// <summary>
    /// Multiply all components of this quaternion by a scalar.
    /// </summary>
    /// <remarks>
    /// Generally not a very useful operation for a quaternion as
    /// quaternions should stay normalised.
    /// </remarks>
    /// <param name="scalar">The scalar to multiply the vector components by.</param>
    public void Multiply(float scalar)
    {
      X *= scalar;
      Y *= scalar;
      Z *= scalar;
      W *= scalar;
    }

    /// <summary>
    /// Performs a spherical linear interpolation of one quaternion to another.
    /// </summary>
    /// <remarks>
    /// This results in a quaternion which rotates partway between <paramref name="from"/>
    /// and <paramref name="to"/>.
    ///
    /// This is an ambiguity in rotation when <paramref name="from"/> and <paramref name="to"/>
    /// are exactly opposed and the resulting quaternion is not well defined.
    /// </remarks>
    /// <param name="from">The quaternion rotation to interpolate from.</param>
    /// <param name="to">The quaternion rotation to interpolate to.</param>
    /// <param name="t">The interpolation "time", [0, 1]. Zero selects <paramref name="from"/>
    ///   while one selects <paramref name="to"/>.</param>
    /// <returns>The interpolated result.</returns>
    public static Quaternion Slerp(Quaternion from, Quaternion to, float t)
    {
      float dCoeff0, dCoeff1, dAngle, dSin, dCos, dInvSin;

      if (from == to)
      {
        return from;
      }

      dCos = from.Dot(to);

      Quaternion temp = new Quaternion();

      // numerical round-off error could create problems in call to acos
      if (dCos < 0)
      {
        dCos = -dCos;
        temp.X = -to.X;
        temp.Y = -to.Y;
        temp.Z = -to.Z;
        temp.W = -to.W;
      }
      else
      {
        temp.X = to.X;
        temp.Y = to.Y;
        temp.Z = to.Z;
        temp.W = to.W;
      }

      if ((1.0f - dCos) > Vector3.Epsilon)
      {
        dAngle = (float)Math.Acos(dCos);
        dSin = (float)Math.Sin(dAngle);  // fSin >= 0 since fCos >= 0

        dInvSin = 1.0f / dSin;
        dCoeff0 = (float)Math.Sin((1.0f - t) * dAngle) * dInvSin;
        dCoeff1 = (float)Math.Sin(t * dAngle) * dInvSin;
      }
      else
      {
        dCoeff0 = 1.0f - t;
        dCoeff1 = t;
      }

      temp.X = dCoeff0 * from.X + dCoeff1 * temp.X;
      temp.Y = dCoeff0 * from.Y + dCoeff1 * temp.Y;
      temp.Z = dCoeff0 * from.Z + dCoeff1 * temp.Z;
      temp.W = dCoeff0 * from.W + dCoeff1 * temp.W;

      return temp;
    }

    /// <summary>
    /// Compare two quaternions for precise numeric equality.
    /// </summary>
    /// <param name="obj">The quaternion to compare to.</param>
    /// <returns>True if the quaternions are precisely equal.</returns>
    /// <exception cref="InvalidCastException">When <paramref name="obj"/> is not a <see cref="Quaternion"/>.</exception>
    public override bool Equals(object obj)
    {
      Quaternion other = (Quaternion)obj;
      return this == other;
    }

    /// <summary>
    /// Generates a simple hash code for the quaternion.
    /// </summary>
    /// <remarks>Not a very good hash.</remarks>
    /// <returns>The hash code for this quaternion.</returns>
    public override int GetHashCode()
    {
      int hash;
      hash = FloatUtil.ToInt32Bits(X);
      hash = 31 * hash + FloatUtil.ToInt32Bits(Y);
      hash = 31 * hash + FloatUtil.ToInt32Bits(Z);
      hash = 31 * hash + FloatUtil.ToInt32Bits(W);
      return hash;
    }

    /// <summary>
    /// Compare two quaternions for precise numeric equality.
    /// </summary>
    /// <param name="a">A quaternion to compare.</param>
    /// <param name="b">A quaternion to compare.</param>
    /// <returns>True if the quaternions are precisely equal.</returns>
    public static bool operator ==(Quaternion a, Quaternion b)
    {
      return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
    }

    /// <summary>
    /// Compare two quaternions for inequality using precise numeric equality.
    /// </summary>
    /// <param name="a">A quaternion to compare.</param>
    /// <param name="b">A quaternion to compare.</param>
    /// <returns>True if the quaternions are not precisely equal.</returns>
    public static bool operator !=(Quaternion a, Quaternion b)
    {
      return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
    }

    /// <summary>
    /// Transform (rotate) one quaternion by another.
    /// </summary>
    /// <remarks>
    /// The resulting quaternion represents a combined rotation,
    /// applying <paramref name="b"/> then <paramref name="a"/>.
    /// </remarks>
    /// <param name="a">The quaternion to rotate by.</param>
    /// <param name="b">The quaternion to rotate</param>
    /// <returns>The quaternion <paramref name="a"/> rotated by <paramref name="b"/>.</returns>
    public static Quaternion operator *(Quaternion a, Quaternion b)
    {
      return new Quaternion {
        X = a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        Y = a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        Z = a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
        W = a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
      };
    }

    /// <summary>
    /// Transform (rotate) a vector by a quaternion.
    /// </summary>
    /// <param name="q">The quaternion to rotate by.</param>
    /// <param name="v">The vector to rotate.</param>
    /// <returns>The vector <paramref name="v"/> rotated by <paramref name="q"/>.</returns>
    public static Vector3 operator *(Quaternion q, Vector3 v) { return q.Transform(v); }

    /// <summary>
    /// Multiply a quaternion by a scalar.
    /// </summary>
    /// <remarks>
    /// Not a very useful operation for a quaternion as they should stay normalised.
    /// Provided for completeness.
    /// </remarks>
    /// <param name="q">The quaternion to operate on.</param>
    /// <param name="s">The scalar to multiply by.</param>
    /// <returns>The scaled quaternion.</returns>
    public static Quaternion operator *(Quaternion q, float s)
    {
      return new Quaternion { X = q.X * s, Y = q.Y * s, Z = q.Z * s };
    }

    /// <summary>
    /// Multiply a quaternion by a scalar.
    /// </summary>
    /// <remarks>
    /// Not a very useful operation for a quaternion as they should stay normalised.
    /// Provided for completeness.
    /// </remarks>
    /// <param name="s">The scalar to multiply by.</param>
    /// <param name="q">The quaternion to operate on.</param>
    /// <returns>The scaled quaternion.</returns>
    public static Quaternion operator *(float s, Quaternion q)
    {
      return new Quaternion { X = q.X * s, Y = q.Y * s, Z = q.Z * s, W = q.W * s };
    }

    /// <summary>
    /// Divide a quaternion by a scalar.
    /// </summary>
    /// <remarks>
    /// Not a very useful operation for a quaternion as they should stay normalised.
    /// Provided for completeness.
    /// </remarks>
    /// <param name="q">The quaternion to operate on.</param>
    /// <param name="s">The scalar to divide by.</param>
    /// <returns>The scaled quaternion.</returns>
    public static Quaternion operator /(Quaternion q, float s)
    {
      float sinv = 1.0f / s;
      return new Quaternion { X = q.X * sinv, Y = q.Y * sinv, Z = q.Z * sinv, W = q.W * sinv };
    }
  }
}
