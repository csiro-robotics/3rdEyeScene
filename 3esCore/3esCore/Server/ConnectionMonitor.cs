using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Tes.Net;

namespace Tes.Server
{
  /// <summary>
  /// A TCP/IP based implementation of <see cref="IConnectionMonitor"/>.
  /// </summary>
  /// <remarks>
  /// To be used with <see cref="TcpServer"/>.
  /// 
  /// The <see cref="IConnectionMonitor"/> interface generally contains better
  /// documentation for the class members.
  /// </remarks>
  public class ConnectionMonitor : IConnectionMonitor
  {
    /// <summary>
    /// The connection mode.
    /// </summary>
    public ConnectionMonitorMode Mode { get; protected set; }

    /// <summary>
    /// Port on which the server is listening (if relevant).
    /// </summary>
    public int Port
    {
      get
      {
        if (_listen != null)
        {
          IPEndPoint ipEndPoint = _listen.LocalEndpoint as IPEndPoint;
          //DnsEndPoint dnsEndPoint = _listen.LocalEndpoint as DnsEndPoit;
          if (ipEndPoint != null)
          {
            return ipEndPoint.Port;
          }
        }
        return 0;
      }
    }

    /// <summary>
    /// Create a connection monitor for the given server.
    /// </summary>
    /// <param name="server">The server object.</param>
    public ConnectionMonitor(TcpServer server)
    {
      _server = server;
      Mode = ConnectionMonitorMode.None;
    }

    /// <summary>
    /// Start listening for connections in the given <paramref name="mode"/>.
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool Start(ConnectionMonitorMode mode)
    {
      if (mode == ConnectionMonitorMode.None || Mode != ConnectionMonitorMode.None && mode != Mode)
      {
        return false;
      }

      if (mode == Mode)
      {
        return true;
      }

      _lock = new Threading.SpinLock();

      switch (mode)
      {
      case ConnectionMonitorMode.Synchronous:
        Listen();
        Mode = ConnectionMonitorMode.Synchronous;
        break;

      case ConnectionMonitorMode.Asynchronous:
        _quitFlag = false;
        if (Listen())
        {
          _thread = new System.Threading.Thread(MonitorThread);
          _thread.Start();
          Mode = ConnectionMonitorMode.Asynchronous;
        }
        break;

      default:
        _lock = null;
        return false;
      }

      return Mode != ConnectionMonitorMode.None;
    }


    /// <summary>
    /// Stop listening for connections.
    /// </summary>
    public void Stop()
    {
      switch (Mode)
      {
      case ConnectionMonitorMode.Synchronous:
        StopListening();
        Mode = ConnectionMonitorMode.None;
        break;

      case ConnectionMonitorMode.Asynchronous:
        _quitFlag = true;
        break;

      default:
        break;
      }
    }

    /// <summary>
    /// Join the background thread if running <see cref="ConnectionMonitorMode.Asynchronous"/>.
    /// </summary>
    /// <remarks>
    /// Does nothing in other mods.
    /// </remarks>
    public void Join()
    {
      if (_thread != null)
      {
        if (!_quitFlag && (Mode == ConnectionMonitorMode.Asynchronous || Mode == ConnectionMonitorMode.None))
        {
          throw new Exception("ConnectionMonitor.Join() called on asynchronous connection monitor without calling Stop()");
        }
        _thread.Join();
        _thread = null;
      }
      _lock = null;
    }

    /// <summary>
    /// Block until the connection thread has started.
    /// </summary>
    /// <returns><c>true</c> in synchronous mode, or in asynchronous mode and the connection monitor has started.</returns>
    public bool WaitForStart()
    {
      if (Mode == ConnectionMonitorMode.Synchronous)
      {
        return true;
      }

      if (Mode == ConnectionMonitorMode.None || _thread == null)
      {
        return false;
      }

      for (;;)
      {
        switch (_thread.ThreadState)
        {
          case System.Threading.ThreadState.Aborted:
          case System.Threading.ThreadState.Stopped:
          case System.Threading.ThreadState.Suspended:
          case System.Threading.ThreadState.Unstarted:
            return false;

          case System.Threading.ThreadState.WaitSleepJoin:
          case System.Threading.ThreadState.Running:
            return true;

          default:
            System.Threading.Thread.Sleep(0);
            break;
        }
      }
    }

