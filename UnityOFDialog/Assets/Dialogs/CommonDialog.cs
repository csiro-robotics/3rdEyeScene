using System.ComponentModel;
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

    /// <summary>
    /// Use flag to allow the use of a native dialog if supported and viable.
    /// </summary>
    [DefaultValue(true)]
    public bool AllowNative { get; set; }

    /// <summary>
    /// True if a native dialog is available and is allowed (<see cref="AllowNative"/>).
    /// </summary>
    public abstract bool CanShowNative { get; }

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

    /// <summary>
    /// Call to show the dialog.
    /// </summary>
    /// <param name="closeEvent">Delegate to invoke on closing the dialog.</param>
    /// <remarks>
    /// When <see cref="CanShowNative"/> is <c>true</c>, a call is made to <see cref="ShowNative()"/>.
    /// Otherwise the <see cref="DialogPanel"/> is made active then <see cref="OnShow()"/> is called.
    /// </remarks>
    public void ShowDialog(DialogCloseDelegate closeEvent)
    {
      Reset();
      _closeDelegate += closeEvent;
      if (CanShowNative)
      {
        OnShowNative();
      }
      else
      {
        DialogPanel.gameObject.SetActive(true);
        OnShow();
      }
    }

    protected abstract void OnShowNative();


    protected abstract void OnShow();

    internal virtual void Close(DialogResult result)
    {
      if (DialogPanel != null && !CanShowNative)
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
