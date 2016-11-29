using System;
using System.Collections.Generic;
using Tes;
using Tes.IO;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  /// <summary>
  /// This is the base class for any spatial shape represented by 3rd Eye Scene.
  /// </summary>
  /// <remarks>
  /// The base shape exposes core shape properties, such as position scale and
  /// rotation, colour, type and message creation. Most simple shapes will not require
  /// any additional data, however, the semantics of certain properties may vary.
  /// For instance, a sphere requires only one scale element. A cylinder may use
  /// the scale fields to represent length and radius.
  /// 
  /// Shape message handling depends on <see cref="RoutingID"/>. This this ID must
  /// be unique for each type of <see cref="Shape"/>. For each <see cref="RoutingID"/>
  /// value, each shape may have its own unique <see cref="ID"/>. Any shape
  /// with a non-zero ID is considered persistent and must be explicitly removed from
  /// a connection later. Any shape with a zero ID is transient, lasting only a single
  /// frame. Such shapes will not have a destroy message sent.
  /// 
  /// The core shape attributes are serialised in creation and update messages via
  /// <see cref="WriteCreate(PacketBuffer)"/> and <see cref="WriteUpdate(PacketBuffer)"/>. A simple
  /// destruction message is authored by <see cref="WriteDestroy(PacketBuffer)"/>. Subclasses
  /// generally need only use the base implementation. Some subclasses may need to
  /// write additional data on creation by overriding the <see cref="WriteCreate(PacketBuffer)"/>.
  /// The base method should always be called first before appending custom data.
  /// 
  /// Some shapes may be marked as <see cref="IsComplex"/>. These shapes contain a
  /// large amount of data which cannot be adequately contained in a single (create)
  /// message. Such shapes may implement <see cref="WriteData(PacketBuffer, ref uint)"/> to serialise
  /// one or more additional packets of data. This method is only called when
  /// <see cref="IsComplex"/> is true.
  /// 
  /// Finally, a shape may have additional <see cref="Resource"/> requirements.
  /// A shape exposes its resources via the <see cref="Resources"/> property.
  /// Resources may be shared between shapes. They are reference counted in a client
  /// connection and sent and destroyed as needed. See <see cref="Resource"/> for more
  /// details.
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
    /// Update the attributes of this shape to match @p other.
    /// </summary>
    /// <param name="other">The shape to update data from.</param>
    /// <remarks>
    /// Used in maintaining cached copies of shapes. The shapes should
    /// already represent the same object.
    ///
    /// Not all attributes need to be updated. Only attributes which may be updated 
    /// via an @c UpdateMessage for this shape need be copied.
    ///
    /// The default implementation copies only the @c ObjectAttributes.
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
    ///   Initially zero, the @c Shape manages its own semantics.
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
