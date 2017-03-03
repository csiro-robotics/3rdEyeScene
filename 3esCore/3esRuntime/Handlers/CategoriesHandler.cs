using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Main;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers
{

  /// <summary>
  /// Maintains camera objects which may be used to view the scene.
  /// </summary>
  /// <remarks>
  /// Camera objects represent predetermined views into the scene. A camera object is
  /// really just a reference transform which the scene camera may optionally follow.
  /// 
  /// Camera objects are implicitly created when a message with a new camera ID arrives.
  /// A camera object is never destroyed and may only be updated with a new message.
  /// </remarks>
  public class CategoriesHandler : MessageHandler
  {
    /// <summary>
    /// Category details.
    /// </summary>
    public class Category
    {
      /// <summary>
      /// Display name.
      /// </summary>
      public string Name;
      /// <summary>
      /// ID
      /// </summary>
      public ushort ID;
      /// <summary>
      /// Parent category ID: zero for none.
      /// </summary>
      public ushort ParentID;
      /// <summary>
      /// Is the category active?
      /// </summary>
      public bool Active;

      /// <summary>
      /// Shallow copy implementation.
      /// </summary>
      /// <returns></returns>
      public Category ShallowCopy()
      {
        return (Category)MemberwiseClone();
      }
    }

    /// <summary>
    /// Delegate for receiving a message defining a new category.
    /// </summary>
    /// <param name="category">The new category details.</param>
    public delegate void NewCategoryDelegate(Category category);
    /// <summary>
    /// Delegate for clearing/resetting all categories.
    /// </summary>
    public delegate void ClearCategoriesDelegate();
    /// <summary>
    /// Delegate for changes to the active state of a category.
    /// </summary>
    /// <param name="categoryId">The category changing state.</param>
    /// <param name="active">True when becoming active.</param>
    public delegate void ActivationChangeDelegate(ushort categoryId, bool active);

    /// <summary>
    /// Invoked on receiving a message defining a new category.
    /// </summary>
    public event NewCategoryDelegate OnNewCategory;
    /// <summary>
    /// Invoked on clearing/resetting all categories.
    /// </summary>
    public event ClearCategoriesDelegate OnClearCategories;
    /// <summary>
    /// Invoked on changes to the active state of a category.
    /// </summary>
    public event ActivationChangeDelegate OnActivationChange;

    /// <summary>
    /// Constructor initialising the persistent and transient caches.
    /// </summary>
    public CategoriesHandler()
      : base(null)
    {
      //UpdateServerInfo(ServerInfoMessage.Default);

      //AddTest();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Categories"; } }
    /// <summary>
    /// Routing ID.
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Net.RoutingID.Category; } }

    /// <summary>
    /// Add a category to the list of known categories.
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="parentId">The parent ID. Zero is none (even though 0 is a valid category).</param>
    /// <param name="name">The category display name.</param>
    /// <param name="active">The default active state.</param>
    public void AddCategory(ushort id, ushort parentId, string name, bool active)
    {
      Category cat = new Category();
      cat.ID = id;
      cat.ParentID = parentId;
      cat.Name = name;
      cat.Active = active;
      _categories[cat.ID] = cat;
      NotifyNewCategory(cat.ShallowCopy());
    }

    /// <summary>
    /// Lookup and retrieve details of a category.
    /// </summary>
    /// <param name="id">The category to lookup.</param>
    /// <returns>A shallow copy of the category on success, null on failure.</returns>
    public Category Lookup(ushort id)
    {
      Category cat = null;
      if (_categories.TryGetValue(id, out cat))
      {
        // Prevent mutation.
        return cat.ShallowCopy();
      }

      return cat;
    }

    /// <summary>
    /// Enumerates the known categories.
    /// </summary>
    public IEnumerable<Category> Categories
    {
      get
      {
        foreach (Category cat in _categories.Values)
        {
          yield return cat;
        }
      }
    }

    /// <summary>
    /// Enumerates categories which are children of the category with <paramref name="id"/>
    /// </summary>
    /// <param name="id">The category ID to enumerate the children of.</param>
    public IEnumerable<Category> ChildCategories(ushort id)
    {
      foreach (Category cat in _categories.Values)
      {
        if (cat.ParentID == id && cat.ID != id)
        {
          yield return cat;
        }
      }
    }

    /// <summary>
    /// Check if a category is active.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>True if active or unknown/</returns>
    public bool IsActive(ushort id)
    {
      Category cat = null;
      if (_categories.TryGetValue(id, out cat))
      {
        return cat.Active;
      }
      // Unregistered categories are all active.
      return true;
    }

    /// <summary>
    /// Set the active state of a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="active">The desired active state.</param>
    /// <remarks>
    /// This invokes the <see cref="OnActivationChange"/> event when <paramref name="active"/>
    /// does not match the category state.
    /// 
    /// Unknown <paramref name="id"/> values are ignored.
    /// </remarks>
    public void SetActive(ushort id, bool active)
    {
      Category cat = null;
      if (_categories.TryGetValue(id, out cat))
      {
        if (cat.Active != active)
        {
          cat.Active = active;
          if (OnActivationChange != null)
          {
            OnActivationChange(id, active);
          }
          SetChildrenActive(id, active);
        }
      }
    }

    /// <summary>
    /// Set the active state of all descendants of the category identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The parent category ID.</param>
    /// <param name="active">The new active state.</param>
    private void SetChildrenActive(ushort id, bool active)
    {
      // Also affect all children.
      foreach (Category cat in ChildCategories(id))
      {
        if (cat.Active != active)
        {
          cat.Active = active;
          if (OnActivationChange != null)
          {
            OnActivationChange(cat.ID, active);
          }
        }
        SetChildrenActive(cat.ID, active);
      }
    }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <param name="maintainTransient"></param>
    public override void BeginFrame(uint frameNumber, bool maintainTransient)
    {
    }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="frameNumber"></param>
    public override void EndFrame(uint frameNumber)
    {
    }

    /// <summary>
    /// Initialise the shape handler by initialising the shape scene root and
    /// fetching the default materials.
    /// </summary>
    /// <param name="root">The 3rd Eye Scene root object.</param>
    /// <param name="serverRoot">The server scene root (transformed into the server reference frame).</param>
    /// <param name="materials">Material library from which to resolve materials.</param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      // FIXME: localisation.
      AddCategory(0, 0, "Default", true);
    }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="info"></param>
    public override void UpdateServerInfo(ServerInfoMessage info)
    {
    }

    /// <summary>
    /// Clear all current objects.
    /// </summary>
    public override void Reset()
    {
      _categories.Clear();
      if (OnClearCategories != null)
      {
        OnClearCategories();
      }
      // FIXME: localisation.
      AddCategory(0, 0, "Default", true);
    }

    /// <summary>
    /// The primary message handling function.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public override Error ReadMessage(PacketBuffer packet, BinaryReader reader)
    {
      switch (packet.Header.MessageID)
      {
      case (ushort)CategoryMessageID.Name:
        CategoryNameMessage msg = new CategoryNameMessage();
        if (!msg.Read(reader))
        {
          return new Error(ErrorCode.MalformedMessage);
        }

        AddCategory(msg.CategoryID, msg.ParentID, msg.Name, msg.DefaultActive);
        break;
      default:
        return new Error(ErrorCode.InvalidMessageID, packet.Header.MessageID);
      }

      return new Error();
    }

    /// <summary>
    /// Serialises the currently active objects in for playback from file.
    /// </summary>
    /// <param name="writer">The write to serialise to.</param>
    /// <param name="info">Statistics</param>
    /// <returns>An error code on failure.</returns>
    public override Error Serialise(BinaryWriter writer, ref SerialiseInfo info)
    {
      Error err = new Error();
      info.TransientCount = info.PersistentCount = 0u;

      PacketBuffer packet = new PacketBuffer(1024);
      CategoryNameMessage msg = new CategoryNameMessage();
      foreach (Category cat in _categories.Values)
      {
        ++info.PersistentCount;
        msg.CategoryID = cat.ID;
        msg.ParentID = cat.ParentID;
        msg.DefaultActive = cat.Active;
        msg.Name = cat.Name;
        packet.Reset(RoutingID, CategoryNameMessage.MessageID);
        msg.Write(packet);
        packet.FinalisePacket();
        packet.ExportTo(writer);
      }

      return err;
    }

    /// <summary>
    /// Empty: this event is sourced from this class.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="active"></param>
    public override void OnCategoryChange(ushort categoryId, bool active)
    {
      // Ignore. Sourced from here.
    }

    /// <summary>
    /// Invoke <see cref="OnNewCategory"/>
    /// </summary>
    /// <param name="category">The new category details.</param>
    protected void NotifyNewCategory(Category category)
    {
      if (OnNewCategory != null)
      {
        OnNewCategory(category);
      }
    }

    private Dictionary<ushort, Category> _categories = new Dictionary<ushort, Category>();
  }
}
