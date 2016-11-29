//
// author: Kazys Stepanas
//
#ifndef _3ESCAPSULE_H_
#define _3ESCAPSULE_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  class _3es_coreAPI Capsule : public Shape
  {
  public:
    static const Vector3f DefaultUp;

    Capsule(uint32_t id, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &up = DefaultUp, float radius = 1.0f, float length = 1.0f);
    Capsule(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), const V3Arg &up = DefaultUp, float radius = 1.0f, float length = 1.0f);

    Capsule &setRadius(float radius);
    float radius() const;
    Capsule &setLength(float radius);
    float length() const;
    Capsule &setCentre(const V3Arg &centre);
    Vector3f centre() const;
    Capsule &setUp(const V3Arg &up);
    Vector3f up() const;
  };


  inline Capsule::Capsule(uint32_t id, const V3Arg &centre, const V3Arg &up, float radius, float length)
    : Shape(SIdCapsule, id)
  {
    setPosition(centre);
    setUp(up);
    setScale(Vector3f(radius, radius, length));
  }


  inline Capsule::Capsule(uint32_t id, uint16_t category, const V3Arg &centre, const V3Arg &up, float radius, float length)
    : Shape(SIdCapsule, id, category)
  {
    setPosition(centre);
    setUp(up);
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


  inline Capsule &Capsule::setUp(const V3Arg &up)
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


  inline Vector3f Capsule::up() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultUp;
  }
}

#endif // _3ESCAPSULE_H_
