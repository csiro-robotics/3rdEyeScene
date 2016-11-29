using UnityEngine;
using UnityEngine.UI;
using Tes.Main;

namespace UI
{
  public class PlaybarButton : MonoBehaviour
  {
    /// <summary>
    /// List of modes the button is active for.
    /// </summary>
    public RouterMode[] EnabledModes;
    /// <summary>
    /// Optional list of sprites used when matching each of the above modes.
    /// Null entries result in no change from the previous state.
    /// </summary>
    public Sprite[] ModeSprites;
    public TesComponent Controller;

    // Use this for initialization
    void Start()
    {
      UpdateState();
    }

    /// <summary>
    /// Update the current state of the button.
    /// </summary>
    public void UpdateState()
    {
      if (Controller == null)
      {
        return;
      }

      Button meButton = GetComponent<Button>();

      if (meButton == null)
      {
        return;
      }

      RouterMode currentMode = Controller.Mode;
      bool active = false;
      for (int i = 0; i < EnabledModes.Length; ++i)
      {
        if (EnabledModes[i] == currentMode)
        {
          active = true;
          if (i < ModeSprites.Length && ModeSprites[i] != null)
          {
            meButton.image.sprite = ModeSprites[i];
          }
        }
      }

      meButton.interactable = active;
    }
  }
}
