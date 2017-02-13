//
// author: Kazys Stepanas
//
#include "3estcpserver.h"

#include "3estcpconnection.h"
#include "3estcpconnectionmonitor.h"
#include "3espacketwriter.h"

#include <algorithm>
#include <mutex>

using namespace tes;

Server *Server::create(const ServerSettings &settings, const ServerInfoMessage *serverInfo)
{
  return new TcpServer(settings, serverInfo);
}


TcpServer::TcpServer(const ServerSettings &settings, const ServerInfoMessage *serverInfo)
: _monitor(nullptr)
, _settings(settings)
, _active(true)
{
  _monitor = new TcpConnectionMonitor(*this);

  if (serverInfo)
  {
    memcpy(&_serverInfo, serverInfo, sizeof(_serverInfo));
  }
  else
  {
    initDefaultServerInfo(&_serverInfo);
  }
}


TcpServer::~TcpServer()
{
  delete _monitor;
}


void TcpServer::dispose()
{
  delete this;
}


unsigned TcpServer::flags() const
{
  return _settings.flags;
}


void TcpServer::close()
{
  _monitor->stop();
  _monitor->join();

  std::lock_guard<Lock> guard(_lock);

  for (TcpConnection *con : _connections)
  {
    con->close();
  }
}


void TcpServer::setActive(bool enable)
{
  _active = enable;
}


bool TcpServer::active() const
{
  return _active;
}


const char *TcpServer::address() const
{
  return "TcpServer";
}


uint16_t TcpServer::port() const
{
  return _monitor->mode() != ConnectionMonitor::None ? _settings.listenPort : 0;
}


bool TcpServer::isConnected() const
{
  std::lock_guard<Lock> guard(_lock);
  return !_connections.empty();
}


int TcpServer::create(const Shape &shape)
{
  if (!_active)
  {
    return 0;
  }

  std::lock_guard<Lock> guard(_lock);
  int transferred = 0;
  bool error = false;
  for (TcpConnection *con : _connections)
  {
    int txc = con->create(shape);
    if (txc >= 0)
    {
      transferred += txc;
    }
    else
    {
      error = true;
    }
  }

  return (!error) ? transferred : -transferred;
}


int TcpServer::destroy(const Shape &shape)
{
  if (!_active)
  {
    return 0;
  }

  std::lock_guard<Lock> guard(_lock);
  int transferred = 0;
  bool error = false;
  for (TcpConnection *con : _connections)
  {
    int txc = con->destroy(shape);
    if (txc >= 0)
    {
      transferred += txc;
    }
    else
    {
      error = true;
    }
  }

  return (!error) ? transferred : -transferred;
}


int TcpServer::update(const Shape &shape)
{
  if (!_active)
  {
    return 0;
  }

  std::lock_guard<Lock> guard(_lock);
  int transferred = 0;
  bool error = false;
  for (TcpConnection *con : _connections)
  {
    int txc = con->update(shape);
    if (txc >= 0)
    {
      transferred += txc;
    }
    else
    {
      error = true;
    }
  }

  return (!error) ? transferred : -transferred;
}


int TcpServer::updateFrame(float dt, bool flush)
{
  if (!_active)
  {
    return 0;
  }

  std::unique_lock<Lock> guard(_lock);
  int transferred = 0;
  bool error = false;
  for (TcpConnection *con : _connections)
  {
    int txc = con->updateFrame(dt, flush);
    if (txc >= 0)
    {
      transferred += txc;
    }
    else
    {
      error = true;
    }
  }

  // Async mode: commit new connections after the current frame is sent.
  // We do it after a frame update to prevent doubling up on creation messages.
  // Consider this: the application code uses a callback on new connections
  // to create objects to reflect the current state, invoked when commitConnections()
  // is called. If we did this before the end of frame transfer, then we may
  // generate create messages in the callback for objects which have buffered
  // create messages. Alternatively, if the server is not in collated mode, the
  // we'll get different behaviour between collated and uncollated modes.
  guard.unlock();
  if (_monitor->mode() == tes::ConnectionMonitor::Asynchronous)
  {
    _monitor->commitConnections();
  }

  return (!error) ? transferred : -transferred;
}


