using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// Arrow tessellation helper.
  /// </summary>
  public static class Arrow
  {
    /// <summary>
    /// Default number of facets around the arrow wall.
    /// </summary>
    static readonly int ArrowFacets = 8;

    /// <summary>
    /// Default arrow length.
    /// </summary>
    public static readonly float ArrowLength = 1.0f;
    /// <summary>
    /// Default cone base radius.
    /// </summary>
    public static readonly float ConeRadius = 1.5f;
    /// <summary>
    /// Default ratio along the arrow where the cone base starts.
    /// </summary>
    public static readonly float ConeOffset = 0.8f;
    /// <summary>
    /// Top of the wall vertices. Inset into the cone for lighting.
    /// </summary>
    public static readonly float WallTop = 0.81f;
    /// <summary>
    /// Wall radius.
    /// </summary>
    public static readonly float WallRadius = 1.0f;

    /// <summary>
    /// The default arrow direction. This is the major axis the arrow is built with.
    /// The base lies at the origin, while the tip is at the direction vertex by the length.
    /// </summary>
    public static readonly Vector3 Direction = new Vector3(0, 0, 1);

    /// <summary>
    /// Tessellate a solid cone mesh.
    /// </summary>
    /// <param name="mesh">The target mesh object.</param>
    /// <param name="facets">Number of cylinder and cone facets.</param>
    /// <param name="arrowRadius">Arrow cone base radius.</param>
    /// <param name="cylinderRadius">Arrow cylinder radius.</param>
    /// <param name="cylinderLength">Cylinder length.</param>
    /// <param name="arrowLength">Arrow cone length.</param>
    /// <returns></returns>
    public static bool Solid(Mesh mesh, int facets, float arrowRadius, float cylinderRadius, float cylinderLength, float arrowLength)
    {
      if (facets < 3 || cylinderLength <= 0 || arrowLength < 0 || arrowLength <= cylinderLength ||
          arrowRadius <= 0 || cylinderRadius <= 0 || arrowRadius <= cylinderRadius)
      {
        return false;
      }

      // For each facet we have these vertices:
      // - cone walls (apex and base) * 2
      // - cone base * 2
      // - cylinder walls top/bottom * 2
      // - cylinder end cap
      Vector3[] vertices = new Vector3[facets * (2 + 2 + 2 + 1)];
      Vector3[] normals = new Vector3[vertices.Length];
      Vector3 normal, apex, toApex;
      Vector3 primaryAxis = new Vector3(0, 0, 1);

      // Set the cone apex.
      int vind = 0;
      for (int i = 0; i < facets; ++i)
      {
        vertices[vind] = new Vector3(0, 0, arrowLength);
        ++vind;
      }

      // Cone wall and base: 2 vertices offset by facets
      for (int i = 0; i < facets; ++i)
      {
        vertices[vind] = vertices[vind + facets] = new Vector3(arrowRadius * Mathf.Sin(i * 2 * Mathf.PI / facets), arrowRadius * Mathf.Cos(i * 2 * Mathf.PI / facets), cylinderLength);
        ++vind;
      }
      // Account for having built two vertices per facet above.
      vind += facets;

      // Cylinder/cone seem (cone base and cylinder top): 2 vertices per facet for normal generation.
      for (int i = 0; i < facets; ++i)
      {
        vertices[vind] = vertices[vind + facets] = new Vector3(cylinderRadius * Mathf.Sin(i * 2 * Mathf.PI / facets), cylinderRadius * Mathf.Cos(i * 2 * Mathf.PI / facets), cylinderLength);
        ++vind;
      }
      // Account for having built two vertices per facet above.
      vind += facets;

      // Cylinder bottom and base: 2 vertices each
      for (int i = 0; i < facets; ++i)
      {
        vertices[vind] = vertices[vind + facets] = new Vector3(cylinderRadius * Mathf.Sin(i * 2 * Mathf.PI / facets), cylinderRadius * Mathf.Cos(i * 2 * Mathf.PI / facets), 0);
        ++vind;
      }

      // Generate normals. Cone walls first (apex and cylinder wall).
      vind = 0;
      for (int i = 0; i < facets; ++i)
      {
        normal = vertices[vind + facets];
        apex = vertices[vind];
        toApex = apex - normal;
        // Remove height component from the future normal
        normal -= Vector3.Dot(normal, primaryAxis) * primaryAxis;

        // Cross and normalise to get the actual normal.
        Vector3 v2 = Vector3.Cross(toApex, normal);
        normal = Vector3.Cross(v2, toApex).normalized;
        normals[vind] = normals[vind + facets] = normal;
        ++vind;
      }
      // Account for having built two normals per facet above.
      vind += facets;

      // Cone base * 2.
      for (int i = 0; i < facets; ++i)
      {
        normals[vind] = normals[vind + facets] = new Vector3(0, 0, -1);
        ++vind;
      }
      // Account for having built two normals per facet above.
      vind += facets;

      // Cylinder walls: top and bottom.
      for (int i = 0; i < facets; ++i)
      {
        // Use the cylinder base vertices as normals. They have no y component.
        normals[vind] = normals[vind + facets] = vertices[vind + 2 * facets].normalized;
        ++vind;
      }
      // Account for having built two normals per facet above.
      vind += facets;

      // Cylinder base.
      for (int i = 0; i < facets; ++i)
      {
        normals[vind] = new Vector3(0, 0, -1);
        ++vind;
      }

      // Now generate indices to tessellate. Listed below are the number of triangles per part,
      // using three indices per triangle.
      // - Arrow head => facets
      // - Arrow base (cylinder transition) => 2 * facets
      // - Cylinder walls => 2 * facets
      // - Cylinder base => facets - 2
      int[] indices = new int[(facets + 2 * facets + 2 * facets + facets - 2) * 3];
      int iind = 0;

      // Cone walls
      for (int i = 0; i < facets; ++i)
      {
        indices[iind++] = i;
        indices[iind++] = (i + 1) % facets + facets;
        indices[iind++] = i + facets;
      }

      // Cone base.
      int[] quad = new int[4];
      vind = 2 * facets;
      for (int i = 0; i < facets; ++i)
      {
        quad[0] = vind + i;
        quad[1] = vind + (i + 1) % facets;
        quad[2] = vind + facets + i;
        quad[3] = vind + facets + (i + 1) % facets;
        indices[iind++] = quad[0];
        indices[iind++] = quad[1];
        indices[iind++] = quad[2];
        indices[iind++] = quad[1];
        indices[iind++] = quad[3];
        indices[iind++] = quad[2];
      }

      // Cylinder walls.
      vind += 2 * facets;
      for (int i = 0; i < facets; ++i)
      {
        quad[0] = vind + i;
        quad[1] = vind + (i + 1) % facets;
        quad[2] = vind + facets + i;
        quad[3] = vind + facets + (i + 1) % facets;
        indices[iind++] = quad[0];
        indices[iind++] = quad[1];
        indices[iind++] = quad[2];
        indices[iind++] = quad[1];
        indices[iind++] = quad[3];
        indices[iind++] = quad[2];
      }

      // Cylinder/arrow base.
      vind += 2 * facets;
      for (int i = 1; i < facets - 1; ++i)
      {
        indices[iind++] = vind;
        indices[iind++] = vind + i;
        indices[iind++] = vind + i + 1;
      }

      if (mesh.subMeshCount < 1)
      {
        mesh.subMeshCount = 1;
      }

      mesh.vertices = vertices;
      mesh.normals = normals;
      mesh.SetIndices(indices, MeshTopology.Triangles, 0);

      return true;
    }

    /// <summary>
    /// Build a wireframe arrow.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="facets"></param>
    /// <param name="circleResolution">Controls the number of vertices in the cone and cylinder
    /// circles The number of vertices is calculated as <paramref name="facets"/> times
    /// <paramref name="circleResolution"/>.</param>
    /// <param name="arrowRadius"></param>
    /// <param name="cylinderRadius"></param>
    /// <param name="cylinderLength"></param>
    /// <param name="arrowLength"></param>
    /// <returns></returns>
    public static bool Wireframe(Mesh mesh, int facets, int circleResolution, float arrowRadius, float cylinderRadius, float cylinderLength, float arrowLength)
    {
      if (facets < 3 || circleResolution < 1 || cylinderLength <= 0 || arrowLength < 0 || arrowLength <= cylinderLength ||
          arrowRadius <= 0 || cylinderRadius <= 0 || arrowRadius <= cylinderRadius)
      {
        return false;
      }

      // Vertex count:
      // - apex => 1
      // - cone circle => facets * circleResolution
      // - cylinder circles => 2 * facets * circleResolution
      Vector3[] vertices = new Vector3[1 + 3 * (facets * circleResolution)];

      int vind = 0;
      vertices[vind] = new Vector3(0, 0, arrowLength);
      ++vind;

      // Build all three circles at the same time.
      int circleVertCount = circleResolution * facets;
      for (int i = 0; i < circleVertCount; ++i)
      {
        vertices[vind] = new Vector3(arrowRadius * Mathf.Sin(i * 2 * Mathf.PI / circleVertCount), arrowRadius * Mathf.Cos(i * 2 * Mathf.PI / circleVertCount), cylinderLength);
        vertices[vind + circleVertCount] = new Vector3(cylinderRadius * Mathf.Sin(i * 2 * Mathf.PI / circleVertCount), cylinderRadius * Mathf.Cos(i * 2 * Mathf.PI / circleVertCount), cylinderLength);
        vertices[vind + 2 * circleVertCount] = new Vector3(cylinderRadius * Mathf.Sin(i * 2 * Mathf.PI / circleVertCount), cylinderRadius * Mathf.Cos(i * 2 * Mathf.PI / circleVertCount), 0);
        vind++;
      }

      // Index count (built of lines):
      // - apex connections: facets * 2
      // - cone base circle: facets * circleResolution * 2
      // - cylinder top circle: facets * circleResolution * 2
      // - cylinder base circle: facets * circleResolution * 2
      int[] indices = new int[(facets * 2 + 3 * circleVertCount) * 2];

      // Cone lines.
      int iind = 0;
      for (int i = 0; i < facets; ++i)
      {
        indices[iind++] = 0;
        indices[iind++] = (i * circleResolution) + 1;
      }

      // Cone base circle.
      int circleOffset = 1;
      for (int i = 0; i < circleVertCount; ++i)
      {
        indices[iind++] = circleOffset + i;
        indices[iind++] = circleOffset + (i + 1) % circleVertCount;
      }

      // Cylinder top.
      circleOffset += circleVertCount;
      for (int i = 0; i < circleVertCount; ++i)
      {
        indices[iind++] = circleOffset + i;
        indices[iind++] = circleOffset + (i + 1) % circleVertCount;
      }

      // Cylinder bottom.
      circleOffset += circleVertCount;
      for (int i = 0; i < circleVertCount; ++i)
      {
        indices[iind++] = circleOffset + i;
        indices[iind++] = circleOffset + (i + 1) % circleVertCount;
      }

      // Cylinder wall lines.
      // offset: skip apex and cone circle.
      circleOffset = 1 + circleVertCount;
      for (int i = 0; i < facets; ++i)
      {
        indices[iind++] = 1 * circleOffset + (i * circleResolution);
        indices[iind++] = 2 * circleOffset + (i * circleResolution);
      }


      if (mesh.subMeshCount < 1)
      {
        mesh.subMeshCount = 1;
      }

      mesh.vertices = vertices;
      mesh.SetIndices(indices, MeshTopology.Lines, 0);

      return true;
    }

    /// <summary>
    /// Build the default solid mesh.
    /// </summary>
    /// <returns>The mesh object.</returns>
    public static Mesh Solid()
    {
      Mesh mesh = new Mesh();
      if (Solid(mesh, ArrowFacets, ConeRadius, WallRadius, WallTop, ArrowLength))
      { 
        return mesh;
      }
      return null;
    }

    /// <summary>
    /// Build the default wireframe mesh.
    /// </summary>
    /// <returns>The mesh object.</returns>
    public static Mesh Wireframe()
    {
      Mesh mesh = new Mesh();
      if (Wireframe(mesh, ArrowFacets, 4, ConeRadius, WallRadius, WallTop, ArrowLength))
      {
        return mesh;
      }
      return null;
    }
  }
}
