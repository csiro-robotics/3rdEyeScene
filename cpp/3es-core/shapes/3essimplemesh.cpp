//
// author: Kazys Stepanas
//
#include "3essimplemesh.h"

#include "3esspinlock.h"
#include "3esrotation.h"

#include <vector>
#include <mutex>

using namespace tes;

namespace tes
{
  struct UV
  {
    float u, v;
  };

  struct SimpleMeshImp
  {
    SpinLock lock;
    std::vector<Vector3f> vertices;
    std::vector<uint32_t> indices;
    std::vector<uint32_t> colours;
    std::vector<Vector3f> normals;
    std::vector<UV> uvs;
    Matrix4f transform;
    uint32_t id;
    uint32_t tint;
    unsigned components;
    unsigned references;
    DrawType drawType;

    inline SimpleMeshImp(unsigned components)
      : id(0)
      , tint(0xffffffffu)
      , components(components)
      , references(1)
      , drawType(DtTriangles)
    {
      transform = Matrix4f::identity;
    }


    inline SimpleMeshImp *clone() const
    {
      SimpleMeshImp *copy = new SimpleMeshImp(this->components);
      copy->vertices = vertices;
      copy->indices = indices;
      copy->colours = colours;
      copy->normals = normals;
      copy->uvs = uvs;
      copy->transform = transform;
      copy->tint = tint;
      copy->components = components;
      copy->drawType = drawType;
      copy->references = 1;
      return copy;
    }

    inline void clear(unsigned componentFlags)
    {
      clearArrays();
      transform = Matrix4f::identity;
      id = 0;
      tint = 0xffffffffu;
      components = componentFlags;
      drawType = DtTriangles;
    }

    inline void clearArrays()
    {
      // Should only be called if the reference count is 1.
      vertices.resize(0);
      indices.resize(0);
      colours.resize(0);
      normals.resize(0);
      uvs.resize(0);
    }
  };
}


SimpleMesh::SimpleMesh(uint32_t id, const IntArg &vertexCount, const IntArg &indexCount, DrawType drawType, unsigned components)
  : _imp(new SimpleMeshImp(components))
{
  _imp->id = id;
  _imp->drawType = drawType;
  _imp->transform = Matrix4f::identity;
  _imp->tint = 0xffffffff;

  if (vertexCount)
  {
    setVertexCount(vertexCount);
  }

  if (indexCount && (components & Index))
  {
    setIndexCount(indexCount);
  }
}


SimpleMesh::SimpleMesh(const SimpleMesh &other)
  : _imp(other._imp)
{
  std::unique_lock<SpinLock> guard(_imp->lock);
  ++_imp->references;
}


SimpleMesh::~SimpleMesh()
{
  std::unique_lock<SpinLock> guard(_imp->lock);
  if (_imp->references == 1)
  {
    // Unlock for delete.
    guard.unlock();
    delete _imp;
  }
  else
  {
    --_imp->references;
  }
}


void SimpleMesh::clear()
{
  // Note: _imp may change before leaving this function, but the guard will hold
  // a reference to the correct lock.
  std::unique_lock<SpinLock> guard(_imp->lock);
  if (_imp->references == 1)
  {
    _imp->clear(Vertex | Index);
  }
  else
  {
    --_imp->references;
    _imp = new SimpleMeshImp(Vertex | Index);
  }
}


void SimpleMesh::clearData()
{
  // Note: _imp may change before leaving this function, but the guard will hold
  // a reference to the correct lock.
  std::unique_lock<SpinLock> guard(_imp->lock);
  if (_imp->references == 1)
  {
    _imp->clearArrays();
  }
  else
  {
    SimpleMeshImp *old = _imp;
    --_imp->references;
    _imp = new SimpleMeshImp(Vertex | Index);
    *_imp = *old;
    _imp->references = 1;
    _imp->clearArrays();
  }
}


uint32_t SimpleMesh::id() const
{
  return _imp->id;
}


Matrix4f SimpleMesh::transform() const
{
  return _imp->transform;
}


void SimpleMesh::setTransform(const Matrix4f &transform)
{
  copyOnWrite();
  _imp->transform = transform;
}


uint32_t SimpleMesh::tint() const
{
  return _imp->tint;
}


void SimpleMesh::setTint(uint32_t tint)
{
  copyOnWrite();
  _imp->tint = tint;
}


 SimpleMesh *SimpleMesh::clone() const
 {
   return new SimpleMesh(*this);
 }


uint8_t SimpleMesh::drawType(int /*stream*/) const
{
  return _imp->drawType;
}


DrawType SimpleMesh::getDrawType() const
{
  return _imp->drawType;
}


void SimpleMesh::setDrawType(DrawType type)
{
  copyOnWrite();
  _imp->drawType = type;
}


unsigned SimpleMesh::components() const
{
  return _imp->components;
}


