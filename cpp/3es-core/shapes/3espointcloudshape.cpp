//
// author: Kazys Stepanas
//
#include "3espointcloudshape.h"

#include "3esmeshresource.h"

#include <3escoreutil.h>
#include <3espacketwriter.h>

#include <algorithm>

using namespace tes;

bool PointCloudShape::writeCreate(PacketWriter &stream) const
{
  bool ok = Shape::writeCreate(stream);
  // Write the point cloud ID.
  uint32_t valueU32 = _mesh->id();
  ok = stream.writeElement(valueU32) == sizeof(valueU32) && ok;
  // Write the index count.
  valueU32 = _indexCount;
  ok = stream.writeElement(valueU32) == sizeof(valueU32) && ok;
  // Write point size.
  ok = stream.writeElement(_pointSize) == sizeof(_pointSize) && ok;
  return ok;
}


int PointCloudShape::writeData(PacketWriter &stream, unsigned &progressMarker) const
{
  // Max items based on packet size of 0xffff, minus some overhead divide by index size.
  const uint32_t MaxItems = ((0xffffu - 256u) / 4u);
  DataMessage msg;
  bool ok = true;
  stream.reset(routingId(), DataMessage::MessageId);
  msg.id = id();
  msg.write(stream);

  // Write indices for this view into the cloud.
  uint32_t offset = progressMarker;
  uint32_t count = _indexCount - progressMarker;
  if (count > MaxItems)
  {
    count = MaxItems;
  }

  // Use 32-bits for both values though count will never need greater than 16-bit.
  ok = stream.writeElement(offset) == sizeof(offset) && ok;
  ok = stream.writeElement(count) == sizeof(count) && ok;

  if (count)
  {
    ok = stream.writeArray(_indices + offset, count) == count && ok;
  }

  if (!ok)
  {
    return -1;
  }

  progressMarker += count;
  return (progressMarker < _indexCount) ? 1 : 0;
}


int PointCloudShape::enumerateResources(const Resource **resources, int capacity, int fetchOffset) const
{
  if (!resources || !capacity)
  {
    return 1;
  }

  if (fetchOffset == 0)
  {
    resources[0] = _mesh;
    return 1;
  }

  return 0;
}


Shape *PointCloudShape::clone() const
{
  PointCloudShape *copy = new PointCloudShape(_mesh);
  onClone(copy);
  return copy;
}


void PointCloudShape::onClone(PointCloudShape *copy) const
{
  Shape::onClone(copy);
  if (_indexCount)
  {
    copy->_indices = copy->allocateIndices(_indexCount);
    memcpy(copy->_indices, _indices, sizeof(*_indices) * _indexCount);
    copy->_indexCount = _indexCount;
  }
  copy->_mesh = _mesh;
  copy->_pointSize = _pointSize;
}


uint32_t *PointCloudShape::allocateIndices(uint32_t count)
{
  // Hidden for consistent allocator usage.
  return new uint32_t[count];
}


void PointCloudShape::freeIndices(const uint32_t *indices)
{
  // Hidden for consistent allocator usage.
  delete[] indices;
}
