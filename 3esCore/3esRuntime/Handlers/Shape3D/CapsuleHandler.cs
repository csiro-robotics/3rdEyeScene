using Tes.Net;
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
      if (Root != null)
      {
        Root.name = Name;
      }
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Capsule"; } }

    /// <summary>
    /// <see cref="ShapeID.Capsule"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)ShapeID.Capsule; } }

    /// <summary>
    /// Solid mesh representation.
    /// </summary>
    public override Mesh SolidMesh { get { return _solidMeshes[Tes.Tessellate.Capsule.CylinderIndex]; } }
    /// <summary>
    /// Wireframe mesh representation.
    /// </summary>
    public override Mesh WireframeMesh { get { return _wireframeMeshes[Tes.Tessellate.Capsule.CylinderIndex]; } }

    /// <summary>
    /// Override to create an object per capsule part: top, bottom and cylinder.
    /// </summary>
    /// <returns>The capsule object.</returns>
    protected override GameObject CreateObject()
    {
      GameObject obj = new GameObject();
      obj.AddComponent<ShapeComponent>();

      // Top must be first to line up with mesh indexing.
      GameObject part = new GameObject();
      part.name = "top";
      part.AddComponent<MeshFilter>();
      part.AddComponent<MeshRenderer>();
      part.transform.SetParent(obj.transform, false);

      // Bottom must be second to line up with mesh indexing.
      part = new GameObject();
      part.name = "bottom";
      part.AddComponent<MeshFilter>();
      part.AddComponent<MeshRenderer>();
      part.transform.SetParent(obj.transform, false);

      // Walls must be third to line up with mesh indexing.
      part = new GameObject();
      part.name = "cylinder";
      part.AddComponent<MeshFilter>();
      part.AddComponent<MeshRenderer>();
      part.transform.SetParent(obj.transform, false);
      return obj;
    }

    /// <summary>
    /// Initialise the visual components (e.g., meshes) for <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">The object to initialise visuals for.</param>
    /// <param name="colour">Primary rendering colour.</param>
    /// <remarks>
    /// Requires special handling as the capsule is made up of several components.
    /// </remarks>
    protected override void InitialiseVisual(ShapeComponent obj, Color colour)
    {
      Mesh[] meshes = (!obj.Wireframe) ? _solidMeshes : _wireframeMeshes;
      InitialiseMesh(obj, meshes, colour);
    }

    /// <summary>
    /// Override to decode ScaleX as radius and ScaleZ as length.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, Transform transform, ObjectFlag flags)
    {
      float radius = attributes.ScaleX;
      float length = attributes.ScaleZ;
      float cylinderLength = Mathf.Max(0.0f, length - 2.0f * radius);
      if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Position) != 0)
      {
        transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      }
      if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Rotation) != 0)
      {
        transform.localRotation = new Quaternion(attributes.RotationX, attributes.RotationY, attributes.RotationZ, attributes.RotationW);
      }

      // Apply radius and length to sub components. Also move sphere caps to match the length.
      Transform child;
      child = transform.GetChild(Tes.Tessellate.Capsule.TopIndex);
      if (child != null)
      {
        if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Position) != 0)
        {
          child.localPosition = cylinderLength * 0.5f * Tessellate.Capsule.PrimaryAxis;
        }
        if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Scale) != 0)
        {
          child.localScale = new Vector3(radius, radius, radius);
        }
      }
      child = transform.GetChild(Tes.Tessellate.Capsule.BottomIndex);
      if (child != null)
      {
        if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Position) != 0)
        {
          child.localPosition = cylinderLength * -0.5f * Tessellate.Capsule.PrimaryAxis;
        }
        if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Scale) != 0)
        {
          child.localScale = new Vector3(radius, radius, radius);
        }
      }
      child = transform.GetChild(Tes.Tessellate.Capsule.CylinderIndex);
      if (child != null)
      {
        if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Scale) != 0)
        {
          child.localScale = new Vector3(radius, radius, cylinderLength);
        }
      }
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
      Transform child;
      child = transform.GetChild(Tes.Tessellate.Capsule.CylinderIndex);
      if (child)
      {
        attr.ScaleX = attr.ScaleY = child.localScale.x;
        attr.ScaleZ = child.localScale.y;
      }
      if (comp != null)
      {
        attr.Colour = ShapeComponent.ConvertColour(comp.Colour);
      }
      else
      {
        attr.Colour = 0xffffffu;
      }
    }

    /// <summary>
    /// Creates an capsule shape for serialisation.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      Shapes.Shape shape = new Shapes.Capsule();
      ConfigureShape(shape, shapeComponent);
      return shape;
    }

    private Mesh[] _solidMeshes;
    private Mesh[] _wireframeMeshes;
  }
}
