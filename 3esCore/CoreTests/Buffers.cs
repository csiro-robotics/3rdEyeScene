// // Copyright (c) CSIRO 2017
// // Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// // ABN 41 687 119 230
// //
// // author Kazys Stepanas
//
using Xunit;
using System;
using System.Reflection;
using System.Collections.Generic;
using Tes.Buffers;
using Tes.IO;
using Tes.Net;
using Tes.Maths;
using Tes.TestSupport;

namespace Tes.CoreTests
{
  public class Buffers
  {
    private VertexBuffer TestPacktised(VertexBuffer referenceBuffer, DataStreamType sendType, double quantisationUnit = 1e-1)
    {
      // Prepare the packet buffer.
      // Keep a relatively small size to ensure we need more than one cycle.
      PacketBuffer packet = new PacketBuffer(16 * 1024);

      // Create a receiving buffer.
      VertexBuffer recvBuffer = new VertexBuffer();

      // Start write/read cycle.
      uint offset = 0;
      while (offset < referenceBuffer.Count)
      {
        // We will cheat here by having no specific message type and only focus on the payload.
        packet.Reset(0, 0);
        uint written = 0;
        if (sendType != DataStreamType.PackedFloat16 && sendType != DataStreamType.PackedFloat32)
        {
          written = referenceBuffer.Write(packet, offset, sendType, (uint)(packet.Data.Length - packet.Count));
        }
        else
        {
          written = referenceBuffer.WritePacked(packet, offset, sendType, (uint)(packet.Data.Length - packet.Count), quantisationUnit);
        }
        Assert.True(written > 0);
        offset += written;

        // Read the data we just wrote.
        NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));
        recvBuffer.Read(packet, packetReader);
      }

      // Validate the data.
      Assert.Equal(referenceBuffer.AddressableCount, recvBuffer.AddressableCount);

      for (int i = 0; i < referenceBuffer.Count; ++i)
      {
        // This will definitely fail. need to assert near.
        for (int c = 0; c < referenceBuffer.ComponentCount; ++c)
        {
          int refIndex = i * referenceBuffer.ElementStride + c;
          int testIndex = i * recvBuffer.ElementStride + c;
          switch (sendType)
          {
            case DataStreamType.Int8:
              Assert.Equal(referenceBuffer.GetSByte(refIndex), recvBuffer.GetSByte(testIndex));
              break;
            case DataStreamType.UInt8:
              Assert.Equal(referenceBuffer.GetByte(refIndex), recvBuffer.GetByte(testIndex));
              break;
            case DataStreamType.Int16:
              Assert.Equal(referenceBuffer.GetInt16(refIndex), recvBuffer.GetInt16(testIndex));
              break;
            case DataStreamType.UInt16:
              Assert.Equal(referenceBuffer.GetUInt16(refIndex), recvBuffer.GetUInt16(testIndex));
              break;
            case DataStreamType.Int32:
              Assert.Equal(referenceBuffer.GetInt32(refIndex), recvBuffer.GetInt32(testIndex));
              break;
            case DataStreamType.UInt32:
              Assert.Equal(referenceBuffer.GetUInt32(refIndex), recvBuffer.GetUInt32(testIndex));
              break;
            case DataStreamType.Int64:
              Assert.Equal(referenceBuffer.GetInt64(refIndex), recvBuffer.GetInt64(testIndex));
              break;
            case DataStreamType.UInt64:
              Assert.Equal(referenceBuffer.GetUInt64(refIndex), recvBuffer.GetUInt64(testIndex));
              break;
            case DataStreamType.Float32:
              Assert.Equal(referenceBuffer.GetSingle(refIndex), recvBuffer.GetSingle(testIndex));
              break;
            case DataStreamType.Float64:
              Assert.Equal(referenceBuffer.GetDouble(refIndex), recvBuffer.GetDouble(testIndex));
              break;
            case DataStreamType.PackedFloat16:
              AssertExt.Near(referenceBuffer.GetSingle(refIndex), recvBuffer.GetSingle(testIndex), (float)quantisationUnit);
              break;
            case DataStreamType.PackedFloat32:
              AssertExt.Near(referenceBuffer.GetDouble(refIndex), recvBuffer.GetDouble(testIndex), quantisationUnit);
              break;
          }
        }
      }

