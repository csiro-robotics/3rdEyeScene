using System;
using System.Collections;
using System.Collections.Generic;
using Tes.Buffers.Converters;

namespace Tes.Buffers
{
  /// <summary>
  /// A <c>VertexBuffer</c> is used as a data abstraction for adapting data buffers into a form which can be consumed
  /// by <c>Tes</c> code.
  /// </summary>
  ///
  /// <remarks>
  /// The <c>VertexBuffer</c> supports wrapping array and <c>List&lt;T&gt;</c> containers containing either built in
  /// data types (except for <c>bool</c>) or <see cref="Vector2"/> and <see cref="Vector3"/> data types. This is to
  /// support adapting user buffers for use in <c>Tes</c>. The <c>VertexBuffer</c> can then be used to write to or
  /// extract data from a <see cref="PacketBuffer"/>.
  /// 
  /// The data abstraction supports wrapping buffers with multiple data channels, such as a <c>float</c> array used
  /// to contain a set of XYZ triples. This is reflected in <see cref="ComponentCount"/>. The input buffer may
  /// optionally specify a stride value where other data or padding is included in the source buffer. For example, an
  /// array of <c>float</c> of XYZ data with a redundnat W value in between each coordiate (XYZW ordering).
  /// 
  /// The <c>VertexBuffer</c> supports a series of <c>Get&lt;Type&gt;()</c> and <c>GetRange&lt;Type&gt;()</c> functions.
  /// These <c>Get</c> functions only support built in types, which creates a potential ambiguity in the sematics of
  /// index and count arguments and in the <see cref="Count"/> property.
  /// 
  /// The <c>VertexBuffer</c> treats index values and count arguments (excluding <see cref="Count"/>) as indices and
  /// count values as if the <c>VertexBuffer</c> contained only built in types with no <see cref="ComponentCount"/>.
  /// For example, consider a <c>List&lt;Vector3&gt;</c> containing 10 <c>Vector3</c> items wrapped in a
  /// <c>VertexBuffer</c>. Index and count values given to the <c>Get</c> methods index float elements, rather than
  /// <c>Vector3</c> elements. Thus, index 0 maps to Vector3[0].X, index 1 to Vector3[0].Y, index 2 to Vector3[0].Z,
  /// index 3 to Vector3[0].X and so on. Similarly, the extracted count must be a multiple of 3 in order to extract
  /// full <c>Vector3</c> items.
  /// 
  /// The <see cref="Count"/> has similar semantics. However, the <see cref="StridedCount"/> corresponds to the
  /// number of @c <c>Vector3</c> item count.
  /// </remarks>
  public class VertexBuffer
  {
    /// <summary>
    /// Query the number of addressable elements in the buffer.
    /// </summary>
    /// <remarks>
    /// This is equivalent to <c>StridedCount * ComponentCount</c>.
    /// </remarks>
    public int AddressableCount { get { return _converter.AddressableCount(_buffer); } }

    /// <summary>
    /// The number of data components or data channels in the buffer. For example, <c>Vector3</c> has 3.
    /// </summary>
    public int ComponentCount { get { return _componentCount; } }
    
    /// <summary>
    /// The number of padding channes channels in the buffer.
    /// </summary>
    /// <remarks>
    /// For example, a <c>Vector4[]</c> where only XYZ are to be packed would have a stride of 4 to skip the W channel.
    /// </remarks>
    /// <value></value>
    public int ElementStride { get { return _elementStride; } }

    /// <summary>
    /// Internal constructor. Public API uses <c>Wrap()</c>.
    /// </summary>
    /// <param name="sourceBuffer">The data buffer being wrapped.</param>
    /// <param name="sourceType">The data type stored in the buffer.</param>
    /// <param name="converter">Data conerter used to extract data.</param>
    /// <param name="componentCount">Number of data components or data channels.</param>
    /// <param name="elementStride">Number of data channels with padding. Zero for tight packing.</param>
    internal VertexBuffer(IList sourceBuffer, Type sourceType, BufferConverter converter, int componentCount, int elementStride = 0)
    {
      _buffer = sourceBuffer;
      _internalType = sourceType;
      _converter = converter;
      _componentCount = componentCount;
      _elementStride = (elementStride > componentCount) ? elementStride : componentCount;
      _readOnly = true;
    }

