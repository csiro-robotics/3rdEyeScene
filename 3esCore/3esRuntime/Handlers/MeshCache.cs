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

      public RenderMesh Mesh { get; internal set; }

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

      public Matrix4x4 LocalTransform { get; }

      /// <summary>
      /// A tint colour for primitives in this mesh.
      /// </summary>
      public Color32 Tint { get; internal set; }
      /// <summary>
      /// Render material.
      /// </summary>
      public Material Material { get; internal set; }
      /// <summary>
      /// True when finalised.
      /// </summary>
      /// <remarks>
      /// May become false on a redefine message.
      /// </remarks>
      public bool Finalised { get; internal set; }
    }

    /// <summary>
    /// A cache of the material library.
    /// </summary>
    /// <remarks>
    /// Set in <see cref="Initialise(GameObject, GameObject, MaterialLibrary)"/>.
    /// </remarks>
    public MaterialLibrary Materials { get; set; }

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
        RenderMesh mesh = details.Mesh;

        // Migrate data fields.
        ID = details.ID;
        Maths.Matrix4 transform = Maths.Rotation.ToMatrix4(Maths.QuaternionExt.FromUnity(details.LocalRotation));
        transform.Translation = Maths.Vector3Ext.FromUnity(details.LocalPosition);
        transform.ApplyScaling(Maths.Vector3Ext.FromUnity(details.LocalScale));
        Transform = transform;
        Tint = Maths.ColourExt.FromUnity(details.Tint).Value;
        DrawType = (byte)mesh.DrawType;
        // TODO: (KS) track this flag. Mind you, the normals will have been calculated by now...
        CalculateNormals = false;//Details.Builder.CalculateNormals;

        MeshComponentFlag components = 0;

        // Copy arrays into the correct format.
        _vertices = new Maths.Vector3[mesh.VertexCount];
        mesh.VertexBuffer.GetData(_vertices);

        _indices = new int[mesh.IndexCount];
        mesh.GetIndices(_indices);

        if (mesh.HasNormals)
        {
          _normals = new Maths.Vector3[mesh.VertexCount];
          mesh.NormalsBuffer.GetData(_normals);
        }
        if (mesh.HasUVs)
        {
          _uvs = new Maths.Vector2[mesh.VertexCount];
          mesh.UvsBuffer.GetData(_uvs);
        }
        if (mesh.HasColours)
        {
          _colours = new uint[mesh.VertexCount];
          mesh.GetColours(_colours);
        }

        if (mesh.VertexCount > 0) { components |= MeshComponentFlag.Vertex; }
        if (mesh.IndexCount > 0) { components |= MeshComponentFlag.Index; }
        if (_normals != null) { components |= MeshComponentFlag.Normal; }
        if (_uvs != null) { components |= MeshComponentFlag.UV; }
        if (_colours != null) { components |= MeshComponentFlag.Colour; }
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
        return (uint)Details.Mesh.IndexCount;
      }

      public override Tes.Maths.Vector3[] Vertices(int stream = 0) { return _vertices; }


      public override int[] Indices4(int stream = 0) { return _indices; }

      public override Tes.Maths.Vector3[] Normals(int stream = 0) { return _normals; }

      public override Tes.Maths.Vector2[] UVs(int stream = 0) { return _uvs; }

      public override uint[] Colours(int stream = 0) { return _colours; }
      #endregion

      private Maths.Vector3[] _vertices;
      private int[] _indices;
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

    public Material SingleSidedMaterial { get; protected set; }
    public Material DoubleSidedSidedMaterial { get; protected set; }
    public Material TransparentMaterial { get; protected set; }
    public Material PointsMaterial { get; protected set; }

    // /// <summary>
    // /// Material for rendering lit polygons.
    // /// </summary>
    // public Material LitMaterial { get; protected set; }
    // /// <summary>
    // /// Material for rendering unlit polygons.
    // /// </summary>
    // public Material UnlitMaterial { get; protected set; }
    // /// <summary>
    // /// Material for rendering lit points.
    // /// </summary>
    // public Material PointsLitMaterial { get; protected set; }
    // /// <summary>
    // /// Material for rendering unlit points.
    // /// </summary>
    // public Material PointsUnlitMaterial { get; protected set; }

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
    /// Initialise, caching the required materials from <paramref name="materials"/>.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="serverRoot"></param>
    /// <param name="materials"></param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      Materials = materials;
      SingleSidedMaterial = materials[MaterialLibrary.OpaqueMesh];
      DoubleSidedSidedMaterial = materials[MaterialLibrary.OpaqueTwoSidedMesh];
      TransparentMaterial = materials[MaterialLibrary.TransparentMesh];
      // PointsMaterial = materials[MaterialLibrary.Wireframe];
      PointsMaterial = materials[MaterialLibrary.Points];
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
      foreach (var entry in _meshes.Values)
      {
        entry.Mesh.ReleaseBuffers();
      }
      _meshes.Clear();
    }

    /// <summary>
    /// Query details of mesh resource using <paramref name="meshId"/>.
    /// </summary>
    /// <param name="meshId">ID of the resource of interest.</param>
    /// <returns>The mesh details, or null when <paramref name="meshId"/> is unknown.</returns>
    public MeshDetails GetEntry(uint meshId)
    {
      MeshDetails meshEntry = null;
      _meshes.TryGetValue(meshId, out meshEntry);
      return meshEntry;
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
      msg.VertexCount = (uint)mesh.Mesh.VertexCount;
      msg.IndexCount = (uint)mesh.Mesh.IndexCount;
      msg.DrawType = (byte)mesh.Mesh.DrawType;

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

      msg.Attributes.Colour = Maths.ColourExt.FromUnity(mesh.Tint).Value;

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

      MeshDetails meshEntry = _meshes[msg.MeshID];

      NotifyMeshRemoved(meshEntry);

      _meshes.Remove(msg.MeshID);
      if (meshEntry.Mesh != null)
      {
        meshEntry.Mesh.ReleaseBuffers();
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

      if (msg.DrawType < 0 || msg.DrawType > Enum.GetValues(typeof(MeshDrawType)).Length)
      {
        return new Error(ErrorCode.UnsupportedFeature, msg.DrawType);
      }

      MeshDetails meshEntry = new MeshDetails();
      RenderMesh renderMesh = new RenderMesh((MeshDrawType)msg.DrawType, (int)msg.VertexCount, (int)msg.IndexCount);
      meshEntry.Mesh = renderMesh;

      meshEntry.ID = msg.MeshID;
      meshEntry.LocalPosition = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      meshEntry.LocalRotation = new Quaternion(msg.Attributes.RotationX, msg.Attributes.RotationY, msg.Attributes.RotationZ, msg.Attributes.RotationW);
      meshEntry.LocalScale = new Vector3(msg.Attributes.ScaleX, msg.Attributes.ScaleY, msg.Attributes.ScaleZ);
      meshEntry.Tint = Maths.ColourExt.ToUnity32(new Maths.Colour(msg.Attributes.Colour));
      meshEntry.Finalised = false;
      _meshes.Add(meshEntry.ID, meshEntry);

      NotifyMeshAdded(meshEntry);

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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = meshEntry.Mesh.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector3 v = Vector3.zero;
      _v3Buffer.Clear();
      Vector3 minBounds = Vector3.zero;
      Vector3 maxBounds = Vector3.zero;
      for (int vInd = 0; ok && vInd < msg.Count; ++vInd)
      {
        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();
        v.z = reader.ReadSingle();
        if (vInd > 0)
        {
          minBounds.x = Mathf.Min(minBounds.x, v.x);
          minBounds.y = Mathf.Min(minBounds.y, v.z);
          minBounds.z = Mathf.Min(minBounds.z, v.y);
          maxBounds.x = Mathf.Max(maxBounds.x, v.x);
          maxBounds.y = Mathf.Max(maxBounds.y, v.z);
          maxBounds.z = Mathf.Max(maxBounds.z, v.y);
        }
        else
        {
          minBounds = maxBounds = v;
        }
        _v3Buffer.Add(v);
      }
      meshEntry.Mesh.SetVertices(_v3Buffer, 0, voffset, _v3Buffer.Count);
      _v3Buffer.Clear();

      // Update bounds.
      if (meshEntry.Mesh.BoundsSet)
      {
        minBounds.x = Mathf.Min(minBounds.x, meshEntry.Mesh.MinBounds.x);
        minBounds.y = Mathf.Min(minBounds.y, meshEntry.Mesh.MinBounds.z);
        minBounds.z = Mathf.Min(minBounds.z, meshEntry.Mesh.MinBounds.y);
        maxBounds.x = Mathf.Max(maxBounds.x, meshEntry.Mesh.MaxBounds.x);
        maxBounds.y = Mathf.Max(maxBounds.y, meshEntry.Mesh.MaxBounds.z);
        maxBounds.z = Mathf.Max(maxBounds.z, meshEntry.Mesh.MaxBounds.y);
      }

      meshEntry.Mesh.MinBounds = minBounds;
      meshEntry.Mesh.MaxBounds = maxBounds;

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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshEntry.Mesh.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.VertexColour);
      }

      // Check for settings initial bounds.
      bool ok = true;
      uint colour;
      _uintBuffer.Clear();
      for (int vInd = 0; ok && vInd < msg.Count; ++vInd)
      {
        colour = reader.ReadUInt32();
        _uintBuffer.Add(colour);
      }
      meshEntry.Mesh.SetColours(_uintBuffer, 0, voffset, _uintBuffer.Count);
      _uintBuffer.Clear();

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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      int ioffset = (int)msg.Offset;
      // Bounds check.
      int indexCount = (int)meshEntry.Mesh.IndexCount;
      if (ioffset >= indexCount || ioffset + msg.Count > indexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Index);
      }

      // Check for settings initial bounds.
      bool ok = true;
      int index;
      _intBuffer.Clear();
      for (int iInd = 0; ok && iInd < msg.Count; ++iInd)
      {
        index = reader.ReadInt32();
        _intBuffer.Add(index);
      }
      meshEntry.Mesh.SetIndices(_intBuffer, 0, ioffset, _intBuffer.Count);
      _intBuffer.Clear();

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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshEntry.Mesh.VertexCount;
      if (voffset >= vertexCount || voffset + msg.Count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Normal);
      }

      // Check for settings initial bounds.
      bool ok = true;
      Vector3 n = Vector3.zero;
      _v3Buffer.Clear();
      Vector3 boundsPadding = Vector3.zero;
      for (int vInd = 0; ok && vInd < (int)msg.Count; ++vInd)
      {
        n.x = reader.ReadSingle();
        n.y = reader.ReadSingle();
        n.z = reader.ReadSingle();
        _v3Buffer.Add(n);
        // Bounds padding used to cater for voxel rendering.
        boundsPadding.x = Mathf.Max(boundsPadding.x, Mathf.Abs(n.x));
        boundsPadding.y = Mathf.Max(boundsPadding.y, Mathf.Abs(n.y));
        boundsPadding.z = Mathf.Max(boundsPadding.z, Mathf.Abs(n.z));
      }
      meshEntry.Mesh.SetNormals(_v3Buffer, 0, voffset, _v3Buffer.Count);
      _v3Buffer.Clear();

      // Pad the bounds by largest the normal for voxels.
      if (meshEntry.Mesh.DrawType == MeshDrawType.Voxels)
      {
        boundsPadding.x = Mathf.Max(boundsPadding.x, Mathf.Abs(meshEntry.Mesh.BoundsPadding.x));
        boundsPadding.y = Mathf.Max(boundsPadding.y, Mathf.Abs(meshEntry.Mesh.BoundsPadding.y));
        boundsPadding.z = Mathf.Max(boundsPadding.z, Mathf.Abs(meshEntry.Mesh.BoundsPadding.z));
        meshEntry.Mesh.BoundsPadding = boundsPadding;
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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (msg.Count == 0)
      {
        return new Error();
      }

      int voffset = (int)msg.Offset;
      // Bounds check.
      int vertexCount = (int)meshEntry.Mesh.VertexCount;
      _v2Buffer.Clear();
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
        _v2Buffer.Add(uv);
      }
      meshEntry.Mesh.SetUVs(_v2Buffer, 0, voffset, _v2Buffer.Count);
      _v2Buffer.Clear();

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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      meshEntry.Finalised = false;
      if (msg.VertexCount != meshEntry.Mesh.VertexCount)
      {
        meshEntry.Mesh.SetVertexCount((int)msg.VertexCount);
      }
      if (msg.IndexCount != 0)
      {
        meshEntry.Mesh.SetIndexCount((int)msg.IndexCount);
      }

      meshEntry.ID = msg.MeshID;
      meshEntry.LocalPosition = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      meshEntry.LocalRotation = new Quaternion(msg.Attributes.RotationX, msg.Attributes.RotationY, msg.Attributes.RotationZ, msg.Attributes.RotationW);
      meshEntry.LocalScale = new Vector3(msg.Attributes.ScaleX, msg.Attributes.ScaleY, msg.Attributes.ScaleZ);
      meshEntry.Tint = Maths.ColourExt.ToUnity32(new Maths.Colour(msg.Attributes.Colour));

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

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      if (meshEntry.Finalised)
      {
        return new Error(ErrorCode.MeshAlreadyFinalised, meshEntry.ID);
      }

      bool generateNormals = (msg.Flags & (uint)MeshBuildFlags.CalculateNormals) != 0;
      if (generateNormals)
      {
        GenerateNormals(meshEntry.Mesh);
      }

      SelectMaterial(meshEntry.Mesh);

      // Generate the meshes here.
      meshEntry.Finalised = true;
      NotifyMeshFinalised(meshEntry);
      return new Error();
    }

    void SelectMaterial(RenderMesh mesh)
    {
      switch (mesh.Topology)
      {
      case MeshTopology.Triangles:
      case MeshTopology.Quads:
        mesh.Material = new Material(SingleSidedMaterial);

        if (mesh.HasNormals)
        {
          mesh.Material.EnableKeyword("WITH_NORMALS");
        }

        if (mesh.HasColours)
        {
          mesh.Material.EnableKeyword("WITH_COLOURS_UINT");
        }
        break;
      case MeshTopology.Points:
        if (mesh.DrawType == MeshDrawType.Voxels)
        {
          mesh.Material = new Material(VoxelsMaterial);
        }
        else
        {
          mesh.Material = new Material(PointsMaterial);
          mesh.Material.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);

          if (mesh.HasNormals)
          {
            mesh.Material.EnableKeyword("WITH_NORMALS");
          }

          if (mesh.HasColours)
          {
            mesh.Material.EnableKeyword("WITH_COLOURS_UINT");
          }

          int pointSize = (Materials != null) ? Materials.DefaultPointSize : 4;
          mesh.Material.SetInt("_PointSize", pointSize);
        }
        break;
      default:
      case MeshTopology.Lines:
      case MeshTopology.LineStrip:
        mesh.Material = new Material(SingleSidedMaterial);
        if (mesh.HasColours)
        {
          mesh.Material.EnableKeyword("WITH_COLOURS_UINT");
        }
        break;
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

    private void GenerateNormals(RenderMesh mesh)
    {
      if (mesh.DrawType == MeshDrawType.Triangles)
      {
        if (_vertexArray.Length < mesh.VertexCount)
        {
          _vertexArray = new Vector3[mesh.VertexCount];
        }
        if (_normalsArray.Length < mesh.VertexCount)
        {
          _normalsArray = new Vector3[mesh.VertexCount];
        }
        if (_indexArray.Length < mesh.IndexCount)
        {
          _indexArray = new int[mesh.IndexCount];
        }
        mesh.GetVertices(_vertexArray);
        mesh.GetIndices(_indexArray);

        // Accumulate per face normals in the vertices.
        int faceStride = 3;
        Vector3 edgeA = Vector3.zero;
        Vector3 edgeB = Vector3.zero;
        Vector3 partNormal = Vector3.zero;
        for (int i = 0; i < _indexArray.Length; i += faceStride)
        {
          // Generate a normal for the current face.
          edgeA = _vertexArray[_indexArray[i + 1]] - _vertexArray[_indexArray[i + 0]];
          edgeB = _vertexArray[_indexArray[i + 2]] - _vertexArray[_indexArray[i + 1]];
          partNormal = Vector3.Cross(edgeB, edgeA);
          for (int v = 0; v < faceStride; ++v)
          {
            _normalsArray[_indexArray[i + v]] += partNormal;
          }
        }

        // Normalise all the part normals.
        for (int i = 0; i < _normalsArray.Length; ++i)
        {
          _normalsArray[i] = _normalsArray[i].normalized;
        }

        mesh.SetNormals(_normalsArray);
      }
    }

    /// <summary>
    /// Invoke <see cref="OnMeshAdded"/>
    /// </summary>
    /// <param name="meshEntry"></param>
    protected void NotifyMeshAdded(MeshDetails meshEntry)
    {
      if (OnMeshAdded != null)
      {
        OnMeshAdded(meshEntry);
      }
    }


    /// <summary>
    /// Invoke <see cref="OnMeshRemoved"/>
    /// </summary>
    /// <param name="meshEntry"></param>
    protected void NotifyMeshRemoved(MeshDetails meshEntry)
    {
      if (OnMeshRemoved != null)
      {
        OnMeshRemoved(meshEntry);
      }
    }


    /// <summary>
    /// Invoke <see cref="OnMeshFinalised"/>
    /// </summary>
    /// <param name="meshEntry"></param>
    protected void NotifyMeshFinalised(MeshDetails meshEntry)
    {
      if (OnMeshFinalised != null)
      {
        OnMeshFinalised(meshEntry);
      }
    }


    private Dictionary<uint, MeshDetails> _meshes = new Dictionary<uint, MeshDetails>();
    private List<Vector3> _v3Buffer = new List<Vector3>();
    private Vector3[] _vertexArray = new Vector3[0];
    private Vector3[] _normalsArray = new Vector3[0];
    private int[] _indexArray = new int[0];
    private List<Vector2> _v2Buffer = new List<Vector2>();
    private List<uint> _uintBuffer = new List<uint>();
    private List<int> _intBuffer = new List<int>();
  }
}
