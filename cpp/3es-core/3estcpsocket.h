//
// author: Kazys Stepanas
//
#ifndef TCPSOCKET_H_
#define TCPSOCKET_H_

#include "3es-core.h"

#include <cstddef>

namespace tes
{
  struct TcpSocketDetail;

  /// A TCP/IP communication socket implementation.
  class _3es_coreAPI TcpSocket
  {
  public:
    /// Value used to signify an indefinite timeout.
    static const unsigned IndefiniteTimeout;

    /// Constructor.
    TcpSocket();
    /// Constructor.
    TcpSocket(TcpSocketDetail *detail);
    /// Destructor.
    ~TcpSocket();

    /// Open a connection to the target @p host and @p port (blocking).
    /// @param host The host IP address or host name.
    /// @param port The target port.
    /// @return True if the connection was successfully established before timing out.
    bool open(const char *host, unsigned short port);

    /// Close the socket connection (no custom messages sent). Safe to call when not open.
    void close();

    /// Checks the connected state.
    /// @bug This is not reliable for a client socket. It only works when
    /// the @c TcpSocket was created from a @c TcpListenSocket.
    /// @return @c true if the socket is currently connected.
    bool isConnected() const;

    /// Disable Nagle's algorithm, effectively disabling send delays?
    /// @param noDelay Disable the delay?
    void setNoDelay(bool noDelay);

    /// Check if Nagle's algorithm is disable.
    /// @return True if Nagle's algorithm is disabled.
    bool noDelay() const;

    /// Sets the blocking timeout on calls to @c read().
    /// All calls to @c read() either until there are data
    /// available or this time elapses. Set to zero for non-blocking
    /// Use @c setIndefiniteReadTimeout() to block undefinately.
    /// @c read() calls.
    /// @param timeoutMs Read timeout in milliseconds.
    void setReadTimeout(unsigned timeoutMs);

    /// Returns the read timeout.
    /// @return The current read timeout in milliseconds. A value of
    ///   @c IndefiniteTimeout indicates indefinite blocking.
    unsigned readTimeout() const;

    /// Clears the timeout for @c read() calls, blocking indefinitely
    /// until data are available.
    void setIndefiniteReadTimeout();

    /// Sets the blocking timeout on calls to @c write().
    /// This behaves in the same way as @c setReadTimeout(),
    /// except that it relates to @c write() calls.
    /// @param timeoutMs Read timeout in milliseconds.
    void setWriteTimeout(unsigned timeoutMs);

    /// Returns the write timeout.
    /// @return The current write timeout in milliseconds. A value of
    ///   @c IndefiniteTimeout indicates indefinite blocking.
    unsigned writeTimeout() const;

    /// Clears the timeout for @c read() calls, blocking indefinitely
    /// until data have been sent.
    void setIndefiniteWriteTimeout();

    /// Sets the read buffer size (bytes).
    /// @param bufferSize The new buffer size. Max is 2^16 - 1.
    void setReadBufferSize(int bufferSize);

    /// Gets the current read buffer size (bytes).
    int readBufferSize() const;

    /// Sets the send buffer size (bytes).
    /// @param bufferSize The new buffer size. Max is 2^16 - 1.
    void setSendBufferSize(int bufferSize);

    /// Gets the current send buffer size (bytes).
    int sendBufferSize() const;

    /// Attempts to read data from the socket until the buffer is full.
    /// This may block. The blocking time may vary, but it will only block
    /// for at least the read timeout value so long as there is no activity.
    ///
    /// @param buffer The data buffer to read into.
    /// @param bufferLength The maximum number of types to read into @p buffer.
    /// @return The number of bytes read, which may be less than @p bufferLength, or -1 on error.
    int read(char *buffer, int bufferLength) const;

    /// @overload
    inline int read(unsigned char *buffer, int bufferLength) const { return read((char*)buffer, bufferLength); }

    /// Reads available data from the socket, returning immediately if there
    /// are no data available.
    /// @param buffer The data buffer to read into.
    /// @param bufferLength The maximum number of types to read into @p buffer.
    /// @return The number of bytes read, or -1 on error.
    int readAvailable(char *buffer, int bufferLength) const;

    /// @overload
    inline int readAvailable(unsigned char *buffer, int bufferLength) const { return readAvailable((char*)buffer, bufferLength); }

    /// Attempts to write data from the socket. This may block for the set write
    /// timeout.
    /// @param buffer The data buffer to send.
    /// @param bufferLength The number of bytes to send.
    /// @return The number of bytes sent, or -1 on error.
    int write(const char *buffer, int bufferLength) const;

    /// @overload
    inline int write(const unsigned char *buffer, int bufferLength) const { return write((const char*)buffer, bufferLength); }

    unsigned short port() const;

  private:
    TcpSocketDetail *_detail; ///< Implementation detail.
  };
}

#endif // TCPSOCKET_H_
