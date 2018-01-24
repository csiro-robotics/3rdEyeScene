//
// author: Kazys Stepanas
//
#ifndef _3ESCAPSULE_H_
#define _3ESCAPSULE_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  /// Defines a capsule shape to display. A capsule is a cylinder with hemisphere end caps.
  ///
  /// An arrow is defined by:
  /// Component      | Description
  /// -------------- | -----------------------------------------------------------------------------------------------
  /// @c centre()    | The centre of the capsule. Alias for @p position().
  /// @c axis()      | Defines the capsule primary axis. Affects @c rotation().
  /// @c length()    | The length of the cylindrical part of the capsule. The end caps increase the extents further.
  /// @c radius()    | Radius of the capsule cylinder and end caps.
  class _3es_coreAPI Capsule : public Shape
  {
  public:
    /// Default direction used as a reference orientation for packing the rotation.
    ///
    /// The @c rotation() value is relative to this vector.
    ///
    /// The default is <tt>(0, 0, 1)</tt>
    static const Vector3f DefaultAxis;

    /// @overload
    Capsule(uint32_t id, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &axis = DefaultAxis, float radius = 1.0f, float length = 1.0f);
    /// Construct a capsule object.
    /// @param id The shape ID, unique among @c Capsule objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param centre Centre @c position() of the capsule.
    /// @param axis Defines the capsule's primary axis.
    /// @param radius Radius of the capsule cylinder and end caps.
    /// @param length Length of the capsule cylinder body.
    Capsule(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &axis = DefaultAxis, float radius = 1.0f, float length = 1.0f);

    inline const char *type() const override { return "capsule"; }

    /// Set the capsule body radius.
    /// @param radius The radius to set.
    /// @return @c *this
    Capsule &setRadius(float radius);
    /// Get the capsule radius.
    /// @return The capsule radius.
    float radius() const;

    /// Set the capsule body length. The end caps extend beyond this by the radius at each end.
    /// @param length The body length to set.
    /// @return @c *this
    Capsule &setLength(float radius);
    /// Get the capsule body length.
    /// @param The body length.
    float length() const;

    /// Set the position fo the capsule centre.
    /// @param centre The centre coordinate.
    /// @return @c *this
    Capsule &setCentre(const V3Arg &centre);
    /// Get the capsule centre position.
    /// @return The centre coordinate.
    Vector3f centre() const;

    /// Set the capsule primary axis. Affects @p rotation().
    /// @param axis The new axis to set.
    /// @return @c *this
    Capsule &setAxis(const V3Arg &axis);
    /// Get the capsule primary axis.
    ///
    /// May not exactly match the axis given via @p setAxis() as the axis is defined by the quaternion @c rotation().
    /// @return The primary axis.
    Vector3f axis() const;
  };


  inline Capsule::Capsule(uint32_t id, const V3Arg &centre, const V3Arg &axis, float radius, float length)
    : Shape(SIdCapsule, id)
  {
    setPosition(centre);
    setAxis(axis);
    setScale(Vector3f(radius, radius, length));
  }


  inline Capsule::Capsule(uint32_t id, uint16_t category, const V3Arg &centre, const V3Arg &axis, float radius, float length)
    : Shape(SIdCapsule, id, category)
  {
    setPosition(centre);
    setAxis(axis);
    setScale(Vector3f(radius, length, 1));
  }


  inline Capsule &Capsule::setRadius(float radius)
  {
    Vector3f s = Shape::scale();
    s.x = s.y = radius;
    setScale(s);
    return *this;
  }


  inline float Capsule::radius() const
  {
    return scale().x;
  }


  inline Capsule &Capsule::setLength(float length)
  {
    Vector3f s = Shape::scale();
    s.z = length;
    setScale(s);
    return *this;
  }


  inline float Capsule::length() const
  {
    return scale().z;
  }


  inline Capsule &Capsule::setCentre(const V3Arg &centre)
  {
    setPosition(centre);
    return *this;
  }


  inline Vector3f Capsule::centre() const
  {
    return position();
  }


  inline Capsule &Capsule::setAxis(const V3Arg &axis)
  {
    Quaternionf rot;
    if (axis.v3.dot(DefaultAxis) > -0.9998f)
    {
      rot = Quaternionf(DefaultAxis, axis);
    }
    else
    {
      rot.setAxisAngle(Vector3f::axisx, float(M_PI));
    }
    setRotation(rot);
    return *this;
  }


  inline Vector3f Capsule::axis() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultAxis;
  }
}

#endif // _3ESCAPSULE_H_
