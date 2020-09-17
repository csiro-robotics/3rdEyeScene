using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tes.Buffers.Converters;
using Tes.IO;
using Tes.Net;

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
  /// The <see cref="Count"/> has similar semantics. However, the <see cref="AddressableCount"/> corresponds to the
  /// number of @c <c>Vector3</c> item count.
  /// </remarks>
  public class VertexBuffer
  {
    /// <summary>
    /// Query the number of addressable elements in the buffer.
    /// </summary>
    /// <remarks>
    /// This is equivalent to <c>ElementStride * ComponentCount</c>.
    /// </remarks>
    public int AddressableCount { get { return _converter.AddressableCount(_buffer); } }

    /// <summary>
    /// The number of data components or data channels in the buffer. For example, <c>Vector3</c> has 3.
    /// </summary>
    public int ComponentCount { get { return _componentCount; } }

    /// <summary>
    /// The number of channels in the buffer including padding.
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

    /// <summary>
    /// Estimate the number of elements which can be transferred at the given <paramref name="byteLimit"/>
    /// </summary>
    /// <param name="elementSize">The byte size of each element.</param>
    /// <param name="byteLimit">The maximum number of bytes to transfer. Note: a hard limit of 0xffff is
    ///   enforced.</param>
    /// <param name="overhead">Additional byte overhead to a account for. This reduces the effectivel, total byte limit.</param>
    /// <returns>The maximum number of elements which can be accommodated in the byte limit (conservative).</returns>
    public static ushort EstimateTransferCount(uint elementSize, uint byteLimit, uint overhead = 0)
    {
      uint maxTransfer = (uint)((0xffff - (PacketHeader.Size + overhead + Crc16.CrcSize)) / elementSize);
      uint count = (byteLimit > 0) ? byteLimit / elementSize : maxTransfer;
      if (count < 1)
      {
        count = 1;
      }
      else if (count > maxTransfer)
      {
        count = maxTransfer;
      }

      return (ushort)count;
    }

    public uint Write(PacketBuffer packet, uint offset, DataStreamType writeAsType, uint byteLimit)
    {
      if (writeAsType == DataStreamType.PackedFloat16 || writeAsType == DataStreamType.PackedFloat32)
      {
        // TODO(KS): throw an exception here.
        return 0;
      }

      uint itemSize = DataStreamTypeInfo.SizeoOf(writeAsType) * (uint)ComponentCount;

      // Overhead: account for:
      // - uint32_t offset
      // - uint16_t count
      // - uint8_t element stride
      // - uint8_t data type
      const uint overhead = 4 +                             // offset
                            2 +                             // count
                            1 +                             // element stride
                            1;                              // data type

      ushort count = (ushort)Math.Min(EstimateTransferCount(itemSize, byteLimit, overhead), (uint)AddressableCount - offset);

      // To write:
      // - uint32 strided element offset
      // - uint16 strided element count
      // - uint8 channel/component count
      // - uint8 write type
      // if packed, also write:
      // - float32/64 quantisation unit. 32 for PackedFloat16, 64 for PackedFloat32
      // - float32/64[3] packing origin. 32 for PackedFloat16, 64 for PackedFloat32

      packet.WriteBytes(BitConverter.GetBytes(offset), true);
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      byte byteValue = (byte)ComponentCount;
      packet.WriteBytes(BitConverter.GetBytes(byteValue), true);
      byteValue = (byte)writeAsType;
      packet.WriteBytes(BitConverter.GetBytes(byteValue), true);

      switch (writeAsType)
      {
        case DataStreamType.Float32:
          WriteFloat32(packet, offset, count);
          break;
          // case DataStreamType.Float64:
          //   WriteFloat64(packet, offset, count);
          //   break;
      }

      return count;
    }

    public uint WritePacked(PacketBuffer packet, uint offset, DataStreamType writeAsType, uint byteLimit, double quantisationUnit)
    {
      if (writeAsType != DataStreamType.PackedFloat16 && writeAsType != DataStreamType.PackedFloat32)
      {
        // TODO(KS): throw an exception here.
        return 0;
      }

      uint itemSize = DataStreamTypeInfo.SizeoOf(writeAsType) * (uint)ComponentCount;
      uint floatSize = writeAsType == DataStreamType.PackedFloat16 ? 4u : 8u;

      // Overhead: account for:
      // - uint32_t offset
      // - uint16_t count
      // - uint8_t element stride
      // - uint8_t data type
      // - FloatType quantisationUnit
      // - FloatType[stream.componentCount()] packingOrigin
      uint overhead = 4 +                             // offset
                      2 +                             // count
                      1 +                              // element stride
                      1 +                               // data type
                      floatSize +                // quantisation unit
                      floatSize * 3;             // packing origin.

      ushort count = (ushort)Math.Min(EstimateTransferCount(itemSize, byteLimit, overhead), (uint)AddressableCount - offset);

      // To write:
      // - uint32 strided element offset
      // - uint16 strided element count
      // - uint8 channel/component count
      // - uint8 write type
      // if packed, also write:
      // - float32/64 quantisation unit. 32 for PackedFloat16, 64 for PackedFloat32
      // - float32/64[3] packing origin. 32 for PackedFloat16, 64 for PackedFloat32

      packet.WriteBytes(BitConverter.GetBytes(offset), true);
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      byte byteValue = (byte)ComponentCount;
      packet.WriteBytes(BitConverter.GetBytes(byteValue), true);
      byteValue = (byte)writeAsType;
      packet.WriteBytes(BitConverter.GetBytes(byteValue), true);

      if (writeAsType == DataStreamType.PackedFloat16)
      {
        // Write quantisation and origin info as floats
        float quantisationFloat = (float)quantisationUnit;
        packet.WriteBytes(BitConverter.GetBytes(quantisationFloat), true);

        float[] packedOrigin = new float[ComponentCount];
        for (int i = 0; i < packedOrigin.Length; ++i)
        {
          packedOrigin[i] = 0.0f;
          packet.WriteBytes(BitConverter.GetBytes(packedOrigin[i]), true);
        }

        // Now write result.
        WritePackedFloat16(packet, offset, count, packedOrigin, quantisationFloat);
      }
      else
      {
        packet.WriteBytes(BitConverter.GetBytes(quantisationUnit), true);

        double[] packedOrigin = new double[ComponentCount];
        for (int i = 0; i < packedOrigin.Length; ++i)
        {
          packedOrigin[i] = 0.0f;
          packet.WriteBytes(BitConverter.GetBytes(packedOrigin[i]), true);
        }
        WritePackedFloat32(packet, offset, count, packedOrigin, quantisationUnit);
      }
      return count;
    }


    private void WriteFloat32(PacketBuffer packet, uint offset, uint count)
    {
      for (int i = 0; i < (int)count; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          packet.WriteBytes(BitConverter.GetBytes(GetSingle((int)offset * ComponentCount + i * ComponentCount + c)), true);
        }
      }
    }


    private void WritePackedFloat16(PacketBuffer packet, uint offset, uint count, float[] packedOrigin, float quantisationUnit)
    {
      // Write quantisation
      packet.WriteBytes(BitConverter.GetBytes(quantisationUnit), true);

      // Write the packed origin.
      for (int c = 0; c < ComponentCount; ++c)
      {
        packet.WriteBytes(BitConverter.GetBytes(packedOrigin[c]), true);
      }

      float quantisationInverse = 1.0f / quantisationUnit;
      for (int i = 0; i < (int)count; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          float value = GetSingle((int)offset * ComponentCount + i * ComponentCount + c) - packedOrigin[c];
          short quantisedValue = (short)(value * quantisationUnit);
          packet.WriteBytes(BitConverter.GetBytes(quantisedValue), true);
        }
      }
    }


    private void WritePackedFloat32(PacketBuffer packet, uint offset, uint count, double[] packedOrigin, double quantisationUnit)
    {
      // Write quantisation
      packet.WriteBytes(BitConverter.GetBytes(quantisationUnit), true);

      // Write the packed origin.
      for (int c = 0; c < ComponentCount; ++c)
      {
        packet.WriteBytes(BitConverter.GetBytes(packedOrigin[c]), true);
      }

      double quantisationInverse = 1.0 / quantisationUnit;
      for (int i = 0; i < (int)count; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          double value = GetDouble((int)offset * ComponentCount + i * ComponentCount + c) - packedOrigin[c];
          int quantisedValue = (int)(value * quantisationInverse);
          packet.WriteBytes(BitConverter.GetBytes(quantisedValue), true);
        }
      }
    }

    public void Read(PacketBuffer packet, BinaryReader reader)
    {
      // reader.Read();

      uint offset = reader.ReadUInt32();
      int count = reader.ReadUInt16();
      int componentCount = reader.ReadByte();
      int dataType = reader.ReadByte();

      if (componentCount != _componentCount)
      {
        // TODO: throw invalid component count
        return;
      }

      if (count + offset >= AddressableCount)
      {
        // TODO: throw insufficient allocation
        return;
      }

      switch ((DataStreamType)dataType)
      {
        case DataStreamType.Float32:
          ReadFloat32(packet, reader, offset, count, componentCount);
          break;
        case DataStreamType.PackedFloat16:
          ReadPackedFloat16(packet, reader, offset, count, componentCount);
          break;
      }

      // - uint32 strided element offset
      // - uint16 strided element count
      // - uint8 channel/component count
      // - uint8 write type
      // if packed, also write:
      // - float32/64 quantisation unit. 32 for PackedFloat16, 64 for PackedFloat32
      // - float32/64[3] packing origin. 32 for PackedFloat16, 64 for PackedFloat32
    }

    private void ReadFloat32(PacketBuffer packet, BinaryReader reader, uint offset, int count, int componentCount)
    {
      // Won't work for vector2/3
      IList<float> buffer = (IList<float>)_buffer;

      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          buffer[((int)offset + i) * AddressableCount + c] = reader.ReadSingle();
        }
      }
    }

    private void ReadFloat64(PacketBuffer packet, BinaryReader reader, uint offset, int count, int componentCount)
    {
      // Won't work for vector2/3
      IList<double> buffer = (IList<double>)_buffer;

      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          buffer[((int)offset + i) * AddressableCount + c] = reader.ReadDouble();
        }
      }
    }
    private void ReadPackedFloat16(PacketBuffer packet, BinaryReader reader, uint offset, int count, int componentCount)
    {
      // Won't work for vector2/3
      IList<float> buffer = (IList<float>)_buffer;

      // Read the quantisation and offset values.
      float quantisationUnit = reader.ReadSingle();
      float[] packedOrigin = new float[componentCount];

      for (int i = 0; i < componentCount; ++i)
      {
        packedOrigin[i] = reader.ReadSingle();
      }

      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          // Read packed value.
          short packedValue = reader.ReadInt16();
          float value = (float)packedValue * quantisationUnit + packedOrigin[c];
          buffer[((int)offset + i) * AddressableCount + c] = value;
        }
      }
    }

    private void ReadPackedFloat32(PacketBuffer packet, BinaryReader reader, uint offset, int count, int componentCount)
    {
      // Won't work for vector2/3
      IList<double> buffer = (IList<double>)_buffer;

      // Read the quantisation and offset values.
      double quantisationUnit = reader.ReadDouble();
      double[] packedOrigin = new double[componentCount];

      for (int i = 0; i < componentCount; ++i)
      {
        packedOrigin[i] = reader.ReadDouble();
      }

      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          // Read packed value.
          int packedValue = reader.ReadInt32();
          double value = (double)packedValue * quantisationUnit + packedOrigin[c];
          buffer[((int)offset + i) * AddressableCount + c] = value;
        }
      }
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