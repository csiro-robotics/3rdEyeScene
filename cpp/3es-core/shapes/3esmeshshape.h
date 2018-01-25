//
// author: Kazys Stepanas
//
#ifndef _3ESMESHSHAPE_H_
#define _3ESMESHSHAPE_H_

#include "3es-core.h"
#include "3esshape.h"
#include "3esmeshmessages.h"

namespace tes
{
  /// A @c Shape which uses vertices and indices to render.
  ///
  /// @c Use @c MeshSet for large data sets.
  class _3es_coreAPI MeshShape : public Shape
  {
  public:
    /// Codes for @c writeData(). Note: normals must be sent before completing vertices and indices. Best done first.
    enum SendDataType
    {
      SDT_Vertices,
      SDT_Indices,
      SDT_Normals,
      /// Sending a single normals for all vertices (voxel extents).
      SDT_UniformNormal
    };

    MeshShape();

    /// Transient triangle set constructor accepting an iterator and optional positioning.
    /// @param vertices Pointer to the vertex array. Must be at least 3 elements per vertex.
    /// @param vertexCount The number of vertices in @c vertices.
    /// @param vertexByteSize The size of a single vertex in @p vertices. Must be at least three floats (12).
    /// @param position Local to world positioning of the triangles. Defaults to the origin.
    /// @param rotation Local to world rotation of the triangles. Defaults to identity.
    /// @param scale Scaling for the triangles. Defaults to one.
    MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
              const V3Arg &position = V3Arg(0, 0, 0),
              const QuaternionArg &rotation = QuaternionArg(0, 0, 0, 1),
              const V3Arg &scale = V3Arg(1, 1, 1));

