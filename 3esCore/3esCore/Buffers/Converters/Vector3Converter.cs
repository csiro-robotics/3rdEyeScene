// This is a generated file. Do not modify it directly.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tes.Maths;

namespace Tes.Buffers.Converters
{
  /// <summary>
  /// Type conversion helper from Vector3 typed List and Array types.
  /// </summary>
  internal class Vector3Converter : BufferConverter
  {
    /// <summary>
    /// Query the supported buffer type.
    /// </summary>
    public Type Type { get { return typeof(Vector3); } }

    /// <summary>
    /// Query the default packing data type.
    /// </summary>
    public Tes.Net.DataStreamType DefaultPackingType { get { return Tes.Net.DataStreamType.Single; } }

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    public int AddressableCount(IList list)
    {
      return ((IList<Vector3>)list).Count * 3;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((sbyte)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (sbyte)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (sbyte)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadSByte();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((byte)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (byte)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (byte)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadByte();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((short)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (short)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (short)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadInt16();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((ushort)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (ushort)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (ushort)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadUInt16();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((int)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (int)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (int)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadInt32();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((uint)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (uint)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (uint)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadUInt32();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((long)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (long)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (long)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadInt64();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((ulong)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (ulong)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (ulong)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadUInt64();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((float)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (float)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (float)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadSingle();
        }
        dstList[i + offset] = value;
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst.Add((double)srcList[i + srcOffset / 3][j]);
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      int initialComponent = srcOffset % 3;
      for (int i = 0; i < count / 3; ++i)
      {
        for (int j = initialComponent; j < 3 && j + i * 3 < count; ++j)
        {
          dst[i * 3 + j] = (double)srcList[i + srcOffset / 3][j];
        }
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
      IList<Vector3> srcList = (IList<Vector3>)src;
      return (double)srcList[srcOffset / 3][srcOffset % 3];
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
      IList<Vector3> dstList = (IList<Vector3>)dst;
      int readCount = 0;
      Vector3 value = new Vector3();
      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          value[c] = (float)reader.ReadDouble();
        }
        dstList[i + offset] = value;
        ++readCount;
      }
      return readCount;
    }
  }
}
