using System;

namespace Tes.Maths
{
  ///<summary>
  /// A row major 3x3 rotation matrix.
  /// </summary>
  /// <remarks>
  /// The matrix is laid out as follows:
  /// 
  /// <code>
  ///     | m00  m01  m02 |   |  0   1   2 |   | xx  yx  zx |
  /// M = | m10  m11  m12 | = |  3   4   5 | = | xy  yy  zy |
  ///     | m20  m21  m22 |   |  6   7   8 |   | xz  yz  zz |
  /// </code>
  /// Where (xx, xy, xz) are the components of the X axis. Similarly, yn and zn
  /// form the Y axis and Z axis of the basis vectors respectively. Finally,
  /// (tx, ty, tz) is the translation.
  /// </remarks>
  public unsafe struct Matrix3
  {
    /// <summary>
    /// The matrix array.
    /// </summary>
    private fixed float _m[9];

    /// <summary>
    /// A matrix with all zero components.
    /// </summary>
    public static Matrix3 Zero = All(0.0f);
    /// <summary>
    /// The 3x3 identity matrix.
    /// </summary>
    public static Matrix3 Identity = IdentityMatrix();

    /// <summary>
    /// Create a matrix with all components set to <paramref name="val"/>.
    /// </summary>
    /// <param name="val">The value to assign to each component.</param>
    /// <returns>The matrix with all components <paramref name="val"/>.</returns>
    public static Matrix3 All(float val)
    {
      Matrix3 mat = new Matrix3();
      for (int i = 0; i < 16; ++i)
      {
        mat._m[i] = val;
      }
      return mat;
    }

    /// <summary>
    /// The 3x3 identity matrix.
    /// </summary>
    /// <returns>The identity matrix.</returns>
    public static Matrix3 IdentityMatrix()
    {
      Matrix3 mat = All(0.0f);
      mat[0, 0] = mat[1, 1] = mat[2, 2] = 1.0f;
      return mat;
    }

    /// <summary>
    /// Indexing accessor across the range [0, 8].
    /// </summary>
    /// <remarks>
    /// See class documentation on the matrix layout.
    /// </remarks>
    /// <param name="index">The component index [0, 8].</param>
    /// <returns>The requested component.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/>is out of range.</exception>
    public float this[int index]
    {
      get
      {
        if (index < 0 || index > 8)
        {
          throw new IndexOutOfRangeException();
        }
        fixed (float *m = _m)
        {
          return m[index];
        }
      }

      set
      {
        if (index < 0 || index > 8)
        {
          throw new IndexOutOfRangeException();
        }
        fixed (float* m = _m)
        {
          m[index] = value;
        }
      }
    }

    /// <summary>
    /// Row/column indexing accessor. Valid row and column ranges are: [0, 2].
    /// </summary>
    /// <remarks>
    /// See class documentation on the matrix layout.
    /// </remarks>
    /// <param name="r">The row index [0, 2].</param>
    /// <param name="c">The column index [0, 2].</param>
    /// <returns>The requested component.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="r"/> or <paramref name="c"/> are out of range.</exception>
    public float this[int r, int c]
    {
      get
      {
        if (r < 0 || r > 2 || c < 0 || c > 2)
        {
          throw new IndexOutOfRangeException();
        }
        fixed (float* m = _m)
        {
          return m[r * 3 + c];
        }
      }

      set
      {
        if (r < 0 || r > 2 || c < 0 || c > 2)
        {
          throw new IndexOutOfRangeException();
        }
        fixed (float* m = _m)
        {
          m[r * 3 + c] = value;
        }
      }
    }

    /// <summary>
    /// Calculate the determinant of this matrix.
    /// </summary>
    public float Determinant
    {
      get
      {
        fixed (float *m = _m)
        {
          return m[0] * m[4] * m[8] + m[1] * m[5] * m[6] + m[2] * m[3] * m[7] -
                 m[2] * m[4] * m[6] - m[1] * m[3] * m[8] - m[0] * m[5] * m[7];
        }
      }
    }

