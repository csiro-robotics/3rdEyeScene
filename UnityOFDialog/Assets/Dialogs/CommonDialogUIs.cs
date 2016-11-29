using System;
using UnityEngine;

namespace Dialogs
{
  /// <summary>
  /// A utility object for registering and tracking the available common dialog
  /// user inerface components.
  /// </summary>
  /// <remarks>
  /// A game object should exist in the scene with this behaviour attached. All common
  /// dialog interfaces are registered with this object.
  /// </remarks>
  public class CommonDialogUIs : MonoBehaviour
  {
    [SerializeField]
    private GameObject[] _uiObjects = new GameObject[0];

    private static CommonDialogUIs _instance;
    public static CommonDialogUIs Instance
    {
      get { return _instance; }
      private set { _instance = value; }
    }

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

    public static GameObject FindGlobalUI(Type uiType)
    {
      if (Instance != null)
      {
        return Instance.FindUI(uiType);
      }
      return null;
    }

    public GameObject FindUI(Type uiType)
    {
      foreach (GameObject uiobj in _uiObjects)
      {
        if (uiobj.GetComponent(uiType) != null)
        {
          return uiobj;
        }
      }

      return null;
    }

    public static T FindGlobalUI<T>() where T : MonoBehaviour
    {
      if (Instance != null)
      {
        return Instance.FindUI<T>();
      }
      return null;
    }

    public T FindUI<T>() where T : MonoBehaviour
    {
      foreach (GameObject uiobj in _uiObjects)
      {
        T uiimp = uiobj.GetComponent<T>();
        if (uiimp != null)
        {
          return uiimp;
        }
      }

      return null;
    }
  }
}
