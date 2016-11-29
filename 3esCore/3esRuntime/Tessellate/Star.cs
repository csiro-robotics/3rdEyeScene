using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// Tessellate a small star shape.
  /// </summary>
  /// <remarks>
  /// The wireframe version is represented by three, crossed, axis aligned lines. The solid/triangle
  /// version gives the lines some thickness.
  /// </remarks>
  public static class Star
  {
    /// <summary>
    /// The default triangle width at the cross over point for the solid mesh.
    /// </summary>
    static readonly float BaseWidth = 0.15f;
    
    /// <summary>
    /// Triangle vertices.
    /// </summary>
    static readonly Vector3[] SolidVertices = new Vector3[]
    {
      // X
      new Vector3(-0.5f, 0, 0),
      new Vector3(0, -BaseWidth, 0),
      new Vector3(0, BaseWidth, 0),
      new Vector3(0, 0, -BaseWidth),
      new Vector3(0, 0, BaseWidth),
      new Vector3(0.5f, 0, 0),
      // Y
      new Vector3(0, -0.5f, 0),
      new Vector3(-BaseWidth, 0, 0),
      new Vector3(BaseWidth, 0, 0),
      new Vector3(0, 0, -BaseWidth),
      new Vector3(0, 0, BaseWidth),
      new Vector3(0, 0.5f, 0),
      // Z
      new Vector3(0, 0, -0.5f),
      new Vector3(-BaseWidth, 0, 0),
      new Vector3( BaseWidth, 0, 0),
      new Vector3(0, -BaseWidth, 0),
      new Vector3(0,  BaseWidth, 0),
      new Vector3(0, 0, 0.5f),
    };

    /// <summary>
    /// Triangle indices.
    /// </summary>
    static readonly int[] SolidIndices = new int[]
    {
      0, 1, 2, 0, 2, 1, 0, 3, 4, 0, 4, 3, 5, 1, 2, 5, 2, 1, 5, 3, 4, 5, 4, 3,
      6, 7, 8, 6, 8, 7, 6, 9, 10, 6, 10, 9, 11, 7, 8, 11, 8, 7, 11, 9, 10, 11, 10, 9,
      12, 13, 14, 12, 14, 13, 12, 15, 16, 12, 16, 15, 17, 13, 14, 17, 14, 13, 17, 15, 16, 17, 16, 15
    };

    /// <summary>
    /// Line vertices.
    /// </summary>
    static readonly Vector3[] LineVertices = new Vector3[]
    {
      // X
      new Vector3(-0.5f, 0, 0),
      new Vector3(0.5f, 0, 0),
      // Y
      new Vector3(0, -0.5f, 0),
      new Vector3(0, 0.5f, 0),
      // Z
      new Vector3(0, 0, -0.5f),
      new Vector3(0, 0, 0.5f)
    };

    /// <summary>
    /// Line indices.
    /// </summary>
    static readonly int[] LineIndices = new int[]
    {
      0, 1, 2, 3, 4, 5
    };

    /// <summary>
    /// Create a solid, triangle based representation of the star.
    /// </summary>
    /// <returns>The requested mesh.</returns>
    public static Mesh Solid()
    {
      Mesh mesh = new Mesh();
      mesh.vertices = SolidVertices;
      // mesh.normals = _solidNormals;
      mesh.subMeshCount = 1;
      mesh.SetIndices(SolidIndices, MeshTopology.Triangles, 0);
      return mesh;
    }

    /// <summary>
    /// Create a line based representation of the star.
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