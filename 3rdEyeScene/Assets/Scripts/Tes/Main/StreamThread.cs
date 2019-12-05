using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using UnityEngine;
using Tes.Collections;
using Tes.IO;
using Tes.Logging;
using Tes.Net;
using Tes.Runtime;

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
    /// <summary>
    /// Details of a keyframe.
    /// </summary>
    /// <remarks>
    /// Keyframes must be taken at the end of a frame. However, we can't guarantee that the
    /// visualiser included transient objects in the serialisation as the message handlers
    /// may have been ignoring transient objects. This is marked by the state of
    /// <see cref="IncludesTransient"/>. When this is true, we can restore the exact
    /// <see cref="FrameNumber"/>. When false, we can restore to <see cref="FrameNumber"/>
    /// as a starting point for subsequent frames, but <see cref="FrameNumber"/> may not
    /// be an accurate representation of that frame.
    /// </remarks>
    class Keyframe
    {
      /// <summary>
      /// Offset into the serialisation stream (bytes) at which the keyframe is made.
      /// </summary>
      public long StreamOffset = 0;
      /// <summary>
      /// The (end of) frame number for the keyframe.
      /// </summary>
      public uint FrameNumber = 0;
      /// <summary>
      /// The (end of) frame number at the StreamOffset.
      /// </summary>
      /// <remarks>
      /// This may be less than FrameNumber, in which case we have to ignore intervening frames.
      /// </remarks>
      public uint OffsetFrameNumber = 0;
      /// <summary>
      /// Does this keyframe include transient objects? See class remarks.
      /// </summary>
      public bool IncludesTransient = false;
      /// <summary>
      /// File name capturing the keyframe.
      /// </summary>
      public string TemporaryFilePath;
      /// <summary>
      /// File stream to the keyframe stream. Matches <see cref="TemporaryFilePath"/>.
      /// Closed once saved.
      /// </summary>
      public Stream OpenStream;
      /// <summary>
      /// Is the keyframe valid for restoration.
      /// </summary>
      public bool Valid = false;
    }

    /// <summary>
    /// Allow use of keyframes?
    /// </summary>
    public bool AllowKeyframes { get; set; }

    /// <summary>
    /// Take a keyframe after this many kilo (technically kibi, but I can't bring myself to call it that) bytes.
    /// </summary>
    public long KeyframeMiB
    {
      get { lock (this) { return _keyframeMiB; } }
      set { lock (this) { _keyframeMiB = value; } }
    }
    private long _keyframeMiB = 20;

    /// <summary>
    /// Do not take a keyframe unless at least this many frames have elapsed.
    /// </summary>
    public uint KeyframeMinFrames
    {
      get { lock (this) { return _keyframeMinFrames; } }
      set { lock (this) { _keyframeMinFrames = value; } }
    }
    private uint _keyframeMinFrames = 5;

    /// <summary>
    /// Do not take a keyframe unless at least this many frames have elapsed.
    /// </summary>
    public uint KeyframeFrames
    {
      get { lock (this) { return _keyframeFrames; } }
      set { lock (this) { _keyframeFrames = value; } }
    }
    private uint _keyframeFrames = 100;

    public uint KeyframeSkipForwardFrames
    {
      get { lock (this) { return _keyframeSkipForwardFrames; } }
      set { lock (this) { _keyframeSkipForwardFrames = value; } }
    }
    private uint _keyframeSkipForwardFrames = 50;

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
        return _catchingUp;
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

    public StreamThread()
    {
    }

    /// <summary>
    /// Clean up temporary files.
    /// </summary>
    ~StreamThread()
    {
      CleanupKeyframes();
    }

    public bool SetStream(Stream dataStream)
    {
      if (Started)
      {
        return false;
      }

      _currentFrame = 0;
      _packetStream = new PacketStreamReader(dataStream);
      CleanupKeyframes();
      return true;
    }

    public bool Start(Stream dataStream)
    {
      if (Started)
      {
        // Already started.
        return false;
      }
      _packetStream = new PacketStreamReader(dataStream);
      return Start();
    }

    public override bool Start()
    {
      if (Started || _packetStream == null)
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
        if (_packetStream != null)
        {
          if (_packetStream.CanRead)
          {
            _packetStream.Close();
          }
          _packetStream = null;
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

    public override bool CanSuspend { get { return Workthread.CanSuspend; } }

    public override bool IsSuspended { get { return _thread != null && _thread.IsSuspended; } }

    public override bool Suspend() { return _thread != null && _thread.Suspend(); }

    public override bool Resume() { return _thread != null && _thread.Resume(); }

    public bool Eos { get; private set; }

    private IEnumerator Run()
    {
      System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
      long elapsedUs = 0;
      // Last position in the stream we can seek to.
      long lastSeekablePosition = 0;
      long lastKeyframePosition = 0;
      long bytesRead = 0;
      bool allowYield = false;
      bool failedKeyframe = false;
      bool atSeekablePosition = false;
      bool wasPaused = _paused;
      // HACK: when restoring keyframes to precise frames we don't do the main update. Needs to be cleaned up.
      bool skipUpdate = false;
      uint lastKeyframeFrame = 0;
      uint lastSeekableFrame = 0;

      stopwatch.Start();
      while (!_quitFlag && _packetStream != null) // && _packetStream.Position + PacketHeader.Size <= _packetStream.Length)
      {
        if (_paused && _targetFrame == 0)
        {
          wasPaused = true;
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
          _catchingUp = false;

          // Measure elapsed time. Start and stop the timer to set the elapsed time and continue tracking time.
          stopwatch.Stop();
          stopwatch.Start();
          // Should this have _frameOverrunUs added?
          elapsedUs = stopwatch.ElapsedTicks / (System.Diagnostics.Stopwatch.Frequency / (1000L * 1000L));
          elapsedUs = (long)(elapsedUs * (double)_playbackSpeed);

          if (wasPaused && !_paused)
          {
            // Just unpaused. Force an immediate frame update.
            elapsedUs = _frameDelayUs;
          }
          wasPaused = false;

          // Scale by playback speed.
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
              lastKeyframeFrame = 0;
              lastSeekablePosition = 0;
              lastSeekableFrame = 0;
              bool restoredKeyframe = false;
              if (AllowKeyframes && !failedKeyframe)
              {
                Keyframe keyframe;
                if (TryRestoreKeyframe(out keyframe, _targetFrame))
                {
                  // No failures. Does not meek we have a valid keyframe.
                  if (keyframe != null)
                  {
                    lastKeyframeFrame = _currentFrame = keyframe.FrameNumber;
                    restoredKeyframe = true;
                    skipUpdate = _currentFrame == keyframe.FrameNumber;
                  }
                }
                else
                {
                  // Failed a keyframe. Disallow further keyframes.
                  // TODO: consider just invalidating the failed keyframe.
                  failedKeyframe = true;
                }
              }

              // Not available, not allowed or failed keyframe.
              if (!restoredKeyframe)
              {
                _packetStream.Reset();
                ResetQueue(0);
              }
              _catchingUp = _currentFrame + 1 < _targetFrame;
              stopwatch.Reset();
              stopwatch.Start();
            }
            else if (_targetFrame > _currentFrame + KeyframeSkipForwardFrames)
            {
              // Skipping forward a fair number of frames. Try for a keyframe.
              if (AllowKeyframes && !failedKeyframe)
              {
                // Ok to try for a keyframe.
                Keyframe keyframe;
                if (TryRestoreKeyframe(out keyframe, _targetFrame, _currentFrame))
                {
                  // No failure. Check if we have a keyframe.
                  if (keyframe != null)
                  {
                    lastKeyframeFrame = _currentFrame = keyframe.FrameNumber;
                    _catchingUp = _currentFrame + 1 < _targetFrame;
                    skipUpdate = _currentFrame == keyframe.FrameNumber;
                  }
                }
                else
                {
                  // Failed. Stream has been reset.
                  failedKeyframe = true;
                }
              }
            }
          } // lock(this)
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
          allowYield = skipUpdate;

          if (skipUpdate)
          {
            // HACK: more unclean code. When skipping a frame update we need to ensure the frame number is in sync.
            // For that reason we force a frame flush with the current frame number.
            _packetQueue.Enqueue(CreateFrameFlushPacket(_currentFrame));
            skipUpdate = false;
          }

          while (!allowYield && !_packetStream.EndOfStream)
          {
            PacketBuffer packet = _packetStream.NextPacket(ref bytesRead);
            _paused = _paused || bytesRead == 0 && !_loop;
            allowYield = true;

            // Can we update the stream seek position?
            atSeekablePosition = false;
            if (!_packetStream.DecodingCollated && _packetStream.CanSeek && _currentFrame > 0)
            {
              lastSeekablePosition = _packetStream.Position;
              // May be one off when the end of frame is now.
              lastSeekableFrame = _currentFrame;
              atSeekablePosition = true;
            }

            if (packet != null)
            {
              // Read the header. Determine the expected size and read that much more data.
              if (packet.Status == PacketBufferStatus.Complete)
              {
                allowYield = false;
                // Check for end of frame messages to yield on.
                // TODO: check frame elapsed time as well.
                if (packet.Header.RoutingID == (ushort)RoutingID.Control)
                {
                  // Store frame number for setting of lastSeekableFrame. See below.
                  uint preControlMsgFrame = _currentFrame;
                  // HandleControlMessage() returns true on an end of frame event.
                  if (HandleControlMessage(packet, (ControlMessageID)packet.Header.MessageID))
                  {
                    // Clear keyframe failure if at target frame.
                    failedKeyframe = failedKeyframe && !(_targetFrame == 0 || _currentFrame == _targetFrame);

                    if (atSeekablePosition)
                    {
                      // We don't use _currentFrame here as we will have just incremented it, so we actually
                      // want the frame before. We use a cached copy just in case in future we can be here
                      // for other reasons.
                      lastSeekableFrame = preControlMsgFrame;
                      Log.Diag("Last seekable: {0}", lastSeekableFrame);
                    }

                    bool keyframeRequested = false;
                    // Ended a frame. Check for keyframe. We'll queue the request after the end of
                    // frame message below.
                    if ((lastSeekablePosition - lastKeyframePosition >= _keyframeMiB * 1024 * 1024 ||
                        _currentFrame - lastKeyframeFrame >= _keyframeFrames) &&
                        lastKeyframeFrame < _currentFrame &&
                        _currentFrame - lastKeyframeFrame >= _keyframeMinFrames)
                    {
                      // A keyframe is due. However, the stream may not be seekable to the current location.
                      // We may request a keyframe now.
                      Log.Diag("Request keyframe for frame {0} from frame {1} after {2} MiB",
                        _currentFrame, lastSeekableFrame, (lastSeekablePosition - lastKeyframePosition) / (1024 * 1024));
                      lastKeyframeFrame = _currentFrame;
                      lastKeyframePosition = lastSeekablePosition;
                      RequestKeyframe(lastKeyframeFrame, lastSeekableFrame, lastSeekablePosition);
                      keyframeRequested = true;
                    }
                    // Make sure we yield so as to support the later check to avoid flooding the packet queue.
                    allowYield = !_catchingUp || keyframeRequested;
                  }
                }
                else if (packet.Header.RoutingID == (ushort)RoutingID.ServerInfo)
                {
                  HandleServerInfo(packet);
                }
                _packetQueue.Enqueue(packet);
                allowYield = allowYield || _targetFrame == 0 && _frameDelayUs > 0 && !_catchingUp;
              }
              else
              {
                Log.Error("Incomplete packet, ID: {0}", LookupRoutingIDName(packet.Header.RoutingID));
              }
            }
          }

          if (_packetStream.EndOfStream)
          {
            allowYield = true;
            if (_loop)
            {
              // Restart
              TargetFrame = 1;
            }
            else
            {
              // Playback complete. Autopause.
              Paused = true;
            }
          }
        }
        catch (Exception e)
        {
          _quitFlag = true;
          bytesRead = 0;
          Log.Exception(e);
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

      _catchingUp = false;
      Eos = true;
    }

    private void QuitThread()
    {
      _quitFlag = true;
    }

    private void OnStop()
    {
      if (_packetStream != null)
      {
        _packetStream.Close();
      }
    }

    /// <summary>
    /// Handle a control message.
    /// </summary>
    /// <param name="packet">The packet containing the control message.</param>
    /// <param name="messageId">The ID of the control message.</param>
    /// <returns>True if this ended a frame and requires a flush. An end frame message may be processed without a
    /// flush.</returns>
    /// <remarks>
    /// This method may modify the packet data before it is queued for processing. The primary
    /// use case is to write the current frame number into and end of frame message.
    /// </remarks>
    private bool HandleControlMessage(PacketBuffer packet, ControlMessageID messageId)
    {
      bool frameFlush = false;
      ControlMessage msg = new ControlMessage();
      if (!msg.Peek(packet))
      {
        return frameFlush;
      }

      if (messageId == ControlMessageID.EndFrame)
      {
        // Replace the end of frame packet with a new one including the current frame number.
        bool flush = OnEndFrame(msg.Value32);
        // Overwrite msg.Value64 with the current frame number.
        int memberOffset = PacketHeader.Size + Marshal.OffsetOf(typeof(ControlMessage), "Value64").ToInt32();
        byte[] packetData = packet.Data;
        byte[] frameNumberBytes = BitConverter.GetBytes(Endian.ToNetwork((ulong)_currentFrame));
        Array.Copy(frameNumberBytes, 0, packetData, memberOffset, frameNumberBytes.Length);
        // Override the flags to include the flush value.
        if (flush)
        {
          memberOffset = PacketHeader.Size + Marshal.OffsetOf(typeof(ControlMessage), "ControlFlags").ToInt32();
          uint controlFlags = BitConverter.ToUInt32(packetData, memberOffset);
          controlFlags = Endian.FromNetwork(controlFlags);
          // Add the flush flag
          controlFlags |= (uint)EndFrameFlag.Flush;
          // Convert back to bytes and copy into the packet buffer.
          frameNumberBytes = BitConverter.GetBytes(Endian.ToNetwork(controlFlags));
          Array.Copy(frameNumberBytes, 0, packetData, memberOffset, frameNumberBytes.Length);
        }
        frameFlush = !_catchingUp && flush;
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
      return frameFlush;
    }

    /// <summary>
    /// Decode the <see cref="Tes.Net.ServerInfoMessage" /> packet.
    /// </summary>
    /// <param name="packet">The packet containing a <c>ServerInfoMessage</c></param>
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
    /// The <see cref="CurrentFrame"/> is updated as well as the time to delay until the next frame is processed
    /// (<see cref="_frameDelayUs"/>). In some cases, the play time elapsed may still exceed the playback time for this
    /// completed frame (e.g., on increased playback speed). In this case, the <see cref="_frameOverrunUs" /> will be
    /// greater than zero. This is indicated by the return value: true if <c>_frameOverrunUs</c> is zero (or less).
    /// </remarks>
    /// <returns>True if a sufficient time has elapsed and a frame flush should occur.</returns>
    private bool OnEndFrame(uint frameDelta)
    {
      // Forward playback. Update current frame.
      lock(this)
      {
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
          if (_frameOverrunUs <= frameDeltaUs || _skippedFrameCount >= _maxFrameSkip)
          {
            _frameDelayUs = frameDeltaUs - _frameOverrunUs;
            // Overrun has been applied.
            _frameOverrunUs = 0;
            _skippedFrameCount = 0;
          }
          else
          {
            // Haven't handled the overrun. Need to skip multiple frames.
            _frameDelayUs = 0;
            _frameOverrunUs -= frameDeltaUs;
            ++_skippedFrameCount;
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

      // Catching up if we are targeting a frame ahead of the next frame or we have not processed sufficient time.
      // The latter case is indicated by _frameOverrunUs being non-zero.
      _catchingUp = _currentFrame + 1 < _targetFrame;
      return _frameOverrunUs <= 0;
    }

    #region Keyframes

    /// <summary>
    /// Try find a keyframe near the given frame number.
    /// </summary>
    /// <param name="targetFrame">The frame number we are trying to achieve.</param>
    /// <param name="currentFrame">The current frame number. A valid keyframe must occur after this number.</param>
    /// <returns>The frame number reached by the keyframe.</returns>
    /// <remarks>
    /// Calling this change the <see cref="_packetStream"/> position. It is recommended the stream be reset
    /// on failure along with calling <see cref="ResetQueue(uint)"/>.
    /// </remarks>
    private bool TryRestoreKeyframe(out Keyframe keyframe, uint targetFrame, uint currentFrame = 0)
    {
      keyframe = null;
      lock (_keyframes)
      {
        foreach (Keyframe snap in _keyframes)
        {
          // Suitable keyframe if it's before the target frame, or
          // at the target frame and includes transient objects.
          // See Keyframe class remarks.
          if (snap.Valid &&
             (snap.FrameNumber <= targetFrame || snap.IncludesTransient && snap.FrameNumber == targetFrame))
          {
            if (snap.FrameNumber > currentFrame)
            {
              keyframe = snap;
            }
          }
          else
          {
            break;
          }
        }

        if (keyframe != null)
        {
          if (RestoreKeyframe(keyframe))
          {
            return true;
          }
        }
      }

      return false;
    }

    private Keyframe FindKeyframe(uint targetFrame)
    {
      if (!AllowKeyframes)
      {
        return null;
      }

      Keyframe bestShot = null;
      lock (_keyframes)
      {
        foreach (Keyframe keyframe in _keyframes)
        {
          if (keyframe.FrameNumber <= targetFrame)
          {
            if (keyframe.Valid)
            {
              bestShot = keyframe;
            }
          }
          else
          {
            break;
          }
        }
      }

      return bestShot;
    }

    /// <summary>
    /// Restore the given keyframe.
    /// </summary>
    /// <param name="keyframe">To restore</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// On success, the <see cref="PacketQueue"/> is cleared, a reset packet pushed followed by the
    /// decoded keyframe packets. On failure <see cref="ResetStream()"/> is called which includes
    /// queueing a reset packet.
    /// </remarks>
    private bool RestoreKeyframe(Keyframe keyframe)
    {
      if (!keyframe.Valid || string.IsNullOrEmpty(keyframe.TemporaryFilePath) || !File.Exists(keyframe.TemporaryFilePath))
      {
        return false;
      }

      PacketStreamReader keyframeStream = null;
      try
      {
        // Ensure stream has been reset.
        keyframeStream = new PacketStreamReader(
                                new FileStream(keyframe.TemporaryFilePath, FileMode.Open, FileAccess.Read));
        System.Collections.Generic.List<PacketBuffer> decodedPackets = new System.Collections.Generic.List<PacketBuffer>();
        _packetStream.Seek(keyframe.StreamOffset, SeekOrigin.Begin);
        long streamPos = _packetStream.Position;

        // Decode the keyframe data.
        if (streamPos == keyframe.StreamOffset)
        {
          long processedBytes = 0;
          PacketBuffer packet = null;
          try
          {
            while ((packet = keyframeStream.NextPacket(ref processedBytes)) != null)
            {
              decodedPackets.Add(packet);
            }
          }
          catch (TesIOException e)
          {
            Log.Exception(e);
            return false;
          }

          uint currentFrame = keyframe.OffsetFrameNumber;
          uint droppedPacketCount = 0;

          // The stream seek position may not exactly match the frame end marker such as when it appears within a
          // collated packet. We must therefore continue to read messages from the main file stream (after seeking)
          // until we are at the keyframe number. We can ignore all these messages as the keyframe includes their
          // side effects.
          while (currentFrame < keyframe.FrameNumber)
          {
            long bytesRead = 0;
            packet = _packetStream.NextPacket(ref bytesRead);
            if (packet != null)
            {
              if (packet.Status == PacketBufferStatus.Complete)
              {
                ++droppedPacketCount;
                if (packet.Header.RoutingID == (ushort)RoutingID.Control &&
                    packet.Header.MessageID == (ushort)ControlMessageID.EndFrame)
                {
                  ++currentFrame;
                }
              }
              else
              {
                // Failed.
                Log.Error("Incomplete packet during processing of extra keyframe packets");
                return false;
              }
            }
            else
            {
              // Failed.
              Log.Error("Failed to process extra keyframe packets");
              return false;
            }
          }

          // We are now up to where we should be. Send reset and migrate the loaded packets into the packet queue for
          // processing.
          ResetQueue(currentFrame);
          _packetQueue.Enqueue(decodedPackets);
          _currentFrame = currentFrame;

          Log.Diag("Dropped {0} additional packets to catch up to frame {1}.", droppedPacketCount, _currentFrame);
          Log.Info("Restored frame: {0} -> {1}", _currentFrame, _targetFrame);
          if (_targetFrame == _currentFrame)
          {
            _targetFrame = 0;
          }
          return true;
        }
      }
      catch (Exception e)
      {
        if (keyframeStream != null)
        {
          // Explicitly close the stream to make sure we aren't hanging on to handles we shouldn't.
          keyframeStream.Close();
          keyframeStream = null;
        }
        Log.Exception(e);
      }

      Log.Error("Failed to decode keyframe for frame {0}", keyframe.FrameNumber);
      return false;
    }

    /// <summary>
    /// Release all keyframes and cleanup the temporary files.
    /// </summary>
    private void CleanupKeyframes()
    {
      lock (_keyframes)
      {
        // Look for the keyframe.
        Keyframe keyframe = null;
        for (int i = 0; i < _keyframes.Count; ++i)
        {
          keyframe = _keyframes[i];
          if (keyframe.OpenStream != null)
          {
            keyframe.OpenStream.Close();
            keyframe.OpenStream = null;
          }
          if (!string.IsNullOrEmpty(keyframe.TemporaryFilePath) && File.Exists(keyframe.TemporaryFilePath))
          {
            File.Delete(keyframe.TemporaryFilePath);
            keyframe.TemporaryFilePath = null;
          }
        }
        _keyframes.Clear();
      }
    }

    /// <summary>
    /// Request a stream for the keyframe of the given frame number.
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <returns></returns>
    /// <remarks>
    /// The caller must save the keyframe to the stream then call <see cref="ReleaseKeyframeStream(Stream, bool)"/>
    /// regardless of success.
    /// </remarks>
    public Stream RequestKeyframeStream(uint frameNumber)
    {
      lock (_keyframes)
      {
        // Look for the keyframe.
        Keyframe keyframe = null;
        for (int i = 0; i < _keyframes.Count; ++i)
        {
          if (_keyframes[i].FrameNumber == frameNumber)
          {
            keyframe = _keyframes[i];
            //keyframe.TemporaryFilePath = Path.GetFullPath(Path.Combine("temp", Path.GetRandomFileName()));
            keyframe.TemporaryFilePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Log.Info("Keyframe path {0}", keyframe.TemporaryFilePath);
            try
            {
              keyframe.OpenStream = new FileStream(keyframe.TemporaryFilePath, FileMode.Create);
              return keyframe.OpenStream;
            }
            catch (Exception e)
            {
              Log.Exception(e);
            }
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Release a keyframe stream.
    /// </summary>
    /// <param name="frameNumber">The frame number the snap shop serialised.</param>
    /// <param name="keyframeStream">The stream to release</param>
    /// <param name="includesTransient">Does the stream include transient objects? See <see cref="Keyframe"/> remarks.</param>
    /// <param name="valid">True if the keyframe was successful.</param>
    public void ReleaseKeyframeStream(uint frameNumber, Stream keyframeStream, bool includesTransient, bool valid)
    {
      lock (_keyframes)
      {
        // Look for the keyframe.
        Keyframe keyframe = null;
        for (int i = 0; i < _keyframes.Count; ++i)
        {
          if (_keyframes[i].FrameNumber == frameNumber)
          {
            keyframe = _keyframes[i];
            keyframe.IncludesTransient = includesTransient;
            // Validate.
            keyframe.Valid = valid && keyframeStream != null && keyframeStream == keyframe.OpenStream;
            // Ensure the stream is properly closed.
            if (keyframe.OpenStream != null && keyframe.OpenStream.CanWrite)
            {
              keyframe.OpenStream.Flush();
              keyframe.OpenStream.Close();
            }
            keyframe.OpenStream = null;
            // Delete temporary file.
            if (!string.IsNullOrEmpty(keyframe.TemporaryFilePath) && !valid && File.Exists(keyframe.TemporaryFilePath))
            {
              File.Delete(keyframe.TemporaryFilePath);
              keyframe.TemporaryFilePath = null;
            }
            if (valid)
            {
              Log.Info("Keyframe {0} complete", frameNumber);
            }
            else
            {
              Log.Warning("Keyframe {0} failed", frameNumber);
            }
            return;
          }
        }
      }
    }

    /// <summary>
    /// Is there a keyframe available for the given frame number?
    /// </summary>
    /// <param name="frameNumber">The desired frame number.</param>
    /// <returns>True when there is a valid keyframe available for <paramref name="frameNumber"/>.</returns>
    public bool HaveKeyframe(uint frameNumber)
    {
      lock (_keyframes)
      {
        // Look for the keyframe.
        for (int i = 0; i < _keyframes.Count; ++i)
        {
          if (_keyframes[i].FrameNumber == frameNumber)
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Request the additional of a keyframe at the given frame and post the associated control message.
    /// </summary>
    /// <param name="frameNumber">The keyframe frame number.</param>
    /// <param name="streamOffset">The stream position/bytes read (total) on this frame.</param>
    private void RequestKeyframe(uint frameNumber, uint offsetFrameNumber, long streamOffset)
    {
      if (HaveKeyframe(frameNumber) || !AllowKeyframes)
      {
        return;
      }

      // Add a placeholder.
      Keyframe keyframe = new Keyframe();
      keyframe.FrameNumber = frameNumber;
      keyframe.OffsetFrameNumber = offsetFrameNumber;
      keyframe.StreamOffset = streamOffset;
      lock (_keyframes)
      {
        _keyframes.Add(keyframe);
      }

      PacketBuffer packet = new PacketBuffer(PacketHeader.Size + 32);
      ControlMessage message = new ControlMessage();

      message.ControlFlags = 0;
      message.Value32 = frameNumber;
      message.Value64 = 0;

      packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.Keyframe);
      message.Write(packet);

      if (packet.FinalisePacket())
      {
        _packetQueue.Enqueue(packet);
      }
    }

    #endregion

    /// <summary>
    /// Creates a frame flush packet targeting the given frame number.
    /// </summary>
    /// <param name="frameNumber">Optional frame number to flush with (zero for none).</param>
    /// <returns></returns>
    /// <remarks>
    /// The returned packet will ensure visualisation of a frame and that the current frame is set
    /// to <paramref name="frameNumber"/>. Transient objects are preserved.
    /// </remarks>
    PacketBuffer CreateFrameFlushPacket(uint frameNumber)
    {
      PacketBuffer packet = new PacketBuffer();
      ControlMessage message = new ControlMessage();

      message.ControlFlags = (uint)EndFrameFlag.Persist;
      message.Value32 = 0;
      message.Value64 = frameNumber;

      packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.ForceFrameFlush);
      message.Write(packet);
      packet.FinalisePacket();
      return packet;
    }

    /// <summary>
    /// Resets the packet queue, clearing the contents and enqueing a reset packet.
    /// </summary>
    void ResetQueue(uint currentFrame)
    {
      _packetQueue.Clear();
      _packetQueue.Enqueue(BuildResetPacket(currentFrame));
      _currentFrame = currentFrame;
      _frameDelayUs = _frameOverrunUs = 0;
      _catchingUp = false;
    }

    //private Queue<PacketBuffer> _packetQueue = new Queue<PacketBuffer>();
    private Tes.Collections.Queue<PacketBuffer> _packetQueue = new Tes.Collections.Queue<PacketBuffer>();
    private PacketStreamReader _packetStream = null;

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
    /// Number of frames which have been skipped in trying to maintain the target frame rate.
    /// </summary>
    private uint _skippedFrameCount = 0;
    /// <summary>
    /// Maximum number of frames which may be skipped to try and catch up.
    /// </summary>
    private uint _maxFrameSkip = 100;

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
    private volatile bool _paused = false;
    /// <summary>
    /// Set when the current frame is behind the target frame.
    /// </summary>
    /// <remarks>
    /// Should be cleared before processing the last frame so that the last frame is
    /// processed with this flag false.
    /// </remarks>
    private volatile bool _catchingUp = false;
    private volatile bool _loop = false;
    /// <summary>
    /// A list of frame keyframes to improve step back behaviour.
    /// </summary>
    private System.Collections.Generic.List<Keyframe> _keyframes = new System.Collections.Generic.List<Keyframe>();
  }
}
