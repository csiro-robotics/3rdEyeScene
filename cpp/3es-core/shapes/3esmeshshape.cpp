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

namespace
{
  // Helper for automating data sending.
  struct DataPhase
  {
    // The SendDataType.
    uint16_t type;
    // Number of things to send. May be zero.
    unsigned itemCount;
    // Data pointer. May be null with zero itemCount.
    const uint8_t *dataSrc;
    // Byte stride between elements.
    size_t dataStrideBytes;
    // Base data item size, requiring endian swap.
    // See tupleSize.
    size_t dataSizeByte;
    // Number of data items in each stride. tupleSize * dataSizeBytes must be <= dataStrideBytes.
    //
    // Usage by example.
    // For a Vector3 data type of 3 packed floats:
    // - tupleSize = 3
    // - dataSizeBytes = sizeof(float)
    // - dataStrideBytes = tupleSize * dataSizeBytes = 12
    //
    // For a Vector3 data type aligned to 16 bytes (3 floats):
    // - tupleSize = 3
    // - dataSizeBytes = sizeof(float)
    // - dataStrideBytes = 16
    int tupleSize;
  };


  unsigned readElements(PacketReader &stream, unsigned offset, unsigned itemCount,
                        uint8_t *dstPtr, size_t elementSizeBytes, unsigned elementCount,
                        unsigned tupleSize = 1)
  {
    if (offset > elementCount)
    {
      return ~0u;
    }

    if (itemCount == 0)
    {
      return offset + itemCount;
    }

    if (offset + itemCount > elementCount)
    {
      itemCount = elementCount - itemCount;
    }

    offset *= tupleSize;
    itemCount *= tupleSize;

    uint8_t *dst = const_cast<uint8_t *>(dstPtr);
    dst += offset * elementSizeBytes;
    size_t readCount = stream.readArray(dst, elementSizeBytes, itemCount);
    if (readCount != itemCount)
    {
      return ~0u;
    }

    return unsigned((readCount + offset) / tupleSize);
  }
}

MeshShape::~MeshShape()
{
  if (_ownPointers)
  {
    freeVertices(_vertices);
    freeIndices(_indices);
    delete [] _colours;
  }
  if (_ownNormals)
  {
    freeVertices(_normals);
  }
}


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


