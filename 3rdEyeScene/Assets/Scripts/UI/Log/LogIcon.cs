using Tes.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Log
{
  public class LogIcon : MonoBehaviour
  {
    [SerializeField]
    private Image _targetImage = null;
    public Image TargetImage
    {
      get { return _targetImage; }
      set { _targetImage = value; }
    }
    [SerializeField]
    private Sprite _normalIcon = null;
    public Sprite NormalIcon
    {
      get { return _normalIcon; }
      set { _normalIcon = value; }
    }
    [SerializeField]
    private Sprite _warningIcon = null;
    public Sprite WarningIcon
    {
      get { return _warningIcon; }
      set { _warningIcon = value; }
    }
    [SerializeField]
    private Sprite _errorIcon = null;
    public Sprite ErrorIcon
    {
      get { return _errorIcon; }
      set { _errorIcon = value; }
    }
    [SerializeField]
    private LogWindow _logWindow = null;
    public LogWindow LogWindow
    {
      get { return _logWindow; }
    }

    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
      if (LogWindow.WorstLevel != _lastLevel)
      {
        _lastLevel = LogWindow.WorstLevel;
        switch (_lastLevel)
        {
        case LogLevel.Critical:
        case LogLevel.Error:
          _targetImage.sprite = _errorIcon;
          break;
        case LogLevel.Warning:
          _targetImage.sprite = _warningIcon;
          break;
        default:
          _targetImage.sprite = _normalIcon;
          break;
        }
      }
    }

    private LogLevel _lastLevel = LogLevel.Diagnostic;
  }
}
