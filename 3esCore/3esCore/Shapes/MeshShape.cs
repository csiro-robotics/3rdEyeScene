using System;
using System.Collections.Generic;
using System.IO;
using Tes.Buffers;
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
  /// For point types, the points are coloured by type when the colour value is zero (black, with zero alpha).
  /// This is the default colour for points.
  ///
  /// The shape supports splitting large data sets for transmission.
  /// </remarks>
  public class MeshShape : Shape
  {
    /// <summary>
    /// Codes for <see cref="WriteData(PacketBuffer, ref uint)"/>.
    /// </summary>
    /// <remarks>
    /// This ordering is fixed and assumptions are made in sending and receiving code about this order. In particular,
    /// the relationship between Normals and UniformNormal is important.
    /// </remarks>
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
      /// Sending per vertex, 4 byte colours. See <see cref="Colour"/>.
      /// </summary>
      Colours,

      /// Last send message?
      End = ((ushort)0xffffu)
    };

    /// <summary>
    /// Default constructor creating a transient, empty mesh shape.
    /// </summary>
    public MeshShape() : this(MeshDrawType.Points, new Vector3[0]) { _vertices.ReadOnly = false; }

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
      _vertices = DataBuffer.Wrap(vertices);
      _indices = DataBuffer.Wrap(indices) ?? DataBuffer.Wrap(new int[0]);
      DrawType = drawType;
      Position = position;
      Rotation = rotation;
      Scale = scale;

      if (drawType == MeshDrawType.Points)
      {
        ColourByHeight = true;
      }
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
      _vertices = DataBuffer.Wrap(vertices);
      _indices = DataBuffer.Wrap(indices) ?? DataBuffer.Wrap(new int[0]);
      DrawType = drawType;
      Position = position;
      Rotation = rotation;
      Scale = scale;

      if (drawType == MeshDrawType.Points)
      {
        ColourByHeight = true;
      }
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
      _vertices = DataBuffer.Wrap(vertices);
      _indices = DataBuffer.Wrap(indices) ?? DataBuffer.Wrap(new int[0]);
      DrawType = drawType;
      Position = position;
      Rotation = rotation;
      Scale = scale;

      if (drawType == MeshDrawType.Points)
      {
        ColourByHeight = true;
      }
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
    /// <param name="category">Optional display category for the shape.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, uint id = 0, ushort category = 0)
      : this(drawType, vertices, null, id) { }

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
    /// <param name="indices">Index data. Must match topology.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, int[] indices, uint id = 0, ushort category = 0)
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
    /// Colour <see cref="MeshDrawType.Points"/> by height.
    /// </summary>
    /// <remarks>
    /// This sets the shape colour to zero (black, with zero alpha).
    ///
    /// Ignored for non point types.
    /// </remarks>
    public bool ColourByHeight
    {
      get
      {
        if (DrawType == MeshDrawType.Points)
        {
          return _data.Attributes.Colour == 0;
        }
        return false;
      }

      set
      {
        if (DrawType == MeshDrawType.Points)
        {
          if (value)
          {
            _data.Attributes.Colour = 0;
          }
          else if (_data.Attributes.Colour == 0)
          {
            _data.Attributes.Colour = 0xFFFFFFFFu;
          }
        }
      }
    }

    /// <summary>
    /// Access the draw weight used to (de)emphasise the rendering.
    /// </summary>
    /// <remarks>
    /// This equates to point size for <see cref="MeshDrawType.Points"/> or line width for
    /// <see cref="MeshDrawType.Lines"/>. A zero value indicates use of the viewer default drawing weight.
    ///
    /// The viewer is free to ignore this value.
    /// </remarks>
    public float DrawScale
    {
      get { return _drawScale; }
      set
      {
        _drawScale = value;
      }
    }

    /// <summary>
    /// Access vertex array.
    /// </summary>
    public DataBuffer Vertices { get { return _vertices; } }

    /// <summary>
    /// Optional normals access.
    /// </summary>
    public DataBuffer Normals
    {
      get { return _normals; }
      // set { _normals = value; if (_normals != null) { CalculateNormals = false; } }
    }

    public MeshShape SetNormals(Vector3[] normals)
    {
      _normals = DataBuffer.Wrap(normals);
      return this;
    }

    public MeshShape SetNormals(List<Vector3> normals)
    {
      _normals = DataBuffer.Wrap(normals);
      return this;
    }

    /// <summary>
    /// Set a single normal to be applied to all vertices (e.g. for voxels).
    /// </summary>
    /// <param name="normal">The shared normal.</param>
    public MeshShape SetUniformNormal(Vector3 normal)
    {
      _normals = DataBuffer.Wrap(new Vector3[] { normal });
      return this;
    }

    /// <summary>
    /// Optional 32-bit colours array. See <see cref="Colour"/>.
    /// </summary>
    /// <remarks>
    /// For points, this clears <see cref="ColourByHeight"/>.
    /// </remarks>
    public DataBuffer Colours
    {
      get { return _colours; }
      // set { ColourByHeight = false; _colours = value; }
    }

    public MeshShape SetColours(UInt32[] colours)
    {
      ColourByHeight = false;
      _colours = DataBuffer.Wrap(colours);
      return this;
    }

    public MeshShape SetColours(List<UInt32> colours)
    {
      ColourByHeight = false;
      _colours = DataBuffer.Wrap(colours);
      return this;
    }

    /// <summary>
    /// Read the optional colours array converted to the <see cref="Colour"/> type.
    /// </summary>
    public IEnumerable<Colour> ConvertedColours
    {
      get
      {
        if (_colours != null)
        {
          for (int i = 0; i < _colours.Count; ++i)
          {
            yield return new Colour(_colours.GetUInt32(i));
          }
        }
      }
    }

    /// <summary>
    /// Access indices array.
    /// </summary>
    public DataBuffer Indices { get { return _indices; } }

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
      int count = _vertices.Count;
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      count = _indices.Count;
      packet.WriteBytes(BitConverter.GetBytes(count), true);
      byte drawType = (byte)DrawType;
      packet.WriteBytes(BitConverter.GetBytes(_drawScale), true);
      packet.WriteBytes(new byte[] { drawType }, false);

      return true;
    }

    /// <summary>
    /// A helper structure used to manage data packing in <see cref="WriteData()"/>
    /// </summary>
    struct DataPhase
    {
      /// <summary>
      /// The buffer being packed.
      /// </summary>
      public DataBuffer Buffer;
      /// <summary>
      /// The target data type to packed.
      /// </summary>
      /// <remarks>
      /// This should be the widest target data type. For example, Float64 encompases both double and float types
      /// and UInt64 would allow all UInt widths from { 8, 4, 2, 1 }. The type may be explicit by setting
      /// <see cref="Explicit"/>.
      /// </remarks>
      public DataStreamType TargetType;
      /// <summary>
      /// Allow writing a PackedFloat style buffer?
      /// </summary>
      /// <remarks>
      /// When true, a Float32 buffer will be written as a PackedFloat16 and a Float64 as PackedFloat32.
      /// </remarks>
      public bool AllowPacked;
      /// <summary>
      /// Is the <see cref="TargetType" explicit, disallowing narrower types.
      /// </summary>
      public bool Explicit;

      /// <summary>
      /// Resolve the data type to pack and send as. This deals with data packing and narrowing.
      /// </summary>
      /// <param name="quantisationUnit"></param>
      /// <returns></returns>
      public DataStreamType GetStreamType(double quantisationUnit)
      {
        if (Buffer == null || Explicit)
        {
          // No wiggle room.
          return TargetType;
        }

        // For now, just handle narrowing float types.
        // Later we may add UInt narrowing for index buffers.
        if (TargetType == DataStreamType.Float64)
        {
          if (Buffer.NativePackingType != DataStreamType.Float64)
          {
            // Handle quantisation requests
            if (quantisationUnit == 0)
            {
              return DataStreamType.Float32;
            }
            return DataStreamType.PackedFloat16;
          }
        }

        // Handle quantisation requests
        if (quantisationUnit != 0)
        {
          return DataStreamType.PackedFloat32;
        }

        return TargetType;
      }
    };

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
    /// <param name="colours">Per vertex colours. See <see cref="Colour"/> for format details.</param>
    /// <remarks>Call recursively until zero is returned. Packet does not get finalised here.</remarks>
    public static int WriteData(ushort routingID, uint objectID,
                                PacketBuffer packet, ref uint progressMarker,
                                DataBuffer vertices, DataBuffer normals, DataBuffer indices, DataBuffer colours)
    {
      DataMessage msg = new DataMessage();
      msg.ObjectID = objectID;
      packet.Reset(routingID, DataMessage.MessageID);
      msg.Write(packet);

      uint offset;
      uint itemCount;
      ushort sendType;

      short phaseIndex = 0;
      uint previousPhaseOffset = 0u;

      // This ordering matches the SendDataType
      DataPhase[] phases = new DataPhase[] {
        new DataPhase{ Buffer = vertices, TargetType = DataStreamType.Float64, AllowPacked = true },
        new DataPhase{ Buffer = indices, TargetType = DataStreamType.UInt32 },
        new DataPhase{ Buffer = normals, TargetType = DataStreamType.Float32, AllowPacked = true },
        new DataPhase{ Buffer = colours, TargetType = DataStreamType.UInt32, Explicit = true }
      };

      // While progressMarker is greater than or equal to the sum of the previous phase counts and the current phase count.
      // Also terminate of out of phases. Note: we always skip SendDataType.Normals as that's handled differently when
      // the phaseIndex matches SendDataType.Normals.
      while (phaseIndex < phases.Length &&
            (phases[phaseIndex].Buffer == null ||
             progressMarker >= previousPhaseOffset + phases[phaseIndex].Buffer.Count))
      {
        previousPhaseOffset += (phases[phaseIndex].Buffer != null) ? (uint)phases[phaseIndex].Buffer.Count : 0u;
        ++phaseIndex;
      }

      bool done = false;
      int byteLimit = 0xffff;
      // TODO: quantisation support.
      double quantisationUnit = 0.0;
      if (phaseIndex < phases.Length /* && phases[phaseIndex].Buffer != null  */)
      {
        sendType = (ushort)phaseIndex;
        packet.WriteBytes(BitConverter.GetBytes(sendType), true);
        offset = progressMarker - previousPhaseOffset;
        DataStreamType packingType = phases[phaseIndex].GetStreamType(quantisationUnit);
        if (packingType != DataStreamType.PackedFloat16 && packingType != DataStreamType.PackedFloat32)
        {
          progressMarker += (uint)phases[phaseIndex].Buffer.Write(packet, (int)offset, packingType,
                                                                  byteLimit - packet.Count);
        }
        else
        {
          progressMarker += (uint)phases[phaseIndex].Buffer.WritePacked(packet, (int)offset, packingType,
                                                                        byteLimit - packet.Count, quantisationUnit);
        }
      }
      else
      {
        // Either all done or no data to send.
        // In the latter case, we need to populate the message anyway.
        offset = itemCount = 0;
        sendType = (ushort)SendDataType.End;
        packet.WriteBytes(BitConverter.GetBytes(sendType), true);
        packet.WriteBytes(BitConverter.GetBytes(offset), true);
        packet.WriteBytes(BitConverter.GetBytes(itemCount), true);
        done = true;
      }

      // Return 1 while there is more data to process.
      return (!done) ? 1 : 0;
    }

    /// <summary>
    /// Overridden to write mesh data.
    /// </summary>
    /// <param name="packet">Packet to write the message to.</param>
    /// <param name="progressMarker">Progress marker.</param>
    /// <returns>Zero when done, 1 otherwise.</returns>
    public override int WriteData(PacketBuffer packet, ref uint progressMarker)
    {
      return WriteData(RoutingID, ID, packet, ref progressMarker, _vertices, _normals, _indices, _colours);
    }

    /// <summary>
    /// Read a <see cref="CreateMessage"/> and additional payload.
    /// </summary>
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">Stream to read from</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Read the additional payload to resolve vertex and index counts.
    /// </remarks>
    public override bool ReadCreate(PacketBuffer packet, BinaryReader reader)
    {
      if (!base.ReadCreate(packet, reader))
      {
        return false;
      }

      UInt32 vertexCount;
      UInt32 indexCount;
      byte drawType;

      vertexCount = reader.ReadUInt32();
      indexCount = reader.ReadUInt32();
      _drawScale = reader.ReadSingle();
      drawType = reader.ReadByte();
      DrawType = (MeshDrawType)drawType;

      if (_vertices == null || _vertices.Count != vertexCount)
      {
        _vertices = DataBuffer.Wrap(new Vector3[vertexCount]);
        _vertices.ReadOnly = false;
      }

      if (_indices == null || _indices.Count != indexCount)
      {
        _indices = DataBuffer.Wrap(new uint[indexCount]);
        _indices.ReadOnly = false;
      }

      _normals = null;

      return true;
    }

    /// <summary>
    /// Read <see cref="DataMessage"/> and payload generated by <see cref="WriteData(PacketBuffer, ref uint)"/>.
    /// </summary>
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">Stream to read from</param>
    /// <returns>True on success.</returns>
    public override bool ReadData(PacketBuffer packet, BinaryReader reader)
    {
      DataMessage msg = new DataMessage();

      if (!msg.Read(reader))
      {
        return false;
      }

      if (ID != msg.ObjectID)
      {
        return false;
      }

      int sendDataType = reader.ReadUInt16();
      int offset = (int)reader.ReadUInt32();
      int count = reader.ReadUInt16();

      switch ((SendDataType)sendDataType)
      {
        case SendDataType.Vertices:
          _vertices.Read(reader, offset, count);
          break;
        case SendDataType.Indices:
          _indices.Read(reader, offset, count);
          break;
        case SendDataType.Normals:
          if (_normals == null)
          {
            // If receving just one normal, then we have a single uniform normal for the mesh.
            int normalCount = (offset > 0 || count != 1) ? _vertices.Count : 1;
            _normals = DataBuffer.Wrap(new Vector3[normalCount]);
            _normals.ReadOnly = false;
          }
          _normals.Read(reader, offset, count);
          break;
        case SendDataType.Colours:
          if (_colours == null)
          {
            _colours = new DataBuffer();
            _colours.ReadOnly = false;
          }
          _colours.Read(reader, offset, count);
          break;
        case SendDataType.End:
          // Data completion message.
          if (offset != 0 || count != 0)
          {
            // Unexpected offset/count
            return false;
          }
          break;
      }

      return true;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      // Clone the data buffers. We could consider sharing data.
      Vector3[] vertices = null;
      int[] indices = null;
      Vector3[] normals = null;
      uint[] colours = null;

      Vector3 v = new Vector3();
      if (_vertices != null)
      {
        vertices = new Vector3[_vertices.Count];
        for (int i = 0; i < vertices.Length; ++i)
        {
          v.X = _vertices.GetSingle(i * 3 + 0);
          v.Y = _vertices.GetSingle(i * 3 + 1);
          v.Z = _vertices.GetSingle(i * 3 + 2);
        }
      }

      if (_indices != null)
      {
        indices = new int[_indices.Count];
        _indices.GetRange(indices, 0, indices.Length);
      }

      if (_normals != null)
      {
        normals = new Vector3[_normals.Count];
        for (int i = 0; i < normals.Length; ++i)
        {
          v.X = _normals.GetSingle(i * 3 + 0);
          v.Y = _normals.GetSingle(i * 3 + 1);
          v.Z = _normals.GetSingle(i * 3 + 2);
        }
      }

      if (_colours != null)
      {
        colours = new uint[_colours.Count];
        _colours.GetRange(colours, 0, colours.Length);
      }

      // Create the clone
      MeshShape shape = new MeshShape(DrawType, vertices, indices);
      shape.DrawScale = DrawScale;
      if (normals != null)
      {
        if (normals.Length != 1)
        {
          shape.SetNormals(normals);
        }
        else
        {
          shape.SetUniformNormal(normals[0]);
        }
      }
      if (colours != null)
      {
        shape.SetColours(colours);
      }
      OnClone(shape);
      return shape;
    }

    /// <summary>
    /// Vertex data.
    /// </summary>
    // private Vector3[] _vertices;
    private DataBuffer _vertices;
    /// <summary>
    /// Normals data. May contain a single normal, in which case it is applied to all vertices (e.g., for voxels).
    /// </summary>
    // private Vector3[] _normals;
    private DataBuffer _normals;
    /// <summary>
    /// Per vertex colours.
    /// </summary>
    // private UInt32[] _colours;
    private DataBuffer _colours;
    /// <summary>
    /// Index data.
    /// </summary>
    // private int[] _indices;
    private DataBuffer _indices;
    /// <summary>
    /// Draw weight: equates to point size or line width.
    /// </summary>
    private float _drawScale = 0.0f;
  }
}
