using System.IO;
using System.Text;

namespace Tes.IO
{
  /// <summary>
  /// A <see cref="BinaryReader"/> implementation designed to read data from
  /// network Endian (big) to the local host Endian.
  /// </summary>
  /// <remarks>
  /// See <see cref="BinaryReader"/> class for details on possible exceptions throw.
  /// </remarks>
  public class NetworkReader : BinaryReader
  {
    /// <summary>
    /// Create a reader to read from the given <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public NetworkReader(Stream stream)
      : base(stream)
    {
    }

    /// <summary>
    /// Create a reader to read from the given <paramref name="stream"/> with encoding.
    /// </summary>
    /// <remarks>
    /// See <see cref="BinaryReader"/> constructor fo details on <paramref name="encoding"/>.
    /// </remarks>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The stream encoding.</param>
    public NetworkReader(Stream stream, Encoding encoding)
      : base(stream, encoding)
    {
    }

    /// <summary>
    /// Read a single precision floating point value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override float ReadSingle()
    {
      return Endian.FromNetwork(base.ReadSingle());
    }

    /// <summary>
    /// Read a double precision floating point value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override double ReadDouble()
    {
      return Endian.FromNetwork(base.ReadDouble());
    }

    /// <summary>
    /// Read a 16-bit integer value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override short ReadInt16()
    {
      return Endian.FromNetwork(base.ReadInt16());
    }

    /// <summary>
    /// Read a 16-bit integer value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override ushort ReadUInt16()
    {
      return Endian.FromNetwork(base.ReadUInt16());
    }

    /// <summary>
    /// Read a 32-bit integer value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override int ReadInt32()
    {
      return Endian.FromNetwork(base.ReadInt32());
    }

    /// <summary>
    /// Read a 32-bit integer value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override uint ReadUInt32()
    {
      return Endian.FromNetwork(base.ReadUInt32());
    }

    /// <summary>
    /// Read a 64-bit integer value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override long ReadInt64()
    {
      return Endian.FromNetwork(base.ReadInt64());
    }

    /// <summary>
    /// Read a 64-bit integer value.
    /// </summary>
    /// <returns>The requested value.</returns>
    public override ulong ReadUInt64()
    {
      return Endian.FromNetwork(base.ReadUInt64());
    }
  }
}
