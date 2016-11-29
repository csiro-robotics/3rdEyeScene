using System;
using System.Threading;

namespace Tes.Thread
{
  /// <summary>
  /// A spin lock using atomic operations.
  /// </summary>
  /// <remarks>
  /// To ensure correct unlocking even on exception, the recommended usage is:
  /// <code>
  /// void ProtectedFunction(SpinLock lock)
  /// {
  ///   lock.lock();
  ///   try
  ///   {
  ///     // Your code here.
  ///   }
  ///   finally
  ///   {
  ///     lock.unlock();
  ///   }
  /// }
  /// </code>
  /// </remarks>
  public class SpinLock
  {
    /// <summary>
    /// Block until the lock is attained.
    /// </summary>
    public void Lock()
    {
      while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
      {
        System.Threading.Thread.Sleep(0);
      }
    }

    /// <summary>
    /// Try to attain the lock. This supports non-blocking lock attempts.
    /// </summary>
    /// <returns><c>true</c> if the lock has been attained, <c>false</c> if the lock is owned elsewhere.</returns>
    public bool TryLock()
    {
      return Interlocked.CompareExchange(ref _lock, 1, 0) == 0;
    }

    /// <summary>
    /// Unlocks if currently locked. This should only ever be called by the owner of the
    /// lock, otherwise the lock will be erroneously released.
    /// </summary>
    public void Unlock()
    {
      Interlocked.CompareExchange(ref _lock, 0, 1);
    }

    /// <summary>
    /// The lock variable.
    /// </summary>
    private int _lock = 0;
  }
}