    /// <summary>
    /// Update the connection list.
    /// </summary>
    /// <remarks>
    /// Must be called from the main thread when running <see cref="ConnectionMonitorMode.Synchronous"/>.
    /// </remarks>
    public void MonitorConnections()
    {
      _lock.Lock();
      try
      {
        // Expire lost connections.
        for (int i = 0; i < _connections.Count; )
        {
          IConnection connection = _connections[i];
          if (connection.Connected)
          {
            ++i;
          }
          else
          {
            _connections.RemoveAt(i);
          }
        }

        // Look for new connections.
        if (_listen != null)
        {
          TcpClient accepted = null;
          if (_listen.Pending())
          {
            // New connection.
            accepted = _listen.AcceptTcpClient();
            //accepted.NoDelay = true;
            IConnection connection = new TcpConnection(accepted, _server.Settings.Flags);
            _connections.Add(connection);
          }
        }
      }
      finally
      {
        _lock.Unlock();
      }
    }

    /// <summary>
    /// Commit new and expired connections to the list and update the <see cref="IServer"/>.
    /// </summary>
    /// <param name="callback">The callback to invoke for each new connection. May be null.</param>
    /// <remarks>
    /// Must be called from the main thread, regardless of mode, to ensure the
    /// server reflects the current connection list.
    /// </remarks>
    public void CommitConnections(NewConnectionCallback callback = null)
    {
      bool locked = false;
      _lock.Lock();
      locked = true;
      try
      { 
        _server.UpdateConnections(_connections, callback);

        _lock.Unlock();
        locked = false;
      }
      finally
      {
        if (locked)
        { 
          _lock.Unlock();
        }
      }
    }

    /// <summary>
    /// Wait for up to <paramref name="timeoutMs"/> milliseconds for at least one new connection before continuing.
    /// </summary>
    /// <param name="timeoutMs">The time to wait in milliseconds.</param>
    /// <returns>True if a new connection has been found.</returns>
    /// <remarks>
    /// The method returns once either a new connection exists or <paramref name="timeoutMs"/> has elapsed.
    /// When there is a new connection, it has yet to be committed.
    /// </remarks>
    public bool WaitForConnections(uint timeoutMs)
    {
      Stopwatch timer = new Stopwatch();
      timer.Start();

      do
      {
        _lock.Lock();
        try
        {
          if (_connections.Count > 1)
          {
            return true;
          }
        }
        finally
        {
          _lock.Unlock();
        }

        switch (Mode)
        {
          case ConnectionMonitorMode.None:
            // Not actually looking for connections. Abort.
            return false;
          case ConnectionMonitorMode.Synchronous:
            // Synchronous mode. Need to update monitor.
            MonitorConnections();
            break;
          case ConnectionMonitorMode.Asynchronous:
            // Asynchronous mode. Yield.
            System.Threading.Thread.Sleep(0);
            break;
        }
      } while (timer.ElapsedMilliseconds < timeoutMs);

      // Final check.
      if (Mode == ConnectionMonitorMode.Synchronous)
      {
        MonitorConnections();
      }

      _lock.Lock();
      try
      {
        if (_connections.Count > 1)
        {
          return true;
        }
      }
      finally
      {
        _lock.Unlock();
      }
      return false;
    }

    /// <summary>
    /// Start listening for connections.
    /// </summary>
    private bool Listen()
    {
      if (_listen != null)
      {
        return true;
      }

      IPAddress local = IPAddress.Loopback;
      int port = _server.Settings.ListenPort;
      while (_listen == null && port <= _server.Settings.ListenPort + _server.Settings.PortRange)
      {
        _listen = new TcpListener(local, port++);
        try
        {
          _listen.Start();
          return true;
        }
        catch (SocketException /*e*/)
        {
          _listen = null;
        }
      }

      return false;
    }

    /// <summary>
    /// Stop listening for connections.
    /// </summary>
    private void StopListening()
    {
      foreach (IConnection connection in _connections)
      {
        connection.Close();
      }
      _connections.Clear();
      if (_listen != null)
      {
        _listen.Stop();
        _listen = null;
      }
    }

    /// <summary>
    /// Asynchronous mode thread entry point.
    /// </summary>
    private void MonitorThread()
    {
      if (_listen == null)
      {
        return;
      }

      while (!_quitFlag)
      {
        MonitorConnections();
        System.Threading.Thread.Sleep(50);
      }

      StopListening();
    }

    /// <summary>
    /// The owning server.
    /// </summary>
    private TcpServer _server;
    /// <summary>
    /// The thread for asynchronous mode.
    /// </summary>
    private System.Threading.Thread _thread = null;
    /// <summary>
    /// The current connections list.
    /// </summary>
    private List<IConnection> _connections = new List<IConnection>();
    /// <summary>
    /// Mutex guard for maintaining connections.
    /// </summary>
    private Threading.SpinLock _lock = null;
    /// <summary>
    /// TCP server socket.
    /// </summary>
    private TcpListener _listen = null;
    /// <summary>
    /// True when the asynchronous thread should quit.
    /// </summary>
    private volatile bool _quitFlag = false;
  }
}
