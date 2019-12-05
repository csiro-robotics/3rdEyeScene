using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers
{
  /// <summary>
  /// Defines the base functionality for <see cref="MessageHandler"/> objects
  /// which add 3D objects to the scene.
  /// </summary>
  /// <remarks>
  /// The base class handles incoming create, update and destroy messages
  /// with consideration given to transient and non-transient and solid or wire frame
  /// shapes.
  ///
  /// The <see cref="ShapeHandler"/> class uses a <see cref="ShapeCache"/> and
  /// <see cref="TransientShapeCache"/> to track persistent and transient objects
  /// respectively.
  ///
  /// Derivations must define the <see cref="SolidMesh"/> and <see cref="WireframeMesh"/>
  /// properties to yield the solid and wire frame mesh objects respectively. Derivations
  /// must also complete the <see cref="Shape3D.MeshSetHandler"/> definition by implementing the
  /// <see cref="Shape3D.MeshSetHandler.Name"/> and <see cref="Shape3D.MeshSetHandler.RoutingID"/>
  /// properties.
  ///
  /// Derivations may optionally override the default message handling, or parts thereof.
  /// Most commonly, the methods <see cref="DecodeTransform"/> and <see cref="EncodeAttributes"/>
  /// may be overridden.
  /// </remarks>
  public abstract class ShapeHandler : MessageHandler
  {
    /// <summary>
    /// Constructor initialising the persistent and transient caches.
    /// </summary>
    public ShapeHandler(CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _transientCache = new TransientShapeCache();
      _shapeCache = new ShapeCache();
      _root = new GameObject();
    }

    /// <summary>
    /// Defines the scene root for all objects of this shape type.
    /// </summary>
    /// <value>The root for objects of this shape.</value>
    public GameObject Root { get { return _root; } }

    /// <summary>
    /// A cache of the material library.
    /// </summary>
    /// <remarks>
    /// Set in <see cref="Initialise(GameObject, GameObject, MaterialLibrary)"/>.
    /// </remarks>
    public MaterialLibrary Materials { get; set; }

    /// <summary>
    /// Defines the mesh object to use for solid instances of this shape.
    /// </summary>
    /// <returns>The solid mesh for this shape.</returns>
    public abstract Mesh SolidMesh { get; }
    /// <summary>
    /// Defines the mesh object to use for wire frame instances of this shape.
    /// </summary>
    /// <returns>The wire frame mesh for this shape.</returns>
    public abstract Mesh WireframeMesh { get; }

    /// <summary>
    /// Initialise the shape handler by initialising the shape scene root and
    /// fetching the default materials.
    /// </summary>
    /// <param name="root">The 3rd Eye Scene root object.</param>
    /// <param name="serverRoot">The server scene root (transformed into the server reference frame).</param>
    /// <param name="materials">Material library from which to resolve materials.</param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      Materials = materials;
      _root.transform.SetParent(serverRoot.transform, false);
    }

    /// <summary>
    /// Clear all current objects.
    /// </summary>
    public override void Reset()
    {
      _transientCache.Reset(true);
      _shapeCache.Reset();
    }

    /// <summary>
    /// Start the frame by flushing transient objects.
    /// </summary>
    /// <param name="frameNumber">A monotonic frame number.</param>
    /// <param name="maintainTransient">True to disable transient flush.</param>
    public override void BeginFrame(uint frameNumber, bool maintainTransient)
    {
      if (!maintainTransient)
      {
        _transientCache.Reset();
      }
    }

    /// <summary>
    /// The primary message handling function.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <remarks>
    /// Invokes <see cref="HandleMessage(ushort, PacketBuffer, BinaryReader)"/>
    /// </remarks>
    public override Error ReadMessage(PacketBuffer packet, BinaryReader reader)
    {
      return HandleMessage(packet.Header.MessageID, packet, reader);
    }

    /// <summary>
    /// Serialises the currently active objects in for playback from file.
    /// </summary>
    /// <param name="writer">The write to serialise to.</param>
    /// <param name="info">Statistics</param>
    /// <returns>An error code on failure.</returns>
    public override Error Serialise(BinaryWriter writer, ref SerialiseInfo info)
    {
      info.TransientCount = info.PersistentCount = 0u;
      Error err = SerialiseObjects(writer, _transientCache.Objects.GetEnumerator(), ref info.TransientCount);
      if (err.Failed)
      {
        return err;
      }

      err = SerialiseObjects(writer, _shapeCache.Objects.GetEnumerator(), ref info.PersistentCount);
      return err;
    }

    /// <summary>
    /// Invoked when an object category changes active state.
    /// </summary>
    /// <param name="categoryId">The category changing state.</param>
    /// <param name="active">The new active state.</param>
    /// <remarks>
    /// Handlers should only ever visualise objects in active categories.
    /// </remarks>
    public override void OnCategoryChange(ushort categoryId, bool active)
    {
      foreach (GameObject obj in _transientCache.Objects)
      {
        ShapeComponent shape = obj.GetComponent<ShapeComponent>();
        if (shape != null && shape.Category == categoryId)
        {
          obj.SetActive(active);
        }
      }

      foreach (GameObject obj in _shapeCache.Objects)
      {
        ShapeComponent shape = obj.GetComponent<ShapeComponent>();
        if (shape != null && shape.Category == categoryId)
        {
          obj.SetActive(active);
        }
      }
    }

    /// <summary>
    /// Create a dummy shape object used to generate serialisation messages.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    /// <remarks>
    /// Base classes should implement this method to return an instance of the appropriate
    /// <see cref="Shapes.Shape"/> derivation. For example, the <see cref="Shape3D.SphereHandler"/>
    /// should return a <see cref="Shapes.Sphere"/> object. See
    /// <see cref="SerialiseObjects(BinaryWriter, IEnumerator&lt;GameObject&gt;, ref uint)"/> for further
    /// details.
    /// </remarks>
    protected abstract Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent);

    /// <summary>
    /// A helper functio to configure the TES <paramref name="shape"/> to match the Unity object <paramref name="shapeComponent"/>.
    /// </summary>
    /// <param name="shape">The shape object to configure to match the Unity representation.</param>
    /// <param name="shapeComponent">The Unity shape representation.</param>
    protected void ConfigureShape(Shapes.Shape shape, ShapeComponent shapeComponent)
    {
      ObjectAttributes attr = new ObjectAttributes();
      shape.ID = shapeComponent.ObjectID;
      shape.Category = shapeComponent.Category;
      shape.Flags = shapeComponent.ObjectFlags;
      EncodeAttributes(ref attr, shapeComponent.gameObject, shapeComponent);
      shape.SetAttributes(attr);
    }

    /// <summary>
    /// Serialises a list of objects to <paramref name="writer"/>
    /// </summary>
    /// <param name="writer">The writer to serialise to.</param>
    /// <param name="objects">The object to write.</param>
    /// <param name="processedCount">Number of objects processed.</param>
    /// <returns>An error code on failure.</returns>
    /// <remarks>
    /// Default serialisation uses the following logic on each object:
    /// <list type="bullet">
    /// <item>Call <see cref="CreateSerialisationShape(ShapeComponent)"/> to
    ///       create a temporary shape to match the unity object</item>
    /// <item>Call <see cref="Shapes.Shape.WriteCreate(PacketBuffer)"/> to generate the creation message
    ///       and serialise the packet.</item>
    /// <item>For complex shapes, call <see cref="Shapes.Shape.WriteData(PacketBuffer, ref uint)"/> as required
    ///       and serialise the packets.</item>
    /// </list>
    ///
    /// Using the <see cref="Shapes.Shape"/> classes ensures serialisation is consistent with the server code
    /// and reduces the code maintenance to one code path.
    /// </remarks>
    protected virtual Error SerialiseObjects(BinaryWriter writer, IEnumerator<GameObject> objects, ref uint processedCount)
    {
      // Serialise transient objects.
      PacketBuffer packet = new PacketBuffer();
      Error err;
      Shapes.Shape tempShape = null;
      uint dataMarker = 0;
      int dataResult = 0;

      Debug.Assert(tempShape != null && tempShape.RoutingID == RoutingID);

      processedCount = 0;
      while (objects.MoveNext())
      {
        ++processedCount;
        GameObject obj = objects.Current;
        tempShape = CreateSerialisationShape(obj.GetComponent<ShapeComponent>());
        if (tempShape != null)
        {
          tempShape.WriteCreate(packet);
          packet.FinalisePacket();
          packet.ExportTo(writer);

          if (tempShape.IsComplex)
          {
            dataResult = 1;
            dataMarker = 0;
            while (dataResult > 0)
            {
              dataResult = tempShape.WriteData(packet, ref dataMarker);
              packet.FinalisePacket();
              packet.ExportTo(writer);
            }

            if (dataResult < 0)
            {
              return new Error(ErrorCode.SerialisationFailure);
            }

            // Post serialisation extensions.
            err = PostSerialiseCreateObject(packet, writer, obj.GetComponent<ShapeComponent>());
            if (err.Failed)
            {
              return err;
            }
          }
        }
        else
        {
          return new Error(ErrorCode.SerialisationFailure);
        }
      }

      return new Error();
    }

    /// <summary>
    /// Called after serialising the creation message for an object. This allows follow up
    /// data messages to be serialised.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="writer"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    protected virtual Error PostSerialiseCreateObject(PacketBuffer packet, BinaryWriter writer, ShapeComponent shape)
    {
      return new Error();
    }

    /// <summary>
    /// Message routing function.
    /// </summary>
    /// <param name="messageID">The ID of the message.</param>
    /// <param name="packet">Packet buffer used to decode the message.</param>
    /// <param name="reader">The reader from which additional message data may be read.</param>
    /// <returns>An error code on failure.</returns>
    /// <remarks>
    /// The default implementation handles the following messages:
    /// Routes the following messages:
    /// <list type="bullet">
    /// <item><see cref="ObjectMessageID.Create"/></item>
    /// <item><see cref="ObjectMessageID.Update"/></item>
    /// <item><see cref="ObjectMessageID.Destroy"/></item>
    /// <item><see cref="ObjectMessageID.Data"/></item>
    /// </list>
    /// </remarks>
    protected virtual Error HandleMessage(ushort messageID, PacketBuffer packet, BinaryReader reader)
    {
      switch ((ObjectMessageID)messageID)
      {
      default:
      case ObjectMessageID.Null:
        return new Error(ErrorCode.InvalidMessageID, messageID);

      case ObjectMessageID.Create:
        // Read the create message details.
        CreateMessage create = new CreateMessage();
        if (!create.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, messageID);
        }
        if (!FilterMessage(messageID, create.ObjectID, create.Category))
        {
          return new Error();
        }
        return HandleMessage(create, packet, reader);

      case ObjectMessageID.Update:
        // Read the create message details.
        UpdateMessage update = new UpdateMessage();
        if (!update.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, messageID);
        }
        if (!FilterMessage(messageID, update.ObjectID, 0))
        {
          return new Error();
        }
        return HandleMessage(update, packet, reader);

      case ObjectMessageID.Destroy:
        // Read the create message details.
        DestroyMessage destroy = new DestroyMessage();
        if (!destroy.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, messageID);
        }
        if (!FilterMessage(messageID, destroy.ObjectID, 0))
        {
          return new Error();
        }
        return HandleMessage(destroy, packet, reader);

      case ObjectMessageID.Data:
        // Read the create message details.
        DataMessage data = new DataMessage();
        if (!data.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, messageID);
        }
        if (!FilterMessage(messageID, data.ObjectID, 0))
        {
          return new Error();
        }
        return HandleMessage(data, packet, reader);
      }

      //return new Error();
    }

    /// <summary>
    /// Used to allow filtering of incoming messages.
    /// </summary>
    /// <param name="messageID">ID of the incoming message.</param>
    /// <param name="objectID">ID of the object to which the message relates.</param>
    /// <param name="category">Category of the object, or zero if this information is unavailable.</param>
    /// <returns>True to allow the message to be processed.</returns>
    /// <remarks>
    /// The default implementation respects the <see cref="MessageHandler.ModeFlags"/> values
    /// filtering out transient object messages when <see cref="MessageHandler.ModeFlags.IgnoreTransient"/>
    /// is set. Destroy messages are not ignored.
    /// </remarks>
    protected virtual bool FilterMessage(ushort messageID, uint objectID, ushort category)
    {
      // Don't ignore destroy messages. Ever.
      if (messageID != (ushort)ObjectMessageID.Destroy)
      {
        if (objectID == 0 && (Mode & ModeFlags.IgnoreTransient) != 0)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Instantiates a transient object from the <see cref="TransientShapeCache"/>
    /// </summary>
    /// <returns>The instantiated object or null on failure.</returns>
    protected virtual GameObject CreateTransient()
    {
      GameObject obj = _transientCache.Fetch();
      if (!obj)
      {
        obj = CreateObject();
        obj.name = string.Format("{0}{1:D3}", Name, _transientCache.Count);
        _transientCache.Add(obj);
      }
      else
      {
        obj.SetActive(true);
      }
      return obj;
    }

    /// <summary>
    /// Instantiates a persistent object from the <see cref="ShapeCache"/>.
    /// The new object is assigned the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The ID of the new object. Must non be zero (for persistent objects).</param>
    /// <returns>The instantiated object or null on failure.</returns>
    protected virtual GameObject CreateObject(uint id)
    {
      GameObject obj = CreateObject();
      obj.name = string.Format("{0}{1:D3}", Name, id);
      ShapeComponent shape = obj.GetComponent<ShapeComponent>();
      shape.ObjectID = id;
      if (_shapeCache.Add(id, obj))
      {
        return obj;
      }
      // Creation failed.
      GameObject.Destroy(obj);
      return null;
    }

    /// <summary>
    /// Instantiate a persistent or transient object for this shape.
    /// </summary>
    /// <returns>An object suitable for use by this shape handler.</returns>
    /// <remarks>
    /// Objects created by this method must implement the <see cref="ShapeComponent"/>
    /// component in order to be correctly tracked by the shape cache.
    /// The default implementation ensures the following components are present:
    /// <list type="bullet">
    /// <item><see cref="MeshFilter"/></item>
    /// <item><see cref="MeshRenderer"/></item>
    /// <item><see cref="ShapeComponent"/></item>
    /// </list>
    ///
    /// This method is used to create both and persistent objects. The object is
    /// then added to the appropriate cache.
    /// </remarks>
    protected virtual GameObject CreateObject()
    {
      GameObject obj = new GameObject();
      /*MeshFilter meshFilter = */obj.AddComponent<MeshFilter>();
      obj.AddComponent<MeshRenderer>();
      obj.AddComponent<ShapeComponent>();
      return obj;
    }

    /// <summary>
    /// Decodes the transformation embedded in <paramref name="attributes"/>
    /// </summary>
    /// <param name="attributes">The message attributes to decode.</param>
    /// <param name="transform">The transform object to decode into.</param>
    /// <param name="flags">The flags associated with the object message. The method considers those related to updating
    /// specific parts of the object transformation.</param>
    /// <remarks>
    /// The default implementations makes the following assumptions about <paramref name="attributes"/>:
    /// <list type="bullet">
    /// <item>The X, Y, Z members define the position.</item>
    /// <item>The RotationX, RotationY, RotationZ, RotationW define a quaternion rotation.</item>
    /// <item>The ScaleX, ScaleY, ScaleZ define a scaling in each axis.</item>
    /// </list>
    ///
    /// This behaviour must be overridden to interpret the attributes differently. For example,
    /// the scale members for a cylinder may indicate length and radius, with a redundant Z component.
    ///
    /// The <paramref name="flags"/> parameter is used to consider the following <see cref="ObjectFlag"/> members:
    /// <list type="bullet">
    /// <item><see cref="UpdateFlag.UpdateMode"/> to indentify that only some transform elements are present.</item>
    /// <item><see cref="UpdateFlag.Position"/></item>
    /// <item><see cref="UpdateFlag.Rotation"/></item>
    /// <item><see cref="UpdateFlag.Scale"/></item>
    /// </list>
    /// </remarks>
    protected virtual void DecodeTransform(ObjectAttributes attributes, Transform transform, ushort flags = (ushort)ObjectFlag.None)
    {
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Position) != 0)
      {
        transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      }
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Rotation) != 0)
      {
        transform.localRotation = new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW);
      }
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Scale) != 0)
      {
        transform.localScale = new Vector3(attributes.ScaleX, attributes.ScaleY, attributes.ScaleZ);
      }
    }

    /// <summary>
    /// Called to extract the current object attributes from an existing object.
    /// </summary>
    /// <param name="attr">Modified to reflect the current state of <paramref name="obj"/></param>
    /// <param name="obj">The object to encode attributes for.</param>
    /// <param name="comp">The <see cref="ShapeComponent"/> of <paramref name="obj"/></param>
    /// <remarks>
    /// This extracts colour and performs the inverse operation of <see cref="DecodeTransform"/>
    /// This method must be overridden whenever <see cref="DecodeTransform"/> is overridden.
    /// </remarks>
    protected virtual void EncodeAttributes(ref ObjectAttributes attr, GameObject obj, ShapeComponent comp)
    {
      Transform transform = obj.transform;
      attr.X = transform.localPosition.x;
      attr.Y = transform.localPosition.y;
      attr.Z = transform.localPosition.z;
      attr.RotationX = transform.localRotation.x;
      attr.RotationY = transform.localRotation.y;
      attr.RotationZ = transform.localRotation.z;
      attr.RotationW = transform.localRotation.w;
      attr.ScaleX = transform.localScale.x;
      attr.ScaleY = transform.localScale.y;
      attr.ScaleZ = transform.localScale.z;
      if (comp != null)
      {
        attr.Colour = ShapeComponent.ConvertColour(comp.Colour);
      }
      else
      {
        attr.Colour = 0xffffffu;
      }
    }

    /// <summary>
    /// Lookup a material for the given <paramref name="shape"/> created by this handler.
    /// </summary>
    /// <param name="shape">The shape to lookup a material for.</param>
    /// <returns>The material for that shape.</returns>
    /// <remarks>
    /// Respects various <see cref="ObjectFlag"/> values.
    /// </remarks>
    public virtual Material LookupMaterialFor(ShapeComponent shape)
    {
      if (shape.Wireframe)
      {
        // Note: Wireframe triangles is not for rendering line shapes, but outlining triangles.
        // Therefore, not what we want here.
        return Materials[MaterialLibrary.VertexColourUnlit];
      }

      if (shape.Transparent)
      {
        return Materials[MaterialLibrary.VertexColourTransparent];
      }

      if (shape.TwoSided)
      {
        return Materials[MaterialLibrary.VertexColourUnlitTwoSided];
      }

      return Materials[MaterialLibrary.VertexColourUnlit];
    }

    /// <summary>
    /// Initialise the visual components (e.g., meshes) for <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">The object to initialise visuals for.</param>
    /// <param name="colour">Primary rendering colour.</param>
    /// <remarks>
    /// The default implementation resolves a single mesh using <see cref="SolidMesh"/>
    /// or <see cref="WireframeMesh"/> (depending on <see cref="ShapeComponent.Wireframe"/>)
    /// and invokes <see cref="InitialiseMesh(ShapeComponent, Mesh, Color)"/>.
    /// </remarks>
    protected virtual void InitialiseVisual(ShapeComponent obj, Color colour)
    {
      Mesh sharedMesh = (!obj.Wireframe) ? SolidMesh : WireframeMesh;
      InitialiseMesh(obj, sharedMesh, colour);
    }

    /// <summary>
    /// A convenient overload accepting a single mesh argument.
    /// </summary>
    /// <param name="obj">The object to initialise visuals for.</param>
    /// <param name="sharedMesh">The mesh to render with.</param>
    /// <param name="colour">Primary rendering colour.</param>
    /// <remarks>
    /// Maps to the overload: <see cref="InitialiseMesh(ShapeComponent, Mesh[], Color)"/>.
    /// </remarks>
    protected virtual void InitialiseMesh(ShapeComponent obj, Mesh sharedMesh, Color colour)
    {
      InitialiseMesh(obj, new Mesh[] { sharedMesh }, colour);
    }

    /// <summary>
    /// Sets the render meshes for <paramref name="obj"/> using <paramref name="sharedMeshes"/>.
    /// </summary>
    /// <param name="obj">The object to initialise visuals for.</param>
    /// <param name="sharedMeshes">The meshes to render with.</param>
    /// <param name="colour">Primary rendering colour.</param>
    /// <remarks>
    /// This function sets <paramref name="sharedMeshes"/> as a set of shared mesh resources
    /// to render <paramref name="obj"/> with. When there is one element in
    /// <paramref name="sharedMeshes"/>, the mesh and material are set on the
    /// <c>MeshFilter</c> and <c>MeshRenderer</c> belonging to <paramref name="obj"/> itself.
    /// When multiple items are given, the children of <paramref name="obj"/> are used instead.
    /// This means the number of children must match the number of elements in
    /// <paramref name="sharedMeshes"/>.
    ///
    /// The materials used are attained via <see cref="LookupMaterialFor(ShapeComponent)"/>
    /// where the <see cref="ShapeComponent"/> belongs to <paramref name="obj"/>.
    /// </remarks>
    protected virtual void InitialiseMesh(ShapeComponent obj, Mesh[] sharedMeshes, Color colour)
    {
      MeshFilter filter = obj.GetComponent<MeshFilter>();
      MeshRenderer render = obj.GetComponent<MeshRenderer>();
      ShapeComponent shape = obj.GetComponent<ShapeComponent>();

      if (sharedMeshes.Length == 1)
      {
        // Single mesh. Set on this object.
        if (filter != null)
        {
          filter.sharedMesh = sharedMeshes[0];
          if (render != null)
          {
            int componentCount = (filter.sharedMesh != null) ? filter.sharedMesh.subMeshCount : 0;
            SetMaterial(LookupMaterialFor(shape), render, componentCount, colour);
          }
        }
      }
      else
      {
        // Multiple meshes. Set on children.
        Transform child;
        for (int i = 0; i < sharedMeshes.Length; ++i)
        {
          child = obj.transform.GetChild(i);
          if ((filter = child.GetComponent<MeshFilter>()) != null)
          {
            filter.sharedMesh = sharedMeshes[i];
          }

          if ((render = child.GetComponent<MeshRenderer>()))
          {
            SetMaterial(LookupMaterialFor(shape), render, sharedMeshes[i].subMeshCount, colour);
          }
        }
      }
    }

    /// <summary>
    /// Set the material for <paramref name="render"/>.
    /// </summary>
    /// <param name="material">The material to instance.</param>
    /// <param name="render">The target object.</param>
    /// <param name="componentCount">The number of mesh components. Instance this many materials.</param>
    /// <param name="colour">The main render colour.</param>
    public static void SetMaterial(Material material, MeshRenderer render, int componentCount, Color colour)
    {
      if (componentCount <= 1)
      {
        render.material = material;
        render.material.color = colour;
      }
      else
      {
        Material[] mats = new Material[componentCount];
        for (int i = 0; i < componentCount; ++i)
        {
          mats[i] = material;
        }
        render.materials = mats;
        // Set colour after initialisation for correct material instancing. I think.
        for (int i = 0; i < componentCount; ++i)
        {
          render.materials[i].color = colour;
        }
      }
    }

    /// <summary>
    /// Sets the colour for <paramref name="obj"/> in using its material(s).
    /// </summary>
    /// <param name="obj">The object to set the colour of. Must belong to this handler.</param>
    /// <param name="colour">The target colour.</param>
    protected virtual void SetColour(GameObject obj, Color colour)
    {
      MeshRenderer render = obj.GetComponent<MeshRenderer>();
      if (render != null)
      {
        for (int i = 0; i < render.materials.Length; ++i)
        {
          render.materials[i].color = colour;
        }
      }
    }

    /// <summary>
    /// Message handler for <see cref="CreateMessage"/>
    /// </summary>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    /// <remarks>
    /// Creates either a persistent or transient object depending on the incoming
    /// message ObjectID. An ID of zero signifies a transient object.
    /// Solid or wire shapes are assigned according to the message flags.
    /// </remarks>
    protected virtual Error HandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      GameObject obj = null;
      if (msg.ObjectID == 0)
      {
        // Transient object.
        obj = CreateTransient();
      }
      else
      {
        obj = CreateObject(msg.ObjectID);
        if (!obj)
        {
          // Object already exists.
          return new Error(ErrorCode.DuplicateShape, msg.ObjectID);
        }
      }

      ShapeComponent shapeComp = obj.GetComponent<ShapeComponent>();
      shapeComp.Category = msg.Category;
      shapeComp.ObjectFlags = msg.Flags;
      shapeComp.Colour = ShapeComponent.ConvertColour(msg.Attributes.Colour);

      obj.transform.SetParent(_root.transform, false);
      DecodeTransform(msg.Attributes, obj.transform);

      InitialiseVisual(shapeComp, ShapeComponent.ConvertColour(msg.Attributes.Colour));

      return PostHandleMessage(obj, msg, packet, reader);
    }

    /// <summary>
    /// Called at end of <see cref="HandleMessage(CreateMessage, PacketBuffer, BinaryReader)"/>, only on success.
    /// </summary>
    /// <param name="obj">The newly created message.</param>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error PostHandleMessage(GameObject obj, CreateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      ShapeComponent shape = obj.GetComponent<ShapeComponent>();
      if (shape != null && !CategoryCheck(shape.Category))
      {
        obj.SetActive(false);
      }
      return new Error();
    }

    /// <summary>
    /// Message handler for <see cref="UpdateMessage"/>
    /// </summary>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    /// <remarks>
    /// Decodes the message transform and colour, updating the existing shape
    /// matching the message ObjectID. Only relevant for persistent shapes.
    /// </remarks>
    protected virtual Error HandleMessage(UpdateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      GameObject obj = FindObject(msg.ObjectID);
      if (obj == null)
      {
        return new Error(ErrorCode.InvalidObjectID, msg.ObjectID);
      }

      ushort flags = msg.Flags;
      DecodeTransform(msg.Attributes, obj.transform, flags);

      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Colour) != 0)
      {
        ShapeComponent shapeComp = obj.GetComponent<ShapeComponent>();
        if (shapeComp != null)
        {
          shapeComp.Colour = ShapeComponent.ConvertColour(msg.Attributes.Colour);
        }

        SetColour(obj, ShapeComponent.ConvertColour(msg.Attributes.Colour));
      }

      return new Error();
    }

    /// <summary>
    /// Called at end of <see cref="HandleMessage(UpdateMessage, PacketBuffer, BinaryReader)"/>, only on success.
    /// </summary>
    /// <param name="obj">The object being updated.</param>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error PostHandleMessage(GameObject obj, UpdateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      return new Error();
    }

    /// <summary>
    /// Message handler for <see cref="DestroyMessage"/>
    /// </summary>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    /// <remarks>
    /// Finds and destroys the object matching the message ObjectID.
    /// </remarks>
    protected virtual Error HandleMessage(DestroyMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      GameObject obj = RemoveObject(msg.ObjectID);
      if (obj == null)
      {
        // Does not exist. Not an error as we allow delete messages where the object has yet to be created.
        return new Error();
      }

      Error err = PostHandleMessage(obj, msg, packet, reader);
      UnityEngine.Object.Destroy(obj);
      return err;
    }

    /// <summary>
    /// Called at end of <see cref="HandleMessage(DestroyMessage, PacketBuffer, BinaryReader)"/>
    /// just prior to destroying the object. The object will still be destroyed.
    /// </summary>
    /// <param name="obj">The object being destroyed.</param>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error PostHandleMessage(GameObject obj, DestroyMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      return new Error();
    }

    /// <summary>
    /// Message handler for <see cref="DataMessage"/>
    /// </summary>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns><see cref="ErrorCode.UnsupportedFeature"/></returns>
    /// <remarks>
    /// Not implemented to properly handle the message, returning an appropriate
    /// error code.
    /// </remarks>
    protected virtual Error HandleMessage(DataMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      return new Error(ErrorCode.UnsupportedFeature);
    }

    /// <summary>
    /// Find the persistent object matching the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">ID of the object to find.</param>
    /// <returns>A matching object or null on failure.</returns>
    protected GameObject FindObject(uint id)
    {
      return _shapeCache.Fetch(id);
    }

    /// <summary>
    /// Removes the persistent object matching <paramref name="id"/> from the shape cache
    /// without destroying it.
    /// </summary>
    /// <param name="id">ID of the object to find.</param>
    /// <returns>A matching object or null on failure.</returns>
    protected GameObject RemoveObject(uint id)
    {
      return _shapeCache.Remove(id);
    }

    /// <summary>
    /// Scene root object for objects of this shape.
    /// </summary>
    private GameObject _root = null;
    /// <summary>
    /// Cache for transient objects.
    /// </summary>
    protected TransientShapeCache _transientCache = null;
    /// <summary>
    /// Cache for persistent objects.
    /// </summary>
    protected ShapeCache _shapeCache = null;
  }
}