      return recvBuffer;
    }

    private void TestReadSByte<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, sbyte> compare)
    {
      List<sbyte> readItems = new List<sbyte>();
      buffer.GetRangeSByte(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetSByte(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadByte<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, byte> compare)
    {
      List<byte> readItems = new List<byte>();
      buffer.GetRangeByte(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetByte(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadInt16<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, short> compare)
    {
      List<short> readItems = new List<short>();
      buffer.GetRangeInt16(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetInt16(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadUInt16<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, ushort> compare)
    {
      List<ushort> readItems = new List<ushort>();
      buffer.GetRangeUInt16(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetUInt16(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadInt32<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, int> compare)
    {
      List<int> readItems = new List<int>();
      buffer.GetRangeInt32(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetInt32(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadUInt32<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, uint> compare)
    {
      List<uint> readItems = new List<uint>();
      buffer.GetRangeUInt32(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetUInt32(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadInt64<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, long> compare)
    {
      List<long> readItems = new List<long>();
      buffer.GetRangeInt64(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetInt64(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadUInt64<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, ulong> compare)
    {
      List<ulong> readItems = new List<ulong>();
      buffer.GetRangeUInt64(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetUInt64(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadSingle<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, float> compare)
    {
      List<float> readItems = new List<float>();
      buffer.GetRangeSingle(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetSingle(i));
        compare(i, reference, readItems[i]);
      }
    }

    private void TestReadDouble<T>(VertexBuffer buffer, List<T> reference, Action<int, List<T>, double> compare)
    {
      List<double> readItems = new List<double>();
      buffer.GetRangeDouble(readItems, 0, buffer.AddressableCount);

      Assert.Equal(buffer.AddressableCount, readItems.Count);
      for (int i = 0; i < readItems.Count; ++i)
      {
        compare(i, reference, buffer.GetDouble(i));
        compare(i, reference, readItems[i]);
      }
    }

    [Fact]
    public void SByte()
    {
      List<sbyte> reference = new List<sbyte>();
      for (sbyte i = -127; i < 127; ++i)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.Int8);
    }

    [Fact]
    public void Byte()
    {
      List<byte> reference = new List<byte>();
      for (byte i = 0; i < 255; ++i)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.UInt8);
    }

    [Fact]
    public void Int16()
    {
      List<short> reference = new List<short>();
      for (short i = -32000; i < 32000; ++i)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.Int16);
    }

    [Fact]
    public void UInt16()
    {
      List<ushort> reference = new List<ushort>();
      for (ushort i = 0; i < 64000; ++i)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.UInt16);
    }

    [Fact]
    public void Int32()
    {
      List<int> reference = new List<int>();
      for (int i = -128000; i < 128000; ++i)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.Int32);
    }

    [Fact]
    public void UInt32()
    {
      List<uint> reference = new List<uint>();
      for (uint i = 0; i < 128000; ++i)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.UInt32);
    }

    [Fact]
    public void Single()
    {
      List<float> reference = new List<float>();
      for (float i = -64000.0f; i < 64000.0f; i += 2.5f)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.Float32);
      TestPacktised(referenceBuffer, DataStreamType.PackedFloat16, 2.5f);
    }

    [Fact]
    public void Double()
    {
      List<double> reference = new List<double>();
      for (double i = -128000.0; i < 128000.0; i += 2.5)
      {
        reference.Add(i);
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i], val));

      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.Float64);
      TestPacktised(referenceBuffer, DataStreamType.PackedFloat32, 2.5);
    }

    [Fact]
    public void Vector2()
    {
      List<Vector2> reference = new List<Vector2>();
      for (float i = -200.0f; i < 200.0f; i += 3.2f)
      {
        reference.Add(new Vector2(i, 0.5f * i));
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i / 2][i % 2], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i / 2][i % 2], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i / 2][i % 2], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i / 2][i % 2], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i / 2][i % 2], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i / 2][i % 2], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i / 2][i % 2], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i / 2][i % 2], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i / 2][i % 2], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i / 2][i % 2], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i / 2][i % 2], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i / 2][i % 2], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i / 2][i % 2], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i / 2][i % 2], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i / 2][i % 2], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i / 2][i % 2], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i / 2][i % 2], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i / 2][i % 2], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i / 2][i % 2], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i / 2][i % 2], val));
    }

    [Fact]
    public void Vector3()
    {
      List<Vector3> reference = new List<Vector3>();
      for (float i = -200.0f; i < 200.0f; i += 3.2f)
      {
        reference.Add(new Vector3(i, 0.5f * i, -0.5f * i));
      }

      VertexBuffer buffer = VertexBuffer.Wrap(reference);
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i / 3][i % 3], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i / 3][i % 3], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i / 3][i % 3], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i / 3][i % 3], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i / 3][i % 3], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i / 3][i % 3], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i / 3][i % 3], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i / 3][i % 3], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i / 3][i % 3], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i / 3][i % 3], val));

      buffer = VertexBuffer.Wrap(reference.ToArray());
      TestReadSByte(buffer, reference, (i, refList, val) => Assert.Equal((sbyte)refList[i / 3][i % 3], val));
      TestReadByte(buffer, reference, (i, refList, val) => Assert.Equal((byte)refList[i / 3][i % 3], val));
      TestReadInt16(buffer, reference, (i, refList, val) => Assert.Equal((short)refList[i / 3][i % 3], val));
      TestReadUInt16(buffer, reference, (i, refList, val) => Assert.Equal((ushort)refList[i / 3][i % 3], val));
      TestReadInt32(buffer, reference, (i, refList, val) => Assert.Equal((int)refList[i / 3][i % 3], val));
      TestReadUInt32(buffer, reference, (i, refList, val) => Assert.Equal((uint)refList[i / 3][i % 3], val));
      TestReadInt64(buffer, reference, (i, refList, val) => Assert.Equal((long)refList[i / 3][i % 3], val));
      TestReadUInt64(buffer, reference, (i, refList, val) => Assert.Equal((ulong)refList[i / 3][i % 3], val));
      TestReadSingle(buffer, reference, (i, refList, val) => Assert.Equal((float)refList[i / 3][i % 3], val));
      TestReadDouble(buffer, reference, (i, refList, val) => Assert.Equal((double)refList[i / 3][i % 3], val));
    }

    [Fact]
    public void PacketConversion()
    {
      // Build a reference array.
      List<float> reference = new List<float>();
      const float increment = 2.5f;
      // Ensure we remain in byte range.
      float value = 0.0f;
      for (; value < 127.0f; value += increment)
      {
        reference.Add(value);
      }
      // Wrap the buffer.
      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      TestPacktised(referenceBuffer, DataStreamType.Int8);
      TestPacktised(referenceBuffer, DataStreamType.UInt8);

      // Expand into short range
      for (; value < 16000.0f; value += increment)
      {
        reference.Add(value);
      }

      TestPacktised(referenceBuffer, DataStreamType.Int16);
      TestPacktised(referenceBuffer, DataStreamType.UInt16);

      // This is also before the limit of a 16-bit quantised buffer.
      TestPacktised(referenceBuffer, DataStreamType.PackedFloat16, increment);

      // Expand into int range
      for (; value < 128000.0f; value += increment)
      {
        reference.Add(value);
      }

      TestPacktised(referenceBuffer, DataStreamType.Int32);
      TestPacktised(referenceBuffer, DataStreamType.UInt32);
      TestPacktised(referenceBuffer, DataStreamType.Int64);
      TestPacktised(referenceBuffer, DataStreamType.UInt64);
      TestPacktised(referenceBuffer, DataStreamType.Float32);
      TestPacktised(referenceBuffer, DataStreamType.Float64);
      TestPacktised(referenceBuffer, DataStreamType.PackedFloat32, increment);
    }

    [Fact]
    void PacketVector3()
    {
      // Generate a Vector3 array.
      List<Vector3> reference = new List<Vector3>();
      for (float i = -200.0f; i < 200.0f; i += 0.12f)
      {
        reference.Add(new Vector3(i, 0.5f * i, -0.5f * i));
      }

      // Wrap into a VertexBuffer
      VertexBuffer referenceBuffer = VertexBuffer.Wrap(reference);
      // Test unpacking into various formats.
      // Pack/unpack as is.
      VertexBuffer recvBuffer = TestPacktised(referenceBuffer, DataStreamType.Float32);
      Assert.Equal(reference.Count, recvBuffer.Count);
      Assert.Equal(3, recvBuffer.ComponentCount);
      // Pack using doubles
      recvBuffer = TestPacktised(referenceBuffer, DataStreamType.Float64);
      Assert.Equal(reference.Count, recvBuffer.Count);
      Assert.Equal(3, recvBuffer.ComponentCount);
      // Pack into a quantised, 32-bit buffer
      recvBuffer = TestPacktised(referenceBuffer, DataStreamType.PackedFloat32, 0.5 * 0.12);
      Assert.Equal(reference.Count, recvBuffer.Count);
      Assert.Equal(3, recvBuffer.ComponentCount);
      // Pack into a quantised, 16-bit buffer
      recvBuffer = TestPacktised(referenceBuffer, DataStreamType.PackedFloat16, 0.5f * 0.12f);
      Assert.Equal(reference.Count, recvBuffer.Count);
      Assert.Equal(3, recvBuffer.ComponentCount);

      // Now try extract a Vector3 array and ensure we haven't destroyed out data.
      Vector3[] vectorArray = new Vector3[recvBuffer.Count];
      for (int i = 0; i < recvBuffer.Count; ++i)
      {
        for (int c = 0; c < 3; ++c)
        {
          vectorArray[i][c] = recvBuffer.GetSingle(i * 3 + c);
        }
      }
      Assert.Equal(reference.Count, vectorArray.Length);

      // Compare the array.
      float epsilon = 0.5f * 0.12f;
      for (int i = 0; i < vectorArray.Length; ++i)
      {
        AssertExt.Near(reference[i].X, vectorArray[i].X, epsilon);
        AssertExt.Near(reference[i].Y, vectorArray[i].Y, epsilon);
        AssertExt.Near(reference[i].Z, vectorArray[i].Z, epsilon);
      }
    }
  }
}
