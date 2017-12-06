//
// author: Kazys Stepanas
//

#include <3esconnection.h>
#include <3esconnectionmonitor.h>
#include <3escoordinateframe.h>
#include <3esfeature.h>
#include <3esserver.h>
#include <shapes/3esshapes.h>

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


enum Categories
{
  CatRoot,
  Cat3D,
  CatText,
  CatSimple3D,
  CatComplex3D,
  CatArrow,
  CatBox,
  CatCapsule,
  CatCylinder,
  CatCone,
  CatLines,
  CatMesh,
  CatPlane,
  CatPoints,
  CatSphere,
  CatStar,
  CatText2D,
  CatText3D,
  CatTriangles
};


class ShapeMover
{
public:
  ShapeMover(Shape *shape)
    : _shape(shape)
  {
  }

  virtual inline ~ShapeMover() {}

  inline Shape *shape() { return _shape; }
  inline const Shape *shape() const { return _shape; }
  inline void setShape(Shape *shape) { _shape = shape; onShapeChange(); }

  virtual void reset() {}

  virtual void update(float time, float dt) {}

protected:
  virtual void onShapeChange() {}

private:
  Shape *_shape;
};


class Oscilator : public ShapeMover
{
public:
  inline Oscilator(Shape *shape, float amplitude = 1.0f, float period = 5.0f, const Vector3f &axis = Vector3f(0, 0, 1))
    : ShapeMover(shape)
    , _axis(axis)
    , _referencePos(shape ? shape->position() : Vector3f::zero)
    , _amplitude(amplitude)
    , _period(period)
  {
    onShapeChange();
  }

  inline const Vector3f &axis() const { return _axis; }
  inline const Vector3f &referencePos() const { return _referencePos; }
  inline float amplitude() const { return _amplitude; }
  inline float period() const { return _period; }

  void reset() override
  {
    _referencePos = shape() ? shape()->position() : _referencePos;
  }

  void update(float time, float dt) override
  {
    Vector3f pos = _referencePos + _amplitude * std::sin(time) * _axis;
    shape()->setPosition(pos);
  }

protected:
  void onShapeChange() override
  {
    _referencePos = (shape()) ? shape()->position() : _referencePos;
  }

private:
  Vector3f _referencePos;
  Vector3f _axis;
  float _amplitude;
  float _period;
};


MeshResource *createTestMesh()
{
  SimpleMesh *mesh = new SimpleMesh(1, 4, 6, DtTriangles, SimpleMesh::Vertex | SimpleMesh::Index | SimpleMesh::Colour);
  mesh->setVertex(0, Vector3f(-0.5f, 0, -0.5f));
  mesh->setVertex(1, Vector3f(0.5f, 0, -0.5f));
  mesh->setVertex(2, Vector3f(0.5f, 0,  0.5f));
  mesh->setVertex(3, Vector3f(-0.5f, 0,  0.5f));

  mesh->setIndex(0, 0);
  mesh->setIndex(1, 1);
  mesh->setIndex(2, 2);
  mesh->setIndex(3, 0);
  mesh->setIndex(4, 2);
  mesh->setIndex(5, 3);

  mesh->setColour(0, 0xff0000ff);
  mesh->setColour(1, 0xffff00ff);
  mesh->setColour(2, 0xff00ffff);
  mesh->setColour(3, 0xffffffff);

  //mesh->setNormal(0, Vector3f(0, 1, 0));
  //mesh->setNormal(1, Vector3f(0, 1, 0));
  //mesh->setNormal(2, Vector3f(0, 1, 0));
  //mesh->setNormal(3, Vector3f(0, 1, 0));

  return mesh;
}


