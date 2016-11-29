using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Message sent to destroy a previously defined mesh resource.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MeshDestroyMessage
  {
    /// <summary>
    /// <see cref="MeshMessageType"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)MeshMessageType.Destroy; } }

    /// <summary>
    /// The mesh resource ID. Unique among mesh resources.
    /// </summary>
    public uint MeshID;

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      MeshID = reader.ReadUInt32();
      return true;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(MeshID), true);
      return true;
    }
  }
}
