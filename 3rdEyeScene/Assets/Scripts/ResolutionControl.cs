using UnityEngine;

/// <summary>
/// Manages changing to fixed resolutions using hot keys.
/// </summary>
class ResolutionControl : MonoBehaviour
{
  void Start()
  {
#if !UNITY_EDITOR
    ValidateResolution();
#endif // !UNITY_EDITOR
  }

  /// <summary>
  /// Update looking for resolution change requests.
  /// </summary>
  void Update()
  {
#if !UNITY_EDITOR
    if (Input.GetButtonDown("Resolution") &&
        (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    {
      float sizeChange = Input.GetAxis("Resolution");
      if (sizeChange != 0)
      {
        UpdateResolution(sizeChange > 0);
      }
    }
#endif // !UNITY_EDITOR
  }

  /// <summary>
  /// Validate the screen resolution, ensuring it fits on the current display.
  /// </summary>
  /// <remarks>
  /// The resolution is adjusted to the current display if required.
  /// </remarks>
  private void ValidateResolution()
  {
    // Note: When not in full screen mode, Screen.currentResolution gives the current device
    // resolution capability (i.e., desktop resolution), while Screen.width and Screen.height
    // yield the current window resolution. However, we use Screen.SetResolution() to change the
    // window resolution. This means that Screen.SetResolution() does not change
    // Screen.currentResolution unless we are in full screen mode.
    if (!Screen.fullScreen)
    {
      Resolution res = Screen.currentResolution;
      if (res.width < Screen.width || res.height < Screen.height)
      {
        Screen.SetResolution(res.width, res.height, false);
      }
    }
  }

  /// <summary>
  /// Change the resolution up/down to the next supported resolution.
  /// </summary>
  /// <param name="changeUp">True to change up, false to change down.</param>
  private void UpdateResolution(bool changeUp)
  {
    bool changeRes = false;
    Resolution res = new Resolution();
    res.width = Screen.width;
    res.height = Screen.height;
    res.refreshRate = Screen.currentResolution.refreshRate;
    if (changeUp)
    {
      foreach (Resolution deviceRes in Screen.resolutions)
      {
        if (deviceRes.width > res.width)
        {
          res = deviceRes;
          changeRes = true;
          break;
        }
      }
    }
    else
    {
      for (int i = Screen.resolutions.Length - 1; i >= 0; --i)
      {
        Resolution deviceRes = Screen.resolutions[i];
        if (deviceRes.width < res.width)
        {
          res = deviceRes;
          changeRes = true;
          break;
        }
      }
    }

    if (changeRes)
    {
      Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
  }
}
