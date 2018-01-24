//
// author: Kazys Stepanas
//
#ifndef _3ESARROW_H_
#define _3ESARROW_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  /// Defines an arrow shape to display.
  ///
  /// An arrow is defined by:
  /// Component      | Description
  /// -------------- | -----------------------------------------------------------------------------------------------
  /// @c origin()    | The arrow base position. Alias for @p position().
  /// @c direction() | The arrow direction vector. Must be unit length.
  /// @c length()    | Length of the arrow from base to tip.
  /// @c radius()    | Radius of the arrow body. The arrow head will be slightly larger.
  class _3es_coreAPI Arrow : public Shape
  {
  public:
    /// Default direction used as a reference orientation for packing the rotation.
    ///
    /// The @c rotation() value is relative to this vector.
    ///
    /// The default is <tt>(0, 0, 1)</tt>
    static const Vector3f DefaultDirection;

    /// @overload
    Arrow(uint32_t id = 0u, const V3Arg &origin = V3Arg(0, 0, 0), const V3Arg &dir = DefaultDirection, float length = 1.0f, float radius = 0.025f);
    /// Construct an arrow object.
    /// @param id The shape ID, unique among @c Arrow objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param origin The start point for the array.
    /// @param dir The direction vector of the arrow.
    /// @param length The arrow length.
    /// @param radius Radius of the arrow body.
    Arrow(uint32_t id, uint16_t category, const V3Arg &origin = V3Arg(0, 0, 0), const V3Arg &dir = V3Arg(0, 0, 1), float length = 1.0f, float radius = 0.025f);

    inline const char *type() const override { return "arrow"; }

    /// Set the arrow radius.
    /// @param radius The new arrow radius.
    /// @return @c *this
    Arrow &setRadius(float radius);
    /// Get the arrow radius. Defines the shaft radius, while the head flanges to a sightly larger radius.
    /// @return The arrow body radius.
    float radius() const;

    /// Set the arrow length from base to tip.
    /// @param length Set the length to set.
    /// @return @c *this
    Arrow &setLength(float length);
    /// Get the arrow length from base to tip.
    /// @return The arrow length.
    float length() const;

    /// Set the arrow origin. This is the arrow base position.
    ///
    /// Note: this aliases @p setPosition().
    ///
    /// @param origin The arrow base position.
    /// @return @c *this
    Arrow &setOrigin(const V3Arg &origin);

    /// Get the arrow base position.
    ///
    /// Note: this aliases @c position().
    /// @return The arrow base position.
    Vector3f origin() const;

    /// Set the arrow direction vector.
    /// @param direction The direction vector to set. Must be unit length.
    /// @return @c *this
    Arrow &setDirection(const V3Arg &direction);
    /// Get the arrow direction vector.
    ///
    /// May not exactly match the axis given via @p setDirection() as the direction is defined by the quaternion
    /// @c rotation().
    /// @return The arrow direction vector.
    Vector3f direction() const;
  };


  inline Arrow::Arrow(uint32_t id, const V3Arg &origin, const V3Arg &dir, float length, float radius)
    : Shape(SIdArrow, id)
  {
    setPosition(origin);
    setDirection(dir);
    setScale(Vector3f(radius, radius, length));
  }


  inline Arrow::Arrow(uint32_t id, uint16_t category, const V3Arg &origin, const V3Arg &dir, float length, float radius)
    : Shape(SIdArrow, id, category)
  {
    setPosition(origin);
    setDirection(dir);
    setScale(Vector3f(radius, radius, length));
  }


  inline Arrow &Arrow::setRadius(float radius)
  {
    Vector3f s = Shape::scale();
    s.x = s.y = radius;
    setScale(s);
    return *this;
  }


  inline float Arrow::radius() const
  {
    return scale().x;
  }


  inline Arrow &Arrow::setLength(float length)
  {
    Vector3f s = Shape::scale();
    s.z = length;
    setScale(s);
    return *this;
  }


  inline float Arrow::length() const
  {
    return scale().z;
  }


  inline Arrow &Arrow::setOrigin(const V3Arg &origin)
  {
    setPosition(origin);
    return *this;
  }


  inline Vector3f Arrow::origin() const
  {
    return position();
  }


  inline Arrow &Arrow::setDirection(const V3Arg &direction)
  {
    Quaternionf rot;
    if (direction.v3.dot(DefaultDirection) > -0.9998f)
    {
      rot = Quaternionf(DefaultDirection, direction);
    }
    else
    {
      rot.setAxisAngle(Vector3f::axisx, float(M_PI));
    }
    setRotation(rot);
    return *this;
  }


  inline Vector3f Arrow::direction() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultDirection;
  }
}

#endif // _3ESARROW_H_
