using System;
using System.IO;
using Tes.IO;

namespace Tes.Net
{  
  /// <summary>
  /// A system control message.
  /// </summary>
  public struct ControlMessage : IMessage
  {
    /// <summary>
    /// Flags, particular to this <see cref="ControlMessageID"/> type of message.
    /// </summary>
    public uint ControlFlags;
    /// <summary>
    /// 32-bit value  particular to this <see cref="ControlMessageID"/> type of message.
    /// </summary>
    public uint Value32;
    /// <summary>
    /// 64-bit value  particular to this <see cref="ControlMessageID"/> type of message.
    /// </summary>
    public ulong Value64;

    /// <summary>
    /// Create a control message with the given 32-bit value.
    /// </summary>
    /// <param name="value">Value assigned to the <see cref="Value32"/> field.</param>
    /// <returns>A zeroed message with <paramref name="value"/> assigned to <see cref="Value32"/>.</returns>
    public static ControlMessage Create(uint value)
    {
      return new ControlMessage { ControlFlags = 0, Value32 = value, Value64 = 0 };
    }

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      ControlFlags = reader.ReadUInt32();
      Value32 = reader.ReadUInt32();
      Value64 = reader.ReadUInt64();
      return true;
    }

    /// <summary>
    /// Peek into the given <paramref name="packet"/> and read this message.
    /// Does not modify the cursor or contents of <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to read from.</param>
    /// <returns>True on success.</returns>
    public bool Peek(PacketBuffer packet)
    {
      int offset = PacketHeader.Size;
      if (packet.ValidHeader)
      {
        offset += packet.Header.PayloadOffset;
      }
      ControlFlags = packet.PeekUInt32(offset);
      offset += 4;
      Value32 = packet.PeekUInt32(offset);
      offset += 4;
      Value64 = packet.PeekUInt64(offset);
      //offset += 8;
      return true;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(ControlFlags), true);
      packet.WriteBytes(BitConverter.GetBytes(Value32), true);
      packet.WriteBytes(BitConverter.GetBytes(Value64), true);
      return true;
    }
  }
}

