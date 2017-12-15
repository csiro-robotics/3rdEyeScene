//
// author: Kazys Stepanas
//
#ifndef _3ESBOX_H_
#define _3ESBOX_H_

#include "3es-core.h"
#include "3esshape.h"

namespace tes
{
  /// Defines a rectangular prism shape.
  ///
  /// The box is defined by its centre, scale and orientation. The scale defines the full extents from one corner to another.
  ///
  /// A box is defined by:
  /// Component      | Description
  /// -------------- | -----------------------------------------------------------------------------------------------
  /// @c position()  | The box base position.
  /// @c scale()     | The box size/scale, where (1, 1, 1) defines a unit box.
  /// @c rotation()  | Quaternion rotation to apply to the box.
  class _3es_coreAPI Box : public Shape
  {
  public:
    /// @overload
    Box(uint32_t id = 0u, const V3Arg &pos = V3Arg(0, 0, 0), const V3Arg &scale = V3Arg(1.0f, 1.0f, 1.0f), const QuaternionArg &rot = QuaternionArg(0, 0, 0, 1));
    /// Construct a box object.
    /// @param id The shape ID, unique among @c Box objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param pos Marks the centre position of the box.
    /// @param scale Defines the size of the box, were (1, 1, 1) denotes a unit box.
    /// @param rot Quaternion rotation to apply to the box.
    Box(uint32_t id, uint16_t category, const V3Arg &pos = V3Arg(0, 0, 0), const V3Arg &scale = V3Arg(1, 1, 1), const QuaternionArg &rot = QuaternionArg(0, 0, 0, 1));
  };


  inline Box::Box(uint32_t id, const V3Arg &pos, const V3Arg &scale, const QuaternionArg &rot)
    : Shape(SIdBox, id)
  {
    setPosition(pos);
    setRotation(rot);
    setScale(scale);
  }


  inline Box::Box(uint32_t id, uint16_t category, const V3Arg &pos, const V3Arg &scale, const QuaternionArg &rot)
    : Shape(SIdBox, id, category)
  {
    setPosition(pos);
    setRotation(rot);
    setScale(scale);
  }
}

#endif // _3ESBOX_H_
