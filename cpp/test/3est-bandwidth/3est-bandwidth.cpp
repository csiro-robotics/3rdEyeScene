//
// author: Kazys Stepanas
//
#include <3esconnection.h>
#include <3esconnectionmonitor.h>
#include <3escoordinateframe.h>
#include <3esfeature.h>
#include <3esserver.h>
#include <3esspheretessellator.h>
#include <shapes/3esshapes.h>

#include <3esvector3.h>
#include <3estimer.h>
#include <shapes/3essimplemesh.h>
#include <shapes/3espointcloud.h>

#include <algorithm>
#include <cmath>
#include <csignal>
#include <chrono>
#include <iostream>
#include <thread>
#include <vector>

// Bandwidth test. Tessellate a sphere to a high number of polygons and repeatedly send this data to the server.

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

  const unsigned targetPolyCount = 10000;
  std::vector<Vector3f> vertices;
  std::vector<Vector3f> triangles;
  std::vector<unsigned> indices;
  tes::SphereVertexMap sphereMap;

  std::cout << "Tessellating to at least " << targetPolyCount << " polygons." << std::endl;

  tes::sphereInitialise(vertices, indices, &sphereMap);
  while (indices.size() / 3 < targetPolyCount)
  {
    tes::subdivideUnitSphere(vertices, indices, sphereMap);
  }

  std::cout << "Created " << indices.size() / 3 << " triangles." << std::endl;

  // Unwrap the "mesh" to use contiguous indexing. This will duplicate vertices.
  std::cout << "Unrolling indexing." << std::endl;

  triangles.reserve(indices.size());
  for (unsigned vindex : indices)
  {
    triangles.push_back(vertices[vindex]);
  }
  vertices.clear();
  indices.clear();

  std::cout << "Starting server and sending triangle data." << std::endl;


  ServerInfoMessage info;
  initDefaultServerInfo(&info);
  info.coordinateFrame = XYZ;
  unsigned serverFlags = SF_Collate;
  if (haveOption("compress", argc, argv))
  {
    serverFlags |= SF_Compress;
  }

  Server *server = Server::create(ServerSettings(serverFlags), &info);

  const unsigned targetFrameTimeMs = 1000 / 30;
  float time = 0;
  auto lastTime = std::chrono::system_clock::now();

  server->connectionMonitor()->start(tes::ConnectionMonitor::Asynchronous);

  while (!quit)
  {
    auto now = std::chrono::system_clock::now();
    auto elapsed = now - lastTime;

    lastTime = now;
    float dt = std::chrono::duration_cast<std::chrono::microseconds>(elapsed).count() * 1e-6f;
    time += dt;

    // Send triangle data in chunks.
    size_t offset = 0;
    size_t count = 0;
    const size_t limit = 64998; // Must be divisible by 3.

    MeshShape shape(DtTriangles, triangles.data()->v, (unsigned)triangles.size(), sizeof(*triangles.data()));  // Transient triangles.
    server->create(shape);

    server->updateFrame(0.0f);
    if (server->connectionMonitor()->mode() == tes::ConnectionMonitor::Synchronous)
    {
      server->connectionMonitor()->monitorConnections();
    }
    server->connectionMonitor()->commitConnections();
    server->updateTransfers(0);

    printf("\rFrame %f: %u connection(s)    ", dt, server->connectionCount());
    fflush(stdout);

    now = std::chrono::system_clock::now();
    elapsed = now - lastTime;
    //unsigned elapsedMs = unsigned(std::chrono::duration_cast<std::chrono::milliseconds>(elapsed).count());
    //unsigned sleepTimeMs = (elapsedMs <= targetFrameTimeMs) ? targetFrameTimeMs - elapsedMs : 0u;
    //std::this_thread::sleep_for(std::chrono::milliseconds(sleepTimeMs));
  }

  server->updateFrame(0.0f, false);
  server->close();

  server->connectionMonitor()->stop();
  server->connectionMonitor()->join();

  server->dispose();
  server = nullptr;

  return 0;
}
