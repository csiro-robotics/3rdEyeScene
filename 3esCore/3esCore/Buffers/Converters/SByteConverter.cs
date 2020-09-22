// This is a generated file. Do not modify it directly.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Tes.Buffers.Converters
{
  /// <summary>
  /// Type conversion helper from <c>sbyte</c> typed List and Array types.
  /// </summary>
  internal class SByteConverter : BufferConverter
  {
    /// <summary>
    /// Query the supported buffer type.
    /// </summary>
    public Type Type { get { return typeof(sbyte); } }

    /// <summary>
    /// Query the default packing data type.
    /// </summary>
    public Tes.Net.DataStreamType DefaultPackingType { get { return Tes.Net.DataStreamType.SByte; } }

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    public int AddressableCount(IList list)
    {
      return (list != null) ? ((IList<sbyte>)list).Count : 0;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (sbyte)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of SByte values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of SByte items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of SByte items to read for each <paramref name="count"/></param>
    public int ReadSByte(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadSByte();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (byte)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of Byte values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of Byte items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of Byte items to read for each <paramref name="count"/></param>
    public int ReadByte(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadByte();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (short)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of Int16 values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of Int16 items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of Int16 items to read for each <paramref name="count"/></param>
    public int ReadInt16(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadInt16();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (ushort)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of UInt16 values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of UInt16 items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of UInt16 items to read for each <paramref name="count"/></param>
    public int ReadUInt16(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadUInt16();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (int)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of Int32 values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of Int32 items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of Int32 items to read for each <paramref name="count"/></param>
    public int ReadInt32(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadInt32();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (uint)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of UInt32 values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of UInt32 items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of UInt32 items to read for each <paramref name="count"/></param>
    public int ReadUInt32(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadUInt32();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (long)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of Int64 values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of Int64 items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of Int64 items to read for each <paramref name="count"/></param>
    public int ReadInt64(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadInt64();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (ulong)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of UInt64 values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of UInt64 items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of UInt64 items to read for each <paramref name="count"/></param>
    public int ReadUInt64(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadUInt64();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (float)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of Single values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of Single items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of Single items to read for each <paramref name="count"/></param>
    public int ReadSingle(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadSingle();
        }
        ++readCount;
      }
      return readCount;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
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
      IList<sbyte> srcList = (IList<sbyte>)src;
      return (double)srcList[srcOffset];
    }

    /// <summary>
    /// Read a range of Double values from a <c>System.IO.BinaryReader</c>.
    /// </summary>
    /// <param name="dst">The list to read into. The stored type must match the implementation type.</param>
    /// <param name="reader">The binary reader from which to read data.</param>
    /// <param name="offset">The offset into dst at which to start writing. This offset is element based, so must be
    /// multiplied by the <paramref name="componentCount"/>.</param>
    /// <param name="count">The number of elements to read. Must be multiplied by the <paramref name="componentCount"/>
    ///   to calculate the total number of Double items to read from <paramref name="reader"/>.</param>
    /// <param name="componentCount">The number of Double items to read for each <paramref name="count"/></param>
    public int ReadDouble(IList dst, BinaryReader reader, int offset, int count, int componentCount)
    {
      IList<sbyte> dstList = (IList<sbyte>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (sbyte)reader.ReadDouble();
        }
        ++readCount;
      }
      return readCount;
    }
  }
}
