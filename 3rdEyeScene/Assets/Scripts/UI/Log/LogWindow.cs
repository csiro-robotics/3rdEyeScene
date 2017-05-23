using System;
using System.Collections.Generic;
using Tes.Logging;
using UnityEngine;

namespace UI.Log
{
  public class LogWindow : MonoBehaviour, Tes.Logging.ILog
  {
    public LogLevel WorstLevel
    {
      get { return _worstLevel; }
    }

    public void Log(string message, params object[] args)
    {
      Log(LogLevel.Info, 0, message, args);
    }

    public void Log(int category, string message, params object[] args)
    {
      Log(LogLevel.Info, category, message, args);
    }

    public void Log(LogLevel level, string message, params object[] args)
    {
      Log(level, 0, message, args);
    }

    public void Log(LogLevel level, int category, string message, params object[] args)
    {
      if ((int)level < (int)_worstLevel)
      {
        _worstLevel = level;
      }
      _logView.Append(level, category, string.Format(message, args));
    }

    public void Log(int category, Exception e)
    {
      Log(LogLevel.Critical, category, e.ToString());
    }

    public void Log(Exception e)
    {
      Log(LogLevel.Critical, e.ToString());
    }

    public void ScrollToBottom()
    {
      _logView.ScrollToBottom();
    }

    public void Clear()
    {
      _logView.Clear();
      _worstLevel = LogLevel.Diagnostic;
    }

    public void Toggle()
    {
      _logPanel.gameObject.SetActive(!_logPanel.gameObject.activeSelf);
    }

    // Use this for initialization
    void Start()
    {
      for (int i = 0; i < _categories.Count; ++i)
      {
        LogCategories.SetCategoryName(i, _categories[i]);
      }

      Tes.Logging.Log.AddTarget(this);
    }

    [SerializeField]
    private List<string> _categories = new List<string>();
    [SerializeField]
    private LogScrollView _logView = null;
    [SerializeField]
    private RectTransform _logPanel = null;
    private LogLevel _worstLevel = LogLevel.Diagnostic;
  }
}
