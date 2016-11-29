using UnityEngine;
using System.Collections;

namespace Dialogs
{ 
  public enum MessageBoxButtons
  {
    OK,
    OKCancel,
    AbortRetryIgnore,
    YesNoCancel,
    YesNo,
    RetryCancel
  }

  public class MessageBox : CommonDialog
  {
    public MessageBoxUI UI { get; protected set; }
    public MessageBoxButtons Buttons { get; protected set; }
    public string Title { get; protected set; }
    public string Message { get; protected set; }
    public Sprite Icon { get; protected set; }

    protected MessageBox(DialogCloseDelegate callback, string message, string title, MessageBoxButtons buttons, Sprite icon, MessageBoxUI ui)
    {
      _closeDelegate += callback;
      Message = message;
      Title = title;
      Buttons = buttons;
      Icon = icon;
      UI = ui;
      UI.Owner = this;
      UI.gameObject.SetActive(true);
      ShowMessageOn(UI);
    }

    internal void ShowMessageOn(MessageBoxUI ui)
    {
      UI.Message = Message;
      UI.Title = Title;
      UI.Buttons = Buttons;
      UI.Icon = Icon;
    }

    public static void Show(DialogCloseDelegate callback, string message)
    {
      Show(callback, message, "", MessageBoxButtons.OK, null, MessageBoxUI.DefaultUI);
    }

    public static void Show(DialogCloseDelegate callback, string message, MessageBoxUI ui)
    {
      Show(callback, message, "", MessageBoxButtons.OK, null, ui);
    }

    public static void Show(DialogCloseDelegate callback, string message, string title)
    {
      Show(callback, message, title, MessageBoxButtons.OK, null, MessageBoxUI.DefaultUI);
    }

    public static void Show(DialogCloseDelegate callback, string message, string title, MessageBoxUI ui)
    {
      Show(callback, message, title, MessageBoxButtons.OK, null, ui);
    }

    public static void Show(DialogCloseDelegate callback, string message, string title, MessageBoxButtons buttons)
    {
      Show(callback, message, title, buttons, null, MessageBoxUI.DefaultUI);
    }

    public static void Show(DialogCloseDelegate callback, string message, string title, MessageBoxButtons buttons, MessageBoxUI ui)
    {
      Show(callback, message, title, buttons, null, ui);
    }

    public static void Show(DialogCloseDelegate callback, string message, string title, MessageBoxButtons buttons, Sprite icon)
    {
      Show(callback, message, title, buttons, icon, MessageBoxUI.DefaultUI);
    }

    public static void Show(DialogCloseDelegate callback, string message, string title, MessageBoxButtons buttons, Sprite icon, MessageBoxUI ui)
    {
      if (ui != null)
      {
        new MessageBox(callback, message, title, buttons, icon, ui);
      }
      else
      {
        Debug.LogError("Missing message box UI");
      }
    }

    protected override void OnShow()
    {
      UI.Buttons = Buttons;
      UI.Owner = this;
      UI.gameObject.SetActive(true);
    }
  }
}
