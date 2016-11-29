using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tes.Handlers
{
  /// <summary>
  /// Caches the active non-transient shapes, supporting fetching by ID.
  /// </summary>
  public class ShapeCache
  {
    /// <summary>
    /// Enumerate active objects in the cache.
    /// </summary>
    public IEnumerable<GameObject> Objects
    {
      get
      {
        foreach (GameObject obj in _map.Values)
        {
          yield return obj;
        }
      }
    }
    
    /// <summary>
    /// Add an object to the cache.
    /// </summary>
    /// <param name="id">The object ID.</param>
    /// <param name="obj">The associated object.</param>
    /// <returns>True when no object using <paramref name="id"/> preexisted. When false, 
    /// <paramref name="obj"/> is not added.</returns>
    public bool Add(uint id, GameObject obj)
    {
      if (!_map.ContainsKey(id))
      { 
        _map.Add(id, obj);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Retrieve the object with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The ID of the object of interest.</param>
    /// <returns>The requested object or null when not found.</returns>
    public GameObject Fetch(uint id)
    {
      GameObject obj;
      _map.TryGetValue(id, out obj);
      return obj;
    }

    /// <summary>
    /// Remove the object with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The ID of the object to remove.</param>
    /// <returns>The removed object or null when not found.</returns>
    public GameObject Remove(uint id)
    {
      GameObject obj = Fetch(id);
      if (obj != null)
      {
        _map.Remove(id);
      }
      return obj;
    }

    /// <summary>
    /// Remove all object (and destroy).
    /// </summary>
    public void Reset()
    {
      foreach (GameObject obj in _map.Values)
      {
        UnityEngine.Object.Destroy(obj);
      }
      _map.Clear();
    }

    private Dictionary<uint, GameObject> _map = new Dictionary<uint, GameObject>();
  }
}
