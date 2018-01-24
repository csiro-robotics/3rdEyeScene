//
// author: Kazys Stepanas
//
#ifndef _3ESMATRIX4_H_
#define _3ESMATRIX4_H_

#include "3es-core.h"

#include "3esvector3.h"
#include "3esvector4.h"

namespace tes
{
  /// A row major 4x4 transformation matrix.
  ///
  /// The matrix is laid out as follows:
  /// @code{.unparsed}
  ///     | rc00  rc01  rc02  rc03 |   | 0   1   2   3  |   | xx  yx  zx  tx |
  /// M = | rc10  rc11  rc12  rc13 | = |  4   5   6   7 | = | xy  yy  zy  ty |
  ///     | rc20  rc21  rc22  rc23 |   |  8   9  10  11 |   | xz  yz  zz  tz |
  ///     | rc30  rc31  rc32  rc33 |   | 12  13  14  15 |   |  0   0   0   1 |
  /// @endcode
  /// Where (xx, xy, xz) are the components of the X axis. Similarly, yn and zn
  /// form the Y axis and Z axis of the basis vectors respectively. Finally,
  /// (tx, ty, tz) is the translation.
  template <typename T>
  class Matrix4
  {
  public:
    union
    {
      T rc[4][4]; ///< Row/column indexing representation.
      T m[16];    ///< Array representation.
    };

    static const Matrix4<T> zero; ///< A matrix with all zero elements.
    static const Matrix4<T> identity; ///< The identity matrix.

    /// Empty constructor; contents are undefined.
    inline Matrix4() {}
    /// Array initialisation constructor from an array of at least 16 elements. No bounds checking.
    /// @param array16 The array of at least 16 value to initialise from.
    Matrix4(const T *array16);
    /// Copy constructor.
    /// @param other The matrix to copy from.
    Matrix4(const Matrix4<T> &other);
    /// Copy constructor from a different numeric type.
    /// @param other The matrix to copy from.
    template <typename Q>
    Matrix4(const Matrix4<Q> &other);

    /// Per element constructor, specifying each row in order.
    /// @param rc00 Element at row/column 00
    /// @param rc01 Element at row/column 01
    /// @param rc02 Element at row/column 02
    /// @param rc03 Element at row/column 03
    /// @param rc10 Element at row/column 10
    /// @param rc11 Element at row/column 11
    /// @param rc12 Element at row/column 12
    /// @param rc13 Element at row/column 13
    /// @param rc20 Element at row/column 20
    /// @param rc21 Element at row/column 21
    /// @param rc22 Element at row/column 22
    /// @param rc23 Element at row/column 23
    /// @param rc30 Element at row/column 30
    /// @param rc31 Element at row/column 31
    /// @param rc32 Element at row/column 32
    /// @param rc33 Element at row/column 33
    Matrix4(const T &rc00, const T &rc01, const T &rc02, const T &rc03,
            const T &rc10, const T &rc11, const T &rc12, const T &rc13,
            const T &rc20, const T &rc21, const T &rc22, const T &rc23,
            const T &rc30, const T &rc31, const T &rc32, const T &rc33);

    /// Row/column access. Not bounds checked.
    /// @param r The row to access [0, 3]
    /// @param c The column to access [0, 3].
    inline T &operator()(int r, int c) { return rc[r][c]; }
    /// Row/column immutable access. Not bounds checked.
    /// @param r The row to access [0, 3]
    /// @param c The column to access [0, 3].
    inline const T &operator()(int r, int c) const { return rc[r][c]; }

    /// Indexing operator (no bounds checking).
    /// @param index The element to access [0, 15].
    /// @return The matrix element at @p index.
    inline T &operator[](int index) { return m[index]; }

    /// Indexing operator (no bounds checking).
    /// @param index The element to access [0, 15].
    /// @return The matrix element at @p index.
    inline const T &operator[](int index) const { return m[index]; }

