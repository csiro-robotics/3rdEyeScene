//
// author: Kazys Stepanas
//
#ifndef _3ESTCPSERVER_H_
#define _3ESTCPSERVER_H_

#include "../3esserver.h"

#include <3esmeshmessages.h>
#include <3esspinlock.h>

#include <atomic>
#include <functional>
#include <vector>

namespace tes
{
  class TcpConnection;
  class TcpConnectionMonitor;
  class TcpListenSocket;
  class TcpServer;

  /// A TCP based implementation of a 3es @c Server.
  class TcpServer : public Server
  {
  public:
    typedef SpinLock Lock;

    TcpServer(const ServerSettings &settings, const ServerInfoMessage *serverInfo);
    ~TcpServer();

    inline const ServerSettings &settings() const { return _settings; }

    void dispose() override;

    unsigned flags() const override;

    /// Close all connections and stop listening for new connections.
    void close() override;

    /// Activate/deactivate the connection. Messages are ignored while inactive.
    /// @param enable The active state to set.
    void setActive(bool enable) override;

    /// Check if currently active.
    /// @return True while active.
    bool active() const override;

    /// Always "TcpServer".
    /// @return "TcpServer".
    const char *address() const override;
    /// Return the listen port or zero when not listening.
    /// @return The listen port.
    uint16_t port() const override;

    /// Any current connections?
    /// @return True if we have at least one connection.
    bool isConnected() const override;

    int create(const Shape &shape) override;
    int destroy(const Shape &shape) override;
    int update(const Shape &shape) override;

    int updateFrame(float dt, bool flush = true) override;
    int updateTransfers(unsigned byteLimit) override;

    /// Override
    /// @param resource The resource to reference.
    /// @return The count from the last connection.
    unsigned referenceResource(const Resource *resource) override;

    /// Override
    /// @param resource The resource to release.
    /// @return The count from the last connection.
    unsigned releaseResource(const Resource *resource) override;

    /// Ignored. Controlled by this class.
    /// @param info Ignored.
    /// @return False.
    bool sendServerInfo(const ServerInfoMessage &info) override;

    int send(const PacketWriter &packet) override;
    int send(const CollatedPacket &collated) override;
    int send(const uint8_t *data, int byteCount) override;

    ConnectionMonitor *connectionMonitor() override;
    unsigned connectionCount() const override;
    Connection *connection(unsigned index) override;
    const Connection *connection(unsigned index) const override;

    /// Updates the internal connections list to the given one.
    /// Intended only for use by the @c ConnectionMonitor.
    void updateConnections(const std::vector<TcpConnection *> &connections, const std::function<void (Server &, Connection &)> &callback);

  private:
    mutable Lock _lock;
    std::vector<TcpConnection *> _connections;
    TcpConnectionMonitor *_monitor;
    ServerSettings _settings;
    ServerInfoMessage _serverInfo;
    std::atomic_bool _active;
  };
}

#endif // _3ESTCPSERVER_H_
