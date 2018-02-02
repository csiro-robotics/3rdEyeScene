using System.Collections.Generic;
using Tes.IO;
using Tes.Net;
using Tes.Shapes;
using Tes.Threading;

namespace Tes.Server
{
  /// <summary>
  /// Implements a 3rd Eye Scene server over TCP/IP.
  /// </summary>
  public class TcpServer : IServer
  {
    /// <summary>
    /// The settings for this server.
    /// </summary>
    public ServerSettings Settings { get; protected set; }
    /// <summary>
    /// The connection monitor used to manage connections.
    /// </summary>
    public IConnectionMonitor ConnectionMonitor { get; protected set; }
    /// <summary>
    /// The server info message sent to each new connection identifying this server's world configuration.
    /// </summary>
    public ServerInfoMessage ServerInfo { get; protected set; }
    /// <summary>
    /// The number of current connections.
    /// </summary>
    public int ConnectionCount { get { _lock.Lock(); try { return _connections.Count; } finally { _lock.Unlock(); } } }

    /// <summary>
    /// Create a new <see cref="TcpServer"/> with the given <paramref name="settings"/>.
    /// </summary>
    /// <param name="settings">The settings to instantiate with.</param>
    public TcpServer(ServerSettings settings)
      : this(settings, ServerInfoMessage.Default)
    {
    }

    /// <summary>
    /// Create a new <see cref="TcpServer"/> with the given <paramref name="settings"/>
    /// and <paramref name="serverInfo"/>.
    /// </summary>
    /// <param name="settings">The settings to instantiate with.</param>
    /// <param name="serverInfo">The <see cref="ServerInfoMessage"/> sent to new clients.</param>
    public TcpServer(ServerSettings settings, ServerInfoMessage serverInfo)
    {
      Settings = settings;
      ServerInfo = serverInfo;
      ConnectionMonitor = new ConnectionMonitor(this);
    }