void SimpleMesh::setComponents(unsigned comps)
{
  copyOnWrite();
  _imp->components = comps | Vertex;
  // Fix up discrepencies.
  if (!(_imp->components & Index) && !_imp->indices.empty())
  {
    _imp->indices.resize(0);
  }

  if ((_imp->components & Colour) && _imp->colours.empty())
  {
    _imp->colours.resize(_imp->vertices.size());
  }
  else if (!(_imp->components & Colour) && !_imp->colours.empty())
  {
    _imp->colours.resize(0);
  }

  if ((_imp->components & Normal) && _imp->normals.empty())
  {
    _imp->normals.resize(_imp->vertices.size());
  }
  else if (!(_imp->components & Normal) && !_imp->normals.empty())
  {
    _imp->normals.resize(0);
  }

  if ((_imp->components & Uv) && _imp->uvs.empty())
  {
    _imp->uvs.resize(_imp->vertices.size());
  }
  else if (!(_imp->components & Uv) && !_imp->uvs.empty())
  {
    _imp->uvs.resize(0);
  }
}


unsigned SimpleMesh::vertexCount() const
{
  return unsigned(_imp->vertices.size());
}


unsigned SimpleMesh::vertexCount(int stream) const
{
  if (stream == 0)
  {
    return unsigned(_imp->vertices.size());
  }
  return 0;
}


void SimpleMesh::setVertexCount(const IntArg &count)
{
  copyOnWrite();
  _imp->vertices.resize(count);
  if ((_imp->components & Colour))
  {
    _imp->colours.resize(_imp->vertices.size());
  }

  if ((_imp->components & Normal))
  {
    _imp->normals.resize(_imp->vertices.size());
  }

  if ((_imp->components & Uv))
  {
    _imp->uvs.resize(_imp->vertices.size());
  }
}


void SimpleMesh::reserveVertexCount(const IntArg &count)
{
  copyOnWrite();
  _imp->vertices.reserve(count);
}


unsigned SimpleMesh::addVertices(const Vector3f *v, const IntArg &count)
{
  copyOnWrite();
  size_t offset = _imp->vertices.size();
  setVertexCount(unsigned(_imp->vertices.size() + count.u()));
  for (unsigned i = 0; i < count; ++i)
  {
    _imp->vertices[offset + i] = v[i];
  }
  return unsigned(offset);
}


unsigned SimpleMesh::setVertices(const IntArg &at, const Vector3f *v, const IntArg &count)
{
  copyOnWrite();
  unsigned set = 0;
  for (unsigned i = at.u(); i < at.u() + count.u() && i < _imp->vertices.size(); ++i)
  {
    _imp->vertices[i] = v[i - at.u()];
    ++set;
  }
  return set;
}


const Vector3f *SimpleMesh::vertices() const
{
  return _imp->vertices.data();
}


const float *SimpleMesh::vertices(unsigned &stride, int stream) const
{
  stride = sizeof(Vector3f);
  if (!stream && !_imp->vertices.empty())
  {
    return &_imp->vertices[0].x;
  }
  return nullptr;
}


unsigned SimpleMesh::indexCount() const
{
  return unsigned(_imp->indices.size());
}


unsigned SimpleMesh::indexCount(int stream) const
{
  if (!stream && (_imp->components & Index) && !_imp->indices.empty())
  {
    return unsigned(_imp->indices.size());
  }
  return 0;
}


void SimpleMesh::setIndexCount(const IntArg &count)
{
  copyOnWrite();
  _imp->indices.resize(count);
  if (count)
  {
    _imp->components |= Index;
  }
}


void SimpleMesh::reserveIndexCount(const IntArg &count)
{
  copyOnWrite();
  _imp->indices.reserve(count);
}


void SimpleMesh::addIndices(const uint32_t *idx, const IntArg &count)
{
  copyOnWrite();
  size_t offset = _imp->indices.size();
  setIndexCount(unsigned(count.u() + offset));
  for (unsigned i = 0; i < count; ++i)
  {
    _imp->indices[i + offset] = idx[i];
  }
}


unsigned SimpleMesh::setIndices(const IntArg &at, const uint32_t *idx, const IntArg &count)
{
  copyOnWrite();
  unsigned set = 0;
  for (unsigned i = at; i < at.u() + count.u() && i < _imp->indices.size(); ++i)
  {
    _imp->indices[i] = idx[set++];
  }
  return set;
}


const uint32_t *SimpleMesh::indices() const
{
  return _imp->indices.data();
}


const uint8_t *SimpleMesh::indices(unsigned &stride, unsigned &width, int stream) const
{
  stride = width = sizeof(uint32_t);
  if (!stream && (_imp->components & Index) && !_imp->indices.empty())
  {
    return reinterpret_cast<const uint8_t *>(_imp->indices.data());
  }
  return nullptr;
}


unsigned SimpleMesh::setNormals(const IntArg &at, const Vector3f *n, const IntArg &count)
{
  copyOnWrite();
  unsigned set = 0;
  if (!(_imp->components & Normal) && _imp->vertices.size())
  {
    _imp->normals.resize(_imp->vertices.size());
    _imp->components |= Normal;
  }
  for (unsigned i = at; i < at.u() + count.u() && i < _imp->normals.size(); ++i)
  {
    _imp->normals[i] = n[set++];
  }
  return set;
}


