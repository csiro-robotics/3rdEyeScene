using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Shape message handler for point clouds.
  /// </summary>
  /// <remarks>
  /// Whilst very similar to meshes, point cloud shapes do not support referencing an
  /// external mesh. The point data are encoded in a series of
  /// <see cref="DataMessage">DataMessages</see>.
  /// 
  /// Note: objects from the <see cref="MeshCache"/> can be marked for redefinition. In this case
  /// objects maintain the last valid visuals until a new finalisation message arrives.
  /// </remarks>
  public class PointCloudHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    /// <param name="meshCache">The mesh cache from which to read resources.</param>
    public PointCloudHandler(Runtime.CategoryCheckDelegate categoryCheck, MeshCache meshCache)
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
      _litMaterial = materials[MaterialLibrary.PointsLit];
      _unlitMaterial = materials[MaterialLibrary.PointsUnlit];
    }

    /// <summary>
    /// Clear all current objects and mesh references.
    /// </summary>
    public override void Reset()
    {
      base.Reset();
      foreach (List<PointsComponent> list in _registeredParts.Values)
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
      PointsComponent points;
      for (int i = 0; i < _awaitingFinalisation.Count; ++i)
      {
        points = _awaitingFinalisation[i];
        if (points.MeshDirty)
        {
          MeshCache.MeshDetails meshDetails = _meshCache.GetEntry(points.MeshID);
          if (meshDetails != null && meshDetails.FinalMeshes != null)
          {
            // Build the mesh parts.
            SetMesh(points, meshDetails);
          }
          points.MeshDirty = false;
        }
      }

      _awaitingFinalisation.Clear();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "PointCloud"; } }

    /// <summary>
    /// <see cref="ShapeID.PointCloud"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.PointCloud; } }

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
    /// Overridden to not create a mesh filter or renderer for the points root.
    /// </summary>
    /// <returns></returns>
    protected override GameObject CreateObject()
    {
      GameObject obj = new GameObject();
      obj.AddComponent<ShapeComponent>();
      return obj;
    }

    /// <summary>
    /// Creates a point cloud shape for serialising <paramref name="shapeComponent"/> and its point data.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      PointsComponent pointsComp = shapeComponent.GetComponent<PointsComponent>();
      if (pointsComp != null)
      {
        ObjectAttributes attr = new ObjectAttributes();
        EncodeAttributes(ref attr, shapeComponent.gameObject, shapeComponent);

        Shapes.PointCloudShape points = new Shapes.PointCloudShape(new MeshResourcePlaceholder(pointsComp.MeshID),
                                                                   shapeComponent.ObjectID,
                                                                   shapeComponent.Category,
                                                                   (byte)pointsComp.PointSize);
        points.SetAttributes(attr);

        return points;
      }
      return null;
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
        // Cannot support transient objects: we need to get point data through additional
        // messages which requires a valid object id.
        return new Error(ErrorCode.InvalidObjectID, 0);
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

      // Read additional attributes.
      PointsComponent pointsComp = obj.AddComponent<PointsComponent>();
      // Read the mesh ID to render points from.
      pointsComp.MeshID = reader.ReadUInt32();
      // Read the number of indices (zero implies show entire mesh).
      pointsComp.IndexCount = reader.ReadUInt32();
      pointsComp.PointSize = reader.ReadByte();

      if (pointsComp.IndexCount == 0)
      {
        // Not expecting any index data messages.
        // Register the mesh now.
        RegisterForMesh(pointsComp);
      }

      return new Error();
    }


    /// <summary>
    /// Overridden to read optional point cloud indices.
    /// </summary>
    /// <param name="msg">Message header.</param>
    /// <param name="packet">Data packet.</param>
    /// <param name="reader">Data packet reader.</param>
    /// <returns></returns>
    protected override Error HandleMessage(DataMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      GameObject obj = null;

      if (msg.ObjectID != 0)
      {
        obj = FindObject(msg.ObjectID);
      }
      else
      {
        obj = _transientCache.LastObject;
      }

      if (obj == null)
      {
        return new Error(ErrorCode.InvalidObjectID, msg.ObjectID);
      }

      // Read index offset and count.
      uint offset = reader.ReadUInt32();
      uint count = reader.ReadUInt32();

      PointsComponent pointsComp = obj.GetComponent<PointsComponent>();
      int[] indices = pointsComp.Indices;
      for (uint i = 0; i < count; ++i)
      {
        indices[i + offset] = reader.ReadInt32();
      }

      if (pointsComp.IndexCount != 0 &&  offset + count >= pointsComp.IndexCount)
      {
        // Done. Register for the mesh.
        RegisterForMesh(pointsComp);
      }

      return new Error();
    }

    /// <summary>
    /// Post destroy message: destroy sub-objects.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(GameObject obj, DestroyMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      PointsComponent points = obj.GetComponent<PointsComponent>();
      if (points != null)
      {
        // Remove the registered parts.
        List<PointsComponent> parts = null;
        if (_registeredParts.TryGetValue(points.MeshID, out parts))
        {
          // Remove from parts.
          parts.Remove(points);
          _awaitingFinalisation.RemoveAll((PointsComponent cmp) => { return cmp == points; });
          //parts.RemoveAll((PointsComponent cmp) => { return cmp == part; }));
        }
      }
      return new Error();
    }


    /// <summary>
    /// Register <paramref name="points"/> for mesh resolving.
    /// </summary>
    /// <remarks>
    /// The mesh is either resolved now, or on mesh finalisation.
    /// Note that any limited indexing must be completed before calling
    /// this function. That is <see cref="PointsComponent.Indices"/>
    /// must have been resolved.
    /// </remarks>
    /// <param name="points">The points component.</param>
    protected void RegisterForMesh(PointsComponent points)
    {
      List<PointsComponent> parts = null;
      if (!_registeredParts.TryGetValue(points.MeshID, out parts))
      {
        // Add new list.
        parts = new List<PointsComponent>();
        _registeredParts.Add(points.MeshID, parts);
      }

      parts.Add(points);

      // Try resolve the mesh from the cache now.
      if (_meshCache != null)
      {
        MeshCache.MeshDetails meshDetails = _meshCache.GetEntry(points.MeshID);
        if (meshDetails != null && meshDetails.Finalised)
        {
          points.MeshDirty = true;
          _awaitingFinalisation.Add(points);
        }
      }
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
      List<PointsComponent> parts;
      if (!_registeredParts.TryGetValue(meshDetails.ID, out parts))
      {
        // Nothing waiting.
        return;
      }

      // Have parts to resolve.
      foreach (PointsComponent part in parts)
      {
        part.MeshDirty = true;
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
      List<PointsComponent> parts;
      if (!_registeredParts.TryGetValue(meshDetails.ID, out parts))
      {
        // Nothing using this mesh.
        return;
      }

      // Have things using this mesh. Clear them.
      MeshFilter filter = null;
      foreach (PointsComponent part in parts)
      {
        filter = part.GetComponent<MeshFilter>();
        if (filter != null)
        {
          filter.sharedMesh = null;
          filter.mesh = null;
        }
      }
    }

    /// <summary>
    /// Set the visuals of <pararef name="points"/> to use <paramref name="meshDetails"/>.
    /// </summary>
    /// <param name="points">The points object</param>
    /// <param name="meshDetails">The mesh details.</param>
    /// <remarks>
    /// Adds multiple children to <paramref name="points"/> when <paramref name="meshDetails"/>
    /// contains multiple mesh objects.
    /// </remarks>
    protected virtual void SetMesh(PointsComponent points, MeshCache.MeshDetails meshDetails)
    {
      ShapeComponent shape = points.GetComponent<ShapeComponent>();
      // Clear all children as a hard reset.
      foreach (Transform child in points.GetComponentsInChildren<Transform>())
      {
        if (child.gameObject != points.gameObject)
        {
          child.parent = null;
          GameObject.Destroy(child.gameObject);
        }
      }

      // Use shared resources if we have no limited indexing.
      if (points.IndexCount == 0)
      {
        // Add children for each mesh sub-sub-part.
        int partNumber = 0;
        foreach (Mesh mesh in meshDetails.FinalMeshes)
        {
          GameObject partMesh = new GameObject(string.Format("cloud{0}", partNumber));
          partMesh.transform.localPosition = meshDetails.LocalPosition;
          partMesh.transform.localRotation = meshDetails.LocalRotation;
          partMesh.transform.localScale = meshDetails.LocalScale;
          partMesh.AddComponent<MeshFilter>().sharedMesh = mesh;

          MeshRenderer renderer = partMesh.AddComponent<MeshRenderer>();
          if (meshDetails.Topology == MeshTopology.Points)
          {
            // Use mesh material as is.
            renderer.material = meshDetails.Material;
          }
          else
          {
            // Rendering a mesh with non-point topology. Set tha points based material.
            renderer.material = mesh.normals.Length > 0 ? _litMaterial : _unlitMaterial;
          }
          renderer.material.SetInt("PointSize", points.PointSize);
          renderer.material.color = (shape != null) ? shape.Colour : new Color32(255, 255, 255, 255);
          partMesh.transform.SetParent(points.transform, false);
          ++partNumber;
        }
      }
      else
      {
        // We are going to need to remap the indexing. For this we'll
        // copy the required vertices from the MeshDetails and create new
        // meshes with new indices. We could potentially share vertex data,
        // but this is difficult with the Unity vertex count limit.
        Mesh[] meshes = meshDetails.Builder.GetReindexedMeshes(points.Indices, MeshTopology.Points);
        for (int i = 0; i < meshes.Length; ++i)
        {
          // Create this mesh piece.
          bool defaultMaterial = meshDetails.Topology == MeshTopology.Points;
          Material material = (defaultMaterial) ? meshDetails.Material : _unlitMaterial;
          if (!defaultMaterial && meshDetails.Builder.Normals.Length != 0)
          {
            material = _litMaterial;
          }

          GameObject child = new GameObject();
          child.AddComponent<MeshFilter>().mesh = meshes[i];
          material.SetInt("PointSize", points.PointSize);
          child.AddComponent<MeshRenderer>().material = material;
          child.transform.SetParent(points.transform, false);
        }
      }
    }


    /// <summary>
    /// Mapping of Mesh ID to PointsComponents.
    /// </summary>
    private Dictionary<uint, List<PointsComponent>> _registeredParts = new Dictionary<uint, List<PointsComponent>>();
    private Material _litMaterial = null;
    private Material _unlitMaterial = null;
    private MeshCache _meshCache = null;
    private List<PointsComponent> _awaitingFinalisation = new List<PointsComponent>();
  }
}
