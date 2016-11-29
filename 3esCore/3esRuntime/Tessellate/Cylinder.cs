using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// A utility class for tessellating cylinders.
  /// </summary>
  public static class Cylinder
  {
    /// <summary>
    /// Tessellation control flags.
    /// </summary>
    [Flags]
    public enum Flags
    {
      /// <summary>
      /// Zero.
      /// </summary>
      Zero = 0,
      /// <summary>
      /// Leave the end caps open? Cylinder is still one sided.
      /// </summary>
      Open = (1 << 0),
    }

    /// <summary>
    /// Tessellate a unit length, unit radius shape centred on the origin along the axis (0, 0, 1).
    /// </summary>
    /// <returns>The tessellated mesh.</returns>
    public static Mesh Solid()
    {
      List<Vector3> vertices = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();
      if (!Solid(vertices, normals, indices, new Vector3(0, 0, -0.5f), new Vector3(0, 0, 0.5f), 1.0f, 8, 3, Flags.Zero))
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
    /// Create a line based cylinder of unit length, unit radius shape centred on the origin along the axis (0, 0, 1).
    /// </summary>
    /// <returns>The line mesh.</returns>
    public static Mesh Wireframe()
    {
      List<Vector3> vertices = new List<Vector3>();
      List<int> indices = new List<int>();
      if (!Wireframe(vertices, indices, new Vector3(0, 0, -0.5f), new Vector3(0, 0, 0.5f), 1.0f, 32, 8))
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
    /// Calculate the number of cylinder facets and rings required to create a cylinder which
    /// keeps the circular chord length and ring slice length at or under the specified values.
    /// </summary>
    /// <param name="facets">Set to the calculated number of facets. Minimum is 3.</param>
    /// <param name="rings">Set to the calculated number of ring slices. Minimum is 2; top and bottom.</param>
    /// <param name="length">The length of the cylinder.</param>
    /// <param name="radius">The radius of the cylinder.</param>
    /// <param name="chordLength">The maximum length for a chord around the circumference.
    ///   Controls the number of facets.</param>
    /// <param name="sliceLength">The maximum length of a segment along the cylinder axis.
    ///   Controls the number of slices along the cylinder. Zero to use just one slice.</param>
    /// <remarks>
    /// The cylinder is tessellation is calculated to the specified <paramref name="radius"/> and
    /// <paramref name="length"/>. The number of facets is determined by the
    /// <paramref name="chordLength"/> and is set such that the length of the chord created
    /// around the cylinder circle does not exceed the argument value.
    /// 
    /// Similarly, the <paramref name="sliceLength"/> limits the length of facet polygons along the
    /// length of the cylinder. The area of any quadrilateral along a cylinder facet is at
    /// most <paramref name="chordLength"/> * <paramref name="sliceLength"/>.
    /// 
    /// If the <paramref name="chordLength"/> is zero, then the number of facets is set to 8.
    /// If the <paramref name="sliceLength"/> is zero, then one quad is used per facet, covering
    /// the entire length of the cylinder.
    /// </remarks>
    public static void CalculateRingsAndFacets(out int facets, out int rings, float length, float radius, float chordLength, float sliceLength)
    {
      // Calculate the number of facets by setting the circle chord length to the tessellation resolution.
      //  chord length = 2rsin(theta / 2)
      //  theta = 2 * asin(chord length / 2r)
      float segmentAngle = (chordLength >= 0) ? 2.0f * Mathf.Asin(chordLength / (2 * radius)) : 2 * Mathf.PI;
      int minFacets = 3;
      facets = Mathf.FloorToInt(2 * Mathf.PI / segmentAngle + 0.5f);
      if (facets < minFacets)
      {
        facets = minFacets;
      }

      rings = Mathf.Max(2, Mathf.FloorToInt(length / sliceLength + 0.5f) + 1);
    }

    /// <summary>
    /// Tessellate a cylinder with the target number of facets and rings.
    /// </summary>
    /// <param name="vertices">Populated with the sphere vertices. Must be non-null and empty.</param>
    /// <param name="normals">Populated with the sphere vertices. May be null to skip normal calculation, but must be empty.</param>
    /// <param name="indices">Populated with the sphere line indices (pairs). Must be non-null and empty.</param>
    /// <param name="bottom">The position of the bottom of the cylinder.</param>
    /// <param name="top">The position of the top of the cylinder.</param>
    /// <param name="radius">The radius of the cylinder.</param>
    /// <param name="facets">The number of facets to divide the cylinder up into. Minimum 3 is enforced.</param>
    /// <param name="rings">The number of rings to slice the cylinder up into. Minimum 2 is enforced.</param>
    /// <param name="flags">Mesh generation flags.</param>
    public static bool Solid(List<Vector3> vertices, List<Vector3> normals, List<int> indices,
                              Vector3 bottom, Vector3 top, float radius, int facets, int rings, Flags flags)
    {
      if (vertices == null || vertices.Count != 0 ||
          normals != null && normals.Count != 0 ||
          indices != null && indices.Count != 0 ||
          bottom == top || radius <= 0 || facets < 3 || rings < 2)
      {
        return false;
      }

      Vector3 axis = (top - bottom).normalized;
      int bottomCapStartIndex = 0;
      int topCapStartIndex = 0;
      //int uvsFacetAdjustment = (flags & Flags.UVs) == Flags.UVs ? 1 : 0;
      int uvsFacetAdjustment = 0;

      facets = Math.Max(3, facets);
      rings = Math.Max(2, rings);

      // Note: the first and last vertex in each ring share a position, but use different UVs.
      // Note on UVs: u runs up the cylinder axis.
      int vertexCount = (rings) * (facets + uvsFacetAdjustment);

      // Duplicate end rings for distinct normals.
      if ((flags & Flags.Open) == 0)
      {
        vertexCount += facets * 2;
      }

      vertices.Capacity = vertexCount;
      normals.Capacity = vertexCount;

      Vector3[] radials = new Vector3[2];
      float nearAlignedDot = Mathf.Cos(85.0f / 180.0f * Mathf.PI);
      if (Vector3.Dot(new Vector3(0, 1, 0), axis) < nearAlignedDot)
      {
        radials[0] = Vector3.Cross(new Vector3(0, 1, 0), axis).normalized;
      }
      else
      {
        radials[0] = Vector3.Cross(new Vector3(1, 0, 0), axis).normalized;
      }
      radials[1] = Vector3.Cross(axis, radials[0]);

      // We build vertices for each ring first, then tessellate between the rings.
      Vector3 ringCentre, radial, firstVertex, firstNormal, v;
      float angleStep = 2.0f * Mathf.PI / (float)facets;
      for (int r = 0; r < rings; ++r)
      {
        float uf = (r / (float)(rings - 1));
        ringCentre = bottom + (top - bottom) * uf;
        firstVertex = ringCentre + radius * radials[0];
        vertices.Add(firstVertex);
        firstNormal = firstVertex.normalized;
        if (normals != null)
        { 
          normals.Add(firstNormal);
        }

        for (int f = 1; f < facets; ++f)
        {
          float angle = f * angleStep;
          radial = radius * (Mathf.Cos(angle) * radials[0] + Mathf.Sin(angle) * radials[1]);
          v = ringCentre + radial;
          vertices.Add(v);
          if (normals != null)
          {
            normals.Add(v.normalized);
          }
        }

        //// Duplicate the first ring vertex with different UV.
        //if ((flags & Flags.UVs) != 0)
        //{
        //  vertices.Add(firstVertex);
        //  if (normals != null)
        //  {
        //    normals.Add(firstNormal);
        //  }
        //}
      }

      // Duplicate top and bottom with different normals for end caps.
      if ((flags & Flags.Open) == 0)
      {
        int copyOffset = 0;
        bottomCapStartIndex = vertices.Count;
        firstNormal = (bottom - top).normalized;
        for (int f = 0; f < facets; ++f)
        {
          vertices.Add(vertices[f + copyOffset]);
          if (normals != null)
          {
            normals.Add(firstNormal);
          }
        }

        copyOffset = (rings - 1) * (facets + uvsFacetAdjustment);
        topCapStartIndex = vertices.Count;
        firstNormal = (top - bottom).normalized;
        for (int f = 0; f < facets; ++f)
        {
          vertices.Add(vertices[f + copyOffset]);
          if (normals != null)
          {
            normals.Add(firstNormal);
          }
        }
      }

      // Now triangulate between the rings.
      for (int r = 1; r < rings; ++r)
      {
        int ringStartIndex = r * (facets + uvsFacetAdjustment);
        int prevRingStartIndex = ringStartIndex - (facets + uvsFacetAdjustment);
        for (int f = 1; f <= facets + uvsFacetAdjustment; ++f)
        {
          indices.Add(prevRingStartIndex + f - 1);
          indices.Add(prevRingStartIndex + (f % facets));
          indices.Add(ringStartIndex + (f % facets));

          indices.Add(prevRingStartIndex + f - 1);
          indices.Add(ringStartIndex + (f % facets));
          indices.Add(ringStartIndex + f - 1);
        }
      }

      // Tessellate the end caps.
      if ((flags & Flags.Open) == 0)
      {
        for (int i = 1; i < facets; ++i)
        {
          indices.Add(bottomCapStartIndex);
          indices.Add(bottomCapStartIndex + (i + 1) % facets);
          indices.Add(bottomCapStartIndex + i);
        }

        for (int i = 1; i < facets; ++i)
        {
          indices.Add(topCapStartIndex);
          indices.Add(topCapStartIndex + i);
          indices.Add(topCapStartIndex + (i + 1) % facets);
        }
      }

      return true;
    }

    /// <summary>
    /// Calculate details of a line base cylinder mesh.
    /// </summary>
    /// <param name="vertices">Populated with the cylinder vertices (output).</param>
    /// <param name="indices">Populated with the cylinder indices (output).</param>
    /// <param name="bottom">Specifies the centre of the cylinder base.</param>
    /// <param name="top">Specifies the centre of the cylinder top.</param>
    /// <param name="radius">Specifies the cylinder radius.</param>
    /// <param name="circleVertexCount">The number of vertices around the cylinder circle.</param>
    /// <param name="wallConnections">The number of line connections between the top and bottom.</param>
    /// <returns>True on success, false if parameters prevent calculate.</returns>
    public static bool Wireframe(List<Vector3> vertices, List<int> indices, Vector3 bottom, Vector3 top, float radius, int circleVertexCount, int wallConnections)
    {
      if (vertices == null || vertices.Count != 0 ||
          indices != null && indices.Count != 0 ||
          bottom == top || radius <= 0 || circleVertexCount < 3 || wallConnections < 0)
      {
        return false;
      }

      Vector3 axis = (top - bottom).normalized;
      Vector3[] radials = new Vector3[2];
      float nearAlignedDot = Mathf.Cos(85.0f / 180.0f * Mathf.PI);

      vertices.Capacity = circleVertexCount * 2;

      if (Vector3.Dot(new Vector3(0, 1, 0), axis) < nearAlignedDot)
      {
        radials[0] = Vector3.Cross(new Vector3(0, 1, 0), axis).normalized;
      }
      else
      {
        radials[0] = Vector3.Cross(new Vector3(1, 0, 0), axis).normalized;
      }
      radials[1] = Vector3.Cross(axis, radials[0]);

      Vector3 ringCentre, radial;
      float angleStep = 2.0f * Mathf.PI / (float)circleVertexCount;
      int ioffset = 0;
      for (int r = 0; r < 2; ++r)
      {
        ioffset = r * circleVertexCount;
        ringCentre = bottom + (top - bottom) * r;
        for (int f = 0; f < circleVertexCount; ++f)
        {
          float angle = f * angleStep;
          radial = radius * (Mathf.Cos(angle) * radials[0] + Mathf.Sin(angle) * radials[1]);
          vertices.Add(ringCentre + radial);
          indices.Add(ioffset + f);
          indices.Add(ioffset + (f + 1) % circleVertexCount);
        }
      }

      // Generate wall lines.
      if (wallConnections > 0)
      {
        int wallConnectionStep = circleVertexCount / wallConnections;
        for (int i = 0; i < wallConnections; ++i)
        {
          indices.Add((i * wallConnectionStep) % circleVertexCount);
          indices.Add((i * wallConnectionStep) % circleVertexCount + circleVertexCount);
        }
      }

      return true;
    }
  }
}
