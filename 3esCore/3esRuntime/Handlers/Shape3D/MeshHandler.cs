using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Shapes;
using Tes.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Shape handler for mesh shapes.
  /// </summary>
  /// <remarks>
  /// Mesh shapes represent pseudo immediate mode rendering of vertex data with optional indexing.
  /// </remarks>
  public class MeshHandler : ShapeHandler
  {
    public struct MeshEntry : IShapeData
    {
      public RenderMesh Mesh;
      public Material Material;
      public bool CalculateNormals;
      /// <summary>
      /// Render scaling for points, lines, etc.
      /// </summary>
      public float DrawScale;
    }

    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public MeshHandler()
    {
      // if (Root != null)
      // {
      //   Root.name = Name;
      // }
      _shapeCache.AddShapeDataType<MeshEntry>();
      _transientCache.AddShapeDataType<MeshEntry>();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Mesh"; } }

    /// <summary>
    /// <see cref="ShapeID.Mesh"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Mesh; } }

    /// <summary>
    /// Initialise, caching the <see cref="MaterialLibrary"/>.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="serverRoot"></param>
    /// <param name="materials"></param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      base.Initialise(root, serverRoot, materials);

      // Add a render mesh component to the shapes.
      _transientCache.TransientExpiry = this.ExpireTransientShapes;
    }

    /// <summary>
    /// Overridden to release mesh resources.
    /// </summary>
    public override void Reset()
    {
      // Clear out all the mesh data in our objects.
      foreach (int shapeIndex in _transientCache.ShapeIndices)
      {
        ResetObject(_transientCache, shapeIndex);
      }

      foreach (int shapeIndex in _shapeCache.ShapeIndices)
      {
        ResetObject(_shapeCache, shapeIndex);
      }

      base.Reset();
    }

    /// <summary>
    /// Finalises mesh objects.
    /// </summary>
    public override void BeginFrame(uint frameNumber, bool maintainTransient)
    {
      for (int i = 0; i < _toCalculateNormals.Count; ++i)
      {
        _toCalculateNormals[i].CalculateNormals();
      }

      _toCalculateNormals.Clear();

      base.BeginFrame(frameNumber, maintainTransient);
    }

    protected void ExpireTransientShapes(ShapeCache cache, int fromIndex, int count)
    {
      for (int i = 0; i < count; ++i)
      {
        ResetObject(_transientCache, i + fromIndex);
      }
    }

    public override void Render(CameraContext cameraContext)
    {
      // TODO: (KS) Resolve categories.
      foreach (int shapeIndex in _transientCache.ShapeIndices)
      {
        RenderObject(cameraContext, _transientCache, shapeIndex);
      }

      foreach (int shapeIndex in _shapeCache.ShapeIndices)
      {
        RenderObject(cameraContext, _shapeCache, shapeIndex);
      }
    }

    protected void RenderObject(CameraContext cameraContext, ShapeCache cache, int shapeIndex)
    {
      CreateMessage shape = cache.GetShapeByIndex(shapeIndex);

      if (CategoriesState != null && !CategoriesState.IsActive(shape.Category))
      {
        return;
      }

      MeshEntry meshEntry = cache.GetShapeDataByIndex<MeshEntry>(shapeIndex);
      Matrix4x4 transform = cache.GetShapeTransformByIndex(shapeIndex);

      // TODO: (KS) select command buffer for transparent rendering.
      CommandBuffer renderQueue = cameraContext.OpaqueBuffer;
      Material material = meshEntry.Material;
      RenderMesh mesh = meshEntry.Mesh;

      if (material == null || mesh == null)
      {
        return;
      }

      Matrix4x4 modelWorld = cameraContext.TesSceneToWorldTransform * transform;

      // Transform and cull bounds
      // FIXME: (KS) this really isn't how culling should be performed.
      Bounds bounds = GeometryUtility.CalculateBounds(new Vector3[] { mesh.MinBounds, mesh.MaxBounds }, modelWorld);
      if (!GeometryUtility.TestPlanesAABB(cameraContext.CameraFrustumPlanes, bounds))
      {
        return;
      }

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
        material.SetColor("_Color", Maths.ColourExt.ToUnity(new Maths.Colour(shape.Attributes.Colour)));
      }

      if (material.HasProperty("_Tint"))
      {
        material.SetColor("_Tint", Maths.ColourExt.ToUnity(mesh.Tint));
      }

      if (material.HasProperty("_BackColour"))
      {
        material.SetColor("_BackColour", Maths.ColourExt.ToUnity(new Maths.Colour(shape.Attributes.Colour)));
      }

      // TODO: (KS) Need to derive this from the shape properties.
      if (mesh.Topology == MeshTopology.Points)
      {
        // Set min/max shader values.
        if (material.HasProperty("_BoundsMin"))
        {
          material.SetVector("_BoundsMin", mesh.MinBounds);
        }
        if (material.HasProperty("_BoundsMax"))
        {
          material.SetVector("_BoundsMax", mesh.MaxBounds);
        }

        float pointScale = (meshEntry.DrawScale > 0) ? meshEntry.DrawScale : 1.0f;
        material.SetFloat("_PointSize", GlobalSettings.PointSize * pointScale);

        // Colour by height if we have a zero colour value.
        if (shape.Attributes.Colour == 0)
        {
          material.SetColor("_Color", Color.white);
          material.SetColor("_BackColour", Color.white);
          switch (CoordinateFrameUtil.AxisIndex(ServerInfo.CoordinateFrame, 2))
          {
          case 0:
            material.EnableKeyword("WITH_COLOURS_RANGE_X");
            break;
          case 1:
            material.EnableKeyword("WITH_COLOURS_RANGE_Y");
            break;
          default:
          case 2:
            material.EnableKeyword("WITH_COLOURS_RANGE_Z");
            break;
          }
        }
      }

      // Bind vertices and draw.
      material.SetBuffer("_Vertices", mesh.VertexBuffer);

      if (mesh.IndexBuffer != null)
      {
        renderQueue.DrawProcedural(mesh.IndexBuffer, modelWorld, material, 0, mesh.Topology, mesh.IndexCount);
      }
      else
      {
        renderQueue.DrawProcedural(modelWorld, material, 0, mesh.Topology, mesh.VertexCount);
      }
    }

    /// <summary>
    /// Creates a mesh shape for serialising <paramref name="shapeComponent"/> and its associated mesh data.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeCache cache, int shapeIndex, CreateMessage shapeData)
    {
      MeshEntry meshEntry = cache.GetShapeDataByIndex<MeshEntry>(shapeIndex);
      RenderMesh mesh = meshEntry.Mesh;

      Debug.Log($"serialise mesh. Normals? {mesh.HasNormals}");

      Shapes.MeshShape meshShape = new Shapes.MeshShape(meshEntry.Mesh.DrawType,
                                                        Maths.Vector3Ext.FromUnity(mesh.Vertices),
                                                        mesh.Indices,
                                                        shapeData.ObjectID, shapeData.Category);

      if (mesh.HasNormals && !meshEntry.CalculateNormals)
      {
        // We have normals which were not locally calculated.
        meshShape.Normals = Tes.Maths.Vector3Ext.FromUnity(mesh.Normals);
      }

      meshShape.SetAttributes(shapeData.Attributes);
      return meshShape;
    }

    /// <summary>
    /// Handle triangle count.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      int vertexCount = reader.ReadInt32();
      int indexCount = reader.ReadInt32();
      MeshDrawType drawType = (MeshDrawType)reader.ReadByte();

      RenderMesh mesh = new RenderMesh(drawType, vertexCount, indexCount);
      MeshEntry meshEntry = new MeshEntry
      {
        Mesh = mesh,
        CalculateNormals = (msg.Flags & (ushort)MeshShapeFlag.CalculateNormals) != 0,
        DrawScale = 0.0f
      };

      if (packet.Header.VersionMajor == 0 && packet.Header.VersionMinor == 1)
      {
        // Legacy handling.
        meshEntry.DrawScale = 0.0f;
      }
      else
      {
        meshEntry.DrawScale = reader.ReadSingle();
      }

      cache.SetShapeDataByIndex(shapeIndex, meshEntry);

      if (meshEntry.CalculateNormals)
      {
        _toCalculateNormals.Add(mesh);
      }
      return base.PostHandleMessage(msg, packet, reader, cache, shapeIndex);
    }

    /// <summary>
    /// Overridden to handle triangle data in the <paramref name="msg"/>
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error HandleMessage(DataMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      ShapeCache cache = (msg.ObjectID == 0) ? _transientCache : _shapeCache;
      int shapeIndex = (msg.ObjectID == 0) ? _lastTransientIndex : cache.GetShapeIndex(msg.ObjectID);

      if (shapeIndex < 0)
      {
        return new Error(ErrorCode.InvalidObjectID, msg.ObjectID);
      }

      // Naive support for multiple packets. Assume:
      // - In order.
      MeshEntry meshEntry = cache.GetShapeDataByIndex<MeshEntry>(shapeIndex);

      // Well, this is confusing indirection...
      int readComponent = MeshShape.ReadDataComponentDeferred(
        reader, (uint)meshEntry.Mesh.VertexCount, (uint)meshEntry.Mesh.IndexCount,
        // Vertex handler.
        new MeshShape.ComponentBlockReader((MeshShape.SendDataType dataType, BinaryReader reader2, uint offset, uint count) =>
        {
          return ReadMeshVector3Data(reader2, offset, count, (Vector3[] buffer, int writeOffset, int writeCount) =>
          {
            meshEntry.Mesh.SetVertices(buffer, 0, writeOffset, writeCount, true);
          });
        }),
        // Index handler
        new MeshShape.ComponentBlockReader((MeshShape.SendDataType dataType, BinaryReader reader2, uint offset, uint count) =>
        {
          return ReadIndexComponent(reader2, offset, count, meshEntry.Mesh);
        }),
        // Normals handler.
        new MeshShape.ComponentBlockReader((MeshShape.SendDataType dataType, BinaryReader reader2, uint offset, uint count) =>
        {
          return ReadMeshVector3Data(reader2, offset, count, (Vector3[] buffer, int writeOffset, int writeCount) =>
          {
            if (dataType == MeshShape.SendDataType.UniformNormal)
            {
              // Only one normal for the whole mesh.
              // Fill the buffer and write in chunks.
              for (int i = 1; i < buffer.Length; ++i)
              {
                buffer[i] = buffer[0];
              }
              int localOffset = 0;
              for (int i = 0; i < meshEntry.Mesh.VertexCount; i += buffer.Length)
              {
                int blockCount = Math.Min(buffer.Length, meshEntry.Mesh.VertexCount - localOffset);
                meshEntry.Mesh.SetNormals(buffer, 0, localOffset, blockCount);
                writeOffset += blockCount;
              }
            }
            else
            {
              meshEntry.Mesh.SetNormals(buffer, 0, writeOffset, writeCount);
            }
          });
        }),
        // Colours handler.
        new MeshShape.ComponentBlockReader((MeshShape.SendDataType dataType, BinaryReader reader2, uint offset, uint count) =>
        {
          return ReadColourComponent(reader2, offset, count, meshEntry.Mesh);
        })
      );

      if (readComponent == -1)
      {
        return new Error(ErrorCode.MalformedMessage, DataMessage.MessageID);
      }

      if (readComponent == (int)(MeshShape.SendDataType.Vertices |  MeshShape.SendDataType.End))
      {
        // Finalise the material.
        meshEntry.Material = CreateMaterial(cache.GetShapeByIndex(shapeIndex), meshEntry);
        cache.SetShapeDataByIndex(shapeIndex, meshEntry);
      }

      return new Error();
    }

    delegate void WriteToMesh(Vector3[] buffer, int writeOffset, int count);
    private uint ReadMeshVector3Data(BinaryReader reader, uint offset, uint count, WriteToMesh writer)
    {
      int dstIndex = 0;
      int meshDstOffset = (int)offset;
      Vector3 v3 = Vector3.zero;
      for (uint srcIndex = 0; srcIndex < count; ++srcIndex)
      {
        v3.x = reader.ReadSingle();
        v3.y = reader.ReadSingle();
        v3.z = reader.ReadSingle();
        _v3Buffer[dstIndex++] = v3;
        if (dstIndex == _v3Buffer.Length)
        {
          // Flush buffer.
          writer(_v3Buffer, meshDstOffset, dstIndex);
          meshDstOffset += dstIndex;
          dstIndex = 0;
        }
      }

      if (dstIndex > 0)
      {
        // Flush buffer.
        writer(_v3Buffer, meshDstOffset, dstIndex);
        meshDstOffset += dstIndex;
        dstIndex = 0;
      }

      return (uint)meshDstOffset;
    }

    private uint ReadIndexComponent(BinaryReader reader, uint offset, uint count, RenderMesh mesh)
    {
      int dstIndex = 0;
      int meshDstOffset = (int)offset;
      for (uint srcIndex = 0; srcIndex < count; ++srcIndex)
      {
        _intBuffer[dstIndex++] = reader.ReadInt32();
        if (dstIndex == _intBuffer.Length)
        {
          // Flush buffer.
          mesh.SetIndices(_intBuffer, 0, meshDstOffset, dstIndex);
          meshDstOffset += dstIndex;
          dstIndex = 0;
        }
      }

      if (dstIndex > 0)
      {
        // Flush buffer.
        mesh.SetIndices(_intBuffer, 0, meshDstOffset, dstIndex);
        meshDstOffset += dstIndex;
        dstIndex = 0;
      }

      return (uint)meshDstOffset;
    }

    private uint ReadColourComponent(BinaryReader reader, uint offset, uint count, RenderMesh mesh)
    {
      int dstIndex = 0;
      int meshDstOffset = (int)offset;
      for (uint srcIndex = 0; srcIndex < count; ++srcIndex)
      {
        _uintBuffer[dstIndex++] = reader.ReadUInt32();
        if (dstIndex == _uintBuffer.Length)
        {
          // Flush buffer.
          mesh.SetColours(_uintBuffer, 0, meshDstOffset, dstIndex);
          meshDstOffset += dstIndex;
          dstIndex = 0;
        }
      }

      if (dstIndex > 0)
      {
        // Flush buffer.
        mesh.SetColours(_uintBuffer, 0, meshDstOffset, dstIndex);
        meshDstOffset += dstIndex;
        dstIndex = 0;
      }

      return (uint)meshDstOffset;
    }

    /// <summary>
    /// Overridden to clear mesh data and release resources even for transient objects.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(DestroyMessage msg, ShapeCache cache, int shapeIndex)
    {
      if (shapeIndex >= 0)
      {
        // Delete of invalid object is allowed.
        ResetObject(cache, shapeIndex);
      }

      return new Error();
    }

    /// <summary>
    /// Select the material for <paramref name="shape"/> based on <paramref name="meshData"/>
    /// configuration.
    /// </summary>
    /// <remarks>
    /// Wire frame selection may only be made for triangle draw types. In this case the
    /// <see cref="ShapeComponent"/> is checked for it's <code>Wireframe</code> flag.
    /// </remarks>
    /// <param name="shape">The shape object to select a material for.</param>
    /// <param name="meshData">Details of the mesh shape we are selecting a material for.</param>
    /// <returns>The appropriate material for rendering <paramref name="meshData"/>.</returns>
    Material CreateMaterial(CreateMessage shape, MeshEntry meshEntry)
    {
      Material mat;
      switch (meshEntry.Mesh.DrawType)
      {
      case MeshDrawType.Points:
        mat = new Material(Materials[MaterialLibrary.Points]);
        MaterialLibrary.SetupMaterial(mat, meshEntry.Mesh);
        int pointSize = (Materials != null) ? Materials.DefaultPointSize : 4;
        mat.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        break;
      case MeshDrawType.Voxels:
        mat = new Material(Materials[MaterialLibrary.Voxels]);
        MaterialLibrary.SetupMaterial(mat, meshEntry.Mesh);
        mat.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        break;
      default:
      case MeshDrawType.Lines:
        mat = new Material(Materials[MaterialLibrary.OpaqueMesh]);
        if (meshEntry.Mesh.HasColours)
        {
          mat.EnableKeyword("WITH_COLOURS_UINT");
        }
        break;
      case MeshDrawType.Triangles:
        // Check wire frame.
        if ((shape.Flags & (uint)ObjectFlag.Wireframe) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.WireframeMesh]);
        }
        else if ((shape.Flags & (uint)ObjectFlag.Transparent) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.TransparentMesh]);
        }
        else if ((shape.Flags & (uint)ObjectFlag.TwoSided) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.OpaqueTwoSidedMesh]);
        }
        else
        {
          //mat = new Material(Materials[MaterialLibrary.OpaqueTwoSidedMesh]);
          mat = new Material(Materials[MaterialLibrary.OpaqueMesh]);
        }
        MaterialLibrary.SetupMaterial(mat, meshEntry.Mesh);
        break;
      }

      if (mat.HasProperty("_Color"))
      {
        mat.SetColor("_Color", Maths.ColourExt.ToUnity(new Maths.Colour(shape.Attributes.Colour)));
      }

      if (mat.HasProperty("_BackColour"))
      {
        mat.SetColor("_BackColour", Maths.ColourExt.ToUnity(new Maths.Colour(shape.Attributes.Colour)));
      }

      return mat;
    }

    /// <summary>
    /// Release the mesh resources of <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj"></param>
    void ResetObject(ShapeCache cache, int shapeIndex)
    {
      MeshEntry meshEntry = cache.GetShapeDataByIndex<MeshEntry>(shapeIndex);
      meshEntry.Mesh.ReleaseBuffers();
    }

    /// <summary>
    /// Buffer chuck size when reading mesh components before migrating into compute buffers.
    /// </summary>
    private static readonly int s_bufferChuckSize = 0xffff;
    private Vector3[] _v3Buffer = new Vector3[s_bufferChuckSize];
    private Vector2[] _v2Buffer = new Vector2[s_bufferChuckSize];
    private int[] _intBuffer = new int[s_bufferChuckSize];
    private uint[] _uintBuffer = new uint[s_bufferChuckSize];
    private List<RenderMesh> _toCalculateNormals = new List<RenderMesh>();
  }
}
