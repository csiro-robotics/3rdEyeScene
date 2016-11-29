//
// author: Kazys Stepanas
//
#define _USE_MATH_DEFINES
#include "3es-sphere-view.h"

#define TES_ENABLE
#include <3esservermacros.h>
#include <3esvectorhash.h>

#include <algorithm>
#include <cmath>
#include <csignal>
#include <chrono>
#include <iostream>
#include <sstream>
#include <thread>
#include <unordered_map>
#include <vector>

#define TEXT_ID ((uint32_t)1)
#define SPHERE_ID 2

// Example to view a sphere tessellation. This code duplicates 3esspheretessellator code and adds 3ES commands.
typedef tes::Vector3f Vector3f;

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

  TES_SERVER_DECL(tesServer);
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
  std::cout << argv[0] << " [options] [iterations]\n";
  std::cout << "\nValid options:\n";
  std::cout << "  help: show this message\n";
  std::cout << "  collate: use packet collation.\n";
  TES_IF_FEATURES(TES_FEATURE_FLAG(tes::TFeatureCompression),
    std::cout << "  compress: collated and compress packets (implies collation).\n");
  std::cout.flush();
}

namespace
{
  struct SphereVertexHash
  {
    inline size_t operator()(const Vector3f &v) const
    {
      return tes::vhash::hash(v.x, v.y, v.z);
    }
  };

  typedef std::unordered_multimap<Vector3f, unsigned, SphereVertexHash> SphereVertexMap;


  int tesUnrollDisplay(const std::vector<Vector3f> &vertices, const std::vector<unsigned> &indices)
  {
    int shapeCount = 0;
    // Maximum 6500 vertices per message. Take it down to the nearest multiple of 3 (triangle).
    const size_t sendLimit = 64998;
    size_t cursor = 0;
    size_t count = 0;

    std::vector<Vector3f> localVertices(sendLimit);
    while (cursor < indices.size())
    {
      count = std::min(indices.size() - cursor, sendLimit);
      // Copy vertices into localVertices.
      for (size_t i = 0; i < count; ++i)
      {
        localVertices[i] = vertices[indices[cursor + i]];
      }

      cursor += count;
      TES_TRIANGLES(*tesServer, TES_RGB(200, 200, 200), localVertices.data()->v, (unsigned)count, sizeof(*localVertices.data()), SPHERE_ID + shapeCount);
      ++shapeCount;
    }

    return shapeCount;
  }


  /// Add a vertex to @p points, reusing an existing vertex is a matching one is found.
  ///
  /// This first searches for a matching vertex in @p point and returns its index if found.
  /// Otherwise a new vertex is added.
  ///
  /// @param vertex The vertex to add.
  /// @param vertices The vertex data to add to.
  /// @return The index which can be used to refer to the target vertex.
  unsigned insertVertex(const Vector3f &vertex, std::vector<Vector3f> &vertices, SphereVertexMap &vertexMap)
  {
    auto findResult = vertexMap.find(vertex);
    size_t hashVal = tes::vhash::hash(vertex.x, vertex.y, vertex.z);
    if (findResult != vertexMap.end())
    {
      do
      {
        if (findResult->first.isEqual(vertex, 0))
        {
          return findResult->second;
        }
        ++findResult;
      } while (findResult != vertexMap.end() && tes::vhash::hash(findResult->first.x, findResult->first.y, findResult->first.z) == hashVal);

    }


    // Add new vertex.
    unsigned idx = unsigned(vertices.size());
    vertices.push_back(vertex);
    vertexMap.insert(std::make_pair(vertex, idx));
    return idx;
  }

