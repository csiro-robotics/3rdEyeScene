//
// author: Kazys Stepanas
//
#ifndef _3ESPOINTCLOUD_H_
#define _3ESPOINTCLOUD_H_

#include "3es-core.h"

#include "3escolour.h"
#include "3esintarg.h"
#include "3esmeshresource.h"

namespace tes
{
  struct PointCloudImp;

  /// A @c MeshResource which defines a point cloud by its contained vertices..
  ///
  /// The @c PointCloud supports a set of vertices, normals and colours only. Indices, UVs are not supported.
  class _3es_coreAPI PointCloud : public MeshResource
  {
  protected:
    /// A shallow copy constructor, supporting copy on write semantics.
    /// @param other The cloud to copy.
    PointCloud(const PointCloud &other);

  public:
    /// Create a @c PointCloud resource with the given ID.
    /// @param id A user assigned, unique ID for the point cloud resource.
    PointCloud(uint32_t id);
    /// Destructor.
    ~PointCloud();

    /// @copydoc MeshResource::id()
    uint32_t id() const override;

    /// Clone the cloud resource.
    ///
    /// This performs a shallow copy clone with copy on write semantics. That is to say, the copy becomes a deep copy
    /// if either the clone or the original are modified.
    ///
    /// @return A clone of the point cloud resource.
    PointCloud *clone() const override;

    /// Always the identity matrix.
    /// @return An identity matrix.
    Matrix4f transform() const override;

    /// Always returns white.
    /// @return White.
    uint32_t tint() const override;

    /// Always returns @c DtPoints.
    /// @return @c DtPoints
    uint8_t drawType(int stream = 0) const override;

    /// Reserve sufficient vertex, normal and colour data for @c size points.
    /// @param size The number of points to reserve space for.
    void reserve(const UIntArg &size);

    /// Resize the point cloud to contain @c count vertices, normals and colours.
    /// @param count The number of points to resize the cloud to.
    void resize(const UIntArg &count);

    /// Reduce allocated memory to exactly match the number of points currently in the cloud.
    void squeeze();

    /// Return the number of points allocated memory currently supports.
    /// @return The number of points supported by the current memory allocation.
    unsigned capacity() const;

    /// @copydoc MeshResource::vertexCount()
    unsigned vertexCount(int stream = 0) const override;
    /// @copydoc MeshResource::vertices()
    const float *vertices(unsigned &stride, int stream = 0) const override;
    /// Access vertices as a @c Vector3f array.
    /// @return A pointer to the vertex data as a @c Vector3f array. The number of elements matches @c vertexCount().
    const Vector3f *vertices() const;

    /// Not supported.
    /// @return Zero.
    unsigned indexCount(int stream = 0) const override;

    /// Not supported.
    /// @return null
    const uint8_t *indices(unsigned &stride, unsigned &width, int stream = 0) const override;

    /// @copydoc MeshResource::normals()
    const float *normals(unsigned &stride, int stream = 0) const override;
    /// Access normals as a @c Vector3f array.
    /// @return A pointer to the normal data as a @c Vector3f array. The number of elements matches @c vertexCount().
    const Vector3f *normals() const;

    /// @copydoc MeshResource::colours()
    const uint32_t *colours(unsigned &stride, int stream = 0) const override;
    /// Access colours as a @c Colour array.
    /// @return A pointer to the colour data as a @c Colour array. The number of elements matches @c vertexCount().
    const Colour *colours() const;

    /// Not supported.
    /// @return null
    const float *uvs(unsigned &, int) const override;

    /// Add a single point to the cloud.
    /// The normal is set to zero and the colour to white.
    /// @param point The point to add.
    void addPoint(const Vector3f &point);
    /// Add a single point to the cloud.
    /// The colour is set to white.
    /// @param point The point to add.
    /// @param normal The point normal.
    void addPoint(const Vector3f &point, const Vector3f &normal);
    /// Add a single point to the cloud.
    /// @param point The point to add.
    /// @param normal The point normal.
    /// @param colour The point colour.
    void addPoint(const Vector3f &point, const Vector3f &normal, const Colour &colour);

