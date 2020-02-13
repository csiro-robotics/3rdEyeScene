using System.Collections.Generic;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

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
    /// <param name="categoryCheck"></param>
    public CapsuleHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
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

    public override void Render(ulong categoryMask, Matrix4x4 primaryCameraTransform)
    {
      // TODO: (KS) Reduce duplication with base ShapeHandler.Render(ulong) method.
      // TODO: (KS) Address the same issues as ShapeHandler.Render(ulong).
      // TODO: (KS) the scaling applied to
      int itemCount = 0;
      _solidTransforms.Clear();
      _transparentTransforms.Clear();
      _wireframeTransforms.Clear();
      _transientCache.CollectTransforms(_solidTransforms, _transparentTransforms, _wireframeTransforms, categoryMask);
      if (_solidTransforms.Count > 0)
      {
        Render(_solidMeshes, Materials[MaterialLibrary.Opaque], _solidTransforms, itemCount);
      }
      if (_transparentTransforms.Count > 0)
      {
        Render(_solidMeshes, Materials[MaterialLibrary.Transparent], _transparentTransforms, itemCount);
      }
      if (_wireframeTransforms.Count > 0)
      {
        Render(_wireframeMeshes, Materials[MaterialLibrary.Wireframe], _wireframeTransforms, itemCount);
      }

      _solidTransforms.Clear();
      _transparentTransforms.Clear();
      _wireframeTransforms.Clear();
       _shapeCache.CollectTransforms(_solidTransforms, _transparentTransforms, _wireframeTransforms, categoryMask);
      if (_solidTransforms.Count > 0)
      {
        Render(_solidMeshes, Materials[MaterialLibrary.Opaque], _solidTransforms, itemCount);
      }
      if (_transparentTransforms.Count > 0)
      {
        Render(_solidMeshes, Materials[MaterialLibrary.Transparent], _transparentTransforms, itemCount);
      }
      if (_wireframeTransforms.Count > 0)
      {
        Render(_wireframeMeshes, Materials[MaterialLibrary.Wireframe], _wireframeTransforms, itemCount);
      }
    }

    protected void Render(Mesh[] meshes, Material material, List<Matrix4x4> transforms, int itemCount)
    {
      // Note: scaling must change while rendering. The cylinder must be scaled by radius (XY) and length (Z) while the
      // sphere end caps are only scale by radius applied as a uniform scale.
      // Axes XY will already share the radius scale so we only update Z scale.

      // Render with full scaling for the cylinder part.
      Graphics.DrawMeshInstanced(meshes[Tessellate.Capsule.CylinderIndex], 0, material, transforms.ToArray(), itemCount);

      _modifiedTransforms.Clear();
      if (_modifiedTransforms.Capacity < transforms.Capacity)
      {
        _modifiedTransforms.Capacity = transforms.Capacity;
      }

      // Convert to uniform scaling and modify the position for the bottom cap.
      for (int i = 0; i < itemCount; ++i)
      {
        Matrix4x4 transform = transforms[i];
        float radius = transform.GetColumn(0).magnitude;
        Vector4 zAxis = transform.GetColumn(2);
        float length = zAxis.magnitude;
        zAxis *= 1.0f / (length != 0 ? length : 1.0f);
        transform.SetColumn(2, zAxis * radius);
        // Adjust position.
        Vector4 tAxis = transform.GetColumn(3);
        tAxis += -0.5f * length * zAxis;
        transform.SetColumn(3, tAxis);
        _modifiedTransforms.Add(transform);
      }

      Graphics.DrawMeshInstanced(meshes[Tessellate.Capsule.BottomIndex], 0, material, _modifiedTransforms.ToArray(),
                                 itemCount);

      // Convert to uniform scaling and modify the position for the top cap.
      for (int i = 0; i < itemCount; ++i)
      {
        // Modify the bottom cap transform.
        Matrix4x4 transform = _modifiedTransforms[i];
        // Read the original zAxis for direction and length.
        Vector4 zAxis = transforms[i].GetColumn(2);
        // Adjust position.
        Vector4 tAxis = transform.GetColumn(3);
        tAxis += zAxis;
        transform.SetColumn(3, tAxis);
        _modifiedTransforms[i] = transform;
      }

      Graphics.DrawMeshInstanced(meshes[Tessellate.Capsule.TopIndex], 0, material, _modifiedTransforms.ToArray(),
                                 itemCount);
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
    private List<Matrix4x4> _modifiedTransforms = new List<Matrix4x4>();
  }
}
