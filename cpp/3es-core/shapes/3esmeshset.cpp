//
// author: Kazys Stepanas
//
#include "3esmeshset.h"

#include "3esmeshmessages.h"
#include "3esrotation.h"
#include "3estransferprogress.h"

#include <algorithm>

using namespace tes;

namespace
{
  /// Estimate the number of elements which can be transferred at the given @p byteLimit.
  /// @param elementSize The byte size of each element.
  /// @param byteLimit The maximum number of bytes to transfer. Note: a hard limit of 0xffff is
  ///   enforced.
  uint16_t estimateTransferCount(size_t elementSize, unsigned byteLimit)
  {
    //                                    packet header           crc
    const size_t maxTransfer = (0xffffu - sizeof(PacketHeader) - sizeof(uint16_t)) / elementSize;
    size_t count = byteLimit ? byteLimit / elementSize : maxTransfer;
    if (count < 1)
    {
      count = 1;
    }
    else if (count > maxTransfer)
    {
      count = maxTransfer;
    }

    return uint16_t(count);
  }


  template <typename T, int ELEMCOUNT = 1>
  unsigned writeComponent(PacketWriter &packet, uint32_t meshId,
                          uint32_t offset, unsigned byteLimit,
                          const uint8_t *dataSource, unsigned dataStride,
                          uint32_t componentCount)
  {
    MeshComponentMessage msg;
    const unsigned elementSize = sizeof(T) * ELEMCOUNT;
    int effectiveByteLimit = packet.bytesRemaining() - (sizeof(msg) + sizeof(PacketWriter::CrcType));
    if (effectiveByteLimit < 0)
    {
      effectiveByteLimit = 0;
    }
    // Truncate to 16-bits and allow for a fair amount of overhead.
    // FIXME: Without the additional overhead I was getting missing messages at the client with
    // no obvious error path.
    byteLimit = std::min(byteLimit, 0xff00u);
    effectiveByteLimit = byteLimit ? std::min<int>(effectiveByteLimit, byteLimit) : effectiveByteLimit;
    uint16_t transferCount = estimateTransferCount(elementSize, effectiveByteLimit);
    if (transferCount > componentCount - offset)
    {
      transferCount = componentCount - offset;
    }

    msg.meshId = meshId;
    msg.offset = offset;
    msg.reserved = 0;
    msg.count = transferCount;

    unsigned write;
    write = msg.write(packet);
    // Jump to offset.
    dataSource += dataStride *offset;
    for (unsigned i = 0; i < transferCount; ++i)
    {
      const T *element = reinterpret_cast<const T *>(dataSource);
      write = unsigned(packet.writeArray(element, ELEMCOUNT));
      dataSource += dataStride;
    }

    return transferCount;
  }
}


uint16_t MeshResource::typeId() const
{
  return MtMesh;
}


int MeshResource::create(PacketWriter &packet) const
{
  Vector3f pos, scale;
  Quaternionf rot;
  MeshCreateMessage msg;

  packet.reset(typeId(), MeshCreateMessage::MessageId);

  msg.meshId = id();
  msg.vertexCount = vertexCount();
  msg.indexCount = indexCount();
  msg.drawType = drawType();

  transformToQuaternionTranslation(transform(), rot, pos, scale);
  msg.attributes.colour = tint();
  msg.attributes.position[0] = pos[0];
  msg.attributes.position[1] = pos[1];
  msg.attributes.position[2] = pos[2];
  msg.attributes.rotation[0] = rot[0];
  msg.attributes.rotation[1] = rot[1];
  msg.attributes.rotation[2] = rot[2];
  msg.attributes.rotation[3] = rot[3];
  msg.attributes.scale[0] = scale[0];
  msg.attributes.scale[1] = scale[1];
  msg.attributes.scale[2] = scale[2];

  msg.write(packet);

  return 0;
}


int MeshResource::destroy(PacketWriter &packet) const
{
  MeshDestroyMessage destroy;
  packet.reset(typeId(), MeshDestroyMessage::MessageId);
  destroy.meshId = id();
  destroy.write(packet);
  return 0;
}


