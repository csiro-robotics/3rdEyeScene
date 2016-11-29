using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// Box tessellation helper. Ensures discrete normals.
  /// </summary>
  public static class Box
  {
    /// <summary>
    /// Default vertex data.
    /// </summary>
    public static readonly Vector3[] VertexData = new Vector3[]
    {
      // +X
      new Vector3(0.5f, 0.5f, -0.5f),
      new Vector3(0.5f, 0.5f, 0.5f),
      new Vector3(0.5f, -0.5f, 0.5f),
      new Vector3(0.5f, -0.5f, -0.5f),
      // -X
      new Vector3(-0.5f, -0.5f, -0.5f),
      new Vector3(-0.5f, -0.5f, 0.5f),
      new Vector3(-0.5f, 0.5f, 0.5f),
      new Vector3(-0.5f, 0.5f, -0.5f),

      // +Y
      new Vector3(-0.5f, 0.5f, -0.5f),
      new Vector3(-0.5f, 0.5f, 0.5f),
      new Vector3(0.5f, 0.5f, 0.5f),
      new Vector3(0.5f, 0.5f, -0.5f),
      // -Y
      new Vector3(0.5f, -0.5f, -0.5f),
      new Vector3(0.5f, -0.5f, 0.5f),
      new Vector3(-0.5f, -0.5f, 0.5f),
      new Vector3(-0.5f, -0.5f, -0.5f),

      // +Z
      new Vector3(0.5f, -0.5f, 0.5f),
      new Vector3(0.5f, 0.5f, 0.5f),
      new Vector3(-0.5f, 0.5f, 0.5f),
      new Vector3(-0.5f, -0.5f, 0.5f),
      // -Z
      new Vector3(0.5f, 0.5f, -0.5f),
      new Vector3(0.5f, -0.5f, -0.5f),
      new Vector3(-0.5f, -0.5f, -0.5f),
      new Vector3(-0.5f, 0.5f, -0.5f)
    };

    /// <summary>
    /// Default vertex data.
    /// </summary>
    public static readonly Vector3[] LineVertexData = new Vector3[]
    {
      // Right
      new Vector3(0.5f, 0.5f, -0.5f),
      new Vector3(0.5f, 0.5f, 0.5f),
      new Vector3(0.5f, -0.5f, 0.5f),
      new Vector3(0.5f, -0.5f, -0.5f),
      // Left
      new Vector3(-0.5f, 0.5f, -0.5f),
      new Vector3(-0.5f, 0.5f, 0.5f),
      new Vector3(-0.5f, -0.5f, 0.5f),
      new Vector3(-0.5f, -0.5f, -0.5f)
    };

    /// <summary>
    /// Normals.
    /// </summary>
    public static readonly Vector3[] NormalData = new Vector3[]
    {
      new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
      new Vector3(-1, 0, 0), new Vector3(-1, 0, 0), new Vector3(-1, 0, 0), new Vector3(-1, 0, 0),
      new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
      new Vector3(0, -1, 0), new Vector3(0, -1, 0), new Vector3(0, -1, 0), new Vector3(0, -1, 0),
      new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
      new Vector3(0, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 0, -1)
    };
    
    /// <summary>
    /// Face indices for solid shapes.
    /// </summary>
    public static readonly int[] SolidIndices = new int[]
    {
      // +X
      0, 1, 2, 0, 2, 3,
      // -X
      4, 5, 6, 4, 6, 7,
      // +Y
      8, 9, 10, 8, 10, 11,
      // -Y
      12, 13, 14, 12, 14, 15,
      // +Z
      16, 17, 18, 16, 18, 19,
      // -Z
      20, 21, 22, 20, 22, 23
    };
    
    /// <summary>
    /// Wireframe indices.
    /// </summary>
    public static readonly int[] LineIndices = new int[]
    {
      // right
      0, 1, 1, 2, 2, 3, 3, 0,

      // left
      4, 5, 5, 6, 6, 7, 7, 4,

      // Connect faces faces.
      0, 4, 1, 5, 2, 6, 3, 7
    };

    /// <summary>
    /// Build the default solid mesh. A 1m cube centred on the origin.
    /// </summary>
    /// <returns>The mesh object.</returns>
    public static Mesh Solid()
    {
      Mesh mesh = new Mesh();
      mesh.vertices = VertexData;
      mesh.normals = NormalData;
      mesh.subMeshCount = 1;
      mesh.SetIndices(SolidIndices, MeshTopology.Triangles, 0);
      return mesh;
    }

    /// <summary>
    /// Build the default wireframe mesh. A 1m cube centred on the origin.
    /// </summary>
    /// <returns>The mesh object.</returns>
    public static Mesh Wireframe()
    {
      Mesh mesh = new Mesh();
      mesh.vertices = LineVertexData;
      mesh.subMeshCount = 1;
      mesh.SetIndices(LineIndices, MeshTopology.Lines, 0);
      return mesh;
    }
  }
}