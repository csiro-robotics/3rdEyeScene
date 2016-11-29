using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Defines an object destroy message.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DestroyMessage
  {
    /// <summary>
    /// <see cref="ObjectMessageID"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)ObjectMessageID.Destroy; } }
    /// <summary>
    /// ID of the object to destroy.
    /// </summary>
    public uint ObjectID;

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

