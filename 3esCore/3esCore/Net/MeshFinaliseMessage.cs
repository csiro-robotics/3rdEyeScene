using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Mesh finalisation flags controlling how to complete the mesh.
  /// </summary>
  [Flags]
  public enum MeshFinaliseFlag
  {
    /// <summary>
    /// Zero flag.
    /// </summary>
    Zero = 0,
    /// <summary>
    /// Instruct the calculation of vertex normals.
    /// </summary>
    CalculateNormals = (1 << 0)
  }

  /// <summary>
  /// Message used to (re)finalise a mesh resource. The mesh resource becomes usable after this message.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MeshFinaliseMessage
  {
    /// <summary>
    /// <see cref="MeshMessageType"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)MeshMessageType.Finalise; } }

    /// <summary>
    /// The mesh resource ID. Unique among mesh resources.
    /// </summary>
    public uint MeshID;
    /// <summary>
    /// <see cref="MeshFinaliseFlag"/> values.
    /// </summary>
    public uint Flags;

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      MeshID = reader.ReadUInt32();
      Flags = reader.ReadUInt32();
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
      packet.WriteBytes(BitConverter.GetBytes(Flags), true);
      return true;
    }
  }
}
