using Tes.Net;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles box shapes.
  /// </summary>
  public class BoxHandler : ShapeHandler
  {
    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public BoxHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      SolidMesh = Tes.Tessellate.Box.Solid();
      WireframeMesh = Tes.Tessellate.Box.Wireframe();
    }

    public override void Initialise(GameObject root, GameObject serverRoot, Runtime.MaterialLibrary materials)
    {
      base.Initialise(root, serverRoot, materials);

      // GameObject box = new GameObject();
      // box.transform.position = new Vector3(2, 0, 0);
      // MeshRenderer renderer = box.AddComponent<MeshRenderer>();
      // MeshFilter mesh = box.AddComponent<MeshFilter>();
      // mesh.mesh = SolidMesh;
      // renderer.material = materials[Runtime.MaterialLibrary.OpaqueInstanced];
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Box"; } }

    /// <summary>
    /// <see cref="ShapeID.Box"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Box; } }
  }
}
