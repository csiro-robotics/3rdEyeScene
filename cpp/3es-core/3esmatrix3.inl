//
// author: Kazys Stepanas
//
#include <cstdlib>

namespace tes
{
  template <typename T>
  Matrix3<T> operator * (const Matrix3<T> &a, const Matrix3<T> &b)
  {
    Matrix3<T> m;
    m(0, 0) = a(0, 0) * b(0, 0) + a(0, 1) * b(1, 0) + a(0, 2) * b(2, 0);
    m(0, 1) = a(0, 0) * b(0, 1) + a(0, 1) * b(1, 1) + a(0, 2) * b(2, 1);
    m(0, 2) = a(0, 0) * b(0, 2) + a(0, 1) * b(1, 2) + a(0, 2) * b(2, 2);

    m(1, 0) = a(1, 0) * b(0, 0) + a(1, 1) * b(1, 0) + a(1, 2) * b(2, 0);
    m(1, 1) = a(1, 0) * b(0, 1) + a(1, 1) * b(1, 1) + a(1, 2) * b(2, 1);
    m(1, 2) = a(1, 0) * b(0, 2) + a(1, 1) * b(1, 2) + a(1, 2) * b(2, 2);

    m(2, 0) = a(2, 0) * b(0, 0) + a(2, 1) * b(1, 0) + a(2, 2) * b(2, 0);
    m(2, 1) = a(2, 0) * b(0, 1) + a(2, 1) * b(1, 1) + a(2, 2) * b(2, 1);
    m(2, 2) = a(2, 0) * b(0, 2) + a(2, 1) * b(1, 2) + a(2, 2) * b(2, 2);

    return m;
  }

  template <typename T>
  Vector3<T> operator * (const Matrix3<T> &a, const Vector3<T> &v)
  {
    Vector3<T> r;

    r.x = a(0, 0) * v[0] + a(0, 1) * v[1] + a(0, 2) * v[2];
    r.y = a(1, 0) * v[0] + a(1, 1) * v[1] + a(1, 2) * v[2];
    r.z = a(2, 0) * v[0] + a(2, 1) * v[1] + a(2, 2) * v[2];

    return r;
  }


  template <typename T>
  const Matrix3<T> Matrix3<T>::zero(0, 0, 0, 0, 0, 0, 0, 0, 0);
  template <typename T>
  const Matrix3<T> Matrix3<T>::identity(1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1);

  template <typename T>
  Matrix3<T>::Matrix3(const T *array9)
  {
    for (int i = 0; i < 9; ++i)
    {
      m[i] = array9[i];
    }
  }

  template <typename T>
  Matrix3<T>::Matrix3(const Matrix3<T> &other)
  {
    for (int i = 0; i < 9; ++i)
    {
      m[i] = other.m[i];
    }
  }

  template <typename T>
  template <typename Q>
  Matrix3<T>::Matrix3(const Matrix3<Q> &other)
  {
    for (int i = 0; i < 9; ++i)
    {
      m[i] = T(other.m[i]);
    }
  }

  template <typename T>
  Matrix3<T>::Matrix3(const T &rc00, const T &rc01, const T &rc02,
                      const T &rc10, const T &rc11, const T &rc12,
                      const T &rc20, const T &rc21, const T &rc22)
  {
    rc[0][0] = rc00; rc[0][1] = rc01; rc[0][2] = rc02;
    rc[1][0] = rc10; rc[1][1] = rc11; rc[1][2] = rc12;
    rc[2][0] = rc20; rc[2][1] = rc21; rc[2][2] = rc22;
  }

  template <typename T>
  Matrix3<T> Matrix3<T>::rotationX(const T &angle)
  {
    Matrix3<T> m = identity;
    T s = std::sin(angle);
    T c = std::cos(angle);
    m[4] = m[8] = c;
    m[5] = -s; m[7] = s;
    return m;
  }

  template <typename T>
  inline Matrix3<T> Matrix3<T>::rotationY(const T &angle)
  {
    Matrix3<T> m = identity;
    T s = std::sin(angle);
    T c = std::cos(angle);
    m[0] = m[8] = c;
    m[6] = -s; m[2] = s;
    return m;
  }

  template <typename T>
  inline Matrix3<T> Matrix3<T>::rotationZ(const T &angle)
  {
    Matrix3<T> m = identity;
    T s = std::sin(angle);
    T c = std::cos(angle);
    m[0] = m[4] = c;
    m[1] = -s; m[3] = s;
    return m;
  }

  template <typename T>
  inline Matrix3<T> Matrix3<T>::rotation(const T &x, const T &y, const T &z)
  {
    Matrix3<T> m = rotationZ(x);
    m = rotationX(y) * m;
    m = rotationZ(z) * m;
    return m;
  }

  template <typename T>
  inline Matrix3<T> Matrix3<T>::scaling(const Vector3<T> &scale)
  {
    Matrix3<T> m = identity;
    m.rc[0][0] = scale.x;
    m.rc[1][1] = scale.y;
    m.rc[2][2] = scale.z;
    return m;
  }

