using System;

namespace Tes.Maths
{
  ///<summary>
  /// A row major 4x4 transformation matrix.
  /// </summary>
  /// <remarks>
  /// The matrix is laid out as follows:
  /// 
  /// <code>
  ///     | m00  m01  m02  m03 |   |  0   1   2   3 |   | xx  yx  zx  tx |
  /// M = | m10  m11  m12  m13 | = |  4   5   6   7 | = | xy  yy  zy  ty |
  ///     | m20  m21  m22  m23 |   |  8   9  10  11 |   | xz  yz  zz  tz |
  ///     | m30  m31  m32  m33 |   | 12  13  14  15 |   |  0   0   0   1 |
  /// </code>
  /// Where (xx, xy, xz) are the components of the X axis. Similarly, yn and zn
  /// form the Y axis and Z axis of the basis vectors respectively. Finally,
  /// (tx, ty, tz) is the translation.
  /// </remarks>
  public unsafe struct Matrix4
  {
    /// <summary>
    /// The matrix array.
    /// </summary>
    private fixed float _m[16];

    /// <summary>
    /// A matrix with all zero components.
    /// </summary>
    public static Matrix4 Zero = All(0.0f);
    /// <summary>
    /// The 4x4 identity matrix.
    /// </summary>
    public static Matrix4 Identity = IdentityMatrix();

    /// <summary>
    /// Create a matrix with all components set to <paramref name="val"/>.
    /// </summary>
    /// <param name="val">The value to assign to each component.</param>
    /// <returns>The matrix with all components <paramref name="val"/>.</returns>
    public static Matrix4 All(float val)
    {
      Matrix4 mat = new Matrix4();
      for (int i = 0; i < 16; ++i)
      {
        mat._m[i] = val;
      }
      return mat;
    }

    /// <summary>
    /// The 4x4 identity matrix.
    /// </summary>
    /// <returns>The identity matrix.</returns>
    public static Matrix4 IdentityMatrix()
    {
      Matrix4 mat = All(0.0f);
      mat[0, 0] = mat[1, 1] = mat[2, 2] = mat[3, 3] = 1.0f;
      return mat;
    }

