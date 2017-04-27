//
// author Kazys Stepanas
//
// Copyright (c) Kazys Stepanas 2014
//

#ifdef TES_ENABLE

#include "3es-core.h"

#include "3esconnectionmonitor.h"
#include "3esserver.h"
#include "3esserverutil.h"

#include "3escolour.h"
#include "3escoordinateframe.h"
#include "3esfeature.h"
#include "3esmessages.h"
#include "3esmeshmessages.h"
#include "shapes/3esshapes.h"

//-----------------------------------------------------------------------------
// General macros.
//-----------------------------------------------------------------------------

/// Enable @p statement if TES is enabled.
///
/// The statement is completely removed when TES is not enabled.
/// @param statement The code statement to execute.
#define TES_STMT(statement) statement

/// Begins an if statement with condition, but only if TES is enabled. Otherwise the macro is
/// <tt>if (false)</tt>
/// @param condition The if statement condition.
#define TES_IF(condition) if (condition)

/// A helper macro to convert a pointer, such as @c this, into a 32-bit ID value.
/// This can be used as a rudimentary object ID assignment system.
/// @param ptr A pointer value.
#define TES_PTR_ID(ptr) static_cast<uint32_t>(reinterpret_cast<uint64_t>(ptr))

/// Colour from RGB.
/// @param r Red channel value [0, 255].
/// @param g Green channel value [0, 255].
/// @param b Blue channel value [0, 255].
#define TES_RGB(r, g, b) tes::Colour(r, g, b)
/// Colour from RGBA.
/// @param r Red channel value [0, 255].
/// @param g Green channel value [0, 255].
/// @param b Blue channel value [0, 255].
/// @param a Alpha channel value [0, 255].
#define TES_RGBA(r, g, b, a) tes::Colour(r, g, b, a)

/// Colour by name.
/// @param name a member of @p tes::Colour::Predefined.
#define TES_COLOUR(name) tes::Colour::Colours[tes::Colour::name]

/// Colour by predefined index.
/// @param index A valid value within @p tes::Colour::Predefined.
#define TES_COLOUR_I(index) tes::Colour::Colours[index]

/// Colour by name with alpha.
/// @param name a member of @p tes::Colour::Predefined.
/// @param a Alpha channel value [0, 255].
#define TES_COLOUR_A(name, a) tes::Colour(tes::Colour::Colours[tes::Colour::name], a)

//-----------------------------------------------------------------------------
// Server setup macros
//-----------------------------------------------------------------------------

/// Exposes details of a category to connected clients.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param _name A null terminated, UTF-8 string name for the category.
/// @param _categoryId ID of the category being named [0, 65535].
/// @param _parentId ID of the parent category, to support category trees. Zero for none. [0, 65535]
/// @param _active Default the category to the active state (true/false)?
#define TES_CATEGORY(server, _name, _categoryId, _parentId, _active) \
  { \
    tes::CategoryNameMessage msg; \
    msg.categoryId = _categoryId; \
    msg.parentId = _parentId; \
    msg.defaultActive = (_active) ? 1 : 0; \
    const size_t nameLen = (_name) ? strlen(_name) : 0u; \
    msg.nameLength = (uint16_t)((nameLen <= 0xffffu) ? nameLen : 0xffffu); \
    msg.name = _name; \
    tes::sendMessage((server), tes::MtCategory, tes::CategoryNameMessage::MessageId, msg); \
  }

/// A helper macro used to declare a @p Server pointer and compile out when TES is not enabled.
/// Initialises @p server as a @p Server variable with a null value.
/// @param server The variable name for the @c Server object.
#define TES_SERVER_DECL(server) tes::Server *server = nullptr;

/// A helper macro used to declare and initialise @p ServerSettings and compile out when TES is
/// not enabled.
/// @param settings The variable name for the @p ServerSettings.
/// @param ... Additional arguments passed to the @p ServerSettings constructor.
#define TES_SETTINGS(settings, ...) tes::ServerSettings settings = tes::ServerSettings(__VA_ARGS__);
/// Initialise a default @p ServerInfoMessage and assign the specified @p CoordinateFrame.
///
/// The time unit details for @p info can be initialise using @c TES_SERVER_INFO_TIME()
/// @see @c initDefaultServerInfo()
/// @param info Variable name for the @c ServerInfoMessage structure.
/// @param infoCoordinateFrame The server's @c CoordinateFrame value.
#define TES_SERVER_INFO(info, infoCoordinateFrame) \
  tes::ServerInfoMessage info; \
  tes::initDefaultServerInfo(&info); \
  info.coordinateFrame = infoCoordinateFrame;

