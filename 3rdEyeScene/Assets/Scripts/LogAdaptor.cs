using System;
using Tes.Logging;
using UnityEngine;

public class LogAdaptor : ILog
{
  public void Log(string message, params object[] args)
  {
    Debug.Log(string.Format(message, args));
  }

  public void Log(int category, string message, params object[] args)
  {
    if (category > 0)
    {
      Debug.Log(string.Format("[{0}] {1}", category, string.Format(message, args)));
    }
    else
    {
      Debug.Log(string.Format(message, args));
    }
  }

  public void Log(LogLevel level, string message, params object[] args)
  {
    switch (level)
    {
    case LogLevel.Critical:
    case LogLevel.Error:
      Debug.LogError(string.Format(message, args));
      break;
    case LogLevel.Warning:
      Debug.LogWarning(string.Format(message, args));
      break;
    default:
      Debug.Log(string.Format(message, args));
      break;
    }
  }

  public void Log(LogLevel level, int category, string message, params object[] args)
  {
    switch (level)
    {
    case LogLevel.Critical:
    case LogLevel.Error:
      Debug.LogError(FormatMessage(level, category, message, args));
      break;
    case LogLevel.Warning:
      Debug.LogWarning(FormatMessage(level, category, message, args));
      break;
    default:
      Debug.Log(FormatMessage(level, category, message, args));
      break;
    }
  }

  public void Log(int category, Exception e)
  {
    Debug.LogException(e);
  }

  public void Log(Exception e)
  {
    Debug.LogException(e);
  }

  public static string FormatMessage(LogLevel level, int category, string message, params object[] args)
  {
    if (category == 0)
    {
      return string.Format(message, args);
    }
    return string.Format("[{0}] {1}", category, string.Format(message, args));
  }
}
