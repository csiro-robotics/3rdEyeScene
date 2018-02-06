//
// author: Kazys Stepanas
//
#ifndef _3ESTCPLISTENSOCKET_H_
#define _3ESTCPLISTENSOCKET_H_

#include "3es-core.h"

#include <cinttypes>

namespace tes
{
  class TcpSocket;
  struct TcpListenSocketDetail;

  /// Represents a TCP server socket, listening for connections.
  /// Each new connection is serviced by it's own @c TcpSocket,
  /// spawned from this class.
  class _3es_coreAPI TcpListenSocket
  {
  public:
    /// Constructor.
    TcpListenSocket();
    /// Destructor.
    ~TcpListenSocket();

    /// The port on which the socket is listening, or zero when not listening.
    /// @return The TCP listen port.
    uint16_t port() const;

    /// Start listening for connections on the specified port.
    /// @param port The port to listen on.
    /// @return @c true on success. Failure may be because it is
    /// already listening.
    bool listen(unsigned short port);

    /// Close the connection and stop listening.
    /// Spawned sockets remaining active.
    ///
    /// Safe to call if not already listening.
    void close();

    /// Checks the listening state.
    /// @return @c true if listening for connections.
    bool isListening() const;

    /// Accepts the first pending connection. This will block for the
    /// given timeout period.
    /// @param timeoutMs The timeout to block for, awaiting new connections.
    /// @return A new @c TcpSocket representing the accepted connection,
    ///   or @c nullptr if there are not pending connections. The caller
    ///   takes ownership of the socket and must delete it when done either
    ///   by invoking @c delete, or by calling @c releaseClient() (preferred).
    TcpSocket *accept(unsigned timeoutMs = 0);

    /// Disposes of a socket allocated by @c accept(). This is safer
    /// than invoking @c delete on the socket, because it ensures the
    /// same allocator is used to dispose of the socket.
    /// @param client The socket to close and release.
    void releaseClient(TcpSocket *client);

  private:
    TcpListenSocketDetail *_detail; ///< Implementation detail.
  };

}

#endif // _3ESTCPLISTENSOCKET_H_
