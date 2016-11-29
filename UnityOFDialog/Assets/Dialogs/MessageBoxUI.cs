using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Dialogs
{
  /// <summary>
  /// The UI script for <see cref="MessageBox"/>.
  /// </summary>
  /// <remarks>
  /// The message box UI may be shared by different message boxes. However, we assume we can only display one
  /// at a time with any priority. To help with this, the UI maintains a stack of <see cref="Owner"/>
  /// items. Whenever an <see cref="Owner"/> is set, the previous one is pushed onto the stack.
  /// Once the UI closes, the previous <see cref="Owner"/> is restored and the UI calls
  /// <see cref="MessageBox.ShowMessageOn()"/> is called to update the UI to that owner.
  /// Setting a null <see cref="Owner"/> prevents this behaviour and clears the stack.
  /// </remarks>
  public class MessageBoxUI : MonoBehaviour
  {
    [SerializeField]
    private bool _defaultMessageBox = false;
    public bool DefaultMesageBox { get { return _defaultMessageBox; } }

    [SerializeField]
    protected Text _titleText = null;
    [SerializeField]
    protected Text _messageText = null;
    /// <summary>
    /// Yes/Abort/OK button.
    /// </summary>
    [SerializeField]
    protected Button _confirmButton = null;
    /// <summary>
    /// No/Retry button.
    /// </summary>
    [SerializeField]
    protected Button _denyButton = null;
    /// <summary>
    /// Cancel button.
    /// </summary>
    [SerializeField]
    protected Button _cancelButton = null;

    /// <summary>
    /// Control used to display the icon.
    /// </summary>
    [SerializeField]
    protected Image _iconImage = null;

    [Serializable]
    public struct ButtonTextLabels
    {
      public string OK;
      public string Cancel;
      public string Yes;
      public string No;
      public string Abort;
      public string Retry;
      public string Ignore;

      public static ButtonTextLabels Default()
      {
        return new ButtonTextLabels()
        {
          OK = "OK",
          Cancel = "Cancel",
          Yes = "Yes",
          No = "No",
          Abort = "Abort",
          Retry = "Retry",
          Ignore = "Ignore"
        };
      }
    }

    [SerializeField]
    protected ButtonTextLabels _buttonText = ButtonTextLabels.Default();
    public ButtonTextLabels ButtonText { get { return _buttonText; } set { _buttonText = value; } }

    private MessageBoxButtons _buttons;
    public MessageBoxButtons Buttons
    {
      get { return _buttons; }
      set
      {
        _buttons = value;
        UpdateButtons();
      }
    }

    public MessageBox Owner
    {
      get { return _owner; }

      set
      {
        if (value != null)
        {
          // Push the previous owner onto the stack.
          if (_owner != null && value != null)
          {
            if (_ownerStack == null)
            {
              _ownerStack = new Stack<MessageBox>();
            }

            if (!_ownerStack.Contains(_owner))
            {
              // Handle two cases:
              // 1: Message box while already open or => push current owner and show new owner.
              // 2: Message box while closing => push new owner and finish closing, then show new owner.
              _ownerStack.Push(_closing ? value : _owner);
            }
          }
        }
        else if (_ownerStack != null)
        {
          _ownerStack.Clear();
        }
        _owner = value;
      }
    }

    public string ConfirmText { set { SetText(_confirmButton, value); } get { return GetText(_confirmButton); } }
    public string DenyText { set { SetText(_denyButton, value); } get { return GetText(_denyButton); } }
    public string CancelText { set { SetText(_cancelButton, value); } get { return GetText(_cancelButton); } }
    public string Title { set { SetText(_titleText, value); } get { return GetText(_titleText); } }
    public string Message { set { SetText(_messageText, value); } get { return GetText(_messageText); } }
    public Sprite Icon
    {
      set
      {
        if (_iconImage != null)
        {
          _iconImage.sprite = value;
          _iconImage.gameObject.SetActive(value != null);
        }
      }

      get
      {
        if (_iconImage != null)
        {
          return _iconImage.sprite;
        }
        return null;
      }
    }

    public DialogResult ConfirmResult { get; protected set; }
    public DialogResult DenyResult { get; protected set; }
    public DialogResult CancelResult { get; protected set; }

    public static MessageBoxUI DefaultUI
    {
      get { return CommonDialogUIs.FindGlobalUI<MessageBoxUI>(); }
    }

    void Start()
    {
      UpdateButtons();
    }

    void OnDisable()
    {
      _confirmButton.gameObject.SetActive(false);
      _denyButton.gameObject.SetActive(false);
      _cancelButton.gameObject.SetActive(false);
      _iconImage.gameObject.SetActive(false);
    }

    protected void UpdateButtons()
    {
      if (_confirmButton != null) _confirmButton.gameObject.SetActive(false);
      if (_denyButton != null) _denyButton.gameObject.SetActive(false);
      if (_cancelButton != null) _cancelButton.gameObject.SetActive(false);

      switch (Buttons)
      {
      case MessageBoxButtons.OK:
        ConfirmText = ButtonText.OK;
        ConfirmResult = DialogResult.OK;
        break;
      case MessageBoxButtons.OKCancel:
        ConfirmText = ButtonText.OK;
        CancelText = ButtonText.Cancel;
        ConfirmResult = DialogResult.OK;
        CancelResult = DialogResult.Cancel;
        break;
      case MessageBoxButtons.AbortRetryIgnore:
        ConfirmText = ButtonText.Abort;
        DenyText = ButtonText.Retry;
        CancelText = ButtonText.Ignore;
        ConfirmResult = DialogResult.Abort;
        DenyResult = DialogResult.Retry;
        CancelResult = DialogResult.Ignore;
        break;
      case MessageBoxButtons.YesNoCancel:
        ConfirmText = ButtonText.Yes;
        DenyText = ButtonText.No;
        CancelText = ButtonText.Cancel;
        ConfirmResult = DialogResult.Yes;
        DenyResult = DialogResult.No;
        CancelResult = DialogResult.Cancel;
        break;
      case MessageBoxButtons.YesNo:
        ConfirmText = ButtonText.Yes;
        DenyText = ButtonText.No;
        ConfirmResult = DialogResult.Yes;
        DenyResult = DialogResult.No;
        break;
      case MessageBoxButtons.RetryCancel:
        DenyText = ButtonText.Retry;
        CancelText = ButtonText.Cancel;
        DenyResult = DialogResult.Retry;
        CancelResult = DialogResult.Cancel;
        break;
      }
    }

    public void OnConfirm()
    {
      Complete(ConfirmResult);
    }

    public void OnDeny()
    {
      Complete(DenyResult);
    }

    public void OnCancel()
    {
      Complete(CancelResult);
    }

    private void Complete(DialogResult result)
    {
      _closing = true;
      if (Owner != null)
      {
        Owner.Close(result);
      }
      gameObject.SetActive(false);
      PopOwnerStack();
      _closing = false;
    }

    protected static void SetText(Button button, string text)
    {
      if (button != null)
      {
        SetText(button.GetComponentInChildren<Text>(), text);
        button.gameObject.SetActive(true);
      }
    }

    protected static string GetText(Button button)
    {
      if (button != null)
      {
        return GetText(button.GetComponentInChildren<Text>());
      }

      return string.Empty;
    }

    protected static void SetText(Text uiText, string text)
    {
      if (uiText != null)
      {
        uiText.text = text;
        uiText.gameObject.SetActive(true);
      }
    }

    protected static string GetText(Text text)
    {
      if (text != null)
      {
        return text.text;
      }

      return string.Empty;
    }

    private void PopOwnerStack()
    {
      if (_ownerStack != null && _ownerStack.Count != 0)
      {
        _owner = _ownerStack.Pop();
        if (_owner != null)
        {
          _owner.ShowMessageOn(this);
          // Display the UI again.
          gameObject.SetActive(true);
        }
      }
      else
      {
        _owner = null;
      }
    }

    private Stack<MessageBox> _ownerStack = null;
    private MessageBox _owner = null;
    private bool _closing = false;
  }
}