MeshResource *createTestCloud()
{
  PointCloud *cloud = new PointCloud(2);  // Considered a Mesh for ID purposes.
  cloud->resize(8);

  cloud->setPoint(0, Vector3f(0, 0, 0), Vector3f(0, 0, 1), Colour(0, 255, 255));
  cloud->setPoint(1, Vector3f(1, 0, 0), Vector3f(0, 0, 1), Colour(0, 255, 255));
  cloud->setPoint(2, Vector3f(0, 1, 0), Vector3f(0, 0, 1), Colour(255, 255, 255));
  cloud->setPoint(3, Vector3f(0, 0, 1), Vector3f(0, 0, 1), Colour(0, 255, 255));
  cloud->setPoint(4, Vector3f(1, 1, 0), Vector3f(0, 0, 1), Colour(0, 0, 0));
  cloud->setPoint(5, Vector3f(0, 1, 1), Vector3f(0, 0, 1), Colour(0, 255, 255));
  cloud->setPoint(6, Vector3f(1, 0, 1), Vector3f(0, 0, 1), Colour(0, 255, 255));
  cloud->setPoint(7, Vector3f(1, 1, 1), Vector3f(0, 0, 1), Colour(0, 255, 255));

  return cloud;
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


void createAxes(unsigned &nextId, std::vector<Shape *> &shapes, std::vector<ShapeMover *> &movers, std::vector<Resource *> &resources, int argc, const char **argv)
{
  if (!haveOption("noaxes", argc, argv))
  {
    const float arrowLength = 1.0f;
    const float arrowRadius = 0.025f;
    const Vector3f pos(0.0f);
    Arrow *arrow;

    arrow = new Arrow(nextId++, pos, Vector3f(1, 0, 0), arrowLength, arrowRadius);
    arrow->setColour(Colour::Colours[Colour::Red]);
    shapes.push_back(arrow);

    arrow = new Arrow(nextId++, pos, Vector3f(0, 1, 0), arrowLength, arrowRadius);
    arrow->setColour(Colour::Colours[Colour::ForestGreen]);
    shapes.push_back(arrow);

    arrow = new Arrow(nextId++, pos, Vector3f(0, 0, 1), arrowLength, arrowRadius);
    arrow->setColour(Colour::Colours[Colour::DodgerBlue]);
    shapes.push_back(arrow);
  }
}


void createShapes(unsigned &nextId, std::vector<Shape *> &shapes, std::vector<ShapeMover *> &movers, std::vector<Resource *> &resources, int argc, const char **argv)
{
  bool allShapes = haveOption("all", argc, argv);
  bool noMove = haveOption("nomove", argc, argv);
  size_t initialShapeCount = shapes.size();

  if (allShapes || haveOption("arrow", argc, argv))
  {
    Arrow *arrow = new Arrow(nextId++, CatArrow);
    arrow->setRadius(0.5f);
    arrow->setLength(1.0f);
    arrow->setColour(Colour::Colours[Colour::SeaGreen]);
    shapes.push_back(arrow);
    if (!noMove)
    {
      movers.push_back(new Oscilator(arrow, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("box", argc, argv))
  {
    Box *box = new Box(nextId++, CatBox);
    box->setScale(Vector3f(0.45f));
    box->setColour(Colour::Colours[Colour::MediumSlateBlue]);
    shapes.push_back(box);
    if (!noMove)
    {
      movers.push_back(new Oscilator(box, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("capsule", argc, argv))
  {
    Capsule *capsule = new Capsule(nextId++, CatCapsule, Vector3f(0, 0, 0));
    capsule->setLength(2.0f);
    capsule->setRadius(0.3f);
    capsule->setColour(Colour::Colours[Colour::LavenderBlush]);
    shapes.push_back(capsule);
    if (!noMove)
    {
      movers.push_back(new Oscilator(capsule, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("cone", argc, argv))
  {
    Cone *cone = new Cone(nextId++, CatCone);
    cone->setLength(2.0f);
    cone->setAngle(25.0f / 360.0f * float(M_PI));
    cone->setColour(Colour::Colours[Colour::SandyBrown]);
    shapes.push_back(cone);
    if (!noMove)
    {
      movers.push_back(new Oscilator(cone, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("cylinder", argc, argv))
  {
    Cylinder *cylinder = new Cylinder(nextId++, CatCylinder);
    cylinder->setScale(Vector3f(0.45f));
    cylinder->setColour(Colour::Colours[Colour::FireBrick]);
    shapes.push_back(cylinder);
    if (!noMove)
    {
      movers.push_back(new Oscilator(cylinder, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("plane", argc, argv))
  {
    Plane *plane = new Plane(nextId++, CatPlane);
    plane->setNormal(Vector3f(1.0f, 1.0f, 0.0f).normalised());
    plane->setScale(1.5f);
    plane->setNormalLength(0.5f);
    plane->setColour(Colour::Colours[Colour::LightSlateGrey]);
    shapes.push_back(plane);
    if (!noMove)
    {
      movers.push_back(new Oscilator(plane, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("sphere", argc, argv))
  {
    Sphere *sphere = new Sphere(nextId++, CatSphere);
    sphere->setRadius(0.75f);
    sphere->setColour(Colour::Colours[Colour::Coral]);
    shapes.push_back(sphere);
    if (!noMove)
    {
      movers.push_back(new Oscilator(sphere, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("star", argc, argv))
  {
    Star *star = new Star(nextId++, CatStar);
    star->setRadius(0.75f);
    star->setColour(Colour::Colours[Colour::DarkGreen]);
    shapes.push_back(star);
    if (!noMove)
    {
      movers.push_back(new Oscilator(star, 2.0f, 2.5f));
    }
  }

  if (allShapes || haveOption("lines", argc, argv))
  {
    static const Vector3f lineSet[] =
    {
      Vector3f(0, 0, 0), Vector3f(0, 0, 1),
      Vector3f(0, 0, 1), Vector3f(0.25f, 0, 0.8f),
      Vector3f(0, 0, 1), Vector3f(-0.25f, 0, 0.8f)
    };
    const unsigned lineVertexCount = sizeof(lineSet) / sizeof(lineSet[0]);
    MeshShape *lines = new MeshShape(DtLines, lineSet[0].v, lineVertexCount, sizeof(lineSet[0]), nextId++, CatLines);
    shapes.push_back(lines);
    // if (!noMove)
    // {
    //   movers.push_back(new Oscilator(mesh, 2.0f, 2.5f));
    // }
  }

  if (allShapes || haveOption("triangles", argc, argv))
  {
    static const Vector3f triangleSet[] =
    {
      Vector3f(0, 0, 0), Vector3f(0, 0.25f, 1), Vector3f(0.25f, 0, 1),
      Vector3f(0, 0, 0), Vector3f(-0.25f, 0, 1), Vector3f(0, 0.25f, 1),
      Vector3f(0, 0, 0), Vector3f(0, -0.25f, 1), Vector3f(-0.25f, 0, 1),
      Vector3f(0, 0, 0), Vector3f(0.25f, 0, 1), Vector3f(0, -0.25f, 1)
    };
    const unsigned triVertexCount = sizeof(triangleSet) / sizeof(triangleSet[0]);
    MeshShape *triangles = new MeshShape(DtTriangles, triangleSet[0].v, triVertexCount, sizeof(triangleSet[0]), nextId++, CatTriangles);
    shapes.push_back(triangles);
    // if (!noMove)
    // {
    //   movers.push_back(new Oscilator(mesh, 2.0f, 2.5f));
    // }
  }

  if (allShapes || haveOption("mesh", argc, argv))
  {
    MeshResource *mesRes = createTestMesh();
    resources.push_back(mesRes);
    MeshSet *mesh = new MeshSet(mesRes, nextId++, CatMesh);
    shapes.push_back(mesh);
    // if (!noMove)
    // {
    //   movers.push_back(new Oscilator(mesh, 2.0f, 2.5f));
    // }
  }

  if (allShapes || haveOption("points", argc, argv))
  {
    static const Vector3f pts[] =
    {
      Vector3f(0, 0, 0), Vector3f(0, 0.25f, 1), Vector3f(0.25f, 0, 1),
      Vector3f(0, 0, 0), Vector3f(-0.25f, 0, 1), Vector3f(0, 0.25f, 1),
      Vector3f(0, 0, 0), Vector3f(0, -0.25f, 1), Vector3f(-0.25f, 0, 1),
      Vector3f(0, 0, 0), Vector3f(0.25f, 0, 1), Vector3f(0, -0.25f, 1)
    };
    const unsigned pointsCount = sizeof(pts) / sizeof(pts[0]);
    MeshShape *points = new MeshShape(DtPoints, pts[0].v, pointsCount, sizeof(pts[0]), nextId++, CatPoints);
    shapes.push_back(points);
    // if (!noMove)
    // {
    //   movers.push_back(new Oscilator(mesh, 2.0f, 2.5f));
    // }
  }

  if (allShapes || haveOption("cloud", argc, argv) || haveOption("cloudpart", argc, argv))
  {
    MeshResource *cloud = createTestCloud();
    PointCloudShape *points = new PointCloudShape(cloud, nextId++, CatPoints, 16u);
    if (haveOption("cloudpart", argc, argv))
    {
      // Partial indexing.
      std::vector<unsigned> partialIndices;
      partialIndices.resize((cloud->vertexCount() + 1) / 2);
      unsigned nextIndex = 0;
      for (size_t i = 0; i < partialIndices.size(); ++i)
      {
        partialIndices[i] = nextIndex;
        nextIndex += 2;
      }
      points->setIndices(partialIndices.begin(), (uint32_t)partialIndices.size());
    }
    shapes.push_back(points);
    resources.push_back(cloud);
    // if (!noMove)
    // {
    //   movers.push_back(new Oscilator(points, 2.0f, 2.5f));
    // }
  }

  if (haveOption("wire", argc, argv))
  {
    for (size_t i = initialShapeCount; i < shapes.size(); ++i)
    {
      shapes[i]->setWireframe(true);
    }
  }

  // Position the shapes so they aren't all on top of one another.
  if (shapes.size() > initialShapeCount)
  {
    Vector3f pos(0.0f);
    const float spacing = 2.0f;
    pos.x -= spacing * ((shapes.size() - initialShapeCount) / 2u);

    for (size_t i = initialShapeCount; i < shapes.size(); ++i)
    {
      shapes[i]->setPosition(pos);
      pos.x += spacing;
    }

    for (ShapeMover *mover : movers)
    {
      mover->reset();
    }
  }


  // Add text after positioning and mover changes to keep fixed positions.
  if (allShapes || haveOption("text2d", argc, argv))
  {
    Text2D *text;
    text = new Text2D("Hello Screen", nextId++, CatText2D, Vector3f(0.25f, 0.75f, 0.0f));
    shapes.push_back(text);
    text = new Text2D("Hello World 2D", nextId++, CatText2D, Vector3f(1.0f, 1.0f, 1.0f));
    text->setInWorldSpace(true);
    shapes.push_back(text);
  }

  if (allShapes || haveOption("text3d", argc, argv))
  {
    Text3D *text;
    text = new Text3D("Hello World 3D", nextId++, CatText3D, Vector3f(-1.0f, -1.0f, 1.0f), Vector3f(-1.0f, 0, 0));
    shapes.push_back(text);
    text = new Text3D("Hello World 3D Facing", nextId++, CatText3D, Vector3f(-1.0f, -1.0f, 0.0f), 8);
    text->setScreenFacing(true);
    shapes.push_back(text);
  }

  // Did we create anything?
  if (initialShapeCount == shapes.size())
  {
    // Nothing created. Create the default shape by providing some fake arguments.
    const char *defaultArgv[] =
    {
      "this arg is not read",
      "sphere"
    };

    createShapes(nextId, shapes, movers, resources, sizeof(defaultArgv) / sizeof(defaultArgv[0]), defaultArgv);
  }
}


void showUsage(int argc, char **argv)
{
  std::cout << "Usage:\n";
  std::cout << argv[0] << " [options] [shapes]\n";
  std::cout << "\nValid options:\n";
  std::cout << "  help: show this message\n";
  if (tes::checkFeature(tes::TFeatureCompression))
  {
    std::cout << "  compress: write collated and compressed packets\n";
  }
  std::cout << "  noaxes: Don't create axis arrow objects\n";
  std::cout << "  nomove: don't move objects (keep stationary)\n";
  std::cout << "  wire: Show wireframe shapes, not slide for relevant objects\n";
  std::cout << "\nValid shapes:\n";
  std::cout << "\tall: show all shapes\n";
  std::cout << "\tarrow\n";
  std::cout << "\tbox\n";
  std::cout << "\tcapsule\n";
  std::cout << "\tcloud\n";
  std::cout << "\tcloudpart\n";
  std::cout << "\tcone\n";
  std::cout << "\tcylinder\n";
  std::cout << "\tlines\n";
  std::cout << "\tmesh\n";
  std::cout << "\tplane\n";
  std::cout << "\tpoints\n";
  std::cout << "\tsphere\n";
  std::cout << "\tstar\n";
  std::cout << "\ttext2d\n";
  std::cout << "\ttext3d\n";
  std::cout << "\ttriangles\n";
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

  std::vector<Shape *> shapes;
  std::vector<ShapeMover *> movers;
  std::vector<Resource *> resources;

  unsigned nextId = 1;
  createAxes(nextId, shapes, movers, resources, argc, argv);
  createShapes(nextId, shapes, movers, resources, argc, argv);

  const unsigned targetFrameTimeMs = 1000 / 30;
  float time = 0;
  auto lastTime = std::chrono::system_clock::now();
  auto onNewConnection = [&shapes](Server &/*server*/, Connection &connection)
  {
    // Test categories API.
    TES_CATEGORY(&connection, "3D", Cat3D, CatRoot, true);
    TES_CATEGORY(&connection, "Text", CatText, CatRoot, true);
    TES_CATEGORY(&connection, "Primitives", CatSimple3D, Cat3D, true);
    TES_CATEGORY(&connection, "Mesh Based", CatComplex3D, Cat3D, true);
    TES_CATEGORY(&connection, "Arrows", CatArrow, CatSimple3D, true);
    TES_CATEGORY(&connection, "Boxes", CatBox, CatSimple3D, true);
    TES_CATEGORY(&connection, "Capsules", CatCapsule, CatSimple3D, true);
    TES_CATEGORY(&connection, "Cylinders", CatCylinder, CatSimple3D, true);
    TES_CATEGORY(&connection, "Cones", CatCone, CatSimple3D, true);
    TES_CATEGORY(&connection, "Lines", CatLines, CatComplex3D, true);
    TES_CATEGORY(&connection, "Meshes", CatMesh, CatComplex3D, true);
    TES_CATEGORY(&connection, "Planes", CatPlane, CatSimple3D, true);
    TES_CATEGORY(&connection, "Points", CatPoints, CatComplex3D, true);
    TES_CATEGORY(&connection, "Spheres", CatSphere, CatSimple3D, true);
    TES_CATEGORY(&connection, "Stars", CatStar, CatSimple3D, true);
    TES_CATEGORY(&connection, "Text2D", CatText2D, CatText, true);
    TES_CATEGORY(&connection, "Text3D", CatText3D, CatText, true);
    TES_CATEGORY(&connection, "Triangles", CatTriangles, CatComplex3D, true);
    for (Shape *shape : shapes)
    {
      connection.create(*shape);
    }
  };

  // Register shapes with server.
  for (Shape *shape : shapes)
  {
    server->create(*shape);
  }

  server->connectionMonitor()->setConnectionCallback(onNewConnection);
  server->connectionMonitor()->start(tes::ConnectionMonitor::Asynchronous);

  while (!quit)
  {
    auto now = std::chrono::system_clock::now();
    auto elapsed = now - lastTime;

    lastTime = now;
    float dt = std::chrono::duration_cast<std::chrono::microseconds>(elapsed).count() * 1e-6f;
    time += dt;

    for (ShapeMover *mover : movers)
    {
      mover->update(time, dt);
      server->update(*mover->shape());
    }

    server->updateFrame(dt);
    if (server->connectionMonitor()->mode() == tes::ConnectionMonitor::Synchronous)
    {
      server->connectionMonitor()->monitorConnections();
    }
    server->connectionMonitor()->commitConnections();
    server->updateTransfers(64 * 1024);

    printf("\rFrame %f: %u connection(s)    ", dt, server->connectionCount());
    fflush(stdout);

    now = std::chrono::system_clock::now();
    elapsed = now - lastTime;
    unsigned elapsedMs = unsigned(std::chrono::duration_cast<std::chrono::milliseconds>(elapsed).count());
    unsigned sleepTimeMs = (elapsedMs <= targetFrameTimeMs) ? targetFrameTimeMs - elapsedMs : 0u;
    std::this_thread::sleep_for(std::chrono::milliseconds(sleepTimeMs));
  }

  for (ShapeMover *mover : movers)
  {
    delete mover;
  }
  movers.clear();

  for (Shape *shape : shapes)
  {
    server->destroy(*shape);
    delete shape;
  }
  shapes.clear();

  for (Resource *resource : resources)
  {
    delete resource;
  }
  resources.clear();

  server->close();

  server->connectionMonitor()->stop();
  server->connectionMonitor()->join();

  server->dispose();
  server = nullptr;

  return 0;
}
