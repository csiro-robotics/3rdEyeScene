using UnityEngine;
using Tes.Main;

namespace UI.Info
{
  public class RouterStatus : MonoBehaviour
  {
    [SerializeField]
    private TesComponent _controller = null;
    public TesComponent Controller { get { return _controller; } }

    public RouterMode LastMode { get; protected set; }

    void Start()
    {
      LastMode = RouterMode.Idle;
      ShowMode();
    }

    void Update()
    {
      if (_controller != null)
      {
        if (LastMode != _controller.Mode)
        {
          LastMode = _controller.Mode;
          ShowMode();
        }
      }
    }

    protected void ShowMode()
    {
      UnityEngine.UI.Text display = GetComponent<UnityEngine.UI.Text>();
      if (display != null)
      {
        display.text = LastMode.ToString();
      }
    }
  }
}
