using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// Tessellates a small quadrilateral representing a plane. Includes a small spike representing the plane normal.
  /// </summary>
  public static class Plane
  {
    /// <summary>
    /// Vertices when using triangles.
    /// </summary>
    public static readonly Vector3[] SolidVertices = new Vector3[]
    {
      new Vector3(-0.5f, -0.5f, 0),
      new Vector3(0.5f, -0.5f, 0),
      new Vector3(0.5f, 0.5f, 0),
      new Vector3(-0.5f, 0.5f, 0),
      new Vector3(-0.05f, 0, 0),
      new Vector3(0.05f, 0, 0),
      new Vector3(0, -0.05f, 0),
      new Vector3(0,  0.05f, 0),
      new Vector3(0, 0, 1),
    };
    
    private static Vector3[] _solidNormals;
    /// <summary>
    /// Normals when using triangles.
    /// </summary>
    public static Vector3[] SolidNormals { get { return _solidNormals; } }
    
    /// <summary>
    /// Vertices when using lines.
    /// </summary>
    public static readonly Vector3[] LineVertices = new Vector3[]
    {
      new Vector3(-0.5f, -0.5f, 0),
      new Vector3(0.5f, -0.5f, 0),
      new Vector3(0.5f,  0.5f, 0),
      new Vector3(-0.5f,  0.5f, 0),
      new Vector3(0, 0, 0),
      new Vector3(0, 0, 1),
    };

    /// <summary>
    /// Indices for line vertices.
    /// </summary>
    public static readonly int[] LineIndices = new int[]
    {
      0, 1, 1, 2, 2, 3, 3, 0, 4, 5
    };

    /// <summary>
    /// Indices for triangle vertices.
    /// </summary>
    public static readonly int[] SolidIndices = new int[]
    {
      0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2,
      4, 5, 8, 5, 4, 8, 6, 7, 8, 7, 6, 8
    };
    
    /// <summary>
    /// Initialise normals.
    /// </summary>
    static Plane()
    {
      _solidNormals = new Vector3[SolidVertices.Length];
      for (int i = 0; i < 4; ++i)
      {
        _solidNormals[i] = new Vector3(0, 0, 1);
      }
      
      _solidNormals[4] = _solidNormals[5] = new Vector3(0, 1, 0);
      _solidNormals[6] = _solidNormals[7] = new Vector3(1, 0, 0);
      _solidNormals[8] = new Vector3(0, 0, 1);
    }

    /// <summary>
    /// Create a solid, triangle based representation of the plane.
    /// </summary>
    /// <returns>The requested mesh.</returns>
    public static Mesh Solid()
    {
      Mesh mesh = new Mesh();
      mesh.vertices = SolidVertices;
      mesh.normals = _solidNormals;
      mesh.subMeshCount = 1;
      mesh.SetIndices(SolidIndices, MeshTopology.Triangles, 0);
      return mesh;
    }

    /// <summary>
    /// Create a line based representation of the plane.
    /// </summary>
    /// <returns>The requested mesh.</returns>
    public static Mesh Wireframe()
    {
      Mesh mesh = new Mesh();
      mesh.vertices = LineVertices;
      mesh.subMeshCount = 1;
      mesh.SetIndices(LineIndices, MeshTopology.Lines, 0);
      return mesh;
    }
  }
}