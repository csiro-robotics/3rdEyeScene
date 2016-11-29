//
// author: Kazys Stepanas
//
#ifndef _3ESSIMPLEMESH_H_
#define _3ESSIMPLEMESH_H_

#include "3es-core.h"

#include "3esmeshset.h"
#include "3esmeshmessages.h"

namespace tes
{
  struct SimpleMeshImp;

  /// An encapsulated definition of a mesh. It manages all its own vertices,
  /// indices, etc.
  class _3es_coreAPI SimpleMesh : public MeshResource
  {
  public:
    /// Flags indicating which components are present. @c Vertex flag is
    /// always set. Other flags are optional, though @c Index is preferred.
    enum ComponentFlag
    {
      Vertex = (1 << 0), ///< Contains vertices. This flag is enforced.
      Index = (1 << 1),
      Colour = (1 << 2),
      Color = Colour,
      Normal = (1 << 3),
      Uv = (1 << 4)
    };

    SimpleMesh(uint32_t id, unsigned vertexCount = 0, unsigned indexCount = 0, DrawType drawType = DtTriangles,
               unsigned components = Vertex | Index);

  protected:
    SimpleMesh(const SimpleMesh &other);

  public:
    ~SimpleMesh();

    virtual void clear();

    virtual uint32_t id() const override;

    virtual Matrix4f transform() const override;
    void setTransform(const Matrix4f &transform);

    virtual uint32_t tint() const override;
    void setTint(uint32_t tint);

    /// Performs a shallow copy of this mesh. Note that any modification
    /// of the mesh data results in a copy of the existing data. Otherwise
    /// @c SimpleMesh objects can share their data.
    SimpleMesh *clone() const override;

  public:
    virtual uint8_t drawType(int stream) const override;
    DrawType getDrawType() const;
    void setDrawType(DrawType type);

    unsigned components() const;
    void setComponents(unsigned comps);

    void addComponents(unsigned components);

    unsigned vertexCount() const;
    virtual unsigned vertexCount(int stream) const override;
    void setVertexCount(unsigned count);
    void reserveVertexCount(unsigned count);

    inline unsigned addVertex(const Vector3f &v) { return addVertices(&v, 1u); }
    unsigned addVertices(const Vector3f *v, unsigned count);
    inline bool setVertex(unsigned at, const Vector3f &v) { return setVertices(at, &v, 1u) == 1u; }
    unsigned setVertices(unsigned at, const Vector3f *v, const unsigned count);
    const Vector3f *vertices() const;
    virtual const float *vertices(unsigned &stride, int stream = 0) const override;

    unsigned indexCount() const;
    virtual unsigned indexCount(int stream) const override;
    void setIndexCount(unsigned count);
    void reserveIndexCount(unsigned count);

    inline void addIndex(uint32_t i) { return addIndices(&i, 1u); }
    void addIndices(const uint32_t *idx, unsigned count);
    inline bool setIndex(unsigned at, uint32_t i) { return setIndices(at, &i, 1u) == 1u; }
    unsigned setIndices(unsigned at, const uint32_t *idx, unsigned count);
    const uint32_t *indices() const;
    virtual const uint8_t *indices(unsigned &stride, unsigned &width, int stream = 0) const override;

    inline bool setNormal(unsigned at, const Vector3f &n) { return setNormals(at, &n, 1u) == 1u; }
    unsigned setNormals(unsigned at, const Vector3f *n, const unsigned count);
    const Vector3f *normals() const;
    virtual const float *normals(unsigned &stride, int stream) const override;

    inline bool setColour(unsigned at, uint32_t c) { return setColours(at, &c, 1u) == 1u; }
    unsigned setColours(unsigned at, const uint32_t *c, unsigned count);
    const uint32_t *colours() const;
    virtual const uint32_t *colours(unsigned &stride, int stream) const override;

    inline bool setUv(unsigned at, float u, float v) { const float uv[2] = { u, v }; return setUvs(at, uv, 1u) == 1u; }
    unsigned setUvs(unsigned at, const float *uvs, const unsigned count);
    const float *uvs() const;
    virtual const float *uvs(unsigned &stride, int stream) const override;

  private:
    void copyOnWrite();

    SimpleMeshImp *_imp;
  };


  inline void SimpleMesh::addComponents(unsigned components)
  {
    setComponents(this->components() | components);
  }
}

#endif // _3ESSIMPLEMESH_H_

