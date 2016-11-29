using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// A utility for tessellating a cone shape.
  /// </summary>
  public static class Cone
  {
    /// <summary>
    /// Number of facets to the cone.
    /// </summary>
    static readonly int ConeFacets = 8;

    /// <summary>
    /// Default cone length.
    /// </summary>
    static readonly float ConeLength = 1.0f;
    /// <summary>
    /// Default code radius at the base.
    /// </summary>
    static readonly float ConeRadius = 1.0f;

    /// <summary>
    /// Cone vertices. Built as a unit long, unit radius Cone.
    /// </summary>
    static readonly Vector3[] ConeVertices = new Vector3[]
    {
      // Cone point indices. Duplicated for separate normals.
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,

      // Cone wall vertices.
      new Vector3(ConeRadius * Mathf.Sin(0 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(0 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(1 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(1 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(2 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(2 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(3 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(3 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(4 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(4 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(5 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(5 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(6 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(6 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(7 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(7 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),

      // Duplicated vertices for cone base. Allows separate normals.
      new Vector3(ConeRadius * Mathf.Sin(0 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(0 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(1 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(1 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(2 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(2 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(3 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(3 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(4 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(4 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(5 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(5 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(6 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(6 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
      new Vector3(ConeRadius * Mathf.Sin(7 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeRadius * Mathf.Cos(7 * 2 * ConeRadius * Mathf.PI / ConeFacets), ConeLength),
    };

    /// <summary>
    /// Cone triangle indices into <see cref="ConeVertices"/>.
    /// </summary>
    public static readonly int[] ConeIndices = new int[]
    {
      // Cone walls
      0, 8, 9,
      1, 9, 10,
      2, 10, 11,
      3, 11, 12,
      4, 12, 13, 
      5, 13, 14,
      6, 14, 15,
      7, 15, 8,

      // Cone base.
      16, 18, 17,
      16, 19, 18,
      16, 20, 19,
      16, 21, 20,
      16, 22, 21,
      16, 23, 22
    };
    
    private static Vector3[] _normals;
    /// <summary>
    /// Cone normals array.
    /// </summary>
    public static Vector3[] Normals { get { return _normals; } }

    /// <summary>
    /// Cone line indices into <see cref="ConeVertices"/>.
    /// </summary>
    static readonly int[] ConeLineIndices = new int[]
    {
      0, 8,
      1, 9,
      2, 10,
      3, 11,
      4, 12,
      5, 13,
      6, 14,
      7, 15,

      // Ring around cone.
      16, 17,
      17, 18,
      18, 19,
      19, 20,
      20, 21,
      21, 22,
      22, 23,
      23, 16
    };
    
    /// <summary>
    /// Static constructor to calculate normals.
    /// </summary>
    static Cone()
    {
      _normals = new Vector3[ConeVertices.Length];
      for (int i = 0; i < ConeFacets; ++i)
      {
        _normals[i] = _normals[i + ConeFacets] = ConeVertices[i + ConeFacets].normalized;
      }
      
      for (int i = ConeFacets * 2; i < ConeVertices.Length; ++i)
      {
        _normals[i] = new Vector3(0, 0, Mathf.Sign(ConeVertices[i].z));
      }
    }

    /// <summary>
    /// Create a solid cone mesh.
    /// </summary>
    /// <returns>The cone mesh.</returns>
    public static Mesh Solid()
    {
      List<Vector3> vertices = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();

      if (!SolidByFacets(vertices, normals, indices, Vector3.zero, new Vector3(0, 0, 1), 1.0f, 45.0f * Mathf.Deg2Rad, 12))
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
    /// Create a line representation of a cone mesh.
    /// </summary>
    /// <returns>The cone mesh.</returns>
    public static Mesh Wireframe()
    {
      Mesh mesh = new Mesh();
      mesh.vertices = ConeVertices;
      mesh.normals = Normals;
      mesh.subMeshCount = 1;
      mesh.SetIndices(ConeLineIndices, MeshTopology.Lines, 0);
      return mesh;
    }

    private const int MinFacets = 8;

    /// <summary>
    /// Tessellate a solid cone by specifying the number of facets.
    /// </summary>
    /// <param name="vertices">Populated with the cone vertices (output).</param>
    /// <param name="normals">Populated with the cone normals (output).</param>
    /// <param name="indices">Populated with the triangle indices (output).</param>
    /// <param name="apex">The cone apex vertex to construct with.</param>
    /// <param name="axis">The cone primary axis (direction) to construct with.</param>
    /// <param name="length">The cone length.</param>
    /// <param name="angle">The cone angle between the wall and the primary <paramref name="axis"/>
    ///     (radians).</param>
    /// <param name="facetCount">The number of facets around the cone. Minimum 3.</param>
    /// <param name="tessellateBase">Tessellate the base (true) or leave it open (false).</param>
    /// <returns>True on success, false if the arguments prevent tessellation.</returns>
    public static bool SolidByFacets(List<Vector3> vertices, List<Vector3> normals, List<int> indices,
                             Vector3 apex, Vector3 axis, float length, float angle,
                             int facetCount, bool tessellateBase = true)
    {
      facetCount = Math.Max(MinFacets, facetCount);

      // Chord length = 2sin(angle/2)
      float baseChordLength = 2.0f * Mathf.Sin(0.5f * (2.0f * Mathf.PI / facetCount));
      return SolidByBaseChord(vertices, normals, indices, apex, axis, length, angle, baseChordLength, tessellateBase);
    }

    /// <summary>
    /// Tessellate a solid cone by specifying the facet chord length around the base circle.
    /// </summary>
    /// <param name="vertices">Populated with the cone vertices (output).</param>
    /// <param name="normals">Populated with the cone normals (output).</param>
    /// <param name="indices">Populated with the triangle indices (output).</param>
    /// <param name="apex">The cone apex vertex to construct with.</param>
    /// <param name="axis">The cone primary axis (direction) to construct with.</param>
    /// <param name="length">The cone length.</param>
    /// <param name="angle">The cone angle between the wall and the primary <paramref name="axis"/>
    ///     (radians).</param>
    /// <param name="baseChordLength">The chord length around the base circle.</param>
    /// <param name="tessellateBase">Tessellate the base (true) or leave it open (false).</param>
    /// <returns>True on success, false if the arguments prevent tessellation.</returns>
    /// <remarks>
    /// This method tessellates to try and keep quadrilaterals (trapezoids) of the same area.
    /// The cone is divided into rings, but the distance between the rings increases
    /// away from the cone base in order to maintain a similar area.
    /// The last division is set to consume the remainder. The ASCII art below illustrates the
    /// divisions.
    /// 
    /// <code>
    /// Cone setup:
    /// h[3] = 0    /\
    ///            /  \
    ///           /    \
    ///          /      \
    /// h[2]    /--------\
    ///        /          \
    /// h[1]  /------------\
    /// h[0] /--------------\
    ///
    /// h[n] defines the length of each ring from the apex, with:
    ///  h[0] &lt; h[1] &lt; ... &lt; h[n-1] &lt; h[n]
    /// </code>
    /// 
    /// There are a known number of facets around the ring, where <c>c[n]</c> is
    /// the chord length of a facet (defined by the ring of radius <c>r[n]</c>).
    /// Set <c>c[0] = baseChordLength</c>, which generally defines the tessellation resolution.
    /// </remarks>
    public static bool SolidByBaseChord(List<Vector3> vertices, List<Vector3> normals, List<int> indices,
                             Vector3 apex, Vector3 axis, float length, float angle,
                             float baseChordLength = 0, bool tessellateBase = true)
    {
      float quadArea = baseChordLength * baseChordLength;
      float baseRadius = length * Mathf.Tan(angle);
      float segmentAngle = 2.0f * Mathf.Asin(baseChordLength / (2 * baseRadius));
      int facets = Mathf.FloorToInt(2.0f * Mathf.PI / segmentAngle + 0.5f);
      float facetAngle = 2.0f * Mathf.PI / facets;
      if (facets < MinFacets)
      {
        facets = MinFacets;
        facetAngle = 2.0f * Mathf.PI / facets;
        baseChordLength = 2 * baseRadius * Mathf.Sin(0.5f * facetAngle);
        quadArea = baseChordLength * baseChordLength;
      }
      segmentAngle = 2.0f * Mathf.PI / facets;

      if (baseChordLength <= 0)
      {
        return false;
      }

      // Building on the XML comment above:
      // Constants or initial values:
      // A: quadArea (c[0] * c[0])
      // theta: cone angle
      // phi: segmentAngle

      // At ring n:
      // r[n] : ring radius
      // c[n] : facet chord length (trapezoid base length).
      // d[n] : distance to the next ring (n+1).
      // h[n] : ring length or the distance from the cone apex to the ring.
      // A : quadArea
      //
      // A[i] = A[j] = quadArea                 Constant
      // A[n] = d[n] * (c[n-1] + c[n]) / 2      Trapezoid area.

      // c[n] = 2r[n] sin(phi / 2)              (1)
      // d[n] = 2A / (c[n-1] + c[n])            (2)
      // h[n-1] = h[n] + d[n]                   (3)
      // h[n] = r[n] / tan(theta)               (4)

      // Thus from (4):
      // r[n] = h[n] tan(theta)                 (4a)
      // Substitute (3) in (4a):
      // r[n] = (h[n-1] - d[n]) tan(theta)      (5)
      //
      // Substitute (5) in (1):
      // c[n] = 2(h[n-1] - d[n]) tan(theta) sin(phi/2)
      // Let T = 2 * tan(theta) sin(phi/2)
      // c[n] = T(h[n-1] - d[n])                (6)
      //
      // Substitute (2) in (6):
      // c[n] = T(h[n-1] - 2A / (c[n-1] + c[n]))
      // ...
      // c[n] * c[n] + (c[n-1] - Th[n-1])c[n] + T(2A - h[n-1]c[n-1]) = 0
      //                                        (7)
      //
      // However, we can show that the coefficient of c[n] is zero:
      // From (4):
      // r[n] = h[n] tan(theta)                 (4a)
      // Substitute (4a) in (1):
      // c[n] = 2 h[n] tan(theta) sin(phi / 2)
      //      = h[n] 2 tan(theta) sin(phi / 2)
      //      = Th[n]
      // Therefore:
      // c[n-1] = Th[n-1]                       (8)
      //
      // Substitute (8) in (7):
      // c[n] * c[n] + (c[n-1] - Th[n-1])c[n] + T(2A - h[n-1]c[n-1]) = 0
      // c[n] * c[n] + (c[n-1] - c[n-1])c[n] + T(2A - h[n-1]c[n-1])  = 0
      // c[n] * c[n] + T(2A - h[n-1]c[n-1])                          = 0
      // c[n] * c[n] = -T(2A - h[n-1]c[n-1])
      // c[n] * c[n] = T(h[n-1]c[n-1] - 2A)
      // c[n] = sqrt(T(h[n-1]c[n-1] - 2A))      (9)
      //
      // We continue so long as the root term in (9) is positive. The sign changes
      // once we pass the cone apex. We also add a restriction h[n] >= baseChordLength,
      // stopping once that fails. I suspect that this only occurs once the root term
      // in (9) changes sign.

      // We don't know how many vertices we have head of time.

      Vector3[] radials = new Vector3[2];
      float nearAlignedDot = Mathf.Cos(85.0f / 180.0f * Mathf.PI);
      if (Vector3.Dot(axis, new Vector3(0, 1, 0)) < nearAlignedDot)
      {
        radials[0] = Vector3.Cross(new Vector3(0, 1, 0), axis).normalized;
      }
      else
      {
        radials[0] = Vector3.Cross(new Vector3(1, 0, 0), axis).normalized;
      }
      radials[1] = Vector3.Cross(axis, radials[0]).normalized;

      // Add the base ring.
      // We build vertices for each ring first, then tessellate between the rings.
      Vector3 ringCentre, radial, vertex, firstVertex, normal;

      // Add each following ring.
      int rings = 0;
      float hn = length;  // h[n]
      float cn = baseChordLength;  // c[n]
      float dn = 0;   // d[n]
      float radius = baseChordLength; // r[n-1]
      float hp, cp;   // h[n-1] and c[n-1] ('p' for previous).
                      // let T = 2 * tan(theta) sin(phi/2)
      float T = 2 * Mathf.Tan(angle) * Mathf.Sin(0.5f * segmentAngle);

      while (hn > baseChordLength)
      {
        hp = hn;
        cp = cn;

        // Add vertices for the ring at hp before.
        {
          //// Bitmap V coordinate:
          //// 1 at base, zero at apex.
          //float uf = (hn / length);
          radius = hp * Mathf.Tan(angle);
          //float vrange = radius / baseRadius;
          //float vstart = 0.5f - 0.5f * vrange;
          //float rlen;
          ringCentre = apex + hp * axis;
          firstVertex = ringCentre + radius * radials[0];
          vertices.Add(firstVertex);

          for (int f = 1; f < facets; ++f)
          {
            //// Bitmap V coordinate. Range varies such that at the base it is [0, 1] and it is 0.5 a the apex.
            //float vf = vstart + vrange * ((float)f / (float)facets);
            float currentFacetAngle = f * segmentAngle;
            radial = radius * (Mathf.Cos(currentFacetAngle) * radials[0] + Mathf.Sin(currentFacetAngle) * radials[1]);
            //rlen = radial.magnitude;
            vertex = ringCentre + radial;
            vertices.Add(vertex);
          }
        }

        // c[n] = sqrt(T(h[n-1]c[n-1] - 2A))      (9)
        float rootTerm = T * (hp * cp - 2 * quadArea);
        if (rootTerm > 0)
        {
          cn = Mathf.Sqrt(rootTerm);

          // d[n] = 2A / (c[n-1] + c[n])            (2)
          dn = 2 * quadArea / (cp + cn);

          // Setup next iteration.
          hn = hp - dn;
        }
        else
        {
          hn = dn = 0;
        }
        ++rings;
      }

      // Add the apex vertex as the last vertex.
      // Add apex vertices. If not generating normals, we just add one.
      // If generating normals, we add one for each facet to support individual normals.
      int apexStart = vertices.Count;
      if (normals == null)
      {
        vertices.Add(apex);
      }
      else
      {
        for (int i = 0; i < facets; ++i)
        {
          vertices.Add(apex);
        }

        // Add normals for each of the existing vertices.
        // Each facet vertex has the same normal as the other vertices for the same facet.
        Vector3 toApex, v;
        // rings + 1 to cover the apex vertices
        for (int r = 0; r < rings + 1; ++r)
        {
          for (int f = 0; f < facets; ++f)
          {
            normal = vertices[f]; // Always start with base vertex.
            // Find the vector to the apex.
            toApex = apex - normal;
            // Remove height component from the future normal
            normal -= Vector3.Dot(normal, axis) * axis;
            //normal.Normalize();

            // Cross and normalise to get the actual normal.
            v = Vector3.Cross(toApex, normal);
            normal = Vector3.Cross(v, toApex).normalized;

            normals.Add(normal);
          }
        }
      }

      // Now triangulate between the rings.
      for (int r = 1; r < rings; ++r)
      {
        int ringStartIndex = r * facets;
        int prevRingStartIndex = ringStartIndex - facets;
        for (int f = 0; f < facets; ++f)
        {
          indices.Add(prevRingStartIndex + f);
          indices.Add(ringStartIndex + (f + 1) % facets);
          indices.Add(prevRingStartIndex + (f + 1) % facets);

          indices.Add(prevRingStartIndex + f);
          indices.Add(ringStartIndex + f);
          indices.Add(ringStartIndex + (f + 1) % facets);
        }
      }

      // Triangulate with the apex vertex.
      int lastStartIndex = (rings - 1) * facets;
      if (normals == null)
      {
        for (int f = 0; f < facets; ++f)
        {
          indices.Add(apexStart); // Apex vertex
          indices.Add(lastStartIndex + (f + 1) % facets);
          indices.Add(lastStartIndex + f);
        }
      }
      else
      {
        for (int f = 0; f < facets; ++f)
        {
          indices.Add(apexStart + f); // Apex vertex
          indices.Add(lastStartIndex + (f + 1) % facets);
          indices.Add(lastStartIndex + f);
        }
      }

      if (tessellateBase)
      { 
        // Add and tessellate base vertices. We copy or re-use the initial ring
        int firstBaseIndex = 0;
        if (normals != null)
        {
          // Copy initial ring.
          firstBaseIndex = vertices.Count;
          for (int i = 0; i < facets; ++i)
          {
            vertices.Add(vertices[i]);
            normals.Add(axis);
          }
        }

        for (int i = 1; i < facets - 1; ++i)
        {
          indices.Add(firstBaseIndex);
          indices.Add(firstBaseIndex + i);
          indices.Add(firstBaseIndex + i + 1);
        }
      }

      return true;
    }
  }
}
