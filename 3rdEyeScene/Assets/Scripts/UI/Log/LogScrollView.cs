using System.Collections.Generic;
using Tes.Logging;
using UnityEngine;

namespace UI.Log
{
  public class LogScrollView : TextScrollView
  {
    public void Append(LogLevel level, int category, string message)
    {
      RectTransform newItem = GameObject.Instantiate(TextItem);
      UnityEngine.UI.Text uiText = newItem.GetComponentInChildren<UnityEngine.UI.Text>();
      UnityEngine.UI.Image icon = newItem.GetComponentInChildren<UnityEngine.UI.Image>();
      LogTag tag = newItem.GetComponentInChildren<LogTag>();
      bool startVisible = (int)level <= _filterLevel && (_exclusiveCategory == 0 || category == _exclusiveCategory);

      if (tag)
      {
        tag.Level = level;
        tag.Category = category;
      }

      if (icon)
      {
        if (0 <= level && (int)level < _logLevelIcons.Count)
        {
          icon.sprite = _logLevelIcons[(int)level];
        }
        else
        {
          icon.color = new Color32(0, 0, 0, 0);
        }
      }

      newItem.gameObject.SetActive(startVisible);

      if (uiText)
      {
        uiText.text = message;
        Append(newItem);
      }
    }

    public void FilterCategory(int exclusive)
    {
      Filter(1000, exclusive);
    }

    public void Filter(int level, int exclusiveCategory = 0)
    {
      RectTransform content = ScrollView.content.transform as RectTransform;
      for (int i = 0; i < content.childCount; ++i)
      {
        GameObject child = content.GetChild(i).gameObject;
        LogTag tag = child.GetComponent<LogTag>();
        if (tag != null)
        {
          child.SetActive((int)tag.Level <= level &&
              (exclusiveCategory == 0 || tag.Category == exclusiveCategory)
            );
        }
      }

      ScrollView.LayoutContentV();
    }

    [SerializeField]
    private List<Sprite> _logLevelIcons = new List<Sprite>();
    private int _filterLevel = (int)LogLevel.Info;
    private int _exclusiveCategory = 0;
  }
}
