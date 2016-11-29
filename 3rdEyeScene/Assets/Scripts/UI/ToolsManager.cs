using UnityEngine;
using System.Collections;

namespace UI
{
  public class ToolsManager : MonoBehaviour
  {
    public GameObject ActivePanel { get; protected set; }
    
    public void SetActivePanel(GameObject panel)
    {
      if (ActivePanel != null)
      {
        ActivePanel.SetActive(false);
        ActivePanel = null;
      }
      ActivePanel = panel;
      if (ActivePanel)
      {
        ActivePanel.SetActive(true);
      }
    }
    
    public void ToggleActivePanel(GameObject panel)
    {
      bool currentlyActive = panel == ActivePanel;
      if (ActivePanel != null)
      {
        ActivePanel.SetActive(false);
        ActivePanel = null;
        if (currentlyActive)
        {
          return;
        }
      }
      ActivePanel = panel;
      if (ActivePanel)
      {
        ActivePanel.SetActive(true);
      }
    }
  }
}
