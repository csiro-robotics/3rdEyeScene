using UnityEngine;
using UnityEngine.UI;
using Tes.Handlers;
using Tes.Net;

namespace UI
{
  public class CategoriesPanel : MonoBehaviour
  {
    public CategoryItem CategoryItemUI = null;
    public TreeView TreeUI = null;
    public TesComponent Tes = null;

    void OnEnable()
    {
      if (Tes != null)
      {
        _categories = Tes.GetHandler((ushort)RoutingID.Category) as CategoriesHandler;
        if (_categories != null)
        {
          Populate();
          _categories.OnNewCategory += this.OnNewCategory;
          _categories.OnClearCategories += this.ClearCategories;
          _categories.OnActivationChange += this.OnActivationChange;
        }
      }
    }

    void OnDisable()
    {
      if (Tes != null)
      {
        if (_categories != null)
        {
          _categories.OnNewCategory -= this.OnNewCategory;
          _categories.OnClearCategories -= this.ClearCategories;
          _categories.OnActivationChange -= this.OnActivationChange;
        }
      }
    }


    void Populate()
    {
      if (TreeUI != null && CategoryItemUI != null && _categories != null)
      {
        // Remove spacer to re add at the end.
        if (_spacer != null)
        {
          _spacer.SetParent(null, false);
        }

        // Clear existing.
        TreeUI.Clear();

        // Add the default if available. It is skipped later.
        if (_categories != null && FindCategoryTreeItem(TreeUI.Root, 0) == null)
        {
          CategoriesHandler.Category defaultCat = _categories.Lookup(0);
          if (defaultCat != null)
          {
            TreeViewItem treeItem = CreateTreeItem(defaultCat, false);
            TreeUI.Root.AddChild(treeItem);
          }
        }
        PopulateChildren(TreeUI.Root, 0);

        // Restore Spacer.
        //if (_spacer != null)
        //{
        //  _spacer.SetParent(TreeUI.transform, false);
        //}
      }
    }

    protected TreeViewItem CreateTreeItem(CategoriesHandler.Category category, bool expanded)
    {
      CategoryItem visual = Instantiate(CategoryItemUI);
      visual.CategoryName = visual.gameObject.name = category.Name;
      visual.Toggle.isOn = category.Active;
      visual.ID = category.ID;
      visual.ParentID = category.ParentID;
      visual.Toggle.isOn = !expanded;
      visual.Toggle.onValueChanged.AddListener((bool on) => OnToggle(visual, on));
      TreeViewItem treeItem = new TreeViewItem(visual.CategoryName,
                                               visual.gameObject.transform as RectTransform,
                                               visual.Spacer, category.ID);
      if (expanded)
      {
        treeItem.Expand();
      }
      treeItem.Expander = visual.ExpandToggle;
      return treeItem;
    }

    /// <summary>
    /// Add child items for <paramref name="item"/>.
    /// </summary>
    /// <param name="item"></param>
    protected void PopulateChildren(TreeViewItem item, ushort parentId)
    {
      TreeViewItem treeItem;
      bool startExpanded = false;
      foreach (CategoriesHandler.Category category in _categories.Categories)
      {
        // Populate only root level items.
        // Don't add the default in this way.
        if (category.ID != 0 && category.ParentID == parentId)
        {
          if (FindCategoryTreeItem(TreeUI.Root, category.ID) == null)
          {
            startExpanded = WantExpanded(category);
            treeItem = CreateTreeItem(category, startExpanded);

            // This test is mostly for dealing with the default/root (0).
            if (parentId != category.ID)
            {
              PopulateChildren(treeItem, category.ID);
            }
            item.AddChild(treeItem);
          }
        }
      }
    }

    /// <summary>
    /// Check if <paramref name="category"/> should start expanded.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    /// <remarks>
    /// Uses cached settings.
    /// </remarks>
    protected bool WantExpanded(CategoriesHandler.Category category)
    {
      return false;
    }

    /// <summary>
    /// Callback on expand/contract request for an item.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="on"></param>
    protected void OnToggle(CategoryItem item, bool on)
    {
      if (item != null && _categories != null)
      {
        _categories.SetActive((ushort)item.ID, on);
      }
    }

    protected void OnNewCategory(CategoriesHandler.Category category)
    {
      TreeViewItem treeItem = FindCategoryTreeItem(TreeUI.Root, category.ID);
      // Clear existing item in case it's being renamed.
      if (treeItem != null)
      {
        treeItem.Parent.RemoveChild(treeItem);
        //GameObject.Destroy(treeItem);
        treeItem = null;
      }
      // Find the parent and add new.
      TreeViewItem parent = (category.ParentID != 0) ? FindCategoryTreeItem(TreeUI.Root, category.ParentID) : TreeUI.Root;
      // Parent may be null if categories are not sent in hierarchical order. That's fine.
      // We will add this item when the parent is populated.
      if (parent != null)
      {
        treeItem = CreateTreeItem(category, WantExpanded(category));
        PopulateChildren(treeItem, category.ID);
        parent.AddChild(treeItem);
      }
    }

    protected void OnActivationChange(ushort categoryId, bool active)
    {
      TreeViewItem treeItem = FindCategoryTreeItem(TreeUI.Root, categoryId);
      if (treeItem != null)
      {
        CategoryItem catItem = treeItem.Visual.GetComponent<CategoryItem>();
        if (catItem != null)
        {
          catItem.Toggle.isOn = active;
        }
      }
    }

    protected TreeViewItem FindParent(CategoriesHandler.Category category)
    {

      return null;
    }

    protected TreeViewItem FindCategoryTreeItem(TreeViewItem parent, ushort catId)
    {
      ushort itemCat;
      TreeViewItem item;
      for (int i = 0; i < parent.ChildCount; ++i)
      {
        item = parent.GetChild(i);
        itemCat = (ushort)item.Tag;
        if (itemCat == catId)
        {
          return item;
        }

        item = FindCategoryTreeItem(item, catId);
        if (item != null)
        {
          return item;
        }
      }

      return null;
    }

    protected void ClearCategories()
    {
      TreeUI.Clear();
    }

    private RectTransform _spacer = null;
    private CategoriesHandler _categories = null;
  }
}
