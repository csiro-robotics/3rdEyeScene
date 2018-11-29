
using System;

namespace Tes.IO
{
  /// <summary>
  /// Endian swap operations.
  /// </summary>
  /// <remarks>
  /// Supports explicit swapping of various types via <see cref="Swap(byte[])"/> functions
  /// or swapping to target Endian via <see cref="ToBig(int)"/>, <see cref="ToLittle(int)"/>
  /// and <see cref="ToNetwork(int)"/> or <see cref="FromNetwork(int)"/>.
  /// </remarks>
  public static class Endian
  {
    /// <summary>
    /// Performs an Endian swap on the given byte array, reversing the byte order.
    /// </summary>
    /// <param name="bytes">The byte array to re-order.</param>
    public static byte[] Swap(byte[] bytes)
    {
      byte bt;
      for (int i = 0, j = bytes.Length - 1; i < bytes.Length / 2; ++i, --j)
      {
        bt = bytes[i];
        bytes[i] = bytes[j];
        bytes[j] = bt;
      }
      return bytes;
    }

    /// <summary>
    /// A 1-byte value Endian swap: noop.
    /// </summary>
    /// <remarks>
    /// For completeness.
    /// </remarks>
    /// <param name="val">The 1-byte value.</param>
    /// <returns><paramref name="val"/> as is.</returns>
    public static byte Swap(byte val) { return val; }
    /// <summary>
    /// A 1-byte value Endian swap: noop.
    /// </summary>
    /// <remarks>
    /// For completeness.
    /// </remarks>
    /// <param name="val">The 1-byte value.</param>
    /// <returns><paramref name="val"/> as is.</returns>
    public static sbyte Swap(sbyte val) { return val; }

    /// <summary>
    /// Perform a 2-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 2-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt16 Swap(UInt16 val)
    {
      return BitConverter.ToUInt16(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform a 2-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 2-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int16 Swap(Int16 val)
    {
      return BitConverter.ToInt16(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform a 4-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 4-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt32 Swap(UInt32 val)
    {
      return BitConverter.ToUInt32(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform a 4-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 4-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int32 Swap(Int32 val)
    {
      return BitConverter.ToInt32(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform an 8-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 8-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt64 Swap(UInt64 val)
    {
      return BitConverter.ToUInt64(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform an 8-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 8-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int64 Swap(Int64 val)
    {
      return BitConverter.ToInt32(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform a 4-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 4-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static float Swap(float val)
    {
      return BitConverter.ToSingle(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Perform an 8-byte value Endian swap.
    /// </summary>
    /// <param name="val">The 8-byte value.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static double Swap(double val)
    {
      return BitConverter.ToSingle(Swap(BitConverter.GetBytes(val)), 0);
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <remarks>
    /// Always noop on a one byte value. Provided for completeness.
    /// </remarks>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static byte ToLittle(byte val) { return val; }
    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <remarks>
    /// Always noop on a one byte value. Provided for completeness.
    /// </remarks>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static sbyte ToLittle(sbyte val) { return val; }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt16 ToLittle(UInt16 val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToUInt16(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int16 ToLittle(Int16 val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToInt16(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt32 ToLittle(UInt32 val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToUInt32(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int32 ToLittle(Int32 val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToInt32(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt64 ToLittle(UInt64 val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToUInt64(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int64 ToLittle(Int64 val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToInt32(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static float ToLittle(float val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToSingle(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static double ToLittle(double val)
    {
      return (!BitConverter.IsLittleEndian) ? BitConverter.ToDouble(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <remarks>
    /// Always noop on a one byte value. Provided for completeness.
    /// </remarks>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static byte ToBig(byte val) { return val; }
    /// <summary>
    /// Convert a value to little Endian. Does nothing if the host platform is little endian.
    /// </summary>
    /// <remarks>
    /// Always noop on a one byte value. Provided for completeness.
    /// </remarks>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static sbyte ToBig(sbyte val) { return val; }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt16 ToBig(UInt16 val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToUInt16(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int16 ToBig(Int16 val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToInt16(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt32 ToBig(UInt32 val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToUInt32(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int32 ToBig(Int32 val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToInt32(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt64 ToBig(UInt64 val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToUInt64(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int64 ToBig(Int64 val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToInt32(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static float ToBig(float val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToSingle(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to big Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static double ToBig(double val)
    {
      return (BitConverter.IsLittleEndian) ? BitConverter.ToDouble(Swap(BitConverter.GetBytes(val)), 0) : val;
    }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="bytes">The value to endian swap.</param>
    /// <returns><paramref name="bytes"/> with a swapped byte order.</returns>
    public static byte[] ToNetwork(byte[] bytes) { return (BitConverter.IsLittleEndian) ? Swap(bytes) : bytes; }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <remarks>
    /// Always noop on a one byte value. Provided for completeness.
    /// </remarks>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static byte ToNetwork(byte val) { return val; }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <remarks>
    /// Always noop on a one byte value. Provided for completeness.
    /// </remarks>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static sbyte ToNetwork(sbyte val) { return val; }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt16 ToNetwork(UInt16 val) { return ToBig(val); }
    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int16 ToNetwork(Int16 val) { return ToBig(val); }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt32 ToNetwork(UInt32 val) { return ToBig(val); }
    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int32 ToNetwork(Int32 val) { return ToBig(val); }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt64 ToNetwork(UInt64 val) { return ToBig(val); }
    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int64 ToNetwork(Int64 val) { return ToBig(val); }

    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static float ToNetwork(float val) { return ToBig(val); }
    /// <summary>
    /// Convert a value to network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static double ToNetwork(double val) { return ToBig(val); }

    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static byte FromNetwork(byte val) { return val; }
    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static sbyte FromNetwork(sbyte val) { return val; }

    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt16 FromNetwork(UInt16 val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }
    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int16 FromNetwork(Int16 val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }

    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt32 FromNetwork(UInt32 val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }
    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int32 FromNetwork(Int32 val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }

    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static UInt64 FromNetwork(UInt64 val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }
    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static Int64 FromNetwork(Int64 val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }

    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static float FromNetwork(float val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }
    /// <summary>
    /// Convert a value from network (big) Endian. Does nothing if the host platform is big Endian.
    /// </summary>
    /// <param name="val">The value to endian swap.</param>
    /// <returns><paramref name="val"/> with a swapped byte order.</returns>
    public static double FromNetwork(double val) { return (BitConverter.IsLittleEndian) ? Swap(val) : val; }

    /// <summary>
    /// Network to local Endian swap for an arbitrary byte array. Reversed if an swap is required.
    /// </summary>
    /// <param name="bytes">The byte array to operate on.</param>
    /// <returns>The <paramref name="bytes"/> after potential modification. Returned for convenience.</returns>
    /// <remarks>
    /// The <paramref name="bytes"/> array is modified in place and also returned.
    /// </remarks>
    public static byte[] FromNetwork(byte[] bytes) { if (BitConverter.IsLittleEndian) { Swap(bytes); } return bytes; }
  }
}
