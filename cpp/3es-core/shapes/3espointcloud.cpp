//
// author: Kazys Stepanas
//
#include "3espointcloud.h"

#include "3esspinlock.h"

#include "3esmeshmessages.h"

#include <algorithm>
#include <cstring>
#include <mutex>

using namespace tes;

namespace tes
{
  struct PointCloudImp
  {
    SpinLock lock;
    Vector3f *vertices;
    Vector3f *normals;
    Colour *colours;
    unsigned vertexCount;
    unsigned capacity;
    uint32_t id;
    unsigned references;

    inline PointCloudImp(uint32_t id)
      : vertices(nullptr)
      , normals(nullptr)
      , colours(nullptr)
      , vertexCount(0)
      , capacity(0)
      , id(id)
      , references(1)
    {
    }


    inline ~PointCloudImp()
    {
      delete [] vertices;
      delete [] normals;
      delete [] colours;
    }


    inline PointCloudImp *clone() const
    {
      PointCloudImp *copy = new PointCloudImp(this->id);
      copy->vertexCount = copy->capacity = vertexCount;
      copy->id = id;

      copy->vertices = (vertices && vertexCount) ? new Vector3f[vertexCount] : nullptr;
      memcpy(copy->vertices, vertices, sizeof(*vertices) * vertexCount);

      copy->normals = (normals && vertexCount) ? new Vector3f[vertexCount] : nullptr;
      memcpy(copy->normals, normals, sizeof(*normals) * vertexCount);

      copy->colours = (colours && vertexCount) ? new Colour[vertexCount] : nullptr;
      memcpy(copy->colours, colours, sizeof(*colours) * vertexCount);

      copy->references = 1;
      return copy;
    }
  };
}

PointCloud::PointCloud(const PointCloud &other)
  : _imp(other._imp)
{
  std::unique_lock<SpinLock> guard(_imp->lock);
  ++_imp->references;
}


PointCloud::PointCloud(uint32_t id)
  : _imp(new PointCloudImp(id))
{
}


PointCloud::~PointCloud()
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


uint32_t PointCloud::id() const
{
  return _imp->id;
}


PointCloud *PointCloud::clone() const
{
  PointCloud *copy = new PointCloud(*this);
  return copy;
}


Matrix4f PointCloud::transform() const
{
  return Matrix4f::identity;
}


uint32_t PointCloud::tint() const
{
  return 0xffffffffu;
}


uint8_t PointCloud::drawType(int stream) const
{
  return DtPoints;
}


void PointCloud::reserve(unsigned size)
{
  if (_imp->capacity < size)
  {
    setCapacity(size);
  }
}


void PointCloud::resize(unsigned count)
{
  if (_imp->capacity < count)
  {
    reserve(count);
  }

  _imp->vertexCount = count;
}


void PointCloud::squeeze()
{
  if (_imp->capacity > _imp->vertexCount)
  {
    setCapacity(_imp->vertexCount);
  }
}


unsigned PointCloud::capacity() const
{
  return _imp->capacity;
}


unsigned PointCloud::vertexCount(int stream) const
{
  return _imp->vertexCount;
}


const float *PointCloud::vertices(unsigned &stride, int stream) const
{
  stride = sizeof(Vector3f);
  return (_imp->vertices) ? &_imp->vertices->x : nullptr;
}


const Vector3f *PointCloud::vertices() const
{
  return _imp->vertices;
}


unsigned PointCloud::indexCount(int stream) const
{
  return 0;
}


const uint8_t *PointCloud::indices(unsigned &stride, unsigned &width, int stream) const
{
  return nullptr;
}


const float *PointCloud::normals(unsigned &stride, int stream) const
{
  stride = sizeof(Vector3f);
  return (_imp->normals) ? &_imp->normals->x : nullptr;
}


const Vector3f *PointCloud::normals() const
{
  return _imp->normals;
}


const uint32_t *PointCloud::colours(unsigned &stride, int stream) const
{
  stride = sizeof(Colour);
  return (_imp->colours) ? &_imp->colours->c : nullptr;
}


const Colour *PointCloud::colours() const
{
  return _imp->colours;
}


const float *PointCloud::uvs(unsigned &, int) const
{
  return nullptr;
}


void PointCloud::addPoints(const Vector3f *points, unsigned count)
{
  if (count)
  {
    copyOnWrite();
    unsigned initial = _imp->vertexCount;
    resize(_imp->vertexCount + count);
    memcpy(_imp->vertices + initial, points, sizeof(*points) * count);

    // Initialise other data
    for (unsigned i = initial; i < _imp->vertexCount; ++i)
    {
      _imp->normals[i] = Vector3f::zero;
    }

    const Colour c = Colour::Colours[Colour::White];
    for (unsigned i = initial; i < _imp->vertexCount; ++i)
    {
      _imp->colours[i] = c;
    }
  }
}


