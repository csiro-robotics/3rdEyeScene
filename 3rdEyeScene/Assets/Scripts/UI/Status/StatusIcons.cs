using UnityEngine;
using UnityEngine.UI;

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
    private Image _connected = null;
    public Image Connected { get { return _connected; } }

    [SerializeField]
    private Sprite _connectedImage = null;
    public Sprite ConnectedImage { get { return _connectedImage; } }
    [SerializeField]
    private Sprite _disconnectedImage = null;
    public Sprite DisconnectedImage { get { return _disconnectedImage; } }

    void Start()
    {
      UpdateIcon(_connected, false, _connectedImage, _disconnectedImage);
    }

    void SetVisible(Image icon, bool on)
    {
      if (icon != null)
      {
        icon.gameObject.SetActive(on);
      }
    }

    void UpdateIcon(Image icon, bool isActive, Sprite active, Sprite inactive)
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
        UpdateIcon(_connected, _tes.Connected, _connectedImage, _disconnectedImage);
      }
    }
  }
}