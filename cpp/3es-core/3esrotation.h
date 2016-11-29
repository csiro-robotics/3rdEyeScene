//
// author: Kazys Stepanas
//
#ifndef _3ESROTATION_H
#define _3ESROTATION_H

#include "3es-core.h"

#include "3esmatrix3.h"
#include "3esmatrix4.h"
#include "3esquaternion.h"
#include "3esvector3.h"

namespace tes
{
  /// Multiplies a rotation matrix by a quaternion.
  /// The result is M = Aq
  /// @param a The matrix to multiply.
  /// @param q The quaternion.
  /// @return The result, Aq.
  template <typename T>
  Matrix3<T> operator * (const Matrix3<T> &a, const Quaternion<T> &q);
  template Matrix3<float> _3es_coreAPI operator * (const Matrix3<float> &a, const Quaternion<float> &q);
  template Matrix3<double> _3es_coreAPI operator * (const Matrix3<double> &a, const Quaternion<double> &q);

  /// Multiplies a quaternion by a rotation matrix resulting in a rotation matrix.
  /// The result is M = qB
  /// @param q The quaternion.
  /// @param b The matrix to multiply.
  /// @return The result, qB.
  template <typename T>
  Matrix3<T> operator * (const Quaternion<T> &q, const Matrix3<T> &b);
  template Matrix3<float> _3es_coreAPI operator * (const Quaternion<float> &q, const Matrix3<float> &b);
  template Matrix3<double> _3es_coreAPI operator * (const Quaternion<double> &q, const Matrix3<double> &b);

  /// Multiplies a transformation matrix by a quaternion.
  /// The result is M = Aq
  /// @param a The matrix to multiply.
  /// @param q The quaternion.
  /// @return The result, Aq.
  template <typename T>
  Matrix4<T> operator * (const Matrix4<T> &a, const Quaternion<T> &q);
  template Matrix4<float> _3es_coreAPI operator * (const Matrix4<float> &a, const Quaternion<float> &q);
  template Matrix4<double> _3es_coreAPI operator * (const Matrix4<double> &a, const Quaternion<double> &q);

  /// Builds a 4x4 transformation matrix from a quaternion rotation and vector translation.
  /// @param quaternion The quaternion rotation.
  /// @param translation The vector translation.
  template <typename T>
  Matrix4<T> quaternionTranslationToTransform(const Quaternion<T> &quaternion, const Vector3<T> &translation);
  template Matrix4<float> _3es_coreAPI quaternionTranslationToTransform(const Quaternion<float> &quaternion, const Vector3<float> &translation);
  template Matrix4<double> _3es_coreAPI quaternionTranslationToTransform(const Quaternion<double> &quaternion, const Vector3<double> &translation);

  template <typename T>
  Matrix4<T> prsTransform(const Vector3<T> &translation, const Quaternion<T> &quaternion, const Vector3<T> &scale);
  template Matrix4<float> _3es_coreAPI prsTransform(const Vector3<float> &translation, const Quaternion<float> &quaternion, const Vector3<float> &scale);
  template Matrix4<double> _3es_coreAPI prsTransform(const Vector3<double> &translation, const Quaternion<double> &quaternion, const Vector3<double> &scale);

  /// Decomposes a transformation matrix into a quaternion rotation and a vector translation.
  /// @param m The matrix to decompose.
  /// @param[out] q The quaternion rotation is written here.
  /// @param[out] translation The translation value is written here.
  /// @param[out] scale Optional pointer to receive the scale values the transform contains.
  template <typename T>
  void transformToQuaternionTranslation(const Matrix4<T> &m, Quaternion<T> &q, Vector3<T> &translation, Vector3<T> *scale = nullptr);
  template void _3es_coreAPI transformToQuaternionTranslation(const Matrix4<float> &m, Quaternion<float> &q, Vector3<float> &translation, Vector3<float> *scale);
  template void _3es_coreAPI transformToQuaternionTranslation(const Matrix4<double> &m, Quaternion<double> &q, Vector3<double> &translation, Vector3<double> *scale);

  /// @overload
  template <typename T>
  void transformToQuaternionTranslation(const Matrix4<T> &m, Quaternion<T> &q, Vector3<T> &translation, Vector3<T> &scale);
  template void _3es_coreAPI transformToQuaternionTranslation(const Matrix4<float> &m, Quaternion<float> &q, Vector3<float> &translation, Vector3<float> &scale);
  template void _3es_coreAPI transformToQuaternionTranslation(const Matrix4<double> &m, Quaternion<double> &q, Vector3<double> &translation, Vector3<double> &scale);

  /// Decomposes a transformation matrix into a quaternion rotation and a vector translation.
  /// @param m The matrix to decompose.
  /// @param[out] translation The translation value is written here.
  /// @return The quaternion extracted from the transformation matrix.
  template <typename T>
  inline Quaternion<T> transformToQuaternionTranslation(const Matrix4<T> &m, Vector3<T> &translation)
  {
    Quaternion<T> q;
    transformToQuaternionTranslation(m, q, translation);
    return q;
  }
  template Quaternion<float> _3es_coreAPI transformToQuaternionTranslation(const Matrix4<float> &m, Vector3<float> &translation);
  template Quaternion<double> _3es_coreAPI transformToQuaternionTranslation(const Matrix4<double> &m, Vector3<double> &translation);

  /// Extracts just the rotation part of a transformation matrix into a quaternion.
  /// @param m The matrix to decompose.
  /// @return The quaternion rotation.
  template <typename T>
  Quaternion<T> rotationToQuaternion(const Matrix3<T> &m);
  template Quaternion<float> _3es_coreAPI rotationToQuaternion(const Matrix3<float> &m);
  template Quaternion<double> _3es_coreAPI rotationToQuaternion(const Matrix3<double> &m);

  /// Extracts just the rotation part of a transformation matrix into a quaternion.
  /// @param m The matrix to decompose.
  /// @return The quaternion rotation.
  template <typename T>
  Quaternion<T> transformToQuaternion(const Matrix4<T> &m);
  template Quaternion<float> _3es_coreAPI transformToQuaternion(const Matrix4<float> &m);
  template Quaternion<double> _3es_coreAPI transformToQuaternion(const Matrix4<double> &m);

  /// Builds a rotation matrix from a quaternion rotation.
  /// @param q The quaternion rotation.
  /// @return The rotation matrix.
  template <typename T>
  Matrix3<T> quaternionToRotation(const Quaternion<T> &q);
  template Matrix3<float> _3es_coreAPI quaternionToRotation(const Quaternion<float> &m);
  template Matrix3<double> _3es_coreAPI quaternionToRotation(const Quaternion<double> &m);

  /// Builds a rotation transformation matrix from a quaternion rotation.
  /// @param q The quaternion rotation.
  /// @return The transformation rotation matrix.
  template <typename T>
  Matrix4<T> quaternionToTransform(const Quaternion<T> &q);
  template Matrix4<float> _3es_coreAPI quaternionToTransform(const Quaternion<float> &m);
  template Matrix4<double> _3es_coreAPI quaternionToTransform(const Quaternion<double> &m);

  //Matrix4<T> &orthoNormalise(Matrix4<T> &m);
}

#include "3esrotation.inl"

#endif  // _3ESROTATION_H
