using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
// using Ionic.Zlib;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using UnityEngine;
using UnityEngine.Events;
using Tes.IO;
using Tes.Logging;
using Tes.Net;
using Tes.Runtime;

namespace Tes.Main
{
  /// <summary>
  /// Active mode options for the <see cref="Rounter"/>.
  /// </summary>
  public enum RouterMode
  {
    /// <summary>
    /// Inactive.
    /// </summary>
    Idle,
    /// <summary>
    /// Connecting to a remote process.
    /// </summary>
    Connecting,
    /// <summary>
    /// Connected and viewing a remote process.
    /// </summary>
    Connected,
    /// <summary>
    /// Connected. and recording a remote process.
    /// </summary>
    Recording,
    /// <summary>
    /// Playing a recording from a file/stream..
    /// </summary>
    Playing,
    /// <summary>
    /// Paused playback of a file/stream.
    /// </summary>
    Paused,
    /// <summary>
    /// Stopped playback (end of stream).
    /// </summary>
    /// <remarks>>
    /// From this state we should be able to step back and resume playback inside the stream.
    /// </remarks>
    Stopped
  }

  /// <summary>
  /// The Tes <see cref="Router"/> is the main control process for 3rd Eye Scene.
  /// </summary>
  /// <remarks>
  /// The <see cref="Router"/> manages the data active streaming thread and routes
  /// incoming messages to registered handlers.
  /// </remarks>
  public class Router : MonoBehaviour
  {
    [System.Serializable]
    public class ModeEventEvent : UnityEvent<RouterMode>
    {
    }

    [SerializeField]
    protected ModeEventEvent _onModeChange = new ModeEventEvent();
    public ModeEventEvent OnModeChange { get { return _onModeChange; } }

    /// <summary>
    /// Reports the current mode.
    /// </summary>
    public RouterMode Mode
    {
      get
      {
        NetworkThread netThread = _dataThread as NetworkThread;
        StreamThread streamThread = _dataThread as StreamThread;

        if (streamThread != null)
        {
          if (streamThread.Eos)
          {
            return RouterMode.Stopped;
          }
          else if (streamThread.Paused)
          {
            return RouterMode.Paused;
          }
          return RouterMode.Playing;
        }

        if (netThread != null)
        {
          switch (netThread.Status)
          {
          case NetworkThreadStatus.None:
            return RouterMode.Idle;
          case NetworkThreadStatus.Connecting:
          case NetworkThreadStatus.Reconnecting:
            return RouterMode.Connecting;
          case NetworkThreadStatus.Connected:
            if (_recordingWriter != null)
            {
              return RouterMode.Recording;
            }
            return RouterMode.Connected;
          default:
            break;
          }
        }

        return RouterMode.Idle;
      }
    }

    public bool Connected
    {
      get
      {
        RouterMode m = Mode;
        return m == RouterMode.Connected || m == RouterMode.Connecting || m == RouterMode.Recording;
      }
    }

    /// <summary>
    /// Reports the current frame number.
    /// </summary>
    /// <remarks>
    /// During live view and recording this is always one less than the <see cref="TotalFrames"/>.
    /// This only equals the <see cref="TotalFrames"/> when playback ends (stopped state).
    /// </remarks>
    public uint CurrentFrame { get { return _currentFrame; } }

    /// <summary>
    /// Reports the previous frame viewed.
    /// </summary>
    /// <remarks>
    /// This will generally be <c>CurrentFrame - 1</c> during normal playback. When stepping back, this will
    /// report <c>CurrentFrame + 1</c> and when stepping between non-sequential frames, this value will report
    /// the last requested frame.
    /// </remarks>
    public uint PreviousFrame { get { return _previousFrame; } }

    /// <summary>
    /// Reports the total number of frames.
    /// </summary>
    /// <remarks>
    /// This continually increases from 1 on the first frame during live view and recording.
    /// During playback, this is set to the number of frames reported by the recorded file.
    /// </remarks>
    public uint TotalFrames { get { return _totalFrames; } }

    /// <summary>
    /// Access to the registered <see cref="MessageHandler"/> objects.
    /// </summary>
    public MessageHandlerLibrary Handlers { get { return _handlers; } }

    /// <summary>
    /// Access to the known materials library.
    /// </summary>
    public MaterialLibrary Materials { get { return _materials; } }

    /// <summary>
    /// Access to the scene root.
    /// </summary>
    public Scene Scene { get { return _scene; } }

    /// <summary>
    /// Access the most up to date server info.
    /// </summary>
    public ServerInfoMessage ServerInfo { get { return _serverInfo; } }

    public bool Looping
    {
      get { return PlaybackSettings.Instance.Looping; }
      set
      {
        PlaybackSettings.Instance.Looping = value;
        StreamThread streamThread = _dataThread as StreamThread;
        if (streamThread != null)
        {
          streamThread.Loop = value;
        }
      }
    }

    /// <summary>
    /// Playback speed scaling.
    /// </summary>
    public float PlaybackSpeed
    {
      get { return _playbackSpeed; }
      set
      {
        _playbackSpeed = value;
        StreamThread streamThread = _dataThread as StreamThread;
        if (streamThread != null)
        {
          streamThread.PlaybackSpeed = value;
        }
      }
    }

    /// Request a handler for the given routing id.
    /// </summary>
    /// <param name="id">ID of the required message handler.</param>
    /// <returns>The <see cref="MessageHandler"/> with <see cref="MessageHandler.RoutingID">RoutingID</see>
    /// matching <paramref name="id"/>.</returns>
    public MessageHandler GetHandler(ushort id) { return _handlers.HandlerFor(id); }

    public float FrameTime
    {
      get
      {
        NetworkThread netThread = _dataThread as NetworkThread;
        if (netThread != null)
        {
          return netThread.FrameTime;
        }
        return 0;
      }
    }

    public float AverageFrameTime
    {
      get
      {
        NetworkThread netThread = _dataThread as NetworkThread;
        if (netThread != null)
        {
          return netThread.AverageFrameTime;
        }
        return 0;
      }
    }