/// Initialise the time unit details of a @c ServerInfoMessage.
/// @param info the @c ServerInfoMessage structure variable.
/// @param timeUnit The @c ServerInfoMessage::timeUnit value to set.
/// @param defaultFrameTime The @c ServerInfoMessage::defaultFrameTime value to set.
#define TES_SERVER_INFO_TIME(info, timeUnit, defaultFrameTime) \
  info.timeUnit = timeUnit; \
  info.defaultFrameTime = defaultFrameTime;

/// Initialise @p server to a new @c Server object with the given @C ServerSettings and
/// @c ServerInfoMessage.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param settings The @c ServerSettings structure to initialise the server with.
/// @param info The @c ServerInfoMessage structure to initialise the server with.
#define TES_SERVER_CREATE(server, settings, info) server = tes::Server::create(settings, info);

/// Start the given @c Server in the given mode (synchronous or asynchronous).
///
/// After this call, the server can accept connections.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @mode The server mode: @c ConnectionMonitor::Synchronous or @c ConnectionMonitor::Asynchronous.
#define TES_SERVER_START(server, mode) (server).connectionMonitor()->start(mode);

/// Call to update the server flushing the frame and potentially monitoring new connections.
///
/// This update macro performs the following update commands:
/// - Call @c Server::updateFrame()
/// - Update connections, accepting new and expiring old.
/// - Updates any pending cache transfers.
///
/// Any additional macro arguments are passed to @c Server::updateFrame(). At the very least
/// a delta time value must be passed (floating point, in seconds). This should be zero when
/// using TES for algorithm debugging, or a valid time delta in real-time debugging.
///
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ... Arguments for @c Server::updateFrame()
#define TES_SERVER_UPDATE(server, ...) \
  { \
    (server).updateTransfers(0); \
    (server).updateFrame(__VA_ARGS__); \
    tes::ConnectionMonitor *_conMon = (server).connectionMonitor(); \
    if (_conMon->mode() == tes::ConnectionMonitor::Synchronous) \
    { \
      _conMon->monitorConnections(); \
    } \
    _conMon->commitConnections(); \
  }

/// Wait for the server to be ready to accept incoming connections.
/// This blocks until at least one connection is established up to @p timems milliseconds.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param timems The wait time out to wait for (milliseconds).
#define TES_SERVER_START_WAIT(server, timems) \
  if ((server).connectionMonitor()->waitForConnection(timems) > 0) \
  { \
    (server).connectionMonitor()->commitConnections(); \
  }

/// Set the connection callback via @c ConnectionMonitor::setConnectionCallback().
#define TES_SET_CONNECTION_CALLBACK(server, ...) \
  (server).connectionMonitor()->setConnectionCallback(__VA_ARGS__);

/// Stop the server. The server is closed and disposed and is no longer valid for use after
/// this call.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
#define TES_SERVER_STOP(server) \
  (server).close(); \
  (server).dispose();

/// Check if @p server is enabled.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
#define TES_ACTIVE(server) (server).active()
/// Enable/disable @p server.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
#define TES_SET_ACTIVE(server, _active) (server).setActive(_active)

/// Check if a feature is enabled using @c checkFeature().
/// @param feature The feature to check for.
#define TES_FEATURE(feature) tes::checkFeature(feature)
/// Get the flag for a feature.
/// @param feature The feature identifier.
#define TES_FEATURE_FLAG(feature) tes::featureFlag(feature)
/// Check if the given set of features are enabled using @c checkFeatures().
/// @param featureFlags The flags to check for.
#define TES_FEATURES(featureFlags) tes::checkFeatures(featureFlags)

/// Execute @c expression if @p featureFlags are all present using @c checkFeatures().
/// @param featureFlags The flags to require before executing @p expression.
/// @param expression The code statement or expression to execute if @c checkFeatures() passes.
#define TES_IF_FEATURES(featureFlags, expression) \
  if (tes::checkFeatures(featureFlags)) \
  { \
    expression; \
  }

//-----------------------------------------------------------------------------
// Shape macros
//-----------------------------------------------------------------------------

