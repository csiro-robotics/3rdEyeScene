using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  /// <summary>
  /// This is the base class for any spatial shape represented by 3rd Eye Scene.
  /// </summary>
  ///
  /// <remarks>
  /// A shape instance is unique represented by its <see cref="RoutingID"/> and <see cref="ID"/>
  /// combined. The <see cref="RoutingID"/> can be considered a unique shape type
  /// identifier (see <see cref="ShapeID"/>), while the <see cref="ID"/> represents the
  /// shape instance.
  ///
  /// Shape instances may be considered transient or persistent. Transient
  /// shapes have an <see cref="ID"/> of zero and are automatically destroyed (by the
  /// client) on the next frame update. Persistent shapes have a non-zero
  /// <see cref="ID"/> and persist until an explicit <see cref="DestroyMessage"/> arrives.
  ///
  /// Internally, the <see cref="Shape"/> class is represented by a <see cref="CreateMessage"/>.
  /// This includes an ID, category, flags, position, rotation, scale and
  /// colour. These data represent the minimal data required to represent the
  /// shape and suffice for most primitive shapes. Derivations may store
  /// additional data members. Derivations may also adjust the semantics of
  /// some of the fields in the <see cref="CreateMessage"/>; e.g., the scale XYZ values
  /// have a particular interpretation for the <see cref="Capsule"/> shape.
  ///
  /// Shapes may be considered simple or complex (<see cref="IsComplex"/> reports
  /// <c>true</c>). Simple shapes only need a <see cref="WriteCreate(PacketBuffer)"/> call to be fully
  /// represented, after which <see cref="WriteUpdate(PacketBuffer)"/> may move the object. Complex
  /// shapes required additional data to be fully represented and the
  /// <see cref="WriteCreate(PacketBuffer)"/> packet stream may not be large enough to hold all the
  /// data. Such complex shapes will have <see cref="WriteData(PacketBuffer, ref uint)"/> called multiple times
  /// with a changing progress marker.
  ///
  /// Note that a shape which is not complex may override the <see cref="WriteCreate(PacketBuffer)"/>
  /// method and add additional data, but must always begin with the
  /// <see cref="CreateMessage"/>. Complex shapes are only required when this is not
  /// sufficient and the additional data may overflow the packet buffer.
  ///
  /// In general, use the <see cref="CreateMessage"/> only where possible. If additional
  /// information is required and the additional data is sufficient to fit
  /// easily in a single data packet (~64KiB), then write this information
  /// in <see cref="WriteCreate(PacketBuffer)"/> immediately following the <see cref="CreateMessage"/>. For
  /// larger data requirements, then the shape should report as complex
  /// (<see cref="IsComplex"/> returning <c>true</c>) and this information should be written
  /// in <see cref="WriteData(PacketBuffer, ref uint)"/>.
  ///
  /// The API also includes message reading functions, which creates a
  /// read/write symmetry. The read methods are intended primarily for testing
  /// purposes and to serve as an example of how to process messages. Whilst
  /// reading methods may be used to implement a visualisation client, they
  /// may lead to sub-optimal message handling and memory duplication. There may
  /// also be issues with synchronising the shape ID with the intended instance.
  ///
  /// No method for reading <see cref="DestroyMessage"/> is provided as it does not apply
  /// modifications to the shape members.
  /// </remarks>
  public class Shape : ICloneable
  {
    /// <summary>
    /// Initialises a new instance of the <see cref="Shape"/>.
    /// </summary>
    /// <param name="routingID">The routing ID for the shape type.</param>
    /// <param name="id">The shape ID. Zero for transient.</param>
    protected Shape(ushort routingID, uint id = 0) : this(routingID, id, (ushort)0) { }

    /// <summary>
    /// Initialises a new instance of the <see cref="Shape"/>.
    /// </summary>
    /// <param name="routingID">The routing ID for the shape type.</param>
    /// <param name="id">The shape ID. Zero for transient.</param>
    /// <param name="category">Optional category used to display the shape.</param>
    protected Shape(ushort routingID, uint id, ushort category)
    {
      RoutingID = routingID;
      ID = id;
      Category = category;
      Colour = 0xFFFFFFFFu;
      IsComplex = false;
      ScaleX = ScaleY = ScaleZ = _data.Attributes.RotationW = 1.0f;
    }

    /// <summary>
    /// Empty constructor for cloning.
    /// </summary>
    private Shape()
    {
    }

    /// <summary>
    /// Direct creation for non-complex shapes.
    /// </summary>
    /// <param name="routingID">The routing ID for the shape type.</param>
    /// <param name="data">Data packet representing the non-complex shape.</param>
    public Shape(ushort routingID, CreateMessage data)
    {
      RoutingID = routingID;
      _data = data;
    }

    /// <summary>
    /// Access to the routing ID for this shape.
    /// </summary>
    public ushort RoutingID { get; protected set; }
    /// <summary>
    /// Set to <c>true</c> for complex shapes. Only complex shapes will have
    /// <see cref="WriteData(PacketBuffer, ref uint)"/> called.
    /// </summary>
    public bool IsComplex { get; protected set; }
    /// <summary>
    /// Shape ID. Zero for transient.
    /// </summary>
    public uint ID { get { return _data.ObjectID; } set { _data.ObjectID = value; } }
    /// <summary>
    /// Shape category (optional).
    /// </summary>
    public ushort Category { get { return _data.Category; } set { _data.Category = value; } }

    /// <summary>
    /// Direct access to the <see cref="ObjectFlag"/> set for this shape.
    /// </summary>
    /// <value>The flags.</value>
    public ushort Flags
    {
      get { return _data.Flags; }
      set { _data.Flags = value; }
    }

    /// <summary>
    /// Gets or sets the wire frame display flag.
    /// </summary>
    public bool Wireframe
    {
      get { return (_data.Flags & (ushort)ObjectFlag.Wireframe) != 0; }
      set { _data.Flags &= (ushort)~ObjectFlag.Wireframe; _data.Flags |= value ? (ushort)ObjectFlag.Wireframe : (ushort)0; }
    }

    /// <summary>
    /// Gets or sets the transparent display flag.
    /// </summary>
    /// <value><c>true</c> if transparent; otherwise, <c>false</c>.</value>
    public bool Transparent
    {
      get { return (_data.Flags & (ushort)ObjectFlag.Transparent) != 0; }
      set { _data.Flags &= (ushort)~ObjectFlag.Transparent; _data.Flags |= value ? (ushort)ObjectFlag.Transparent : (ushort)0; }
    }

    /// <summary>
    /// Gets or sets the two sided display flag.
    /// </summary>
    /// <value><c>true</c> if two sided; otherwise, <c>false</c>.</value>
    public bool TwoSided
    {
      get { return (_data.Flags & (ushort)ObjectFlag.TwoSided) != 0; }
      set { _data.Flags &= (ushort)~ObjectFlag.TwoSided; _data.Flags |= value ? (ushort)ObjectFlag.TwoSided : (ushort)0; }
    }

    /// <summary>
    /// Gets or sets the replace on creation flag.
    /// </summary>
    /// <value><c>true</c> if replacing; otherwise, <c>false</c>.</value>
    public bool Replace
    {
      get { return (_data.Flags & (ushort)ObjectFlag.Replace) != 0; }
      set { _data.Flags &= (ushort)~ObjectFlag.Replace; _data.Flags |= value ? (ushort)ObjectFlag.Replace : (ushort)0; }
    }

    /// <summary>
    /// Gets or sets skip resources flag.
    /// </summary>
    /// <value><c>true</c> if skipping resource referencing; otherwise, <c>false</c>.</value>
    public bool SkipResources
    {
      get { return (_data.Flags & (ushort)ObjectFlag.SkipResources) != 0; }
      set
      {
        _data.Flags &=
          (ushort)~ObjectFlag.SkipResources; _data.Flags |= value ?(ushort)ObjectFlag.SkipResources : (ushort)0;
        }
    }

    public CreateMessage Data { get { return _data; } }

    /// <summary>
    /// Exposes shape details via an <see cref="ObjectAttributes"/> structure.
    /// </summary>
    /// <returns>The attributes.</returns>
    public ObjectAttributes GetAttributes()
    {
      return _data.Attributes;
    }

    /// <summary>
    /// Sets shape details via an <see cref="ObjectAttributes"/> structure.
    /// </summary>
    /// <param name="attr">The attributes to set.</param>
    public void SetAttributes(ObjectAttributes attr)
    {
      _data.Attributes = attr;
    }

    /// <summary>
    /// Position X coordinate.
    /// </summary>
    public float X { get { return _data.Attributes.X; } set { _data.Attributes.X = value; } }
    /// <summary>
    /// Position Y coordinate.
    /// </summary>
    public float Y { get { return _data.Attributes.Y; } set { _data.Attributes.Y = value; } }
    /// <summary>
    /// Position Z coordinate.
    /// </summary>
    public float Z { get { return _data.Attributes.Z; } set { _data.Attributes.Z = value; } }

    /// <summary>
    /// Collated position vector.
    /// </summary>
    public Vector3 Position
    {
      get { return new Vector3 { X = X, Y = Y, Z = Z }; }
      set { SetPosition(value.X, value.Y, value.Z); }
    }

    /// <summary>
    /// Scale X component.
    /// </summary>
    public float ScaleX
    {
      get { return _data.Attributes.ScaleX; }
      set { _data.Attributes.ScaleX = value; }
    }

    /// <summary>
    /// Scale Y component.
    /// </summary>
    public float ScaleY
    {
      get { return _data.Attributes.ScaleY; }
      set { _data.Attributes.ScaleY = value; }
    }

    /// <summary>
    /// Scale Z component.
    /// </summary>
    public float ScaleZ
    {
      get { return _data.Attributes.ScaleZ; }
      set { _data.Attributes.ScaleZ = value; }
    }

    /// <summary>
    /// Collated scale vector.
    /// </summary>
    public Vector3 Scale
    {
      get { return new Vector3 { X = _data.Attributes.ScaleX, Y = _data.Attributes.ScaleY, Z = _data.Attributes.ScaleZ }; }
      set { _data.Attributes.ScaleX = value.X; _data.Attributes.ScaleY = value.Y; _data.Attributes.ScaleZ = value.Z; }
    }

    /// <summary>
    /// Collated quaternion rotation value.
    /// </summary>
    /// <value>The rotation.</value>
    public Quaternion Rotation
    {
      get
      {
        return new Quaternion
        {
          X = _data.Attributes.RotationX, Y = _data.Attributes.RotationY,
          Z = _data.Attributes.RotationZ, W = _data.Attributes.RotationW,
        };
      }

      set
      {
        _data.Attributes.RotationX = value.X;
        _data.Attributes.RotationY = value.Y;
        _data.Attributes.RotationZ = value.Z;
        _data.Attributes.RotationW = value.W;
      }
    }

    /// <summary>
    /// Encoded integer colour value. See <see cref="Maths.Colour"/>.
    /// </summary>
    public uint Colour
    {
      get { return _data.Attributes.Colour; }
      set { _data.Attributes.Colour = value; }
    }

    /// <summary>
    /// Called to enumerate resources for this shape.
    /// </summary>
    public virtual IEnumerable<Resource> Resources
    {
      get { return new Resource[0]; }
    }

    /// <summary>
    /// Set the position.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    public void SetPosition(float x, float y, float z)
    {
      _data.Attributes.X = x;
      _data.Attributes.Y = y;
      _data.Attributes.Z = z;
    }

    /// <summary>
    /// Set the scale.
    /// </summary>
    /// <param name="x">The x scale.</param>
    /// <param name="y">The y scale.</param>
    /// <param name="z">The z scale.</param>
    public void SetScale(float x, float y, float z)
    {
      _data.Attributes.ScaleX = x;
      _data.Attributes.ScaleY = y;
      _data.Attributes.ScaleZ = z;
    }

    /// <summary>
    /// Update the attributes of this shape to match <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The shape to update data from.</param>
    /// <remarks>
    /// Used in maintaining cached copies of shapes. The shapes should
    /// already represent the same object.
    ///
    /// Not all attributes need to be updated. Only attributes which may be updated
    /// via an <see cref="UpdateMessage"/> for this shape need be copied.
    ///
    /// The default implementation copies only the <see cref="ObjectAttributes"/>.
    /// </remarks>
    public virtual void UpdateFrom(Shape other)
    {
      _data.Attributes = other._data.Attributes;
    }

    /// <summary>
    /// Write a create message to <paramref name="packet"/>.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    /// <param name="packet">Packet to write the message to.</param>
    public virtual bool WriteCreate(PacketBuffer packet)
    {
      packet.Reset(RoutingID, CreateMessage.MessageID);
      return _data.Write(packet);
    }

    /// <summary>
    /// Write a update message to <paramref name="packet"/>.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    /// <param name="packet">Packet to write the message to.</param>
    public virtual bool WriteUpdate(PacketBuffer packet)
    {
      UpdateMessage msg = new UpdateMessage();
      msg.ObjectID = ID;
      msg.Flags = _data.Flags;
      msg.Attributes = _data.Attributes;
      packet.Reset(RoutingID, UpdateMessage.MessageID);
      return msg.Write(packet);
    }

    /// <summary>
    /// Write a destroy message to <paramref name="packet"/>.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    /// <param name="packet">Packet to write the message to.</param>
    public virtual bool WriteDestroy(PacketBuffer packet)
    {
      DestroyMessage msg = new DestroyMessage();
      msg.ObjectID = ID;
      packet.Reset(RoutingID, DestroyMessage.MessageID);
      return msg.Write(packet);
    }

    /// <summary>
    /// Called only for complex shapes to write additional creation data.
    /// </summary>
    ///
    /// <param name="packet">The data stream to write to.</param>
    /// <param name="progressMarker">Indicates data transfer progress.
    ///   Initially zero, the <see cref="Shape"/> manages its own semantics.
    /// </param>
    /// <returns>
    /// Indicates completion progress. 0 indicates completion,
    /// 1 indicates more data are available and more calls should be made.
    /// -1 indicates an error. No more calls should be made.
    /// </returns>
    public virtual int WriteData(PacketBuffer packet, ref uint progressMarker)
    {
      return 0;
    }

    /// <summary>
    /// Read a <see cref="CreateMessage"/> for this shape. This will override the
    /// <see cref="ID"/> of this instance.
    /// </summary>
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">stream The stream to read message data from.</param>
    /// <returns><c>true</c> if the message is successfully read.</returns>
    /// <remarks>
    /// The <see cref="RoutingID"/> must have already been resolved.
    /// </remarks>
    public virtual bool ReadCreate(PacketBuffer packet, BinaryReader reader)
    {
      return _data.Read(reader);
    }

    /// <summary>
    /// Read an <see cref="UpdateMessage"/> for this shape.
    /// </summary>
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">The stream to read message data from.</param>
    /// <returns><c>true</c> if the message is successfully read.</returns>
    /// <remarks>
    /// Respects the <see cref="UpdateFlag"/> values, only modifying requested data.
    /// </remarks>
    public virtual bool ReadUpdate(PacketBuffer packet, BinaryReader reader)
    {
      UpdateMessage up = new UpdateMessage();
      if (up.Read(reader))
      {
        if ((up.Flags & (ushort)UpdateFlag.UpdateMode) == 0)
        {
          // Full update.
          _data.Attributes = up.Attributes;
        }
        else
        {
          // Partial update.
          if ((up.Flags & (short)UpdateFlag.Position) != 0)
          {
            _data.Attributes.X = up.Attributes.X;
            _data.Attributes.Y = up.Attributes.Y;
            _data.Attributes.Z = up.Attributes.Z;
          }
          if ((up.Flags & (short)UpdateFlag.Rotation) != 0)
          {
            _data.Attributes.RotationX = up.Attributes.RotationX;
            _data.Attributes.RotationY = up.Attributes.RotationY;
            _data.Attributes.RotationZ = up.Attributes.RotationZ;
            _data.Attributes.RotationW = up.Attributes.RotationW;
          }
          if ((up.Flags & (short)UpdateFlag.Scale) != 0)
          {
            _data.Attributes.ScaleX = up.Attributes.ScaleX;
            _data.Attributes.ScaleY = up.Attributes.ScaleY;
            _data.Attributes.ScaleZ = up.Attributes.ScaleZ;
          }
          if ((up.Flags & (short)UpdateFlag.Colour) != 0)
          {
            _data.Attributes.Colour = up.Attributes.Colour;
          }
        }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Read back data written by <see cref="WriteData(PacketBuffer, ref uint)"/>.
    /// </summary>
    ///
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">The stream to read message data from.</param>
    /// <returns><c>true</c> if the message is successfully read.</returns>
    ///
    /// <remarks>
    /// Must be implemented by complex shapes, first reading the <see cref="DataMessage"/>
    /// then data payload. The base implementation returns <c>false</c> assuming a
    /// simple shape.
    /// </remarks>
    public virtual bool ReadData(PacketBuffer packet, BinaryReader reader)
    {
      return false;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    public virtual object Clone()
    {
      Shape copy = new Shape();
      OnClone(copy);
      return copy;
    }

    /// <summary>
    /// Perform clone copy operations.
    /// </summary>
    /// <param name="copy">The new object to copy into.</param>
    protected void OnClone(Shape copy)
    {
      copy.RoutingID = RoutingID;
      copy._data = _data;
    }

    /// <summary>
    /// Core shape data.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="CreateMessage"/> because that conveniently holds all the
    /// core data required.
    /// </remarks>
    protected CreateMessage _data = new CreateMessage();
  }
}
