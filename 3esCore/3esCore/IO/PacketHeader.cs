using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Tes.IO
{
  /// <summary>
  /// Flag values for <see cref="PacketHeader"/>.
  /// </summary>
  [Flags]
  public enum PacketFlag : byte
  {
    /// <summary>
    /// Marks the packet has missing its 16-bit CRC.
    /// </summary>
    NoCrc = (1 << 0)
  }

  ///<summary>
  /// The header for an incoming 3ES data packet. All packet data, including payload
  /// bytes, must be in network endian which is big endian.
  /// </summary>
  ///
  /// <remarks>
  /// A two byte CRC value is to appear immediately after the packet header and payload.
  /// </remarks>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct PacketHeader
  {
    /// <summary>
    /// Marker bytes. Identifies the packet start.
    /// </summary>
    public UInt32 Marker;
    /// <summary>
    /// Packet major version number. May be used to control decoding.
    /// </summary>
    public UInt16 VersionMajor;
    /// <summary>
    /// Packet major version number. May be used to control decoding.
    /// </summary>
    public UInt16 VersionMinor;
    /// <summary>
    /// Identifies the main packet receiver.
    /// </summary>
    public UInt16 RoutingID;
    /// <summary>
    /// Identifies the message ID or message type.
    /// </summary>
    public UInt16 MessageID;
    /// <summary>
    /// Size of the payload (bytes) following this header.
    /// </summary>
    public UInt16 PayloadSize; 
    /// <summary>
    /// A byte offset from the end of the packet header to the payload data.
    /// </summary>
    /// <remarks>
    /// Typically, the offset is zero and the payload begins immediately after
    /// the <see cref="PacketHeader"/>. However, this value serves in part as
    /// future proofing, to support additional information in the
    /// <see cref="PacketHeader"/> immediately following the core data presented
    /// here.
    /// </remarks>
    public byte PayloadOffset;
    /// <summary>
    /// <see cref="PacketFlag"/> values.
    /// </summary>
    public byte Flags;

    /// <summary>
    /// Returns type byte size of a <see cref="PacketHeader"/>.
    /// </summary>
    /// <value>The byte size of this structure type.</value>
    public static int Size { get { return Marshal.SizeOf(typeof(PacketHeader)); } }

    /// <summary>
    /// Returns the byte offset to the PayloadSize member.
    /// </summary>
    /// <value>The byte offset to the payload size member.</value>
    public static int PayloadSizeOffset { get { return Marshal.OffsetOf(typeof(PacketHeader), "PayloadSize").ToInt32(); } }

    /// <summary>
    /// Returns the byte offset to the Flags member.
    /// </summary>
    /// <value>The byte offset to the packet flags member.</value>
    public static int FlagsOffset { get { return Marshal.OffsetOf(typeof(PacketHeader), "Flags").ToInt32(); } }

    /// <summary>
    /// Create a new header with the default values set. This includes the
    /// default marker and version number.
    /// </summary>
    public static PacketHeader Default { get { return Create(0, 0); } }
    
    /// <summary>
    /// Create a default header with the given <paramref name="routingID"/>.
    /// </summary>
    /// <param name="routingID">Assigned to the header's <see cref="RoutingID"/>.</param>
    /// <param name="messageID">Assigned to the header's <see cref="MessageID"/>.</param>
    /// <returns></returns>
    public static PacketHeader Create(ushort routingID, ushort messageID)
    {
      return new PacketHeader
      {
        Marker = PacketMarker,
        VersionMajor = PacketVersionMajor,
        VersionMinor = PacketVersionMinor,
        RoutingID = routingID,
        MessageID = messageID,
        PayloadSize = 0,
        PayloadOffset = 0,
        Flags = 0
      };
    }

    /// <summary>
    /// Calculate and return the size of this packet, including the payload, but not CRC.
    /// </summary>
    /// <remarks>
    /// The size is calculated as the sum of the this header size, the payload offset
    /// (additional header size) and the payload size.
    /// </remarks>
    /// <value>The size of the packet in bytes.</value>
    public int PacketSize
    {
      get
      {
        return Marshal.SizeOf(typeof(PacketHeader)) + PayloadOffset + PayloadSize;
      }
    }

    /// <summary>
    /// Calculate the size of the data in the packet, without the header.
    /// </summary>
    /// <remarks>
    /// This is the number of bytes following the header.
    /// </remarks>
    public int DataSize { get { return PayloadOffset + PayloadSize; } }

    /// <summary>
    /// Write this packet to a network writer.
    /// </summary>
    /// <param name="writer">The network writer.</param>
    /// <returns>True on success</returns>
    public bool Write(BinaryWriter writer)
    {
      writer.Write(Marker);
      writer.Write(VersionMajor);
      writer.Write(VersionMinor);
      writer.Write(RoutingID);
      writer.Write(MessageID);
      writer.Write(PayloadSize);
      writer.Write(PayloadOffset);
      writer.Write(Flags);
      return true;
    }

    /// <summary>
    /// Attempt to reader a packet header.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True if the marker and version number match the expected values.</returns>
    public bool Read(BinaryReader reader)
    {
      Marker = reader.ReadUInt32();
      VersionMajor = reader.ReadUInt16();
      VersionMinor = reader.ReadUInt16();
      RoutingID = reader.ReadUInt16();
      MessageID = reader.ReadUInt16();
      PayloadSize = reader.ReadUInt16();
      PayloadOffset = reader.ReadByte();
      Flags = reader.ReadByte();
      return Marker == PacketMarker && VersionMajor == PacketVersionMajor && VersionMinor == PacketVersionMinor;
    }

    /// <summary>
    /// A four byte sequence used to identify a 3rd Eye Scene packet.
    /// </summary>
    public static readonly UInt32 PacketMarker = 0x03e55e30u;
    /// <summary>
    /// The current major version number for a 3rd Eye Scene packet.
    /// </summary>
    public static readonly UInt16 PacketVersionMajor = (UInt16)0x0u;
    /// <summary>
    /// The current minor version number for a 3rd Eye Scene packet.
    /// </summary>
    public static readonly UInt16 PacketVersionMinor = (UInt16)0x1u;
  }
}
