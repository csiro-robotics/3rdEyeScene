using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Logging;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handle messages for objects representing instances of mesh data from the <see cref="MeshCache"/>.
  /// </summary>
  /// <remarks>
  /// Supports dual creation order: meshes resources then objects or objects then mesh resources.
  ///
  /// Note: objects from the <see cref="MeshCache"/> can be marked for redefinition. In this case
  /// objects maintain the last valid visuals until a new finalisation message arrives.
  /// </remarks>
  public class MeshSetHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    /// <param name="meshCache">The mesh cache from which to read resources.</param>
    public MeshSetHandler(Runtime.CategoryCheckDelegate categoryCheck, MeshCache meshCache)
      : base(categoryCheck)
    {
      if (Root != null)
      {
        Root.name = Name;
      }
      MeshCache = meshCache;
    }

    /// <summary>
    /// Override.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="serverRoot"></param>
    /// <param name="materials"></param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      Root.transform.SetParent(serverRoot.transform, false);
    }

    /// <summary>
    /// Clear all current objects and mesh references.
    /// </summary>
    public override void Reset()
    {
      base.Reset();
      foreach (List<ShapeComponent> list in _registeredParts.Values)
      {
        list.Clear();
      }
      _awaitingFinalisation.Clear();
    }


    /// <summary>
    /// Ensures mesh objects are finalised.
    /// </summary>
    public override void PreRender()
    {
      base.PreRender();
      // Ensure meshes are finalised.
      ShapeComponent part;
      for (int i = 0; i < _awaitingFinalisation.Count; ++i)
      {
        part = _awaitingFinalisation[i];
        if (part.Dirty)
        {
          MeshCache.MeshDetails meshDetails = _meshCache.GetEntry(part.ObjectID);
          if (meshDetails != null && meshDetails.FinalMeshes != null)
          {
            // Build the mesh parts.
            SetMesh(part, meshDetails);
          }
          part.Dirty = false;
        }
      }

      _awaitingFinalisation.Clear();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "MeshSet"; } }

    /// <summary>
    /// <see cref="ShapeID.MeshSet"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.MeshSet; } }

    /// <summary>
    /// Irrelevant. Each object has its own geometry.
    /// </summary>
    public override Mesh SolidMesh { get { return null; } }
    /// <summary>
    /// Irrelevant. Each object has its own geometry.
    /// </summary>
    public override Mesh WireframeMesh { get { return null; } }

    /// <summary>
    /// Access the <see cref="MeshCache"/> from which mesh resources are resolved.
    /// </summary>
    public MeshCache MeshCache
    {
      get { return _meshCache; }
      set
      {
        if (_meshCache != null)
        {
          _meshCache.OnMeshFinalised -= this.OnMeshFinalised;
          _meshCache.OnMeshRemoved -= this.OnMeshRemoved;
        }
        _meshCache = value;
        if (_meshCache != null)
        {
          _meshCache.OnMeshFinalised += this.OnMeshFinalised;
          _meshCache.OnMeshRemoved += this.OnMeshRemoved;
        }
      }
    }

    /// <summary>
    /// Overridden to not add mesh components. These are handled by child objects.
    /// </summary>
    /// <returns></returns>
    protected override GameObject CreateObject()
    {
      GameObject obj = new GameObject();
      obj.AddComponent<ShapeComponent>();
      return obj;
    }

    /// <summary>
    /// Creates a mesh set shape for serialising the mesh and its resource references.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      // Start by building the resource list.
      int partCount = shapeComponent.transform.childCount;
      ObjectAttributes attrs = new ObjectAttributes();
      Shapes.MeshSet meshSet = new Shapes.MeshSet(shapeComponent.ObjectID, shapeComponent.Category);
      EncodeAttributes(ref attrs, shapeComponent.gameObject, shapeComponent);
      meshSet.SetAttributes(attrs);
      for (int i = 0; i < partCount; ++i)
      {
        // Write the mesh ID
        ShapeComponent partSrc = shapeComponent.transform.GetChild(i).GetComponent<ShapeComponent>();
        if (partSrc != null)
        {
          MeshResourcePlaceholder part = new MeshResourcePlaceholder(partSrc.ObjectID);
          // Encode attributes into target format.
          EncodeAttributes(ref attrs, partSrc.gameObject, partSrc);
          // And convert to matrix.
          meshSet.AddPart(part, attrs.GetTransform());
        }
        else
        {
          Log.Error("Failed to extract child {0} for mesh set {1}.", i, shapeComponent.name);
          return null;
        }
      }
      return meshSet;
    }

    /// <summary>
    /// Overridden to read information about mesh parts.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error HandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader)
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
      if (shapeComp)
      {
        shapeComp.Category = msg.Category;
        shapeComp.ObjectFlags = msg.Flags;
        shapeComp.Colour = ShapeComponent.ConvertColour(msg.Attributes.Colour);
      }

      obj.transform.SetParent(Root.transform, false);
      DecodeTransform(msg.Attributes, obj.transform);

      // Read mesh parts.
      ushort meshPartCount = reader.ReadUInt16();
      // Read part IDs
      for (ushort i = 0; i < meshPartCount; ++i)
      {
        Error err = AddMeshPart(obj, reader, i);
        if (err.Failed)
        {
          return err;
        }
      }

      foreach (ShapeComponent part in obj.GetComponentsInChildren<ShapeComponent>())
      {
        if (part.gameObject != obj)
        {
          List<ShapeComponent> parts = null;
          if (!_registeredParts.TryGetValue(part.ObjectID, out parts))
          {
            // Add new list.
            parts = new List<ShapeComponent>();
            _registeredParts.Add(part.ObjectID, parts);
          }

          parts.Add(part);

          // Try resolve the mesh part.
          if (_meshCache != null)
          {
            MeshCache.MeshDetails meshDetails = _meshCache.GetEntry(part.ObjectID);
            if (meshDetails != null && meshDetails.Finalised)
            {
              part.Dirty = true;
              _awaitingFinalisation.Add(part);
              //SetMesh(part, meshDetails);
            }
          }
        }
      }

      return new Error();
    }

    /// <summary>
    /// Called for each mesh part in the create messages.
    /// </summary>
    /// <param name="parent">The parent object for the part object.</param>
    /// <param name="reader">Message data reader.</param>
    /// <param name="partNumber">The part number/index.</param>
    /// <returns>An error code on failure.</returns>
    protected virtual Error AddMeshPart(GameObject parent, BinaryReader reader, int partNumber)
    {
      uint meshId = reader.ReadUInt32();
      ObjectAttributes attributes = new ObjectAttributes();
      if (!attributes.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage, (int)ObjectMessageID.Create);
      }

      // Add the part and a child for the part's mesh.
      // This supports the mesh having its own transform or pivot offset.
      GameObject part = new GameObject();
      ShapeComponent shape = part.AddComponent<ShapeComponent>();

      part.name = string.Format("part{0}", partNumber);

      shape.ObjectID = meshId;  // Use for mesh ID.
      shape.Category = 0;
      shape.ObjectFlags = 0;
      shape.Colour = ShapeComponent.ConvertColour(attributes.Colour);

      DecodeTransform(attributes, part.transform);
      part.transform.SetParent(parent.transform, false);

      return new Error();
    }

    /// <summary>
    /// Post destroy handling: destroy all sub-part objects.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(GameObject obj, DestroyMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      List<ShapeComponent> parts = null;
      foreach (ShapeComponent part in obj.GetComponentsInChildren<ShapeComponent>())
      {
        if (part.gameObject != obj)
        {
          if (_registeredParts.TryGetValue(part.ObjectID, out parts))
          {
            // Remove from parts.
            parts.Remove(part);
            _awaitingFinalisation.RemoveAll((ShapeComponent cmp) => { return cmp == part; });
            //parts.RemoveAll((ShapeComponent cmp) => { return cmp == part; }));
          }
        }
      }

      return new Error();
    }

    /// <summary>
    /// Mesh resource completion notification.
    /// </summary>
    /// <param name="meshDetails">The mesh(es) finalised.</param>
    /// <remarks>
    /// Links objects waiting on <paramref name="meshDetails"/> to use the associated meshes.
    /// </remarks>
    protected virtual void OnMeshFinalised(MeshCache.MeshDetails meshDetails)
    {
      // Find any parts waiting on this mesh.
      List<ShapeComponent> parts;
      if (!_registeredParts.TryGetValue(meshDetails.ID, out parts))
      {
        // Nothing waiting.
        return;
      }

      // Have parts to resolve.
      foreach (ShapeComponent part in parts)
      {
        part.Dirty = true;
        _awaitingFinalisation.Add(part);
      }
    }

    /// <summary>
    /// Mesh resource removal notification.
    /// </summary>
    /// <param name="meshDetails">The mesh(es) being removed.</param>
    /// <remarks>
    /// Stops referencing the associated mesh objects.
    /// </remarks>
    protected virtual void OnMeshRemoved(MeshCache.MeshDetails meshDetails)
    {
      // Find objects using the removed mesh.
      List<ShapeComponent> parts;
      if (!_registeredParts.TryGetValue(meshDetails.ID, out parts))
      {
        // Nothing using this mesh.
        return;
      }

      // Have things using this mesh. Clear them.
      foreach (ShapeComponent part in parts)
      {
        foreach (MeshFilter filter in part.GetComponentsInChildren<MeshFilter>())
        {
          filter.sharedMesh = null;
          filter.mesh = null;
        }
      }
    }

    /// <summary>
    /// Set the visuals of <pararef name="partObject"/> to use <paramref name="meshDetails"/>.
    /// </summary>
    /// <param name="partObject">The part object</param>
    /// <param name="meshDetails">The mesh details.</param>
    /// <remarks>
    /// Adds multiple children to <paramref name="partObject"/> when <paramref name="meshDetails"/>
    /// contains multiple mesh objects.
    /// </remarks>
    protected virtual void SetMesh(ShapeComponent partObject, MeshCache.MeshDetails meshDetails)
    {
      // Clear all children as a hard reset.
      foreach (Transform child in partObject.GetComponentsInChildren<Transform>())
      {
        if (child.gameObject != partObject.gameObject)
        {
          child.parent = null;
          GameObject.Destroy(child.gameObject);
        }
      }

      // Add children for each mesh sub-sub-part.
      int subPartNumber = 0;
      foreach (Mesh mesh in meshDetails.FinalMeshes)
      {
        GameObject partMesh = new GameObject();
        partMesh.name = string.Format("sub-part{0}", subPartNumber);
        partMesh.transform.localPosition = meshDetails.LocalPosition;
        partMesh.transform.localRotation = meshDetails.LocalRotation;
        partMesh.transform.localScale = meshDetails.LocalScale;

        MeshFilter filter = partMesh.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = partMesh.AddComponent<MeshRenderer>();
        renderer.material = meshDetails.Material;
        renderer.material.color = partObject.Colour;
        partMesh.transform.SetParent(partObject.transform, false);

        ++subPartNumber;
      }
    }

    private Dictionary<uint, List<ShapeComponent>> _registeredParts = new Dictionary<uint, List<ShapeComponent>>();
    private MeshCache _meshCache = null;
    private List<ShapeComponent> _awaitingFinalisation = new List<ShapeComponent>();
  }
}
