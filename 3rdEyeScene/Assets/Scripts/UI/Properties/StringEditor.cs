using UnityEngine.UI;
using System;

namespace UI.Properties
{
  class StringEditor : ValueEditor
  {
    public InputField Input = null;

    protected override void Start()
    {
      // Make bindings. Remove then add in case they were bound in the editor.		protected virtual void OnValueChanged()
      if (Input == null)
      {
        Input = GetComponentInChildren<InputField>();
      }

      if (Input != null)
      {
        Input.onValueChanged.RemoveListener(this.OnValueSet);
        Input.onValueChanged.AddListener(this.OnValueSet);
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
    protected void OnValueSet(string val)
    {
      Property.SetValue(Target, Convert.ChangeType(val, Property.PropertyType), null);
    }

    protected void UpdateDisplay()
    {
      string val = (string)Convert.ChangeType(Property.GetValue(Target, null), typeof(string));
      if (Input != null)
      {
        Input.text = val;
      }
    }
  }
}
