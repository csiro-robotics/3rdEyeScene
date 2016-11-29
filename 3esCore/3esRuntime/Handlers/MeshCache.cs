using System.Collections.Generic;
using System.IO;
using Tes.Net;
using Tes.Runtime;
using Tes.IO;
using UnityEngine;
using System;

namespace Tes.Handlers
{
  /// <summary>
  /// Implements handling of <see cref="RoutingID.Mesh"/> messages, maintaining the appropriate
  /// mesh objects.
  /// </summary>
  /// <remarks>
  /// Handles the messages specified in <see cref="MeshMessageType"/>
  /// </remarks>
  public class MeshCache : MessageHandler
  {
    /// <summary>
    /// Limit on the number of indices per Mesh part.
    /// </summary>
    public const int IndexCountLimit = 65000;

    /// <summary>
    /// Mesh details.
    /// </summary>
    public class MeshEntry
    {
      /// <summary>
      /// Mesh resource ID.
      /// </summary>
      public uint ID { get; internal set; }
      /// <summary>
      /// Target vertex count.
      /// </summary>
      public int VertexCount { get; internal set; }
      /// <summary>
      /// Target index count
      /// </summary>
      public int IndexCount { get; internal set; }
      /// <summary>
      /// Vertex array.
      /// </summary>
      public Vector3[] Vertices { get; internal set; }
      /// <summary>
      /// Per vertex normal array.
      /// </summary>
      public Vector3[] Normals { get; internal set; }
      /// <summary>
      /// Per vertex UV array.
      /// </summary>
      public Vector2[] UVs { get; internal set; }
      /// <summary>
      /// Per vertex colours array.
      /// </summary>
      public Color32[] VertexColours { get; internal set; }
      /// <summary>
      /// Mesh indices.
      /// </summary>
      public int[] Indices { get; internal set; }
      /// <summary>
      /// Mesh local transform position.
      /// </summary>
      public Vector3 LocalPosition { get; internal set; }
      /// <summary>
      /// Mesh local transform rotation.
      /// </summary>
      public Quaternion LocalRotation { get; internal set; }
      /// <summary>
      /// Mesh local transform scale.
      /// </summary>
      public Vector3 LocalScale { get; internal set; }
      /// <summary>
      /// A tint colour for primitives in this mesh.
      /// </summary>
      public Color32 Tint { get; internal set; }
      /// <summary>
      /// Render material.
      /// </summary>
      public Material Material { get; internal set; }
      /// <summary>
      /// Defines the mesh topology.
      /// </summary>
      public byte DrawType { get; internal set; }
      /// <summary>
      /// Alias <see cref="DrawType"/> to the equivalent Unity <c>Topology</c>.
      /// </summary>
      public MeshTopology Topology { get; internal set; }
      /// <summary>
      /// Array of finalised mesh parts. Valid when <see cref="Finalised"/>.
      /// </summary>
      public Mesh[] FinalMeshes { get; internal set; }
      /// <summary>
      /// True when finalised.
      /// </summary>
      /// <remarks>
      /// May become false on a redefine message.
      /// </remarks>
      public bool Finalised { get; internal set; }
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "MeshCache"; } }
    /// <summary>
    /// <see cref="Net.RoutingID.Mesh"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Net.RoutingID.Mesh; } }

    /// <summary>
    /// Material for rendering lit polygons.
    /// </summary>
    public Material LitMaterial { get; protected set; }
    /// <summary>
    /// Material for rendering unlit polygons.
    /// </summary>
    public Material UnlitMaterial { get; protected set; }
    /// <summary>
    /// Material for rendering lit points.
    /// </summary>
    public Material PointsLitMaterial { get; protected set; }
    /// <summary>
    /// Material for rendering unlit points.
    /// </summary>
    public Material PointsUnlitMaterial { get; protected set; }

    /// <summary>
    /// Delegate for various mesh events.
    /// </summary>
    /// <param name="mesh">The mesh changing state.</param>
    public delegate void MeshNotificationDelegate(MeshEntry mesh);

    /// <summary>
    /// Invoked on a new mesh resource being added. The mesh is not finalised.
    /// </summary>
    public event MeshNotificationDelegate OnMeshAdded;
    /// <summary>
    /// Invoked on a new mesh being removed/destroyed.
    /// </summary>
    public event MeshNotificationDelegate OnMeshRemoved;
    /// <summary>
    /// Invoked on first finalisation or re-finalisation of a mesh.
    /// </summary>
    public event MeshNotificationDelegate OnMeshFinalised;

