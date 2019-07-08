using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interfaces between playback speed UI components and the playback speed control values.
/// </summary>
public class PlaybackSpeed : MonoBehaviour
{
  [SerializeField]
  Slider _slider = null;
  Slider Slider { get { return _slider; } }

  [SerializeField]
  InputField _inputSpeed = null;
  InputField InputSpeed { get { return _inputSpeed; } }

  [SerializeField]
  TesComponent _tes = null;

  [Range(1.0f, 100.0f)]
  float _maxSpeed = 20.0f;

  [Range(0.0001f, 1.0f)]
  float _minSpeed = 0.0f;

  void Start()
  {
    UpdateText(1.0f);
    UpdateSlider(1.0f);
  }

  public void OnSliderChanged(float value)
  {
    // Scale the speed.
    float newSpeed = 1.0f;
    float norm;
    if (value < 0)
    {
      norm = 1.0f - value / _slider.minValue;
      newSpeed = norm * (1.0f - _minSpeed) + _minSpeed;
    }
    else
    {
      norm = value / _slider.maxValue;
      newSpeed = 1.0f + norm * (_maxSpeed - 1.0f);
    }
    UpdateText(newSpeed);
    UpdatePlaybackSpeed(newSpeed);
  }


  public void OnInputChanged(string value)
  {
    float newSpeed = 1.0f;
    if (float.TryParse(value, out newSpeed))
    {
      newSpeed = Mathf.Clamp(newSpeed, _minSpeed, _maxSpeed);
      UpdateSlider(newSpeed);
      UpdatePlaybackSpeed(newSpeed);
    }
  }

  void UpdateText(float speed)
  {
    if (_inputSpeed != null)
    {
      _inputSpeed.text = string.Format("{0}", speed);
    }
  }

  void UpdateSlider(float speed)
  {
    if (_slider != null)
    {
      // Put into the slider range.
      float norm;
      if (speed >= 1.0f)
      {
        norm = (speed - 1.0f) / (_maxSpeed - 1.0f);
        _slider.value = norm * _slider.maxValue;
      }
      else
      {
        norm = (speed - 1.0f) / (_minSpeed - 1.0f);
        _slider.value = norm * _slider.minValue;
      }
    }
  }

  /// <summary>
  /// Update playback speed on the playback controller.
  /// </summary>
  /// <param name="speed">The new playback speed multiplier.</param>
  void UpdatePlaybackSpeed(float speed)
  {
    if (_tes != null)
    {
      _tes.PlaybackSpeed = speed;
    }
  }
}
