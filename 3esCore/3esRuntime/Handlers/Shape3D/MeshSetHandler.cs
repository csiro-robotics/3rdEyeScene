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
    public class PartSet
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
      if (Root != null)
      {
        Root.name = Name;
      }
      MeshCache = meshCache;
      _shapeCache.AddExtensionType<PartSet>();
      _transientCache.AddExtensionType<PartSet>();
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
    }

    /// <summary>
    /// Render all the current objects.
    /// </summary>
    public override void Render(ulong categoryMask, Matrix4x4 primaryCameraTransform)
    {
      var renderMeshesFunc = (ShapeCache cache, int shapeIndex) =>
      {
        CreateMessage shape = cache.GetShapeDataByIndex<CreateMessage>(shapeIndex);
        Matrix4x4 transform = cache.GetShapeDataByIndex<Matrix4x4>(shapeIndex);
        PartSet parts = cache.GetShapeDataByIndex<PartSet>(shapeIndex);

        if (parts == null)
        {
          // No mesh.
          Debug.LogWarning($"Point cloud shape {shape.ObjectID} missing mesh with ID {points.MeshID}");
          continue;
        }

        GL.PushMatrix();

        try
        {
          // Add shape transform.
          GL.MultMatrix(transform);

          for (int i = 0; i < parts.Meshes.Length; ++i)
          {
            MeshCache.MeshDetails mesh = parts.Meshes[i];

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

            GL.PushMatrix();

            try
            {
              // Push the part transform.
              GL.MultMatrix(parts.Transforms[i]);

              // Push any transform associated with the part's mesh.
              GL.MultMatrix(mesh.LocalTransform);

              // Activate material and bind available buffers.
              material.SetPass(0);

              if (mesh.HasColours)
              {
                material.SetBuffer("colours", mesh.ColoursBuffer);
              }

              if (mesh.HasNormals)
              {
                material.SetBuffer("normals", mesh.NormalsBuffer);
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
                material.SetColor("_Tint", new Maths.Colour(shape.Attributes.Colour).ToUnity32());
              }

              if (material.HasProperty("_BackColour"))
              {
                material.SetColor("_BackColour", mesh.Tint.ToUnity32());
              }

              // Bind vertices and draw.
              material.SetBuffer("vertices", mesh.VertexBuffer);

              if (mesh.IndexBuffer != null)
              {
                Graphics.DrawProceduralNow(mesh.Topology, mesh.IndexBuffer, mesh.IndexCount, 1);
              }
              else
              {
                Graphics.DrawProceduralNow(mesh.Topology, mesh.VertexCount, 1);
              }
            }
            finally
            {
              GL.PopMatrix();
            }
          }
        }
        finally
        {
          GL.PopMatrix();
        }
      };

      // TODO: (KS) category handling.
      foreach (int index in _transientCache.ShapeIndices)
      {
        renderMeshesFunc(_transientCache, index);
      }
      foreach (int index in _shapeCache.ShapeIndices)
      {
        renderMeshesFunc(_shapeCache, index);
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
    /// Creates a mesh set shape for serialising the mesh and its resource references.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeCache cache, int shapeIndex, CreateMessage shapeData)
    {
      // Start by building the resource list.
      PartSet partSet = cache.GetShapeDataByIndex<PartSet>(shapeIndex);
      Shapes.MeshSet meshSet = new Shapes.MeshSet(shapeData.ObjectID, shapeData.Category);
      meshSet.SetAttributes(shapeData);
      for (int i = 0; i < partSet.MeshIDs.Length; ++i)
      {
        MeshResourcePlaceholder part = new MeshResourcePlaceholder(partSet.MeshIDs[i]);
        meshSet.AddPart(part, partSet.Transforms[i]);
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
        material = Materials[MaterialLibrary.WireframeTriangles];
      }
      else if ((partSet.Transparent & ObjectFlag.Transparent) == ObjectFlag.Transparent)
      {
        material = Materials[MaterialLibrary.Transparent];
      }
      else if ((partSet.TwoSided & ObjectFlag.TwoSided) == ObjectFlag.TwoSided)
      {
        material = Materials[MaterialLibrary.OpaqueTwoSided];
      }
      // else no override.

      if (material && partSet.Meshes[partIndex])
      {
        // Have an override material. Validate the topology. Only supported for triangles and quads.
        RenderMesh mesh = partSet.Meshes[partIndex].Mesh;
        if (mesh.Topology == MeshTopology.Triangles || mesh.Topology == MeshTopology.Quads)
        {
          // Instance the material and setup streams.
          material = new Material(material);
          if (mesh.HasColours)
          {
            material.EnableKeyword("WITH_COLOURS");
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
