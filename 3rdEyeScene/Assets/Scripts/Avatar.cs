using UnityEngine;
using System.Collections.Generic;

public class Avatar : MonoBehaviour
{
  public enum Mode
  {
    /// <summary>
    /// User controlled camera.
    /// </summary>
    User,
    /// <summary>
    /// Bound to a camera message.
    /// </summary>
    BoundCamera,
  }

  [SerializeField, Range(0.0f, 100.0f)]
  private float _highSpeedMultiplier = 10.0f;
  public float HighSpeedMultiplier
  {
    get { return _highSpeedMultiplier; }
    set { _highSpeedMultiplier = Mathf.Max(0.0f, value); }
  }

  [SerializeField, Range(0.0f, 1.0f)]
  private float _lowSpeedMultiplier = 0.1f;
  public float LowSpeedMultiplier
  {
    get { return _lowSpeedMultiplier; }
    set { _lowSpeedMultiplier = Mathf.Max(0.0f, value); }
  }

  public const float MinBaseSpeedMultipler = 0.1f;
  public const float MaxBaseSpeedMultipler = 100.0f;
  /// <summary>
  /// Current base speed multiplier. Combined with the high or low multipliers.
  /// </summary>
  [Range(MinBaseSpeedMultipler, MaxBaseSpeedMultipler)]
  public float BaseSpeedMultiplier = 1.0f;

  /// <summary>
  /// Speed multiplier to apply to movement. Checks if speed mode is on.
  /// </summary>
  public float SpeedMultiplier
  {
    get
    {
      bool speed = _inputLayer.GetButton("Fast");
      bool slow = _inputLayer.GetButton("Slow");
      if (speed && slow)
      {
        // Mostly to avoid issues in classes with Ctrl+Shift+R reload.
        return 0;
      }
      if (speed)
      {
        return HighSpeedMultiplier * BaseSpeedMultiplier;
      }
      if (slow)
      {
        return LowSpeedMultiplier * BaseSpeedMultiplier;
      }

      return BaseSpeedMultiplier;
    }
  }

  [SerializeField, Range(0.1f, 100.0f)]
  private float _baseMovementRate = 5.0f;
  public float BaseMovementRate
  {
    get { return _baseMovementRate; }
    set { _baseMovementRate = value; }
  }

  public float MovementRate { get { return _baseMovementRate * SpeedMultiplier; } }

  [SerializeField, Range(1.0f, 89.0f)]
  private float _pitchLimit = 89.0f;
  public float PitchLimit
  {
    get { return _pitchLimit; }
    set { _pitchLimit = value; }
  }

  //[SerializeField, Range(1.0f, 360.0f * 10.0f)]
  //private float _turnRate = 360.0f;
  //public float TurnRate
  //{
  //  get { return _turnRate; }
  //  set { _turnRate = value; }
  //}

  [SerializeField, Range(0.1f, 100.0f)]
  private float _mouseSensitivity = 15.0f;
  public float MouseSensitivity
  {
    get { return _mouseSensitivity; }
    set { _mouseSensitivity = Mathf.Clamp(value, 0.0f, 100.0f); }
  }

  [SerializeField]
  private Camera _camera;
  public Camera Camera
  {
    get { return _camera; }
    set { _camera = value; }
  }

  [SerializeField]
  private TesComponent _thirdEyeScene;
  public TesComponent ThirdEyeScene
  {
    get { return _thirdEyeScene; }
    set { _thirdEyeScene = value; }
  }

  [SerializeField]
  private string _inputLayerName = "Scene";
  public string InputLayerName
  {
    get { return _inputLayerName; }
  }

  public bool MouseMove { get; set; }

  public void SetAllowMouseMove(bool enable)
  {
    MouseMove = enable;
  }

  void Start()
  {
    MouseMove = false;
    _cameras = null;
    if (_thirdEyeScene != null)
    {
      _cameras = _thirdEyeScene.GetHandler((ushort)Tes.Net.RoutingID.Camera) as Tes.Handlers.CameraHandler;
      _inputLayer = _thirdEyeScene.InputStack.GetLayer(_inputLayerName);
    }
    if (_inputLayer == null)
    {
      Debug.LogError("Unable to resolve input layer for avatar control.");
    }
  }

  void Update()
  {
    // Look for mode switch.
    UpdateMode();

    if (_inputLayer.GetAxis("SpeedAdjust") != 0)
    {
      float speedChange = _inputLayer.GetAxis("SpeedAdjust");
      BaseSpeedMultiplier = Mathf.Clamp(BaseSpeedMultiplier + speedChange * BaseSpeedMultiplier,
                                        MinBaseSpeedMultipler, MaxBaseSpeedMultipler);
    }

    if (_mode == Mode.BoundCamera && _cameras != null && _cameras.ActiveCamera != null)
    {
      UpdateBoundCamera(_cameras.ActiveCamera, _cameras.AllowRemoteCameraSettings);
    }
    else
    {
      if (_mode != Mode.User)
      {
        PrepareUserMode();
      }
      _mode = Mode.User;
      if (MouseMove)
      {
        if (_inputLayer.GetButton("CameraActive"))
        {
          UpdateRotation(Time.deltaTime);
        }
      }
      UpdateLocomotion(Time.deltaTime);
    }
  }


