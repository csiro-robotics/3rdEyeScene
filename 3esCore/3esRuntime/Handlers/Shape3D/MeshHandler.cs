using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Shapes;
using Tes.Runtime;
using UnityEngine;

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
    public struct MeshEntry
    {
      public RenderMesh Mesh;
      public Material Material;
      public bool CalculateNormals;
    }

    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public MeshHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      // if (Root != null)
      // {
      //   Root.name = Name;
      // }
      _shapeCache.AddExtensionType<MeshEntry>();
      _transientCache.AddExtensionType<MeshEntry>();
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
    /// Finalises Unity mesh objects.
    /// </summary>
    public override void PreRender()
    {
      base.PreRender();

      for (int i = 0; i < _toCalculateNormals.Count; ++i)
      {
        _toCalculateNormals[i].CalculateNormals();
      }

      _toCalculateNormals.Clear();
    }

    public override void Render(ulong categoryMask, Matrix4x4 primaryCameraTransform)
    {
      // TODO: (KS) Resolve categories.
      foreach (int shapeIndex in _transientCache.ShapeIndices)
      {
        RenderObject(_transientCache, shapeIndex);
      }

      foreach (int shapeIndex in _shapeCache.ShapeIndices)
      {
        RenderObject(_shapeCache, shapeIndex);
      }
    }

    /// <summary>
    /// Overridden to release mesh resources.
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <param name="maintainTransient"></param>
    public override void BeginFrame(uint frameNumber, bool maintainTransient)
    {
      if (!maintainTransient)
      {
        foreach (int shapeIndex in _transientCache.ShapeIndices)
        {
          ResetObject(_transientCache, shapeIndex);
        }
      }
      base.BeginFrame(frameNumber, maintainTransient);
    }

    protected void RenderObject(ShapeCache cache, int shapeIndex)
    {
      MeshEntry meshEntry = cache.GetShapeDataByIndex<MeshEntry>(shapeIndex);
      Matrix4x4 transform = cache.GetShapeDataByIndex<Matrix4x4>(shapeIndex);

      // Set buffers.
      GL.PushMatrix();
      GL.MultMatrix(transform);

      meshEntry.Mesh.Render();

      // material.SetPass(0);
      // // material.EnableKeyword("WITH_COLOUR");

      // if (_indexBuffer != null)
      // {
      //   Graphics.DrawProceduralNow(Topology, _indexBuffer, VertexCount, 1);
      // }
      // else
      // {
      //   Graphics.DrawProceduralNow(Topology, VertexCount, 1);
      // }

      GL.PopMatrix();
    }

    /// <summary>
    /// Creates a mesh shape for serialising <paramref name="shapeComponent"/> and its associated mesh data.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      MeshDataComponent meshData = shapeComponent.GetComponent<MeshDataComponent>();
      if (meshData != null)
      {
        ObjectAttributes attr = new ObjectAttributes();
        EncodeAttributes(ref attr, shapeComponent.gameObject, shapeComponent);

        Shapes.MeshShape mesh = new Shapes.MeshShape(meshData.DrawType,
                                                     Maths.Vector3Ext.FromUnity(meshData.Vertices),
                                                     meshData.Indices,
                                                     shapeComponent.ObjectID,
                                                     shapeComponent.Category,
                                                     Maths.Vector3.Zero,
                                                     Maths.Quaternion.Identity,
                                                     Maths.Vector3.One);
        mesh.SetAttributes(attr);
        mesh.CalculateNormals = meshData.CalculateNormals;
        if (!meshData.CalculateNormals && meshData.Normals != null && meshData.Normals.Length > 0)
        {
          mesh.Normals = Maths.Vector3Ext.FromUnity(meshData.Normals);
        }

        return mesh;
      }
      return null;
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
      uint vertexCount = reader.ReadUInt32();
      uint indexCount = reader.ReadUInt32();
      MeshDrawType drawType = (MeshDrawType)reader.ReadByte();

      RenderMesh mesh = new RenderMesh(drawType, vertexCount, indexCount);
      MeshEntry meshEntry = new MeshEntry
      {
        Mesh = mesh,
        CalculateNormals = (msg.Flags & (ushort)MeshShapeFlag.CalculateNormals) != 0
      };

      meshEntry.Material = CreateMaterial(msg, meshEntry);

      cache.SetShapeDataByIndex(shapeIndex, meshEntry);

      if (meshEntry.CalculateNormals)
      {
        _toCalculateNormals.Add(mesh);
      }
      return base.PostHandleMessage(obj, msg, packet, reader);
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

      int readComponent = MeshShape.ReadDataComponent(reader,
        (SendDataType dataType, BinaryReader reader, uint offset, uint count) =>
        {
          return ReadMeshVector3Data(reader, offset, count, (Vector3[] buffer, int count) =>
          {
            meshEntry.Mesh.SetVertices(buffer, 0, meshDstOffset, dstIndex);
          });
        },
        (SendDataType dataType, BinaryReader reader, uint offset, uint count) =>
        {
          return ReadIndexComponent(reader, offset, count, meshEntry.Mesh);
        },
        (SendDataType dataType, BinaryReader reader, uint offset, uint count) =>
        {
          return ReadMeshVector3Data(reader, offset, count, (Vector3[] buffer, int count) =>
          {
            if (dataType == SendDataType.UniformNormals)
            {
              // Only one normal for the whole mesh.
              // Fill the buffer and write in chunks.
              for (int i = 1; i < buffer.Length; ++i)
              {
                buffer[i] = buffer[0];
              }
              int offset = 0;
              for (int i = 0; i < meshEntry.Mesh.VertexCount; i += buffer.Length)
              {
                int count = Math.Min(buffer.Length, meshEntry.Mesh.VertexCount - offset);
                meshEntry.Mesh.SetNormals(buffer, 0, offset, count);
                offset += count;
              }
            }
            else
            {
              meshEntry.Mesh.SetNormals(buffer, 0, meshDstOffset, dstIndex);
            }
          });
        },
        (SendDataType dataType, BinaryReader reader, uint offset, uint count) =>
        {
          return ReadColourComponent(reader, offset, count, meshEntry.Mesh);
        }
      );

      if (readComponent == -1)
      {
        return new Error(ErrorCode.MalformedMessage, DataMessage.MessageID);
      }

      return new Error();
    }

    delegate void WriteToMesh(Vector3[] buffer, int count);
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
          writer(_v3Buffer, dstIndex);
          meshDstOffset += dstIndex;
          dstIndex = 0;
        }
      }

      if (dstIndex > 0)
      {
        // Flush buffer.
        writer(_v3Buffer, dstIndex);
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
          mesh.SetIndices(_uintBuffer, 0, meshDstOffset, dstIndex);
          meshDstOffset += dstIndex;
          dstIndex = 0;
        }
      }

      if (dstIndex > 0)
      {
        // Flush buffer.
        mesh.SetIndices(_uintBuffer, 0, meshDstOffset, dstIndex);
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
    protected override Error PostHandleMessage(DestroyMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      ResetObject(cache, shapeIndex);
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
        mat = new Material(Materials[MaterialLibrary.PointsUnlit]);
        int pointSize = (Materials != null) ? Materials.DefaultPointSize : 4;
        mat.SetInt("_PointSize", pointSize);
        mat.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        break;
      case MeshDrawType.Voxels:
        mat = new Material(Materials[MaterialLibrary.Voxels]);
        break;
      default:
      case MeshDrawType.Lines:
        mat = new Material(Materials[MaterialLibrary.VertexColourUnlit]);
        break;
      case MeshDrawType.Triangles:
        // Check wire frame.
        if ((shape.Flags & (uint)ObjectFlag.Wireframe) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.WireframeTriangles]);
        }
        else if ((shape.Flags & (uint)shape.TwoSided) != 0u)
        {
          if (meshData.CalculateNormals)
          {
            mat = new Material(Materials[MaterialLibrary.VertexColourLitTwoSided]);
          }
          else
          {
            mat = new Material(Materials[MaterialLibrary.VertexColourUnlitTwoSided]);
          }
          if (mat.HasProperty("_BackColour"))
          {
            mat.SetColor("_BackColour", new Maths.Colour(shape.ObjectAttributes.Colour).ToUnity32());
          }
        }
        else if (meshData.CalculateNormals)
        {
          mat = new Material(Materials[MaterialLibrary.VertexColourLit]);
        }
        else
        {
          mat = new Material(Materials[MaterialLibrary.VertexColourUnlit]);
        }
        break;
      }

      if (mat.HasProperty("_Colour"))
      {
        mat.SetColor("_Color", new Maths.Colour(shape.ObjectAttributes.Colour).ToUnity32());
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
    private Tes.Math.Vector3[] _v3Buffer = new Tes.Math.Vector3[s_bufferChuckSize];
    private Tes.Math.Vector2[] _v2Buffer = new Tes.Math.Vector2[s_bufferChuckSize];
    private int[] _intBuffer = new int[s_bufferChuckSize];
    private uint[] _uintBuffer = new uint[s_bufferChuckSize];
    private List<RenderMesh> _toCalculateNormals = new List<RenderMesh>();
  }
}
