using Tes.Net;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles Star shapes.
  /// </summary>
  public class StarHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public StarHandler()
    {
      SolidMesh = Tes.Tessellate.Star.Solid();
      WireframeMesh = Tes.Tessellate.Star.Wireframe();
      // Use the wireframe material to flat shade the star.
      SolidMaterialName = Runtime.MaterialLibrary.WireframeInstanced;
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Star"; } }

    /// <summary>
    /// <see cref="ShapeID.Star"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Star; } }

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
