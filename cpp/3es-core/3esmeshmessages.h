//
// author: Kazys Stepanas
//
#ifndef _3ESMESHHANDLERMESSAGES_H_
#define _3ESMESHHANDLERMESSAGES_H_

#include "3es-core.h"

#include "3esmessages.h"

#include "3espacketreader.h"
#include "3espacketwriter.h"

/// @ingroup tescpp
/// @defgroup meshmsg MeshResource Messages
/// Defines the set of messages used to construct mesh objects.
///
/// A mesh object is defined via a series of messages. This allows
/// meshes to be defined over a number of updates, limiting per frame
/// communications.
///
/// MeshResource instantiation supports the following messages:
/// - Create : instantiates a new, empty mesh object and the draw type.
/// - Destroy : destroys an existing mesh object.
/// - Vertex : adds vertices to a mesh object.
/// - Vertex colour : adds vertex colours.
/// - Index : Defines the vertex indices. Usage depends on draw type.
/// - Normal : adds normals.
/// - UV : Adds UV coordinates.
/// - Set material : Sets the material for the mesh object.
/// - Finalise : Finalises the mesh object.
///
/// Within a @c PacketHeader, the mesh message is arranged as follows:
/// - PacketHeader header
/// - uint16 Message type = @c MtMesh
/// - uint16 @c MeshMessageType
///
/// A valid mesh definition requires at least the following messages:
/// Create, Vertex, Index, Finalise. Additional vertex streams, normals, etc
/// can be added with the complete set of messages.
///
/// Each mesh definition specifies one of the following draw modes or primitive types:
/// - DtPoints
/// - DtLines
/// - DtLineLoop
/// - DtLineStrip
/// - DtTriangles
/// - DtTriangleStrip
/// - DtTriangleFan
///
/// A mesh object defined through the @c MeshHandler doe snot support any
/// child or sub-objects. These sorts of relationships are defined in the
/// mesh renderer.
///
/// @par Message Formats
/// | Message   | Data Type  | Semantics                              |
/// | --------- | ---------- | -------------------------------------- |
/// | Create    | uint32     | Unique mesh ID                         |
/// |           | uint32     | Vertex count                           |
/// |           | uint32     | Index count                            |
/// |           | uint8      | Draw type                              |
/// |           | uint32     | MeshResource tint                      |
/// |           | float32[3] | Position part of the mesh transform    |
/// |           | float32[4] | Quaternion rotation for mesh transform |
/// |           | float32[3] | Scale factor part of mesh transform    |
/// | Destroy   | uint32     | MeshResource ID                        |
/// | Finalise  | uint32     | MeshResource ID                        |
/// | Component | uint32     | MeshResource ID                        |
/// |           | uint32     | Offset of the first data item          |
/// |           | uint32     | Reserved (e.g., stream index support)  |
/// |           | uint16     | Count                                  |
/// |           | element*   | Array of count elements. Type varies.  |
/// | Material  | uint32     | MeshResource ID                        |
/// |           | uint32     | Material ID                            |
///
/// The @c Component message above refers to of the data content messages.
/// The offset specicifies the first index of the incomping data, which
/// allows the data streams to be sent in blocks. The element type matches
/// the component type as follows:
///
/// | Component Message | Element type         |
/// | ----------------- | -------------------- |
/// | Vertex            | 32-bit float triples |
/// | Vertex colour     | 32-bit uint          |
/// | Index             | 32-bit uint          |
/// | Normal            | 32-bit float triples |
/// | UV                | 32-bit float pairs   |
///
/// By default, one of the following materials are chosen:
/// - Lit with vertex colour if normals are specified or calculated.
/// - Unlit with vertex colour otherwise.
/// Vertex colours are initialised to white.

namespace tes
{
  /// @ingroup meshmsg
  /// The set of valid flags used in finalise messages.
  enum MeshBuildFlags
  {
    /// Calculate normals. Overwrites normals if present.
    MbfCalculateNormals = (1<<0)
  };

