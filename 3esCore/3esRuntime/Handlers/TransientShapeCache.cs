using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tes.Handlers
{
  struct ShapeTransformPair
  {
    public Matrix4x4 Transform;
    public ObjectAttributes Shape;
  }

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
      _cache = new ShapeTransformPair[_initialCapacity];
    }

    /// <summary>
    /// Access the activated transient object.
    /// </summary>
    /// <returns></returns>
    public ShapeTransformPair LastObject
    {
      get
      {
        return _lastObject;
      }
    }

    /// <summary>
    /// Iterate the active objects (active only).
    /// </summary>
    public IEnumerable<ShapeTransformPair> Objects
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
    /// <param name="shape">The new transient shape.</param>
    /// <return>The index of the added shape.</remarks>
    public int Add(ShapeTransformPair shape, Matrix4x4 transform)
    {
      if (_cachedCount == _cache.Length)
      {
        Array.Resize(ref _cache, _cache.Length * 2);
      }

      _lastObjectIndex = _cachedCount;
      _cache[_cachedCount] = new ShapeTransformPair { Shape = shape, Transform = transform };
      ++_activeCount;
      return _cachedCount++;
    }

    /// <summary>
    /// Deactivate all objects.
    /// </summary>
    public void Reset()
    {
      _activeCount = 0;
    }

    private ShapeTransformPair[] _shapes;
    private Matrix4x4[] _transforms;
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
    private int _lastObjectIndex = -1;
  }
}
