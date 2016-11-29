using UnityEngine;
using System.Reflection;
using System.ComponentModel;

namespace UI.Properties
{
  /// <summary>
  /// The base behaviour to attach to value editor UI components.
  /// </summary>
  /// <remarks>
  /// Links to a property member of some object.
  /// </remarks>
  public class ValueEditor : MonoBehaviour
  {
    public UnityEngine.UI.Text NameField = null;

    /// <summary>
    /// Get the value of the underlying property as the requested type.
    /// </summary>
    /// <returns>The value of the property.</returns>
    /// <remarks>
    /// The type must match the property type. Does not check <see cref="CanRead"/>.
    /// </remarks>
    public T GetValue<T>()
    {
      return (T)_property.GetValue(_target, null);
    }

    /// <summary>
    /// Set the value of the underlying property.
    /// </summary>
    /// <param name="value">The new value.</param>
    /// <remarks>
    /// The type must match the property type. Does not check <see cref="CanWrite"/>.
    /// </remarks>
    public void SetValue<T>(T value)
    {
      _property.SetValue(_target, value, null);
    }

    /// <summary>
    /// True if the property value can be read.
    /// </summary>
    public bool CanRead
    {
      get
      {
        return _target != null && _property != null && _property.CanRead;
      }
    }

    /// <summary>
    /// True if the property value can be written to.
    /// </summary>
    public bool CanWrite
    {
      get
      {
        return _target != null && _property != null && _property.CanWrite;
      }
    }

    /// <summary>
    /// Access the target object.
    /// </summary>
    public object Target { get { return _target; } }

    /// <summary>
    /// Access the target property.
    /// </summary>
    public PropertyInfo Property { get { return _property; } }

    protected virtual void Start()
    {
      if (NameField == null)
      {
        NameField = GetComponentInChildren<UnityEngine.UI.Text>();
      }
    }

    /// <summary>
    /// Initialise the property target.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="property">The property reflection object.</param>
    public void SetTarget(object target, PropertyInfo property)
    {
      INotifyPropertyChanged notify;
      if (_target != null)
      {
        notify = target as INotifyPropertyChanged;
        if (notify != null)
        {
          notify.PropertyChanged -= OnPropertyChanged;
        }
        OnReleaseTarget();
      }

      _target = target;
      _property = property;

      notify = target as INotifyPropertyChanged;
      if (notify != null)
      {
        notify.PropertyChanged += OnPropertyChanged;
      }

      if (_property != null && NameField != null)
      {
        NameField.text = _property.Name;
      }

      OnSetTarget();
    }

    protected void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      // Validate whether this is the property of interest.
      if (sender == Target && Property != null)
      {
        if (args.PropertyName == Property.Name)
        {
          OnValueChanged();
        }
      }
    }

    /// <summary>
    /// Invoked after a new target is set.
    /// </summary>
    protected virtual void OnSetTarget()
    {
      // Nothing.
    }

    /// <summary>
    /// Invoked when the curren target is about to be replaced.
    /// </summary>
    protected virtual void OnReleaseTarget()
    {
      // Nothing.
    }

    protected virtual void OnValueChanged()
    {
      // Nothig here.
    }

    private object _target = null;
    private PropertyInfo _property = null;
  }
}
