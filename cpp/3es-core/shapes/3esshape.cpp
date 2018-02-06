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


bool Shape::readCreate(PacketReader &stream)
{
  // Assume the routing ID has already been read and resolve.
  return _data.read(stream);
}


bool Shape::readUpdate(PacketReader &stream)
{
  UpdateMessage up;
  if (up.read(stream))
  {
    if ((up.flags & UFUpdateMode) == 0)
    {
      // Full update.
      _data.attributes = up.attributes;
    }
    else
    {
      // Partial update.
      if (up.flags & UFPosition)
      {
        memcpy(_data.attributes.position, up.attributes.position, sizeof(up.attributes.position));
      }
      if (up.flags & UFRotation)
      {
        memcpy(_data.attributes.rotation, up.attributes.rotation, sizeof(up.attributes.rotation));
      }
      if (up.flags & UFScale)
      {
        memcpy(_data.attributes.scale, up.attributes.scale, sizeof(up.attributes.scale));
      }
      if (up.flags & UFColour)
      {
        _data.attributes.colour = up.attributes.colour;
      }
    }
    return true;
  }
  return false;
}


bool Shape::readData(PacketReader &stream)
{
  return false;
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
