using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Message for parts of a mesh resource (e.g., vertices).
  /// </summary>
  /// <remarks>
  /// The corresponding <see cref="PacketHeader.MessageID"/> is set to
  /// a <see cref="MeshMessageType"/> for one of the component messages listed below:
  /// <list type="bullet">
  /// <item><see cref="MeshMessageType.Vertex"/></item>
  /// <item><see cref="MeshMessageType.Index"/></item>
  /// <item><see cref="MeshMessageType.VertexColour"/></item>
  /// <item><see cref="MeshMessageType.Normal"/></item>
  /// <item><see cref="MeshMessageType.UV"/></item>
  /// </list>
  /// </remarks>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MeshComponentMessage : IMessage
  {
    /// <summary>
    /// ID of the target mesh.
    /// </summary>
    public uint MeshID;

    /// <summary>
    /// Returns type byte size this structure.
    /// </summary>
    /// <value>The byte size of this structure type.</value>
    public static int Size { get { return Marshal.SizeOf(typeof(MeshComponentMessage)); } }

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
