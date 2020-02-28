using System;
using System.Collections.Generic;
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
      UniformNormal,
      /// <summary>
      /// Sending per vertex, 4 byte colours. See <see cref="Colour"/>.
      /// </summary>
      Colours,

      /// Should the received expect a message with an explicit finalisation marker?
      ExpectEnd = ((ushort)1u << 14),
      /// Last send message?
      End = ((ushort)1u << 15)
    };

    #region Data read adaptors
    /// <summary>
    /// Interface for use with the helper method
    /// <see cref="ReadDataComponent(BinaryReader, ComponentAdaptor&lt;Vector3&gt;, ComponentAdaptor&lt;int&gt;, ComponentAdaptor&lt;Vector3&gt;, ComponentAdaptor&lt;Colour&gt;)">ReadDataComponent()</see>.
    /// </summary>
    /// <remarks>
    /// This interface is used to read data from <see cref="DataMessage"/> payloads. The <c>ReadDataComponent()</c> method
    /// uses this interface to prevent read overruns using <see cref="Count"/> and set individual elements via
    /// <see cref="Set(int, T)"/>. For normals and colours, the <see cref="Count"/> property will also be set to ensure
    /// correct array sizing.
    ///
    /// The adaptor may be used reading into a non-array container or to perform data conversion.
    /// </remarks>
    public interface ComponentAdaptor<T>
    {
      /// <summary>
      /// Gets or sets the number of elements in the data set.
      /// </summary>
      int Count { get; set; }

      /// <summary>
      /// Set the element at index <paramref name="at"/>.
      /// </summary>
      /// <param name="at">The index to set a value at.</param>
      /// <param name="val">The value to set.</param>
      void Set(int at, T val);

      /// <summary>
      /// Retrieve the value at index <paramref name="at"/>.
      /// </summary>
      /// <param name="at">The index to retrieve a value at.</param>
			/// <returns>The requested value.</returns>
      T Get(int at);
    }


    /// <summary>
    /// An array adaptor implementation interfacing with an array.
    /// </summary>
    public class ArrayComponentAdaptor<T> : ComponentAdaptor<T>
    {
      /// <summary>
      /// Accesses the underlying array.
      /// </summary>
      /// <value>The array.</value>
      public T[] Array { get; protected set; }

      /// <summary>
      /// Create a wrapper around <paramref name="array."/>.
      /// </summary>
      /// <param name="array">Array.</param>
      public ArrayComponentAdaptor(T[] array)
      {
        Array = array;
      }

      /// <summary>
      /// Retrieve the array length or resize the array (data lost).
      /// </summary>
      public int Count
      {
        get { return (Array != null) ? Array.Length : 0; }
        set { if (Count != value) { Array = new T[value]; } }
      }

      /// <summary>
      /// Set the element at index <paramref name="at"/>.
      /// </summary>
      /// <param name="at">The index to set a value at.</param>
      /// <param name="val">The value to set.</param>
      public void Set(int at, T val) { Array[at] = val; }

      /// <summary>
      /// Retrieve the value at index <paramref name="at"/>.
      /// </summary>
      /// <param name="at">The index to retrieve a value at.</param>
      /// <returns>The requested value.</returns>
      public T Get(int at) { return Array[at]; }
    }

    /// <summary>
    /// An array adaptor to read <see cref="Colour"/> and store as <c>uint</c>.
    /// </summary>
    public class ColoursAdaptor : ComponentAdaptor<Colour>
    {
      /// <summary>
      /// Accesses the underlying array.
      /// </summary>
      /// <value>The array.</value>
      public UInt32[] Array { get; protected set; }

      /// <summary>
      /// Create a wrapper around <paramref name="array"/>.
      /// </summary>
      /// <param name="array">Array.</param>
      public ColoursAdaptor(UInt32[] array)
      {
        Array = array;
      }

      /// <summary>
      /// Retrieve the array length or resize the array (data lost).
      /// </summary>
      public int Count
      {
        get { return (Array != null) ? Array.Length : 0; }
        set { if (Count != value) { Array = new UInt32[value]; } }
      }

      /// <summary>
      /// Set the element at index <paramref name="at"/>.
      /// </summary>
      /// <param name="at">The index to set a value at.</param>
      /// <param name="val">The value to set.</param>
      public void Set(int at, Colour val) { Array[at] = val.Value; }

      /// <summary>
      /// Retrieve the value at index <paramref name="at"/>.
      /// </summary>
      /// <param name="at">The index to retrieve a value at.</param>
      /// <returns>The requested value.</returns>
      public Colour Get(int at) { return new Colour(Array[at]); }
    }
    #endregion

    /// <summary>
    /// Default constructor creating a transient, empty mesh shape.
    /// </summary>
    public MeshShape() : this(MeshDrawType.Points, new Vector3[0]) { }

    /// <summary>
    /// Create a mesh shape.
    /// </summary>
    /// <param name="drawType">Mesh topology.</param>
    /// <param name="vertices">Vertex data.</param>
    /// <param name="position">Local position.</param>
    /// <param name="rotation">Local rotation.</param>
    /// <param name="scale">Local scaling.</param>
    public MeshShape(MeshDrawType drawType, Vector3[] vertices, Vector3 position, Quaternion rotation, Vector3 scale)
      : this(drawType, vertices, null, position, rotation, scale) {}

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
      _vertices = vertices;
      _indices = indices ?? new int[0];
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
      _vertices = vertices;
      _indices = indices ?? new int[0];
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
      get { return _drawWeight; }
      set
      {
        _drawWeight = value;
      }
    }

    /// <summary>
    /// Access vertex array.
    /// </summary>
    public Vector3[] Vertices { get { return _vertices; } }

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
    public MeshShape SetUniformNormal(Vector3 normal)
    {
      Normals = new Vector3[] { normal };
      return this;
    }

    /// <summary>
    /// Optional 32-bit colours array. See <see cref="Colour"/>.
    /// </summary>
    /// <remarks>
    /// For points, this clears <see cref="ColourByHeight"/>.
    /// </remarks>
    public UInt32[] Colours
    {
      get { return _colours; }
      set { ColourByHeight = false; _colours = value; }
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
          for (int i = 0; i < _colours.Length; ++i)
          {
            yield return new Colour(_colours[i]);
          }
        }
      }
    }

    /// <summary>
    /// Access indices array.
    /// </summary>
    public int[] Indices { get { return _indices; } }

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
      packet.WriteBytes(BitConverter.GetBytes(_drawWeight), true);

      return true;
    }

    struct DataPhase
    {
      public SendDataType Type;
      public uint ItemCount;
      public int DataSizeBytes;
      public int TupleSize;
      public delegate void WriteElementDelegate(uint index);
      public WriteElementDelegate WriteElement;

      public DataPhase(SendDataType type, int itemCount, WriteElementDelegate write, int dataSizeBytes, int tupleSize = 1)
      {
        Type = type;
        ItemCount = (uint)itemCount;
        DataSizeBytes = dataSizeBytes;
        TupleSize = tupleSize;
        WriteElement = write;
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
                                Vector3[] vertices, Vector3[] normals, int[] indices, UInt32[] colours)
    {
      DataMessage msg = new DataMessage();
      // Local byte overhead needs to account for the size of sendType, offset and itemCount.
      // Use a larger value as I haven't got the edge cases quite right yet.
      const int localByteOverhead = 100;
      msg.ObjectID = objectID;
      packet.Reset(routingID, DataMessage.MessageID);
      msg.Write(packet);

      uint offset;
      uint itemCount;
      ushort sendType;

      int verticesLength = (vertices != null) ? vertices.Length : 0;
      int normalsLength = (normals != null) ? normals.Length : 0;
      int coloursLength = (colours != null) ? colours.Length : 0;
      int indicesLength = (indices != null) ? indices.Length : 0;
      DataPhase[] phases = new DataPhase[]
      {
        new DataPhase((normalsLength == 1) ? SendDataType.UniformNormal : SendDataType.Normals, normalsLength,
                      (uint index) => {
                        Vector3 n = normals[index];
                        packet.WriteBytes(BitConverter.GetBytes(n.X), true);
                        packet.WriteBytes(BitConverter.GetBytes(n.Y), true);
                        packet.WriteBytes(BitConverter.GetBytes(n.Z), true);
                      },
                      4, 3),
        new DataPhase(SendDataType.Colours, coloursLength,
                      (uint index) => { packet.WriteBytes(BitConverter.GetBytes(colours[index]), true); },
                      4),
        new DataPhase(SendDataType.Vertices, verticesLength,
                      (uint index) => {
                        Vector3 v = vertices[index];
                        packet.WriteBytes(BitConverter.GetBytes(v.X), true);
                        packet.WriteBytes(BitConverter.GetBytes(v.Y), true);
                        packet.WriteBytes(BitConverter.GetBytes(v.Z), true);
                      },
                      4, 3),
        new DataPhase(SendDataType.Indices, indicesLength,
                      (uint index) => { packet.WriteBytes(BitConverter.GetBytes(indices[index]), true); },
                      4),
      };

      int phaseIndex = 0;
      uint previousPhaseOffset = 0u;

      // While progressMarker is greater than or equal to the sum of the previous phase counts and the current phase count.
      // Also terminate of out of phases.
      while (phaseIndex < phases.Length &&
             progressMarker >= previousPhaseOffset + phases[phaseIndex].ItemCount)
      {
        previousPhaseOffset += phases[phaseIndex].ItemCount;
        ++phaseIndex;
      }

      bool done = false;
      // Check if we have anything to send.
      if (phaseIndex < phases.Length)
      {
        DataPhase phase = phases[phaseIndex];
        // Send part of current phase.
        // Estimate element count limit.
        int maxItemCount = MeshBase.EstimateTransferCount(phase.DataSizeBytes * phase.TupleSize, 0, DataMessage.Size + localByteOverhead);
        offset = progressMarker - previousPhaseOffset;
        itemCount = (uint)Math.Min(phase.ItemCount - offset, maxItemCount);

        sendType = (ushort)((int)phase.Type | (int)SendDataType.ExpectEnd);

        packet.WriteBytes(BitConverter.GetBytes(sendType), true);
        packet.WriteBytes(BitConverter.GetBytes(offset), true);
        packet.WriteBytes(BitConverter.GetBytes(itemCount), true);

        for (uint i = offset; i < offset + itemCount; ++i)
        {
          phase.WriteElement(i);
        }

        progressMarker += itemCount;
      }
      else
      {
        // Either all done or no data to send.
        // In the latter case, we need to populate the message anyway.
        offset = itemCount = 0;
        sendType = (int)SendDataType.ExpectEnd | (int)SendDataType.End;
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
      drawType = reader.ReadByte();
      DrawType = (MeshDrawType)drawType;

      if (_vertices == null || _vertices.Length != vertexCount)
      {
        _vertices = new Vector3[vertexCount];
      }

      if (_indices == null || _indices.Length != indexCount)
      {
        _indices = null;
        if (indexCount > 0)
        {
          _indices = new int[indexCount];
        }
      }

      _normals = null;

      if (packet.Header.VersionMajor != 0 || packet.Header.VersionMajor == 0 && packet.Header.VersionMinor >= 2)
      {
        _drawWeight = reader.ReadSingle();
      }
      else
      {
        // Legacy support
        _drawWeight = 0;
      }

      return true;
    }

    private delegate void ElementReaderDelegate(uint index);
    private static uint ReadElements(uint offset, uint itemCount, uint maxItems, ElementReaderDelegate read)
    {
      if (offset > maxItems)
      {
        return ~0u;
      }

      if (itemCount == 0)
      {
        return offset + itemCount;
      }

      if (offset + itemCount > maxItems)
      {
        itemCount = maxItems - itemCount;
      }

      for (uint i = offset; i < offset + itemCount; ++i)
      {
        read(i);
      }

      return offset + itemCount;
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

      ArrayComponentAdaptor<Vector3> normalsAdaptor = new ArrayComponentAdaptor<Vector3>(_normals);
      ColoursAdaptor coloursAdaptor = new ColoursAdaptor(_colours);
      int readComponent = ReadDataComponent(reader,
                                            new ArrayComponentAdaptor<Vector3>(_vertices),
                                            new ArrayComponentAdaptor<int>(_indices),
                                            normalsAdaptor, coloursAdaptor);

      if (readComponent == -1)
      {
        return false;
      }

      // Normals and colours may have been (re)allocated. Store the results.
      switch (readComponent & ~(int)(SendDataType.End | SendDataType.ExpectEnd))
      {
        case (int)SendDataType.Normals:
        case (int)SendDataType.UniformNormal:
          // Normals array may have been (re)allocated.
          _normals = normalsAdaptor.Array;
          break;

        case (int)SendDataType.Colours:
          // Colours array may have been (re)allocated.
          _colours = coloursAdaptor.Array;
          break;
      }

      // Check for finalisation.
      // Complete if ((readComponent & (int)SendDataType.End) != 0)

      return true;
    }


    /// <summary>
    /// A utility function for reading the payload of a <see cref="DataMessage"/> for a <c>MeshShape</c>.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="vertices"></param>
    /// <param name="indices"></param>
    /// <param name="normals"></param>
    /// <param name="colours"></param>
    /// <returns>Returns the updated component <see cref="SendDataType"/>. The <see cref="SendDataType.End"/>
    /// flag is also set, or alone when done. Returns -1 on failure.</returns>
    /// <remarks>
    /// This may be called immediately after reading the <see cref="DataMessage"/> for a
    /// <c>MeshShape</c> to decode the payload content. The method uses a set of
    /// <see cref="ComponentAdaptor{T}"/> interfaces to resolve data adaption to the required type or
    /// container. Each call will only interface with the adaptor relevant to the message payload
    /// calling <see cref="ComponentAdaptor{T}.Set(int, T)"/> for the incoming data. Vertices and
    /// indices must be correctly pre-sized, while other components may have the
    /// <see cref="ComponentAdaptor{T}.Count"/> property set to ensure the correct size (matching
    /// the vertex count, or 1 for uniform normals).
    /// </remarks>
    public static int ReadDataComponent(BinaryReader reader,
                                        ComponentAdaptor<Vector3> vertices,
                                        ComponentAdaptor<int> indices,
                                        ComponentAdaptor<Vector3> normals,
                                        ComponentAdaptor<Colour> colours)
    {
      UInt32 offset;
      UInt32 itemCount;
      UInt16 dataType;

      dataType = reader.ReadUInt16();
      offset = reader.ReadUInt32();
      itemCount = reader.ReadUInt32();

      // Record and mask out end flags.
      UInt16 endFlags = (ushort)(dataType & ((int)SendDataType.ExpectEnd | (int)SendDataType.End));
      dataType = (ushort)(dataType & ~endFlags);

      bool ok = true;
      bool complete = false;
      uint endReadCount = 0;
      switch ((SendDataType)dataType)
      {
        case SendDataType.Vertices:
          endReadCount = ReadElements(offset, itemCount, (uint)vertices.Count,
                                      (uint index) =>
                                      {
                                        Vector3 v = new Vector3();
                                        v.X = reader.ReadSingle();
                                        v.Y = reader.ReadSingle();
                                        v.Z = reader.ReadSingle();
                                        vertices.Set((int)index, v);
                                      });
          ok = ok && endReadCount != ~0u;

          // Expect end marker.
          if ((endFlags & (int)SendDataType.End) != 0)
          {
            // Done.
            complete = true;
          }

          // Check for completion.
          if ((endFlags & (int)SendDataType.ExpectEnd) == 0)
          {
            complete = endReadCount == vertices.Count;
          }
          break;

        case SendDataType.Indices:
          endReadCount = ReadElements(offset, itemCount, (uint)indices.Count,
                                      (uint index) => { indices.Set((int)index, reader.ReadInt32()); });
          ok = ok && endReadCount != ~0u;
          break;

        // Normals handled together.
        case SendDataType.Normals:
        case SendDataType.UniformNormal:
          if (normals == null)
          {
            return -1;
          }

          int normalsCount = ((SendDataType)dataType == SendDataType.Normals) ? vertices.Count : 1;
          if (normals.Count != normalsCount)
          {
            normals.Count = normalsCount;
          }

          endReadCount = ReadElements(offset, itemCount, (uint)normals.Count,
                                      (uint index) =>
                                      {
                                        Vector3 n = new Vector3();
                                        n.X = reader.ReadSingle();
                                        n.Y = reader.ReadSingle();
                                        n.Z = reader.ReadSingle();
                                        normals.Set((int)index, n);
                                      });
          ok = ok && endReadCount != ~0u;
          break;

        case SendDataType.Colours:
          if (colours == null)
          {
            return -1;
          }

          if (colours.Count != vertices.Count)
          {
            colours.Count = vertices.Count;
          }

          endReadCount = ReadElements(offset, itemCount, (uint)colours.Count,
                                      (uint index) => { colours.Set((int)index, new Colour(reader.ReadUInt32())); });
          ok = ok && endReadCount != ~0u;
          break;
        default:
          // Unknown data type.
          ok = false;
          break;
      }

      int returnValue = -1;

      if (ok)
      {
        returnValue = dataType;
        if (complete)
        {
          returnValue |= (int)SendDataType.End;
        }
      }

      return returnValue;
    }

    public delegate uint ComponentBlockReader(SendDataType dataType, BinaryReader reader, uint offset, uint count);

    /// <summary>
    /// A utility function for reading the payload of a <see cref="DataMessage"/> for a <c>MeshShape</c>.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="vertices"></param>
    /// <param name="indices"></param>
    /// <param name="normals"></param>
    /// <param name="colours"></param>
    /// <returns>Returns the updated component <see cref="SendDataType"/>. The <see cref="SendDataType.End"/>
    /// flag is also set, or alone when done. Returns -1 on failure.</returns>
    /// <remarks>
    /// This may be called immediately after reading the <see cref="DataMessage"/> for a
    /// <c>MeshShape</c> to decode the payload content. The method uses a set of
    /// <see cref="ComponentAdaptor{T}"/> interfaces to resolve data adaption to the required type or
    /// container. Each call will only interface with the adaptor relevant to the message payload
    /// calling <see cref="ComponentAdaptor{T}.Set(int, T)"/> for the incoming data. Vertices and
    /// indices must be correctly pre-sized, while other components may have the
    /// <see cref="ComponentAdaptor{T}.Count"/> property set to ensure the correct size (matching
    /// the vertex count, or 1 for uniform normals).
    /// </remarks>
    public static int ReadDataComponentDeferred(BinaryReader reader, uint vertexCount, uint indexCount,
                                        ComponentBlockReader vertexReader,
                                        ComponentBlockReader indexReader,
                                        ComponentBlockReader normalsReader,
                                        ComponentBlockReader coloursReader)
    {
      UInt32 offset;
      UInt32 itemCount;
      SendDataType dataType;

      dataType = (SendDataType)reader.ReadUInt16();
      offset = reader.ReadUInt32();
      itemCount = reader.ReadUInt32();

      // Record and mask out end flags.
      SendDataType endFlags = dataType & (SendDataType.ExpectEnd | SendDataType.End);
      dataType = dataType & ~endFlags;

      bool ok = true;
      bool complete = false;
      uint endReadCount = 0;
      switch (dataType)
      {
        case SendDataType.Vertices:
          endReadCount = vertexReader(dataType, reader, offset, itemCount);
          ok = ok && endReadCount != ~0u;

          // Expect end marker.
          if ((endFlags & SendDataType.End) != 0)
          {
            // Done.
            complete = true;
          }

          // Check for completion.
          if ((endFlags & SendDataType.ExpectEnd) == 0)
          {
            complete = endReadCount == vertexCount;
          }
          break;

        case SendDataType.Indices:
          endReadCount = indexReader(dataType, reader, offset, itemCount);
          ok = ok && endReadCount != ~0u;
          break;

        // Normals handled together.
        case SendDataType.Normals:
        case SendDataType.UniformNormal:
          if (normalsReader == null)
          {
            return -1;
          }

          endReadCount = normalsReader(dataType, reader, offset, itemCount);
          ok = ok && endReadCount != ~0u;
          break;

        case SendDataType.Colours:
          if (coloursReader == null)
          {
            return -1;
          }

          endReadCount = coloursReader(dataType, reader, offset, itemCount);
          ok = ok && endReadCount != ~0u;
          break;
        default:
          // Unknown data type.
          ok = false;
          break;
      }

      int returnValue = -1;

      if (ok)
      {
        returnValue = (int)dataType;
        if (complete)
        {
          returnValue |= (int)SendDataType.End;
        }
      }

      return returnValue;
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
    /// Per vertex colours.
    /// </summary>
    private UInt32[] _colours;
    /// <summary>
    /// Index data.
    /// </summary>
    private int[] _indices;
    /// <summary>
    /// Draw weight: equates to point size or line width.
    /// </summary>
    private float _drawWeight = 0.0f;
  }
}
