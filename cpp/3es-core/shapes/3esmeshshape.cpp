//
// author: Kazys Stepanas
//
#include "3esmeshshape.h"

#include "3esmeshresource.h"
#include "3esmeshset.h"

#include <3escoreutil.h>
#include <3espacketwriter.h>

#include <algorithm>

using namespace tes;


MeshShape &MeshShape::setNormals(const float *normals, size_t normalByteSize)
{
  if (_ownNormals)
  {
    freeVertices(_normals);
  }
  _ownNormals = false;
  _normals = normals;
  _normalsStride = unsigned(normalByteSize / sizeof(*_normals));
  _normalsCount = _normals ? _vertexCount : 0;
  if (_ownPointers)
  {
    // Pointers are owned. Need to copy the normals.
    float *newNormals = nullptr;
    _normalsStride = 3;
    if (_normalsCount)
    {
      newNormals = allocateVertices(_normalsCount);
      if (normalByteSize == sizeof(*_normals) * _normalsStride)
      {
        memcpy(newNormals, normals, normalByteSize * _normalsCount);
      }
      else
      {
        const size_t elementStride = normalByteSize / sizeof(*normals);
        for (size_t i = 0; i < _normalsCount; ++i)
        {
          newNormals[i * 3 + 0] = normals[0];
          newNormals[i * 3 + 1] = normals[1];
          newNormals[i * 3 + 2] = normals[2];
          normals += elementStride;
        }
      }
    }
    _normals = newNormals;
    _ownNormals = true;
    setCalculateNormals(false);
  }
  return *this;
}


MeshShape &MeshShape::setUniformNormal(const Vector3f &normal)
{
  if (_ownNormals)
  {
    freeVertices(_normals);
  }

  float *normals = allocateVertices(1);
  _normalsCount = 1;
  _normals = normals;
  _ownNormals = true;
  normals[0] = normal[0];
  normals[1] = normal[1];
  normals[2] = normal[2];
  setCalculateNormals(false);
  return *this;
}


MeshShape &MeshShape::expandVertices()
{
  if (!_indices && !_indexCount)
  {
    return *this;
  }
  // We unpack all vertices and stop indexing.
  float *verts = allocateVertices(_indexCount);
  float *dst = verts;
  for (unsigned i = 0; i < _indexCount; ++i)
  {
    *dst++ = _vertices[_indices[i] * _vertexStride + 0];
    *dst++ = _vertices[_indices[i] * _vertexStride + 1];
    *dst++ = _vertices[_indices[i] * _vertexStride + 2];
  }

  float *normals = nullptr;
  if (_normals && _normalsCount == _vertexCount)
  {
    normals = allocateVertices(_indexCount);
    dst = normals;
    for (unsigned i = 0; i < _indexCount; ++i)
    {
      *dst++ = _normals[_indices[i] * _normalsStride + 0];
      *dst++ = _normals[_indices[i] * _normalsStride + 1];
      *dst++ = _normals[_indices[i] * _normalsStride + 2];
    }
  }

  if (_ownPointers)
  {
    freeVertices(_vertices);
    freeIndices(_indices);
  }
  if (_ownNormals)
  {
    freeVertices(_normals);
  }

  _vertices = verts;
  _vertexCount = _indexCount;
  _normals = normals;
  _indices = nullptr;
  _indexCount = 0;
  _ownPointers = true;
  _ownNormals = normals != nullptr;

  return *this;
}


bool MeshShape::writeCreate(PacketWriter &stream) const
{
  bool ok = Shape::writeCreate(stream);
  uint32_t count = _vertexCount;
  ok = stream.writeElement(count) == sizeof(count) && ok;
  count = _indexCount;
  ok = stream.writeElement(count) == sizeof(count) && ok;
  uint8_t drawType = _drawType;
  ok = stream.writeElement(drawType) == sizeof(drawType) && ok;
  return ok;
}


