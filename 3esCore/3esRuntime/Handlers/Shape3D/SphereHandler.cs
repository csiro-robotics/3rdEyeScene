using UnityEngine;
using Tes.Net;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles sphere shapes.
  /// </summary>
  public class SphereHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public SphereHandler()
    {
      SolidMesh = Tes.Tessellate.Sphere.Solid();
      WireframeMesh = Tes.Tessellate.Sphere.Wireframe();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Sphere"; } }

    /// <summary>
    /// <see cref="ShapeID.Sphere"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Sphere; } }

    /// <summary>
    /// Override to ensure uniform scaling.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, out Matrix4x4 transform)
    {
      transform = Matrix4x4.identity;

      float radius = attributes.ScaleX;
      transform.SetColumn(3, new Vector4(attributes.X, attributes.Y, attributes.Z, 1.0f));
      var pureRotation = Matrix4x4.Rotate(new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW));
      transform.SetColumn(0, pureRotation.GetColumn(0) * radius);
      transform.SetColumn(1, pureRotation.GetColumn(1) * radius);
      transform.SetColumn(2, pureRotation.GetColumn(2) * radius);
    }
  }
}
