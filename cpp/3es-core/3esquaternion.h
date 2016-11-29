//
// author: Kazys Stepanas
//
#ifndef _3ESQUATERNION_H_
#define _3ESQUATERNION_H_

#include "3es-core.h"

#include "3esvector3.h"

namespace tes
{
  template <typename T>
  class Quaternion
  {
  public:
    union
    {
      struct
      {
        /// Direct data member access.
        T x, y, z, w;
      };
      /// Array representation of the vector members.
      T q[4];
    };

    /// The identity quaternion (0, 0, 0, 1).
    static const Quaternion<T> identity;

    /// Default constructor: undefined initialisation behaviour.
    inline Quaternion() {}

    /// The identity constructor. The boolean parameter is ignored, being
    /// used only for overloading. General usage is to set it to true.
    inline Quaternion(bool) { *this = identity; }

    /// Copy constructor.
    /// @param other Vector to copy the value of.
    inline Quaternion(const Quaternion<T> &other) : x(other.x), y(other.y), z(other.z), w(other.w) {}
    /// Copy constructor from a different numeric type.
    /// @param other Vector to copy the value of.
    template <typename Q>
    inline Quaternion(const Quaternion<Q> &other) : x(T(other.x)), y(T(other.y)), z(T(other.z)), w(T(other.w)) {}
    /// Per coordinate initialisation.
    /// @param x The x coordinate.
    /// @param y The y coordinate.
    /// @param z The z coordinate.
    /// @param w The w coordinate.
    inline Quaternion(const T &x, const T &y, const T &z, const T &w) : x(x), y(y), z(z), w(w) {}
    /// Vector plus scalar initialisation.
    /// @param v Values for x, y and z.
    /// @param w The w coordinate.
    inline Quaternion(const Vector3<T> &v, const T &w) : x(v.x), y(v.y), z(v.z), w(w) {}
    /// Initialisation from a array of at least length 4.
    /// No bounds checking is performed.
    /// @param array4 An array of at least length 4. Copies elements (0, 1, 2, 3).
    inline Quaternion(const T *array4) : x(array4[0]), y(array4[1]), z(array4[2]), w(array4[3]) {}

    /// Create the quaternion rotation transforming @p from => @p to.
    /// @param from Starting vector.
    /// @param to Target vector.
    Quaternion(const Vector3<T> &from, const Vector3<T> &to);

    /// Index operator. Not bounds checked.
    /// @param index Access coordinates by index; 0 = x, 1 = y, 2 = z.
    /// @return The coordinate value.
    inline T &operator[](int index) { return q[index]; }
    /// @overload
    inline const T &operator[](int index) const { return q[index]; }

    /// Simple assignment operator.
    /// @param other Vector to copy the value of.
    /// @return This.
    inline Quaternion<T> &operator=(const Quaternion<T> &other) { x = other.x; y = other.y; z = other.z; w = other.w; return *this; }

    /// Simple assignment operator from a different numeric type.
    /// @param other Vector to copy the value of.
    /// @return This.
    template <typename Q>
    inline Quaternion<T> &operator=(const Quaternion<Q> &other) { x = T(other.x); y = T(other.y); z = T(other.z); w = T(other.w); return *this; }

    /// Exact equality operator. Compares each component with the same operator.
    /// @param other The vector to compare to.
    /// @return True if this is exactly equal to @p other.
    bool operator==(const Quaternion<T> &other) const;
    /// Exact inequality operator. Compares each component with the same operator.
    /// @param other The vector to compare to.
    /// @return True if this is not exactly equal to @p other.
    bool operator!=(const Quaternion<T> &other) const;

    /// Equality test with error. Defaults to using @c Epsilon.
    ///
    /// The vectors are considered equal of each individual component is
    /// within @c +/- @p epsilon of it's corresponding component in @p other.
    /// @param other The vector to compare to.
    /// @param epsilon The error tolerance.
    /// @return True this and @p other are equal with @p epsilon.
    bool isEqual(const Quaternion<T> &other, const T &epsilon = Vector3<T>::Epsilon);

    /// Checks if this quaternion is exactly identity.
    /// @return True if this is exactly the identity quaternion.
    bool isIdentity();

