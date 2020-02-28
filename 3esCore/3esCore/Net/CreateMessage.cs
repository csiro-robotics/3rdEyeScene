using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Defines an object create message.
  /// </summary>
  /// <remarks>
  /// When <see cref="ObjectFlag.MultiShape"/> is set, the create message contains a payload containing an array of
  /// <see cref="ObjectAttributes"/> structures. The array is preceded by a single UInt16 which identifies the number
  /// of items in the array. This set of objects is managed on a single object ID. That is, a single object ID will
  /// destroy the set of shapes in a single message. Complex shapes generally do not support this flag, however for
  /// those that do, a second array may follow the first containing complex creation payload data.
  ///
  /// For a multi shape, the Attributes in the create messate are interpreted as a normal 4x4 transform, while colour
  /// is ignored. This transformation matrix is applied to all members of the set.
  ///
  /// Not all shapes support this flag.
  /// </remarks>
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
    /// <remarks>
    /// ID zero identifies transient shapes and can be used multiple times. ID <c>0xFFFFFFFF</c> is reserved for
    /// internal use.
    /// </remarks>
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

    public CreateMessage Clone()
    {
      CreateMessage copy = new CreateMessage();
      copy.ObjectID = ObjectID;
      copy.Category = Category;
      copy.Flags = Flags;
      copy.Reserved = Reserved;
      copy.Attributes = Attributes.Clone();
      return copy;
    }
  }
}

