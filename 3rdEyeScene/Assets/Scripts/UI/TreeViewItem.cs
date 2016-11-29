using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UI
{
  /// <summary>
  /// Exception thrown when modifying <see cref="TreeViewItem"/> objects from different
  /// hierarchies.
  /// </summary>
  public class InvalidHierarchyException : Exception
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Error message.</param>
    public InvalidHierarchyException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Constructor identifying that <paramref name="parent"/> is not actually a parent of
    /// <paramref name="child"/>.
    /// </summary>
    /// <param name="parent">The wrong parent. May be null.</param>
    /// <param name="child">The child object. May be null or have a null parent.</param>
    public InvalidHierarchyException(TreeViewItem parent, TreeViewItem child, string message = null)
      : base(string.Format("{0} is not a parent of {1}. Actual parent: {2}.{3}{4}",
          parent != null ? parent.Name : "null",
          child != null ? child.Name : "null",
          child != null && child.Parent != null ? child.Parent.Name : "null",
          !string.IsNullOrEmpty(message) ? " " : "",
          !string.IsNullOrEmpty(message) ? message : ""
        ))
    {
    }
  }

  public delegate void ChildAddedDelegate(TreeViewItem parent, TreeViewItem child);
  public delegate void ChildRemovedDelegate(TreeViewItem parent, TreeViewItem child);
  public delegate void ExpandedChangeDelegate(TreeViewItem item);
  //public delegate void OnParentChanged(TreeViewItem parent, TreeViewItem child);

  /// <summary>
  /// An entry in a <see cref="TreeView"/>.
  /// </summary>
  public class TreeViewItem
  {
    /// <summary>
    /// Reference name. Not displayed.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The visual for the tree item.
    /// </summary>
    public RectTransform Visual { get; set; }
    /// <summary>
    /// A spacing component used to indent items in the tree view.
    /// </summary>
    public LayoutElement Indent { get; set; }
    /// <summary>
    /// Expander UI button to disable when no children, review when there are children.
    /// </summary>
    public Button Expander
    {
      get { return _expander; }
      set
      {
        if (_expander != null)
        {
          value.onClick.RemoveListener(ToggleExpansion);
        }
        _expander = value;
        if (value != null)
        {
          value.onClick.AddListener(ToggleExpansion);
          UpdateExpanderVisual();
        }
      }
    }
    /// <summary>
    /// Object associated with the item.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// The parent item.
    /// </summary>
    public TreeViewItem Parent { get; protected set; }

    /// <summary>
    /// Event for new children.
    /// </summary>
    public event ChildAddedDelegate OnChildAdded;
    /// <summary>
    /// Event for removed children.
    /// </summary>
    public event ChildRemovedDelegate OnChildRemoved;

    /// <summary>
    /// Event for expansion state change.
    /// </summary>
    public event ExpandedChangeDelegate OnExpandChange;

    /// <summary>
    /// Count the number of child items.
    /// </summary>
    public int ChildCount { get { return _children.Count; } }

    /// <summary>
    /// Is this item expanded?
    /// </summary>
    /// <remarks>
    /// Also controls the initial expansion state on adding to a tree.
    /// </remarks>
    public bool Expanded { get; protected set; }

    /// <summary>
    /// The index of this item in its parent.
    /// </summary>
    public int IndexInParent
    {
      get
      {
        if (Parent != null)
        {
          return Parent.IndexOf(this);
        }
        return -1;
      }
    }

    /// <summary>
    /// Create a new tree item.
    /// </summary>
    /// <param name="name">Reference name. Not displayed.</param>
    /// <param name="visual">Visuals</param>
    /// <param name="indent">Spacer/indent.</param>
    /// <param name="tag">Associated object</param>
    public TreeViewItem(string name, RectTransform visual, LayoutElement indent, object tag = null)
    {
      Name = name;
      Visual = visual;
      Indent = indent;
      Tag = tag;
    }

    /// <summary>
    /// Remove this item from its parent.
    /// </summary>
    /// <returns>True if the item had a parent and has been removed.</returns>
    public bool RemoveFromParent()
    {
      if (Parent != null)
      {
        return Parent.RemoveChild(this);
      }
      return false;
    }

    /// <summary>
    /// Add a child to this item.
    /// </summary>
    /// <param name="child"></param>
    /// <remarks>
    /// Removes <paramref name="child"/> from its current parent first.
    /// </remarks>
    public void AddChild(TreeViewItem child)
    {
      child.RemoveFromParent();
      _children.Add(child);
      child.Parent = this;
      UpdateExpanderVisual();
      NodifyChildAdded(child);
    }

    /// <summary>
    /// Add a child positioned before the <paramref name="before"/> object.
    /// </summary>
    /// <param name="before">The object to position before. Must be null or a child of this object.</param>
    /// <param name="child">The item to add.</param>
    /// <remarks>
    /// Removes <paramref name="child"/> from its current parent first.
    /// Adds as the first child if <paramref name="before"/> is null.
    /// </remarks>
    /// <exception cref="InvalidHierarchyException">Thrown when <paramref name="after"/> is not a child of this item.</exception>
    public void AddChildBefore(TreeViewItem before, TreeViewItem child)
    {
      if (before != null && before.Parent != this)
      {
        throw new InvalidHierarchyException(this, before, "Cannot insert before.");
      }

      child.RemoveFromParent();
      if (before != null)
      {
        _children.Insert(before.IndexInParent, child);
      }
      else
      {
        _children.Insert(0, child);
      }
      child.Parent = this;
      UpdateExpanderVisual();
      NodifyChildAdded(child);
    }

    /// <summary>
    /// Add a child positioned after the <paramref name="after"/> object.
    /// </summary>
    /// <param name="after">The object to position after. Must be null or a child of this object.</param>
    /// <param name="child">The item to add.</param>
    /// <remarks>
    /// Removes <paramref name="child"/> from its current parent first.
    /// Adds as the last child if <paramref name="after"/> is null.
    /// </remarks>
    /// <exception cref="InvalidHierarchyException">Thrown when <paramref name="after"/> is not a child of this item.</exception>
    public void AddChildAfter(TreeViewItem after, TreeViewItem child)
    {
      if (after.Parent != this)
      {
        throw new InvalidHierarchyException(this, after, "Cannot insert after.");
      }

      child.RemoveFromParent();
      int idx = IndexOf(after);
      if (idx >= 0 && idx + 1 < _children.Count)
      {
        _children.Insert(idx + 1, child);
      }
      else
      {
        _children.Add(child);
      }
      child.Parent = this;
      UpdateExpanderVisual();
      NodifyChildAdded(child);
    }

    /// <summary>
    /// Get the child at the given index.
    /// </summary>
    /// <param name="index">The item index [0, <see cref="ChildCount"/>). Must be in range.</param>
    /// <returns>The requested item.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/> is out of range.</exception>
    public TreeViewItem GetChild(int index)
    {
      return _children[index];
    }

    /// <summary>
    /// Remove <paramref name="child"/> from this item.
    /// </summary>
    /// <param name="child">The item to remove. May be null or not a child of this item.</param>
    /// <returns>True if <paramref name="child"/> was a child of this item and has been removed.</returns>
    public bool RemoveChild(TreeViewItem child)
    {
      if (child.Parent == this)
      {
        int idx = _children.IndexOf(child);
        if (idx >= 0)
        {
          _children.RemoveAt(idx);
          child.Parent = null;
          UpdateExpanderVisual();
          if (OnChildRemoved != null)
          {
            OnChildRemoved(this, child);
          }
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Search for <paramref name="child"/> and return its index.
    /// </summary>
    /// <param name="child">The child item to search for.</param>
    /// <returns>The index of <paramref name="child"/> or -1 when <paramref name="child"/> is
    /// null or has a different parent./// </returns>
    public int IndexOf(TreeViewItem child)
    {
      return _children.IndexOf(child);
    }

    /// <summary>
    /// Contract if expanded, or expand if contracted.
    /// </summary>
    public void SetExpanded(bool expand)
    {
      if (expand != Expanded)
      {
        ToggleExpansion();
      }
    }

    /// <summary>
    /// Contract if expanded, or expand if contracted.
    /// </summary>
    public void ToggleExpansion()
    {
      Expanded = !Expanded;
      if (OnExpandChange != null)
      {
        OnExpandChange(this);
      }
    }

    /// <summary>
    /// Expand the item visual.
    /// </summary>
    public void Expand()
    {
      if (!Expanded)
      {
        ToggleExpansion();
      }
    }

    /// <summary>
    /// Contract the item visual.
    /// </summary>
    public void Contract()
    {
      if (Expanded)
      {
        ToggleExpansion();
      }
    }

    /// <summary>
    /// Update the active/visual state of <see cref="Expander"/> to reflect the presence or absence of children.
    /// </summary>
    public void UpdateExpanderVisual()
    {
      if (_expander != null)
      {

        _expander.interactable = _children.Count != 0;
      }
    }

    /// <summary>
    /// Helper for firing <paramref name="OnChildAdded"/>.
    /// </summary>
    /// <param name="child">The added child.</param>
    protected void NodifyChildAdded(TreeViewItem child)
    {
      if (OnChildAdded != null)
      {
        OnChildAdded(this, child);
      }
    }

    private List<TreeViewItem> _children = new List<TreeViewItem>();
    private Button _expander = null;
  }
}
