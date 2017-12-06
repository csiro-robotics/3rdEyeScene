using UnityEngine;
using System;
using System.Collections.Generic;
using Tes.Runtime;

namespace UI
{
  public class ObjectsPanel : MonoBehaviour
  {
    public CategoryItem ObjectItemUI = null;
    public TreeView ObjectsTree = null;
    public Properties.PropertiesView PropertiesView = null;
    public TesComponent Tes = null;

    public delegate IEnumerable<GameObject> HandlerObjectEnumerator(MessageHandler handler);

    void OnEnable()
    {
      if (Tes != null)
      {
        Populate();
      }
    }

    void OnDisable()
    {
      if (Tes != null)
      {
      }
    }


    void Populate()
    {
      if (ObjectsTree != null && ObjectItemUI != null)
      {
        // Remove spacer to re add at the end.
        if (_spacer != null)
        {
          _spacer.SetParent(null, false);
        }

        // Clear existing.
        ObjectsTree.Clear();

        foreach (MessageHandler handler in Tes.Handlers.Handlers)
        {
          TreeViewItem treeItem = CreateTreeItem(handler, false);
          ObjectsTree.Root.AddChild(treeItem);
        }

        // Restore Spacer.
        //if (_spacer != null)
        //{
        //  _spacer.SetParent(TreeUI.transform, false);
        //}
      }
    }

    protected TreeViewItem CreateTreeItem(MessageHandler handler, bool expanded)
    {
      CategoryItem visual = Instantiate(ObjectItemUI);
      visual.CategoryName = visual.gameObject.name = handler.Name;
      visual.Toggle.isOn = true;
      // visual.ID = category.ID;
      // visual.ParentID = category.ParentID;
      TreeViewItem treeItem = new TreeViewItem(visual.CategoryName,
                                               visual.gameObject.transform as RectTransform,
                                               visual.Spacer, handler);

      foreach (GameObject obj in handler.Objects)
      {
        TreeViewItem childItem = CreateTreeItem(obj);
        treeItem.AddChild(childItem);
      }

      if (expanded)
      {
        treeItem.Expand();
      }
      treeItem.Expander = visual.ExpandToggle;
      return treeItem;
    }

    protected TreeViewItem CreateTreeItem(GameObject obj)
    {
      CategoryItem visual = Instantiate(ObjectItemUI);
      visual.CategoryName = visual.gameObject.name = obj.name;
      visual.Toggle.isOn = true;
      // visual.ID = category.ID;
      // visual.ParentID = category.ParentID;
      TreeViewItem treeItem = new TreeViewItem(visual.CategoryName,
                                               visual.gameObject.transform as RectTransform,
                                               visual.Spacer, obj);
      treeItem.Expander = visual.ExpandToggle;
      return treeItem;
    }
    protected void ClearView()
    {
      ObjectsTree.Clear();
    }

    private RectTransform _spacer = null;
  }
}