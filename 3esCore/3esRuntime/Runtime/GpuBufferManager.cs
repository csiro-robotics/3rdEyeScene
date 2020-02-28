using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tes.Runtime
{
  /// <summary>
  /// The <c>GpuBufferManager</c> maintains a set of Unity <c>ComputeBuffer</c> and <c>GraphicsBuffer</c> objects for
  /// re-use.
  /// </summary>
  /// <remarks>
  /// The purpose of this class is to reduce the number of GPU allocations made during runtime. When a GPU buffer is
  /// required, the buffer manager either allocates a new buffer or assigns an existing, free buffer. The assigned
  /// buffer is of the required size or larger.
  /// </remarks>
  public class GpuBufferManager
  {
    private static GpuBufferManager _instance = new GpuBufferManager();

    /// <summary>
    /// Provides singleton access to the default buffer manager.
    /// </summary>
    public static GpuBufferManager Instance { get { return _instance; } }

    public GpuBufferManager()
    {
      _indexPool = new GpuBufferPool<GraphicsBuffer>(Marshal.SizeOf(typeof(int)), new IndexBufferAbstraction());
      _vertexPool = new GpuBufferPool<ComputeBuffer>(Marshal.SizeOf(typeof(Vector3)), new ComputeBufferAbstraction());
      _normalsPool = new GpuBufferPool<ComputeBuffer>(Marshal.SizeOf(typeof(Vector3)), new ComputeBufferAbstraction());
      _coloursUIntPool = new GpuBufferPool<ComputeBuffer>(Marshal.SizeOf(typeof(uint)), new ComputeBufferAbstraction());
      _uvsPool = new GpuBufferPool<ComputeBuffer>(Marshal.SizeOf(typeof(Vector2)), new ComputeBufferAbstraction());
    }

    ~GpuBufferManager()
    {
      Reset();
    }

    public void Reset()
    {
      _indexPool.Reset();
      _vertexPool.Reset();
      _normalsPool.Reset();
      _coloursUIntPool.Reset();
      _uvsPool.Reset();
    }

    public GraphicsBuffer AllocateIndexBuffer(int elementCount)
    {
      return _indexPool.AllocateBuffer(elementCount);
    }

    public void ReleaseIndexBuffer(GraphicsBuffer buffer)
    {
      _indexPool.ReleaseBuffer(buffer);
    }

    public ComputeBuffer AllocateVertexBuffer(int elementCount)
    {
      return _vertexPool.AllocateBuffer(elementCount);
    }

    public void ReleaseVertexBuffer(ComputeBuffer buffer)
    {
      _vertexPool.ReleaseBuffer(buffer);
    }

    public ComputeBuffer AllocateNormalsBuffer(int elementCount)
    {
      return _normalsPool.AllocateBuffer(elementCount);
    }

    public void ReleaseNormalsBuffer(ComputeBuffer buffer)
    {
      _normalsPool.ReleaseBuffer(buffer);
    }

    public ComputeBuffer AllocateColoursUIntBuffer(int elementCount)
    {
      return _coloursUIntPool.AllocateBuffer(elementCount);
    }

    public void ReleaseColoursUIntBuffer(ComputeBuffer buffer)
    {
      _coloursUIntPool.ReleaseBuffer(buffer);
    }

    public ComputeBuffer AllocateUVsBuffer(int elementCount)
    {
      return _uvsPool.AllocateBuffer(elementCount);
    }

    public void ReleaseUVsBuffer(ComputeBuffer buffer)
    {
      _uvsPool.ReleaseBuffer(buffer);
    }

    private GpuBufferPool<GraphicsBuffer> _indexPool;
    private GpuBufferPool<ComputeBuffer> _vertexPool;
    private GpuBufferPool<ComputeBuffer> _normalsPool;
    private GpuBufferPool<ComputeBuffer> _coloursUIntPool;
    private GpuBufferPool<ComputeBuffer> _uvsPool;
  }
}