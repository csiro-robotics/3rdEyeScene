using Tes.Net;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles Plane shapes.
  /// </summary>
  public class PlaneHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    public PlaneHandler()
    {
      SolidMesh = Tes.Tessellate.Plane.Solid();
      WireframeMesh = Tes.Tessellate.Plane.Wireframe();
      // Use the wireframe material to flat shade the star.
      SolidMaterialName = Runtime.MaterialLibrary.WireframeInstanced;
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Plane"; } }

    /// <summary>
    /// <see cref="ShapeID.Plane"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Plane; } }

    /// <summary>
    /// Override to interpret rotation as a normal direction.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, out Matrix4x4 transform)
    {
      transform = Matrix4x4.identity;

      // Rotation component is a normal vector. Need to generate a rotation.
      Vector3 up = Vector3.up;
      Vector3 normalDir = new Vector3(attributes.RotationX, attributes.RotationY, attributes.RotationZ);
      Quaternion rotation = Quaternion.identity;
      float dotProduct = Vector3.Dot(up, normalDir);
      if (dotProduct != -1.0f)
      {
        // Find the angle between the default normal direction and the given direction.
        float rotationAngle = Mathf.Cos(dotProduct);
        // Build a quaternion rotation for this deviation.
        rotation = Quaternion.AngleAxis(rotationAngle, Vector3.Cross(up * Mathf.PI / 360.0f, normalDir));
      }
      else
      {
        // Opposed vectors. Rotate 180 degrees.
        rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
      }

      transform.SetColumn(3, new Vector4(attributes.X, attributes.Y, attributes.Z, 1.0f));
      var pureRotation = Matrix4x4.Rotate(rotation);
      transform.SetColumn(0, pureRotation.GetColumn(0) * attributes.ScaleX);
      transform.SetColumn(1, pureRotation.GetColumn(1) * attributes.ScaleX);
      transform.SetColumn(2, pureRotation.GetColumn(2) * attributes.ScaleZ);
    }
  }
}
