using System;
using System.Threading;

namespace Tes
{
  /// <summary>
  /// Secondary thread for displaying frame progress. Use Start()/Stop() to manage the thread.
  /// </summary>
  class FrameDisplay
  {
    /// <summary>
    /// Increment the current frame value by 1.
    /// </summary>
    public void IncrementFrame()
    {
      Interlocked.Increment(ref _frameNumber);
    }

    /// <summary>
    /// Increment the current frame by a given value.
    /// </summary>
    /// <param name="increment">The increment to add.</param>
    public void IncrementFrame(long increment)
    {
      Interlocked.Add(ref _frameNumber, increment);
    }

    /// <summary>
    /// Reset frame number to zero.
    /// </summary>
    public void Reset()
    {
      Interlocked.Exchange(ref _frameNumber, 0);
    }

    /// <summary>
    /// Start the display thread. Ignored if already running.
    /// </summary>
    public void Start()
    {
      if (_thread == null)
      {
        Console.WriteLine("Start display thread");
        _thread = new Thread(this.Run);
        _thread.Start();
      }
    }

    /// <summary>
    /// Stop the display thread. Ok to call when not running.
    /// </summary>
    public void Stop()
    {
      if (_thread != null)
      {
        Interlocked.Exchange(ref _quitCount, 1);
        _thread.Join();
        _thread = null;
        Interlocked.Exchange(ref _quitCount, 0);
        Reset();
      }
    }

    /// <summary>
    /// Thread loop.
    /// </summary>
    private void Run()
    {
      long lastFrame = 0;
      while (_quitCount == 0)
      {
        long frameNumber = Interlocked.Read(ref _frameNumber);

        if (lastFrame > frameNumber)
        {
          // Last frame is larger => takes up more space. Clear the line.
          Console.Write("\r                    ");
        }

        if (lastFrame != frameNumber)
        {
          Console.Write($"\r{frameNumber}");
          lastFrame = frameNumber;
        }
        Thread.Sleep(100);
      }
    }

    private long _frameNumber = 0;
    private int _quitCount = 0;
    private Thread _thread = null;
  }
}