  void sphereInitialise(std::vector<Vector3f> &vertices, std::vector<unsigned> &indices, SphereVertexMap *vertexMap)
  {
    // We start with two hexagonal rings to approximate the sphere.
    // All subdivision occurs on a unit radius sphere, at the origin. We translate and
    // scale the vertices at the end.
    vertices.clear();
    indices.clear();

    static const float ringControlAngle = 25.0f / 180.0f * float(M_PI);
    static const float ringHeight = std::sin(ringControlAngle);
    static const float ringRadius = std::cos(ringControlAngle);
    static const float hexAngle = 2.0f * float(M_PI) / 6.0f;
    static const float ring2OffsetAngle = 0.5f * hexAngle;
    static const Vector3f initialVertices[] =
    {
      Vector3f(0, 0, 1),

      // Upper hexagon.
      Vector3f(ringRadius, 0, ringHeight),
      Vector3f(ringRadius * std::cos(hexAngle), ringRadius * std::sin(hexAngle), ringHeight),
      Vector3f(ringRadius * std::cos(2 * hexAngle), ringRadius * std::sin(2 * hexAngle), ringHeight),
      Vector3f(ringRadius * std::cos(3 * hexAngle), ringRadius * std::sin(3 * hexAngle), ringHeight),
      Vector3f(ringRadius * std::cos(4 * hexAngle), ringRadius * std::sin(4 * hexAngle), ringHeight),
      Vector3f(ringRadius * std::cos(5 * hexAngle), ringRadius * std::sin(5 * hexAngle), ringHeight),

      // Lower hexagon.
      Vector3f(ringRadius * std::cos(ring2OffsetAngle), ringRadius * std::sin(ring2OffsetAngle), -ringHeight),
      Vector3f(ringRadius * std::cos(ring2OffsetAngle + hexAngle), ringRadius * std::sin(ring2OffsetAngle + hexAngle), -ringHeight),
      Vector3f(ringRadius * std::cos(ring2OffsetAngle + 2 * hexAngle), ringRadius * std::sin(ring2OffsetAngle + 2 * hexAngle), -ringHeight),
      Vector3f(ringRadius * std::cos(ring2OffsetAngle + 3 * hexAngle), ringRadius * std::sin(ring2OffsetAngle + 3 * hexAngle), -ringHeight),
      Vector3f(ringRadius * std::cos(ring2OffsetAngle + 4 * hexAngle), ringRadius * std::sin(ring2OffsetAngle + 4 * hexAngle), -ringHeight),
      Vector3f(ringRadius * std::cos(ring2OffsetAngle + 5 * hexAngle), ringRadius * std::sin(ring2OffsetAngle + 5 * hexAngle), -ringHeight),

      Vector3f(0, 0, -1),
    };
    const unsigned initialVertexCount = sizeof(initialVertices) / sizeof(initialVertices[0]);

    const unsigned initialIndices[] =
    {
      0, 1, 2,    0, 2, 3,    0, 3, 4,    0, 4, 5,    0, 5, 6,    0, 6, 1,

      1, 7, 2,    2, 8, 3,    3, 9, 4,    4, 10, 5,   5, 11, 6,   6, 12, 1,

      7, 8, 2,    8, 9, 3,    9, 10, 4,   10, 11, 5,  11, 12, 6,  12, 7, 1,

      7, 13, 8,   8, 13, 9,   9, 13, 10,  10, 13, 11, 11, 13, 12, 12, 13, 7
    };
    const unsigned initialIndexCount = sizeof(initialIndices) / sizeof(initialIndices[0]);

    for (unsigned i = 0; i < initialVertexCount; ++i)
    {
      unsigned idx = i;
      vertices.push_back(initialVertices[i]);
      if (vertexMap)
      {
        vertexMap->insert(std::make_pair(initialVertices[i], i));
      }
    }

    for (unsigned i = 0; i < initialIndexCount; i += 3)
    {
      indices.push_back(initialIndices[i + 0]);
      indices.push_back(initialIndices[i + 1]);
      indices.push_back(initialIndices[i + 2]);
    }

    // Send the initial sphere. We know it has less than 65k vertices.
    if (!vertices.empty() && !indices.empty())
    {
      TES_TRIANGLES(*tesServer, TES_RGB(200, 200, 200), vertices.data()->v, (unsigned)vertices.size(), sizeof(*vertices.data()),
                    indices.data(), (unsigned)indices.size(), SPHERE_ID);
    }
  }


