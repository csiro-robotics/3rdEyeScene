using System;

namespace Tes.Net
{
  /// <summary>
  /// Enumeration of the primary routing IDs used for sending Tes messages.
  /// Assigned to <see cref="Tes.IO.PacketHeader.RoutingID"/>.
  /// </summary>
  public enum RoutingID : ushort
  {
    /// <summary>
    /// Invalid/null ID.
    /// </summary>
    Null = 0,
    /// <summary>
    /// <see cref="ServerInfoMessage"/>
    /// </summary>
    ServerInfo,
    /// <summary>
    /// <see cref="ControlMessage"/>
    /// </summary>
    Control,
    /// <summary>
    /// The packet contains a collection of collated packets, optionally compressed.
    /// <see cref="CollatedPacketMessage"/>
    /// </summary>
    CollatedPacket,
    /// <summary>
    /// <see cref="MeshMessageType"/>
    /// </summary>
    Mesh,
    /// <summary>
    /// <see cref="CameraMessage"/>
    /// </summary>
    Camera,
    /// <summary>
    /// A message relating to categories.
    /// </summary>
    Category,
    /// <summary>
    /// Extension message for materials (NYI).
    /// </summary>
    Material,
    /// <summary>
    /// Built in shape types start at this ID.
    /// </summary>
    ShapeIDsStart = 64,
    /// <summary>
    /// User message routing IDs start here.
    /// </summary>
    UserIDStart = 2048
  }
}