/// Adds a reference to the given @c resource. See @c tes::Connection::referenceResource().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param resource A pointer to the resource.
#define TES_REFERENCE_RESOURCE(server, resource) { (server).referenceResource(resource); }

/// Releases a reference to the given @c resource. See @c tes::Connection::referenceResource().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param resource A pointer to the resource.
#define TES_RELEASE_RESOURCE(server, resource)  { (server).releaseResource(resource); }

/// Makes a stack declaration of a placeholder mesh resource.
/// Primarily for use with @c TES_REFERENCE_RESOURCE(), @c TES_RELEASE_RESOURCE and @c TES_MESHSET_END().
/// @param id The mesh resource ID to proxy.
#define TES_MESH_PLACEHOLDER(id) tes::MeshPlaceholder(id)

/// Solid arrow.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Arrow() constructor.
#define TES_ARROW(server, colour, ...) { (server).create(tes::Arrow(__VA_ARGS__).setColour(colour)); }
/// Transparent arrow.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Arrow() constructor.
#define TES_ARROW_T(server, colour, ...) { (server).create(tes::Arrow(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe arrow.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Arrow() constructor.
#define TES_ARROW_W(server, colour, ...) { (server).create(tes::Arrow(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Solid box.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Box() constructor.
#define TES_BOX(server, colour, ...) { (server).create(tes::Box(__VA_ARGS__).setColour(colour)); }
/// Transparent box.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Box() constructor.
#define TES_BOX_T(server, colour, ...) { (server).create(tes::Box(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe box.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Box() constructor.
#define TES_BOX_W(server, colour, ...) { (server).create(tes::Box(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Solid capsule.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Capsule() constructor.
#define TES_CAPSULE(server, colour, ...) { (server).create(tes::Capsule(__VA_ARGS__).setColour(colour)); }
/// Transparent capsule.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Capsule() constructor.
#define TES_CAPSULE_T(server, colour, ...) { (server).create(tes::Capsule(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe capsule.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Capsule() constructor.
#define TES_CAPSULE_W(server, colour, ...) { (server).create(tes::Capsule(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Solid cone.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Cone() constructor.
#define TES_CONE(server, colour, ...) { (server).create(tes::Cone(__VA_ARGS__).setColour(colour)); }
/// Transparent cone.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Cone() constructor.
#define TES_CONE_T(server, colour, ...) { (server).create(tes::Cone(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe cone.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Cone() constructor.
#define TES_CONE_W(server, colour, ...) { (server).create(tes::Cone(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Solid cylinder.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Cylinder() constructor.
#define TES_CYLINDER(server, colour, ...) { (server).create(tes::Cylinder(__VA_ARGS__).setColour(colour)); }
/// Transparent cylinder.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Cylinder() constructor.
#define TES_CYLINDER_T(server, colour, ...) { (server).create(tes::Cylinder(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe cylinder.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Cylinder() constructor.
#define TES_CYLINDER_W(server, colour, ...) { (server).create(tes::Cylinder(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Render a set of lines.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_LINES(server, colour, ...) { (server).create(tes::MeshShape(tes::DtLines, ##__VA_ARGS__).setColour(colour)); }

/// Render a set of lines, calling @c MeshShape::expandVertices().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_LINES_E(server, colour, ...) { (server).create(tes::MeshShape(tes::DtLines, ##__VA_ARGS__).expandVertices().setColour(colour)); }

