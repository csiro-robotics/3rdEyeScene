using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Defines an object update message.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct UpdateMessage : IMessage
  {
    /// <summary>
    /// <see cref="ObjectMessageID"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)ObjectMessageID.Update; } }
    /// <summary>
    /// ID of the object to update.
    /// </summary>
    public uint ObjectID;
    /// <summary>
    /// Flags for the update.
    /// </summary>
    public ushort Flags;
    /// <summary>
    /// Updated object attributes.
    /// </summary>
    public ObjectAttributes Attributes;

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      ObjectID = reader.ReadUInt32();
      Flags = reader.ReadUInt16();
      return Attributes.Read(reader);
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(ObjectID), true);
      packet.WriteBytes(BitConverter.GetBytes(Flags), true);
      return Attributes.Write(packet);
   }
  }
}

