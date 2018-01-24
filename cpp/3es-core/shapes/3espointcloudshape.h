//
// author: Kazys Stepanas
//
#ifndef _3ESPOINTS_H_
#define _3ESPOINTS_H_

#include "3es-core.h"

#include "3esshape.h"
#include "3esmeshset.h"

namespace tes
{
  /// A @c Shape which renders a set of points as in a point cloud.
  ///
  /// The points are contained in a @c MeshResource (e.g., @c PointCloud)
  /// and may be shared between @c PointCloudShape shapes. The @c MeshResource should
  /// have a @c MeshResource.drawType() of @c DtPoints or the behaviour may
  /// be undefined.
  ///
  /// The @c PointCloudShape shape supports limiting the view into the @c MeshResource
  /// by having its own set of indices (see @c setIndices()).
  class _3es_coreAPI PointCloudShape : public Shape
  {
  public:
    /// Construct a point cloud shape object.
    /// mesh The mesh resource to render point data from. See class comments.
    /// @param id The shape ID, unique among @c Arrow objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    /// @param pointSize Desired point render size (pixels).
    PointCloudShape(const MeshResource *mesh, uint32_t id = 0, uint16_t category = 0, uint8_t pointSize = 1);

    /// Destructor.
    ~PointCloudShape();

    inline const char *type() const override { return "pointCloudShape"; }

    /// Set the desired point render size (pixels).
    /// @param size The desired render size (pixels).
    /// @return @c *this
    inline PointCloudShape &setPointSize(uint8_t size) { _pointSize = size; return *this; }
    /// Get the desired point render size (pixels).
    /// @return The desired point render size.
    inline uint8_t pointSize() const { return _pointSize; }

    /// Return the number of @c indices().
    ///
    /// Only non-zero when referencing a subset of @c mesh() vertices.
    ///
    /// @return Zero when using all @c mesh() vertices, non-zero when referencing a subset of @c mesh().
    unsigned indexCount() const { return _indexCount; }

    /// Return the index array when a subset of @c mesh() vertices.
    ///
    /// Indices are only set when overriding indexing from @c mesh().
    ///
    /// @return An array of indices, length @c indexCount(), or null when referencing all vertices from @c mesh().
    const unsigned *indices() const { return _indices; }

    /// Sets the (optional) indices for this @c PointCloudShape @c Shape.
    /// This shape will only visualise the indexed points from its @c PointSource.
    /// This allows multiple @c PointCloudShape shapes to reference the same cloud, but reveal
    /// sub-sets of the cloud.
    ///
    /// This method is designed to copy any iterable sequence between @p begin and @p end,
    /// however the number of elements must be provided in @p indexCount.
    ///
    /// @tparam I An iterable item. Must support dereferencing to an unsigned integer and
    ///   an increment operator.
    /// @param iter The index iterator.
    /// @param indexCount The number of elements to copy from @p iter.
    /// @return This.
    template <typename I>
    PointCloudShape &setIndices(I begin, uint32_t indexCount);

    /// Get the mesh resource containing the point data to render.
    /// @return The point cloud mesh resource.
    inline const MeshResource *mesh() const { return _mesh; }

    /// Writes the standard create message and appends the point cloud ID (@c uint32_t).
    /// @param stream The stream to write to.
    /// @return True on success.
    bool writeCreate(PacketWriter &stream) const override;

    /// Write index data set in @c setIndices() if any.
    /// @param stream The data stream to write to.
    /// @param[in,out] progressMarker Indicates data transfer progress.
    ///   Initially zero, the @c Shape manages its own semantics.
    /// @return Indicates completion progress. 0 indicates completion,
    ///   1 indicates more data are available and more calls should be made.
    ///   -1 indicates an error. No more calls should be made.
    int writeData(PacketWriter &stream, unsigned &progressMarker) const override;

    bool readCreate(PacketReader &stream) override;

    bool readData(PacketReader &stream) override;

    /// Defines this class as a complex shape. See Shape::isComplex().
    /// @return @c true
    virtual inline bool isComplex() const override { return true; }

    /// Enumerates the mesh resource given on construction. See @c Shape::enumerateResources().
    /// @param resources Resource output array.
    /// @param capacity of @p resources.
    /// @param fetchOffset Indexing offset for the resources in this object.
    int enumerateResources(const Resource **resources, int capacity, int fetchOffset) const override;

    /// Deep copy clone. The source is only cloned if @c ownSource() is true.
    /// It is shared otherwise.
    /// @return A deep copy.
    Shape *clone() const override;

  private:
    void onClone(PointCloudShape *copy) const;

    /// Reallocate the index array preserving current data.
    /// @param count The new element size for the array.
    void reallocateIndices(uint32_t count);
    uint32_t *allocateIndices(uint32_t count);
    void freeIndices(const uint32_t *indices);

    const MeshResource *_mesh;
    uint32_t *_indices;
    uint32_t _indexCount;
    uint8_t _pointSize;
    bool _ownMesh;
  };


  inline PointCloudShape::PointCloudShape(const MeshResource *mesh, uint32_t id, uint16_t category, uint8_t pointSize)
    : Shape(SIdPointCloud, id, category)
    , _mesh(mesh)
    , _indices(nullptr)
    , _indexCount(0)
    , _pointSize(pointSize)
    , _ownMesh(false)
  {
  }


  template <typename I>
  PointCloudShape &PointCloudShape::setIndices(I iter, uint32_t indexCount)
  {
    freeIndices(_indices);
    _indices = nullptr;
    _indexCount = indexCount;
    if (indexCount)
    {
      _indices = allocateIndices(indexCount);
      uint32_t *ind = _indices;
      for (uint32_t i = 0; i < indexCount; ++i, ++ind, ++iter)
      {
        *ind = *iter;
      }
    }

    return *this;
  }
}

#endif // _3ESPOINTS_H_