    /// <summary>
    /// Access the current network connection end point.
    /// </summary>
    /// <value>The connected end point or null while not connected.</value>
    public IPEndPoint Connection
    {
      get
      {
        NetworkThread netThread = _dataThread as NetworkThread;
        if (netThread != null)
        {
          return netThread.EndPoint;
        }
        return null;
      }
    }

    /// <summary>
    /// Attempt to resolve a name for a routing ID based on the known ID set.
    /// </summary>
    /// <param name="id">The ID to resolve.</param>
    /// <returns>The name corresponding to <paramref name="id"/>, or an empty string when
    /// unknown.</returns>
    /// <remarks>
    /// This method checks a set of known routing ID enumerations for a match and converts
    /// the entry to string when matched. The following enumerations are checked:
    /// <list type="bullet">
    /// <item><see cref="Tes.Net.RoutingID"/></item>
    /// <item><see cref="Tes.Net.ShapeID"/></item>
    /// </list>
    /// </remarks>
    public string RoutingIDName(ushort id)
    {
      Type[] enumTypes = new Type[]
      {
        typeof(RoutingID),
        typeof(ShapeID)
      };

      // Do RoutingID explicitly as we ignore some entries.
      int firstTypeIndex = 0;

      // Skip RoutingID if we are beyond RoutingID.ShapeIDsStart
      // as we don't want to return any of the control values like ShapeIDsStart
      // or UserIDStart
      if (id >= (ushort)RoutingID.ShapeIDsStart)
      {
        ++firstTypeIndex;
      }

      for (int i = firstTypeIndex; i < enumTypes.Length; ++i)
      {
        Type enumType = enumTypes[i];
        if (enumType.IsEnum && Enum.IsDefined(enumType, id))
        {
          foreach (var value in Enum.GetValues(enumType))
          {
            try
            {
              if ((ushort)value == id)
              {
                // Found a match. Convert to string.
                return Enum.GetName(enumType, value);
              }
            }
            catch (InvalidCastException )
            {
              // Not sure why I'm getting this cast exception. Oddly, it trips
              // when value and id are not equal, but not when they are equal.
            }
          }
        }
      }

      // Not found in the lists. Find the appropriate hander and use its name.
      MessageHandler handler = Handlers.HandlerFor(id);
      if (handler != null)
      {
        return handler.Name;
      }

      return id.ToString();
    }

    /// <summary>
    /// Attempts to open a file stream to <see cref="fileName"/> and playback the recorded packets.
    /// </summary>
    /// <param name="fileName">The name of the file to open and playback.</param>
    /// <returns>True on successfully opening the file and validating the header.</returns>
    /// <remarks>
    /// On success, this sets the active data thread to a <see cref="StreamThead"/> and continues
    /// to playback messages from this stream.
    ///
    /// Any active data stream is terminated before attempting to open <paramref name="fileName"/>.
    /// </remarks>
    public bool OpenFile(string fileName)
    {
      Reset();
      if (!File.Exists(fileName))
      {
        return false;
      }
      Application.runInBackground = true;
      StreamThread thread = new StreamThread();
      thread.RoutingIDName = RoutingIDName;
      _dataThread = thread;
      PlaybackSettings playbackSettings = PlaybackSettings.Instance;
      if (playbackSettings != null)
      {
        thread.AllowKeyframes = playbackSettings.AllowKeyframes;
        thread.KeyframeMiB = playbackSettings.KeyframeEveryMiB;
        thread.KeyframeMinFrames = (uint)Math.Max(0, playbackSettings.KeyframeFrameSeparation);
        thread.KeyframeSkipForwardFrames = (uint)Math.Max(0, playbackSettings.KeyframeSkipForwardFrames);
        thread.KeyframeFrames = (uint)Math.Max(0, playbackSettings.KeyframeEveryFrames);
        thread.Loop = playbackSettings.Looping;
      }
      thread.PlaybackSpeed = PlaybackSpeed;
      Stream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      if (!thread.Start(inputStream))
      {
        Log.Error("Failed to start stream thread with file: {0}", fileName);
        _dataThread.Quit();
        _dataThread.Join();
        _dataThread = null;
        Application.runInBackground = false;
        return false;
      }

      return true;
    }

    /// <summary>
    /// Attempts to connect to a 3rd Eye Scene server at the given adress, <paramref name="endPoint"/>.
    /// </summary>
    /// <param name="endPoint">The IP end point at which the server can be found.</param>
    /// <param name="autoReconnect">True to keep attempting the connection if the initial connection fails.</param>
    /// <returns>True on successfully connecting to the server.</returns>
    /// <remarks>
    /// On success, this sets the active data thread to a <see cref="NetworkThread"/> and continues
    /// to read and process messages from this stream.
    ///
    /// Any active data stream is terminated before attempting to open the network connection.
    /// </remarks>
    public bool Connect(IPEndPoint endPoint, bool autoReconnect)
    {
      Reset();
      NetworkThread thread = new NetworkThread();
      thread.RoutingIDName = RoutingIDName;
      // Run in background with a network thread.
      Application.runInBackground = true;
      _dataThread = thread;
      if (!thread.Start(endPoint, autoReconnect))
      {
        _dataThread.Quit();
        _dataThread.Join();
        _dataThread = null;
        Application.runInBackground = false;
        return false;
      }
      return true;
    }

    /// <summary>
    /// Toggle playback pause state when playing back a file stream..
    /// </summary>
    /// <remarks>
    /// Only takes effect if the active data thread is a <see cref="StreamThread"/>.
    /// </remarks>
    public void TogglePause()
    {
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        _dataThread.TargetFrame = _dataThread.CurrentFrame;
        _dataThread.Paused = !_dataThread.Paused;
        Application.runInBackground = !_dataThread.Paused;
      }
    }

    /// <summary>
    /// Disconnect from the current network connection.
    /// </summary>
    /// <remarks>
    /// Only takes effect if the active data thread is a <see cref="NetworkThread"/>.
    /// </remarks>
    public void Disconnect()
    {
      if (_dataThread as NetworkThread != null)
      {
        Reset();
      }
    }

