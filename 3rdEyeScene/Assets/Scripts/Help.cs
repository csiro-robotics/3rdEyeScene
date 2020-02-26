using UnityEngine;
using System;

public class Help : MonoBehaviour
{
  private struct HelpEntry
  {
    public string Input;
    public string Description;

    public HelpEntry(string input, string description)
    {
      Input = input;
      Description = description;
    }
  }

  void Start()
  {
    if (_thirdEyeScene != null)
    {
      _inputLayer = _thirdEyeScene.InputStack.GetLayer(_inputLayerName);
    }

    if (_inputLayer == null)
    {
      Debug.LogError("Unable to resolve input layer for help.");
    }
  }

  void Update()
  {
    if (_thirdEyeScene == null || _inputLayer == null)
    {
      return;
    }

    bool toggleHelp = (_showHelp) ? _inputLayer.GetButtonDown("Help") : Input.GetButtonDown("Help");
    if (_showHelp)
    {
      toggleHelp = toggleHelp || Input.GetKeyDown(KeyCode.Escape);
    }
    if (toggleHelp)
    {
      _showHelp = !_showHelp;
      if (_showHelp)
      {
        _thirdEyeScene.InputStack.SetLayerEnabled(_inputLayerName, true);
        _scrollPos = Vector2.zero;
      }
      else
      {
        _thirdEyeScene.InputStack.SetLayerEnabled(_inputLayerName, false);
      }
    }
  }

  void OnGUI()
  {
    if (_showHelp)
    {
      const int maxWidth = 1200;
      const int maxHeight = 800;
      Rect helpRect = new Rect(0, 0, Math.Min(maxWidth, Screen.width), Math.Min(maxHeight, Screen.height));
      // Position the rect.
      helpRect.x = (Screen.width - helpRect.width) / 2;
      helpRect.y = (Screen.height - helpRect.height) / 2;
      GUILayout.BeginArea(helpRect);
      {
        GUILayout.BeginVertical("box");
        {
          GUILayout.Label("Help");
          _scrollPos = GUILayout.BeginScrollView(_scrollPos, "box");
          {
            GUILayout.BeginVertical("box");
            {
              foreach (HelpEntry help in _helpEntries)
              {
                GUILayout.BeginHorizontal();
                {
                  GUILayout.Label(help.Input, GUILayout.ExpandWidth(false), GUILayout.MinWidth(150));
                  GUILayout.Label(help.Description);
                  GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
              }
            }
            GUILayout.EndVertical();
          }
          GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
      }
      GUILayout.EndArea();
    }
  }

  private bool _showHelp = false;
  private Vector2 _scrollPos = Vector2.zero;
  private HelpEntry[] _helpEntries = new HelpEntry[]
  {
    new HelpEntry("F1", "Toggle this help dialog"),

    new HelpEntry("", ""),
    new HelpEntry("", "Movement and camera"),
    new HelpEntry("shift", "Hold for speed mode (faster movement). Does not affect mouse"),
    new HelpEntry("ctrl", "Hold for slow mode (slower movement). Affects mouse"),
    new HelpEntry("left mouse", "Hold for mouse camera control"),
    new HelpEntry("Y", "Toggle inverted mouse Y axis"),
    new HelpEntry("Mouse X", "Camera yaw"),
    new HelpEntry("Mouse Y", "Camera pitch"),
    new HelpEntry("Mouse Wheel", "Adjust movement speed"),
    new HelpEntry("Mouse Wheel + control", "Adjust mouse speed"),
    new HelpEntry("F/C", "Camera elevation - world aligned"),
    new HelpEntry("G/V", "Camera elevation - camera aligned"),

    new HelpEntry("", ""),
    new HelpEntry("", "Playback controls"),
    new HelpEntry("ctrl R", "Record/reset. Start recording or reset (stop) playback"),
    new HelpEntry("ctrl P", "Play/pause"),
    new HelpEntry("ctrl shift ,", "Skip to the start of playback"),
    new HelpEntry("ctrl shift .", "Skip to the end of playback"),
    new HelpEntry(",", "Step back"),
    new HelpEntry(".", "Step forward"),
    new HelpEntry("backspace", "Jump to the last view frame"),
    new HelpEntry("ctrl G", "Goto frame - focus the frame number input box"),

    new HelpEntry("", ""),
    new HelpEntry("", "Panel controls"),
    new HelpEntry("ctrl H", "Open the connection panel"),
    new HelpEntry("ctrl T", "Open the categories panel"),
    new HelpEntry("ctrl ,", "Open the settings panel"),

    new HelpEntry("", ""),
    new HelpEntry("", "Resolution"),
    new HelpEntry("ctrl =", "Increase window resolution"),
    new HelpEntry("ctrl -", "Decrease window resolution"),
  };

  [SerializeField]
  private TesComponent _thirdEyeScene = null;
  [SerializeField]
  private string _inputLayerName = "Help";
  private InputSystem.InputLayer _inputLayer = null;
}
