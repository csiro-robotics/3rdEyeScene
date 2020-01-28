using Tes.Net;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles Cylinder shapes.
  /// </summary>
  public class CylinderHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public CylinderHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      SolidMesh = Tes.Tessellate.Cylinder.Solid();
      WireframeMesh = Tes.Tessellate.Cylinder.Wireframe();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Cylinder"; } }

    /// <summary>
    /// <see cref="ShapeID.Cylinder"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Cylinder; } }

    /// <summary>
    /// Override to decode ScaleX as radius and ScaleZ as length.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, out Matrix4x4 transform)
    {
      float radius = attributes.ScaleX;
      float length = attributes.ScaleZ;
      transform = Matrix4x4.identity;

      Vector3 scale = new Vector3(radius, radius, length);
      transform.SetColumn(3, new Vector4(attributes.X, attributes.Y, attributes.Z, 1.0f));
      var pureRotation = Matrix4x4.Rotate(new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW));
      transform.SetColumn(0, pureRotation.GetColumn(0) * radius);
      transform.SetColumn(1, pureRotation.GetColumn(1) * radius);
      transform.SetColumn(2, pureRotation.GetColumn(2) * length);
    }
  }
}
