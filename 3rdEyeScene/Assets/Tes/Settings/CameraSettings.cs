using System.ComponentModel;
using UnityEngine;

public class CameraSettings : Settings
{
  private static CameraSettings _instance = new CameraSettings();
  public static CameraSettings Instance { get { return _instance; } }

  public CameraSettings()
  {
    Name = "Camera";
  }

  [Browsable(true), Tooltip("Invert mouse Y axis?")]
  public bool InvertY
  {
    get { return PlayerPrefsX.GetBool("camera.inverty", false); }
    set { PlayerPrefsX.SetBool("camera.inverty", value); Notify("InvertY"); }
  }

  [Browsable(true), Tooltip("Use remote clip plane and field of view settings?")]
  public bool AllowRemoteSettings
  {
    get { return PlayerPrefsX.GetBool("camera.allowRemote", false); }
    set { PlayerPrefsX.SetBool("camera.allowRemote", value); Notify("AllowRemoteSettings"); }
  }

  [Browsable(true), SetRange(0, 100), Tooltip("The default near clip plane when not using remote settings.")]
  public float NearClip
  {
    get { return PlayerPrefs.GetFloat("camera.nearClip", 0.3f); }
    set { PlayerPrefs.SetFloat("camera.nearClip", value); Notify("NearClip"); }
  }

  [Browsable(true), SetRange(0, 3000), Tooltip("The default far clip plane when not using remote settings.")]
  public float FarClip
  {
    get { return PlayerPrefs.GetFloat("camera.farClip", 2000.0f); }
    set { PlayerPrefs.SetFloat("camera.farClip", value); Notify("FarClip"); }
  }

  [Browsable(true), SetRange(0, 90), Tooltip("The default vertical field of view (degrees).")]
  public float FOV
  {
    get { return PlayerPrefs.GetFloat("camera.fov", 60.0f); }
    set { PlayerPrefs.SetFloat("camera.fov", value); Notify("FOV"); }
  }
}
