using UnityEngine;
using System.Collections.Generic;

namespace Tes.Tessellate
{
  /// <summary>
  /// Tessellation of a capsule shape. This is a cylinder with hemispherical end caps.
  /// </summary>
  /// <remarks>
  /// The meshes are constructed with in three sections with a cylinder of length 1
  /// and sphere radius of 1. All are centred on the origin, making the final shape a
  /// unit sphere and overlapping length 1, radius 1 cylinder on.
  /// Different proportions can be achieved by separating the mesh parts and scaling
  /// accordingly.
  /// </remarks>
  public static class Capsule
  {
    /// <summary>
    /// Index of the top hemisphere mesh component.
    /// </summary>
    public static int TopIndex { get { return 0; } }
    /// <summary>
    /// Index of the bottom hemisphere mesh component.
    /// </summary>
    public static int BottomIndex { get { return 1; } }
    /// <summary>
    /// Index of the cylindrical mesh component.
    /// </summary>
    public static int CylinderIndex { get { return 2; } }
    /// <summary>
    /// The default primary axis used in construction (0, 0, 1).
    /// </summary>
    public static Vector3 PrimaryAxis { get { return new Vector3(0, 0, 1); } }
    
    /// <summary>
    /// The number of ring layers used in constructing the hemisphere sections.
    /// </summary>
    /// <remarks>
    /// The spherical parts are constructed using latitude/longitude based tessellation.
    /// This value marks the number of rings used, including the equatorial ring.
    /// </remarks>
    public static int HemiSphereLayers { get { return 4; } }
    /// <summary>
    /// Number of facets used in constructing the spherical and cylindrical components.
    /// </summary>
    public static int HemiSphereFacets { get { return 8; } }

    /// <summary>
    /// Mesh triangle indices for the hemisphere sections.
    /// </summary>
    public static readonly int[] HemiSphereIndices = new int[]
    {
      // Inner ring
      0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7, 0, 7, 8, 0, 8, 1,

      // Top ring
      1, 9, 10, 1, 10, 2,
      2, 10, 11, 2, 11, 3,
      3, 11, 12, 3, 12, 4,
      4, 12, 13, 4, 13, 5,
      5, 13, 14, 5, 14, 6,
      6, 14, 15, 6, 15, 7,
      7, 15, 16, 7, 16, 8,
      8, 16, 9, 8, 9, 1,

      // Middle ring
      9, 17, 18, 9, 18, 10,
      10, 18, 19, 10, 19, 11,
      11, 19, 20, 11, 20, 12,
      12, 20, 21, 12, 21, 13,
      13, 21, 22, 13, 22, 14,
      14, 22, 23, 14, 23, 15,
      15, 23, 24, 15, 24, 16,
      16, 24, 17, 16, 17, 9,

      // Bottom ring
      17, 25, 26, 17, 26, 18,
      18, 26, 27, 18, 27, 19,
      19, 27, 28, 19, 28, 20,
      20, 28, 29, 20, 29, 21,
      21, 29, 30, 21, 30, 22,
      22, 30, 31, 22, 31, 23,
      23, 31, 32, 23, 32, 24,
      24, 32, 25, 24, 25, 17
    };
    
    /// <summary>
    /// Mesh triangle indices for the cylindrical part.
    /// </summary>
    public static readonly int[] CylinderIndices = new int[]
    {
      0, 8, 1, 1, 8, 9,
      1, 9, 2, 2, 9, 10,
      2, 10, 3, 3, 10, 11,
      3, 11, 4, 4, 11, 12,
      4, 12, 5, 5, 12, 13,
      5, 13, 6, 6, 13, 14,
      6, 14, 7, 7, 14, 15,
      7, 15, 0, 0, 15, 8
    };