int MeshShape::writeData(PacketWriter &stream, unsigned &progressMarker) const
{
  bool ok = true;
  DataMessage msg;
  // Local byte overhead needs to account for the size of sendType, offset and itemCount.
  // Use a larger value as I haven't got the edge cases quite right yet.
  const size_t localByteOverhead = 100;
  msg.id = _data.id;
  stream.reset(routingId(), DataMessage::MessageId);
  ok = msg.write(stream);

  // Send vertices or indices?
  uint32_t offset;
  uint32_t itemCount;
  uint16_t sendType = SDT_Vertices;
  // Normals must be sent first as the mesh if finalised on completing vertex/index data.
  if (progressMarker < _normalsCount)
  {
    // Send normals.
    const int maxPacketNormals = MeshResource::estimateTransferCount(12, 0, sizeof(DataMessage) + localByteOverhead);
    offset = progressMarker;
    itemCount = uint32_t(std::min<uint32_t>(_normalsCount - offset, maxPacketNormals));

    sendType = (_normalsCount != 1) ? SDT_Normals : SDT_UniformNormal;  // Sending normals.
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;

    const float *n = _normals + offset * _normalsStride;
    if (_normalsStride == 3)
    {
      ok = stream.writeArray(n, itemCount * 3) == itemCount * 3 && ok;
    }
    else
    {
      for (unsigned i = 0; i < itemCount; ++i, n += _normalsStride)
      {
        ok = stream.writeArray(n, 3) == 3 && ok;
      }
    }

    progressMarker += itemCount;
  }
  else if (progressMarker < _normalsCount + _vertexCount)
  {
    // Send vertices.
    const int maxPacketVertices = MeshResource::estimateTransferCount(12, 0, sizeof(DataMessage) + localByteOverhead);
    offset = progressMarker - _normalsCount;
    itemCount = uint32_t(std::min<uint32_t>(_vertexCount - offset, maxPacketVertices));

    sendType = SDT_Vertices; // Sending vertices.
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;

    const float *v = _vertices + offset * _vertexStride;
    if (_vertexStride == 3)
    {
      ok = stream.writeArray(v, itemCount * 3) == itemCount * 3 && ok;
    }
    else
    {
      for (unsigned i = 0; i < itemCount; ++i, v += _vertexStride)
      {
        ok = stream.writeArray(v, 3) == 3 && ok;
      }
    }

    progressMarker += itemCount;
  }
  else if (progressMarker < _normalsCount + _vertexCount + _indexCount)
  {
    // Send indices.
    const int maxPacketIndices = MeshResource::estimateTransferCount(4, 0, sizeof(DataMessage) + localByteOverhead);
    offset = progressMarker - _normalsCount - _vertexCount;
    itemCount = uint32_t(std::min<uint32_t>(_indexCount - offset, maxPacketIndices));

    sendType = SDT_Indices; // Sending indices.
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;

    const unsigned *idx = _indices + offset;
    ok = stream.writeArray(idx, itemCount) == itemCount && ok;
    progressMarker += itemCount;
  }
  else if (_vertexCount == 0 && _indexCount == 0)
  {
    // Won't have written anything with zero vertex/index counts. Write zeros to
    // ensure a well formed message.
    offset = itemCount = 0;
    sendType = SDT_Vertices; // Sending vertices.
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;
  }

  if (!ok)
  {
    return -1;
  }
  // Return 1 while there are more triangles to process.
  return (progressMarker < _normalsCount + _vertexCount + _indexCount) ? 1 : 0;
}


bool MeshShape::readCreate(PacketReader &stream)
{
  if (!Shape::readCreate(stream))
  {
    return false;
  }

  uint32_t vertexCount = 0;
  uint32_t indexCount = 0;
  uint8_t drawType = 0;
  bool ok = true;

  ok = ok && stream.readElement(vertexCount) == sizeof(vertexCount);
  ok = ok && stream.readElement(indexCount) == sizeof(indexCount);

  if (ok)
  {
    if (!_ownPointers)
    {
      _vertices = nullptr;
      _indices = nullptr;
      _vertexCount = _indexCount = 0;
    }

    _ownPointers = true;
    if (_vertexCount < vertexCount || _vertexStride != 3)
    {
      freeVertices(_vertices);
      _vertices = allocateVertices(vertexCount);
      _vertexStride = 3;
    }

    if (_indexCount < indexCount)
    {
      freeIndices(_indices);
      _indices = allocateIndices(indexCount);
    }

    _vertexCount = vertexCount;
    _indexCount = indexCount;
  }

  if (_ownNormals)
  {
    freeVertices(_normals);
  }

  // Normals may or may not come. We find out in writeData().
  // _normalCount will either be 1 (uniform normals) or match _vertexCount.
  // Depends on SendDataType in readData()
  _normals = nullptr;
  _normalsCount = 0;
  _ownNormals = false;

  ok = ok && stream.readElement(drawType) == sizeof(drawType);
  _drawType = (DrawType)drawType;

  return ok;
}


