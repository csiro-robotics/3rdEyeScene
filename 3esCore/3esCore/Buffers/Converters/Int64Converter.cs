// This is a generated file. Do not modify it directly.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Tes.Buffers.Converters
{
  /// <summary>
  /// Type conversion helper from <c>long</c> typed List and Array types.
  /// </summary>
  internal class Int64Converter : BufferConverter
  {
    /// <summary>
    /// Query the supported buffer type.
    /// </summary>
    public Type Type { get { return typeof(long); } }

    /// <summary>
    /// Query the default packing data type.
    /// </summary>
    public Tes.Net.DataStreamType DefaultPackingType { get { return Tes.Net.DataStreamType.Int64; } }

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    public int AddressableCount(IList list)
    {
      return (list != null) ? ((IList<long>)list).Count : 0;
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>sbyte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(List<sbyte> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((sbyte)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>sbyte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(sbyte[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (sbyte)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadSByte();
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
    public void GetRange(List<byte> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((byte)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>byte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(byte[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (byte)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadByte();
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
    public void GetRange(List<short> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((short)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>short</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(short[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (short)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadInt16();
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
    public void GetRange(List<ushort> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((ushort)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>ushort</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(ushort[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (ushort)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadUInt16();
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
    public void GetRange(List<int> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((int)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>int</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(int[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (int)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadInt32();
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
    public void GetRange(List<uint> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((uint)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>uint</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(uint[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (uint)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadUInt32();
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
    public void GetRange(List<long> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((long)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>long</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(long[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (long)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadInt64();
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
    public void GetRange(List<ulong> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((ulong)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>ulong</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(ulong[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (ulong)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadUInt64();
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
    public void GetRange(List<float> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((float)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>float</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(float[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (float)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadSingle();
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
    public void GetRange(List<double> dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst.Add((double)srcList[i + srcOffset]);
      }
    }

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>double</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange(double[] dst, IList src, int srcOffset, int count)
    {
      IList<long> srcList = (IList<long>)src;
      for (int i = 0; i < count; ++i)
      {
        dst[i] = (double)srcList[i + srcOffset];
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
      IList<long> srcList = (IList<long>)src;
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
      IList<long> dstList = (IList<long>)dst;
      int readCount = 0;
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          dstList[(i + offset) * componentCount + c] = (long)reader.ReadDouble();
        }
        ++readCount;
      }
      return readCount;
    }
  }
}