    /// <summary>
    /// Start recording the current network stream to <paramref cref="filePath">
    /// </summary>
    /// <param name="filePath">The file path to save the recording to.</param>
    /// <returns><code>true</code> on successfully starting recording.</returns>
    /// <remarks>
    /// Only takes effect if the active thread is a <see cref="NetworkThread"/>.
    /// </remarks>
    public bool StartRecording(string filePath)
    {
      _recordOnConnectPath = null;
      if (_dataThread == null)
      {
        // Record on connect request.
        StopRecording();
        _recordOnConnectPath = filePath;
        Dialogs.MessageBox.Show(null, string.Format("Record on connect to: {0}", _recordOnConnectPath), "Record on Connect");
        Log.Info("Record on connect " + filePath);
        return true;
      }

      // Active recording request.
      NetworkThread netThread = _dataThread as NetworkThread;
      if (netThread == null)
      {
        return false;
      }

      StopRecording();
      try
      {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        _currentFrame = 0;
        _previousFrame = 0;
        _totalFrames = 0;

        bool ok;
        BinaryWriter writer = SerialiseScene(fileStream, true, out ok);
        // Write the initial camera position.
        WriteCameraPosition(writer, Camera.main, 255);
        WriteFrameFlush(writer);
        _recordingWriter = writer;
        _recordingFileStream = fileStream;
      }
      catch (Exception e)
      {
        Log.Exception(e);
      }

      return _recordingWriter != null;
    }

    /// <summary>
    /// Stop the current recording.
    /// </summary>
    public void StopRecording()
    {
      if (_recordingWriter != null)
      {
        FinaliseFrameCount(_recordingWriter, _recordingFileStream, CurrentFrame);
        _recordingWriter.Flush();
        _recordingWriter.Close();
        _recordingWriter = null;
        _recordingFileStream = null;
      }
    }

    public void Stop()
    {
      Reset();
    }

