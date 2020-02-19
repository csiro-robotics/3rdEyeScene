using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Logging;
using Tes.Net;
using Tes.Runtime;
using Tes.Maths;
using UnityEngine;
using UnityEngine.Rendering;

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
    public class PartSet : IShapeData
    {
      public uint[] MeshIDs;
      public MeshCache.MeshDetails[] Meshes;
      public Matrix4x4[] Transforms;
      public Maths.Colour[] Tints;
      public Material[] MaterialOverrides;
      /// <summary>
      /// <see cref="ObjectFlag"/> values used to resolve the render material.
      /// </summary>
      public ObjectFlag ObjectFlags;
    }

    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    /// <param name="meshCache">The mesh cache from which to read resources.</param>
    public MeshSetHandler(Runtime.CategoryCheckDelegate categoryCheck, MeshCache meshCache)
      : base(categoryCheck)
    {
      MeshCache = meshCache;
      _shapeCache.AddShapeDataType<PartSet>();
      _transientCache.AddShapeDataType<PartSet>();
    }

    // /// <summary>
    // /// Override.
    // /// </summary>
    // /// <param name="root"></param>
    // /// <param name="serverRoot"></param>
    // /// <param name="materials"></param>
    // public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    // {
    //   Root.transform.SetParent(serverRoot.transform, false);
    // }

    /// <summary>
    /// Clear all current objects and mesh references.
    /// </summary>
    public override void Reset()
    {
      base.Reset();
      foreach (List<PartSet> list in _registeredParts.Values)
      {
        list.Clear();
      }
    }

    /// <summary>
    /// Render all the current objects.
    /// </summary>
    public override void Render(CameraContext cameraContext)
    {
      // TODO: (KS) category handling.
      foreach (int index in _transientCache.ShapeIndices)
      {
        RenderMeshes(cameraContext, _transientCache, index);
      }
      foreach (int index in _shapeCache.ShapeIndices)
      {
        RenderMeshes(cameraContext, _shapeCache, index);
      }
    }

    private void RenderMeshes(CameraContext cameraContext, ShapeCache cache, int shapeIndex)
    {
      CreateMessage shape = cache.GetShapeByIndex(shapeIndex);
      Matrix4x4 shapeWorldTransform = cameraContext.TesSceneToWorldTransform * cache.GetShapeTransformByIndex(shapeIndex);
      PartSet parts = cache.GetShapeDataByIndex<PartSet>(shapeIndex);

      for (int i = 0; i < parts.Meshes.Length; ++i)
      {
        RenderMesh mesh = (parts.Meshes[i] != null) ? parts.Meshes[i].Mesh : null;

        if (mesh == null)
        {
          continue;
        }

        if (mesh.MaterialDirty)
        {
          mesh.UpdateMaterial();
        }

        Material material =
          (parts.MaterialOverrides != null && parts.MaterialOverrides.Length > 0 && parts.MaterialOverrides[i]) ?
            parts.MaterialOverrides[i] : mesh.Material;

        if (material == null)
        {
          continue;
        }

        // TODO: (KS) use transparent queue for parts with transparency.
        CommandBuffer renderQueue = cameraContext.OpaqueBuffer;

        // Push the part transform.
        Matrix4x4 partWorldTransform = shapeWorldTransform * parts.Transforms[i] * mesh.LocalTransform;

        if (mesh.HasColours)
        {
          material.SetBuffer("_Colours", mesh.ColoursBuffer);
        }

        if (mesh.HasNormals)
        {
          material.SetBuffer("_Normals", mesh.NormalsBuffer);
        }

        // if (mesh.HasUVs)
        // {
        //   material.SetBuffer("uvs", mesh.UvsBuffer);
        // }

        if (material.HasProperty("_Color"))
        {
          material.SetColor("_Color", new Maths.Colour(shape.Attributes.Colour).ToUnity32());
        }

        if (material.HasProperty("_Tint"))
        {
          material.SetColor("_Tint", mesh.Tint.ToUnity32());
        }

        if (material.HasProperty("_BackColour"))
        {
          material.SetColor("_BackColour", new Maths.Colour(shape.Attributes.Colour).ToUnity32());
        }

        // Bind vertices and draw.
        material.SetBuffer("_Vertices", mesh.VertexBuffer);

        if (mesh.IndexBuffer != null)
        {
          renderQueue.DrawProcedural(mesh.IndexBuffer, partWorldTransform, material, 0, mesh.Topology, mesh.IndexCount);
        }
        else
        {
          renderQueue.DrawProcedural(partWorldTransform, material, 0, mesh.Topology, mesh.VertexCount);
        }
      }
    }

    /// <summary>
    /// Handler name.
    /// /// </summary>
    public override string Name { get { return "MeshSet"; } }

    /// <summary>
    /// <see cref="ShapeID.MeshSet"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.MeshSet; } }

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
    /// Creates a mesh set shape for serialising the mesh and its resource references.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeCache cache, int shapeIndex, CreateMessage shapeData)
    {
      // Start by building the resource list.
      PartSet partSet = cache.GetShapeDataByIndex<PartSet>(shapeIndex);
      Shapes.MeshSet meshSet = new Shapes.MeshSet(shapeData.ObjectID, shapeData.Category);
      meshSet.SetAttributes(shapeData.Attributes);
      for (int i = 0; i < partSet.MeshIDs.Length; ++i)
      {
        MeshResourcePlaceholder part = new MeshResourcePlaceholder(partSet.MeshIDs[i]);
        meshSet.AddPart(part, Tes.Maths.Matrix4Ext.FromUnity(partSet.Transforms[i]));
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
    protected override Error PostHandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      PartSet parts = new PartSet();
      ushort meshPartCount = reader.ReadUInt16();

      parts.MeshIDs = new uint[meshPartCount];
      parts.Meshes = new MeshCache.MeshDetails[meshPartCount];
      parts.Transforms = new Matrix4x4[meshPartCount];
      parts.Tints = new Maths.Colour[meshPartCount];
      parts.MaterialOverrides = new Material[meshPartCount];
      parts.ObjectFlags = (ObjectFlag)msg.Flags;

      for (ushort i = 0; i < meshPartCount; ++i)
      {
        parts.MeshIDs[i] = reader.ReadUInt32();
        ObjectAttributes attributes = new ObjectAttributes();
        if (!attributes.Read(reader))
        {
          return new Error(ErrorCode.MalformedMessage, (int)ObjectMessageID.Create);
        }

        DecodeTransform(attributes, out parts.Transforms[i]);
        parts.Tints[i] = new Maths.Colour(attributes.Colour);
      }

      cache.SetShapeDataByIndex(shapeIndex, parts);
      RegisterForMesh(msg, parts);

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
    protected override Error PostHandleMessage(DestroyMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      PartSet partSet = cache.GetShapeDataByIndex<PartSet>(shapeIndex);

      if (partSet != null)
      {
        // Remove from the registered mesh list.
        for (int i = 0; i < partSet.MeshIDs.Length; ++i)
        {
          List<PartSet> parts;
          if (_registeredParts.TryGetValue(partSet.MeshIDs[i], out parts))
          {
            // Remove from the list.
            parts.RemoveAll((PartSet cmp) => { return cmp == partSet; });
          }
        }
      }

      return new Error();
    }

    protected void RegisterForMesh(CreateMessage shape, PartSet partSet)
    {
      for (int i = 0; i < partSet.MeshIDs.Length; ++i)
      {
        List<PartSet> parts = null;
        if (!_registeredParts.TryGetValue(partSet.MeshIDs[i], out parts))
        {
          // Add new list.
          parts = new List<PartSet>();
          _registeredParts.Add(partSet.MeshIDs[i], parts);
        }

        parts.Add(partSet);

        // Resolve the mesh.
        if (_meshCache != null)
        {
          MeshCache.MeshDetails meshDetails = _meshCache.GetEntry(partSet.MeshIDs[i]);
          UpdateMesh(partSet, i, meshDetails);
          if (meshDetails != null && meshDetails.Finalised)
          {
            partSet.Meshes[i] = meshDetails;
            BindMaterial(partSet, i);
          }
        }
      }
    }

    protected void UpdateMesh(PartSet partSet, int index, MeshCache.MeshDetails meshDetails)
    {
      if (meshDetails != null && meshDetails.Finalised)
      {
        partSet.Meshes[index] = meshDetails;
        // Select a material override if required.
      }
      else
      {
        partSet.Meshes[index] = null;
        partSet.MaterialOverrides[index] = null;
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
      List<PartSet> parts;
      if (!_registeredParts.TryGetValue(meshDetails.ID, out parts))
      {
        // Nothing waiting.
        return;
      }

      // Have parts to resolve.
      foreach (PartSet partSet in parts)
      {
        for (int i = 0; i < partSet.MeshIDs.Length; ++i)
        {
          if (partSet.MeshIDs[i] == meshDetails.ID)
          {
            partSet.Meshes[i] = meshDetails;
            BindMaterial(partSet, i);
          }
        }
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
      List<PartSet> parts;
      if (!_registeredParts.TryGetValue(meshDetails.ID, out parts))
      {
        // Nothing using this mesh.
        return;
      }

      // Have things using this mesh. Clear them.
      foreach (PartSet partSet in parts)
      {
        for (int i = 0; i < partSet.MeshIDs.Length; ++i)
        {
          if (partSet.MeshIDs[i] == meshDetails.ID)
          {
            partSet.Meshes[i] = null;
          }
        }
      }
    }

    protected virtual void BindMaterial(PartSet partSet, int partIndex)
    {
      Material material = null;
      if ((partSet.ObjectFlags & ObjectFlag.Wireframe) == ObjectFlag.Wireframe)
      {
        material = Materials[MaterialLibrary.WireframeMesh];
      }
      else if ((partSet.ObjectFlags & ObjectFlag.Transparent) == ObjectFlag.Transparent)
      {
        material = Materials[MaterialLibrary.TransparentMesh];
      }
      else if ((partSet.ObjectFlags & ObjectFlag.TwoSided) == ObjectFlag.TwoSided)
      {
        material = Materials[MaterialLibrary.OpaqueTwoSidedMesh];
      }
      // else no override.

      if (material != null && partSet.Meshes[partIndex] != null)
      {
        // Have an override material. Validate the topology. Only supported for triangles and quads.
        RenderMesh mesh = partSet.Meshes[partIndex].Mesh;
        if (mesh.Topology == MeshTopology.Triangles || mesh.Topology == MeshTopology.Quads)
        {
          // Instance the material and setup streams.
          material = new Material(material);
          if (mesh.HasColours)
          {
            material.EnableKeyword("WITH_COLOURS_UINT");
          }

          if (mesh.HasNormals)
          {
            material.EnableKeyword("WITH_NORMALS");
          }

          partSet.MaterialOverrides[partIndex] = material;
        }
      }
    }

    private Dictionary<uint, List<PartSet>> _registeredParts = new Dictionary<uint, List<PartSet>>();
    private MeshCache _meshCache = null;
  }
}
