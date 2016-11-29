//
// author: Kazys Stepanas
//
#include "3esmeshshape.h"

#include <3escoreutil.h>
#include <3espacketwriter.h>

#include <algorithm>

using namespace tes;


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
    *dst++ = _vertices[_indices[i] * 3 + 0];
    *dst++ = _vertices[_indices[i] * 3 + 1];
    *dst++ = _vertices[_indices[i] * 3 + 2];
  }

  if (_ownPointers)
  {
    freeVertices(_vertices);
    freeIndices(_indices);
  }

  _vertices = verts;
  _vertexCount = _indexCount;
  _indices = nullptr;
  _indexCount = 0;
  _ownPointers = true;

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
  msg.id = _data.id;
  stream.reset(routingId(), DataMessage::MessageId);
  ok = msg.write(stream);

  // Send vertices or indices?
  uint32_t offset;
  uint32_t itemCount;
  uint16_t sendType = 0;
  if (progressMarker < _vertexCount)
  {
    // Send vertices.
    // Approximate vertex limit per packet. Packet size maximum is 0xffff.
    // Take off a bit for overheads (256) and divide by 12 bytes per vertex.
    const unsigned maxPacketVertices = ((0xffff - 256) / 12);
    offset = progressMarker;
    itemCount = uint32_t(std::min(_vertexCount - offset, maxPacketVertices));

    sendType = 0; // Sending vertices.
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
  else if (progressMarker < _vertexCount + _indexCount)
  {
    // Send indices.
    // Approximate index limit per packet. Packet size maximum is 0xffff.
    // Take off a bit for overheads (256) and divide by 4 bytes per index.
    const unsigned maxPacketIndices = ((0xffff - 256) / 4);
    offset = progressMarker - _vertexCount;
    itemCount = uint32_t(std::min(_indexCount - offset, maxPacketIndices));

    sendType = 1; // Sending indices.
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
    sendType = 0; // Sending vertices.
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;
  }

  if (!ok)
  {
    return -1;
  }
  // Return 1 while there are more triangles to process.
  return (progressMarker < _vertexCount + _indexCount) ? 1 : 0;
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
  copy->_vertexCount = _vertexCount;
  copy->_indexCount = _indexCount;
  copy->_vertexStride = 3;
  copy->_drawType = _drawType;
  copy->_ownPointers = true;
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
}


float *MeshShape::allocateVertices(unsigned count)
{
  // Hidden to avoid allocation resource clashes.
  return new float[count * 3];
}


void MeshShape::freeVertices(const float *vertices)
{
  // Hidden to deallocate from the same resources.
  delete[] vertices;
}


unsigned *MeshShape::allocateIndices(unsigned count)
{
  // Hidden to avoid allocation resource clashes.
  return new unsigned[count];
}


void MeshShape::freeIndices(const unsigned *indices)
{
  // Hidden to deallocate from the same resources.
  delete[] indices;
}
