using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// Supports construction of <c>UnityEngine.Mesh</c> objects from raw vertex and index data.
  /// </summary>
  /// <remarks>
  /// This class supports taking arbitrarily large meshes and breaking them up into multiple
  /// <c>UnityEngine.Mesh</c> objects, respecting the indexing limits of that mesh class.
  /// 
  /// The class also supports updating mesh data and rebuilding the mesh objects, though it
  /// is not intended for highly dynamic meshes. That is, the update support is intended for
  /// sporadic updates rather than per frame updates of all vertex data.
  /// 
  /// The class attempts to discriminately rebuild only components which need it using various
  /// dirty flags.
  /// 
  /// Note: it is up to the caller to maintain valid element counts in the various containers.
  /// </remarks>
  public class MeshBuilder
  {
    /// <summary>
    /// Flags identifying which parts of the mesh data are dirty.
    /// </summary>
    [Flags]
    public enum DirtyFlag
    {
      /// <summary>
      /// Zero flag.
      /// </summary>
      None = 0,
      /// <summary>
      /// Indices have been modified.
      /// </summary>
      Indices = (1 << 0),
      /// <summary>
      /// Indices have been modified.
      /// </summary>
      Vertices = (1 << 1),
      /// <summary>
      /// Normals have been modified.
      /// </summary>
      Normals = (1 << 2),
      /// <summary>
      /// Vertex colours have been modified.
      /// </summary>
      Colours = (1 << 3),
      /// <summary>
      /// UV coordinates have been modified.
      /// </summary>
      UVs = (1 << 4),
      /// <summary>
      /// Everything is dirty.
      /// </summary>
      All = Indices | Vertices | Normals | Colours | UVs
    }

    /// <summary>
    /// Limit on the number of indices per Mesh part imposed by Unity.
    /// </summary>
    public const int UnityIndexLimit = 65000;

    /// <summary>
    /// Is any part of the mesh data dirty?
    /// </summary>
    /// <value><c>true</c> if dirty; otherwise, <c>false</c>.</value>
    public bool Dirty { get { return _dirty != DirtyFlag.None; } }

    /// <summary>
    /// Defines the indexing topology for the mesh objects.
    /// </summary>
    public MeshTopology Topology { get { return _topology; } set { _topology = value; } }

    /// <summary>
    /// Padding value applied to the bounds of all generated meshes.
    /// </summary>
    /// <remarks>
    /// Only effected on subsequent mesh reconstructions.
    /// </remarks>
    public Vector3 BoundsPadding = Vector3.zero;

    /// <summary>
    /// Access the vertex array. Intended for read only access.
    /// </summary>
    public Vector3[] Vertices { get { return _vertices.ToArray(); } }
    /// <summary>
    /// Access the index array. Intended for read only access.
    /// </summary>
    public int[] Indices { get { return _indices.ToArray(); } }
    /// <summary>
    /// Access the normals array. Intended for read only access.
    /// </summary>
    public Vector3[] Normals { get { return _normals.ToArray(); } }
    /// <summary>
    /// Access the colours array. Intended for read only access.
    /// </summary>
    public Color32[] Colours { get { return _colours.ToArray(); } }
    /// <summary>
    /// Access the uvs array. Intended for read only access.
    /// </summary>
    public Vector2[] UVs { get { return _uvs.ToArray(); } }

    /// <summary>
    /// Query the number of indices per element for <paramref name="topology"/>.
    /// </summary>
    /// <param name="topology">The mesh topology</param>
    /// <returns>The number of indices per primitive.</returns>
    /// <remarks>
    /// <list type="table">
    /// <listheader><term>Type</term><description>Index Count</description></listheader>
    /// <item><term><see cref="MeshTopology.Points"/></term><description>1</description></item>
    /// <item><term><see cref="MeshTopology.LineStrip"/></term><description>1 (not supported).</description></item>
    /// <item><term><see cref="MeshTopology.Lines"/></term><description>2</description></item>
    /// <item><term><see cref="MeshTopology.Triangles"/></term><description>3</description></item>
    /// <item><term><see cref="MeshTopology.Quads"/></term><description>4</description></item>
    /// <item><term>Else</term><description>1</description></item>
    /// </list>
    /// </remarks>
    public static int GetTopologyIndexStep(MeshTopology topology)
    {
      switch (topology)
      {
        case MeshTopology.Triangles:
          return 3;
        case MeshTopology.Quads:
          return 4;
        case MeshTopology.Lines:
          return 2;
        case MeshTopology.LineStrip:
          return 1;
        case MeshTopology.Points:
          return 1;
      }
      return 1;
    }

    /// <summary>
    /// Query the per primitive index count for this mesh.
    /// </summary>
    /// <remarks>
    /// See <see cref="GetTopologyIndexStep(MeshTopology)"/>.
    /// </remarks>
    public int TopologyIndexStep { get { return GetTopologyIndexStep(Topology); } }

    /// <summary>
    /// Calculate per vertex normals during construction?
    /// </summary>
    /// <remarks>
    /// This may prove expensive for large meshes.
    /// </remarks>
    public bool CalculateNormals { get; set; }

    /// <summary>
    /// Forces a full bounds recalculation.
    /// </summary>
    /// <remarks>
    /// This forces a full mesh rebuild on the next call to <see cref="GetMeshes()"/>.
    /// </remarks>
    public void RecalculateBounds()
    {
      _dirty |= DirtyFlag.Vertices;
      if (_vertices.Count > 0)
      {
        _bounds.SetMinMax(_vertices[0], _vertices[0]);
        for (int i = 0; i < _vertices.Count; ++i)
        {
          _bounds.Encapsulate(_vertices[i]);
        }
      }
      else
      {
        _bounds.center = _bounds.extents = Vector3.zero;
      }

      if (IndexCount > UnityIndexLimit)
      {
        for (int i = 0; i < _meshDetails.Count; ++i)
        {
          _meshDetails[i].Dirty |= DirtyFlag.Vertices;
        }
      }
    }

    /// <summary>
    /// Gets the number of indices across the complete mesh data.
    /// </summary>
    /// <remarks>
    /// This is based in the number of indices added when indices have been added,
    /// or based on the number of vertices when no indices have been added. In the
    /// latter case, sequential indexing of vertices is assumed.
    /// </remarks>
    public int IndexCount
    {
      get
      {
        if (_indices.Count == 0)
        {
          return _vertices.Count;
        }
        return _indices.Count;
      }
    }

    /// <summary>
    /// Has a set of explicit indices been provided, or are vertices indexed in sequence?
    /// </summary>
    public bool ExplicitIndices
    {
      get { return _indices.Count > 0; }
    }

    /// <summary>
    /// Query the index at the given index.
    /// </summary>
    /// <param name="iind">The index index to query.</param>
    /// <returns>The index at <paramref name="iind"/></returns>
    /// <remarks>
    /// This is a convenience function which caters for inferred, sequential indexing
    /// (no explicit indices). It either returns an index from the index array, or
    /// returns <paramref name="iind"/> when indexing is inferred. No bounds checking.
    /// </remarks>
    public int IndexAt(int iind)
    {
      return (_indices.Count > 0) ? _indices[iind] : iind;
    }

    /// <summary>
    /// Returns the number of vertices in the mesh.
    /// </summary>
    public int VertexCount
    {
      get
      {
        return _vertices.Count;
      }
    }

    /// <summary>
    /// Generate mesh objects or return cached mesh objects.
    /// </summary>
    /// <returns>The mesh objects required to represent the stored mesh data.</returns>
    /// <remarks>
    /// This may generate new mesh objects when the mesh data are <see cref="Dirty"/>
    /// or return a cached set of objects.
    /// </remarks>
    public Mesh[] GetMeshes()
    {
      if (Dirty)
      {
        if (IndexCount <= UnityIndexLimit)
        {
          UpdateSingleMesh();
        }
        else
        {
          UpdateMultiMesh();
        }
        _dirty = DirtyFlag.None;
        // Clear warnings log.
        _warningFlags = DirtyFlag.None;
      }
      return _meshObjects.ToArray();
    }

    /// <summary>
    /// Create a set of mesh objects using the given set of indices instead of the
    /// stored indices.
    /// </summary>
    /// <returns>The mesh objects created from by using <paramref name="indices"/> with the
    /// stored vertex data.</returns>
    /// <param name="indices">The index data to use with the given meshes.</param>
    /// <param name="topology">The topology of the given index set.</param>
    /// <remarks>
    /// This supports creating a sub-mesh using <paramref name="indices"/> from the stored
    /// mesh data. The topology may also be changed by specifying a different topology to
    /// that of this builder. The indices are assumed to suit the specified topology.
    /// 
    /// This method never uses cached meshes and is only recommended for small index sets.
    /// </remarks>
    public Mesh[] GetReindexedMeshes(int[] indices, MeshTopology topology)
    {
      List<Mesh> meshes = new List<Mesh>();
      List<int> inds = new List<int>();
      List<Vector3> verts = new List<Vector3>();
      List<Vector3> norms = (_normals.Count > 0) ? new List<Vector3>() : null;
      List<Vector2> uvs = (_uvs.Count > 0) ? new List<Vector2>() : null;
      List<Color32> colours = (_colours.Count > 0) ? new List<Color32>() : null;

      int indexStride = 1;
      switch (topology)
      {
        case MeshTopology.Triangles:
          indexStride = 3;
          break;
        case MeshTopology.Quads:
          indexStride = 4;
          break;
        case MeshTopology.Lines:
          indexStride = 2;
          break;
        case MeshTopology.LineStrip:
          indexStride = 1;
          break;
        case MeshTopology.Points:
          indexStride = 1;
          break;
      }

      // Generate meshes by unwinding indexing for simplicity.
      int vind;
      for (int i = 0; i + indexStride - 1 < indices.Length; i += indexStride)
      {
        for (int j = 0; j < indexStride; ++j)
        {
          vind = _indices[i + j];
          inds.Add(i + j);
          verts.Add(_vertices[vind]);
          if (norms != null)
          {
            // Support single, uniform normal option.
            norms.Add(_normals[(_normals.Count > 1) ? vind : 0]);
          }
          if (uvs != null)
          {
            uvs.Add(_uvs[vind]);
          }
          // TODO: Force white colours when none are specifies? For point clouds.
          if (colours != null)
          {
            colours.Add(_colours[vind]);
          }
        }

        // Complete this mesh object if the next index will overrun the limit
        // or it's the last index.
        if (i + indexStride >= UnityIndexLimit || i + indexStride >= indices.Length)
        {
          Mesh mesh = CreateMesh();
          mesh.vertices = verts.ToArray();
          if (norms != null)
          {
            mesh.normals = norms.ToArray();
            norms.Clear();
          }
          if (colours != null)
          {
            mesh.colors32 = colours.ToArray();
            colours.Clear();
          }
          if (uvs != null)
          {
            mesh.uv = uvs.ToArray();
            uvs.Clear();
          }
          mesh.SetIndices(inds.ToArray(), topology, 0);

          verts.Clear();
          inds.Clear();

          meshes.Add(mesh);

          // Rewind indexing for line strips to maintain continuity,
          // but only if we need more mesh objects.
          if (topology == MeshTopology.LineStrip && i + indexStride < indices.Length)
          {
            --i;
          }
        }
      }

      return meshes.ToArray();
    }

    /// <summary>
    /// Add a vertex.
    /// </summary>
    /// <param name="v">The vertex to add.</param>
    public void AddVertex(Vector3 v)
    {
      _dirty |= DirtyFlag.Vertices;
      _vertices.Add(v);
      _bounds.Encapsulate(v);
    }

    /// <summary>
    /// A a number of vertices.
    /// </summary>
    /// <param name="verts">The vertices to add.</param>
    public void AddVertices(IEnumerable<Vector3> verts)
    {
      _dirty |= DirtyFlag.Vertices;
      if (_indices.Count == 0)
      {
        // Handle sequential indexing.
        _dirty |= DirtyFlag.Indices;
      }
      _vertices.AddRange(verts);
      var iter = verts.GetEnumerator();
      while (iter.MoveNext())
      {
        _bounds.Encapsulate(iter.Current);
      }
    }

    /// <summary>
    /// Update the vertex at <paramref name="index"/>, or add a new vertex if <paramref name="index"/> is out of range.
    /// </summary>
    /// <param name="index">The index of the vertex of interest.</param>
    /// <param name="v">The new vertex value.</param>
    public void UpdateVertex(int index, Vector3 v)
    {
      // Only works if the vertex exists or it is the next vertex.
      // Results are undefined otherwise.
      if (index < _vertices.Count)
      {
        _vertices[index] = v;
      }
      else
      {
        _vertices.Add(v);
      }
      _bounds.Encapsulate(v);
      DirtyVertices(index, 1, DirtyFlag.Vertices);
    }

    /// <summary>
    /// Update the vertices starting at <paramref name="baseIndex"/>, or add a new vertices if out of range.
    /// </summary>
    /// <param name="baseIndex">The index of the first vertex of interest.</param>
    /// <param name="verts">The new vertex values.</param>
    public void UpdateVertices(int baseIndex, IEnumerable<Vector3> verts)
    {
      DirtyVertices(baseIndex, verts.Count(), DirtyFlag.Vertices);
      int vind = baseIndex;
      var iter = verts.GetEnumerator();
      while (iter.MoveNext())
      {
        if (vind < _vertices.Count)
        {
          _vertices[vind] = iter.Current;
        }
        else
        {
          _vertices.Add(iter.Current);
        }
        ++vind;
        _bounds.Encapsulate(iter.Current);
      }
    }

    /// <summary>
    /// Add a single mesh index.
    /// </summary>
    /// <param name="i">The index value to add.</param>
    public void AddIndex(int i)
    {
      _dirty |= DirtyFlag.Indices;
      _indices.Add(i);
    }

    /// <summary>
    /// Add a range of indices.
    /// </summary>
    /// <param name="inds">The element to add.</param>
    public void AddIndices(IEnumerable<int> inds)
    {
      _dirty |= DirtyFlag.Indices;
      _indices.AddRange(inds);
    }

    /// <summary>
    /// Update the index at <paramref name="index"/>, or add a new index if <paramref name="index"/> is out of range.
    /// </summary>
    /// <param name="index">The index of the index of interest.</param>
    /// <param name="i">The new index value.</param>
    public void UpdateIndex(int index, int i)
    {
      // Only works if the vertex exists or it is the next vertex.
      // Results are undefined otherwise.
      if (index < _indices.Count)
      {
        _indices[index] = i;
      }
      else
      {
        _indices.Add(i);
      }
      DirtyIndices(index, 1);
    }

    /// <summary>
    /// Update the indices starting at <paramref name="baseIndex"/>, or add a new indices out of range.
    /// </summary>
    /// <param name="baseIndex">The index of the first index of interest.</param>
    /// <param name="inds">The new index values.</param>
    public void UpdateIndices(int baseIndex, IEnumerable<int> inds)
    {
      DirtyIndices(baseIndex, inds.Count());
      int iind = baseIndex;
      var iter = inds.GetEnumerator();
      while (iter.MoveNext())
      {
        if (iind < _indices.Count)
        {
          _indices[iind] = iter.Current;
        }
        else
        {
          _indices.Add(iter.Current);
        }
        ++iind;
      }
    }

    /// <summary>
    /// Add a single mesh vertex normal.
    /// </summary>
    /// <param name="n">The normal to add.</param>
    public void AddNormal(Vector3 n)
    {
      _dirty |= DirtyFlag.Normals;
      _normals.Add(n);
    }

    /// <summary>
    /// Add a range of vertex normals.
    /// </summary>
    /// <param name="normals">The range of elements to add.</param>
    public void AddNormals(IEnumerable<Vector3> normals)
    {
      _dirty |= DirtyFlag.Normals;
      _normals.AddRange(normals);
    }

    /// <summary>
    /// Update the normal at <paramref name="index"/>, or add a new normal if <paramref name="index"/> is out of range.
    /// </summary>
    /// <param name="index">The index of the normal of interest.</param>
    /// <param name="n">The new normal value.</param>
    public void UpdateNormal(int index, Vector3 n)
    {
      // Only works if the vertex exists or it is the next vertex.
      // Results are undefined otherwise.
      if (index < _normals.Count)
      {
        _normals[index] = n;
      }
      else
      {
        _normals.Add(n);
      }
      DirtyVertices(index, 1, DirtyFlag.Normals);
    }

    /// <summary>
    /// Update the normals starting at <paramref name="baseIndex"/>, or add a new normals out of range.
    /// </summary>
    /// <param name="baseIndex">The index of the first normal of interest.</param>
    /// <param name="normals">The new normal values.</param>
    public void UpdateNormals(int baseIndex, IEnumerable<Vector3> normals)
    {
      DirtyVertices(baseIndex, normals.Count(), DirtyFlag.Normals);
      int vind = baseIndex;
      var iter = normals.GetEnumerator();
      while (iter.MoveNext())
      {
        if (vind < _normals.Count)
        {
          _normals[vind] = iter.Current;
        }
        else
        {
          _normals.Add(iter.Current);
        }
        ++vind;
      }
    }

    /// <summary>
    /// Add a single UV coordinate.
    /// </summary>
    /// <param name="uv">The UV coordinate to add.</param>
    public void AddUV(Vector2 uv)
    {
      _dirty |= DirtyFlag.UVs;
      _uvs.Add(uv);
    }

    /// <summary>
    /// Add a range of UV coordinates.
    /// </summary>
    /// <param name="uvs">The range of elements to add.</param>
    public void AddUVs(IEnumerable<Vector2> uvs)
    {
      _dirty |= DirtyFlag.UVs;
      _uvs.AddRange(uvs);
    }

    /// <summary>
    /// Update the UV at <paramref name="index"/>, or add a new UV if <paramref name="index"/> is out of range.
    /// </summary>
    /// <param name="index">The index of the vertex of interest.</param>
    /// <param name="uv">The new uv value.</param>
    public void UpdateUV(int index, Vector2 uv)
    {
      // Only works if the vertex exists or it is the next vertex.
      // Results are undefined otherwise.
      if (index < _uvs.Count)
      {
        _uvs[index] = uv;
      }
      else
      {
        _uvs.Add(uv);
      }
      DirtyVertices(index, 1, DirtyFlag.UVs);
    }

    /// <summary>
    /// Update the UVs starting at <paramref name="baseIndex"/>, or add a new UVs out of range.
    /// </summary>
    /// <param name="baseIndex">The index of the vertex of interest.</param>
    /// <param name="uvs">The new uv values.</param>
    public void UpdateUVs(int baseIndex, IEnumerable<Vector2> uvs)
    {
      DirtyVertices(baseIndex, uvs.Count(), DirtyFlag.UVs);
      int vind = baseIndex;
      var iter = uvs.GetEnumerator();
      while (iter.MoveNext())
      {
        if (vind < _uvs.Count)
        {
          _uvs[vind] = iter.Current;
        }
        else
        {
          _uvs.Add(iter.Current);
        }
        ++vind;
      }
    }

    /// <summary>
    /// Add a single vertex colour.
    /// </summary>
    /// <param name="c">The colour to add.</param>
    public void AddColour(Color32 c)
    {
      _dirty |= DirtyFlag.Colours;
      _colours.Add(c);
    }

    /// <summary>
    /// Add a range of vertex colours.
    /// </summary>
    /// <param name="colours">The range of elements to add.</param>
    public void AddColours(IEnumerable<Color32> colours)
    {
      _dirty |= DirtyFlag.Colours;
      _colours.AddRange(colours);
    }

    /// <summary>
    /// Update the colour at <paramref name="index"/>, or add a new colour if <paramref name="index"/> is out of range.
    /// </summary>
    /// <param name="index">The index of the colour of interest.</param>
    /// <param name="c">The new colour value.</param>
    public void UpdateColour(int index, Color32 c)
    {
      // Only works if the vertex exists or it is the next vertex.
      // Results are undefined otherwise.
      if (index < _colours.Count)
      {
        _colours[index] = c;
      }
      else
      {
        _colours.Add(c);
      }
      DirtyVertices(index, 1, DirtyFlag.Colours);
    }

    /// <summary>
    /// Update the colours beginning at <paramref name="baseIndex"/>, or add a new colour if out of range.
    /// </summary>
    /// <param name="baseIndex">The index of the first colour of interest.</param>
    /// <param name="colours">The new colour values.</param>
    public void UpdateColours(int baseIndex, IEnumerable<Color32> colours)
    {
      DirtyVertices(baseIndex, colours.Count(), DirtyFlag.Colours);
      int vind = baseIndex;
      var iter = colours.GetEnumerator();
      while (iter.MoveNext())
      {
        if (vind < _colours.Count)
        {
          _colours[vind] = iter.Current;
        }
        else
        {
          _colours.Add(iter.Current);
        }
        ++vind;
      }
    }

    /// <summary>
    /// Mark some vertex data as dirty. 
    /// </summary>
    /// <param name="vertexIndex">Identifies the vertex of interest.</param>
    /// <param name="flag">The dirty flag to apply. Should be one of the vertex related values.</param>
    protected void DirtyVertex(int vertexIndex, DirtyFlag flag)
    {
      DirtyVertices(vertexIndex, 1, flag);
    }

    /// <summary>
    /// Mark a range of vertex data as dirty. 
    /// </summary>
    /// <param name="firstVertexIndex">Identifies the first vertex of interest.</param>
    /// <param name="count">The number of vertices to dirty.</param>
    /// <param name="flag">The dirty flag to apply. Should be one of the vertex related values.</param>
    protected void DirtyVertices(int firstVertexIndex, int count, DirtyFlag flag)
    {
      if ((flag & DirtyFlag.Vertices) != 0 && _indices.Count == 0)
      {
        // Handle sequential indexing.
        flag |= DirtyFlag.Indices;
      }

      _dirty |= flag;
      for (int i = 0; i < _meshDetails.Count; ++i)
      {
        // Always dirty the last part.
        if (i + 1 == _meshDetails.Count || _meshDetails[i].UsesVertices(firstVertexIndex, count))
        {
          _meshDetails[i].Dirty |= flag;
        }
      }
    }

    /// <summary>
    /// Mark an index as dirty. 
    /// </summary>
    /// <param name="indexIndex">Identifies the index of interest.</param>
    protected void DirtyIndex(int indexIndex)
    {
      DirtyIndices(indexIndex, 1);
    }

    /// <summary>
    /// Mark a range of indices as dirty. 
    /// </summary>
    /// <param name="firstIndexIndex">Identifies the first index of interest.</param>
    /// <param name="count">The number of indices to dirty.</param>
    protected void DirtyIndices(int firstIndexIndex, int count)
    {
      _dirty |= DirtyFlag.Indices;

      for (int i = 0; i < _meshDetails.Count; ++i)
      {
        if (_meshDetails[i].UsesIndices(firstIndexIndex, count))
        {
          _meshDetails[i].Dirty = DirtyFlag.Indices;
        }
      }
    }

    /// <summary>
    /// Represents details of part of the mesh.
    /// </summary>
    /// <remarks>
    /// A mesh part may be used to represent all of or part of the data in the owning
    /// <see cref="MeshBuilder"/>. In the case only a subset is represented (to respect
    /// Unity's limited indexing range), the part represents a contiguous set of
    /// indices from the original mesh data. The index range can be identifies by
    /// the <see cref="FirstSourceIndex"/> and <see cref="IndexCount"/>.
    /// </remarks>
    protected class PartDetails
    {
      /// <summary>
      /// Does the <see cref="Mesh"/> required update?
      /// </summary>
      public DirtyFlag Dirty = DirtyFlag.All;

      /// <summary>
      /// The Unity mesh representation.
      /// </summary>
      public Mesh Mesh;

      /// <summary>
      /// Mesh bounds. May be rough.
      /// </summary>
      public Bounds Bounds = new Bounds();

      /// <summary>
      /// Maps the part vertex indices back to the original vertex indices.
      /// </summary>
      /// <remarks>
      /// For example, if <c>ToSourceVertexMappings[3]</c> is 101, this means that
      /// the vertex at index 3 in the <c>UnityEngine.Mesh</c> is at vertex 101 in the
      /// <c>MeshBuilder</c> vertex array.
      /// </remarks>
      public List<int> ToSourceVertexMappings = new List<int>();
      /// <summary>
      /// Maps the original vertex indices to the part indices.
      /// </summary>
      /// <remarks>
      /// This creates the reverse mappings of <see cref="ToSourceVertexMappings"/>.
      /// If an entry is not found, then it is not used by this part.
      /// </remarks>
      public Dictionary<int, int> ToPartVertexMappings = new Dictionary<int, int>();

      /// <summary>
      /// Identifies the first index in the original mesh data this part represents.
      /// </summary>
      public int FirstSourceIndex = 0;
      /// <summary>
      /// Identifies the number of index elements in this part.
      /// </summary>
      public int IndexCount = 0;

      /// <summary>
      /// Clear the data mappings.
      /// </summary>
      public void Clear()
      {
        ToSourceVertexMappings.Clear();
        ToPartVertexMappings.Clear();
      }

      /// <summary>
      /// Add a vertex and associated data from the <paramref name="builder"/>.
      /// </summary>
      /// <param name="sourceIndex">The index of the vertex to add in <paramref name="builder"/>.</param>
      /// <param name="builder">The <see cref="MeshBuilder"/> to add from.</param>
      /// <param name="partIndices">Index data for this part.</param>
      /// <param name="partVerts">Vertex data for this part.</param>
      /// <param name="partNormals">Normals data for this part.</param>
      /// <param name="partColours">Colours data for this part.</param>
      /// <param name="partUVs">UV data for this part.</param>
      /// <remarks>
      /// The part maintains a map of previously added vertices and re-uses those if re-referenced.
      /// </remarks>
      public void AddFromIndex(int sourceIndex, MeshBuilder builder,
                               List<int> partIndices,
                               List<Vector3> partVerts, List<Vector3> partNormals,
                               List<Color32> partColours, List<Vector2> partUVs)
      {
        int partIndex;
        Vector3 v;
        if (!ToPartVertexMappings.TryGetValue(sourceIndex, out partIndex))
        {
          Bounds bounds = Mesh.bounds;
          // No existing mapping.
          partIndex = partVerts.Count;
          ToPartVertexMappings.Add(sourceIndex, partIndex);
          ToSourceVertexMappings.Add(sourceIndex);

          partIndices.Add(partIndex);
          if (sourceIndex < builder._vertices.Count)
          {
            v = builder._vertices[sourceIndex];
            partVerts.Add(v);
          }
          else
          {
            v = Vector3.zero;
            partVerts.Add(Vector3.zero);
            builder.LogRangeWarning(sourceIndex, builder._vertices.Count, DirtyFlag.Vertices);
          }

          if (builder._normals.Count > 1)
          {
            if (sourceIndex < builder._normals.Count)
            {
              partNormals.Add(builder._normals[sourceIndex]);
            }
            else
            {
              partNormals.Add(Vector3.one);
              builder.LogRangeWarning(sourceIndex, builder._normals.Count, DirtyFlag.Normals);
            }
          }
          else if (builder._normals.Count == 1)
          {
            // Uniform normal.
            partNormals.Add(builder._normals[0]);
          }
          if (builder._colours.Count > 0)
          {
            if (sourceIndex < builder._colours.Count)
            {
              partColours.Add(builder._colours[sourceIndex]);
            }
            else
            {
              partColours.Add(new Color32(255, 0, 255, 255));
              builder.LogRangeWarning(sourceIndex, builder._colours.Count, DirtyFlag.Colours);
            }
          }
          if (builder._uvs.Count > 0)
          {
            if (sourceIndex < builder._uvs.Count)
            { 
              partUVs.Add(builder._uvs[sourceIndex]);
            }
            else
            {
              partUVs.Add(Vector2.zero);
              builder.LogRangeWarning(sourceIndex, builder._uvs.Count, DirtyFlag.UVs);
            }
          }

          bounds.Encapsulate(v);
          bounds.Expand(builder.BoundsPadding);
          Mesh.bounds = bounds;
        }
        else
        {
          // Reference existing vertex.
          partIndices.Add(partIndex);
          ToSourceVertexMappings.Add(sourceIndex);
        }
      }

      /// <summary>
      /// Updates used vertices by copying mapped vertices from <paramref name="builder"/>.
      /// </summary>
      /// <param name="builder">The owning builder object.</param>
      public void UpdateVertices(MeshBuilder builder)
      {
        Vector3[] verts = Mesh.vertices;
        Bounds bounds = new Bounds();
        int vind;
        foreach (var kvp in ToPartVertexMappings)
        {
          vind = kvp.Key;
          if (vind < builder._vertices.Count)
          {
            verts[kvp.Value] = builder._vertices[vind];
          }
          else
          {
            builder.LogRangeWarning(vind, builder._vertices.Count, DirtyFlag.Vertices);
          }
          bounds.Encapsulate(verts[kvp.Value]);
        }
        bounds.Expand(builder.BoundsPadding);
        Mesh.bounds = bounds;
        Mesh.vertices = verts;
      }

      /// <summary>
      /// Updates used normals by copying mapped normals from <paramref name="builder"/>.
      /// </summary>
      /// <param name="builder">The owning builder object.</param>
      public void UpdateNormals(MeshBuilder builder)
      {
        Vector3[] normals = Mesh.normals;
        int vind;
        foreach (var kvp in ToPartVertexMappings)
        {
          vind = kvp.Key;
          if (builder._normals.Count == 1)
          {
            // Uniform normal.
            normals[kvp.Value] = builder._normals[0];
          }
          else if (vind < builder._normals.Count)
          {
            normals[kvp.Value] = builder._normals[vind];
          }
          else
          {
            builder.LogRangeWarning(vind, builder._normals.Count, DirtyFlag.Normals);
          }
        }
        Mesh.normals = normals;
      }

      /// <summary>
      /// Updates used colours by copying mapped colours from <paramref name="builder"/>.
      /// </summary>
      /// <param name="builder">The owning builder object.</param>
      public void UpdateColours(MeshBuilder builder)
      {
        Color32[] colours = Mesh.colors32;
        int vind;
        foreach (var kvp in ToPartVertexMappings)
        {
          vind = kvp.Key;
          if (vind < builder._colours.Count)
          {
            colours[kvp.Value] = builder._colours[vind];
          }
          else
          {
            builder.LogRangeWarning(vind, builder._colours.Count, DirtyFlag.Colours);
          }
        }
        Mesh.colors32 = colours;
      }

      /// <summary>
      /// Updates used uvs by copying mapped uvs from <paramref name="builder"/>.
      /// </summary>
      /// <param name="builder">The owning builder object.</param>
      public void UpdateUVs(MeshBuilder builder)
      {
        Vector2[] uvs = Mesh.uv;
        int vind;
        foreach (var kvp in ToPartVertexMappings)
        {
          vind = kvp.Key;
          if (vind < builder._uvs.Count)
          {
            uvs[kvp.Value] = builder._uvs[vind];
          }
          else
          {
            builder.LogRangeWarning(vind, builder._uvs.Count, DirtyFlag.UVs);
          }
        }
        Mesh.uv = uvs;
      }

      /// <summary>
      /// Check if this part references the vertex at <paramref name="vertexIndex"/> in the source data.
      /// </summary>
      /// <param name="vertexIndex">The source data index to test.</param>
      /// <returns>True if referenced.</returns>
      public bool UsesVertex(int vertexIndex)
      {
        return UsesVertices(vertexIndex, 1);
      }

      /// <summary>
      /// Check if this part uses any vertex in the range given by <c>[firstVertexIndex, firstVertexIndex + count]</c>.
      /// </summary>
      /// <param name="firstVertexIndex">The first source data index to test.</param>
      /// <param name="count">The number of indices to test.</param>
      /// <returns>True if any vertex in the given range is referenced by this part.</returns>
      public bool UsesVertices(int firstVertexIndex, int count)
      {
        for (int i = firstVertexIndex; i < firstVertexIndex + count; ++i)
        {
          if (ToPartVertexMappings.ContainsKey(i))
          {
            return true;
          }
        }
        return false;
      }

      /// <summary>
      /// Check if this part references the index at <paramref name="indexIndex"/> in the source data.
      /// </summary>
      /// <param name="indexIndex">The source data index to test.</param>
      /// <returns>True if referenced.</returns>
      public bool UsesIndex(int indexIndex)
      {
        return UsesIndices(indexIndex, 1);
      }

      /// <summary>
      /// Check if this part uses any index in the range given by <c>[firstIndexIndex, firstIndexIndex + count]</c>.
      /// </summary>
      /// <param name="firstIndexIndex">The first source data index to test.</param>
      /// <param name="count">The number of indices to test.</param>
      /// <returns>True if any index in the given range is referenced by this part.</returns>
      public bool UsesIndices(int firstIndexIndex, int count)
      {
        // firstIndexIndex bounded by index range:
        if (FirstSourceIndex <= firstIndexIndex && firstIndexIndex < FirstSourceIndex + IndexCount)
        {
          return true;
        }

        if (count > 1)
        {
          // last index bounded:
          int lastIndexIndex = firstIndexIndex + count - 1;
          if (FirstSourceIndex <= lastIndexIndex && lastIndexIndex < FirstSourceIndex + IndexCount)
          {
            return true;
          }

          // FirstSourceIndex bounded by the range arguments.
          if (firstIndexIndex <= FirstSourceIndex && FirstSourceIndex < firstIndexIndex + count)
          {
            return true;
          }

          int lastSourceIndex = FirstSourceIndex + IndexCount - 1;
          // Last source index bounded:
          if (firstIndexIndex <= lastSourceIndex && lastSourceIndex < firstIndexIndex + count)
          {
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Create or update a single mesh object representation of the contained mesh data.
    /// </summary>
    /// <remarks>
    /// Should only be called when <c><see cref="IndexCount"/> &lt; <see cref="UnityIndexLimit"/></c>.
    /// Use <see cref="UpdateMultiMesh()"/> otherwise.
    /// </remarks>
    private void UpdateSingleMesh()
    {
      // Not needed.
      _meshDetails.Clear();

      Mesh mesh = null;
      DirtyFlag dirty = _dirty;
      if (_meshObjects.Count == 0)
      {
        // New mesh. Ensure we know everything is dirty.
        dirty = DirtyFlag.All;
        mesh = CreateMesh();
        _meshObjects.Add(mesh);
      }
      else
      {
        mesh = _meshObjects[0];
        if (_meshObjects.Count > 1)
        {
          _meshObjects.RemoveRange(1, _meshObjects.Count - 1);
          // Not a distinctly supported case. Dirty everything to be sure.
          dirty = DirtyFlag.All;
        }
      }

      if ((dirty & DirtyFlag.Vertices) != 0)
      {
        mesh.vertices = _vertices.ToArray();
      }

      if ((dirty & DirtyFlag.Indices) != 0)
      {
        if (_indices.Count > 0)
        {
          mesh.SetIndices(_indices.ToArray(), Topology, 0);
        }
        else
        {
          // Build sequential indexing.
          int[] indices = new int[VertexCount];
          for (int i = 0; i < indices.Length; ++i)
          {
            indices[i] = i;
          }
          mesh.SetIndices(indices, Topology, 0);
        }
      }

      if ((dirty & DirtyFlag.UVs) != 0)
      {
        if (_uvs.Count == VertexCount)
        {
          mesh.uv = _uvs.ToArray();
        }
      }

      if ((dirty & DirtyFlag.Colours) != 0)
      {
        if (_colours.Count == VertexCount)
        {
          mesh.colors32 = _colours.ToArray();
        }
      }

      if ((dirty & DirtyFlag.Normals) != 0)
      {
        if (_normals.Count == VertexCount)
        {
          mesh.normals = _normals.ToArray();
        }
        // Single uniform normal.
        else if (_normals.Count == 1)
        {
          Vector3[] normals = new Vector3[VertexCount];
          for (int i = 0; i < normals.Length; ++i)
          {
            normals[i] = _normals[0];
          }
          mesh.normals = normals;
        }
      }

      if (CalculateNormals && _normals.Count == 0 && (dirty & (DirtyFlag.Vertices | DirtyFlag.Indices)) != 0)
      {
        mesh.RecalculateNormals();
      }

      Bounds bounds = _bounds;
      bounds.Expand(BoundsPadding);
      mesh.bounds = bounds;
    }

    /// <summary>
    /// Create or update a multi mesh object representation of the contained mesh data.
    /// </summary>
    /// <remarks>
    /// Should only be called when <c><see cref="IndexCount"/> &gt;= <see cref="UnityIndexLimit"/></c>.
    /// Use <see cref="UpdateSingleMesh()"/> otherwise.
    /// </remarks>
    private void UpdateMultiMesh()
    {
      // Build meshes until all indices have been consumed.
      int currentIndexCursor = 0;
      int indexStep = TopologyIndexStep;
      int partIndex = 0;
      List<int> indices = new List<int>();
      List<Vector3> verts = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<Vector2> uvs = new List<Vector2>();
      List<Color32> colours = new List<Color32>();
      bool dirtyNextIndices = false;

      while (currentIndexCursor < IndexCount)
      {
        // Create or fetch a the next part.
        // New parts start all dirty, so we will know to do a full update.
        PartDetails part = GetPart(partIndex);

        // Previous part was reindexed. We have to re-build all following parts.
        if (dirtyNextIndices)
        {
          part.Dirty = DirtyFlag.Indices;
        }

        if (part.Dirty == DirtyFlag.None)
        {
          // Nothing to update in this part.
          currentIndexCursor += part.IndexCount;
          ++partIndex;
          continue;
        }

        // Note: if a part changes its indices, then it we must also rebuild its vertices.
        // This essentially means we rebuild the whole part.
        if ((part.Dirty & DirtyFlag.Indices) != 0)
        {
          // Dirty indices. All subsequent parts must be rebuild.
          dirtyNextIndices = true;
          indices.Clear();
          verts.Clear();
          normals.Clear();
          colours.Clear();
          uvs.Clear();

          part.FirstSourceIndex = currentIndexCursor;
          while (verts.Count + indexStep < UnityIndexLimit && currentIndexCursor < IndexCount)
          {
            part.Clear();

            for (int i = 0; i < indexStep && currentIndexCursor < IndexCount; ++i)
            {
              part.AddFromIndex(IndexAt(currentIndexCursor++),
                this, indices, verts, normals, colours, uvs);
            }
          }
          part.IndexCount = indices.Count;

          part.Mesh.vertices = verts.ToArray();
          if (indices.Count > 0)
          {
            part.Mesh.SetIndices(indices.ToArray(), Topology, 0);
          }
          if (normals.Count > 0)
          {
            part.Mesh.normals = normals.ToArray();
          }
          if (colours.Count > 0)
          {
            part.Mesh.colors32 = colours.ToArray();
          }
          if (uvs.Count > 0)
          {
            part.Mesh.uv = uvs.ToArray();
          }

          if (CalculateNormals && normals.Count == 0)
          {
            part.Mesh.RecalculateNormals();
          }
        }
        else
        {
          // Only update changed vertex data.
          if ((part.Dirty & DirtyFlag.Vertices) != 0 && _vertices.Count > 0)
          {
            part.UpdateVertices(this);
          }

          if ((part.Dirty & DirtyFlag.Normals) != 0 && _normals.Count > 0)
          {
            part.UpdateNormals(this);
          }

          if ((part.Dirty & DirtyFlag.Colours) != 0 && _colours.Count > 0)
          {
            part.UpdateColours(this);
          }

          if ((part.Dirty & DirtyFlag.UVs) != 0 && _uvs.Count > 0)
          {
            part.UpdateUVs(this);
          }

          currentIndexCursor += part.IndexCount;

          // When dealing with loop topology, we have to rewind the index cursor if not done.
          if (currentIndexCursor < _indices.Count)
          {
            switch (Topology)
            {
              case MeshTopology.LineStrip:
                --currentIndexCursor;
                break;
              default:
                break;
            }
          }
        }

        part.Dirty = DirtyFlag.None;

        ++partIndex;
      }

      // Erase obsolete parts.
      while (partIndex < _meshObjects.Count)
      {
        _meshObjects.RemoveAt(_meshObjects.Count - 1);
      }

      // Update mesh list.
      while (_meshObjects.Count > _meshDetails.Count)
      {
        _meshObjects.RemoveAt(_meshObjects.Count - 1);
      }

      while (_meshObjects.Count < _meshDetails.Count)
      {
        _meshObjects.Add(null);
      }

      for (int i = 0; i < _meshDetails.Count; ++i)
      {
        _meshObjects[i] = _meshDetails[i].Mesh;
      }
    }

    /// <summary>
    /// Initialise a new unity <c>Mesh</c> object for use by this builder.
    /// </summary>
    /// <returns></returns>
    private Mesh CreateMesh()
    {
      Mesh mesh = new Mesh();
      mesh.subMeshCount = 1;
      return mesh;
    }

    /// <summary>
    /// Create or acces an existing part at <paramref name="partIndex"/>
    /// </summary>
    /// <param name="partIndex">The index of the part of interest.</param>
    /// <returns>The requested part.</returns>
    private PartDetails GetPart(int partIndex)
    {
      Mesh mesh = null;
      while (_meshDetails.Count <= partIndex)
      {
        mesh = CreateMesh();
        _meshDetails.Add(new PartDetails() { Mesh = mesh });
      }

      return _meshDetails[partIndex];
    }

    /// <summary>
    /// Log a warning about list indexing being out of range.
    /// </summary>
    /// <param name="index">The out of range index.</param>
    /// <param name="rangeCount">The container's actual element count.</param>
    /// <param name="context">Identifies the list which was indexed out of range.</param>
    /// <remarks>
    /// Sets an error dirty flag to prevent log span. The flag is reset every time meshes are requested.
    /// </remarks>
    internal void LogRangeWarning(int index, int rangeCount, DirtyFlag context)
    {
      if ((_warningFlags & context) == 0)
      {
        _warningFlags |= context;
        Debug.LogWarning(
            string.Format("Mesh {0} index out of range: {1} : [0, {2})",
              context.ToString(), index, rangeCount)
          );
      }
    }

    private Bounds _bounds = new Bounds();
    internal List<Vector3> _vertices = new List<Vector3>();
    internal List<int> _indices = new List<int>();
    internal List<Vector3> _normals = new List<Vector3>();
    internal List<Vector2> _uvs = new List<Vector2>();
    internal List<Color32> _colours = new List<Color32>();
    private MeshTopology _topology = MeshTopology.Points;
    private List<Mesh> _meshObjects = new List<Mesh>();
    /// <summary>
    /// Details of each mesh part when multiple parts are required. Empty when a single mesh is sufficient.
    /// </summary>
    private List<PartDetails> _meshDetails = new List<PartDetails>();
    private DirtyFlag _dirty = DirtyFlag.None;
    private DirtyFlag _warningFlags = DirtyFlag.None;
  }
}
