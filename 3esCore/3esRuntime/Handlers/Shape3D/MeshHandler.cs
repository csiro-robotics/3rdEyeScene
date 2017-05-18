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
  /// Shape handler for mesh shapes.
  /// </summary>
  /// <remarks>
  /// Mesh shapes represent pseudo immediate mode rendering of vertex data with optional indexing.
  /// </remarks>
  public class MeshHandler : ShapeHandler
  {
    /// <summary>
    /// Tracks support data for mesh shapes.
    /// </summary>
    public class MeshDataComponent : MonoBehaviour
    {
      /// <summary>
      /// Mesh vertices.
      /// </summary>
      public Vector3[] Vertices;
      /// <summary>
      /// Mesh normals.
      /// </summary>
      public Vector3[] Normals;
      /// <summary>
      /// Mesh indices. Sequential indices into <see cref="Vertices"/> when no explicit
      /// Indices are provided.
      /// </summary>
      public int[] Indices;
      /// <summary>
      /// Defines how to draw <see cref="Vertices"/> using <see cref="Indices"/>.
      /// </summary>
      public MeshDrawType DrawType;
      /// <summary>
      /// Do we need to calculate normals?
      /// </summary>
      /// <remarks>
      /// Requires <see cref="MeshDrawType.Triangles"/> and <see cref="MeshShapeFlag.CalculateNormals"/>.
      /// </remarks>
      public bool CalculateNormals;
    }

    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public MeshHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      if (Root != null)
      {
        Root.name = Name;
      }
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
    /// Irrelevant. Each object has its own geometry.
    /// </summary>
    public override Mesh SolidMesh { get { return null; } }
    /// <summary>
    /// Irrelevant. Each object has its own geometry.
    /// </summary>
    public override Mesh WireframeMesh { get { return null; } }

    /// <summary>
    /// Initialise, caching the <see cref="MaterialLibrary"/>.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="serverRoot"></param>
    /// <param name="materials"></param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      base.Initialise(root, serverRoot, materials);
    }

    /// <summary>
    /// Overridden to release mesh resources.
    /// </summary>
    public override void Reset()
    {
      // Clear out all the mesh data in our objects.
      foreach (GameObject obj in _transientCache.Objects)
      {
        ResetObject(obj);
      }

      foreach (GameObject obj in _shapeCache.Objects)
      {
        ResetObject(obj);
      }

      _awaitingFinalisation.Clear();
      base.Reset();
    }

    /// <summary>
    /// Finalises Unity mesh objects.
    /// </summary>
    public override void PreRender()
    {
      base.PreRender();
      MeshDataComponent meshData;
      ShapeComponent shape;
      Material mat;
      for (int i = 0; i < _awaitingFinalisation.Count; ++i)
      {
        meshData = _awaitingFinalisation[i];
        shape = meshData.GetComponent<ShapeComponent>();
        mat = SelectMaterial(shape, meshData);
        FinaliseMesh(meshData.gameObject, shape, meshData, mat, shape.Colour);
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
        foreach (GameObject obj in _transientCache.Objects)
        {
          ResetObject(obj);
        }
      }
      base.BeginFrame(frameNumber, maintainTransient);
    }

    /// <summary>
    /// Overridden to add <see cref="MeshDataComponent"/>.
    /// </summary>
    /// <returns>A new object supporting the mesh shape.</returns>
    protected override GameObject CreateObject()
    {
      GameObject obj = base.CreateObject();
      obj.AddComponent<MeshDataComponent>();
      return obj;
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
    protected override Error PostHandleMessage(GameObject obj, CreateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      uint vertexCount = reader.ReadUInt32();
      uint indexCount = reader.ReadUInt32();

      MeshDataComponent meshData = obj.GetComponent<MeshDataComponent>();

      meshData.Vertices = new Vector3[vertexCount];
      meshData.Indices = new int[indexCount];
      meshData.Normals = null;
      meshData.DrawType = (MeshDrawType)reader.ReadByte();
      meshData.CalculateNormals = meshData.DrawType == MeshDrawType.Triangles &&
                                  (msg.Flags & (ushort)MeshShapeFlag.CalculateNormals) != 0;

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
      GameObject obj = null;
      if (msg.ObjectID == 0)
      {
        // Transient object.
        obj = _transientCache.LastObject;
        if (!obj)
        {
          return new Error(ErrorCode.InvalidObjectID, 0);
        }
      }
      else
      {
        obj = FindObject(msg.ObjectID);
        if (!obj)
        {
          // Object already exists.
          return new Error(ErrorCode.InvalidObjectID, msg.ObjectID);
        }
      }

      ushort receiveType = 0;
      uint offset = 0;
      uint itemCount = 0;

      // Read what we are receiving, offset and item count.
      receiveType = reader.ReadUInt16();
      offset = reader.ReadUInt32();
      itemCount = reader.ReadUInt32();

      // Naive support for multiple packets. Assume:
      // - In order.
      // - Under the overall Unity mesh indexing limit.
      MeshDataComponent meshData = obj.GetComponent<MeshDataComponent>();

      if (receiveType == (ushort)Shapes.MeshShape.SendDataType.Normals ||
          receiveType == (ushort)Shapes.MeshShape.SendDataType.UniformNormal)  // Normals incoming
      {
        Vector3[] normals = meshData.Normals;

        if (normals == null || normals.Length < meshData.Vertices.Length)
        {
          normals = meshData.Normals = new Vector3[meshData.Vertices.Length];
        }

        // Read new normals.
        Vector3 n = Vector3.zero;
        if (receiveType == (ushort)Shapes.MeshShape.SendDataType.Normals)
        {
          for (int i = 0; i < itemCount; ++i)
          {
            n.x = reader.ReadSingle();
            n.y = reader.ReadSingle();
            n.z = reader.ReadSingle();
            normals[offset + i] = n;
          }
        }
        else
        {
          // Single, shared normals. Expand into the array.
          n.x = reader.ReadSingle();
          n.y = reader.ReadSingle();
          n.z = reader.ReadSingle();

          for (int i = 0; i < normals.Length; ++i)
          {
            normals[i] = n;
          }
        }

        // Can't finalise on normals.
      }
      else if (receiveType == (ushort)Shapes.MeshShape.SendDataType.Vertices) // Vertices incoming
      {
        Vector3[] vertices = meshData.Vertices;

        // Read new vertices.
        Vector3 v = Vector3.zero;
        for (int i = 0; i < itemCount; ++i)
        {
          v.x = reader.ReadSingle();
          v.y = reader.ReadSingle();
          v.z = reader.ReadSingle();
          vertices[offset + i] = v;
        }

        if (offset + itemCount == vertices.Length && meshData.Indices.Length == 0)
        {
          // Last vertices and expecting no indices.
          _awaitingFinalisation.Add(meshData);
        }
      }
      else if (receiveType == (ushort)Shapes.MeshShape.SendDataType.Indices)  // Indices incoming
      {
        // Receiving indices.
        int[] indices = meshData.Indices;

        // Read new indices.
        for (int i = 0; i < itemCount; ++i)
        {
          indices[offset + i] = reader.ReadInt32();
        }

        if (offset + itemCount == indices.Length)
        {
          // Last indices.
          _awaitingFinalisation.Add(meshData);
        }
      }
      else
      {
        return new Error(ErrorCode.MalformedMessage);
      }

      return new Error();
    }

    /// <summary>
    /// Overridden to clear mesh data and release resources even for transient objects.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(GameObject obj, DestroyMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      ResetObject(obj);
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
    Material SelectMaterial(ShapeComponent shape, MeshDataComponent meshData)
    {
      Material mat;
      switch (meshData.DrawType)
      {
      case MeshDrawType.Points:
        mat = Materials[MaterialLibrary.PointsUnlit];
        break;
      case MeshDrawType.Voxels:
        mat = Materials[MaterialLibrary.Voxels];
        break;
      default:
      case MeshDrawType.Lines:
        mat = Materials[MaterialLibrary.VertexColourUnlit];
        break;
      case MeshDrawType.Triangles:
        // Check wire frame.
        if (shape != null && shape.Wireframe)
        {
          mat = Materials[MaterialLibrary.WireframeTriangles];
        }
        else if (shape != null && shape.TwoSided)
        {
          if (meshData.CalculateNormals)
          {
            mat = Materials[MaterialLibrary.VertexColourLitTwoSided];
          }
          else
          {
            mat = Materials[MaterialLibrary.VertexColourUnlitTwoSided];
          }
        }
        else if (meshData.CalculateNormals)
        {
          mat = Materials[MaterialLibrary.VertexColourLit];
        }
        else
        {
          mat = Materials[MaterialLibrary.VertexColourUnlit];
        }
        break;
      }

      return mat;
    }

    /// <summary>
    /// Release the mesh resources of <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj"></param>
    void ResetObject(GameObject obj)
    {
      MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
      if (meshFilter != null)
      {
        meshFilter.mesh = null;
      }
      MeshDataComponent meshData = obj.GetComponent<MeshDataComponent>();
      meshData.Vertices = null;
      meshData.Indices = null;

      // Destroy children.
      for (int i = obj.transform.childCount - 1; i >= 0; --i)
      {
        GameObject.Destroy(obj.transform.GetChild(i).gameObject);
      }

      _awaitingFinalisation.RemoveAll((MeshDataComponent cmp) => { return cmp == meshData; });
    }

    /// <summary>
    /// Finalises the mesh object an child objects.
    /// </summary>
    /// <param name="obj">Game object to set the mesh on or create children on.</param>
    /// <param name="shape">The <see cref="ShapeComponent"/> belonging to <paramref name="obj"/>.</param>
    /// <param name="meshData">Mesh vertex and index data.</param>
    /// <param name="material">Material to render with. Chosen based on topology.</param>
    /// <param name="colour">The mesh render colour.</param>
    protected void FinaliseMesh(GameObject obj, ShapeComponent shape, MeshDataComponent meshData, Material material, Color32 colour)
    {
      MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
      MeshTopology topology = MeshCache.DrawTypeToTopology(meshData.DrawType);
      bool haveNormals = (meshData.Normals != null && meshData.Vertices != null &&
                          meshData.Normals.Length == meshData.Vertices.Length);
      if (meshData.Vertices.Length < 65000)
      {
        if (meshFilter == null)
        {
          meshFilter = obj.AddComponent<MeshFilter>();
        }
        // Can do it with one object.
        Mesh mesh = new Mesh();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        mesh.subMeshCount = 1;
        mesh.vertices = meshData.Vertices;
        if (haveNormals)
        {
          mesh.normals = meshData.Normals;
        }
        else
        {
          mesh.normals = null;
        }

        if (meshData.Indices.Length > 0)
        {
          mesh.SetIndices(meshData.Indices, topology, 0);
        }
        else
        {
          // No explicit indices. Set sequential indexing.
          int[] indices = new int[meshData.Vertices.Length];
          for (int i = 0; i < indices.Length; ++i)
          {
            indices[i] = i;
          }
          mesh.SetIndices(indices, topology, 0);
        }

        MeshRenderer render = obj.GetComponent<MeshRenderer>();
        render.material = material;
        render.material.color = colour;
        if (shape.TwoSided && render.material.HasProperty("_BackColour"))
        {
          render.material.SetColor("_BackColour", colour);
        }
        if (meshData.DrawType == MeshDrawType.Points)
        {
          render.material.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
        }

        mesh.RecalculateBounds();
        if (meshData.CalculateNormals && !haveNormals)
        {
          //mesh.RecalculateNormals();
        }
      }
      else
      {
        // Need multiple objects.
        // Destroy the current mesh filter.
        if (meshFilter != null)
        {
          GameObject.Destroy(meshFilter);
        }

        // Create children.
        int elementIndices = MeshCache.TopologyIndexStep(MeshCache.DrawTypeToTopology(meshData.DrawType));
        // Calculate the number of vertices per mesh by truncation.
        int itemsPerMesh = (65000 / elementIndices) * elementIndices;
        int[] indices = meshData.Indices;

        if (indices.Length == 0)
        {
          // No explicit indices. Set sequential indexing.
          indices = new int[meshData.Vertices.Length];
          for (int i = 0; i < indices.Length; ++i)
          {
            indices[i] = i;
          }
        }

        // For now just duplicate vertex data.
        int indexOffset = 0;
        int partCount = 0;
        int elementCount;
        while (indexOffset < indices.Length)
        {
          ++partCount;
          GameObject part = new GameObject(string.Format("{0}{1:D2}", topology.ToString(), partCount));
          Mesh partMesh = part.AddComponent<MeshFilter>().mesh;
          MeshRenderer render = part.AddComponent<MeshRenderer>();
          render.material = material;
          render.material.color = colour;
          if (shape.TwoSided)
          {
            render.material.SetColor("_BackColour", colour);
          }
          if (meshData.DrawType == MeshDrawType.Points)
          {
            render.material.SetInt("_LeftHanded", ServerInfo.IsLeftHanded ? 1 : 0);
          }
          partMesh.subMeshCount = 1;
          elementCount = Math.Min(itemsPerMesh, indices.Length - indexOffset);

          Vector3[] partVerts = new Vector3[elementCount];
          Vector3[] partNorms = (haveNormals) ? new Vector3[elementCount] : null;
          int[] partInds = new int[elementCount];

          for (int i = 0; i < elementCount; ++i)
          {
            partVerts[i] = meshData.Vertices[indices[i + indexOffset]];
            if (partNorms != null)
            {
              partNorms[i] = meshData.Normals[indices[i + indexOffset]];
            }
            partInds[i] = i;
          }

          part.transform.SetParent(obj.transform, false);
          partMesh.vertices = partVerts;
          if (partNorms != null)
          {
            partMesh.normals = partNorms;
          }
          partMesh.SetIndices(partInds, topology, 0);
          partMesh.RecalculateBounds();
          if (meshData.CalculateNormals && !haveNormals)
          {
            //partMesh.RecalculateNormals();
          }

          indexOffset += elementCount;
        }
      }
    }

    private List<MeshDataComponent> _awaitingFinalisation = new List<MeshDataComponent>();
  }
}
