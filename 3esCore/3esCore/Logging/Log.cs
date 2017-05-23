using System;
using System.Collections.Generic;

namespace Tes.Logging
{
  /// <summary>
  /// Static logging interface.
  /// </summary>
  public static class Log
  {
    private struct Entry
    {
      public LogLevel Level;
      public int Category;
      public string Message;
      public Exception Except;
    }
    
    private static List<ILog> _targets = new List<ILog>();
    private static Collections.Queue<Entry> _logQueue = new Collections.Queue<Entry>();

    public static void AddTarget(ILog log)
    {
      _targets.Add(log);
    }

    public static bool RemoveTarget(ILog log)
    {
      return _targets.RemoveAll(item => item == log) > 0;
    }

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

    public static void Flush()
    {
      Entry entry = new Entry();
      while (_logQueue.TryDequeue(ref entry))
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
			}
    }
  }
}