    /// <summary>
    /// Send a packet to all clients.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <returns>The number of bytes sent. Negative on any failure.</returns>
    /// <remarks>
    /// The <paramref name="packet"/> must be finalised before this call.
    /// </remarks>
    public int Send(PacketBuffer packet)
    {
      _lock.Lock();
      int transferred = 0;
      bool error = false;
      try
      {
        for (int i = 0; i < _connections.Count; ++i)
        {
          int txc = _connections[i].Send(packet.Data, 0, packet.Count);
          if (txc >= 0)
          {
            transferred += txc;
          }
          else
          {
            error = true;
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }

      return (!error) ? transferred : -transferred;
    }

    /// <summary>
    /// Send a shape create message to connected clients.
    /// </summary>
    /// <param name="shape">The shape to create.</param>
    /// <returns>The number of bytes sent. Negative if a client failed to send.</returns>
    public int Create(Shape shape)
    {
      _lock.Lock();
      int transferred = 0;
      bool error = false;
      try
      {
        for (int i = 0; i < _connections.Count; ++i)
        {
          int txc = _connections[i].Create(shape);
          if (txc >= 0)
          {
            transferred += txc;
          }
          else
          {
            error = true;
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }

      return (!error) ? transferred : -transferred;
    }

    /// <summary>
    /// Send a shape destroy message to connected clients.
    /// </summary>
    /// <param name="shape">The shape to destroy.</param>
    /// <returns>The number of bytes sent. Negative if a client failed to send.</returns>
    public int Destroy(Shape shape)
    {
      _lock.Lock();
      int transferred = 0;
      bool error = false;
      try
      {
        for (int i = 0; i < _connections.Count; ++i)
        {
          int txc = _connections[i].Destroy(shape);
          if (txc >= 0)
          {
            transferred += txc;
          }
          else
          {
            error = true;
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }

      return (!error) ? transferred : -transferred;
    }

    /// <summary>
    /// Send a shape update message to connected clients.
    /// </summary>
    /// <param name="shape">The shape to update.</param>
    /// <returns>The number of bytes sent. Negative if a client failed to send.</returns>
    public int Update(Shape shape)
    {
      _lock.Lock();
      int transferred = 0;
      bool error = false;
      try
      {
        for (int i = 0; i < _connections.Count; ++i)
        {
          int txc = _connections[i].Update(shape);
          if (txc >= 0)
          {
            transferred += txc;
          }
          else
          {
            error = true;
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }

      return (!error) ? transferred : -transferred;
    }

    /// <summary>
    /// Update pending resource transfers on all clients.
    /// </summary>
    /// <param name="byteLimit">Limits the number of byte to send per client. Zero for no limit.</param>
    /// <returns>The number of bytes sent. Negative if a client failed to send.</returns>
    public int UpdateTransfers(int byteLimit)
    {
      _lock.Lock();
      int transferred = 0;
      bool error = false;
      try
      {
        for (int i = 0; i < _connections.Count; ++i)
        {
          int txc = _connections[i].UpdateTransfers(byteLimit);
          if (txc >= 0)
          {
            transferred += txc;
          }
          else
          {
            error = true;
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }

      return (!error) ? transferred : -transferred;
    }

    /// <summary>
    /// Send a frame update message to each client.
    /// </summary>
    /// <param name="dt">The time elapsed for this frame (seconds).</param>
    /// <param name="flush">Flush transient shapes this frame?</param>
    /// <returns>The number of bytes sent. Negative if a client failed to send.</returns>
    public int UpdateFrame(float dt, bool flush = true)
    {
      _lock.Lock();
      try
      {
        int transferred = 0;
        bool error = false;
        for (int i = 0; i < _connections.Count; ++i)
        {
          TcpConnection connection = _connections[i];
          int txc = connection.UpdateFrame(dt, flush);
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
      finally
      {
        _lock.Unlock();
      }
    }

    /// <summary>
    /// Request direct access to one of the connections.
    /// </summary>
    /// <param name="index">The connection index [0, <see cref="ConnectionCount"/>)</param>
    /// <returns>The connection, or null if <paramref name="index"/> is out of range.</returns>
    public IConnection Connection(int index)
    {
      _lock.Lock();
      try
      {
        if (0 <= index && index < _connections.Count)
        {
          return _connections[index];
        }
      }
      finally
      {
        _lock.Unlock();
      }

      return null;
    }

    /// <summary>
    /// Enumerates the current connections.
    /// </summary>
    public IEnumerable<IConnection> Connections
    {
      get
      {
        _lock.Lock();
        try
        {
          // Copy to a new array for thread safety.
          TcpConnection[] connections = new TcpConnection[_connections.Count];
          _connections.CopyTo(connections);
          return connections;
        }
        finally
        {
          _lock.Unlock();
        }
      }
    }

    /// <summary>
    /// Update the list of current connections.
    /// </summary>
    /// <param name="connections">The list of active connections.</param>
    /// <param name="callback">A callback to invoke for each new connection in <paramref name="connections"/>.
    /// May be null</param>
    /// <remarks>
    /// The internal connections list is updated to match <paramref name="connections"/>.
    /// 
    /// This method is called by the <see cref="ConnectionMonitor"/> and should not be
    /// called directly.
    /// </remarks>
    public void UpdateConnections(IList<IConnection> connections, NewConnectionCallback callback = null)
    {
      _lock.Lock();
      try
      {
        List<IConnection> newConnections = new List<IConnection>();

        if (connections.Count > 0)
        {
          // Collate new connections.
          newConnections = new List<IConnection>();
          for (int i = 0; i < connections.Count; ++i)
          {
            bool existing = false;
            for (int j = 0; j < _connections.Count; ++j)
            {
              if (connections[i] == _connections[j])
              {
                existing = true;
                break;
              }
            }

            if (!existing)
            {
              newConnections.Add(connections[i]);
            }
          }
        }

        // Update internal connections list.
        _connections.Clear();
        for (int i = 0; i < connections.Count; ++i)
        {
          _connections.Add((TcpConnection)connections[i]);
        }

        // Add cache only to new connections.
        for (int i = 0; i < newConnections.Count; ++i)
        {
          newConnections[i].SendServerInfo(ServerInfo);
          if (callback != null)
          {
            callback(this, newConnections[i]);
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }
    }

    /// <summary>
    /// Close the server connection, stopping the connection monitor and disconnecting all clients.
    /// </summary>
    public void Close()
    {
      ConnectionMonitor.Stop();
      ConnectionMonitor.Join();

      _lock.Lock();
      try
      {
        foreach (TcpConnection connection in _connections)
        {
          connection.Close();
        }
        _connections.Clear();
      }
      finally
      {
        _lock.Unlock();
      }
    }

    /// <summary>
    /// Mutex lock on <see cref="_connections"/>.
    /// </summary>
    private SpinLock _lock = new SpinLock();
    /// <summary>
    /// Active connections list.
    /// </summary>
    private List<TcpConnection> _connections = new List<TcpConnection>();
  }
}
