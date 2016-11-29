using UnityEngine;
using System.Collections;
using Tes.Shapes;

/// <summary>
/// Wraps a Unity mesh exposing it as a TES mesh.
/// </summary>
public class TesMeshWrapper : MeshBase
{
  public TesMeshWrapper(Mesh unityMesh)
  {
    UnityMesh = unityMesh;
    switch (UnityMesh.GetTopology(0))
    {
    case MeshTopology.Lines:
      MeshDrawType = Tes.Net.MeshDrawType.Lines;
      break;
    case MeshTopology.LineStrip:
      // Not correct.
      MeshDrawType = Tes.Net.MeshDrawType.Lines;
      break;
    case MeshTopology.Points:
      MeshDrawType = Tes.Net.MeshDrawType.Points;
      break;
    case MeshTopology.Quads:
      // FIXME:
      MeshDrawType = Tes.Net.MeshDrawType.Triangles;
      break;
    default:
    case MeshTopology.Triangles:
      MeshDrawType = Tes.Net.MeshDrawType.Triangles;
      break;
    }
    CalculateNormals = false;
  }

  public Mesh UnityMesh { get; protected set; }
  public override int IndexSize { get { return 4; } }

  public override uint VertexCount(int stream = 0)
  {
    return (uint)UnityMesh.vertexCount;
  }


  public override uint IndexCount(int stream = 0)
  {
    return UnityMesh.GetIndices(0) != null ? (uint)UnityMesh.GetIndices(0).Length : 0;
  }

  protected Tes.Maths.Vector3[] CachedVertices { get; set; }
  public override Tes.Maths.Vector3[] Vertices(int stream = 0)
  {
    if (CachedVertices == null)
    {
      CachedVertices = new Tes.Maths.Vector3[UnityMesh.vertexCount];
      for (int i = 0; i < UnityMesh.vertexCount; ++i)
      {
        CachedVertices[i] = new Tes.Maths.Vector3(UnityMesh.vertices[i].x, UnityMesh.vertices[i].y, UnityMesh.vertices[i].z);
      }
    }

    return CachedVertices;
  }


  public override int[] Indices4(int stream = 0)
  {
    return UnityMesh.GetIndices(0);
  }


  protected Tes.Maths.Vector3[] CachedNormals { get; set; }
  public override Tes.Maths.Vector3[] Normals(int stream = 0)
  {
    if (CachedNormals == null && UnityMesh.normals != null)
    {
      CachedNormals = new Tes.Maths.Vector3[UnityMesh.vertexCount];
      for (int i = 0; i < UnityMesh.vertexCount; ++i)
      {
        CachedNormals[i] = new Tes.Maths.Vector3(UnityMesh.vertices[i].x, UnityMesh.vertices[i].y, UnityMesh.vertices[i].z);
      }
    }

    return CachedNormals;
  }

  protected Tes.Maths.Vector2[] CachedUVs { get; set; }
  public override Tes.Maths.Vector2[] UVs(int stream = 0)
  {
    if (CachedUVs == null && UnityMesh.uv != null)
    {
      CachedUVs = new Tes.Maths.Vector2[UnityMesh.vertexCount];
      for (int i = 0; i < UnityMesh.vertexCount; ++i)
      {
        CachedUVs[i] = new Tes.Maths.Vector2(UnityMesh.uv[i].x, UnityMesh.uv[i].y);
      }
    }

    return CachedUVs;
  }

  protected uint[] CachedColours { get; set; }
  public override uint[] Colours(int stream = 0)
  {
    if (CachedColours == null && UnityMesh.colors32 != null)
    {
      CachedColours = new uint[UnityMesh.vertexCount];
      Color32 c;
      for (int i = 0; i < UnityMesh.vertexCount; ++i)
      {
        c = UnityMesh.colors32[i];
        CachedColours[i] = new Tes.Maths.Colour(c.r, c.g, c.b, c.a).Value;
      }
    }

    return CachedColours;
  }
}
