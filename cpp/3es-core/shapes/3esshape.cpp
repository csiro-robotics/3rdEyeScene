//
// author: Kazys Stepanas
//
#include "3esshape.h"

#include <3espacketwriter.h>

#include <cstdio>

using namespace tes;


void Shape::updateFrom(const Shape &other)
{
  _data.attributes = other._data.attributes;
}


bool Shape::writeCreate(PacketWriter &stream) const
{
  stream.reset(routingId(), CreateMessage::MessageId);
  return _data.write(stream);
}


bool Shape::writeUpdate(PacketWriter &stream) const
{
  UpdateMessage up;
  up.id = _data.id;
  up.flags = _data.flags;
  up.attributes = _data.attributes;
  stream.reset(routingId(), UpdateMessage::MessageId);
  return up.write(stream);
}


bool Shape::writeDestroy(PacketWriter &stream) const
{
  DestroyMessage dm;
  dm.id = _data.id;
  stream.reset(routingId(), DestroyMessage::MessageId);
  return dm.write(stream);
}


int Shape::enumerateResources(const Resource **resources, int capacity, int fetchOffset) const
{
  return 0;
}


Shape *Shape::clone() const
{
  Shape *copy = new Shape(_routingId);
  onClone(copy);
  return copy;
}


void Shape::onClone(Shape *copy) const
{
  copy->_data = _data;
}
