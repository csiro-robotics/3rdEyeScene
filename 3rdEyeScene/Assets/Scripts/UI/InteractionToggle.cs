using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
  /// <summary>
  /// Raises events when mouse press/release registers on the UI.
  /// </summary>
  /// <remarks>
  /// Can be used to limit mouse iteraction behind other UI components.
  /// </remarks>
  class InteractionToggle : UnityEngine.UI.Selectable, IPointerDownHandler, IPointerUpHandler
  {
    public UnityEvent onMouseDown = new UnityEvent();
    public UnityEvent onMouseUp = new UnityEvent();

    public override void OnPointerDown(PointerEventData eventData)
    {
      if (onMouseDown != null)
      {
        onMouseDown.Invoke();
      }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
      if (onMouseUp != null)
      {
        onMouseUp.Invoke();
      }
    }
  }
}