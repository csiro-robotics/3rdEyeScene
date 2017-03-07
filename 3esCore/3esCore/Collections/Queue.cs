using System;
using System.Collections;
using System.Collections.Generic;
using Tes.Threading;

namespace Tes.Collections
{
  /// <summary>
  /// This is a thread safe queue implementation.
  /// </summary>
  /// <remarks>
  /// The <see cref="Queue"/> explicitly does not implement any standard
  /// collection interfaces as they do not lend themselves well to maintaining
  /// thread safe access.
  /// 
  /// While the implementation details may change, the queue is currently 
  /// underpinned by the <c>System.Collections.Generic.Queue"</c>.
  /// 
  /// Note: The current .Net framework contains <c>System.Collections.Concurrent.ConcurrentQueue</c>.
  /// However, Unity does not support the required .Net framework version and that class is
  /// not available within Unity.
  /// </remarks>
  public class Queue<T>
  {
    /// <summary>
    /// Calculates the number of items in the queue.
    /// </summary>
    /// <remarks>
    /// Locks the queue to make the calculation.
    /// </remarks>
    public int Count
    {
      get
      {
        bool haveLock = false;
        try
        {
          _lock.Lock();
          haveLock = true;
          return _internalQueue.Count;
        }
        finally
        {
          if (haveLock)
          {
            _lock.Unlock();
          }
        }
      }
    }

    /// <summary>
    /// Clear the queue contents. Thread safe.
    /// </summary>
    public void Clear()
    {
      bool haveLock = false;
      try
      {
        _lock.Lock();
        haveLock = true;
        _internalQueue.Clear();
      }
      finally
      {
        if (haveLock)
        {
          _lock.Unlock();
        }
      }
    }

    /// <summary>
    /// Push <paramref name="item"/> onto the queue tail.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    public void Enqueue(T item)
    {
      bool haveLock = false;
      try
      {
        _lock.Lock();
        haveLock = true;
        _internalQueue.Enqueue(item);
      }
      finally
      {
        if (haveLock)
        {
          _lock.Unlock();
        }
      }
    }

    /// <summary>
    /// Pop the head of the queue.
    /// </summary>
    /// <seealso cref="TryDequeue(ref T)"/>
    /// <returns>The head of the queue.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public T Dequeue()
    {
      bool haveLock = false;
      try
      {
        _lock.Lock();
        haveLock = true;
        return _internalQueue.Dequeue();
      }
      catch (InvalidOperationException e)
      {
        throw new InvalidOperationException("Queue is empty.", e);
      }
      finally
      {
        if (haveLock)
        {
          _lock.Unlock();
        }
      }
    }

    /// <summary>
    /// Peek at the head of the queue. The head is not removed.
    /// </summary>
    /// <seealso cref="TryPeek(ref T)"/>
    /// <returns>The head of the queue.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public T Peek()
    {
      bool haveLock = false;
      try
      {
        _lock.Lock();
        haveLock = true;
        return _internalQueue.Peek();
      }
      catch (InvalidOperationException e)
      {
        throw new InvalidOperationException("Queue is empty.", e);
      }
      finally
      {
        if (haveLock)
        {
          _lock.Unlock();
        }
      }
    }

    /// <summary>
    /// Pop the head of the queue if the queue is not empty.
    /// </summary>
    /// <remarks>
    /// The <paramref name="item"/> value is unchanged if the queue is empty.
    /// </remarks>
    /// <param name="item">Set to the value of the head of the queue.</param>
    /// <returns>True if the queue was not empty and the head has been retrieved.
    /// False if the queue was empty (<paramref name="item"/> is unchanged).
    /// </returns>
    public bool TryDequeue(ref T item)
    {
      bool haveLock = false;
      try
      {
        _lock.Lock();
        haveLock = true;
        if (_internalQueue.Count > 0)
        {
          item = _internalQueue.Dequeue();
          return true;
        }
      }
      finally
      {
        if (haveLock)
        {
          _lock.Unlock();
        }
      }

      return false;
    }

    /// <summary>
    /// Peek at the head of the queue if the queue is not empty. The head is not removed.
    /// </summary>
    /// <remarks>
    /// The <paramref name="item"/> value is unchanged if the queue is empty.
    /// </remarks>
    /// <param name="item">Set to the value of the head of the queue.</param>
    /// <returns>True if the queue is not empty and the head has been retrieved.
    /// False if the queue is empty (<paramref name="item"/> is unchanged).
    /// </returns>
    public bool TryPeek(ref T item)
    {
      bool haveLock = false;
      try
      {
        _lock.Lock();
        haveLock = true;
        if (_internalQueue.Count > 0)
        {
          item = _internalQueue.Peek();
          return true;
        }
      }
      finally
      {
        if (haveLock)
        {
          _lock.Unlock();
        }
      }
      return false;
    }

    /// <summary>
    /// Internal queue implementation.
    /// </summary>
    private System.Collections.Generic.Queue<T> _internalQueue = new System.Collections.Generic.Queue<T>();
    /// <summary>
    /// Spin lock guard.
    /// </summary>
    private SpinLock _lock = new SpinLock();
  }
}
