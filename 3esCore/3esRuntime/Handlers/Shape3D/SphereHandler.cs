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
    /// <param name="categoryCheck"></param>
    public SphereHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _solidMesh = Tes.Tessellate.Sphere.Solid();
      _wireframeMesh = Tes.Tessellate.Sphere.Wireframe();
      if (Root != null)
      {
        Root.name = Name;
      }
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
    /// Solid mesh representation.
    /// </summary>
    public override Mesh SolidMesh { get { return _solidMesh; } }
    /// <summary>
    /// Wireframe mesh representation.
    /// </summary>
    public override Mesh WireframeMesh { get { return _wireframeMesh; } }

    /// <summary>
    /// Override to ensure uniform scaling.
    /// </summary>
    protected override void DecodeTransform(ObjectAttributes attributes, Transform transform, ushort flags)
    {
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Position) != 0)
      {
        transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      }
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Rotation) != 0)
      {
        transform.localRotation = Quaternion.identity; // Irrelevant for spheres.
      }
      if ((flags & (ushort)UpdateFlag.UpdateMode) == 0 || (flags & (ushort)UpdateFlag.Scale) != 0)
      {
        transform.localScale = new Vector3(attributes.ScaleX, attributes.ScaleY, attributes.ScaleZ);
      }
    }

    /// <summary>
    /// Creates a sphere shape for serialisation.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      Shapes.Shape shape = new Shapes.Sphere();
      ConfigureShape(shape, shapeComponent);
      return shape;
    }

    private Mesh _solidMesh;
    private Mesh _wireframeMesh;
  }
}
