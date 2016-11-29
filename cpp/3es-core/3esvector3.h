//
// author: Kazys Stepanas
//
#ifndef _3ESVECTOR3_H_
#define _3ESVECTOR3_H_

#include "3es-core.h"

#include <cstdlib>
#include <cmath>

namespace tes
{
  template <typename T> class Vector3;
  /// Defines a single precision vector.
  typedef Vector3<float> Vector3f;
  /// Defines a double precision vector.
  typedef Vector3<double> Vector3d;

  /// Represents a vector in R3.
  template <typename T>
  class Vector3
  {
  public:
    union
    {
      struct
      {
        /// Direct data member access.
        T x, y, z;
      };
      /// Array representation of the vector members.
      T v[3];
    };

    /// The default epsilon value used comparison operators.
    static const T Epsilon;

    /// A vector with all zero values.
    static const Vector3<T> zero;
    /// The vector (1, 1, 1).
    static const Vector3<T> one;
    /// The vector (1, 0, 0).
    static const Vector3<T> axisx;
    /// The vector (0, 1, 0).
    static const Vector3<T> axisy;
    /// The vector (0, 0, 1).
    static const Vector3<T> axisz;

    /// Default constructor: undefined initialisation behaviour.
    inline Vector3() {}
    /// Initialises all members to @p scalar.
    /// @param scalar The value for all members.
    inline Vector3(const T &scalar) : x(scalar), y(scalar), z(scalar) {}
    /// Copy constructor.
    /// @param other Vector to copy the value of.
    inline Vector3(const Vector3<T> &other) : x(other.x), y(other.y), z(other.z) {}
    /// Per coordinate initialisation.
    /// @param x The x coordinate.
    /// @param y The y coordinate.
    /// @param z The z coordinate.
    inline Vector3(const T &x, const T &y, const T &z) : x(x), y(y), z(z) {}
    /// Initialisation from a array of at least length 3.
    /// No bounds checking is performed.
    /// @param array3 An array of at least length 3. Copies elements (0, 1, 2).
    inline Vector3(const T *array3) : x(array3[0]), y(array3[1]), z(array3[2]) {}

    /// Copy constructor from a different numeric type.
    /// @param other Vector to copy the value of.
    template <typename Q>
    explicit inline Vector3(const Vector3<Q> &other) : x(T(other.x)), y(T(other.y)), z(T(other.z)) {}

    /// Index operator. Not bounds checked.
    /// @param index Access coordinates by index; 0 = x, 1 = y, 2 = z.
    /// @return The coordinate value.
    inline T &operator[](int index) { return v[index]; }
    /// @overload
    inline const T &operator[](int index) const { return v[index]; }

    /// Index operator. Not bounds checked.
    /// @param index Access coordinates by index; 0 = x, 1 = y, 2 = z.
    /// @return The coordinate value.
    inline T &operator[](unsigned index) { return v[index]; }
    /// @overload
    inline const T &operator[](unsigned index) const { return v[index]; }

    /// Simple assignment operator.
    /// @param other Vector to copy the value of.
    /// @return This.
    inline Vector3<T> &operator=(const Vector3<T> &other) { x = other.x; y = other.y; z = other.z; return *this; }

    /// Simple assignment operator from a different numeric type.
    /// @param other Vector to copy the value of.
    /// @return This.
    template <typename Q>
    inline Vector3<T> &operator=(const Vector3<Q> &other) { x = T(other.x); y = T(other.y); z = T(other.z); return *this; }

    /// Exact equality operator. Compares each component with the same operator.
    /// @param other The vector to compare to.
    /// @return True if this is exactly equal to @p other.
    bool operator==(const Vector3<T> &other) const;
    /// Exact inequality operator. Compares each component with the same operator.
    /// @param other The vector to compare to.
    /// @return True if this is not exactly equal to @p other.
    bool operator!=(const Vector3<T> &other) const;

    /// Unarary negation operator. Equivalent to calling @c negated().
    /// @return A negated copy of the vector.
    inline Vector3<T> operator-() const { return negated(); }

    /// Equality test with error. Defaults to using @c Epsilon.
    ///
    /// The vectors are considered equal if the distance between the vectors is
    /// less than @p epsilon.
    /// @param other The vector to compare to.
    /// @param epsilon The error tolerance.
    /// @return True this and @p other are equal with @p epsilon.
    bool isEqual(const Vector3<T> &other, const T &epsilon = Epsilon) const;

    /// Zero test with error. Defaults to using @c Epsilon.
    ///
    /// The vector is considered zero if the distance to zero
    /// less than @p epsilon.
    /// @param epsilon The error tolerance.
    /// @return True this within @p epsilon of zero.
    bool isZero(const T &epsilon = Epsilon) const;

    /// Negates all components of this vector.
    /// @return This.
    inline Vector3<T> &negate() { x = -x; y = -y; z = -y; return *this; }

