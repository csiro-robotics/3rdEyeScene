using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags controlling object creation.
  /// </summary>
  [Flags]
  public enum ObjectFlag : ushort
  {
    /// <summary>
    /// Null flag.
    /// </summary>
    None = 0,
    /// <summary>
    /// Shape should be rendered using wireframe rendering.
    /// </summary>
    Wireframe = (1 << 0),
    /// <summary>
    /// Shape is transparent. Colour should include an appropriate alpha value.
    /// </summary>
    Transparent = (1 << 1),
    /// <summary>
    /// Shape used a two sided shader (triangle culling disabled).
    /// </summary>
    TwoSided = (1 << 2),
    /// <summary>
    /// Shape creation should replace any pre-exiting shape with the same object ID.
    /// </summary>
    /// <remarks>
    /// Normally duplicate shape creation messages are not allowed. This flag allows a duplicate shape ID
    /// (non-transient) by replacing the previous shape.
    /// </remarks>
    Replace = (1 << 3),
    /// <summary>
    /// Creating multiple shapes in one message.
    /// </summary>
    MultiShape = (1 << 4),
    /// <summary>
    /// Do not reference count resources or queue resources for sending.
    /// </summary>
    /// <remarks>
    /// By default each connection reference counts and queues resources for each shape, sending them from
    /// <see cref="IServer.UpdateTransfers(int)"/>. This flag prevents resources from being sent automatically for a
    /// shape. References are then dereferenced (potentially destroyed) when destroying a resource using shape. This
    /// flag prevents this reference counting for a shape, essentially assuming the client has the resources via
    /// explicit references using <see cref="IConnection.AddResource(Resource)"/>.
    ///
    /// This should always be used when using the <c>Replace</c> flag as reference counting can only be maintained with
    /// proper create/destroy command pairs.
    /// </remarks>
    SkipResources = (1 << 5),
    /// <summary>
    /// Indicates <see cref="ObjectAttributes"/> is in double precision.
    /// </summary>
    DoublePrecision = (1 << 6),
    /// <summary>
    /// User flags start here.
    /// </summary>
    User = (1 << 8)
  }
}
