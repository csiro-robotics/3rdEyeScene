using UnityEngine;
using UnityEngine.UI;

public class LoopingInit : MonoBehaviour
{
  void Start()
  {
    Toggle toggle = GetComponentInChildren<Toggle>();
    if (toggle != null)
    {
      toggle.isOn = PlaybackSettings.Instance.Looping;
    }
  }
}
