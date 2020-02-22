using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tes.Runtime
{
  public interface GpuBufferAbstraction
  {
    object Allocate(int count, int stride);
    void Release(object buffer);
    int ElementCount(object buffer);
  }

  public class IndexBufferAbstraction : GpuBufferAbstraction
  {
    public object Allocate(int count, int stride)
    {
      return new GraphicsBuffer(GraphicsBuffer.Target.Index, count, stride);
    }

    public void Release(object buffer)
    {
      ((GraphicsBuffer)buffer).Release();
    }

    public int ElementCount(object buffer)
    {
      return ((GraphicsBuffer)buffer).count;
    }
  }

  public class ComputeBufferAbstraction : GpuBufferAbstraction
  {
    public object Allocate(int count, int stride)
    {
      return new ComputeBuffer(count, stride);
    }

    public void Release(object buffer)
    {
      ((ComputeBuffer)buffer).Release();
    }

    public int ElementCount(object buffer)
    {
      return ((ComputeBuffer)buffer).count;
    }
  }

  public class GpuBufferPool<T> where T : class
  {
    public static readonly int BucketIndexOffset = 11;
    public static readonly int BucketCount = 10;
    public static int MinimumElementCount { get { return 1 << BucketIndexOffset; } }

    public int Stride { get; private set; }

    public GpuBufferPool(int elementStride, GpuBufferAbstraction bufferAbstraction)
    {
      Stride = elementStride;
      _bufferAbstraction = bufferAbstraction;
      _buckets = new Bucket[BucketCount];
      for (int i = 0; i < _buckets.Length; ++i)
      {
        _buckets[i] = new Bucket(1 << (i + BucketIndexOffset), elementStride, bufferAbstraction);
      }
    }

    public void Reset()
    {
      for (int i = 0; i < _buckets.Length; ++i)
      {
        _buckets[i].Reset();
      }
    }

    public T AllocateBuffer(int elementCount)
    {
      int bucketIndex = ResolveBucketIndex(elementCount);

      if (bucketIndex < 0)
      {
        Debug.LogError($"Failed to resolve bucket({bucketIndex}) for element count {elementCount}");
        throw new System.Exception($"Failed to resolve bucket({bucketIndex}) for element count {elementCount}");
      }

      if (bucketIndex > _buckets.Length)
      {
        // Very large buffer. Allocate directly.
        Debug.LogWarning($"Failed to allocate GPU buffer from pool. Buffer too large with {elementCount} elements.");
        return (T)_bufferAbstraction.Allocate(elementCount, Stride);
      }

      return (T)_buckets[bucketIndex].AllocateBuffer();
    }

    public void ReleaseBuffer(T buffer)
    {
      int bucketIndex = ResolveBucketIndex(_bufferAbstraction.ElementCount(buffer));

      if (bucketIndex < 0)
      {
        // TODO: (KS) throw exception.
        _bufferAbstraction.Release(buffer);
        return;
      }

      if (bucketIndex > _buckets.Length)
      {
        // Very large buffer. Release as is.
        _bufferAbstraction.Release(buffer);
        return;
      }

      _buckets[bucketIndex].ReleaseBuffer(buffer);
    }

    private int ResolveBucketIndex(int elementCount)
    {
      // Convert element count to the next power of 2.
      int allocateCount = Maths.IntUtil.NextPowerOf2(elementCount);
      // We allocate a minimum element count.
      allocateCount = Math.Max(allocateCount, MinimumElementCount);

      // Convert the power of 2 to a bit index to resolve the bucket index.
      int bitIndex = Maths.IntUtil.ToBitIndex(allocateCount);
      return bitIndex - BucketIndexOffset;
    }

    private class Bucket
    {
      public int ElementCount { get; private set; }
      public int Stride { get; private set; }
      private List<object> _buffers = new List<object>();
      private List<object> _freeList = new List<object>();
      private GpuBufferAbstraction _bufferAbstraction;

      public Bucket(int elementCount, int stride, GpuBufferAbstraction abstraction)
      {
        ElementCount = elementCount;
        Stride = stride;
        _bufferAbstraction = abstraction;
      }

      public object AllocateBuffer()
      {
        // Check free list first.
        if (_freeList.Count > 0)
        {
          // Pop the last element.
          int freeIndex = _freeList.Count - 1;
          object buffer = _freeList[freeIndex];
          _freeList.RemoveAt(freeIndex);

          Debug.Assert(buffer != null);
          return buffer;
        }

        // Need to allocate a new bufffer.
        object newbuffer = _bufferAbstraction.Allocate(ElementCount, Stride);
        Debug.Assert(newbuffer != null);
        _buffers.Add(newbuffer);
        _freeList.Add(newbuffer);
        return newbuffer;
      }

      public void ReleaseBuffer(object buffer)
      {
        // TODO: (KS) throw if stride or count do not match.
        if (buffer != null)
        {
          _freeList.Add(buffer);
        }
      }

      public void Reset()
      {
        for (int i = 0; i < _buffers.Count; ++i)
        {
          if (_buffers[i] != null)
          {
            _bufferAbstraction.Release(_buffers[i]);
            _buffers[i] = null;
          }
        }
        _buffers.Clear();
        _freeList.Clear();
      }
    }

    private GpuBufferAbstraction _bufferAbstraction;
    private Bucket[] _buckets;
  }
}
