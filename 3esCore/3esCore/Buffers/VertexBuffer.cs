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
  /// number of channeled elements which can be addressed in the buffer.<!-- For-->
  /// </remarks>
  public class VertexBuffer
  {
    /// <summary>
    /// Query the number of addressable elements in the buffer.
    /// </summary>
    /// <remarks>
    /// This is equivalent to <c>ElementStride * ComponentCount</c>.
    /// </remarks>
    public int Count { get { return AddressableCount / ComponentCount; } }
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
    /// Query the native type contained in the buffer as a <see cref="Tes.Net.DataStreamType"/>
    /// </summary>
    /// <value></value>
    public DataStreamType NativePackingType { get { return _converter.DefaultPackingType; } }

    public bool IsValid { get { return _buffer != null; } }

    public bool ReadOnly { get { return _readOnly; } set { _readOnly = value;  } }

    /// <summary>
    /// Create VertexBuffer to use with Read methods. The buffer type is set on the first read call.
    /// </summary>
    public VertexBuffer()
    {
      _buffer = null;
      _internalType = null;
      _converter = null;
      _componentCount = 0;
      _elementStride = 0;
      _readOnly = false;
    }

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
    public static ushort EstimateTransferCount(int elementSize, int byteLimit, int overhead = 0)
    {
      // Set a default byte limit
      byteLimit = (byteLimit > 0) ? byteLimit : 0xff00;
      int maxTransfer = ((byteLimit - (PacketHeader.Size + overhead + Crc16.CrcSize)) / elementSize);
      return (ushort)maxTransfer;
    }

    public int Write(PacketBuffer packet, int offset, DataStreamType writeAsType, int byteLimit)
    {
      if (writeAsType == DataStreamType.PackedFloat16 || writeAsType == DataStreamType.PackedFloat32)
      {
        // TODO(KS): throw an exception here.
        return 0;
      }

      int itemSize = DataStreamTypeInfo.SizeoOf(writeAsType) * ComponentCount;

      // Overhead: account for:
      // - uint32_t offset
      // - uint16_t count
      // - uint8_t element stride
      // - uint8_t data type
      const int overhead = 4 +                             // offset
                            2 +                             // count
                            1 +                             // element stride
                            1;                              // data type

      ushort count = (ushort)Math.Min(EstimateTransferCount(itemSize, byteLimit, overhead), Count - offset);

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
      packet.WriteBytes(new byte[] { byteValue }, true);
      byteValue = (byte)writeAsType;
      packet.WriteBytes(new byte[] { byteValue }, true);

      switch (writeAsType)
      {
        case DataStreamType.Int8:
          // This conversion from SByte to Byte may fail rather than doing the bit conversion we want.
          return WritePayload(packet, offset, count, (index) => new byte[] { (byte)GetSByte(index) });
        case DataStreamType.UInt8:
          return WritePayload(packet, offset, count, (index) => new byte[] { GetByte(index) });
        case DataStreamType.Int16:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetInt16(index)));
        case DataStreamType.UInt16:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetUInt16(index)));
        case DataStreamType.Int32:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetInt32(index)));
        case DataStreamType.UInt32:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetUInt32(index)));
        case DataStreamType.Int64:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetInt64(index)));
        case DataStreamType.UInt64:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetUInt64(index)));
        case DataStreamType.Float32:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetSingle(index)));
        case DataStreamType.Float64:
          return WritePayload(packet, offset, count, (index) => BitConverter.GetBytes(GetDouble(index)));
      }

      return 0;
    }

    public int WritePacked(PacketBuffer packet, int offset, DataStreamType writeAsType, int byteLimit, double quantisationUnit)
    {
      if (writeAsType != DataStreamType.PackedFloat16 && writeAsType != DataStreamType.PackedFloat32)
      {
        // TODO(KS): throw an exception here.
        return 0;
      }

      int itemSize = DataStreamTypeInfo.SizeoOf(writeAsType) * ComponentCount;
      int floatSize = writeAsType == DataStreamType.PackedFloat16 ? 4 : 8;

      // Overhead: account for:
      // - uint32_t offset
      // - uint16_t count
      // - uint8_t element stride
      // - uint8_t data type
      // - FloatType quantisationUnit
      // - FloatType[ComponentCount] packingOrigin
      int overhead = 4 +                             // offset
                      2 +                             // count
                      1 +                              // element stride
                      1 +                               // data type
                      floatSize +                // quantisation unit
                      floatSize * ComponentCount;             // packing origin.

      ushort count = (ushort)Math.Min(EstimateTransferCount(itemSize, byteLimit, overhead), Count - offset);

      // Write 32-bit offset
      packet.WriteBytes(BitConverter.GetBytes(offset), true);
      // Write 16 bit count
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      // Write 1 byte component count
      byte byteValue = (byte)ComponentCount;
      packet.WriteBytes(new byte[] { byteValue }, true);
      // Write 1 byte data type
      byteValue = (byte)writeAsType;
      packet.WriteBytes(new byte[] { byteValue }, true);

      if (writeAsType == DataStreamType.PackedFloat16)
      {
        // Write quantisation and origin info as floats
        float[] packedOrigin = new float[ComponentCount];
        for (int i = 0; i < packedOrigin.Length; ++i)
        {
          packedOrigin[i] = 0.0f;
        }

        // Now write result.
        return WritePackedFloat16(packet, offset, count, packedOrigin, (float)quantisationUnit);
      }
      else if (writeAsType == DataStreamType.PackedFloat32)
      {
        double[] packedOrigin = new double[ComponentCount];
        for (int i = 0; i < packedOrigin.Length; ++i)
        {
          packedOrigin[i] = 0.0;
        }
        return WritePackedFloat32(packet, offset, count, packedOrigin, quantisationUnit);
      }
      return 0;
    }


    private int WritePayload(PacketBuffer packet, int offset, int count, Func<int, byte[]> getComponent)
    {
      int written = 0;
      for (int i = 0; i < (int)count; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          // packet.WriteBytes(BitConverter.GetBytes(GetUInt64((int)offset * ComponentCount + i * ComponentCount + c)), true);
          packet.WriteBytes(getComponent((int)offset * ComponentCount + i * ComponentCount + c), true);
        }
        ++written;
      }
      return written;
    }


    private int WritePackedFloat16(PacketBuffer packet, int offset, int count, float[] packedOrigin, float quantisationUnit)
    {
      // Write quantisation
      packet.WriteBytes(BitConverter.GetBytes(quantisationUnit), true);

      // Write the packed origin.
      for (int c = 0; c < ComponentCount; ++c)
      {
        packet.WriteBytes(BitConverter.GetBytes(packedOrigin[c]), true);
      }

      float quantisationInverse = 1.0f / quantisationUnit;
      int written = 0;
      for (int i = 0; i < (int)count; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          float value = GetSingle((int)offset * ComponentCount + i * ComponentCount + c) - packedOrigin[c];
          value *= quantisationInverse;
          short quantisedValue = (short)(Math.Round(value));
          packet.WriteBytes(BitConverter.GetBytes(quantisedValue), true);
        }
        ++written;
      }
      return written;
    }


    private int WritePackedFloat32(PacketBuffer packet, int offset, int count, double[] packedOrigin, double quantisationUnit)
    {
      // Write quantisation
      packet.WriteBytes(BitConverter.GetBytes(quantisationUnit), true);

      // Write the packed origin.
      for (int c = 0; c < ComponentCount; ++c)
      {
        packet.WriteBytes(BitConverter.GetBytes(packedOrigin[c]), true);
      }

      double quantisationInverse = 1.0 / quantisationUnit;
      int written = 0;
      for (int i = 0; i < (int)count; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          double value = GetDouble((int)offset * ComponentCount + i * ComponentCount + c) - packedOrigin[c];
          value *= quantisationInverse;
          int quantisedValue = (int)(Math.Round(value));
          packet.WriteBytes(BitConverter.GetBytes(quantisedValue), true);
        }
        ++written;
      }
      return written;
    }

    public void Read(BinaryReader reader)
    {
      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();
      Read(reader, offset, count);
    }

    public void Read(BinaryReader reader, int offset, int count)
    {
      int componentCount = reader.ReadByte();
      int dataType = reader.ReadByte();

      if (!IsValid)
      {
        // Create the buffer required to handle the incoming data.
        CreateWriteBuffer((int)(offset + count), componentCount, (DataStreamType)dataType);
      }
      else
      {
        // Validate existing data against incoming data.
        if (_readOnly)
        {
          // TODO: throw. Read only. Cannot write.
          return;
        }

        if (componentCount != _componentCount)
        {
          // TODO: throw invalid component count
          return;
        }

        if (count + offset > Count)
        {
          // Increase the buffer capacity.
          Expand((int)(count + offset));
        }
      }


      switch ((DataStreamType)dataType)
      {
        case DataStreamType.Int8:
          _converter.ReadSByte(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.UInt8:
          _converter.ReadByte(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.Int16:
          _converter.ReadInt16(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.UInt16:
          _converter.ReadUInt16(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.Int32:
          _converter.ReadInt32(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.UInt32:
          _converter.ReadUInt32(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.Int64:
          _converter.ReadInt64(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.UInt64:
          _converter.ReadUInt64(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.Float32:
          _converter.ReadSingle(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.PackedFloat16:
          // Read values via a proxy reader which deals with the data conversion.
          _converter.ReadSingle(_buffer, new PackedFloat16Reader(reader, componentCount), offset, count,
                                componentCount);
          break;
        case DataStreamType.Float64:
          _converter.ReadDouble(_buffer, reader, offset, count, componentCount);
          break;
        case DataStreamType.PackedFloat32:
          _converter.ReadDouble(_buffer, new PackedFloat32Reader(reader, componentCount), offset, count,
                                componentCount);
          break;
      }
    }

    private void ReadPayload<T>(int offset, int count, int componentCount, Func<T> readNext)
    {
      // Won't work for vector2/3
      IList<T> buffer = (IList<T>)_buffer;

      for (int i = 0; i < count; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          buffer[((int)offset + i) * ElementStride + c] = readNext();
        }
      }
    }

    private void ReadPackedFloat16(BinaryReader reader, int offset, int count, int componentCount)
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
          buffer[((int)offset + i) * ElementStride + c] = value;
        }
      }
    }

    private void ReadPackedFloat32(BinaryReader reader, int offset, int count, int componentCount)
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
          buffer[((int)offset + i) * ElementStride + c] = value;
        }
      }
    }

    private void CreateBufferOfType<T>(int initialItemCount, int componentCount, T defaultValue)
    {
      List<T> buffer = new List<T>();
      for (int i = 0; i < initialItemCount; ++i)
      {
        for (int c = 0; c < componentCount; ++c)
        {
          buffer.Add(defaultValue);
        }
      }

      _buffer = buffer;
      _internalType = typeof(T);
      _elementStride = _componentCount = componentCount;
      _converter = ConverterSet.Get(typeof(T));
    }

    private void CreateWriteBuffer(int initialItemCount, int componentCount, DataStreamType dataType)
    {
      switch (dataType)
      {
        case DataStreamType.None:
          // TODO: throw
          break;
        case DataStreamType.Int8:
          CreateBufferOfType<sbyte>(initialItemCount, componentCount, (sbyte)0);
          break;
        case DataStreamType.UInt8:
          CreateBufferOfType<byte>(initialItemCount, componentCount, (byte)0);
          break;
        case DataStreamType.Int16:
          CreateBufferOfType<short>(initialItemCount, componentCount, (short)0);
          break;
        case DataStreamType.UInt16:
          CreateBufferOfType<ushort>(initialItemCount, componentCount, (ushort)0);
          break;
        case DataStreamType.Int32:
          CreateBufferOfType<int>(initialItemCount, componentCount, (int)0);
          break;
        case DataStreamType.UInt32:
          CreateBufferOfType<uint>(initialItemCount, componentCount, (uint)0);
          break;
        case DataStreamType.Int64:
          CreateBufferOfType<long>(initialItemCount, componentCount, (long)0);
          break;
        case DataStreamType.UInt64:
          CreateBufferOfType<ulong>(initialItemCount, componentCount, (ulong)0);
          break;
        case DataStreamType.Float32:
        case DataStreamType.PackedFloat16:
          CreateBufferOfType<float>(initialItemCount, componentCount, (float)0);
          break;
        case DataStreamType.Float64:
        case DataStreamType.PackedFloat32:
          CreateBufferOfType<double>(initialItemCount, componentCount, (double)0);
          break;
        default:
          break;
      }
    }

    private void ExpandOfType<T>(int capacity, T defaultValue)
    {
      IList<T> buffer = (IList<T>)_buffer;
      for (int i = Count; i < capacity; ++i)
      {
        for (int c = 0; c < ComponentCount; ++c)
        {
          buffer.Add(defaultValue);
        }
      }
    }

    private void Expand(int capacity)
    {
      if (_internalType == typeof(sbyte))
      {
        ExpandOfType<sbyte>(capacity, (sbyte)0);
      }
      else if (_internalType == typeof(byte))
      {
        ExpandOfType<byte>(capacity, (byte)0);
      }
      else if (_internalType == typeof(short))
      {
        ExpandOfType<short>(capacity, (short)0);
      }
      else if (_internalType == typeof(ushort))
      {
        ExpandOfType<ushort>(capacity, (ushort)0);
      }
      else if (_internalType == typeof(int))
      {
        ExpandOfType<int>(capacity, (int)0);
      }
      else if (_internalType == typeof(uint))
      {
        ExpandOfType<uint>(capacity, (uint)0);
      }
      else if (_internalType == typeof(long))
      {
        ExpandOfType<long>(capacity, (long)0);
      }
      else if (_internalType == typeof(ulong))
      {
        ExpandOfType<ulong>(capacity, (ulong)0);
      }
      else if (_internalType == typeof(float))
      {
        ExpandOfType<float>(capacity, (float)0);
      }
      else if (_internalType == typeof(double))
      {
        ExpandOfType<double>(capacity, (double)0);
      }
      else
      {
        // TODO: throw
      }
    }

    #region Get<Type> functions

    public sbyte GetSByte(int index)
    {
      return _converter.GetSByte(_buffer, index);
    }

    public void GetRangeSByte(IList<sbyte> range, int startElementIndex, int count)
    {
      _converter.GetRangeSByte(range, _buffer, startElementIndex, count);
    }

    public byte GetByte(int index)
    {
      return _converter.GetByte(_buffer, index);
    }

    public void GetRangeByte(IList<byte> range, int startElementIndex, int count)
    {
      _converter.GetRangeByte(range, _buffer, startElementIndex, count);
    }

    public short GetInt16(int index)
    {
      return _converter.GetInt16(_buffer, index);
    }

    public void GetRangeInt16(IList<short> range, int startElementIndex, int count)
    {
      _converter.GetRangeInt16(range, _buffer, startElementIndex, count);
    }

    public ushort GetUInt16(int index)
    {
      return _converter.GetUInt16(_buffer, index);
    }

    public void GetRangeUInt16(IList<ushort> range, int startElementIndex, int count)
    {
      _converter.GetRangeUInt16(range, _buffer, startElementIndex, count);
    }

    public int GetInt32(int index)
    {
      return _converter.GetInt32(_buffer, index);
    }

    public void GetRangeInt32(IList<int> range, int startElementIndex, int count)
    {
      _converter.GetRangeInt32(range, _buffer, startElementIndex, count);
    }

    public uint GetUInt32(int index)
    {
      return _converter.GetUInt32(_buffer, index);
    }

    public void GetRangeUInt32(IList<uint> range, int startElementIndex, int count)
    {
      _converter.GetRangeUInt32(range, _buffer, startElementIndex, count);
    }

    public long GetInt64(int index)
    {
      return _converter.GetInt64(_buffer, index);
    }

    public void GetRangeInt64(IList<long> range, int startElementIndex, int count)
    {
      _converter.GetRangeInt64(range, _buffer, startElementIndex, count);
    }

    public ulong GetUInt64(int index)
    {
      return _converter.GetUInt64(_buffer, index);
    }

    public void GetRangeUInt64(IList<ulong> range, int startElementIndex, int count)
    {
      _converter.GetRangeUInt64(range, _buffer, startElementIndex, count);
    }

    public float GetSingle(int index)
    {
      return _converter.GetSingle(_buffer, index);
    }

    public void GetRangeSingle(IList<float> range, int startElementIndex, int count)
    {
      _converter.GetRangeSingle(range, _buffer, startElementIndex, count);
    }

    public double GetDouble(int index)
    {
      return _converter.GetDouble(_buffer, index);
    }

    public void GetRangeDouble(IList<double> range, int startElementIndex, int count)
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