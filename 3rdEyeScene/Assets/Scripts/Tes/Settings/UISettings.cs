using System.ComponentModel;
// using UnityEngine;

public class UISettings : Settings
{
  private static UISettings _instance = new UISettings();
  public static UISettings Instance { get { return _instance; } }

  public UISettings()
  {
    Name = "UI";
  }

  [Browsable(true), Tooltip("Use native file browser dialogs where possible (Windows/MacOS)")]
  public bool NativeDialogs
  {
    get { return PlayerPrefsX.GetBool("ui.nativeDialogs", true); }
    set { PlayerPrefsX.SetBool("ui.nativeDialogs", value); Notify("NativeDialogs"); }
  }
}
