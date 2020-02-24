using System.Collections.Generic;
using Tes;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles box shapes.
  /// </summary>
  public class PoseHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public PoseHandler()
    {
      _solidMeshes[0] = new Mesh();
      Tessellate.Arrow.Solid(_solidMeshes[0], 8, 0.1f, 0.05f, 0.81f, 1.0f);
      TransformAndColour(_solidMeshes[0], 0, new Color32(255, 0, 0, 255));
      _solidMeshes[1] = new Mesh();
      Tessellate.Arrow.Solid(_solidMeshes[1], 8, 0.1f, 0.05f, 0.81f, 1.0f);
      TransformAndColour(_solidMeshes[1], 1, new Color32(0, 255, 0, 255));
      _solidMeshes[2] = new Mesh();
      Tessellate.Arrow.Solid(_solidMeshes[2], 8, 0.1f, 0.05f, 0.81f, 1.0f);
      TransformAndColour(_solidMeshes[2], 2, new Color32(0, 0, 255, 255));

      // Should probably look into how sub-meshes work for this.
      _wireframeMeshes[0] = MakeLine(new Vector3(1, 0, 0), new Color32(255, 0, 0, 255));
      _wireframeMeshes[1] = MakeLine(new Vector3(0, 1, 0), new Color32(0, 255, 0, 255));
      _wireframeMeshes[2] = MakeLine(new Vector3(0, 0, 1), new Color32(0, 0, 255, 255));

      SolidMesh = _solidMeshes[0];
      WireframeMesh = _wireframeMeshes[0];
    }

    public override void Initialise(GameObject root, GameObject serverRoot, Runtime.MaterialLibrary materials)
    {
      base.Initialise(root, serverRoot, materials);
    }

    protected override void RenderInstances(CameraContext cameraContext, CommandBuffer renderQueue, Mesh mesh,
                                            List<Matrix4x4> transforms, List<Matrix4x4> parentTransforms,
                                            List<CreateMessage> shapes, Material material)
    {
      // Work out which mesh set we are rendering from the parent call: solid or wireframe. We could also look at
      // the first CreateMessage flags.
      Mesh[] meshes =
        (shapes.Count > 0 && (shapes[0].Flags & (ushort)ObjectFlag.Wireframe) != 0)  ? _wireframeMeshes : _solidMeshes;
      CategoriesState categories = this.CategoriesState;

      // Handle instancing block size limits.
      for (int i = 0; i < transforms.Count; i += _instanceTransforms.Length)
      {
        MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
        int itemCount = 0;
        _instanceColours.Clear();
        for (int j = 0; j < _instanceTransforms.Length && j + i < transforms.Count; ++j)
        {
          if (categories != null && !categories.IsActive(shapes[i + j].Category))
          {
            continue;
          }

          // Build the end cap transforms.
          Matrix4x4 modelToSceneTransform = cameraContext.TesSceneToWorldTransform * parentTransforms[i + j];
          Matrix4x4 transform = transforms[i + j];

          // Work out the scaling. We only want to lengthen each line.
          Vector3 scale = Vector3.zero;
          scale.x = transform.GetColumn(0).magnitude;
          scale.y = transform.GetColumn(1).magnitude;
          scale.z = transform.GetColumn(2).magnitude;

          // Remove scaling.
          transform.SetColumn(0, transform.GetColumn(0) / scale.x);
          transform.SetColumn(1, transform.GetColumn(1) / scale.y);
          transform.SetColumn(2, transform.GetColumn(2) / scale.z);

          // Scale each mesh.
          Matrix4x4 transform2 = transform;
          transform2.SetColumn(0, transform2.GetColumn(0) * scale.x);
          _instanceTransforms[itemCount] = modelToSceneTransform * transform2;

          transform2 = transform;
          transform2.SetColumn(1, transform2.GetColumn(1) * scale.y);
          _axis1Transforms[itemCount] = modelToSceneTransform * transform2;

          transform2 = transform;
          transform2.SetColumn(2, transform2.GetColumn(2) * scale.z);
          _axis2Transforms[itemCount] = modelToSceneTransform * transform2;

          Maths.Colour colour = new Maths.Colour(shapes[i + j].Attributes.Colour);
          _instanceColours.Add(Maths.ColourExt.ToUnityVector4(colour));
          ++itemCount;
        }

        if (itemCount > 0)
        {
          materialProperties.SetVectorArray("_Color", _instanceColours);
          // Render body.
          renderQueue.DrawMeshInstanced(meshes[0], 0, material, 0, _instanceTransforms, itemCount, materialProperties);
          // Render end caps.
          renderQueue.DrawMeshInstanced(meshes[1], 0, material, 0, _axis1Transforms, itemCount, materialProperties);
          renderQueue.DrawMeshInstanced(meshes[2], 0, material, 0, _axis2Transforms, itemCount, materialProperties);
        }
      }
    }

    private void TransformAndColour(Mesh mesh, int axis, Color32 colour)
    {
      // Assumes default arrow direction is (0, 0, 1)
      Quaternion rotation = Quaternion.identity;
      switch (axis)
      {
      case 0:
        // Rotate 90 around Y.
        rotation = Quaternion.AngleAxis(90.0f, new Vector3(0, 1, 0));
        break;
      case 1:
        // Rotate 90 around X.
        rotation = Quaternion.AngleAxis(-90.0f, new Vector3(1, 0, 0));
        break;
      case 2:
      default:
        // No change.
        return;
      }

      Vector3[] vertices = mesh.vertices;
      for (int i = 0; i < vertices.Length; ++i)
      {
        vertices[i] = rotation * vertices[i];
      }
      mesh.vertices = vertices;
      if (mesh.normals != null)
      {
        vertices = mesh.normals;
        for (int i = 0; i < vertices.Length; ++i)
        {
          vertices[i] = rotation * vertices[i];
        }
        mesh.normals = vertices;
      }

      Color32[] colours = new Color32[mesh.vertices.Length];
      for (int i = 0; i < colours.Length; ++i)
      {
        colours[i] = colour;
      }
      mesh.colors32 = colours;
    }

    private Mesh MakeLine(Vector3 vertex, Color32 colour)
    {
      Mesh mesh = new Mesh();
      mesh.vertices = new Vector3[] { Vector3.zero, vertex };
      mesh.colors32 = new Color32[] { colour, colour };
      mesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
      return mesh;
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Pose"; } }

    /// <summary>
    /// <see cref="ShapeID.Pose"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Pose; } }

    private Mesh[] _solidMeshes = new Mesh[3];
    private Mesh[] _wireframeMeshes = new Mesh[3];
    private Matrix4x4[] _axis1Transforms = new Matrix4x4[InstanceRenderLimit];
    private Matrix4x4[] _axis2Transforms = new Matrix4x4[InstanceRenderLimit];
  }
}
