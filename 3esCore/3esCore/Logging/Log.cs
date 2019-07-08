using System;
using System.Collections.Generic;

namespace Tes.Logging
{
  /// <summary>
  /// Static logging interface for global Tes logging.
  /// </summary>
  /// <remarks>
  /// Implements a set of thread safe logging calls with routing to any number of <see cref="T:ILog"/> objects.
  /// All logging calls are queued and must be flushed by calling <see cref="Flush(int)"/>. There is no implicit
  /// way to setup asynchronous <see cref="Flush(int)"/> because of how this class is to interact with Unity.
  /// In Unity, the log must be flushed on the main thread and the current implementation of Unity does not
  /// support threadding on all targets.
  ///
  /// Logging supports categorisation by integer identifier. This may be resolved to a string
  /// by using <see cref="LogCategories"/> to register or query names. Zero represents the default, unnamed
  /// category.
  ///
  /// Various log functions support string formatting underpinend by <c>string.Format()</c>.
  /// </remarks>
  public static class Log
  {
    /// <summary>
    /// Entry for the log queue.
    /// </summary>
    private struct Entry
    {
      public LogLevel Level;
      public int Category;
      public string Message;
      public Exception Except;
    }

    private static List<ILog> _targets = new List<ILog>();
    private static Tes.Collections.Queue<Entry> _logQueue = new Tes.Collections.Queue<Entry>();

    /// <summary>
    /// Add a log target.
    /// </summary>
    /// <param name="log">The log target implementation.</param>
    /// <remarks>
    /// Each log call will be passed to this target on <see cref="Flush(int)"/>.
    /// </remarks>
    public static void AddTarget(ILog log)
    {
      lock(_targets)
      {
        _targets.Add(log);
      }
    }

    /// <summary>
    /// Remove a log target.
    /// </summary>
    /// <param name="log">The log target implementation.</param>
    public static bool RemoveTarget(ILog log)
    {
      return _targets.RemoveAll(item => item == log) > 0;
    }

    /// <summary>
    /// Log a diagnostic level message.
    /// </summary>
    /// <param name="category">The category of the message.</param>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Diag(int category, string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Diagnostic,
        Category = category,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a diagnostic level message to the default category.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Diag(string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Diagnostic,
        Category = 0,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a information level message.
    /// </summary>
    /// <param name="category">The category of the message.</param>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Info(int category, string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Info,
        Category = category,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a information level message to the default category.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Info(string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Info,
        Category = 0,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a warning level message.
    /// </summary>
    /// <param name="category">The category of the message.</param>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Warning(int category, string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Warning,
        Category = category,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a warning level message to the default category.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Warning(string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Warning,
        Category = 0,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a error level message.
    /// </summary>
    /// <param name="category">The category of the message.</param>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Error(int category, string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Error,
        Category = category,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a error level message to the default category.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Error(string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Error,
        Category = 0,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a critical error level message.
    /// </summary>
    /// <param name="category">The category of the message.</param>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Critical(int category, string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Critical,
        Category = category,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a critical error level message to the default category.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Arguments for the message format string.</param>
    public static void Critical(string message, params object[] args)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Critical,
        Category = 0,
        Message = string.Format(message, args),
        Except = null
      });
    }

    /// <summary>
    /// Log a exception message.
    /// </summary>
    /// <param name="category">The category of the message.</param>
    /// <param name="e">The exception to log.</param>
    public static void Exception(int category, Exception e)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Critical,
        Category = category,
        Message = null,
        Except = e
      });
    }

    /// <summary>
    /// Log a exception message to the default category.
    /// </summary>
    /// <param name="e">The exception to log.</param>
    public static void Exception(Exception e)
    {
      _logQueue.Enqueue(new Entry
      {
        Level = LogLevel.Critical,
        Category = 0,
        Message = null,
        Except = e
      });
    }

    /// <summary>
    /// Flush pending log messages to each registered <see cref="T:ILog"/> target.
    /// </summary>
    /// <param name="iterationLimit">Limits the number of messages which may be flushed. Zero or less for no limit.</param>
    /// <remarks>
    /// Must be called periodicall to ensure pending messages are removed.
    ///
    /// The number of iterations performed may be limited by <paramref name="iterationLimit"/>
    /// to avoid any infinite loops (e.g., logging from a call to <see cref="T:ILog"/>).
    /// The default is set high to ensure some limit which should not be practically reached.
    /// </remarks>
    public static void Flush(int iterationLimit = 1000000)
    {
      lock(_targets)
      {
        int iterations = 0;
        Entry entry = new Entry();
        while ((iterationLimit <= 0 || iterations < iterationLimit) && _logQueue.TryDequeue(ref entry))
        {
          for (int i = 0; i < _targets.Count; ++i)
          {
            if (entry.Except == null)
            {
              _targets[i].Log(entry.Level, entry.Category, entry.Message);
            }
            else
            {
              _targets[i].Log(entry.Except);
            }
          }
          ++iterations;
        }
      }
    }
  }
}
