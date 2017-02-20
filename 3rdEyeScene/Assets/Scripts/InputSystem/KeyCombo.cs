using UnityEngine;
using System;

namespace InputSystem
{
  [Flags]
  public enum MetaKey
  {
    Shift = (1 << 0),
    Alt = (1 << 1),
    Control = (1 << 2),
    Super = (1 << 3),
  }

  /// <summary>
  /// Defines a combination of keys use to trigger a command.
  /// </summary>
  [Serializable]
  public class KeyCombo
  {
    public string KeyName { get { return _keyName; } }
    public KeyCode KeyCode { get { return _keyCode; } }
    public MetaKey Meta
    {
      get
      {
        #if UNITY_EDITOR
        if (_editorMeta != 0)
        {
          return _editorMeta;
        }
        #endif // UNITY_EDITOR
        return _meta;
      }
    }

    public KeyCombo() { }

    public KeyCombo(KeyCode code, MetaKey meta = 0)
    {
      _keyCode = code;
      // Using _editorMeta is not important, but helps avoid a build warning.
      _editorMeta = meta;
      _meta = _editorMeta;
    }

    public KeyCombo(string keyName, MetaKey meta = 0)
    {
      _keyName = keyName;
      // Using _editorMeta is not important, but helps avoid a build warning.
      _editorMeta = meta;
      _meta = _editorMeta;
    }

    public bool Parse(string sequence)
    {
      string[] parts = sequence.Split(new char[] { '+' });
      string keyName = null;
      MetaKey meta = 0;
      for (int i = 0; i < parts.Length; ++i)
      {
        if (!ParseMeta(ref meta, parts[i]))
        {
          if (keyName != null)
          {
            // Multiple keys specified.
            return false;
          }
          keyName = parts[i];
        }
      }

      if (keyName == null)
      {
        // No key.
        return false;
      }

      _meta = meta;
      if (ParseKey(ref _keyCode, keyName))
      {
        _keyName = null;
      }
      else
      {
        _keyCode = KeyCode.None;
        _keyName = keyName;
      }
      return true;
    }

    public static bool ParseMeta(ref MetaKey meta, string str)
    {
      if (string.Compare(str, "shift", true) == 0)
      {
        meta |= MetaKey.Shift;
        return true;
      }
      else if (string.Compare(str, "control", true) == 0)
      {
        meta |= MetaKey.Control;
        return true;
      }
      else if (string.Compare(str, "windows", true) == 0)
      {
        meta |= MetaKey.Super;
        return true;
      }
      else if (string.Compare(str, "command", true) == 0)
      {
        meta |= MetaKey.Super;
        return true;
      }
      else if (string.Compare(str, "super", true) == 0)
      {
        meta |= MetaKey.Super;
        return true;
      }
      return false;
    }

