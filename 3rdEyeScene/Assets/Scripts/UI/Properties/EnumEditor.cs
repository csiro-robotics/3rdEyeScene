using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UI.Properties
{
  class EnumEditor : ValueEditor
  {
    public Dropdown Dropdown = null;

    protected override void Start()
    {
      // Make bindings. Remove then add in case they were bound in the editor.		protected virtual void OnValueChanged()
      if (Dropdown == null)
      {
        Dropdown = GetComponentInChildren<Dropdown>();
      }

      if (Dropdown != null)
      {
        Dropdown.onValueChanged.RemoveListener(this.OnDowndownChanged);
        Dropdown.onValueChanged.AddListener(this.OnDowndownChanged);
      }
    }

    protected override void OnSetTarget()
    {
      // Enumerate the entries.
      List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
      Dropdown.ClearOptions();

      if (Property.PropertyType.IsEnum)
      {
        string[] strings = Enum.GetNames(Property.PropertyType);
        for (int i = 0; i < strings.Length; ++i)
        {
          Dropdown.OptionData opt = new Dropdown.OptionData();
          opt.text = strings[i];
        }
      }

      Dropdown.AddOptions(options);
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
    protected void OnDowndownChanged(int index)
    {
      string valueString = Dropdown.options[index].text;
      try
      {
        object val = Enum.Parse(Property.PropertyType, valueString);
        Property.SetValue(Target, val, null);
      }
      catch (ArgumentException e)
      {
        Debug.LogWarning("Enum parse failure.");
        Debug.LogException(e);
      }
    }

    protected void UpdateDisplay()
    {
      string valueStr = Property.GetValue(Target, null).ToString();
      for (int i = 0; i < Dropdown.options.Count; ++i)
      {
        if (string.Compare(valueStr, Dropdown.options[i].text) == 0)
        {
          Dropdown.value = i;
          break;
        }
      }
    }
  }
}