    /// Returns a negated copy of this vector. This vector is unchanged.
    /// @return The negated value of this vector.
    inline Vector3<T> negated() const { return Vector3<T>(-x, -y, -z); }

    /// Attempts to normalise this vector.
    ///
    /// Normalisation fails if the length of this vector is less than or
    /// equal to @p epsilon. In this case, the vector remains unchanged.
    ///
    /// @return The length of this vector before normalisation or
    /// zero if normalisation failed.
    T normalise(const T &epsilon = Epsilon);

    /// Returns a normalised copy of this vector.
    ///
    /// Normalisation fails if the length of this vector is less than or
    /// equal to @p epsilon.
    ///
    /// @return A normalised copy of this vector, or a zero vector if
    /// if normalisation failed.
    Vector3<T> normalised(const T &epsilon = Epsilon) const;

    /// Adds @p other to this vector. Component-wise addition.
    /// @param other The operand.
    /// @return This vector after the operation.
    Vector3<T> &add(const Vector3<T> &other);

    /// Adds @p scalar to all components in this vector.
    /// @param scalar The scalar value to add.
    /// @return This vector after the operation.
    Vector3<T> &add(const T &scalar);

    /// Subtracts @p other from this vector (this - other). Component-wise subtraction.
    /// @param other The operand.
    /// @return This vector after the operation.
    Vector3<T> &subtract(const Vector3<T> &other);

    /// Subtracts @p scalar from all components in this vector.
    /// @param scalar The scalar value to subtract.
    /// @return This vector after the operation.
    Vector3<T> &subtract(const T &scalar);

    /// Multiplies all components in this vector by @p scalar.
    /// @param scalar The scalar value to multiply by.
    /// @return This vector after the operation.
    Vector3<T> &multiply(const T &scalar);

    /// An alias for @p multiply(const T &).
    /// @param scalar The scalar value to multiply by.
    /// @return This vector after the operation.
    inline Vector3<T> &scale(const T &scalar) { return multiply(scalar); }

    /// Divides all components in this vector by @p scalar.
    /// @param scalar The scalar value to divide by. Performs no operation if @p scalar is zero.
    /// @return This vector after the operation.
    Vector3<T> &divide(const T &scalar);

    /// Calculates the dot product of this.other.
    /// @return The dot product.
    T dot(const Vector3<T> &other) const;

    /// Calculates the cross product of this x other.
    /// @return The cross product vector.
    Vector3<T> cross(const Vector3<T> &other) const;

    /// Calculates the magnitude of this vector.
    /// @return The magnitude.
    T magnitude() const;

    /// Calculates the magnitude squared of this vector.
    /// @return The magnitude squared.
    T magnitudeSquared() const;

    /// Arithmetic operator.
    inline Vector3<T> &operator+=(const Vector3 &other) { return add(other); }
    /// Arithmetic operator.
    inline Vector3<T> &operator+=(const T &scalar) { return add(scalar); }
    /// Arithmetic operator.
    inline Vector3<T> &operator-=(const Vector3 &other) { return subtract(other); }
    /// Arithmetic operator.
    inline Vector3<T> &operator-=(const T &scalar) { return subtract(scalar); }
    /// Arithmetic operator.
    inline Vector3<T> &operator*=(const T &scalar) { return multiply(scalar); }
    /// Arithmetic operator.
    inline Vector3<T> &operator/=(const T &scalar) { return divide(scalar); }

    // Swizzle operations.

    /// Return a copy of this vector. Provided for swizzle completeness.
    inline Vector3<T> xyz() const { return Vector3<T>(*this); }
    /// Return a copy of this vector. Provided for swizzle completeness.
    inline Vector3<T> xzy() const { return Vector3<T>(x, z, y); }
    /// Swizzle operation.
    inline Vector3<T> yzx() const { return Vector3<T>(y, z, x); }
    /// Swizzle operation.
    inline Vector3<T> yxz() const { return Vector3<T>(y, x, z); }
    /// Swizzle operation.
    inline Vector3<T> zxy() const { return Vector3<T>(z, x, y); }
    /// Swizzle operation.
    inline Vector3<T> zyx() const { return Vector3<T>(x, y, x); }
  };

  template class _3es_coreAPI Vector3<float>;
  template class _3es_coreAPI Vector3<double>;


  //---------------------------------------------------------------------------
  // Arithmetic operators
  //---------------------------------------------------------------------------

  /// Adds two vectors.
  template <class T>
  inline Vector3<T> operator+(const Vector3<T> &a, const Vector3<T> &b)
  {
    Vector3<T> v(a);
    v.add(b);
    return v;
  }

  /// Adds two vectors.
  template <class T>
  inline Vector3<T> operator+(const Vector3<T> &a, const T &b)
  {
    Vector3<T> v(a);
    v.add(b);
    return v;
  }