  template <typename T>
  Matrix3<T> Matrix3<T>::lookAt(const Vector3<T> &eye, const Vector3<T> &target, const Vector3<T> &axisUp, int forwardAxisIndex, int upAxisIndex)
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

    Matrix3<T> m = identity;
    m.setAxis(sideAxisIndex, axes[sideAxisIndex]);
    m.setAxis(forwardAxisIndex, axes[forwardAxisIndex]);
    m.setAxis(upAxisIndex, axes[upAxisIndex]);

    return m;
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::transpose()
  {
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
    return *this;
  }

  template <typename T>
  inline Matrix3<T> Matrix3<T>::transposed() const
  {
    const Matrix3<T> m(rc[0][0], rc[1][0], rc[2][0],
                       rc[0][1], rc[1][1], rc[2][1],
                       rc[0][2], rc[1][2], rc[2][2]);
    return m;
  }

  template <typename T>
  Matrix3<T> &Matrix3<T>::invert()
  {
    Matrix3<T> adj;
    const T det = getAdjoint(adj);
    const T detInv = T(1) / det;

    for (int i = 0; i < 9; ++i)
    {
      m[i] = adj[i] * detInv;
    }

    return *this;
  }

  template <typename T>
  Matrix3<T> Matrix3<T>::inverse() const
  {
    Matrix3<T> inv;
    const T det = getAdjoint(inv);
    const T detInv = T(1) / det;

    for (int i = 0; i < 9; ++i)
    {
      inv[i] *= detInv;
    }

    return inv;
  }

  template <typename T>
  T Matrix3<T>::getAdjoint(Matrix3<T> &adj) const
  {
    adj.m[0] = m[4] * m[8] - m[7] * m[5];
    adj.m[1] = m[7] * m[2] - m[1] * m[8];
    adj.m[2] = m[1] * m[5] - m[4] * m[2];
    adj.m[3] = m[6] * m[5] - m[3] * m[8];
    adj.m[4] = m[0] * m[8] - m[6] * m[2];
    adj.m[5] = m[3] * m[2] - m[0] * m[5];
    adj.m[6] = m[3] * m[7] - m[6] * m[4];
    adj.m[7] = m[6] * m[1] - m[0] * m[7];
    adj.m[8] = m[0] * m[4] - m[3] * m[1];

    return m[0] * adj.m[0] + m[1] * adj.m[3] + m[2] * adj.m[6];
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::rigidBodyInvert()
  {
    return transpose();
  }

  template <typename T>
  inline Matrix3<T> Matrix3<T>::rigidBodyInverse() const
  {
    return transposed();
  }

  template <typename T>
  inline T Matrix3<T>::determinant() const
  {
    const T det = m[0] * m[4] * m[8] + m[1] * m[5] * m[6] + m[2] * m[3] * m[7] -
                  m[2] * m[4] * m[6] - m[1] * m[3] * m[8] - m[0] * m[5] * m[7];
    return det;
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::axisX() const
  {
    return axis(0);
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::axisY() const
  {
    return axis(1);
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::axisZ() const
  {
    return axis(2);
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::axis(int index) const
  {
    const Vector3<T> v(rc[0][index], rc[1][index], rc[2][index]);
    return v;
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::setAxisX(const Vector3<T> &axis)
  {
    return setAxis(0, axis);
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::setAxisY(const Vector3<T> &axis)
  {
    return setAxis(1, axis);
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::setAxisZ(const Vector3<T> &axis)
  {
    return setAxis(2, axis);
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::setAxis(int index, const Vector3<T> &axis)
  {
    rc[0][index] = axis.x;
    rc[1][index] = axis.y;
    rc[2][index] = axis.z;
    return *this;
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::scale() const
  {
    const Vector3<T> v(axisX().magnitude(), axisY().magnitude(), axisZ().magnitude());
    return v;
  }

  template <typename T>
  inline Matrix3<T> &Matrix3<T>::scale(const Vector3<T> &scaling)
  {
    rc[0][0] *= scaling.x;
    rc[1][0] *= scaling.x;
    rc[2][0] *= scaling.x;

    rc[0][1] *= scaling.y;
    rc[1][1] *= scaling.y;
    rc[2][1] *= scaling.y;

    rc[0][2] *= scaling.z;
    rc[1][2] *= scaling.z;
    rc[2][2] *= scaling.z;

    return *this;
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::transform(const Vector3<T> &v) const
  {
    return *this * v;
  }

  template <typename T>
  inline Vector3<T> Matrix3<T>::rotate(const Vector3<T> &v) const
  {
    Vector3<T> r;

    r.x = (*this)(0, 0) * v[0] + (*this)(0, 1) * v[1] + (*this)(0, 2) * v[2];
    r.y = (*this)(1, 0) * v[0] + (*this)(1, 1) * v[1] + (*this)(1, 2) * v[2];
    r.z = (*this)(2, 0) * v[0] + (*this)(2, 1) * v[1] + (*this)(2, 2) * v[2];

    return r;
  }


  template <typename T>
  inline bool Matrix3<T>::equals(const Matrix3<T> &a, const T epsilon) const
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
           std::abs(m[9] - a.m[9]) <= epsilon;
  }
}