    /// <summary>
    /// Indexing accessor across the range [0, 15].
    /// </summary>
    /// <remarks>
    /// See class documentation on the matrix layout.
    /// </remarks>
    /// <param name="index">The component index [0, 15].</param>
    /// <returns>The requested component.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/>is out of range.</exception>
    public float this[int index]
    {
      get
      {
        if (index < 0 || index > 16)
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
        if (index < 0 || index > 16)
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
    /// Row/column indexing accessor. Valid row and column ranges are: [0, 3].
    /// </summary>
    /// <remarks>
    /// See class documentation on the matrix layout.
    /// </remarks>
    /// <param name="r">The row index [0, 3].</param>
    /// <param name="c">The column index [0, 3].</param>
    /// <returns>The requested component.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="r"/> or <paramref name="c"/> are out of range.</exception>
    public float this[int r, int c]
    {
      get
      {
        if (r < 0 || r > 3 || c < 0 || c > 3)
        {
          throw new IndexOutOfRangeException();
        }
        fixed (float* m = _m)
        {
          return m[r * 4 + c];
        }
      }

      set
      {
        if (r < 0 || r > 3 || c < 0 || c > 3)
        {
          throw new IndexOutOfRangeException();
        }
        fixed (float* m = _m)
        {
          m[r * 4 + c] = value;
        }
      }
    }

    /// <summary>
    /// Calculate the determinant of this matrix.
    /// </summary>
    /// <remarks>
    /// Not a cheap operation.
    /// </remarks>
    public float Determinant
    {
      get
      {
        Matrix4 transpose = Transposed;         // transposed source matrix
        float[] pairs = new float[12];            // temp array for cofactors
        float[] tmp = new float[4];

        // calculate pairs for first 8 elements
        pairs[0] = transpose[10] * transpose[15];
        pairs[1] = transpose[14] * transpose[11];
        pairs[2] = transpose[6] * transpose[15];
        pairs[3] = transpose[14] * transpose[7];
        pairs[4] = transpose[6] * transpose[11];
        pairs[5] = transpose[10] * transpose[7];
        pairs[6] = transpose[2] * transpose[15];
        pairs[7] = transpose[14] * transpose[3];
        pairs[8] = transpose[2] * transpose[11];
        pairs[9] = transpose[10] * transpose[3];
        pairs[10] = transpose[2] * transpose[7];
        pairs[11] = transpose[6] * transpose[3];

        // calculate first 8 elements (cofactors)
        tmp[0] = pairs[0] * transpose[5] + pairs[3] * transpose[9] + pairs[4] * transpose[13];
        tmp[0] -= pairs[1] * transpose[5] + pairs[2] * transpose[9] + pairs[5] * transpose[13];
        tmp[1] = pairs[1] * transpose[1] + pairs[6] * transpose[9] + pairs[9] * transpose[13];
        tmp[1] -= pairs[0] * transpose[1] + pairs[7] * transpose[9] + pairs[8] * transpose[13];
        tmp[2] = pairs[2] * transpose[1] + pairs[7] * transpose[5] + pairs[10] * transpose[13];
        tmp[2] -= pairs[3] * transpose[1] + pairs[6] * transpose[5] + pairs[11] * transpose[13];
        tmp[3] = pairs[5] * transpose[1] + pairs[8] * transpose[5] + pairs[11] * transpose[9];
        tmp[3] -= pairs[4] * transpose[1] + pairs[9] * transpose[5] + pairs[10] * transpose[9];

        // calculate determinant
        return (transpose[0] * tmp[0] + transpose[4] * tmp[1] + transpose[8] * tmp[2] + transpose[12] * tmp[3]);
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
    /// Get/set the conceptual translation axis from this matrix.
    /// </summary>
    /// <remarks>
    /// This equates to column 3.
    /// </remarks>
    public Vector3 Translation { get { return GetAxis(3); } set { SetAxis(3, value); } }

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
    /// <remarks>
    /// A Vector4 class is need to properly extract the whole column.
    /// </remarks>
    /// <param name="index">The axis/column index [0, 3]</param>
    /// <returns>The requested axis.</returns>
    public Vector3 GetAxis(int index)
    {
      return new Vector3 { X = this[0, index], Y = this[1, index], Z = this[2, index] };
    }

    /// <summary>
    /// Set an axis or column from the matrix.
    /// </summary>
    /// <remarks>
    /// The last row is set to either zero for columns [0, 2] and 1 for column 3.
    /// A Vector4 class is need to properly set the whole column.
    /// </remarks>
    /// <param name="index">The axis/column index [0, 3]</param>
    /// <param name="axis">The axis value.</param>
    public void SetAxis(int index, Vector3 axis)
    {
      this[0, index] = axis[0];
      this[1, index] = axis[1];
      this[2, index] = axis[2];
      this[3, index] = index < 3 ? 0 : 1;
    }

    /// <summary>
    /// Builds a rotation matrix around the X axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix4 RotationX(float angle)
    {
      Matrix4 mat = new Matrix4();
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
      this[5] = this[10] = c;
      this[6] = -s; this[9] = s;
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix around the Y axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    public static Matrix4 RotationY(float angle)
    {
      Matrix4 mat = new Matrix4();
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
      this[0] = this[10] = c;
      this[8] = -s; this[2] = s;
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix around the Z axis.
    /// </summary>
    /// <param name="angle">The rotation angle (radians)</param>
    public static Matrix4 RotationZ(float angle)
    {
      Matrix4 mat = new Matrix4();
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
      this[0] = this[5] = c;
      this[1] = -s; this[4] = s;
    }

    /// <summary>
    /// Initialise a translation matrix, no rotation or scale.
    /// </summary>
    /// <param name="trans">The translation to apply.</param>
    public void InitTranslationMatrix(Vector3 trans)
    {
      this = Identity;
      Translation = trans;
    }

    /// <summary>
    /// Create a translation matrix, no rotation or scale.
    /// </summary>
    /// <param name="trans">The translation to apply.</param>
    /// <returns>The constructed translation matrix.</returns>
    public static Matrix4 TranslationMatrix(Vector3 trans)
    {
      Matrix4 mat = Identity;
      mat.Translation = trans;
      return mat;
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
    public static Matrix4 Rotation(float angleX, float angleY, float angleZ)
    {
      Matrix4 mat = new Matrix4();
      mat.InitRotation(angleX, angleY, angleZ);
      return mat;
    }

    /// <summary>
    /// Initialise this matrix from the given Euler angles and translation.
    /// </summary>
    /// <param name="angleX">The rotation around the X axis (radians).</param>
    /// <param name="angleY">The rotation around the Y axis (radians).</param>
    /// <param name="angleZ">The rotation around the Z axis (radians).</param>
    /// <param name="trans">The translation component for matrix.</param>
    public void InitRotationTranslation(float angleX, float angleY, float angleZ, Vector3 trans)
    {
      InitRotation(angleX, angleY, angleZ);
      Translation = trans;
    }

    /// <summary>
    /// Build a transformation matrix from the given Euler angles and translation.
    /// </summary>
    /// <param name="angleX">The rotation around the X axis (radians).</param>
    /// <param name="angleY">The rotation around the Y axis (radians).</param>
    /// <param name="angleZ">The rotation around the Z axis (radians).</param>
    /// <param name="trans">The translation component for matrix.</param>
    /// <returns>The transformation matrix.</returns>
    public static Matrix4 RotationTranslation(float angleX, float angleY, float angleZ, Vector3 trans)
    {
      Matrix4 mat = new Matrix4();
      mat.InitRotationTranslation(angleX, angleY, angleZ, trans);
      return mat;
    }

    /// <summary>
    /// Initialise this matrix as a scaling matrix.
    /// </summary>
    /// <param name="scale">The scaling factor. Each coordinate corresponds to
    /// one of the first three columns of the matrix.</param>
    public void InitScaling(Vector3 scale)
    {
      this = All(0.0f);
      this[0, 0] = scale.X;
      this[1, 1] = scale.Y;
      this[2, 2] = scale.Z;
      this[3, 3] = 1.0f;
    }

    /// <summary>
    /// Initialise a scaling matrix.
    /// </summary>
    /// <param name="scale">The scaling factor. Each coordinate corresponds to
    /// a column of the matrix.</param>
    public static Matrix4 Scaling(Vector3 scale)
    {
      Matrix4 mat = All(0.0f);
      mat[0, 0] = scale.X;
      mat[1, 1] = scale.Y;
      mat[2, 2] = scale.Z;
      mat[3, 3] = 1.0f;
      return mat;
    }

    /// <summary>
    /// Scales the matrix basis vectors.
    /// </summary>
    /// <remarks>
    /// The translation column is left unmodified.
    /// </remarks>
    /// <param name="scaling">Per axis scaling. Each coordinate corresponds to
    /// a column of the matrix.</param>
    public void ApplyScaling(Vector3 scaling)
    {
      this[0, 0] *= scaling.X;
      this[1, 0] *= scaling.X;
      this[2, 0] *= scaling.X;
      this[3, 0] *= scaling.X;

      this[0, 1] *= scaling.Y;
      this[1, 1] *= scaling.Y;
      this[2, 1] *= scaling.Y;
      this[3, 1] *= scaling.Y;

      this[0, 2] *= scaling.Z;
      this[1, 2] *= scaling.Z;
      this[2, 2] *= scaling.Z;
      this[3, 2] *= scaling.Z;
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
    /// </remarks>
    /// <param name="eye"> The position of the eye/camera.</param>
    /// <param name="target"> The target to look at. Should not be equal to <paramref name="eye"/>.</param>
    /// <param name="axisUp"> The axis defining the initial up vector. Must be normalised.</param>
    /// <param name="forwardAxisIndex"> The index of the forward axis. This is to point at <paramref name="target"/>.</param>
    /// <param name="upAxisIndex"> The index of the up axis. Must not be equal to <paramref name="forwardAxisIndex"/>.</param>
    /// <returns>A model matrix at <paramref name="eye"/> pointing at <paramref name="target"/>. Returns identity if
    /// there are errors in the specification of <paramref name="forwardAxisIndex"/> and <paramref name="upAxisIndex"/>.</returns>
    public static Matrix4 LookAt(Vector3 eye, Vector3 target, Vector3 axisUp, int forwardAxisIndex = 1, int upAxisIndex = 2)
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

      Matrix4 mat = Identity;
      mat.SetAxis(sideAxisIndex, axes[sideAxisIndex]);
      mat.SetAxis(forwardAxisIndex, axes[forwardAxisIndex]);
      mat.SetAxis(upAxisIndex, axes[upAxisIndex]);
      mat.Translation = eye;

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

      temp = this[0, 3];
      this[0, 3] = this[3, 0];
      this[3, 0] = temp;

      temp = this[1, 2];
      this[1, 2] = this[2, 1];
      this[2, 1] = temp;

      temp = this[1, 3];
      this[1, 3] = this[3, 1];
      this[3, 1] = temp;

      temp = this[2, 3];
      this[2, 3] = this[3, 2];
      this[3, 2] = temp;
    }

    /// <summary>
    /// Return the transpose of this matrix.
    /// </summary>
    public Matrix4 Transposed
    {
      get
      {
        Matrix4 trans = this;
        trans.Transpose();
        return trans;
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
      // Inversion with Cramer's rule
      //
      // 1. Transpose the matrix.
      // 2. Calculate cofactors of matrix elements. Form a new matrix from cofactors of the given matrix elements.
      // 3. Calculate the determinant of the given matrix.
      // 4. Multiply the matrix obtained in step 3 by the reciprocal of the determinant.
      //
      Matrix4 transpose = Transposed;             // transposed source matrix
      float[] pairs = new float[12];              // temp array for cofactors
      float det;                                  // determinant

      // calculate pairs for first 8 elements
      pairs[0] = transpose[10] * transpose[15];
      pairs[1] = transpose[14] * transpose[11];
      pairs[2] = transpose[6] * transpose[15];
      pairs[3] = transpose[14] * transpose[7];
      pairs[4] = transpose[6] * transpose[11];
      pairs[5] = transpose[10] * transpose[7];
      pairs[6] = transpose[2] * transpose[15];
      pairs[7] = transpose[14] * transpose[3];
      pairs[8] = transpose[2] * transpose[11];
      pairs[9] = transpose[10] * transpose[3];
      pairs[10] = transpose[2] * transpose[7];
      pairs[11] = transpose[6] * transpose[3];

      fixed (float *m = _m)
      {
        // calculate first 8 elements (cofactors)
        m[0] = pairs[0] * transpose[5] + pairs[3] * transpose[9] + pairs[4] * transpose[13];
        m[0] -= pairs[1] * transpose[5] + pairs[2] * transpose[9] + pairs[5] * transpose[13];
        m[4] = pairs[1] * transpose[1] + pairs[6] * transpose[9] + pairs[9] * transpose[13];
        m[4] -= pairs[0] * transpose[1] + pairs[7] * transpose[9] + pairs[8] * transpose[13];
        m[8] = pairs[2] * transpose[1] + pairs[7] * transpose[5] + pairs[10] * transpose[13];
        m[8] -= pairs[3] * transpose[1] + pairs[6] * transpose[5] + pairs[11] * transpose[13];
        m[12] = pairs[5] * transpose[1] + pairs[8] * transpose[5] + pairs[11] * transpose[9];
        m[12] -= pairs[4] * transpose[1] + pairs[9] * transpose[5] + pairs[10] * transpose[9];
        m[1] = pairs[1] * transpose[4] + pairs[2] * transpose[8] + pairs[5] * transpose[12];
        m[1] -= pairs[0] * transpose[4] + pairs[3] * transpose[8] + pairs[4] * transpose[12];
        m[5] = pairs[0] * transpose[0] + pairs[7] * transpose[8] + pairs[8] * transpose[12];
        m[5] -= pairs[1] * transpose[0] + pairs[6] * transpose[8] + pairs[9] * transpose[12];
        m[9] = pairs[3] * transpose[0] + pairs[6] * transpose[4] + pairs[11] * transpose[12];
        m[9] -= pairs[2] * transpose[0] + pairs[7] * transpose[4] + pairs[10] * transpose[12];
        m[13] = pairs[4] * transpose[0] + pairs[9] * transpose[4] + pairs[10] * transpose[8];
        m[13] -= pairs[5] * transpose[0] + pairs[8] * transpose[4] + pairs[11] * transpose[8];

        // calculate pairs for second 8 elements (cofactors)
        pairs[0] = transpose[8] * transpose[13];
        pairs[1] = transpose[12] * transpose[9];
        pairs[2] = transpose[4] * transpose[13];
        pairs[3] = transpose[12] * transpose[5];
        pairs[4] = transpose[4] * transpose[9];
        pairs[5] = transpose[8] * transpose[5];
        pairs[6] = transpose[0] * transpose[13];
        pairs[7] = transpose[12] * transpose[1];
        pairs[8] = transpose[0] * transpose[9];
        pairs[9] = transpose[8] * transpose[1];
        pairs[10] = transpose[0] * transpose[5];
        pairs[11] = transpose[4] * transpose[1];

        // calculate second 8 elements (cofactors)
        m[2] = pairs[0] * transpose[7] + pairs[3] * transpose[11] + pairs[4] * transpose[15];
        m[2] -= pairs[1] * transpose[7] + pairs[2] * transpose[11] + pairs[5] * transpose[15];
        m[6] = pairs[1] * transpose[3] + pairs[6] * transpose[11] + pairs[9] * transpose[15];
        m[6] -= pairs[0] * transpose[3] + pairs[7] * transpose[11] + pairs[8] * transpose[15];
        m[10] = pairs[2] * transpose[3] + pairs[7] * transpose[7] + pairs[10] * transpose[15];
        m[10] -= pairs[3] * transpose[3] + pairs[6] * transpose[7] + pairs[11] * transpose[15];
        m[14] = pairs[5] * transpose[3] + pairs[8] * transpose[7] + pairs[11] * transpose[11];
        m[14] -= pairs[4] * transpose[3] + pairs[9] * transpose[7] + pairs[10] * transpose[11];
        m[3] = pairs[2] * transpose[10] + pairs[5] * transpose[14] + pairs[1] * transpose[6];
        m[3] -= pairs[4] * transpose[14] + pairs[0] * transpose[6] + pairs[3] * transpose[10];
        m[7] = pairs[8] * transpose[14] + pairs[0] * transpose[2] + pairs[7] * transpose[10];
        m[7] -= pairs[6] * transpose[10] + pairs[9] * transpose[14] + pairs[1] * transpose[2];
        m[11] = pairs[6] * transpose[6] + pairs[11] * transpose[14] + pairs[3] * transpose[2];
        m[11] -= pairs[10] * transpose[14] + pairs[2] * transpose[2] + pairs[7] * transpose[6];
        m[15] = pairs[10] * transpose[10] + pairs[4] * transpose[2] + pairs[9] * transpose[6];
        m[15] -= pairs[8] * transpose[6] + pairs[11] * transpose[10] + pairs[5] * transpose[2];

        // calculate determinant
        det = transpose[0] * m[0] + transpose[4] * m[4] + transpose[8] * m[8] + transpose[12] * m[12];

        // calculate matrix inverse
        float detInv = 1.0f / det;
        for (int i = 0; i < 16; ++i)
        {
          m[i] *= detInv;
        }
      }
    }

    /// <summary>
    /// Get the inverse of this matrix.
    /// </summary>
    /// <remarks>
    /// Undefined behaviour for singular matrices.
    /// </remarks>
    public Matrix4 Inverse
    {
      get
      {
        Matrix4 inv = this;
        inv.Invert();
        return inv;
      }
    }

    /// <summary>
    /// Calculate the inverse of a rigid body matrix (no skew or scaling).
    /// </summary>
    /// <remarks>
    /// Performs a matrix transpose and negates the translation.
    /// </remarks>
    public void RigidBodyInvert()
    {
      // Transpose 3x3.
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

      // Negate translation.
      this[0, 3] = -this[0, 3]; this[1, 3] = -this[1, 3]; this[2, 3] = -this[2, 3];

      // Multiply by the negated translation.
      Vector3 v = new Vector3
      {
        X = this[0, 0] * this[0, 3] + this[0, 1] * this[1, 3] + this[0, 2] * this[2, 3],
        Y = this[1, 0] * this[0, 3] + this[1, 1] * this[1, 3] + this[1, 2] * this[2, 3],
        Z = this[2, 0] * this[0, 3] + this[2, 1] * this[1, 3] + this[2, 2] * this[2, 3]
      };

      // Set the new translation.
      Translation = v;
    }

    /// <summary>
    /// Get the inverse of a rigid body matrix (no skew or scaling).
    /// </summary>
    /// <remarks>
    /// Performs a matrix transpose and negates the translation.
    /// </remarks>
    public Matrix4 RigidBodyInverse
    {
      get
      {
        Matrix4 inv = this;
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
    /// <param name="v">The vector to rotate.</param>
    /// <returns>
    /// Av, where A is this matrix converted to a 3x3 rotation matrix (no translation).
    /// </returns>
    /// <remarks>
    /// FIXME: remove scaling effects.
    /// </remarks>
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
    /// Calculates the transformation <c>Av</c>.
    /// </summary>
    /// <param name="a">The transformation matrix.</param>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The matrix product <c>Av</c> where <c>A</c> is this matrix and <c>v</c> is the
    /// vector argument.</returns>
    public static Vector3 operator *(Matrix4 a, Vector3 v)
    {
      return a.Transform(v);
    }

    /// <summary>
    /// Calculates the matrix product of two 4x4 matrices.
    /// </summary>
    /// <remarks>
    /// Conceptually, when the resulting matrix transforms a vector,
    /// the matrix <paramref name="b"/> is applied first, then <paramref name="a"/>.
    /// </remarks>
    /// <param name="a">A matrix operand.</param>
    /// <param name="b">A matrix operand.</param>
    /// <returns>The matrix product of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Matrix4 operator *(Matrix4 a, Matrix4 b)
    {
      Matrix4 m = new Matrix4();
      m[0, 0] = a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0] + a[0, 3] * b[3, 0];
      m[0, 1] = a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1] + a[0, 3] * b[3, 1];
      m[0, 2] = a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2] + a[0, 3] * b[3, 2];
      m[0, 3] = a[0, 0] * b[0, 3] + a[0, 1] * b[1, 3] + a[0, 2] * b[2, 3] + a[0, 3] * b[3, 3];

      m[1, 0] = a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0] + a[1, 3] * b[3, 0];
      m[1, 1] = a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1] + a[1, 3] * b[3, 1];
      m[1, 2] = a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2] + a[1, 3] * b[3, 2];
      m[1, 3] = a[1, 0] * b[0, 3] + a[1, 1] * b[1, 3] + a[1, 2] * b[2, 3] + a[1, 3] * b[3, 3];

      m[2, 0] = a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0] + a[2, 3] * b[3, 0];
      m[2, 1] = a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1] + a[2, 3] * b[3, 1];
      m[2, 2] = a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2] + a[2, 3] * b[3, 2];
      m[2, 3] = a[2, 0] * b[0, 3] + a[2, 1] * b[1, 3] + a[2, 2] * b[2, 3] + a[2, 3] * b[3, 3];

      m[3, 0] = a[3, 0] * b[0, 0] + a[3, 1] * b[1, 0] + a[3, 2] * b[2, 0] + a[3, 3] * b[3, 0];
      m[3, 1] = a[3, 0] * b[0, 1] + a[3, 1] * b[1, 1] + a[3, 2] * b[2, 1] + a[3, 3] * b[3, 1];
      m[3, 2] = a[3, 0] * b[0, 2] + a[3, 1] * b[1, 2] + a[3, 2] * b[2, 2] + a[3, 3] * b[3, 2];
      m[3, 3] = a[3, 0] * b[0, 3] + a[3, 1] * b[1, 3] + a[3, 2] * b[2, 3] + a[3, 3] * b[3, 3];

      return m;
    }
  }
}
