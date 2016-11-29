using System.Collections;
using UnityEngine;

namespace Dialogs
{
  public enum DialogResult
  {
    None,
    OK,
    Cancel,
    Abort,
    Retry,
    Ignore,
    Yes,
    No
  }

  public delegate void DialogCloseDelegate(CommonDialog dialog, DialogResult result);

  public abstract class CommonDialog
  {
    public RectTransform DialogPanel { get; set; }

    public CommonDialog()
    {
    }

    public virtual void Reset()
    {
      if (_closeDelegate != null)
      {
        foreach (System.Delegate d in _closeDelegate.GetInvocationList())
        {
          _closeDelegate -= (DialogCloseDelegate)d;
        }
      }
    }

    public void ShowDialog(DialogCloseDelegate closeEvent)
    {
      Reset();
      _closeDelegate += closeEvent;
      DialogPanel.gameObject.SetActive(true);
      OnShow();
    }

    protected abstract void OnShow();

    internal virtual void Close(DialogResult result)
    {
      if (DialogPanel != null)
      {
        DialogPanel.gameObject.SetActive(false);
      }

      if (_closeDelegate != null)
      {
        _closeDelegate(this, result);
      }
    }

//    public DialogResult ShowDialog()
//    {
//      bool done = false;
//      DialogResult result = DialogResult.None;
//      DialogCloseEvent onClose = (CommonDialog dialog, DialogResult eventResult) =>
//      {
//        result = eventResult;
//        done = true;
//      };
//
//      while (!done)
//      {
//
//      }
//    }

    protected event DialogCloseDelegate _closeDelegate;
  }
}
