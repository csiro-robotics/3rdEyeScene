//
// author: Kazys Stepanas
//

// This test program creates a 3es server and publishes various shapes. It outputs a JSON representation of each
// published item to standard output. The program should be paired with a similar client application which logs
// received data in JSON format. The JSON may be parsed and compared to validate equivalents of what is sent and what
// is received.

#include <3esconnection.h>
#include <3esconnectionmonitor.h>
#include <3escoordinateframe.h>
#include <3esfeature.h>
#include <3esserver.h>
#include <shapes/3esshapes.h>
#include <3esspheretessellator.h>
#include <3esmaths.h>

#define TES_ENABLE
#include <3esservermacros.h>

#include <3esvector3.h>
#include <3estimer.h>
#include <shapes/3essimplemesh.h>
#include <shapes/3espointcloud.h>

#include <cmath>
#include <csignal>
#include <chrono>
#include <functional>
#include <iostream>
#include <thread>
#include <vector>

using namespace tes;

namespace
{
  bool quit = false;

  void onSignal(int arg)
  {
    if (arg == SIGINT || arg == SIGTERM)
    {
      quit = true;
    }
  }
}



MeshShape *createPointsMesh(unsigned id, const std::vector<Vector3f> &vertices)
{
  MeshShape *shape = new MeshShape(DtPoints, vertices.data()->v, unsigned(vertices.size()),
                                   sizeof(Vector3f), id);
  return shape;
}


MeshShape *createLinesMesh(unsigned id, const std::vector<Vector3f> &vertices, const std::vector<unsigned> &indices)
{
  std::vector<unsigned> lineIndices;

  // Many duplicate lines generated, but we are validating transfer, not rendering.
  for (size_t i = 0; i < indices.size(); i += 3)
  {
    lineIndices.push_back(indices[i + 0]);
    lineIndices.push_back(indices[i + 1]);
    lineIndices.push_back(indices[i + 1]);
    lineIndices.push_back(indices[i + 2]);
    lineIndices.push_back(indices[i + 2]);
    lineIndices.push_back(indices[i + 0]);
  }

  MeshShape *shape = new MeshShape(DtLines, vertices.data()->v, unsigned(vertices.size()),
                                   sizeof(Vector3f),
                                   lineIndices.data(), unsigned(lineIndices.size()),
                                   id);
  return shape;
}


MeshShape *createTrianglesMesh(unsigned id, const std::vector<Vector3f> &vertices, const std::vector<unsigned> &indices)
{
  MeshShape *shape = new MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()),
                                   sizeof(Vector3f),
                                   indices.data(), unsigned(indices.size()),
                                   id);
  return shape;
}


MeshShape *createVoxelsMesh(unsigned id)
{
  const float voxelScale = 0.1f;
  std::vector<Vector3f> vertices;

  Vector3f v;
  for (int z = -8; z < 8; ++z)
  {
    v.z = z * voxelScale;
    for (int y = -8; y < 8; ++y)
    {
      v.y = y * voxelScale;
      for (int x = -8; x < 8; ++x)
      {
        v.x = x * voxelScale;
        vertices.push_back(v);
      }
    }
  }

  MeshShape *shape = new MeshShape(DtVoxels, vertices.data()->v, unsigned(vertices.size()),
                                   sizeof(Vector3f), id);

  shape->setUniformNormal(Vector3f(voxelScale));
  return shape;
}


PointCloudShape *createCloud(unsigned id, const std::vector<Vector3f> &vertices, std::vector<MeshResource *> &resources)
{
  PointCloud *mesh = new PointCloud(id * 100);
  resources.push_back(mesh);

  mesh->addPoints(vertices.data(), unsigned(vertices.size()));
  PointCloudShape *shape = new PointCloudShape(mesh, id);

  return shape;
}


