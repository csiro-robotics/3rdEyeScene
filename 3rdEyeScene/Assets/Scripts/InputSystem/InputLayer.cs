using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputSystem
{
  /// <summary>
  /// Defines how an input layer behaves in relation to those below.
  /// </summary>
  public enum InputLayerOcclusion
  {
    /// <summary>
    /// The layer allows those below it to behave normally.
    /// </summary>
    Transparent,
    /// <summary>
    /// The layer blocks those below.
    /// </summary>
    Opaque,
  }

  /// <summary>
  /// A layer in the <see cref="InputStack"/>
  /// </summary>
  /// <remarks>
  /// The layer replicates functions for <tt>UnityEngine.Input</tt>, but filters
  /// the results based on whether the layer is active.
  /// </remarks>
  [Serializable]
  public class InputLayer
  {
    [SerializeField]
    private string _name;
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// Currently active? Depends on AllowActive and Enabled.
    /// </summary>
    public bool Active
    {
      get { return (!_blocked || _ignoreBlocking) && _enabled; }
    }

    /// <summary>
    /// Currently allowed to be active? Must also be enabled to report true.
    /// </summary>
    public bool Blocked
    {
      get { return _blocked; }
      set { _blocked = value; }
    }

    /// <summary>
    /// Currently enabled?
    /// </summary>
    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }

    /// <summary>
    /// Stay active even when blocked by a layer above?
    /// </summary>
    public bool IgnoreBlocking
    {
      get { return _ignoreBlocking; }
      set { _ignoreBlocking = value; }
    }

    [SerializeField]
    InputLayerOcclusion _occlusion = InputLayerOcclusion.Transparent;
    /// <summary>
    /// Defines how this layer affects those below it.
    /// </summary>
    public InputLayerOcclusion Occlusion
    {
      get { return _occlusion; }
      set { _occlusion = value; }
    }

    /// <summary>
    /// Has the layer become active this frame?
    /// </summary>
    /// <returns></returns>
    public bool Activated { get { return Active && !_lastActive; } }

    /// <summary>
    /// Has the layer become inactive this frame?
    /// </summary>
    /// <returns></returns>
    public bool Deactivated { get { return !Active && _lastActive; } }

    public void Update()
    {
      _lastActive = Active;
      if (Active)
      {
        for (int i = 0; i < _commands.Count; ++i)
        {
          if (_commands[i].KeyCombo.Pressed(this))
          {
            _commands[i].OnTriggered.Invoke();
          }
        }
      }
    }

    public IEnumerable<Command> Commands
    {
      get { return _commands; }
    }

    public AccelerationEvent GetAccelerationEvent(int index)
    {
      return Active ? UnityEngine.Input.GetAccelerationEvent(index) : new AccelerationEvent();
    }

    public float GetAxis(string axisName)
    {
      return Active ? UnityEngine.Input.GetAxis(axisName) : 0.0f;
    }

    public float GetAxisRaw(string axisName)
    {
      return Active ? UnityEngine.Input.GetAxisRaw(axisName) : 0.0f;
    }

    public bool GetButton(string buttonName)
    {
      return Active ? UnityEngine.Input.GetButton(buttonName) : false;
    }

    public bool GetButtonDown(string buttonName)
    {
      if (Activated && UnityEngine.Input.GetButton(buttonName))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetButtonDown(buttonName) : false;
    }

    public bool GetButtonUp(string buttonName)
    {
      if (Deactivated && UnityEngine.Input.GetButton(buttonName))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetButtonUp(buttonName) : false;
    }

    public bool GetKey(string keyName)
    {
      return Active ? UnityEngine.Input.GetKey(keyName) : false;
    }

    public bool GetKeyDown(string keyName)
    {
      if (Activated && UnityEngine.Input.GetKey(keyName))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetKeyDown(keyName) : false;
    }

    public bool GetKeyUp(string keyName)
    {
      if (Deactivated && UnityEngine.Input.GetKey(keyName))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetKeyUp(keyName) : false;
    }

    public bool GetKey(KeyCode keyCode)
    {
      return Active ? UnityEngine.Input.GetKey(keyCode) : false;
    }

    public bool GetKeyDown(KeyCode keyCode)
    {
      if (Activated && UnityEngine.Input.GetKey(keyCode))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetKeyDown(keyCode) : false;
    }

    public bool GetKeyUp(KeyCode keyCode)
    {
      if (Deactivated && UnityEngine.Input.GetKey(keyCode))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetKeyUp(keyCode) : false;
    }

    public bool GetMouseButton(int button)
    {
      return Active ? UnityEngine.Input.GetMouseButton(button) : false;
    }

    public bool GetMouseButtonDown(int button)
    {
      if (Activated && UnityEngine.Input.GetMouseButton(button))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetMouseButtonDown(button) : false;
    }

    public bool GetMouseButtonUp(int button)
    {
      if (Deactivated && UnityEngine.Input.GetMouseButton(button))
      {
        return true;
      }
      return Active ? UnityEngine.Input.GetMouseButtonUp(button) : false;
    }

    public Touch GetTouch(int index)
    {
      return Active ? UnityEngine.Input.GetTouch(index) : new Touch();
    }

    private bool _blocked = false;
    [SerializeField]
    private bool _enabled = true;
    private bool _lastActive = false;
    [SerializeField]
    private bool _ignoreBlocking = false;
    [SerializeField]
    private List<Command> _commands = new List<Command>();
  }
}