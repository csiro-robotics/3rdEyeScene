//
// author: Kazys Stepanas
//
#include "3estcplistensocket.h"

#include "3estcpbase.h"
#include "3estcpsocket.h"

#include "3estcpdetail.h"

#include <cstring>

#define TCPBASE_MAX_BACKLOG 10

using namespace tes;

namespace
{
  bool acceptConnection(TcpListenSocketDetail &server, TcpSocketDetail &client)
  {
    socklen_t addressLen = sizeof(client.address);

    client.socket = (int)::accept(server.listenSocket, (sockaddr *)&client.address, (socklen_t *)&addressLen);

    if (client.socket <= 0)
    {
      return false;
    }

    //printf("remote connection on port %d (%d) created from port %d (%d)\n",
    //       tcpbase::getSocketPort(client.socket), client.socket,
    //       tcpbase::getSocketPort(server.listenSocket), server.listenSocket);

#ifdef __APPLE__
  // Don't throw a SIGPIPE signal
    int noSignal = 1;
    if (::setsockopt(client.socket, SOL_SOCKET, SO_NOSIGPIPE, &noSignal, sizeof(noSignal)) < 0)
    {
      tcpbase::close(client.socket);
      return false;
    }
#endif // __APPLE__

#ifdef WIN32
    // Set non blocking.
    u_long iMode = 1;
    ::ioctlsocket(client.socket, FIONBIO, &iMode);
#endif // WIN32

    //tcpbase::dumpSocketOptions(client.socket);
    return true;
  }
}

TcpListenSocket::TcpListenSocket()
  : _detail(new TcpListenSocketDetail)
{
}


TcpListenSocket::~TcpListenSocket()
{
  close();
  delete _detail;
}


uint16_t TcpListenSocket::port() const
{
  return (isListening()) ? ntohs(_detail->address.sin_port) : 0;
}


bool TcpListenSocket::listen(unsigned short port)
{
  if (isListening())
  {
    return false;
  }

  _detail->listenSocket = tcpbase::create();
  if (_detail->listenSocket == -1)
  {
    return false;
  }

  _detail->address.sin_family = AF_INET;
  _detail->address.sin_addr.s_addr = htonl( INADDR_ANY );
  _detail->address.sin_port = htons(port);

  //  Give the socket a local address as the TCP server
  if (::bind(_detail->listenSocket, (struct sockaddr *)&_detail->address, sizeof(_detail->address)) < 0)
  {
    close();
    return false;
  }

  if (::listen(_detail->listenSocket, TCPBASE_MAX_BACKLOG) < 0)
  {
    close();
    return false;
  }

  //printf("Listening on port %d\n", tcpbase::getSocketPort(_detail->listenSocket));

  return true;
}


void TcpListenSocket::close()
{
  if (_detail->listenSocket != -1)
  {
    tcpbase::close(_detail->listenSocket);
    _detail->listenSocket = -1;
    memset(&_detail->address, 0, sizeof(_detail->address));
  }
}



bool TcpListenSocket::isListening() const
{
  return _detail->listenSocket != -1;
}


TcpSocket *TcpListenSocket::accept(unsigned timeoutMs)
{
  struct timeval timeout;
  fd_set fdRead;

  if (_detail->listenSocket < 0)
  {
    return nullptr;
  }

  // use select() to avoid blocking on accept()

  FD_ZERO(&fdRead);          // Clear the set of selected objects
  FD_SET(_detail->listenSocket, &fdRead);    // Add socket to read set

  tcpbase::timevalFromMs(timeout, timeoutMs);

  if (::select(_detail->listenSocket + 1, &fdRead, NULL, NULL, &timeout) < 0)
  {
    return nullptr;
  }

  // Test if the socket file descriptor (m_sockLocal) is part of the
  // set returned by select().  If not, then select() timed out.

  if (FD_ISSET(_detail->listenSocket, &fdRead) == 0)
  {
    return nullptr;
  }

  // Accept a connection from a client
  TcpSocketDetail *clientDetail = new TcpSocketDetail();
  if (!::acceptConnection(*_detail, *clientDetail))
  {
    return nullptr;
  }

  return new TcpSocket(clientDetail);
}


void TcpListenSocket::releaseClient(TcpSocket *client)
{
  if (client)
  {
    client->close();
    delete client;
  }
}
