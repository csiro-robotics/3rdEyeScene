using System;
using System.IO;

namespace Tes.Buffers
{
  /// <summary>
  /// A utility class to handle reading from a 16-bit packed data buffer into a <see cref="VertexBuffer"/>.
  /// Assumes the target type is <c>float</c>.
  /// </summary>
  class PackedFloat16Reader : BinaryReader
  {
    /// <summary>
    /// Create a <c>PackedFloat16Reader</c> around a <c>BinaryReader</c>
    /// </summary>
    /// <param name="sourceReader">The underlying <c>BinaryReader</c> to read data from.</param>
    /// <param name="componentCount">The number of components for each element to read from the buffer.</param>
    /// <remarks>
    /// This immediately reads the quantisation unit (float) and the origin (float array of
    /// <paramref name="componentCount"/> items) from the <paramref name="sourceReader"/>.
    /// </remarks>
    public PackedFloat16Reader(BinaryReader sourceReader, int componentCount)
      : base(sourceReader.BaseStream)
    {
      _reader = sourceReader;
      _componentIndex = 0;
      _componentCount = componentCount;
      _origin = new float[componentCount];

      // Read the quantisation unit and origin.
      _quantisationUnit = _reader.ReadSingle();

      for (int i = 0; i < componentCount; ++i)
      {
        _origin[i] = _reader.ReadSingle();
      }
    }

    /// <summary>
    /// The only valid reading function: unpack a packed 16-bit value into a float.
    /// </summary>
    /// <returns>An unpacked <c>float</c> value read from the wrapped <c>BinaryReader</c>.</returns>
    public override float ReadSingle()
    {
      short packedValue = _reader.ReadInt16();
      float unpackedValue = (float)packedValue * _quantisationUnit + _origin[_componentIndex];
      _componentIndex = (_componentIndex + 1) % _componentCount;
      return unpackedValue;
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override double ReadDouble()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadDouble() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override sbyte ReadSByte()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadSByte() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override byte ReadByte()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadByte() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override short ReadInt16()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadInt16() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override ushort ReadUInt16()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadUInt16() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override int ReadInt32()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadInt32() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override uint ReadUInt32()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadUInt32() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override long ReadInt64()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadInt64() not supported");
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Noting: throws</returns>
    /// <exception cref="NotImplementedException">Thrown always</exception>
    public override ulong ReadUInt64()
    {
      throw new NotImplementedException("PackedFloat16Reader.ReadUInt64() not supported");
    }

    private BinaryReader _reader;
    private int _componentIndex = 0;
    private int _componentCount = 1;
    private float _quantisationUnit = 1.0f;
    private float[] _origin;
  }
}