    /// Create a matrix which represents a rotation around the X axis.
    /// @param angle The rotation angle in radians.
    /// @return The rotation matrix.
    static Matrix4<T> rotationX(const T &angle);

    /// Initialise this matrix as a rotation around the X axis.
    /// @param angle The rotation angle (radians).
    /// @return @c this
    inline Matrix4<T> &initRotationX(const T &angle) { *this = rotationX(angle); return *this; }

    /// Create a matrix which represents a rotation around the Y axis.
    /// @param angle The rotation angle in radians.
    /// @return The rotation matrix.
    static Matrix4<T> rotationY(const T &angle);

    /// Initialise this matrix as a rotation around the Y axis.
    /// @param angle The rotation angle (radians).
    /// @return @c this
    inline Matrix4<T> &initRotationY(const T &angle) { *this = rotationY(angle); return *this; }

    /// Create a matrix which represents a rotation around the Z axis.
    /// @param angle The rotation angle in radians.
    /// @return The rotation matrix.
    static Matrix4<T> rotationZ(const T &angle);

    /// Initialise this matrix as a rotation around the Z axis.
    /// @param angle The rotation angle (radians).
    /// @return @c this
    inline Matrix4<T> &initRotationZ(const T &angle) { *this = rotationZ(angle); return *this; }

    /// Create a matrix which represents a rotation around each axis (Euler angles).
    /// Rotations are applied in x, y, z order.
    /// @param x Rotation around the X axis (radians).
    /// @param y Rotation around the Y axis (radians).
    /// @param z Rotation around the Z axis (radians).
    /// @return The rotation matrix.
    static Matrix4<T> rotation(const T &x, const T &y, const T &z);

    /// Initialise this matrix as a rotation using the given Euler angles.
    /// @param x Rotation around the X axis (radians).
    /// @param y Rotation around the Y axis (radians).
    /// @param z Rotation around the Z axis (radians).
    /// @return  @c this
    inline Matrix4<T> &initRotation(const T &x, const T &y, const T &z) { *this = rotation(x, y, z); return *this; }

    /// Create a translation matrix (no rotation).
    /// @param trans The translation the matrix applies.
    /// @return The translation matrix.
    static Matrix4<T> translation(const Vector3<T> &trans);

    /// Initialise this matrix as a translation only matrix.
    /// @param trans The translation.
    /// @return  @c this
    inline Matrix4<T> initTranslation(const Vector3<T> &trans) { *this = translation(trans); return *this; }

    /// Create a transformation matrix with rotation (Euler angles) and translation. Rotation is applied first.
    /// @param x Rotation around the X axis (radians).
    /// @param y Rotation around the Y axis (radians).
    /// @param z Rotation around the Z axis (radians).
    /// @param trans The translation the matrix applies.
    /// @return The transformation matrix.
    static Matrix4<T> rotationTranslation(const T &x, const T &y, const T &z, const Vector3<T> &trans);
    /// @return @c this
    inline Matrix4<T> &initRotationTranslation(const T &x, const T &y, const T &z, const Vector3<T> &trans) { *this = rotationTranslation(x, y, z, trans); return *this; }

    /// Create a scaling matrix.
    /// @param scale The scaling to apply along each axis.
    /// @return The scaling matrix.
    static Matrix4<T> scaling(const Vector3<T> &scale);

    /// Initialise this matrix as scaling matrix with no rotation or translation.
    /// @param scale The scaling vector. Each component corresponds to each axis.
    /// @return @c this
    inline Matrix4<T> &initScaling(const Vector3<T> &scale) { *this = scaling(scale); return *this; }