int TcpServer::updateTransfers(unsigned byteLimit)
{
  if (!_active)
  {
    return 0;
  }

  std::lock_guard<Lock> guard(_lock);
  int transferred = 0;
  bool error = false;
  for (TcpConnection *con : _connections)
  {
    int txc = con->updateTransfers(byteLimit);
    if (txc >= 0)
    {
      transferred += txc;
    }
    else
    {
      error = true;
    }
  }

  return (!error) ? transferred : -transferred;
}


bool TcpServer::sendServerInfo(const ServerInfoMessage &info)
{
  return false;
}


unsigned TcpServer::referenceResource(const Resource *resource)
{
  if (!_active)
  {
    return 0;
  }

  std::lock_guard<Lock> guard(_lock);
  unsigned lastCount = 0;
  for (TcpConnection *con : _connections)
  {
    lastCount = con->referenceResource(resource);
  }
  return lastCount;
}


unsigned TcpServer::releaseResource(const Resource *resource)
{
  if (!_active)
  {
    return 0;
  }

  std::lock_guard<Lock> guard(_lock);
  unsigned lastCount = 0;
  for (TcpConnection *con : _connections)
  {
    lastCount = con->releaseResource(resource);
  }
  return lastCount;
}


int TcpServer::send(const PacketWriter &packet)
{
  return send(packet.data(), packet.packetSize());
}


int TcpServer::send(const CollatedPacket &collated)
{
  if (!_active)
  {
    return 0;
  }

  int sent = 0;
  bool failed = false;
  std::lock_guard<Lock> guard(_lock);
  for (TcpConnection *con : _connections)
  {
    sent = con->send(collated);
    if (sent == -1)
    {
      failed = true;
    }
  }

  return (!failed) ? sent : -1;
}


int TcpServer::send(const uint8_t *data, int byteCount)
{
  if (!_active)
  {
    return 0;
  }

  int sent = 0;
  bool failed = false;
  std::lock_guard<Lock> guard(_lock);
  for (TcpConnection *con : _connections)
  {
    sent = con->send(data, byteCount);
    if (sent == -1)
    {
      failed = true;
    }
  }

  return (!failed) ? sent : -1;
}


ConnectionMonitor *TcpServer::connectionMonitor()
{
  return _monitor;
}


unsigned TcpServer::connectionCount() const
{
  std::lock_guard<Lock> guard(_lock);
  return unsigned(_connections.size());
}


Connection *TcpServer::connection(unsigned index)
{
  std::lock_guard<Lock> guard(_lock);
  if (index < _connections.size())
  {
    return _connections[index];
  }
  return nullptr;
}


const Connection *TcpServer::connection(unsigned index) const
{
  std::lock_guard<Lock> guard(_lock);
  if (index < _connections.size())
  {
    return _connections[index];
  }
  return nullptr;
}


void TcpServer::updateConnections(const std::vector<TcpConnection *> &connections, const std::function<void(Server &, Connection &)> &callback)
{
  if (!_active)
  {
    return;
  }

  std::lock_guard<Lock> guard(_lock);
  std::vector<TcpConnection *> newConnections;

  if (!connections.empty())
  {
    newConnections.reserve(32);
    for (TcpConnection *con : connections)
    {
      bool existing = false;
      for (TcpConnection *exist : _connections)
      {
        if (exist == con)
        {
          existing = true;
          break;
        }
      }

      if (!existing)
      {
        newConnections.push_back(con);
      }
    }
  }

  _connections.clear();
  std::for_each(connections.begin(), connections.end(),
                [this] (TcpConnection *con){ _connections.push_back(con);});

  // Send server info to new connections.
  for (TcpConnection *con : newConnections)
  {
    con->sendServerInfo(_serverInfo);
    if (callback)
    {
      (callback)(*this, *con);
    }
  }
}