MeshSet *createMeshSet(unsigned id, const std::vector<Vector3f> &vertices, const std::vector<unsigned> &indices, std::vector<MeshResource *> &resources)
{
  int partCount = 5;

  MeshSet *shape = new MeshSet(id, 0, partCount);

  for (int i = 0; i < partCount; ++i)
  {
    SimpleMesh *mesh = new SimpleMesh(id * 100 + i, unsigned(vertices.size()), unsigned(indices.size()));
    mesh->addComponents(SimpleMesh::Normal);

    mesh->setTransform(Matrix4f::translation(Vector3f(i * 2.0f, i * 2.0f, 0)));

    mesh->addVertices(vertices.data(), unsigned(vertices.size()));
    mesh->addIndices(indices.data(), unsigned(indices.size()));

    for (unsigned v = 0; v < unsigned(vertices.size()); ++v)
    {
      // Assume sphere around origin.
      mesh->setNormal(v, vertices[v].normalised());
    }

    resources.push_back(mesh);
    shape->setPart(i, mesh, Matrix4f::identity);
  }

  return shape;
}


const char *drawTypeString(DrawType type)
{
  switch (type)
  {
    case DtPoints: return "points";
    case DtLines: return "lines";
    case DtTriangles: return "triangles";
    case DtVoxels: return "voxels";
    default: break;
  }

  return "unknown";
}


bool haveOption(const char *opt, int argc, const char **argv)
{
  for (int i = 1; i < argc; ++i)
  {
    if (strcmp(opt, argv[i]) == 0)
    {
      return true;
    }
  }

  return false;
}


void defineCategory(Server *server, const char *name, unsigned id, unsigned parentId, bool active)
{
  CategoryNameMessage msg;
  msg.categoryId = id;
  msg.parentId = parentId;
  msg.defaultActive = (active) ? 1 : 0;
  const size_t nameLen = (name) ? strlen(name) : 0u;
  msg.nameLength = (uint16_t)((nameLen <= 0xffffu) ? nameLen : 0xffffu);
  msg.name = name;
  sendMessage(*(server), MtCategory, CategoryNameMessage::MessageId, msg);
  std::cout << "  \"category-" << name << "\" : {\n"
            << "    \"categoryId\" : " << id << ",\n"
            << "    \"parentId\" : " << parentId << ",\n"
            << "    \"defaultActive\" : " << (active ? "true" : "false") << ",\n"
            << "    \"nameLength\" : " << msg.nameLength << ",\n"
            << "    \"name\" : \"" << msg.name << "\"\n"
            << "  },\n";
}


/// Initialises a shape by setting a position and colour dependent on its @c id().
/// @param shape The shape to initialised.
/// @return @p shape
template <class T>
T *initShape(T *shape)
{
  shape->setPosition(Vector3f(1.0f * shape->id(), 0.1f * shape->id(), -0.75f * shape->id()));
  shape->setColour(Colour::cycle(shape->id()));
  return shape;
}

template <class T>
std::ostream &logShapeExtensions(std::ostream &o, const T &shape, const std::string &indent)
{
  return o;
}


std::ostream &logShapeExtensions(std::ostream &o, const Text2D &shape, const std::string &indent)
{
  o << ",\n";
  o << indent << "\"textLength\" : " << shape.textLength() << ",\n";
  std::string text(shape.text(), shape.textLength());
  o << indent << "\"text\" : \"" << text << "\"";
  return o;
}


std::ostream &logShapeExtensions(std::ostream &o, const Text3D &shape, const std::string &indent)
{
  o << ",\n";
  o << indent << "\"textLength\" : " << shape.textLength() << ",\n";
  std::string text(shape.text(), shape.textLength());
  o << indent << "\"text\" : \"" << text << "\"";
  return o;
}


