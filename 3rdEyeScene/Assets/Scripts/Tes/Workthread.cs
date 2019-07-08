using System;
using System.Collections;
#if TRUE_THREADS
using System.Threading;
#endif // TRUE_THREADS
using UnityEngine;

namespace Tes
{
  /// <summary>
  /// The <see cref="Workthread"/> represents either a lightweight micro-thread or a
  /// system thread.
  /// </summary>
  /// <remarks>
  /// The <see cref="Workthread"/> can be compiled in two modes: micro-thread and system
  /// thread mode. Micro-thread mode executes using Unity's coroutine system, while
  /// system thread mode uses <see cref="System.Threading.Thread"/>. This allows
  /// platforms not supporting system threads to execute the same code using coroutines
  /// instead.
  ///
  /// Because of the need to support coroutines, the <see cref="Workthread"/> executes
  /// a function via <see cref="System.Collections.IEnumerator"/>, just like Unity
  /// coroutines. This class simply wraps that enumerator and executes it. As such
  /// the function underpinning the enumerator must periodically yield return or
  /// potentially block the main thread.
  ///
  /// A <see cref="Workthread"/> also requires a <see cref="QuitDelegate"/> and
  /// an optional <see cref="StopDelegate"/>. The former is called to gracefully when
  /// <see cref="Stop()"/>. The thread function enumerator must terminate on the next update
  /// and may no longer execute or risk blocking the system thread. The <see cref="StopDelegate"/>
  /// is called after the thread has been stopped and joined. Both are called from the
  /// main thread.
  ///
  /// The thread function is normally called as fast as possible - at the frame rate for
  /// coroutines or at the CPU rate for system threads. If the work thread needs to sleep,
  /// it should yield return a <see cref="CreateWait(float)"/>. This results in a sleep
  /// call for a system thread or a Unity WaitForSeconds for coroutines.
  ///
  /// Finally the thread supports signalling via <see cref="Wait()"/> and <see cref="Notify()"/>.
  /// On calling <see cref="Wait()"/> a system thread will block until <see cref="Notify()"/>
  /// is called by another thread. Coroutines never block <see cref="Wait()"/>, so the return
  /// value must be checked. The direction of signalling is usage dependent.
  /// </remarks>
  public class Workthread
  {
    /// <summary>
    /// Delegate called when <see cref="Stop()"/> is called to inform the thread function
    /// it is time to quit. Called from the main thread.
    /// </summary>
    public delegate void QuitDelegate();
    /// <summary>
    /// Delegate called on <see cref="Stop()"/> after the thread has been joined.
    /// Called from the main thread.
    /// </summary>
    public delegate void StopDelegate();

    /// <summary>
    /// Create a new worker thread.
    /// </summary>
    /// <param name="threadFunction">The enumerator coroutine to call on each thread loop.</param>
    /// <param name="onQuit">The delegate to called to request the thread complete execution.</param>
    /// <param name="onStop">The delegate to call after joining the thread in <see cref="Stop()"/></param>
    public Workthread(IEnumerator threadFunction, QuitDelegate onQuit = null, StopDelegate onStop = null)
    {
      _threadFunction = threadFunction;
      _onStop = onStop;
      _onQuit = onQuit;
    }

#if TRUE_THREADS
    /// <summary>
    /// Call and yield return to cause the thread to sleep for <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">How long to sleep for in seconds. Only supports millisecond precision.</param>
    /// <returns>The object to yield return to effect the sleep.</returns>
    public static YieldInstruction CreateWait(float seconds)
    {
      return new WorkthreadSleep((int)(seconds * 1e3f));
    }

    /// <summary>
    /// Is the thread running?
    /// </summary>
    public bool Running
    {
      get
      {
        return (_thread != null) ? _thread.ThreadState == ThreadState.Running : false;
      }
    }