    /// Add a set of points to the cloud.
    /// The normals are set to zero and the colours to white.
    /// @param points The points to add.
    /// @param count Number of points in @p points.
    void addPoints(const Vector3f *points, const UIntArg &count);
    /// Add a set of points to the cloud.
    /// The colours are set to white.
    /// @param points The points to add.
    /// @param normals The point normals.
    /// @param count Number of points in @p points and @p normals.
    void addPoints(const Vector3f *points, const Vector3f *normals, const UIntArg &count);
    /// Add a set of points to the cloud.
    /// @param points The points to add.
    /// @param normals The point normals.
    /// @param colours The point colours.
    /// @param count Number of points in @p points, @p normals and @p colours.
    void addPoints(const Vector3f *points, const Vector3f *normals, const Colour *colours, const UIntArg &count);

    /// Replace an existing point.
    /// Ignore if out of range.
    /// @param index The point index.
    /// @param point The new point coordinate.
    void setPoint(const UIntArg &index, const Vector3f &point);
    /// Replace an existing point.
    /// Ignore if out of range.
    /// @param index The point index.
    /// @param point The new point coordinate.
    /// @param normal The new point normal.
    void setPoint(const UIntArg &index, const Vector3f &point, const Vector3f &normal);
    /// Replace an existing point.
    /// Ignore if out of range.
    /// @param index The point index.
    /// @param point The new point coordinate.
    /// @param normal The new point normal.
    /// @param colour The new point colour.
    void setPoint(const UIntArg &index, const Vector3f &point, const Vector3f &normal, const Colour &colour);

    /// Replace an existing point normal.
    /// Ignore if out of range.
    /// @param index The point index.
    /// @param normal The new point normal.
    void setNormal(const UIntArg &index, const Vector3f &normal);
    /// Replace an existing point colour.
    /// Ignore if out of range.
    /// @param index The point index.
    /// @param colour The new point colour.
    void setColour(const UIntArg &index, const Colour &colour);

    /// Replace a set of existing points.
    /// Normal and colour data are left as is.
    ///
    /// Overrun points are ignored.
    /// @param index The point index.
    /// @param points The new point coordinates.
    /// @param count The number of points in @p points.
    void setPoints(const UIntArg &index, const Vector3f *points, const UIntArg &count);
    /// Replace a set of existing points.
    /// Colour data are left as is.
    ///
    /// Overrun points are ignored.
    /// @param index The point index.
    /// @param points The new point coordinates.
    /// @param normals The new point normals.
    /// @param count The number of points in @p points and @p normals.
    void setPoints(const UIntArg &index, const Vector3f *points, const Vector3f *normals, const UIntArg &count);
    /// Replace a set of existing points.
    ///
    /// Overrun points are ignored.
    /// @param index The point index.
    /// @param points The new point coordinates.
    /// @param normals The new point normals.
    /// @param colours The new point colours.
    /// @param count The number of points in @p points, @p normals and @p colours.
    void setPoints(const UIntArg &index, const Vector3f *points, const Vector3f *normals, const Colour *colours, const UIntArg &count);

  private:
    /// Reserve memory for @p capacity points.
    /// @param capacity Number of points to reserve capacity for.
    void setCapacity(unsigned capacity);

    /// Make a copy of underlying data if currently shared with another instance.
    void copyOnWrite();

    bool processCreate(const MeshCreateMessage &msg) override;
    bool processVertices(const MeshComponentMessage &msg, const float *vertices, unsigned vertexCount) override;
    bool processColours(const MeshComponentMessage &msg, const uint32_t *colours, unsigned colourCount) override;
    bool processNormals(const MeshComponentMessage &msg, const float *normals, unsigned normalCount) override;

    PointCloudImp *_imp;
  };


  inline void PointCloud::addPoint(const Vector3f &point)
  {
    addPoints(&point, 1);
  }


  inline void PointCloud::addPoint(const Vector3f &point, const Vector3f &normal)
  {
    addPoints(&point, &normal, 1);
  }


  inline void PointCloud::addPoint(const Vector3f &point, const Vector3f &normal, const Colour &colour)
  {
    addPoints(&point, &normal, &colour, 1);
  }



  inline void PointCloud::setPoint(const UIntArg &index, const Vector3f &point)
  {
    setPoints(index, &point, 1);
  }


  inline void PointCloud::setPoint(const UIntArg &index, const Vector3f &point, const Vector3f &normal)
  {
    setPoints(index, &point, &normal, 1);
  }


  inline void PointCloud::setPoint(const UIntArg &index, const Vector3f &point, const Vector3f &normals, const Colour &colours)
  {
    setPoints(index, &point, &normals, &colours, 1);
  }
}

#endif // _3ESPOINTCLOUD_H_
