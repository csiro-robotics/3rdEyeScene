using UnityEngine;
using System;
using System.Reflection;

namespace UI.Properties
{
  /// <summary>
  /// A helper class for dealing with object properties.
  /// </summary>
  public class PropertyEditors : MonoBehaviour
  {
    [Serializable]
    public struct EditorEntry
    {
      public string Name;
      public ValueEditor Editor;
    }

    public ValueEditor Default;
    public ValueEditor Boolean;
    public ValueEditor Enum;
    public ValueEditor Numeric;
    public ValueEditor String;
    public EditorEntry[] ValueEditors;

    public static PropertyEditors Instance { get; private set; }

    void Start()
    {
      if (Instance == null)
      {
        Instance = this;
      }
    }

    void OnDestroy()
    {
      if (Instance == this)
      {
        Instance = null;
      }
    }

    /// <summary>
    /// Search for an attribute on a property.
    /// </summary>
    /// <param name="prop">The property to query on.</param>
    /// <typeparam name="T">The attribute type to look for.</typeparam>
    /// <returns>The attribute if found.</returns>
    public static T FindAttribute<T>(PropertyInfo prop) where T : class
    {
      object[] attributes = prop.GetCustomAttributes(true);
      for (int i = 0; i < attributes.Length; ++i)
      {
        if (typeof(T).IsAssignableFrom(attributes[i].GetType()))
        {
          return (T)attributes[i];
        }
      }
      return null;
    }

    public ValueEditor CreateEditor(object target, PropertyInfo prop)
    {
      ValueEditor editor = null;
      if (prop.PropertyType.IsEnum)
      {
        editor = Enum;
      }
      else if (NumericEditor.IsNumericType(prop.PropertyType))
      {
        editor = Numeric;
      }
      else if (prop.PropertyType == typeof(bool))
      {
        editor = Boolean;
      }
      else if (prop.PropertyType == typeof(string))
      {
        editor = String;
      }

      if (editor == null)
      {
        string typeName = prop.PropertyType.ToString();
        for (int i = 0; i < ValueEditors.Length; ++i)
        {
          if (string.Compare(ValueEditors[i].Name, typeName) == 0)
          {
            editor = ValueEditors[i].Editor;
            break;
          }
        }
      }

      if (editor == null)
      {
        editor = Default;
        if (editor == null)
        {
          return null;
        }
      }

      editor = Instantiate(editor);
      editor.SetTarget(target, prop);

      return editor;
    }
  }
}