std::ostream &logShapeExtensions(std::ostream &o, const MeshShape &shape, const std::string &indent)
{
  o << ",\n";
  o << indent << "\"drawType\" : \"" << drawTypeString(shape.drawType()) << "\",\n";

  bool dangling = false;
  auto closeDangling = [&o] (bool &dangling)
  {
    if (dangling)
    {
      o << ",\n";
      dangling = false;
    }
  };

  o << indent << "\"vertices\" : [";

  const float *verts = shape.vertices();
  for (unsigned v = 0; v < shape.vertexCount(); ++v, verts += shape.vertexStride())
  {
    if (v > 0)
    {
      o << ',';
    }
    o << '\n' << indent << "  " << verts[v + 0] << ", " << verts[v + 1] << ", " << verts[v + 2];
  }
  o << '\n' << indent << "]";
  dangling = true;

  if (shape.indexCount())
  {
    closeDangling(dangling);
    o << indent << "\"indices\" : [";
    for (unsigned i = 0; i < shape.indexCount(); ++i)
    {
      if (i > 0)
      {
        o << ", ";
      }

      if (i % 16 == 0)
      {
        o << '\n' << indent << "  ";
      }

      o << shape.indices()[i];
    }
    o << '\n' << indent << "]";
    dangling = true;
  }

  if (shape.normalsCount())
  {
    closeDangling(dangling);
    o << indent << "\"normals\" : [";

    const float *normals = shape.normals();
    for (unsigned n = 0; n < shape.normalsCount(); ++n, normals += shape.normalsStride())
    {
      if (n > 0)
      {
        o << ',';
      }
      o << '\n' << indent << "  " << normals[n + 0] << ',' << normals[n + 1] << ',' << normals[n + 2];
    }
    o << '\n' << indent << "]";
    dangling = true;
  }

  return o;
}


std::ostream &operator << (std::ostream &o, const Matrix4f &transform)
{
  o << "[\n";

  for (int i = 0; i < 4; ++i)
  {
    if (i > 0)
    {
      o << ",\n";
    }
    o << transform.rc[i][0] << ", " << transform.rc[i][1] << ", " << transform.rc[i][2] << ", " << transform.rc[i][3];
  }

  o << " ]";
  return o;
}


