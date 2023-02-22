using System.Collections.Generic;
using System.IO;
using Tes.Buffers;
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
        DrawScale = mesh.DrawScale;
        // TODO: (KS) track this flag. Mind you, the normals will have been calculated by now...
        CalculateNormals = false;//Details.Builder.CalculateNormals;

        MeshComponentFlag components = 0;

        // Copy arrays into the correct format.
        _vertices = Maths.Vector3Ext.FromUnity(mesh.Vertices);

        if (mesh.Indices != null)
        {
          _indices = new int[mesh.IndexCount];
          Array.Copy(mesh.Indices, _indices, _indices.Length);
        }

        if (mesh.HasNormals)
        {
          _normals = Maths.Vector3Ext.FromUnity(mesh.Normals);
        }
        if (mesh.HasUVs)
        {
          _uvs = Maths.Vector2Ext.FromUnity(mesh.UVs);
        }
        if (mesh.HasColours)
        {
          _colours = new uint[mesh.VertexCount];
          Array.Copy(mesh.Colours, _colours, _colours.Length);
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

      float drawScale = 0;
      if ((msg.Flags & (ushort)MeshCreateFlag.DrawScale) != 0u)
      {
        drawScale = reader.ReadSingle();
      }

      MeshDetails meshEntry = new MeshDetails();
      RenderMesh renderMesh = new RenderMesh((MeshDrawType)msg.DrawType, (int)msg.VertexCount, (int)msg.IndexCount);
      renderMesh.DrawScale = drawScale;
      meshEntry.Mesh = renderMesh;

      meshEntry.ID = msg.MeshID;
      meshEntry.LocalPosition = new Vector3((float)msg.Attributes.X, (float)msg.Attributes.Y, (float)msg.Attributes.Z);
      meshEntry.LocalRotation = new Quaternion((float)msg.Attributes.RotationX, (float)msg.Attributes.RotationY,
                                               (float)msg.Attributes.RotationZ, (float)msg.Attributes.RotationW);
      meshEntry.LocalScale = new Vector3((float)msg.Attributes.ScaleX, (float)msg.Attributes.ScaleY,
                                        (float)msg.Attributes.ScaleZ);
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

      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      if (count == 0)
      {
        return new Error();
      }

      DataBuffer readBuffer = new DataBuffer();
      // We read into a temporary buffer with a zero offset.
      // We use the offset later to place in the destination buffer.
      readBuffer.Read(reader, 0, count);

      // Bounds check.
      int vertexCount = meshEntry.Mesh.VertexCount;
      if (offset >= vertexCount || offset + count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      if (readBuffer.ComponentCount != 3)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      meshEntry.Mesh.SetVertices(readBuffer, offset, true);

      // Update bounds.
      meshEntry.Mesh.MinBounds = meshEntry.Mesh.MinBounds;
      meshEntry.Mesh.MaxBounds = meshEntry.Mesh.MaxBounds;

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
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Vertex);
      }

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      if (count == 0)
      {
        return new Error();
      }

      DataBuffer readBuffer = new DataBuffer();
      // We read into a temporary buffer with a zero offset.
      // We use the offset later to place in the destination buffer.
      readBuffer.Read(reader, 0, count);

      // Bounds check.
      int vertexCount = meshEntry.Mesh.VertexCount;
      if (offset >= vertexCount || offset + count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      if (readBuffer.ComponentCount != 1)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      meshEntry.Mesh.SetColours(readBuffer, offset);

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
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Vertex);
      }

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      if (count == 0)
      {
        return new Error();
      }

      DataBuffer readBuffer = new DataBuffer();
      // We read into a temporary buffer with a zero offset.
      // We use the offset later to place in the destination buffer.
      readBuffer.Read(reader, 0, count);

      // Bounds check.
      int indexCount = meshEntry.Mesh.IndexCount;
      if (offset >= indexCount || offset + count > indexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      if (readBuffer.ComponentCount != 1)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      meshEntry.Mesh.SetIndices(readBuffer, offset);

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
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Vertex);
      }

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      if (count == 0)
      {
        return new Error();
      }

      DataBuffer readBuffer = new DataBuffer();
      // We read into a temporary buffer with a zero offset.
      // We use the offset later to place in the destination buffer.
      readBuffer.Read(reader, 0, count);

      // Bounds check.
      int vertexCount = meshEntry.Mesh.VertexCount;
      if (offset >= vertexCount || offset + count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      if (readBuffer.ComponentCount != 3)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      meshEntry.Mesh.SetNormals(readBuffer, offset);

      // Pad the bounds by largest the normal for voxels.
      if (meshEntry.Mesh.DrawType == MeshDrawType.Voxels)
      {
        // Calculate bounds padding used to cater for voxel rendering.
        Vector3 boundsPadding = meshEntry.Mesh.BoundsPadding;
        for (int i = offset; i < meshEntry.Mesh.Normals.Length && i < offset + count; ++i)
        {
          // Bounds padding used to cater for voxel rendering.
          Vector3 n = meshEntry.Mesh.Normals[i];
          boundsPadding.x = Mathf.Max(boundsPadding.x, Mathf.Abs(n.x));
          boundsPadding.y = Mathf.Max(boundsPadding.y, Mathf.Abs(n.y));
          boundsPadding.z = Mathf.Max(boundsPadding.z, Mathf.Abs(n.z));
        }
        meshEntry.Mesh.BoundsPadding = boundsPadding;
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
        return new Error(ErrorCode.MalformedMessage, (ushort)MeshMessageType.Vertex);
      }

      MeshDetails meshEntry;
      if (!_meshes.TryGetValue(msg.MeshID, out meshEntry))
      {
        return new Error(ErrorCode.InvalidObjectID, msg.MeshID);
      }

      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      if (count == 0)
      {
        return new Error();
      }

      DataBuffer readBuffer = new DataBuffer();
      // We read into a temporary buffer with a zero offset.
      // We use the offset later to place in the destination buffer.
      readBuffer.Read(reader, 0, count);

      // Bounds check.
      int vertexCount = meshEntry.Mesh.VertexCount;
      if (offset >= vertexCount || offset + count > vertexCount)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      if (readBuffer.ComponentCount != 2)
      {
        return new Error(ErrorCode.IndexingOutOfRange, (ushort)MeshMessageType.Vertex);
      }

      meshEntry.Mesh.SetUVs(readBuffer, offset);

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
      meshEntry.LocalPosition = new Vector3((float)msg.Attributes.X, (float)msg.Attributes.Y, (float)msg.Attributes.Z);
      meshEntry.LocalRotation = new Quaternion((float)msg.Attributes.RotationX, (float)msg.Attributes.RotationY,
                                               (float)msg.Attributes.RotationZ, (float)msg.Attributes.RotationW);
      meshEntry.LocalScale = new Vector3((float)msg.Attributes.ScaleX, (float)msg.Attributes.ScaleY,
                                         (float)msg.Attributes.ScaleZ);
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
        meshEntry.Mesh.CalculateNormals();
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
