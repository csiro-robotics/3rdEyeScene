//
// author: Kazys Stepanas
//

namespace tes
{
  template <typename T>
  inline Quaternion<T> operator * (const Quaternion<T> &a, const Quaternion<T> &b)
  {
    Quaternion<T> q;
    q.x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
    q.y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x;
    q.z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w;
    q.w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
    return q;
  }


  template <typename T>
  inline Vector3<T> operator * (const Quaternion<T> &a, const Vector3<T> &v)
  {
    return a.transform(v);
  }

  template <typename T>
  Quaternion<T>::Quaternion(const Vector3<T> &from, const Vector3<T> &to)
  {
    Vector3<T> half = from + to;
    half.normalise();
    Vector3<T> v = from.cross(half);
    x = v.x;
    y = v.y;
    z = v.z;
    w = from.dot(half);
  }


  template <typename T>
  bool Quaternion<T>::operator==(const Quaternion<T> &other) const
  {
    return x == other.x && y == other.y && z == other.z && w == other.w;
  }


  template <typename T>
  bool Quaternion<T>::operator!=(const Quaternion<T> &other) const
  {
    return x != other.x || y != other.y || z != other.z || w != other.w;
  }


  template <typename T>
  bool Quaternion<T>::isEqual(const Quaternion<T> &other, const T &epsilon)
  {
    return std::abs(x - other.x) <= epsilon &&
           std::abs(y - other.y) <= epsilon &&
           std::abs(z - other.z) <= epsilon &&
           std::abs(w - other.w) <= epsilon;
  }


  template <typename T>
  bool Quaternion<T>::isIdentity()
  {
    return *this == identity;
  }


  template <typename T>
  void Quaternion<T>::getAxisAngle(Vector3<T> &axis, T &angle) const
  {
    T mag = x * x + y * y + z * z;

    if (mag <= Vector3<T>::Epsilon)
    {
      axis = Vector3<T>::axisz;
      angle = 0;
      return;
    }

    const T magInv = T(1) / mag;
    axis = Vector3<T>(x * magInv, y * magInv, z * magInv);
    angle = T(2) * std::acos(w);
  }


  template <typename T>
  Quaternion<T> &Quaternion<T>::setAxisAngle(const Vector3<T> &axis, const T &angle)
  {
    T sinHalfAngle = std::sin(T(0.5) * angle);
    w = std::cos(T(0.5) * angle);
    x = axis.x * sinHalfAngle;
    y = axis.y * sinHalfAngle;
    z = axis.z * sinHalfAngle;
    normalise();
    return *this;
  }


  template <typename T>
  Quaternion<T> &Quaternion<T>::invert()
  {
    T mag2 = magnitudeSquared();
    conjugate();
    *this *= T(1) / mag2;
    return *this;
  }


  template <typename T>
  Quaternion<T> Quaternion<T>::inverse() const
  {
    Quaternion<T> q = *this;
    return q.invert();
  }


  template <typename T>
  Quaternion<T> &Quaternion<T>::conjugate()
  {
    x = -x;
    y = -y;
    z = -z;
    return *this;
  }


  template <typename T>
  Quaternion<T> Quaternion<T>::conjugated() const
  {
    Quaternion<T> q = *this;
    return q.conjugate();
  }


  template <typename T>
  T Quaternion<T>::normalise(const T &epsilon)
  {
    T mag = magnitude();
    if (mag <= epsilon)
    {
      *this = identity;
      return 0;
    }

    const T magInv = T(1) / mag;
    x *= magInv;
    y *= magInv;
    z *= magInv;
    w *= magInv;

    return mag;
  }


  template <typename T>
  Quaternion<T> Quaternion<T>::normalised(const T &epsilon) const
  {
    Quaternion<T> q = *this;
    q.normalise();
    return q;
  }


  template <typename T>
  T Quaternion<T>::magnitude() const
  {
    return std::sqrt(magnitudeSquared());
  }


  template <typename T>
  T Quaternion<T>::magnitudeSquared() const
  {
    return x * x + y * y + z * z + w * w;
  }


  template <typename T>
  T Quaternion<T>::dot(const Quaternion<T> other) const
  {
    return x * other.x + y * other.y + z * other.z + w * other.w;
  }


  template <typename T>
  Vector3<T> Quaternion<T>::transform(const Vector3<T> &v) const
  {
    const T xx = x*x, xy = x*y, xz = x*z, xw = x*w;
    const T yy = y*y, yz = y*z, yw = y*w;
    const T zz = z*z, zw = z*w;

    Vector3<T> res;

    res.x = (1 - 2 * (yy + zz)) * v.x +
            (2 * (xy - zw)) * v.y +
            (2 * (xz + yw)) * v.z;

    res.y = (2 * (xy + zw)) * v.x +
            (1 - 2 * (xx + zz)) * v.y +
            (2 * (yz - xw)) * v.z;

    res.z = (2 * (xz - yw)) * v.x +
            (2 * (yz + xw)) * v.y +
            (1 - 2 * (xx + yy)) * v.z;

    return res;
  }


  template <typename T>
  Quaternion<T> &Quaternion<T>::multiply(const T &scalar)
  {
    x *= scalar;
    y *= scalar;
    z *= scalar;
    w *= scalar;
    return *this;
  }


  template <typename T>
  Quaternion<T> Quaternion<T>::slerp(const Quaternion<T> &from, const Quaternion<T> &to, const T &t)
  {
    T dCoeff0, dCoeff1, dAngle, dSin, dCos, dInvSin;

    if (from == to)
    {
      return from;
    }

    dCos = from.dot(to);

    Quaternion<T> temp;

    // numerical round-off error could create problems in call to acos
    if (dCos < T(0))
    {
      dCos = -dCos;
      temp.x = -to.x;
      temp.y = -to.y;
      temp.z = -to.z;
      temp.w = -to.w;
    }
    else
    {
      temp.x = to.x;
      temp.y = to.y;
      temp.z = to.z;
      temp.w = to.w;
    }

    if ((T(1) - dCos) > Vector3<T>::Epsilon)
    {
      dAngle = std::acos(dCos);
      dSin = std::sin(dAngle);  // fSin >= 0 since fCos >= 0

      dInvSin = T(1) / dSin;
      dCoeff0 = std::sin((T(1) - t) * dAngle) * dInvSin;
      dCoeff1 = std::sin(t * dAngle) * dInvSin;
    }
    else
    {
      dCoeff0 = T(1) - t;
      dCoeff1 = t;
    }

    temp.x = dCoeff0 * from.x + dCoeff1 * temp.x;
    temp.y = dCoeff0 * from.y + dCoeff1 * temp.y;
    temp.z = dCoeff0 * from.z + dCoeff1 * temp.z;
    temp.w = dCoeff0 * from.w + dCoeff1 * temp.w;

    return temp;
  }


  template<typename T>
  inline Quaternion<T> Quaternion<T>::operator *= (const Quaternion<T> &other)
  {
    Quaternion<T> q = *this * other;
    *this = q;
    return *this;
  }


  template <typename T> const Quaternion<T> Quaternion<T>::identity(0, 0, 0, 1);
}
