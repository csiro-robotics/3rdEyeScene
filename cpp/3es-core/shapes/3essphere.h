//
// author: Kazys Stepanas
//
#ifndef _3ESSPHERE_H_
#define _3ESSPHERE_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  class _3es_coreAPI Sphere : public Shape
  {
  public:
    Sphere(uint32_t id = 0u, const V3Arg &centre = V3Arg(0, 0, 0), float radius = 1.0f);
    Sphere(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), float radius = 1.0f);

    Sphere &setRadius(float radius);
    float radius() const;
    Sphere &setCentre(const V3Arg &centre);
    Vector3f centre() const;
  };


  inline Sphere::Sphere(uint32_t id, const V3Arg &centre, float radius)
    : Shape(SIdSphere, id)
  {
    setPosition(centre);
    setScale(Vector3f(radius));
  }


  inline Sphere::Sphere(uint32_t id, uint16_t category, const V3Arg &centre, float radius)
    : Shape(SIdSphere, id, category)
  {
    setPosition(centre);
    setScale(Vector3f(radius));
  }


  inline Sphere &Sphere::setRadius(float radius)
  {
    setScale(Vector3f(radius));
    return *this;
  }


  inline float Sphere::radius() const
  {
    return scale().x;
  }


  inline Sphere &Sphere::setCentre(const V3Arg &centre)
  {
    setPosition(centre);
    return *this;
  }


  inline Vector3f Sphere::centre() const
  {
    return position();
  }
}

#endif // _3ESSPHERE_H_
