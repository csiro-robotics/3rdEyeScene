using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Flag values for <see cref="MeshCreateMessage"/>.
  /// </summary>
  public enum MeshCreateFlag
  {
    /// <summary>
    /// Indicates the use of double precision floating point values.
    /// </summary>
    DoublePrecision = (1 << 0),
  };

  /// <summary>
  /// Message used to define a new mesh resource.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MeshCreateMessage : IMessage
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
    /// <see cref="MeshCreateFlag"/> values.
    /// </summary>
    public ushort Flags;
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
      Flags = reader.ReadUInt16();
      DrawType = reader.ReadByte();
      bool readDoublePrecision = ((Flags & (ushort)MeshCreateFlag.DoublePrecision) != 0);
      return Attributes.Read(reader, readDoublePrecision);
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
      packet.WriteBytes(BitConverter.GetBytes(Flags), true);
      packet.WriteBytes(new byte[] { DrawType }, true);
      bool writeDoublePrecision = ((Flags & (ushort)MeshCreateFlag.DoublePrecision) != 0);
      return Attributes.Write(packet, writeDoublePrecision);
    }
  }
}
