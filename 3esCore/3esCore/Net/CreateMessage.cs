using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Defines an object create message.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct CreateMessage : IMessage
  {
    /// <summary>
    /// <see cref="ObjectMessageID"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)ObjectMessageID.Create; } }
    /// <summary>
    /// User assigned ID of the object to create. Zero for transient objects.
    /// </summary>
    public uint ObjectID;
    /// <summary>
    /// Used defined object categorisation. Used to control visibility.
    /// </summary>
    public ushort Category;
    /// <summary>
    /// <see cref="ObjectFlag"/> bit field.
    /// </summary>
    public ushort Flags;
    /// <summary>
    /// Reserved. Must be zero.
    /// </summary>
    public ushort Reserved;
    /// <summary>
    /// Initial transformation and colour.
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
      Category = reader.ReadUInt16();
      Flags = reader.ReadUInt16();
      Reserved = reader.ReadUInt16();
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
      packet.WriteBytes(BitConverter.GetBytes(Category), true);
      packet.WriteBytes(BitConverter.GetBytes(Flags), true);
      packet.WriteBytes(BitConverter.GetBytes(Reserved), true);
      return Attributes.Write(packet);
    }
  }
}

