using UnityEngine;
using Tes.Handlers;
using Tes.Net;

namespace UI.Status
{
  /// <summary>
  /// Maintains status icons in the UI.
  /// </summary>
  public class StatusIcons : MonoBehaviour
  {
    [SerializeField]
    private TesComponent _tes = null;
    public TesComponent TesComponent { get { return _tes; } }

    [SerializeField]
    private UnityEngine.UI.Image _slaveCamera = null;
    public UnityEngine.UI.Image SlaveCamera { get { return _slaveCamera; } }

    [SerializeField]
    private UnityEngine.UI.Image _connected = null;
    public UnityEngine.UI.Image Connected { get { return _connected; } }

    [SerializeField]
    private Sprite _connectedImage = null;
    public Sprite ConnectedImage { get { return _connectedImage; } }
    [SerializeField]
    private Sprite _disconnectedImage = null;
    public Sprite DisconnectedImage { get { return _disconnectedImage; } }

    void Start()
    {
      SetVisible(_slaveCamera, false);
      UpdateIcon(_connected, false, _connectedImage, _disconnectedImage);
    }

    void SetVisible(UnityEngine.UI.Image icon, bool on)
    {
      if (icon != null)
      {
        icon.gameObject.SetActive(on);
      }
    }

    void UpdateIcon(UnityEngine.UI.Image icon, bool isActive, Sprite active, Sprite inactive)
    {
      if (icon != null)
      {
        icon.sprite = (isActive) ? active : inactive;
      }
    }

    void Update()
    {
      if (_tes != null)
      {
        CameraHandler camHandle = _tes.GetHandler((ushort)RoutingID.Camera) as CameraHandler;
        SetVisible(_slaveCamera, camHandle != null && camHandle.ActiveCamera != null);
        UpdateIcon(_connected, _tes.Connected, _connectedImage, _disconnectedImage);
      }
    }
  }
}