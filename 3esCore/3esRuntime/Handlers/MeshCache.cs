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
    public class MeshDetails
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
      /// The bounds for the entire vertex data.
      /// </summary>
      public Bounds VertexBounds;// { get; internal set; }
      /// <summary>
      /// The largest vertex normal.
      /// </summary>
      /// <remarks>
      /// This is primarily used for expanding the <see cref="VertexBounds"/> when drawing voxels.
      /// Remembering that the normal values mark the extents of each voxel, the vertex bounds are
      /// expanded by the largest normal.
      /// </remarks>
      public Vector3 LargestNormal = Vector3.zero;// { get; internal set; }
      /// <summary>
      /// Array of finalised mesh parts. Valid when <see cref="Finalised"/>.
      /// </summary>
      public List<Mesh> FinalMeshes { get; internal set; }
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
    /// Material for rendering geometry shader based voxels. Vertices mark the centre, normals the half extents.
    /// </summary>
    public Material VoxelsMaterial { get; protected set; }

    /// <summary>
    /// Delegate for various mesh events.
    /// </summary>
    /// <param name="mesh">The mesh changing state.</param>
    public delegate void MeshNotificationDelegate(MeshDetails mesh);

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
        // No break.
      case MeshDrawType.Voxels:
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
    /// Initialise, caching the required materials from <paramref name="materials"/>.
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
      VoxelsMaterial = materials[MaterialLibrary.Voxels];
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
    public MeshDetails GetEntry(uint meshId)
    {
      MeshDetails meshDetails = null;
      _meshes.TryGetValue(meshId, out meshDetails);
      return meshDetails;
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
      foreach (MeshDetails mesh in _meshes.Values)
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
    protected Error Serialise(MeshDetails mesh, PacketBuffer packet, BinaryWriter writer)
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

      MeshDetails meshDetails = _meshes[msg.MeshID];

      NotifyMeshRemoved(meshDetails);

      _meshes.Remove(msg.MeshID);
      if (meshDetails.FinalMeshes != null)
      { 
        foreach (Mesh mesh in meshDetails.FinalMeshes)
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

      MeshDetails meshDetails = new MeshDetails();
      meshDetails.VertexBounds = new Bounds();

      meshDetails.DrawType = msg.DrawType;
      switch (msg.DrawType)
      {
      case (byte)MeshDrawType.Points:
        // No break.
      case (byte)MeshDrawType.Voxels:
          meshDetails.Topology = MeshTopology.Points;
        break;
      case (byte)MeshDrawType.Lines:
        meshDetails.Topology = MeshTopology.Lines;
        break;
      case (byte)MeshDrawType.Triangles:
        meshDetails.Topology = MeshTopology.Triangles;
        break;
      default:
        return new Error(ErrorCode.UnsupportedFeature, msg.DrawType);
      }

      if (msg.VertexCount != 0)
      {
        meshDetails.VertexCount = (int)msg.VertexCount;
        meshDetails.Vertices = new Vector3[msg.VertexCount];
      }
      if (msg.IndexCount != 0)
      {
        meshDetails.IndexCount = (int)msg.IndexCount;
        meshDetails.Indices = new int[msg.IndexCount];
      }

      meshDetails.ID = msg.MeshID;
      meshDetails.LocalPosition = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      meshDetails.LocalRotation = new Quaternion(msg.Attributes.RotationX, msg.Attributes.RotationY, msg.Attributes.RotationZ, msg.Attributes.RotationW);
      meshDetails.LocalScale = new Vector3(msg.Attributes.ScaleX, msg.Attributes.ScaleY, msg.Attributes.ScaleZ);
      meshDetails.Tint = ShapeComponent.ConvertColour(msg.Attributes.Colour);
      meshDetails.Finalised = false;
      _meshes.Add(meshDetails.ID, meshDetails);

      NotifyMeshAdded(meshDetails);

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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = meshDetails.VertexCount;
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
        meshDetails.Vertices[vInd + voffset] = v;
        meshDetails.VertexBounds.Encapsulate(v);
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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      if (meshDetails.VertexColours == null)
      {
        meshDetails.VertexColours = new Color32[meshDetails.VertexCount];
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = meshDetails.VertexCount;
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
        meshDetails.VertexColours[vInd + voffset] = ShapeComponent.ConvertColour(colour);
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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      uint ioffset = msg.Offset;
      // Bounds check.
      int indexCount = meshDetails.IndexCount;
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
        meshDetails.Indices[iInd + ioffset] = index;
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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      if (meshDetails.Normals == null)
      {
        meshDetails.Normals = new Vector3[meshDetails.VertexCount];
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = meshDetails.VertexCount;
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
        meshDetails.Normals[vInd + voffset] = n;
        meshDetails.LargestNormal.x = Mathf.Max(meshDetails.LargestNormal.x, Mathf.Abs(n.x));
        meshDetails.LargestNormal.y = Mathf.Max(meshDetails.LargestNormal.y, Mathf.Abs(n.y));
        meshDetails.LargestNormal.z = Mathf.Max(meshDetails.LargestNormal.z, Mathf.Abs(n.z));
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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      if (meshDetails.UVs == null)
      {
        meshDetails.UVs = new Vector2[meshDetails.VertexCount];
      }

      uint voffset = msg.Offset;
      // Bounds check.
      int vertexCount = meshDetails.VertexCount;
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
        meshDetails.UVs[vInd + voffset] = uv;
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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      meshDetails.Finalised = false;
      if (msg.VertexCount != meshDetails.VertexCount)
      {
        meshDetails.VertexCount = (int)msg.VertexCount;
        meshDetails.Vertices = ResizeMeshArray(meshDetails.Vertices, meshDetails.VertexCount);
        if (meshDetails.Normals != null)
        {
          meshDetails.Normals = ResizeMeshArray(meshDetails.Normals, meshDetails.VertexCount);
        }
        if (meshDetails.UVs != null)
        {
          meshDetails.UVs = ResizeMeshArray(meshDetails.UVs, meshDetails.VertexCount);
        }
        if (meshDetails.VertexColours != null)
        {
          meshDetails.VertexColours = ResizeMeshArray(meshDetails.VertexColours, meshDetails.VertexCount);
        }
      }
      if (msg.IndexCount != 0)
      {
        meshDetails.IndexCount = (int)msg.IndexCount;
        meshDetails.Indices = ResizeMeshArray(meshDetails.Indices, meshDetails.IndexCount);
      }

      meshDetails.ID = msg.MeshID;
      meshDetails.LocalPosition = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      meshDetails.LocalRotation = new Quaternion(msg.Attributes.RotationX, msg.Attributes.RotationY, msg.Attributes.RotationZ, msg.Attributes.RotationW);
      meshDetails.LocalScale = new Vector3(msg.Attributes.ScaleX, msg.Attributes.ScaleY, msg.Attributes.ScaleZ);
      meshDetails.Tint = ShapeComponent.ConvertColour(msg.Attributes.Colour);

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

      MeshDetails meshDetails;
      if (!_meshes.TryGetValue(msg.MeshID, out meshDetails))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (meshDetails.Finalised)
      {
        return new Error(ErrorCode.MeshAlreadyFinalised, meshDetails.ID);
      }

      bool generateNormals = (msg.Flags & (uint)MeshBuildFlags.CalculateNormals) != 0;
      switch (meshDetails.Topology)
      {
      case MeshTopology.Triangles:
      case MeshTopology.Quads:
        if (meshDetails.Normals != null || generateNormals)
        {
          meshDetails.Material = LitMaterial;
        }
        else
        {
          meshDetails.Material = UnlitMaterial;
        }
        break;
      case MeshTopology.Points:
        generateNormals = false;
        if (meshDetails.DrawType == (byte)MeshDrawType.Voxels)
        {
          meshDetails.Material = VoxelsMaterial;
          generateNormals = meshDetails.Normals == null;
        }
        else if (meshDetails.Normals != null)
        {
          meshDetails.Material = PointsLitMaterial;
        }
        else
        {
          meshDetails.Material = PointsUnlitMaterial;
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
        meshDetails.Material = UnlitMaterial;
        break;
      }

      // Generate the meshes here!
      Error err = GenerateMeshes(meshDetails, generateNormals);

      meshDetails.Finalised = true;
      NotifyMeshFinalised(meshDetails);

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
    /// A helper function to generate the Unit <c>Mesh</c> objects for <paramref name="meshDetails"/>
    /// </summary>
    /// <param name="meshDetails">The mesh object details.</param>
    /// <param name="recalculateNormals">(Re)Calculate normals for the mesh objects?</param>
    /// <returns></returns>
    protected Error GenerateMeshes(MeshDetails meshDetails, bool recalculateNormals)
    {
      int[] meshIndices = null;
      bool sequentialIndexing = false;
      if (meshDetails.IndexCount > 0)
      {
        meshIndices = meshDetails.Indices;
      }
      else if (meshDetails.Topology == MeshTopology.Points)
      {
        sequentialIndexing = true;
      }

      // Need to break the mesh up to keep under the index limit.
      if (meshDetails.VertexCount == 0 || (meshIndices == null && !sequentialIndexing))
      {
        // No current mesh data. Maybe redefined later.
        // Add an empty mesh.
        meshDetails.FinalMeshes = new List<Mesh> { new Mesh() };
      }
      else if (sequentialIndexing && meshDetails.VertexCount < IndexCountLimit || meshIndices != null && meshIndices.Length < IndexCountLimit)
      {
        // Easy: single mesh.
        // Handle mesh redefinition by looking at the FinalMeshes array.
        Mesh mesh = null;
        if (meshDetails.FinalMeshes != null)
        {
          mesh = meshDetails.FinalMeshes[0];
          if (meshDetails.FinalMeshes.Count > 1)
          {
            // Do we need to explicitly destroy the meshes or will garbage collection do it?
            meshDetails.FinalMeshes.RemoveRange(1, meshDetails.FinalMeshes.Count - 1);
          }
        }

        if (mesh == null)
        {
          mesh = new Mesh();
          meshDetails.FinalMeshes = new List<Mesh> { mesh };
        }

        mesh.subMeshCount = 1;
        mesh.vertices = meshDetails.Vertices;
        if (meshDetails.Normals != null)
        {
          mesh.normals = meshDetails.Normals;
        }
        if (meshDetails.UVs != null)
        {
          mesh.uv = meshDetails.UVs;
        }
        if (meshDetails.VertexColours != null)
        {
          mesh.colors32 = meshDetails.VertexColours;
        }

        if (sequentialIndexing)
        {
          // Points only with no specific index list.
          // Construct the index list to match the vertices.
          meshIndices = new int[meshDetails.VertexCount];
          for (int i = 0; i < meshDetails.VertexCount; ++i)
          {
            meshIndices[i] = i;
          }
        }

        if (meshIndices != null)
        {
          mesh.SetIndices(meshIndices, meshDetails.Topology, 0);

          if (recalculateNormals)
          {
            CalculateNormals(mesh, meshDetails);
          }
        }

        // Don't recalculate bounds. Assign directly.
        Bounds meshBounds = new Bounds();
        meshBounds.Encapsulate(meshDetails.VertexBounds);
        if (meshDetails.DrawType == (ushort)MeshDrawType.Voxels)
        {
          // See LargestNormal comment.
          meshBounds.min -= meshDetails.LargestNormal;
          meshBounds.max += meshDetails.LargestNormal;
        }
        mesh.bounds = meshBounds;
      }
      else
      {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = meshDetails.Normals != null ? new List<Vector3>() : null;
        List<Vector2> uvs = meshDetails.UVs != null ? new List<Vector2>() : null;
        List<Color32> colours = meshDetails.VertexColours != null ? new List<Color32>() : null;
        List<int> indices = new List<int>();
        // Index mappings from original indices to mesh part indices.
        // Mapping is 1 based, so zero is unmapped and 1 is vertex 0, 2 is vertex 1, etc.
        List<int> indexMap = null;
        int indicesStep = TopologyIndexStep(meshDetails.Topology);
        int meshIndex = 0;
        int indexCount = 0;

        if (sequentialIndexing)
        {
          indexCount = meshDetails.VertexCount;
        }
        else
        {
          indexCount = meshIndices.Length;
          indexMap = new List<int>(meshIndices.Length);
          // Fill indexMap.
          for (int i = 0; i < meshIndices.Length; ++i)
          {
            indexMap.Add(-1);
          }
        }

        // For now, just copy all the vertex data. This would work well if we could share
        // the vertex data between meshes.
        indices.Capacity = IndexCountLimit;
        for (int iidx = 0; iidx < indexCount; ++meshIndex)
        {
          Bounds meshBounds = new Bounds();
          vertices.Clear();
          if (normals != null) { normals.Clear(); }
          if (uvs != null) { uvs.Clear(); }
          if (colours != null) { colours.Clear(); }
          indices.Clear();

          if (!sequentialIndexing)
          {
            while (indices.Count + indicesStep < IndexCountLimit && iidx < meshIndices.Length)
            {
              for (int i = 0; i < indicesStep; ++i)
              {
                CopyIndex(iidx++, indexMap, meshDetails, indices, vertices, normals, uvs, colours, meshBounds);
              }
            }
          }
          else
          {
            while (indices.Count + indicesStep < IndexCountLimit && iidx < meshDetails.VertexCount)
            {
              for (int i = 0; i < indicesStep; ++i)
              {
                indices.Add(indices.Count);
                vertices.Add(meshDetails.Vertices[iidx]);
                meshBounds.Encapsulate(meshDetails.Vertices[iidx]);
                if (normals != null)
                {
                  // Support single uniform normal.
                  if (meshDetails.Normals.Length > 1)
                  {
                    normals.Add(meshDetails.Normals[iidx]);
                  }
                  else
                  {
                    normals.Add(meshDetails.Normals[0]);
                  }
                }
                if (uvs != null) { uvs.Add(meshDetails.UVs[iidx]); }
                if (colours != null) { colours.Add(meshDetails.VertexColours[iidx]); }
                ++iidx;
              }
            }
          }

          if (meshDetails.DrawType == (ushort)MeshDrawType.Voxels)
          {
            // See comment on LargestNormal.
            meshBounds.min -= meshDetails.LargestNormal;
            meshBounds.max += meshDetails.LargestNormal;
          }

          // Hit the limit or done. Extract a mesh.
          // Either modify an existing mesh or generate a new one as required.
          Mesh mesh = NextMesh(meshIndex, meshDetails.FinalMeshes);
          mesh.subMeshCount = 1;
          mesh.vertices = vertices.ToArray();
          if (normals != null) { mesh.normals = normals.ToArray(); }
          if (uvs != null) { mesh.uv = uvs.ToArray(); }
          if (colours != null) { mesh.colors32 = colours.ToArray(); }
          mesh.SetIndices(indices.ToArray(), meshDetails.Topology, 0);

          if (recalculateNormals)
          {
            CalculateNormals(mesh, meshDetails);
          }
          // Assign calculated bounds.
          mesh.bounds = meshBounds;

          // Clear the index map.
          if (!sequentialIndexing && iidx + 1 < meshIndices.Length)
          {
            for (int i = 0; i < indexMap.Count; ++i)
            {
              indexMap[i] = -1;
            }
          }

          // Back track to repeat an index for loops.
          if (meshDetails.Topology == MeshTopology.LineStrip)
          {
            --iidx;
          }
        }
      }

      return new Error();
    }

    /// <summary>
    /// (Re)Calculate normals for <paramref name="mesh"/> from <paramref name="meshDetails"/>.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="meshDetails"></param>
    /// <remarks>
    /// Normally uses <c>mesh.RecalculateNormals()</c> unless rendering voxels, in which
    /// case all normals are set to (0.5, 0.5, 0.5) to render unit voxels.
    /// </remarks>
    private void CalculateNormals(Mesh mesh, MeshDetails meshDetails)
    {
      if (meshDetails.DrawType != (byte)MeshDrawType.Voxels)
      {
        mesh.RecalculateNormals();
      }
      else
      {
        // No given for voxels. Make unit voxels.
        Vector3[] voxelExtents = new Vector3[mesh.vertexCount];
        Vector3 unitVoxelExt = 0.5f * Vector3.one;
        for (int i = 0; i < voxelExtents.Length; ++i)
        {
          voxelExtents[i] = unitVoxelExt;
        }
        mesh.normals = voxelExtents;
      }
    }

    /// <summary>
    /// Get or create the mesh at <paramref name="index"/> from <paramref name="existingMeshes"/>
    /// </summary>
    /// <param name="index">The mesh index</param>
    /// <param name="existingMeshes">The mesh array</param>
    /// <returns>The <paramref name="existingMeshes"/> element at <paramref name="index"/>
    /// if valid, or a new <c>Mesh</c> object.</returns>
    private Mesh NextMesh(int index, List<Mesh> existingMeshes)
    {
      if (existingMeshes != null && index < existingMeshes.Count)
      {
        return existingMeshes[index];
      }
      Mesh mesh = new Mesh();
      existingMeshes.Add(mesh);
      return mesh;
    }


    /// <summary>
    /// Copies and maps a vertex from <c>meshDetails.Vertices</c> unless it has already been mapped.
    /// The mapped index is added to the input lists.
    /// </summary>
    /// <remarks>
    /// This is a helper function for mapping mesh data from the original data set into multiple
    /// <c>Mesh</c> objects to respect Unity's vertex count limit.
    /// 
    /// Each entry in <paramref name="indexMap"/> corresponds to an element in <c>meshDetails.Vertices</c>
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
    /// <param name="srcIndicesIndex">The vertex index in <c>meshDetails.Vertices</c></param>
    /// <param name="indexMap">The list of vertex mappings. A zero entry is unmapped.</param>
    /// <param name="meshDetails">The mesh details.</param>
    /// <param name="indices">Indices for the new mesh component.</param>
    /// <param name="vertices">Vertices for the new mesh component.</param>
    /// <param name="normals">Normals for the new mesh component. May be null.</param>
    /// <param name="uvs">UVs for the new mesh component. May be null.</param>
    /// <param name="colours">Colours for the new mesh component. May be null.</param>
    /// <param name="bounds">Modified to encapsulate all addedv vertices.</param>
    protected void CopyIndex(int srcIndicesIndex, List<int> indexMap, MeshDetails meshDetails, List<int> indices,
                             List<Vector3> vertices, List<Vector3> normals,
                             List<Vector2> uvs, List<Color32> colours,
                             Bounds bounds)
    {
      int mapping = indexMap[srcIndicesIndex];
      if (mapping <= 0)
      {
        // New mapping.
        indexMap[srcIndicesIndex] = vertices.Count + 1;
        indices.Add(vertices.Count);
        int vertIndex = (meshDetails.Indices != null) ? meshDetails.Indices[srcIndicesIndex] : srcIndicesIndex;
        Vector3 vert = meshDetails.Vertices[vertIndex];
        vertices.Add(vert);
        bounds.Expand(vert);
        if (normals != null)
        {
          // Support single uniform normal.
          if (meshDetails.Normals.Length > 1)
          {
            normals.Add(meshDetails.Normals[srcIndicesIndex]);
          }
          else
          {
            normals.Add(meshDetails.Normals[0]);
          }
        }
        if (uvs != null) { uvs.Add(meshDetails.UVs[srcIndicesIndex]); }
        if (colours != null) { colours.Add(meshDetails.VertexColours[srcIndicesIndex]); }
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
    /// <param name="meshDetails"></param>
    protected void NotifyMeshAdded(MeshDetails meshDetails)
    {
      if (OnMeshAdded != null)
      {
        OnMeshAdded(meshDetails);
      }
    }


    /// <summary>
    /// Invoke <see cref="OnMeshRemoved"/>
    /// </summary>
    /// <param name="meshDetails"></param>
    protected void NotifyMeshRemoved(MeshDetails meshDetails)
    {
      if (OnMeshRemoved != null)
      {
        OnMeshRemoved(meshDetails);
      }
    }


    /// <summary>
    /// Invoke <see cref="OnMeshFinalised"/>
    /// </summary>
    /// <param name="meshDetails"></param>
    protected void NotifyMeshFinalised(MeshDetails meshDetails)
    {
      if (OnMeshFinalised != null)
      {
        OnMeshFinalised(meshDetails);
      }
    }


    private Dictionary<uint, MeshDetails> _meshes = new Dictionary<uint, MeshDetails>();
  }
}
