using System;
using System.Runtime.InteropServices;
using System.IO;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Identifies a message packet which contains a collection of other messages, optionally compressed.
  /// </summary>
  /// <remarks>
  /// A collated packet message contains multiple messages in the standard format: <see cref="PacketHeader"/>
  /// followed by the message data. However, no individual message in the collated packet has its own CRC.
  /// Instead, only the <code>CollatedPacketMessage</code> has a CRC.
  /// 
  /// Use <see cref="Util.CollatedPacketDecoder"/> to decode such packets into constituent messages.
  /// </remarks>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct CollatedPacketMessage
  {
    /// <summary>
    /// See CollatedPacketFlag.
    /// </summary>
    public ushort Flags;
    /// <summary>
    /// Reserved for future use. Must be zero.
    /// </summary>
    public ushort Reserved;
    /// <summary>
    /// Number of uncompressed bytes in the collated packet data.
    /// </summary>
    public uint UncompressedBytes;

    /// <summary>
    /// Returns the byte offset to the <see cref="UncompressedBytes"/> field.
    /// </summary>
    /// <remarks>
    /// FIXME: use reflection to calculate the offset.
    /// </remarks>
    public static int UncompressedBytesOffset
    {
      get { return 8; }
    }

    /// <summary>
    /// Returns the byte size of this structure.
    /// </summary>
    public static int Size { get { return Marshal.SizeOf(typeof(CollatedPacketMessage)); } }

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      Flags = reader.ReadUInt16();
      Reserved = reader.ReadUInt16();
      UncompressedBytes = reader.ReadUInt32();
      //return reader.ok
      return true;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(Flags), true);
      packet.WriteBytes(BitConverter.GetBytes(Reserved), true);
      packet.WriteBytes(BitConverter.GetBytes(UncompressedBytes), true);
      return true;
    }

    /// <summary>
    /// Write this message to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <returns>True</returns>
    public bool Write(BinaryWriter writer)
    {
      writer.Write(Flags);
      writer.Write(Reserved);
      writer.Write(UncompressedBytes);
      return true;
    }
  }
}
