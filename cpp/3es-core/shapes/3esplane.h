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
  /// Defines a square planar section to display.
  ///
  /// The scale is interpreted as the size of the square to display at the plane origin. The normal render length is
  /// also adjustable.
  class _3es_coreAPI Plane : public Shape
  {
  public:
    /// Defines the default plane normal orientation.
    static const Vector3f DefaultNormal;

    /// @overload
    Plane(uint32_t id = 0u, const V3Arg &position = V3Arg(0, 0, 0), const V3Arg &normal = DefaultNormal, float scale = 1.0f, float normalLength = 1.0f);
    /// Create a plane.
    /// @param id The shape ID. Zero for transient shapes.
    /// @param category The category grouping for the shape used for filtering.
    /// @param position Defines the plane origin and where the plane's square is display.
    /// @param normal Defines the plane normal.
    /// @param scale Defines the size of the square to display.
    /// @param normalLength Adjusts the display length of the normal.
    Plane(uint32_t id, uint16_t category, const V3Arg &position = V3Arg(0, 0, 0), const V3Arg &normal = DefaultNormal, float scale = 1.0f, float normalLength = 1.0f);

    Plane &setNormal(const V3Arg &normal);
    Vector3f normal() const;
    Plane &setScale(float scale);
    float scale() const;
    Plane &setNormalLength(float len);
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