    public static VertexBuffer Wrap<T>(List<T> list, int componentCount = 1, int elementStride = 0) where T : IConvertible
    {
      BufferConverter converter = ConverterSet.Get(typeof(T));
      return new VertexBuffer(list, typeof(T), converter, componentCount, elementStride);
    }

    public static VertexBuffer Wrap<T>(T[] list, int componentCount = 1, int elementStride = 0) where T : IConvertible
    {
      BufferConverter converter = ConverterSet.Get(typeof(T));
      return new VertexBuffer(list, typeof(T), converter, componentCount, elementStride);
    }

    public static VertexBuffer Wrap(List<Maths.Vector2> list)
    {
      return new VertexBuffer(list, typeof(Maths.Vector2), ConverterSet.Get(typeof(Maths.Vector2)), 2);
    }

    public static VertexBuffer Wrap(Maths.Vector2[] list)
    {
      return new VertexBuffer(list, typeof(Maths.Vector2), ConverterSet.Get(typeof(Maths.Vector2)), 2);
    }

    public static VertexBuffer Wrap(List<Maths.Vector3> list)
    {
      return new VertexBuffer(list, typeof(Maths.Vector3), ConverterSet.Get(typeof(Maths.Vector3)), 3);
    }

    public static VertexBuffer Wrap(Maths.Vector3[] list)
    {
      return new VertexBuffer(list, typeof(Maths.Vector3), ConverterSet.Get(typeof(Maths.Vector3)), 3);
    }

    #region Get<Type> functions

    public sbyte GetSByte(int index)
    {
      return _converter.GetSByte(_buffer, index);
    }

    public void GetRangeSByte(List<sbyte> range, int startElementIndex, int count)
    {
      _converter.GetRangeSByte(range, _buffer, startElementIndex, count);
    }

    public byte GetByte(int index)
    {
      return _converter.GetByte(_buffer, index);
    }

    public void GetRangeByte(List<byte> range, int startElementIndex, int count)
    {
      _converter.GetRangeByte(range, _buffer, startElementIndex, count);
    }

    public short GetInt16(int index)
    {
      return _converter.GetInt16(_buffer, index);
    }

    public void GetRangeInt16(List<short> range, int startElementIndex, int count)
    {
      _converter.GetRangeInt16(range, _buffer, startElementIndex, count);
    }

    public ushort GetUInt16(int index)
    {
      return _converter.GetUInt16(_buffer, index);
    }

    public void GetRangeUInt16(List<ushort> range, int startElementIndex, int count)
    {
      _converter.GetRangeUInt16(range, _buffer, startElementIndex, count);
    }

    public int GetInt32(int index)
    {
      return _converter.GetInt32(_buffer, index);
    }

    public void GetRangeInt32(List<int> range, int startElementIndex, int count)
    {
      _converter.GetRangeInt32(range, _buffer, startElementIndex, count);
    }

    public uint GetUInt32(int index)
    {
      return _converter.GetUInt32(_buffer, index);
    }

    public void GetRangeUInt32(List<uint> range, int startElementIndex, int count)
    {
      _converter.GetRangeUInt32(range, _buffer, startElementIndex, count);
    }

    public long GetInt64(int index)
    {
      return _converter.GetInt64(_buffer, index);
    }

    public void GetRangeInt64(List<long> range, int startElementIndex, int count)
    {
      _converter.GetRangeInt64(range, _buffer, startElementIndex, count);
    }

    public ulong GetUInt64(int index)
    {
      return _converter.GetUInt64(_buffer, index);
    }

    public void GetRangeUInt64(List<ulong> range, int startElementIndex, int count)
    {
      _converter.GetRangeUInt64(range, _buffer, startElementIndex, count);
    }

    public float GetSingle(int index)
    {
      return _converter.GetSingle(_buffer, index);
    }

    public void GetRangeSingle(List<float> range, int startElementIndex, int count)
    {
      _converter.GetRangeSingle(range, _buffer, startElementIndex, count);
    }

    public double GetDouble(int index)
    {
      return _converter.GetDouble(_buffer, index);
    }

    public void GetRangeDouble(List<double> range, int startElementIndex, int count)
    {
      _converter.GetRangeDouble(range, _buffer, startElementIndex, count);
    }

    #endregion

    private IList _buffer = null;
    private Type _internalType = null;
    private int _componentCount = 1;
    private int _elementStride = 1;
    private bool _readOnly = true;
    private BufferConverter _converter = null;
  }
}