    /// <summary>
    /// Converts <paramref name="drawType"/> to the equivalent Unity <c>Topology</c>.
    /// </summary>
    /// <param name="drawType">The Tes topology.</param>
    /// <returns>The equivalent Unity <c>Topology</c></returns>
    /// <exception cref="NotImplementedException">Thrown when <paramref name="drawType"/> is
    /// not supported by the Unity implementation.</exception>
    public static MeshTopology DrawTypeToTopology(MeshDrawType drawType)
    {
      switch (drawType)
      {
      case MeshDrawType.Points:
        return MeshTopology.Points;
      case MeshDrawType.Lines:
        return MeshTopology.Lines;
      case MeshDrawType.Triangles:
        return MeshTopology.Triangles;
      default:
        break;
      }
      throw new NotImplementedException("Unsupported draw type.");
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public MeshCache()
      : base(null)
    {
    }

    /// <summary>
    /// Initalise, caching the required materials from <paramref name="materials"/>.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="serverRoot"></param>
    /// <param name="materials"></param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      LitMaterial = materials[MaterialLibrary.VertexColourLit];
      UnlitMaterial = materials[MaterialLibrary.VertexColourUnlit];
      PointsLitMaterial = materials[MaterialLibrary.PointsLit];
      PointsUnlitMaterial = materials[MaterialLibrary.PointsUnlit];
    }


    /// <summary>
    /// Reset the cache.
    /// </summary>
    /// <remarks>
    /// Does not invoke <see cref="OnMeshRemoved"/> events.
    /// </remarks>
    public override void Reset()
    {
      _meshes.Clear();
    }

    /// <summary>
    /// Query details of mesh resource using <paramref name="meshId"/>.
    /// </summary>
    /// <param name="meshId">ID of the resource of interest.</param>
    /// <returns>The mesh details, or null when <paramref name="meshId"/> is unknown.</returns>
    public MeshEntry GetEntry(uint meshId)
    {
      MeshEntry entry = null;
      _meshes.TryGetValue(meshId, out entry);
      return entry;
    }

    /// <summary>
    /// Message handler.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <remarks>
    /// Handles messages IDs from <see cref="MeshMessageType"/>
    /// </remarks>
    public override Error ReadMessage(PacketBuffer packet, BinaryReader reader)
    {
      //if (!ok)
      //{
      //  return new Error(ErrorCode.MalformedMessage);
      //}

      switch (packet.Header.MessageID)
      {
      case (ushort)MeshMessageType.Invalid:
        return new Error(ErrorCode.NullMessageCode);

      case (ushort)MeshMessageType.Destroy:
        return DestroyMesh(packet, reader);
      case (ushort)MeshMessageType.Create:
        return CreateMesh(packet, reader);
      case (ushort)MeshMessageType.Vertex:
        return AddVertices(packet, reader);
      case (ushort)MeshMessageType.VertexColour:
        return AddVertexColours(packet, reader);
      case (ushort)MeshMessageType.Index:
        return AddIndices(packet, reader);
      case (ushort)MeshMessageType.Normal:
        return AddNormals(packet, reader);
      case (ushort)MeshMessageType.UV:
        return AddUVs(packet, reader);
      case (ushort)MeshMessageType.SetMaterial:
        return new Error(ErrorCode.UnsupportedFeature); // NYI
      case (ushort)MeshMessageType.Redefine:
        return RedefineMesh(packet, reader);
      case (ushort)MeshMessageType.Finalise:
        return FinaliseMesh(packet, reader);

      default:
        break;
      }

      return new Error(ErrorCode.InvalidMessageID, packet.Header.MessageID);
    }

    /// <summary>
    /// Serialise messages required to restore the current state.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="info">Statistics</param>
    /// <returns>An error code on failure.</returns>
    public override Error Serialise(BinaryWriter writer, ref SerialiseInfo info)
    {
      // Iterate and serialise the meshes.
      PacketBuffer packet = new PacketBuffer();
      info.TransientCount = info.PersistentCount = 0u;
      foreach (MeshEntry mesh in _meshes.Values)
      {
        ++info.PersistentCount;
        Error err = Serialise(mesh, packet, writer);
        if (err.Failed)
        {
          return err;
        }
      }

      return new Error();
    }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="active"></param>
    public override void OnCategoryChange(ushort categoryId, bool active)
    {
    }

    /// <summary>
    /// Serialise messages to generate <paramref name="mesh"/>.
    /// </summary>
    /// <param name="mesh">The mesh of interest.</param>
    /// <param name="packet">Packet buffer to compose messages in</param>
    /// <param name="writer">Writer to export completed message packets to.</param>
    /// <returns></returns>
    /// <remarks>
    /// Writes:
    /// <list type="bullet">
    /// <item><see cref="MeshCreateMessage"/></item>
    /// <item><see cref="MeshComponentMessage"/> for each component type from
    /// <see cref="MeshMessageType"/></item>
    /// <item><see cref="MeshFinaliseMessage"/> only when <paramref name="mesh"/> is already
    /// finalised.</item>
    /// </list>
    /// </remarks>
    protected Error Serialise(MeshEntry mesh, PacketBuffer packet, BinaryWriter writer)
    {
      // First write a create message.
      MeshCreateMessage msg = new MeshCreateMessage();
      packet.Reset((ushort)RoutingID, (ushort)MeshCreateMessage.MessageID);

      msg.MeshID = mesh.ID;
      msg.VertexCount = (uint)mesh.VertexCount;
      msg.IndexCount = (uint)mesh.IndexCount;
      msg.DrawType = mesh.DrawType;

      msg.Attributes.X = mesh.LocalPosition.x;
      msg.Attributes.Y = mesh.LocalPosition.y;
      msg.Attributes.Z = mesh.LocalPosition.z;

      msg.Attributes.RotationX = mesh.LocalRotation.x;
      msg.Attributes.RotationY = mesh.LocalRotation.y;
      msg.Attributes.RotationZ = mesh.LocalRotation.z;
      msg.Attributes.RotationW = mesh.LocalRotation.w;

      msg.Attributes.ScaleX = mesh.LocalScale.x;
      msg.Attributes.ScaleY = mesh.LocalScale.y;
      msg.Attributes.ScaleZ = mesh.LocalScale.z;

      msg.Attributes.Colour = ShapeComponent.ConvertColour(mesh.Tint);

      msg.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);

