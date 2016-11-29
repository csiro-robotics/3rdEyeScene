using System;
using System.IO;
using Tes.Runtime;
using Tes.Net;
using UnityEngine;

namespace Tes
{
  public class PyramidHandler : Handlers.ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public PyramidHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _solidMesh = SolidPyramid();
      _wireframeMesh = WireframePyramid();
      if (Root != null)
      {
        Root.name = Name;
      }
    }

    public static Mesh SolidPyramid()
    {
      // Arrangement:
      // 6/2---------3/7
      //   |         |
      //   |         |
      //   |    8    |
      //   |         |
      //   |         |
      // 4/0---------1/5
      //
      // Y
      // /\
      // ||
      // -->X

      Vector3[] vertices = new Vector3[]
      {
          new Vector3(-1, -1, 0),
          new Vector3( 1, -1, 0),
          new Vector3(-1,  1, 0),
          new Vector3( 1,  1, 0),
          new Vector3(-1, -1, 0),
          new Vector3( 1, -1, 0),
          new Vector3(-1,  1, 0),
          new Vector3( 1,  1, 0),
          new Vector3(0, 0, 1)
      };

      int[] indices = new int[]
      {
        // Base
        0, 2, 1, 1, 2, 3,

        // Walls
        4, 8, 6, 6, 8, 7, 7, 8, 5, 5, 8, 4
      };

      Mesh mesh = new Mesh();
      mesh.subMeshCount = 1;
      mesh.vertices = vertices;
      mesh.normals = new Vector3[vertices.Length];
      mesh.SetIndices(indices, MeshTopology.Triangles, 0);
      mesh.RecalculateNormals();
      return mesh;
    }

    public static Mesh WireframePyramid()
    {
      // Arrangement:
      // 2---------3
      // |         |
      // |         |
      // |    4    |
      // |         |
      // |         |
      // 0---------1
      //
      // Y
      // /\
      // ||
      // -->X

      Vector3[] vertices = new Vector3[]
      {
          new Vector3(-1, -1, 0),
          new Vector3( 1, -1, 0),
          new Vector3(-1,  1, 0),
          new Vector3( 1,  1, 0),
          new Vector3(0, 0, 1)
      };

      int[] indices = new int[]
      {
        // Base
        0, 2, 2, 3, 3, 1, 1, 0,

        // Walls
        0, 4, 1, 4, 2, 4, 3, 4
      };

      Mesh mesh = new Mesh();
      mesh.subMeshCount = 1;
      mesh.vertices = vertices;
      mesh.SetIndices(indices, MeshTopology.Lines, 0);
      return mesh;
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Pyramid"; } }

    /// <summary>
    /// <see cref="RoutingID.UserIDStart"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Net.RoutingID.UserIDStart; } }

    /// <summary>
    /// Solid mesh representation.
    /// </summary>
    public override Mesh SolidMesh { get { return _solidMesh; } }
    /// <summary>
    /// Wireframe mesh representation.
    /// </summary>
    public override Mesh WireframeMesh { get { return _wireframeMesh; } }

    private Mesh _solidMesh;
    private Mesh _wireframeMesh;
  }
}
