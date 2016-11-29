using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// A utility class for tessellating a sphere.
  /// </summary>
  /// <remarks>
  /// The class supports three kinds of sphere:
  /// <list type="bullet">
  /// <item>Subdivision</item>
  /// <item>Latitude/longitude</item>
  /// <item>Axis circles (wireframe)</item>
  /// </list>
  /// 
  /// The subdivision sphere begins with an icosahedron approximating the sphere. Triangles
  /// on the shape are recursively subdivided by adding vertices at the midpoint of each
  /// existing triangle and projected onto the surface of the sphere. Using these new vertices
  /// each existing triangle is converted into four new triangles.
  /// 
  /// The latitude/longitude sphere creates rings of latitude around the sphere and lines of
  /// longitude from pole to pole. The intersections create quadrilaterals (triangles at the
  /// poles), which are triangulated. This technique supports generating a hemi-sphere.
  /// 
  /// The axis circles technique is used to create a wireframe representation of a sphere.
  /// It is simply three circles around the sphere origin, each circle lying on one of the
  /// axis planes; XY, XZ and YZ.
  /// </remarks>
  public static class Sphere
  {
    /// <summary>
    /// Limit to the number of iterations in the subdivision sphere technique.
    /// This avoids excessive computation and triangulation. Generally 2 iterations
    /// are sufficient.
    /// </summary>
    public const int MaxIterations = 8;
    
    /// <summary>
    /// Create a solid sphere mesh representation.
    /// </summary>
    /// <param name="radius">The radius of the sphere to generation.</param>
    /// <param name="iterations">Number of subdivision iterations to make.</param>
    /// <returns>The sphere mesh, or null on failure.</returns>
    /// <remarks>
    /// Default to using two iterations of the subdivision technique.
    /// </remarks>
    public static Mesh Solid(float radius = 1, int iterations = 2)
    {
      List<Vector3> vertices = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();
      if (!SubdivisionSphere(vertices, normals, indices, Vector3.zero, radius, iterations))
      {
        return null;
      }
      Mesh mesh = new Mesh();
      mesh.subMeshCount = 1;
      mesh.vertices = vertices.ToArray();
      mesh.normals = normals.ToArray();
      mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
      return mesh;
    }

    /// <summary>
    /// Creates a wireframe sphere representation.
    /// </summary>
    /// <param name="ringVertexCount">The number of vertices in each sphere circle.</param>
    /// <returns>The sphere mesh, or null on failure.</returns>
    public static Mesh Wireframe(int ringVertexCount = 36)
    {
      List<Vector3> vertices = new List<Vector3>();
      List<int> indices = new List<int>();
      if (!Wireframe(vertices, indices, ringVertexCount))
      {
        return null;
      }
      Mesh mesh = new Mesh();
      mesh.subMeshCount = 1;
      mesh.vertices = vertices.ToArray();
      mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
      return mesh;
    }

    /// <summary>
    /// Builds a sphere using a triangle subdivision technique.
    /// </summary>
    /// <param name="vertices">Populated with the sphere vertices. Must be non-null and empty.</param>
    /// <param name="normals">Populated with the sphere vertices. May be null to skip normal calculation, but must be empty.</param>
    /// <param name="indices">Populated with the sphere line indices (pairs). Must be non-null and empty.</param>
    /// <param name="centre">Controls the centre of the sphere.</param>
    /// <param name="radius">Controls the radius of the sphere.</param>
    /// <param name="iterations">The number of sub-division iterations to make.</param>
    /// <remarks>
    /// The initial sphere is made up of 24 triangles of approximately equal size.
    /// Each iteration divides the existing triangles in to four by adding a vertex
    /// in the centre of each existing triangle edge, moving it to the sphere surface and
    /// tessellating.
    /// </remarks>
    public static bool SubdivisionSphere(List<Vector3> vertices, List<Vector3> normals, List<int> indices,
                                         Vector3 centre, float radius, int iterations)
    {
      if (vertices == null || vertices.Count != 0 ||
          normals != null && normals.Count != 0 ||
          indices == null || indices.Count != 0 ||
          radius <= 0)
      {
        return false;
      }

      // We'll use triangle division to attain the target resolution.
      // We start with two hexagonal rings to approximate the sphere.
      // All subdivision occurs on a unit radius sphere, at the origin. We translate and
      // scale the vertices at the end.
      const float ringControlAngle = 25.0f / 180.0f * Mathf.PI;
      float ringHeight = Mathf.Sin(ringControlAngle);
      float ringRadius = Mathf.Cos(ringControlAngle);
      const float hexAngle = 2.0f * Mathf.PI / 6.0f;
      float ring2OffsetAngle = 0.5f * hexAngle;

      Vector3[] initialVertices = new Vector3[]
      {
        new Vector3(0, 0, 1),

        // Upper hexagon.
        new Vector3(ringRadius, 0, ringHeight),
        new Vector3(ringRadius * Mathf.Cos(hexAngle), ringRadius * Mathf.Sin(hexAngle), ringHeight),
        new Vector3(ringRadius * Mathf.Cos(2 * hexAngle), ringRadius * Mathf.Sin(2 * hexAngle), ringHeight),
        new Vector3(ringRadius * Mathf.Cos(3 * hexAngle), ringRadius * Mathf.Sin(3 * hexAngle), ringHeight),
        new Vector3(ringRadius * Mathf.Cos(4 * hexAngle), ringRadius * Mathf.Sin(4 * hexAngle), ringHeight),
        new Vector3(ringRadius * Mathf.Cos(5 * hexAngle), ringRadius * Mathf.Sin(5 * hexAngle), ringHeight),

        // Lower hexagon.
        new Vector3(ringRadius * Mathf.Cos(ring2OffsetAngle), ringRadius * Mathf.Sin(ring2OffsetAngle), -ringHeight),
        new Vector3(ringRadius * Mathf.Cos(ring2OffsetAngle + hexAngle), ringRadius * Mathf.Sin(ring2OffsetAngle + hexAngle), -ringHeight),
        new Vector3(ringRadius * Mathf.Cos(ring2OffsetAngle + 2 * hexAngle), ringRadius * Mathf.Sin(ring2OffsetAngle + 2 * hexAngle), -ringHeight),
        new Vector3(ringRadius * Mathf.Cos(ring2OffsetAngle + 3 * hexAngle), ringRadius * Mathf.Sin(ring2OffsetAngle + 3 * hexAngle), -ringHeight),
        new Vector3(ringRadius * Mathf.Cos(ring2OffsetAngle + 4 * hexAngle), ringRadius * Mathf.Sin(ring2OffsetAngle + 4 * hexAngle), -ringHeight),
        new Vector3(ringRadius * Mathf.Cos(ring2OffsetAngle + 5 * hexAngle), ringRadius * Mathf.Sin(ring2OffsetAngle + 5 * hexAngle), -ringHeight),

        new Vector3(0, 0, -1),
      };
      
      int[] initialIndices = new int[]
      {
        0, 1, 2,    0, 2, 3,    0, 3, 4,    0, 4, 5,    0, 5, 6,    0, 6, 1,
        1, 7, 2,    2, 8, 3,    3, 9, 4,    4, 10, 5,   5, 11, 6,   6, 12, 1,
        7, 8, 2,    8, 9, 3,    9, 10, 4,   10, 11, 5,  11, 12, 6,  12, 7, 1,
        7, 13, 8,   8, 13, 9,   9, 13, 10,  10, 13, 11, 11, 13, 12, 12, 13, 7
      };
      
      Dictionary<SphereVector3Hash, int> sphereVertexMap = new Dictionary<SphereVector3Hash, int>();

      foreach (Vector3 vert in initialVertices)
      {
        sphereVertexMap.Add(new SphereVector3Hash(vert), vertices.Count);
        vertices.Add(vert);
      }
      
      indices.AddRange(initialIndices);
      
      // Limit the maximum number of iterations to be reasonable.
      // This also keeps the number of vertices below the 65K limit in Unity.
      iterations = Math.Max(0, Math.Min(iterations, MaxIterations));
      for (int i = 0; i < iterations; ++i)
      {
        SubdivideUnitSphere(vertices, indices, sphereVertexMap);
      }

      // Set the normals before we move and scale the mesh.
      // The 'vertices' are currently all unity length and zero centred, thus
      // can be used to initialise the sphere normals.
      if (normals != null)
      {
        normals.AddRange(vertices);
      }

      // Move and scale points.
      if (centre != Vector3.zero || radius != 1.0f)
      {
        for (int i = 0; i < vertices.Count; ++i)
        {
          vertices[i] = centre + vertices[i] * radius;
        }
      }

      return true;
    }

    /// <summary>
    /// Subdivides the triangles in a unity sphere into four triangles each.
    /// </summary>
    /// <param name="vertices">The sphere vertices. Modified to accomodate the new vertices.</param>
    /// <param name="indices">The sphere triangle indices. Modified to accomodate the new triangles.</param>
    /// <param name="sphereVertexMap">Used to resolve existing vertex positions to their indices.</param>
    /// <remarks>
    /// Assumes the input defines a unit sphere made of near equilateral triangles.
    /// </remarks>
    private static void SubdivideUnitSphere(List<Vector3> vertices, List<int> indices, Dictionary<SphereVector3Hash, int> sphereVertexMap)
    {
      int triangleCount = indices.Count / 3;
      int[] abc = new int[3];
      int[] def = new int[3];
      Vector3[] verts = new Vector3[3];
      Vector3[] newVerts = new Vector3[3];
      
      for (int i = 0; i < triangleCount; ++i)
      {
        abc[0] = indices[i * 3 + 0];
        abc[1] = indices[i * 3 + 1];
        abc[2] = indices[i * 3 + 2];

        // Fetch the vertices.
        verts[0] = vertices[abc[0]];
        verts[1] = vertices[abc[1]];
        verts[2] = vertices[abc[2]];

        // Create new triangles.
        // Given triangle ABC, and adding vertices DEF such that:
        //  D = AB/2  E = BC/2  F = CA/2
        newVerts[0] = (0.5f * (verts[0] + verts[1])).normalized;
        newVerts[1] = (0.5f * (verts[1] + verts[2])).normalized;
        newVerts[2] = (0.5f * (verts[2] + verts[0])).normalized;

        // We have four new triangles:
        //  ADF, BED, CFE, DEF
        // ABC are is order in the local variable 'abc', while DEF is in 'def'.
        def[0] = InsertVertex(newVerts[0], vertices, sphereVertexMap);
        def[1] = InsertVertex(newVerts[1], vertices, sphereVertexMap);
        def[2] = InsertVertex(newVerts[2], vertices, sphereVertexMap);

        // Replace the original triangle ABC with DEF
        indices[i * 3 + 0] = def[0];
        indices[i * 3 + 1] = def[1];
        indices[i * 3 + 2] = def[2];
        
        // Add triangles ADF, BED, CFE
        indices.Add(abc[0]);
        indices.Add(def[0]);
        indices.Add(def[2]);

        indices.Add(abc[1]);
        indices.Add(def[1]);
        indices.Add(def[0]);

        indices.Add(abc[2]);
        indices.Add(def[2]);
        indices.Add(def[1]);
      }
    }

    /// <summary>
    /// Add a vertex to @p points, reusing an existing vertex is a matching one is found.
    /// </summary>
    /// <param name="vertex">The vertex to add.</param>
    /// <param name="vertices">Vertex array to add the vertex to.</param>
    /// <param name="sphereVertexMap">Hash map for sphere vertices in <paramref name="vertices"/>. Used to match
    /// <paramref name="vertex"/> against existing vertices.</param>
    /// <returns>The index which can be used to refer to the target vertex.</returns>
    /// <remarks>
    /// This first searches for a matching vertex in <paramref name="vertices"/> and returns its index if found.
    /// Otherwise a new vertex is added.
    /// </remarks>
    private static int InsertVertex(Vector3 vertex, List<Vector3> vertices, Dictionary<SphereVector3Hash, int> sphereVertexMap)
    {
      SphereVector3Hash hash = new SphereVector3Hash(vertex);
      int index;
      if (sphereVertexMap.TryGetValue(hash, out index))
      {
        // Found vertex match.
        return index;
      }
      
      // Add new vertex.
      index = vertices.Count;
      vertices.Add(vertex);
      sphereVertexMap.Add(hash, index);
      return index;
    }

    /// <summary>
    /// Tessellates a sphere by the latitude/longitude technique.
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="normals"></param>
    /// <param name="indices"></param>
    /// <param name="centre"></param>
    /// <param name="radius"></param>
    /// <param name="hemisphereRingCount"></param>
    /// <param name="segments"></param>
    /// <param name="axis"></param>
    /// <param name="hemisphereOnly"></param>
    /// <returns></returns>
    /// <remarks>
    /// This technique supports adding vertices to an initial vertex set. That is
    /// the <paramref name="vertices"/>, <paramref name="normals"/> and <paramref name="indices"/>
    /// need not be empty to begin with.
    /// </remarks>
    public static bool LatLong(List<Vector3> vertices, List<Vector3> normals, List<int> indices,
                               Vector3 centre, float radius,
                               int hemisphereRingCount, int segments, Vector3 axis,
                               bool hemisphereOnly)
    {
      if (vertices == null || indices == null ||
          hemisphereRingCount < 1 || segments < 3 ||
          Mathf.Abs(axis.sqrMagnitude - 1.0f) > 1e-2f)
      {
        return false;
      }

      Vector3[] radials = new Vector3[2];
      Vector3 v;
      float segmentAngle = 2.0f * Mathf.PI / segments;
      float ringStepAngle = 0.5f * Mathf.PI / hemisphereRingCount;
      int initialVertexCount = vertices.Count;

      if (Vector3.Dot(axis, new Vector3(1, 0, 0)) < 1e-3f)
      {
        radials[0] = new Vector3(1, 0, 0);
      }
      else
      {
        radials[0] = new Vector3(0, 1, 0);
      }

      radials[1] = Vector3.Cross(axis, radials[0]).normalized;
      radials[0] = Vector3.Cross(radials[1], axis).normalized;

      // Two triangles (six indices) per segment per ring.
      // Last ring has only one trinagle per segment, for which we make the adjustment.
      // Will be doubled for full sphere.
      int indexCap = hemisphereRingCount * segments * 2 * 3 - 3 * segments;
      if (hemisphereOnly)
      {
        vertices.Capacity = (hemisphereRingCount * segments + 1);
      }
      else
      { 
        // Double the vertices excluding the shared, equatorial vertices.
        vertices.Capacity = vertices.Count + 2 * (hemisphereRingCount * segments + 1) - segments;
        indexCap *= 2;
      }
      indices.Capacity = indices.Count + indexCap;

      if (normals != null)
      {
        normals.Capacity = vertices.Capacity;
      }

      // First build a unit sphere.
      // Create vertices for the rings.
      for (int r = 0; r < hemisphereRingCount; ++r)
      {
        float ringHeight = Mathf.Sin(r * ringStepAngle);
        float ringRadius = Mathf.Sqrt(1 - ringHeight * ringHeight);
        for (int i = 0; i < segments; ++i)
        {
          float angle = i * segmentAngle;
          v = ringRadius * Mathf.Cos(angle) * radials[0] + ringRadius * Mathf.Sin(angle) * radials[1];
          v += ringHeight * axis;
          vertices.Add(v);
        }
      }

      // Add the polar vertex.
      vertices.Add(axis);

      // We have vertices for a hemi-sphere. Mirror if we are building a full sphere.
      if (!hemisphereOnly)
      {
        int mirrorStart = segments; // Skip the shared, equatorial ring.
        int mirrorCount = vertices.Count - 1;
        for (int i = mirrorStart; i < mirrorCount; ++i)
        {
          v = vertices[i];
          v -= 2.0f * Vector3.Dot(v, axis) * axis;
          vertices.Add(v);
        }

        // Add the polar vertex.
        vertices.Add(-axis);
      }

      // We have a unit sphere. These can be used as normals as is.
      if (normals != null)
      {
        normals.AddRange(vertices);
      }

      // Move and offset the vertices.
      for (int i = 0; i < vertices.Count; ++i)
      {
        vertices[i] = centre + radius * vertices[i];
      }

      // Finally build the indices for the triangles.
      // Tessellate each ring up the hemispheres.
      int ringStartIndex, previousRingStartIndex;
      previousRingStartIndex = initialVertexCount;
      for (int r = 1; r < hemisphereRingCount; ++r)
      {
        ringStartIndex = r * segments + initialVertexCount;

        for (int i = 0; i < segments; ++i)
        {
          indices.Add(previousRingStartIndex + i);
          indices.Add(previousRingStartIndex + (i + 1) % segments);
          indices.Add(ringStartIndex + (i + 1) % segments);

          indices.Add(previousRingStartIndex + i);
          indices.Add(ringStartIndex + (i + 1) % segments);
          indices.Add(ringStartIndex + i);
        }

        previousRingStartIndex = ringStartIndex;
      }

      // Connect the final ring to the polar vertex.
      ringStartIndex = (hemisphereRingCount - 1) * segments + initialVertexCount;
      for (int i = 0; i < segments; ++i)
      {
        indices.Add(ringStartIndex + i);
        indices.Add(ringStartIndex + (i + 1) % segments);
        indices.Add(ringStartIndex + segments); // Polar vertex
      }

      // Build lower hemi-sphere as required.
      if (!hemisphereOnly)
      {
        int hemisphereOffset = hemisphereRingCount * segments + 1 + initialVertexCount;
        // Stil use zero as the first previous ring. This is the shared equator.
        previousRingStartIndex = initialVertexCount;
        for (int r = 1; r < hemisphereRingCount; ++r)
        {
          // Take one off r for the shared equator.
          ringStartIndex = (r - 1) * segments + hemisphereOffset;

          for (int i = 0; i < segments; ++i)
          {
            indices.Add(previousRingStartIndex + i);
            indices.Add(ringStartIndex + (i + 1) % segments);
            indices.Add(previousRingStartIndex + (i + 1) % segments);

            indices.Add(previousRingStartIndex + i);
            indices.Add(ringStartIndex + i);
            indices.Add(ringStartIndex + (i + 1) % segments);
          }

          previousRingStartIndex = ringStartIndex;
        }

        // Connect the final ring to the polar vertex.
        // Take two from hemisphereRingCount for the shared equator.
        if (hemisphereRingCount > 1)
        { 
          ringStartIndex = (hemisphereRingCount - 1 - 1) * segments + hemisphereOffset;
        }
        else
        {
          // Shared equator.
          ringStartIndex = 0;
        }
        for (int i = 0; i < segments; ++i)
        {
          indices.Add(ringStartIndex + (i + 1) % segments);
          indices.Add(ringStartIndex + i);
          indices.Add(ringStartIndex + segments); // Polar vertex
        }
      }

      return true;
    }

    /// <summary>
    /// Performs the wireframe circle generation technique.
    /// </summary>
    /// <param name="vertices">Populated with the sphere vertices. Must be non-null and empty.</param>
    /// <param name="indices">Populated with the sphere line indices (pairs). Must be non-null and empty.</param>
    /// <param name="ringVertexCount">Number of vertices in each circle.</param>
    /// <returns></returns>
    public static bool Wireframe(List<Vector3> vertices, List<int> indices, int ringVertexCount = 36)
    {
      if (vertices == null || vertices.Count != 0 ||
          indices == null || indices.Count != 0 ||
          ringVertexCount <= 3)
      {
        return false;
      }

      vertices.Capacity = ringVertexCount * 3;
      indices.Capacity = 2 * ringVertexCount * 3;

      // Build a circle around the Z axis.
      for (int i = 0; i < ringVertexCount; ++i)
      {
        float angle = i * 2.0f * Mathf.PI / (float)ringVertexCount;
        vertices.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
      }

      // Build a circle around the Y axis.
      for (int i = 0; i < ringVertexCount; ++i)
      {
        float angle = i * 2.0f * Mathf.PI / (float)ringVertexCount;
        vertices.Add(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
      }

      // Build a circle around the X axis.
      for (int i = 0; i < ringVertexCount; ++i)
      {
        float angle = i * 2.0f * Mathf.PI / (float)ringVertexCount;
        vertices.Add(new Vector3(0, Mathf.Cos(angle), Mathf.Sin(angle)));
      }

      // Build indices.
      // Z circle.
      int voffset = 0;
      for (int i = 0; i < ringVertexCount - 1; ++i)
      {
        indices.Add(voffset + i);
        indices.Add(voffset + i + 1);
      }
      // Complete the circle.
      indices.Add(voffset + ringVertexCount - 1);
      indices.Add(voffset);

      // Y circle.
      voffset += ringVertexCount;
      for (int i = 0; i < ringVertexCount - 1; ++i)
      {
        indices.Add(voffset + i);
        indices.Add(voffset + i + 1);
      }
      // Complete the circle.
      indices.Add(voffset + ringVertexCount - 1);
      indices.Add(voffset);

      // Y circle.
      voffset += ringVertexCount;
      for (int i = 0; i < ringVertexCount - 1; ++i)
      {
        indices.Add(voffset + i);
        indices.Add(voffset + i + 1);
      }
      // Complete the circle.
      indices.Add(voffset + ringVertexCount - 1);
      indices.Add(voffset);

      return true;
    }
  }
}
