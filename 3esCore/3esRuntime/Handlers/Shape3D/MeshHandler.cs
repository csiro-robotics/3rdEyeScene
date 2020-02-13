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
    public struct MeshEntry : IShapeData
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
      if (!maintainTransient)
      {
        foreach (int shapeIndex in _transientCache.ShapeIndices)
        {
          ResetObject(_transientCache, shapeIndex);
        }
      }

      for (int i = 0; i < _toCalculateNormals.Count; ++i)
      {
        _toCalculateNormals[i].CalculateNormals();
      }

      _toCalculateNormals.Clear();

      base.BeginFrame(frameNumber, maintainTransient);
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

    protected void RenderObject(ShapeCache cache, int shapeIndex)
    {
      CreateMessage shape = cache.GetShapeByIndex(shapeIndex);
      MeshEntry meshEntry = cache.GetShapeDataByIndex<MeshEntry>(shapeIndex);
      Matrix4x4 transform = cache.GetShapeTransformByIndex(shapeIndex);

      // Set buffers.
      GL.PushMatrix();
      GL.MultMatrix(transform);

      try
      {
        Material material = meshEntry.Material;
        RenderMesh mesh = meshEntry.Mesh;

        material.SetPass(0);

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
          material.SetColor("_Color", Maths.ColourExt.ToUnity(new Maths.Colour(shape.Attributes.Colour)));
        }

        // Bind vertices and draw.
        material.SetBuffer("_Vertices", mesh.VertexBuffer);

        if (mesh.IndexBuffer != null)
        {
          Graphics.DrawProceduralNow(mesh.Topology, mesh.IndexBuffer, mesh.IndexCount, 1);
        }
        else
        {
          Graphics.DrawProceduralNow(mesh.Topology, mesh.VertexCount, 1);
        }
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
      }
      finally
      {
        GL.PopMatrix();
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

      Vector3[] vertices = new Vector3[mesh.VertexCount];
      int[] indices = (mesh.IndexCount > 0) ? new int[mesh.IndexCount] : null;

      mesh.GetVertices(vertices);
      if (indices != null)
      {
        mesh.GetIndices(indices);
      }

      Shapes.MeshShape meshShape = new Shapes.MeshShape(meshEntry.Mesh.DrawType,
                                                        Maths.Vector3Ext.FromUnity(vertices),
                                                        indices,
                                                        shapeData.ObjectID, shapeData.Category);
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
        CalculateNormals = (msg.Flags & (ushort)MeshShapeFlag.CalculateNormals) != 0
      };

      meshEntry.Material = CreateMaterial(msg, meshEntry);

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
            meshEntry.Mesh.SetVertices(buffer, 0, writeOffset, writeOffset);
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
        mat = new Material(Materials[MaterialLibrary.Points]);
        MaterialLibrary.SetupMaterial(mat, meshEntry.Mesh);
        int pointSize = (Materials != null) ? Materials.DefaultPointSize : 4;
        mat.SetInt("_PointSize", pointSize);
        mat.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        break;
      case MeshDrawType.Voxels:
        mat = new Material(Materials[MaterialLibrary.Voxels]);
        MaterialLibrary.SetupMaterial(mat, meshEntry.Mesh);
        mat.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        break;
      default:
      case MeshDrawType.Lines:
        mat = new Material(Materials[MaterialLibrary.Opaque]);
        if (meshEntry.Mesh.HasColours)
        {
          mat.EnableKeyword("WITH_COLOURS");
        }
        break;
      case MeshDrawType.Triangles:
        // Check wire frame.
        if ((shape.Flags & (uint)ObjectFlag.Wireframe) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.Wireframe]);
        }
        else if ((shape.Flags & (uint)ObjectFlag.Transparent) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.Transparent]);
        }
        else if ((shape.Flags & (uint)ObjectFlag.TwoSided) != 0u)
        {
          mat = new Material(Materials[MaterialLibrary.OpaqueTwoSided]);
        }
        else
        {
          mat = new Material(Materials[MaterialLibrary.Opaque]);
        }
        MaterialLibrary.SetupMaterial(mat, meshEntry.Mesh);
        break;
      }

      if (mat.HasProperty("_Colour"))
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
