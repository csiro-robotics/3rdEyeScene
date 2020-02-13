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
      // if (Root != null)
      // {
      //   Root.name = Name;
      // }
      _shapeCache.AddShapeDataType<PointsComponent>();
      _transientCache.AddShapeDataType<PointsComponent>();
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
      // Root.transform.SetParent(serverRoot.transform, false);
      _pointsMaterial = materials[MaterialLibrary.Points];
    }

    /// <summary>
    /// Clear all current objects and mesh references.
    /// </summary>
    public override void Reset()
    {
      base.Reset();
      foreach (List<PointsComponent> list in _registeredParts.Values)
      {
        foreach (var points in list)
        {
          points.Release();
        }
        list.Clear();
      }
    }

    /// <summary>
    /// Render all the current objects.
    /// </summary>
    public override void Render(ulong categoryMask, Matrix4x4 primaryCameraTransform)
    {
      // TODO: (KS) category handling.
      foreach (int index in _transientCache.ShapeIndices)
      {
        RenderPoints(_transientCache, index);
      }
      foreach (int index in _shapeCache.ShapeIndices)
      {
        RenderPoints(_shapeCache, index);
      }
    }

    void RenderPoints(ShapeCache cache, int shapeIndex)
    {
      CreateMessage shape = cache.GetShapeByIndex(shapeIndex);
      Matrix4x4 transform = cache.GetShapeTransformByIndex(shapeIndex);
      PointsComponent points = cache.GetShapeDataByIndex<PointsComponent>(shapeIndex);
      RenderMesh mesh = points.Mesh != null ? points.Mesh.Mesh : null;

      if (mesh == null)
      {
        // No mesh.
        Debug.LogWarning($"Point cloud shape {shape.ObjectID} missing mesh with ID {points.MeshID}");
        return;
      }

      if (points.Material == null)
      {
        // No mesh.
        Debug.LogWarning($"Point cloud shape {shape.ObjectID} missing material");
        return;
      }

      GL.PushMatrix();

      try
      {
        // Bind the material
        points.Material.SetPass(0);

        // Add shape transform.
        GL.MultMatrix(transform);
        // Add mesh local transform.
        GL.MultMatrix(points.Mesh.LocalTransform);

        // Check rendering with index buffer?
        GraphicsBuffer indexBuffer = null;
        int indexCount = 0;
        if (mesh.IndexCount > 0 || points.IndexCount > 0)
        {
          if ((int)points.IndexCount > 0)
          {
            indexBuffer = points.IndexBuffer;
            indexCount = (int)points.IndexCount;
          }
          // We only use the mesh index buffer if the mesh has points topology.
          // Otherwise we convert to points using vertices as is.
          else if (mesh.Topology == MeshTopology.Points)
          {
            indexBuffer = mesh.IndexBuffer;
            indexCount = mesh.IndexCount;
          }
        }

        if (mesh.HasColours)
        {
          points.Material.SetBuffer("_Colours", mesh.ColoursBuffer);
        }

        if (mesh.HasNormals)
        {
          points.Material.SetBuffer("_Normals", mesh.NormalsBuffer);
        }

        points.Material.SetBuffer("_Vertices", mesh.VertexBuffer);

        if (indexBuffer != null)
        {
          Graphics.DrawProceduralNow(mesh.Topology, indexBuffer, indexCount, 1);
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

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "PointCloud"; } }

    /// <summary>
    /// <see cref="ShapeID.PointCloud"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.PointCloud; } }

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
    /// Creates a point cloud shape for serialising <paramref name="shapeComponent"/> and its point data.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeCache cache, int shapeIndex, CreateMessage shapeData)
    {
      PointsComponent pointsComp = cache.GetShapeDataByIndex<PointsComponent>(shapeIndex);
      Shapes.PointCloudShape points = new Shapes.PointCloudShape(new MeshResourcePlaceholder(pointsComp.MeshID),
                                                                 shapeData.ObjectID,
                                                                 shapeData.Category,
                                                                 (byte)pointsComp.PointSize);
      points.SetAttributes(shapeData.Attributes);
      if (pointsComp.IndexCount > 0)
      {
        points.SetIndices(pointsComp.Indices);
      }
      return points;
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
      // Read additional attributes.
      PointsComponent pointsComp = new PointsComponent();
      // Read the mesh ID to render points from.
      pointsComp.MeshID = reader.ReadUInt32();
      // Read the number of indices (zero implies show entire mesh).
      pointsComp.IndexCount = reader.ReadUInt32();
      pointsComp.PointSize = reader.ReadByte();

      cache.SetShapeDataByIndex(shapeIndex, pointsComp);

      if (pointsComp.IndexCount == 0)
      {
        pointsComp.IndicesDirty = false;
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
      ShapeCache cache = (msg.ObjectID == 0) ? _transientCache : _shapeCache;
      int shapeIndex = (msg.ObjectID == 0) ? _lastTransientIndex : cache.GetShapeIndex(msg.ObjectID);

      if (shapeIndex < 0)
      {
        return new Error(ErrorCode.InvalidObjectID, msg.ObjectID);
      }

      PointsComponent pointsComp = cache.GetShapeDataByIndex<PointsComponent>(shapeIndex);

      // Read index offset and count.
      uint offset = reader.ReadUInt32();
      uint count = reader.ReadUInt32();

      int[] indices = pointsComp.Indices;
      for (uint i = 0; i < count; ++i)
      {
        indices[i + offset] = reader.ReadInt32();
      }

      pointsComp.IndicesDirty = true;

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
    protected override Error PostHandleMessage(DestroyMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      PointsComponent pointsComp = cache.GetShapeDataByIndex<PointsComponent>(shapeIndex);
      if (pointsComp != null)
      {
        // Remove from the registered mesh list.
        List<PointsComponent> parts;
        if (_registeredParts.TryGetValue(pointsComp.MeshID, out parts))
        {
          // Remove from the list.
          parts.RemoveAll((PointsComponent cmp) => { return cmp == pointsComp; });
        }

        pointsComp.Release();
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
          points.Mesh = meshDetails;
          BindMaterial(points);
        }
      }
    }

    protected virtual void BindMaterial(PointsComponent points)
    {
      // Select the material by topology.
      Material material = new Material(_pointsMaterial);

      if (points.Mesh.Mesh.HasColours)
      {
        material.EnableKeyword("WITH_COLOURS");
      }

      if (points.Mesh.Mesh.HasNormals)
      {
        material.EnableKeyword("WITH_NORMALS");
      }

      if (material.HasProperty("_LeftHanded"))
      {
        material.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
      }

      if (material.HasProperty("_PointSize"))
      {
        material.SetInt("_PointSize", points.PointSize);
      }

      if (material.HasProperty("_Color"))
      {
        material.SetColor("_Color", points.Colour);
      }

      if (material.HasProperty("_Tint"))
      {
        material.SetColor("_Tint", points.Mesh.Tint);
      }

      points.Material = material;
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
      foreach (PointsComponent part in parts)
      {
        part.Mesh = null;
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
        part.Mesh = meshDetails;
        BindMaterial(part);
      }
    }

    /// <summary>g
    /// Mapping of Mesh ID to PointsComponents.
    /// </summary>
    private Dictionary<uint, List<PointsComponent>> _registeredParts = new Dictionary<uint, List<PointsComponent>>();
    private Material _pointsMaterial = null;
    private MeshCache _meshCache = null;
  }
}
