using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Dialogs
{
  /// <summary>
  /// Manages detecting the need for and the displaying of tool tips.
  /// </summary>
  /// <remarks>
  /// The update loop detects objects under the current pointer position which
  /// implement the behaviour <see cref="ToolTipInfo"/>. If such is detected,
  /// then the <see cref="ToolTipUI"/> is enabled and modified to display the
  /// text of the <see cref="ToolTipInfo"/>. The UI requires a <see cref="UnityEngine.UI.Text"/>
  /// component to display the tool tip text. Other UI aspects are configurable.
  ///
  /// The UI is positioned by setting its position to the mouse pointer position plus
  /// the <see cref="PositionOffset"/>. The position is modified to keep the UI within
  /// the screen bounds. The UI is moved whenever the cursor overlaps it or when a
  /// new <see cref="ToolTipInfo"/> is detected.
  ///
  /// The UI component should be ordered in the hierarchy to enusre it overdraws other components.
  /// The recommendation is to add it to the Dialogs UI canvas and make that canvas appear as the last
  /// UI component. The default tool tip UI is configured as the last component in the dialogs canvas.
  ///
  /// The class is designed only to work with overlay style UIs.
  /// </remarks>
  public class ToolTipOverlay : MonoBehaviour
  {
    [SerializeField]
    private RectTransform _toolTipUI;
    /// <summary>
    /// The UI component used to display the tool tip.
    /// </summary>
    public RectTransform ToolTipUI { get { return _toolTipUI; } }
    [SerializeField]
    private Vector2 _positionOffset = new Vector2(5, 5);
    /// <summary>
    /// Offset applied when positioning the UI component for rendering.
    /// </summary>
    public Vector2 PositionOffset
    {
      get { return _positionOffset; }
      set { _positionOffset = value; }
    }

    [SerializeField, Range(0, 10)]
    private float _toolTipDelay = 1.0f;
    /// <summary>
    /// Delay before showing the tooltip for the current control (seconds).
    /// </summary>
    public float ToolTipDelay
    {
      get { return _toolTipDelay; }
      set { _toolTipDelay = value; }
    }

    /// <summary>
    /// A helper to extract a Text object from the <see  cref="ToolTipUIText"/>
    /// </summary>
    /// <returns></returns>
    public Text ToolTipUIText
    {
      get
      {
        if (_toolTipUI != null)
        {
          return _toolTipUI.GetComponentInChildren<Text>(true);
        }
        return null;
      }
    }

    /// <summary>
    /// Manages the tool tip display.
    /// </summary>
    /// <remarks>
    /// Raycasts using the current event system and locate the last <see cref="ToolTipInfo"/> object.
    /// Manages the tool tip UI position.
    /// </remarks>
    void Update()
    {
      // Do ray casting.
      PointerEventData pointerPosition = new PointerEventData(EventSystem.current);
      pointerPosition.position = Input.mousePosition;
      _hits.Clear();
      EventSystem.current.RaycastAll(pointerPosition, _hits);

      // Search results: use the last object found.
      ToolTipInfo toolTipObj = null;
      ToolTipInfo tti = null;
      for (int i = 0; i < _hits.Count; ++i)
      {
        if (_hits[i].gameObject != null)
        {
          tti = _hits[i].gameObject.GetComponent<ToolTipInfo>();
          if (tti)
          {
            toolTipObj = tti;
          }
        }
      }

      if (toolTipObj != null)
      {
        if (_currentToolTip == null)
        {
          _hoverTime = ToolTipDelay;
          _currentToolTip = toolTipObj;
          _hover = true;
        }
        else
        {
          _hoverTime -= Time.deltaTime;
        }

        if (_hoverTime <= 0)
        {
          _hoverTime = 0;
          ShowToolTip(toolTipObj, pointerPosition.position + _positionOffset);
        }
      }
      else
      {
        _hoverTime = 0.0f;
        _hover = false;
        HideToolTip();
      }
    }

    /// <summary>
    /// Show the tool tip UI.
    /// </summary>
    /// <param name="info">The tool tip to show.</param>
    /// <param name="pointerPos">Where to position the UI in UI space (apply offset before calling).</param>
    private void ShowToolTip(ToolTipInfo info, Vector2 pointerPos)
    {
      bool positionToolTip = false;
      if (_currentToolTip != info || _hover)
      {
        // Changing or new tool tip.
        positionToolTip = true;
      }
      else
      {
        // Validate positioning: is the cursor over the tooltip?
        RectTransform ttui = ToolTipUI;
        if (ttui != null)
        {
          Rect rect = GetGlobalRect(ttui);
          positionToolTip = rect.Contains(pointerPos);
        }
      }

      if (positionToolTip)
      {
        Text ttx = ToolTipUIText;
        if (ttx != null)
        {
          _currentToolTip = info;
          ttx.text = info.ToolTip;
          RectTransform ttui = ToolTipUI;
          if (ttui != null)
          {
            ResizeToolTip(ttui, ttx, info);
            PositionToolTip(ttui, info, pointerPos);
            ttui.gameObject.SetActive(true);
          }
        }
      }

      _hover = false;
    }

    /// <summary>
    /// Ensures the tool tip UI is hidden.
    /// </summary>
    private void HideToolTip()
    {
      if (_currentToolTip != null)
      {
        RectTransform ttui = ToolTipUI;
        if (ttui != null)
        {
          ttui.gameObject.SetActive(false);
          _currentToolTip = null;
        }
      }
    }

    /// <summary>
    /// Resize the UI to fit the text.
    /// </summary>
    /// <param name="rect">The UI rect</param>
    /// <param name="textUI">The tool tip text display object.</param>
    /// <param name="toolTip">Tool tip to display.</param>
    private void ResizeToolTip(RectTransform rect, Text textUI, ToolTipInfo toolTip)
    {
      float textWidth = textUI.cachedTextGenerator.GetPreferredWidth(toolTip.ToolTip,
              textUI.GetGenerationSettings(rect.rect.size));
      textWidth += GetGlobalRect(rect).width - GetGlobalRect(textUI.rectTransform).width;
      Vector2 omax = rect.offsetMax;
      omax.x = rect.offsetMin.x + textWidth;
      rect.offsetMax = omax;
    }

    /// <summary>
    /// Position the tool tip around <paramref name="pointerPos"/>. Adjust to fit to screen.
    /// </summary>
    /// <param name="rect">UI rect.</param>
    /// <param name="toolTip">Tool tip to display.</param>
    /// <param name="pointerPos">Where to display in UI space.</param>
    private void PositionToolTip(RectTransform rect, ToolTipInfo toolTip, Vector2 pointerPos)
    {
      // Position near current cursor.
      Vector2 canvasPos;
      Canvas canvas = rect.GetComponentInParent<Canvas>();
      RectTransform canvasTransform = (RectTransform)canvas.transform;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
          canvasTransform, pointerPos,
          canvas.worldCamera, out canvasPos);
      canvasPos = canvasTransform.TransformPoint(canvasPos);
      rect.position = canvasPos;
      FitTo(rect, canvasTransform);
    }

    /// <summary>
    /// Fit one rectangle to another.
    /// </summary>
    /// <param name="toFit">The rectangle to fit.</param>
    /// <param name="parent">The rectangle to fit within.</param>
    /// <remarks>
    /// Adjust <paramref name="toFit"/> position and size to fit within the <paramref name="parent"/>.
    /// </remarks>
    private void FitTo(RectTransform toFit, RectTransform parent)
    {
      Rect rect = GetGlobalRect(toFit);
      Rect parentRect = GetGlobalRect(parent);

      if (rect.xMax > parentRect.xMax)
      {
        rect.xMin -= rect.xMax - parentRect.xMax;
      }
      if (rect.yMax > parentRect.yMax)
      {
        rect.yMin -= rect.yMax - parentRect.yMax;
      }

      if (rect.xMin < parentRect.xMin)
      {
        rect.xMin = parentRect.xMin;
      }

      if (rect.yMin < parentRect.yMin)
      {
        rect.yMin = parentRect.yMin;
      }

      toFit.position = new Vector2(rect.xMin, rect.yMin);
    }

    /// <summary>
    /// Get the world/global rectangle for a <tt>RectTransform</tt>
    /// </summary>
    /// <param name="transform">The transform to generate a rectangle for.</param>
    /// <returns>The global rectangle for <paramref name="transform"/>.</returns>
    public static Rect GetGlobalRect(RectTransform transform)
    {
      Vector3[] corners = new Vector3[4];
      Rect rect = new Rect();
      transform.GetWorldCorners(corners);
      rect.xMin = corners[0].x;
      rect.yMin = corners[0].y;
      rect.xMax = corners[2].x;
      rect.yMax = corners[2].y;
      return rect;
    }

    private List<RaycastResult> _hits = new List<RaycastResult>();
    private ToolTipInfo _currentToolTip = null;
    /// <summary>
    /// Hover time remaining.
    /// </summary>
    private float _hoverTime = 0;
    /// <summary>
    /// Currently hovering before dislay?
    /// </summary>
    private bool _hover = false;
  }
}