MeshShape &MeshShape::setColours(const uint32_t *colours)
{
  if (_ownPointers)
  {
    if (colours)
    {
      if (vertexCount())
      {
        delete _colours;
        uint32_t *newColours = new uint32_t[vertexCount()];
        _colours = newColours;
        memcpy(newColours, colours, sizeof(*colours) * vertexCount());
      }
    }
    else
    {
      delete _colours;
      _colours = nullptr;
    }
  }
  else
  {
    _colours = colours;
  }

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

  uint32_t *colours = nullptr;
  if (_colours)
  {
    colours = new uint32_t[_indexCount];
    uint32_t *dst = colours;
    for (unsigned i = 0; i < _indexCount; ++i)
    {
      *dst++ = _colours[_indices[i]];
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
  _vertexStride = 3;
  _normals = normals;
  _normalsCount = (_normals) ? _indexCount : 0;
  _normalsStride = 3;
  _colours = colours;
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
  const unsigned colourCount = (_colours) ? _vertexCount : 0;
  msg.id = _data.id;
  stream.reset(routingId(), DataMessage::MessageId);
  ok = msg.write(stream);

  // Send vertices or indices?
  uint32_t offset;
  uint32_t itemCount;
  uint16_t sendType;

  // Resolve what we are currently sending.
  int phaseIndex = 0;
  unsigned previousPhaseOffset = 0;

  // Order to send data in and information required to automate sending.
  const uint16_t normalsSendType = (_normalsCount == 1) ? SDT_UniformNormal : SDT_Normals;
  const DataPhase phases[] =
  {
    { normalsSendType, _normalsCount, (const uint8_t *)_normals, _normalsStride * sizeof(*_normals), sizeof(*_normals), 3 },
    { SDT_Colours, (_colours) ? _vertexCount : 0, (const uint8_t *)_colours, sizeof(*_colours), sizeof(*_colours), 1 },
    { SDT_Vertices, _vertexCount, (const uint8_t *)_vertices, _vertexStride * sizeof(*_vertices), sizeof(*_vertices), 3 },
    { SDT_Indices, _indexCount, (const uint8_t *)_indices, sizeof(*_indices), sizeof(*_indices), 1 }
  };

  // While progressMarker is greater than or equal to the sum of the previous phase counts and the current phase count.
  // Also terminate of out of phases.
  while (phaseIndex < sizeof(phases) / sizeof(phases[0]) &&
         progressMarker >= previousPhaseOffset + phases[phaseIndex].itemCount)
  {
    previousPhaseOffset += phases[phaseIndex].itemCount;
    ++phaseIndex;
  }

  bool done = false;
  // Check if we have anything to send.
  if (phaseIndex < sizeof(phases) / sizeof(phases[0]))
  {
    const DataPhase &phase = phases[phaseIndex];
    // Send part of current phase.
    const int maxItemCout = MeshResource::estimateTransferCount(phase.dataSizeByte * phase.tupleSize, 0, sizeof(DataMessage) + localByteOverhead);
    offset = progressMarker - previousPhaseOffset;
    itemCount = uint32_t(std::min<uint32_t>(phase.itemCount - offset, maxItemCout));

    sendType = phase.type | SDT_ExpectEnd;
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;

    const uint8_t *src = phase.dataSrc + offset * phase.dataStrideBytes;
    if (phase.dataStrideBytes == phase.dataSizeByte * phase.tupleSize)
    {
      ok = stream.writeArray(src, phase.dataSizeByte, itemCount * phase.tupleSize) == itemCount * phase.tupleSize && ok;
    }
    else
    {
      for (unsigned i = 0; i < itemCount; ++i, src += phase.dataStrideBytes)
      {
        ok = stream.writeArray(src, phase.dataSizeByte, phase.tupleSize) == phase.tupleSize && ok;
      }
    }

    progressMarker += itemCount;
  }
  else
  {
    // Either all done or no data to send.
    // In the latter case, we need to populate the message anyway.
    offset = itemCount = 0;
    sendType = SDT_ExpectEnd | SDT_End;
    ok = stream.writeElement(sendType) == sizeof(sendType) && ok;
    ok = stream.writeElement(offset) == sizeof(offset) && ok;
    ok = stream.writeElement(itemCount) == sizeof(itemCount) && ok;

    done = true;
  }

  if (!ok)
  {
    return -1;
  }
  // Return 1 while there is more data to process.
  return (!done) ? 1 : 0;
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
      _colours = nullptr;
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

  // Record and mask out end flags.
  uint16_t endFlags = (dataType & (SDT_ExpectEnd | SDT_End));
  dataType &= ~endFlags;

  // Can only read if we own the pointers.
  if (!_ownPointers)
  {
    return false;
  }

  // FIXME: resolve the 'const' pointer casting. Reading was a retrofit.
  bool complete = false;
  unsigned endReadCount = 0;
  switch (dataType)
  {
  case SDT_Vertices:
    endReadCount = readElements(stream, offset, itemCount, (uint8_t *)_vertices, sizeof(*_vertices), _vertexCount, 3);
    ok = ok && endReadCount != ~0u;

    // Expect end marker.
    if (endFlags & SDT_End)
    {
      // Done.
      complete = true;
    }

    // Check for completion.
    if (!(endFlags & SDT_ExpectEnd))
    {
      complete = endReadCount == _vertexCount;
    }
    break;

  case SDT_Indices:
    endReadCount = readElements(stream, offset, itemCount, (uint8_t *)_indices, sizeof(*_indices), _indexCount);
    ok = ok && endReadCount != ~0u;
    break;

    // Normals handled together.
  case SDT_Normals:
  case SDT_UniformNormal:
    if (!_normals)
    {
      _normalsCount = (dataType == SDT_Normals) ? _vertexCount : 1;
      _normalsStride = 3;
      if (_normalsCount)
      {
        _normals = allocateVertices(_normalsCount);
        _ownNormals = true;
      }
    }

    endReadCount = readElements(stream, offset, itemCount, (uint8_t *)_normals, sizeof(*_normals), _normalsCount, 3);
    ok = ok && endReadCount != ~0u;
    break;

  case SDT_Colours:
    if (!_colours && _vertexCount)
    {
      _colours = new uint32_t[_vertexCount];
    }

    endReadCount = readElements(stream, offset, itemCount, (uint8_t *)_colours, sizeof(*_colours), _vertexCount);
    ok = ok && endReadCount != ~0u;
    break;
  default:
    // Unknown data type.
    ok = false;
    break;
  }

  if (complete)
  {
    // Nothing in the test code.
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
