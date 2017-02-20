using System;
using UnityEngine;
using UnityEngine.Events;

namespace InputSystem
{
  /// <summary>
  /// A command ties a <see cref="KeyCombo"/> sequence to an event, implementing
  /// UI hotkeys.
  /// </summary>
  [Serializable]
  public class Command
  {
    [SerializeField]
    private string _name;
    /// <summary>
    /// Access the command name.
    /// </summary>
    /// <remarks>
    /// The name may be displayed to the user as in a menu item.
    /// </remarks>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    [SerializeField]
    private string _toolTip;
    /// <summary>
    /// Access the command tool tip.
    /// </summary>
    /// <remarks>
    /// The tool tip is displayed when hovering over UI components used to trigger the command.
    /// </remarks>
    public string ToolTip
    {
      get { return _toolTip; }
      set { _toolTip = value; }
    }

    [SerializeField]
    private KeyCombo _keyCombo;
    /// <summary>
    /// The key combo used to trigger the command.
    /// </summary>
    public KeyCombo KeyCombo
    {
      get { return _keyCombo; }
      set { _keyCombo = value; }
    }

    [SerializeField]
    UnityEvent _onTriggered;
    /// <summary>
    /// The event invoked when the command is triggered.
    /// </summary>
    /// <returns></returns>
    public UnityEvent OnTriggered
    {
      get { return _onTriggered; }
      set { _onTriggered = value; }
    }
  }
}
