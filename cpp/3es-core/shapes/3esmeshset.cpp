//
// author: Kazys Stepanas
//
#include "3esmeshset.h"

#include "3esmeshmessages.h"
#include "3esmeshplaceholder.h"
#include "3esrotation.h"

#include <algorithm>

using namespace tes;

MeshSet::MeshSet(uint32_t id, uint16_t category, const IntArg &partCount)
  : Shape(SIdMeshSet, id, category)
  , _parts(partCount.i() ? new const MeshResource *[partCount.i()] : nullptr)
  , _transforms(partCount.i() ? new Matrix4f[partCount.i()] : nullptr)
  , _partCount(partCount)
  , _ownParts(false)
{
  if (partCount)
  {
    memset(_parts, 0, sizeof(*_parts) * partCount.i());
    for (int i = 0; i < partCount.i(); ++i)
    {
      _transforms[i] = Matrix4f::identity;
    }
  }
}


MeshSet::MeshSet(const MeshResource *part, uint32_t id, uint16_t category)
  : Shape(SIdMeshSet, id, category)
  , _parts(new const MeshResource *[1])
  , _transforms(new Matrix4f[1])
  , _partCount(1)
  , _ownParts(false)
{
  _parts[0] = part;
  _transforms[0] = Matrix4f::identity;
}


MeshSet::~MeshSet()
{
  cleanupParts();
}


bool MeshSet::writeCreate(PacketWriter &stream) const
{
  if (!Shape::writeCreate(stream))
  {
    return false;
  }

  ObjectAttributes attr;
  Quaternionf rot;
  Vector3f pos, scale;
  uint32_t partId;
  uint16_t numberOfParts = partCount();

  memset(&attr, 0, sizeof(attr));
  attr.colour = 0xffffffffu;

  stream.writeElement(numberOfParts);

  for (int i = 0; i < numberOfParts; ++i)
  {
    const MeshResource *mesh = _parts[i];
    if (mesh)
    {
      partId = mesh->id();
    }
    else
    {
      // Write a dummy.
      partId = 0;
    }

    transformToQuaternionTranslation(_transforms[i], rot, pos, scale);
    attr.position[0] = pos[0];
    attr.position[1] = pos[1];
    attr.position[2] = pos[2];
    attr.rotation[0] = rot[0];
    attr.rotation[1] = rot[1];
    attr.rotation[2] = rot[2];
    attr.rotation[3] = rot[3];
    attr.scale[0] = scale[0];
    attr.scale[1] = scale[1];
    attr.scale[2] = scale[2];

    stream.writeElement(partId);
    attr.write(stream);
  }

  return true;
}


bool MeshSet::readCreate(PacketReader &stream)
{
  if (!Shape::readCreate(stream))
  {
    return false;
  }

  ObjectAttributes attr;
  Quaternionf rot;
  Vector3f pos, scale;
  uint32_t partId = 0;
  uint16_t numberOfParts = 0;

  bool ok = true;

  memset(&attr, 0, sizeof(attr));

  ok = ok && stream.readElement(numberOfParts) == sizeof(numberOfParts);

  if (ok && numberOfParts > _partCount)
  {
    cleanupParts();

    _parts = new const MeshResource *[numberOfParts];
    _transforms = new Matrix4f[numberOfParts];
    _ownParts = true;

    memset(_parts, 0, sizeof(*_parts) * numberOfParts);

    _partCount = numberOfParts;
  }

  for (int i = 0; i < _partCount; ++i)
  {
    transformToQuaternionTranslation(_transforms[i], rot, pos, scale);

    attr.position[0] = pos[0];
    attr.position[1] = pos[1];
    attr.position[2] = pos[2];
    attr.rotation[0] = rot[0];
    attr.rotation[1] = rot[1];
    attr.rotation[2] = rot[2];
    attr.rotation[3] = rot[3];
    attr.scale[0] = scale[0];
    attr.scale[1] = scale[1];
    attr.scale[2] = scale[2];

    ok = ok && stream.readElement(partId) == sizeof(partId);
    ok = ok && attr.read(stream);

    if (ok)
    {
      _transforms[i] = prsTransform(Vector3f(attr.position), Quaternionf(attr.rotation), Vector3f(attr.scale));
      // We can only reference dummy meshes here.
      _parts[i] = new MeshPlaceholder(partId);
    }
  }

  return ok;
}


int MeshSet::enumerateResources(const Resource **resources, int capacity, int fetchOffset) const
{
  if (!resources || !capacity)
  {
    return _partCount;
  }

  int copyCount = std::min<int>(capacity, _partCount - fetchOffset);
  if (copyCount <= 0)
  {
    return 0;
  }

  const MeshResource **src = _parts + fetchOffset;
  const Resource **dst = resources;
  for (int i = 0; i < copyCount; ++i)
  {
    *dst++ = *src++;
  }
  return copyCount;
}


Shape *MeshSet::clone() const
{
  MeshSet *copy = new MeshSet(_partCount);
  onClone(copy);
  return copy;
}


void MeshSet::onClone(MeshSet *copy) const
{
  Shape::onClone(copy);
  if (copy->_partCount != _partCount)
  {
    delete [] copy->_parts;
    delete [] copy->_transforms;
    if (_partCount)
    {
      copy->_parts = new const MeshResource*[_partCount];
      copy->_transforms = new Matrix4f[_partCount];
    }
    else
    {
      copy->_parts = nullptr;
      copy->_transforms = nullptr;
    }
  }
  memcpy(copy->_parts, _parts, sizeof(*_parts) * _partCount);
  memcpy(copy->_transforms, _transforms, sizeof(*_transforms) * _partCount);
}


void MeshSet::cleanupParts()
{
  if (_ownParts && _parts)
  {
    for (int i = 0; i < _partCount; ++i)
    {
      delete _parts[i];
    }
  }

  delete [] _parts;
  delete [] _transforms;

  _parts = nullptr;
  _transforms = nullptr;
  _ownParts = false;
}