int MeshResource::transfer(PacketWriter &packet, int byteLimit, TransferProgress &progress) const
{
  //packet.reset(typeId(), 0);
  if (progress.phase == 0)
  {
    // Initialise phase.
    progress.phase = MmtVertex;
    progress.progress = 0;
  }

  const uint8_t *dataSource = nullptr;
  uint32_t targetCount = 0;
  unsigned writeCount = 0;
  unsigned dataStride = 0;
  switch (progress.phase)
  {
  case MmtVertex:
    dataSource = reinterpret_cast<const uint8_t *>(vertices(dataStride));
    targetCount = vertexCount();
    packet.reset(typeId(), MmtVertex);
    writeCount = writeComponent<float, 3>(packet, id(),
                                          (uint32_t)progress.progress, byteLimit,
                                          dataSource, dataStride,
                                          targetCount);
    break;

  case MmtVertexColour:
    dataSource = reinterpret_cast<const uint8_t *>(colours(dataStride));
    targetCount = vertexCount();
    packet.reset(typeId(), MmtVertexColour);
    writeCount = writeComponent<uint32_t>(packet, id(),
                                          (uint32_t)progress.progress, byteLimit,
                                          dataSource, dataStride,
                                          targetCount);
    break;

  case MmtIndex:
  {
    // Indices need special handling to cater for the potential data width change.
    unsigned width = 0;
    dataSource = reinterpret_cast<const uint8_t *>(indices(dataStride, width));
    if (width == 0 || dataStride == 0)
    {
      // Width or stride not specified.
      return -1;
    }
    targetCount = indexCount();
    packet.reset(typeId(), MmtIndex);
    writeCount = writeIndices(packet, id(), (uint32_t)progress.progress,
                              byteLimit,
                              dataSource, dataStride, width,
                              targetCount);
    break;
  }

  case MmtNormal:
    dataSource = reinterpret_cast<const uint8_t *>(normals(dataStride));
    targetCount = vertexCount();
    packet.reset(typeId(), MmtNormal);
    writeCount = writeComponent<float, 3>(packet, id(),
                                          (uint32_t)progress.progress, byteLimit,
                                          dataSource, dataStride,
                                          targetCount);
    break;

  case MmtUv:
    dataSource = reinterpret_cast<const uint8_t *>(uvs(dataStride));
    targetCount = vertexCount();
    packet.reset(typeId(), MmtUv);
    writeCount = writeComponent<float, 3>(packet, id(),
                                          (uint32_t)progress.progress, byteLimit,
                                          dataSource, dataStride,
                                          targetCount);
    break;

  case MmtFinalise:
    {
      MeshFinaliseMessage msg;
      unsigned stride = 0;
      packet.reset(typeId(), MeshFinaliseMessage::MessageId);
      msg.meshId = id();
      msg.flags = (normals(stride) == nullptr) ? MbfCalculateNormals : 0;
      msg.write(packet);
      // Mark complete.
      progress.complete = true;
    }
    break;

  default:
    // Unknown state really.
    progress.failed = true;
    break;
  }

  progress.progress += writeCount;
  if (!progress.complete && progress.progress >= targetCount)
  {
    // Phase complete. Progress to the next phase.
    nextPhase(progress);
  }

  return 0;
}


unsigned MeshResource::writeIndices(PacketWriter &packet, uint32_t meshId,
                                    uint32_t offset, unsigned byteLimit,
                                    const uint8_t *dataSource, unsigned dataStride,
                                    unsigned indexByteWidth, uint32_t componentCount)
{
  uint32_t index;
  MeshComponentMessage msg;
  const unsigned elementSize = sizeof(index);
  int effectiveByteLimit = packet.bytesRemaining() - sizeof(msg) - sizeof(PacketWriter::CrcType);
  if (effectiveByteLimit < 0)
  {
    effectiveByteLimit = 0;
  }
  // Truncate to 16-bits and allow for a fair amount of overhead.
  // FIXME: Without the additional overhead I was getting missing messages at the client with
  // no obvious error path.
  byteLimit = std::min(byteLimit, 0xff00u);
  effectiveByteLimit = byteLimit ? std::min<int>(effectiveByteLimit, byteLimit) : effectiveByteLimit;
  uint16_t transferCount = estimateTransferCount(elementSize, effectiveByteLimit);
  if (transferCount > componentCount - offset)
  {
    transferCount = (uint16_t)(componentCount - offset);
  }

  msg.meshId = meshId;
  msg.offset = offset;
  msg.reserved = 0;
  msg.count = transferCount;

  //printf("MeshResource indices: %d : %d\n", msg.messageId, componentCount);
  unsigned write;
  write = msg.write(packet);
  // Jump to offset.
  dataSource += dataStride *offset;
  if (indexByteWidth == 1)
  {
    for (unsigned i = 0; i < transferCount; ++i)
    {
      index = *dataSource;
      write += unsigned(packet.writeElement(index));
      dataSource += dataStride;
    }
  }
  else if (indexByteWidth == 2)
  {
    for (unsigned i = 0; i < transferCount; ++i)
    {
      index = *reinterpret_cast<const uint16_t *>(dataSource);
      write += unsigned(packet.writeElement(index));
      dataSource += dataStride;
    }
  }
  else if (indexByteWidth == 4)
  {
    write += unsigned(packet.writeArray(reinterpret_cast<const uint32_t *>(dataSource), transferCount));
    dataSource += dataStride * transferCount;
  }

  return transferCount;
}


unsigned MeshResource::writeVectors3(PacketWriter &packet, uint32_t meshId,
                                     uint32_t offset, unsigned byteLimit,
                                     const uint8_t *dataSource, unsigned dataStride,
                                     uint32_t componentCount)
{
  return writeComponent<float, 3>(packet, meshId, offset, byteLimit, dataSource, dataStride, componentCount);
}


unsigned MeshResource::writeVectors2(PacketWriter &packet, uint32_t meshId,
                                     uint32_t offset, unsigned byteLimit,
                                     const uint8_t *dataSource, unsigned dataStride,
                                     uint32_t componentCount)
{
  return writeComponent<float, 2>(packet, meshId, offset, byteLimit, dataSource, dataStride, componentCount);
}


