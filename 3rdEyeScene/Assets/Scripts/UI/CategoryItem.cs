using UnityEngine;
using UnityEngine.UI;

public class CategoryItem : MonoBehaviour
{
  public delegate void ExpandChange(CategoryItem item, bool expanded);
  public event ExpandChange OnExpandChange;

  public int IndentWidth { get { return _indentWidth; } }

  public string CategoryName
  {
    get { return _categoryName; }
    set
    {
      _categoryName = value;
      if (Text != null)
      {
        Text.text = value;
      }
    }
  }

  public int ID
  {
    get { return _id; }
    set { _id = value; }
  }

  public int ParentID
  {
    get { return _parentID; }
    set { _parentID = value; }
  }

  public bool Expanded { get { return _expanded; } }

  public LayoutElement Spacer { get { return _spacer; } }
  public Toggle Toggle { get { return GetComponentInChildren<Toggle>(); } }
  public Text Text { get { return GetComponentInChildren<Text>(); } }
  public Button ExpandToggle { get { return GetComponentInChildren<Button>(); } }

  public int IndentLevel
  {
    get { return _indentLevel; }
    set
    {
      _indentLevel = System.Math.Max(value, 0);
      if (_spacer != null)
      {
        _spacer.minWidth = _indentLevel * IndentWidth;
      }
    }
  }

  public void ToggleExpand()
  {
    _expanded = !_expanded;
    ExpandToggle.image.sprite = (_expanded) ? _contractImage : _expandImage;
    if (OnExpandChange != null)
    {
      OnExpandChange(this, _expanded);
    }
  }

  [SerializeField]
  LayoutElement _spacer = null;
  [SerializeField]
  Sprite _expandImage = null;
  [SerializeField]
  Sprite _contractImage = null;
  private string _categoryName = "";
  private int _id = 0;
  private int _parentID = 0;
  [SerializeField, Range(0, 50)]
  private int _indentWidth = 20;
  private int _indentLevel = 0;
  private bool _expanded = false;
}
