using System;
using System.Collections.Generic;
using Tes.Buffers;
using Tes.Net;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// A collection of compute and graphics buffers representing a renderable mesh
  /// </summary>
  public class RenderMesh
  {
    public MeshDrawType DrawType
    {
      get { return _drawType; }
      set { _drawType = value; }
    }

    public MeshTopology Topology
    {
      get
      {
        switch (DrawType)
        {
          case MeshDrawType.Points:
          // No break.
          case MeshDrawType.Voxels:
            return MeshTopology.Points;
          case MeshDrawType.Lines:
            return MeshTopology.Lines;
          case MeshDrawType.Triangles:
            return MeshTopology.Triangles;
          default:
            break;
        }
        // Programmatic error if this happens.
        throw new System.Exception("Unsupported draw type");
      }
    }

    private Matrix4x4 _localTransform = Matrix4x4.identity;
    public Matrix4x4 LocalTransform
    {
      get { return _localTransform; }
      set { _localTransform = value; }
    }

    private Tes.Maths.Colour _tint = new Tes.Maths.Colour(255, 255, 255);
    public Tes.Maths.Colour Tint
    {
      get { return _tint; }
      set { _tint = value; }
    }

    public Bounds Bounds
    {
      get
      {
        return new Bounds(0.5f * (_maxBounds + _minBounds), 0.5f * (_maxBounds - _minBounds) + _boundsPadding);
      }
    }

    public Vector3 MinBounds
    {
      get { return _minBounds; }
      set
      {
        if (!_boundsSet)
        {
          _maxBounds = value;
        }
        _minBounds = value;
        _boundsSet = true;
      }
    }

    public Vector3 MaxBounds
    {
      get { return _maxBounds; }
      set
      {
        if (!_boundsSet)
        {
          _minBounds = value;
        }
        _maxBounds = value;
        _boundsSet = true;
      }
    }

    public Vector3 BoundsPadding
    {
      get { return _boundsPadding; }
      set { _boundsPadding = value; }
    }

    public bool BoundsSet
    {
      get { return _boundsSet; }
      set { _boundsSet = value; }
    }

    public int IndexCount
    {
      get { return _indexCount; }
    }

    public int VertexCount
    {
      get { return _vertexCount; }
    }

    public bool HasNormals { get { return _normals != null; } }
    public bool HasColours { get { return _colours != null; } }
    public bool HasUVs { get { return _uvs != null; } }

    public GraphicsBuffer IndexBuffer
    {
      get
      {
        if (IndexCount == 0)
        {
          return null;
        }

        if (_indexBuffer == null)
        {
          _indexBuffer = GpuBufferManager.Instance.AllocateIndexBuffer(IndexCount);
          _indicesDirty = true;
        }

        if (_indicesDirty)
        {
          _indexBuffer.SetData(_indices);
          _indicesDirty = false;
        }

        return _indexBuffer;
      }
    }

    public ComputeBuffer VertexBuffer
    {
      get
      {
        if (VertexCount == 0)
        {
          return null;
        }

        if (_vertexBuffer == null)
        {
          _vertexBuffer = GpuBufferManager.Instance.AllocateVertexBuffer(VertexCount);
          _verticesDirty = true;
        }

        if (_verticesDirty)
        {
          _vertexBuffer.SetData(_vertices);
          _verticesDirty = false;
        }

        return _vertexBuffer;
      }
    }

    public ComputeBuffer NormalsBuffer
    {
      get
      {
        if (VertexCount == 0)
        {
          return null;
        }

        if (_normalsBuffer == null)
        {
          _normalsBuffer = GpuBufferManager.Instance.AllocateNormalsBuffer(VertexCount);
          _normalsDirty = true;
        }

        if (_normalsDirty)
        {
          _normalsBuffer.SetData(_normals);
          _normalsDirty = false;
        }

        return _normalsBuffer;
      }
    }

    public ComputeBuffer ColoursBuffer
    {
      get
      {
        if (VertexCount == 0)
        {
          return null;
        }

        if (_coloursBuffer == null)
        {
          _coloursBuffer = GpuBufferManager.Instance.AllocateColoursUIntBuffer(VertexCount);
          _coloursDirty = true;
        }

        if (_coloursDirty)
        {
          _coloursBuffer.SetData(_colours);
          _coloursDirty = false;
        }

        return _coloursBuffer;
      }
    }

    public ComputeBuffer UvsBuffer
    {
      get
      {
        if (VertexCount == 0)
        {
          return null;
        }

        if (_uvsBuffer == null)
        {
          _uvsBuffer = GpuBufferManager.Instance.AllocateUVsBuffer(VertexCount);
          _uvsDirty = true;
        }

        if (_uvsDirty)
        {
          _uvsBuffer.SetData(_uvs);
          _uvsDirty = false;
        }

        return _uvsBuffer;
      }
    }

    private Material _material = null;
    public Material Material
    {
      get { return _material; }
      set { _material = value; _materialDirty = true; }
    }

    private bool _materialDirty = false;
    public bool MaterialDirty
    {
      get { return _materialDirty; }
    }

    public RenderMesh() { }

    public RenderMesh(MeshDrawType drawType, int vertexCount, int indexCount = 0)
    {
      SetVertexCount(vertexCount);
      SetIndexCount(indexCount);
      _drawType = drawType;
    }

    ~RenderMesh()
    {
      ReleaseBuffers();
    }

    public void ReleaseBuffers()
    {
      if (_indexBuffer != null)
      {
        GpuBufferManager.Instance.ReleaseIndexBuffer(_indexBuffer);
        _indexBuffer = null;
      }
      if (_vertexBuffer != null)
      {
        GpuBufferManager.Instance.ReleaseVertexBuffer(_vertexBuffer);
        _vertexBuffer = null;
      }
      if (_normalsBuffer != null)
      {
        GpuBufferManager.Instance.ReleaseNormalsBuffer(_normalsBuffer);
        _normalsBuffer = null;
      }
      if (_coloursBuffer != null)
      {
        GpuBufferManager.Instance.ReleaseColoursUIntBuffer(_coloursBuffer);
        _coloursBuffer = null;
      }
      if (_uvsBuffer != null)
      {
        GpuBufferManager.Instance.ReleaseUVsBuffer(_uvsBuffer);
        _uvsBuffer = null;
      }
    }

    public void UpdateMaterial()
    {
      if (_material != null)
      {
        if (HasColours)
        {
          _material.EnableKeyword("WITH_COLOURS_UINT");
        }

        if (HasNormals)
        {
          _material.EnableKeyword("WITH_NORMALS");
        }

        if (HasUVs)
        {
          _material.EnableKeyword("WITH_UVS");
        }

        if (_material.HasProperty("_Tint"))
        {
          _material.SetColor("_Tint", Maths.ColourExt.ToUnity(Tint));
        }

        _materialDirty = false;
      }
    }

    public void SetIndexCount(int indexCount)
    {
      _indexCount = indexCount;
      ValidateBufferSizes();
    }

    public void SetVertexCount(int vertexCount)
    {
      _vertexCount = vertexCount;
      ValidateBufferSizes();
    }

    public void SetIndices(List<int> indices)
    {
      Debug.Assert(_indices != null && indices.Count <= _indices.Length);
      _indicesDirty = true;
      for (int i = 0; i < indices.Count; ++i)
      {
        _indices[i] = indices[i];
      }
    }

    public void SetIndices(int[] indices)
    {
      Debug.Assert(_indices != null && indices.Length <= _indices.Length);
      _indicesDirty = true;
      Array.Copy(indices, 0, _indices, 0, indices.Length);
    }

    public void SetIndices(List<int> indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 && _indices != null &&
                   0 <= listStartIndex && listStartIndex < indices.Count &&
                   listStartIndex + count <= indices.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _indicesDirty = true;
      for (int i = 0; i < count; ++i)
      {
        _indices[bufferStartIndex + i] = indices[listStartIndex + i];
      }
    }

    public void SetIndices(int[] indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 && _indices != null &&
                   0 <= listStartIndex && listStartIndex < indices.Length &&
                   listStartIndex + count <= indices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _indicesDirty = true;
      Array.Copy(indices, listStartIndex, _indices, bufferStartIndex, count);
    }

    public void SetIndices(VertexBuffer buffer, int offset)
    {
      Debug.Assert(0 <= offset && offset + buffer.Count <= IndexCount && buffer.ComponentCount == 1);

      for (int i = 0; i < buffer.Count; ++i)
      {
        _indices[offset + i] = buffer.GetInt32(i);
      }
      _indicesDirty = true;
    }

    public int[] Indices { get { return _indices; } }

    public void SetVertices(List<Vector3> vertices, bool adjustBounds = false)
    {
      Debug.Assert(vertices.Count <= VertexCount);
      if (adjustBounds)
      {
        _minBounds = _maxBounds = (vertices.Count > 0) ? vertices[0] : Vector3.zero;
        for (int i = 0; i < vertices.Count; ++i)
        {
          _minBounds.x = Mathf.Min(vertices[i].x, _minBounds.x);
          _minBounds.y = Mathf.Min(vertices[i].y, _minBounds.y);
          _minBounds.z = Mathf.Min(vertices[i].z, _minBounds.z);
          _maxBounds.x = Mathf.Max(vertices[i].x, _maxBounds.x);
          _maxBounds.y = Mathf.Max(vertices[i].y, _maxBounds.y);
          _maxBounds.z = Mathf.Max(vertices[i].z, _maxBounds.z);
        }
      }
      for (int i = 0; i < vertices.Count; ++i)
      {
        _vertices[i] = vertices[i];
      }
      _verticesDirty = true;
    }

    public void SetVertices(Vector3[] vertices, bool adjustBounds)
    {
      Debug.Assert(vertices.Length <= VertexCount);
      if (adjustBounds)
      {
        _minBounds = _maxBounds = (vertices.Length > 0) ? vertices[0] : Vector3.zero;
        for (int i = 0; i < vertices.Length; ++i)
        {
          _minBounds.x = Mathf.Min(vertices[i].x, _minBounds.x);
          _minBounds.y = Mathf.Min(vertices[i].y, _minBounds.y);
          _minBounds.z = Mathf.Min(vertices[i].z, _minBounds.z);
          _maxBounds.x = Mathf.Max(vertices[i].x, _maxBounds.x);
          _maxBounds.y = Mathf.Max(vertices[i].y, _maxBounds.y);
          _maxBounds.z = Mathf.Max(vertices[i].z, _maxBounds.z);
        }
      }
      Array.Copy(vertices, _vertices, vertices.Length);
      _verticesDirty = true;
    }

    public void SetVertices(List<Vector3> vertices, int listStartIndex, int bufferStartIndex, int count,
                            bool adjustBounds = false)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < vertices.Count &&
                   listStartIndex + count <= vertices.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (adjustBounds)
      {
        if (bufferStartIndex == 0)
        {
          _minBounds = _maxBounds = (count > 0) ? vertices[listStartIndex] : Vector3.zero;
        }
        for (int i = listStartIndex; i < listStartIndex + count; ++i)
        {
          _minBounds.x = Mathf.Min(vertices[i].x, _minBounds.x);
          _minBounds.y = Mathf.Min(vertices[i].y, _minBounds.y);
          _minBounds.z = Mathf.Min(vertices[i].z, _minBounds.z);
          _maxBounds.x = Mathf.Max(vertices[i].x, _maxBounds.x);
          _maxBounds.y = Mathf.Max(vertices[i].y, _maxBounds.y);
          _maxBounds.z = Mathf.Max(vertices[i].z, _maxBounds.z);
        }
      }
      for (int i = 0; i < count; ++i)
      {
        _vertices[bufferStartIndex + i] = vertices[listStartIndex + i];
      }
      _verticesDirty = true;
    }

    public void SetVertices(Vector3[] vertices, int listStartIndex, int bufferStartIndex, int count,
                            bool adjustBounds = false)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < vertices.Length &&
                   listStartIndex + count <= vertices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (adjustBounds)
      {
        if (bufferStartIndex == 0)
        {
          _minBounds = _maxBounds = (count > 0) ? vertices[listStartIndex] : Vector3.zero;
        }
        for (int i = listStartIndex; i < listStartIndex + count; ++i)
        {
          _minBounds.x = Mathf.Min(vertices[i].x, _minBounds.x);
          _minBounds.y = Mathf.Min(vertices[i].y, _minBounds.y);
          _minBounds.z = Mathf.Min(vertices[i].z, _minBounds.z);
          _maxBounds.x = Mathf.Max(vertices[i].x, _maxBounds.x);
          _maxBounds.y = Mathf.Max(vertices[i].y, _maxBounds.y);
          _maxBounds.z = Mathf.Max(vertices[i].z, _maxBounds.z);
        }
      }
      Array.Copy(vertices, listStartIndex, _vertices, bufferStartIndex, count);
      _verticesDirty = true;
    }

    public void SetVertices(VertexBuffer buffer, int offset, bool adjustBounds = false)
    {
      Debug.Assert(0 <= offset && offset + buffer.Count <= VertexCount && buffer.ComponentCount == 3);
      Vector3 v = new Vector3();
      for (int i = 0; i < buffer.Count; ++i)
      {
        v.x = buffer.GetSingle(i * 3 + 0);
        v.y = buffer.GetSingle(i * 3 + 1);
        v.z = buffer.GetSingle(i * 3 + 2);
        _vertices[offset + i] = v;

        if (adjustBounds)
        {
          if (i + offset > 0)
          {
            _minBounds.x = Mathf.Min(v.x, _minBounds.x);
            _minBounds.y = Mathf.Min(v.y, _minBounds.y);
            _minBounds.z = Mathf.Min(v.z, _minBounds.z);
            _maxBounds.x = Mathf.Max(v.x, _maxBounds.x);
            _maxBounds.y = Mathf.Max(v.y, _maxBounds.y);
            _maxBounds.z = Mathf.Max(v.z, _maxBounds.z);
          }
          else
          {
            _minBounds = _maxBounds = v;
          }
        }
      }
      _verticesDirty = true;
    }

    public Vector3[] Vertices { get { return _vertices; } }

    public void SetUniformNormal(Vector3 normal)
    {
      if (_normals == null)
      {
        _normals = new Vector3[VertexCount];
      }

      for (int i = 0; i < _normals.Length; ++i)
      {
        _normals[i] = normal;
      }
      _normalsDirty = true;
    }

    public void SetNormals(List<Vector3> normals)
    {
      Debug.Assert(normals.Count <= VertexCount);
      if (_normals == null)
      {
        _normals = new Vector3[VertexCount];
      }

      for (int i = 0; i < normals.Count; ++i)
      {
        _normals[i] = normals[i];
      }
      _normalsDirty = true;
    }

    public void SetNormals(Vector3[] normals)
    {
      Debug.Assert(normals.Length <= VertexCount);
      if (_normals == null)
      {
        _normals = new Vector3[VertexCount];
      }

      Array.Copy(normals, _normals, normals.Length);
      _normalsDirty = true;
    }

    public void SetNormals(List<Vector3> normals, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < normals.Count &&
                   listStartIndex + count <= normals.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (_normals == null)
      {
        _normals = new Vector3[VertexCount];
      }

      for (int i = 0; i < count; ++i)
      {
        _normals[bufferStartIndex + i] = normals[listStartIndex + i];
      }
      _normalsDirty = true;
    }

    public void SetNormals(Vector3[] normals, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < normals.Length &&
                   listStartIndex + count <= normals.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (_normals == null)
      {
        _normals = new Vector3[VertexCount];
      }

      Array.Copy(normals, listStartIndex, _normals, bufferStartIndex, count);
      _normalsDirty = true;
    }

    public void SetNormals(VertexBuffer buffer, int offset)
    {
      if (_normals == null)
      {
        _normals = new Vector3[VertexCount];
      }
      Debug.Assert(0 <= offset && offset + buffer.Count <= VertexCount && buffer.ComponentCount == 3);

      Vector3 n = new Vector3();
      for (int i = 0; i < buffer.Count; ++i)
      {
        n.x = buffer.GetSingle(i * 3 + 0);
        n.y = buffer.GetSingle(i * 3 + 1);
        n.z = buffer.GetSingle(i * 3 + 2);
        _normals[offset + i] = n;
      }
      _normalsDirty = true;
    }

    public Vector3[] Normals { get { return _normals; } }

    public void SetColours(List<uint> colours)
    {
      Debug.Assert(colours.Count <= VertexCount);
      if (_colours == null)
      {
        _colours = new uint[VertexCount];
      }

      for (int i = 0; i < colours.Count; ++i)
      {
        _colours[i] = colours[i];
      }
      _coloursDirty = true;
    }

    public void SetColours(uint[] colours)
    {
      Debug.Assert(colours.Length <= VertexCount);
      if (_colours == null)
      {
        _colours = new uint[VertexCount];
      }

      Array.Copy(colours, _colours, colours.Length);
      _coloursDirty = true;
    }

    public void SetColours(List<uint> colours, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < colours.Count &&
                   listStartIndex + count <= colours.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (_colours == null)
      {
        _colours = new uint[VertexCount];
      }

      for (int i = 0; i < count; ++i)
      {
        _colours[bufferStartIndex + i] = colours[listStartIndex + i];
      }
      _coloursDirty = true;
    }

    public void SetColours(uint[] colours, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < colours.Length &&
                   listStartIndex + count <= colours.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (_colours == null)
      {
        _colours = new uint[VertexCount];
      }

      Array.Copy(colours, listStartIndex, _colours, bufferStartIndex, count);
      _coloursDirty = true;
    }

    public void SetColours(VertexBuffer buffer, int offset)
    {
      if (_colours == null)
      {
        _colours = new uint[VertexCount];
      }
      Debug.Assert(0 <= offset && offset + buffer.Count <= VertexCount && buffer.ComponentCount == 1);

      for (int i = 0; i < buffer.Count; ++i)
      {
        _colours[offset + i] = buffer.GetUInt32(i);
      }
      _coloursDirty = true;
    }

    public uint[] Colours { get { return _colours; } }

    public void SetUVs(List<Vector2> uvs)
    {
      Debug.Assert(uvs.Count <= VertexCount);
      if (_uvs == null)
      {
        _uvs = new Vector2[VertexCount];
      }

      for (int i = 0; i < uvs.Count; ++i)
      {
        _uvs[i] = uvs[i];
      }
      _uvsDirty = true;
    }

    public void SetUVs(Vector2[] uvs)
    {
      Debug.Assert(uvs.Length <= VertexCount);
      if (_uvs == null)
      {
        _uvs = new Vector2[VertexCount];
      }

      Array.Copy(uvs, _uvs, uvs.Length);
      _uvsDirty = true;
    }

    public void SetUVs(List<Vector2> uvs, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < uvs.Count &&
                   listStartIndex + count <= uvs.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (_uvs == null)
      {
        _uvs = new Vector2[VertexCount];
      }

      for (int i = 0; i < count; ++i)
      {
        _uvs[bufferStartIndex + i] = uvs[listStartIndex + i];
      }
      _uvsDirty = true;
    }

    public void SetUVs(Vector2[] uvs, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < uvs.Length &&
                   listStartIndex + count <= uvs.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      if (_uvs == null)
      {
        _uvs = new Vector2[VertexCount];
      }

      Array.Copy(uvs, listStartIndex, _uvs, bufferStartIndex, count);
      _uvsDirty = true;
    }

    public void SetUVs(VertexBuffer buffer, int offset)
    {
      if (_uvs == null)
      {
        _uvs = new Vector2[VertexCount];
      }
      Debug.Assert(0 <= offset && offset + buffer.Count <= VertexCount && buffer.ComponentCount == 2);

      Vector2 uv = new Vector2();
      for (int i = 0; i < buffer.Count; ++i)
      {
        uv.x = buffer.GetSingle(i * 2 + 0);
        uv.y = buffer.GetSingle(i * 2 + 1);
        _uvs[offset + i] = uv;
      }
      _uvsDirty = true;
    }

    public Vector2[] UVs { get { return _uvs; } }

    public void CalculateNormals()
    {
      if (DrawType == MeshDrawType.Triangles)
      {
        if (_normals == null)
        {
          _normals = new Vector3[VertexCount];
        }

        // Handle explicit and implied indexing.
        int[] indexArray = _indices;

        if (IndexCount == 0)
        {
          // Implicit indexing.
          indexArray = new int[VertexCount];
          for (int i = 0; i < indexArray.Length; ++i)
          {
            indexArray[i] = i;
          }
        }

        // Accumulate per face normals in the vertices.
        int faceStride = 3;
        Vector3 edgeA = Vector3.zero;
        Vector3 edgeB = Vector3.zero;
        Vector3 partNormal = Vector3.zero;
        for (int i = 0; i < indexArray.Length; i += faceStride)
        {
          // Generate a normal for the current face.
          edgeA = _vertices[indexArray[i + 1]] - _vertices[indexArray[i + 0]];
          edgeB = _vertices[indexArray[i + 2]] - _vertices[indexArray[i + 1]];
          partNormal = Vector3.Cross(edgeB, edgeA);
          for (int v = 0; v < faceStride; ++v)
          {
            _normals[indexArray[i + v]] += partNormal;
          }
        }

        // Normalise all the part normals.
        for (int i = 0; i < _normals.Length; ++i)
        {
          _normals[i] = _normals[i].normalized;
        }

        _normalsDirty = true;
      }
    }

    /// <summary>
    /// To be called prior to rendering to ensure the correct material state.
    /// </summary>
    public void PreRender()
    {
      if (_materialDirty && _material != null)
      {
        if (HasColours)
        {
          _material.EnableKeyword("WITH_COLOUR");
        }
        if (HasNormals)
        {
          _material.EnableKeyword("WITH_NORMALS");
        }
        if (HasUVs)
        {
          _material.EnableKeyword("WITH_UVS");
        }

        _materialDirty = true;
      }
    }

    private void ValidateBufferSizes()
    {
      if (_indexBuffer != null && _indexBuffer.count < _indexCount)
      {
        GpuBufferManager.Instance.ReleaseIndexBuffer(_indexBuffer);
        _indexBuffer = null;
        _indicesDirty = true;
      }
      if (_indices == null || _indices.Length != _indexCount)
      {
        int[] indices = null;
        if (_indexCount > 0)
        {
          indices = new int[_indexCount];
          if (_indices != null)
          {
            Array.Copy(_indices, indices, _indices.Length);
          }
          _indices = indices;
        }
      }

      if (_vertexBuffer != null && _vertexBuffer.count < _vertexCount)
      {
        GpuBufferManager.Instance.ReleaseVertexBuffer(_vertexBuffer);
        _vertexBuffer = null;
        _verticesDirty = true;
      }
      if (_vertices == null || _vertices.Length != _vertexCount)
      {
        Vector3[] vertices = null;
        if (_vertexCount > 0)
        {
          vertices = new Vector3[_vertexCount];
          if (_vertices != null)
          {
            Array.Copy(_vertices, vertices, _vertices.Length);
          }
          _vertices = vertices;
        }
      }

      if (_normalsBuffer != null && _normalsBuffer.count < _vertexCount)
      {
        GpuBufferManager.Instance.ReleaseNormalsBuffer(_normalsBuffer);
        _normalsBuffer = null;
        _normalsDirty = true;
      }
      if (_normals != null && _normals.Length != _vertexCount)
      {
        Vector3[] normals = null;
        if (_vertexCount > 0)
        {
          normals = new Vector3[_vertexCount];
          Array.Copy(_normals, normals, _normals.Length);
          _normals = normals;
        }
      }

      if (_coloursBuffer != null && _coloursBuffer.count < _vertexCount)
      {
        GpuBufferManager.Instance.ReleaseColoursUIntBuffer(_coloursBuffer);
        _coloursBuffer = null;
        _coloursDirty = true;
      }
      if (_colours != null && _colours.Length != _vertexCount)
      {
        uint[] colours = null;
        if (_vertexCount > 0)
        {
          colours = new uint[_vertexCount];
          Array.Copy(_colours, colours, _colours.Length);
          _colours = colours;
        }
      }

      if (_uvsBuffer != null && _uvsBuffer.count < _vertexCount)
      {
        GpuBufferManager.Instance.ReleaseUVsBuffer(_uvsBuffer);
        _uvsBuffer = null;
        _uvsDirty = true;
      }
      if (_uvs != null && _uvs.Length != _vertexCount)
      {
        Vector2[] uvs = null;
        if (_vertexCount > 0)
        {
          uvs = new Vector2[_vertexCount];
          Array.Copy(_uvs, uvs, _uvs.Length);
          _uvs = uvs;
        }
      }
    }

    // Can't read indices back from the GraphicsBuffer, so we have to have a cache here.
    private int[] _indices = null;
    private Vector3[] _vertices = null;
    private Vector3[] _normals = null;
    private uint[] _colours = null;
    private Vector2[] _uvs = null;
    private GraphicsBuffer _indexBuffer = null;
    private ComputeBuffer _vertexBuffer = null;
    private ComputeBuffer _normalsBuffer = null;
    private ComputeBuffer _coloursBuffer = null;
    private ComputeBuffer _uvsBuffer = null;
    private bool _indicesDirty = true;
    private bool _verticesDirty = true;
    private bool _normalsDirty = true;
    private bool _coloursDirty = true;
    private bool _uvsDirty = true;
    private Vector3 _minBounds = Vector3.zero;
    private Vector3 _maxBounds = Vector3.zero;
    private Vector3 _boundsPadding = Vector3.zero;
    private bool _boundsSet = false;
    private int _vertexCount = 0;
    private int _indexCount = 0;
    private MeshDrawType _drawType = MeshDrawType.Triangles;
  }
}