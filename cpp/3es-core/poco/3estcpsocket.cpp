//
// author: Kazys Stepanas
//
#include "3estcpsocket.h"

#include "3estcpdetail.h"

using namespace tes;

const unsigned TcpSocket::IndefiniteTimeout = ~unsigned(0u);

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
  if (_detail->socket)
  {
    return false;
  }

  QTcpSocket *socket;
  socket = _detail->socket = new QTcpSocket;
  socket->connectToHost(host, port);
  return true;
}


void TcpSocket::close()
{
  if (_detail->socket)
  {
    _detail->socket->close();
    delete _detail->socket;
    _detail->socket = nullptr;
  }
}


bool TcpSocket::isConnected() const
{
  if (_detail->socket)
  {
    switch (_detail->socket->state())
    {
    case QTcpSocket::ConnectingState:
    case QTcpSocket::ConnectedState:
      return true;
    }
  }

  return false;
}


void TcpSocket::setNoDelay(bool noDelay)
{
  if (_detail->socket)
  {
    _detail->socket->setSocketOption(QTcpSocket::LowDelayOption, noDelay ? 1 : 0);
  }
}


bool TcpSocket::noDelay() const
{
  if (_detail->socket)
  {
    return _detail->socket->socketOption(QTcpSocket::LowDelayOption).toInt() == 1;
  }
  return false;
}


void TcpSocket::setReadTimeout(unsigned timeoutMs)
{
  _detail->readTimeout = timeoutMs;
}


unsigned TcpSocket::readTimeout() const
{
  return _detail->readTimeout;
}


void TcpSocket::setIndefiniteReadTimeout()
{
  _detail->readTimeout = IndefiniteTimeout;
}


void TcpSocket::setWriteTimeout(unsigned timeoutMs)
{
  _detail->writeTimeout = timeoutMs;
}


unsigned TcpSocket::writeTimeout() const
{
  return _detail->writeTimeout;
}


void TcpSocket::setIndefiniteWriteTimeout()
{
  _detail->writeTimeout = IndefiniteTimeout;
}


void TcpSocket::setReadBufferSize(int bufferSize)
{
  if (_detail->socket)
  {
    _detail->socket->setSocketOption(QTcpSocket::ReceiveBufferSizeSocketOption, bufferSize);
  }
}


int TcpSocket::readBufferSize() const
{
  if (_detail->socket)
  {
    _detail->socket->socketOption(QTcpSocket::ReceiveBufferSizeSocketOption).toInt();
  }
  return 0;
}


void TcpSocket::setSendBufferSize(int bufferSize)
{
  if (_detail->socket)
  {
    _detail->socket->setSocketOption(QTcpSocket::SendBufferSizeSocketOption, bufferSize);
  }
}


int TcpSocket::sendBufferSize() const
{
  if (_detail->socket)
  {
    _detail->socket->socketOption(QTcpSocket::SendBufferSizeSocketOption).toInt();
  }
  return 0;
}


int TcpSocket::read(char *buffer, int bufferLength) const
{
  if (_detail->socket && _detail->socket->waitForReadyRead(_detail->readTimeout))
  {
    return _detail->socket->read(buffer, bufferLength);
  }

  return 0;
}


int TcpSocket::readAvailable(char *buffer, int bufferLength) const
{
  if (!_detail->socket)
  {
    return -1;
  }

  _detail->socket->waitForReadyRead(0);
  return _detail->socket->read(buffer, bufferLength);
}


int TcpSocket::write(const char *buffer, int bufferLength) const
{
  if (!_detail->socket)
  {
    return -1;
  }

  int wrote = -1;
  int totalWritten = 0;
  while (wrote < 0 && bufferLength > 0)
  {
    wrote = _detail->socket->write(buffer, bufferLength);
    if (wrote > 0)
    {
      totalWritten += wrote;
      buffer += wrote;
      bufferLength -= wrote;
    }
    else
    {
      _detail->socket->waitForBytesWritten(0);
    }
  }

  _detail->socket->waitForBytesWritten(0);

  return wrote >= 0 ? totalWritten : -1;
}


unsigned short TcpSocket::port() const
{
  return _detail->socket ? _detail->socket->localPort() : 0;
}
