//
// author: Kazys Stepanas
//
#ifndef _3ESSTAR_H_
#define _3ESSTAR_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  /// Defines a star to display. A star is a shape with extrusions in both directions along each axis with spherical
  /// extents.
  ///
  /// A sphere is defined by:
  /// Component      | Description
  /// -------------- | -----------------------------------------------------------------------------------------------
  /// @c centre()    | The sphere centre. An alias for @p position().
  /// @c radius()    | The sphere radius.
  class _3es_coreAPI Star : public Shape
  {
  public:
    Star(uint32_t id = 0u, const V3Arg &centre = V3Arg(0, 0, 0), float radius = 1.0f);
    /// Create a star.
    /// @param id The shape ID, unique among @c Star objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param centre Defines the star centre coordinate.
    /// @param radius The star radius.
    Star(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), float radius = 1.0f);

    inline const char *type() const override { return "star"; }

    /// Set the star radial extents.
    /// @param radius The star radius.
    /// @return @c *this
    Star &setRadius(float radius);
    /// Get the star radial extents.
    /// @return The star radius.
    float radius() const;

    /// Set the star centre coordinate.
    /// @param centre The new star centre.
    /// @return @c *this
    Star &setCentre(const V3Arg &centre);
    /// Get the star centre coordinate.
    /// @return The star centre.
    Vector3f centre() const;
  };


  inline Star::Star(uint32_t id, const V3Arg &centre, float radius)
    : Shape(SIdStar, id)
  {
    setPosition(centre);
    setScale(Vector3f(radius));
  }


  inline Star::Star(uint32_t id, uint16_t category, const V3Arg &centre, float radius)
    : Shape(SIdStar, id, category)
  {
    setPosition(centre);
    setScale(Vector3f(radius));
  }


  inline Star &Star::setRadius(float radius)
  {
    setScale(Vector3f(radius));
    return *this;
  }


  inline float Star::radius() const
  {
    return scale().x;
  }


  inline Star &Star::setCentre(const V3Arg &centre)
  {
    setPosition(centre);
    return *this;
  }


  inline Vector3f Star::centre() const
  {
    return position();
  }
}

#endif // _3ESSTAR_H_
