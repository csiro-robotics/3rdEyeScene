using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
  /// <summary>
  /// An extension to a <c>ScrollRect</c> which supports a tree view.
  /// </summary>
  /// <remarks>
  /// The <see cref="TreeViewItem"/> visual must support a means of expansion which it
  /// manages itself.
  /// </remarks>
  [RequireComponent(typeof(ScrollRect))]
  public class TreeView : MonoBehaviour
  {
    /// <summary>
    /// The <c>ScrollRect</c> visual.
    /// </summary>
    public ScrollRect ScrollView
    {
      get
      {
        if (_visual == null)
        {
          _visual = GetComponent<ScrollRect>();
        }
        return _visual;
      }
    }

    /// <summary>
    /// Indent this many UI units for each depth step in the tree.
    /// </summary>
    [Range(0, 100)]
    public int Indent = 20;

    /// <summary>
    /// Get the root item for this tree. This item belongs to the.
    /// </summary>
    /// <remarks>
    /// The root should not be modified, other than to add/remove children.
    /// </remarks>
    public TreeViewItem Root
    {
      get
      {
        if (_root == null)
        {
          _root = new TreeViewItem("<root>", null, null);
          _root.OnChildAdded += OnAdd;
          _root.OnChildRemoved += OnRemove;
        }
        return _root;
      }
    }

    protected RectTransform Spacer
    {
      get
      {
        // Property due to access ordering.
        if (_spacer == null)
        {
          _spacer = CreateSpacer();
        }
        return _spacer;
      }
    }

    /// <summary>
    /// Clear all items in the tree.
    /// </summary>
    public void Clear()
    {
      // Prevent rebuilding the visuals.
      _propagatingBindings = true;
      Clear(Root);
      _propagatingBindings = false;
      // Update/clear the UI.
      RebuildVisuals();
    }

    private void Clear(TreeViewItem item)
    {
      // Remove backwards for better List<> management.
      TreeViewItem child;
      for (int i = item.ChildCount - 1; i >= 0; --i)
      {
        child = item.GetChild(i);
        Clear(child);
        RemoveItem(child);
        Destroy(child.Visual.gameObject);
      }
    }

    /// <summary>
    /// Add an item to the tree.
    /// </summary>
    /// <param name="rootItem">The item to add. Must not already be in a tree.</param>
    /// <exception cref="InvalidHierarchyException">Thrown when <paramref name="rootItem"/>is already in a tree.</exception>
    public void AddItem(TreeViewItem rootItem)
    {
      if (rootItem.Parent != null)
      {
        throw new InvalidHierarchyException(string.Format("Cannot add {0} as a root tree item. It already has a parent: {1}",
          rootItem.Name, rootItem.Parent.Name));
      }

      Root.AddChild(rootItem);
    }

    /// <summary>
    /// Remove a root item from this tree.
    /// </summary>
    /// <param name="rootItem">The root item to remove.</param>
    /// <returns>True if <paramref name="rootItem"/> was a root of this tree and has been removed.</returns>
    public bool RemoveItem(TreeViewItem rootItem)
    {
      return Root.RemoveChild(rootItem);
    }

    /// <summary>
    /// Handle bindings for adding an item to the tree. Must already be in <c>Root</c>.
    /// </summary>
    /// <param name="item"></param>
    private void OnAdd(TreeViewItem parent, TreeViewItem item)
    {
      bool updateVisuals = !_propagatingBindings;
      _propagatingBindings = true;
      item.OnChildAdded += OnAdd;
      item.OnChildRemoved += OnRemove;
      item.OnExpandChange += OnExpansionChange;
      item.Visual.gameObject.SetActive(false);
      item.Visual.SetParent(ScrollView.content, false);
      // Recur on children to ensure they are also bound.
      for (int i = 0; i < item.ChildCount; ++i)
      {
        OnAdd(item, item.GetChild(i));
      }

      if (updateVisuals)
      {
        _propagatingBindings = false;
        RebuildVisuals();
      }
    }

    /// <summary>
    /// Handle bindings for removing an item from the tree. Must already be removed from <c>Root</c>.
    /// </summary>
    /// <param name="item"></param>
    private void OnRemove(TreeViewItem parent, TreeViewItem item)
    {
      bool updateVisuals = !_propagatingBindings;
      _propagatingBindings = true;

      item.OnChildAdded -= OnAdd;
      item.OnChildRemoved -= OnRemove;
      item.OnExpandChange -= OnExpansionChange;
      //item.Visual.SetParent(null, false);
      item.Visual.gameObject.SetActive(false);

      // Recur on children to ensure they are also unbound.
      for (int i = 0; i < item.ChildCount; ++i)
      {
        OnRemove(item, item.GetChild(i));
      }

      if (updateVisuals)
      {
        _propagatingBindings = false;
        RebuildVisuals();
      }
    }

    /// <summary>
    /// Rebuild the list view.
    /// </summary>
    /// <remarks>
    /// Easy way out for maintaining the list view: remove all items and repopulate.
    /// </remarks>
    private void RebuildVisuals()
    {
      // Store the Y position of scroll view to restore the position?
      TreeViewItem child;
      ScrollView.content.transform.DetachChildren();
      for (int i = 0; i < Root.ChildCount; ++i)
      {
        child = Root.GetChild(i);
        AddVisuals(child, 0);
      }
      Spacer.SetParent(ScrollView.content, false);
    }

    /// <summary>
    /// Add the visual to the scroll view and recur on children.
    /// </summary>
    /// <param name="item">The item to add the visual for.</param>
    private void AddVisuals(TreeViewItem item, int depth, bool visible = true)
    {
      TreeViewItem child;
      // Add visual.
      item.Visual.gameObject.SetActive(visible);
      item.Visual.transform.SetParent(ScrollView.content, false);
      if (item.Indent != null)
      {
        item.Indent.minWidth = depth * Indent;
      }
      // Add children.
      for (int i = 0; i < item.ChildCount; ++i)
      {
        child = item.GetChild(i);
        AddVisuals(child, depth + 1, visible && item.Expanded);
      }
    }

    /// <summary>
    /// Event for changes in the expanded state.
    /// </summary>
    /// <param name="item"></param>
    private void OnExpansionChange(TreeViewItem item)
    {
      RebuildVisuals();
    }

    /// <summary>
    /// Create  UI layout object which consumes all available space.
    /// </summary>
    /// <returns></returns>
    public static RectTransform CreateSpacer()
    {
      GameObject spacer = new GameObject("Spacer");
      RectTransform rt = spacer.AddComponent<RectTransform>();
      LayoutElement layout = spacer.AddComponent<LayoutElement>();
      layout.flexibleHeight = 100;
      layout.flexibleWidth = 100;
      //layout.Flags
      return rt;
    }

    /// <summary>
    /// The root item. Never displayed. Do not access directly: use <see cref="Root"/>.
    /// </summary>
    private TreeViewItem _root = null;
    private ScrollRect _visual;
    /// <summary>
    /// Propagating event bindings in <paramref name="OnAdd"/>? For managing adding visuals.
    /// </summary>
    private bool _propagatingBindings = false;
    /// <summary>
    /// Spacer for the end of the list.
    /// </summary>
    private RectTransform _spacer;
  }
}