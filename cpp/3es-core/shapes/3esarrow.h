//
// author: Kazys Stepanas
//
#ifndef _3ESARROW_H_
#define _3ESARROW_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  class _3es_coreAPI Arrow : public Shape
  {
  public:
    static const Vector3f DefaultDirection;

    Arrow(uint32_t id = 0u, const V3Arg &origin = V3Arg(0, 0, 0), const V3Arg &dir = DefaultDirection, float length = 1.0f, float radius = 0.025f);
    Arrow(uint32_t id, uint16_t category, const V3Arg &origin = V3Arg(0, 0, 0), const V3Arg &dir = V3Arg(0, 0, 1), float length = 1.0f, float radius = 0.025f);

    Arrow &setRadius(float radius);
    float radius() const;
    Arrow &setLength(float radius);
    float length() const;
    Arrow &setOrigin(const V3Arg &origin);
    Vector3f origin() const;
    Arrow &setDirection(const V3Arg &direction);
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