std::ostream &logMeshResource(std::ostream &o, const MeshResource &mesh, const std::string &indent, bool vertexOnly = false)
{
  std::string indent2 = indent + "  ";
  o << indent << "\"mesh\" : {\n"
    << indent2 << "\"id\" : " << mesh.id() << ",\n"
    << indent2 << "\"typeId\" : " << mesh.typeId() << ",\n"
    << indent2 << "\"uniqueKey\" : " << mesh.uniqueKey() << ",\n"
    << indent2 << "\"drawType\" : " << '"' << drawTypeString((DrawType)mesh.drawType()) << '"' << ",\n"
    << indent2 << "\"tint\" : " << mesh.tint() << ",\n"
    << indent2 << "\"transform\" : " << mesh.transform() << ",\n";

  bool dangling = false;
  auto closeDangling = [&o] (bool &dangling)
  {
    if (dangling)
    {
      dangling = false;
      o << ",\n";
    }
  };

  unsigned stride = 0;
  if (mesh.vertexCount())
  {
    closeDangling(dangling);
    o << indent2 << "\"vertices\" : [";
    const float *verts = mesh.vertices(stride);
    stride /= sizeof(*verts);
    for (unsigned v = 0; v < mesh.vertexCount(); ++v, verts += stride)
    {
      if (v > 0)
      {
        o << ',';
      }
      o << '\n' << indent2 << "  " << verts[v + 0] << ", " << verts[v + 1] << ", " << verts[v + 2];
    }
    o << '\n' << indent2 << "]";
    dangling = true;
  }

  if (!vertexOnly && mesh.indexCount())
  {
    closeDangling(dangling);
    o << indent2 << "\"indices\" : [";
    unsigned indexWidth = 0;
    const uint8_t *indBytes = mesh.indices(stride, indexWidth);

    for (unsigned i = 0; i < mesh.indexCount(); ++i, indBytes += stride)
    {
      if (i > 0)
      {
        o << ", ";
      }

      if (i % 20 == 0)
      {
        o << '\n' << indent2 << "  ";
      }

      switch (indexWidth)
      {
      case 1:
        o << int(*indBytes);
        break;
      case 2:
        o << *(const uint16_t *)indBytes;
        break;
      case 4:
        o << *(const uint32_t *)indBytes;
        break;
      default:
        o << "\"invalid\" : true\n";
        i = mesh.indexCount();
        break;
      }
    }
    o << '\n' << indent2 << "]";
    dangling = true;
  }

  if (!vertexOnly && mesh.normals(stride))
  {
    closeDangling(dangling);
    o << indent2 << "\"normals\" : [";
    const float *normals = mesh.normals(stride);
    stride /= sizeof(*normals);
    for (unsigned n = 0; n < mesh.vertexCount(); ++n, normals += stride)
    {
      if (n > 0)
      {
        o << ',';
      }
      o << '\n' << indent2 << "  " << normals[n + 0] << ", " << normals[n + 1] << ", " << normals[n + 2];
    }
    o << '\n' << indent2 << "]";
    dangling = true;
  }

  if (!vertexOnly && mesh.uvs(stride))
  {
    closeDangling(dangling);
    o << indent2 << "\"uvs\" : [";
    const float *uvs = mesh.uvs(stride);
    stride /= sizeof(*uvs);
    for (unsigned u = 0; u < mesh.vertexCount(); ++u, uvs += stride)
    {
      if (u > 0)
      {
        o << ',';
      }
      o << '\n' << indent2 << "  " << uvs[u + 0] << ", " << uvs[u + 1];
    }
    o << '\n' << indent2 << "]";
    dangling = true;
  }

  if (mesh.colours(stride))
  {
    closeDangling(dangling);
    o << indent << "\"colours\" : [";
    const uint32_t *colours = mesh.colours(stride);
    stride /= sizeof(*colours);
    for (unsigned c = 0; c < mesh.vertexCount(); ++c, colours += stride)
    {
      if (c > 0)
      {
        o << ',';
      }

      if (c % 20 == 0)
      {
        o << '\n' << indent2 << "  ";
      }
      o << colours[c];
    }
    o << '\n' << indent2 << "]";
    dangling = true;
  }

  o << '\n' << indent << "}";

  return o;
}


std::ostream &logShapeExtensions(std::ostream &o, const PointCloudShape &shape, const std::string &indent)
{
  o << ",\n";
  o << indent << "\"pointSize\" : " << int(shape.pointSize()) << ",\n";

  logMeshResource(o, *shape.mesh(), indent, true);

  if (shape.indexCount())
  {
    o << ",\n";
    o << indent << "\"indices\" : [";
    for (unsigned i = 0; i < shape.indexCount(); ++i)
    {
      if (i % 20 == 0)
      {
        o << '\n' << indent;
      }

      if (i > 0)
      {
        o << ',';
      }

      o << shape.indices()[i];
    }
    o << indent << "\n]";
  }

  return o;
}


std::ostream &logShapeExtensions(std::ostream &o, const MeshSet &shape, const std::string &indent)
{
  o << ",\n";
  o << indent << "\"parts\" : {\n";

  std::string indent2 = indent + "  ";
  std::string indent3 = indent2 + "  ";
  for (int i = 0; i < shape.partCount(); ++i)
  {
    if (i > 0)
    {
      o << ",\n";
    }
    o << indent2 << "\"part-" << i << "\" : {\n";
    o << indent3 << "\"transform\" : " << shape.partTransform(i) << ",\n";
    logMeshResource(o, *shape.partAt(i), indent3) << '\n';
    o << indent2 << "}";
  }

  o << '\n' << indent << "}";

  return o;
}


