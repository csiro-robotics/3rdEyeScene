using System;
using System.Collections.Generic;
using Tes.Exception;
using Tes.Net;
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
  ///
  /// This functionality should eventually be migrated to Unity ECS as that technology matures.
  /// </remarks>
  public class ShapeCache
  {
    /// <summary>
    /// ID reserved to mark shape entries which are the entry point for <see cref="Shapes.MultiShape"/> sets.
    /// </summary>
    public static readonly uint MultiShapeID = 0xFFFFFFFFu;

    /// <summary>
    /// Delegate function signature invoked when a set of shapes in a transient cache expire. For cleanup.
    /// </summary>
    /// <param name="cache">The calling cache.</param>
    /// <param name="srcIndex">The index of the first expiring shape.</param>
    /// <param name="count">Number of expiring entries.</param>
    public delegate void TransientExpiryDelegate(ShapeCache cache, int srcIndex, int count);

    /// <summary>
    /// Delegate to invoke when expiring transient shapes expire. This allows cleanup operations such as buffer release.
    /// </summary>
    public TransientExpiryDelegate TransientExpiry;

    /// <summary>
    /// Create a new shape cache.
    /// </summary>
    /// <param name="initialCapacity">Sets the initial storage capcity. Use zero to set the default capacity.</param>
    /// <param name="transient">True to create a transient cache.</param>
    public ShapeCache(int initialCapacity, bool transient)
    {
      _capacity = (initialCapacity > 0) ? initialCapacity : 1024;
      _shapes = new CreateMessage[_capacity];
      _transforms = new Matrix4x4[_capacity];
      _parentTransforms = new Matrix4x4[_capacity];
      _multiShapeChain = new int[_capacity];
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
    /// <remarks>
    /// The <see cref="TransientExpiry"/> delegate is called to cover transient shape expiry.
    /// </remarks>
    public void Reset()
    {
      if (IsTransientCache)
      {
        if (TransientExpiry != null)
        {
          TransientExpiry(this, 0, _currentCount);
        }
      }
      if (_freeList != null)
      {
        _freeList.Clear();
      }
      if (_idMap != null)
      {
        _idMap.Clear();
      }
      CreateMessage clearMsg = new CreateMessage();
      for (int i = 0; i < _shapes.Length; ++i)
      {
        _shapes[i] = clearMsg;
      }
      _currentCount = 0;
    }

    /// <summary>
    /// Add additional data to store for each shape in this cache.
    /// </summary>
    /// <typeparam name="T">The data type to store.</typeparam>
    public void AddShapeDataType<T>() where T: IShapeData
    {
      _dataExtensions.Add(new DataExtension
      {
        DataType = typeof(T),
        Elements = new IShapeData[_capacity]
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
    public int CreateShape(CreateMessage shape, Matrix4x4 transform, int multiShapeChainIndex = -1)
    {
      int shapeIndex = AssignNewShapeIndex(shape.ObjectID);

      _shapes[shapeIndex] = shape;
      _transforms[shapeIndex] = transform;
      _parentTransforms[shapeIndex] = Matrix4x4.identity;
      _multiShapeChain[shapeIndex] = multiShapeChainIndex;

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
        return -1;
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
    /// Destroy a shape by its internal index.
    /// </summary>
    /// <param name="index">Index of the shape to remove.</param>
    public void DestroyShapeByIndex(int index)
    {
      if (IsTransientCache)
      {
        // Irrelevant for transient caches.
        return;
      }

      uint id = _shapes[index].ObjectID;
      _shapes[index].ObjectID = 0;
      _freeList.Add(index);
      --_currentCount;
      if (id != MultiShapeID)
      {
        _idMap.Remove(id);
      }
    }

    /// <summary>
    /// Get core shape data for an entry.
    /// </summary>
    /// <param name="objectID">ID of the shape to retrieve data for.</param>
    /// <returns>The shape's core data.</returns>
    /// <exception cref="InvalidIDException">Thrown if this is a transient shape cache</exception>
    public CreateMessage GetShape(uint objectID)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      return _shapes[_idMap[objectID]];
    }

    /// <summary>
    /// Get core shape data for an entry by its index
    /// </summary>
    /// <param name="index">Index of the shape..</param>
    /// <returns>The shape's core data.</returns>
    public CreateMessage GetShapeByIndex(int index)
    {
      return _shapes[index];
    }

    /// <summary>
    /// Set core shape data.
    /// </summary>
    /// <param name="objectID">ID of the shape.</param>
    /// <param name="shape">Core shape data to set.</param>
    /// <exception cref="InvalidIDException">Thrown if this is a transient shape cache</exception>
    public void SetShape(uint objectID, CreateMessage shape)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      SetShapeByIndex(_idMap[objectID], shape);
    }

    /// <summary>
    /// Set core shape data by index.
    /// </summary>
    /// <param name="index">Index of the shape.</param>
    /// <param name="shape">Core shape data to set.</param>
    public void SetShapeByIndex(int index, CreateMessage shape)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      _shapes[index] = shape;
    }

    /// <summary>
    /// Get transform for a shape.
    /// </summary>
    /// <param name="objectID">ID of the shape.</param>
    /// <returns>The shape's transform.</returns>
    /// <exception cref="InvalidIDException">Thrown if this is a transient shape cache</exception>
    public Matrix4x4 GetShapeTransform(uint objectID)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      return _transforms[_idMap[objectID]];
    }

    /// <summary>
    /// Get transform for a shape by its index.
    /// </summary>
    /// <param name="index">Index of the shape.</param>
    /// <returns>The shape's transform.</returns>
    public Matrix4x4 GetShapeTransformByIndex(int index)
    {
      return _transforms[index];
    }

    /// <summary>
    /// Set transform for a shape.
    /// </summary>
    /// <param name="objectID">ID of the shape.</param>
    /// <param name="transform">The transform matrix to set.</param>
    /// <exception cref="InvalidIDException">Thrown if this is a transient shape cache</exception>
    public void SetShapeTransform(uint objectID, Matrix4x4 transform)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      SetShapeTransformByIndex(_idMap[objectID], transform);
    }

    /// <summary>
    /// Set transform for a shape by its index.
    /// </summary>
    /// <param name="index">Index of the shape.</param>
    /// <param name="transform">The transform matrix to set.</param>
    public void SetShapeTransformByIndex(int index, Matrix4x4 transform)
    {
      _transforms[index] = transform;
    }

    /// <summary>
    /// Get parent transform for a shape.
    /// </summary>
    /// <param name="objectID">ID of the shape.</param>
    /// <returns>The shape's parent transform.</returns>
    /// <exception cref="InvalidIDException">Thrown if this is a transient shape cache</exception>
    public Matrix4x4 GetParentTransform(uint objectID)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      return _parentTransforms[_idMap[objectID]];
    }

    /// <summary>
    /// Get parent transform for a shape by its index.
    /// </summary>
    /// <param name="index">Index of the shape.</param>
    /// <returns>The shape's parent transform.</returns>
    public Matrix4x4 GetParentTransformByIndex(int index)
    {
      return _parentTransforms[index];
    }

    /// <summary>
    /// Set parent transform for a shape.
    /// </summary>
    /// <param name="objectID">ID of the shape.</param>
    /// <param name="transform">The parent transform matrix to set.</param>
    /// <exception cref="InvalidIDException">Thrown if this is a transient shape cache</exception>
    public void SetParentTransform(uint objectID, Matrix4x4 transform)
    {
      if (IsTransientCache)
      {
        throw new InvalidIDException("Access by ObjectID not supported on a transient shape cache.");
      }
      SetParentTransformByIndex(_idMap[objectID], transform);
    }

    /// <summary>
    /// Set parent transform for a shape by its index.
    /// </summary>
    /// <param name="index">Index of the shape.</param>
    /// <param name="transform">The parent transform matrix to set.</param>
    public void SetParentTransformByIndex(int index, Matrix4x4 transform)
    {
      _parentTransforms[index] = transform;
    }

    /// <summary>
    /// Get the multi-shape chain index for a shape.
    /// </summary>
    /// <param name="index">Index of the shape.</param>
    /// <returns>The multi-shape chain index transform.</returns>
    /// <remarks>
    /// <see cref="MultiShape"/> objects are represented in the cache by chaining this index as a linked list. This
    /// function essentially retrieves the next index value, returning -1 at the end of the chain. This index can only
    /// be set when creating the shape.
    /// </remarks>
    public int GetMultiShapeChainByIndex(int index)
    {
      return _multiShapeChain[index];
    }

    /// <summary>
    /// Set the multi-shape chain index for a shape.
    /// </summary>
    /// <param name="index">Index of the shape (where to write).</param>
    /// <param name="multiChainIndex">The multi-chain index value to write (what to write).</param>
    /// <remarks>
    /// This function should be used with great care in order to correctly maintain the linked list created by these
    /// indices.
    /// </remarks>
    public void SetMultiShapeChainByIndex(int index, int multiChainIndex)
    {
      _multiShapeChain[index] = multiChainIndex;
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
    public T GetShapeData<T>(uint objectID) where T: IShapeData
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
    public T GetShapeDataByIndex<T>(int index) where T: IShapeData
    {
      for (int i = 0; i < _dataExtensions.Count; ++i)
      {
        if (typeof(T) == _dataExtensions[i].DataType)
        {
          return (T)_dataExtensions[i].Elements[index];
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
    public void SetShapeData<T>(uint objectID, T data) where T: IShapeData
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
    public void SetShapeDataByIndex<T>(int index, T data) where T: IShapeData
    {
      for (int i = 0; i < _dataExtensions.Count; ++i)
      {
        if (typeof(T) == _dataExtensions[i].DataType)
        {
          _dataExtensions[i].Elements[index] = data;
          return;
        }
      }

      throw new InvalidDataTypeException($"Unregistered shape extension type : {typeof(T).Name}");
    }

    /// <summary>
    /// Defines what to collect when calling <see cref="Collect()"/>.
    /// </summary>
    public enum CollectType
    {
      /// <summary>
      /// Collect solid shapes.
      /// </summary>
      Solid,
      /// <summary>
      /// Collect transparent shapes.
      /// </summary>
      Transparent,
      /// <summary>
      /// Collect wireframe shapes.
      /// </summary>
      Wireframe
    }

    /// <summary>
    /// Iterate the current shapes collecting their transforms (for rendering).
    /// </summary>
    /// <param name="transforms">List to populate with shape transforms.</param>
    /// <param name="parentTransforms">List to populate with parent transforms.</param>
    /// <param name="shapes">List of shapes collected</param>
    /// <param name="collectType">Defines what kind of shapes to collect.</param>
    /// <remarks>
    /// This method is used to collect shapes for rendering, collecting each valid shape's transform, parent transform
    /// and core shape data in the relevant lists.
    /// </remarks>
    public void Collect(List<Matrix4x4> transforms, List<Matrix4x4> parentTransforms,
                        List<CreateMessage> shapes, CollectType collectType)
    {
      // Walk the _headers and _transforms arrays directly. We can tell which indices are valid by investigating the
      // _headers[].ID value. A zero value is not in use.
      // TODO: (KS) consider storing a high water mark to limit iterating the arrays.
      bool transientCache = IsTransientCache;
      int itemLimit = (transientCache) ? _currentCount : _capacity;
      for (int i = 0; i < itemLimit; ++i)
      {
        CreateMessage shape = _shapes[i];

        // Transient cached we know everything up to the item limit is valid. All IDs will be zero.
        // Non transient cache, we look for shapes with non-Render IDs.
        // A transient cache has all IDs set to zero. A non-transient cache has all valid IDs as non-zero.
        if ((transientCache || shape.ObjectID != 0))
        {
          bool add = false;
          // Don't visualise multi-shape entries; they are essentially the parent transform for a set of other shapes
          // not bearing this flag.
          if ((shape.Flags & (ushort)Tes.Net.ObjectFlag.MultiShape) == 0)
          {
            if ((shape.Flags & (ushort)Tes.Net.ObjectFlag.Wireframe) != 0)
            {
              // Collecting wireframe.
              add = collectType == CollectType.Wireframe;
            }
            else if ((shape.Flags & (ushort)Tes.Net.ObjectFlag.Transparent) != 0)
            {
              // Collecting transparent.
              add = collectType == CollectType.Transparent;
            }
            else if (collectType == CollectType.Solid)
            {
              // Collecting solid.
              add = true;
            }
          }

          if (add)
          {
            transforms.Add(_transforms[i]);
            parentTransforms.Add(_parentTransforms[i]);
            shapes.Add(shape);
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
        if (id != MultiShapeID)
        {
          _idMap[id] = newIndex;
        }

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
      _capacity = Maths.IntUtil.NextPowerOf2(_capacity + 1);
      Debug.Assert(_capacity > _shapes.Length);
      Array.Resize(ref _shapes, _capacity);
      Array.Resize(ref _transforms, _capacity);
      Array.Resize(ref _parentTransforms, _capacity);
      Array.Resize(ref _multiShapeChain, _capacity);

      for (int i = 0; i < _dataExtensions.Count; ++i)
      {
        DataExtension dataExtension = _dataExtensions[i];
        Array.Resize(ref dataExtension.Elements, _capacity);
        _dataExtensions[i] = dataExtension;
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
      public IShapeData[] Elements;
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
    /// Parent transform for each shape. Used for multi-shape sets.
    /// </summary>
    Matrix4x4[] _parentTransforms = null;
    /// <summary>
    /// Multi-shape index shape. Each entry is an index to the next shape in the chain. -1 terminates.
    /// </summary>
    int[] _multiShapeChain = null;
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
