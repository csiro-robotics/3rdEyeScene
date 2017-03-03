//
// author: Kazys Stepanas
//
#ifndef _3ESMESHPLACEHOLDER_H_
#define _3ESMESHPLACEHOLDER_H_

#include "3es-core.h"
#include "3esmeshresource.h"

namespace tes
{
  /// A placeholder for a mesh resource, carrying only a mesh ID. All other fields
  /// and data manipulations are null and void.
  ///
  /// This can be use to reference an existing mesh resource, primarily when using the
  /// macro interface to release a mesh set.
  class _3es_coreAPI MeshPlaceholder : public MeshResource
  {
  public:
    /// Create a placeholder mesh resource for the given @p id.
    /// @param id The ID this placeholder publishes.
    MeshPlaceholder(uint32_t id);

    /// Changes the ID the placeholder publishes. Use with care.
    /// @param newId The new value for @c id().
    void setId(uint32_t newId);

    /// Returns the ID the placeholder was constructed with.
    uint32_t id() const override;

#ifndef DOXYGEN_SHOULD_SKIP_THIS
    Matrix4f transform() const override;
    uint32_t tint() const override;
    uint8_t drawType(int stream = 0) const override;
    unsigned vertexCount(int stream = 0) const override;
    unsigned indexCount(int stream = 0) const override;
    const float *vertices(unsigned &stride, int stream = 0) const override;
    const uint8_t *indices(unsigned &stride, unsigned &width, int stream = 0) const override;
    const float *normals(unsigned &stride, int stream = 0) const override;
    const float *uvs(unsigned &stride, int stream = 0) const override;
    const uint32_t *colours(unsigned &stride, int stream = 0) const override;
#endif // !DOXYGEN_SHOULD_SKIP_THIS

  private:
    uint32_t _id;
  };
}

#endif // _3ESMESHPLACEHOLDER_H_
