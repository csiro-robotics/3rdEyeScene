//
// author: Kazys Stepanas
//
#include "3esmeshplaceholder.h"

using namespace tes;

MeshPlaceholder::MeshPlaceholder(uint32_t id)
  : _id (id)
{
}


void MeshPlaceholder::setId(uint32_t newId)
{
  _id = newId;
}


uint32_t MeshPlaceholder::id() const
{
  return _id;
}


Matrix4f MeshPlaceholder::transform() const
{
  return Matrix4f::identity;
}


uint32_t MeshPlaceholder::tint() const
{
  return 0;
}


uint8_t MeshPlaceholder::drawType(int /* stream */) const
{
  return 0;
}


unsigned MeshPlaceholder::vertexCount(int /* stream */) const
{
  return 0;
}


unsigned MeshPlaceholder::indexCount(int /* stream */) const
{
  return 0;
}


const float *MeshPlaceholder::vertices(unsigned &stride, int /* stream */) const
{
  return nullptr;
}


const uint8_t *MeshPlaceholder::indices(unsigned &stride, unsigned &width, int /* stream */) const
{
  return nullptr;
}


const float *MeshPlaceholder::normals(unsigned &stride, int /* stream */) const
{
  return nullptr;
}


const float *MeshPlaceholder::uvs(unsigned &stride, int /* stream */) const
{
  return nullptr;
}


const uint32_t *MeshPlaceholder::colours(unsigned &stride, int /* stream */) const
{
  return nullptr;
}


Resource *MeshPlaceholder::clone() const
{
  return new MeshPlaceholder(_id);
}
