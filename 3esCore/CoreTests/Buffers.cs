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
using Tes.Shapes;

namespace Tes.CoreTests
{
  public class Buffers
  {
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
    }

    [Fact]
    public void Single()
    {
      List<float> reference = new List<float>();
      for (float i = -128000.0f; i < 128000.0f; i += 2.5f)
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
  }
}