  void subdivideUnitSphere(std::vector<Vector3f> &vertices, std::vector<unsigned> &indices, SphereVertexMap &vertexMap)
  {
    const unsigned triangleCount = unsigned(indices.size() / 3);
    unsigned triangle[3];
    unsigned abc[3];
    unsigned def[3];
    Vector3f verts[3];
    Vector3f newVertices[3];

    for (unsigned i = 0; i < triangleCount; ++i)
    {
      triangle[0] = abc[0] = indices[i * 3 + 0];
      triangle[1] = abc[1] = indices[i * 3 + 1];
      triangle[2] = abc[2] = indices[i * 3 + 2];

      // Fetch the vertices.
      verts[0] = vertices[triangle[0]];
      verts[1] = vertices[triangle[1]];
      verts[2] = vertices[triangle[2]];

      // Highlight the working triangle: extrude it a bit to make it pop.
      TES_TRIANGLE_W(*tesServer, TES_COLOUR(FireBrick), verts[0] * 1.01f, verts[1] * 1.01f, verts[2] * 1.01f);

      // Calculate the new vertex at the centre of the existing triangle.
      newVertices[0] = (0.5f * (verts[0] + verts[1])).normalised();
      newVertices[1] = (0.5f * (verts[1] + verts[2])).normalised();
      newVertices[2] = (0.5f * (verts[2] + verts[0])).normalised();

      // Create new triangles.
      // Given triangle ABC, and adding vertices DEF such that:
      //  D = AB/2  E = BC/2  F = CA/2
      // We have four new triangles:
      //  ADF, BED, CFE, DEF
      // ABC are in order in 'abc', while DEF will be in 'def'.
      // FIXME: find existing point to use.
      def[0] = insertVertex(newVertices[0], vertices, vertexMap);
      def[1] = insertVertex(newVertices[1], vertices, vertexMap);
      def[2] = insertVertex(newVertices[2], vertices, vertexMap);

      TES_TRIANGLE_IW(*tesServer, TES_COLOUR(Cyan), vertices.data()->v, def[0], def[1], def[2]);

      // Replace the original triangle ABC with DEF
      indices[i * 3 + 0] = def[0];
      indices[i * 3 + 1] = def[1];
      indices[i * 3 + 2] = def[2];

      // Add triangles ADF, BED, CFE
      indices.push_back(abc[0]);
      indices.push_back(def[0]);
      indices.push_back(def[2]);

      TES_TRIANGLE_IW(*tesServer, TES_COLOUR(Cyan), vertices.data()->v, abc[0], def[0], def[2]);

      indices.push_back(abc[1]);
      indices.push_back(def[1]);
      indices.push_back(def[0]);

      TES_TRIANGLE_IW(*tesServer, TES_COLOUR(Cyan), vertices.data()->v, abc[1], def[1], def[0]);

      indices.push_back(abc[2]);
      indices.push_back(def[2]);
      indices.push_back(def[1]);

      TES_TRIANGLE_IW(*tesServer, TES_COLOUR(Cyan), vertices.data()->v, abc[2], def[2], def[1]);

      TES_SERVER_UPDATE(*tesServer, 0, true); // Flush.
    }
  }
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

  unsigned iterations = 5;
  if (argc > 1)
  {
    std::istringstream in(argv[argc - 1]);
    in >> iterations;
  }

  std::vector<Vector3f> vertices;
  std::vector<unsigned> indices;
  SphereVertexMap sphereMap;

  // Initialise settings: zero flags: no cache, compression or collation.
#ifdef TES_ENABLE
  unsigned serverFlags = 0;
  if (haveOption("collate", argc, argv))
  {
    serverFlags = tes::SF_Collate;
  }
  if (tes::checkFeature(tes::TFeatureCompression) && haveOption("compress", argc, argv))
  {
    serverFlags = tes::SF_Compress | tes::SF_Collate;
  }
#endif // TES_ENABLE
  TES_SETTINGS(settings, serverFlags);
  // Initialise server info.
  TES_SERVER_INFO(info, tes::XYZ);
  // Create the server. Use tesServer declared globally above.
  TES_SERVER_CREATE(tesServer, settings, &info);

  // Start the server and wait for the connection monitor to start.
  TES_SERVER_START(*tesServer, tes::ConnectionMonitor::Asynchronous);
  TES_SERVER_START_WAIT(*tesServer, 1000);

#ifdef TES_ENABLE
  std::cout << "Starting with " << tesServer->connectionCount() << " connection(s)." << std::endl;
#endif // TES_ENABLE

  // Start building the sphere.
  std::cout << "Initialise sphere for " << iterations << " iterations." << std::endl;
  sphereInitialise(vertices, indices, &sphereMap);
  const tes::Vector3f textPos(0.05f, 0.05f, 0);
  TES_TEXT2D_SCREEN(*tesServer, TES_COLOUR(LimeGreen), "Initial", textPos);
  TES_SERVER_UPDATE(*tesServer, 0.0f);
#ifdef TES_ENABLE
  int shapeCount = 1; // Consider initial shape.
#endif // TES_ENABLE
  for (unsigned i = 0; i < iterations; ++i)
  {
    std::stringstream label;
    label << "Division " << i + 1;
    std::cout << label.str() << std::endl;
    subdivideUnitSphere(vertices, indices, sphereMap);
#ifdef TES_ENABLE
    for (int i = 0; i < shapeCount; ++i)
    {
      TES_TRIANGLES_END(*tesServer, SPHERE_ID + i);
    }
    // Send the updated sphere. We must unroll into sets of triangles of less than 65K vertices.
    shapeCount = tesUnrollDisplay(vertices, indices);
    if (i)
    {
      TES_TEXT2D_END(*tesServer, TEXT_ID);
    }
#endif // TES_ENABLE
    TES_TEXT2D_SCREEN(*tesServer, TES_COLOUR(LimeGreen), label.str().c_str(), TEXT_ID, textPos);
    // Update after every iteration.
    TES_SERVER_UPDATE(*tesServer, 0.0f);
  }

  std::cout << "Done" << std::endl;

  // Stop and close the server. Point is left non null.
  TES_SERVER_STOP(*tesServer);

  return 0;
}
