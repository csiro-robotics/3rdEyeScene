using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Tes.Net;
using Tes.IO;
using Tes.Shapes;
using Tes.Threading;
using Tes.Util;

namespace Tes.Server
{
  /// <summary>
  /// A client connection over TCP/IP.
  /// </summary>
  class TcpConnection : IConnection
  {
    /// <summary>
    /// Instantiates a <code>TcpClient</code> using the given socket and flags.
    /// </summary>
    /// <param name="socket">Socket.</param>
    /// <param name="serverFlags">Server flags.</param>
    public TcpConnection(TcpClient socket, ServerFlag serverFlags)
    {
      _client = socket;
      EndPoint = _client.Client.RemoteEndPoint as IPEndPoint;
      ServerFlags = serverFlags;
      _collator = new CollatedPacketEncoder((serverFlags & ServerFlag.Compress) == ServerFlag.Compress);
      _collator.Reset();
      _currentResourceProgress.Reset();
    }

    /// <summary>
    /// The end point of the connection.
    /// </summary>
    /// <value>The end point.</value>
    public IPEndPoint EndPoint { get; private set; }
    /// <summary>
    /// The client address as extracted from the <see cref="EndPoint"/>
    /// </summary>
    /// <value>The address.</value>
    public string Address { get { return EndPoint.Address.ToString(); } }
    /// <summary>
    /// The port on which the client is connected.
    /// </summary>
    /// <value>The port.</value>
    public int Port { get { return EndPoint.Port; } }
    /// <summary>
    /// True if the client is still connected.
    /// </summary>
    /// <value>The connected.</value>
    public bool Connected { get { return _client != null && _client.Connected; } }
    /// <summary>
    /// The flags with which the client was connected.
    /// </summary>
    /// <value>The server flags.</value>
    /// <remarks>
    /// Not all flags are used with the client, but collation and compression are most notably used.
    /// </remarks>
    public ServerFlag ServerFlags { get; private set; }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Close()
    {
      if (_client != null)
      {
        _client.Close();
      }
    }

    /// <summary>
    /// Frame update implementation. See base class.
    /// </summary>
    /// <param name="dt">Elapsed time for the frame just passed (seconds).</param>
    /// <param name="flush">Flush transient objects?</param>
    /// <returns>The number of bytes sent to trigger the frame update.</returns>
    public int UpdateFrame(float dt, bool flush = true)
    {
      int wrote = -1;
      ControlMessage msg = new ControlMessage();
      msg.ControlFlags = (flush) ? (ushort)0 : (ushort)EndFrameFlag.Persist;
      // Convert dt to desired time unit.
      msg.Value32 = (uint)(dt * _secondsToTimeUnit);
      msg.Value64 = 0;
      _packetLock.Lock();
      try
      {
        // Send frame number too?
        _packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.EndFrame);
        if (msg.Write(_packet))
        {
          _packet.FinalisePacket();
          wrote = SendInternal(_packet.Data, 0, _packet.Count, true);
          FlushCollatedPacket();
        }
      }
      finally
      {
        _packetLock.Unlock();
      }
      return wrote;
    }

    /// <summary>
    /// Sends a <see cref="CreateMessage"/> for the given <paramref name="shape"/>
    /// </summary>
    /// <param name="shape">The shape for which to send a create message.</param>
    public int Create(Shape shape)
    {
      _packetLock.Lock();
      try
      {
        if (shape.WriteCreate(_packet))
        {
          _packet.FinalisePacket();
          int writeSize = Send(_packet.Data, _packet.Cursor, _packet.Count);

          if (shape.IsComplex)
          {
            uint progress = 0;
            int res = 0;
            while ((res = shape.WriteData(_packet, ref progress)) >= 0)
            {
              if (!_packet.FinalisePacket())
              {
                return -1;
              }

              writeSize += Send(_packet.Data, _packet.Cursor, _packet.Count);

              if (res == 0)
              {
                break;
              }
            }

            if (res < 0)
            {
              return res;
            }
          }
          AddResources(shape);
          return writeSize;
        }
      }
      finally
      {
        _packetLock.Unlock();
      }

      return -1;
    }


    /// <summary>
    /// Sends a <see cref="DestroyMessage"/> for the given <paramref name="shape"/>
    /// </summary>
    /// <param name="shape">The shape for which to send a create message.</param>
    public int Destroy(Shape shape)
    {
      _packetLock.Lock();
      try
      {
        RemoveResources(shape);

        if (shape.WriteDestroy(_packet))
        {
          _packet.FinalisePacket();
          return Send(_packet.Data, _packet.Cursor, _packet.Count);
        }
      }
      finally
      {
        _packetLock.Unlock();
      }
      return -1;
    }


