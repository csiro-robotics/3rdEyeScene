using System;
using System.Collections;
using System.IO;
using Tes.Collections;
using Tes.IO;
using Tes.IO.Compression;
using Tes.Net;
using UnityEngine;

namespace Tes.Main
{
  /// <summary>
  /// A data thread which pulls data from an arbitrary thread.
  /// </summary>
  /// <remarks>
  /// Intended primarily for use with files.
  /// </remarks>
  public class StreamThread : DataThread
  {
    class Snapshot
    {
      public long StreamOffset = 0;
      public uint FrameNumber = 0;
      public string TemporaryFilePath;
      public Stream OpenStream;
      public bool Valid = false;
    }

    /// <summary>
    /// Take a snapshot after this many kilo (technically kibi, but I can't bring myself to call it that) bytes.
    /// </summary>
    public long SnapshotBytes
    {
      get { lock (this) { return _snapshotKiloBytes; } }
      set { lock (this) { _snapshotKiloBytes = value; } }
    }
    private long _snapshotKiloBytes = 512;

    /// <summary>
    /// Do not take a snapshot unless at least this many frames have elapsed.
    /// </summary>
    public uint SnapshotMinFrames
    {
      get { lock (this) { return _snapshotMinFrames; } }
      set { lock (this) { _snapshotMinFrames = value; } }
    }
    private uint _snapshotMinFrames = 5;

    public override Queue<PacketBuffer> PacketQueue { get { return _packetQueue; } }
    public override uint CurrentFrame 
    {
      get { lock(this) { return _currentFrame; } }
      set { TargetFrame = value; }
    }
    public override uint TotalFrames
    {
      get { lock(this) { return _totalFrames; } }
      set { /* ignored */ }
    }
    public override uint TargetFrame
    {
      get { lock(this) { return _targetFrame; } }
      set
      {
        lock (this)
        {
          // Clamp the value to be in range.
          if (value <= _totalFrames)
          {
            _targetFrame = value;
          }
          else
          {
            _targetFrame = _totalFrames;
          }
        }
      }
    }
    public override bool IsLiveStream { get { return false; } }
    public override bool Started { get { return _thread != null && _thread.Running; } }
    public override bool Paused
    {
      get { return _paused; }
      set
      {
        _paused = value;
        if (_thread != null)
        {
          _thread.Notify();
        }
      }
    }

    public override bool CatchingUp
    {
      get
      {
        return _catcingUp;
      }
    }

    public bool Loop
    {
      get { return _loop; }
      set { _loop = value; }
    }

    public float PlaybackSpeed
    {
      get
      {
        lock(this)
        {
          return _playbackSpeed;
        }
      }
      
      set
      {
        lock(this)
        {
          _playbackSpeed = value;
        }
      }
    }

    /// <summary>
    /// Allow use of snapshots?
    /// </summary>
    public bool AllowSnapshots { get; set; }

    public StreamThread()
    {
    }

    /// <summary>
    /// Clean up temporary files.
    /// </summary>
    ~StreamThread()
    {
      CleanupSnapshots();
    }

    public bool SetStream(Stream dataStream)
    {
      if (Started)
      {
        return false;
      }

      // We have up until the first end frame to switch to compressed data.
      _allowCompressStream = true;
      _currentFrame = 0;
      _stream = dataStream;
      CleanupSnapshots();
      return true;
    }

    public bool Start(Stream dataStream)
    {
      if (Started)
      {
        // Already started.
        return false;
      }
      _stream = dataStream;
      return Start();
    }

    public override bool Start()
    {
      if (Started || _stream == null)
      {
        return false;
      }

      // Start paused up to the first frame.
      _paused = true;
      _targetFrame = 1;
      _quitFlag = false;
      Eos = false;
      // _thread = new System.Threading.Thread(this.Run);
      _thread = new Workthread(Run(), this.QuitThread, this.OnStop);
      _thread.Start();
      return true;
    }

    public override bool Join()
    {
      if (_thread != null)
      {
        Paused = false;
        _thread.Stop();
        _thread = null;
        if (_stream != null)
        {
          _stream.Close();
          _stream = null;
        }
      }
      return true;
    }

    public override void Quit()
    {
      _quitFlag = true;
      if (_thread != null)
      {
        _thread.Notify();
      }
    }

