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
      _solidMesh = Tes.Tessellate.Cylinder.Solid();
      _wireframeMesh = Tes.Tessellate.Cylinder.Wireframe();
      if (Root != null)
      {
        Root.name = Name;
      }
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
    /// Solid mesh representation.
    /// </summary>
    public override Mesh SolidMesh { get { return _solidMesh; } }
    /// <summary>
    /// Wireframe mesh representation.
    /// </summary>
    public override Mesh WireframeMesh { get { return _wireframeMesh; } }

    /// <summary>
    /// Override to decode ScaleX as radius and ScaleZ as length.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, Transform transform)
    {
      float radius = attributes.ScaleX;
      float length = attributes.ScaleZ;
      transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      transform.localRotation = new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW);
      // Unity uses Y up;
      transform.localScale = new Vector3(radius, length, radius);
    }

    /// <summary>
    /// Overridden to handle component pieces.
    /// </summary>
    protected override void EncodeAttributes(ref ObjectAttributes attr, GameObject obj, ShapeComponent comp)
    {
      Transform transform = obj.transform;
      attr.X = transform.localPosition.x;
      attr.Y = transform.localPosition.y;
      attr.Z = transform.localPosition.z;
      attr.RotationX = transform.localRotation.x;
      attr.RotationY = transform.localRotation.y;
      attr.RotationZ = transform.localRotation.z;
      attr.RotationW = transform.localRotation.w;
      attr.ScaleX = attr.ScaleY = transform.localScale.x;
      // Unity uses Y up;
      attr.ScaleZ = transform.localScale.y;
    }

    private Mesh _solidMesh;
    private Mesh _wireframeMesh;
  }
}
