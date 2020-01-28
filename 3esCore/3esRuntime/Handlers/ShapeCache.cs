using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tes.Exception;
using UnityEngine;

namespace Tes.Handlers
{
  /// <summary>
  /// Caches the active non-transient shapes, supporting fetching by ID.
  /// </summary>
  /// <remarks>
  /// The paradigm for storing object data in the shape cache is similar to the new Unity ECS pattern. That system
  /// has been ruled out here because Unity does not currently ship assembly DLLs which can be added as references.
  /// Rather Unity ECS is provided as source built in the editor only.
  ///
  /// Unlike Unity ECS, this supports non blittable types.
  /// </remarks>
  public class ShapeCache
  {
    /// <summary>
    /// Create a new shape cache.
    /// </summary>
    /// <param name="initialCapacity">Sets the initial storage capcity. Use zero to set the default capacity.</param>
    /// <param name="transient">True to create a transient cache.</param>
    public ShapeCache(int initialCapacity, bool transient)
    {
      _capacity = (initialCapacity > 0) ? initialCapacity : 128;
      _shapes = new CreateMessage[_capacity];
      _transforms = new Matrix4x4[_capacity];
      if (!transient)
      {
        // Non-transient cache. Use the ID map.
        _idMap = new Dictionary<uint, int>();
        _freeList = new List<int>();
      }
    }

    /// <summary>
    /// Enumerate the indices of all active objects. The results be used to retrieve data with
    /// <see cref="GetShapeDataByIndex(int)"/>
    /// </summary>
    public IEnumerable<int> ShapeIndices
    {
      get
      {
        int itemLimit = (IsTransientCache) ? _currentCount : _capacity;
        for (int i = 0; i < itemLimit; ++i)
        {
          CreateMessage shape = _shapes[i];
          // Check ID and category. A transient cache has all IDs set to zero. A non-transient cache has all valid IDs
          // as non-zero.
          if (IsTransientCache || shape.ObjectID != 0)
          {
            yield return i;
          }
        }
      }
    }

    /// <summary>
    /// Is this a transient cache?
    /// </summary>
    /// <remarks>
    /// A transient shape cache does not support queries by ID, expecting every shape to have an ID of zero.
    /// Such a cache must be periodically <see cref="Reset()"/>, nominally every data frame.
    /// </remarks>
    public bool IsTransientCache
    {
      get { return _idMap == null; }
    }

    /// <summary>
    /// Destroy all current shapes, clearing the cache.
    /// </summary>
    public void Reset()
    {
      if (_freeList != null)
      {
        _freeList.Clear();
      }
      if (_idMap != null)
      {
        _idMap.Clear();
      }
      _currentCount = 0;
    }

    /// <summary>
    /// Add additional data to store for each shape in this cache.
    /// </summary>
    /// <typeparam name="T">The data type to store.</typeparam>
    public void AddExtensionType<T>()
    {
      _dataExtensions.Add(new DataExtension
      {
        DataType = typeof(T),
        Elements = new T[_capacity]
      });
    }

    /// <summary>
    /// Create a new shape instance for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">Core data for the new shape.</param>
    /// <param name="transform">The world transform for the new shape.</param>
    /// <returns>The index value for the object. This should not be used after new shapes are added or removed</returns>
    /// <remarks>
    /// The extracted from <c>shape.ID</c>. Which must not already be present. The ID can only be zero for a
    /// transient cache.
    /// </remarks>
    /// <exception cref="DuplicateIDException">Thrown whn an object with the same ID already exists and the case is not
    /// transient.</exception>
    public int CreateShape(CreateMessage shape, Matrix4x4 transform)
    {
      int shapeIndex = AssignNewShapeIndex(shape.ID);

      _shapes[shapeIndex] = shape;
      _transforms[shapeIndex] = transform;

      return shapeIndex;
    }

    /// <summary>
    /// Attempt to resolve the index value from a shape ID.
    /// </summary>
    /// <param name="objectID">Shape ID to lookup.</param>
    /// <returns>The ID's index or -1 on failure.</returns>
    /// <remarks>
    /// The index value may become invalid after new shapes are added, shapes are removed or the cache cleared.
    /// </remarks>
    public int GetShapeIndex(uint objectID)
    {
      int index;
      if (_idMap.TryGetValue(objectID, out index))
      {
        return index;
      }

      return -1;
    }

