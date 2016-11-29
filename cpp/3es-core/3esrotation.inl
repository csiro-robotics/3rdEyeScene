//
// author: Kazys Stepanas
//

namespace tes
{
  template <typename T>
  inline Matrix3<T> operator * (const Matrix3<T> &a, const Quaternion<T> &q)
  {
    Matrix3<T> b = quaternionToRotation(q);
    return a * b;
  }

  template <typename T>
  inline Matrix3<T> operator * (const Quaternion<T> &q, const Matrix3<T> &b)
  {
    Matrix3<T> a = quaternionToRotation(q);
    return a * b;
  }

  template <typename T>
  inline Matrix4<T> operator * (const Matrix4<T> &a, const Quaternion<T> &q)
  {
    Matrix4<T> b = quaternionToTransform(q);
    return a * b;
  }

  template <typename T>
  inline Matrix4<T> operator * (const Quaternion<T> &q, const Matrix4<T> &b)
  {
    Matrix4<T> a = quaternionToTransform(q);
    return a * b;
  }

  template <typename T>
  inline Matrix4<T> quaternionTranslationToTransform(const Quaternion<T> &quaternion, const Vector3<T> &translation)
  {
    Matrix4<T> m = quaternionToTransform(quaternion);
    m.setTranslation(translation);
    return m;
  }

  template <typename T>
  inline Matrix4<T> prsTransform(const Vector3<T> &translation, const Quaternion<T> &quaternion, const Vector3<T> &scale)
  {
    Matrix4<T> m = quaternionToTransform(quaternion);
    m.setTranslation(translation);
    m.scale(scale);
    return m;
  }

  template <typename T>
  inline void transformToQuaternionTranslation(const Matrix4<T> &m, Quaternion<T> &q, Vector3<T> &translation, Vector3<T> *scalePtr)
  {
    Matrix4<T> m2 = m;
    Vector3<T> scale = m2.removeScale();
    if (scalePtr)
    {
      *scalePtr = scale;
    }
    q = transformToQuaternion(m2);
    translation = m2.translation();
  }


  template <typename T>
  inline void transformToQuaternionTranslation(const Matrix4<T> &m, Quaternion<T> &q, Vector3<T> &translation, Vector3<T> &scale)
  {
    return transformToQuaternionTranslation(m, q, translation, &scale);
  }

  template <typename T, class M>
  Quaternion<T> matrixToQuaternion(const M &m)
  {
    // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
    // article "CSLQuaternion Calculus and Fast Animation".
    Quaternion<T> q;
    T trace = m(0, 0) + m(1, 1) + m(2, 2);
    T root;
    const int next[3] = { 1, 2, 0 };
    int i = 0, j, k;

    if (trace >= 0.0f)
    {
      // |w| > 1/2, may as well choose w > 1/2
      root = std::sqrt(trace + 1.0f);  // 2w
      q.w = 0.5f * root;
      root = 0.5f / root;  // 1/(4w)
      q.x = (m(2, 1) - m(1, 2)) * root;
      q.y = (m(0, 2) - m(2, 0)) * root;
      q.z = (m(1, 0) - m(0, 1)) * root;
    }
    else
    {
      // |w| <= 1/2
      if (m(1, 1) > m(0, 0))
      {
        i = 1;
      }
      if (m(2, 2) > m(i, i))
      {
        i = 2;
      }
      j = next[i];
      k = next[j];

      root = std::sqrt(m(i, i) - m(j, j) - m(k, k) + 1.0f);
      q.q[i] = 0.5f * root;
      root = 0.5f / root;
      q.w = (m(k, j) - m(j, k)) * root;
      q.q[j] = (m(j, i) + m(i, j)) * root;
      q.q[k] = (m(k, i) + m(i, k)) * root;
    }

    q.normalise();
    return q;
  }

  template <typename T>
  inline Quaternion<T> rotationToQuaternion(const Matrix3<T> &m)
  {
    return matrixToQuaternion<T>(m);
  }

  template <typename T>
  inline Quaternion<T> transformToQuaternion(const Matrix4<T> &m)
  {
    return matrixToQuaternion<T>(m);
  }

  template <class M, typename T>
  M quaternionToMatrix(const Quaternion<T> &q)
  {
    M m;
    T tx = q.x + q.x;
    T ty = q.y + q.y;
    T tz = q.z + q.z;
    T twx = tx * q.w;
    T twy = ty * q.w;
    T twz = tz * q.w;
    T txx = tx * q.x;
    T txy = ty * q.x;
    T txz = tz * q.x;
    T tyy = ty * q.y;
    T tyz = tz * q.y;
    T tzz = tz * q.z;

    m(0, 0) = T(1) - (tyy + tzz);
    m(0, 1) = txy - twz;
    m(0, 2) = txz + twy;
    m(1, 0) = txy + twz;
    m(1, 1) = T(1) - (txx + tzz);
    m(1, 2) = tyz - twx;
    m(2, 0) = txz - twy;
    m(2, 1) = tyz + twx;
    m(2, 2) = T(1) - (txx + tyy);

    return m;
  }


  template <typename T>
  Matrix3<T> quaternionToRotation(const Quaternion<T> &q)
  {
    return quaternionToMatrix<Matrix3<T> >(q);
  }


  template <typename T>
  Matrix4<T> quaternionToTransform(const Quaternion<T> &q)
  {
    Matrix4<T> m = quaternionToMatrix<Matrix4<T> >(q);
    m(3, 0) = m(3, 1) = m(3, 2) = m(0, 3) = m(1, 3) = m(2, 3) = T(0);
    m(3, 3) = T(1);
    return m;
  }
}
