//
// author: Kazys Stepanas
//
#include "3estcplistensocket.h"

#include "3estcpsocket.h"

#include "3estcpdetail.h"

#include <QHostAddress>

#include <cstring>

using namespace tes;
using namespace Poco::Net;

TcpListenSocket::TcpListenSocket()
  : _detail(new TcpListenSocketDetail)
{
}


TcpListenSocket::~TcpListenSocket()
{
  close();
  delete _detail;
}


bool TcpListenSocket::listen(unsigned short port)
{
  if (isListening())
  {
    return false;
  }

  TCPServerParams *params = new TCPServerParams;
  params->setMaxQueued(16);
  params->setMaxThreads(1);

  _detail->listenSocket = new TCPServer(new TCPRequestHandlerFactory(), ServerSocket(port), params);
  _detail->listenSocket->start();
  return true;
}


void TcpListenSocket::close()
{
  if (isListening())
  {
    _detail->listenSocket->stop();
    delete _detail->listenSocket;
    _detail->listenSocket = nullptr;
  }
}



bool TcpListenSocket::isListening() const
{
  return _detail->listenSocket != nullptr;
}


TcpSocket *TcpListenSocket::accept(unsigned timeoutMs)
{
  if (!_detail->listenSocket.waitForNewConnection(timeoutMs))
  {
    return nullptr;
  }

  if (!_detail->listenSocket.hasPendingConnections())
  {
    return nullptr;
  }

  QTcpSocket *newSocket = _detail->listenSocket.nextPendingConnection();
  if (!newSocket)
  {
    return nullptr; 
  }
  TcpSocketDetail *clientDetail = new TcpSocketDetail;
  clientDetail->socket = newSocket;
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
