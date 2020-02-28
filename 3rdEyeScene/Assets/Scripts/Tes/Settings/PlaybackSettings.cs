using System.ComponentModel;
using UnityEngine;

public class PlaybackSettings : Settings
{
  private static PlaybackSettings _instance = new PlaybackSettings();
  public static PlaybackSettings Instance { get { return _instance; } }

  public PlaybackSettings()
  {
    Name = "Playback";
  }

  [Browsable(true), Tooltip("Enable scene keyframes to cache frames during playback and stepping?")]
  public bool AllowKeyframes
  {
    get { return PlayerPrefsX.GetBool("playback.keyframes", true); }
    set { PlayerPrefsX.SetBool("playback.keyframes", value); Notify("AllowKeyframes"); }
  }

  [Browsable(true), SetRange(64, 1024 * 1024),
    Tooltip("Create a keyframe after reading this many kilobytes from the playback stream.")]
  public int KeyframeEveryMiB
  {
    get { return PlayerPrefs.GetInt("playback.keyframeMiB", 20); }
    set { PlayerPrefs.SetInt("playback.keyframeMiB", value); Notify("KeyframeEveryMiB"); }
  }

  [Browsable(true), SetRange(1, 10000),
    Tooltip("Create a keyframe after this number of frames elapses.")]
  public int KeyframeEveryFrames
  {
    get { return PlayerPrefs.GetInt("playback.keyframeFrames", 5000); }
    set { PlayerPrefs.SetInt("playback.keyframeFrames", value); Notify("KeyframeEveryFrames"); }
  }

  [Browsable(true), SetRange(1, 10000),
    Tooltip("Try restore keyframe when skipping forwards at least this number of frames.")]
  public int KeyframeSkipForwardFrames
  {
    get { return PlayerPrefs.GetInt("playback.keyframeSkipForwardFrames", 20); }
    set { PlayerPrefs.SetInt("playback.keyframeSkipForwardFrames", value); Notify("KeyframeSkipForwardFrames"); }
  }

  [Browsable(true), SetRange(1, 1000),
    Tooltip("Minimum number of frames to have elapsed between keyframes.")]
  public int KeyframeFrameSeparation
  {
    get { return PlayerPrefs.GetInt("playback.keyframeFrameSeparation", 5); }
    set { PlayerPrefs.SetInt("playback.keyframeFrameSeparation", value); Notify("KeyframeFrameSeparation"); }
  }

  [Browsable(true),
    Tooltip("Compress snap shots?")]
  public bool KeyframeCompression
  {
    get { return PlayerPrefsX.GetBool("playback.keyframeCompression", true); }
    set { PlayerPrefsX.SetBool("playback.keyframeCompression", value); Notify("KeyframeCompression"); }
  }

  [Browsable(true),
    Tooltip("Automatically restart playback at the end of a file stream?")]
  public bool Looping
  {
    get { return PlayerPrefsX.GetBool("playback.looping", false); }
    set { PlayerPrefsX.SetBool("playback.looping", value); Notify("Looping"); }
  }

  [Browsable(true),
    Tooltip("Pause if an error occurs during playback? Only affects file playback.")]
  public bool PauseOnError
  {
    get { return PlayerPrefsX.GetBool("playback.pauseOnError", true); }
    set { PlayerPrefsX.SetBool("playback.pauseOnError", value); Notify("PauseOnError"); }
  }
}