template <typename T>
std::ostream &logShape(std::ostream &o, const T &shape, const char *suffix)
{
  auto precision = o.precision(20);

  o << "  \"" << shape.type() << suffix << "\" : {\n"
    << "    \"routingId\" : " << shape.routingId() << ",\n"
    << "    \"id\" : " << shape.data().id << ",\n"
    << "    \"category\" : " << shape.data().category << ",\n"
    << "    \"flags\" : " << shape.data().flags << ",\n"
    << "    \"reserved\" : " << shape.data().reserved << ",\n"
    << "    \"attributes\" : {\n"
    << "      \"colour\" : " << shape.data().attributes.colour << ",\n"
    << "      \"position\" : [\n"
    << "        " << shape.data().attributes.position[0] << ", " << shape.data().attributes.position[1] << ", " << shape.data().attributes.position[2] << "\n"
    << "      ],\n"
    << "      \"rotation\" : [\n"
    << "        " << shape.data().attributes.rotation[0] << ", " << shape.data().attributes.rotation[1] << ", " << shape.data().attributes.rotation[2] << ", " << shape.data().attributes.rotation[3] << "\n"
    << "      ],\n"
    << "      \"scale\" : [\n"
    << "        " << shape.data().attributes.scale[0] << ", " << shape.data().attributes.scale[1] << ", " << shape.data().attributes.scale[2] << "\n"
    << "      ]\n"
    << "    }";

    std::string indent = "    ";
    logShapeExtensions(o, shape, indent) << ",\n";

    o << "    \"isComplex\" : " << (shape.isComplex() ? "true" : "false") << "\n";

    o << "  }";

  o.precision(precision);

  return o;
}


const char *coordinateFrameString(uint8_t frame)
{
  const char *frames[] =
  {
    "xyz",
    "xz-y",
    "yx-z",
    "yzx",
    "zxy",
    "zy-x",
    "xy-z",
    "xzy",
    "yxz",
    "yz-x",
    "zx-y",
    "zyx"
  };

  if (frame < sizeof(frames) / sizeof(frames[0]))
  {
    return frames[frame];
  }

  return "unknown";
}


std::ostream &operator << (std::ostream &o, const ServerInfoMessage &info)
{
  o << "  \"server\" : {\n"
    << "    \"timeUnit\" : " << info.timeUnit << ",\n"
    << "    \"defaultFrameTime\" : " << info.defaultFrameTime << ",\n"
    << "    \"coordinateFrame\" : \"" << coordinateFrameString(info.coordinateFrame) << "\"\n"
    << "  },";
  return o;
}


/// Add @p shape to the @p server and @p shapes, printing it's attributes in JSON to @c stdout.
template <class T>
void addShape(T *shape, Server *server, std::vector<Shape *> &shapes, const char *suffix = "")
{
  server->create(*shape);
  logShape(std::cout, *shape, suffix) << ",\n";
  shapes.push_back(shape);
}


void showUsage(int argc, char **argv)
{
  std::cout << "Usage:\n";
  std::cout << argv[0] << " [options] [shapes]\n";
  std::cout << "\nValid options:\n";
  std::cout << "  help: show this message\n";
  if (checkFeature(TFeatureCompression))
  {
    std::cout << "  compress: write collated and compressed packets\n";
  }
  std::cout.flush();
}


