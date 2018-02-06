//
// author: Kazys Stepanas
//
#include "3esmeshresource.h"

#include "3esmeshmessages.h"
#include "3esrotation.h"
#include "3estransferprogress.h"

#include <algorithm>
#include <vector>

using namespace tes;

namespace
{
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
    uint16_t transferCount = MeshResource::estimateTransferCount(elementSize, effectiveByteLimit, int(sizeof(MeshComponentMessage)));
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
    dataSource += dataStride * offset;
    for (unsigned i = 0; i < transferCount; ++i)
    {
      const T *element = reinterpret_cast<const T *>(dataSource);
      write = unsigned(packet.writeArray(element, ELEMCOUNT));
      dataSource += dataStride;
    }

    return transferCount;
  }


 template <typename T, int ELEMSTRIDE = 1>
 bool readComponent(PacketReader &packet, MeshComponentMessage &msg, std::vector<T> &elements)
  {
    if (!msg.read(packet))
    {
      return false;
    }

    bool ok = true;
    elements.resize(msg.count * ELEMSTRIDE);
    ok = ok && packet.readArray(elements.data(), msg.count * ELEMSTRIDE) == msg.count * ELEMSTRIDE;

    return ok;
  }
}


int MeshResource::estimateTransferCount(size_t elementSize, unsigned byteLimit, int overhead)
{
  //                                    packet header           message                         crc
  const size_t maxTransfer = (0xffffu - (sizeof(PacketHeader) + overhead + sizeof(uint16_t))) / elementSize;
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
  uint16_t transferCount = estimateTransferCount(elementSize, effectiveByteLimit, int(sizeof(MeshComponentMessage)));
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

  // FIXME: should write the index width.
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


bool MeshResource::readCreate(PacketReader &packet)
{
  MeshCreateMessage msg;
  bool ok = true;
  ok = ok && msg.read(packet);
  return ok && processCreate(msg);
}


bool MeshResource::readTransfer(int messageType, PacketReader &packet)
{
  MeshComponentMessage msg;
  bool ok = false;

  switch (messageType)
  {
  case MmtVertex:
  {
    std::vector<float> verts;
    ok = readComponent<float, 3>(packet, msg, verts);
    ok = ok && processVertices(msg, verts.data(), unsigned(verts.size() / 3));
    break;
  }
  case MmtIndex:
  {
    // FIXME: should read the index width from packet.
    unsigned indexStride = 0, indexWidth = 0;
    indices(indexStride, indexWidth);
    if (indexStride && (indexWidth == 1 || indexWidth == 2 || indexWidth == 4))
    {
      switch (indexWidth)
      {
        case 1:
        {
          std::vector<uint8_t> indices;
          ok = readComponent<uint8_t>(packet, msg, indices);
          ok = ok && processIndices(msg, indices.data(), unsigned(indices.size()));
          break;
        }
        case 2:
        {
          std::vector<uint16_t> indices;
          ok = readComponent<uint16_t>(packet, msg, indices);
          ok = ok && processIndices(msg, indices.data(), unsigned(indices.size()));
          break;
        }
        case 4:
        {
          std::vector<uint32_t> indices;
          ok = readComponent<uint32_t>(packet, msg, indices);
          ok = ok && processIndices(msg, indices.data(), unsigned(indices.size()));
          break;
        }
      }
    }
    else
    {
      ok = false;
    }
    break;
  }
  case MmtVertexColour:
  {
    std::vector<uint32_t> colours;
    ok = readComponent<uint32_t>(packet, msg, colours);
    ok = ok && processColours(msg, colours.data(), unsigned(colours.size()));
    break;
  }
  case MmtNormal:
  {
    std::vector<float> normals;
    ok = readComponent<float, 3>(packet, msg, normals);
    ok = ok && processNormals(msg, normals.data(), unsigned(normals.size() / 3));
    break;
  }
  case MmtUv:
  {
    std::vector<float> uvs;
    ok = readComponent<float, 2>(packet, msg, uvs);
    ok = ok && processUVs(msg, uvs.data(), unsigned(uvs.size() / 2));
    break;
  }
  }

  if (msg.meshId != id())
  {
    ok = false;
  }

  return ok;
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


bool MeshResource::processCreate(const MeshCreateMessage &msg)
{
  return false;
}


bool MeshResource::processVertices(const MeshComponentMessage &msg, const float *vertices, unsigned vertexCount)
{
  return false;
}


bool MeshResource::processIndices(const MeshComponentMessage &msg, const uint8_t *indices, unsigned indexCount)
{
  return false;
}


bool MeshResource::processIndices(const MeshComponentMessage &msg, const uint16_t *indices, unsigned indexCount)
{
  return false;
}


bool MeshResource::processIndices(const MeshComponentMessage &msg, const uint32_t *indices, unsigned indexCount)
{
  return false;
}


bool MeshResource::processColours(const MeshComponentMessage &msg, const uint32_t *colours, unsigned colourCount)
{
  return false;
}


bool MeshResource::processNormals(const MeshComponentMessage &msg, const float *normals, unsigned normalCount)
{
  return false;
}


bool MeshResource::processUVs(const MeshComponentMessage &msg, const float *uvs, unsigned uvCount)
{
  return false;
}
