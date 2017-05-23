using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  /// <summary>
  /// Updates the slider components to show the current progress.
  /// </summary>
  public class Timeline : MonoBehaviour
  {
    [SerializeField]
    private TesComponent _controller = null;
    public TesComponent Controller { get { return _controller; } }

    [SerializeField]
    private Slider _slider = null;
    public Slider Slider { get { return _slider; } }

    [SerializeField]
    private InputField _currentFrame = null;
    public InputField CurrentFrame { get { return _currentFrame; } }

    [SerializeField]
    private InputField _totalFrames = null;
    public InputField TotalFrames { get { return _totalFrames; } }

    /// <summary>
    /// Timeout before we respond to the timeline UI being slid by the user.
    /// </summary>
    [SerializeField, Range(0.001f, 10.0f)]
    public float _sliderResponseTimeout = 0.2f;
    private float SliderResponseTimeout { get { return _sliderResponseTimeout; } }

    /// <summary>
    /// Initialisation.
    /// </summary>
    void Start()
    {
      if (_slider != null)
      {
        // See FixNavigation() comment.
        _slider.FixNavigation();
      }
    }

    /// <summary>
    /// Update the
    /// </summary>
    void Update()
    {
      if (_controller != null)
      {
        TotalFrames.text = _controller.TotalFrames.ToString();

        // Updating the slider max value before current value prevents a flickering
        // in the slider position.
        _slider.maxValue = _controller.TotalFrames;

        // Are we waiting to respond to a user slide event?
        if (_sliderResponseTimer > 0)
        {
          _sliderResponseTimer -= Time.deltaTime;
          if (_sliderResponseTimer <= 0)
          {
            _sliderResponseTimer = 0;
            // Update the target frame in the controller.
            _controller.SetFrame(_sliderTargetFrame);
            _sliderTargetFrame = 0;
          }
        }

        if (_sliderResponseTimer <= 0)
        {
          _sliderResponseTimer = 0;
          if (_lastCurrentFrame != _controller.CurrentFrame)
          {
            _lastCurrentFrame = _controller.CurrentFrame;
            CurrentFrame.text = _lastCurrentFrame.ToString();
            _slider.value = _lastCurrentFrame;
          }
        }
      }
    }

    /// <summary>
    /// Invoked whenver the slider controll value changes.
    /// </summary>
    /// <remarks>
    /// This includes programattic and user changes, so we have to take care in the handling logic.
    /// </remarks>
    public void OnFrameSlide()
    {
      if (_slider != null && _controller != null)
      {
        uint targetFrame = (uint)_slider.value;
        // If the target frame and _lastCurrentFrame are the same, then this is
        // definitely a programmatic change and can be ignored.
        if (targetFrame != _lastCurrentFrame)
        {
          // Not a programmatic change. Set up a slight detail before we update the
          // controller frame to prevent excessive updates. We will update the
          // current frame edit box though.
          _sliderTargetFrame = targetFrame;
          _sliderResponseTimer = SliderResponseTimeout;
          CurrentFrame.text = targetFrame.ToString();
          //Log.Diag("Slide to: {0}", targetFrame);
        }
      }
    }

    public void CurrentFrameEditEnd()
    {
      if (_controller != null)
      {
        uint targetFrame;
        if (uint.TryParse(CurrentFrame.text, out targetFrame))
        {
          _controller.SetFrame(targetFrame);
        }
      }
    }


    private uint _lastCurrentFrame = 0;
    private uint _sliderTargetFrame = 0;
    /// <summary>
    /// Time remaining before we effect the change requested by the user sliding the timeline.
    /// </summary>
    private float _sliderResponseTimer = 0;
  }
}