    /// <summary>
    /// Get/set the conceptual X axis from this matrix.
    /// </summary>
    /// <remarks>
    /// This equates to column 0.
    /// </remarks>
    public Vector3 AxisX { get { return GetAxis(0); } set { SetAxis(0, value); } }
    /// <summary>
    /// Get/set the conceptual Y axis from this matrix.
    /// </summary>
    /// <remarks>
    /// This equates to column 1.
    /// </remarks>
    public Vector3 AxisY { get { return GetAxis(1); } set { SetAxis(1, value); } }
    /// <summary>
    /// Get/set the conceptual Z axis from this matrix.
    /// </summary>
    /// <remarks>
    /// This equates to column 2.
    /// </remarks>
    public Vector3 AxisZ { get { return GetAxis(2); } set { SetAxis(2, value); } }

    /// <summary>
    /// Calculate the per axis scaling of this matrix.
    /// </summary>
    public Vector3 Scale
    {
      get
      {
        return new Vector3
        {
          X = AxisX.Magnitude,
          Y = AxisY.Magnitude,
          Z = AxisZ.Magnitude,
        };
      }
    }

    /// <summary>
    /// Requests an axis or column from the matrix, dropping the last row.
    /// </summary>
    /// <param name="index">The axis/column index [0, 2]</param>
    /// <returns>The requested axis.</returns>
    public Vector3 GetAxis(int index)
    {
      return new Vector3 { X = this[0, index], Y = this[1, index], Z = this[2, index] };
    }

    /// <summary>
    /// Set an axis or column from the matrix.
    /// </summary>
    /// <param name="index">The axis/column index [0, 2]</param>
    /// <param name="axis">The axis value.</param>
    public void SetAxis(int index, Vector3 axis)
    {
      this[0, index] = axis[0];
      this[1, index] = axis[1];
      this[2, index] = axis[2];
    }

