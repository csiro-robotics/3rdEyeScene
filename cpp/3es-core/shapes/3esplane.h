//
// author: Kazys Stepanas
//
#ifndef _3ESPLANE_H_
#define _3ESPLANE_H_

#include "3es-core.h"
#include "3esshape.h"

#include <3esrotation.h>

namespace tes
{
  /// Defines a rectangular planar section to display.
  ///
  /// A plane is defined by:
  /// Component      | Description
  /// -------------- | -----------------------------------------------------------------------------------------------
  /// @c position()  | Where to display a planar section.
  /// @c normal()    | The plane normal.
  /// @c scale()     | Defines the size of the plane rectangle (X,Y) and @c normalLength() (Z).
  class _3es_coreAPI Plane : public Shape
  {
  public:
    /// Defines the default plane normal orientation.
    ///
    /// The @c rotation() value is relative to this vector.
    ///
    /// The default is <tt>(0, 0, 1)</tt>
    static const Vector3f DefaultNormal;

    /// @overload
    Plane(uint32_t id = 0u, const V3Arg &position = V3Arg(0, 0, 0), const V3Arg &normal = DefaultNormal, float scale = 1.0f, float normalLength = 1.0f);
    /// Create a plane.
    /// @param id The shape ID, unique among @c Plane objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param position Defines the plane origin and where the plane's square is display.
    /// @param normal Defines the plane normal.
    /// @param scale Defines the size of the square to display.
    /// @param normalLength Adjusts the display length of the normal.
    Plane(uint32_t id, uint16_t category, const V3Arg &position = V3Arg(0, 0, 0), const V3Arg &normal = DefaultNormal, float scale = 1.0f, float normalLength = 1.0f);

    /// Set the plane normal. Affects @p rotation().
    /// @param axis The new axis to set.
    /// @return @c *this
    Plane &setNormal(const V3Arg &normal);
     /// Get the plane normal.
    ///
    /// May not exactly match the axis given via @p setNormal() as the axis is defined by the quaternion @c rotation().
    /// @return The plane normal.
    Vector3f normal() const;

    /// Set the plane "scale", which controls the render size.
    ///
    /// The X,Y axes control the size of the rectangle used to display the plane at @p position(). The Z is the same
    /// as the @c normalLength(). Note there is non guarantee on the orientation of the plane rectangle.
    ////
    /// @param scale The scaling values to set.
    /// @return @c *this
    Plane &setScale(float scale);
    /// Get the plane scaling values.
    /// @return The plane scaling values.
    float scale() const;

    /// Set the plane normal's display length. Alias for @c scale().z
    /// @param length Display length to set.
    /// @return @c *this
    Plane &setNormalLength(float length);

    /// Get the plane normal display length.
    /// @return The normal display length.
    float normalLength() const;
  };


  inline Plane::Plane(uint32_t id, const V3Arg &position, const V3Arg &normal, float scale, float normalLength)
    : Shape(SIdPlane, id)
  {
    setPosition(position);
    setNormal(normal);
    Shape::setScale(Vector3f(scale, normalLength, scale));
  }


  inline Plane::Plane(uint32_t id, uint16_t category, const V3Arg &position, const V3Arg &normal, float scale, float normalLength)
    : Shape(SIdPlane, id, category)
  {
    setPosition(position);
    setNormal(normal);
    Shape::setScale(Vector3f(scale, normalLength, scale));
  }


  inline Plane &Plane::setNormal(const V3Arg &normal)
  {
    Quaternionf rot(DefaultNormal, normal);
    setRotation(rot);
    return *this;
  }


  inline Vector3f Plane::normal() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultNormal;
  }


  inline Plane &Plane::setScale(float scale)
  {
    Vector3f s = Shape::scale();
    s.x = s.z = scale;
    Shape::setScale(s);
    return *this;
  }


  inline float Plane::scale() const
  {
    return Shape::scale().x;
  }


  inline Plane &Plane::setNormalLength(float len)
  {
    Vector3f s = Shape::scale();
    s.y = len;
    Shape::setScale(s);
    return *this;
  }


  inline float Plane::normalLength() const
  {
    return Shape::scale().y;
  }
}

#endif // _3ESPLANE_H_
