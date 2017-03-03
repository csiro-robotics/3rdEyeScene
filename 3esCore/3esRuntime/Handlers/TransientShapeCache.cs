using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tes.Handlers
{
  /// <summary>
  /// A cache for transient shape objects.
  /// </summary>
  /// <remarks>
  /// The intended usage is to create shape objects as required, then register them
  /// with the cache. The cache ensures they are remove from the scene when
  /// <see cref="Reset()"/> is called. However, the objects are not destroyed and
  /// may be re-used by requesting an existing object calling <see cref="Fetch()"/>.
  /// This fails if no cached objects are available, in which case a new object
  /// should be registered using <see cref="Add(GameObject)"/>.
  /// </remarks>
  public class TransientShapeCache
  {
    /// <summary>
    /// Default number of objects to start with.
    /// </summary>
    public const int DefaultCacheSize = 512;

    /// <summary>
    /// The number of cached objects. Fewer objects may be active.
    /// </summary>
    public int Count { get { return _cachedCount; } }

    /// <summary>
    /// Create a new cache starting with the default number of objects.
    /// </summary>
    public TransientShapeCache() : this(DefaultCacheSize) {}

    /// <summary>
    /// Create a new cache starting with the given initial number of objects.
    /// </summary>
    /// <param name="initialCapacity">The initial number of objects.</param>
    public TransientShapeCache(int initialCapacity)
    {
      _initialCapacity = initialCapacity;
      _cache = new GameObject[_initialCapacity];
    }

    /// <summary>
    /// Access the activated transient object.
    /// </summary>
    /// <returns></returns>
    public GameObject LastObject
    {
      get
      {
        return _lastObject;
      }
    }

    /// <summary>
    /// Iterate the active objects (active only).
    /// </summary>
    public IEnumerable<GameObject> Objects
    {
      get
      {
        for (int i = 0; i < _activeCount; ++i)
        {
          yield return _cache[i];
        }
      }
    }

    /// <summary>
    /// Add a transient object to the cache.
    /// </summary>
    /// <param name="obj">The new transient object.</param>
    public void Add(GameObject obj)
    {
      if (_cachedCount == _cache.Length)
      {
        Array.Resize(ref _cache, _cache.Length * 2);
      }

      _cache[_cachedCount++] = obj;
      _lastObject = obj;
      ++_activeCount;
    }


    /// <summary>
    /// Fetch and reuse an existing transient object from the cache.
    /// </summary>
    /// <returns>A new transient object</returns>
    public GameObject Fetch()
    {
      if (_activeCount < _cachedCount)
      {
        return (_lastObject = _cache[_activeCount++]);
      }

      return null;
    }

    /// <summary>
    /// Deactivate all objects.
    /// </summary>
    public void Reset()
    {
      // De-activate objects.
      for (int i = 0; i < _activeCount; ++i)
      {
        _cache[i].SetActive(false);
      }
      _activeCount = 0;
      _lastObject = null;
    }

    /// <summary>
    /// Reset the cache to have no active objects, optinally deleting the objects.
    /// </summary>
    /// <param name="deleteObjects">True to delete the objects.</param>
    public void Reset(bool deleteObjects)
    {
      if (!deleteObjects)
      {
        Reset();
        return;
      }

      // Delete the game objects and reset the array.
      for (int i = 0; i < _cachedCount; ++i)
      {
        GameObject obj = _cache[i];
        _cache[i] = null;
        UnityEngine.Object.Destroy(obj);
      }
      _cachedCount = 0;

      //if (_cache.Length != _initialCapacity)
      //{
      //  _cache = new GameObject[_initialCapacity];
      //}
      _activeCount = 0;
      _lastObject = null;
    }


    private GameObject[] _cache;
    /// <summary>
    ///  Number of valid cached pointers in _cache.
    /// </summary>
    private int _cachedCount = 0;
    /// <summary>
    /// Number of cached objects currently in use. Always less than or equal to _cachedCount
    /// </summary>
    private int _activeCount = 0;
    private int _initialCapacity;
    /// <summary>
    /// The last object created by the cache.
    /// </summary>
    private GameObject _lastObject = null;
  }
}
