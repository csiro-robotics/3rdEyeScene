using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    public bool HasNormals { get { return _normalsBuffer != null; } }
    public bool HasColours { get { return _coloursBuffer != null; } }
    public bool HasUVs { get { return _uvsBuffer != null; } }

    public GraphicsBuffer IndexBuffer { get { return _indexBuffer; } }
    public ComputeBuffer VertexBuffer { get { return _vertexBuffer; } }
    public ComputeBuffer NormalsBuffer { get { return _normalsBuffer; } }
    public ComputeBuffer ColoursBuffer { get { return _coloursBuffer; } }
    public ComputeBuffer UvsBuffer { get { return _uvsBuffer; } }

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

    public RenderMesh() {}

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
        _indexBuffer.Release();
        _indexBuffer = null;
      }
      _indices = null;
      if (_vertexBuffer != null)
      {
        _vertexBuffer.Release();
        _vertexBuffer = null;
      }
      if (_normalsBuffer != null)
      {
        _normalsBuffer.Release();
        _normalsBuffer = null;
      }
      if (_coloursBuffer != null)
      {
        _coloursBuffer.Release();
        _coloursBuffer = null;
      }
      if (_uvsBuffer != null)
      {
        _uvsBuffer.Release();
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
      CreateIndexBuffer();
    }

    public void SetVertexCount(int vertexCount)
    {
      _vertexCount = vertexCount;
      CreateVertexBuffer();
      if (HasNormals)
      {
        CreateColoursBuffer();
      }
      if (HasColours)
      {
        CreateColoursBuffer();
      }
      if (HasUVs)
      {
        CreateUVsBuffer();
      }
    }

    public void SetIndices(List<int> indices)
    {
      Debug.Assert(_indices != null && indices.Count <= _indices.Length);
      for (int i = 0; i < indices.Count; ++i)
      {
        _indices[i] = indices[i];
      }
      _indexBuffer.SetData(_indices);
    }

    public void SetIndices(int[] indices)
    {
      Debug.Assert(_indices != null && indices.Length <= _indices.Length);
      Array.Copy(indices, 0, _indices, 0, indices.Length);
      _indexBuffer.SetData(indices);
    }

    public void SetIndices(List<int> indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 && _indices != null &&
                   0 <= listStartIndex && listStartIndex < indices.Count &&
                   listStartIndex + count <= indices.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      for (int i = 0; i < count; ++i)
      {
        _indices[bufferStartIndex + i] = indices[listStartIndex + i];
      }
      _indexBuffer.SetData(_indices, bufferStartIndex, bufferStartIndex, count);
    }

    public void SetIndices(int[] indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 && _indices != null &&
                   0 <= listStartIndex && listStartIndex < indices.Length &&
                   listStartIndex + count <= indices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      Array.Copy(indices, listStartIndex, _indices, bufferStartIndex, count);
      _indexBuffer.SetData(_indices, bufferStartIndex, bufferStartIndex, count);
    }

    public void GetIndices(int[] indices)
    {
      Debug.Assert(_indices != null && indices.Length <= IndexCount);
      Array.Copy(_indices, 0, indices, 0, indices.Length);
    }

    public void GetIndices(int[] indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 && _indices != null &&
                   0 <= listStartIndex && listStartIndex < indices.Length &&
                   listStartIndex + count <= indices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      Array.Copy(_indices, bufferStartIndex, indices, listStartIndex, count);
    }

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
      _vertexBuffer.SetData(vertices);
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
      _vertexBuffer.SetData(vertices);
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
          _minBounds = _maxBounds = (vertices.Count > 0) ? vertices[0] : Vector3.zero;
        }
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
      _vertexBuffer.SetData(vertices, listStartIndex, bufferStartIndex, count);
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
          _minBounds = _maxBounds = (vertices.Length > 0) ? vertices[0] : Vector3.zero;
        }
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
      _vertexBuffer.SetData(vertices, listStartIndex, bufferStartIndex, count);
    }

    public void GetVertices(Vector3[] vertices)
    {
      Debug.Assert(vertices.Length <= VertexCount);
      _vertexBuffer.GetData(vertices);
    }

    public void GetVertices(Vector3[] vertices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < vertices.Length &&
                   listStartIndex + count <= vertices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _vertexBuffer.GetData(vertices, listStartIndex, bufferStartIndex, count);
    }

    public void SetNormals(List<Vector3> normals)
    {
      Debug.Assert(normals.Count <= VertexCount);
      CreateNormalsBuffer();
      _normalsBuffer.SetData(normals);
    }

    public void SetNormals(Vector3[] normals)
    {
      Debug.Assert(normals.Length <= VertexCount);
      CreateNormalsBuffer();
      _normalsBuffer.SetData(normals);
    }

    public void SetNormals(List<Vector3> normals, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < normals.Count &&
                   listStartIndex + count <= normals.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      CreateNormalsBuffer();
      _normalsBuffer.SetData(normals, listStartIndex, bufferStartIndex, count);
    }

    public void SetNormals(Vector3[] normals, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < normals.Length &&
                   listStartIndex + count <= normals.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      CreateNormalsBuffer();
      _normalsBuffer.SetData(normals, listStartIndex, bufferStartIndex, count);
    }

    public void GetNormals(Vector3[] normals)
    {
      Debug.Assert(_normalsBuffer != null && normals.Length <= VertexCount);
      _normalsBuffer.GetData(normals);
    }

    public void GetNormals(Vector3[] normals, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(_normalsBuffer != null && count >= 0 &&
                   0 <= listStartIndex && listStartIndex < normals.Length &&
                   listStartIndex + count <= normals.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _normalsBuffer.GetData(normals, listStartIndex, bufferStartIndex, count);
    }

    public void SetColours(List<uint> colours)
    {
      Debug.Assert(colours.Count <= VertexCount);
      CreateColoursBuffer();
      _coloursBuffer.SetData(colours);
    }

    public void SetColours(uint[] colours)
    {
      Debug.Assert(colours.Length <= VertexCount);
      CreateColoursBuffer();
      _coloursBuffer.SetData(colours);
    }

    public void SetColours(List<uint> colours, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < colours.Count &&
                   listStartIndex + count <= colours.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      CreateColoursBuffer();
      _coloursBuffer.SetData(colours, listStartIndex, bufferStartIndex, count);
    }

    public void SetColours(uint[] colours, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < colours.Length &&
                   listStartIndex + count <= colours.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      CreateColoursBuffer();
      _coloursBuffer.SetData(colours, listStartIndex, bufferStartIndex, count);
    }

    public void GetColours(uint[] colours)
    {
      Debug.Assert(_normalsBuffer != null && colours.Length <= VertexCount);
      _coloursBuffer.GetData(colours);
    }

    public void GetColours(uint[] colours, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(_normalsBuffer != null && count >= 0 &&
                   0 <= listStartIndex && listStartIndex < colours.Length &&
                   listStartIndex + count <= colours.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _coloursBuffer.GetData(colours, listStartIndex, bufferStartIndex, count);
    }

    public void SetUVs(List<uint> uvs)
    {
      Debug.Assert(uvs.Count <= VertexCount);
      CreateUVsBuffer();
      _uvsBuffer.SetData(uvs);
    }

    public void SetUVs(Vector2[] uvs)
    {
      Debug.Assert(uvs.Length <= VertexCount);
      CreateUVsBuffer();
      _uvsBuffer.SetData(uvs);
    }

    public void SetUVs(List<Vector2> uvs, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < uvs.Count &&
                   listStartIndex + count <= uvs.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      CreateUVsBuffer();
      _uvsBuffer.SetData(uvs, listStartIndex, bufferStartIndex, count);
    }

    public void SetUVs(Vector2[] uvs, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < uvs.Length &&
                   listStartIndex + count <= uvs.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      CreateUVsBuffer();
      _uvsBuffer.SetData(uvs, listStartIndex, bufferStartIndex, count);
    }

    public void GetUVs(Vector2[] uvs)
    {
      Debug.Assert(_normalsBuffer != null && uvs.Length <= VertexCount);
      _uvsBuffer.GetData(uvs);
    }

    public void GetUVs(Vector2[] uvs, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(_normalsBuffer != null && count >= 0 &&
                   0 <= listStartIndex && listStartIndex < uvs.Length &&
                   listStartIndex + count <= uvs.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _uvsBuffer.GetData(uvs, listStartIndex, bufferStartIndex, count);
    }

    public void CalculateNormals()
    {
      if (DrawType == MeshDrawType.Triangles)
      {
        Vector3[] vertexArray = new Vector3[VertexCount];
        Vector3[] normalsArray = new Vector3[VertexCount];
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

        GetVertices(vertexArray);

        // Accumulate per face normals in the vertices.
        int faceStride = 3;
        Vector3 edgeA = Vector3.zero;
        Vector3 edgeB = Vector3.zero;
        Vector3 partNormal = Vector3.zero;
        for (int i = 0; i < indexArray.Length; i += faceStride)
        {
          // Generate a normal for the current face.
          edgeA = vertexArray[indexArray[i + 1]] - vertexArray[indexArray[i + 0]];
          edgeB = vertexArray[indexArray[i + 2]] - vertexArray[indexArray[i + 1]];
          partNormal = Vector3.Cross(edgeB, edgeA);
          for (int v = 0; v < faceStride; ++v)
          {
            normalsArray[indexArray[i + v]] += partNormal;
          }
        }

        // Normalise all the part normals.
        for (int i = 0; i < normalsArray.Length; ++i)
        {
          normalsArray[i] = normalsArray[i].normalized;
        }

        SetNormals(normalsArray);
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

    // public void Render(string vertexStreamName = "_Vertices", string normalsStreamName = "_Normals",
    //                    string coloursStreamName = "_Colours")
    // {
    //   // Set buffers.
    //   // GL.PushMatrix();
    //   // GL.MultMatrix(cloud.Matrix4);

    //   _material.SetPass(0);
    //   // material.SetColor("_Color", Color.white);
    //   // material.SetColor("_Tint", Color.white);
    //   // // material.SetFloat("_PointSize", 8);
    //   // materialmaterial.SetInt("_PointHighlighting", 0);
    //   // material.SetInt("_LeftHanded", 1);
    //   // // material.EnableKeyword("WITH_COLOUR");

    //   _material.SetBuffer(vertexStreamName, _vertexBuffer);
    //   if (_normalsBuffer != null)
    //   {
    //     _material.SetBuffer(normalsStreamName, _normalsBuffer);
    //   }
    //   if (_normalsBuffer != null)
    //   {
    //     _material.SetBuffer(coloursStreamName, _vertexBuffer);
    //   }

    //   if (_indexBuffer != null)
    //   {
    //     Graphics.DrawProceduralNow(Topology, _indexBuffer, IndexCount, 1);
    //   }
    //   else
    //   {
    //     Graphics.DrawProceduralNow(Topology, VertexCount, 1);
    //   }
    //   // GL.PopMatrix();
    // }

    protected void CreateIndexBuffer()
    {
      if (_indices == null || _indices.Length != IndexCount)
      {
        _indices = null;
        if (_indexBuffer != null)
        {
          _indexBuffer.Release();
          _indexBuffer = null;
        }
        if (IndexCount > 0)
        {
          _indices = new int[IndexCount];
          _indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, IndexCount, Marshal.SizeOf(typeof(int)));
        }
      }
    }

    protected void CreateVertexBuffer()
    {
      EnsureBufferSize<Vector3>(ref _vertexBuffer, _vertexCount);
    }

    protected void CreateNormalsBuffer()
    {
      _materialDirty = _normalsBuffer == null;
      EnsureBufferSize<Vector3>(ref _normalsBuffer, _vertexCount);
    }

    protected void CreateColoursBuffer()
    {
      _materialDirty = _normalsBuffer == null;
      EnsureBufferSize<uint>(ref _coloursBuffer, _vertexCount);
    }

    protected void CreateUVsBuffer()
    {
      _materialDirty = _normalsBuffer == null;
      EnsureBufferSize<Vector2>(ref _uvsBuffer, _vertexCount);
    }

    protected void EnsureBufferSize<T>(ref ComputeBuffer buffer, int count)
    {
      int stride = Marshal.SizeOf(typeof(T));
      T[] copyFrom = null;
      if (buffer != null)
      {
        if (count > 0)
        {
          if (buffer.stride != stride || buffer.count != count)
          {
            copyFrom = new T[Math.Min(count, buffer.count)];
            if (copyFrom.Length > 0)
            {
              buffer.GetData(copyFrom, 0, 0, copyFrom.Length);
            }
            buffer.Release();
            buffer = null;
          }
        }
        else
        {
          buffer.Release();
          buffer = null;
        }
      }

      if (buffer == null)
      {
        if (count > 0)
        {
          buffer = new ComputeBuffer(count, stride);
          if (copyFrom != null)
          {
            buffer.SetData(copyFrom, 0, 0, copyFrom.Length);
          }
        }
      }
    }


    private GraphicsBuffer _indexBuffer = null;
    // Can't read indices back from the GraphicsBuffer, so we have to have a cache here.
    private int[] _indices = null;
    private ComputeBuffer _vertexBuffer = null;
    private ComputeBuffer _normalsBuffer = null;
    private ComputeBuffer _coloursBuffer = null;
    private ComputeBuffer _uvsBuffer = null;
    private Vector3 _minBounds = Vector3.zero;
    private Vector3 _maxBounds = Vector3.zero;
    private Vector3 _boundsPadding = Vector3.zero;
    private bool _boundsSet = false;
    private int _vertexCount = 0;
    private int _indexCount = 0;
    private MeshDrawType _drawType = MeshDrawType.Triangles;
  }
}