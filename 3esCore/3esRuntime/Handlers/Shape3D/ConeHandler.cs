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
    public ConeHandler()
    {
      SolidMesh = Tes.Tessellate.Cone.Solid();
      WireframeMesh = Tes.Tessellate.Cone.Wireframe();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Cone"; } }

    /// <summary>
    /// <see cref="ShapeID.Cone"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Cone; } }
  }
}