    /// Create a model or camera matrix at @p eye looking at @p target.
    ///
    /// Supports specifying the up and forward axes (inferring the left/right axis),
    /// where the indices [0, 1, 2] correspond to the axes (X, Y, Z).
    ///
    /// The default behaviour is to use Y as the forward axis and Z as up.
    ///
    /// @param eye The position of the eye/camera.
    /// @param target The target to look at. Should not be equal to @p eye.
    /// @param axisUp The axis defining the initial up vector. Must be normalised.
    /// @param forwardAxisIndex The index of the forward axis. This is to point at @p target.
    /// @param upAxisIndex The index of the up axis. Must not be equal to @p forwardAxisIndex.
    /// @return A model matrix at @p eye pointing at @p target. Returns identity if
    /// there are errors in the specification of @p forwardAxisIndex and @p upAxisIndex.
    static Matrix4<T> lookAt(const Vector3<T> &eye, const Vector3<T> &target, const Vector3<T> &axisUp, int forwardAxisIndex = 1, int upAxisIndex = 2);

    /// Initialise this matrix as a model or camera matrix.
    /// @see @c lookAt().
    /// @param eye The position of the eye/camera.
    /// @param target The target to look at. Should not be equal to @p eye.
    /// @param axisUp The axis defining the initial up vector. Must be normalised.
    /// @param forwardAxisIndex The index of the forward axis. This is to point at @p target.
    /// @param upAxisIndex The index of the up axis. Must not be equal to @p forwardAxisIndex.
    /// @return @c this
    inline Matrix4<T> &initLookAt(const Vector3<T> &eye, const Vector3<T> &target, const Vector3<T> &axisUp, int forwardAxisIndex = 1, int upAxisIndex = 2)
    {
      *this = lookAt(eye, target, axisUp, forwardAxisIndex, upAxisIndex);
      return *this;
    }

    /// Transposes this matrix.
    /// @return This matrix after the operation.
    Matrix4<T> &transpose();

    /// Returns the transpose of this matrix, leaving this matrix unchanged.
    /// @return The transpose of this matrix.
    Matrix4<T> transposed() const;

    /// Inverts this matrix.
    /// @return This matrix after the operation.
    Matrix4<T> &invert();

    /// Returns the inverse of this matrix, leaving this matrix unchanged.
    /// @return The inverse of this matrix.
    Matrix4<T> inverse() const;

    /// Inverts this matrix assuming this matrix represents a rigid body transformation.
    ///
    /// A rigid body transformation contains no skewing. That is, the basis vectors
    /// are orthogonal.
    ///
    /// The inverse is calculated by transposing the rotation and inverting the
    /// translation.
    /// @return This matrix after the operation.
    Matrix4<T> &rigidBodyInvert();

    /// Returns the inverse of this matrix assuming this is a rigid body transformation.
    /// See @c rigidBodyInvert().
    /// @return The inverse of this matrix.
    Matrix4<T> rigidBodyInverse() const;

    /// Calculates the determinant of this matrix.
    /// @return The determinant.
    T determinant() const;

    /// Returns the X axis of this matrix (elements (0, 0), (1, 0), (2, 0)).
    /// @return The X axis.
    Vector3<T> axisX() const;
    /// Returns the Y axis of this matrix (elements (0, 1), (1, 1), (2, 1)).
    /// @return The Y axis.
    Vector3<T> axisY() const;
    /// Returns the Z axis of this matrix (elements (0, 2), (1, 2), (2, 2)).
    /// @return The Z axis.
    Vector3<T> axisZ() const;

    /// Returns the T axis or translation component of this matrix (elements (0, 3), (1, 3), (2, 3)).
    /// @return The translation component.
    Vector3<T> axisT() const;

    /// Returns the T axis or translation component of this matrix (elements (0, 3), (1, 3), (2, 3)).
    /// @return The translation component.
    Vector3<T> translation() const;

    /// Returns one of the axes of this matrix.
    /// @param index The index of the axis of interest.
    ///   0 => X, 1 => Y, 2 => Z, 3 => T/translation.
    /// @return The axis of interest.
    Vector3<T> axis(int index) const;

    /// Sets the X axis of this matrix. See @p axisX().
    /// @param axis The value to set the axis to.
    /// @return This matrix after the operation.
    Matrix4<T> &setAxisX(const Vector3<T> &axis);

