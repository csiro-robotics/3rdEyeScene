
namespace Tes.Server
{
  /// <summary>
  /// Defines the operation mode for a connection monitor.
  /// </summary>
  public enum ConnectionMonitorMode
  {
    /// <summary>
    /// Inactive.
    /// </summary>
    None,
    /// <summary>
    /// <see cref="IConnectionMonitor"/> to be updated from the main thread.
    /// </summary>
    Synchronous,
    /// <summary>
    /// <see cref="IConnectionMonitor"/> to be updated in a background thread
    /// managed by itself.
    /// </summary>
    Asynchronous
  }

  /// <summary>
  /// Defines a utility interface for monitoring new connections for a <see cref="IServer"/>.
  /// </summary>
  /// <remarks>
  /// The monitor manages listening for new connections and expiring old ones.
  /// Doing so can be time consuming, so the monitor supports running its
  /// own monitor thread. It also supports synchronous operation in case
  /// connection monitoring is unnecessary, or unable to be pushed off thread.
  ///
  /// Asynchronous mode is activated by calling <see cref="Start(ConnectionMonitorMode)"/> and stopped with
  /// <see cref="Stop()"/>Calls to <see cref="Join()"/> will block until the monitor thread has
  /// completed, but should only be called after @c stop() has been called.
  /// The <see cref="CommitConnections(NewConnectionCallback)"/> method must still be called by the main thread
  /// (synchronously) to control when connections are activated and deactivated.
  ///
  /// Synchronous mode is supported by calling <see cref="MonitorConnections()"/> to
  /// accept new connections. This must be followed by a call to
  /// <see cref="CommitConnections(NewConnectionCallback)"/> to commit the changes to the owning <see cref="IServer"/>.
  ///
  /// <example>
  /// Synchronous usage.
  /// <code lang="c#">
  /// float dt = 0;
  /// IServer server = /* Server.Create() */;
  /// server.ConnectionMonitor.Start(ConnectionMonitorMode.Synchronous);
  /// for (;;)
  /// {
  ///   // Prepare frame.
  ///   //...
  ///
  ///   server.UpdateFrame(dt);
  ///   server.ConnectionMonitor.MonitorConnections();
  ///   server.ConnectionMonitor.CommitConnections();
  ///
  ///   // Loop end...
  /// }
  /// </code>
  /// </example>
  /// 
  /// <example>
  /// Asynchronous Usage
  /// <code lang="C#">
  /// float dt = 0;
  /// IServer server = /* Server.Create() */;
  /// server.ConnectionMonitor.Start(ConnectionMonitorMode.Asynchronous);
  /// for (;;)
  /// {
  ///   // Prepare frame.
  ///   //...
  ///
  ///   server.UpdateFrame(dt);
  ///   server.ConnectionMonitor.CommitConnections();
  ///
  ///   // Loop end...
  /// }
  ///
  /// server.ConnectionMonitor.Stop();  // Safe even if Synchronous
  /// server.ConnectionMonitor.Join();  // Safe even if Synchronous
  /// </code>
  /// </example>
  /// </remarks>
  public interface IConnectionMonitor
  {
    /// <summary>
    /// Returns the current running mode.
    /// </summary>
    /// <remarks>
    /// <code>Asyncrhonous</code> mode is set as soon as <see cref="Start(ConnectionMonitorMode)"/>
    /// is called and drops to <code>None</code> after calling <see cref="Stop()"/> once the thread has stopped.
    ///
    /// <code>Syncrhonous</code> mode is set as soon as <see cref="Start(ConnectionMonitorMode)"/> is called and
    /// drops to <code>None</code> on calling <see cref="Stop()"/>.
    ///
    /// The mode is <code>None</code> if not running in either mode.
    /// </remarks>
    ConnectionMonitorMode Mode { get; }

    /// <summary>
    /// Starts the monitor listening in the specified mode.
    /// </summary>
    /// <param name="mode">The listening mode. Mode @c Node is ignored.</param>
    /// <returns>
    /// True if listening is running in the specified mode. This includes both newly
    /// started and if it was already running in that mode. False is returned if mode is
    /// <code>None</code>, or differs from the running mode.
    /// </returns>
    /// <remarks>
    /// The listening thread is started if <paramref name="mode"/> is <code>Asynchronous</code>.
    /// </remarks>
    bool Start(ConnectionMonitorMode mode);

    /// <summary>
    /// Stops listening for further connections. This requests termination
    /// of the monitor thread if running.
    /// </summary>
    /// <remarks>
    /// Safe to call if not running.
    /// </remarks>
    void Stop();

    /// <summary>
    /// Called to join the monitor thread. Returns immediately
    /// if not running.
    /// </summary>
    void Join();

    /// <summary>
    /// Accepts new connections and checks for expired connections, but
    /// effects neither in the @c Server.
    /// </summary>
    /// <remarks>
    /// This is either called on the main thread for synchronous operation,
    /// or internally in asynchronous mode.
    /// </remarks>
    void MonitorConnections();

    /// <summary>
    /// Migrates new connections to the owning @c Server and removes expired
    /// connections.
    /// </summary>
    /// <param name="callback">Optional callback to invoke for each new connection.</param>
    void CommitConnections(NewConnectionCallback callback);

    /// <summary>
    /// Wait for up to <paramref name="timeoutMs"/> milliseconds for at least one connection before continuing.
    /// </summary>
    /// <param name="timeoutMs">The time to wait in milliseconds.</param>
    /// <returns>True if a new connection has been found.</returns>
    /// <remarks>
    /// The method returns once either a new connection exists or <paramref name="timeoutMs"/> has elapsed.
    /// When there is a new connection, it has yet to be committed. This should be done by calling
    /// <see cref="CommitConnections(NewConnectionCallback)"/> after this method returns true.
    /// </remarks>
    bool WaitForConnections(uint timeoutMs);
  }
}
