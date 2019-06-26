using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Tes.IO;
using Tes.Logging;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Main
{
  /// <summary>
  /// Status of the <see cref="NetworkThread"/>.
  /// </summary>
  public enum NetworkThreadStatus
  {
    /// <summary>
    /// Invalid status.
    /// </summary>
    None,
    /// <summary>
    /// Thread has started. Not connecting yet.
    /// </summary>
    Started,
    /// <summary>
    /// Thread has been disconnected.
    /// </summary>
    /// <remarks>
    /// It is important to still flush any pending messages once disconnected. Some sessions may be very
    /// short lived and all messages should still be processed and the initial connection handled.
    /// </remarks>
    Disconnected,
    /// <summary>
    /// Waiting for a connection.
    /// </summary>
    Connecting,
    /// <summary>
    /// Currently connected.
    /// </summary>
    Connected,
    /// <summary>
    /// Was disconnected, but waiting on a new connection.
    /// </summary>
    /// <remarks>
    /// As will <see cref="Disconnected"/>, all pending messages should be processed.
    /// </remarks>
    Reconnecting,
    /// <summary>
    /// Failed to connect.
    /// </summary>
    ConnectionFailed
  }

  public class NetworkThread : DataThread
  {
    public NetworkThread()
    {
      Status = NetworkThreadStatus.None;
      _collatedDecoder = new CollatedPacketDecoder();
    }

    public override Tes.Collections.Queue<PacketBuffer> PacketQueue { get { return _packetQueue; } }

    public NetworkThreadStatus Status { get; private set; }

    public override uint CurrentFrame
    {
      get { lock (this) { return _currentFrame; } }
      set { lock (this) { _currentFrame = value; } }
    }
    public override uint TotalFrames
    {
      get { lock (this) { return _totalFrames; } }
      set { lock (this) { _totalFrames = value; } }
    }
    public override uint TargetFrame { get { lock(this) { return _currentFrame; } } set { /* ignored */ } }
    public override bool IsLiveStream { get { return true; } }
    public override bool Started { get { return _thread != null && _thread.Running; } }
    public override bool Paused {  get { return false; } set { /* ignored */ } }
    public override bool CatchingUp { get { return Status == NetworkThreadStatus.Connected || Status == NetworkThreadStatus.Connecting; } }

    public float FrameTime
    {
      get { return _frameTime; }
      private set
      {
        _frameTime = value;
        _frameTimeWindow[_frameTimeCursor] = value;
        _frameTimeCursor = (_frameTimeCursor + 1) % _frameTimeWindow.Length;
      }
    }
    public float AverageFrameTime
    {
      get
      {
        float sum = 0;
        for (int i = 0; i < _frameTimeWindow.Length; ++i)
        {
          sum += _frameTimeWindow[i];
        }
        return sum / (float)_frameTimeWindow.Length;
      }
    }
    private float _frameTime = 0;
    private float[] _frameTimeWindow = new float[30];
    private int _frameTimeCursor = 0;

    public IPEndPoint EndPoint
    {
      get
      {
        if (_connection.EndPoint != null)
        {
          return _connection.EndPoint;
        }
        return new IPEndPoint(IPAddress.None, 0);
      }
    }

    public bool AutoReconnect
    {
      get { return _connection.AutoReconnect; }
    }

    public bool SetConnection(IPEndPoint endPoint, bool autoReconnect)
    {
      if (Started)
      {
        return false;
      }
      _connection = new Connection();
      _connection.EndPoint = endPoint;
      _connection.AutoReconnect = autoReconnect;
      _connection.Connected = false;
      return true;
    }

    public bool Start(IPEndPoint endPoint, bool autoReconnect)
    {
      if (!SetConnection(endPoint, autoReconnect))
      {
        return false;
      }
      return Start();
    }

    public override bool Start()
    {
      if (Started)
      {
        return false;
      }

      if (!_connection.IsValid)
      {
        return false;
      }

      _quitFlag = false;
      Status = NetworkThreadStatus.Started;
      _thread = new Workthread(Run(), this.OnQuit);
      _thread.Start();
      return true;
    }

    public override bool Join()
    {
      if (_thread != null)
      {
        _thread.Stop();
        _thread = null;
      }
      return true;
    }

    public override void Quit()
    {
      _quitFlag = true;
    }

    private IEnumerator Run()
    {
      // Reconnection attempts so we don't hammer the system on failure.
      float connectionPollTimeSec = 0.25f;
      TcpClient socket = null;
      PacketBuffer packetBuffer = new PacketBuffer(4 * 1024);
      System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
      ClientConnector connector = null;

      while (!_quitFlag)
      {
        // First try establish a connection.
        while (!_quitFlag && !_connection.Connected)
        {
          if (connector == null)
          {
            connector = new ClientConnector(_connection.EndPoint);
          }

          if (!connector.Connecting)
          {
            if (connector.Connected)
            {
              socket = connector.Accept();
              connector = null;
              _connection.Connected = true;
              Status = NetworkThreadStatus.Connected;
              _currentFrame = _totalFrames = 0u;
              _packetQueue.Enqueue(BuildResetPacket());
            }
            else
            {
              connector.Abort();
              connector = null;
              if (!_connection.AutoReconnect)
              {
                // Failed connection and no auto reconnect.
                Status = NetworkThreadStatus.ConnectionFailed;
                break;
              }
            }
          }

          if (socket == null)
          {
            // Wait the timeout period before attempting to reconnect.
            yield return Workthread.CreateWait(connectionPollTimeSec);
          }
        }

        timer.Start();
        // Read while connected.
        while (!_quitFlag && TcpClientUtil.Connected(socket))
        {
          // We have a connection. Read messages while we can.
          if (socket.Available > 0)
          {
            // Data available. Read from the network stream into a buffer and attempt to
            // read a valid message.
            packetBuffer.Append(socket.GetStream(), socket.Available);
            PacketBuffer completedPacket;
            bool crcOk = true;
            while ((completedPacket = packetBuffer.PopPacket(out crcOk)) != null || !crcOk)
            {
              if (crcOk)
              {
                // Decode and decompress collated packets. This will just return the same packet
                // if not collated.
                _collatedDecoder.SetPacket(completedPacket);
                while ((completedPacket = _collatedDecoder.Next()) != null)
                {
                  if (completedPacket.Header.RoutingID == (ushort)RoutingID.Control)
                  {
                    ushort controlMessageId = completedPacket.Header.MessageID;
                    if (controlMessageId == (ushort)ControlMessageID.EndFrame)
                    {
                      // Add a frame flush flag to every end frame message to ensure the render thread renders.
                      // TODO: consider a way to frame skip in a live visualisation link.
                      byte[] packetData = completedPacket.Data;
                      int memberOffset = PacketHeader.Size + Marshal.OffsetOf(typeof(ControlMessage), "ControlFlags").ToInt32();
                      uint controlFlags = BitConverter.ToUInt32(packetData, memberOffset);
                      controlFlags = Endian.FromNetwork(controlFlags);
                      // Add the flush flag
                      controlFlags |= (uint)EndFrameFlag.Flush;
                      // Convert back to bytes and copy into the packet buffer.
                      byte[] frameNumberBytes = BitConverter.GetBytes(Endian.ToNetwork(controlFlags));
                      Array.Copy(frameNumberBytes, 0, packetData, memberOffset, frameNumberBytes.Length);

                      // Update the frame
                      timer.Stop();
                      FrameTime = timer.ElapsedMilliseconds * 1e-3f;
                      timer.Reset();
                      timer.Start();
                      ++_currentFrame;
                      ++_totalFrames;
                    }
                  }

                  _packetQueue.Enqueue(completedPacket);
                }
              }
              else
              {
                // TODO: Log CRC failure.
              }
            }
          }
          else
          {
            yield return null;
          }
        }

        // Disconnected.
        if (socket != null)
        {
          socket.LingerState.Enabled = false;
          socket.Close();
          socket = null;
        }
        if (_connection.Connected)
        {
          Status = NetworkThreadStatus.Disconnected;
          _connection.Connected = false;
        }

        if (!_connection.AutoReconnect)
        {
          break;
        }
        Status = NetworkThreadStatus.Reconnecting;
      }
    }

    private void OnQuit()
    {
      _quitFlag = true;
    }

    /// <summary>
    /// Attempt to establish the specified <paramref name="connection"/>, blocking until
    /// success or failure.
    /// </summary>
    /// <returns><c>true</c>, if a connection has been established.</returns>
    /// <param name="socket">The TCP socket to connect with. Must not be null.</param>
    /// <param name="connection">The connection description.</param>
    private TcpClient AttemptConnection(Connection connection)
    {
      try
      {
        TcpClient socket = new TcpClient();
        socket.Connect(connection.EndPoint);
        if (socket.Connected)
        {
          return socket;
        }
        socket.Close();
      }
      catch (SocketException e)
      {
        switch (e.ErrorCode)
        {
          case 10061: // WSAECONNREFUSED
            break;
          default:
            Log.Exception(e);
            break;
        }
      }
      catch (System.Exception e)
      {
        Log.Exception(e);
      }

      return null;
    }

    protected struct Connection
    {
      public IPEndPoint EndPoint;
      public bool AutoReconnect;
      public bool Connected;

      public static Connection NullConnection()
      {
        return new Connection
        {
          EndPoint = null,
          AutoReconnect = false,
          Connected = false,
        };
      }

      public bool IsValid { get { return EndPoint != null && EndPoint.Address != IPAddress.None && EndPoint.Port != 0; } }
    }

    private uint _currentFrame = 0;
    private uint _totalFrames = 0;
    private Connection _connection = Connection.NullConnection();
    private Collections.Queue<PacketBuffer> _packetQueue = new Tes.Collections.Queue<PacketBuffer>();
    private Workthread _thread;
    private CollatedPacketDecoder _collatedDecoder;
    private bool _quitFlag;
  }
}