    /// Sets the Y axis of this matrix. See @p axisY().
    /// @param axis The value to set the axis to.
    /// @return This matrix after the operation.
    Matrix4<T> &setAxisY(const Vector3<T> &axis);

    /// Sets the Z axis of this matrix. See @p axisZ().
    /// @param axis The value to set the axis to.
    /// @return This matrix after the operation.
    Matrix4<T> &setAxisZ(const Vector3<T> &axis);

    /// Sets the translation of this matrix. See @p translation().
    /// @param axis The value to set the translation to.
    /// @return This matrix after the operation.
    Matrix4<T> &setAxisT(const Vector3<T> &axis);

    /// Sets the translation of this matrix. See @p translation().
    /// @param axis The value to set the translation to.
    /// @return This matrix after the operation.
    Matrix4<T> &setTranslation(const Vector3<T> &axis);

    /// Sets the indexed axis of this matrix. See @p axis().
    /// @param index The index of the axis of interest.
    ///   0 => X, 1 => Y, 2 => Z, 3 => T/translation.
    /// @param axis The value to set the axis to.
    /// @return This matrix after the operation.
    Matrix4<T> &setAxis(int index, const Vector3<T> &axis);

    /// Returns the scale contained in this matrix. This is the length of each axis.
    /// @return The scale of each rotation axis in this matrix.
    Vector3<T> scale() const;

    /// Removes scale from the matrix, leaving a rotation/translation matrix.
    /// @return The scale which was present along each axis.
    Vector3<T> removeScale();

    /// Scales this matrix, adjusting the scale of each rotation, but leaving the translation.
    /// @param scaling The scaling to apply.
    /// @return This matrix after the operation.
    Matrix4<T> &scale(const Vector3<T> &scaling);

    /// Transforms the vector @p v by this matrix.
    /// @return Av, where A is this matrix.
    Vector3<T> transform(const Vector3<T> &v) const;

    /// Transforms the vector @p v by this matrix.
    /// @return Av, where A is this matrix.
    Vector4<T> transform(const Vector4<T> &v) const;

    /// Transforms the vector @p v by the rotation component of this matrix. No translation is applied.
    /// @return Av, where A is this matrix converted to a 3x3 rotation matrix (no translation).
    Vector3<T> rotate(const Vector3<T> &v) const;

    /// Transforms the vector @p v by the rotation component of this matrix. No translation is applied.
    /// @return Av, where A is this matrix converted to a 3x3 rotation matrix (no translation).
    Vector4<T> rotate(const Vector4<T> &v) const;

    /// Numerical equality comparison. Reports @c true if each element of this matrix is within of
    /// @p Epsilon @p a (or equal to).
    /// @return a Matrix to compare to.
    /// @param epsilon Comparison tolerance value.
    /// @return @c true when each element in this matrix is within @p epsilon of each element of @p a.
    bool equals(const Matrix4<T> &a, const T epsilon = Vector3<T>::Epsilon);
  };

  /// Defines a single precision 4x4 matrix.
  typedef Matrix4<float> Matrix4f;
  /// Defines a double precision 4x4 matrix.
  typedef Matrix4<double> Matrix4d;

  template class _3es_coreAPI Matrix4<float>;
  template class _3es_coreAPI Matrix4<double>;

  /// Performs the matrix multiplication AB.
  /// @return The result of AB.
  template <typename T>
  Matrix4<T> operator * (const Matrix4<T> &a, const Matrix4<T> &b);

  /// Performs the matrix multiplication Av.
  /// @return The result of Av.
  template <typename T>
  Vector3<T> operator * (const Matrix4<T> &a, const Vector3<T> &v);

  /// Performs the matrix multiplication Av.
  /// @return The result of Av.
  template <typename T>
  Vector4<T> operator * (const Matrix4<T> &a, const Vector4<T> &v);
}

#include "3esmatrix4.inl"

#endif // _3ESMATRIX4_H_
