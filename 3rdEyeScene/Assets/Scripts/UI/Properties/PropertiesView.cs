using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using System.Reflection;

namespace UI.Properties
{
  /// <summary>
  /// An object property editor.
  /// </summary>
  /// <remarks>
  /// Uses reflection to find properties with the <c>Browsable</c> attribute.
  /// </remarks>
  [RequireComponent(typeof(ScrollRect))]
  public class PropertiesView : MonoBehaviour
  {
    public object Target
    {
      get { return _target; }
    }

    protected LayoutElement Spacer
    {
      get
      {
        if (_spacer == null)
        {
          _spacer = Util.CreateSpacer();
          _spacer.transform.SetParent(transform, false);
        }
        return _spacer;
      }
    }

    public void SetTarget(object target)
    {
      Clear();
      _target = target;
      if (target != null)
      {
        ScrollRect scroll = GetComponent<ScrollRect>();
        BrowsableAttribute browse;
        foreach (PropertyInfo prop in _target.GetType().GetProperties())
        {
          browse = PropertyEditors.FindAttribute<BrowsableAttribute>(prop);
          if (browse != null && browse.Browsable)
          {
            ValueEditor editor = PropertyEditors.Instance.CreateEditor(target, prop);
            if (editor != null)
            {
              editor.transform.SetParent(scroll.content.transform, false);
            }
            else
            {
              Debug.LogError(string.Format("Failed to create editor for property: {0} ({1})",
                              prop.Name, target.GetType().Name));
            }
          }
        }
        // Ensure the spacer appears at the end.
        Spacer.transform.SetParent(null, false);
        Spacer.transform.SetParent(scroll.content.transform, false);
      }
    }

    public void Clear()
    {
      _target = null;
      // TODO: clear UI.
      ScrollRect scroll = GetComponent<ScrollRect>();
      foreach (ValueEditor edit in scroll.content.GetComponentsInChildren<ValueEditor>())
      {
        edit.transform.SetParent(null, false);
        Destroy(edit.gameObject);
      }
    }

    private object _target = null;
    private LayoutElement _spacer = null;
  }
}
