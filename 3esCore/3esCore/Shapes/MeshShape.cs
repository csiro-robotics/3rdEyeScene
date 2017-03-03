using System;
using System.IO;
using Tes.IO;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines a simple mesh shape for remote rendering.
  /// </summary>
  /// <remarks>
  /// This equates to so called "immediate mode" rendering and should only be used for
  /// small data sets.
  /// 
  /// The shape can be used to render triangles, lines or points. The vertex and/or index data
  /// must match the topology. That is, there must be three indices or vertices per triangle.
  /// 
  /// The shape does support splitting large data sets.
  /// </remarks>
  public class MeshShape : Shape
  {
    /// <summary>
    /// Codes for <see cref="WriteData(PacketBuffer, ref uint)"/>.
    /// </summary>
    public enum SendDataType : ushort
    {
      /// <summary>
      /// Sending vertex data.
      /// </summary>
      Vertices,
      /// <summary>
      /// Sending index data.
      /// </summary>
      Indices,
      /// <summary>
      /// Sending per vertex normals.
      /// </summary>
      Normals,
      /// <summary>
      /// Sending a single normals for all vertices (voxel extents).
      /// </summary>
      UniformNormal
    };

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, Vector3 position, Quaternion rotation, Vector3 scale)
      : this(drawType, vertices, null, position, rotation, scale) { }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, Vector3 position, Quaternion rotation, Vector3 scale)
      : base((ushort)Tes.Net.ShapeID.Mesh)
    {
      IsComplex = true;
      _vertices = vertices;
      _indices = indices ?? new int[0];
      DrawType = drawType;
      Position = position;
      Rotation = rotation;
      Scale = scale;
    }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, Vector3 position, Quaternion rotation, Vector3 scale)
      : this(drawType, vertices, null, id, position, rotation, scale) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, Vector3 position, Quaternion rotation, Vector3 scale)
      : base((ushort)Tes.Net.ShapeID.Mesh, id)
    {
      IsComplex = true;
      _vertices = vertices;
      _indices = indices ?? new int[0];
      DrawType = drawType;
      Position = position;
      Rotation = rotation;
      Scale = scale;
    }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, ushort category, Vector3 position, Quaternion rotation, Vector3 scale)
      : this(drawType, vertices, null, id, category, position, rotation, scale) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, ushort category, Vector3 position, Quaternion rotation, Vector3 scale)
      : base((ushort)Tes.Net.ShapeID.Mesh, id, category)
    {
      IsComplex = true;
      _vertices = vertices;
      _indices = indices ?? new int[0];
      DrawType = drawType;
      Position = position;
      Rotation = rotation;
      Scale = scale;
    }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, Vector3 position, Quaternion rotation)
      : this(drawType, vertices, null, position, rotation) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, Vector3 position, Quaternion rotation)
      : this(drawType, vertices, indices, position, rotation, Vector3.One)
    { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="position">Local position.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, Vector3 position)
      : this(drawType, vertices, null, position) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="position">Local position.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, Vector3 position)
      : this(drawType, vertices, indices, position, Quaternion.Identity, Vector3.One)
    { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices) : this(drawType, vertices, null) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices)
      : this(drawType, vertices, indices, Vector3.Zero, Quaternion.Identity, Vector3.One)
    { }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, Vector3 position, Quaternion rotation)
      : this(drawType, vertices, null, id, position, rotation) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, Vector3 position, Quaternion rotation)
      : this(drawType, vertices, indices, id, position, rotation, Vector3.One)
    { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Local position.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, Vector3 position)
      : this(drawType, vertices, null, id, position) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="position">Local position.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, Vector3 position)
      : this(drawType, vertices, indices, id, position, Quaternion.Identity, Vector3.One)
    { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id)
      : this(drawType, vertices, null, id) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id)
      : this(drawType, vertices, indices, id, Vector3.Zero, Quaternion.Identity, Vector3.One)
    { }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, ushort category, Vector3 position, Quaternion rotation)
      : this(drawType, vertices, null, id, category, position, rotation) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, ushort category, Vector3 position, Quaternion rotation)
      : this(drawType, vertices, indices, id, category, position, rotation, Vector3.One)
    { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Local position.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, ushort category, Vector3 position)
      : this(drawType, vertices, null, id, category, position) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="position">Local position.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, ushort category, Vector3 position)
      : this(drawType, vertices, indices, id, category, position, Quaternion.Identity, Vector3.One)
    { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id, ushort category)
      : this(drawType, vertices, null, id, category) { }
    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id, ushort category)
      : this(drawType, vertices, indices, id, category, Vector3.Zero, Quaternion.Identity, Vector3.One)
    { }

    /// <summary>
    /// Reflects the state of the flag <see cref="MeshShapeFlag.CalculateNormals"/>
    /// </summary>
    public bool CalculateNormals
    {
      get
      {
        return (_data.Flags & (ushort)MeshShapeFlag.CalculateNormals) != 0;
      }

      set
      {
        _data.Flags &= (ushort)~MeshShapeFlag.CalculateNormals;
        _data.Flags |= (ushort)((value) ? MeshShapeFlag.CalculateNormals : 0);
      }
    }

    /// <summary>
    /// Optional normals access.
    /// </summary>
    public Vector3[] Normals
    {
      get { return _normals; }
      set { _normals = value; if (_normals != null) { CalculateNormals = false; } }
    }

    /// <summary>
    /// Set a single normal to be applied to all vertices (e.g. for voxels).
    /// </summary>
    /// <param name="normal">The shared normal.</param>
    public void SetUniformNormal(Vector3 normal)
    {
      Normals = new Vector3[] { normal };
    }

    /// <summary>
    /// Defines the mesh topology.
    /// </summary>
    public MeshDrawType DrawType { get; protected set; }

    /// <summary>
    /// Overridden to include the triangle count.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    /// <param name="packet">Packet to write the message to.</param>
    public override bool WriteCreate(PacketBuffer packet)
    {
      if (!base.WriteCreate(packet))
      {
        return false;
      }
      // Write number of vertices and indices.
      // Index support to come.
      uint count = (uint)_vertices.Length;
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      count = (uint)_indices.Length;
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      byte drawType = (byte)DrawType;
      packet.WriteBytes(new byte[] { drawType }, false);
      return true;
    }

    /// <summary>
    /// A helper function for writing as many <see cref="DataMessage"/> messsages as required.
    /// </summary>
    /// <param name="routingID">Routing id for the message being composed.</param>
    /// <param name="objectID">ID of the object to which the data belong.</param>
    /// <param name="packet">Data packet to compose in.</param>
    /// <param name="progressMarker">Progress or pagination marker.</param>
    /// <param name="vertices">Mesh vertex array.</param>
    /// <param name="normals">Mesh normal array. One per vertex or just a single normal to apply to all vertices.</param>
    /// <param name="indices">Mesh indices.</param>
    /// <remarks>Call recursively until zero is returned. Packet does not get finalised here.</remarks>
    public static int WriteData(ushort routingID, uint objectID,
                                PacketBuffer packet, ref uint progressMarker,
                                Vector3[] vertices, Vector3[] normals, int[] indices)
    {
      DataMessage msg = new DataMessage();
      // Local byte overhead needs to account for the size of sendType, offset and itemCount.
      // Use a larger value as I haven't got the edge cases quite right yet.
      const int localByteOverhead = 100;
      msg.ObjectID = objectID;
      packet.Reset(routingID, DataMessage.MessageID);
      msg.Write(packet);

      int totalVertices = (vertices != null) ? vertices.Length : 0;
      int totalNormals = (normals != null) ? normals.Length : 0;
      int totalIndices = (indices != null) ? indices.Length : 0;
      uint offset;
      uint itemCount;
      ushort sendCode;  // 0 for vertices, 1 for indices.

      // Must send normals before final vertices/indices as those trigger mesh finalisation.
      if (progressMarker < totalNormals)
      {
        // Estimate element count limit.
        int maxPacketNormals = MeshBase.EstimateTransferCount(12, 0, DataMessage.Size + localByteOverhead);
        sendCode = (totalNormals != 1) ? (ushort)SendDataType.Normals : (ushort)SendDataType.UniformNormal;
        offset = (uint)progressMarker;
        itemCount = (uint)(normals.Length - offset);
        if (itemCount > maxPacketNormals)
        {
          itemCount = (uint)maxPacketNormals;
        }
        packet.WriteBytes(BitConverter.GetBytes(sendCode), true);
        packet.WriteBytes(BitConverter.GetBytes(offset), true);
        packet.WriteBytes(BitConverter.GetBytes(itemCount), true);

        Vector3 n;
        for (uint i = offset; i < (itemCount + offset); ++i)
        {
          n = normals[i];
          packet.WriteBytes(BitConverter.GetBytes(n.X), true);
          packet.WriteBytes(BitConverter.GetBytes(n.Y), true);
          packet.WriteBytes(BitConverter.GetBytes(n.Z), true);
        }

        progressMarker += itemCount;
      }
      else if (progressMarker < totalNormals + totalVertices)
      {
        // Estimate element count limit.
        int maxPacketVertices = MeshBase.EstimateTransferCount(12, 0, DataMessage.Size + localByteOverhead);
        sendCode = (ushort)SendDataType.Vertices; // Vertices
        offset = (uint)(progressMarker - totalNormals);
        itemCount = (uint)(vertices.Length - offset);
        if (itemCount > maxPacketVertices)
        {
          itemCount = (uint)maxPacketVertices;
        }
        packet.WriteBytes(BitConverter.GetBytes(sendCode), true);
        packet.WriteBytes(BitConverter.GetBytes(offset), true);
        packet.WriteBytes(BitConverter.GetBytes(itemCount), true);

        Vector3 v;
        for (uint i = offset; i < (itemCount + offset); ++i)
        {
          v = vertices[i];
          packet.WriteBytes(BitConverter.GetBytes(v.X), true);
          packet.WriteBytes(BitConverter.GetBytes(v.Y), true);
          packet.WriteBytes(BitConverter.GetBytes(v.Z), true);
        }

        progressMarker += itemCount;
      }
      else if (progressMarker < totalNormals + totalVertices + totalIndices)
      {
        // Estimate element count limit.
        int maxPacketIndices = MeshBase.EstimateTransferCount(4, 0, DataMessage.Size + localByteOverhead);
        sendCode = (ushort)SendDataType.Indices; // Indices
        offset = (uint)(progressMarker - totalNormals - totalVertices);
        itemCount = (uint)(indices.Length - offset);
        if (itemCount > maxPacketIndices)
        {
          itemCount = (uint)maxPacketIndices;
        }
        packet.WriteBytes(BitConverter.GetBytes(sendCode), true);
        packet.WriteBytes(BitConverter.GetBytes(offset), true);
        packet.WriteBytes(BitConverter.GetBytes(itemCount), true);

        for (uint i = offset; i < offset + itemCount; ++i)
        {
          packet.WriteBytes(BitConverter.GetBytes(indices[i]), true);
        }

        progressMarker += itemCount;
      }

      // Return 1 while there are more vertices to process.
      return (progressMarker < totalNormals + totalVertices + totalIndices) ? 1 : 0;
    }

    /// <summary>
    /// Overridden to write mesh data.
    /// </summary>
    /// <param name="packet">Packet to write the message to.</param>
    /// <param name="progressMarker">Progress marker.</param>
    /// <returns>Zero when done, 1 otherwise.</returns>
    public override int WriteData(PacketBuffer packet, ref uint progressMarker)
    {
      return WriteData(RoutingID, ID, packet, ref progressMarker, _vertices, _normals, _indices);
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      MeshShape triangles = new MeshShape(DrawType, _vertices, _indices);
      OnClone(triangles);
      return triangles;
    }

    /// <summary>
    /// Vertex data.
    /// </summary>
    private Vector3[] _vertices;
    /// <summary>
    /// Normals data. May contain a single normal, in which case it is applied to all vertices (e.g., for voxels).
    /// </summary>
    private Vector3[] _normals;
    /// <summary>
    /// Index data.
    /// </summary>
    private int[] _indices;
  }
}
