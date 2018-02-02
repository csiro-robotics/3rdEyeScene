using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Messages used to redefine an existing a mesh resource.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MeshRedefineMessage : IMessage
  {
    /// <summary>
    /// <see cref="MeshMessageType"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)MeshMessageType.Redefine; } }

    /// <summary>
    /// Resource ID for this mesh. Unique amount mesh resources.
    /// </summary>
    public uint MeshID;
    /// <summary>
    /// Redefines the number of vertices in the mesh.
    /// </summary>
    public uint VertexCount;
    /// <summary>
    /// Redefines the number of indices in the mesh.
    /// </summary>
    public uint IndexCount;
    /// <summary>
    /// Redefines the mesh topology. See <see cref="MeshDrawType"/>.
    /// </summary>
    public byte DrawType;
    /// <summary>
    /// Redefines the a pivot for the vertex data and mesh colour.
    /// </summary>
    public ObjectAttributes Attributes;

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      MeshID = reader.ReadUInt32();
      VertexCount = reader.ReadUInt32();
      IndexCount = reader.ReadUInt32();
      DrawType = reader.ReadByte();
      return Attributes.Read(reader);
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(MeshID), true);
      packet.WriteBytes(BitConverter.GetBytes(VertexCount), true);
      packet.WriteBytes(BitConverter.GetBytes(IndexCount), true);
      packet.WriteBytes(BitConverter.GetBytes(DrawType), true);
      return Attributes.Write(packet);
    }
  }
}
