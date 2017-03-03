using UnityEngine.UI;
using System;

namespace UI.Properties
{
  class BoolEditor : ValueEditor
  {
    public Toggle Toggle = null;

    protected override void Start()
    {
      // Make bindings. Remove then add in case they were bound in the editor.    protected virtual void OnValueChanged()
      if (Toggle == null)
      {
        Toggle = GetComponentInChildren<Toggle>();
      }

      if (Toggle != null)
      {
        Toggle.onValueChanged.RemoveListener(this.OnToggle);
        Toggle.onValueChanged.AddListener(this.OnToggle);
      }
    }

    protected override void OnSetTarget()
    {
      UpdateDisplay();
    }

    protected override void OnValueChanged()
    {
      UpdateDisplay();
    }

    /// <summary>
    /// Toggle callback.
    /// </summary>
    /// <param name="val"></param>
    protected void OnToggle(bool val)
    {
      SetValue<bool>((bool)Convert.ChangeType(val, Property.PropertyType));
    }

    protected void UpdateDisplay()
    {
      bool val = GetValue<bool>();
      if (Toggle != null)
      {
        Toggle.isOn = val;
      }
    }
  }
}
