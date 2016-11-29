using System.IO;
using System.Text;

namespace Tes.IO
{
  /// <summary>
  /// A <see cref="BinaryWriter"/> designed to write to network Endian (big).
  /// </summary>
  /// <remarks>
  /// See <see cref="BinaryWriter"/> class for details on possible exceptions throw.
  /// </remarks>
  public class NetworkWriter : BinaryWriter
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public NetworkWriter() {}

    /// <summary>
    /// Create a writer to writer to the given <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public NetworkWriter(Stream stream)
      : base(stream)
    {
    }

    /// <summary>
    /// Create a writer to writer to the given <paramref name="stream"/> with encoding.
    /// </summary>
    /// <remarks>
    /// See <see cref="BinaryReader"/> constructor fo details on <paramref name="encoding"/>.
    /// </remarks>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="encoding">The stream encoding.</param>
    public NetworkWriter(Stream stream, Encoding encoding)
      : base(stream, encoding)
    {
    }

    /// <summary>
    /// Write a double precision floating point value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(double value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a single precision floating point value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(float value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a 16-bit integer value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(short value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a 16-bit integer value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(ushort value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a 32-bit integer value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(int value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a 32-bit integer value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(uint value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a 64-bit integer value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(long value)
    {
      base.Write(Endian.ToNetwork(value));
    }

    /// <summary>
    /// Write a 64-bit integer value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public override void Write(ulong value)
    {
      base.Write(Endian.ToNetwork(value));
    }
  }
}