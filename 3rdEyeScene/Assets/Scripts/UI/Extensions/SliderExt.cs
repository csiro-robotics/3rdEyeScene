using UnityEngine.UI;

namespace UI
{
  /// <summary>
  /// Slider extension methods.
  /// </summary>
  public static class SliderExt
  {
    /// <summary>
    /// Address an issue with sliders and mouse navigation.
    /// </summary>
    /// <param name="slider">The slider to fixup.</param>
    /// <remarks>
    /// On mouse based platforms, clicking a slider focuses the slider and it starts following
    /// the mouse movement and keyboard input. This is highly undesirable in many circumstances,
    /// such as the timeline scrubber.
    /// 
    /// As a workaround, this method disables slider navigation for standalone platforms
    /// (Linux, MacOS, Windows). It has no effect on other platforms.
    /// </remarks>
    public static void FixNavigation(this Slider slider)
    {
#if UNITY_STANDALONE
      // FIXME: find a better solution for this issue.
      if (slider != null)
      {
        Navigation nav = slider.navigation;
        nav.mode = Navigation.Mode.None;
        slider.navigation = nav;
      }
#endif // UNITY_STANDALONE
    }
  }
}
