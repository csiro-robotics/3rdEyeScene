// Documentation of the messagging protocol.

/*!
@page docprotocol Messaging Protocol
3<sup>rd</sup> Eye Scene debugging is built on top of a core messaging system used to expose 3D data for external viewing. This section details the packet header used for all messages, documents the core messages and details compression.

The primary transport layer is intended to be TCP/IP, however other protocols may be used in future. For consistency, all data elements are in <a href="https://en.wikipedia.org/wiki/Endianness">Network Byte Order (Big Endian)</a>. Many common platforms are Little Endian and will required byte order swaps before using data values; e.g., Intel based processors are generally Little Endian.

@section Protocol Version
This page documents version 0.1 of the 3<sup>rd</sup> Eye Scene protocol. Changes to the major version number indicate a breaking change or an introduction of major features in the core protocol. Point version changes indicate minor changes such as the introduction of new flag values.

The protocol version number will be at least 1.0 on the initial release of the 3<sup>rd</sup> Eye Scene viewer regardless of whether or not the protocol contains actual changes from the previous version.


@section secheader Packet Header
All messages begin with a standard @ref secheader "packet header". This header begins with a common, 4-byte marker identifying the packet start, followed by the remaining header, a message payload and in most cases a 2-byte CRC. The packet header also contains protocol version details, routing and message IDs, payload size details and a small number of packet flags.

The routing and message IDs are used to identify how the packet is to be handled. Conceptually, the routing ID identifies the recipient while the message ID identifies the packet content. Routing IDs must uniquely identify the receiver, while message IDs have different meanings depending on the routing ID.

The packet header is layed out as follows (16-bytes).
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Marker            | 4         | Identifies the start of a packet. Always the hex value 0x03e55e30.
Major version     | 2         | The major version of the message protocol. With the minor version, This identifies the packet header and message contents and layout.
Minor version     | 2         | The minor version of the message protocol.
Routing ID        | 2         | Identifies the message recipient. See @ref secroutingids.
Message ID        | 2         | The message ID identifies the payload contents to the handler.
Payload size      | 2         | Size of the payload after this header. Excludes header size and CRC bytes.
Payload offset    | 1         | For future use. Identifies a byte offset after this header where the payload begins. Initially always zero, in future this may be used to put additional data into the packet header.
Flags             | 1         | Packet flags. See below.

The initial protocol version is 0.1.

The @c Flags member supports the following flags:
Flag              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
No CRC            | 1         | The packet does not include a CRC after the header and payload.

The header is followed by the message payload separated by the number of bytes specified in <tt>Payload offset</tt>. The offset should always be zero in the original protocol with the message payload immediately following the packer header.

The payload is followed by a 2-byte CRC, unless the <tt>No CRC</tt> flag is set. This CRC is calculated over the entire packet header and message content.



@section secroutingids Core Routing IDs
The 3<sup>rd</sup> Eye Scene core supports the following routing IDs and message handlers.

Name              | RoutingID | Description
----------------- | --------: | -------------------------------------------------------------------
Null              | 0         | An invalid routing ID. Not used.
Server Info       | 1         | Handles messages about the server connection.
Control           | 2         | Used for control messages such as frame refresh.
Collated Packet   | 3         | Handles collated and and optionally compressed packet data.
Mesh              | 4         | Handles mesh resource messages. See TODO.
Camera            | 5         | Handles camera related messages.
Category          | 6         | Handles category and related messages.
Material          | 7         | Not implemented. Intended for future handling of material messages.
Shapes            | 64+       | Shape handlers share core message structures, but identify different 3D shapes and primitives.



@subsection secshapeids Shape Routing IDs
Shape handlers all use a common message structure (see TODO) using the same create, update and destroy messages. Shapes support a data message for sending additional data specific to that shape. Shapes have the following routing IDs.
Shape             | RoutingID | Description
----------------- | --------: | -------------------------------------------------------------------
Sphere            | 64        | A sphere primitive.
Box               | 65        | A box primitive.
Cone              | 66        | A cone primitive.
Cylinder          | 67        | A cylinder primitive.
Capsule           | 68        | A capsule primitive. A capsule is formed from two hemispheres connected by a cylinder. Often used as a physics primitive.
Plane             | 69        | A 2D quadrilateral positioned in 3D space with a normal component.Used to represent 3D planes at a point.
Star              | 70        | A star shape.
Arrow             | 71        | An arrow shape made from a conical head and cylindrical body.
Mesh Shape        | 72        | A single mesh object made up of vertices and indices with a specified draw type or topology.
Mesh Set          | 73        | A collection of mesh objects created via Mesh Routing ID. A mesh set supports more complex mesh structures than Mesh Set allowing multiple shared mesh resources. Meshes themselves support vertex colour and vertex normals.  See TODO "mesh resources".
Point Cloud       | 74        | Similar to a mesh set, a point cloud shape supports a single, shared point cloud mesh resource.
Text 3D           | 75        | Supports text with full 3D positioning and scale, with optional billboarding.
Text 2D           | 76        | Supports 2D text either located in screen space, or located in 3D and projected into screen space.



@section secserverinfomsg Server Info Messages
Server info messages provide information about the server. There is currently only one server message, message ID zero. It is detailed below.

Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Time Unit         | 8         | Specifies the time unit used in frame messages. That is, each time unit in other messages is scaled by this value. This value is measured in microseconds and defaults to 1000us (1 milliscond).
Default Frame Time| 4         | Specifies the default time to display a frame for when an end frame message does not specify a time value (zero). Generally not relevant for real time visualisation as there should always be a real frame
                  |           | time in this case. This value is scaled by the <tt>Time Unit</tt> and defaults to 33(ms).
Coordinate Frame  | 1         | Specifies the server's coordinate frame. See below.
Reserved          | 35        | Additionaly bytes reserved for future expansion. These pad the structure to 48-bytes, which pads to 64-bytes when combined with the packet header. All bytes must be zero for correct CRC.

The coordinate frame identifies the server's basis axes. It is set from one of the following values. The coordinate frame name identifies the right, forward and up axes in turn. For example XYZ, specifies the X axis as right, Y as forward and Z as up and is right handed. Similarly XZY specifies X right, Z forward, Y up and is left handed. Some axes are prefixed with a '-' sign. This indicates the axis is flipped. 
Frame Name        | Value     | Left/Right Handed
----------------- | --------: | -----------------
XYZ               | 0         | Right
XZ-Y              | 1         | Right
YX-Z              | 2         | Right
YZX               | 3         | Right
ZXY               | 4         | Right
ZY-X              | 5         | Right
XY-Z              | 6         | Left
XZY               | 7         | Left
YXZ               | 8         | Left
YZ-X              | 9         | Left
ZX-Y              | 10        | Left
ZYX               | 11        | Left



@section seccontrolmsg Control Messages
Control messages are special commands to the client. The most commonly used control message is the @c Frame message which identifies an "end of frame" event and causes the client to display expire transient shapes and dispaly new shapes shapes. All control messages have the same message structure, but the semantics of the message content vary.

Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Flags             | 4         | Flags for the control message. Semantics vary.
Value 32          | 4         | A 32-bit value. Semantics vary.
Value 64          | 8         | A 64-bit value. Semantics vary.

Valid control messages and their value and flag semantics are:
Name              | MessageID | Description
----------------- | --------: | -------------------------------------------------------------------
Null              | 0         | An invalid message ID. Not used.
Frame             | 1         | An end of frame message causing a frame flush. Uses flags (see TODO). Value 32 stores the frame time in the server time unit (see @ref secserverinfomsg) or zero to use the default frame time.
Frame Count       | 3         | Value 32 specifies the total number of frames. Intended only for recorded file streams to identify the target frame count.
Force Frame Flush | 4         | Force a frame flush without advancing the frame number or time. Intended as an internal control message only on the client. Values not used.
Reset             | 5         | Clear the scene, dropping all existing data. Values not used.



@subsection seccontrolframemsg Frame Message Flags
Frame message flags are:
Name              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
Persist           | 1         | Request that transient objects persist and are not flushed this frame.



@section seccollatedmsg Collated Packet Messages
Collated packet rounting has one valid message ID (zero), and identifies the packet payload as containing additional packet headers and packet data, optionally GZIP compressed. Collated packets serve two purposes:
-# Collating messages into a single chunk, generally for thread safety.
-# To support packet compression.

The collated packet message has the following payload:
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Flags             | 2         | See @ref seccollatedflags
Reserved          | 2         | Reserved for future use (padding)
Uncompressed Bytes| 4         | Specifies the number of @em uncompressed bytes in the payload. This is the sum of all the payload's individual packets (excluding the message header).

It is important to note that collated packet sizes are generally limited to less than 65535 bytes due to the packet payload size restriction with one important exception (see below). When a server creates collated packets, it is free to add as much data as possible to reach the payload limit, but must not contain partial packets. That is, a packet contained in a collated packet must be wholly contained in that packet regardless of compression settings.

There is an exception to the size limitation which caters to how a collated packet is used in file saving. File streams may contain a collated packet which exceeds the packet size limitation. To support this, it cannot have a CRC as the CRC location cannot be calculated based on a 16-bit payload size in the collated packet header. The collated packet can break the size restrictions by expressing that it contains more than 65535 uncompressed bytes.

Collated packet data begins immediately following the collated packet header and message (above) and is expressed as a series of valid packet headers with their corresponding message payloads. These bytes may be GZIP compressed as specified by the collated packet message flags. A collated packet reader knows it has reached the end of the collated data when it has processed a number of uncompressed bytes equal to <tt>Uncompressed Bytes</tt> as specified in the collated packet message (this includes CRCs if present for the contained packets). The next two bytes will either be the CRC for the collated packet message (if present) or the beginning of the next packet, as identified by the packet header marker bytes.   



@subsection seccollatedflags Collated Packet Flags
Collated packet message flags are:
Name              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
Compress          | 1         | Collated packet payload is GZIP compressed. Compression begins after the message structure. That is, neither packet header nor the message are compressed. 


@section secmeshmsg Mesh Resource Messages
Mesh resource messages are used to create and populate mesh resources for viewing by the client.  These resources may be referenced in other messages such as those pertaining to the Mesh Set shape type. Mesh resources are identified by an unique mesh resource ID. This ID is only unique among other mesh resources. It is the server's responsibility to ensure unique resource ID assignment.

Valid mesh resource messages are:
Name              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
Invalid           | 0         | Not used.
Destroy           | 1         | Message to destroy a mesh resource.
Create            | 2         | Message to create a mesh resource.
Vertex            | 3         | Transfer of 3D vertex data.
Index             | 4         | Transfer of index data.
Vertex Colour     | 5         | Transfer of per vertex colour data.
Normal            | 6         | Transfer of per vertex normal data data.
UV                | 7         | Transfer of per vertex UV data. Not supported yet.
Set Material      | 8         | Identifies the material resource(s) for the mesh. Not supported yet.
Redefine          | 9         | Prepares the mesh for modification. New vertex, per vertex or index data may be incoming.
Finalise          | 10        | Finalises the mesh resource, making it ready for viewing.

The vertex, index, vertex colour, normal and UV messages all share a common message header followed by specific data arrays (see below). All other messages are unique.



@subsection secmeshmsgdestroy Destroy Mesh Message
Instructs the client it may safely release a previously created mesh resource as identified by its unique mesh resource ID. This message may be sent with an invalid resource ID.
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Resource ID       | 4         | The resource ID of the mesh to destroy.



@subsection secmeshmsgcreate Create Mesh Message
Instructs the client to create a new mesh resource with the specified ID. The resource is not valid for use until a finalise message is sent.
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Resource ID       | 4         | The resource ID of the mesh to create. Zero is a valid ID.
Vertex Count      | 4         | Specifies the number of vertices in the mesh.
Index Count       | 4         | Specifies the number of indices in the mesh.
Topology          | 1         | Identifies the mesh topology. See TODO
Tint              | 4         | Mesh tint colour. See @ref secencodingcolour .
Translation       | 12        | 3 32-bit floats identifying the translation component of the overall mesh tranformation.
Rotation          | 16        | 4 32-bit floats identifying the quaternion rotation of the overall mesh tranformation.
Scale             | 12        | 3 32-bit floats identifying the scale component of the overall mesh transformation.



@subsection secmeshtopology Mesh Topology
The mesh topology details how the vertices and indices are interpreted. Valid values are:
Name              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
Points            | 0         | Vertices represent individual points. Indices are optional and
                  |           | each index referencing a single point.
Lines             | 1         | Vertices represent line end points. Indices must come in pairs.
Triangles         | 2         | Vertices represent triangles. Indices must come in triples.
Quads             | 3         | Vertices represent quadrilaterals. Indices must come in sets of four. Not supported yet.



@subsection secmeshdatamsg Mesh Element Messages
This section details the message structure of transmitting mesh elements; vertices, indices, vertex colour, normals and UVs. All messages have the same structure:
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Resource ID       | 4         | The resource ID of the mesh to which the data belong.
Index Offset      | 4         | Index offset for the the first element. See below.
Reserved          | 2         | Reserved. Must be zero.
Count             | 2         | Number of elements.
Elements          | N * Count | Count data elements.

The element byte size depends on what is being sent as follows:
Message           | Element Byte Size | Description
----------------- | ----------------: | -----------------------------------------------------------
Vertex            | 12                | Triples of 32-bit floats identifying 3D coordinates.
Vertex Colour     | 4                 | 32-bit integer colour. See @ref secencodingcolour .
Index             | 4                 | 32-bit unsigned integer indices.
Normal            | 12                | Triples of 32-bit floats identifying a vertex normal.
UV                | 8                 | Doubles of 32-bit floats identifying 2D UV coordinates.

From this we see that each message identifies the target mesh resource, an index offset to start writing the new data, an element count for this message followed by the elements themselves. The message only supports a 2-byte count, which is likely exceeded by any complex mesh, so the offset identifies where to start writing. Consider a mesh with 80000 vertices. Each message can only transmit at most 65535 vertices because of the 2-byte count limit. The limit is actually much lower than this because of the overall packet payload byte limit of 65535. For the sake of this example, let us say that each packet can contain 30000 vertices. This will be split across three vertex messages. Each message identifies the vertex @c Count and the <tt>Index Offset</tt> which is the index of the first vertex in the packet. In this case, we have the following three messages:
- offset 0, count 30000
- offset 30000, count 30000
- offset 60000, count 20000



@section seccameramsg Camera Messages
Camera routing supports a single message (ID zero) which specifies settings for a camera view. This is an optional camera and the client can elect to ignore or override any camera message, or parts of a camera. However, this message can be used to set an initial focus, or to show what the server is viewing. Multiple cameras are supported and the client can elect to lock their view to any valid camera.

Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Camera ID         | 1         | Identifies the camera. ID 255 is reserved for serialising details
                  |           | of the client's own camera when recording.
Reserved          | 3         | Three bytes of padding. Must be zero.
Position          | 12        | XYZ coordinate for the camera position. Each element is a 32-bit float.
Facing            | 12        | XYZ forward vector (normalised) identifying the camera facing relative to Position.
Up                | 12        | XYZ up vector (normalised) for the camera.
Near              | 4         | Near clip plane (32-bit float). Zero implies no change.
Far               | 4         | Far clip plane (32-bit float). Zero implies no change.
FOV               | 4         | Horizontal field of view in degrees. Zero implies no change.



@section seccategorymsg Category Messages
Category messages are used to identify and control categories associated with @ref secshapemsg "shapes". These messages can be used to identyfing valid categories, give a category a name, build a category hierarchy or set the default state for a category.

There is currently one category message with ID of zero.
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Category ID       | 2         | Specifies the category ID.
Parent ID         | 2         | Sets the parent of this category, building category hierarchy. Zero always implies no parent.
Default Active    | 2         | Default the category to the active state? Must be either 1 (active) or 0 (inactive).
Name Length       | 2         | Number of bytes in the name string, excluding the null terminator.
Name              | Varies    | The name string in UTF-8 encoding. Contains <tt>Name Length</tt> bytes.



@section secmaterialmsg Material Messages
Material messages are not yet supported. They are intended for future expansion to detail material resources which may be used with mesh resources.



@section secshapemsg Shape Messages
Shapes for the core of the 3<sup>rd</sup> Eye Scene message protocol. Shapes are used to visualise 3D geometry on the client. Shapes cover a range of routing IDs all with a common message structure. Shapes may also be extended to user defined routing IDs supporting the same message protocol. Shapes may be simple or complex. A simple shape is fully defined by its create message, while a complex shape requires additional data messages to be completed.

Shapes support four primary messages:
- Create to instantiate a shape.
- Data to transmit additional data required to create a shape (complex shapes only).
- Update updates the shape transformation or colour.
- Destroy destroys a previously created shape.

In all cases a shape may append additional data after the main message (essential for @c secshapemsgdata "data messages"). Details of how individual shapes vary are detailed in later sections.

Shapes may be persistent or transient. Persistent shapes require unique IDs - unique within the context of the shape's routing ID, not across all existing shapes - and persist until explicitly destroy. Transient shapes have no ID and are generally destroyed on the next frame (unless the end frame message includes the persist flag).



@subsection secshapemsgcreate Create Shape Message
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Object ID         | 4         | Unique identifies the shape within the context of the current shape routing ID. Zero is reserved for transient shapes.
Category ID       | 2         | Identifies the category to which the shape belongs. Zero is the catch-all default.
Flags             | 2         | Shape creation flags. See @ref secshapemsgflags "below".
Reserved          | 2         | Reserved. Must be zero.
Attributes        | *         | See @ref secshapemsgattributes



@subsection secshapemsgflags Shape Flags 
Shapes may support the following set of flags, though not all shapes will support all flags.
Flag              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
Wireframe         | 1         | The shape should be visualised using wirefame rendering if supported.
Transparent       | 2         | The shape should be visualised as transparent. The colour alpha channel specifies the opacity.
User              | 256       | Shape specific flags start here.



@subsection secshapemsgattributes Shape Attributes
Shape attributes are used in both shape create and update messages and contain the following data:
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Colour            | 4         | A four byte encoded colour for the same. See @ref secencodingcolour .
Translation       | 12        | 3 32-bit floats identifying the translation component of the overall shape transformation.
Rotation          | 16        | Usage varies, but generally defines 4 32-bit floats identifying a rotation for the shape.
Scale             | 12        | Usage varies, but generally defines 3 32-bit floats identifying the scale of the shape.

Note: Individual shapes may treat rotation and scale differently. Spheres, for example, ignore rotation, while planes use rotation to store the plane normal. Spheres may only consider the first scale component, while cylinders interpret the first components as radius, radius (ignored), length.



@subsection secencodingcolour Colour Encoding
@ref secshapemsgattributes "Shape attributes" include a @c Colour value to apply to the shape. Meshes also support a tint colour. All 3<sup>rd</sup> Eye Scene colour values are 32-bit, RGBA colour values with 8-bits per colour channel. The channels are ordered in low to high byte as red, green, blue, alpha.



@subsection secshapemsgspecificcreate Shape Specific Create Messages
This section lists any how specific shapes interpret the translation, rotation and scale components of their @ref secshapemsgattributes "attributes" and also any additional data appended to a create message. Any unlisted shapes interpret the translation, rotation and scale simply as is (rotation as a quaternion rotation) and append no data. Unless otherwise specifies, each shape's local pivot or origin is at the centre of the shape.

Shape             | Complex?  | Variation
----------------  | --------- | ----------------------------
Sphere            | No        | Pivot is at the centre, rotation is ignored, scale[0] specifies the radius.
Box               | No        | Scale sets the box edge length. E.g., (1, 1, 1) makes a unit cube.
Cone              | No        | Pivot is at the apex. Default direction is (0, 0, 1) away from the apex. Scale 0 is the radius at the base (also in element 1). Scale 2 is the length away from the apex.
Cylinder          | No        | Default direction is (0, 0, 1). Scale 0 is the radius (also in element 1). Scale 2 is the length.
Capsule           | No        | Default direction is (0, 0, 1). Scale 0 is the cylinder and hemisphere radius (also in element 1). Scale 2 is the cylinder length (total length is length + 2 * radius).
Plane             | No        | Position defines a local pivot point to centre the quad on. Rotation defines the quaternion rotation away from the default 'normal' (0, 0, 1) Scale components 0, 2, define the quad size (both are equal) Scale component 1 specifies the length to render the normal with.
Star              | No        | Pivot is at the centre, rotation is ignored, scale[0] specifies the radius.
Arrow             | No        | Pivot is at the arrow base. Default direction is (0, 0, 1) away from the base. Scale 0 is the radius (of the arrow wall) (also in element 1). Scale 2 is the length.
Mesh Shape        | Yes       | Writes additional data.  
Mesh Set          | No        | Writes additional data. 
Point Cloud       | Yes       | Writes additional data.
Text 3D           | No        | Supports user flags. Writes additional data. Position may defaults to screen space with (0, 0) the upper left corer and (1, 1) the lower right (Z ignored).
Text 2D           | No        | Supports user flags. Writes additional data.

The directional shapes (arrow, capsule, cone, cylinder, plane, text 3d) all use rotation as a quaternion rotation away from the default direction (0, 0, 1). It could have been overridden to specify a true direction vector. However, this was not done to better support a common set of get/set rotation methods with consistent semantics. A rotation is either ignored, or it is a quaternion rotation.

@subsection secshapemsgmeshcreate Mesh Shape Create Addendum
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Vertex Count      | 4         | Number of vertices in the mesh. Must match topology if there are not indices.
Index Count       | 4         | Number of indices. Must match topology, or zero. Zero implies sequential vertex indexing.
Topology          | 1         | See @ref secmeshtopology "topology"

@subsection secshapemsgmeshsetcreate Mesh Set Create Addendum
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Number of Parts   | 2         | Number of mesh resource parts to the mesh set.
Mesh Resource ID  | 4 *       | Mesh Resource ID for the next part.
Attributes        | N *       | @ref secshapemsgattributes for the part.
* A <tt>Part ID</tt> and <tt>Attributes</tt> Are written for each part and appear a number of times equal to the <tt>Number of Parts</tt>.


@subsection secshapemsgpointcloudcreate Point Cloud Create Addendum
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Mesh Resource ID  | 4 *       | Mesh Resource ID for this point cloud. Shared with mesh resources (mind the topology).
Index Count       | 4         | Optional index count used to index a subset of the vertices in the mesh resource.
Point Size        | 1         | Optional override for the point rendering size. Zero to use the default.


@subsection secshapemsgtextcreate Text 2D/3D Create Addendum
Text 2D/3D additional data:
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Text Length       | 2         | Number of bytes in the following text (excludes null terminator).
Text              | N         | UTF-8 encoded string with <tt>Text Length</tt> bytes. No null terminator.

Text 2D shapes also support the following flags:
Flag              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
World Space       | 256       | Position is in world space to be projected onto the screen.

Text 3D Flags:
Flag              | Value     | Description
----------------- | --------: | -------------------------------------------------------------------
Screen Facing     | 256       | Text should always face the screen (billboarding).



@subsection secshapemsgdata Shape Data Message
Shape data messages are only sent for complex shapes. These are shapes which cannot be succinctly defined by their create message, even with additional data appended to the create message. All data messages share the following header, after which the data layout is entirely dependent on the shape type.
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Object ID         | 4         | ID of the target shape. Transient shapes are ID zero.

Note: Transient shapes may be complex and require additional data messages. However, the message must have an object ID of zero, which cannot uniquely identify a shape. Clients can only assume that a data message targeting a transient shape pertains to the last transient shape created for that shape routing ID.

@subsection secshapemsgmeshdata Mesh Shape Data Payload
Mesh shapes write the following data messages:
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Data Type ID      | 2         | Identifies the data payload: vertices or indices.
Index Offset      | 4         | Offset into the overall data array to start indexing at. See TODO
Element Count     | 4         | Number of elements.
Elements          | N * Count | Data elements follow.

The index offset/count values behave in the same was as for @ref secmeshdatamsg "meshes".

Vertex elements are 12 byte, single precision XYZ coordinates. Index elements are 4 byte unsigned integers.

Note: packet payload size restrictions may be ignored when serialising to disk.

@subsection secshapemsgpointclouddata Point Cloud Data Payload
Point cloud shapes can optionally write a set of indices which restrict what is viewed in the referenced point cloud mesh resource.
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Index Offset      | 4         | Offset into the overall data array to start indexing at. See TODO
Element Count     | 4         | Number of elements.
Indices           | 4 * Count | Index array.

The index offset/count values behave in the same was as for @ref secmeshdatamsg "meshes".

Note: packet payload size restrictions may be ignored when serialising to disk.



@subsection secshapemsgupdate Update Shape Message
Shape udpate messages are used to adjust the core attributes of a shape. Most notably, update messages are used to reposition shapes in 3D space. Updates messages are only for persistent shapes. Update messages are formatted as follows: 
Datum             | Byte Size | Description
----------------- | --------: | -------------------------------------------------------------------
Object ID         | 4         | ID of the target shape. Transient (ID zero) do not support update messages.
Reserved          | 2         | Was intended for object flags, but has been removed. Must be zero.
Attributes        | *         | See @ref secshapemsgattributes

Update messages generally do not support additional data, shape specific data, though this is not a hard restriction.




*/
