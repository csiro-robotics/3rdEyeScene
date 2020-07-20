// This is a generated file. Do not modify it directly.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tes.Buffers.Converters
{
  /// <summary>
  /// Type conversion helper from <c>int</c> typed List and Array types.
  /// </summary>
  internal class Int32Converter : BufferConverter
  {
    /// <summary>
    /// Query the supported buffer type.
    /// </summary>
    public Type Type { get { return typeof(int); } }

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    public int AddressableCount(IList list)
    {
      return ((IList<int>)list).Count;
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>sbyte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeSByte(IList<sbyte> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((sbyte)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>sbyte</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public sbyte GetSByte(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (sbyte)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>byte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeByte(IList<byte> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((byte)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>byte</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public byte GetByte(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (byte)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>short</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeInt16(IList<short> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((short)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>short</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public short GetInt16(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (short)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>ushort</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeUInt16(IList<ushort> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((ushort)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>ushort</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public ushort GetUInt16(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (ushort)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>int</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeInt32(IList<int> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((int)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>int</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public int GetInt32(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (int)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>uint</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeUInt32(IList<uint> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((uint)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>uint</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public uint GetUInt32(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (uint)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>long</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeInt64(IList<long> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((long)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>long</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public long GetInt64(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (long)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>ulong</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeUInt64(IList<ulong> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((ulong)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>ulong</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public ulong GetUInt64(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (ulong)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>float</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeSingle(IList<float> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((float)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>float</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public float GetSingle(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (float)srcList[srcOffset];
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>double</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRangeDouble(IList<double> dst, IList src, int srcOffset, int count)
    {
      IList<int> srcList = (IList<int>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((double)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Read a single <c>double</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public double GetDouble(IList src, int srcOffset)
    {
      IList<int> srcList = (IList<int>)src;
      return (double)srcList[srcOffset];
    }
  }
}
