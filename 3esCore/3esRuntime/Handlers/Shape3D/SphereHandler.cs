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
    protected override void DecodeTransform(ObjectAttributes attributes, Transform transform)
    {
      transform.localPosition = new Vector3(attributes.X, attributes.Y, attributes.Z);
      transform.localRotation = Quaternion.identity; // Irrelevant for spheres.
      transform.localScale = new Vector3(attributes.ScaleX, attributes.ScaleX, attributes.ScaleX);
    }

    private Mesh _solidMesh;
    private Mesh _wireframeMesh;
  }
}
