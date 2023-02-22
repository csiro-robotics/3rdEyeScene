// This is a generated file. Do not modify it directly.
using System;
using System.Collections.Generic;
using Tes.Maths;

namespace Tes.Buffers.Converters
{
  /// <summary>
  /// Defines a collection of <see cref="BufferConverter"/> bound to explicit buffer types.
  /// </summary>
  /// <remarks>
  /// A <see cref="BufferConverter"/> may be retrieved for a specific <c>Type</c>. Supported types are all built in
  /// types except for <c>bool</c> and Tes vector types :
  /// [sbyte, byte, short, ushort, int, uint, long, ulong, float, double, double, double]
  /// </remarks>
  internal static class ConverterSet
  {
    /// <summary>
    /// Get the <see cref="BufferConverter"/> for <paramref name="forType"/>
    /// </summary>
    /// <remarks>
    /// <paramref name="forType"/> must be a supported type or an exception will be thrown.
    /// </remarks>
    /// <param name="forType">The type to get a converter for.</param>
    /// <return>The converter for the requested type.</return>
    internal static BufferConverter Get(Type forType)
    {
      return _converters[forType];
    }

    static ConverterSet()
    {
      _converters.Add(typeof(sbyte), new SByteConverter());
      _converters.Add(typeof(byte), new ByteConverter());
      _converters.Add(typeof(short), new Int16Converter());
      _converters.Add(typeof(ushort), new UInt16Converter());
      _converters.Add(typeof(int), new Int32Converter());
      _converters.Add(typeof(uint), new UInt32Converter());
      _converters.Add(typeof(long), new Int64Converter());
      _converters.Add(typeof(ulong), new UInt64Converter());
      _converters.Add(typeof(float), new SingleConverter());
      _converters.Add(typeof(double), new DoubleConverter());
      _converters.Add(typeof(Vector2), new Vector2Converter());
      _converters.Add(typeof(Vector3), new Vector3Converter());
    }

    private static Dictionary<Type, BufferConverter> _converters = new Dictionary<Type, BufferConverter>();
  }
}
