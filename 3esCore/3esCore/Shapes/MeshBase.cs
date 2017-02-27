using System;
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

  
    #region Resource transfer
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
    /// <returns>The maximum number of elements which can be accommodated in the byte limit (conservative).</returns>
    public static ushort EstimateTransferCount(int elementSize, int byteLimit)
    {
      int maxTransfer = (0xffff - PacketHeader.Size + Crc16.CrcSize) / elementSize;
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
        phaseComplete = Transfer(packet, MeshMessageType.Vertex, Vertices(), byteLimit, ref progress);
        break;
      case (int)Phase.Index:
        if (IndexSize == 2)
        {
          phaseComplete = Transfer(packet, MeshMessageType.Index, Indices2(), byteLimit, ref progress);
        }
        else if (IndexSize == 4)
        {
          phaseComplete = Transfer(packet, MeshMessageType.Index, Indices4(), byteLimit, ref progress);
        }
        break;
      case (int)Phase.Normal:
        phaseComplete = Transfer(packet, MeshMessageType.Normal, Normals(), byteLimit, ref progress);
        break;
      case (int)Phase.Colour:
        phaseComplete = Transfer(packet, MeshMessageType.VertexColour, Colours(), byteLimit, ref progress);
        break;
      case (int)Phase.UV:
        phaseComplete = Transfer(packet, MeshMessageType.UV, UVs(), byteLimit, ref progress);
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
    /// Data transfer helper for packing <see cref="Vector3"/> data.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="component">The component type.</param>
    /// <param name="items">Data to transfer.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    protected bool Transfer(PacketBuffer packet, MeshMessageType component, Vector3[] items, int byteLimit, ref TransferProgress progress)
    {
      // Compose component message.
      int elementSize = 12; // 3 * 4 byte floats
      MeshComponentMessage msg = new MeshComponentMessage();
      msg.MeshID = ID;
      msg.Offset = (uint)progress.Progress;
      msg.Reserved = 0;
      msg.Count = (ushort)Math.Min(EstimateTransferCount(elementSize, byteLimit), (uint)items.Length - msg.Offset);

      packet.Reset((ushort)RoutingID.Mesh, (ushort)component);
      msg.Write(packet);
      uint ii = msg.Offset;
      for (int i = 0; i < msg.Count; ++i, ++ii)
      {
        packet.WriteBytes(BitConverter.GetBytes(items[ii].X), true);
        packet.WriteBytes(BitConverter.GetBytes(items[ii].Y), true);
        packet.WriteBytes(BitConverter.GetBytes(items[ii].Z), true);
      }

      progress.Progress += msg.Count;
      return progress.Progress == items.Length;
    }

    /// <summary>
    /// Data transfer helper for packing <see cref="Vector2"/> data.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="component">The component type.</param>
    /// <param name="items">Data to transfer.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    protected bool Transfer(PacketBuffer packet, MeshMessageType component, Vector2[] items, int byteLimit, ref TransferProgress progress)
    {
      // Compose component message.
      MeshComponentMessage msg = new MeshComponentMessage();
      int elementSize = 8; // 2 * 4 byte floats
      msg.MeshID = ID;
      msg.Offset = (uint)progress.Progress;
      msg.Reserved = 0;
      msg.Count = (ushort)Math.Min(EstimateTransferCount(elementSize, byteLimit), (uint)items.Length - msg.Offset);

      packet.Reset(TypeID, (ushort)component);
      msg.Write(packet);
      uint ii = msg.Offset;
      for (int i = 0; i < msg.Count; ++i, ++ii)
      {
        packet.WriteBytes(BitConverter.GetBytes(items[ii].X), true);
        packet.WriteBytes(BitConverter.GetBytes(items[ii].Y), true);
      }

      progress.Progress += msg.Count;
      return progress.Progress == items.Length;
    }

    /// <summary>
    /// Data transfer helper for packing <c>ushort</c> data.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="component">The component type.</param>
    /// <param name="items">Data to transfer.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    protected bool Transfer(PacketBuffer packet, MeshMessageType component, ushort[] items, int byteLimit, ref TransferProgress progress)
    {
      // Compose component message.
      MeshComponentMessage msg = new MeshComponentMessage();
      int elementSize = 2; // 2 bytes
      msg.MeshID = ID;
      msg.Offset = (uint)progress.Progress;
      msg.Reserved = 0;
      msg.Count = (ushort)Math.Min(EstimateTransferCount(elementSize, byteLimit), (uint)items.Length - msg.Offset);

      packet.Reset(TypeID, (ushort)component);
      msg.Write(packet);
      uint ii = msg.Offset;
      for (int i = 0; i < msg.Count; ++i, ++ii)
      {
        packet.WriteBytes(BitConverter.GetBytes(items[ii]), true);
      }

      progress.Progress += msg.Count;
      return progress.Progress == items.Length;
    }

    /// <summary>
    /// Data transfer helper for packing <c>int</c> data.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="component">The component type.</param>
    /// <param name="items">Data to transfer.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    protected bool Transfer(PacketBuffer packet, MeshMessageType component, int[] items, int byteLimit, ref TransferProgress progress)
    {
      // Compose component message.
      MeshComponentMessage msg = new MeshComponentMessage();
      int elementSize = 4; // 4 bytes
      msg.MeshID = ID;
      msg.Offset = (uint)progress.Progress;
      msg.Reserved = 0;
      msg.Count = (ushort)Math.Min(EstimateTransferCount(elementSize, byteLimit), (uint)items.Length - msg.Offset);

      packet.Reset(TypeID, (ushort)component);
      msg.Write(packet);
      uint ii = msg.Offset;
      for (int i = 0; i < msg.Count; ++i, ++ii)
      {
        packet.WriteBytes(BitConverter.GetBytes(items[ii]), true);
      }

      progress.Progress += msg.Count;
      return progress.Progress == items.Length;
    }

    /// <summary>
    /// Data transfer helper for packing <c>uint</c> data.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="component">The component type.</param>
    /// <param name="items">Data to transfer.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    protected bool Transfer(PacketBuffer packet, MeshMessageType component, uint[] items, int byteLimit, ref TransferProgress progress)
    {
      // Compose component message.
      MeshComponentMessage msg = new MeshComponentMessage();
      int elementSize = 4; // 4 bytes
      msg.MeshID = ID;
      msg.Offset = (uint)progress.Progress;
      msg.Reserved = 0;
      msg.Count = (ushort)Math.Min(EstimateTransferCount(elementSize, byteLimit), (uint)items.Length - msg.Offset);

      packet.Reset(TypeID, (ushort)component);
      msg.Write(packet);
      uint ii = msg.Offset;
      for (int i = 0; i < msg.Count; ++i, ++ii)
      {
        packet.WriteBytes(BitConverter.GetBytes(items[ii]), true);
      }

      progress.Progress += msg.Count;
      return progress.Progress == items.Length;
    }
    #endregion
}
}
