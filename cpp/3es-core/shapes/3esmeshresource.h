//
// author: Kazys Stepanas
//
#ifndef _3ESMESHRESOURCE_H_
#define _3ESMESHRESOURCE_H_

#include "3es-core.h"
#include "3esresource.h"

#include "3esmatrix4.h"

namespace tes
{
  /// Represents a mesh part or object. These are visualised via @c MeshSet,
  /// which may contain several @c MeshResource parts.
  class _3es_coreAPI MeshResource : public Resource
  {
  public:
    /// Virtual destructor.
    virtual ~MeshResource() {}

    /// Estimate the number of elements which can be transferred at the given @p byteLimit.
    /// @param elementSize The byte size of each element.
    /// @param byteLimit The maximum number of bytes to transfer. Note: a hard limit of 0xffff,
    ///   packet header size and CRC is enforced. Zero indices use of the hard limit.
    /// @param Additional byte overhead to account for.
    static int estimateTransferCount(size_t elementSize, unsigned byteLimit, int overhead = 0);

    /// Returns @c MtMesh
    uint16_t typeId() const override;

    virtual Matrix4f transform() const = 0;
    virtual uint32_t tint() const = 0;

    /// Returns the @c DrawType of the mesh.
    /// @param stream Reserved for future use.
    virtual uint8_t drawType(int stream = 0) const = 0;

    /// Returns the number of vertices in the mesh.
    /// @param stream Reserved for future use.
    /// @return The number of vertices.
    virtual unsigned vertexCount(int stream = 0) const = 0;

    /// Returns the number of indices in the mesh.
    /// @param stream Reserved for future use.
    /// @return The number of indices.
    virtual unsigned indexCount(int stream = 0) const = 0;

    /// Returns a pointer to the vertex stream. Each element is taken
    /// as a triple of single precision floats: (x, y, z).
    /// @param[out] stride The stride between vertex elements (bytes).
    ///   This would be 12 for a contiguous array of float triples.
    /// @param stream Reserved for future use.
    /// @return A pointer to the first vertex.
    virtual const float *vertices(unsigned &stride, int stream = 0) const = 0;

    /// Returns a pointer to the index stream. Supports different index widths.
    /// Expects @c indexCount(stream) elements or null if no indices.
    /// @param[out] stride Specifies the stride between index elements (bytes).
    ///   This would be 4 for a contiguous array of @c uint32_t indices.
    /// @param[out] Specifies the index byte width. Supports [1, 2, 4].
    /// @param stream Reserved for future use.
    /// @return A pointer to the first index.
    virtual const uint8_t *indices(unsigned &stride, unsigned &width, int stream = 0) const = 0;

    /// Returns a pointer to the normal stream. Each element is taken
    /// as a triple of single precision floats: (x, y, z). Expects
    /// @c vertexColour(stream) elements or null if no normals.
    /// @param[out] stride The stride between normal elements (bytes).
    ///   This would be 12 for a contiguous array of float triples.
    /// @param stream Reserved for future use.
    /// @return A pointer to the first normal or null
    virtual const float *normals(unsigned &stride, int stream = 0) const = 0;

    /// Returns a pointer to the UV stream. Each element is taken
    /// as a pair of single precision floats: (u, v). Expects
    /// @c vertexCount(stream) elements or null if no UVs.
    /// @param[out] stride The stride between UV elements (bytes).
    ///   This would be 8 for a contiguous array of float pairs.
    /// @param stream Reserved for future use.
    /// @return A pointer to the first UV coordinate or null.
    virtual const float *uvs(unsigned &stride, int stream = 0) const = 0;

    /// Returns a pointer to the colour stream. Each element is taken
    /// 32-bit integer. Expects  @c vertexCount(stream) elements or null
    /// if no vertex colours.
    ///
    /// @param[out] stride The stride between colour elements (bytes).
    ///   This would be 4 for a contiguous array of @c uint32_t colours.
    /// @param stream Reserved for future use.
    /// @return A pointer to the first colour value or null.
    virtual const uint32_t *colours(unsigned &stride, int stream = 0) const = 0;

    /// Populate a mesh creation packet.
    /// @param packet A packet to populate and send.
    /// @return Zero on success, an error code otherwise.
    int create(PacketWriter &packet) const override;

    /// Populate a mesh destroy packet.
    /// @param packet A packet to populate and send.
    /// @return Zero on success, an error code otherwise.
    int destroy(PacketWriter &packet) const override;

    /// Populate the next mesh data packet.
    ///
    /// The @c progress.phase is used to track which data array currently being transfered,
    /// from the various @c MeshMessageType values matching components (e.g., vertices, indices).
    /// The @p progress.progress value is used to track how many have been transfered.
    ///
    /// @param packet A packet to populate and send.
    /// @param byteLimit A nominal byte limit on how much data a single @p transfer() call may add.
    /// @param[in,out] progress A progress marker tracking how much has already been transfered, and
    ///     updated to indicate what has been added to @p packet.
    /// @return Zero on success, an error code otherwise.
    int transfer(PacketWriter &packet, int byteLimit, TransferProgress &progress) const override;

