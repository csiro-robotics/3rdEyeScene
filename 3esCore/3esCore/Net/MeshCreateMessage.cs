using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Message used to define a new mesh resource.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MeshCreateMessage
  {
    /// <summary>
    /// <see cref="MeshMessageType"/> for this message.
    /// </summary>
    public static ushort MessageID { get { return (ushort)MeshMessageType.Create; } }

    /// <summary>
    /// The mesh resource ID. Unique among mesh resources.
    /// </summary>
    public uint MeshID;
    /// <summary>
    /// The number of vertices in the mesh.
    /// </summary>
    public uint VertexCount;
    /// <summary>
    /// The number of mesh indices.
    /// </summary>
    public uint IndexCount;
    /// <summary>
    /// Defines the topology. See <see cref="MeshDrawType"/>.
    /// </summary>
    public byte DrawType;
    /// <summary>
    /// Defines a local pivot for the vertices and default mesh colour.
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
      //packet.WriteBytes(BitConverter.GetBytes(DrawType), true);
      packet.WriteBytes(new byte[] { DrawType }, true);
      return Attributes.Write(packet);
    }
  }
}
