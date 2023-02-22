using Tes.Net;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles Arrow shapes.
  /// </summary>
  public class ArrowHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public ArrowHandler()
    {
      SolidMesh = Tes.Tessellate.Arrow.Solid();
      WireframeMesh = Tes.Tessellate.Arrow.Wireframe();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Arrow"; } }

    /// <summary>
    /// <see cref="ShapeID.Arrow"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Arrow; } }

    /// <summary>
    /// Override to decode ScaleX as radius and ScaleZ as length.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, out Matrix4x4 transform)
    {
      float radius = (float)attributes.ScaleX;
      float length = (float)attributes.ScaleZ;
      transform = Matrix4x4.identity;

      transform.SetColumn(3, new Vector4((float)attributes.X, (float)attributes.Y, (float)attributes.Z, 1.0f));
      var pureRotation = Matrix4x4.Rotate(new Quaternion((float)attributes.RotationX, (float)attributes.RotationY,
                                                         (float)attributes.RotationZ, (float)attributes.RotationW));
      transform.SetColumn(0, pureRotation.GetColumn(0) * radius);
      transform.SetColumn(1, pureRotation.GetColumn(1) * radius);
      transform.SetColumn(2, pureRotation.GetColumn(2) * length);
    }
  }
}