  /// Adds two vectors.
  template <class T>
  inline Vector3<T> operator+(const T &a, const Vector3<T> &b) { return b * a; }

  /// Sutracts @p b from @p a.
  template <class T>
  inline Vector3<T> operator-(const Vector3<T> &a, const Vector3<T> &b)
  {
    Vector3<T> v(a);
    v.subtract(b);
    return v;
  }

  /// Adds two vectors.
  template <class T>
  inline Vector3<T> operator-(const Vector3<T> &a, const T &b)
  {
    Vector3<T> v(a);
    v.subtract(b);
    return v;
  }

  /// Multiplies a vector by a scalar.
  template <class T>
  inline Vector3<T> operator*(const Vector3<T> &a, const T &b)
  {
    Vector3<T> v(a);
    v.multiply(b);
    return v;
  }

  /// Multiplies a vector by a scalar.
  template <class T>
  inline Vector3<T> operator*(const T &a, const Vector3<T> &b) { return b * a; }

  /// Divides a vector by a scalar.
  template <class T>
  inline Vector3<T> operator/(const Vector3<T> &a, const T &b)
  {
    Vector3<T> v(a);
    v.divide(b);
    return v;
  }


  template <typename T>
  inline bool Vector3<T>::operator==(const Vector3<T> &other) const
  {
    return x == other.x && y == other.y && z == other.z;
  }


  template <typename T>
  inline bool Vector3<T>::operator!=(const Vector3<T> &other) const
  {
    return x != other.x || y != other.y || z != other.z;
  }


  template <typename T>
  inline bool Vector3<T>::isEqual(const Vector3<T> &other, const T &epsilon) const
  {
    const T distanceSquared = std::abs((*this - other).magnitudeSquared());
    return distanceSquared <= epsilon * epsilon;
  }


  template <typename T>
  inline bool Vector3<T>::isZero(const T &epsilon) const
  {
    return isEqual(zero, epsilon);
  }


  template <typename T>
  inline T Vector3<T>::normalise(const T &epsilon)
  {
    T mag = magnitude();
    if (mag > epsilon)
    {
      divide(mag);
    }
    return mag;
  }


  template <typename T>
  inline Vector3<T> Vector3<T>::normalised(const T &epsilon) const
  {
    T mag = magnitude();
    if (mag > epsilon)
    {
      Vector3<T> v(*this);
      v.divide(mag);
      return v;
    }
    return zero;
  }


  template <typename T>
  inline Vector3<T> &Vector3<T>::add(const Vector3<T> &other)
  {
    x += other.x;
    y += other.y;
    z += other.z;
    return *this;
  }


  template <typename T>
  inline Vector3<T> &Vector3<T>::add(const T &scalar)
  {
    x += scalar;
    y += scalar;
    z += scalar;
    return *this;
  }


  template <typename T>
  inline Vector3<T> &Vector3<T>::subtract(const Vector3<T> &other)
  {
    x -= other.x;
    y -= other.y;
    z -= other.z;
    return *this;
  }


  template <typename T>
  inline Vector3<T> &Vector3<T>::subtract(const T &scalar)
  {
    x -= scalar;
    y -= scalar;
    z -= scalar;
    return *this;
  }


  template <typename T>
  inline Vector3<T> &Vector3<T>::multiply(const T &scalar)
  {
    x *= scalar;
    y *= scalar;
    z *= scalar;
    return *this;
  }


  template <typename T>
  inline Vector3<T> &Vector3<T>::divide(const T &scalar)
  {
    const T div = T(1) / scalar;
    x *= div;
    y *= div;
    z *= div;
    return *this;
  }


  template <typename T>
  inline T Vector3<T>::dot(const Vector3<T> &other) const
  {
    return x * other.x + y * other.y + z * other.z;
  }


  template <typename T>
  inline Vector3<T> Vector3<T>::cross(const Vector3<T> &other) const
  {
    Vector3<T> v;
    v.x = y * other.z - z * other.y;
    v.y = z * other.x - x * other.z;
    v.z = x * other.y - y * other.x;
    return v;
  }


  template <typename T>
  inline T Vector3<T>::magnitude() const
  {
    T mag = magnitudeSquared();
    mag = std::sqrt(mag);
    return mag;
  }


  template <typename T>
  inline T Vector3<T>::magnitudeSquared() const
  {
    return dot(*this);
  }


  template <typename T> const T Vector3<T>::Epsilon = T(1e-6);
  template <typename T> const Vector3<T> Vector3<T>::zero(T(0));
  template <typename T> const Vector3<T> Vector3<T>::one(T(1));
  template <typename T> const Vector3<T> Vector3<T>::axisx(1, 0, 0);
  template <typename T> const Vector3<T> Vector3<T>::axisy(0, 1, 0);
  template <typename T> const Vector3<T> Vector3<T>::axisz(0, 0, 1);
}

#endif // _3ESVECTOR3_H_