    /// <summary>
    /// Generates a solid capsule mesh, 1 unit long and 1 unit radius.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// This returns an array of three meshes so that each piece may be positioned and scaled 
    /// independently. Use <see cref="TopIndex"/>, <see cref="BottomIndex"/> and
    /// <see cref="CylinderIndex"/> to address the top and bottom hemispheres and the
    /// cylindrical part respectively. The hemispheres are positioned such that their
    /// origin lies at (0, 0, 0), while the cylindrical piece is positioned such that its
    /// centre also lies at the same point.
    /// </remarks>
    public static Mesh[] Solid()
    {
      Mesh[] meshes = new Mesh[3];

      List<Vector3> vertices = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();

      // Generate the end cap meshes.
      Vector3 axis = PrimaryAxis;

      Sphere.LatLong(vertices, normals, indices, Vector3.zero, 1.0f, 3, 8, axis, true);
      meshes[TopIndex] = new Mesh();
      meshes[TopIndex].subMeshCount = 1;
      meshes[TopIndex].vertices = vertices.ToArray();
      meshes[TopIndex].normals = normals.ToArray();
      meshes[TopIndex].SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

      vertices.Clear();
      normals.Clear();
      indices.Clear();
      Sphere.LatLong(vertices, normals, indices, Vector3.zero, 1.0f, 3, 8, -axis, true);
      meshes[BottomIndex] = new Mesh();
      meshes[BottomIndex].subMeshCount = 1;
      meshes[BottomIndex].vertices = vertices.ToArray();
      meshes[BottomIndex].normals = normals.ToArray();
      meshes[BottomIndex].SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

      vertices.Clear();
      normals.Clear();
      indices.Clear();
      Cylinder.Solid(vertices, normals, indices, -0.5f * axis, 0.5f * axis, 1.0f, 8, 2, Cylinder.Flags.Open);
      meshes[CylinderIndex] = new Mesh();
      meshes[CylinderIndex].subMeshCount = 1;
      meshes[CylinderIndex].vertices = vertices.ToArray();
      meshes[CylinderIndex].normals = normals.ToArray();
      meshes[CylinderIndex].SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

      return meshes;
    }

    /// <summary>
    /// Construct a line based representation of a capsule.
    /// </summary>
    /// <returns>The constructed mesh components.</returns>
    /// <remarks>
    /// The resulting array can be indexed using <see cref="TopIndex"/>,
    /// <see cref="BottomIndex"/> and <see cref="CylinderIndex"/> to identify
    /// the specific components.
    /// </remarks>
    public static Mesh[] Wireframe()
    {
      Mesh[] meshes = new Mesh[3];

      meshes[TopIndex] = new Mesh();
      meshes[TopIndex].subMeshCount = 2;
      
      meshes[BottomIndex] = new Mesh();
      meshes[BottomIndex].subMeshCount = 2;

      // Build line geometry. For this we build a single hoop in two pieces:
      // semi circles and joining lines.
      // We create 2 mesh sub-parts for this.
      const int semiCircleVertCount = 36;
      Vector3[] vertices = new Vector3[semiCircleVertCount * 2];
      int[] indices = new int[semiCircleVertCount * 2];
      for (int i = 0; i < semiCircleVertCount; ++i)
      {
        float angle = i * 2.0f * Mathf.PI / (semiCircleVertCount - 1);
        // Y plane top.
        Vector3 v = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        vertices[i] = v;
        // X plane top.
        v.y = v.x;
        v.x = 0;
        vertices[semiCircleVertCount * 1 + i] = v;
      }

      for (int i = 0; i < indices.Length; ++i)
      {
        indices[i] = i;
      }

      meshes[TopIndex].vertices = vertices;

      // Adjust vertices for the bottom rings. Only reflect in Z this time as we have no normals
      // or triangle winding to concern ourselves with.
      for (int i = 0; i < vertices.Length; ++i)
      {
        vertices[i].z *= -1.0f;
      }
      meshes[BottomIndex].vertices = vertices;
      
      // Set indices for top and bottom. Y plane ring.
      meshes[TopIndex].SetIndices(indices, MeshTopology.LineStrip, 0);
      meshes[BottomIndex].SetIndices(indices, MeshTopology.LineStrip, 0);
      
      // Create the second mesh part indices. Just offset by semiCircleVertCount.
      for (int i = 0; i < semiCircleVertCount; ++i)
      {
        indices[i] += semiCircleVertCount;
      }
      meshes[TopIndex].SetIndices(indices, MeshTopology.LineStrip, 1);
      meshes[BottomIndex].SetIndices(indices, MeshTopology.LineStrip, 1);
      
      // Finally create the joining 'cylinder' mesh.
      vertices = new Vector3[8];
      vertices[0] = new Vector3(1.0f, 0, 0.5f);
      vertices[1] = new Vector3(1.0f, 0, -0.5f);
      vertices[2] = new Vector3(-1.0f, 0, 0.5f);
      vertices[3] = new Vector3(-1.0f, 0, -0.5f);
      vertices[4] = new Vector3(0, 1.0f, 0.5f);
      vertices[5] = new Vector3(0, 1.0f, -0.5f);
      vertices[6] = new Vector3(0, -1.0f, 0.5f);
      vertices[7] = new Vector3(0, -1.0f, -0.5f);
      
      indices = new int[8];
      for (int i = 0; i < indices.Length; ++i)
      {
        indices[i] = i;
      }
      
      meshes[CylinderIndex] = new Mesh();
      meshes[CylinderIndex].subMeshCount = 1;
      meshes[CylinderIndex].vertices = vertices;
      meshes[CylinderIndex].SetIndices(indices, MeshTopology.Lines, 0);
      
      return meshes;
    }
  }
}