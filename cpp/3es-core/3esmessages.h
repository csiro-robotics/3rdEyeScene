//
// author: Kazys Stepanas
//
#ifndef _3ESMESSAGES_H_
#define _3ESMESSAGES_H_

#include "3es-core.h"

#include "3espacketreader.h"
#include "3espacketwriter.h"

#include <cinttypes>
#include <cstring>

namespace tes
{
  /// List of routing IDs of common, built in message handlers.
  /// These map to the @c MessageHandler::routingId() member.
  ///
  /// Limited to 2^16 - 1.
  enum MessageTypeIDs
  {
    MtNull,
    MtServerInfo,
    MtControl,
    MtCollatedPacket,

    // TODO: Move MtMesh and MtMaterial into a resources set.
    MtMesh,
    MtCamera,
    MtCategory,
    /// Extension. NYI.
    MtMaterial,

    /// First ID for renderers.
    ShapeHandlersIDStart = 64,
    /// First user ID
    UserIDStart = 2048
  };

  /// Default/built in renderers (routing IDs).
  enum ShapeHandlerIDs
  {
    SIdSphere = ShapeHandlersIDStart,
    SIdBox,
    SIdCone,
    SIdCylinder,
    SIdCapsule,
    SIdPlane,
    SIdStar,
    SIdArrow,
    SIdMeshShape,
    SIdMeshSet,
    SIdPointCloud,
    SIdText3D,
    SIdText2D,

    SIdBuiltInLast = SIdText2D
  };

  /// Message IDs for a @c ControlMessage.
  enum ControlId
  {
    CIdNull,
    CIdFrame,           ///< Defines a new frame. Value is the delta time in microseconds.
    CIdCoordinateFrame, ///< Specifies the a change in coordinate frame view.
    CIdFrameCount,      ///< Set the total number of frames to expect. More for serialised streams.
    CIdForceFrameFlush, ///< Forces a frame update (render) without advancing the time.
    CIdReset,           ///< Clear the scene. This drops all existing data.
  };

  /// Message IDs for @c MtCategory routing.
  enum CategoryMessageId
  {
    CMIdName, ///< Category name definition.
  };

  /// Object/shape management message ID. Used with @c ShapeHandlerIDs routing IDs.
  enum ObjectMessageId
  {
    OIdNull,
    OIdCreate,
    OIdUpdate,
    OIdDestroy,
    OIdData
  };

  /// Flags controlling the creation and appearance of an object.
  enum
  {
    OFNone = 0, ///< No flags. Default appearance.
    OFWire = (1 << 0), ///< Show the object as a wireframe mesh.
    OFTransparent = (1 << 1), ///< The object supports transparency. Use the colour alpha channel.
    OFTwoSided = (1 << 2),  ///< Use a two sided shader.

    OFUpdateMode = (1 << 3),  ///< Update attributes using only explicitly specified flags from the following.
    OFPosition = (1 << 4),    ///< Update position data.
    OFRotation = (1 << 5),    ///< Update rotation data.
    OFScale = (1 << 6),       ///< Update scale data.
    OFColour = (1 << 7),      ///< Update colour data.
    OFColor = OFColour,       ///< Spelling alias for colour.

    OFUser = (1 << 12)        ///< Use flags start here.
  };

  /// Additional attributes for point data sources.
  enum PointsAttributeFlag
  {
    PAFNone = 0,  ///< No additional data (points only)
    PAFNormals = (1 << 0), ///< Per point normals.
    PAFColours = (1 << 1) ///< Per point colours.
  };

  /// @c ObjectFlag extensions for Text2D rendering.
  enum Text2DFlag
  {
    Text2DFWorldSpace = OFUser  ///< Position is given in world space and mapped to screen space.
                                ///< Otherwise in screen space with (0, 0, z) at the top left corner
                                ///< and (1, 1, z) at the bottom right.
  };