    public static bool ParseKey(ref KeyCode code, string str)
    {
      foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
      {
        if (string.Compare(str, kc.ToString()) == 0)
        {
          code = kc;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Has the sequence been pressed this frame?
    /// </summary>
    public bool Pressed(InputLayer layer)
    {
      if (_keyCode != KeyCode.None && layer.GetKeyDown(_keyCode) ||
          !string.IsNullOrEmpty(_keyName) && layer.GetButtonDown(_keyName))
      {
        if (MetaActive(Meta, layer))
        {
          if (MetaActive(Meta, layer))
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Has the sequence been pressed this frame?
    /// </summary>
    public bool Released(InputLayer layer)
    {
      if (_keyCode != KeyCode.None && layer.GetKeyUp(_keyCode) ||
          !string.IsNullOrEmpty(_keyName) && layer.GetButtonUp(_keyName))
      {
        if (MetaActive(Meta, layer))
        {
          return true;
        }
      }
      else if (_keyCode != KeyCode.None && layer.GetKey(_keyCode) ||
          !string.IsNullOrEmpty(_keyName) && layer.GetButton(_keyName))
      {
        if (MetaReleased(Meta, layer))
        {

        }
      }
      return false;
    }

    /// <summary>
    /// Has the sequence been pressed this frame?
    /// </summary>
    public bool IsDown(InputLayer layer)
    {
      if (_keyCode != KeyCode.None && layer.GetKey(_keyCode) ||
          !string.IsNullOrEmpty(_keyName) && layer.GetButton(_keyName))
      {
        if (MetaActive(Meta, layer))
        {
          return true;
        }
      }
      return false;
    }

    public static bool MatchMetaState(MetaKey meta, InputLayer layer, MetaKey testFlag, KeyCode key1, KeyCode key2)
    {
      if ((meta & testFlag) != 0 && !layer.GetKey(key1) && !layer.GetKey(key2) ||
          (meta & testFlag) == 0 && (layer.GetKey(key1) || layer.GetKey(key2)))
      {
        return false;
      }
      return true;
    }

    public static bool MatchMetaState(MetaKey meta, InputLayer layer, MetaKey testFlag, KeyCode key1, KeyCode key2, KeyCode key3, KeyCode key4)
    {
      if ((meta & testFlag) != 0 && !layer.GetKey(key1) && !layer.GetKey(key2) && !layer.GetKey(key3) && !layer.GetKey(key4) ||
          (meta & testFlag) == 0 && (layer.GetKey(key1) || layer.GetKey(key2) || layer.GetKey(key3) || layer.GetKey(key4)))
      {
        return false;
      }
      return true;
    }

    public static bool MatchMetaState(MetaKey meta, InputLayer layer, MetaKey testFlag, KeyCode key)
    {
      return MatchMetaState(meta, layer, testFlag, key, key);
    }

    public static bool MetaActive(MetaKey meta, InputLayer layer)
    {
      // FIXME: this is not precise enough for multiple meta combos.
      bool active = true;

      active = active && MatchMetaState(meta, layer, MetaKey.Shift, KeyCode.LeftShift, KeyCode.RightShift);
      active = active && MatchMetaState(meta, layer, MetaKey.Alt, KeyCode.LeftAlt, KeyCode.RightAlt);
      active = active && MatchMetaState(meta, layer, MetaKey.Control, KeyCode.LeftControl, KeyCode.RightControl);
      active = active && MatchMetaState(meta, layer, MetaKey.Super, KeyCode.LeftWindows, KeyCode.RightWindows, KeyCode.LeftCommand, KeyCode.RightCommand);

      return active;
    }

    public static bool MetaReleased(MetaKey meta, InputLayer layer)
    {
      // FIXME: this is not precise enough for multiple meta combos.
      bool active = true;

      if ((meta & MetaKey.Shift) != 0 &&
          !layer.GetKeyUp(KeyCode.LeftShift) && !layer.GetKeyUp(KeyCode.RightShift))
      {
        active = false;
      }

      if ((meta & MetaKey.Alt) != 0 &&
          !layer.GetKeyUp(KeyCode.LeftAlt) && !layer.GetKeyUp(KeyCode.RightAlt))
      {
        active = false;
      }

      if ((meta & MetaKey.Control) != 0 &&
          !layer.GetKeyUp(KeyCode.LeftControl) && !layer.GetKeyUp(KeyCode.RightControl))
      {
        active = false;
      }

      if ((meta & MetaKey.Super) != 0 &&
          !layer.GetKeyUp(KeyCode.LeftCommand) && !layer.GetKeyUp(KeyCode.RightCommand) &&
          !layer.GetKeyUp(KeyCode.LeftWindows) && !layer.GetKeyUp(KeyCode.RightWindows))
      {
        active = false;
      }

      return active;
    }

    [SerializeField]
    private string _keyName = null;
    [SerializeField]
    private KeyCode _keyCode = KeyCode.None;
    [SerializeField, EnumFlags]
    private MetaKey _meta = 0;
    [SerializeField, EnumFlags]
    private MetaKey _editorMeta = 0;
  }
}