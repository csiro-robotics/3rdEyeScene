//
// author: Kazys Stepanas
//
#ifndef _3ESVECTOR4_H_
#define _3ESVECTOR4_H_

#include "3es-core.h"

#include "3esvector3.h"

namespace tes
{
  template <typename T> class Vector4;

  /// Defines a single precision vector.
  typedef Vector4<float> Vector4f;
  /// Defines a double precision vector.
  typedef Vector4<double> Vector4d;

  /// Represents a vector in R4.
  template <typename T>
  class Vector4
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
      T v[4];
    };

    /// The default epsilon value used comparison operators.
    static const T Epsilon;

    /// A vector with all zero values.
    static const Vector4<T> zero;
    /// The vector (1, 1, 1, 1).
    static const Vector4<T> one;
    /// The vector (1, 0, 0, 0).
    static const Vector4<T> axisx;
    /// The vector (0, 1, 0, 0).
    static const Vector4<T> axisy;
    /// The vector (0, 0, 1, 0).
    static const Vector4<T> axisz;
    /// The vector (0, 0, 0, 1).
    static const Vector4<T> axisw;

    /// Default constructor: undefined initialisation behaviour.
    inline Vector4() {}
    /// Initialises all members to @p scalar.
    /// @param scalar The value for all members.
    inline Vector4(const T &scalar) : x(scalar), y(scalar), z(scalar), w(scalar) {}
    /// Copy constructor.
    /// @param other Vector to copy the value of.
    inline Vector4(const Vector4<T> &other) : x(other.x), y(other.y), z(other.z), w(other.w) {}
    /// Copy constructor from a Vector3.
    /// @param other Vector to copy the value of.
    /// @param w The w component value.
    inline Vector4(const Vector3<T> &other, const T &w) : x(other.x), y(other.y), z(other.z), w(w) {}
    /// Per coordinate initialisation.
    /// @param x The x coordinate.
    /// @param y The y coordinate.
    /// @param z The z coordinate.
    inline Vector4(const T &x, const T &y, const T &z, const T &w) : x(x), y(y), z(z), w(w) {}
    /// Initialisation from a array of at least length 4.
    /// No bounds checking is performed.
    /// @param array4 An array of at least length 4. Copies elements (0, 1, 2, 3).
    inline Vector4(const T *array4) : x(array4[0]), y(array4[1]), z(array4[2]), w(array4[3]) {}

    /// Copy constructor from a different numeric type.
    /// @param other Vector to copy the value of.
    template <typename Q>
    explicit inline Vector4(const Vector4<Q> &other) : x(other.x), y(other.y), z(other.z), w(other.w) {}

    /// Copy constructor from a Vector3 of a different numeric type.
    /// @param other Vector to copy the value of.
    /// @param w The w component value.
    template <typename Q>
    explicit inline Vector4(const Vector3<Q> &other, const T &w) : x(other.x), y(other.y), z(other.z), w(w) {}

    /// Index operator. Not bounds checked.
    /// @param index Access coordinates by index; 0 = x, 1 = y, 2 = z, 3 = w.
    /// @return The coordinate value.
    inline T &operator[](int index) { return v[index]; }
    /// @overload
    inline const T &operator[](int index) const { return v[index]; }

    /// Simple assignment operator.
    /// @param other Vector to copy the value of.
    /// @return This.
    inline Vector4<T> &operator=(const Vector4<T> &other) { x = other.x; y = other.y; z = other.z; w = other.w; return *this; }

    /// Simple assignment operator from a different numeric type.
    /// @param other Vector to copy the value of.
    /// @return This.
    template <typename Q>
    inline Vector4<T> &operator=(const Vector4<Q> &other) { x = T(other.x); y = T(other.y); z = T(other.z); w = T(other.w);  return *this; }

    /// Exact equality operator. Compares each component with the same operator.
    /// @param other The vector to compare to.
    /// @return True if this is exactly equal to @p other.
    bool operator==(const Vector4<T> &other) const;
    /// Exact inequality operator. Compares each component with the same operator.
    /// @param other The vector to compare to.
    /// @return True if this is not exactly equal to @p other.
    bool operator!=(const Vector4<T> &other) const;

    /// Unarary negation operator. Equivalent to calling @c negated().
    /// @return A negated copy of the vector.
    inline Vector4<T> operator-() const { return negated(); }

    /// Equality test with error. Defaults to using @c Epsilon.
    ///
    /// The vectors are considered equal if the distance between the vectors is
    /// less than @p epsilon.
    /// @param other The vector to compare to.
    /// @param epsilon The error tolerance.
    /// @return True this and @p other are equal with @p epsilon.
    bool isEqual(const Vector4<T> &other, const T &epsilon = Epsilon) const;

    /// Zero test with error. Defaults to using @c Epsilon.
    ///
    /// The vector is considered zero if the distance to zero
    /// less than @p epsilon.
    /// @param epsilon The error tolerance.
    /// @return True this within @p epsilon of zero.
    bool isZero(const T &epsilon = Epsilon) const;

    /// Negates all components of this vector.
    /// @return This.
    inline Vector4<T> &negate() { x = -x; y = -y; z = -y; w = -w;  return *this; }

    /// Returns a negated copy of this vector. This vector is unchanged.
    /// @return The negated value of this vector.
    inline Vector4<T> negated() const { return Vector4<T>(-x, -y, -z, -w); }

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
    Vector4<T> normalised(const T &epsilon = Epsilon) const;

    /// Adds @p other to this vector. Component-wise addition.
    /// @param other The operand.
    /// @return This vector after the operation.
    Vector4<T> &add(const Vector4<T> &other);

    /// Adds @p scalar to all components in this vector.
    /// @param scalar The scalar value to add.
    /// @return This vector after the operation.
    Vector4<T> &add(const T &scalar);

    /// Subtracts @p other from this vector (this - other). Component-wise subtraction.
    /// @param other The operand.
    /// @return This vector after the operation.
    Vector4<T> &subtract(const Vector4<T> &other);

    /// Subtracts @p scalar from all components in this vector.
    /// @param scalar The scalar value to subtract.
    /// @return This vector after the operation.
    Vector4<T> &subtract(const T &scalar);

    /// Multiplies all components in this vector by @p scalar.
    /// @param scalar The scalar value to multiply by.
    /// @return This vector after the operation.
    Vector4<T> &multiply(const T &scalar);

    /// An alias for @p multiply(const T &).
    /// @param scalar The scalar value to multiply by.
    /// @return This vector after the operation.
    inline Vector4<T> &scale(const T &scalar) { return multiply(scalar); }

    /// Divides all components in this vector by @p scalar.
    /// @param scalar The scalar value to divide by. Performs no operation if @p scalar is zero.
    /// @return This vector after the operation.
    Vector4<T> &divide(const T &scalar);

    /// Calculates the dot product of this.other.
    /// @return The dot product.
    T dot(const Vector4<T> &other) const;

    /// Calculates the dot as if using vectors in R3. That is, w is ignored.
    /// @return The dot product.
    T dot3(const Vector4<T> &other) const;

    /// Calculates the dot product with @p other in R3. W is set to 1.
    /// @return The cross product in R3.
    Vector4<T> cross3(const Vector4<T> &other) const;

    /// Calculates the magnitude of this vector.
    /// @return The magnitude.
    T magnitude() const;

    /// Calculates the magnitude squared of this vector.
    /// @return The magnitude squared.
    T magnitudeSquared() const;

    /// Arithmetic operator.
    inline Vector4<T> &operator+=(const Vector4 &other) { return add(other); }
    /// Arithmetic operator.
    inline Vector4<T> &operator+=(const T &scalar) { return add(scalar); }
    /// Arithmetic operator.
    inline Vector4<T> &operator-=(const Vector4 &other) { return subtract(other); }
    /// Arithmetic operator.
    inline Vector4<T> &operator-=(const T &scalar) { return subtract(scalar); }
    /// Arithmetic operator.
    inline Vector4<T> &operator*=(const T &scalar) { return multiply(scalar); }
    /// Arithmetic operator.
    inline Vector4<T> &operator/=(const T &scalar) { return divide(scalar); }

    /// Downcast this vector to a Vector3. W is lost.
    /// @return The x, y, z components.
    inline Vector3<T> xyz() const { return Vector3<T>(x, y, z); }
  };

  template class _3es_coreAPI Vector4<float>;
  template class _3es_coreAPI Vector4<double>;

  //---------------------------------------------------------------------------
  // Arithmetic operators
  //---------------------------------------------------------------------------

  /// Adds two vectors.
  template <class T>
  inline Vector4<T> operator+(const Vector4<T> &a, const Vector4<T> &b)
  {
    Vector4<T> v(a);
    v.add(b);
    return v;
  }

  /// Adds two vectors.
  template <class T>
  inline Vector4<T> operator+(const Vector4<T> &a, const T &b)
  {
    Vector4<T> v(a);
    v.add(b);
    return v;
  }

  /// Adds two vectors.
  template <class T>
  inline Vector4<T> operator+(const T &a, const Vector4<T> &b) { return b * a; }

  /// Sutracts @p b from @p a.
  template <class T>
  inline Vector4<T> operator-(const Vector4<T> &a, const Vector4<T> &b)
  {
    Vector4<T> v(a);
    v.subtract(b);
    return v;
  }

  /// Adds two vectors.
  template <class T>
  inline Vector4<T> operator-(const Vector4<T> &a, const T &b)
  {
    Vector4<T> v(a);
    v.subtract(b);
    return v;
  }

  /// Multiplies a vector by a scalar.
  template <class T>
  inline Vector4<T> operator*(const Vector4<T> &a, const T &b)
  {
    Vector4<T> v(a);
    v.multiply(b);
    return v;
  }

  /// Multiplies a vector by a scalar.
  template <class T>
  inline Vector4<T> operator*(const T &a, const Vector4<T> &b) { return b * a; }

  /// Divides a vector by a scalar.
  template <class T>
  inline Vector4<T> operator/(const Vector4<T> &a, const T &b)
  {
    Vector4<T> v(a);
    v.divide(b);
    return v;
  }


  template <typename T>
  inline bool Vector4<T>::operator==(const Vector4<T> &other) const
  {
    return x == other.x && y == other.y && z == other.z && w == other.w;
  }


  template <typename T>
  inline bool Vector4<T>::operator!=(const Vector4<T> &other) const
  {
    return x != other.x || y != other.y || z != other.z || w != other.w;
  }


  template <typename T>
  inline bool Vector4<T>::isEqual(const Vector4<T> &other, const T &epsilon) const
  {
    const T distanceSquared = std::abs((*this - other).magnitudeSquared());
    return distanceSquared <= epsilon * epsilon;
  }


  template <typename T>
  inline bool Vector4<T>::isZero(const T &epsilon) const
  {
    return isEqual(zero, epsilon);
  }


  template <typename T>
  inline T Vector4<T>::normalise(const T &epsilon)
  {
    T mag = magnitude();
    if (mag > epsilon)
    {
      divide(mag);
    }
    return mag;
  }


  template <typename T>
  inline Vector4<T> Vector4<T>::normalised(const T &epsilon) const
  {
    T mag = magnitude();
    if (mag > epsilon)
    {
      Vector4<T> v(*this);
      v.divide(mag);
      return v;
    }
    return zero;
  }


  template <typename T>
  inline Vector4<T> &Vector4<T>::add(const Vector4<T> &other)
  {
    x += other.x;
    y += other.y;
    z += other.z;
    w += other.w;
    return *this;
  }


  template <typename T>
  inline Vector4<T> &Vector4<T>::add(const T &scalar)
  {
    x += scalar;
    y += scalar;
    z += scalar;
    w += scalar;
    return *this;
  }


  template <typename T>
  inline Vector4<T> &Vector4<T>::subtract(const Vector4<T> &other)
  {
    x -= other.x;
    y -= other.y;
    z -= other.z;
    w -= other.w;
    return *this;
  }


  template <typename T>
  inline Vector4<T> &Vector4<T>::subtract(const T &scalar)
  {
    x -= scalar;
    y -= scalar;
    z -= scalar;
    w -= scalar;
    return *this;
  }


  template <typename T>
  Vector4<T> &Vector4<T>::multiply(const T &scalar)
  {
    x *= scalar;
    y *= scalar;
    z *= scalar;
    w *= scalar;
    return *this;
  }


  template <typename T>
  Vector4<T> &Vector4<T>::divide(const T &scalar)
  {
    const T div = T(1) / scalar;
    x *= div;
    y *= div;
    z *= div;
    w *= div;
    return *this;
  }


  template <typename T>
  T Vector4<T>::dot(const Vector4<T> &other) const
  {
    return x * other.x + y * other.y + z * other.z + w * other.w;
  }


  template <typename T>
  T Vector4<T>::dot3(const Vector4<T> &other) const
  {
    return x * other.x + y * other.y + z * other.z + w * other.w;
  }


  template <typename T>
  Vector4<T> Vector4<T>::cross3(const Vector4<T> &other) const
  {
    Vector4<T> v;
    v.x = y * other.z - z * other.y;
    v.y = z * other.x - x * other.z;
    v.z = x * other.y - y * other.x;
    v.w = T(1);
    return v;
  }


  template <typename T>
  T Vector4<T>::magnitude() const
  {
    T mag = magnitudeSquared();
    mag = std::sqrt(mag);
    return mag;
  }


  template <typename T>
  T Vector4<T>::magnitudeSquared() const
  {
    return dot(*this);
  }


  template <typename T> const T Vector4<T>::Epsilon = T(1e-6);
  template <typename T> const Vector4<T> Vector4<T>::zero(T(0));
  template <typename T> const Vector4<T> Vector4<T>::one(T(1));
  template <typename T> const Vector4<T> Vector4<T>::axisx(1, 0, 0, 0);
  template <typename T> const Vector4<T> Vector4<T>::axisy(0, 1, 0, 0);
  template <typename T> const Vector4<T> Vector4<T>::axisz(0, 0, 1, 0);
  template <typename T> const Vector4<T> Vector4<T>::axisw(0, 0, 0, 1);
}

#endif // _3ESVECTOR4_H_