    /// <summary>
    /// Start the thread.
    /// </summary>
    /// <returns>True if not already running.</returns>
    public bool Start()
    {
      if (Running)
      {
        return false;
      }
      _quit = false;
      _thread = new System.Threading.Thread(ThreadEntry);
      _thread.Name = "Workthread";
      _thread.Start();
      return true;
    }

    /// <summary>
    /// Stop and join the thread.
    /// </summary>
    /// <remarks>
    /// Invokes the <see cref="QuitDelegate"/>, followed by the <see cref="StopDelegate"/>
    /// once the thread is joined.</remarks>
    /// <returns>True if the thread was running and has been stopped.</returns>
    public bool Stop()
    {
      if (_thread != null)
      {
        _quit = true;
        // Make sure we aren't suspended.
        Resume();
        if (_onQuit != null)
        {
          _onQuit();
        }
        _thread.Join();
        if (_onStop != null)
        {
          _onStop();
        }
        _thread = null;
        _quit = false;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Entry point for the system thread.
    /// </summary>
    /// <remarks>
    /// Loops calling the thread function enumerator until it exists without a
    /// yield return. Also effects sleeping as returned by the thread function.
    /// </remarks>
    private void ThreadEntry()
    {
      if (_threadFunction != null)
      {
        WorkthreadSleep wait = null;
        while (!_quit && _threadFunction.MoveNext())
        {
          wait = _threadFunction.Current as WorkthreadSleep;
          if (wait != null)
          {
            System.Threading.Thread.Sleep(wait.Milliseconds);
          }

          if (_suspend)
          {
            // Suspend requested
            _suspendEvent.WaitOne();
          }
        }
      }
    }

    /// <summary>
    /// Block until <see cref="Notify()"/> is called. May also unblock
    /// for other reasons, so check the result.
    /// </summary>
    /// <remarks>
    /// This function behaves differently for system threads and coroutines.
    /// </remarks>
    /// <returns>True if returning with a successful notification.</returns>
    public bool Wait()
    {
      return _sync.WaitOne();
    }

    /// <summary>
    /// Notify the thread, unblocking <see cref="Wait()"/>.
    /// </summary>
    public void Notify()
    {
      _sync.Set();
    }

    /// <summary>
    /// Does this implementation support being suspended? True for true threads.
    /// </summary>
    public static bool CanSuspend { get { return true; } }

    /// <summary>
    /// True when a successful call <see cref="Suspend()" />has been made.
    /// </summary>
    public bool IsSuspended { get { return _suspend; } }

    /// <summary>
    /// Requests the thread suspends execution.
    /// </summary>
    /// <returns>True if the suspend request is made.</returns>
    /// <remarks>
    /// A suspended thread will no longer call the thread function until <see cref="Resume()" /> is called.
    /// </remarks>
    public bool Suspend()
    {
      if (!_suspend)
      {
        _suspend = true;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Requests the thread resumes execution after <see cref="Suspend()" />.
    /// </summary>
    /// <returns>True if the resume request is made.</returns>
    public bool Resume()
    {
      if (_suspend)
      {
        _suspend = false;
        _suspendEvent.Set();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Wait handle.
    /// </summary>
    private EventWaitHandle _sync = new EventWaitHandle(false, EventResetMode.AutoReset);
    /// <summary>
    /// Suspend handle.
    /// </summary>
    private EventWaitHandle _suspendEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
    /// <summary>
    /// System thread.
    /// </summary>
    private System.Threading.Thread _thread = null;
    /// <summary>
    /// Flag indicating the background thread should suspend by waiting on _suspendEvent.
    /// </summary>
    private bool _suspend = false;
    /// <summary>
    /// Quit flag. Thread stops looping when set.
    /// </summary>
    private bool _quit = false;
#else  // TRUE_THREADS

    /// <summary>
    /// Game object used to manage the coroutines.
    /// </summary>
    private static GameObject _workthreadObject = null;

    /// <summary>
    /// Fetch the game object used to manage the coroutines.
    /// </summary>
    private static GameObject WorkthreadObject
    {
      get
      {
        if (_workthreadObject == null)
        {
          _workthreadObject = new GameObject();
          _workthreadObject.name = "Workthread";
        }
        return _workthreadObject;
      }
    }

    /// <summary>
    /// Behaviour attached to the <see cref="WorkthreadObject"/>.
    /// </summary>
    public class WorkthreadBehaviour : MonoBehaviour {}

    /// <summary>
    /// Call and yield return to delay next execution for <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">How long to delay for in seconds.</param>
    /// <returns>The object to yield return to effect the delay.</returns>
    public static YieldInstruction CreateWait(float seconds)
    {
      return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Is the thread running?
    /// </summary>
    public bool Running
    {
      get
      {
        return (_threadRoutine != null) ? _threadRoutine.Target != null : false;
      }
    }

    /// <summary>
    /// Start the thread.
    /// </summary>
    /// <returns>True if not already running.</returns>
    public bool Start()
    {
      if (Running)
      {
        return false;
      }
      WorkthreadBehaviour workBehaviour = WorkthreadObject.GetComponent<WorkthreadBehaviour>();
      if (workBehaviour == null)
      {
        workBehaviour = WorkthreadObject.AddComponent<WorkthreadBehaviour>();
      }
      Coroutine threadRoutine = workBehaviour.StartCoroutine(_threadFunction);
      _threadRoutine = new WeakReference(threadRoutine);
      return true;
    }

    /// <summary>
    /// Stop and join the thread.
    /// </summary>
    /// <remarks>
    /// Invokes the <see cref="QuitDelegate"/>, followed by the <see cref="StopDelegate"/>
    /// once the thread is joined.</remarks>
    /// <returns>True if the thread was running and has been stopped.</returns>
    public bool Stop()
    {
      if (_threadRoutine != null && _workthreadObject != null)
      {
        Coroutine routine = _threadRoutine.Target as Coroutine;
        WorkthreadBehaviour workBehaviour = _workthreadObject.GetComponent<WorkthreadBehaviour>();
        if (routine != null && workBehaviour != null)
        {
          if (_onQuit != null)
          {
            _onQuit();
          }
          workBehaviour.StopCoroutine(routine);
          if (_onStop != null)
          {
            _onStop();
          }
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Checks if the notify flag has been set and clears it. Always returns immediately in this mode.
    /// </summary>
    /// <remarks>
    /// This method behaves differently for system threads and coroutine.
    /// </remarks>
    /// <returns>True if the notify flag was set.</returns>
    public bool Wait()
    {
      if (_notifyFlag)
      {
        _notifyFlag = false;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sets the notify flag so the next call to <see cref="Wait()"/> returns true.
    /// </summary>
    public void Notify()
    {
      _notifyFlag = true;
    }

    /// <summary>
    /// Does this implementation support being suspended? False for coroutine workers.
    /// </summary>
    public static bool CanSuspend { get { return false; } }

    /// <summary>
    /// No implementation for coroutine threads: always false.
    /// </summary>
    public bool IsSuspended { get { return false; } }

    /// <summary>
    /// No implementation for coroutine threads.
    /// </summary>
    /// <returns>false</returns>
    public bool Suspend()
    {
      return false;
    }

    /// <summary>
    /// No implementation for coroutine threads.
    /// </summary>
    /// <returns>false</returns>
    public bool Resume()
    {
      return false;
    }

    /// <summary>
    /// Weak references to the actual coroutine.
    /// </summary>
    private WeakReference _threadRoutine;
    /// <summary>
    /// Notification flag for <see cref="Wait()"/> and <see cref="Notify()"/>.
    /// </summary>
    private bool _notifyFlag = false;
#endif // TRUE_THREADS
    /// <summary>
    /// Thread function enumerator.
    /// </summary>
    private IEnumerator _threadFunction = null;
    /// <summary>
    /// Delegate to call to effect quitting.
    /// </summary>
    private QuitDelegate _onQuit = null;
    /// <summary>
    /// Delegate to call after stopping.
    /// </summary>
    private StopDelegate _onStop = null;
  }
}