int main(int argc, char **argvNonConst)
{
  const char **argv = const_cast<const char **>(argvNonConst);
  signal(SIGINT, &onSignal);
  signal(SIGTERM, &onSignal);

  if (haveOption("help", argc, argv))
  {
    showUsage(argc, argvNonConst);
    return 0;
  }

  ServerInfoMessage info;
  initDefaultServerInfo(&info);
  info.coordinateFrame = XYZ;
  unsigned serverFlags = SF_Collate;
  if (haveOption("compress", argc, argv))
  {
    serverFlags |= SF_Compress;
  }
  Server *server = Server::create(ServerSettings(serverFlags), &info);

  std::cout << "{\n";
  std::cout << info << std::endl;

  if (server->connectionMonitor()->waitForConnection(20000U) > 0)
  {
    server->connectionMonitor()->commitConnections();
  }

  server->updateTransfers(0);
  server->updateFrame(0.0f, true);
  ConnectionMonitor *_conMon = (server)->connectionMonitor();
  if (_conMon->mode() == ConnectionMonitor::Synchronous)
  {
    _conMon->monitorConnections();
  }
  _conMon->commitConnections();

  defineCategory(server, "Root", 0, 0, true);
  defineCategory(server, "Branch1", 1, 0, true);
  defineCategory(server, "Branch2", 2, 0, true);
  defineCategory(server, "Branch3", 3, 0, true);
  defineCategory(server, "Branch4-hidden", 4, 0, false);

  defineCategory(server, "Child1", 101, 1, true);
  defineCategory(server, "Child2", 102, 1, true);
  defineCategory(server, "Child3", 103, 1, true);
  defineCategory(server, "Child4", 104, 1, true);

  unsigned nextId = 1u;
  std::vector<Shape *> shapes;
  std::vector<MeshResource *> resources;

  addShape(
    initShape(new Arrow(nextId++, Vector3f(0.0f), Vector3f(1, 0, 0), 1.0f, 0.25f)),
    server, shapes);
  addShape(
    initShape(new Box(nextId++, Vector3f(0.0f), Vector3f(0.1f, 0.2f, 0.23f), rotationToQuaternion(Matrix3f::rotation(degToRad(15.0f), degToRad(25.0f), degToRad(-9.0f))))),
    server, shapes);
  addShape(
    initShape(new Capsule(nextId++, Vector3f(0.0f), Vector3f(1, 2, 0).normalised(), 0.3f, 2.0f)),
    server, shapes);
  addShape(
    initShape(new Cone(nextId++, Vector3f(0.0f), Vector3f(0, 2, 1).normalised(), degToRad(35.0f), 2.25f)),
    server, shapes);
  addShape(
    initShape(new Cylinder(nextId++, Vector3f(0.0f), Vector3f(2, -1.4f, 1).normalised(), 0.15f, 1.2f)),
    server, shapes);
  addShape(
    initShape(new Plane(nextId++, Vector3f(0.0f), Vector3f(-1, -1, 1).normalised())),
    server, shapes);
  addShape(
    initShape(new Sphere(nextId++, Vector3f(0.0f), 1.15f)),
    server, shapes);
  addShape(
    initShape(new Star(nextId++, Vector3f(0.0f), 0.15f)),
    server, shapes);
  addShape(
    initShape(new Text2D("Hello Text2D", nextId++)),
    server, shapes);
  addShape(
    initShape(new Text3D("Hello Text3D", nextId++)),
    server, shapes);

  std::vector<Vector3f> sphereVerts;
  std::vector<unsigned> sphereIndices;

  // Use a large sphere to ensure we need multiple data packets to transfer the vertices.
  sphereSubdivision(sphereVerts, sphereIndices, 2.1f, 4);

  addShape(createPointsMesh(nextId++, sphereVerts), server, shapes, "-points");
  addShape(createLinesMesh(nextId++, sphereVerts, sphereIndices), server, shapes, "-lines");
  addShape(createTrianglesMesh(nextId++, sphereVerts, sphereIndices), server, shapes, "-triangles");
  addShape(createVoxelsMesh(nextId++), server, shapes, "-voxels");
  addShape(createCloud(nextId++, sphereVerts, resources), server, shapes);
  addShape(createMeshSet(nextId++, sphereVerts, sphereIndices, resources), server, shapes);

  server->updateTransfers(0);
  server->updateFrame(0.0f, true);

  server->close();

  server->connectionMonitor()->stop();
  server->connectionMonitor()->join();

  server->dispose();
  server = nullptr;

  for (Shape *shape : shapes)
  {
    delete shape;
  }
  shapes.clear();

  for (Resource *resource : resources)
  {
    delete resource;
  }

  // Next line is partly to keep well formed JSON.
  std::cout << "  \"success\" : true\n";
  std::cout << "}\n";

  return 0;
}
