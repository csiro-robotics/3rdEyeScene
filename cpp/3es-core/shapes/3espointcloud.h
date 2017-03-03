//
// author: Kazys Stepanas
//
#ifndef _3ESPOINTCLOUD_H_
#define _3ESPOINTCLOUD_H_

#include "3es-core.h"

#include "3escolour.h"
#include "3esmeshresource.h"

namespace tes
{
  struct PointCloudImp;

  class _3es_coreAPI PointCloud : public MeshResource
  {
  protected:
    PointCloud(const PointCloud &other);

  public:
    PointCloud(uint32_t id);
    ~PointCloud();

    uint32_t id() const override;

    PointCloud *clone() const override;

    /// Identity matrix.
    Matrix4f transform() const override;

    /// White.
    uint32_t tint() const override;

    /// DtPoints
    uint8_t drawType(int stream = 0) const override;

    void reserve(unsigned size);
    void resize(unsigned count);

    void squeeze();

    unsigned capacity() const;

    unsigned vertexCount(int stream = 0) const override;
    const float *vertices(unsigned &stride, int stream = 0) const override;
    const Vector3f *vertices() const;

    /// Zero.
    unsigned indexCount(int stream = 0) const override;

    /// Null.
    const uint8_t *indices(unsigned &stride, unsigned &width, int stream = 0) const override;

    const float *normals(unsigned &stride, int stream = 0) const override;
    const Vector3f *normals() const;

    const uint32_t *colours(unsigned &stride, int stream = 0) const override;
    const Colour *colours() const;

    const float *uvs(unsigned &, int) const override;

    void addPoint(const Vector3f &point);
    void addPoint(const Vector3f &point, const Vector3f &normal);
    void addPoint(const Vector3f &point, const Vector3f &normal, const Colour &colour);

    void addPoints(const Vector3f *points, unsigned count);
    void addPoints(const Vector3f *points, const Vector3f *normals, unsigned count);
    void addPoints(const Vector3f *points, const Vector3f *normals, const Colour *colours, unsigned count);

    void setPoint(unsigned index, const Vector3f &point);
    void setPoint(unsigned index, const Vector3f &point, const Vector3f &normal);
    void setPoint(unsigned index, const Vector3f &point, const Vector3f &normal, const Colour &colour);

    void setNormal(unsigned index, const Vector3f &normal);
    void setColour(unsigned index, const Colour &colour);

    void setPoints(unsigned index, const Vector3f *points, unsigned count);
    void setPoints(unsigned index, const Vector3f *points, const Vector3f *normals, unsigned count);
    void setPoints(unsigned index, const Vector3f *points, const Vector3f *normals, const Colour *colours, unsigned count);

  private:
    void setCapacity(unsigned capacity);

    void copyOnWrite();

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



  inline void PointCloud::setPoint(unsigned index, const Vector3f &point)
  {
    setPoints(index, &point, 1);
  }


  inline void PointCloud::setPoint(unsigned index, const Vector3f &point, const Vector3f &normal)
  {
    setPoints(index, &point, &normal, 1);
  }


  inline void PointCloud::setPoint(unsigned index, const Vector3f &point, const Vector3f &normals, const Colour &colours)
  {
    setPoints(index, &point, &normals, &colours, 1);
  }
}

#endif // _3ESPOINTCLOUD_H_