  /// @ingroup meshmsg
  /// Defines the messageIDs for mesh message routing.
  enum MeshMessageType
  {
    MmtInvalid,
    MmtDestroy,
    MmtCreate,
    /// Add vertices
    MmtVertex,
    /// Add indices
    MmtIndex,
    /// Add vertex colours.
    MmtVertexColour,
    /// Add normals
    MmtNormal,
    /// Add UV coordinates.
    MmtUv,
    /// Define the material for this mesh.
    /// Extension. NYI.
    MmtSetMaterial,
    /// Redefine the core aspects of the mesh. This invalidates the mesh
    /// requiring re-finalisation, but allows the creation parameters to
    /// be redefined. Component messages (vertex, index, colour, etc) can
    /// also be changed after this message, but before a second @c MmtFinalise.
    MmtRedefine,
    /// Finalise and build the mesh
    MmtFinalise
  };

  /// @ingroup meshmsg
  /// Defines the primitives for a mesh.
  enum DrawType
  {
    DtPoints,
    DtLines,
    DtTriangles,
    //DtQuads,
    //DtLineLoop,
  };

  /// @ingroup meshmsg
  /// MeshResource creation message.
  struct MeshCreateMessage
  {
    /// ID for this message.
    enum { MessageId = MmtCreate };

    uint32_t meshId;      ///< Mesh resource ID.
    uint32_t vertexCount; ///< Total count.
    uint32_t indexCount;  ///< Total index count.
    uint8_t drawType;     ///< Topology: see @c DrawType.
    ObjectAttributes attributes;  ///< Core attributes.

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(meshId) == sizeof(meshId) && ok;
      ok = reader.readElement(vertexCount) == sizeof(vertexCount) && ok;
      ok = reader.readElement(indexCount) == sizeof(indexCount) && ok;
      ok = reader.readElement(drawType) == sizeof(drawType) && ok;
      ok = attributes.read(reader) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(meshId) == sizeof(meshId) && ok;
      ok = writer.writeElement(vertexCount) == sizeof(vertexCount) && ok;
      ok = writer.writeElement(indexCount) == sizeof(indexCount) && ok;
      ok = writer.writeElement(drawType) == sizeof(drawType) && ok;
      ok = attributes.write(writer) && ok;
      return ok;
    }
  };

  /// @ingroup meshmsg
  /// MeshResource redefinition message.
  struct MeshRedefineMessage : MeshCreateMessage
  {
    /// ID for this message.
    enum { MessageId = MmtRedefine };
  };

  /// @ingroup meshmsg
  /// MeshResource destruction message.
  struct MeshDestroyMessage
  {
    /// ID for this message.
    enum { MessageId = MmtDestroy };

    uint32_t meshId;

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(meshId) == sizeof(meshId);
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(meshId) == sizeof(meshId);
      return ok;
    }
  };

  /// @ingroup meshmsg
  /// Message structure for adding vertices, colours, indices, or UVs.
  struct MeshComponentMessage
  {
    uint32_t meshId;
    uint32_t offset;
    uint32_t reserved;
    uint16_t count;

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(meshId) == sizeof(meshId) && ok;
      ok = reader.readElement(offset) == sizeof(offset) && ok;
      ok = reader.readElement(reserved) == sizeof(reserved) && ok;
      ok = reader.readElement(count) == sizeof(count) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(meshId) == sizeof(meshId) && ok;
      ok = writer.writeElement(offset) == sizeof(offset) && ok;
      ok = writer.writeElement(reserved) == sizeof(reserved) && ok;
      ok = writer.writeElement(count) == sizeof(count) && ok;
      return ok;
    }
  };

  /// @ingroup meshmsg
  /// Not ready for use.
  struct Material
  {
    /// ID for this message.
    enum { MessageId = MmtSetMaterial };

    uint32_t meshId;
    uint32_t materialId;

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(meshId) == sizeof(meshId) && ok;
      ok = reader.readElement(materialId) == sizeof(materialId) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(meshId) == sizeof(meshId) && ok;
      ok = writer.writeElement(materialId) == sizeof(materialId) && ok;
      return ok;
    }
  };

  /// @ingroup meshmsg
  /// Message to finalise a mesh, ready for use.
  struct MeshFinaliseMessage
  {
    /// ID for this message.
    enum { MessageId = MmtFinalise };

    uint32_t meshId;
    uint32_t flags; ///< @c MeshBuildFlags

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(meshId) == sizeof(meshId) && ok;
      ok = reader.readElement(flags) == sizeof(flags) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(meshId) == sizeof(meshId) && ok;
      ok = writer.writeElement(flags) == sizeof(flags) && ok;
      return ok;
    }
  };
}

#endif // _3ESMESHHANDLERMESSAGES_H_