bool MeshShape::readData(PacketReader &stream)
{
  DataMessage msg;
  uint32_t offset = 0;
  uint32_t itemCount = 0;
  uint16_t dataType = 0;
  bool ok = true;

  ok = ok && msg.read(stream);

  ok = ok && stream.readElement(dataType) == sizeof(dataType);
  ok = ok && stream.readElement(offset) == sizeof(offset);
  ok = ok && stream.readElement(itemCount) == sizeof(itemCount);

  if (dataType == SDT_Vertices)
  {
    ok = ok && _ownPointers;
    if (_ownPointers)
    {
      if (itemCount + offset > _vertexCount)
      {
        freeVertices(_vertices);
        _vertices = allocateVertices(itemCount + offset);
        _vertexCount = itemCount + offset;
      }

      // FIXME: resolve ownership better. readData was a retrofit.
      float *vertices = const_cast<float *>(_vertices + offset * _vertexStride);
      ok = ok && stream.readArray(vertices, _vertexStride * itemCount) == _vertexStride * itemCount;
    }
  }
  else if (dataType == SDT_Indices)
  {
    ok = ok && _ownPointers;
    if (_ownPointers)
    {
      if (itemCount + offset > _indexCount)
      {
        freeIndices(_indices);
        _indices = allocateIndices(itemCount + offset);
        _indexCount = itemCount + offset;
      }

      // FIXME: resolve ownership better. readData was a retrofit.
      unsigned *indices = const_cast<unsigned *>(_indices + offset);
      ok = ok && stream.readArray(indices, itemCount) == itemCount;
    }
    else
    {
      ok = false;
    }
  }
  else if (dataType == SDT_Normals || dataType == SDT_UniformNormal)
  {
    unsigned requiredNormals = (dataType == SDT_UniformNormal) ? 1 : _vertexCount;;
    if (_normalsCount < requiredNormals || !_ownNormals)
    {
      if (_ownNormals)
      {
        freeVertices(_normals);
      }

      _normalsCount = requiredNormals;
      _ownNormals = true;
      _normals = allocateVertices(_normalsCount);
      _normalsStride = 3;
    }

    if (itemCount + offset <= _normalsCount)
    {
      // FIXME: resolve ownership better. readData was a retrofit.
      float *normals = const_cast<float *>(_normals + _normalsStride * offset);
      ok = ok && stream.readArray(normals, _normalsStride * itemCount) == _normalsStride * itemCount;
    }
    else
    {
      ok = false;
    }
  }

  return ok;
}


Shape *MeshShape::clone() const
{
  MeshShape *triangles = new MeshShape();
  onClone(triangles);
  triangles->_data = _data;
  return triangles;
}


void MeshShape::onClone(MeshShape *copy) const
{
  Shape::onClone(copy);
  copy->_vertices = nullptr;
  copy->_indices = nullptr;
  copy->_normals = nullptr;
  copy->_vertexCount = _vertexCount;
  copy->_normalsCount = _normalsCount;
  copy->_indexCount = _indexCount;
  copy->_vertexStride = 3;
  copy->_normalsStride = 3;
  copy->_drawType = _drawType;
  copy->_ownPointers = true;
  copy->_ownNormals = true;
  if (_vertexCount)
  {
    float *vertices = copy->allocateVertices(_vertexCount);
    if (_vertexStride == 3)
    {
      memcpy(vertices, _vertices, sizeof(*vertices) * _vertexCount * 3);
    }
    else
    {
      const float *src = _vertices;
      float *dst = vertices;
      for (unsigned i = 0; i < _vertexCount; ++i)
      {
        dst[0] = src[0];
        dst[1] = src[1];
        dst[2] = src[2];
        src += _vertexStride;
        dst += 3;
      }
    }
    copy->_vertices = vertices;
  }

  if (_indexCount)
  {
    unsigned *indices = copy->allocateIndices(_indexCount);
    memcpy(indices, _indices, sizeof(*indices) * _indexCount);
    copy->_indices = indices;
  }

  if (_normalsCount)
  {
    float *normals = copy->allocateVertices(_normalsCount);
    if (_normalsStride == 3)
    {
      memcpy(normals, _normals, sizeof(*normals) * _normalsCount * 3);
    }
    else
    {
      const float *src = _normals;
      float *dst = normals;
      for (unsigned i = 0; i < _normalsCount; ++i)
      {
        dst[0] = src[0];
        dst[1] = src[1];
        dst[2] = src[2];
        src += _normalsStride;
        dst += 3;
      }
    }
    copy->_normals = normals;
  }
}


float *MeshShape::allocateVertices(unsigned count)
{
  // Hidden to avoid allocation resource clashes.
  return new float[count * 3];
}


void MeshShape::freeVertices(const float *&vertices)
{
  // Hidden to deallocate from the same resources.
  delete[] vertices;
  vertices = nullptr;
}


unsigned *MeshShape::allocateIndices(unsigned count)
{
  // Hidden to avoid allocation resource clashes.
  return new unsigned[count];
}


void MeshShape::freeIndices(const unsigned *&indices)
{
  // Hidden to deallocate from the same resources.
  delete[] indices;
  indices = nullptr;
}
