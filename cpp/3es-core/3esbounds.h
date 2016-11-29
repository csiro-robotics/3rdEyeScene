//
// author: Kazys Stepanas
//
#ifndef _3ESBOUNDS_H
#define _3ESBOUNDS_H

#include "3es-core.h"

#include "3esvector3.h"

#include <limits>

namespace tes
{
  /// A simple bounding box structure.
  template <typename T>
  class Bounds
  {
  public:
    /// Initialises bounds where max < min at using the limits of the type @c T.
    Bounds();

    /// Copy constructor.
    /// @param other The bounds to copy.
    Bounds(const Bounds<T> &other);

    /// Copy constructor from a different numeric type.
    /// The type @c Q must be compatible with @c T. Generally used to convert between
    /// single and double precision.
    /// @param other The bounds to copy.
    template <typename Q>
    Bounds(const Bounds<Q> &other);

    /// Initialise a bounding box with the given extents.
    /// @param minExt The bounding box minimum. All components must be less than or equal to
    ///     @param maxExtents.
    /// @param maxExt The bounding box maximum. All components must be greater than or equal to
    ///     @param minExtents.
    Bounds(const Vector3<T> &minExt, const Vector3<T> maxExt);

    /// Access the minimum extents.
    /// @return The minimal corder of the bounding box.
    const Vector3<T> &minimum() const;
    /// Access the maximum extents.
    /// @return The maximal corder of the bounding box.
    const Vector3<T> &maximum() const;

    /// Expand the bounding box to include @p point.
    /// @param point The point to include.
    void expand(const Vector3<T> &point);

    /// Expand the bounding box to include @p other.
    /// @param point The point to include.
    void expand(const Bounds<T> &other);

    /// Returns true if the bounds are valid, with minimum extents less than or equal to the
    /// maximum.
    /// @return True when valid.
    bool isValid() const;

    /// Precise equality operator.
    /// @param other The object to compare to.
    /// @return True if this is precisely equal to @p other.
    bool operator==(const Bounds<T> &other) const;

    /// Precise inequality operator.
    /// @param other The object to compare to.
    /// @return True if this is no precisely equal to @p other.
    bool operator!=(const Bounds<T> &other) const;

    /// Assignment operator.
    /// @param other The bounds to copy.
    /// @return @c this.
    Bounds<T> &operator = (const Bounds<T> &other);

  private:
    Vector3<T> _minimum;  ///< Minimum extents.
    Vector3<T> _maximum;  ///< Maximum extents.
  };

  /// Single precision bounds.
  template class _3es_coreAPI Bounds<float>;
  /// Double precision bounds.
  template class _3es_coreAPI Bounds<double>;
  typedef Bounds<float> Boundsf;
  typedef Bounds<double> Boundsd;

  template <typename T>
  inline Bounds<T>::Bounds()
    : _minimum( std::numeric_limits<T>::max())
    , _maximum(-std::numeric_limits<T>::max())
  {
  }


  template <typename T>
  inline Bounds<T>::Bounds(const Bounds<T> &other)
  {
    *this = other;
  }


  template <typename T>
  template <typename Q>
  inline Bounds<T>::Bounds(const Bounds<Q> &other)
  {
    _minimum = Vector3<T>(other._minimum);
    _maximum = Vector3<T>(other._maximum);
  }


  template <typename T>
  inline Bounds<T>::Bounds(const Vector3<T> &minExt, const Vector3<T> maxExt)
    : _minimum(minExt)
    , _maximum(maxExt)
  {
  }


  template <typename T>
  inline const Vector3<T> &Bounds<T>::minimum() const
  {
    return _minimum;
  }


  template <typename T>
  inline const Vector3<T> &Bounds<T>::maximum() const
  {
    return _maximum;
  }


  template <typename T>
  inline void Bounds<T>::expand(const Vector3<T> &point)
  {
    _minimum.x = (point.x < _minimum.x) ? point.x : _minimum.x;
    _minimum.y = (point.y < _minimum.y) ? point.y : _minimum.y;
    _minimum.z = (point.z < _minimum.z) ? point.z : _minimum.z;
    _maximum.x = (point.x > _maximum.x) ? point.x : _maximum.x;
    _maximum.y = (point.y > _maximum.y) ? point.y : _maximum.y;
    _maximum.z = (point.z > _maximum.z) ? point.z : _maximum.z;
  }


  template <typename T>
  inline void Bounds<T>::expand(const Bounds<T> &other)
  {
    expand(other.minimum());
    expand(other.maximum());
  }


  template <typename T>
  inline bool Bounds<T>::isValid() const
  {
    return _minimum.x <= _maximum.x && _minimum.y <= _maximum.y && _minimum.z <= _maximum.z;
  }


  template <typename T>
  inline bool Bounds<T>::operator==(const Bounds<T> &other) const
  {
    return _minimum.x == other._minimum.x && _minimum.y == other._minimum.y && _minimum.z == other._minimum.z &&
           _maximum.x == other._maximum.x && _maximum.y == other._maximum.y && _maximum.z == other._maximum.z;
  }


  template <typename T>
  inline bool Bounds<T>::operator!=(const Bounds<T> &other) const
  {
    return !operator==(other);
  }


  template <typename T>
  inline Bounds<T> &Bounds<T>::operator = (const Bounds<T> &other)
  {
    _minimum = other._minimum;
    _maximum = other._maximum;
    return *this;
  }
}


#endif  // _3ESBOUNDS_H
