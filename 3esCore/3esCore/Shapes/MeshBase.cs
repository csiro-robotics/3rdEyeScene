using System;
using System.IO;
using Tes.Buffers;
using Tes.IO;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  /// <summary>
  /// An implementation of <see cref="MeshResource"/> which implements transfer.
  /// Interface functions are declared as abstract (some exceptions).
  /// </summary>
  public abstract class MeshBase : MeshResource
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    public MeshBase()
    {
      Transform = Matrix4.Identity;
    }

    /// <summary>
    /// Unique ID for the mesh.
    /// </summary>
    public uint ID { get; set; }

    /// <summary>
    /// The Mesh type ID.
    /// </summary>
    public ushort TypeID { get { return (ushort)Tes.Net.RoutingID.Mesh; } }

    /// <summary>
    /// Root transform for the mesh. This defines the origin.
    /// </summary>
    public Matrix4 Transform { get; set; }
    /// <summary>
    /// A global tint colour applied to the mesh.
    /// </summary>
    public uint Tint { get; set; }
    /// <summary>
    /// The <see cref="DrawType"/> of the mesh.
    /// </summary>
    public byte DrawType
    {
      get { return (byte)MeshDrawType; }
      set { MeshDrawType = (Tes.Net.MeshDrawType)value; }
    }

    /// <summary>
    /// Flag the mesh to calculate normals at the client when none are provided here?
    /// </summary>
    public bool CalculateNormals { get; set; }

    /// <summary>
    /// The <see cref="DrawType"/> of the mesh.
    /// </summary>
    public MeshDrawType MeshDrawType { get; set; }

    /// <summary>
    /// Identifies which <see cref="MeshComponentFlag"/> items are present.
    /// </summary>
    public MeshComponentFlag Components { get; protected set; }

    /// <summary>
    /// Defines the byte size used by indices in this mesh.
    /// </summary>
    /// <value>The size of each index value in bytes.</value>
    public abstract int IndexSize { get; }

    /// <summary>
    /// Exposes the number of vertices in the mesh.
    /// </summary>
    /// <returns>The number of vertices in this mesh.</returns>
    /// <param name="stream">For future use. Must be zero.</param>
    public abstract uint VertexCount(int stream = 0);

    /// <summary>
    /// Exposes the number of indices in the mesh.
    /// </summary>
    /// <returns>The number of indices in this mesh.</returns>
    /// <param name="stream">For future use. Must be zero.</param>
    public abstract uint IndexCount(int stream = 0);

    /// <summary>
    /// Supports iteration of the vertices of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public abstract Vector3[] Vertices(int stream = 0);

    /// <summary>
    /// Supports iteration of the indices of the mesh when using two byte indices.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public virtual ushort[] Indices2(int stream = 0) { throw new NotSupportedException("Indices2 not supported."); }
    /// <summary>
    /// Supports iteration of the indices of the mesh when using four byte indices.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public virtual int[] Indices4(int stream = 0) { throw new NotSupportedException("Indices4 not supported."); }

    /// <summary>
    /// Supports iteration of the normal of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public abstract Vector3[] Normals(int stream = 0);

    /// <summary>
    /// Supports iteration of the UV coordinates of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public abstract Vector2[] UVs(int stream = 0);

    /// <summary>
    /// Supports iteration of per vertex colours of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    public abstract uint[] Colours(int stream = 0);


    #region Resource transfer - write
    /// <summary>
    /// <see cref="Resource.Transfer(PacketBuffer, int, ref TransferProgress)"/> phases enumeration.
    /// </summary>
    public enum Phase
    {
      /// <summary>
      /// Vertex data transfer.
      /// </summary>
      Vertex,
      /// <summary>
      /// Index data transfer.
      /// </summary>
      Index,
      /// <summary>
      /// Vertex normal data transfer.
      /// </summary>
      Normal,
      /// <summary>
      /// Vertex colour data transfer.
      /// </summary>
      Colour,
      /// <summary>
      /// Vertex UV data transfer.
      /// </summary>
      UV,
      /// <summary>
      /// Finalisation.
      /// </summary>
      Finalise,
      /// <summary>
      /// Done.
      /// </summary>
      End
    }

    /// <summary>
    /// Helper to set the next phase item.
    /// </summary>
    /// <returns>The next phase after <paramref name="phase"/>.</returns>
    /// <param name="phase">The current phase.</param>
    public Phase NextPhase(Phase phase)
    {
      bool selected = phase == Phase.End;
      while (!selected)
      {
        phase = (Phase)((int)phase + 1);
        switch (phase)
        {
          case Phase.Vertex:
            selected = (Components & MeshComponentFlag.Vertex) == MeshComponentFlag.Vertex && Vertices() != null;
            break;
          case Phase.Index:
            {
              switch (IndexSize)
              {
                default:
                case 0:
                  selected = false;
                  break;

                case 2:
                  selected = (Components & MeshComponentFlag.Index) == MeshComponentFlag.Index && Indices2() != null;
                  break;

                case 4:
                  selected = (Components & MeshComponentFlag.Index) == MeshComponentFlag.Index && Indices4() != null;
                  break;
              }
            }
            break;
          case Phase.Normal:
            selected = (Components & MeshComponentFlag.Normal) == MeshComponentFlag.Normal && Normals() != null;
            break;
          case Phase.Colour:
            selected = (Components & MeshComponentFlag.Colour) == MeshComponentFlag.Colour && Colours() != null;
            break;
          case Phase.UV:
            selected = (Components & MeshComponentFlag.UV) == MeshComponentFlag.UV && UVs() != null;
            break;
          case Phase.Finalise:
            selected = true;
            break;
          case Phase.End:
            selected = true;
            break;
        }
      }

      return phase;
    }

    /// <summary>
    /// Send initial mesh creation message.
    /// </summary>
    /// <param name="packet">Packet to populate with the create message.</param>
    /// <returns>Zero on success.</returns>
    public int Create(PacketBuffer packet)
    {
      MeshCreateMessage msg = new MeshCreateMessage();
      msg.Attributes.SetFromTransform(Transform);
      msg.Attributes.Colour = Tint;

      msg.MeshID = ID;
      msg.VertexCount = VertexCount();
      msg.IndexCount = IndexCount();
      msg.DrawType = DrawType;

      packet.Reset((ushort)RoutingID.Mesh, MeshCreateMessage.MessageID);
      msg.Write(packet);
      return 0;
    }

    /// <summary>
    /// Send mesh destruction message.
    /// </summary>
    /// <param name="packet">Packet to populate with the destruction message.</param>
    /// <returns>Zero on success.</returns>
    public int Destroy(PacketBuffer packet)
    {
      MeshDestroyMessage msg = new MeshDestroyMessage();
      msg.MeshID = ID;
      packet.Reset((ushort)RoutingID.Mesh, MeshDestroyMessage.MessageID);
      msg.Write(packet);
      return 0;
    }

    /// <summary>
    /// Estimate the number of elements which can be transferred at the given <paramref name="byteLimit"/>
    /// </summary>
    /// <param name="elementSize">The byte size of each element.</param>
    /// <param name="byteLimit">The maximum number of bytes to transfer. Note: a hard limit of 0xffff is
    ///   enforced.</param>
    /// <param name="overhead">Additional byte overhead to a account for. This reduces the effectivel, total byte limit.</param>
    /// <returns>The maximum number of elements which can be accommodated in the byte limit (conservative).</returns>
    public static ushort EstimateTransferCount(int elementSize, int byteLimit, int overhead = 0)
    {
      int maxTransfer = (0xffff - (PacketHeader.Size + overhead + Crc16.CrcSize)) / elementSize;
      int count = (byteLimit > 0) ? byteLimit / elementSize : maxTransfer;
      if (count < 1)
      {
        count = 1;
      }
      else if (count > maxTransfer)
      {
        count = maxTransfer;
      }

      return (ushort)count;
    }

    /// <summary>
    /// Handles transfer of mesh content.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">The progress value from the last call to this method.</param>
    /// <returns>A new value for <paramref name="progress"/> to use on the next call.</returns>
    /// <remarks>
    /// Supports amortised transfer via the <paramref name="progress"/> argument.
    /// On first call, this is the default initialised structure (zero). On subsequent
    /// calls it is the last returned value unless <c>Failed</c> was true.
    ///
    /// The semantics of this value are entirely dependent on the internal implementation.
    /// </remarks>
    public void Transfer(PacketBuffer packet, int byteLimit, ref TransferProgress progress)
    {
      // Initial call?
      bool phaseComplete = false;
      switch (progress.Phase)
      {
        case (int)Phase.Vertex:
          phaseComplete = Transfer(packet, MeshMessageType.Vertex, VertexBuffer.Wrap(Vertices()), byteLimit, ref progress);
          break;
        case (int)Phase.Index:
          if (IndexSize == 2)
          {
            phaseComplete = Transfer(packet, MeshMessageType.Index, VertexBuffer.Wrap(Indices2()), byteLimit, ref progress);
          }
          else if (IndexSize == 4)
          {
            phaseComplete = Transfer(packet, MeshMessageType.Index, VertexBuffer.Wrap(Indices4()), byteLimit, ref progress);
          }
          break;
        case (int)Phase.Normal:
          phaseComplete = Transfer(packet, MeshMessageType.Normal, VertexBuffer.Wrap(Normals()), byteLimit, ref progress);
          break;
        case (int)Phase.Colour:
          phaseComplete = Transfer(packet, MeshMessageType.VertexColour, VertexBuffer.Wrap(Colours()), byteLimit, ref progress);
          break;
        case (int)Phase.UV:
          phaseComplete = Transfer(packet, MeshMessageType.UV, VertexBuffer.Wrap(UVs()), byteLimit, ref progress);
          break;
        case (int)Phase.Finalise:
          MeshFinaliseMessage msg = new MeshFinaliseMessage();
          msg.MeshID = ID;
          msg.Flags = 0;
          if (CalculateNormals && (Components & MeshComponentFlag.Normal) != MeshComponentFlag.Normal)
          {
            msg.Flags |= (uint)MeshFinaliseFlag.CalculateNormals;
          }
          packet.Reset((ushort)RoutingID.Mesh, MeshFinaliseMessage.MessageID);
          msg.Write(packet);
          progress.Phase = (int)Phase.End;
          phaseComplete = true;
          break;
      }

      if (!progress.Failed)
      {
        if (phaseComplete)
        {
          progress.Phase = (int)NextPhase((Phase)progress.Phase);
          progress.Progress = 0;
          progress.Complete = progress.Phase == (int)Phase.End;
        }
      }
    }

    /// <summary>
    /// Data transfer helper for packing <see cref="VertexBuffer"/> data.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="component">The component type.</param>
    /// <param name="items">Data to transfer.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    protected bool Transfer(PacketBuffer packet, MeshMessageType component, VertexBuffer buffer, int byteLimit, ref TransferProgress progress)
    {
      // Compose component message.
      MeshComponentMessage msg = new MeshComponentMessage();
      msg.MeshID = ID;

      packet.Reset((ushort)RoutingID.Mesh, (ushort)component);
      msg.Write(packet);

      int offset = (int)progress.Progress;
      int written = buffer.Write(packet, offset, buffer.NativePackingType, 0xffff);

      progress.Progress += written;
      return progress.Progress == buffer.Count;
    }
    #endregion

    #region Resource transfer - read
    /// <summary>
    /// Read and handle the <see cref="MeshCreateMessage"/>.
    /// </summary>
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">Stream to read from.</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Handling is deferred to <see cref="ProcessCreate(MeshCreateMessage)"/> which subclasses
    /// should implement.
    /// </remarks>
    public bool ReadCreate(PacketBuffer packet, BinaryReader reader)
    {
      MeshCreateMessage msg = new MeshCreateMessage();
      if (!msg.Read(reader))
      {
        return false;
      }

      return ProcessCreate(msg);
    }

    /// <summary>
    /// Read and handle a <see cref="MeshComponentMessage"/> message.
    /// </summary>
    /// <param name="messageType">The <see cref="MeshMessageType"/> identifying the mesh component being read.</param>
    /// <param name="reader">Stream to read from.</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Handling is deferred to one of the process methods which subclasses should implement.
    ///
    /// The <paramref name="messageType"/> identifies the mesh component being read, from the following set;
    /// <list type="bullet">
    /// <item><see cref="MeshMessageType.Vertex"/></item>
    /// <item><see cref="MeshMessageType.Index"/></item>
    /// <item><see cref="MeshMessageType.VertexColour"/></item>
    /// <item><see cref="MeshMessageType.Normal"/></item>
    /// <item><see cref="MeshMessageType.UV"/></item>
    /// </list>
    /// </remarks>
    public bool ReadTransfer(int messageType, BinaryReader reader)
    {
      MeshComponentMessage msg = new MeshComponentMessage();

      if (!msg.Read(reader))
      {
        return false;
      }

      // Read offset and count
      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      VertexBuffer readBuffer = new VertexBuffer();
      readBuffer.Read(reader, 0, count);

      switch (messageType)
      {
        case (int)MeshMessageType.Vertex:
          return ProcessVertices(msg, offset, readBuffer);

        case (int)MeshMessageType.Index:
          return ProcessIndices(msg, offset, readBuffer);

        case (int)MeshMessageType.VertexColour:
          return ProcessColours(msg, offset, readBuffer);

        case (int)MeshMessageType.Normal:
          return ProcessNormals(msg, offset, readBuffer);

        case (int)MeshMessageType.UV:
          return ProcessUVs(msg, offset, readBuffer);

        default:
          // Unknown message type.
          break;
      }

      return false;
    }

    /// <summary>
    /// Process the <see cref="MeshCreateMessage"/>.
    /// </summary>
    /// <param name="msg">The message to process.</param>
    /// <returns>True on success</returns>
    /// <remarks>
    /// Called from <see cref="ReadCreate(PacketBuffer, BinaryReader)"/> for subclasses to implement.
    /// </remarks>
    protected virtual bool ProcessCreate(MeshCreateMessage msg)
    {
      return false;
    }

    /// <summary>
    /// Process data for a <see cref="MeshMessageType.Vertex"/> message.
    /// </summary>
    /// <param name="msg">Message details.</param>
    /// <param name="vertices">New vertices read from the message payload.</param>
    /// <returns>True on success.</returns>
    protected virtual bool ProcessVertices(MeshComponentMessage msg, int offset, VertexBuffer readBuffer)
    {
      return false;
    }

    /// <summary>
    /// Process data for a <see cref="MeshMessageType.Index"/> message when receiving 2-byte indices.
    /// </summary>
    /// <param name="msg">Message details.</param>
    /// <param name="indices">New 2-byte indices read from the message payload.</param>
    /// <returns>True on success.</returns>
    protected virtual bool ProcessIndices(MeshComponentMessage msg, int offset, VertexBuffer readBuffer)
    {
      return false;
    }

    /// <summary>
    /// Process data for a <see cref="MeshMessageType.VertexColour"/> message.
    /// </summary>
    /// <param name="msg">Message details.</param>
    /// <param name="colours">New colours read from the message payload.</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Colours may be decoded using the <see cref="Colour"/> class.
    /// </remarks>
    protected virtual bool ProcessColours(MeshComponentMessage msg, int offset, VertexBuffer readBuffer)
    {
      return false;
    }

    /// <summary>
    /// Process data for a <see cref="MeshMessageType.Normal"/> message.
    /// </summary>
    /// <param name="msg">Message details.</param>
    /// <param name="normals">New normals read from the message payload.</param>
    /// <returns>True on success.</returns>
    protected virtual bool ProcessNormals(MeshComponentMessage msg, int offset, VertexBuffer readBuffer)
    {
      return false;
    }

    /// <summary>
    /// Process data for a <see cref="MeshMessageType.UV"/> message.
    /// </summary>
    /// <param name="msg">Message details.</param>
    /// <param name="uvs">New uvs read from the message payload.</param>
    /// <returns>True on success.</returns>
    protected virtual bool ProcessUVs(MeshComponentMessage msg, int offset, VertexBuffer readBuffer)
    {
      return false;
    }

    #endregion
  }
}
