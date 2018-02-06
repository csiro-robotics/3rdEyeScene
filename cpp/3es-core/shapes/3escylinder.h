//
// author: Kazys Stepanas
//
#ifndef _3ESCYLINDER_H_
#define _3ESCYLINDER_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  /// Defines a cylinder shape to display.
  ///
  /// An arrow is defined by:
  /// Component      | Description
  /// -------------- | -----------------------------------------------------------------------------------------------
  /// @c centre()    | The centre of the cylinder. Alias for @p position().
  /// @c axis()      | Defines the cylinder primary axis. Affects @c rotation().
  /// @c length()    | The length of the cylinder body.
  /// @c radius()    | Radius of the cylinder walls.
  class _3es_coreAPI Cylinder : public Shape
  {
  public:
    /// Default direction used as a reference orientation for packing the rotation.
    ///
    /// The @c rotation() value is relative to this vector.
    ///
    /// The default is <tt>(0, 0, 1)</tt>
    static const Vector3f DefaultAxis;

    /// @overload
    Cylinder(uint32_t id = 0u, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &axis = DefaultAxis, float radius = 1.0f, float length = 1.0f);
    /// Construct a cylinder object.
    /// @param id The shape ID, unique among @c Capsule objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param centre Centre @c position() of the cylindef.
    /// @param axis Defines the cylinder's primary axis.
    /// @param radius Radius of the cylinder walls.
    /// @param length Length of the cylinder body.
    Cylinder(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &axis = DefaultAxis, float radius = 1.0f, float length = 1.0f);

    inline const char *type() const override { return "cylinder"; }

    /// Set the cylinder body radius.
    /// @param radius The radius to set.
    /// @return @c *this
    Cylinder &setRadius(float radius);
    /// Get the cylinder radius.
    /// @return The cylinder radius.
    float radius() const;

    /// Set the cylinder body length.
    /// @param length The body length to set.
    /// @return @c *this
    Cylinder &setLength(float radius);
    /// Get the cylinder body length.
    /// @param The body length.
    float length() const;

    /// Set the position fo the cylinder centre.
    /// @param centre The centre coordinate.
    /// @return @c *this
    Cylinder &setCentre(const V3Arg &centre);
    /// Get the cylinder centre position.
    /// @return The centre coordinate.
    Vector3f centre() const;

    /// Set the cylinder primary axis. Affects @p rotation().
    /// @param axis The new axis to set.
    /// @return @c *this
    Cylinder &setAxis(const V3Arg &axis);
    /// Get the cylinder primary axis.
    ///
    /// May not exactly match the axis given via @p setAxis() as the axis is defined by the quaternion @c rotation().
    /// @return The primary axis.
    Vector3f axis() const;
  };


  inline Cylinder::Cylinder(uint32_t id, const V3Arg &centre, const V3Arg &axis, float radius, float length)
    : Shape(SIdCylinder, id)
  {
    setPosition(centre);
    setAxis(axis);
    setScale(Vector3f(radius, radius, length));
  }


  inline Cylinder::Cylinder(uint32_t id, uint16_t category, const V3Arg &centre, const V3Arg &axis, float radius, float length)
    : Shape(SIdCylinder, id, category)
  {
    setPosition(centre);
    setAxis(axis);
    setScale(Vector3f(radius, radius, length));
  }


  inline Cylinder &Cylinder::setRadius(float radius)
  {
    Vector3f s = Shape::scale();
    s.x = s.y = radius;
    setScale(s);
    return *this;
  }


  inline float Cylinder::radius() const
  {
    return scale().x;
  }


  inline Cylinder &Cylinder::setLength(float length)
  {
    Vector3f s = Shape::scale();
    s.z = length;
    setScale(s);
    return *this;
  }


  inline float Cylinder::length() const
  {
    return scale().z;
  }


  inline Cylinder &Cylinder::setCentre(const V3Arg &centre)
  {
    setPosition(centre);
    return *this;
  }


  inline Vector3f Cylinder::centre() const
  {
    return position();
  }


  inline Cylinder &Cylinder::setAxis(const V3Arg &axis)
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


  inline Vector3f Cylinder::axis() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultAxis;
  }
}

#endif // _3ESCYLINDER_H_
