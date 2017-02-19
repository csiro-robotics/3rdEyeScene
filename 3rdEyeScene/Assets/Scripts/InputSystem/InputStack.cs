using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputSystem
{
  /// <summary>
  /// The <tt>InputStack</tt> helps resolve issues with input priority by managing a stack
  /// of active <see cref="InputLayer"/> entries.
  /// </summary>
  /// <remarks>
  /// Each layer in the stack is allowed to block layers below it. This is done holistically
  /// for <tt>UnityEngine.Input</tt> style requests, or by layer for <tt>Command</tt>
  /// processing.
  /// </remarks>
  public class InputStack : MonoBehaviour
  {
    public InputLayer GetLayer(string layerName)
    {
      for (int i = 0; i < _layers.Count; ++i)
      {
        if (string.Compare(_layers[i].Name, layerName) == 0)
        {
          return _layers[i];
        }
      }
      return null;
    }

    public bool SetLayerEnabled(string layerName, bool enabled)
    {
      for (int i = 0; i < _layers.Count; ++i)
      {
        if (string.Compare(_layers[i].Name, layerName) == 0)
        {
          _layers[i].Enabled = enabled;
          return true;
        }
      }
      return false;
    }

    public void Update()
    {
      bool blocked = false;
      for (int i = _layers.Count - 1; i >= 0; --i)
      {
        _layers[i].Blocked = blocked;
        _layers[i].Update();
        if (_layers[i].Active)
        {
          switch (_layers[i].Occlusion)
          {
          case InputLayerOcclusion.Opaque:
            // Block subsequent layers.
            blocked = true;
            break;
          default:
            break;
          }
        }
      }
    }

    [SerializeField]
    private List<InputLayer> _layers;
  }
}