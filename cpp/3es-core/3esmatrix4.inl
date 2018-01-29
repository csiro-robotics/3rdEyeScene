//
// author: Kazys Stepanas
//
#include <cstdlib>

namespace tes
{
  template <typename T>
  Matrix4<T> operator * (const Matrix4<T> &a, const Matrix4<T> &b)
  {
    Matrix4<T> m;
    m(0, 0) = a(0, 0) * b(0, 0) + a(0, 1) * b(1, 0) + a(0, 2) * b(2, 0) + a(0, 3) * b(3, 0);
    m(0, 1) = a(0, 0) * b(0, 1) + a(0, 1) * b(1, 1) + a(0, 2) * b(2, 1) + a(0, 3) * b(3, 1);
    m(0, 2) = a(0, 0) * b(0, 2) + a(0, 1) * b(1, 2) + a(0, 2) * b(2, 2) + a(0, 3) * b(3, 2);
    m(0, 3) = a(0, 0) * b(0, 3) + a(0, 1) * b(1, 3) + a(0, 2) * b(2, 3) + a(0, 3) * b(3, 3);

    m(1, 0) = a(1, 0) * b(0, 0) + a(1, 1) * b(1, 0) + a(1, 2) * b(2, 0) + a(1, 3) * b(3, 0);
    m(1, 1) = a(1, 0) * b(0, 1) + a(1, 1) * b(1, 1) + a(1, 2) * b(2, 1) + a(1, 3) * b(3, 1);
    m(1, 2) = a(1, 0) * b(0, 2) + a(1, 1) * b(1, 2) + a(1, 2) * b(2, 2) + a(1, 3) * b(3, 2);
    m(1, 3) = a(1, 0) * b(0, 3) + a(1, 1) * b(1, 3) + a(1, 2) * b(2, 3) + a(1, 3) * b(3, 3);

    m(2, 0) = a(2, 0) * b(0, 0) + a(2, 1) * b(1, 0) + a(2, 2) * b(2, 0) + a(2, 3) * b(3, 0);
    m(2, 1) = a(2, 0) * b(0, 1) + a(2, 1) * b(1, 1) + a(2, 2) * b(2, 1) + a(2, 3) * b(3, 1);
    m(2, 2) = a(2, 0) * b(0, 2) + a(2, 1) * b(1, 2) + a(2, 2) * b(2, 2) + a(2, 3) * b(3, 2);
    m(2, 3) = a(2, 0) * b(0, 3) + a(2, 1) * b(1, 3) + a(2, 2) * b(2, 3) + a(2, 3) * b(3, 3);

    m(3, 0) = a(3, 0) * b(0, 0) + a(3, 1) * b(1, 0) + a(3, 2) * b(2, 0) + a(3, 3) * b(3, 0);
    m(3, 1) = a(3, 0) * b(0, 1) + a(3, 1) * b(1, 1) + a(3, 2) * b(2, 1) + a(3, 3) * b(3, 1);
    m(3, 2) = a(3, 0) * b(0, 2) + a(3, 1) * b(1, 2) + a(3, 2) * b(2, 2) + a(3, 3) * b(3, 2);
    m(3, 3) = a(3, 0) * b(0, 3) + a(3, 1) * b(1, 3) + a(3, 2) * b(2, 3) + a(3, 3) * b(3, 3);

    return m;
  }

  template <typename T>
  Vector3<T> operator * (const Matrix4<T> &a, const Vector3<T> &v)
  {
    Vector3<T> r;

    r.x = a(0, 0) * v[0] + a(0, 1) * v[1] + a(0, 2) * v[2] + a(0, 3) * T(1);
    r.y = a(1, 0) * v[0] + a(1, 1) * v[1] + a(1, 2) * v[2] + a(1, 3) * T(1);
    r.z = a(2, 0) * v[0] + a(2, 1) * v[1] + a(2, 2) * v[2] + a(2, 3) * T(1);

    return r;
  }

  template <typename T>
  Vector4<T> operator * (const Matrix4<T> &a, const Vector4<T> &v)
  {
    Vector4<T> r;

    r.x = a(0, 0) * v[0] + a(0, 1) * v[1] + a(0, 2) * v[2] + a(0, 3) * v[3];
    r.y = a(1, 0) * v[0] + a(1, 1) * v[1] + a(1, 2) * v[2] + a(1, 3) * v[3];
    r.z = a(2, 0) * v[0] + a(2, 1) * v[1] + a(2, 2) * v[2] + a(2, 3) * v[3];
    r.w = a(3, 0) * v[0] + a(3, 1) * v[1] + a(3, 2) * v[2] + a(3, 3) * v[3];

    return r;
  }


