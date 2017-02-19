using System;
using UnityEngine;
using UnityEngine.Events;

namespace InputSystem
{
  [Serializable]
  public class Command
  {
    [SerializeField]
    private string _name;
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    [SerializeField]
    private string _toolTip;
    public string ToolTip
    {
      get { return _toolTip; }
      set { _toolTip = value; }
    }

    [SerializeField]
    private KeyCombo _keyCombo;
    public KeyCombo KeyCombo
    {
      get { return _keyCombo; }
      set { _keyCombo = value; }
    }

    [SerializeField]
    UnityEvent _onTriggered;
    public UnityEvent OnTriggered
    {
      get { return _onTriggered; }
      set { _onTriggered = value; }
    }
  }
}
