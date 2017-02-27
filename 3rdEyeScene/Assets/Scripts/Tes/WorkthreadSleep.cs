using UnityEngine;

namespace Tes
{
  /// <summary>
  /// YieldInstruction to use with <see cref="Workthread"/> when using true threads.
  /// Only intended to be created from <see cref="Workthread.CreateWait"/>.
  /// </summary>
  class WorkthreadSleep : YieldInstruction
  {
    public int Milliseconds { get; set; }

    public WorkthreadSleep(int sleepMs)
    {
      Milliseconds = sleepMs;
    }
  }
}
