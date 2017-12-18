using System;
using System.Collections.Generic;
using Tes.Shapes;
using Tes.IO;
using Tes.Net;

namespace Tes.Server
{
  /// <summary>
  /// Delegate used as a callback when a new connection is added to the <paramref name="server"/>.
  /// Provided as an argument to <see cref="IServer.UpdateConnections(IList{IConnection}, NewConnectionCallback)"/>.
  /// </summary>
  /// <param name="server">The server to which the connection belongs.</param>
  /// <param name="connection">The new connection.</param>
  public delegate void NewConnectionCallback(IServer server, IConnection connection);

  /// <summary>
  /// Defines the interface for a 3rd Eye Scene server.
  /// </summary>
  public interface IServer
  {
    /// <summary>
    /// Retrieve the settings with which the server was created.
    /// </summary>
    ServerSettings Settings { get; }

    /// <summary>
    /// Returns the connection monitor object for this Server.
    /// Null if connections are not supported (internal only).
    /// </summary>
    IConnectionMonitor ConnectionMonitor { get; }

    /// <summary>
    /// Send a pre-composed packet to all clients.
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    int Send(PacketBuffer packet);

    /// <summary>
    /// Sends a create message for the given shape.
    /// </summary>
    /// <param name="shape">The shape to create.</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int Create(Shape shape);

    /// <summary>
    /// Sends an update message for the given shape.
    /// </summary>
    /// <param name="shape">The shape to destroy.</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int Destroy(Shape shape);

    /// <summary>
    /// Sends a destroy message for the given shape.
    /// </summary>
    /// <param name="shape">The shape to update .</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int Update(Shape shape);

    /// <summary>
    /// Update any pending amortised data transfers (e.g., mesh transfer).
    /// </summary>
    /// <param name="byteLimit">Limit the packet payload size to approximately this
    /// amount of data.</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int UpdateTransfers(int byteLimit);

    /// <summary>
    /// Sends a message marking the end of the current frame (and start of a new frame).
    /// </summary>
    /// <param name="dt">Indicates the time passed since over this frame (seconds).</param>
    /// <param name="flush">True to allow clients to flush transient options, false to clients
    ///   preserver such objects.</param>
    /// <returns>The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int UpdateFrame(float dt, bool flush = true);

    /// <summary>
    /// Returns the number of current connections.
    /// </summary>
    /// <returns>The current number of connections.</returns>
    int ConnectionCount { get; }

    /// <summary>
    /// Requests the connection at the given index.
    /// </summary>
    /// <remarks>
    /// This data may be stale if the <see cref="ConnectionMonitor"/> has yet to update.
    /// </remarks>
    /// <param name="index">The index of the requested connection.</param>
    /// <returns>The requested connection, or null if <paramref name="index"/> is out of range.</returns>
    IConnection Connection(int index);

    /// <summary>
    /// Enumerates the current connections.
    /// </summary>
    IEnumerable<IConnection> Connections { get; }

    /// <summary>
    /// Called from the <see cref="IConnectionMonitor"/> implementation to
    /// update the current server connections.
    /// </summary>
    /// <param name="connections">The collection of active connections to server should handle.</param>
    /// <param name="callback">Optional callback to invoke for each new connection.</param>
    void UpdateConnections(IList<IConnection> connections, NewConnectionCallback callback = null);

    // FIXME: need an explicit method to kick all clients. Can emulate with ConnectionMonitor.Stop() for now.
  }
}
