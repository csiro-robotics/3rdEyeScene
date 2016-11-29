//
// author: Kazys Stepanas
//
#ifndef _3ESCONE_H_
#define _3ESCONE_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  class _3es_coreAPI Cone : public Shape
  {
  public:
    static const Vector3f DefaultDir;

    Cone(uint32_t id = 0u, const V3Arg &point = V3Arg(0, 0, 0), const V3Arg &dir = DefaultDir, float angle = 45.0f / 360.0f * float(M_PI), float length = 1.0f);
    Cone(uint32_t id, uint16_t category, const V3Arg &point = V3Arg(0, 0, 0), const V3Arg &dir = DefaultDir, float angle = 45.0f / 360.0f * float(M_PI), float length = 1.0f);

    Cone &setAngle(float angle);
    float angle() const;
    Cone &setLength(float length);
    float length() const;
    Cone &setPoint(const V3Arg &point);
    Vector3f point() const;
    Cone &setDirection(const V3Arg &dir);
    Vector3f direction() const;
  };


  inline Cone::Cone(uint32_t id, const V3Arg &point, const V3Arg &dir, float angle, float length)
    : Shape(SIdCone, id)
  {
    setPosition(point);
    setDirection(dir);
    setScale(Vector3f(angle, angle, length));
  }


  inline Cone::Cone(uint32_t id, uint16_t category, const V3Arg &point, const V3Arg &dir, float angle, float length)
    : Shape(SIdCone, id, category)
  {
    setPosition(point);
    setDirection(dir);
    setScale(Vector3f(angle, angle, length));
  }


  inline Cone &Cone::setAngle(float angle)
  {
    Vector3f s = Shape::scale();
    s.x = s.y = angle;
    setScale(s);
    return *this;
  }


  inline float Cone::angle() const
  {
    return scale().x;
  }


  inline Cone &Cone::setLength(float length)
  {
    Vector3f s = Shape::scale();
    s.z = length;
    setScale(s);
    return *this;
  }


  inline float Cone::length() const
  {
    return scale().z;
  }


  inline Cone &Cone::setPoint(const V3Arg &point)
  {
    setPosition(point);
    return *this;
  }


  inline Vector3f Cone::point() const
  {
    return position();
  }


  inline Cone &Cone::setDirection(const V3Arg &dir)
  {
    Quaternionf rot;
    if (dir.v3.dot(DefaultDir) > -0.9998f)
    {
      rot = Quaternionf(DefaultDir, dir);
    }
    else
    {
      rot.setAxisAngle(Vector3f::axisx, float(M_PI));
    }
    setRotation(rot);
    return *this;
  }


  inline Vector3f Cone::direction() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultDir;
  }
}

#endif // _3ESCONE_H_
