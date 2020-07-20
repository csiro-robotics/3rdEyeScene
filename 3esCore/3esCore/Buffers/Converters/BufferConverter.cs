// This is a generated file. Do not modify it directly.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tes.Buffers.Converters
{
  /// <summary>
  /// Interface for converting from <c>IList</c> wrapped in a <c><see cref="VertexBuffer"/></c>.
  /// </summary>
  /// <remarks>
  /// The interface consists of a series of <c>Get&lt;Type&gt;()</c> and <c>GetRange&lt;Type&gt;()</c> functions.
  /// As this the converter operates with the internals of a <c><see cref="VertexBuffer"/></c>, the supported source
  /// list arguments will generally be simple types, or of known types with data channels. See
  /// <see cref="VertexBuffer"/> for details on how index and count arguments are treated.
  /// </remarks>
  internal interface BufferConverter
  {
    /// <summary>
    /// Query the supported buffer type. This is the type contained in the <c>IList</c>.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    int AddressableCount(IList list);

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>sbyte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeSByte(IList<sbyte> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>sbyte</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    sbyte GetSByte(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>byte</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeByte(IList<byte> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>byte</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    byte GetByte(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>short</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeInt16(IList<short> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>short</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    short GetInt16(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>ushort</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeUInt16(IList<ushort> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>ushort</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    ushort GetUInt16(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>int</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeInt32(IList<int> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>int</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    int GetInt32(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>uint</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeUInt32(IList<uint> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>uint</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    uint GetUInt32(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>long</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeInt64(IList<long> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>long</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    long GetInt64(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>ulong</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeUInt64(IList<ulong> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>ulong</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    ulong GetUInt64(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>float</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeSingle(IList<float> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>float</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    float GetSingle(IList src, int srcOffset);
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>double</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRangeDouble(IList<double> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>double</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    double GetDouble(IList src, int srcOffset);
  }
}
