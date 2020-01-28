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