    /// Converts this quaternion into a axis of rotation and the rotation angle around that axis (radians).
    /// @param[out] axis Set to the axis of rotation. Set to (0, 0, 1) if
    ///   this quaternion is identity or near zero length.
    /// @param[out] angle Set to the rotation angle (radians). Zero if this
    ///   quaternion is identity.
    void getAxisAngle(Vector3<T> &axis, T &angle) const;

    /// Sets this quaternion from an axis of rotation and the angle of rotation about that axis (radians).
    /// @param axis The axis of rotation.
    /// @param angle The rotation angle (radians).
    Quaternion<T> &setAxisAngle(const Vector3<T> &axis, const T &angle);

    /// Inverts this quaternion, making the counter rotation.
    /// @return This after inversion.
    Quaternion<T> &invert();

    /// Calculates and returns the inverse of this quaternion.
    /// @return The inverse of this quaternion.
    Quaternion<T> inverse() const;

    /// Sets this quaternion to its conjugate.
    /// The conjugate is the same quaternion with x, y, z values negated.
    /// @return This after the conjugate calculation.
    Quaternion<T> &conjugate();

    /// Calculates and returns the conjugate of this quaternion.
    /// The conjugate is the same quaternion with x, y, z values negated.
    /// @return This quaternion's conjugate.
    Quaternion<T> conjugated() const;

    /// Attempts to normalise this quaternion.
    ///
    /// Normalisation fails if the length of this quaternion is less than or
    /// equal to @p epsilon. In this case, the quaternion becomes identity.
    ///
    /// @return The magnitude of this quaternion before normalisation or
    /// zero if normalisation failed.
    T normalise(const T &epsilon = Vector3<T>::Epsilon);

    /// Returns a normalised copy of this quaternion.
    ///
    /// Normalisation fails if the length of this quaternion is less than or
    /// equal to @p epsilon.
    ///
    /// @return A normalised copy of this quaternion, or a zero quaternion if
    /// if normalisation failed.
    Quaternion<T> normalised(const T &epsilon = Vector3<T>::Epsilon) const;

    /// Returns the magnitude of this quaternion.
    T magnitude() const;

    /// Returns the magnitude squared of this quaternion.
    T magnitudeSquared() const;

    /// Calculates the dot product of @c this and @p other.
    T dot(const Quaternion<T> other) const;

    /// Transforms @p v by this quaternion rotation.
    /// @return The transformed vector.
    Vector3<T> transform(const Vector3<T> &v) const;

    /// Multiply all components of this quaternion by a scalar.
    /// @return This after the multiplication.
    Quaternion<T> &multiply(const T &scalar);

    /// Performs a spherical linear interpolation of one quaternion to another.
    /// @param from The quaternion rotation to interpolate from.
    /// @param to The quaternion rotation to interpolate to.
    /// @param t The interpolation "time", [0, 1].
    /// @return The interpolated result.
    static Quaternion<T> slerp(const Quaternion<T> &from, const Quaternion<T> &to, const T &t);

    Quaternion<T> operator *= (const Quaternion<T> &other);
    inline Quaternion<T> operator *= (const T &scalar) { return multiply(scalar); }
  };


  /// Multiplies one quaternion by another. This gives the combined rotation of @p b then @p a.
  /// @param a A quaternion operand.
  /// @param b A quaternion operand.
  /// @return The rotation of @p b by @p a.
  template <typename T>
  Quaternion<T> operator * (const Quaternion<T> &a, const Quaternion<T> &b);

  /// Transforms a vector by a quaternion, rotating it.
  /// @param a The quaternion rotation.
  /// @param v The vector to rotate.
  /// @return The rotation of @p v by @p a.
  template <typename T>
  Vector3<T> operator * (const Quaternion<T> &a, const Vector3<T> &v);

  /// Defines a single precision quaternion.
  typedef Quaternion<float> Quaternionf;
  /// Defines a double precision quaternion.
  typedef Quaternion<double> Quaterniond;

  template class _3es_coreAPI Quaternion<float>;
  template class _3es_coreAPI Quaternion<double>;
}

#include "3esquaternion.inl"

#endif // _3ESQUATERNION_H_
