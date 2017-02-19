using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using System.Collections.Generic;

namespace UI
{
  /// <summary>
  /// Settings panel UI management.
  /// </summary>
  /// <remarks>
  /// All settings objects should be added using <see cref="AddSettings(Settings)"/>.
  /// </remarks>
  public class SettingsPanel : MonoBehaviour
  {
    public ScrollRect SettingsScroll = null;
    public RectTransform SettingsEditor = null;
    public Button SettingsButton = null;
    public Color Highlight = Color.white;
    public TesComponent tes = null;

    void Start()
    {
      if (_spacer == null)
      {
        _spacer = Util.CreateSpacer();
        _spacer.transform.SetParent(SettingsScroll.content.transform, false);
      }
      if (tes != null)
      {
        List<Settings> settingsList = new List<Settings>();
        tes.BuildSettingsList(settingsList);
        foreach (Settings settings in settingsList)
        {
          AddSettings(settings);
        }
      }
      Populate();
    }

    public void AddSettings(Settings settings)
    {
      AddSettings(settings.Name, settings);
    }

    public void AddSettings(string name, INotifyPropertyChanged settings)
    {
      _names.Add(name);
      _settings.Add(settings);
      AddButton(name, settings);
    }

    void OnEnable()
    {
      tes.InputStack.SetLayerEnabled("Settings", true);
      ClearActive();
    }

    void OnDisable()
    {
      tes.InputStack.SetLayerEnabled("Settings", false);
      ClearActive();
    }

    void Clear()
    {
      // Remove the existing buttons.
      foreach (Button old in SettingsScroll.content.GetComponentsInChildren<Button>())
      {
        old.transform.SetParent(null, false);
        Destroy(old.gameObject);
      }
      _spacer.transform.SetParent(null, false);
    }

    void Populate()
    {
      Clear();
      // Make buttons for settings.
      for (int i = 0; i < _settings.Count; ++i)
      {
        AddButton(_names[i], _settings[i]);
      }

      // Push the spacer to the end.
      _spacer.transform.SetParent(SettingsScroll.content.transform, false);
    }

    Button AddButton(string name, INotifyPropertyChanged settings)
    {
        Button entry = Instantiate(SettingsButton);
        entry.GetComponentInChildren<Text>().text = name;
        entry.onClick.AddListener(() => Display(entry, name, settings));
        entry.transform.SetParent(SettingsScroll.content.transform, false);
        return entry;
    }

    private void ClearActive()
    {
      Display(null, null, null);
    }

    void Display(Button button, string name, INotifyPropertyChanged settings)
    {
      if (_activeButton)
      {
        ColorBlock colours = _activeButton.colors;
        colours.normalColor = _cachedColour;
        _activeButton.colors = colours;
      }

      _activeButton = button;

      if (_activeButton)
      {
        ColorBlock colours = _activeButton.colors;
        _cachedColour = colours.normalColor;
        colours.normalColor = Highlight;
        _activeButton.colors = colours;
        SettingsEditor.gameObject.SetActive(true);
        UI.Properties.PropertiesView view = SettingsEditor.GetComponentInChildren<UI.Properties.PropertiesView>();
        // if (view != null)
        view.SetTarget(settings);
      }
      else
      {
      SettingsEditor.gameObject.SetActive(false);
      }
    }

    private LayoutElement _spacer = null;
    private List<INotifyPropertyChanged> _settings = new List<INotifyPropertyChanged>();
    private List<string> _names = new List<string>();
    private Button _activeButton = null;
    private Color _cachedColour = Color.white;
  }
}
