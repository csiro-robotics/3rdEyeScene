//
// author Kazys Stepanas
//
#include "occupancymesh.h"

#include "p2p.h"

#ifdef TES_ENABLE
#include <3esservermacros.h>
#include <3estransferprogress.h>

using namespace tes;

struct OccupancyMeshDetail
{
  std::vector<tes::Vector3f> vertices;
  std::vector<uint32_t> colours;
  //std::vector<uint32_t> indices;
  /// Tracks indices of unused vertices in the vertex array.
  std::vector<uint32_t> unusedVertexList;
  /// Maps voxel keys to their vertex indices.
  KeyToIndexMap voxelIndexMap;
};

OccupancyMesh::OccupancyMesh(unsigned meshId, octomap::OcTree &map)
  : _map(map)
  , _id(meshId)
  , _detail(new OccupancyMeshDetail)
{
  // Expose the mesh resource.
  g_tesServer->referenceResource(this);
}


OccupancyMesh::~OccupancyMesh()
{
  g_tesServer->releaseResource(this);
  delete _detail;
}

uint32_t OccupancyMesh::id() const
{
  return _id;
}


tes::Matrix4f OccupancyMesh::transform() const
{
  return tes::Matrix4f::identity;
}


uint32_t OccupancyMesh::tint() const
{
  return 0xFFFFFFFFu;
}


uint8_t OccupancyMesh::drawType(int stream) const
{
  return tes::DtPoints;
}


unsigned OccupancyMesh::vertexCount(int stream) const
{
  return (unsigned)_detail->vertices.size();
}


unsigned OccupancyMesh::indexCount(int stream) const
{
  //return (unsigned)_detail->indices.size();
  return 0;
}


const float * OccupancyMesh::vertices(unsigned & stride, int stream) const
{
  stride = sizeof(Vector3f);
  return (!_detail->vertices.empty()) ? _detail->vertices.data()->v : nullptr;
}


const uint8_t * OccupancyMesh::indices(unsigned & stride, unsigned & width, int stream) const
{
  //width = stride = sizeof(IndexType);
  //return (!_detail->indices.empty()) ? reinterpret_cast<const uint8_t *>(_detail->indices.data()) : nullptr;
  return nullptr;
}

const float * OccupancyMesh::normals(unsigned & stride, int stream) const
{
  return nullptr;
}


const float * OccupancyMesh::uvs(unsigned & stride, int stream) const
{
  return nullptr;
}


const uint32_t * OccupancyMesh::colours(unsigned &stride, int stream) const
{
  stride = sizeof(uint32_t);
  return (!_detail->colours.empty()) ? _detail->colours.data() : nullptr;
}

tes::Resource *OccupancyMesh::clone() const
{
  OccupancyMesh *copy = new OccupancyMesh(_id, _map);
  *copy->_detail = *_detail;
  return copy;
}


int OccupancyMesh::transfer(tes::PacketWriter & packet, int byteLimit, tes::TransferProgress & progress) const
{
  // Build the voxel set if required.
  if (_detail->voxelIndexMap.empty())
  {
    _detail->vertices.clear();
    _detail->colours.clear();
    for (auto node = _map.begin_leafs(); node != _map.end(); ++node)
    {
      if (_map.isNodeOccupied(*node))
      {
        // Add voxel.
        _detail->voxelIndexMap.insert(std::make_pair(node.getKey(), uint32_t(_detail->vertices.size())));
        _detail->vertices.push_back(p2p(_map.keyToCoord(node.getKey())));
        _detail->colours.push_back(0xffffffffu);
      }
    }
  }

  return tes::MeshResource::transfer(packet, byteLimit, progress);
}