    /// <summary>
    /// Sends an <see cref="UpdateMessage"/> for the given <paramref name="shape"/>
    /// </summary>
    /// <param name="shape">The shape for which to send a create message.</param>
    public int Update(Shape shape)
    {
      _packetLock.Lock();
      try
      {
        if (shape.WriteUpdate(_packet))
        {
          _packet.FinalisePacket();
          return Send(_packet.Data, _packet.Cursor, _packet.Count);
        }
      }
      finally
      {
        _packetLock.Unlock();
      }
      return -1;
    }


    /// <summary>
    /// Sends the given <see cref="ServerInfoMessage"/> to the client.
    /// </summary>
    /// <param name="info">Details of the server to send to the client.</param>
    /// <remarks>
    /// Should always be send as the first message to the client.
    /// </remarks>
    public bool SendServerInfo(ServerInfoMessage info)
    {
      _serverInfo = info;
      const float secondsToMicroseconds = 1e6f;
      _secondsToTimeUnit = secondsToMicroseconds / (_serverInfo.TimeUnit != 0 ? _serverInfo.TimeUnit : 1.0f);

      if (Connected)
      {
        _packetLock.Lock();
        try
        {
          _packet.Reset((ushort)RoutingID.ServerInfo, 0);
          if (info.Write(_packet))
          {
            _packet.FinalisePacket();
            _sendLock.Lock();
            try
            {
              // Do not use collation buffer or compression for this message.
              if (_client != null && _client.Connected)
              { 
                _client.GetStream().Write(_packet.Data, _packet.Cursor, _packet.Count);
              }
              return true;
            }
            catch (System.IO.IOException)
            {
              _client.Close();
              _client = null;
            }
            finally
            {
              _sendLock.Unlock();
            }
          }
        }
        finally
        {
          _packetLock.Unlock();
        }
      }

      return false;
    }


    /// <summary>
    /// Sends a previously encoded message to the client.
    /// </summary>
    /// <param name="data">The data bytes to send.</param>
    /// <param name="offset">Offset into <paramref name="data"/> to start sending from.</param>
    /// <param name="length">Number of bytes to send (from <paramref name="offset"/>).</param>
    /// <returns>The number of bytes send on success (<paramref name="length"/>) or -1 on failure.</returns>
    /// <remarks>
    /// The given data are send as is, except that they may be collated and optionally
    /// compressed before sending, depending on the <see cref="ServerFlag"/> options set.
    /// </remarks>
    public int Send(byte[] data, int offset, int length)
    {
      return SendInternal(data, offset, length, false);
    }

    /// <summary>
    /// Sends a previously encoded message to the client.
    /// </summary>
    /// <param name="data">The data bytes to send.</param>
    /// <param name="offset">Offset into <paramref name="data"/> to start sending from.</param>
    /// <param name="length">Number of bytes to send (from <paramref name="offset"/>).</param>
    /// <param name="flushCollated">Ensure collated data packet is flushed after send?</param>
    /// <returns>The number of bytes send on success (<paramref name="length"/>) or -1 on failure.</returns>
    /// <remarks>
    /// The given data are send as is, except that they may be collated and optionally
    /// compressed before sending, depending on the <see cref="ServerFlag"/> options set.
    /// </remarks>
    protected int SendInternal(byte[] data, int offset, int length, bool flushCollated)
    {
      bool sendDirect = true;
      _sendLock.Lock();
      try
      {
        if ((ServerFlags & ServerFlag.Collate) != 0)
        {
          if (_collator.CollatedBytes + length >= CollatedPacketEncoder.MaxPacketSize)
          {
            // Additional bytes would be too much. Flush collated
            FlushCollatedPacket();
          }
          int added = _collator.Add(data, offset, length);
          // Flush on request, or if we failed to add to the packet. We'll send the data by itself afterwards.
          if (flushCollated || added == -1)
          {
            // Final flush?
            FlushCollatedPacket();
          }
          if (added != -1)
          {
            sendDirect = false;
            return added;
          }
          // At this point we may send without collation or compression.
        }
        
        if (sendDirect)
        {
          if (_client != null && _client.Connected)
          {
            try
            {
              _client.GetStream().Write(data, offset, length);
            }
            catch (System.IO.IOException)
            {
              _client.Close();
              _client = null;
              return -1;
            }
          }
          return length;
        }

        return -1;
      }
      finally
      {
        _sendLock.Unlock();
      }
    }

