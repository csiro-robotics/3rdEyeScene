using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles Cone shapes.
  /// </summary>
  public class ConeHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public ConeHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _solidMesh = Tes.Tessellate.Cone.Solid();
      _wireframeMesh = Tes.Tessellate.Cone.Wireframe();
      if (Root != null)
      {
        Root.name = Name;
      }
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Cone"; } }

    /// <summary>
    /// <see cref="ShapeID.Cone"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Cone; } }

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
    protected override void DecodeTransform(ObjectAttributes attributes, Transform transform, ushort flags)
    {
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Position) != 0)
      {
        transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      }
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Rotation) != 0)
      {
        transform.localRotation = new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW);
      }
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Scale) != 0)
      {
        //Vector3 scaleVector = new Vector3(attributes.ScaleX, attributes.ScaleY, attributes.ScaleZ);
        transform.localScale = new Vector3(attributes.ScaleX, attributes.ScaleY, attributes.ScaleZ);
      }
    }

    /// <summary>
    /// Overridden to handle component pieces.
    /// </summary>
    protected override void EncodeAttributes(ref ObjectAttributes attr, GameObject obj, ShapeComponent comp)
    {
      Transform transform = obj.transform;
      attr.Colour = ShapeComponent.ConvertColour(comp.Colour);
      attr.X = transform.localPosition.x;
      attr.Y = transform.localPosition.y;
      attr.Z = transform.localPosition.z;
      attr.RotationX = transform.localRotation.x;
      attr.RotationY = transform.localRotation.y;
      attr.RotationZ = transform.localRotation.z;
      attr.RotationW = transform.localRotation.w;
      attr.ScaleX = transform.localScale.x;
      attr.ScaleY = transform.localScale.y;
      attr.ScaleZ = transform.localScale.z;
      //attr.ScaleX = attr.ScaleY = transform.localScale.x;
      //// Unity uses Y up;
      //attr.ScaleZ = transform.localScale.y;
    }

    /// <summary>
    /// Creates an cone shape for serialisation.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      Shapes.Shape shape = new Shapes.Cone();
      ConfigureShape(shape, shapeComponent);
      return shape;
    }

    private Mesh _solidMesh;
    private Mesh _wireframeMesh;
  }
}