void PointCloud::addPoints(const Vector3f *points, const Vector3f *normals, unsigned count)
{
  if (count)
  {
    copyOnWrite();
    unsigned initial = _imp->vertexCount;
    resize(_imp->vertexCount + count);
    memcpy(_imp->vertices + initial, points, sizeof(*points) * count);
    memcpy(_imp->normals + initial, normals, sizeof(*normals) * count);

    // Initialise other data
    const Colour c = Colour::Colours[Colour::White];
    for (unsigned i = initial; i < _imp->vertexCount; ++i)
    {
      _imp->colours[i] = c;
    }
  }
}


void PointCloud::addPoints(const Vector3f *points, const Vector3f *normals, const Colour *colours, unsigned count)
{
  if (count)
  {
    copyOnWrite();
    unsigned initial = _imp->vertexCount;
    resize(_imp->vertexCount + count);
    memcpy(_imp->vertices + initial, points, sizeof(*points) * count);
    memcpy(_imp->normals + initial, normals, sizeof(*normals) * count);
    memcpy(_imp->colours + initial, colours, sizeof(*colours) * count);
  }
}


void PointCloud::setNormal(unsigned index, const Vector3f &normal)
{
  if (index < _imp->vertexCount)
  {
    copyOnWrite();
    _imp->normals[index] = normal;
  }
}


void PointCloud::setColour(unsigned index, const Colour &colour)
{
  if (index < _imp->vertexCount)
  {
    copyOnWrite();
    _imp->colours[index] = colour;
  }
}


void PointCloud::setPoints(unsigned index, const Vector3f *points, unsigned count)
{
  if (index >= _imp->vertexCount)
  {
    return;
  }

  if (index + count > _imp->vertexCount)
  {
    count = index + count - _imp->vertexCount;
  }

  if (!count)
  {
    return;
  }

  copyOnWrite();
  memcpy(_imp->vertices + index, points, sizeof(*points) * count);
}


void PointCloud::setPoints(unsigned index, const Vector3f *points, const Vector3f *normals, unsigned count)
{
  if (index >= _imp->vertexCount)
  {
    return;
  }

  if (index + count > _imp->vertexCount)
  {
    count = index + count - _imp->vertexCount;
  }

  if (!count)
  {
    return;
  }

  copyOnWrite();
  memcpy(_imp->vertices + index, points, sizeof(*points) * count);
  memcpy(_imp->normals + index, normals, sizeof(*normals) * count);
}


void PointCloud::setPoints(unsigned index, const Vector3f *points, const Vector3f *normals, const Colour *colours, unsigned count)
{
  if (index >= _imp->vertexCount)
  {
    return;
  }

  if (index + count > _imp->vertexCount)
  {
    count = index + count - _imp->vertexCount;
  }

  if (!count)
  {
    return;
  }

  copyOnWrite();
  memcpy(_imp->vertices + index, points, sizeof(*points) * count);
  memcpy(_imp->normals + index, normals, sizeof(*normals) * count);
  memcpy(_imp->colours + index, colours, sizeof(*colours) * count);
}


void PointCloud::setCapacity(unsigned size)
{
  if (_imp->capacity == size)
  {
    // Already at the requested size.
    return;
  }

  copyOnWrite();
  // Check capacity again. The copyOnWrite() may have set them to be the same.
  if (_imp->capacity != size)
  {
    if (!size)
    {
      delete [] _imp->vertices;
      delete [] _imp->normals;
      delete [] _imp->colours;
      _imp->vertices = _imp->normals = nullptr;
      _imp->colours = nullptr;
      _imp->capacity = 0;
      _imp->vertexCount = 0;
      return;
    }

    Vector3f *points = new Vector3f[size];
    Vector3f *normals = new Vector3f[size];
    Colour *colours = new Colour[size];

    unsigned vertexCount = std::min(_imp->vertexCount, size);
    if (_imp->capacity)
    {
      // Copy existing data.
      if (vertexCount)
      {
        memcpy(points, _imp->vertices, sizeof(*points) * vertexCount);
        memcpy(normals, _imp->normals, sizeof(*normals) * vertexCount);
        memcpy(colours, _imp->colours, sizeof(*colours) * vertexCount);
      }

      delete [] _imp->vertices;
      delete [] _imp->normals;
      delete [] _imp->colours;
    }

    _imp->vertices = points;
    _imp->normals = normals;
    _imp->colours = colours;
    _imp->capacity = size;
    _imp->vertexCount = vertexCount;
  }
}


void PointCloud::copyOnWrite()
{
  std::unique_lock<SpinLock> guard(_imp->lock);
  if (_imp->references > 1)
  {
    --_imp->references;
    _imp = _imp->clone();
  }
}
