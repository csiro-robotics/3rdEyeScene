//
// author: Kazys Stepanas
//
#include "3esspheretessellator.h"

#include "3esvector3.h"

#include <unordered_map>

using namespace tes;

namespace
{
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
    if (findResult == vertexMap.end())
    {
      // Add new vertex.
      unsigned idx = unsigned(vertices.size());
      vertices.push_back(vertex);
      vertexMap.insert(std::make_pair(vertex, idx));
      return idx;
    }

    return findResult->second;
  }
}


void tes::sphereInitialise(std::vector<Vector3f> &vertices, std::vector<unsigned> &indices, SphereVertexMap *vertexMap)
{
  // We start with two hexagonal rings to approximate the sphere.
  // All subdivision occurs on a unit radius sphere, at the origin. We translate and
  // scale the vertices at the end.
  vertices.resize(0);
  indices.resize(0);

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
}


void tes::subdivideUnitSphere(std::vector<Vector3f> &vertices, std::vector<unsigned> &indices, SphereVertexMap &vertexMap)
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

    // Replace the original triangle ABC with DEF
    indices[i * 3 + 0] = def[0];
    indices[i * 3 + 1] = def[1];
    indices[i * 3 + 2] = def[2];

    // Add triangles ADF, BED, CFE
    indices.push_back(abc[0]);
    indices.push_back(def[0]);
    indices.push_back(def[2]);

    indices.push_back(abc[1]);
    indices.push_back(def[1]);
    indices.push_back(def[0]);

    indices.push_back(abc[2]);
    indices.push_back(def[2]);
    indices.push_back(def[1]);
  }
}


void tes::sphereSubdivision(std::vector<Vector3f> &vertices, std::vector<unsigned> &indices, float radius, const Vector3f &origin, int depth)
{
  SphereVertexMap vertexMap;
  sphereInitialise(vertices, indices, &vertexMap);

  // We also limit the maximum number of iterations.
  for (int i = 0; i < depth; ++i)
  {
    // Subdivide polygons.
    subdivideUnitSphere(vertices, indices, vertexMap);
  }

  // Move and scale the points.
  for (Vector3f &vert : vertices)
  {
    vert = vert * radius + origin;
  }
}