/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_LINE(server, colour, v0, v1, ...) \
  { \
    const tes::Vector3f _line[2] = { tes::Vector3f(v0), tes::Vector3f(v1) }; \
    tes::MeshShape shape(tes::DtLines, _line[0].v, 2, sizeof(_line[0]), ##__VA_ARGS__); shape.setColour(colour); \
    (server).create(shape); \
  }

/// Render a complex mesh.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ... Additional arguments follow, passed to @p MeshSet() constructor.
#define TES_MESHSET(server, ...) { (server).create(tes::MeshSet(__VA_ARGS__)); }

/// Solid plane.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Plane() constructor.
#define TES_PLANE(server, colour, ...) { (server).create(tes::Plane(__VA_ARGS__).setColour(colour)); }
/// Transparent plane.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Plane() constructor.
#define TES_PLANE_T(server, colour, ...) { (server).create(tes::Plane(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe plane.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Plane() constructor.
#define TES_PLANE_W(server, colour, ...) { (server).create(tes::Plane(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Render a point cloud.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p PointCloudShape() constructor.
#define TES_POINTCLOUDSHAPE(server, colour, ...) { (server).create(tes::PointCloudShape(__VA_ARGS__).setColour(colour)); }

/// Render a small set of points.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_POINTS(server, colour, ...) { (server).create(tes::MeshShape(tes::DtPoints, ##__VA_ARGS__).setColour(colour)); }

/// Render a small set of points, calling @c MeshShape::expandVertices().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_POINTS_E(server, colour, ...) { (server).create(tes::MeshShape(tes::DtPoints, ##__VA_ARGS__).expandVertices().setColour(colour)); }

/// Render a set of voxels. Vertices represent voxel centres, normals are extents.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param resolution The length of the voxel edge. Only supports cubic voxels.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor. Vertices and normals required.
#define TES_VOXELS(server, colour, resolution, ...) { (server).create(tes::MeshShape(tes::DtVoxels, ##__VA_ARGS__).setUniformNormal(tes::Vector3f(0.5f * resolution)).setColour(colour)); }

/// Solid sphere.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Sphere() constructor.
#define TES_SPHERE(server, colour, ...) { (server).create(tes::Sphere(__VA_ARGS__).setColour(colour)); }
/// Transparent sphere.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Sphere() constructor.
#define TES_SPHERE_T(server, colour, ...) { (server).create(tes::Sphere(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe sphere.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Sphere() constructor.
#define TES_SPHERE_W(server, colour, ...) { (server).create(tes::Sphere(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Solid star.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Star() constructor.
#define TES_STAR(server, colour, ...) { (server).create(tes::Star(__VA_ARGS__).setColour(colour)); }
/// Transparent star.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Star() constructor.
#define TES_STAR_T(server, colour, ...) { (server).create(tes::Star(__VA_ARGS__).setColour(colour).setTransparent(true)); }
/// Wireframe star.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Star() constructor.
#define TES_STAR_W(server, colour, ...) { (server).create(tes::Star(__VA_ARGS__).setColour(colour).setWireframe(true)); }

/// Render 2D text in screen space. Range is from (0, 0) top left to (1, 1) bottom right. Z ignored.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Text2D() constructor.
#define TES_TEXT2D_SCREEN(server, colour, ...) { (server).create(tes::Text2D(__VA_ARGS__).setColour(colour)); }
/// Render 2D text with a 3D world location.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Text2D() constructor.
#define TES_TEXT2D_WORLD(server, colour, ...) { (server).create(tes::Text2D(__VA_ARGS__).setInWorldSpace(true).setColour(colour)); }

/// Render 3D text.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Text3D() constructor.
#define TES_TEXT3D(server, colour, ...) { (server).create(tes::Text3D(__VA_ARGS__).setColour(colour)); }

/// Render 3D text, always facing the screen.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p Text3D() constructor.
#define TES_TEXT3D_FACING(server, colour, ...) { (server).create(tes::Text3D(__VA_ARGS__).setScreenFacing(true).setColour(colour); }

/// Triangles shape.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.setColour(colour); (server).create(shape); }

/// Triangles shape, calling @c MeshShape::expandVertices().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_E(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.expandVertices().setColour(colour); (server).create(shape); }

/// Triangles shape with lighting (_N to calculate normals).
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_N(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.setCalculateNormals(true).setColour(colour); (server).create(shape); }

/// Triangles shape with lighting (_N to calculate normals), calling @c MeshShape::expandVertices().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_NE(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.expandVertices().setCalculateNormals(true).setColour(colour); (server).create(shape); }

/// Triangles wireframe shape.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_W(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.setWireframe(true); shape.setColour(colour); (server).create(shape); }

/// Triangles wireframe shape, calling @c MeshShape::expandVertices().
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_WE(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.expandVertices().setWireframe(true); shape.setColour(colour); (server).create(shape); }

/// Triangles transparent shape.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_T(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.setTransparent(true); shape.setColour(colour); (server).create(shape); }

/// Triangles transparent shape, calling @c MeshShape::expandVertices()
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLES_TE(server, colour, ...) { tes::MeshShape shape(tes::DtTriangles, ##__VA_ARGS__); shape.expandVertices().setTransparent(true); shape.setColour(colour); (server).create(shape); }

/// Single triangle.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLE(server, colour, v0, v1, v2, ...) \
{ \
  const tes::Vector3f _tri[3] = { tes::Vector3f(v0), tes::Vector3f(v1), tes::Vector3f(v2) }; \
  tes::MeshShape shape(tes::DtTriangles, _tri[0].v, 3, sizeof(tes::Vector3f), ##__VA_ARGS__); shape.setColour(colour).setTwoSided(true); (server).create(shape); \
}
/// Single wireframe triangle.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param v0 A triangle vertex.
/// @param v1 A triangle vertex.
/// @param v2 A triangle vertex.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLE_W(server, colour, v0, v1, v2, ...) \
{ \
  const tes::Vector3f _tri[3] = { tes::Vector3f(v0), tes::Vector3f(v1), tes::Vector3f(v2) }; \
  tes::MeshShape shape(tes::DtTriangles, _tri[0].v, 3, sizeof(_tri[0]), ##__VA_ARGS__); shape.setColour(colour); \
  shape.setWireframe(true); \
  (server).create(shape); \
}
/// Single transparent triangle.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param v0 A triangle vertex.
/// @param v1 A triangle vertex.
/// @param v2 A triangle vertex.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLE_T(server, colour, v0, v1, v2, ...) \
{ \
  const tes::Vector3f _tri[3] = { tes::Vector3f(v0), tes::Vector3f(v1), tes::Vector3f(v2) }; \
  tes::MeshShape shape(tes::DtTriangles, _tri[0].v, 3, sizeof(_tri[0]), ##__VA_ARGS__); shape.setColour(colour); \
  shape.setTransparent(true).setTwoSided(true); \
  (server).create(shape); \
}
/// Single triangle extracted by indexing @p verts using @p i0, @p i1, @p i2.
/// @p verts is expected as a float array with 3 elements per vertex.
///
/// Note: Only the indexed vertices are extracted and serialised.
///
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param verts Vertices to index the triangle into. Must be a float array with 3 elements per
///   vertex.
/// @param i0 Index to a triangle vertex.
/// @param i1 Index to a triangle vertex.
/// @param i2 Index to a triangle vertex.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLE_I(server, colour, verts, i0, i1, i2, ...) \
{ \
  const tes::Vector3f _tri[3] = { tes::Vector3f((verts) + (i0 * 3)), tes::Vector3f((verts) + (i1 * 3)), tes::Vector3f((verts) + (i2 * 3)) }; \
  tes::MeshShape shape(tes::DtTriangles, _tri[0].v, 3, sizeof(_tri[0]), ##__VA_ARGS__); shape.setColour(colour); (server).create(shape); \
}
/// Single wireframe triangle extracted by indexing @p verts using @p i0, @p i1, @p i2.
/// @p verts is expected as a float array with 3 elements per vertex.
///
/// Note: Only the indexed vertices are extracted and serialised.
///
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param verts Vertices to index the triangle into. Must be a float array with 3 elements per
///   vertex.
/// @param i0 Index to a triangle vertex.
/// @param i1 Index to a triangle vertex.
/// @param i2 Index to a triangle vertex.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLE_IW(server, colour, verts, i0, i1, i2, ...) \
{ \
  const tes::Vector3f _tri[3] = { tes::Vector3f((verts) + (i0 * 3)), tes::Vector3f((verts) + (i1 * 3)), tes::Vector3f((verts) + (i2 * 3)) }; \
  tes::MeshShape shape(tes::DtTriangles, _tri[0].v, 3, sizeof(_tri[0]), ##__VA_ARGS__); shape.setColour(colour); \
  shape.setWireframe(true); \
  (server).create(shape); \
}
/// Single transparent triangle extracted by indexing @p verts using @p i0, @p i1, @p i2.
/// @p verts is expected as a float array with 3 elements per vertex.
///
/// Note: Only the indexed vertices are extracted and serialised.
///
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param colour The colour to apply to the shape.
/// @param verts Vertices to index the triangle into. Must be a float array with 3 elements per
///   vertex.
/// @param i0 Index to a triangle vertex.
/// @param i1 Index to a triangle vertex.
/// @param i2 Index to a triangle vertex.
/// @param ... Additional arguments follow, passed to @p MeshShape() constructor.
#define TES_TRIANGLE_IT(server, colour, verts, i0, i1, i2, ...) \
{ \
  const tes::Vector3f _tri[3] = { tes::Vector3f((verts) + (i0 * 3)), tes::Vector3f((verts) + (i1 * 3)), tes::Vector3f((verts) + (i2 * 3)) }; \
  tes::MeshShape shape(tes::DtTriangles, _tri[0].v, 3, sizeof(_tri[0]), ##__VA_ARGS__); shape.setColour(colour); \
  shape.setTransparent(true).setTwoSided(true); \
  (server).create(shape); \
}

/// Destroy arrow with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_ARROW_END(server, id) { (server).destroy(tes::Arrow(static_cast<uint32_t>(id))); }
/// Destroy box with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_BOX_END(server, id) { (server).destroy(tes::Box(static_cast<uint32_t>(id))); }
/// Destroy capsule with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_CAPSULE_END(server, id) { (server).destroy(tes::Capsule(static_cast<uint32_t>(id))); }
/// Destroy cone with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_CONE_END(server, id) { (server).destroy(tes::Cone(static_cast<uint32_t>(id))); }
/// Destroy cylinder with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_CYLINDER_END(server, id) { (server).destroy(tes::Cylinder(static_cast<uint32_t>(id))); }
/// Destroy lines with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_LINES_END(server, id) { (server).destroy(tes::MeshShape(tes::DtLines, nullptr, 0, 0, static_cast<uint32_t>(id))); }
/// Destroy mesh with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
/// @param resource The mesh resource associated with the set. Only supports one mesh.
///       Must be a pointer type : <tt>tes::MeshResource *</tt>
#define TES_MESHSET_END(server, id, resource) { (server).destroy(tes::MeshSet(static_cast<uint32_t>(resource, id))); }
/// Destroy plane with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_PLANE_END(server, id) { (server).destroy(tes::Plane(static_cast<uint32_t>(id))); }
/// Destroy point cloud with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_POINTCLOUDSHAPE_END(server, cloud, id) { (server).destroy(tes::PointCloudShape(cloud, static_cast<uint32_t>(id))); }
/// Destroy point set with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_POINTS_END(server, id) { (server).destroy(tes::MeshShape(tes::DtPoints, nullptr, 0, 0, static_cast<uint32_t>(id))); }
/// Destroy voxel set with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_VOXELS_END(server, id) { (server).destroy(tes::MeshShape(tes::DtVoxels, nullptr, 0, 0, static_cast<uint32_t>(id))); }
/// Destroy sphere with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_SPHERE_END(server, id) { (server).destroy(tes::Sphere(static_cast<uint32_t>(id))); }
/// Destroy star with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_STAR_END(server, id) { (server).destroy(tes::Star(static_cast<uint32_t>(id))); }
/// Destroy 2D text with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_TEXT2D_END(server, id) { (server).destroy(tes::Text2D("", static_cast<uint32_t>(id))); }
/// Destroy 3D text with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_TEXT3D_END(server, id) { (server).destroy(tes::Text3D("", static_cast<uint32_t>(id))); }
/// Destroy triangle or triangles with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_TRIANGLES_END(server, id) { (server).destroy(tes::MeshShape(tes::DtTriangles, nullptr, 0, 0, static_cast<uint32_t>(id))); }
/// Destroy arrow with @p id.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param id The ID of the shape to destroy.
#define TES_TRIANGLE_END(server, id) { (server).destroy(tes::MeshShape(tes::DtTriangles, nullptr, 0, 0, static_cast<uint32_t>(id))); }


//-----------------------------------------------------------------------------
// Shape update macros
//-----------------------------------------------------------------------------
/// Send a position update message for a shape.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
#define TES_POS_UPDATE(server, ShapeType, objectID, pos) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setFlags(tes::OFUpdateMode | tes::OFPosition));

/// Send an update message for a shape, updating object rotation.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
#define TES_ROT_UPDATE(server, ShapeType, objectID, quaternion) \
  (server).update(tes::ShapeType(objectID, 0).setRotation(quaternion).setFlags(tes::OFUpdateMode | tes::OFRotation));

/// Send an update message for a shape, updating scale.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param scale The new object scale. A @c V3Arg compatible argument.
#define TES_SCALE_UPDATE(server, ShapeType, objectID, scale) \
  (server).update(tes::ShapeType(objectID, 0).setScale(scale).setFlags(tes::OFUpdateMode | tes::OFScale));

/// Send an update message for a shape, updating colour.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param colour The new object @c Colour.
#define TES_COLOUR_UPDATE(server, ShapeType, objectID, colour) \
  (server).update(tes::ShapeType(objectID, 0).setColour(colour).setFlags(tes::OFUpdateMode | tes::OFColour));

/// Send an update message for a shape, updating colour.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param colour The new object @c Colour.
#define TES_COLOR_UPDATE(server, ShapeType, objectID, colour) \
  (server).update(tes::ShapeType(objectID, 0).setColour(colour).setFlags(tes::OFUpdateMode | tes::OFColour));

/// Send an update message for a shape, updating position and rotation.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
#define TES_POSROT_UPDATE(server, ShapeType, objectID, pos, quaternion) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setRotation(quaternion).setFlags(tes::OFUpdateMode | tes::OFPosition | tes::OFRotation));

/// Send an update message for a shape, updating position and scale.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
/// @param scale The new object scale. A @c V3Arg compatible argument.
#define TES_POSSCALE_UPDATE(server, ShapeType, objectID, pos, scale) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setScale(scale).setFlags(tes::OFUpdateMode | tes::OFPosition | tes::OFRotation));

/// Send an update message for a shape, updating rotation and scale.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
/// @param scale The new object scale. A @c V3Arg compatible argument.
#define TES_ROTSCALE_UPDATE(server, ShapeType, objectID, quaternion, scale) \
  (server).update(tes::ShapeType(objectID, 0).setRotation(quaternion).setScale(scale).setFlags(tes::OFUpdateMode | tes::OFRotation | tes::OFScale ));

/// Send an update message for a shape, updating position, rotation and scale.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
/// @param scale The new object scale. A @c V3Arg compatible argument.
#define TES_PRS_UPDATE(server, ShapeType, objectID, pos, quaternion, scale) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setRotation(quaternion).setScale(scale).setFlags(tes::OFUpdateMode | tes::OFPosition | tes::OFRotation | tes::OFScale ));

/// Send an update message for a shape, updating position, rotation and colour.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
/// @param colour The new object @c Colour.
#define TES_PRC_UPDATE(server, ShapeType, objectID, pos, quaternion, colour) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setRotation(quaternion).setColour(colour).setFlags(tes::OFUpdateMode | tes::OFPosition | tes::OFRotation | tes::OFColour ));

/// Send an update message for a shape, updating position, scale and colour.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
/// @param scale The new object scale. A @c V3Arg compatible argument.
/// @param colour The new object @c Colour.
#define TES_PSC_UPDATE(server, ShapeType, objectID, pos, scale, colour) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setScale(scale).setColour(colour).setFlags(tes::OFUpdateMode | tes::OFPosition | tes::OFScale | tes::OFColour ));

/// Send an update message for a shape, updating rotation, scale and colour.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
/// @param scale The new object scale. A @c V3Arg compatible argument.
/// @param colour The new object @c Colour.
#define TES_RSC_UPDATE(server, ShapeType, objectID, quaternion, scale, colour) \
  (server).update(tes::ShapeType(objectID, 0).setRotation(quaternion).setScale(scale).setColour(colour).setFlags(tes::OFUpdateMode | tes::OFRotation | tes::OFScale | tes::OFColour ));

/// Send an update message for a shape, updating all transform and colour attributes.
/// @param server The @c Server or @c Connection object. Must be a dereferenced pointer.
/// @param ShapeType The class of the shape to update. E.g., @c tes::Box
/// @param objectID The ID of the object to update.
/// @param pos The new position. A @c V3Arg compatible argument.
/// @param quaternion The updated quaternion rotation. A @c QuaternionArg compatible argument.
/// @param scale The new object scale. A @c V3Arg compatible argument.
/// @param colour The new object @c Colour.
#define TES_PRSC_UPDATE(server, ShapeType, objectID, pos, quaternion, scale, colour) \
  (server).update(tes::ShapeType(objectID, 0).setPosition(pos).setRotation(quaternion).setScale(scale).setColour(colour) ));

#else  // !TES_ENABLE

#define TES_STMT(...)
#define TES_IF(...) if (false)
#define TES_PTR_ID(...)
#define TES_RGB(...)
#define TES_RGBA(...)
#define TES_COLOUR(...)
#define TES_COLOUR_I(...)
#define TES_COLOUR_A(...)

#define TES_CATEGORY(...)
#define TES_SERVER_DECL(...)
#define TES_SETTINGS(...)
#define TES_SERVER_INFO(...)
#define TES_SERVER_INFO_TIME(...)
#define TES_SERVER_CREATE(...)
#define TES_SERVER_START(...)
#define TES_SERVER_START_WAIT(...)
#define TES_SET_CONNECTION_CALLBACK(...)
#define TES_SERVER_UPDATE(...)
#define TES_SERVER_STOP(...)
#define TES_ACTIVE(...) false
#define TES_SET_ACTIVE(...)

#define TES_FEATURE(...) false
#define TES_FEATURE_FLAG(...) 0
#define TES_FEATURES(...)
#define TES_IF_FEATURES(...)

#define TES_REFERENCE_RESOURCE(...)
#define TES_RELEASE_RESOURCE(...)
#define TES_MESH_PLACEHOLDER(...)

#define TES_ARROW(...)
#define TES_ARROW_T(...)
#define TES_ARROW_W(...)
#define TES_BOX(...)
#define TES_BOX_T(...)
#define TES_BOX_W(...)
#define TES_CAPSULE(...)
#define TES_CAPSULE_T(...)
#define TES_CAPSULE_W(...)
#define TES_CONE(...)
#define TES_CONE_T(...)
#define TES_CONE_W(...)
#define TES_CYLINDER(...)
#define TES_CYLINDER_T(...)
#define TES_CYLINDER_W(...)
#define TES_LINES(...)
#define TES_LINES_E(...)
#define TES_LINE(...)
#define TES_MESHSET(...)
#define TES_PLANE(...)
#define TES_PLANE_T(...)
#define TES_PLANE_W(...)
#define TES_POINTCLOUDSHAPE(...)
#define TES_POINTS(...)
#define TES_POINTS_E(...)
#define TES_VOXELS(...)
#define TES_SPHERE(...)
#define TES_SPHERE_T(...)
#define TES_SPHERE_W(...)
#define TES_STAR(...)
#define TES_STAR_T(...)
#define TES_STAR_W(...)
#define TES_TEXT2D_SCREEN(...)
#define TES_TEXT2D_WORLD(...)
#define TES_TEXT3D(...)
#define TES_TEXT3D_FACING(...)
#define TES_TRIANGLES(...)
#define TES_TRIANGLES_E(...)
#define TES_TRIANGLES_N(...)
#define TES_TRIANGLES_NE(...)
#define TES_TRIANGLES_W(...)
#define TES_TRIANGLES_WE(...)
#define TES_TRIANGLES_T(...)
#define TES_TRIANGLES_TE(...)
#define TES_TRIANGLE(...)
#define TES_TRIANGLE_W(...)
#define TES_TRIANGLE_I(...)
#define TES_TRIANGLE_T(...)
#define TES_TRIANGLE_IT(...)
#define TES_TRIANGLE_IW(...)

#define TES_ARROW_END(...)
#define TES_BOX_END(...)
#define TES_CAPSULE_END(...)
#define TES_CONE_END(...)
#define TES_CYLINDER_END(...)
#define TES_LINES_END(...)
#define TES_MESHSET_END(...)
#define TES_PLANE_END(...)
#define TES_POINTCLOUDSHAPE_END(...)
#define TES_POINTS_END(...)
#define TES_VOXELS_END(...)
#define TES_SPHERE_END(...)
#define TES_STAR_END(...)
#define TES_TEXT2D_END(...)
#define TES_TEXT3D_END(...)
#define TES_TRIANGLES_END(...)
#define TES_TRIANGLE_END(...)

#define TES_POS_UPDATE(...)
#define TES_ROT_UPDATE(...)
#define TES_SCALE_UPDATE(...)
#define TES_COLOUR_UPDATE(...)
#define TES_COLOR_UPDATE(...)
#define TES_POSROT_UPDATE(...)
#define TES_POSSCALE_UPDATE(...)
#define TES_ROTSCALE_UPDATE(...)
#define TES_PRS_UPDATE(...)
#define TES_PRC_UPDATE(...)
#define TES_PSC_UPDATE(...)
#define TES_RSC_UPDATE(...)
#define TES_PRSC_UPDATE(...)

#endif // TES_ENABLE