    /// <summary>
    /// Builds a rotation matrix around the X axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3 RotationX(float angle)
    {
      Matrix3 mat = new Matrix3();
      mat.InitRotationX(angle);
      return mat;
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix around the X axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    public void InitRotationX(float angle)
    {
      this = Identity;
      float s = (float)Math.Sin(angle);
      float c = (float)Math.Cos(angle);
      this[4] = this[8] = c;
      this[5] = -s; this[7] = s;
    }

    /// <summary>
    /// Builds a rotation matrix around the Y axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3 RotationY(float angle)
    {
      Matrix3 mat = new Matrix3();
      mat.InitRotationY(angle);
      return mat;
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix around the Y axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    public void InitRotationY(float angle)
    {
      this = Identity;
      float s = (float)Math.Sin(angle);
      float c = (float)Math.Cos(angle);
      this[0] = this[8] = c;
      this[6] = -s; this[2] = s;
    }

    /// <summary>
    /// Builds a rotation matrix around the Z axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3 RotationZ(float angle)
    {
      Matrix3 mat = new Matrix3();
      mat.InitRotationZ(angle);
      return mat;
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix around the Z axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    public void InitRotationZ(float angle)
    {
      this = Identity;
      float s = (float)Math.Sin(angle);
      float c = (float)Math.Cos(angle);
      this[0] = this[3] = c;
      this[1] = -s; this[4] = s;
    }

    /// <summary>
    /// Initialise this matrix from the given Euler angles.
    /// </summary>
    /// <param name="angleX">The rotation around the X axis (radians).</param>
    /// <param name="angleY">The rotation around the Y axis (radians).</param>
    /// <param name="angleZ">The rotation around the Z axis (radians).</param>
    public void InitRotation(float angleX, float angleY, float angleZ)
    {
      InitRotationZ(angleZ);
      this = RotationY(angleY) * this;
      this = RotationX(angleX) * this;
    }

    /// <summary>
    /// Build a rotation matrix from the given Euler angles.
    /// </summary>
    /// <param name="angleX">The rotation around the X axis (radians).</param>
    /// <param name="angleY">The rotation around the Y axis (radians).</param>
    /// <param name="angleZ">The rotation around the Z axis (radians).</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3 Rotation(float angleX, float angleY, float angleZ)
    {
      Matrix3 mat = new Matrix3();
      mat.InitRotation(angleX, angleY, angleZ);
      return mat;
    }

    /// <summary>
    /// Initialise this matrix as a scaling matrix.
    /// </summary>
    /// <param name="scale">The scaling factor. Each coordinate corresponds to
    /// a column of the matrix.</param>
    public void InitScaling(Vector3 scale)
    {
      this = All(0.0f);
      this[0, 0] = scale.X;
      this[1, 1] = scale.Y;
      this[2, 2] = scale.Z;
    }

    /// <summary>
    /// Initialise a scaling matrix.
    /// </summary>
    /// <param name="scale">The scaling factor. Each coordinate corresponds to
    /// a column of the matrix.</param>
    public static Matrix3 Scaling(Vector3 scale)
    {
      Matrix3 mat = All(0.0f);
      mat[0, 0] = scale.X;
      mat[1, 1] = scale.Y;
      mat[2, 2] = scale.Z;
      return mat;
    }

    /// <summary>
    /// Scales the matrix basis vectors.
    /// </summary>
    /// <param name="scaling">Per axis scaling. Each coordinate corresponds to
    /// a column of the matrix.</param>
    public void ApplyScaling(Vector3 scaling)
    {
      this[0, 0] *= scaling.X;
      this[1, 0] *= scaling.X;
      this[2, 0] *= scaling.X;

      this[0, 1] *= scaling.Y;
      this[1, 1] *= scaling.Y;
      this[2, 1] *= scaling.Y;

      this[0, 2] *= scaling.Z;
      this[1, 2] *= scaling.Z;
      this[2, 2] *= scaling.Z;
    }

    /// <summary>
    /// Removes scaling from the matrix basis vectors.
    /// </summary>
    /// <returns>The scale before modification.</returns>
    public Vector3 RemoveScale()
    {
      Vector3 scale = Scale;
      ApplyScaling(new Vector3 { X = 1.0f / scale.X, Y = 1.0f / scale.Y, Z = 1.0f / scale.Z });
      return scale;
    }

    /// <summary>
    /// Initialise this matrix as a model or camera matrix.
    /// <see cref="LookAt(Vector3, Vector3, Vector3, int, int)"/>.
    /// </summary>
    /// <param name="eye"> The position of the eye/camera.</param>
    /// <param name="target"> The target to look at. Should not be equal to <paramref name="eye"/>.</param>
    /// <param name="axisUp"> The axis defining the initial up vector. Must be normalised.</param>
    /// <param name="forwardAxisIndex"> The index of the forward axis. This is to point at <paramref name="target"/>.</param>
    /// <param name="upAxisIndex"> The index of the up axis. Must not be equal to <paramref name="forwardAxisIndex"/>.</param>
    public void InitLookAt(Vector3 eye, Vector3 target, Vector3 axisUp, int forwardAxisIndex = 1, int upAxisIndex = 2)
    {
      this = LookAt(eye, target, axisUp, forwardAxisIndex, upAxisIndex);      
    }

    /// <summary>
    /// Create a model or camera matrix at <paramref name="eye"/> looking at <paramref name="target"/>.
    /// </summary>
    /// <remarks>
    /// Supports specifying the up and forward axes (inferring the left/right axis),
    /// where the indices [0, 1, 2] correspond to the axes (X, Y, Z).
    ///
    /// The default behaviour is to use Y as the forward axis and Z as up.
    ///
    /// Note: the resulting matrix can only represent the rotation part of the matrix and
    /// the <paramref name="eye"/> translation is essentially dropped. Use <see cref="Matrix4"/> where the translation
    /// is required.
    /// </remarks>
    /// <param name="eye"> The position of the eye/camera.</param>
    /// <param name="target"> The target to look at. Should not be equal to <paramref name="eye"/>.</param>
    /// <param name="axisUp"> The axis defining the initial up vector. Must be normalised.</param>
    /// <param name="forwardAxisIndex"> The index of the forward axis. This is to point at <paramref name="target"/>.</param>
    /// <param name="upAxisIndex"> The index of the up axis. Must not be equal to <paramref name="forwardAxisIndex"/>.</param>
    /// <returns>A model matrix at <paramref name="eye"/> pointing at <paramref name="target"/>. Returns identity if
    /// there are errors in the specification of <paramref name="forwardAxisIndex"/> and <paramref name="upAxisIndex"/>.</returns>
    public static Matrix3 LookAt(Vector3 eye, Vector3 target, Vector3 axisUp, int forwardAxisIndex = 1, int upAxisIndex = 2)
    {
      if (forwardAxisIndex == upAxisIndex ||
          forwardAxisIndex < 0 || forwardAxisIndex > 2 ||
          upAxisIndex < 0 || upAxisIndex > 2)
      {
        // Bad axis specification.
        throw new IndexOutOfRangeException();
      }

      Vector3[] axes = new Vector3[3];
      int sideAxisIndex = 0;
      if (forwardAxisIndex == 1 && upAxisIndex == 2 ||
          upAxisIndex == 1 && forwardAxisIndex == 2)
      {
        sideAxisIndex = 0;
      }
      if (forwardAxisIndex == 0 && upAxisIndex == 2 ||
          upAxisIndex == 0 && forwardAxisIndex == 2)
      {
        sideAxisIndex = 1;
      }
      if (forwardAxisIndex == 0 && upAxisIndex == 1 ||
          upAxisIndex == 0 && forwardAxisIndex == 1)
      {
        sideAxisIndex = 2;
      }
      axes[forwardAxisIndex] = (target - eye).Normalised;
      axes[sideAxisIndex] = axes[forwardAxisIndex].Cross(axisUp).Normalised;
      axes[upAxisIndex] = axes[sideAxisIndex].Cross(axes[forwardAxisIndex]);

      Matrix3 mat = Identity;
      mat.SetAxis(sideAxisIndex, axes[sideAxisIndex]);
      mat.SetAxis(forwardAxisIndex, axes[forwardAxisIndex]);
      mat.SetAxis(upAxisIndex, axes[upAxisIndex]);

      return mat;
    }

    /// <summary>
    /// Transpose this matrix.
    /// </summary>
    public void Transpose()
    {
      float temp;
      temp = this[0, 1];
      this[0, 1] = this[1, 0];
      this[1, 0] = temp;

      temp = this[0, 2];
      this[0, 2] = this[2, 0];
      this[2, 0] = temp;

      temp = this[1, 2];
      this[1, 2] = this[2, 1];
      this[2, 1] = temp;
    }

    /// <summary>
    /// Return the transpose of this matrix.
    /// </summary>
    public Matrix3 Transposed
    {
      get
      {
        Matrix3 trans = this;
        trans.Transpose();
        return trans;
      }
    }

    /// <summary>
    /// Calculate the adjoint for this matrix into <paramref name="adj"/>.
    /// </summary>
    /// <param name="adj">The matrix in which to store the result.</param>
    /// <returns>The determinant of this matrix.</returns>
    public float GetAdjoint(ref Matrix3 adj)
    {
      fixed (float *a = adj._m, m = _m)
      {
        a[0] = m[4] * m[8] - m[7] * m[5];
        a[1] = m[7] * m[2] - m[1] * m[8];
        a[2] = m[1] * m[5] - m[4] * m[2];
        a[3] = m[6] * m[5] - m[3] * m[8];
        a[4] = m[0] * m[8] - m[6] * m[2];
        a[5] = m[3] * m[2] - m[0] * m[5];
        a[6] = m[3] * m[7] - m[6] * m[4];
        a[7] = m[6] * m[1] - m[0] * m[7];
        a[8] = m[0] * m[4] - m[3] * m[1];

        return m[0] * a[0] + m[1] * a[3] + m[2] * a[6];
      }
    }

    /// <summary>
    /// Inverse this matrix.
    /// </summary>
    /// <remarks>
    /// Undefined behaviour for singular matrices.
    /// </remarks>
    public void Invert()
    {
      Matrix3 adj = new Matrix3();
      float det = GetAdjoint(ref adj);
      float detInv = 1.0f / det;

      fixed (float *m = _m)
      {
        for (int i = 0; i < 9; ++i)
        {
          m[i] = adj[i] * detInv;
        }
      }
    }

    /// <summary>
    /// Get the inverse of this matrix.
    /// </summary>
    /// <remarks>
    /// Undefined behaviour for singular matrices.
    /// </remarks>
    public Matrix3 Inverse
    {
      get
      {
        Matrix3 inv = this;
        inv.Invert();
        return inv;
      }
    }

    /// <summary>
    /// Calculate the inverse of a rigid body matrix (no skew or scaling).
    /// </summary>
    /// <remarks>
    /// Performs a matrix transpose.
    /// </remarks>
    public void RigidBodyInvert() { Transpose(); }

    /// <summary>
    /// Get the inverse of a rigid body matrix (no skew or scaling).
    /// </summary>
    /// <remarks>
    /// Performs a matrix transpose.
    /// </remarks>
    public Matrix3 RigidBodyInverse
    {
      get
      {
        Matrix3 inv = this;
        inv.RigidBodyInvert();
        return inv;
      }
    }

    /// <summary>
    /// Apply this transformation to the vector <paramref name="v"/>.
    /// </summary>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The matrix product <c>Av</c> where <c>A</c> is this matrix and <c>v</c> is the
    /// vector argument.</returns>
    public Vector3 Transform(Vector3 v)
    {
      Vector3 r = new Vector3
      {
        X = this[0, 0] * v[0] + this[0, 1] * v[1] + this[0, 2] * v[2] + this[0, 3] * 1.0f,
        Y = this[1, 0] * v[0] + this[1, 1] * v[1] + this[1, 2] * v[2] + this[1, 3] * 1.0f,
        Z = this[2, 0] * v[0] + this[2, 1] * v[1] + this[2, 2] * v[2] + this[2, 3] * 1.0f
      };
      return r;
    }

    /// <summary>
    /// Transforms the vector <paramref name="v"/> by the rotation component of this matrix. No translation is applied.
    /// </summary>
    /// <returns>
    /// Av, where A is this matrix converted to a 3x3 rotation matrix (no translation).
    /// </returns>
    public Vector3 Rotate(Vector3 v)
    {
      Vector3 r = new Vector3
      {
        X = this[0, 0] * v[0] + this[0, 1] * v[1] + this[0, 2] * v[2],
        Y = this[1, 0] * v[0] + this[1, 1] * v[1] + this[1, 2] * v[2],
        Z = this[2, 0] * v[0] + this[2, 1] * v[1] + this[2, 2] * v[2]
      };

      return r;
    }

    /// <summary>
    /// Calculates the rotation <c>Av</c>.
    /// </summary>
    /// <param name="a">The rotation matrix.</param>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The matrix product <c>Av</c> where <c>A</c> is this matrix and <c>v</c> is the
    /// vector argument.</returns>
    public static Vector3 operator *(Matrix3 a, Vector3 v)
    {
      return a.Transform(v);
    }

    /// <summary>
    /// Calculates the matrix product of two 3x3 matrices.
    /// </summary>
    /// <remarks>
    /// Conceptually, when the resulting matrix transforms a vector,
    /// the matrix <paramref name="b"/> is applied first, then <paramref name="a"/>.
    /// </remarks>
    /// <param name="a">A matrix operand.</param>
    /// <param name="b">A matrix operand.</param>
    /// <returns>The matrix product of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Matrix3 operator *(Matrix3 a, Matrix3 b)
    {
      Matrix3 m = new Matrix3();
      m[0, 0] = a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0];
      m[0, 1] = a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1];
      m[0, 2] = a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2];

      m[1, 0] = a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0];
      m[1, 1] = a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1];
      m[1, 2] = a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2];

      m[2, 0] = a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0];
      m[2, 1] = a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1];
      m[2, 2] = a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2];

      return m;
    }
  }
}
