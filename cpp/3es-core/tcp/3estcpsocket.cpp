//
// author: Kazys Stepanas
//
#include "3estcpsocket.h"

#include "3estcpbase.h"

#include "3estcpdetail.h"

#include <cerrno>
#include <cstring>
#include <cstdio>
#include <thread>

#ifdef WIN32
#include <Ws2tcpip.h>
#endif // WIN32

using namespace tes;

const unsigned TcpSocket::IndefiniteTimeout = ~unsigned(0u);

namespace
{
  const char *socketErrorString(int err)
  {
    switch (err)
    {
//    case EAGAIN:
//      return "again";
    case EWOULDBLOCK:
      return "would block";
    case EBADF:
      return "bad socket";
    case ECONNRESET:
      return "connection reset";
    case EINTR:
      return "interrupt";
    case EINVAL:
      return "no out of bound data";
    case ENOTCONN:
      return "not connected";
    case ENOTSOCK:
      return "invalid socket descriptor";
    case EOPNOTSUPP:
      return "not supported";
    case ETIMEDOUT:
      return "timed out";
    case EIO:
      return "io error";
    case ENOBUFS:
      return "insufficient resources";
    case ENOMEM:
      return "out of memory";
    case ECONNREFUSED:
      return "connection refused";
    default:
      break;
    }

    return "unknown";
  }
}


TcpSocket::TcpSocket()
  : _detail(new TcpSocketDetail)
{
}


TcpSocket::TcpSocket(TcpSocketDetail *detail)
  : _detail(detail)
{
}


TcpSocket::~TcpSocket()
{
  close();
  delete _detail;
}


bool TcpSocket::open(const char *host, unsigned short port)
{
  if (_detail->socket != -1)
  {
    return false;
  }

  _detail->socket = tcpbase::create();

  _detail->address.sin_family = AF_INET;
  _detail->address.sin_port = htons(port);

  if (::inet_pton(AF_INET, host, &_detail->address.sin_addr) <= 0)
  {
    close();  // -1 and 0 are inet_pton() errors
    return false;
  }

  // Connect to server
  if (::connect(_detail->socket, (struct sockaddr *)&_detail->address, sizeof(_detail->address)) != 0)
  {
    int err = errno;
    if (err != ECONNREFUSED)
    {
      fprintf(stderr, "errno : %d -> %s\n", err, socketErrorString(err));
    }

    if (err == EAGAIN)
    {
      fprintf(stderr, "...\n");
    }
    close();
    return false;
  }

#ifdef WIN32
  // Set non blocking.
  u_long iMode = 1;
  ::ioctlsocket(_detail->socket, FIONBIO, &iMode);
#endif // WIN32

  return true;
}


void TcpSocket::close()
{
  if (_detail->socket != -1)
  {
    tcpbase::close(_detail->socket);
    memset(&_detail->address, 0, sizeof(_detail->address));
    _detail->socket = -1;
  }
}


bool TcpSocket::isConnected() const
{
  if (_detail->socket == -1)
  {
    return false;
  }

  return tcpbase::isConnected(_detail->socket);
}


void TcpSocket::setNoDelay(bool noDelay)
{
  tcpbase::setNoDelay(_detail->socket, noDelay);
}


bool TcpSocket::noDelay() const
{
  return tcpbase::noDelay(_detail->socket);
}


void TcpSocket::setReadTimeout(unsigned timeoutMs)
{
  tcpbase::setReceiveTimeout(_detail->socket, timeoutMs);
}


unsigned TcpSocket::readTimeout() const
{
  return tcpbase::getReceiveTimeout(_detail->socket);
}


void TcpSocket::setIndefiniteReadTimeout()
{
  setReadTimeout(IndefiniteTimeout);
}


void TcpSocket::setWriteTimeout(unsigned timeoutMs)
{
  tcpbase::setSendTimeout(_detail->socket, timeoutMs);
}


unsigned TcpSocket::writeTimeout() const
{
  return tcpbase::getSendTimeout(_detail->socket);
}


void TcpSocket::setIndefiniteWriteTimeout()
{
  setWriteTimeout(IndefiniteTimeout);
}


void TcpSocket::setReadBufferSize(int bufferSize)
{
  tcpbase::setReceiveBufferSize(_detail->socket, bufferSize);
}


int TcpSocket::readBufferSize() const
{
  return tcpbase::getReceiveBufferSize(_detail->socket);
}


void TcpSocket::setSendBufferSize(int bufferSize)
{
  tcpbase::setSendBufferSize(_detail->socket, bufferSize);
}


int TcpSocket::sendBufferSize() const
{
  return tcpbase::getSendBufferSize(_detail->socket);
}


int TcpSocket::read(char *buffer, int bufferLength) const
{
#if 0
  int bytesRead = 0;    // bytes read so far

  while (bytesRead < bufferLength)
  {
    int read = ::recv(_detail->socket, buffer + bytesRead, bufferLength - bytesRead, 0);

    if (read < 0)
    {
      if (!tcpbase::checkRecv(_detail->socket, read))
      {
        return -1;
      }
      return 0;
    }
    else if (read == 0)
    {
      return bytesRead;
    }

    bytesRead += read;
  }

  return bytesRead;
#else  // #
  if (_detail->socket == -1)
  {
    return -1;
  }

  int flags = MSG_WAITALL;
  int read = ::recv(_detail->socket, buffer, bufferLength, flags);
  if (read < 0)
  {
    if (!tcpbase::checkRecv(_detail->socket, read))
    {
      return -1;
    }
    return 0;
  }
  return read;
#endif // #
}


int TcpSocket::readAvailable(char *buffer, int bufferLength) const
{
  if (_detail->socket == -1)
  {
    return -1;
  }

//  tcpbase::disableBlocking(_detail->socket);
  int flags = 0;
#ifndef WIN32
  flags |= MSG_DONTWAIT;
#endif // WIN32
  int read = ::recv(_detail->socket, buffer, bufferLength, flags);
  if (read == -1)
  {
    if (!tcpbase::checkRecv(_detail->socket, read))
    {
      return -1;
    }
    return 0;
  }
  return read;
}


int TcpSocket::write(const char *buffer, int bufferLength) const
{
  if (_detail->socket == -1)
  {
    return -1;
  }

  int bytesSent = 0;

  while (bytesSent < bufferLength)
  {
    int flags = 0;
#ifdef __linux__
    flags = MSG_NOSIGNAL;
#endif // __linux__
    int sent;
    bool retry = true;


    while (retry)
    {
      retry = false;
      sent = ::send(_detail->socket, (char *)buffer + bytesSent, bufferLength - bytesSent, flags);
#ifdef WIN32
      if (sent < 0 && WSAGetLastError() == WSAEWOULDBLOCK)
#else  // WIN32
      if (sent < 0 && errno == EWOULDBLOCK)
#endif // WIN32
      {
        // Send buffer full. Wait and retry.
        std::this_thread::yield();
        fd_set wfds;
        struct timeval tv;
        FD_ZERO(&wfds);
        FD_SET(_detail->socket, &wfds);

        tv.tv_sec = 0;
        tv.tv_usec = 1000;

        sent = ::select(1, nullptr, &wfds, nullptr, &tv);
        retry = sent >= 0;
      }
    }


    if (sent < 0)
    {
      if (!tcpbase::checkSend(_detail->socket, sent))
      {
        return -1;
      }
      return 0;
    }
    else if (sent == 0)
    {
      return bytesSent;
    }

    bytesSent += sent;
  }

  return bytesSent;
}


unsigned short TcpSocket::port() const
{
  return _detail->address.sin_port;
}