    /// <summary>
    /// Flush the collated/compressed data if required.
    /// </summary>
    /// <remarks>
    /// The <see cref="_sendLock"/> should be locked when calling this method.
    /// </remarks>
    private void FlushCollatedPacket()
    {
      if (_collator.CollatedBytes > 0 && _collator.FinaliseEncoding())
      {
        int byteCount = _collator.Count;
        // TODO: catch exception and mark client as disconnected.
        if (_client != null && _client.Connected)
        {
          try
          {
            _client.GetStream().Write(_collator.Buffer, 0, byteCount);
          }
          catch (System.IO.IOException)
          {
            _client.Close();
            _client = null;
          }
        }
        _collator.Reset();
      }
    }

    /// <summary>
    /// Update active transfers, transferring up to the given <paramref name="byteLimit"/>.
    /// </summary>
    /// <param name="byteLimit">The maximum number of bytes allowed to be transferred in this update.
    /// Zero for no limit.</param>
    public int UpdateTransfers(int byteLimit)
    {
      int transferred = 0;
      PacketBuffer packet = null;

      if (byteLimit != 0 && transferred >= byteLimit)
      {
        // Byte limit exceeded. Stop.
        return transferred;
      }

      // Next update shared resources.
      _resourceLock.Lock();
      try
      {
        packet = packet ?? new PacketBuffer(2 * 1024);

        // Do shared resource transfer.
        while ((byteLimit == 0 || transferred < byteLimit) && (_currentResource != null || _sharedResourceQueue.Count != 0))
        {
          if (_currentResource == null)
          {
            // No current resource. Pop the next resource.
            _currentResource = _sharedResources[_sharedResourceQueue.First.Value];
            _currentResourceProgress.Reset();
            _sharedResourceQueue.RemoveFirst();
            // Start transfer.
            _currentResource.Started = true;
            _currentResource.Resource.Create(packet);
            if (packet.FinalisePacket())
            {
              int sendRes = Send(packet.Data, 0, packet.Count);
              if (sendRes >= 0)
              {
                transferred += sendRes;
              }
              else
              {
                // TODO: report error.
              }
            }
          }

          // Process the current resource.
          if (_currentResource != null)
          {
            // Update a part transfer.
            _currentResource.Resource.Transfer(packet, Math.Max(0, byteLimit - transferred), ref _currentResourceProgress);
            if (packet.FinalisePacket())
            {
              int sendRes = Send(packet.Data, 0, packet.Count);
              if (sendRes >= 0)
              { 
                transferred += sendRes;
              }
              else
              {
                // TODO: report error.
              }
            }

            // Check for completion
            if (_currentResourceProgress.Complete)
            {
              // Resource transfer complete.
              _currentResource.Sent = true;
              _currentResource = null;
            }
            else if (_currentResourceProgress.Failed)
            {
              // TODO: report error and mark failure.
              _currentResource.Sent = true;
              _currentResource = null;
            }
          }
        }
      }
      finally
      {
        _resourceLock.Unlock();
      }

      return transferred;
    }

    /// <summary>
    /// Checks the reference count of a resource.
    /// </summary>
    /// <param name="resource">The resource of interest.</param>
    /// <returns>The <paramref name="resource"/> reference count. Zero implies an unknown resource.</returns>
    public uint GetReferenceCount(Resource resource)
    {
      uint refCount = 0;
      _resourceLock.Lock();
      try
      {
        if (_sharedResources.ContainsKey(resource.ID))
        {
          refCount = _sharedResources[resource.ID].RefCount;
        }
      }
      finally
      {
        _resourceLock.Unlock();
      }

      return refCount;
    }

    /// <summary>
    /// Add all resources from <paramref name="shape"/>, incrementing reference
    /// counts and sending resource data as required.
    /// </summary>
    /// <param name="shape">The shape of interest.</param>
    /// <remarks>
    /// The resource for <paramref name="shape"/> are attained via <see cref="Shape.Resources"/>.
    /// </remarks>
    public void AddResources(Shape shape)
    {
      // Transient objects not allowed resources. They won't be released properly.
      if (shape.ID != 0)
      {
        foreach (Resource resource in shape.Resources)
        {
          AddResource(resource);
        }
      }
    }

    /// <summary>
    /// Remove references for all resources of <paramref name="shape"/>,
    /// decrementing resource counts as required.
    /// </summary>
    /// <param name="shape">The shape of interest.</param>
    public void RemoveResources(Shape shape)
    {
      if (shape.ID != 0)
      {
        foreach (Resource resource in shape.Resources)
        {
          RemoveResource(resource);
        }
      }
    }

