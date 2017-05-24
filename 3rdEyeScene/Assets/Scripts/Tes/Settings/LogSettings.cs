using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Logging and log display settings.
/// </summary>
public class LogSettings : Settings
{
  private static LogSettings _instance = new LogSettings();
  /// <summary>
  /// Singleton instance.
  /// </summary>
  public static LogSettings Instance { get { return _instance; } }

  /// <summary>
  /// Constructor.
  /// </summary>
  public LogSettings()
  {
    Name = "Log";
  }

  /// <summary>
  /// The size of the log window.
  /// </summary>
  /// <remarks>
  /// Non browsable as it is edited by modifying the window size itself.
  /// </remarks>
  [Browsable(false), Tooltip("Size of the log window.")]
  public int LogWindowSize
  {
    get { return PlayerPrefs.GetInt("log.windowSize", 300); }
    set { PlayerPrefs.SetInt("log.windowSize", value); Notify("LogWindowSize"); }
  }
}
