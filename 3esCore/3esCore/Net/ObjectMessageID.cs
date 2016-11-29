using System;

namespace Tes.Net
{
  /// <summary>
  /// Defines the message IDs for shape messages (starting at  <see cref="RoutingID.ShapeIDsStart"/>).
  /// </summary>
  public enum ObjectMessageID : ushort
  {
    /// <summary>
    /// Null value; not used.
    /// </summary>
    Null,
    /// <summary>
    /// <see cref="CreateMessage"/>
    /// </summary>
    Create,
    /// <summary>
    /// <see cref="UpdateMessage"/>
    /// </summary>
    Update,
    /// <summary>
    /// <see cref="DestroyMessage"/>
    /// </summary>
    Destroy,
    /// <summary>
    /// <see cref="DataMessage"/>
    /// </summary>
    Data
  }
}
