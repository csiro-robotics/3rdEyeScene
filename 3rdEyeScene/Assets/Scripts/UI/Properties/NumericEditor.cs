using UnityEngine;
using UnityEngine.UI;
using System;

namespace UI.Properties
{
  class NumericEditor : ValueEditor
  {
    public Slider Slider = null;
    public InputField Input = null;

    public bool PropertyIsInteger
    {
      get
      {
        if (Property != null)
        {
          return IsIntegerType(Property.PropertyType);
        }

        return false;
      }
    }

    public static bool IsIntegerType(Type type)
    {
      Type[] IntegerTypes = new Type[]
      {
        typeof(System.SByte),
        typeof(System.Byte),
        typeof(System.Int16),
        typeof(System.UInt16),
        typeof(System.Int32),
        typeof(System.UInt32),
        typeof(System.Int64),
        typeof(System.UInt64)
      };

      Type propertyType = type;
      for (int i = 0; i < IntegerTypes.Length; ++i)
      {
        if (propertyType == IntegerTypes[i])
        {
          return true;
        }
      }
      return false;
    }

    public static bool IsFloatingPointType(Type type)
    {
      return type == typeof(float) || type == typeof(double);
    }

    public static bool IsNumericType(Type type)
    {
      return IsFloatingPointType(type) || IsIntegerType(type);
    }

    protected override void Start()
    {
      // Make bindings. Remove then add in case they were bound in the editor.		protected virtual void OnValueChanged()
      if (Input == null)
      {
        Input = GetComponentInChildren<InputField>();
      }
      if (Slider == null)
      {
        Slider = GetComponentInChildren<Slider>();
      }

      if (Slider != null)
      {
        // See FixNavigation() comment.
        Slider.FixNavigation();
      }

      if (Input != null)
      {
        Input.onEndEdit.RemoveListener(this.OnValueSet);
        Input.onEndEdit.AddListener(this.OnValueSet);
      }
      if (Slider != null)
      {
        Slider.onValueChanged.RemoveListener(this.OnValueChanged);
        Slider.onValueChanged.AddListener(this.OnValueChanged);
      }
    }

    protected override void OnReleaseTarget()
    {
      _range = null;
    }

    protected override void OnSetTarget()
    {
      if (Property != null)
      {
        // Check for range property.
        _range = PropertyEditors.FindAttribute<SetRangeAttribute>(Property);
      }
      SetupRange();
      UpdateDisplay();
    }

    protected override void OnValueChanged()
    {
      UpdateDisplay();
    }

    protected void SetupRange()
    {
      bool intType = PropertyIsInteger;
      if (_range != null)
      {
        if (Slider != null)
        {
          Slider.interactable = true;
          Slider.gameObject.SetActive(true);
          Slider.minValue = _range.Min;
          Slider.maxValue = _range.Max;
          Slider.wholeNumbers = intType;
        }
      }
      else if (Slider != null)
      {
        // Hide the slider. Make it non interactible, deactivate children, then active the root object.
        Slider.interactable = false;
        Slider.gameObject.SetActive(false);
      }

      if (Input != null)
      {
        if (intType)
        {
          Input.contentType = InputField.ContentType.IntegerNumber;
        }
        else
        {
          Input.contentType = InputField.ContentType.DecimalNumber;
        }
      }
    }

    /// <summary>
    /// Input callback.
    /// </summary>
    /// <param name="value"></param>
    public void OnValueSet(string value)
    {
      if (!_suppressEvents)
      {
        _suppressEvents = true;
        if (PropertyIsInteger)
        {
          // Parse as 64-bit int.
          long val;
          if (long.TryParse(value, out val))
          {
            SetValue(val);
            _suppressEvents = false;
            return;
          }
        }
        else
        {
          // Parse and set as double.
          double val;
          if (double.TryParse(value, out val))
          {
            SetValue(val);
          }
        }

        // Restore the current value.
        UpdateDisplay();
        _suppressEvents = false;
      }
    }

    /// <summary>
    /// Slider callback.
    /// </summary>
    /// <param name="val"></param>
    public void OnValueChanged(float val)
    {
      if (!_suppressEvents)
      {
        _suppressEvents = true;
        SetValue(val);
        _suppressEvents = false;
      }
    }

    protected void SetValue(long val)
    {
      if (Property != null && Target != null)
      {
        Property.SetValue(Target, Convert.ChangeType(val, Property.PropertyType), null);
      }
    }

    protected void SetValue(double val)
    {
      if (Property != null && Target != null)
      {
        Property.SetValue(Target, Convert.ChangeType(val, Property.PropertyType), null);
      }
    }

    protected void SetValue(float val)
    {
      if (Property != null && Target != null)
      {
        Property.SetValue(Target, Convert.ChangeType(val, Property.PropertyType), null);
      }
    }

    protected void UpdateDisplay()
    {
      double val = (double)Convert.ChangeType(Property.GetValue(Target, null), typeof(double));
      if (Input != null)
      {
        Input.text = val.ToString();
      }
      if (Slider != null && Slider.interactable)
      {
        Slider.value = (float)val;
      }
    }

    private SetRangeAttribute _range = null;
    private bool _suppressEvents = false;
  }
}