    /// Compose a mesh index component message in @p packet.
    ///
    /// This method composes a @c MeshComponentMessage for writing index data to @p packet.
    /// The @p packet must be reset before calling this method and finalised after calling.
    ///
    /// The number of indices written is limited by three factors:
    /// -# The available @p packet buffer space.
    /// -# The @p byteLimit
    /// -# The remaining indices to write from @p offset.
    /// All present hard limits to the number of indices written to @p packet. The number of indices
    /// written is reported in the return value.
    ///
    /// The index source address starts at @p dataSource plus @p dataStride times @p offset.
    /// The @p dataStride identifies the space between indices while @p indexByteWidth
    /// identifies the number of bytes in each integer index.
    ///
    /// A complete index set can be written by repeated calls, using the following code:
    /// <code>
    /// tes::Server *server = /* server pointer */;
    /// tes::PacketWriter packet(/* buffer */);
    /// const tes::MeshResource *mesh = /* initialise */;
    /// uint32_t offset = 0;
    /// unsigned dataStride, indexByteWidth;
    /// const uint8_t *indicesPtr = reinterpret_cast<const uint8_t *>(indices(dataStride, indexByteWidth));
    /// while (offset < mesh->indexCount())
    /// {
    ///   packet.reset(mesh->typeId(), MmtIndex);
    ///   offset += tes::MeshResource::writeIndices(
    ///                       packet, mesh->id(), offset,
    ///                       0xffffffffu, indicesPtr,
    ///                       dataStride, indexByteWidth,
    ///                       mesh->indexCount());
    ///   if (packet.finalise())
    ///   {
    ///     server->send(packet);
    ///   }
    /// }
    /// </code>
    ///
    /// @param packet Packet to write to.
    /// @param meshId The mesh to which the data belong.
    /// @param offset An index count offset to start writing from. See remarks.
    /// @param byteLimit A hard limit on the number of bytes to write.
    /// @param dataSource Base pointer for the index data.
    /// @param dataStride Number of bytes between indices.
    /// @param indexByteWidth The byte size of a single index. Generally equal to @p dataStride.
    /// @param componentCount The total number of indices.
    /// @return The number of indices written.
    static unsigned writeIndices(PacketWriter &packet, uint32_t meshId,
                                 uint32_t offset, unsigned byteLimit,
                                 const uint8_t *dataSource, unsigned dataStride,
                                 unsigned indexByteWidth, uint32_t componentCount);

    /// Compose a mesh float vector 3 component message in @p packet.
    ///
    /// This method composes a @c MeshComponentMessage for writing vector based data to @p packet.
    /// The @p packet must be reset before calling this method and finalised after calling.
    ///
    /// The behaviour and calling pattern matches @p writeIndices(), except that the size of
    /// each vector component is assumed to be three floats (12). The code below shows
    /// the call required to replace @p writeIndices() in that example code:
    /// <code>
    ///   packet.reset(mesh->typeId(), /* MmtVertex or MmtNormal */);
    ///   offset += tes::MeshResource::writeVectors3(
    ///                       packet, mesh->id(), offset,
    ///                       0xffffffffu, vectorPtr,
    ///                       dataStride, mesh->vertexCount());
    /// </code>
    ///
    /// @param packet Packet to write to.
    /// @param meshId The mesh to which the data belong.
    /// @param offset An vertex count offset to start writing from. See remarks.
    /// @param byteLimit A hard limit on the number of bytes to write.
    /// @param dataSource Base pointer for the vertex data.
    /// @param dataStride Number of bytes between indices.
    /// @param componentCount The total number of indices.
    /// @return The number of indices written.
    static unsigned writeVectors3(PacketWriter &packet, uint32_t meshId,
                                 uint32_t offset, unsigned byteLimit,
                                 const uint8_t *dataSource, unsigned dataStride,
                                 uint32_t componentCount);

    /// Compose a mesh float vector 2 component message in @p packet.
    ///
    /// This method is identical to @c writeVectors3(), except that it deals with vector 2
    /// components such as UV coordinates (@c MmtUv).
    /// @param packet Packet to write to.
    /// @param meshId The mesh to which the data belong.
    /// @param offset An vertex count offset to start writing from. See remarks.
    /// @param byteLimit A hard limit on the number of bytes to write.
    /// @param dataSource Base pointer for the vertex data.
    /// @param dataStride Number of bytes between indices.
    /// @param componentCount The total number of indices.
    /// @return The number of indices written.
    static unsigned writeVectors2(PacketWriter &packet, uint32_t meshId,
                                  uint32_t offset, unsigned byteLimit,
                                  const uint8_t *dataSource, unsigned dataStride,
                                  uint32_t componentCount);

    /// Compose a mesh component message for writing colour data in @p packet.
    ///
    /// This method is very similar to @c writeIndices() and @c writeVectors3(), except that it
    /// is intended for 4-byte colour values. Example call below:
    /// <code>
    ///   packet.reset(mesh->typeId(), MmtVertexColour);
    ///   offset += tes::MeshResource::writeColours(
    ///                       packet, mesh->id(), offset,
    ///                       0xffffffffu, coloursPtr,
    ///                       dataStride, mesh->vertexCount());
    /// </code>
    /// @param packet Packet to write to.
    /// @param meshId The mesh to which the data belong.
    /// @param offset An vertex count offset to start writing from. See remarks.
    /// @param byteLimit A hard limit on the number of bytes to write.
    /// @param dataSource Base pointer for the vertex data.
    /// @param dataStride Number of bytes between indices.
    /// @param componentCount The total number of indices.
    /// @return The number of indices written.
    static unsigned writeColours(PacketWriter &packet, uint32_t meshId,
                                 uint32_t offset, unsigned byteLimit,
                                 const uint8_t *dataSource, unsigned dataStride,
                                 uint32_t componentCount);

  protected:
    virtual void nextPhase(TransferProgress &progress) const;
  };
}

#endif // _3ESMESHRESOURCE_H_