  template <typename T>
  const Matrix4<T> Matrix4<T>::zero(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
  template <typename T>
  const Matrix4<T> Matrix4<T>::identity(1, 0, 0, 0,
                                        0, 1, 0, 0,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1);

  template <typename T>
  Matrix4<T>::Matrix4(const T *array16)
  {
    for (int i = 0; i < 16; ++i)
    {
      m[i] = array16[i];
    }
  }

  template <typename T>
  Matrix4<T>::Matrix4(const Matrix4<T> &other)
  {
    for (int i = 0; i < 16; ++i)
    {
      m[i] = other.m[i];
    }
  }

  template <typename T>
  template <typename Q>
  Matrix4<T>::Matrix4(const Matrix4<Q> &other)
  {
    for (int i = 0; i < 16; ++i)
    {
      m[i] = T(other.m[i]);
    }
  }

  template <typename T>
  Matrix4<T>::Matrix4(const T &rc00, const T &rc01, const T &rc02, const T &rc03,
    const T &rc10, const T &rc11, const T &rc12, const T &rc13,
    const T &rc20, const T &rc21, const T &rc22, const T &rc23,
    const T &rc30, const T &rc31, const T &rc32, const T &rc33)
  {
    rc[0][0] = rc00; rc[0][1] = rc01; rc[0][2] = rc02; rc[0][3] = rc03;
    rc[1][0] = rc10; rc[1][1] = rc11; rc[1][2] = rc12; rc[1][3] = rc13;
    rc[2][0] = rc20; rc[2][1] = rc21; rc[2][2] = rc22; rc[2][3] = rc23;
    rc[3][0] = rc30; rc[3][1] = rc31; rc[3][2] = rc32; rc[3][3] = rc33;
  }

  template <typename T>
  Matrix4<T> Matrix4<T>::rotationX(const T &angle)
  {
    Matrix4<T> m = identity;
    T s = std::sin(angle);
    T c = std::cos(angle);
    m[5] = m[10] = c;
    m[6] = -s; m[9] = s;
    return m;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::rotationY(const T &angle)
  {
    Matrix4<T> m = identity;
    T s = std::sin(angle);
    T c = std::cos(angle);
    m[0] = m[10] = c;
    m[8] = -s; m[2] = s;
    return m;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::rotationZ(const T &angle)
  {
    Matrix4<T> m = identity;
    T s = std::sin(angle);
    T c = std::cos(angle);
    m[0] = m[5] = c;
    m[1] = -s; m[4] = s;
    return m;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::rotation(const T &x, const T &y, const T &z)
  {
    Matrix4<T> m = rotationZ(z);
    m = rotationY(y) * m;
    m = rotationX(x) * m;
    return m;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::translation(const Vector3<T> &trans)
  {
    Matrix4<T> m = identity;
    m.setTranslation(trans);
    return m;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::rotationTranslation(const T &x, const T &y, const T &z, const Vector3<T> &trans)
  {
    Matrix4<T> m = rotation(x, y, z);
    m.setTranslation(trans);
    return m;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::scaling(const Vector3<T> &scale)
  {
    Matrix4<T> m = identity;
    m.rc[0][0] = scale.x;
    m.rc[1][1] = scale.y;
    m.rc[2][2] = scale.z;
    return m;
  }

  template <typename T>
  Matrix4<T> Matrix4<T>::lookAt(const Vector3<T> &eye, const Vector3<T> &target, const Vector3<T> &axisUp, int forwardAxisIndex, int upAxisIndex)
  {
    if (forwardAxisIndex == upAxisIndex ||
        forwardAxisIndex < 0 || forwardAxisIndex > 2 ||
        upAxisIndex < 0 || upAxisIndex > 2)
    {
      // Bad axis specification.
      return identity;
    }

    Vector3<T> axes[3];
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
    axes[forwardAxisIndex] = (target - eye).normalised();
    axes[sideAxisIndex] = axes[forwardAxisIndex].cross(axisUp).normalised();
    axes[upAxisIndex] = axes[sideAxisIndex].cross(axes[forwardAxisIndex]);

    Matrix4<T> m = identity;
    m.setAxis(sideAxisIndex, axes[sideAxisIndex]);
    m.setAxis(forwardAxisIndex, axes[forwardAxisIndex]);
    m.setAxis(upAxisIndex, axes[upAxisIndex]);
    m.setTranslation(eye);

    return m;
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::transpose()
  {
    T temp;
    temp = rc[0][1];
    rc[0][1] = rc[1][0];
    rc[1][0] = temp;

    temp = rc[0][2];
    rc[0][2] = rc[2][0];
    rc[2][0] = temp;

    temp = rc[0][3];
    rc[0][3] = rc[3][0];
    rc[3][0] = temp;

    temp = rc[1][2];
    rc[1][2] = rc[2][1];
    rc[2][1] = temp;

    temp = rc[1][3];
    rc[1][3] = rc[3][1];
    rc[3][1] = temp;

    temp = rc[2][3];
    rc[2][3] = rc[3][2];
    rc[3][2] = temp;
    return *this;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::transposed() const
  {
    const Matrix4<T> m(rc[0][0], rc[1][0], rc[2][0], rc[3][0],
                       rc[0][1], rc[1][1], rc[2][1], rc[3][1],
                       rc[0][2], rc[1][2], rc[2][2], rc[3][2],
                       rc[0][3], rc[1][3], rc[2][3], rc[3][3]);
    return m;
  }

  template <typename T>
  Matrix4<T> &Matrix4<T>::invert()
  {
    // Inversion with Cramer's rule
    //
    // 1. Transpose the matrix.
    // 2. Calculate cofactors of matrix elements. Form a new matrix from cofactors of the given matrix elements.
    // 3. Calculate the determinant of the given matrix.
    // 4. Multiply the matrix obtained in step 3 by the reciprocal of the determinant.
    //
    Matrix4<T> transpose = transposed();        // transposed source matrix
    T pairs[12];                                // temp array for cofactors
    T det;                                      // determinant

    // calculate pairs for first 8 elements
    pairs[0] = transpose.m[10] * transpose.m[15];
    pairs[1] = transpose.m[14] * transpose.m[11];
    pairs[2] = transpose.m[6] * transpose.m[15];
    pairs[3] = transpose.m[14] * transpose.m[7];
    pairs[4] = transpose.m[6] * transpose.m[11];
    pairs[5] = transpose.m[10] * transpose.m[7];
    pairs[6] = transpose.m[2] * transpose.m[15];
    pairs[7] = transpose.m[14] * transpose.m[3];
    pairs[8] = transpose.m[2] * transpose.m[11];
    pairs[9] = transpose.m[10] * transpose.m[3];
    pairs[10] = transpose.m[2] * transpose.m[7];
    pairs[11] = transpose.m[6] * transpose.m[3];

    // calculate first 8 elements (cofactors)
    m[0]   = pairs[0] * transpose.m[5] + pairs[3] * transpose.m[9] + pairs[4] * transpose.m[13];
    m[0]  -= pairs[1] * transpose.m[5] + pairs[2] * transpose.m[9] + pairs[5] * transpose.m[13];
    m[4]   = pairs[1] * transpose.m[1] + pairs[6] * transpose.m[9] + pairs[9] * transpose.m[13];
    m[4]  -= pairs[0] * transpose.m[1] + pairs[7] * transpose.m[9] + pairs[8] * transpose.m[13];
    m[8]   = pairs[2] * transpose.m[1] + pairs[7] * transpose.m[5] + pairs[10] * transpose.m[13];
    m[8]  -= pairs[3] * transpose.m[1] + pairs[6] * transpose.m[5] + pairs[11] * transpose.m[13];
    m[12]  = pairs[5] * transpose.m[1] + pairs[8] * transpose.m[5] + pairs[11] * transpose.m[9];
    m[12] -= pairs[4] * transpose.m[1] + pairs[9] * transpose.m[5] + pairs[10] * transpose.m[9];
    m[1]   = pairs[1] * transpose.m[4] + pairs[2] * transpose.m[8] + pairs[5] * transpose.m[12];
    m[1]  -= pairs[0] * transpose.m[4] + pairs[3] * transpose.m[8] + pairs[4] * transpose.m[12];
    m[5]   = pairs[0] * transpose.m[0] + pairs[7] * transpose.m[8] + pairs[8] * transpose.m[12];
    m[5]  -= pairs[1] * transpose.m[0] + pairs[6] * transpose.m[8] + pairs[9] * transpose.m[12];
    m[9]   = pairs[3] * transpose.m[0] + pairs[6] * transpose.m[4] + pairs[11] * transpose.m[12];
    m[9]  -= pairs[2] * transpose.m[0] + pairs[7] * transpose.m[4] + pairs[10] * transpose.m[12];
    m[13]  = pairs[4] * transpose.m[0] + pairs[9] * transpose.m[4] + pairs[10] * transpose.m[8];
    m[13] -= pairs[5] * transpose.m[0] + pairs[8] * transpose.m[4] + pairs[11] * transpose.m[8];

    // calculate pairs for second 8 elements (cofactors)
    pairs[0] = transpose.m[8] * transpose.m[13];
    pairs[1] = transpose.m[12] * transpose.m[9];
    pairs[2] = transpose.m[4] * transpose.m[13];
    pairs[3] = transpose.m[12] * transpose.m[5];
    pairs[4] = transpose.m[4] * transpose.m[9];
    pairs[5] = transpose.m[8] * transpose.m[5];
    pairs[6] = transpose.m[0] * transpose.m[13];
    pairs[7] = transpose.m[12] * transpose.m[1];
    pairs[8] = transpose.m[0] * transpose.m[9];
    pairs[9] = transpose.m[8] * transpose.m[1];
    pairs[10] = transpose.m[0] * transpose.m[5];
    pairs[11] = transpose.m[4] * transpose.m[1];

    // calculate second 8 elements (cofactors)
    m[2]   = pairs[ 0] * transpose.m[ 7] + pairs[ 3] * transpose.m[11] + pairs[ 4] * transpose.m[15];
    m[2]  -= pairs[ 1] * transpose.m[ 7] + pairs[ 2] * transpose.m[11] + pairs[ 5] * transpose.m[15];
    m[6]   = pairs[ 1] * transpose.m[ 3] + pairs[ 6] * transpose.m[11] + pairs[ 9] * transpose.m[15];
    m[6]  -= pairs[ 0] * transpose.m[ 3] + pairs[ 7] * transpose.m[11] + pairs[ 8] * transpose.m[15];
    m[10]  = pairs[ 2] * transpose.m[ 3] + pairs[ 7] * transpose.m[ 7] + pairs[10] * transpose.m[15];
    m[10] -= pairs[ 3] * transpose.m[ 3] + pairs[ 6] * transpose.m[ 7] + pairs[11] * transpose.m[15];
    m[14]  = pairs[ 5] * transpose.m[ 3] + pairs[ 8] * transpose.m[ 7] + pairs[11] * transpose.m[11];
    m[14] -= pairs[ 4] * transpose.m[ 3] + pairs[ 9] * transpose.m[ 7] + pairs[10] * transpose.m[11];
    m[3]   = pairs[ 2] * transpose.m[10] + pairs[ 5] * transpose.m[14] + pairs[ 1] * transpose.m[6];
    m[3]  -= pairs[ 4] * transpose.m[14] + pairs[ 0] * transpose.m[ 6] + pairs[ 3] * transpose.m[10];
    m[7]   = pairs[ 8] * transpose.m[14] + pairs[ 0] * transpose.m[ 2] + pairs[ 7] * transpose.m[10];
    m[7]  -= pairs[ 6] * transpose.m[10] + pairs[ 9] * transpose.m[14] + pairs[ 1] * transpose.m[2];
    m[11]  = pairs[ 6] * transpose.m[ 6] + pairs[11] * transpose.m[14] + pairs[ 3] * transpose.m[2];
    m[11] -= pairs[10] * transpose.m[14] + pairs[ 2] * transpose.m[ 2] + pairs[ 7] * transpose.m[6];
    m[15]  = pairs[10] * transpose.m[10] + pairs[ 4] * transpose.m[ 2] + pairs[ 9] * transpose.m[6];
    m[15] -= pairs[ 8] * transpose.m[ 6] + pairs[11] * transpose.m[10] + pairs[ 5] * transpose.m[2];

    // calculate determinant
    det = transpose.m[0] * m[0] + transpose.m[4] * m[4] + transpose.m[8] * m[8] + transpose.m[12] * m[12];

    // calculate matrix inverse
    const T detInv = T(1) / det;
    for (int i = 0; i < 16; ++i)
    {
      m[i] *= detInv;
    }

    return *this;
  }

  template <typename T>
  Matrix4<T> Matrix4<T>::inverse() const
  {
    Matrix4<T> m = *this;
    m.invert();
    return m;
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::rigidBodyInvert()
  {
    // Transpose 3x3.
    T temp;
    temp = rc[0][1];
    rc[0][1] = rc[1][0];
    rc[1][0] = temp;

    temp = rc[0][2];
    rc[0][2] = rc[2][0];
    rc[2][0] = temp;

    temp = rc[1][2];
    rc[1][2] = rc[2][1];
    rc[2][1] = temp;

    // Negate translation.
    rc[0][3] = -rc[0][3]; rc[1][3] = -rc[1][3]; rc[2][3] = -rc[2][3];

    // Multiply by the negated translation.
    Vector3<T> v;
    v.x = rc[0][0] * rc[0][3] + rc[0][1] * rc[1][3] + rc[0][2] * rc[2][3];
    v.y = rc[1][0] * rc[0][3] + rc[1][1] * rc[1][3] + rc[1][2] * rc[2][3];
    v.z = rc[2][0] * rc[0][3] + rc[2][1] * rc[1][3] + rc[2][2] * rc[2][3];

    // Set the new translation.
    setTranslation(v);

    return *this;
  }

  template <typename T>
  inline Matrix4<T> Matrix4<T>::rigidBodyInverse() const
  {
    Matrix4<T> m = *this;
    m.rigidBodyInvert();
    return m;
  }

  template <typename T>
  T Matrix4<T>::determinant() const
  {
    Matrix4<T> transpose(transposed());         // transposed source matrix
    T pairs[12];                                // temp array for cofactors
    T tmp[4];

    // calculate pairs for first 8 elements
    pairs[0] = transpose.m[10] * transpose.m[15];
    pairs[1] = transpose.m[14] * transpose.m[11];
    pairs[2] = transpose.m[6] * transpose.m[15];
    pairs[3] = transpose.m[14] * transpose.m[7];
    pairs[4] = transpose.m[6] * transpose.m[11];
    pairs[5] = transpose.m[10] * transpose.m[7];
    pairs[6] = transpose.m[2] * transpose.m[15];
    pairs[7] = transpose.m[14] * transpose.m[3];
    pairs[8] = transpose.m[2] * transpose.m[11];
    pairs[9] = transpose.m[10] * transpose.m[3];
    pairs[10] = transpose.m[2] * transpose.m[7];
    pairs[11] = transpose.m[6] * transpose.m[3];

    // calculate first 8 elements (cofactors)
    tmp[0]  = pairs[0] * transpose.m[5] + pairs[3] * transpose.m[9] + pairs[4] * transpose.m[13];
    tmp[0] -= pairs[1] * transpose.m[5] + pairs[2] * transpose.m[9] + pairs[5] * transpose.m[13];
    tmp[1]  = pairs[1] * transpose.m[1] + pairs[6] * transpose.m[9] + pairs[9] * transpose.m[13];
    tmp[1] -= pairs[0] * transpose.m[1] + pairs[7] * transpose.m[9] + pairs[8] * transpose.m[13];
    tmp[2]  = pairs[2] * transpose.m[1] + pairs[7] * transpose.m[5] + pairs[10] * transpose.m[13];
    tmp[2] -= pairs[3] * transpose.m[1] + pairs[6] * transpose.m[5] + pairs[11] * transpose.m[13];
    tmp[3]  = pairs[5] * transpose.m[1] + pairs[8] * transpose.m[5] + pairs[11] * transpose.m[9];
    tmp[3] -= pairs[4] * transpose.m[1] + pairs[9] * transpose.m[5] + pairs[10] * transpose.m[9];

    // calculate determinant
    return (transpose.m[0] * tmp[0] + transpose.m[4] * tmp[1] + transpose.m[8] * tmp[2] + transpose.m[12] * tmp[3]);
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::axisX() const
  {
    return axis(0);
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::axisY() const
  {
    return axis(1);
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::axisZ() const
  {
    return axis(2);
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::axisT() const
  {
    return axis(3);
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::translation() const
  {
    return axis(3);
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::axis(int index) const
  {
    const Vector3<T> v(rc[0][index], rc[1][index], rc[2][index]);
    return v;
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::setAxisX(const Vector3<T> &axis)
  {
    return setAxis(0, axis);
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::setAxisY(const Vector3<T> &axis)
  {
    return setAxis(1, axis);
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::setAxisZ(const Vector3<T> &axis)
  {
    return setAxis(2, axis);
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::setAxisT(const Vector3<T> &axis)
  {
    return setAxis(3, axis);
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::setTranslation(const Vector3<T> &axis)
  {
    return setAxis(3, axis);
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::setAxis(int index, const Vector3<T> &axis)
  {
    rc[0][index] = axis.x;
    rc[1][index] = axis.y;
    rc[2][index] = axis.z;
    return *this;
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::scale() const
  {
    const Vector3<T> v(axisX().magnitude(), axisY().magnitude(), axisZ().magnitude());
    return v;
  }

  template <typename T>
  inline Matrix4<T> &Matrix4<T>::scale(const Vector3<T> &scaling)
  {
    rc[0][0] *= scaling.x;
    rc[1][0] *= scaling.x;
    rc[2][0] *= scaling.x;
    rc[3][0] *= scaling.x;

    rc[0][1] *= scaling.y;
    rc[1][1] *= scaling.y;
    rc[2][1] *= scaling.y;
    rc[3][1] *= scaling.y;

    rc[0][2] *= scaling.z;
    rc[1][2] *= scaling.z;
    rc[2][2] *= scaling.z;
    rc[3][2] *= scaling.z;

    return *this;
  }

  template <class T>
  inline Vector3<T> Matrix4<T>::removeScale()
  {
    Vector3<T> scale(axisX().magnitude(), axisY().magnitude(), axisZ().magnitude());
    this->scale(Vector3<T>(T(1) / scale.x, T(1) / scale.y, T(1) / scale.z));
    return scale;
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::transform(const Vector3<T> &v) const
  {
    return *this * v;
  }

  template <typename T>
  inline Vector3<T> Matrix4<T>::rotate(const Vector3<T> &v) const
  {
    Vector3<T> r;

    r.x = (*this)(0, 0) * v[0] + (*this)(0, 1) * v[1] + (*this)(0, 2) * v[2];
    r.y = (*this)(1, 0) * v[0] + (*this)(1, 1) * v[1] + (*this)(1, 2) * v[2];
    r.z = (*this)(2, 0) * v[0] + (*this)(2, 1) * v[1] + (*this)(2, 2) * v[2];

    return r;
  }

  template <typename T>
  inline Vector4<T> Matrix4<T>::transform(const Vector4<T> &v) const
  {
    return *this * v;
  }

  template <typename T>
  inline Vector4<T> Matrix4<T>::rotate(const Vector4<T> &v) const
  {
    Vector4<T> r;

    r.x = (*this)(0, 0) * v[0] + (*this)(0, 1) * v[1] + (*this)(0, 2) * v[2];
    r.y = (*this)(1, 0) * v[0] + (*this)(1, 1) * v[1] + (*this)(1, 2) * v[2];
    r.z = (*this)(2, 0) * v[0] + (*this)(2, 1) * v[1] + (*this)(2, 2) * v[2];
    r.w = (*this)(3, 0) * v[0] + (*this)(3, 1) * v[1] + (*this)(3, 2) * v[2];

    return r;
  }


  template <typename T>
  inline bool Matrix4<T>::equals(const Matrix4<T> &a, const T epsilon) const
  {
    return std::abs(m[0] - a.m[0]) <= epsilon &&
           std::abs(m[1] - a.m[1]) <= epsilon &&
           std::abs(m[2] - a.m[2]) <= epsilon &&
           std::abs(m[3] - a.m[3]) <= epsilon &&
           std::abs(m[4] - a.m[4]) <= epsilon &&
           std::abs(m[5] - a.m[5]) <= epsilon &&
           std::abs(m[6] - a.m[6]) <= epsilon &&
           std::abs(m[7] - a.m[7]) <= epsilon &&
           std::abs(m[8] - a.m[8]) <= epsilon &&
           std::abs(m[9] - a.m[9]) <= epsilon &&
           std::abs(m[10] - a.m[10]) <= epsilon &&
           std::abs(m[11] - a.m[11]) <= epsilon &&
           std::abs(m[12] - a.m[12]) <= epsilon &&
           std::abs(m[13] - a.m[13]) <= epsilon &&
           std::abs(m[14] - a.m[14]) <= epsilon &&
           std::abs(m[15] - a.m[15]) <= epsilon;
  }
}
