//
// author: Kazys Stepanas
//
#ifndef _3ESSERVER_H_
#define _3ESSERVER_H_

#include "3es-core.h"

#include "3esconnection.h"

#include <cstdint>

namespace tes
{
  class CollatedPacket;
  class Connection;
  class ConnectionMonitor;
  class PacketWriter;
  class Shape;
  struct ServerInfoMessage;

  /// Server option flags.
  enum ServerFlag
  {
    /// Set to collate outgoing messages into larger packets.
    SF_Collate = (1<<1),
    /// Set to compress collated outgoing packets using GZip compression.
    /// Has no effect if @c SF_Collate is not set or if the library is not built against ZLib.
    SF_Compress = (1<<2),
  };

  struct _3es_coreAPI ServerSettings
  {
    /// Port to listen on.
    uint16_t listenPort;
    /// @c ServerFlag values.
    unsigned flags;
    /// Size of the client packet buffers.
    uint16_t clientBufferSize;

    // TODO: Allowed client IPs.

    inline ServerSettings(unsigned flags = SF_Collate, uint16_t port = 33500u, uint16_t bufferSize = 0xffe0)
      : listenPort(port), flags(flags), clientBufferSize(bufferSize) {}
  };

  /// Defines the interface for managing a 3es server.
  ///
  /// Listening must be initiated via the @c Server object's @c ConnectionMonitor,
  /// available via @c connectionMonitor(). See that class's comments for
  /// details of synchronous and asynchronous operation. The monitor
  /// will be null if connections are not supported (generally internal only).
  class _3es_coreAPI Server : public Connection
  {
  public:
    /// Creates a server with the given settings.
    ///
    /// The @p settings affect the local server state, while @p serverInfo describes
    /// the server to newly connected clients (first message sent). The @p serverInfo
    /// may be omitted to use the defaults.
    ///
    /// @param settings The local server settings.
    /// @param serverInfo Server settings published to clients. Null to use the defaults.
    static Server *create(const ServerSettings &settings = ServerSettings(), const ServerInfoMessage *serverInfo = nullptr);

    /// Destroys the server this method is called on. This ensures correct clean up.
    virtual void dispose() = 0;

  protected:
    /// Hidden virtual destructor.
    virtual ~Server() {}

  public:

    /// Retrieve the @c ServerFlag set with which the server was created.
    virtual unsigned flags() const = 0;

    //---------------------
    // Connection methods.
    //---------------------

    using Connection::create;
    using Connection::send;

    /// Set a completed packet to all clients.
    ///
    /// The @p packet must be finalised first.
    ///
    /// @param packet The packet to send.
    virtual int send(const PacketWriter &packet) = 0;

    /// Send a collated packet to all clients.
    ///
    /// This supports sending collections of packets as a single send operation
    /// while maintaining thread safety.
    ///
    /// The collated packet may be larger than the normal send limit as collated
    /// message is extracted and sent individually. To support this, compression on
    /// @p collated is not supported.
    ///
    /// @par Note sending in this way bypasses the shape and resource caches and
    /// can only work when the user maintains state.
    ///
    /// @param collated Collated packets to send. Compression is not supported.
    virtual int send(const CollatedPacket &collated) = 0;

    /// Returns the connection monitor object for this @c Server.
    /// Null if connections are not supported (internal only).
    virtual ConnectionMonitor *connectionMonitor() = 0;

    /// Returns the number of current connections.
    /// @return The current number of connections.
    virtual unsigned connectionCount() const = 0;

    /// Requests the connection at the given index.
    ///
    /// This data may be stale if the @c ConnectionMonitor has yet to update.
    /// @param index The index of the requested connection.
    /// @return The requested connection, or null if @p index is out of range.
    virtual Connection *connection(unsigned index) = 0;

    /// @overload
    virtual const Connection *connection(unsigned index) const = 0;
  };
}

#endif // _3ESSERVER_H_
