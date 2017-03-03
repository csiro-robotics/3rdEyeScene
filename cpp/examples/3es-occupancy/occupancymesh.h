//
// author Kazys Stepanas
//
#include "3es-occupancy.h"

#include <unordered_map>
#include <unordered_set>
#include <vector>

// Disable various OCTOMAP warnings
#ifdef _MSC_VER
// C4267: conversion from size_t to unsigned int
// 4244: conversion from double to float
#pragma warning(disable : 4244 4267)
#endif // _MSC_VER
#include <octomap/octomap.h>

#ifdef TES_ENABLE
#include <shapes/3esmeshresource.h>
#endif // TES_ENABLE

typedef std::unordered_map<octomap::OcTreeKey, uint32_t, octomap::OcTreeKey::KeyHash> KeyToIndexMap;
typedef std::unordered_set<octomap::OcTreeKey, octomap::OcTreeKey::KeyHash> UnorderedKeySet;
typedef std::vector<octomap::OcTreeKey> KeyArray;
class OccupancyMesh;

#ifdef TES_ENABLE
struct OccupancyMeshDetail;

/// Defines and maintains a 3rd Eye Scene mesh resource based on an octomap.
///
/// Renders as a point cloud of occupied voxels.
class OccupancyMesh : public tes::MeshResource
{
public:
  OccupancyMesh(unsigned meshId, octomap::OcTree &map);
  ~OccupancyMesh();

  uint32_t id() const override;
  tes::Matrix4f transform() const override;
  uint32_t tint() const override;
  uint8_t drawType(int stream) const override;

  unsigned vertexCount(int stream) const override;
  unsigned indexCount(int stream) const override;

  const float *vertices(unsigned &stride, int stream) const override;
  const uint8_t *indices(unsigned &stride, unsigned &width, int stream) const override;
  const float *normals(unsigned &stride, int stream) const override;
  const float *uvs(unsigned &stride, int stream) const override;
  const uint32_t *colours(unsigned &stride, int stream) const override;

  tes::Resource *clone() const override;

  int transfer(tes::PacketWriter &packet, int byteLimit, tes::TransferProgress &progress) const override;

  /// Updates noted changes to the debug view.
  /// @param occupiedChange Keys of voxels which have become occupied from free or uncertain since the last update.
  /// @param newlyFree Keys of voxels which have become free from occupied since the last update.
  /// @param touchedOccupied Keys of voxels which have changed occupied probability.
  void update(const UnorderedKeySet &newlyOccupied, const UnorderedKeySet &newlyFree, const UnorderedKeySet &touchedOccupied);

private:
  typedef uint32_t IndexType;

  octomap::OcTree &_map;
  uint32_t _id;

  OccupancyMeshDetail *_detail;
};

#endif // TES_ENABLE