  protected void UpdateMode()
  {
    if (_cameras != null)
    {
      if (_inputLayer.GetKeyDown(KeyCode.BackQuote))
      {
        // Restore user mode.
        PrepareUserMode();
        _mode = Mode.User;
      }
      else
      {
        if (_inputLayer.GetKeyDown(KeyCode.Alpha0))
        {
          // Recorded camera request.
          _cameras.ActiveCameraID = 255;
          if (_cameras.ActiveCamera != null)
          {
            _mode = Mode.BoundCamera;
          }
        }
        else
        {
          for (int i = 1; i < 10; ++i)
          {
            if (_inputLayer.GetKeyDown(KeyCode.Alpha0 + i))
            {
              _cameras.ActiveCameraID = i;
              if (_cameras.ActiveCamera != null)
              {
                _mode = Mode.BoundCamera;
                break;
              }
            }
          }
        }
      }
    }
  }


  protected void UpdateBoundCamera(Tes.Handlers.CameraHandler.CameraInfo cameraInfo, bool bindSettings)
  {
    Transform avatarTransform = gameObject.transform;
    Transform cameraTransform = (Camera != null) ? Camera.transform : gameObject.transform;

    // Move the position.
    avatarTransform.position = cameraInfo.transform.position;

    // Reset the avatar Euler angles to zero.
    avatarTransform.localEulerAngles = Vector3.zero;
    // Set the camera to match the target (not local angles).
    cameraTransform.eulerAngles = cameraInfo.transform.eulerAngles;

    if (bindSettings && Camera != null)
    {
      if (cameraInfo.Near > 0)
      {
        Camera.nearClipPlane = cameraInfo.Near;
      }
      if (cameraInfo.Far > cameraInfo.Near && cameraInfo.Far > 0)
      {
        Camera.farClipPlane = cameraInfo.Far;
      }
      if (cameraInfo.FOV > 0)
      {
        Camera.fieldOfView = cameraInfo.FOV;
      }
    }
  }


  protected void PrepareUserMode()
  {
    // Resolve pitch/yaw separation.
    if (Camera == null)
    {
      // No need.
      return;
    }

    // Remove roll, migrate camera yaw to avatar, preserve camera pitch.
    Transform cameraTransform = Camera.transform;
    Transform avatarTransform = transform;
    float pitch = cameraTransform.eulerAngles.x;
    float yaw = cameraTransform.eulerAngles.y;
    avatarTransform.eulerAngles = new Vector3(0, yaw, 0);
    cameraTransform.localEulerAngles = new Vector3(pitch, 0, 0);

    if (_cameras != null)
    {
      _cameras.ActiveCameraID = -1;
    }
  }


  protected void UpdateRotation(float dt)
  {
    Vector3 localEulerAngles;
    float deltaYaw = 0;
    float deltaPitch = 0;

    Transform pitchTransform = (Camera != null) ? Camera.transform : gameObject.transform;
    Transform yawTransform = gameObject.transform;

    deltaYaw = _inputLayer.GetAxis("Horizontal") * MouseSensitivity;
    deltaPitch = _inputLayer.GetAxis("Vertical") * MouseSensitivity;
    deltaPitch *= (CameraSettings.Instance.InvertY) ? -1.0f : 1.0f;

    localEulerAngles = pitchTransform.localEulerAngles;
    float pitch = localEulerAngles.x;
    if (pitch > 180.0f)
    {
      pitch -= 360.0f;
    }
    pitch = Mathf.Clamp(pitch + deltaPitch, -PitchLimit, PitchLimit);
    localEulerAngles.x = pitch;
    pitchTransform.localEulerAngles = localEulerAngles;

    localEulerAngles = yawTransform.localEulerAngles;
    float yaw = localEulerAngles.y;
    yaw += deltaYaw;
    while (yaw > 360.0f)
    {
      yaw -= 360.0f;
    }
    while (yaw < 0)
    {
      yaw += 360.0f;
    }

    localEulerAngles.y = yaw;
    gameObject.transform.localEulerAngles = localEulerAngles;
  }


  protected void UpdateLocomotion(float dt)
  {
    float moveRate = MovementRate * dt;
    float forward = moveRate * _inputLayer.GetAxis("LinearMove");
    float strafe = moveRate * _inputLayer.GetAxis("StrafeMove");
    float elevation = moveRate * _inputLayer.GetAxis("ElevationMove");

    Vector3 move = Vector3.zero;
    move += forward * ((Camera != null) ? Camera.transform.forward : gameObject.transform.forward);
    move += strafe * gameObject.transform.right;
    move += elevation * gameObject.transform.up;

    gameObject.transform.position += move;
  }

  private Mode _mode = Mode.User;
  private Tes.Handlers.CameraHandler _cameras = null;
  private InputSystem.InputLayer _inputLayer = null;
}
