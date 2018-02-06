//
// author: Kazys Stepanas
//
#include "3espointcloudshape.h"

#include "3esmeshplaceholder.h"

#include <3escoreutil.h>
#include <3espacketwriter.h>

#include <algorithm>

using namespace tes;



PointCloudShape::~PointCloudShape()
{
  freeIndices(_indices);
  if (_ownMesh)
  {
    delete _mesh;
  }
}


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


bool PointCloudShape::readCreate(PacketReader &stream)
{
  if (!Shape::readCreate(stream))
  {
    return false;
  }

  bool ok = true;

  uint32_t valueU32 = 0;

  // Mesh ID.
  ok = ok && stream.readElement(valueU32) == sizeof(valueU32);
  if (_ownMesh)
  {
    delete _mesh;
  }
  _mesh = new MeshPlaceholder(valueU32);
  _ownMesh = true;

  // Index count.
  ok = ok && stream.readElement(valueU32) == sizeof(valueU32);
  if (_indexCount < valueU32)
  {
    freeIndices(_indices);
    _indices = allocateIndices(valueU32);
  }
  _indexCount = valueU32;

  // Point size.
  ok = ok && stream.readElement(_pointSize) == sizeof(_pointSize);
  return ok;
}


bool PointCloudShape::readData(PacketReader &stream)
{
  DataMessage msg;
  bool ok = true;

  ok = msg.read(stream);

  if (ok)
  {
    setId(msg.id);
  }

  uint32_t offset = 0;
  uint32_t count = 0;

  ok = ok && stream.readElement(offset) == sizeof(offset);
  ok = ok && stream.readElement(count) == sizeof(count);

  if (count)
  {
    if (count + offset > _indexCount)
    {
      reallocateIndices(count + offset);
      _indexCount = count + offset;
    }

    ok = ok && stream.readArray(_indices + offset, count) == count;
  }

  return ok;
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


void PointCloudShape::reallocateIndices(uint32_t count)
{
  if (count)
  {
    uint32_t *newIndices = allocateIndices(count);
    if (_indices)
    {
      if (_indexCount)
      {
        memcpy(newIndices, _indices, sizeof(*_indices) * std::min(count, _indexCount));
      }
      freeIndices(_indices);
    }
    _indices = newIndices;
  }
  else
  {
    freeIndices(_indices);
    _indices = nullptr;
  }
  _indexCount = count;
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
