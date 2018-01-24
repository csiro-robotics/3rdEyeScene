//
// author: Kazys Stepanas
//
#ifndef _3ESTCPCONNECTIONMONITOR_H_
#define _3ESTCPCONNECTIONMONITOR_H_

#include "3es-core.h"

#include "3esconnectionmonitor.h"

#include <3esspinlock.h>

#include <atomic>
#include <thread>
#include <vector>

namespace tes
{
  class TcpConnection;
  class TcpServer;
  class TcpListenSocket;

  /// Implements a @c ConnectionMonitor using the TCP protocol. Intended only for use with a @c TcpServer.
  class TcpConnectionMonitor : public ConnectionMonitor
  {
  public:
    typedef SpinLock Lock;

    /// Error codes.
    enum ConnectionError
    {
      CE_None,
      /// Failed to listen on the requested port.
      CE_ListenFailure,
      /// Timeout has expired.
      CE_Timeout
    };

    /// Construct a TCP based connection monitor for @p server.
    /// @param server The server owning this connection monitor.
    TcpConnectionMonitor(TcpServer &server);

    /// Destructor.
    ~TcpConnectionMonitor();

    /// Get the @c TcpServer which owns this @c ConnectionMonitor.
    /// @return The owning server.
    inline TcpServer &server() { return _server; }

    /// @overload
    inline const TcpServer &server() const { return _server; }

    /// Get the TCP socket used to manage connections.
    /// @return The @c TcpListenSocket the connection monitor listens on.
    ///    May be null when not currently listening (before @c start()).
    inline const TcpListenSocket *socket() const { return _listen; }

    /// Get the last error code.
    /// @return The @c ConnectionError for the last error.
    int lastErrorCode() const;

    /// Clear the last error code.
    /// @return The @c ConnectionError for the last error.
    int clearErrorCode();

    /// Report the port on which the connection monitor is listening.
    /// @return The listen port or zero if not listening.
    int port() const override;

    /// Starts the monitor thread (asynchronous mode).
    bool start(Mode mode) override;
    /// Requests termination of the monitor thread.
    /// Safe to call if not running.
    void stop() override;
    /// Called to join the monitor thread. Returns immediately
    /// if not running.
    void join() override;

    /// Returns true if the connection monitor has start.
    /// @return True if running.
    bool isRunning() const override;

    /// Returns the current running mode.
    ///
    /// @c Asynchronous mode is set as soon as @c start(Asynchronous) is called and
    /// drops to @c None after calling @c stop() once the thread has stopped.
    ///
    /// @c Synchronous mode is set as soon as @c start(Synchronous) is called and
    /// drops to @c None on calling @c stop().
    ///
    /// The mode is @c None if not running in either mode.
    Mode mode() const override;

    /// Wait up to @p timeoutMs milliseconds for a connection.
    /// Returns immediately if we already have a connection.
    /// @param timeoutMs The time out to wait in milliseconds.
    /// @return The number of connections on returning. These may need to be committed.
    int waitForConnection(unsigned timeoutMs) override;

    /// Accepts new connections and checks for expired connections, but
    /// effects neither in the @c Server.
    ///
    /// This is either called on the main thread for synchronous operation,
    /// or internally in asynchronous mode.
    void monitorConnections() override;

    /// Sets the callback invoked for each new connection.
    ///
    /// This is invoked from @p commitConnections() for each new connection.
    /// The arguments passed to the callback are:
    /// - @c server : the @c Server object.
    /// - @c connection : the new @c Connection object.
    /// - @c user : the @c user argument given here.
    ///
    /// Write only.
    ///
    /// @param callback The callback function pointer.
    /// @param user A user pointer passed to the @c callback whenever it is invoked.
    void setConnectionCallback(void(*callback)(Server &, Connection &, void *), void *user) override;

    /// An overload of @p setConnectionCallback() using the C++11 @c funtion object.
    /// Both methods are provided to cater for potential ABI issues.
    ///
    /// No user argument is supported as the flexibility of @c std::function obviates the
    /// for such.
    ///
    /// @param callback The function to invoke for each new connection.
    void setConnectionCallback(const std::function<void(Server &, Connection &)> &callback) override;

    /// Migrates new connections to the owning @c Server and removes expired
    /// connections.
    ///
    /// For each new connection, the callback set in @c setConnectionCallback() is invoked,
    /// passing the server, connection and @p user argument.
    ///
    /// @param callback When given, called for each new connection.
    /// @param user User argument passed to @p callback.
    void commitConnections() override;

  private:
    bool listen();
    void stopListening();
    void monitorThread();

    TcpServer &_server;
    TcpListenSocket *_listen;
    std::function<void(Server &, Connection &)> _onNewConnection;
    Mode _mode; ///< Current execution mode.
    std::vector<TcpConnection *> _connections;
    std::vector<TcpConnection *> _expired;
    std::atomic_int _listenPort;
    std::atomic_int _errorCode;
    std::atomic_bool _running;
    std::atomic_bool _quitFlag;
    mutable Lock _connectionLock;
    std::thread *_thread;
  };
}

#endif // _3ESTCPCONNECTIONMONITOR_H_