    /// <summary>
    /// Destroy the shape instance with matching <paramref name="objectID"/>
    /// </summary>
    /// <param name="objectID">ID of the object to destroy.</param>
    /// <remarks>
    /// The call is ignored if <see cref="IsTransientCache"/> is <c>true</c> or no object matches
    /// <paramref name="objectID"/>.
    /// </remarks>
    /// <returns>The original index of the shape in the cache or -1 if the ID is invalid.</returns>
    public int DestroyShape(uint objectID)
    {
      if (IsTransientCache)
      {
        // Irrelevant for transient caches.
        return;
      }

      int index;
      if (_idMap.TryGetValue(objectID, out index))
      {
        // Clear the ObjectID so we know it's unused.
        // TODO: (KS) check this works. I can't remember if blittable type members are assignable in place like this
        // or if the whole thing needs to be written.
        _shapes[index].ObjectID = 0;
        _freeList.Add(index);
        --_currentCount;
        _idMap.Remove(objectID);
        return index;
      }

      return -1;
    }

    /// <summary>
    /// Request data for a shape instance.
    /// </summary>
    /// <param name="objectID">ID of the the shape to request. Must be valid in the cache or zero for a transient
    /// cache.</param>
    /// <param name="data">Set new data value to set</param>
    /// <typeparam name="T">Type of the data to set for the shape</typeparam>
    /// <returns>Data of the requested for the requested object.</returns>
    /// <exception cref="InvalidIDException">Thrown when <paramref name="objectID"/> is non-zero and the case is non
    /// transient.</exception>
    /// <exception cref="InvalidDataTypeException">Thrown when the type <c>T</c> does is not valid in this case.</exception>
    public T GetShapeData<T>(uint objectID)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      return GetShapeDataByIndex<T>(_idMap[objectID]);
    }

    /// <summary>
    /// Request data for a shape instance by index.
    /// </summary>
    /// <param name="index">Index of the shape to request. Must be valid and in range.</param>
    /// <param name="data">Set new data value to set</param>
    /// <returns>Data of the requested at the given index.</returns>
    /// <typeparam name="T">Type of the data to set for the shape</typeparam>
    /// <exception cref="InvalidDataTypeException">Thrown when the type <c>T</c> does is not valid in this case.</exception>
    ///
    public T GetShapeDataByIndex<T>(int index)
    {
      if (typeof(T) == typeof(CreateMessage))
      {
        return _shapes[index];
      }
      if (typeof(T) == typeof(ObjectAttributes))
      {
        return _shapes[index].Attributes;
      }
      if (typeof(T) == typeof(Matrix4x4))
      {
        return _transforms[index];
      }

      for (int i = 0; i < _dataExtensions.Count; ++i)
      {
        if (typeof(T) == _dataExtensions[i].DataType)
        {
          return ((T[])_dataExtensions[i])[index];
        }
      }

      throw new InvalidDataTypeException($"Unregistered shape extension type : {typeof(T).Name}");
    }