    public bool Eos { get; private set; }

    private IEnumerator Run()
    {
      PacketHeader header = new PacketHeader();
      byte[] headerBuffer = new byte[PacketHeader.Size];
      System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
      long elapsedUs = 0;
      int bytesRead = 0;
      bool allowYield = false;
      // Every N bytes we will request a Snapshot to save the current scene state and improve step playback.
      long processedBytes = 0;
      long bytesSinceLastSnapshot = 0;
      uint lastSnapshotFrame = 0;

      stopwatch.Start();
      while (!_quitFlag && _stream != null) // && _stream.Position + PacketHeader.Size <= _stream.Length)
      {
        if (_paused && _targetFrame == 0)
        {
          // Wait for pause change a notification object.
          if (!_thread.Wait())
          {
            yield return null;
          }
          continue;
        }

        if (_targetFrame == 0)
        {
          // Not stepping. Check time elapsed.
          _catcingUp = false;
          stopwatch.Stop();
          stopwatch.Start();
          elapsedUs = stopwatch.ElapsedTicks / (System.Diagnostics.Stopwatch.Frequency / (1000L * 1000L));
          // Scale by playback speed.
          elapsedUs = (long)(elapsedUs * (double)_playbackSpeed);
          if (elapsedUs < _frameDelayUs)
          {
            // Too soon. Yield and wait some more.
            yield return null;
            continue;
          }
        }
        else
        {
          elapsedUs = 0;
          lock(this)
          { 
            if (_targetFrame < _currentFrame)
            {
              // Stepping back.
              // Naive solution: reset the file stream and consume until the requested frame.
              processedBytes = 0;
              bytesSinceLastSnapshot = 0;
              lastSnapshotFrame = 0;
              ResetStream();
              Snapshot snapshot = TrySnapshot(_targetFrame);
              if (snapshot != null)
              {
                lastSnapshotFrame = _currentFrame = snapshot.FrameNumber;
                processedBytes = snapshot.StreamOffset;
                _allowCompressStream = false;
              }
              _catcingUp = _currentFrame + 1 < _targetFrame;
              stopwatch.Reset();
              stopwatch.Start();
            }
          }
        }

        // Time has elapsed. Reset and restart the timer.
        stopwatch.Reset();
        stopwatch.Start();
        // Track the overflow to try keep some precision.
        _frameOverrunUs = elapsedUs - _frameDelayUs;
        _frameDelayUs = 0;
        elapsedUs = 0;

        try
        {
          allowYield = false;
          while (!allowYield)
          {
            if (_allowCompressStream && Util.GZipUtil.IsGZipStream(_stream))
            {
              _allowCompressStream = false;
              _gzipStreamStart = _stream.Position;
              _stream = new GZipStream(_stream, CompressionMode.Decompress);
            }

            bytesRead = _stream.Read(headerBuffer, 0, headerBuffer.Length);
            processedBytes += bytesRead;
            bytesSinceLastSnapshot += bytesRead;
            _paused = _paused || bytesRead == 0 && !_loop;

            allowYield = true;
            if (bytesRead == headerBuffer.Length)
            {
              allowYield = false;
              if (header.Read(new NetworkReader(new MemoryStream(headerBuffer, false))))
              {
                // Read the header. Determine the expected size and read that much more data.
                int crcSize = ((header.Flags & (byte)PacketFlag.NoCrc) == 0) ? Crc16.CrcSize : 0;
                PacketBuffer packet = new PacketBuffer(header.PacketSize + crcSize);
                packet.Emplace(headerBuffer, bytesRead);
                packet.Emplace(_stream, header.PacketSize + crcSize - bytesRead);
                if (packet.Status == PacketBufferStatus.Complete)
                {
                  // Check for end of frame messages to yield on.
                  // TODO: check frame elapsed time as well.
                  if (header.RoutingID == (ushort)RoutingID.Control)
                  {
                    if (HandleControlMessage(packet, (ControlMessageID)header.MessageID))
                    {
                      // Ended a frame. Check for snapshot.
                      if (bytesSinceLastSnapshot >= _snapshotKiloBytes * 1024 &&
                          lastSnapshotFrame < _currentFrame &&
                          _currentFrame - lastSnapshotFrame >= _snapshotMinFrames)
                      {
                        bytesSinceLastSnapshot = 0;
                        RequestSnapshot(_currentFrame, processedBytes);
                        lastSnapshotFrame = _currentFrame;
                      }
                      // Make sure we yield so as to support the later check to avoid flooding the packet queue.
                      allowYield = true;
                    }
                  }
                  else if (header.RoutingID == (ushort)RoutingID.ServerInfo)
                  {
                    HandleServerInfo(packet);
                  }
                  _packetQueue.Enqueue(packet);
                }
                // else notify error.
                allowYield = allowYield || _targetFrame == 0 && _frameDelayUs > 0 && !_catcingUp;
              }
            }
            else if (bytesRead == 0 && allowYield && _loop)
            {
              // Restart
              TargetFrame = 1;
            }
          }
        }
        catch (Exception e)
        {
          _quitFlag = true;
          _allowCompressStream = false;
          bytesRead = 0;
          UnityEngine.Debug.LogException(e);
          allowYield = true;
        }

        if (!_paused)
        {
          // Wait for the enqueued packets to be processed.
          // This stops the queue from being swamped when the main thread can't keep up
          // with processing the queue.
          while (!_quitFlag && _packetQueue.Count > 0)
          {
            yield return null;
          }
        }
      }

      Eos = true;
    }