    /// <summary>
    /// Add a reference for <paramref name="resource"/>.
    /// </summary>
    /// <param name="resource">The resource of interest.</param>
    /// <returns>The reference count after adding.</returns>
    /// <remarks>
    /// The <paramref name="resource"/> is enqueued for sending on the first reference.
    /// </remarks>
    public uint AddResource(Resource resource)
    {
      uint refCount = 0;
      _resourceLock.Lock();
      try
      {
        if (!_sharedResources.ContainsKey(resource.ID))
        {
          ResourceInfo resInfo = new ResourceInfo { Resource = resource, RefCount = 1u, Started = false, Sent = false };
          _sharedResources.Add(resource.ID, resInfo);
          _sharedResourceQueue.AddLast(resource.ID);
          refCount = resInfo.RefCount = 1;
        }
        else
        {
          // Reference count the resource.
          refCount = ++_sharedResources[resource.ID].RefCount;
        }
      }
      finally
      {
        _resourceLock.Unlock();
      }

      return refCount;
    }

    /// <summary>
    /// Remove a reference for <paramref name="resource"/>.
    /// </summary>
    /// <param name="resource">The resource of interest.</param>
    /// <returns>The reference count after removal.</returns>
    /// <remarks>
    /// A destroy message is sent for <paramref name="resource"/> when
    /// the last resource is removed.
    /// </remarks>
    public uint RemoveResource(Resource resource)
    {
      uint refCount = 0;
      _resourceLock.Lock();
      try
      {
        uint resID = resource.ID;
        ResourceInfo info;
        if (_sharedResources.TryGetValue(resID, out info))
        {
          refCount = --info.RefCount;
          if (info.RefCount == 0)
          {
            _sharedResources.Remove(resID);
            _sharedResourceQueue.Remove(resID);
            if (_currentResource != null && _currentResource == info)
            {
              // Remove the shared abort current resource transfer.
              _packetLock.Lock();
              try
              { 
                _currentResource.Resource.Destroy(_packet);
                _packet.FinalisePacket();
                Send(_packet.Data, 0, _packet.Count);
              }
              finally
              {
                _packetLock.Unlock();
                _currentResource = null;
                _currentResourceProgress.Reset();
              }
            }
          }
        }
      }
      finally
      {
        _resourceLock.Unlock();
      }

      return refCount;
    }

    /// <summary>
    /// Resource tracking class.
    /// </summary>
    private class ResourceInfo
    {
      /// <summary>
      /// The resource.
      /// </summary>
      public Resource Resource;
      /// <summary>
      /// The reference count for the resource.
      /// </summary>
      public uint RefCount;
      /// <summary>
      /// True if sending has started.
      /// </summary>
      public bool Started;
      /// <summary>
      /// True if sending has finished.
      /// </summary>
      public bool Sent;
    };

    /// <summary>
    /// Tracks the last server info given to <see cref="SendServerInfo(ServerInfoMessage)"/>.
    /// </summary>
    private ServerInfoMessage _serverInfo = ServerInfoMessage.Default;
    /// <summary>
    /// The client socket.
    /// </summary>
    private TcpClient _client;
    /// <summary>
    /// Mutex lock for the <see cref="_packet"/>.
    /// </summary>
    private SpinLock _packetLock = new SpinLock();
    /// <summary>
    /// Mutex lock for resource management.
    /// </summary>
    /// <remarks>
    /// Guards: <see cref="_sharedResources"/>, <see cref="_sharedResourceQueue"/>,
    /// <see cref="_currentResource"/>, <see cref="_currentResourceProgress"/>.
    /// </remarks>
    private SpinLock _resourceLock = new SpinLock();
    /// <summary>
    /// Resource lock for <see cref="_client"/>.
    /// </summary>
    private SpinLock _sendLock = new SpinLock();
    /// <summary>
    /// Message packet buffer.
    /// </summary>
    private PacketBuffer _packet = new PacketBuffer(2 * 1024);
    /// <summary>
    /// Packet collator if collation is enabled.
    /// </summary>
    private CollatedPacketEncoder _collator = null;
    /// <summary>
    /// Shared resource map.
    /// </summary>
    private Dictionary<uint, ResourceInfo> _sharedResources = new Dictionary<uint, ResourceInfo>();
    /// <summary>
    /// Resource send queue.
    /// </summary>
    private LinkedList<uint> _sharedResourceQueue = new LinkedList<uint>();
    /// <summary>
    /// Current resource being sent.
    /// </summary>
    private ResourceInfo _currentResource = null;
    /// <summary>
    /// Send progress of <see cref="_currentResource"/>.
    /// </summary>
    private TransferProgress _currentResourceProgress = new TransferProgress();
    /// <summary>
    /// Time conversion unique for update messages.
    /// </summary>
    private float _secondsToTimeUnit = 0.0f;
  }
}
