using System;
using System.Reflection;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// A range attribute for use with settings. Can't use the standard RangeAttribute as it
/// does not support properties.
/// </summary>
public class SetRangeAttribute : Attribute
{
  public float Min { get; set; }
  public float Max { get; set; }
  public SetRangeAttribute(float min, float max)
  {
    Min = min;
    Max = max;
  }
}

/// <summary>
/// Tooltip attribute for settings properties.
/// </summary>
public class TooltipAttribute : Attribute
{
  public string Tooltip { get; set; }

  public TooltipAttribute(string tip)
  {
    Tooltip = tip;
  }
}

/// <summary>
/// Base class for settings which can be displayed as part of the settings panel or another
/// properties editor.
/// </summary>
/// <remarks>
/// Derivations can expose any property for editing and serialisation to Unity's "Player Preferences"
/// system. To do so, a properties must be correctly setup.
/// <list type="number">
/// <item>Use only types convertable from string.</item>
/// <item>Add <c>System.ComponentModel.BrowsableAttribute</c> to editable settings.</item>
/// <item>In each property setter, invoke <see cref="Notify(string)"/> after setting the new value.
/// </list>
///
/// It is recommended that the properties are backed by Unity's PlayerPrefs or PlayerPrefsX.
///
/// Strictly speaking a class need not derive this one to expose browsable properties. The
/// attributes and property notification are the critical components.
/// </remarks>
public class Settings : INotifyPropertyChanged
{
  public event PropertyChangedEventHandler PropertyChanged;

  public string Name { get; set; }

  protected void Notify(PropertyInfo property)
  {
    Notify(property.Name);
  }

  protected void Notify(string propertyName)
  {
    if (PropertyChanged != null)
    {
      PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
