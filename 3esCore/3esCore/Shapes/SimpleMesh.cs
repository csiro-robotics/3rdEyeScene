using System;
using System.Collections.Generic;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  /// <summary>
  /// Flags identifying which parts of a <see cref="SimpleMesh"/> are valid.
  /// </summary>
  [Flags]
  public enum MeshComponentFlag
  {
    /// <summary>
    /// Need vertex data.
    /// </summary>
    Vertex = (1 << 0),
    /// <summary>
    /// Need index data.
    /// </summary>
    Index = (1 << 1),
    /// <summary>
    /// Need per vertex colours.
    /// </summary>
    Colour = (1 << 2),
    /// <summary>
    /// Need per vertex normals.
    /// </summary>
    Normal = (1 << 3),
    /// <summary>
    /// Need per vertex UV coordinates.
    /// </summary>
    UV = (1 << 4)
  }

  /// <summary>
  /// A simple <see cref="MeshResource"/> implementation.
  /// </summary>
  public class SimpleMesh : MeshBase
  {
    /// <summary>
    /// Create a new mesh resource.
    /// </summary>
    /// <param name="id">The unique mesh resource ID for this mesh.</param>
    /// <param name="drawType">Defines the topology.</param>
    /// <param name="components">Identifies the required <see cref="MeshComponentFlag"/>.</param>
    public SimpleMesh(uint id, MeshDrawType drawType = MeshDrawType.Triangles,
                      MeshComponentFlag components = MeshComponentFlag.Vertex | MeshComponentFlag.Index)
    {
      ID = id;
      DrawType = (byte)MeshDrawType.Triangles;
      Tint = 0xffffffffu;
      Transform = Matrix4.Identity;
      Components = components;
      _vertices = new List<Vector3>();
      if ((components & MeshComponentFlag.Index) == MeshComponentFlag.Index)
      {
        _indices = new List<int>();
      }
      CalculateNormals = true;
    }

    /// <summary>
    /// Clears the content of this mesh, resulting in an empty mesh.
    /// </summary>
    void Clear()
    {
      Components = MeshComponentFlag.Vertex;
      Transform = Matrix4.Identity;
      Tint = 0xffffffff;
      MeshDrawType = MeshDrawType.Triangles;
      _vertices = new List<Vector3>();
      _normals = null;
      _colours = null;
      _indices = null;
      _uvs = null;
    }

    /// <summary>
    /// Defines the byte size used by indices in this mesh.
    /// </summary>
    /// <value>4</value>
    public override int IndexSize { get { return 4; } }

    /// <summary>
    /// Exposes the number of vertices in the mesh.
    /// </summary>
    /// <returns>The number of vertices in this mesh.</returns>
    /// <param name="stream">For future use. Must be zero.</param>
    public override uint VertexCount(int stream = 0) { return (uint)_vertices.Count; }

    /// <summary>
    /// Exposes the number of indices in the mesh.
    /// </summary>
    /// <returns>The number of indices in this mesh.</returns>
    /// <param name="stream">For future use. Must be zero.</param>
    public override uint IndexCount(int stream = 0) { return (uint)_indices.Count; }

    /// <summary>
    /// Supports iteration of the vertices of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public override Vector3[] Vertices(int stream = 0) { return _vertices.ToArray(); }

    /// <summary>
    /// Supports iteration of the indices of the mesh when using four byte indices.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public override int[] Indices4(int stream = 0) { return _indices.ToArray(); }

    /// <summary>
    /// Supports iteration of the normal of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public override Vector3[] Normals(int stream = 0) { return _normals != null ? _normals.ToArray() : null; }

    /// <summary>
    /// Supports iteration of the UV coordinates of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public override Vector2[] UVs(int stream = 0) { return _uvs != null ? _uvs.ToArray() : null; }

    /// <summary>
    /// Supports iteration of per vertex colours of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public override uint[] Colours(int stream = 0) { return _colours != null ? _colours.ToArray() : null; }

    /// <summary>
    /// Sets the vertex at the given index.
    /// </summary>
    /// <param name="at">Vertex index.</param>
    /// <param name="v">The vertex value.</param>
    public void SetVertex(int at, Vector3 v) { _vertices[at] = v; }

    /// <summary>
    /// Adds a vertex.
    /// </summary>
    /// <param name="v">The vertex to add.</param>
    public void AddVertex(Vector3 v)
    {
      _vertices.Add(v);
    }

    /// <summary>
    /// Adds a number of vertices.
    /// </summary>
    /// <param name="vertices">Vertices to add.</param>
    public void AddVertices(IEnumerable<Vector3> vertices)
    {
      _vertices.AddRange(vertices);
    }

    /// <summary>
    /// Sets an index value.
    /// </summary>
    /// <param name="at">The index's position in the index array.</param>
    /// <param name="index">The index value (referencing a valid vertex).</param>
    public void SetIndex(int at, int index) { _indices[at] = index; }

    /// <summary>
    /// Adds an index value.
    /// </summary>
    /// <param name="index">The index value (referencing a valid vertex).</param>
    public void AddIndex(int index)
    {
      EnsureIndices();
      _indices.Add(index);
    }

    /// <summary>
    /// Add a set of indices.
    /// </summary>
    /// <param name="indices">Indices to add.</param>
    public void AddIndices(IEnumerable<int> indices)
    {
      EnsureIndices();
      _indices.AddRange(indices);
    }

    /// <summary>
    /// Set a vertex normal.
    /// </summary>
    /// <param name="at">The vertex index.</param>
    /// <param name="normal">The normal value.</param>
    public void SetNormal(int at, Vector3 normal) { _normals[at] = normal; }

    /// <summary>
    /// Add a vertex normal.
    /// </summary>
    /// <param name="normal">The normal value.</param>
    public void AddNormal(Vector3 normal)
    {
      EnsureNormals();
      _normals.Add(normal);
    }

    /// <summary>
    /// Add a list of normals.
    /// </summary>
    /// <param name="normals">The normals to add</param>
    public void AddNormals(IEnumerable<Vector3> normals)
    {
      EnsureNormals();
      _normals.AddRange(normals);
    }

    /// <summary>
    /// Set a vertex UV coordinate.
    /// </summary>
    /// <param name="at">The vertex index.</param>
    /// <param name="uv">The UV coordinate.</param>
    public void SetUV(int at, Vector2 uv) { _uvs[at] = uv; }

    /// <summary>
    /// Add a vertex UV coordinate.
    /// </summary>
    /// <param name="uv">The UV coordinate.</param>
    public void AddUV(Vector2 uv)
    {
      EnsureUVs();
      _uvs.Add(uv);
    }

    /// <summary>
    /// Add a list of UV coordinates.
    /// </summary>
    /// <param name="uvs">The UV coordinates.</param>
    public void AddUV(IEnumerable<Vector2> uvs)
    {
      EnsureUVs();
      _uvs.AddRange(uvs);
    }

    /// <summary>
    /// Set a vertex colour.
    /// </summary>
    /// <param name="at">The vertex index.</param>
    /// <param name="colour">The colour value.</param>
    public void SetColour(int at, uint colour) { _colours[at] = colour; }

    /// <summary>
    /// Add a vertex colour.
    /// </summary>
    /// <param name="colour">The colour value.</param>
    public void AddColour(uint colour)
    {
      EnsureColours();
      _colours.Add(colour);
    }

    /// <summary>
    /// Add a list of colours.
    /// </summary>
    /// <param name="colours">The colours to add</param>
    public void AddColours(IEnumerable<uint> colours)
    {
      EnsureColours();
      _colours.AddRange(colours);
    }

    /// <summary>
    /// Ensures indices are marked as required and allocated.
    /// </summary>
    protected void EnsureIndices()
    {
      if (_indices == null)
      {
        _indices = new List<int>();
      }
      Components |= MeshComponentFlag.Index;
    }

    /// <summary>
    /// Ensures normals are marked as required and allocated.
    /// </summary>
    protected void EnsureNormals()
    {
      if (_normals == null)
      {
        _normals = new List<Vector3>();
      }
      Components |= MeshComponentFlag.Normal;
    }

    /// <summary>
    /// Ensures UVs are marked as required and allocated.
    /// </summary>
    protected void EnsureUVs()
    {
      if (_uvs == null)
      {
        _uvs = new List<Vector2>();
      }
      Components |= MeshComponentFlag.UV;
    }

    /// <summary>
    /// Ensures colours are marked as required and allocated.
    /// </summary>
    protected void EnsureColours()
    {
      if (_colours == null)
      {
        _colours = new List<uint>();
      }
      Components |= MeshComponentFlag.Colour;
    }

    /// <summary>
    /// Vertex array.
    /// </summary>
    private List<Vector3> _vertices = null;
    /// <summary>
    /// Index array.
    /// </summary>
    private List<int> _indices = null;
    /// <summary>
    /// Normal array.
    /// </summary>
    private List<Vector3> _normals = null;
    /// <summary>
    /// UV array.
    /// </summary>
    private List<Vector2> _uvs = null;
    /// <summary>
    /// Colour array.
    /// </summary>
    private List<uint> _colours = null;
  }
}
