//
// author: Kazys Stepanas
//
#ifndef _3ESSTAR_H_
#define _3ESSTAR_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  class _3es_coreAPI Star : public Shape
  {
  public:
    Star(uint32_t id = 0u, const V3Arg &centre = V3Arg(0, 0, 0), float radius = 1.0f);
    Star(uint32_t id, uint16_t category, const V3Arg &centre = V3Arg(0, 0, 0), float radius = 1.0f);

    Star &setRadius(float radius);
    float radius() const;
    Star &setCentre(const V3Arg &centre);
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
