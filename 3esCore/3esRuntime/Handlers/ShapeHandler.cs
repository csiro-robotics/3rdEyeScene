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
      _transientCache = new ShapeCache(128, true);
      _shapeCache = new ShapeCache(128, false);
    }

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
    public Mesh SolidMesh { get; protected set; }
    /// <summary>
    /// Defines the mesh object to use for wire frame instances of this shape.
    /// </summary>
    /// <returns>The wire frame mesh for this shape.</returns>
    public Mesh WireframeMesh { get; protected set; }

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
    }

    /// <summary>
    /// Clear all current objects.
    /// </summary>
    public override void Reset()
    {
      _transientCache.Reset();
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
    /// Render all the current objects.
    /// </summary>
    public override void Render(ulong categoryMask, Matrix4x4 primaryCameraTransform)
    {
      // TODO: (KS) Handle categories beyond the 64 which fit into categoryMask.
      // TODO: (KS) Find a better way to split solid, transparent and wireframe rendering. Also need to respect the
      // TwoSided flag.
      // TODO: (KS) Restore colour/tint support from ObjectAttributes to rendering.
      int itemCount = 0;
      _solidTransforms.Clear();
      _transparentTransforms.Clear();
      _wireframeTransforms.Clear();
       _transientCache.CollectTransforms(_solidTransforms, _transparentTransforms, _wireframeTransforms, categoryMask);
      if (_solidTransforms.Count > 0)
      {
        Graphics.DrawMeshInstanced(SolidMesh, 0, Materials[MaterialLibrary.VertexColourUnlit],
                                   _solidTransforms.ToArray(), itemCount);
      }
      if (_transparentTransforms.Count > 0)
      {
        Graphics.DrawMeshInstanced(SolidMesh, 0, Materials[MaterialLibrary.VertexColourTransparent],
                                   _transparentTransforms.ToArray(), itemCount);
      }
      if (_wireframeTransforms.Count > 0)
      {
        Graphics.DrawMeshInstanced(WireframeMesh, 0, Materials[MaterialLibrary.WireframeTriangles],
                                   _wireframeTransforms.ToArray(), itemCount);
      }

      _solidTransforms.Clear();
      _transparentTransforms.Clear();
      _wireframeTransforms.Clear();
       _shapeCache.CollectTransforms(_solidTransforms, _transparentTransforms, _wireframeTransforms, categoryMask);
      if (_solidTransforms.Count > 0)
      {
        Graphics.DrawMeshInstanced(SolidMesh, 0, Materials[MaterialLibrary.VertexColourUnlit],
                                   _solidTransforms.ToArray(), itemCount);
      }
      if (_transparentTransforms.Count > 0)
      {
        Graphics.DrawMeshInstanced(SolidMesh, 0, Materials[MaterialLibrary.VertexColourTransparent],
                                   _transparentTransforms.ToArray(), itemCount);
      }
      if (_wireframeTransforms.Count > 0)
      {
        Graphics.DrawMeshInstanced(WireframeMesh, 0, Materials[MaterialLibrary.WireframeTriangles],
                                   _wireframeTransforms.ToArray(), itemCount);
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
      Error err = SerialiseObjects(writer, _transientCache, ref info.TransientCount);
      if (err.Failed)
      {
        return err;
      }

      err = SerialiseObjects(writer, _shapeCache, ref info.PersistentCount);
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
      // foreach (GameObject obj in _transientCache.Objects)
      // {
      //   ShapeComponent shape = obj.GetComponent<ShapeComponent>();
      //   if (shape != null && shape.Category == categoryId)
      //   {
      //     obj.SetActive(active);
      //   }
      // }

      // foreach (GameObject obj in _shapeCache.Objects)
      // {
      //   ShapeComponent shape = obj.GetComponent<ShapeComponent>();
      //   if (shape != null && shape.Category == categoryId)
      //   {
      //     obj.SetActive(active);
      //   }
      // }
    }

    /// <summary>
    /// Create a dummy shape object used to generate serialisation messages.
    /// </summary>
    /// <param name="cache">The cache in which the shape data reside</param>
    /// <param name="shapeIndex">The index of the shape in <paramref name="cache"/>.
    /// <param name="shape">Creation message for the shape.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    /// <remarks>
    /// Base classes should implement this method to return an instance of the appropriate
    /// <see cref="Shapes.Shape"/> derivation. For example, the <see cref="Shape3D.SphereHandler"/>
    /// should return a <see cref="Shapes.Sphere"/> object. See
    /// <see cref="SerialiseObjects(BinaryWriter, ShapeCache, ref uint)"/> for further
    /// details.
    /// </remarks>
    protected virtual Shapes.Shape CreateSerialisationShape(ShapeCache cache, int shapeIndex, CreateMessage shape)
    {
      return new Shapes.Shape(shape);
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
    /// <item>Call <see cref="CreateSerialisationShape(ShapeCache, int, CreateMessage)"/> to
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
    protected virtual Error SerialiseObjects(BinaryWriter writer, ShapeCache cache, ref uint processedCount)
    {
      // Serialise transient objects.
      PacketBuffer packet = new PacketBuffer();
      Error err;
      Shapes.Shape tempShape = null;
      uint dataMarker = 0;
      int dataResult = 0;

      Debug.Assert(tempShape != null && tempShape.RoutingID == RoutingID);

      processedCount = 0;
      foreach (int shapeIndex in cache.ShapeIndices)
      {
        ++processedCount;
        CreateMessage shapeData = cache.GetShapeByIndex(shapeIndex);
        tempShape = CreateSerialisationShape(cache, shapeIndex, shapeData);
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
            err = PostSerialiseCreateObject(packet, writer, cache, shapeIndex);
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
    /// <param name="cache"></param>
    /// <param name="shapeIndex"></param>
    /// <returns></returns>
    protected virtual Error PostSerialiseCreateObject(PacketBuffer packet, BinaryWriter writer, ShapeCache cache,
                                                      int shapeIndex)
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
        // Filter transient messages if required.
        if (objectID == 0 && (Mode & ModeFlags.IgnoreTransient) != 0)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Instantiates a persistent object from the <see cref="ShapeCache"/>.
    /// The new object is assigned the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The ID of the new object. Must non be zero (for persistent objects).</param>
    /// <returns>The instantiated object or -1 on failure.</returns>
    protected virtual int CreateShape(ShapeCache cache, CreateMessage shape)
    {
      Matrix4x4 transform = Matrix4x4.identity;
      DecodeTransform(shape.Attributes, out transform);
      return cache.CreateShape(shape, transform);
    }

    /// <summary>
    /// Decodes the transformation embedded in <paramref name="attributes"/>
    /// </summary>
    /// <param name="attributes">The message attributes to decode.</param>
    /// <param name="transform">The transform object to decode into.</param>
    /// <remarks>
    /// The default implementations makes the following assumptions about <paramref name="attributes"/>:
    /// <list type="bullet">
    /// <item>The X, Y, Z members define the position.</item>
    /// <item>The RotationX, RotationY, RotationZ, RotationW define a quaternion rotation.</item>
    /// <item>The ScaleX, ScaleY, ScaleZ define a scaling in each axis.</item>
    /// </list>
    ///
    /// This behaviour may be overridden to interpret the attributes differently. For example,
    /// the scale members for a cylinder may indicate length and radius, with a redundant Z component.
    /// </remarks>
    protected virtual void DecodeTransform(ObjectAttributes attributes, out Matrix4x4 transform)
    {
      transform = Matrix4x4.identity;

      Vector3 scale = new Vector3(attributes.ScaleX, attributes.ScaleY, attributes.ScaleZ);
      transform.SetColumn(3, new Vector4(attributes.X, attributes.Y, attributes.Z, 1.0f));
      var pureRotation = Matrix4x4.Rotate(new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW));
      transform.SetColumn(0, pureRotation.GetColumn(0) * scale.x);
      transform.SetColumn(1, pureRotation.GetColumn(1) * scale.y);
      transform.SetColumn(2, pureRotation.GetColumn(2) * scale.z);
    }

    #if OLD
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
    #endif // OLD

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
      int shapeIndex = -1;
      ShapeCache cache = (msg.ObjectID == 0) ? _transientCache : _shapeCache;
      _lastTransientIndex = -1;

      try
      {
        shapeIndex = CreateShape(cache, msg);
      }
      catch (Tes.Exception.DuplicateIDException )
      {
        return new Error(ErrorCode.DuplicateShape, msg.ObjectID);
      }

      if (msg.ObjectID == 0)
      {
        _lastTransientIndex = shapeIndex;
      }

      return PostHandleMessage(msg, packet, reader, cache, shapeIndex);
    }

    /// <summary>
    /// Called at end of <see cref="HandleMessage(CreateMessage, PacketBuffer, BinaryReader)"/>, only on success.
    /// </summary>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <param name="cache">Cache from which the shape was created.</param>
    /// <param name="shapeIndex">Index of the shape in <paramref name="cache"/>.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error PostHandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader,
                                              ShapeCache cache, int shapeIndex)
    {
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
      ushort flags = msg.Flags;

      if (msg.ObjectID == 0)
      {
        // Cannot update transient objects.
        return new Error(ErrorCode.InvalidObjectID, msg.ObjectID);
      }

      int shapeIndex = _shapeCache.GetShapeIndex(msg.ObjectID);
      CreateMessage shape = _shapeCache.GetShapeByIndex(shapeIndex);
      bool updateTransform = false;

      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0)
      {
        shape.Attributes = msg.Attributes;
        updateTransform = true;
      }
      else
        {
        if ((flags & (ushort)UpdateFlag.Position) != 0)
        {
          shape.Attributes.X = msg.Attributes.X;
          shape.Attributes.Y = msg.Attributes.Y;
          shape.Attributes.Z = msg.Attributes.Z;
          updateTransform = true;
        }

        if ((flags & (ushort)UpdateFlag.Rotation) != 0)
        {
          shape.Attributes.RotationX = msg.Attributes.RotationX;
          shape.Attributes.RotationY = msg.Attributes.RotationY;
          shape.Attributes.RotationZ = msg.Attributes.RotationZ;
          shape.Attributes.RotationW = msg.Attributes.RotationW;
          updateTransform = true;
        }

        if ((flags & (ushort)UpdateFlag.Scale) != 0)
        {
          shape.Attributes.ScaleX = msg.Attributes.ScaleX;
          shape.Attributes.ScaleY = msg.Attributes.ScaleY;
          shape.Attributes.ScaleZ = msg.Attributes.ScaleZ;
          updateTransform = true;
        }

        if ((flags & (ushort)UpdateFlag.Colour) != 0)
        {
          shape.Attributes.Colour = msg.Attributes.Colour;
        }
      }

      _shapeCache.SetShapeByIndex(shapeIndex, shape);

      if (updateTransform)
      {
        Matrix4x4 transform = Matrix4x4.identity;
        DecodeTransform(shape.Attributes, out transform);
        _shapeCache.SetShapeTransformByIndex(shapeIndex, transform);
      }

      return PostHandleMessage(msg, packet, reader, _shapeCache, shapeIndex);
    }

    /// <summary>
    /// Called at end of <see cref="HandleMessage(UpdateMessage, PacketBuffer, BinaryReader)"/>, only on success.
    /// </summary>
    /// <param name="shapeIndex">Index of the shape in the _shapeCache which is being updated.</param>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error PostHandleMessage(UpdateMessage msg, PacketBuffer packet, BinaryReader reader,
                                              ShapeCache cache, int shapeIndex)
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
      if (msg.ObjectID == 0)
      {
        return new Error();
      }

      // Do not check for existence. Not an error as we allow delete messages where the object has yet to be created.
      int shapeIndex = DestroyObject(msg.ObjectID);
      return PostHandleMessage(msg, packet, reader, _shapeCache, shapeIndex);
    }

    /// <summary>
    /// Called at end of <see cref="HandleMessage(DestroyMessage, PacketBuffer, BinaryReader)"/>
    /// just prior to destroying the object. The object will still be destroyed.
    /// </summary>
    /// <param name="msg">The incoming message.</param>
    /// <param name="packet">The buffer containing the message.</param>
    /// <param name="reader">The reader from which the message came.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error PostHandleMessage(DestroyMessage msg, PacketBuffer packet, BinaryReader reader,
                                              ShapeCache cache, int shapeIndex)
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
    /// Removes the persistent object matching <paramref name="id"/> from the shape cache
    /// without destroying it.
    /// </summary>
    /// <param name="id">ID of the object to find.</param>
    /// <returns>A matching object or null on failure.</returns>
    protected int DestroyObject(uint id)
    {
      return _shapeCache.DestroyShape(id);
    }

    /// <summary>
    /// Cache for transient objects.
    /// </summary>
    protected ShapeCache _transientCache = null;
    /// <summary>
    /// Cache for persistent objects.
    /// </summary>
    protected ShapeCache _shapeCache = null;
    protected List<Matrix4x4> _solidTransforms = new List<Matrix4x4>();
    protected List<Matrix4x4> _transparentTransforms = new List<Matrix4x4>();
    protected List<Matrix4x4> _wireframeTransforms = new List<Matrix4x4>();
    /// <summary>
    /// Index of the last item added to the _transientCache. Intended for handling DataMessage packets for transient
    /// shapes.
    /// </summary>
    protected int _lastTransientIndex = -1;
  }
}