    private void QuitThread()
    {
      _quitFlag = true;
    }

    private void OnStop()
    {
      if (_stream != null)
      {
        _stream.Close();
      }
    }

    /// <summary>
    /// Handle a control message.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="messageId"></param>
    /// <returns>True if this ended a frame.</returns>
    private bool HandleControlMessage(PacketBuffer packet, ControlMessageID messageId)
    {
      bool endedFrame = false;
      ControlMessage msg = new ControlMessage();
      if (!msg.Peek(packet))
      {
        return endedFrame;
      }
      
      if (messageId == ControlMessageID.EndFrame)
      {
        OnEndFrame(msg.Value32);
        endedFrame = true;
      }
      else if (messageId == ControlMessageID.FrameCount)
      {
        // Set the frame count.
        lock (this)
        {
          // Only set if current value is less than the read value. This caters for
          // badly saved files, where the frame count was improperly finalised.
          if (_totalFrames < msg.Value32)
          {
            _totalFrames = msg.Value32;
          }
        }
      }
      return endedFrame;
    }

    private void HandleServerInfo(PacketBuffer packet)
    {
      NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));
      ServerInfoMessage serverInfo = new ServerInfoMessage();
      if (serverInfo.Read(packetReader))
      {
        _defaultFrameTime = serverInfo.DefaultFrameTime;
        _timeUnit = serverInfo.TimeUnit;
      }
    }

    /// <summary>
    /// Handles reading a <see cref="ControlMessageID.EndFrame"/> message.
    /// </summary>
    /// <param name="frameDelta"></param>
    /// <remarks>
    /// The <see cref="CurrentFrame"/> is updated as well as the time to delay until
    /// the next frame is processed (<see cref="_frameDelayUs"/>).
    /// </remarks>
    private void OnEndFrame(uint frameDelta)
    {
      // Forward playback. Update current frame.
      lock(this)
      {
        _allowCompressStream = false;
        ++_currentFrame;

        if (_currentFrame < _targetFrame)
        {
          // We are stepping. Do not delay/sleep.
          _frameDelayUs = _frameOverrunUs = 0L;
        }
        else
        {
          // FIXME: work out and document the correct semantics for "use default frame time"
          // Zero or ~0?
          if (frameDelta == 0u || frameDelta == ~0u)
          {
            frameDelta = _defaultFrameTime;
          }

          // Convert the time value to microseconds.
          long frameDeltaUs = (long)(frameDelta * _timeUnit);
          // Increment the current frame delay by the specified time step and the cached overrun.
          if (_frameOverrunUs <= frameDeltaUs)
          {
            _frameDelayUs = frameDeltaUs - _frameOverrunUs;
            // Overrun has been applied.
            _frameOverrunUs = 0;
          }
          else
          {
            // Haven't handled the overrun.
            _frameDelayUs = 0;
            _frameOverrunUs -= frameDeltaUs;
          }

          if (_targetFrame != 0)
          {
            // Done stepping.
            _targetFrame = 0;
          }
        }

        if (_currentFrame > _totalFrames)
        {
          _totalFrames = _currentFrame;
        }
      }
      _catcingUp = _currentFrame + 1 < _targetFrame;
    }

    #region Snapshots

    /// <summary>
    /// Try find a snapshot near the given frame number.
    /// </summary>
    /// <param name="targetFrame"></param>
    /// <returns>The frame number reached by the snapshot.</returns>
    private Snapshot TrySnapshot(uint targetFrame)
    {
      if (!AllowSnapshots)
      {
        return null;
      }
      lock (_snapshots)
      {
        Snapshot bestShot = null;
        foreach (Snapshot snapshot in _snapshots)
        {
          if (snapshot.FrameNumber <= targetFrame)
          {
            if (snapshot.Valid)
            {
              bestShot = snapshot;
            }
          }
          else
          {
            break;
          }
        }

        if (bestShot != null)
        {
          if (RestoreSnapshot(bestShot))
          {
            return bestShot;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Restore the given shapshot.
    /// </summary>
    /// <param name="snapshot">To restore</param>
    /// <returns>True on success.</returns>
    private bool RestoreSnapshot(Snapshot snapshot)
    {
      if (!snapshot.Valid || string.IsNullOrEmpty(snapshot.TemporaryFilePath) || !File.Exists(snapshot.TemporaryFilePath))
      {
        return false;
      }

      try
      {
        Stream snapStream = new FileStream(snapshot.TemporaryFilePath, FileMode.Open, FileAccess.Read);
        // Ensure stream has been reset.
        if (_stream as GZipStream != null)
        {
          ResetStream();
        }
        long streamPos = 0;

        if (_gzipStreamStart == 0)
        {
          // No compression. Just seek in _stream.
          _stream.Seek(snapshot.StreamOffset, SeekOrigin.Begin);
          streamPos = _stream.Position;
        }
        else
        {
          // Restart the stream, at the GZip stream and seek to the target position.
          _stream.Seek(_gzipStreamStart, SeekOrigin.Begin);
          _stream = new GZipStream(_stream, CompressionMode.Decompress);

          // Read bytes up to the snapshot.
          byte[] buffer = new byte[1024];
          long read = 0;
          streamPos = _gzipStreamStart;
          do
          {
            read = _stream.Read(buffer, 0, (int)Math.Min(snapshot.StreamOffset - streamPos, buffer.LongLength));
            streamPos += read;
          } while (read > 0 && streamPos < snapshot.StreamOffset);
        }

        // Decode the snapshot data.
        if (streamPos == snapshot.StreamOffset)
        {
          if (DecodeSnapshotStream(snapStream))
          {
            _currentFrame = snapshot.FrameNumber;
            return true;
          }
          Debug.LogError(string.Format("Failed to decode snapshot for frame {0}", snapshot.FrameNumber));
        }
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }

      // Failed. Reset the stream.
      ResetStream();
      return false;
    }

    /// <summary>
    /// Decode the contents of the stream in <paramref name="snapshot"/> and add packets to
    /// the outgoing queue.
    /// </summary>
    /// <param name="snapshot"></param>
    /// <returns></returns>
    private bool DecodeSnapshotStream(Stream snapStream)
    {
      if (snapStream == null)
      {
        return false;
      }

      bool ok = true;
      bool allowCompression = true;
      int bytesRead = 0;
      PacketHeader header = new PacketHeader();
      byte[] headerBuffer = new byte[PacketHeader.Size];

      try
      {
        do
        {
          if (allowCompression && Util.GZipUtil.IsGZipStream(snapStream))
          {
            allowCompression = false;
            snapStream = new GZipStream(snapStream, CompressionMode.Decompress);
          }
          bytesRead = snapStream.Read(headerBuffer, 0, headerBuffer.Length);

          ok = false;
          if (bytesRead == headerBuffer.Length)
          {
            if (header.Read(new NetworkReader(new MemoryStream(headerBuffer, false))))
            {
              // Read the header. Determine the expected size and read that much more data.
              int crcSize = ((header.Flags & (byte)PacketFlag.NoCrc) == 0) ? Crc16.CrcSize : 0;
              PacketBuffer packet = new PacketBuffer(header.PacketSize + crcSize);
              packet.Emplace(headerBuffer, bytesRead);
              packet.Emplace(snapStream, header.PacketSize + crcSize - bytesRead);
              if (packet.Status == PacketBufferStatus.Complete)
              {
                // Check for end of frame messages to yield on.
                _packetQueue.Enqueue(packet);
                ok = true;
              }
            }
          }
        } while (ok && bytesRead != 0);
      }
      catch (Exception e)
      {
        ok = false;
        Debug.LogException(e);
      }

      return ok;
    }

    /// <summary>
    /// Release all snapshots and cleanup the temporary files.
    /// </summary>
    private void CleanupSnapshots()
    {
      lock (_snapshots)
      {
        // Look for the snapshot.
        Snapshot snapshot = null;
        for (int i = 0; i < _snapshots.Count; ++i)
        {
          snapshot = _snapshots[i];
          if (snapshot.OpenStream != null)
          {
            snapshot.OpenStream.Close();
            snapshot.OpenStream = null;
          }
          if (!string.IsNullOrEmpty(snapshot.TemporaryFilePath) && File.Exists(snapshot.TemporaryFilePath))
          {
            File.Delete(snapshot.TemporaryFilePath);
            snapshot.TemporaryFilePath = null;
          }
        }
        _snapshots.Clear();
      }
    }

    /// <summary>
    /// Request a stream for the snapshot of the given frame number.
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <returns></returns>
    /// <remarks>
    /// The caller must save the snapshot to the stream then call <see cref="ReleaseSnapshotStream(Stream, bool)"/>
    /// regardless of success.
    /// </remarks>
    public Stream RequestSnapshotStream(uint frameNumber)
    {
      lock (_snapshots)
      {
        // Look for the snapshot.
        Snapshot snapshot = null;
        for (int i = 0; i < _snapshots.Count; ++i)
        {
          if (_snapshots[i].FrameNumber == frameNumber)
          {
            snapshot = _snapshots[i];
            snapshot.TemporaryFilePath = Path.GetFullPath(Path.Combine("temp", Path.GetRandomFileName()));
            snapshot.OpenStream = new FileStream(snapshot.TemporaryFilePath, FileMode.Create);
            return snapshot.OpenStream;
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Release a snapshot stream.
    /// </summary>
    /// <param name="snapshotStream">The stream to release</param>
    /// <param name="valid">True if the snapshot was successful.</param>
    public void ReleaseSnapshotStream(Stream snapshotStream, bool valid)
    {
      if (snapshotStream == null)
      {
        return;
      }

      lock (_snapshots)
      {
        // Look for the snapshot.
        Snapshot snapshot = null;
        for (int i = 0; i < _snapshots.Count; ++i)
        {
          if (_snapshots[i].OpenStream == snapshotStream)
          {
            snapshot = _snapshots[i];
            snapshot.OpenStream.Close();
            snapshot.OpenStream = null;
            snapshot.Valid = valid;
            if (!string.IsNullOrEmpty(snapshot.TemporaryFilePath) && !valid && File.Exists(snapshot.TemporaryFilePath))
            {
              File.Delete(snapshot.TemporaryFilePath);
              snapshot.TemporaryFilePath = null;
            }
            return;
          }
        }
      }
    }

    /// <summary>
    /// Is there a snapshot available for the given frame number?
    /// </summary>
    /// <param name="frameNumber">The desired frame number.</param>
    /// <returns>True when there is a valid snapshot available for <paramref name="frameNumber"/>.</returns>
    public bool HaveSnapshot(uint frameNumber)
    {
      lock (_snapshots)
      {
        // Look for the snapshot.
        for (int i = 0; i < _snapshots.Count; ++i)
        {
          if (_snapshots[i].FrameNumber == frameNumber)
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Request the additional of a snapshot at the given frame and post the associated control message.
    /// </summary>
    /// <param name="frameNumber">The snapshot frame number.</param>
    /// <param name="streamOffset">The stream position/bytes read (total) on this frame.</param>
    private void RequestSnapshot(uint frameNumber, long streamOffset)
    {
      if (HaveSnapshot(frameNumber))
      {
        return;
      }

      // Add a placeholder.
      Snapshot snapshot = new Snapshot();
      snapshot.FrameNumber = frameNumber;
      snapshot.StreamOffset = streamOffset;
      lock (_snapshots)
      {
        _snapshots.Add(snapshot);
      }

      PacketBuffer packet = new PacketBuffer(PacketHeader.Size + 32);
      ControlMessage message = new ControlMessage();

      message.ControlFlags = 0;
      message.Value32 = frameNumber;
      message.Value64 = 0;

      packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.Snapshop);
      message.Write(packet);

      if (packet.FinalisePacket())
      {
        _packetQueue.Enqueue(packet);
      }
    }

    #endregion

    /// <summary>
    /// Reset playback.
    /// </summary>
    /// <remarks>
    /// This pushes a reset packet into the outgoing packet queue, then resets the stream position.
    /// </remarks>
    void ResetStream()
    {
      _packetQueue.Clear();
      _packetQueue.Enqueue(BuildResetPacket());

      GZipStream zipStream = _stream as GZipStream;
      if (zipStream != null)
      {
        // Switch to uncompressed mode.
        _stream = zipStream.BaseStream;
      }

      _stream.Flush();
      _stream.Seek(0, SeekOrigin.Begin);
      _currentFrame = 0;
      _frameDelayUs = _frameOverrunUs = 0;
      _catcingUp = false;
      _allowCompressStream = true;
    }


    //private Queue<PacketBuffer> _packetQueue = new Queue<PacketBuffer>();
    private Tes.Collections.Queue<PacketBuffer> _packetQueue = new Tes.Collections.Queue<PacketBuffer>();
    private Stream _stream = null;
    /// <summary>
    /// Start of the GZip stream in the core stream if known. Byte position.
    /// </summary>
    private long _gzipStreamStart = 0;
    private Workthread _thread = null;
    /// <summary>
    /// Tracks the current frame number. Updated every  Initialised via the <see cref="ControlMessageID.EndFrame"/> message.
    /// </summary>
    private uint _currentFrame = 0;
    /// <summary>
    /// Tracks the total number of frames. Initialised via the <see cref="ControlMessageID.FrameCount"/> message.
    /// </summary>
    private uint _totalFrames = 0;
    /// <summary>
    /// Controls the target for frame stepping. This is the frame to advance or rewind to.
    /// </summary>
    /// <remarks>
    /// Value is zero when there is no specific target set.
    /// </remarks>
    private uint _targetFrame = 0;
    /// <summary>
    /// The time remaining to pause for until the next frame is allowed (microseconds).
    /// </summary>
    private long _frameDelayUs = 0;
    /// <summary>
    /// Accumulated time after _frameDelayUs elapses.
    /// </summary>
    private long _frameOverrunUs = 0;
    /// <summary>
    /// Tracks the default frame time if none is specified in a frame message (from <see cref="ServerInfoMessage.DefaultFrameTime"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="ServerInfoMessage.DefaultFrameTime"/> for semantic details.
    /// </remarks>
    private uint _defaultFrameTime = ServerInfoMessage.Default.DefaultFrameTime;
    /// <summary>
    /// Tracks the time unit (from <see cref="ServerInfoMessage.TimeUnit"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="ServerInfoMessage.TimeUnit"/> for semantic details.
    /// </remarks>
    private ulong _timeUnit = ServerInfoMessage.Default.TimeUnit;
    private float _playbackSpeed;
    private bool _quitFlag = false;
    /// <summary>
    /// Change to compressed stream still allowed? (Until first frame or made switch).
    /// </summary>
    private bool _allowCompressStream = true;
    private volatile bool _paused = false;
    /// <summary>
    /// Set when the current frame is behind the target frame.
    /// </summary>
    /// <remarks>
    /// Should be cleared before processing the last frame so that the last frame is
    /// processed with this flag false.
    /// </remarks>
    private volatile bool _catcingUp = false;
    private volatile bool _loop = false;
    /// <summary>
    /// A list of frame snapshots to improve step back behaviour.
    /// </summary>
    private System.Collections.Generic.List<Snapshot> _snapshots = new System.Collections.Generic.List<Snapshot>();
  }
}