  /// @c ObjectFlag extensions for Text2D rendering.
  enum Text3DFlag
  {
    Text3DFScreenFacing = OFUser  ///< Text is oriented to face the screen.
  };

  /// @c ObjectFlag extensions for @c MeshShape.
  enum MeshShapeFlag
  {
    MeshShapeCalculateNormals = OFUser  ///< Calculate normals and rendering with lighting.
  };

  /// Flags controlling the creation and appearance of an object.
  enum UpdateFlag
  {
    UFNone = 0, ///< No flags. Default appearance.
    /// Transition to the new position, colour, etc., is interpolated over the render frames.
    /// This is only used if the render frame rate of this application is higher than that of
    /// the incoming data.
    UFInterpolate = (1 << 0)
  };

  /// Flags for @c CollatedPacketMessage.
  enum CollatedPacketFlag
  {
    CPFCompress = (1<<0),
  };

  /// Flags for various @c ControlId messages.
  enum ControlFlag
  {
    /// Flag for @c CIdFrame indicating transient objects should be maintained and not flushed for this frame.
    CFFramePersist = (1 << 0),
  };

  /// Information about the server. This is sent to clients on connection.
  /// Defines global settings for this server.
  struct _3es_coreAPI ServerInfoMessage
  {
    /// Specifies the time unit in a @c CIdFrame @c ControlMessage.
    /// Each unit in the value is scaled by this time unit.
    ///
    /// This value is specified in micro-seconds.
    ///
    /// The default is 1000us (1 millisecond).
    uint64_t timeUnit;
    /// The default time delta between frames to used when none is specified.
    /// Only used in replay.
    ///
    /// This value is specified in the @c timeUnit.
    ///
    /// The default is 33ms (1/30s).
    uint32_t defaultFrameTime;
    /// Specifies the @c CoordinateFrame used by this server.
    ///
    /// The default is @c XYZ.
    uint8_t coordinateFrame;
    /// Reserved for future use. Must be zero.
    /// Aiming to pad out to a total of 64-bytes in the packet.
    uint8_t reserved[35];

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(timeUnit) == sizeof(timeUnit) && ok;
      ok = reader.readElement(coordinateFrame) == sizeof(coordinateFrame) && ok;
      ok = reader.readArray(reserved, sizeof(reserved) / sizeof(reserved[0])) == sizeof(reserved) / sizeof(reserved[0]) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(timeUnit) == sizeof(timeUnit) && ok;
      ok = writer.writeElement(defaultFrameTime) == sizeof(defaultFrameTime) && ok;
      ok = writer.writeElement(coordinateFrame) == sizeof(coordinateFrame) && ok;
      ok = writer.writeArray(reserved, sizeof(reserved) / sizeof(reserved[0])) == sizeof(reserved) / sizeof(reserved[0]) && ok;
      return ok;
    }
  };

  /// Initialise @p info to the defaults.
  /// @param info The structure to initialise. Must not be null.
  void _3es_coreAPI initDefaultServerInfo(ServerInfoMessage *info);

  /// A system control message.
  struct _3es_coreAPI ControlMessage
  {
    /// Flags particular to this type of control message.
    uint32_t controlFlags;
    /// 32-bit value particular to this type of control message.
    uint32_t value32;
    /// 32-bit value particular to this type of control message.
    uint64_t value64;

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(controlFlags) == sizeof(controlFlags) && ok;
      ok = reader.readElement(value32) == sizeof(value32) && ok;
      ok = reader.readElement(value64) == sizeof(value64) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(controlFlags) == sizeof(controlFlags) && ok;
      ok = writer.writeElement(value32) == sizeof(value32) && ok;
      ok = writer.writeElement(value64) == sizeof(value64) && ok;
      return ok;
    }
  };

  /// Category name message.
  struct _3es_coreAPI CategoryNameMessage
  {
    /// ID for this message.
    enum { MessageId = CMIdName };
    /// Identifies the category for the message.
    uint16_t categoryId;
    /// The (new) parent category for @c categoryId. Zero for none.
    uint16_t parentId;
    /// Default @c categoryId to active? Non zero for yes (1).
    uint16_t defaultActive;
    /// Number of bytes in @c name, excluding a null terminator.
    uint16_t nameLength;
    /// The name string @em without a null terminator. Must be exactly
    /// @c nameLength bytes.
    const char *name;

    /// Read message content.
    /// @param reader The stream to read from.
    /// @return True on success, false if there is an issue with amount
    ///   of data available.
    inline bool read(PacketReader &reader, char *nameBuffer, size_t nameBufferSize)
    {
      bool ok = true;
      ok = reader.readElement(categoryId) == sizeof(categoryId) && ok;
      ok = reader.readElement(parentId) == sizeof(parentId) && ok;
      ok = reader.readElement(defaultActive) == sizeof(defaultActive) && ok;
      ok = reader.readElement(nameLength) == sizeof(nameLength) && ok;
      name = nameBuffer;

      if (!nameBuffer && nameLength || nameLength + 1 > nameBufferSize)
      {
        return false;
      }

      reader.readRaw((uint8_t *)nameBuffer, nameLength);
      nameBuffer[nameLength] = '\0';

      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      uint16_t nameLength = (uint16_t)((name) ? strlen(name) : 0);
      ok = writer.writeElement(categoryId) == sizeof(categoryId) && ok;
      ok = writer.writeElement(parentId) == sizeof(parentId) && ok;
      ok = writer.writeElement(defaultActive) == sizeof(defaultActive) && ok;
      ok = writer.writeElement(nameLength) == sizeof(nameLength) && ok;
      if (nameLength > 0)
      {
        if (name)
        {
          ok = writer.writeRaw((const uint8_t *)name, nameLength) == nameLength && ok;
        }
        else
        {
          ok = false;
        }
      }
      return ok;
    }
  };

  /// A packet collation message header.
  struct _3es_coreAPI CollatedPacketMessage
  {
    /// Message flags. See @c CollatedPacketFlag.
    uint16_t flags;
    /// Reserved: must be zero.
    uint16_t reserved;
    /// Number of uncompressed bytes in the payload.
    uint32_t uncompressedBytes;

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(flags) == sizeof(flags) && ok;
      ok = reader.readElement(reserved) == sizeof(reserved) && ok;
      ok = reader.readElement(uncompressedBytes) == sizeof(uncompressedBytes) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(flags) == sizeof(flags) && ok;
      ok = writer.writeElement(reserved) == sizeof(reserved) && ok;
      ok = writer.writeElement(uncompressedBytes) == sizeof(uncompressedBytes) && ok;
      return ok;
    }
  };

  /// Contains core object attributes. This includes details
  /// of the model transform and colour.
  struct _3es_coreAPI ObjectAttributes
  {
    uint32_t colour;    ///< Initial object colour.
    float position[3];  ///< Object position.
    float rotation[4];  ///< Object rotation (quaternion)
    float scale[3];     ///< Object scale.

    /// Set to an identity transform coloured white.
    inline void identity()
    {
      colour = 0xffffffffu;
      position[0] = position[1] = position[2] =
        rotation[0] = rotation[1] = rotation[2] = 0.0f;
      rotation[3] = 1.0f;
      scale[0] = scale[1] = scale[2] = 1.0f;
    }

    /// Read this message from @p reader.
    /// @param reader The data source.
    /// @return True on success.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(colour) == sizeof(colour) && ok;
      ok = reader.readArray(&position[0], 3) == 3 && ok;
      ok = reader.readArray(&rotation[0], 4) == 4 && ok;
      ok = reader.readArray(&scale[0], 3) == 3 && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(colour) == sizeof(colour) && ok;
      ok = writer.writeArray(&position[0], 3) == 3 && ok;
      ok = writer.writeArray(&rotation[0], 4) == 4 && ok;
      ok = writer.writeArray(&scale[0], 3) == 3 && ok;
      return ok;
    }
  };

  /// Defines an object creation message. This is the message header and the type
  /// specific payload follows.
  struct _3es_coreAPI CreateMessage
  {
    /// ID for this message.
    enum { MessageId = OIdCreate };

    uint32_t id;        ///< Id of the object to create. Zero for transient objects.
    uint16_t category;  ///< Object categorisation. Used to control visibility.
    uint16_t flags;     ///< Flags controlling the appearance and creation of the object.
    uint16_t reserved;  ///< Reserved for future use.
    ObjectAttributes attributes;  ///< Initial transformation and colour.

    /// Read message content.
    /// Crc should have been validated already
    /// @param reader The stream to read from.
    /// @return True on success, false if there is an issue with amount
    ///   of data available.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(id) == sizeof(id) && ok;
      ok = reader.readElement(category) == sizeof(category) && ok;
      ok = reader.readElement(flags) == sizeof(flags) && ok;
      ok = reader.readElement(reserved) == sizeof(reserved) && ok;
      ok = attributes.read(reader) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(id) == sizeof(id) && ok;
      ok = writer.writeElement(category) == sizeof(category) && ok;
      ok = writer.writeElement(flags) == sizeof(flags) && ok;
      ok = writer.writeElement(reserved) == sizeof(reserved) && ok;
      ok = attributes.write(writer) && ok;
      return ok;
    }
  };

  /// Defines an object data message. This is for complex shapes to send
  /// additional creation data piecewise. Not supported for transient shapes.
  ///
  /// This is the message header and the payload follows.
  struct _3es_coreAPI DataMessage
  {
    /// ID for this message.
    enum { MessageId = OIdData };

    uint32_t id;        ///< Id of the object to update data.

    /// Read message content.
    /// Crc should have been validated already
    /// @param reader The stream to read from.
    /// @return True on success, false if there is an issue with amount
    ///   of data available.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(id) == sizeof(id) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(id) == sizeof(id) && ok;
      return ok;
    }
  };

  /// A update message is identical in header to a @c CreateMessage. It's payload
  /// may vary and in many cases it will have no further payload.
  struct _3es_coreAPI UpdateMessage
  {
    /// ID for this message.
    enum { MessageId = OIdUpdate };

    uint32_t id;        ///< Object creation id. Zero if defining a transient/single frame message.
    uint16_t flags;     ///< Update flags from @c ObjectFlag.
    ObjectAttributes attributes;  ///< Initial transformation and colour.

    /// Read message content.
    /// Crc should have been validated already
    /// @param reader The stream to read from.
    /// @return True on success, false if there is an issue with amount
    ///   of data available.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(id) == sizeof(id) && ok;
      ok = reader.readElement(flags) == sizeof(flags) && ok;
      ok = attributes.read(reader) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(id) == sizeof(id) && ok;
      ok = writer.writeElement(flags) == sizeof(flags) && ok;
      ok = attributes.write(writer) && ok;
      return ok;
    }
  };

  /// Message to destroy an exiting object by id and type.
  struct _3es_coreAPI DestroyMessage
  {
    /// ID for this message.
    enum { MessageId = OIdDestroy };

    uint32_t id;        ///< Id of the object to destroy, matching the @c CreateMessage id.

    /// Read message content.
    /// Crc should have been validated already
    /// @param reader The stream to read from.
    /// @return True on success, false if there is an issue with amount
    ///   of data available.
    inline bool read(PacketReader &reader)
    {
      bool ok = true;
      ok = reader.readElement(id) == sizeof(id) && ok;
      return ok;
    }

    /// Write this message to @p writer.
    /// @param writer The target buffer.
    /// @return True on success.
    inline bool write(PacketWriter &writer) const
    {
      bool ok = true;
      ok = writer.writeElement(id) == sizeof(id) && ok;
      return ok;
    }
  };
}

#endif // _3ESMESSAGES_H_
