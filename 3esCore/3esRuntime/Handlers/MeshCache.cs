using System.Collections.Generic;
using System.IO;
using Tes.Net;
using Tes.Runtime;
using Tes.Shapes;
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
      /// Access the object being used to construct the Unity mesh objects.
      /// </summary>
      public MeshBuilder Builder { get { return _builder; } }
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
      public MeshTopology Topology { get { return Builder.Topology; } }
      /// <summary>
      /// Array of finalised mesh parts. Valid when <see cref="Finalised"/>.
      /// </summary>
      public Mesh[] FinalMeshes { get { return Builder.GetMeshes(); } }
      /// <summary>
      /// True when finalised.
      /// </summary>
      /// <remarks>
      /// May become false on a redefine message.
      /// </remarks>
      public bool Finalised { get; internal set; }

      private MeshBuilder _builder = new MeshBuilder();
    }

    /// <summary>
    /// Wraps a <see cref="MeshDetails"/> object to expose it as a <see cref="MeshResource"/>
    /// for the purposes of serialisation.
    /// </summary>
    /// <remarks>
    /// This approach is used to ensure consistent serialisation, without duplicating code paths.
    /// </remarks>
    private class MeshSerialiser : MeshBase
    {
      /// <summary>
      /// The wrapped mesh details.
      /// </summary>
      public MeshDetails Details { get; private set; }

      /// <summary>
      /// Create a serialiser for the given mesh details.
      /// </summary>
      /// <param name="details">The mesh to serialise.</param>
      public MeshSerialiser(MeshDetails details)
      {
        Details = details;

        // Migrate data fields.
        ID = details.ID;
        Maths.Matrix4 transform = Maths.Rotation.ToMatrix4(Maths.QuaternionExt.FromUnity(details.LocalRotation));
        transform.Translation = Maths.Vector3Ext.FromUnity(details.LocalPosition);
        transform.ApplyScaling(Maths.Vector3Ext.FromUnity(details.LocalScale));
        Transform = transform;
        Tint = Maths.ColourExt.FromUnity(details.Tint).Value;
        DrawType = Details.DrawType;
        CalculateNormals = Details.Builder.CalculateNormals;

        MeshComponentFlag components = 0;

        // Copy arrays into the correct format.
        _vertices = Maths.Vector3Ext.FromUnity(details.Builder.Vertices);
        _normals = Maths.Vector3Ext.FromUnity(details.Builder.Normals);
        _uvs = Maths.Vector2Ext.FromUnity(details.Builder.UVs);
        _colours = Maths.ColourExt.FromUnityUInts(details.Builder.Colours);

        if (details.VertexCount > 0) { components |= MeshComponentFlag.Vertex; }
        if (details.Builder.ExplicitIndices) { components |= MeshComponentFlag.Index; }
        if (_colours != null && _colours.Length > 0) { components |= MeshComponentFlag.Colour; }
        if (_normals != null && _normals.Length > 0) { components |= MeshComponentFlag.Normal; }
        if (_uvs != null && _uvs.Length > 0) { components |= MeshComponentFlag.UV; }
        Components = components;
      }

      #region MeshBase overrides.
      public override int IndexSize { get { return 4; } }

      public override uint VertexCount(int stream = 0)
      {
        // Return how many we have so far, not the expected amount.
        return (uint)_vertices.Length;
      }

      public override uint IndexCount(int stream = 0)
      {
        // Return how many we have so far, not the expected amount.
        return (uint)Details.Builder.Indices.Length;
      }

      public override Tes.Maths.Vector3[] Vertices(int stream = 0) { return _vertices; }


      public override int[] Indices4(int stream = 0) { return Details.Builder.Indices; }

      public override Tes.Maths.Vector3[] Normals(int stream = 0) { return _normals; }

      public override Tes.Maths.Vector2[] UVs(int stream = 0) { return _uvs; }

      public override uint[] Colours(int stream = 0) { return _colours; }
      #endregion

      private Maths.Vector3[] _vertices;
      private Maths.Vector3[] _normals;
      private Maths.Vector2[] _uvs;
      private uint[] _colours;
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
      msg.VertexCount = (uint)mesh.Builder.VertexCount;
      msg.IndexCount = (uint)(mesh.Builder.ExplicitIndices ? mesh.Builder.IndexCount : 0);
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
      if (!packet.FinalisePacket())
      {
        return new Error(ErrorCode.SerialisationFailure);
      }
      packet.ExportTo(writer);

      // Now use the MeshResource methods to complete serialisation.
      MeshSerialiser serialiser = new MeshSerialiser(mesh);
      TransferProgress prog = new TransferProgress();
      prog.Reset();
      while (!prog.Complete)
      {
        serialiser.Transfer(packet, 0, ref prog);
        if (!packet.FinalisePacket())
        {
          return new Error(ErrorCode.SerialisationFailure);
        }
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

      meshDetails.VertexCount = (int)msg.VertexCount;
      meshDetails.IndexCount = (int)msg.IndexCount;
      meshDetails.DrawType = msg.DrawType;
      switch (msg.DrawType)
      {
      case (byte)MeshDrawType.Points:
        // No break.
      case (byte)MeshDrawType.Voxels:
          meshDetails.Builder.Topology = MeshTopology.Points;
        break;
      case (byte)MeshDrawType.Lines:
          meshDetails.Builder.Topology = MeshTopology.Lines;
        break;
      case (byte)MeshDrawType.Triangles:
          meshDetails.Builder.Topology = MeshTopology.Triangles;
        break;
      default:
        return new Error(ErrorCode.UnsupportedFeature, msg.DrawType);
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

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshDetails.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector3 v = Vector3.zero;
      for (int vInd = 0; ok && vInd < msg.Count; ++vInd)
      {
        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();
        v.z = reader.ReadSingle();
        meshDetails.Builder.UpdateVertex(vInd + voffset, v);
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

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshDetails.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.VertexColour);
      }

      // Check for settings initial bounds.
      bool ok = true;
      uint colour;
      for (int vInd = 0; ok && vInd < msg.Count; ++vInd)
      {
        colour = reader.ReadUInt32();
        meshDetails.Builder.UpdateColour(vInd + voffset, ShapeComponent.ConvertColour(colour));
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

      int ioffset = (int)msg.Offset;
      // Bounds check.
      int indexCount = (int)meshDetails.IndexCount;
      if (ioffset >= indexCount || ioffset + msg.Count > indexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Index);
      }

      // Check for settings initial bounds.
      bool ok = true;
      int index;
      for (int iInd = 0; ok && iInd < msg.Count; ++iInd)
      {
        index = reader.ReadInt32();
        meshDetails.Builder.UpdateIndex(iInd + ioffset, index);
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

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshDetails.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
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
        meshDetails.Builder.UpdateNormal(vInd + voffset, n);
        meshDetails.Builder.BoundsPadding.x = Mathf.Max(meshDetails.Builder.BoundsPadding.x, Mathf.Abs(n.x));
        meshDetails.Builder.BoundsPadding.y = Mathf.Max(meshDetails.Builder.BoundsPadding.y, Mathf.Abs(n.y));
        meshDetails.Builder.BoundsPadding.z = Mathf.Max(meshDetails.Builder.BoundsPadding.z, Mathf.Abs(n.z));
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

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshDetails.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.UV);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector2 uv = Vector2.zero;
      for (int vInd = 0; ok && vInd < msg.Count; ++vInd)
      {
        uv.x = reader.ReadSingle();
        uv.y = reader.ReadSingle();
        meshDetails.Builder.UpdateUV(vInd + voffset, uv);
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
      }
      if (msg.IndexCount != 0)
      {
        meshDetails.IndexCount = (int)msg.IndexCount;
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
      bool haveNormals = meshDetails.Builder.Normals.Length > 0;
      switch (meshDetails.Topology)
      {
      case MeshTopology.Triangles:
      case MeshTopology.Quads:
        if (haveNormals || generateNormals)
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
          generateNormals = !haveNormals;
        }
        else if (haveNormals)
        {
          meshDetails.Material = PointsLitMaterial;
          meshDetails.Material.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        }
        else
        {
          meshDetails.Material = PointsUnlitMaterial;
          meshDetails.Material.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
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

      // Generate the meshes here.
      Error err = new Error();
      meshDetails.Builder.CalculateNormals = generateNormals;
      meshDetails.Builder.GetMeshes();

      meshDetails.Finalised = true;
      NotifyMeshFinalised(meshDetails);

      return err;
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