void OccupancyMesh::update(const UnorderedKeySet &newlyOccupied, const UnorderedKeySet &newlyFree)
{
  if (newlyOccupied.empty() && newlyFree.empty())
  {
    // Nothing to do.
    return;
  }

  if (g_tesServer->connectionCount() == 0)
  {
    // No-one to send to.
    _detail->vertices.clear();
    _detail->colours.clear();
    //_detail->indices.clear();
    _detail->unusedVertexList.clear();
    _detail->voxelIndexMap.clear();
    return;
  }

  // Remove already occupied voxels from touchedKeys.

  // Start by removing freed nodes.
  size_t initialUnusedVertexCount = _detail->unusedVertexList.size();
  std::vector<uint32_t> modifiedVertices;
  for (const octomap::OcTreeKey &key : newlyFree)
  {
    // Resolve the index for this voxel.
    auto voxelLookup = _detail->voxelIndexMap.find(key);
    if (voxelLookup != _detail->voxelIndexMap.end())
    {
      // Invalidate the voxel.
      _detail->colours[voxelLookup->second] = 0u;
      _detail->unusedVertexList.push_back(voxelLookup->second);
      modifiedVertices.push_back(voxelLookup->second);
      _detail->voxelIndexMap.erase(voxelLookup);
    }
  }

  // Now added occupied nodes, initially from the free list.
  size_t processedOccupiedCount = 0;
  auto occupiedIter = newlyOccupied.begin();
  while (!_detail->unusedVertexList.empty() && occupiedIter != newlyOccupied.end())
  {
    const uint32_t vertexIndex = _detail->unusedVertexList.back();
    const octomap::OcTreeKey key = *occupiedIter;
    const bool markAsModified = _detail->unusedVertexList.size() <= initialUnusedVertexCount;
    _detail->unusedVertexList.pop_back();
    ++occupiedIter;
    ++processedOccupiedCount;
    _detail->vertices[vertexIndex] = p2p(_map.keyToCoord(key));
    _detail->colours[vertexIndex] = 0xffffffffu;
    _detail->voxelIndexMap.insert(std::make_pair(key, vertexIndex));
    // Only mark as modified if this vertex wasn't just invalidate by removal.
    // It will already be on the list otherwise.
    if (markAsModified)
    {
      modifiedVertices.push_back(vertexIndex);
    }
  }

  // Send messages for individually changed voxels.
  // Start a mesh redefinition message.
  std::vector<uint8_t> buffer(0xffffu);
  tes::PacketWriter packet(buffer.data(), (uint16_t)buffer.size());
  tes::MeshRedefineMessage msg;
  tes::MeshComponentMessage cmpmsg;
  tes::MeshFinaliseMessage finalmsg;

  // Work out how many vertices we'll have after all modifications are done.
  size_t oldVertexCount = _detail->vertices.size();
  size_t newVertexCount = _detail->vertices.size();
  if (newlyOccupied.size() - processedOccupiedCount > _detail->unusedVertexList.size())
  {
    // We have more occupied vertices than available in the free list.
    // This means we will add new vertices.
    newVertexCount += newlyOccupied.size() - processedOccupiedCount - _detail->unusedVertexList.size();
  }

  msg.meshId = _id;
  msg.vertexCount = (uint32_t)newVertexCount;
  msg.indexCount = 0;
  msg.drawType = drawType(0);
  msg.attributes.identity();

  packet.reset(tes::MtMesh, tes::MeshRedefineMessage::MessageId);
  msg.write(packet);

  packet.finalise();
  g_tesServer->send(packet);

  // Next update changed triangles.
  cmpmsg.meshId = id();
  cmpmsg.reserved = 0;
  cmpmsg.count = 1;

  // Update modified vertices, one at a time.
  for (uint32_t vertexIndex : modifiedVertices)
  {
    cmpmsg.offset = vertexIndex;
    // Send colour and position update.
    packet.reset(tes::MtMesh, tes::MmtVertexColour);
    cmpmsg.write(packet);
    // Write the invalid value.
    packet.writeArray<uint32_t>(&_detail->colours[vertexIndex], 1);
    packet.finalise();
    g_tesServer->send(packet);

    packet.reset(tes::MtMesh, tes::MmtVertex);
    cmpmsg.write(packet);
    // Write the invalid value.
    packet.writeArray<Vector3f>(&_detail->vertices[vertexIndex], 1);
    packet.finalise();
    g_tesServer->send(packet);
  }

  // Add remaining vertices and send a bulk modification message.
  for (; occupiedIter != newlyOccupied.end(); ++occupiedIter, ++processedOccupiedCount)
  {
    const uint32_t vertexIndex = uint32_t(_detail->vertices.size());
    const octomap::OcTreeKey key = *occupiedIter;
    _detail->voxelIndexMap.insert(std::make_pair(key, vertexIndex));
    //_detail->indices.push_back(uint32_t(_detail->vertices.size()));
    _detail->vertices.push_back(p2p(_map.keyToCoord(key)));
    _detail->colours.push_back(0xffffffffu);
  }

  // Send bulk messages for new vertices.
  if (oldVertexCount != newVertexCount)
  {
    const uint16_t transferLimit = 5001;
    // Send colour and position update.
    cmpmsg.offset = oldVertexCount;

    while (cmpmsg.offset < newVertexCount)
    {
      cmpmsg.count = uint16_t(std::min<size_t>(transferLimit, newVertexCount - cmpmsg.offset));

      packet.reset(tes::MtMesh, tes::MmtVertex);
      cmpmsg.write(packet);
      packet.writeArray<float>(_detail->vertices[cmpmsg.offset].v, cmpmsg.count * 3);
      packet.finalise();
      g_tesServer->send(packet);

      packet.reset(tes::MtMesh, tes::MmtVertexColour);
      cmpmsg.write(packet);
      packet.writeArray<uint32_t>(&_detail->colours[cmpmsg.offset], cmpmsg.count);
      packet.finalise();
      g_tesServer->send(packet);

      // Calculate next batch.
      cmpmsg.offset += cmpmsg.count;
    }
  }

  // Finalise the modifications.
  finalmsg.meshId = _id;
  // Rely on EDL shader.
  finalmsg.flags = 0;// tes::MbfCalculateNormals;
  packet.reset(tes::MtMesh, finalmsg.MessageId);
  finalmsg.write(packet);
  packet.finalise();
  g_tesServer->send(packet);
}

#endif // TES_ENABLE
