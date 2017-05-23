using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extension methods for <see cref="ScrollRect"/> scroll view object.
/// </summary>
/// <remarks>
/// These extensions primarily focus on laying out content in a vertical list fashion.
/// </remarks>
public static class ScrollRectExt
{
  /// <summary>
  /// Get the width of the vertical scroll bar component, even if not visible or active.
  /// Vertical scrolling must be enabled.
  /// </summary>
  /// <param name="scroll">The scroll view object to operate on.</param>
  /// <returns>
  /// The width of the vertical scroll bar if vertical scrolling is enabled and
  /// a vertical scroll bar is associated with <paramref cref="scroll"/>
  /// </returns>
  public static float GetVerticalScrollbarWidth(this ScrollRect scroll)
  {
    float scrollWidth = 0;
    if (scroll.content != null && scroll.verticalScrollbar != null && scroll.vertical)
    {
      RectTransform rectXForm = scroll.verticalScrollbar.GetComponent<RectTransform>();
      if (rectXForm != null)
      {
        scrollWidth = rectXForm.rect.width;
      }
    }

    return scrollWidth;
  }


  /// <summary>
  /// Get the height of the horizontal scroll bar component, even if not visible or active.
  /// horizontal scrolling must be enabled.
  /// </summary>
  /// <param name="scroll">The scroll view object to operate on.</param>
  /// <returns>
  /// The height of the horizontal scroll bar if horizontal scrolling is enabled and
  /// a horizontal scroll bar is associated with <paramref cref="scroll"/>
  /// </returns>
  public static float GetHorizontalScrollbarHeight(this ScrollRect scroll)
  {
    float scrollHeight = 0;
    if (scroll.content != null && scroll.horizontalScrollbar != null && scroll.horizontal)
    {
      RectTransform rectXForm = scroll.horizontalScrollbar.GetComponent<RectTransform>();
      if (rectXForm != null)
      {
        scrollHeight = rectXForm.rect.height;
      }
    }

    return scrollHeight;
  }

  /// <summary>
  /// Layout the content of the scroll view in a vertical fashion.
  /// </summary>
  /// <param name="scroll">The scroll view object to operate on.</param>
  /// <param name="subtractVerticalScrollBar">Allow for the width of the vertical scroll bar
  /// such that it does not overlap the children's width?</param>
  /// <remarks>
  /// This lays out children of scroll.content in a vertical fashion, stretching the
  /// children to match the width of the scroll view. Each item is arranged immediately
  /// below the previous item.
  /// </remarks>
  public static void LayoutContentV(this ScrollRect scroll, bool subtractVerticalScrollBar = false)
  {
    float totalHeight = 0;
    float height = 0;
    float scrollWidth = (subtractVerticalScrollBar) ? scroll.GetVerticalScrollbarWidth() : 0;
    RectTransform rectXForm;
    Transform child;

    if (scroll.content == null)
    {
      return;
    }

    for (int i = 0; i < scroll.content.childCount; ++i)
    {
      child = scroll.content.GetChild(i);
      rectXForm = (child != null) ? child.GetComponent<RectTransform>() : null;
      if (rectXForm == null || !rectXForm.gameObject.activeSelf)
      {
        continue;
      }

      // Preserve content height.
      height = rectXForm.rect.height;

      rectXForm.anchorMin = new Vector2(0, 1);
      rectXForm.anchorMax = new Vector2(1, 1);
      rectXForm.pivot = new Vector2(0, 1);

      rectXForm.offsetMin = new Vector2(0, -totalHeight - height);
      rectXForm.offsetMax = new Vector2(scrollWidth, -totalHeight);
      totalHeight += height;
    }

    RectTransform scrollContentRect = (scroll.content.transform as RectTransform);
    scrollContentRect.sizeDelta = new Vector2(scrollContentRect.sizeDelta.x, totalHeight);
  }

  /// <summary>
  /// Append an item to the scroll view, assuming a vertical item layout.
  /// </summary>
  /// <param name="scroll">The scroll view object to operate on.</param>
  /// <param name="contentChild">The child object to append. Must support <see cref="RectTransform"/>
  /// <param name="subtractVerticalScrollBar">Allow for the width of the vertical scroll bar
  /// such that it does not overlap the children's width?</param>
  /// <remarks>
  /// The child is arranged below all the exising child items.
  /// </remarks>
  public static void AppendContentV(this ScrollRect scroll, GameObject contentChild, bool subtractVerticalScrollBar = false)
  {
    if (contentChild != null)
    {
      scroll.AppendContentV(contentChild.GetComponent<RectTransform>(), subtractVerticalScrollBar);
    }
  }

  /// <summary>
  /// Append an item to the scroll view, assuming a vertical item layout.
  /// </summary>
  /// <param name="scroll">The scroll view object to operate on.</param>
  /// <param name="contentChild">The child object to append. Must support <see cref="RectTransform"/>
  /// <param name="subtractVerticalScrollBar">Allow for the width of the vertical scroll bar
  /// such that it does not overlap the children's width?</param>
  /// <remarks>
  /// The child is arranged below all the exising child items.
  /// </remarks>
  public static void AppendContentV(this ScrollRect scroll, RectTransform contentChild, bool subtractVerticalScrollBar = false)
  {
    if (contentChild == null || scroll.content == null)
    {
      return;
    }

    float totalHeight = 0;
    float height = 0;
    float scrollWidth = (subtractVerticalScrollBar) ? scroll.GetVerticalScrollbarWidth() : 0;
    RectTransform rectXForm;
    Transform child;

    for (int i = 0; i < scroll.content.childCount; ++i)
    {
      child = scroll.content.GetChild(i);
      rectXForm = (child != null) ? child.GetComponent<RectTransform>() : null;
      if (rectXForm == null)
      {
        continue;
      }

      // Preserve height.
      totalHeight += rectXForm.rect.height;
    }

    // Preserve content height.
    height = contentChild.rect.height;

    contentChild.SetParent(scroll.content.transform, false);
    contentChild.anchorMin = new Vector2(0, 1);
    contentChild.anchorMax = new Vector2(1, 1);
    contentChild.pivot = new Vector2(0, 1);

    contentChild.offsetMin = new Vector2(0, -totalHeight - height);
    contentChild.offsetMax = new Vector2(scrollWidth, -totalHeight);
    totalHeight += height;

    RectTransform scrollContentRect = (scroll.content.transform as RectTransform);
    scrollContentRect.sizeDelta = new Vector2(scrollContentRect.sizeDelta.x, totalHeight);
  }
}
