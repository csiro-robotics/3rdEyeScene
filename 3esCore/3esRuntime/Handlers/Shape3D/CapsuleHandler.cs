using System.Collections.Generic;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles capsule shapes (cylinders with rounded, hemisphere end caps).
  /// </summary>
  public class CapsuleHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public CapsuleHandler()
    {
      _solidMeshes = Tessellate.Capsule.Solid();
      _wireframeMeshes = Tessellate.Capsule.Wireframe();
      // The following two lines aren't really needed, but ensure we don't have null mesh properties.
      SolidMesh = _solidMeshes[0];
      WireframeMesh = _wireframeMeshes[0];
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Capsule"; } }

    /// <summary>
    /// <see cref="ShapeID.Capsule"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)ShapeID.Capsule; } }


    protected override void RenderInstances(CameraContext cameraContext, CommandBuffer renderQueue, Mesh mesh,
                                            List<Matrix4x4> transforms, List<CreateMessage> shapes,
                                            Material material)
    {
      // Work out which mesh set we are rendering from the parent call: solid or wireframe. We could also look at
      // the first CreateMessage flags.
      Mesh[] meshes = (mesh == SolidMesh)  ? _solidMeshes : _wireframeMeshes;
      CategoriesState categories = this.CategoriesState;

      // Handle instancing block size limits.
      for (int i = 0; i < transforms.Count; i += _instanceTransforms.Length)
      {
        MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
        int itemCount = 0;
        _instanceColours.Clear();
        for (int j = 0; j + i < transforms.Count; ++j)
        {
          if (categories != null && !categories.IsActive(shapes[i + j].Category))
          {
            continue;
          }

          // Build the end cap transforms.
          Matrix4x4 transform = transforms[i + j];
          _instanceTransforms[itemCount] = cameraContext.TesSceneToWorldTransform * transform;

          // Extract radius and length to position the end caps.
          float radius = transform.GetColumn(0).magnitude;
          Vector4 zAxis = transform.GetColumn(2);
          float length = zAxis.magnitude;
          zAxis *= 1.0f / (length != 0 ? length : 1.0f);
          // Scale the length axis to match the other two as the end caps are spheres.
          transform.SetColumn(2, zAxis * radius);

          // Adjust position for the first end cap.
          Vector4 tAxis = transform.GetColumn(3);
          tAxis += -0.5f * length * zAxis;
          transform.SetColumn(3, tAxis);
          _cap1Transforms[j] = cameraContext.TesSceneToWorldTransform * transform;

          // Adjust position for the second end cap.
          tAxis += length * zAxis;
          transform.SetColumn(3, tAxis);
          _cap2Transforms[j] = cameraContext.TesSceneToWorldTransform * transform;

          Maths.Colour colour = new Maths.Colour(shapes[i + j].Attributes.Colour);
          colour.A = 64;
          _instanceColours.Add(Maths.ColourExt.ToUnityVector4(colour));
          ++itemCount;
        }

        if (itemCount > 0)
        {
          materialProperties.SetVectorArray("_Color", _instanceColours);
          // Render body.
          renderQueue.DrawMeshInstanced(meshes[0], 0, material, 0, _instanceTransforms, itemCount, materialProperties);
          // Render end caps.
          renderQueue.DrawMeshInstanced(meshes[1], 0, material, 0, _cap1Transforms, itemCount, materialProperties);
          renderQueue.DrawMeshInstanced(meshes[2], 0, material, 0, _cap2Transforms, itemCount, materialProperties);
        }
      }
    }

    /// <summary>
    /// Override to decode ScaleX as radius and ScaleZ as length.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, out Matrix4x4 transform)
    {
      float radius = attributes.ScaleX;
      float length = attributes.ScaleZ;
      float cylinderLength = Mathf.Max(0.0f, length - 2.0f * radius);

      transform = Matrix4x4.identity;

      Vector3 scale = new Vector3(radius, radius, cylinderLength);
      transform.SetColumn(3, new Vector4(attributes.X, attributes.Y, attributes.Z, 1.0f));
      var pureRotation = Matrix4x4.Rotate(new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW));
      transform.SetColumn(0, pureRotation.GetColumn(0) * radius);
      transform.SetColumn(1, pureRotation.GetColumn(1) * radius);
      transform.SetColumn(2, pureRotation.GetColumn(2) * cylinderLength);
    }

    private Mesh[] _solidMeshes;
    private Mesh[] _wireframeMeshes;
    private Matrix4x4[] _cap1Transforms = new Matrix4x4[InstanceRenderLimit];
    private Matrix4x4[] _cap2Transforms = new Matrix4x4[InstanceRenderLimit];
  }
}
