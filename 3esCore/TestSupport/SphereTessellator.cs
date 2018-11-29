using System;
using System.Collections.Generic;
using Tes.Maths;

namespace Tes.TestSupport
{
  public static class SphereTessellator
  {
    /// Add a vertex to @p points, reusing an existing vertex is a matching one is found.
    ///
    /// This first searches for a matching vertex in @p point and returns its index if found.
    /// Otherwise a new vertex is added.
    ///
    /// @param vertex The vertex to add.
    /// @param vertices The vertex data to add to.
    /// @return The index which can be used to refer to the target vertex.
    static int InsertVertex(Vector3 vertex, List<Vector3> vertices, Dictionary<Vector3, int> vertexMap)
    {
      int idx;
      if (vertexMap.TryGetValue(vertex, out idx))
      {
        return idx;
      }

      // Add new vertex.
      idx = vertices.Count;
      vertices.Add(vertex);
      vertexMap.Add(vertex, idx);
      return idx;
    }


    static void SphereInitialise(List<Vector3> vertices, List<int> indices, Dictionary<Vector3, int> vertexMap)
    {
      // We start with two hexagonal rings to approximate the sphere.
      // All subdivision occurs on a unit radius sphere, at the origin. We translate and
      // scale the vertices at the end.
      vertices.Clear();
      indices.Clear();

      float ringControlAngle = 25.0f / 180.0f * (float)Math.PI;
      float ringHeight = (float)Math.Sin(ringControlAngle);
      float ringRadius = (float)Math.Cos(ringControlAngle);
      float hexAngle = 2.0f * (float)Math.PI / 6.0f;
      float ring2OffsetAngle = 0.5f * hexAngle;
      Vector3[] initialVertices = new Vector3[]
      {
        new Vector3(0, 0, 1),

        // Upper hexagon.
        new Vector3(ringRadius, 0, ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(hexAngle), ringRadius * (float)Math.Sin(hexAngle), ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(2 * hexAngle), ringRadius * (float)Math.Sin(2 * hexAngle), ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(3 * hexAngle), ringRadius * (float)Math.Sin(3 * hexAngle), ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(4 * hexAngle), ringRadius * (float)Math.Sin(4 * hexAngle), ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(5 * hexAngle), ringRadius * (float)Math.Sin(5 * hexAngle), ringHeight),

        // Lower hexagon.
        new Vector3(ringRadius * (float)Math.Cos(ring2OffsetAngle), ringRadius * (float)Math.Sin(ring2OffsetAngle), -ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(ring2OffsetAngle + hexAngle), ringRadius * (float)Math.Sin(ring2OffsetAngle + hexAngle), -ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(ring2OffsetAngle + 2 * hexAngle), ringRadius * (float)Math.Sin(ring2OffsetAngle + 2 * hexAngle), -ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(ring2OffsetAngle + 3 * hexAngle), ringRadius * (float)Math.Sin(ring2OffsetAngle + 3 * hexAngle), -ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(ring2OffsetAngle + 4 * hexAngle), ringRadius * (float)Math.Sin(ring2OffsetAngle + 4 * hexAngle), -ringHeight),
        new Vector3(ringRadius * (float)Math.Cos(ring2OffsetAngle + 5 * hexAngle), ringRadius * (float)Math.Sin(ring2OffsetAngle + 5 * hexAngle), -ringHeight),

        new Vector3(0, 0, -1),
      };

      int[] initialIndices =
      {
        0, 1, 2,    0, 2, 3,    0, 3, 4,    0, 4, 5,    0, 5, 6,    0, 6, 1,
        1, 7, 2,    2, 8, 3,    3, 9, 4,    4, 10, 5,   5, 11, 6,   6, 12, 1,
        7, 8, 2,    8, 9, 3,    9, 10, 4,   10, 11, 5,  11, 12, 6,  12, 7, 1,
        7, 13, 8,   8, 13, 9,   9, 13, 10,  10, 13, 11, 11, 13, 12, 12, 13, 7
      };

      for (int i = 0; i < initialVertices.Length; ++i)
      {
        int idx = i;
        vertices.Add(initialVertices[i]);
        if (vertexMap != null)
        {
          vertexMap.Add(vertices[i], i);
        }
      }

      for (int i = 0; i < initialIndices.Length; i += 3)
      {
        indices.Add(initialIndices[i + 0]);
        indices.Add(initialIndices[i + 1]);
        indices.Add(initialIndices[i + 2]);
      }
    }


    public static void SubdivideUnitSphere(List<Vector3> vertices, List<int> indices, Dictionary<Vector3, int> vertexMap)
    {
      int triangleCount = indices.Count / 3;
      int[] triangle = new int[3];
      int[] abc = new int[3];
      int[] def = new int[3];
      Vector3[] verts = new Vector3[3];
      Vector3[] newVertices = new Vector3[3];

      for (int i = 0; i < triangleCount; ++i)
      {
        triangle[0] = abc[0] = indices[i * 3 + 0];
        triangle[1] = abc[1] = indices[i * 3 + 1];
        triangle[2] = abc[2] = indices[i * 3 + 2];

        // Fetch the vertices.
        verts[0] = vertices[triangle[0]];
        verts[1] = vertices[triangle[1]];
        verts[2] = vertices[triangle[2]];

        // Calculate the new vertex at the centre of the existing triangle.
        newVertices[0] = (0.5f * (verts[0] + verts[1])).Normalised;
        newVertices[1] = (0.5f * (verts[1] + verts[2])).Normalised;
        newVertices[2] = (0.5f * (verts[2] + verts[0])).Normalised;

        // Create new triangles.
        // Given triangle ABC, and adding vertices DEF such that:
        //  D = AB/2  E = BC/2  F = CA/2
        // We have four new triangles:
        //  ADF, BED, CFE, DEF
        // ABC are in order in 'abc', while DEF will be in 'def'.
        // FIXME: find existing point to use.
        def[0] = InsertVertex(newVertices[0], vertices, vertexMap);
        def[1] = InsertVertex(newVertices[1], vertices, vertexMap);
        def[2] = InsertVertex(newVertices[2], vertices, vertexMap);

        // Replace the original triangle ABC with DEF
        indices[i * 3 + 0] = def[0];
        indices[i * 3 + 1] = def[1];
        indices[i * 3 + 2] = def[2];

        // Add triangles ADF, BED, CFE
        indices.Add(abc[0]);
        indices.Add(def[2]);

        indices.Add(abc[1]);
        indices.Add(def[1]);
        indices.Add(def[0]);

        indices.Add(abc[2]);
        indices.Add(def[2]);
        indices.Add(def[1]);
      }
    }


    public static void SphereSubdivision(List<Vector3> vertices, List<int> indices, float radius, Vector3 origin, int depth)
    {
      Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
      SphereInitialise(vertices, indices, vertexMap);

      // We also limit the maximum number of iterations.
      for (int i = 0; i < depth; ++i)
      {
        // Subdivide polygons.
        SubdivideUnitSphere(vertices, indices, vertexMap);
      }

      // Move and scale the points.
      for (int i = 0; i < vertices.Count; ++i)
      {
        vertices[i] = vertices[i] * radius + origin;
      }
    }
  }
}