    public void StepForward()
    {
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        _dataThread.Paused = true;
        _dataThread.TargetFrame = _currentFrame + 1;
        _previousFrame = _currentFrame;
        // Temporarily update in background mode until the frame is processed
        Application.runInBackground = true;
      }
    }

    public void StepBackward()
    {
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        _dataThread.Paused = true;
        if (_currentFrame > 0)
        {
          _dataThread.TargetFrame = _currentFrame - 1;
          _previousFrame = _currentFrame;
        }
        // Temporarily update in background mode until the frame is processed
        Application.runInBackground = true;
      }
    }

    public void SkipStart()
    {
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        _dataThread.Paused = true;
        _dataThread.TargetFrame = 1;
        _previousFrame = _currentFrame;
        // Temporarily update in background mode until the frame is processed
        Application.runInBackground = true;
      }
    }

    public void SkipEnd()
    {
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        _dataThread.Paused = true;
        _dataThread.TargetFrame = _dataThread.TotalFrames;
        _previousFrame = _currentFrame;
        // Temporarily update in background mode until the frame is processed
        Application.runInBackground = true;
      }
    }

    /// <summary>
    /// Step to the <see cref="PreviousFrame"/>.
    /// </summary>
    public void StepPrevious()
    {
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        _dataThread.Paused = true;
        _dataThread.TargetFrame = _previousFrame;
        _previousFrame = _currentFrame;
        // Temporarily update in background mode until the frame is processed
        Application.runInBackground = true;
      }
    }

    /// <summary>
    /// Set a new target frame to skip to. Should be in range [0, <see cref="TotalFrames"/>].
    /// </summary>
    /// <param name="targetFrame">The frame to skip to.</param>
    /// <remarks>
    /// Setting the target frame will pause current playback.
    /// </remarks>
    public void SetFrame(uint targetFrame)
    {
      if (_dataThread != null)
      {
        _dataThread.Paused = true;
        _dataThread.TargetFrame = targetFrame;
        _previousFrame = _currentFrame;
        // Temporarily update in background mode until the frame is processed
        Application.runInBackground = true;
      }
    }

    public void Reset(bool partialReset = false)
    {
      StopRecording();
      if (_dataThread != null)
      {
        _dataThread.Quit();
        _dataThread.Join();
        _dataThread = null;
      }
      if (!partialReset)
      {
        ResetScene();
      }
      // No longer run in background. Will again once we have a DataThread
      Application.runInBackground = false;
    }

    public void InitialiseHandlers()
    {
      foreach (MessageHandler handler in _handlers.Handlers)
      {
        handler.Initialise(_scene.Root, _scene.ServerRoot, _materials);
      }
    }

    protected void ResetScene()
    {
      foreach (MessageHandler handler in _handlers.Handlers)
      {
        handler.Reset();
      }
      _previousFrame = _currentFrame = _totalFrames = 0u;
    }

    protected virtual void Start()
    {
      // Ensure the array buffers are allocated without thread contention.
      // This is being a little cautious because the Shared property is meant
      // to make the allocation using System.Thread.Volatile, which doesn't exist
      // in the Unity .Net runtime.
      {
        var a = Buffers.ArrayPool<byte>.Shared;
        var b = Buffers.ArrayPool<short>.Shared;
        var c = Buffers.ArrayPool<int>.Shared;
        var d = Buffers.ArrayPool<uint>.Shared;
      }
      if (_scene == null)
      {
        _scene = new Scene();
      }

      if (PlaybackSettings.Instance != null)
      {
        PlaybackSettings.Instance.PropertyChanged += OnPropertyChanged;
      }

      Log.AddTarget(new LogAdaptor());
    }

    /// <summary>
    /// Render frame update function. Pumps the packet queue and routes to registered
    /// <see cref="Tes.Runtime.MessageHandler"/> objects.
    /// </summary>
    protected virtual void Update()
    {
      try
      {
        // FIXME: manage propagating dynamic settings better.
        Materials.DefaultPointSize = RenderSettings.Instance.DefaultPointSize;
        var longFrameTimer = new System.Diagnostics.Stopwatch();

        longFrameTimer.Start();

        if (_dataThread != null)
        {
          if (Mode == RouterMode.Playing)
          {
            // Can't record a playback stream.
            _recordOnConnectPath = null;
          }

          // Move data packets from the data thread into the local queue. We are looking for an end frame control
          // message, at which point we will pass all messages on to the appropriate handlers to enact.
          //
          // We also need to handle 'catchUp' situations where the target frame is well in advance of the next frame.
          // In this case we keep processing the packet queue until catchUp is complete (at target frame).
          PacketBuffer packet = null;
          bool catchUp = false;
          bool frameFlush = false;
          bool processingPackets = true;
          while (!frameFlush && processingPackets || catchUp)
          {
            // Set catchup state on data handlers and check update catch up status.
            catchUp = UpdateCatchup(catchUp);
            if ((processingPackets = _dataThread.PacketQueue.TryDequeue(ref packet)))
            {
              // Handle record on connect.
              if (!string.IsNullOrEmpty(_recordOnConnectPath))
              {
                if (!StartRecording(_recordOnConnectPath))
                {
                  Dialogs.MessageBox.Show(null, string.Format("Unable Failed to start recording to {0}", _recordOnConnectPath), "Recording Error");
                }
              }
              _recordOnConnectPath = null;

              //Log.Diag("Routing ID: {0}:{1}", RoutingIDName(packet.Header.RoutingID), packet.Header.MessageID);

              // New message. First handle command/control messages.
              if (packet.Status == PacketBufferStatus.Complete)
              {
                if (_recordingWriter != null)
                {
                  packet.ExportTo(_recordingWriter);
                }

                // Tracks whether we will push the packet into the _pendingPackets queue. The default beahviour is to
                // push the packet, however, some control messages are processed immediately.
                bool pushPacketToPending = true;
                NetworkReader packetReader;
                switch ((RoutingID)packet.Header.RoutingID)
                {
                  case RoutingID.ServerInfo:
                    // Server info is handled immediately.
                    pushPacketToPending = false;
                    packetReader = new NetworkReader(packet.CreateReadStream(true));
                    _serverInfo.Read(packetReader);
                    OnServerInfoUpdate();
                    //_timeUnitInv = (_serverInfo.TimeUnit != 0) ? 1.0 / _serverInfo.TimeUnit : 0.0;
                    Scene.Frame = _serverInfo.CoordinateFrame;
                    foreach (MessageHandler handler in _handlers.Handlers)
                    {
                      handler.UpdateServerInfo(_serverInfo);
                    }
                    break;

                  case RoutingID.Control:
                    packetReader = new NetworkReader(packet.CreateReadStream(true));
                    ControlMessage message = new ControlMessage();
                    if (message.Read(packetReader))
                    {
                      // Special message handling.
                      switch ((ControlMessageID)packet.Header.MessageID)
                      {
                        // Scene reset request
                        case ControlMessageID.Reset:
                          // Reset is handled immediately.
                          pushPacketToPending = false;
                          // Drop pending packets.
                          _pendingPackets.Clear();
                          catchUp = UpdateCatchup(catchUp, true);
                          uint previousFrame = _currentFrame;
                          // Reset all the data handlers, but not the data thread.
                          ResetScene();
                          // Force a frame flush.
                          EndFrame(message.Value32, catchUp);
                          _previousFrame = previousFrame;
                          break;
                        // Key frame request message. Need to make sure we force display of everything even when in
                        // catch up mode.
                        case ControlMessageID.Keyframe:
                          // Redundant statement, however it's here to illustrate that key frame packets should be
                          // pushed onto the _pendingPacket queue.
                          pushPacketToPending = true;
                          catchUp = UpdateCatchup(catchUp, true);
                          break;
                        // Frame end or force flush: ensure the scene is rendered.
                        case ControlMessageID.EndFrame:
                        case ControlMessageID.ForceFrameFlush:
                          // End frame and flush messages are handled immediately by calling EndFrame().
                          pushPacketToPending = false;
                          EndFrame((uint)message.Value64, catchUp, (message.ControlFlags & (ushort)EndFrameFlag.Persist) != 0);
                          if (_recordingWriter != null)
                          {
                            WriteCameraPosition(_recordingWriter, Camera.main, 255);
                          }
                          frameFlush = packet.Header.MessageID == (ushort)ControlMessageID.EndFrame &&
                                      (message.ControlFlags & (uint)EndFrameFlag.Flush) != 0 &&
                                      _dataThread.TargetFrame == 0;

                          // Also allow a frame flush if we've been looping of a long time. This is to prevent freezing.
                          if (_dataThread.TargetFrame == 0 && longFrameTimer.ElapsedMilliseconds > 1000)
                          {
                            frameFlush = true;
                          }
                          break;

                        default: // packet.Header.MessageID
                          // No-op
                          break;
                      }
                    }
                    break;

                  default: // packetHeader.RoutingID
                    // No-op
                    break;
                }

                if (pushPacketToPending)
                {
                  _pendingPackets.Enqueue(packet);
                }
              }
              else
              {
                Log.Error("Dropping bad packet with routing ID: {0}", (ushort)packet.Header.RoutingID);
              }
            }
          }
        }

        RouterMode curMode = Mode;
        if (_lastMode != curMode)
        {
          _lastMode = curMode;
          if (_onModeChange != null)
          {
            _onModeChange.Invoke(curMode);
          }
        }

        // This is better tied to a true pre-cull or pre-render, but this object has no visual
        // so that doesn't get called. Instead we assume the Update() call is tightly bound to
        // the render frame rate (as opposed to FixedUpdate()).
        foreach (MessageHandler handler in Handlers.Handlers)
        {
          handler.PreRender();
        }

        Log.Flush();
      }
      finally
      {
        foreach (MessageHandler handler in Handlers.Handlers)
        {
          handler.Mode = handler.Mode & ~MessageHandler.ModeFlags.IgnoreTransient;
        }

        Application.runInBackground = _dataThread != null && !_dataThread.Paused && !_dataThread.CatchingUp;
      }
    }

    /// <summary>
    /// Handle changes it the background mode/pause status of the application by managing the data thread.
    /// </summary>
    /// <param name="pauseStatus">True when the application now paused/in background mode false on unpause.</param>
    /// <remarks>
    /// This ensures the <see cref="DataThread" /> is suspended when in background mode and resumed when in foreground
    /// mode.
    /// </remarks>
    void OnApplicationPause(bool pauseStatus)
    {
      // Ensure data thread suspension matches state of Application.runInBackground.
      if (_dataThread != null)
      {
        if (_dataThread.IsSuspended != pauseStatus)
        {
          if (_dataThread.IsSuspended)
          {
            _dataThread.Resume();
          }
          else
          {
            _dataThread.Suspend();
          }
        }
      }
    }

    ///// <summary>
    ///// Triggers <see cref="Tes.Runtime.MessageHandler.PreRender()"/> calls in all handlers.
    ///// </summary>
    //public void OnPreRender()
    //{
    //  foreach (MessageHandler handler in Handlers.Handlers)
    //  {
    //    handler.PreRender();
    //  }
    //}

    public void HandleControlMessage(PacketBuffer packet, BinaryReader reader, bool catchingUp)
    {
      ControlMessage message = new ControlMessage();
      if (message.Read(reader))
      {
        switch ((ControlMessageID)packet.Header.MessageID)
        {
        case ControlMessageID.EndFrame:
        case ControlMessageID.ForceFrameFlush:
        case ControlMessageID.Reset:
          // Noop. Already handled when dequeued.
          break;
        case ControlMessageID.CoordinateFrame:
          if (Scene != null && Scene.Root != null)
          {
            Scene.Frame = (CoordinateFrame)message.Value32;
          }
          break;
        case ControlMessageID.Keyframe:
          GenerateKeyframe(message.Value32, !catchingUp);
          break;
        default:
          break;
        }
      }
      else
      {
        Log.Error("Malformed control message.");
      }
    }

    /// <summary>
    /// Called when the server info message comes through and _serverInfo is valid.
    /// </summary>
    protected virtual void OnServerInfoUpdate()
    {
    }

    /// <summary>
    /// Handler for value changes in user settings.
    /// </summary>
    /// <param name="sender">The settings object for which a value is changing. Generally derived from
    ///   <see cref="Settings"/>, though not essential.</param>
    /// <param name="args">Event arguments. Identifies the changed property.</param>
    protected void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      // Looking for changes to PlaybackSettings.
      PlaybackSettings playback;
      if ((playback = sender as PlaybackSettings) != null)
      {
        StreamThread streamThread = _dataThread as StreamThread;
        if (streamThread != null)
        {
          if (string.Compare(args.PropertyName, "AllowKeyframes") == 0)
          {
            streamThread.AllowKeyframes = playback.AllowKeyframes;
          }
          else if (string.Compare(args.PropertyName, "KeyframeEveryMiB") == 0)
          {
            streamThread.KeyframeMiB = playback.KeyframeEveryMiB;
          }
          else if (string.Compare(args.PropertyName, "KeyframeEveryFrames") == 0)
          {
            streamThread.KeyframeFrames = (uint)Math.Min(0, playback.KeyframeEveryFrames);
          }
          else if (string.Compare(args.PropertyName, "KeyframeSkipForwardFrames") == 0)
          {
            streamThread.KeyframeSkipForwardFrames = (uint)Math.Max(0, playback.KeyframeSkipForwardFrames);
          }
          else if (string.Compare(args.PropertyName, "KeyframeFrameSeparation") == 0)
          {
            streamThread.KeyframeMinFrames = (uint)Math.Max(0, playback.KeyframeFrameSeparation);
          }
          else if (string.Compare(args.PropertyName, "Looping") == 0)
          {
            streamThread.Loop = playback.Looping;
          }
        }
      }
    }

    /// <summary>
    /// Manage catchup state changes when skipping more than one fame ahead.
    /// </summary>
    /// <param name="inCatchUp">Currently in catch up mode? Supports looping in the update method.</param>
    /// <param name="forceExitCatchup">Force exit of catchup mode?</param>
    /// <returns>True if the data thread is catch up mode, false otherwise.</returns>
    /// <remarks>
    /// In catch up mode the data thread is trying to make a large step. To reduce main thread
    /// processing, all handers have <see cref="MessageHandler.ModeFlags.IgnoreTransient"/>
    /// set. It is cleared otherwise.
    /// </remarks>
    private bool UpdateCatchup(bool inCatchUp, bool forceExitCatchup = false)
    {
      bool needCatchUp = false;
      if (_dataThread != null && !_dataThread.IsLiveStream)
      {
        if (_dataThread != null && !forceExitCatchup)
        {
          // Don't catch up on frame zero.
          // This can ensure some initial state conditions, including restoring Keyframes.
          needCatchUp = _dataThread.CatchingUp || _currentFrame != 0 && _currentFrame + 1 < _dataThread.CurrentFrame;
        }

        if (inCatchUp != needCatchUp)
        {
          if (needCatchUp)
          {
            foreach (MessageHandler handler in Handlers.Handlers)
            {
              handler.Mode = handler.Mode | MessageHandler.ModeFlags.IgnoreTransient;
            }
          }
          else
          {
            foreach (MessageHandler handler in Handlers.Handlers)
            {
              handler.Mode = handler.Mode & ~MessageHandler.ModeFlags.IgnoreTransient;
            }
          }
        }
      }

      return needCatchUp;
    }

    /// <summary>
    /// Advance the frame.
    /// </summary>
    /// <param name="frameNumber">The frame number of the ending frame.</param>
    /// <param name="catchingUp">Are we in catch up mode (transient objects disabled).</param>
    /// <param name="maintainTransient">True to prevent flushing of transient objects this frame.</param>
    private void EndFrame(uint frameNumber, bool catchingUp, bool maintainTransient = false)
    {
      // TODO: respect the elapsed time. That is, delay processing until the
      // required time has elapsed.
      foreach (MessageHandler handler in _handlers.Handlers)
      {
        handler.BeginFrame(CurrentFrame, maintainTransient);
      }

      PacketBuffer packet = null;
      try
      {
        // We've staged packets for this frame into _pendingPackets. Process all these packets now.
        while (_pendingPackets.Count > 0)
        {
          packet = _pendingPackets.Dequeue();
          NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));
          if (packet.Header.RoutingID == (ushort)RoutingID.Control)
          {
            HandleControlMessage(packet, packetReader, catchingUp);
          }
          else
          {
            // Fetch a handler.
            MessageHandler handler = GetHandler(packet.Header.RoutingID);
            if (handler != null)
            {
              handler.ReadMessage(packet, packetReader);
            }
            else
            {
              Log.Error("Unsupported routing ID: {0} {1}", packet.Header.RoutingID, RoutingIDName(packet.Header.RoutingID));
            }
          }
          // Release the internal buffer (rented) now rather than waiting for the GC thread.
          packet.Dispose();
          packet = null;
        }
      }
      catch (Exception e)
      {
        throw e;
      }

      foreach (MessageHandler handler in _handlers.Handlers)
      {
        handler.EndFrame(CurrentFrame);
      }

      if (_dataThread != null)
      {
        // In recording mode, we dictate the frame here, not the data thread.
        // This keeps the shown frame reflecting the recorded frames even at very
        // high rates.
        if (_recordingWriter != null)
        {
          ++_totalFrames;
        }
        else
        {
          _totalFrames = _dataThread.TotalFrames;
        }

        if (Mode != RouterMode.Paused)
        {
          _previousFrame = _currentFrame;
        }
        _currentFrame = (_dataThread.IsLiveStream) ? _totalFrames : frameNumber;
      }
    }

    /// <summary>
    /// Serialise the current scene to <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The file to serialise to.</param>
    /// <param name="allowCompression">Allow compressed serialisation?</param>
    /// <returns>True on success, false on any exception or failure (logged).</returns>
    public bool SerialiseScene(string fileName, bool allowCompression)
    {
      bool success;

      try
      {
        BinaryWriter writer = SerialiseScene(new FileStream(fileName, FileMode.Create), allowCompression, out success);
        WriteFrameFlush(writer);
        writer.Flush();
        // Must be closed to ensure the compression stream finalises correctly.
        writer.Close();
      }
      catch (Exception e)
      {
        Debug.LogException(e);
        success = false;
      }
      return success;
    }

    /// <summary>
    /// Serialise the current work state to the given (file) stream.
    /// </summary>
    /// <param name="fileStream">The (file) stream to writer to.</param>
    /// <param name="allowCompression">Allow the use of a compressed file stream?</param>
    /// <param name="success">Set to true to indicate success, false otherwise.</param>
    /// <returns>A binary writer to the compressed stream for appending data.</returns>
    /// <remarks>
    /// The <paramref name="fileStream"/> will be wrapped in a GZipStream which is
    /// used to instantiate the <c>BinaryWriter</c>.
    /// </remarks>
    private BinaryWriter SerialiseScene(Stream fileStream, bool allowCompression, out bool success)
    {
      //string fileName = (fileStream as FileStream != null) ? (fileStream as FileStream).Name : "<unknown>";
      //Log.Diag("SerialiseScene({0}, [out])", fileName);
      // Write the recording header uncompressed to the file.
      // We'll rewind here later and update the frame count.
      // Write to a memory stream to prevent corruption of the file stream when we wrap it
      // in a GZipStream.
      MemoryStream headerStream = new MemoryStream(512);
      BinaryWriter writer = new Tes.IO.NetworkWriter(headerStream);
      WriteRecordingHeader(writer);
      writer.Flush();
      byte[] headerBytes = headerStream.ToArray();

      // Copy header to the file.
      fileStream.Write(headerBytes, 0, headerBytes.Length);
      // Dispose of the temporary objects.
      writer = null;
      headerBytes = null;
      headerStream = null;

      // Now wrap the file in a GZip stream to start compression if we are not already doing so.
      if (allowCompression && fileStream as GZipStream == null)
      {
        writer = new NetworkWriter(new GZipStream(fileStream, CompressionMode.Compress, SharpCompress.Compressors.Deflate.CompressionLevel.BestCompression));
      }
      else
      {
        writer = new NetworkWriter(fileStream);
      }

      Error err;
      SerialiseInfo totalInfo = new SerialiseInfo();
      SerialiseInfo info = new SerialiseInfo();
      success = true;
      foreach (MessageHandler handler in _handlers.Handlers)
      {
        err = handler.Serialise(writer, ref info);
        //Log.Diag("{0}: P: {1} T: {2}", handler.Name, info.PersistentCount, info.TransientCount);
        totalInfo.PersistentCount += info.PersistentCount;
        totalInfo.TransientCount += info.TransientCount;
        if (err.Failed)
        {
          Log.Error("Failed to serialise handler: {0}", handler.Name);
          Log.Error(err.ToString());
          success = false;
        }
      }
      //Log.Diag("Total: P: {0} T: {1}", totalInfo.PersistentCount, totalInfo.TransientCount);

      return writer;
    }

    /// <summary>
    /// Generate a keyframe for a frame.
    /// </summary>
    /// <param name="frameNumber">The frame to generate the keyframe for.</param>
    /// <remarks>
    /// Works with the <see cref="StreamThread"/> to create, register and populate the output stream.
    /// </remarks>
    private void GenerateKeyframe(uint frameNumber, bool includesTransients)
    {
      StreamThread streamThread = _dataThread as StreamThread;
      if (streamThread == null)
      {
        Log.Error("Received keyframe request while not using a StreamThread. Ignored.");
        return;
      }

      Stream keyframeStream = streamThread.RequestKeyframeStream(frameNumber);
      bool success = false;

      try
      {
        if (PlaybackSettings.Instance.AllowKeyframes && keyframeStream != null)
        {
          BinaryWriter writer = SerialiseScene(keyframeStream, PlaybackSettings.Instance.KeyframeCompression, out success);
          WriteFrameFlush(writer);
          writer.Flush();
          // Must be closed to ensure the compression stream finalises correctly.
          writer.Close();
          Debug.Log(string.Format("Keyframe @ {0}", frameNumber));
        }
      }
      finally
      {
        // Ensure we release the stream.
        if (streamThread != null && keyframeStream != null)
        {
          streamThread.ReleaseKeyframeStream(frameNumber, keyframeStream, includesTransients, success);
        }
      }
    }

    /// <summary>
    /// Write a <see cref="ControlMessage"/> using <see cref="ControlMessageID.ForceFrameFlush"/>
    /// to <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to serialise with.</param>
    private void WriteFrameFlush(BinaryWriter writer)
    {
      PacketBuffer packet = new PacketBuffer();
      ControlMessage message = new ControlMessage();

      message.ControlFlags = 0;
      message.Value32 = 0;
      message.Value64 = 0;

      packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.ForceFrameFlush);
      message.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);
    }

    /// <summary>
    /// Write the "header" message for a recording stream.
    /// </summary>
    /// <param name="writer">The writer used to write the headers.</param>
    /// <remarks>
    /// By convention, the "header" for a recorded stream contains the following messages:
    /// <list type="bullet">
    /// <item>A <see cref="ServerInfoMessage"/> to set up the scene and playback.</item>
    /// <item>A <see cref="ControlMessage"/> using <see cref="ControlMessageID.FrameCount"/>
    /// to identify the frame count.</item>
    /// </list>
    ///
    /// Both messages should be in uncompressed space, thus the writer should not be using a
    /// zip stream. The frame count written here is zero and is finalised by calling
    /// <see cref="FinaliseFrameCount(BinaryWriter, Stream, uint)"/> once the final frame count is
    /// known.
    /// </remarks>
    private void WriteRecordingHeader(BinaryWriter writer)
    {
      // First write a server info message to define the coordinate frame and other server details.
      PacketBuffer packet = new PacketBuffer();
      ControlMessage frameCountMsg = new ControlMessage();

      packet.Reset();
      packet.Reset((ushort)RoutingID.ServerInfo, 0);
      _serverInfo.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);

      // Next write a placeholder control packet to define the total number of frames.
      frameCountMsg.ControlFlags = 0;
      frameCountMsg.Value32 = 1;  // Placeholder. Frame count is currently unknown.
      frameCountMsg.Value64 = 0;
      packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.FrameCount);
      frameCountMsg.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);
    }

    /// <summary>
    /// Finalises the frame count in the stream underpinning <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The binary writer and stream to export to.</param>
    /// <param name="outStream">The stream underpinning <paramref name="writer" />. May be otherwise inaccessible.
    /// <param name="frameCount">The total number of frames.</param>
    /// <remarks>
    /// By convention, a recording stream includes a <see cref="ControlMessage"/> with ID
    /// <see cref="ControlMessageID.FrameCount"/> to identify the total number of frames in
    /// the stream. This is expected towards the start of the stream before compression begins.
    ///
    /// This method rewinds the stream supporting <paramref name="writer"/>, searches for the
    /// frame count <see cref="ControlMessage"/> and modifies its <see cref="ControlMessage.Value32"/>
    /// to match <paramref name="frameCount"/>.
    ///
    /// The method supports the underlying <paramref name="writer"/> stream to be either <c>GZipStream</c>
    /// or an uncompressed stream. However, in order to succeed, the true stream (underpinning the
    /// zip stream) must support seeking and the message must appear in the uncompressed section.
    /// The method fails if these conditions are not met.
    /// </remarks>
    private void FinaliseFrameCount(BinaryWriter writer, Stream outStream, uint frameCount)
    {
      // Rewind the stream to the beginning and find the first RoutingID.Control message
      // with a ControlMessageID.FrameCount ID. This should be the second message in the stream.
      // We'll limit searching to the first 5 messages.
      long frameCountMessageStart = 0;
      writer.Flush();

      // Extract the stream from the writer. The first stream may be a GZip stream, in which case we must
      // extract the stream that is writing to instead as we can't rewind compression streams
      // and we wrote the header raw.
      writer.BaseStream.Flush();

      // Check we are allowed to seek the stream.
      if (!outStream.CanSeek)
      {
        // Stream does not support seeking. The frame count will not be fixed.
        return;
      }

      // Record the initial stream position to restore later.
      long restorePos = outStream.Position;
      outStream.Seek(0, SeekOrigin.Begin);

      byte[] headerBuffer = new byte[PacketHeader.Size];
      PacketHeader header = new PacketHeader();
      byte[] markerValidationBytes = BitConverter.GetBytes(Tes.IO.Endian.ToNetwork(PacketHeader.PacketMarker));
      byte[] markerBytes = new byte[markerValidationBytes.Length];
      bool found = false;
      bool markerValid = false;
      int attemptsRemaining = 5;
      int byteReadLimit = 0;

      markerBytes[0] = 0;
      while (!found && attemptsRemaining > 0 && outStream.CanRead)
      {
        --attemptsRemaining;
        markerValid = false;

        // Limit the number of bytes we try read in each attempt.
        byteReadLimit = 1024;
        while (byteReadLimit > 0)
        {
          --byteReadLimit;
          outStream.Read(markerBytes, 0, 1);
          if (markerBytes[0] == markerValidationBytes[0])
          {
            markerValid = true;
            int i = 1;
            for (i = 1; markerValid && outStream.CanRead && i < markerValidationBytes.Length; ++i)
            {
              outStream.Read(markerBytes, i, 1);
              markerValid = markerValid && markerBytes[i] == markerValidationBytes[i];
            }

            if (markerValid)
            {
              break;
            }
            else
            {
              // We've failed to fully validate the maker. However, we did read and validate
              // one byte in the marker, then continued reading until the failure. It's possible
              // that the last byte read, the failed byte, may be the start of the actual marker.
              // We check this below, and if so, we rewind the stream one byte in order to
              // start validation from there on the next iteration. We can ignore the byte if
              // it is does not match the first validation byte. We are unlikely to ever make this
              // match though.
              --i;  // Go back to the last read byte.
              if (markerBytes[i] == markerValidationBytes[0])
              {
                // Potentially the start of a new marker. Rewind the stream to attempt to validate it.
                outStream.Seek(-1, SeekOrigin.Current);
              }
            }
          }
        }

        if (markerValid && outStream.CanRead)
        {
          // Potential packet target. Record the stream position at the start of the marker.
          frameCountMessageStart = outStream.Position - markerBytes.Length;
          outStream.Seek(frameCountMessageStart, SeekOrigin.Begin);

          // Test the packet.
          int bytesRead = outStream.Read(headerBuffer, 0, headerBuffer.Length);
          if (bytesRead == headerBuffer.Length)
          {
            // Create a packet.
            if (header.Read(new NetworkReader(new MemoryStream(headerBuffer, false))))
            {
              // Header is OK. Looking for RoutingID.Control and ControlMessageID.FrameCount
              if (header.RoutingID == (ushort)RoutingID.Control && header.MessageID == (ushort)ControlMessageID.FrameCount)
              {
                // We've found the message location.
                found = true;
                break;
              }
              else
              {
                // At this point, we've failed to find the right kind of header. We could use the payload size to
                // skip ahead in the stream which should align exactly to the next message.
                // Not done for initial testing.
              }
            }
          }
        }
      }

      if (found)
      {
        // Found the correct location. Seek the stream to here and write a new FrameCount control message.
        outStream.Seek(frameCountMessageStart, SeekOrigin.Begin);
        PacketBuffer packet = new PacketBuffer();
        ControlMessage frameCountMsg = new ControlMessage();

        packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.FrameCount);
        frameCountMsg.ControlFlags = 0;
        frameCountMsg.Value32 = frameCount;  // Place holder. Frame count is currently unknown.
        frameCountMsg.Value64 = 0;
        frameCountMsg.Write(packet);
        packet.FinalisePacket();
        BinaryWriter patchWriter = new Tes.IO.NetworkWriter(outStream);
        packet.ExportTo(patchWriter);
        patchWriter.Flush();
      }

      if (outStream.Position != restorePos)
      {
        outStream.Seek(restorePos, SeekOrigin.Begin);
      }
    }


    /// <summary>
    /// Exports a camera position and settings message to <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer (and stream) to serialise the message to.</param>
    /// <param name="camera">The camera object from which settings are read.</param>
    /// <param name="cameraID">The camera ID to assign to the exported camera message</param>
    private void WriteCameraPosition(BinaryWriter writer, Camera camera, byte cameraID)
    {
      PacketBuffer packet = new PacketBuffer();
      CameraMessage cameraMessage = new CameraMessage();

      cameraMessage.CameraID = cameraID;
      // Convert from Unity frame into the server frame.
      Vector3 v = Vector3.zero;

      v = FrameTransform.UnityToRemote(camera.transform.position, _serverInfo.CoordinateFrame);
      cameraMessage.X = v.x;
      cameraMessage.Y = v.y;
      cameraMessage.Z = v.z;
      v = FrameTransform.UnityToRemote(camera.transform.up, _serverInfo.CoordinateFrame);
      cameraMessage.UpX = v.x;
      cameraMessage.UpY = v.y;
      cameraMessage.UpZ = v.z;
      v = FrameTransform.UnityToRemote(camera.transform.forward, _serverInfo.CoordinateFrame);
      cameraMessage.DirX = v.x;
      cameraMessage.DirY = v.y;
      cameraMessage.DirZ = v.z;
      cameraMessage.Near = camera.nearClipPlane;
      cameraMessage.Far = camera.farClipPlane;
      cameraMessage.FOV = camera.fieldOfView;
      packet.Reset((ushort)RoutingID.Camera, 0);
      cameraMessage.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);
    }

    private MessageHandlerLibrary _handlers = new MessageHandlerLibrary();
    private Scene _scene = null;
    private MaterialLibrary _materials = new MaterialLibrary();
    private DataThread _dataThread;
    /// <summary>
    /// The queue of packets awaiting an end of frame command in order to push
    /// them to the handlers.
    /// </summary>
    private Queue<PacketBuffer> _pendingPackets = new Queue<PacketBuffer>();
    /// <summary>
    /// Duplicates the current frame number from the data thread to update only when relevant.
    /// </summary>
    private uint _currentFrame = 0;
    /// <summary>
    /// The last frame viewed.
    /// </summary>
    private uint _previousFrame = 0;
    /// <summary>
    /// Duplicates the total frame number from the data thread to update only when relevant.
    /// </summary>
    private uint _totalFrames = 0;
    /// <summary>
    /// This writer is used to record the current network connection.
    /// We are not recording if it is null and we are if it is non-null.
    /// </summary>
    private BinaryWriter _recordingWriter = null;
    /// <summary>
    /// The file stream underpinning the <c>_recordingWriter</c>.
    /// </summary>
    private Stream _recordingFileStream = null;
    /// <summary>
    /// Initialised to the latest incoming server info message received (normally on connection).
    /// </summary>
    private ServerInfoMessage _serverInfo = ServerInfoMessage.Default;
    /// <summary>
    /// Tracks the last mode on <see cref="Update()"/> to as to notify mode changes via <see cref="OnModeChange"/>.
    /// </summary>
    private RouterMode _lastMode = RouterMode.Idle;
    /// <summary>
    /// File path to start recording to as soon as a connection is made.
    /// </summary>
    private string _recordOnConnectPath = null;
    /// <summary>
    /// Playback speed scaling.
    /// </summary>
    private float _playbackSpeed = 1.0f;
  }
}
