using System;

namespace Tes.Net
{
  /// <summary>
  /// Routing IDs for basic shape handlers.
  /// </summary>
  public enum ShapeID : ushort
  {
    /// <summary>
    /// <see cref="Shapes.Sphere"/>
    /// </summary>
    Sphere = RoutingID.ShapeIDsStart,
    /// <summary>
    /// <see cref="Shapes.Box"/>
    /// </summary>
    Box,
    /// <summary>
    /// <see cref="Shapes.Cone"/>
    /// </summary>
    Cone,
    /// <summary>
    /// <see cref="Shapes.Cylinder"/>
    /// </summary>
    Cylinder,
    /// <summary>
    /// <see cref="Shapes.Capsule"/>
    /// </summary>
    Capsule,
    /// <summary>
    /// <see cref="Shapes.Plane"/>
    /// </summary>
    Plane,
    /// <summary>
    /// <see cref="Shapes.Star"/>
    /// </summary>
    Star,
    /// <summary>
    /// <see cref="Shapes.Arrow"/>
    /// </summary>
    Arrow,
    /// <summary>
    /// <see cref="Shapes.MeshShape"/>
    /// </summary>
    Mesh,
    /// <summary>
    /// <see cref="Shapes.MeshSet"/>
    /// </summary>
    MeshSet,
    /// <summary>
    /// Deprecated. Not in use.
    /// </summary>
    DeprecatedPointCloud,
    /// <summary>
    /// <see cref="Shapes.Text3D"/>
    /// </summary>
    Text3D,
    /// <summary>
    /// <see cref="Shapes.Text2D"/>
    /// </summary>
    Text2D,
    /// <summary>
    /// <see cref="Shapes.Pose"/>
    /// </summary>
    Pose,

    BuiltInEnd
  }
}