    /// Transient triangle set constructor accepting vertex and index iterators and optional positioning.
    /// @param vertices Pointer to the vertex array. Must be at least 3 elements per vertex.
    /// @param vertexCount The number of vertices in @c vertices.
    /// @param vertexByteSize The size of a single vertex in @p vertices. Must be at least three floats (12).
    /// @param position Local to world positioning of the triangles. Defaults to the origin.
    /// @param rotation Local to world rotation of the triangles. Defaults to identity.
    /// @param scale Scaling for the triangles. Defaults to one.
    MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
              const unsigned *indices, unsigned indexCount,
              const V3Arg &position = V3Arg(0, 0, 0),
              const QuaternionArg &rotation = QuaternionArg(0, 0, 0, 1),
              const V3Arg &scale = V3Arg(1, 1, 1));

    /// Persistent triangle constructor accepting an iterator and optional positioning.
    /// @param vertices Pointer to the vertex array. Must be at least 3 elements per vertex.
    /// @param vertexCount The number of vertices in @c vertices.
    /// @param vertexByteSize The size of a single vertex in @p vertices. Must be at least three floats (12).
    /// @param id Unique ID for the triangles. Must be non-zero to be persistent.
    /// @param position Local to world positioning of the triangles. Defaults to the origin.
    /// @param rotation Local to world rotation of the triangles. Defaults to identity.
    /// @param scale Scaling for the triangles. Defaults to one.
    MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
              uint32_t id,
              const V3Arg &position = V3Arg(0, 0, 0),
              const QuaternionArg &rotation = QuaternionArg(0, 0, 0, 1),
              const V3Arg &scale = V3Arg(1, 1, 1));

    /// Persistent triangle constructor accepting vertex and triangle iterators and optional positioning.
    /// @param vertices Pointer to the vertex array. Must be at least 3 elements per vertex.
    /// @param vertexCount The number of vertices in @c vertices.
    /// @param vertexByteSize The size of a single vertex in @p vertices. Must be at least three floats (12).
    /// @param id Unique ID for the triangles. Must be non-zero to be persistent.
    /// @param position Local to world positioning of the triangles. Defaults to the origin.
    /// @param rotation Local to world rotation of the triangles. Defaults to identity.
    /// @param scale Scaling for the triangles. Defaults to one.
    MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
              const unsigned *indices, unsigned indexCount, uint32_t id,
              const V3Arg &position = V3Arg(0, 0, 0),
              const QuaternionArg &rotation = QuaternionArg(0, 0, 0, 1),
              const V3Arg &scale = V3Arg(1, 1, 1));

    /// Persistent triangle constructor accepting an iterator and optional positioning.
    /// @param vertices Pointer to the vertex array. Must be at least 3 elements per vertex.
    /// @param vertexCount The number of vertices in @c vertices.
    /// @param vertexByteSize The size of a single vertex in @p vertices. Must be at least three floats (12).
    /// @param id Unique ID for the triangles. Must be non-zero to be persistent.
    /// @param category Categorisation of the triangles. For filtering.
    /// @param position Local to world positioning of the triangles. Defaults to the origin.
    /// @param rotation Local to world rotation of the triangles. Defaults to identity.
    /// @param scale Scaling for the triangles. Defaults to one.
    MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
              uint32_t id, uint16_t category,
              const V3Arg &position = V3Arg(0, 0, 0),
              const QuaternionArg &rotation = QuaternionArg(0, 0, 0, 1),
              const V3Arg &scale = V3Arg(1, 1, 1));

    /// Persistent triangle constructor accepting and triangle iterators and optional positioning.
    /// @param vertices Pointer to the vertex array. Must be at least 3 elements per vertex.
    /// @param vertexCount The number of vertices in @c vertices.
    /// @param vertexByteSize The size of a single vertex in @p vertices. Must be at least three floats (12).
    /// @param id Unique ID for the triangles. Must be non-zero to be persistent.
    /// @param category Categorisation of the triangles. For filtering.
    /// @param position Local to world positioning of the triangles. Defaults to the origin.
    /// @param rotation Local to world rotation of the triangles. Defaults to identity.
    /// @param scale Scaling for the triangles. Defaults to one.
    MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
              const unsigned *indices, unsigned indexCount,
              uint32_t id, uint16_t category,
              const V3Arg &position = V3Arg(0, 0, 0),
              const QuaternionArg &rotation = QuaternionArg(0, 0, 0, 1),
              const V3Arg &scale = V3Arg(1, 1, 1));

    /// Destructor.
    ~MeshShape();

    inline const char *type() const override { return "meshShape"; }

    /// Mark as complex to ensure @c writeData() is called.
    inline bool isComplex() const override { return true; }

    /// Calculate vertex normals in the viewer?
    bool calculateNormals() const;
    /// Should normals be calculated for the mesh by the viewer?
    /// @param calculate True to calculate vertex normals in the viewer.
    MeshShape &setCalculateNormals(bool calculate);

    /// Set (optional) mesh normals. The number of normal elements in @p normals
    /// must match the @p vertexCount.
    ///
    /// The normals array is copied if this object owns its vertex memory such
    /// as after calling @p expandVertices().
    ///
    /// Sets @c calculateNormals() to false.
    ///
    /// @param normals The normals array.
    /// @param normalByteSize the number of bytes between each element of @p normals.
    /// @return this
    MeshShape &setNormals(const float *normals, size_t normalByteSize);

    /// Sets a single normal to be shared by all vertices in the mesh.
    /// Sets @c calculateNormals() to false.
    /// @param normal The shared normal to set.
    /// @return this
    MeshShape &setUniformNormal(const Vector3f &normal);

    /// Expand the vertex set into a new block of memory.
    ///
    /// This is useful when indexing small primitive from a large set of vertices.
    /// The method allocates a new array of vertices, explicitly copying and unpacking
    /// the vertices by traversing the index array. This ensure only the indexed
    /// subset is present.
    ///
    /// Does nothing when the shape does not use indices.
    /// @return this
    MeshShape &expandVertices();

    inline unsigned vertexCount() const { return _vertexCount; }
    inline const float *vertices() const { return _vertices; }
    /// Vertex stride in float elements.
    inline size_t vertexStride() const { return _vertexStride; }
    inline size_t vertexByteStride() const { return _vertexStride * sizeof(float); }
    inline const float *normals() const { return _normals; }
    inline size_t normalsStride() const { return _normalsStride; }
    inline size_t normalsByteStride() const { return _normalsStride * sizeof(float); }
    inline size_t normalsCount() const { return _normalsCount; }
    inline unsigned indexCount() const { return _indexCount; }
    inline const unsigned *indices() const { return _indices; }
    inline DrawType drawType() const { return _drawType; }

    /// Writes the standard create message and appends mesh data.
    ///
    /// - Vertex count : uint32
    /// - Index count : uint32
    /// - Draw type : uint8
    /// @param stream The stream to write to.
    /// @return True on success.
    bool writeCreate(PacketWriter &stream) const override;
    int writeData(PacketWriter &stream, unsigned &progressMarker) const override;

    bool readCreate(PacketReader &stream) override;
    virtual bool readData(PacketReader &stream) override;

    /// Deep copy clone.
    /// @return A deep copy.
    Shape *clone() const override;

  protected:
    void onClone(MeshShape *copy) const;

    float *allocateVertices(unsigned count);
    void freeVertices(const float *&vertices);

    unsigned *allocateIndices(unsigned count);
    void freeIndices(const unsigned *&indices);

    const float *_vertices;     ///< Mesh vertices.
    unsigned _vertexStride;     ///< Stride into _vertices in float elements, not bytes.
    unsigned _vertexCount;      ///< Number of @c _vertices.
    const float *_normals;      ///< Normals array, one per vertex.
    unsigned _normalsStride;    ///< Stride into _normals in float elements, not bytes.
    unsigned _normalsCount;     ///< Number of @c _normals. Must be 0, 1 or @c _vertexCount.
                                ///< 0 indicates no normals, 1 indicates a single, shared normal (for voxels),
                                ///< otherwise there must be one per normal.
    const unsigned *_indices;   ///< Optional triangle indices.
    unsigned _indexCount;       ///< Number of @c indices. Divide by 3 for the triangle count.
    DrawType _drawType;         ///< The primitive to render.
    bool _ownPointers;          ///< Does this instance own its vertices and indices?
    bool _ownNormals;           ///< Does this instance own its normals? Always true if @p _ownPointers is true.
  };


  inline MeshShape::MeshShape()
    : Shape(SIdMeshShape)
    , _vertices(nullptr)
    , _vertexStride(3)
    , _vertexCount(0)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(nullptr)
    , _indexCount(0)
    , _drawType(DtTriangles)
    , _ownPointers(false)
    , _ownNormals(false)
  {
  }


  inline MeshShape::MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
                              const V3Arg &position,
                              const QuaternionArg &rotation,
                              const V3Arg &scale)
    : Shape(SIdMeshShape)
    , _vertices(vertices)
    , _vertexStride(unsigned(vertexByteSize / sizeof(float)))
    , _vertexCount(vertexCount)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(nullptr)
    , _indexCount(0)
    , _drawType(drawType)
    , _ownPointers(false)
    , _ownNormals(false)
  {
    setPosition(position);
    setRotation(rotation);
    setScale(scale);
  }


  inline MeshShape::MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
                              const unsigned *indices, unsigned indexCount,
                              const V3Arg &position,
                              const QuaternionArg &rotation,
                              const V3Arg &scale)
    : Shape(SIdMeshShape)
    , _vertices(vertices)
    , _vertexStride(unsigned(vertexByteSize / sizeof(float)))
    , _vertexCount(vertexCount)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(indices)
    , _indexCount(indexCount)
    , _drawType(drawType)
    , _ownPointers(false)
    , _ownNormals(false)
  {
    setPosition(position);
    setRotation(rotation);
    setScale(scale);
  }


  inline MeshShape::MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
                              uint32_t id,
                              const V3Arg &position,
                              const QuaternionArg &rotation,
                              const V3Arg &scale)
    : Shape(SIdMeshShape, id)
    , _vertices(vertices)
    , _vertexStride(unsigned(vertexByteSize / sizeof(float)))
    , _vertexCount(vertexCount)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(nullptr)
    , _indexCount(0)
    , _drawType(drawType)
    , _ownPointers(false)
    , _ownNormals(false)
  {
    setPosition(position);
    setRotation(rotation);
    setScale(scale);
  }


  inline MeshShape::MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
                              const unsigned *indices, unsigned indexCount,
                              uint32_t id,
                              const V3Arg &position,
                              const QuaternionArg &rotation,
                              const V3Arg &scale)
    : Shape(SIdMeshShape, id)
    , _vertices(vertices)
    , _vertexStride(unsigned(vertexByteSize / sizeof(float)))
    , _vertexCount(vertexCount)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(indices)
    , _indexCount(indexCount)
    , _drawType(drawType)
    , _ownPointers(false)
    , _ownNormals(false)
  {
    setPosition(position);
    setRotation(rotation);
    setScale(scale);
  }


  inline MeshShape::MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
                              uint32_t id, uint16_t category,
                              const V3Arg &position,
                              const QuaternionArg &rotation,
                              const V3Arg &scale)
    : Shape(SIdMeshShape, id, category)
    , _vertices(vertices)
    , _vertexStride(unsigned(vertexByteSize / sizeof(float)))
    , _vertexCount(vertexCount)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(nullptr)
    , _indexCount(0)
    , _drawType(drawType)
    , _ownPointers(false)
    , _ownNormals(false)
  {
    setPosition(position);
    setRotation(rotation);
    setScale(scale);
  }


  inline MeshShape::MeshShape(DrawType drawType, const float *vertices, unsigned vertexCount, size_t vertexByteSize,
                              const unsigned *indices, unsigned indexCount,
                              uint32_t id, uint16_t category,
                              const V3Arg &position,
                              const QuaternionArg &rotation,
                              const V3Arg &scale)
    : Shape(SIdMeshShape, id, category)
    , _vertices(vertices)
    , _vertexStride(unsigned(vertexByteSize / sizeof(float)))
    , _vertexCount(vertexCount)
    , _normals(nullptr)
    , _normalsStride(3)
    , _normalsCount(0)
    , _indices(indices)
    , _indexCount(indexCount)
    , _drawType(drawType)
    , _ownPointers(false)
    , _ownNormals(false)
  {
    setPosition(position);
    setRotation(rotation);
    setScale(scale);
  }


  inline MeshShape::~MeshShape()
  {
    if (_ownPointers)
    {
      freeVertices(_vertices);
      freeIndices(_indices);
    }
    if (_ownNormals)
    {
      freeVertices(_normals);
    }
  }


  inline bool MeshShape::calculateNormals() const
  {
    return (_data.flags & MeshShapeCalculateNormals) != 0;
  }


  inline MeshShape &MeshShape::setCalculateNormals(bool calculate)
  {
    _data.flags &= ~MeshShapeCalculateNormals;
    _data.flags |= MeshShapeCalculateNormals * !!calculate;
    return *this;
  }
}

#endif // _3ESMESHSHAPE_H_
