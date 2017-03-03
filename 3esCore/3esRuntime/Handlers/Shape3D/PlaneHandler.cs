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
    /// <param name="categoryCheck"></param>
    public PlaneHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _solidMesh = Tes.Tessellate.Plane.Solid();
      _wireframeMesh = Tes.Tessellate.Plane.Wireframe();
      if (Root != null)
      {
        Root.name = Name;
      }
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
    /// Solid mesh representation.
    /// </summary>
    public override Mesh SolidMesh { get { return _solidMesh; } }
    /// <summary>
    /// Wireframe mesh representation.
    /// </summary>
    public override Mesh WireframeMesh { get { return _wireframeMesh; } }

    /// <summary>
    /// Override to interpret rotation as a normal direction.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, Transform transform, ObjectFlag flags)
    {
      if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Position) != 0)
      {
        transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      }
      if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Scale) != 0)
      {
        transform.localScale = new Vector3(attributes.ScaleX, attributes.ScaleX, attributes.ScaleY);
      }

      // Interpret rotation as a normal direction.
      if ((flags & ObjectFlag.UpdateMode) == 0 || (flags & ObjectFlag.Rotation) != 0)
      {
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

        transform.localRotation = rotation;
      }
    }

    /// <summary>
    /// Creates a plane shape for serialisation.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      Shapes.Shape shape = new Shapes.Plane();
      ConfigureShape(shape, shapeComponent);
      return shape;
    }

    private Mesh _solidMesh;
    private Mesh _wireframeMesh;
  }
}
