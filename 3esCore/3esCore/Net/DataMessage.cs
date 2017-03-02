using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Defines an object data message. This is for complex shapes to send
  /// additional creation data piecewise. Not supported for transient shapes.
  ///
  /// This is the message header and the payload follows.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DataMessage
  {
    /// <summary>
    /// <see cref="ObjectMessageID"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)ObjectMessageID.Data; } }
    /// <summary>
    /// ID of the object to which the data belong.
    /// </summary>
    public uint ObjectID;

    /// <summary>
    /// Returns type byte size this structure.
    /// </summary>
    /// <value>The byte size of this structure type.</value>
    public static int Size { get { return Marshal.SizeOf(typeof(DataMessage)); } }

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      ObjectID = reader.ReadUInt32();
      return true;
    }

    /// <summary>
    /// Write this message to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(BinaryWriter writer)
    {
      writer.Write(ObjectID);
      return true;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(ObjectID), true);
      return true;
    }
    }
}