unsigned MeshResource::writeColours(PacketWriter &packet, uint32_t meshId,
                                    uint32_t offset, unsigned byteLimit,
                                    const uint8_t *dataSource, unsigned dataStride,
                                    uint32_t componentCount)
{
  return writeComponent<uint32_t>(packet, meshId, offset, byteLimit, dataSource, dataStride, componentCount);
}


 void MeshResource::nextPhase(TransferProgress &progress) const
 {
   int next = MmtFinalise;
   unsigned stride = 0;
   unsigned width = 0;
   switch (progress.phase)
   {
     // First call.
   case 0:
     if (vertexCount() && vertices(stride))
     {
       next = MmtVertex;
       break;
     }
     // Don't break.
     TES_FALLTHROUGH;
   case MmtVertex:
     if (indexCount() && indices(stride, width))
     {
       next = MmtIndex;
       break;
     }
     // Don't break.
     TES_FALLTHROUGH;
   case MmtIndex:
     if (vertexCount() && colours(stride))
     {
       next = MmtVertexColour;
       break;
     }
     // Don't break.
     TES_FALLTHROUGH;
   case MmtVertexColour:
     if (vertexCount() && normals(stride))
     {
       next = MmtNormal;
       break;
     }
     // Don't break.
     TES_FALLTHROUGH;
   case MmtNormal:
     if (vertexCount() && uvs(stride))
     {
       next = MmtUv;
       break;
     }
     // Don't break.
     TES_FALLTHROUGH;
   default:
     break;
   }

   progress.progress = 0;
   progress.phase = next;
 }


MeshSet::MeshSet(uint32_t id, uint16_t category, int partCount)
  : Shape(SIdMeshSet, id, category)
  , _parts(partCount ? new const MeshResource *[partCount] : nullptr)
  , _transforms(partCount ? new Matrix4f[partCount] : nullptr)
  , _partCount(partCount)
{
  if (partCount)
  {
    memset(_parts, 0, sizeof(*_parts) * partCount);
    for (int i = 0; i < partCount; ++i)
    {
      _transforms[i] = Matrix4f::identity;
    }
  }
}


MeshSet::MeshSet(const MeshResource *part, uint32_t id, uint16_t category)
  : Shape(SIdMeshSet, id, category)
  , _parts(new const MeshResource *[1])
  , _transforms(new Matrix4f[1])
  , _partCount(1)
{
  _parts[0] = part;
  _transforms[0] = Matrix4f::identity;
}


MeshSet::~MeshSet()
{
  delete[] _parts;
  delete[] _transforms;
}


bool MeshSet::writeCreate(PacketWriter &stream) const
{
  if (!Shape::writeCreate(stream))
  {
    return false;
  }

  ObjectAttributes attr;
  Quaternionf rot;
  Vector3f pos, scale;
  uint32_t partId;
  uint16_t numberOfParts = partCount();

  memset(&attr, 0, sizeof(attr));
  attr.colour = 0xffffffffu;

  stream.writeElement(numberOfParts);

  for (int i = 0; i < numberOfParts; ++i)
  {
    const MeshResource *mesh = _parts[i];
    if (mesh)
    {
      partId = mesh->id();
    }
    else
    {
      // Write a dummy.
      partId = 0;
    }

    transformToQuaternionTranslation(_transforms[i], rot, pos, scale);
    attr.position[0] = pos[0];
    attr.position[1] = pos[1];
    attr.position[2] = pos[2];
    attr.rotation[0] = rot[0];
    attr.rotation[1] = rot[1];
    attr.rotation[2] = rot[2];
    attr.rotation[3] = rot[3];
    attr.scale[0] = scale[0];
    attr.scale[1] = scale[1];
    attr.scale[2] = scale[2];

    stream.writeElement(partId);
    attr.write(stream);
  }

  return true;
}


int MeshSet::enumerateResources(const Resource **resources, int capacity, int fetchOffset) const
{
  if (!resources || !capacity)
  {
    return _partCount;
  }

  int copyCount = std::min<int>(capacity, _partCount - fetchOffset);
  if (copyCount <= 0)
  {
    return 0;
  }

  const MeshResource **src = _parts + fetchOffset;
  const Resource **dst = resources;
  for (int i = 0; i < copyCount; ++i)
  {
    *dst++ = *src++;
  }
  return copyCount;
}


Shape *MeshSet::clone() const
{
  MeshSet *copy = new MeshSet(_partCount);
  onClone(copy);
  return copy;
}


void MeshSet::onClone(MeshSet *copy) const
{
  Shape::onClone(copy);
  if (copy->_partCount != _partCount)
  {
    delete [] copy->_parts;
    delete [] copy->_transforms;
    if (_partCount)
    {
      copy->_parts = new const MeshResource*[_partCount];
      copy->_transforms = new Matrix4f[_partCount];
    }
    else
    {
      copy->_parts = nullptr;
      copy->_transforms = nullptr;
    }
  }
  memcpy(copy->_parts, _parts, sizeof(*_parts) * _partCount);
  memcpy(copy->_transforms, _transforms, sizeof(*_transforms) * _partCount);
}
