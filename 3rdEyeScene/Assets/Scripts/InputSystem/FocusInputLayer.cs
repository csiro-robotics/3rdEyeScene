using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InputSystem
{
  public class FocusInputLayer : MonoBehaviour, ISelectHandler, IDeselectHandler
  {
    [SerializeField]
    private string _layerName = "Controls";

    public string LayerName
    {
      get { return _layerName; }
      set { _layerName = value; }
    }

    private InputStack _inputStack = null;
    public InputStack InputStack
    {
      get
      {
        if (_inputStack == null)
        {
          _inputStack = GameObject.FindObjectOfType<InputStack>();
        }
        return _inputStack;
      }
    }

    public void OnSelect(BaseEventData data)
    {
      InputStack.SetLayerEnabled(LayerName, true);
    }
    public void OnDeselect(BaseEventData data)
    {
      InputStack.SetLayerEnabled(LayerName, false);
    }
  }
}
