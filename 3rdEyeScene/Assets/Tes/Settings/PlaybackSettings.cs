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

  [Browsable(true), Tooltip("Enable scene snapshots to cache frames during playback and stepping?")]
  public bool AllowSnapshots
  {
    get { return PlayerPrefsX.GetBool("playback.snapshots", true); }
    set { PlayerPrefsX.SetBool("playback.snapshots", value); Notify("AllowSnapshots"); }
  }

  [Browsable(true), SetRange(64, 1024 * 1024),
    Tooltip("Create a snapshot after reading this many kilobytes from the playback stream.")]
  public int SnapshotEveryKb
  {
    get { return PlayerPrefs.GetInt("playback.snapshotKb", 512); }
    set { PlayerPrefs.SetInt("playback.snapshotKb", value); Notify("SnapshotEveryKb"); }
  }

  [Browsable(true), SetRange(1, 1000),
    Tooltip("Minimum number of frames to have elapsed between snapshots.")]
  public int SnapshotFrameSeparation
  {
    get { return PlayerPrefs.GetInt("playback.snapshotFrameSeparation", 5); }
    set { PlayerPrefs.SetInt("playback.snapshotFrameSeparation", value); Notify("SnapshotFrameSeparation"); }
  }

  [Browsable(true),
    Tooltip("Automatically restart playback at the end of a file stream?")]
  public bool Looping
  {
    get { return PlayerPrefsX.GetBool("playback.looping", false); }
    set { PlayerPrefsX.SetBool("playback.looping", value); Notify("Looping"); }
  }
}
