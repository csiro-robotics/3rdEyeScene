using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Test.Net;
using UnityEngine;

namespace Tes.Runtime
{
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
          break;
        case MeshDrawType.Lines:
            return MeshTopology.Lines;
          break;
        case MeshDrawType.Triangles:
            return MeshTopology.Triangles;
          break;
        default:
          break;
        }
        // Programmatic error if this happens.
        throw new Exception("Unsupported draw type");
      }
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

    public boo BoundsSet
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
      Debug.Assert(indices.Count <= IndexCount);
      _indexBuffer.SetData(indices);
    }

    public void SetIndices(int[] indices)
    {
      Debug.Assert(indices.Length <= IndexCount);
      _indexBuffer.SetData(indices);
    }

    public void SetIndices(List<int> indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < indices.Count &&
                   listStartIndex + count <= indices.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _indexBuffer.SetData(indices, listStartIndex, bufferStartIndex, count);
    }

    public void SetIndices(int[] indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < indices.Length &&
                   listStartIndex + count <= indices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _indexBuffer.SetData(indices, listStartIndex, bufferStartIndex, count);
    }

    public void GetIndices(int[] indices)
    {
      Debug.Assert(indices.Length <= IndexCount);
      _indexBuffer.GetData(indices);
    }

    public void GetIndices(int[] indices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < indices.Length &&
                   listStartIndex + count <= indices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < IndexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _indexBuffer.GetData(indices, listStartIndex, bufferStartIndex, count);
    }

    public void SetVertices(List<Vector3> vertices)
    {
      Debug.Assert(vertices.Count <= VertexCount);
      _vertexBuffer.SetData(vertices);
    }

    public void SetVertices(Vector3[] vertices)
    {
      Debug.Assert(vertices.Length <= VertexCount);
      _vertexBuffer.SetData(vertices);
    }

    public void SetVertices(List<Vector3> vertices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < vertices.Count &&
                   listStartIndex + count <= vertices.Count &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
      _vertexBuffer.SetData(vertices, listStartIndex, bufferStartIndex, count);
    }

    public void SetVertices(Vector3[] vertices, int listStartIndex, int bufferStartIndex, int count)
    {
      Debug.Assert(count >= 0 &&
                   0 <= listStartIndex && listStartIndex < vertices.Length &&
                   listStartIndex + count <= vertices.Length &&
                   0 <= bufferStartIndex && bufferStartIndex < VertexCount &&
                   bufferStartIndex + count < bufferStartIndex);
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

    public void Render(string vertexStreamName = "vertices", string normalsStreamName = "normals",
                       string coloursStreamName = "colours")
    {
      // Set buffers.
      // GL.PushMatrix();
      // GL.MultMatrix(cloud.Matrix4);

      // material.SetPass(0);
      // material.SetColor("_Color", Color.white);
      // material.SetColor("_Tint", Color.white);
      // // material.SetFloat("_PointSize", 8);
      // materialmaterial.SetInt("_PointHighlighting", 0);
      // material.SetInt("_LeftHanded", 1);
      // // material.EnableKeyword("WITH_COLOUR");

      material.SetBuffer(vertexStreamName, _vertexBuffer);
      if (_normalsBuffer != null)
      {
        material.SetBuffer(normalsStreamName, _normalsBuffer);
      }
      if (_normalsBuffer != null)
      {
        material.SetBuffer(coloursStreamName, _vertexBuffer);
      }

      if (_indexBuffer != null)
      {
        Graphics.DrawProceduralNow(Topology, _indexBuffer, VertexCount, 1);
      }
      else
      {
        Graphics.DrawProceduralNow(Topology, VertexCount, 1);
      }
      // GL.PopMatrix();
    }

    protected void CreateIndexBuffer()
    {
      EnsureBufferSize<int>(ref _indexBuffer, _indexBuffer);
    }

    protected void CreateVertexBuffer()
    {
      EnsureBufferSize<Vector3>(ref _vertexBuffer, _vertexCount);
    }

    protected void CreateNormalsBuffer()
    {
      EnsureBufferSize<Vector3>(ref _normalsBuffer, _vertexCount);
    }

    protected void CreateColoursBuffer()
    {
      EnsureBufferSize<uint>(ref _coloursBuffer, _vertexCount);
    }

    protected void CreateUVsBuffer()
    {
      EnsureBufferSize<Vector2>(ref _uvsBuffer, _vertexCount);
    }

    protected void EnsureBufferSize<T>(ref ComputeBuffer buffer, int count)
    {
      int stride = Marshal.Sizeof(typeof(T));
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