    /// <summary>
    /// Set data for a shape instance.
    /// </summary>
    /// <param name="objectID">ID of the the shape to request. Must be valid in the cache or zero for a transient
    /// cache.</param>
    /// <param name="data">Set new data value to set</param>
    /// <typeparam name="T">Type of the data to set for the shape</typeparam>
    /// <exception cref="InvalidIDException">Thrown when <paramref name="objectID"/> is non-zero and the case is non
    /// transient.</exception>
    /// <exception cref="InvalidDataTypeException">Thrown when the type <c>T</c> does is not valid in this case.</exception>
    public void SetShapeData<T>(uint objectID, T data)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      SetShapeDataByIndex<T>(_idMap[objectID], data);
    }

    /// <summary>
    /// Set data for a shape instance by index.
    /// </summary>
    /// <param name="index">Index of the shape to request. Must be valid and in range.</param>
    /// <param name="data">Set new data value to set</param>
    /// <typeparam name="T">Type of the data to set for the shape</typeparam>
    /// <exception cref="InvalidDataTypeException">Thrown when the type <c>T</c> does is not valid in this case.</exception>
    public void SetShapeDataByIndex<T>(int index, T data)
    {
      if (typeof(T) == typeof(CreateMessage))
      {
        _shapes[index] = data;
      }
      if (typeof(T) == typeof(ObjectAttributes))
      {
        _shapes[index].Attributes = data;;
      }
      if (typeof(T) == typeof(Matrix4x4))
      {
        _transforms[index] = data;
      }

      for (int i = 0; i < _dataExtensions.Count; ++i)
      {
        if (typeof(T) == _dataExtensions[i].DataType)
        {
          ((T[])_dataExtensions[i])[index] = data;
        }
      }

      throw new InvalidDataTypeException($"Unregistered shape extension type : {typeof(T).Name}");
    }

    /// <summary>
    /// Iterate the active shapes collecting render transforms into the provided lists.
    /// </summary>
    /// <param name="solidTransforms">Transforms for shapes to be rendered as opaque are collected here.</param>
    /// <param name="transparentTransforms">Transforms for shapes to be rendered with transparency are collected here.</param>
    /// <param name="wireframeTransforms">Transforms for shapes to be rendered as wireframe are collected here.</param>
    /// <param name="categoryMask">Mask of active categories.</param>
    /// <remarks>
    /// Only shapes in active categories as indicated by <paramref name="categoryMask"/> are collected.
    /// </remarks>
    public void CollectTransforms(List<Matrix4x4> solidTransforms, List<Matrix4x4> transparentTransforms, List<Matrix4x4> wireframeTransforms, ulong categoryMask)
    {
      // Walk the _headers and _transforms arrays directly. We can tell which indices are valid by investigating the
      // _headers[].ID value. A zero value is not in use.
      // TODO: (KS) consider storing a high water mark to limit iterating the arrays.
      bool transientCache = IsTransientCache;
      int itemLimit = (transientCache) ? _currentCount : _capacity;
      for (int i = 0; i < itemLimit; ++i)
      {
        CreateMessage shape = _shapes[i];
        // Check ID and category. A transient cache has all IDs set to zero. A non-transient cache has all valid IDs
        // as non-zero.
        if ((transientCache || shape.ObjectID) != 0 && (shape.Category == 0 || ((1ul << shape.Category) & categoryMask) != 0))
        {
          // Check add transform to either solid or wireframe lists.
          if ((shape.Flags & (ushort)Tes.Net.ObjectFlag.Wireframe) == 0)
          {
            if ((shape.Flags & (ushort)Tes.Net.ObjectFlag.Transparent) == 0)
            {
              solidTransforms.Add(_transforms[i]);
            }
            else
            {
              transparentTransforms.Add(_transforms[i]);
            }
          }
          else
          {
            wireframeTransforms.Add(_transforms[i]);
          }
        }
      }
    }

    /// <summary>
    /// Allocate and assign an index for a new shape.
    /// </summary>
    /// <param name="id">The ID of the shape to allocate for.</param>
    /// <returns>The index of the newly allocated shape.</returns>
    /// <remarks>
    /// For a non-transient shape cache, this maps the <paramref name="id"/> to an index. For a transient cache the
    /// <paramref name="id"/> is ignored and no mapping is created.
    /// </remarks>
    /// <exception cref="DuplicateIDException">Thrown whn an object with the same ID already exists and the case is not
    /// transient.</exception>
    private int AssignNewShapeIndex(uint id)
    {
      if (!IsTransientCache)
      {
        if (_idMap.ContainsKey(id))
        {
          throw new DuplicateIDException($"Duplicate ID {id}");
        }

        // Assign a new index from either free list or next available.
        int newIndex = -1;
        if (_freeList.Count > 0)
        {
          newIndex = _freeList[_freeList.Count - 1];
          _freeList.RemoveAt(_freeList.Count - 1);
        }
        else
        {
          // Grow if at capacity.
          if (_currentCount + 1 >= _capacity)
          {
            Grow();
          }

          newIndex = _currentCount;
        }

        // Record the mapping from object ID to index.
        _idMap[id] = newIndex;
        // We now have another object.
        ++_currentCount;

        return newIndex;
      }

      // Transient. Just use the next index.
      if (_currentCount + 1 >= _capacity)
      {
        Grow();
      }

      return _currentCount++;
    }

    /// <summary>
    /// Grow the current storage capacity to the next power of 2.
    /// </summary>
    private void Grow()
    {
      // Grow by powers of 2.
      _capacity = Maths.IntUtil.NextPowerOf2(_capacity);
      Array.Resize(_shapes, _capacity);
      Array.Resize(_transforms, _capacity);

      for (int i = 0; i < _dataExtensions.Count; ++i)
      {
        Array.Resize(_dataExtensions[i].Elements, _capacity);
      }
    }

    /// <summary>
    /// Data extension details pairing type info and data array.
    /// </summary>
    struct DataExtension
    {
      /// <summary>
      /// Data type stored in <see cref="Elements"/>.
      /// </summary>
      public Type DataType;
      /// <summary>
      /// Data array.
      /// </summary>
      public Array Elements;
    }

    /// <summary>
    /// Maps from Object ID to internal index.
    /// </summary>
    private Dictionary<uint, int> _idMap = null;
    /// <summary>
    /// Tracks which indices are unused in a FILO fashion.
    /// </summary>
    private List<int> _freeList = null;
    /// <summary>
    /// Core data for each shape.
    /// </summary>
    CreateMessage[] _shapes = null;
    /// <summary>
    /// Transforms for each shape.
    /// </summary>
    Matrix4x4[] _transforms = null;
    /// <summary>
    /// Additional data for each shape.
    /// </summary>
    List<DataExtension> _dataExtensions = new List<DataExtension>();
    /// <summary>
    /// Current capacity. Matches each array length.
    /// </summary>
    int _capacity = 0;
    /// <summary>
    /// Current shape count.
    /// </summary>
    int _currentCount = 0;
  }
}