const Vector3f *SimpleMesh::normals() const
{
  return _imp->normals.data();
}


const float *SimpleMesh::normals(unsigned &stride, int stream) const
{
  stride = sizeof(Vector3f);
  if (!stream && (_imp->components & Normal) && !_imp->normals.empty())
  {
    return &_imp->normals[0].x;
  }
  return nullptr;
}


unsigned SimpleMesh::setColours(const IntArg &at, const uint32_t *c, const IntArg &count)
{
  copyOnWrite();
  unsigned set = 0;
  if (!(_imp->components & Colour) && _imp->vertices.size())
  {
    _imp->colours.resize(_imp->vertices.size());
    _imp->components |= Colour;
  }
  for (unsigned i = at; i < at.u() + count.u() && i < _imp->colours.size(); ++i)
  {
    _imp->colours[i] = c[i - at.u()];
    ++set;
  }
  return set;
}


const uint32_t *SimpleMesh::colours() const
{
  return _imp->colours.data();
}


const uint32_t *SimpleMesh::colours(unsigned &stride, int stream) const
{
  stride = sizeof(uint32_t);
  if (!stream && (_imp->components & Colour) && !_imp->colours.empty())
  {
    return _imp->colours.data();
  }
  return nullptr;
}


unsigned SimpleMesh::setUvs(const IntArg &at, const float *uvs, const IntArg &count)
{
  copyOnWrite();
  unsigned set = 0;
  if (!(_imp->components & Uv) && _imp->vertices.size())
  {
    _imp->uvs.resize(_imp->vertices.size());
    _imp->components |= Uv;
  }
  for (unsigned i = at; i < at.u() + count.u() && i < _imp->uvs.size(); ++i)
  {
    const UV uv = { uvs[(i - at.u()) * 2 + 0], uvs[(i - at.u()) * 2 + 1] };
    _imp->uvs[i] = uv;
    ++set;
  }
  return set;
}


const float *SimpleMesh::uvs() const
{
  if (!_imp->uvs.empty())
  {
    return &_imp->uvs[0].u;
  }
  return nullptr;
}


const float *SimpleMesh::uvs(unsigned &stride, int stream) const
{
  stride = sizeof(UV);
  if (!stream && (_imp->components & Uv) && !_imp->uvs.empty())
  {
    return &_imp->uvs[0].u;
  }
  return nullptr;
}


void SimpleMesh::copyOnWrite()
{
  std::unique_lock<SpinLock> guard(_imp->lock);
  if (_imp->references > 1)
  {
    --_imp->references;
    _imp = _imp->clone();
  }
}


bool SimpleMesh::processCreate(const MeshCreateMessage &msg)
{
  copyOnWrite();
  _imp->id = msg.meshId;
  setVertexCount(msg.vertexCount);
  setIndexCount(msg.indexCount);
  setDrawType((DrawType)msg.drawType);

  Matrix4f transform = prsTransform(Vector3f(msg.attributes.position),
                                    Quaternionf(msg.attributes.rotation),
                                    Vector3f(msg.attributes.scale));

  setTransform(transform);
  setTint(msg.attributes.colour);
  return true;
}


bool SimpleMesh::processVertices(const MeshComponentMessage &msg, const float *vertices, unsigned vertexCount)
{
  static_assert(sizeof(Vector3f) == sizeof(float) * 3, "Vertex size mismatch");
  return setVertices(msg.offset, (const Vector3f *)vertices, vertexCount) == vertexCount;
}


bool SimpleMesh::processIndices(const MeshComponentMessage &msg, const uint8_t *indices, unsigned indexCount)
{
  // Expect 4 byte indices.
  return false;
}


bool SimpleMesh::processIndices(const MeshComponentMessage &msg, const uint16_t *indices, unsigned indexCount)
{
  // Expect 4 byte indices.
  return false;
}


bool SimpleMesh::processIndices(const MeshComponentMessage &msg, const uint32_t *indices, unsigned indexCount)
{
  // Accept 4 byte indices.
  return setIndices(msg.offset, indices, indexCount) == indexCount;
}


bool SimpleMesh::processColours(const MeshComponentMessage &msg, const uint32_t *colours, unsigned colourCount)
{
  return setColours(msg.offset, colours, colourCount) == colourCount;
}


bool SimpleMesh::processNormals(const MeshComponentMessage &msg, const float *normals, unsigned normalCount)
{
  static_assert(sizeof(Vector3f) == sizeof(float) * 3, "Normal size mismatch");
  return setNormals(msg.offset, (const Vector3f *)normals, normalCount) == normalCount;
}


bool SimpleMesh::processUVs(const MeshComponentMessage &msg, const float *uvs, unsigned uvCount)
{
  return setUvs(msg.offset, uvs, uvCount) == uvCount;
}


