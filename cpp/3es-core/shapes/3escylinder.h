//
// author: Kazys Stepanas
//
#ifndef _3ESCYLINDER_H_
#define _3ESCYLINDER_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  class _3es_coreAPI Cylinder : public Shape
  {
  public:
    static const Vector3f DefaultUp;

    Cylinder(uint32_t id = 0u, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &up = DefaultUp, float radius = 1.0f, float length = 1.0f);
    Cylinder(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &up = DefaultUp, float radius = 1.0f, float length = 1.0f);

    Cylinder &setRadius(float radius);
    float radius() const;
    Cylinder &setLength(float radius);
    float length() const;
    Cylinder &setCentre(const V3Arg &centre);
    Vector3f centre() const;
    Cylinder &setUp(const V3Arg &up);
    Vector3f up() const;
  };


  inline Cylinder::Cylinder(uint32_t id, const V3Arg &centre, const V3Arg &up, float radius, float length)
    : Shape(SIdCylinder, id)
  {
    setPosition(centre);
    setUp(up);
    setScale(Vector3f(radius, radius, length));
  }


  inline Cylinder::Cylinder(uint32_t id, uint16_t category, const V3Arg &centre, const V3Arg &up, float radius, float length)
    : Shape(SIdCylinder, id, category)
  {
    setPosition(centre);
    setUp(up);
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


  inline Cylinder &Cylinder::setUp(const V3Arg &up)
  {
    Quaternionf rot;
    if (up.v3.dot(DefaultUp) > -0.9998f)
    {
      rot = Quaternionf(DefaultUp, up);
    }
    else
    {
      rot.setAxisAngle(Vector3f::axisx, float(M_PI));
    }
    setRotation(rot);
    return *this;
  }


  inline Vector3f Cylinder::up() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultUp;
  }
}

#endif // _3ESCYLINDER_H_
