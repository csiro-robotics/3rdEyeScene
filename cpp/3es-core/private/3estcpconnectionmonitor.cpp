//
// author: Kazys Stepanas
//
#include "3estcpconnectionmonitor.h"

#include "3estcpconnection.h"
#include "3estcpserver.h"

#include <3estcplistensocket.h>
#include <3estcpsocket.h>

#include <chrono>
#include <cstdio>
#include <mutex>

using namespace tes;

TcpConnectionMonitor::TcpConnectionMonitor(TcpServer &server)
  : _server(server)
  , _listen(nullptr)
  , _mode(None)
  , _thread(nullptr)
  , _listenPort(0)
  , _errorCode(0)
{
  _running = false;
  _quitFlag = false;
}


TcpConnectionMonitor::~TcpConnectionMonitor()
{
  stop();
  join();

  for (TcpConnection *con : _expired)
  {
    delete con;
  }

  for (TcpConnection *con : _connections)
  {
    delete con;
  }

  delete _listen;
  delete _thread;
}


int TcpConnectionMonitor::lastErrorCode() const
{
  return _errorCode;
}


int TcpConnectionMonitor::clearErrorCode()
{
  int lastError = _errorCode;
  _errorCode = 0;
  return lastError;
}


int TcpConnectionMonitor::port() const
{
  return _listenPort;
}


bool TcpConnectionMonitor::start(Mode mode)
{
  if (mode == None || _mode != None && mode != _mode)
  {
    return false;
  }

  if (mode == _mode)
  {
    return true;
  }

  switch (mode)
  {
  case Synchronous:
    if (listen())
    {
      _running = true;
      _mode = Synchronous;
    }
    else
    {
      _errorCode = CE_ListenFailure;
      stopListening();
    }
    break;

  case Asynchronous:
  {
    delete _thread; // Pointer may linger after quit.
    _thread = new std::thread(std::bind(&TcpConnectionMonitor::monitorThread, this));
    // Wait for the thread to start. We look for _running or an _errorCode.
    auto waitStart = std::chrono::steady_clock::now();
    unsigned elapsedMs = 0;
    while (!_running && !_errorCode && elapsedMs <= _server.settings().asyncTimeoutMs)
    {
      std::this_thread::yield();
      elapsedMs = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now() - waitStart).count();
    }

    // Running will be true if the thread started ok.
    if (_running)
    {
      _mode = Asynchronous;
    }

    if (!_running && !_errorCode && elapsedMs >= _server.settings().asyncTimeoutMs)
    {
      _errorCode = CE_Timeout;
    }
    break;
  }

  default:
    break;
  }

  return _mode != None;
}


void TcpConnectionMonitor::stop()
{
  switch (_mode)
  {
  case Synchronous:
    _running = false;
    stopListening();
    _mode = None;
    break;

  case Asynchronous:
    _quitFlag = true;
    break;

  default:
    break;
  }
}


void TcpConnectionMonitor::join()
{
  if (_thread)
  {
    if (!_quitFlag && (_mode == Asynchronous || _mode == None))
    {
      fprintf(stderr, "ConnectionMonitor::join() called on asynchronous connection monitor without calling stop()\n");
    }
    _thread->join();
    delete _thread;
    _thread = nullptr;
  }
}


bool TcpConnectionMonitor::isRunning() const
{
  return _running;
}


ConnectionMonitor::Mode TcpConnectionMonitor::mode() const
{
  return _mode;
}


int TcpConnectionMonitor::waitForConnection(unsigned timeoutMs)
{
  std::unique_lock<Lock> lock(_connectionLock);
  if (!_connections.empty())
  {
    return int(_connections.size());
  }
  lock.unlock();

  // Wait for start.
  if (mode() == tes::ConnectionMonitor::Asynchronous)
  {
    while (!isRunning() && mode() != tes::ConnectionMonitor::None);
  }

  // Update connections if required.
  auto startTime = std::chrono::steady_clock::now();
  bool timedout = false;
  int connectionCount = 0;
  while (isRunning() && !timedout && connectionCount == 0)
  {
    if (mode() == tes::ConnectionMonitor::Synchronous)
    {
      monitorConnections();
    }
    else
    {
      std::this_thread::yield();
    }
    timedout = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now() - startTime).count() >= timeoutMs;
    lock.lock();
    connectionCount = int(_connections.size());
    lock.unlock();
  }

  return connectionCount;
}


void TcpConnectionMonitor::monitorConnections()
{
  // Lock for connection expiry.
  std::unique_lock<Lock> lock(_connectionLock);

  // Expire lost connections.
  for (auto iter = _connections.begin(); iter != _connections.end();)
  {
    TcpConnection *connection = *iter;
    if (connection->isConnected())
    {
      ++iter;
    }
    else
    {
      _expired.push_back(connection);
      iter = _connections.erase(iter);
    }
  }

  // Unlock while we check for new connections.
  lock.unlock();

  // Look for new connections.
  if (_listen)
  {
    if (TcpSocket *newSocket = _listen->accept(0))
    {
      // Options to try and reduce socket latency.
      // Attempt to prevent periodic latency on osx.
      newSocket->setNoDelay(true);
      newSocket->setWriteTimeout(0);
      newSocket->setReadTimeout(0);
#ifdef __apple__
      // On OSX, set send buffer size. Not sure automatic sizing is working.
      // Remove this code if it is.
      newSocket->setSendBufferSize(0xffff);
#endif // __apple__

      TcpConnection* newConnection = new TcpConnection(newSocket, _server.settings().flags, _server.settings().clientBufferSize);
      // Lock for new connection.
      lock.lock();
      _connections.push_back(newConnection);
      lock.unlock();
    }
  }
}


void TcpConnectionMonitor::setConnectionCallback(void(*callback)(Server &, Connection &, void *), void *user)
{
  _onNewConnection = std::bind(callback, std::placeholders::_1, std::placeholders::_2, user);
}


void TcpConnectionMonitor::setConnectionCallback(const std::function<void(Server &, Connection &)> &callback)
{
  _onNewConnection = callback;
}


void TcpConnectionMonitor::commitConnections()
{
  std::unique_lock<Lock> lock(_connectionLock);
  _server.updateConnections(_connections, _onNewConnection);

  lock.unlock();

  // Delete expired connections.
  for (TcpConnection *con : _expired)
  {
    delete con;
  }
  _expired.resize(0);
}


bool TcpConnectionMonitor::listen()
{
  if (_listen)
  {
    return true;
  }

  _listen = new TcpListenSocket;

  bool listening = false;

  uint16_t port = _server.settings().listenPort;
  while (!listening && port <= _server.settings().listenPort + _server.settings().portRange)
  {
    listening = _listen->listen(port++);
  }

  _listenPort = (listening) ? _listen->port() : 0;

  return listening;
}


void TcpConnectionMonitor::stopListening()
{
  _listenPort = 0;

  // Close all connections.
  for (TcpConnection *con : _connections)
  {
    con->close();
  }

  delete _listen;
  _listen = nullptr;
}


void TcpConnectionMonitor::monitorThread()
{
  if (!listen())
  {
    _errorCode = CE_ListenFailure;
    stopListening();
    return;
  }
  _running = true;

  while (!_quitFlag)
  {
    monitorConnections();
    std::this_thread::sleep_for(std::chrono::milliseconds(50));
  }

  _running = false;
  stopListening();
  _mode = None;
}