      uint blockSize = 65000u;

      // Now write data messages for the mesh content.
      WriteMeshComponent(mesh.ID, MeshMessageType.Vertex, mesh.Vertices, packet, writer, blockSize);
      WriteMeshComponent(mesh.ID, MeshMessageType.Index, mesh.Indices, packet, writer, blockSize);
      WriteMeshComponent(mesh.ID, MeshMessageType.Normal, mesh.Normals, packet, writer, blockSize);
      WriteMeshComponent(mesh.ID, MeshMessageType.VertexColour, mesh.VertexColours, packet, writer, blockSize);
      WriteMeshComponent(mesh.ID, MeshMessageType.UV, mesh.UVs, packet, writer, blockSize);

      // Finalise if possible.
      if (mesh.Finalised)
      {
        MeshFinaliseMessage fmsg = new MeshFinaliseMessage();
        fmsg.MeshID = mesh.ID;
        fmsg.Flags = 0;
        packet.Reset((ushort)RoutingID, MeshFinaliseMessage.MessageID);
        fmsg.Write(packet);
        packet.FinalisePacket();
        packet.ExportTo(writer);
      }

      return new Error();
    }

    /// <summary>
    /// Write a <see cref="MeshComponentMessage"/> to serialise <c>Vector3</c> data.
    /// </summary>
    /// <param name="meshID">Mesh resource ID we are serialising for.</param>
    /// <param name="type">The mesh component type; e.g., <see cref="MeshMessageType.Vertex"/>.</param>
    /// <param name="data">The <c>Vector3</c> data array.</param>
    /// <param name="packet">Packet buffer to compose messages in</param>
    /// <param name="writer">Writer to export completed message packets to.</param>
    /// <param name="blockSize">The maximum number of elements per message.</param>
    /// <remarks>
    /// Writes multiple messages to <paramref name="writer"/> as required to ensure all
    /// <paramref name="data"/> are written.
    /// </remarks>
    protected void WriteMeshComponent(uint meshID, MeshMessageType type, Vector3[] data,
                                      PacketBuffer packet, BinaryWriter writer, uint blockSize)
    {
      if (data == null || data.Length == 0)
      {
        return;
      }

      MeshComponentMessage cmsg = new MeshComponentMessage();
      cmsg.MeshID = meshID;
      cmsg.Offset = 0;
      cmsg.Reserved = 0;
      cmsg.Count = 0;

      while (cmsg.Offset < data.Length)
      {
        Vector3 v;
        cmsg.Count = (ushort)Math.Min(blockSize, (uint)data.Length - cmsg.Offset);
        packet.Reset((ushort)RoutingID, (ushort)type);
        cmsg.Write(packet);
        for (int i = 0; i < cmsg.Count; ++i)
        {
          v = data[i + cmsg.Offset];
          packet.WriteBytes(BitConverter.GetBytes(v.x), true);
          packet.WriteBytes(BitConverter.GetBytes(v.y), true);
          packet.WriteBytes(BitConverter.GetBytes(v.z), true);
        }

        if (cmsg.Count > 0)
        {
          packet.FinalisePacket();
          packet.ExportTo(writer);
        }

        cmsg.Offset += cmsg.Count;
        cmsg.Count = 0;
      }
    }

    /// <summary>
    /// Write a <see cref="MeshComponentMessage"/> to serialise <c>Vector2</c> data.
    /// </summary>
    /// <param name="meshID">Mesh resource ID we are serialising for.</param>
    /// <param name="type">The mesh component type; e.g., <see cref="MeshMessageType.UV"/>.</param>
    /// <param name="data">The <c>Vector2</c> data array.</param>
    /// <param name="packet">Packet buffer to compose messages in</param>
    /// <param name="writer">Writer to export completed message packets to.</param>
    /// <param name="blockSize">The maximum number of elements per message.</param>
    /// <remarks>
    /// Writes multiple messages to <paramref name="writer"/> as required to ensure all
    /// <paramref name="data"/> are written.
    /// </remarks>
    protected void WriteMeshComponent(uint meshID, MeshMessageType type, Vector2[] data, PacketBuffer packet, BinaryWriter writer, uint blockSize)
    {
      if (data == null || data.Length == 0)
      {
        return;
      }

      MeshComponentMessage cmsg = new MeshComponentMessage();
      cmsg.MeshID = meshID;
      cmsg.Offset = 0;
      cmsg.Reserved = 0;
      cmsg.Count = 0;

      while (cmsg.Offset < data.Length)
      {
        Vector2 v;
        cmsg.Count = (ushort)Math.Min(blockSize, (uint)data.Length - cmsg.Offset);
        packet.Reset((ushort)RoutingID, (ushort)type);
        cmsg.Write(packet);
        for (int i = 0; i < cmsg.Count; ++i)
        {
          v = data[i + cmsg.Offset];
          packet.WriteBytes(BitConverter.GetBytes(v.x), true);
          packet.WriteBytes(BitConverter.GetBytes(v.y), true);
        }

        if (cmsg.Count > 0)
        {
          cmsg.Write(packet);
          packet.FinalisePacket();
          packet.ExportTo(writer);
        }

        cmsg.Offset += cmsg.Count;
        cmsg.Count = 0;
      }
    }

    /// <summary>
    /// Write a <see cref="MeshComponentMessage"/> to serialise <c>Color32</c> data.
    /// </summary>
    /// <param name="meshID">Mesh resource ID we are serialising for.</param>
    /// <param name="type">The mesh component type; e.g., <see cref="MeshMessageType.VertexColour"/>.</param>
    /// <param name="data">The <c>Color32</c> data array.</param>
    /// <param name="packet">Packet buffer to compose messages in</param>
    /// <param name="writer">Writer to export completed message packets to.</param>
    /// <param name="blockSize">The maximum number of elements per message.</param>
    /// <remarks>
    /// Writes multiple messages to <paramref name="writer"/> as required to ensure all
    /// <paramref name="data"/> are written.
    /// </remarks>
    protected void WriteMeshComponent(uint meshID, MeshMessageType type, Color32[] data, PacketBuffer packet, BinaryWriter writer, uint blockSize)
    {
      if (data == null || data.Length == 0)
      {
        return;
      }

      MeshComponentMessage cmsg = new MeshComponentMessage();
      cmsg.MeshID = meshID;
      cmsg.Offset = 0;
      cmsg.Reserved = 0;
      cmsg.Count = 0;

      while (cmsg.Offset < data.Length)
      {
        Color32 c;
        cmsg.Count = (ushort)Math.Min(blockSize, (uint)data.Length - cmsg.Offset);
        packet.Reset((ushort)RoutingID, (ushort)type);
        cmsg.Write(packet);
        for (int i = 0; i < cmsg.Count; ++i)
        {
          c = data[i + cmsg.Offset];
          packet.WriteBytes(BitConverter.GetBytes(ShapeComponent.ConvertColour(c)), true);
        }

        if (cmsg.Count > 0)
        {
          cmsg.Write(packet);
          packet.FinalisePacket();
          packet.ExportTo(writer);
        }

        cmsg.Offset += cmsg.Count;
        cmsg.Count = 0;
      }
    }

    /// <summary>
    /// Write a <see cref="MeshComponentMessage"/> to serialise <c>int</c> data.
    /// </summary>
    /// <param name="meshID">Mesh resource ID we are serialising for.</param>
    /// <param name="type">The mesh component type; e.g., <see cref="MeshMessageType.Index"/>.</param>
    /// <param name="data">The <c>int</c> data array.</param>
    /// <param name="packet">Packet buffer to compose messages in</param>
    /// <param name="writer">Writer to export completed message packets to.</param>
    /// <param name="blockSize">The maximum number of elements per message.</param>
    /// <remarks>
    /// Writes multiple messages to <paramref name="writer"/> as required to ensure all
    /// <paramref name="data"/> are written.
    /// </remarks>
    protected void WriteMeshComponent(uint meshID, MeshMessageType type, int[] data, PacketBuffer packet, BinaryWriter writer, uint blockSize)
    {
      if (data == null || data.Length == 0)
      {
        return;
      }

      MeshComponentMessage cmsg = new MeshComponentMessage();
      cmsg.MeshID = meshID;
      cmsg.Offset = 0;
      cmsg.Reserved = 0;
      cmsg.Count = 0;

      while (cmsg.Offset < data.Length)
      {
        cmsg.Count = (ushort)Math.Min(blockSize, (uint)data.Length - cmsg.Offset);
        packet.Reset((ushort)RoutingID, (ushort)type);
        cmsg.Write(packet);
        for (int i = 0; i < cmsg.Count; ++i)
        {
          packet.WriteBytes(BitConverter.GetBytes(data[i + cmsg.Offset]), true);
        }

        if (cmsg.Count > 0)
        {
          cmsg.Write(packet);
          packet.FinalisePacket();
          packet.ExportTo(writer);
        }

        cmsg.Offset += cmsg.Count;
        cmsg.Count = 0;
      }
    }

    /// <summary>
    /// Handles <see cref="MeshDestroyMessage"/>
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// Emits <see cref="OnMeshRemoved"/>.
    protected Error DestroyMesh(PacketBuffer packet, BinaryReader reader)
    {
      MeshDestroyMessage msg = new MeshDestroyMessage();
      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, MeshDestroyMessage.MessageID);
      }

      if (!_meshes.ContainsKey(msg.MeshID))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      MeshEntry entry = _meshes[msg.MeshID];

      NotifyMeshRemoved(entry);

      _meshes.Remove(msg.MeshID);
      if (entry.FinalMeshes != null)
      { 
        foreach (Mesh mesh in entry.FinalMeshes)
        {
          Mesh.Destroy(mesh);
        }
      }

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshCreateMessage"/>
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <remarks>
    /// Emits <see cref="OnMeshAdded"/>.
    /// </remarks>
    protected Error CreateMesh(PacketBuffer packet, BinaryReader reader)
    {
      MeshCreateMessage msg = new MeshCreateMessage();

      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, MeshCreateMessage.MessageID);
      }

      if (_meshes.ContainsKey(msg.MeshID))
      {
        return new Error(ErrorCode.DuplicateShape, msg.MeshID);
      }

      MeshEntry entry = new MeshEntry();

      entry.DrawType = msg.DrawType;
      switch (msg.DrawType)
      {
      case (byte)MeshDrawType.Points:
        entry.Topology = MeshTopology.Points;
        break;
      case (byte)MeshDrawType.Lines:
        entry.Topology = MeshTopology.Lines;
        break;
      case (byte)MeshDrawType.Triangles:
        entry.Topology = MeshTopology.Triangles;
        break;
      default:
        return new Error(ErrorCode.UnsupportedFeature, msg.DrawType);
      }

      if (msg.VertexCount != 0)
      {
        entry.VertexCount = (int)msg.VertexCount;
        entry.Vertices = new Vector3[msg.VertexCount];
      }
      if (msg.IndexCount != 0)
      {
        entry.IndexCount = (int)msg.IndexCount;
        entry.Indices = new int[msg.IndexCount];
      }

      entry.ID = msg.MeshID;
      entry.LocalPosition = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      entry.LocalRotation = new Quaternion(msg.Attributes.RotationX, msg.Attributes.RotationY, msg.Attributes.RotationZ, msg.Attributes.RotationW);
      entry.LocalScale = new Vector3(msg.Attributes.ScaleX, msg.Attributes.ScaleY, msg.Attributes.ScaleZ);
      entry.Tint = ShapeComponent.ConvertColour(msg.Attributes.Colour);
      entry.Finalised = false;
      _meshes.Add(entry.ID, entry);

      NotifyMeshAdded(entry);

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshComponentMessage"/> of type <see cref="MeshMessageType.Vertex"/>.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected Error AddVertices(PacketBuffer packet, BinaryReader reader)
    {
      MeshComponentMessage msg = new MeshComponentMessage();

      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Vertex);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = entry.VertexCount;
      if (voffset >= vertexCount || voffset + (int)msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector3 v = Vector3.zero;
      for (int vInd = 0; ok && vInd < (int)msg.Count; ++vInd)
      {
        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();
        v.z = reader.ReadSingle();
        entry.Vertices[vInd + voffset] = v;
      }

      if (!ok)
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Vertex);
      }

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshComponentMessage"/> of type <see cref="MeshMessageType.VertexColour"/>.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected Error AddVertexColours(PacketBuffer packet, BinaryReader reader)
    {
      MeshComponentMessage msg = new MeshComponentMessage();

      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.VertexColour);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      if (entry.VertexColours == null)
      {
        entry.VertexColours = new Color32[entry.VertexCount];
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = entry.VertexCount;
      if (voffset >= vertexCount || voffset + (int)msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.VertexColour);
      }

      // Check for settings initial bounds.
      bool ok = true;
      uint colour;
      for (int vInd = 0; ok && vInd < (int)msg.Count; ++vInd)
      {
        colour = reader.ReadUInt32();
        entry.VertexColours[vInd + voffset] = ShapeComponent.ConvertColour(colour);
    }

      if (!ok)
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.VertexColour);
      }

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshComponentMessage"/> of type <see cref="MeshMessageType.Index"/>.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected Error AddIndices(PacketBuffer packet, BinaryReader reader)
    {
      MeshComponentMessage msg = new MeshComponentMessage();

      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Index);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      uint ioffset = msg.Offset;
      // Bounds check.
      int indexCount = entry.IndexCount;
      if (ioffset >= indexCount || ioffset + (int)msg.Count > indexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Index);
      }

      // Check for settings initial bounds.
      bool ok = true;
      int index;
      for (int iInd = 0; ok && iInd < (int)msg.Count; ++iInd)
      {
        index = reader.ReadInt32();
        entry.Indices[iInd + ioffset] = index;
      }

      if (!ok)
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Index);
      }

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshComponentMessage"/> of type <see cref="MeshMessageType.Normal"/>.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected Error AddNormals(PacketBuffer packet, BinaryReader reader)
    {
      MeshComponentMessage msg = new MeshComponentMessage();

      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Normal);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      if (entry.Normals == null)
      {
        entry.Normals = new Vector3[entry.VertexCount];
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = entry.VertexCount;
      if (voffset >= vertexCount || voffset + (int)msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Normal);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector3 n = Vector3.zero;
      for (int vInd = 0; ok && vInd < (int)msg.Count; ++vInd)
      {
        n.x = reader.ReadSingle();
        n.y = reader.ReadSingle();
        n.z = reader.ReadSingle();
        entry.Normals[vInd + voffset] = n;
      }

      if (!ok)
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Normal);
      }

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshComponentMessage"/> of type <see cref="MeshMessageType.UV"/>.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected Error AddUVs(PacketBuffer packet, BinaryReader reader)
    {
      MeshComponentMessage msg = new MeshComponentMessage();

      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.UV);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      if (entry.UVs == null)
      {
        entry.UVs = new Vector2[entry.VertexCount];
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = entry.VertexCount;
      if (voffset >= vertexCount || voffset + (int)msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.UV);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector2 uv = Vector2.zero;
      for (int vInd = 0; ok && vInd < (int)msg.Count; ++vInd)
      {
        uv.x = reader.ReadSingle();
        uv.y = reader.ReadSingle();
        entry.UVs[vInd + voffset] = uv;
      }

      if (!ok)
      {
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.UV);
      }

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshRedefineMessage"/>
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <remarks>
    /// The associated mesh is invalidated until another <see cref="MeshFinaliseMessage"/> arrives.
    /// </remarks>
    protected Error RedefineMesh(PacketBuffer packet, BinaryReader reader)
    {
      MeshRedefineMessage msg = new MeshRedefineMessage();
      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, MeshRedefineMessage.MessageID);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      entry.Finalised = false;
      if (msg.VertexCount != entry.VertexCount)
      {
        entry.VertexCount = (int)msg.VertexCount;
        entry.Vertices = ResizeMeshArray(entry.Vertices, entry.VertexCount);
        if (entry.Normals != null)
        {
          entry.Normals = ResizeMeshArray(entry.Normals, entry.VertexCount);
        }
        if (entry.UVs != null)
        {
          entry.UVs = ResizeMeshArray(entry.UVs, entry.VertexCount);
        }
        if (entry.VertexColours != null)
        {
          entry.VertexColours = ResizeMeshArray(entry.VertexColours, entry.VertexCount);
        }
      }
      if (msg.IndexCount != 0)
      {
        entry.IndexCount = (int)msg.IndexCount;
        entry.Indices = ResizeMeshArray(entry.Indices, entry.IndexCount);
      }

      entry.ID = msg.MeshID;
      entry.LocalPosition = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      entry.LocalRotation = new Quaternion(msg.Attributes.RotationX, msg.Attributes.RotationY, msg.Attributes.RotationZ, msg.Attributes.RotationW);
      entry.LocalScale = new Vector3(msg.Attributes.ScaleX, msg.Attributes.ScaleY, msg.Attributes.ScaleZ);
      entry.Tint = ShapeComponent.ConvertColour(msg.Attributes.Colour);

      return new Error();
    }

    /// <summary>
    /// Handles <see cref="MeshFinaliseMessage"/>.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <remarks>
    /// This creates the required Unity <c>Mesh</c> objects and emits <see cref="OnMeshFinalised"/>.
    /// </remarks>
    protected Error FinaliseMesh(PacketBuffer packet, BinaryReader reader)
    {
      MeshFinaliseMessage msg = new MeshFinaliseMessage();
      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, MeshFinaliseMessage.MessageID);
      }

      MeshEntry entry;
      if (!_meshes.TryGetValue(msg.MeshID, out entry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (entry.Finalised)
      {
        return new Error(ErrorCode.MeshAlreadyFinalised, entry.ID);
      }

      bool generateNormals = (msg.Flags & (uint)MeshBuildFlags.CalculateNormals) != 0;
      switch (entry.Topology)
      {
      case MeshTopology.Triangles:
      case MeshTopology.Quads:
        if (entry.Normals != null || generateNormals)
        {
          entry.Material = LitMaterial;
        }
        else
        {
          entry.Material = UnlitMaterial;
        }
        break;
      case MeshTopology.Points:
        generateNormals = false;
        if (entry.Normals != null)
        {
          entry.Material = PointsLitMaterial;
        }
        else
        {
          entry.Material = PointsUnlitMaterial;
        }
        //if (entry.VertexColours == null)
        //{
        //  entry.VertexColours = new Color32[entry.VertexCount];
        //  for (int i = 0; i < entry.VertexCount; ++i)
        //  {
        //    entry.VertexColours[i] = new Color32(1, 1, 1, 1);
        //  }
        //}
        break;
      default:
      case MeshTopology.Lines:
      case MeshTopology.LineStrip:
        generateNormals = false;
        entry.Material = UnlitMaterial;
        break;
      }

      // Generate the meshes here!
      Error err = GenerateMeshes(entry, generateNormals);

      entry.Finalised = true;
      NotifyMeshFinalised(entry);

      return err;
    }


    /// <summary>
    /// A helper function for resizing mesh data arrays and copying existing data.
    /// </summary>
    /// <remarks>
    /// Does not check if the sizes are the same.
    /// </remarks>
    /// <typeparam name="T">The array type.</typeparam>
    /// <param name="original">The original data array. May be null.</param>
    /// <param name="newSize">New array size.</param>
    /// <returns>The resized array.</returns>
    protected T[] ResizeMeshArray<T>(T[] original, int newSize)
    {
      T[] newArray = new T[newSize];
      if (original != null && original.Length != 0)
      {
        Array.Copy(original, newArray, Math.Min(newSize, original.Length));
      }
      return newArray;
    }

    /// <summary>
    /// A helper function to generate the Unit <c>Mesh</c> objects for <paramref name="entry"/>
    /// </summary>
    /// <param name="entry">The mesh object details.</param>
    /// <param name="recalculateNormals">(Re)Calculate normals for the mesh objects?</param>
    /// <returns></returns>
    protected Error GenerateMeshes(MeshEntry entry, bool recalculateNormals)
    {
      int[] meshIndices = null;
      if (entry.IndexCount > 0)
      {
        meshIndices = entry.Indices;
      }
      else if (entry.Topology == MeshTopology.Points)
      {
        // Points only with no specific index list.
        // Construct the index list to match the vertices.
        meshIndices = new int[entry.VertexCount];
        for (int i = 0; i < entry.VertexCount; ++i)
        {
          meshIndices[i] = i;
        }
      }

      // Need to break the mesh up to keep under the index limit.
      if (entry.VertexCount == 0 || meshIndices == null)
      {
        // No current mesh data. Maybe redefined later.
        // Add an empty mesh.
        entry.FinalMeshes = new Mesh[] { new Mesh() };
      }
      else if (meshIndices.Length < IndexCountLimit)
      {
        // Easy: single mesh.
        // Handle mesh redefinition by looking at the FinalMeshes array.
        Mesh mesh = null;
        if (entry.FinalMeshes != null)
        {
          mesh = entry.FinalMeshes[0];
          if (entry.FinalMeshes.Length > 1)
          {
            // Destroy excess meshes.
            // FIXME: Do we use Mesh.Destroy() or simply null the referenes?
            // Will Unity clean them up once there are no other references?
            entry.FinalMeshes = new Mesh[] { mesh };
          }
        }

        if (mesh == null)
        {
          mesh = new Mesh();
          entry.FinalMeshes = new Mesh[] { mesh };
        }

        mesh.subMeshCount = 1;
        mesh.vertices = entry.Vertices;
        if (entry.Normals != null)
        {
          mesh.normals = entry.Vertices;
        }
        if (entry.UVs != null)
        {
          mesh.uv = entry.UVs;
        }
        if (entry.VertexColours != null)
        {
          mesh.colors32 = entry.VertexColours;
        }

        if (meshIndices != null)
        {
          mesh.SetIndices(meshIndices, entry.Topology, 0);

          if (recalculateNormals)
          {
            mesh.RecalculateNormals();
          }
          mesh.RecalculateBounds();
        }
      }
      else
      {
        List<Mesh> meshes = new List<Mesh>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = entry.Normals != null ? new List<Vector3>() : null;
        List<Vector2> uvs = entry.UVs != null ? new List<Vector2>() : null;
        List<Color32> colours = entry.VertexColours != null ? new List<Color32>() : null;
        List<int> indices = new List<int>();
        // Index mappings from original indices to mesh part indices.
        // Mapping is 1 based, so zero is unmapped and 1 is vertex 0, 2 is vertex 1, etc.
        List<int> indexMap = new List<int>(meshIndices.Length);
        int indicesStep = TopologyIndexStep(entry.Topology);

        // Fill indexMap.
        for (int i = 0; i < meshIndices.Length; ++i)
        {
          indexMap.Add(-1);
        }

        // For now, just copy all the vertex data. This would work well if we could share
        // the vertex data between meshes.
        indices.Capacity = IndexCountLimit;
        for (int iidx = 0; iidx < meshIndices.Length; )
        {
          vertices.Clear();
          if (normals != null) { normals.Clear(); }
          if (uvs != null) { uvs.Clear(); }
          if (colours != null) { colours.Clear(); }
          indices.Clear();
          while (indices.Count + indicesStep < IndexCountLimit && iidx < meshIndices.Length)
          {
            for (int i = 0; i < indicesStep; ++i)
            {
              CopyIndex(iidx++, indexMap, entry, indices, vertices, normals, uvs, colours);
            }
          }

          // Hit the limit or done. Extract a mesh.
          // Either modify an existing mesh or generate a new one as required.
          Mesh mesh = NextMesh(meshes.Count, entry.FinalMeshes);
          mesh.subMeshCount = 1;
          mesh.vertices = vertices.ToArray();
          if (normals != null) { mesh.normals = normals.ToArray(); }
          if (uvs != null) { mesh.uv = uvs.ToArray(); }
          if (colours != null) { mesh.colors32 = colours.ToArray(); }
          mesh.SetIndices(indices.ToArray(), entry.Topology, 0);

          if (recalculateNormals)
          {
            mesh.RecalculateNormals();
          }
          mesh.RecalculateBounds();

          meshes.Add(mesh);

          // Clear the index map.
          if (iidx + 1 < meshIndices.Length)
          {
            for (int i = 0; i < indexMap.Count; ++i)
            {
              indexMap[i] = -1;
            }

            // Back track to repeat an index for loops.
            if (entry.Topology == MeshTopology.LineStrip)
            {
              --iidx;
            }
          }
        }

        // FIXME: again, do we need to explicitly delete any excess meshes or will
        // garbage collection handle it.
        entry.FinalMeshes = meshes.ToArray();
      }

      return new Error();
    }

    /// <summary>
    /// Get or create the mesh at <paramref name="index"/> from <paramref name="existingMeshes"/>
    /// </summary>
    /// <param name="index">The mesh index</param>
    /// <param name="existingMeshes">The mesh array</param>
    /// <returns>The <paramref name="existingMeshes"/> element at <paramref name="index"/>
    /// if valid, or a new <c>Mesh</c> object.</returns>
    private Mesh NextMesh(int index, Mesh[] existingMeshes)
    {
      if (existingMeshes != null && index < existingMeshes.Length)
      {
        return existingMeshes[index];
      }
      return new Mesh();
    }


    /// <summary>
    /// Copies and maps a vertex from <c>entry.Vertices</c> unless it has already been mapped.
    /// The mapped index is added to the input lists.
    /// </summary>
    /// <remarks>
    /// This is a helper function for mapping mesh data from the original data set into multiple
    /// <c>Mesh</c> objects to respect Unity's vertex count limit.
    /// 
    /// Each entry in <paramref name="indexMap"/> corresponds to an element in <c>entry.Vertices</c>
    /// and is zero for unmapped vertices. For mapped indices, the value is the
    /// <c>mapped index + 1</c>.
    /// 
    /// A newly mapped vertex is added to <paramref name="vertices"/>, <paramref name="normals"/>,
    /// <paramref name="uvs"/>, <paramref name="colours"/>, skipping any null lists (except
    /// for <paramref name="vertices"/>).
    /// 
    /// Regardless of whether the vertex is already mapped, the vertex index is (re)added to
    /// <paramref name="indices"/>.
    /// </remarks>
    /// <param name="srcIndex">The vertex index in <c>entry.Vertices</c></param>
    /// <param name="indexMap">The list of vertex mappings. A zero entry is unmapped.</param>
    /// <param name="entry">The mesh details.</param>
    /// <param name="indices">Indices for the new mesh component.</param>
    /// <param name="vertices">Vertices for the new mesh component.</param>
    /// <param name="normals">Normals for the new mesh component. May be null.</param>
    /// <param name="uvs">UVs for the new mesh component. May be null.</param>
    /// <param name="colours">Colours for the new mesh component. May be null.</param>
    protected void CopyIndex(int srcIndex, List<int> indexMap, MeshEntry entry, List<int> indices,
                             List<Vector3> vertices, List<Vector3> normals,
                             List<Vector2> uvs, List<Color32> colours)
    {
      int mapping = indexMap[srcIndex];
      if (mapping <= 0)
      {
        // New mapping.
        indexMap[srcIndex] = vertices.Count + 1;
        indices.Add(vertices.Count);
        vertices.Add(entry.Vertices[entry.Indices[srcIndex]]);
        if (normals != null) { normals.Add(entry.Normals[srcIndex]); }
        if (uvs != null) { uvs.Add(entry.UVs[srcIndex]); }
        if (colours != null) { colours.Add(entry.VertexColours[srcIndex]); }
      }
      else
      {
        // Existing mapping.
        indices.Add(mapping - 1);
      }
    }

    /// <summary>
    /// Query the number of indices per element for <paramref name="topology"/>.
    /// </summary>
    /// <param name="topology">The mesh topology</param>
    /// <returns>The number of indices per primitive.</returns>
    /// <remarks>
    /// <list type="table">
    /// <listheader><term>Type</term><description>Index Count</description></listheader>
    /// <item><term><see cref="MeshTopology.Points"/></term><description>1</description></item>
    /// <item><term><see cref="MeshTopology.LineStrip"/></term><description>1 (not supported).</description></item>
    /// <item><term><see cref="MeshTopology.Lines"/></term><description>2</description></item>
    /// <item><term><see cref="MeshTopology.Triangles"/></term><description>3</description></item>
    /// <item><term><see cref="MeshTopology.Quads"/></term><description>4</description></item>
    /// <item><term>Else</term><description>1</description></item>
    /// </list>
    /// </remarks>
    public static int TopologyIndexStep(MeshTopology topology)
    {
      switch (topology)
      {
      case MeshTopology.Triangles:
        return 3;
      case MeshTopology.Quads:
        return 4;
      case MeshTopology.Lines:
        return 2;
      case MeshTopology.LineStrip:
        return 1;
      case MeshTopology.Points:
        return 1;
      }
      return 1;
    }

    /// <summary>
    /// Invoke <see cref="OnMeshAdded"/>
    /// </summary>
    /// <param name="entry"></param>
    protected void NotifyMeshAdded(MeshEntry entry)
    {
      if (OnMeshAdded != null)
      {
        OnMeshAdded(entry);
      }
    }


    /// <summary>
    /// Invoke <see cref="OnMeshRemoved"/>
    /// </summary>
    /// <param name="entry"></param>
    protected void NotifyMeshRemoved(MeshEntry entry)
    {
      if (OnMeshRemoved != null)
      {
        OnMeshRemoved(entry);
      }
    }


    /// <summary>
    /// Invoke <see cref="OnMeshFinalised"/>
    /// </summary>
    /// <param name="entry"></param>
    protected void NotifyMeshFinalised(MeshEntry entry)
    {
      if (OnMeshFinalised != null)
      {
        OnMeshFinalised(entry);
      }
    }


    private Dictionary<uint, MeshEntry> _meshes = new Dictionary<uint, MeshEntry>();
  }
}
