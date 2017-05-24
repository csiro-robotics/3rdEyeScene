using Tes.Handlers;
using Tes.Net;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  /// <summary>
  /// Maintains status icons in the UI.
  /// </summary>
  public class CameraSelect : MonoBehaviour
  {
    [SerializeField]
    private TesComponent _tes = null;
    public TesComponent TesComponent { get { return _tes; } }

    [SerializeField]
    private Dropdown _cameraDropdown = null;
    public Dropdown CameraDropdown { get { return _cameraDropdown; } }

    [SerializeField]
    private Sprite _cameraIcon = null;
    public Sprite CameraIcon { get { return _cameraIcon; } }


    private readonly string RecordedCameraName = "R";
    string CamIDToString(int id)
    {
      return (id != 255) ? id.ToString() : RecordedCameraName;
    }

    int CamStringToID(string str)
    {
      if (string.Compare(str, RecordedCameraName) == 0)
      {
        return 255;
      }
      int id;
      int.TryParse(str, out id);
      return id;
    }

    void Update()
    {
      if (_tes != null)
      {
        UpdateCamera();
      }
    }

    public void OnSelectionChanged()
    {
      // Convert the section to a camera ID.
      if (CameraDropdown.value < CameraDropdown.options.Count)
      {
        var opt = CameraDropdown.options[CameraDropdown.value];
        int camId = CamStringToID(opt.text);
        CameraHandler camHandle = _tes.GetHandler((ushort)RoutingID.Camera) as CameraHandler;
        if (camId == 0)
        {
          camHandle.ActiveCameraID = -1;
        }
        else
        {
          camHandle.ActiveCameraID = camId;
        }
      }
    }

    void UpdateCamera()
    {
      // Update drop down. Easiest to rebuild the list whenever it changes.
      // TODO: optimise
      CameraHandler camHandle = _tes.GetHandler((ushort)RoutingID.Camera) as CameraHandler;
      int insertPos;
      int optId;
      int camId;
      var optionsList = CameraDropdown.options;
      bool optionsChanged = false;

      // Expire unmatched options.
      for (int i = 0; i < optionsList.Count; ++i)
      {
        optId = CamStringToID(optionsList[i].text);
        // Option zero stays.
        if (optId != 0)
        {
          if (camHandle[optId] == null)
          {
            // Remove.
            optionsList.RemoveAt(i);
            --i;
          }
        }
      }

      foreach (int id in camHandle.AvailableCameraIDs)
      {
        // 255 is the recorded camera. Make it appear as option zero.
        camId = id;
        insertPos = optionsList.Count;
        for (int i = 0; i < optionsList.Count; ++i)
        {
          optId = CamStringToID(optionsList[i].text);
          if (camId == optId)
          {
            insertPos = -1;
            break;
          }
          else if (camId > optId)
          {
            // Insert before this one.
            insertPos = i + 1;
          }
        }

        if (insertPos >= 0)
        {
          string camStr = CamIDToString(camId);
          optionsChanged = true;
          if (insertPos < optionsList.Count)
          {
            optionsList.Insert(insertPos, new Dropdown.OptionData { text = camStr, image = CameraIcon });
          }
          else
          {
            optionsList.Add(new Dropdown.OptionData { text = camStr, image = CameraIcon });
          }
        }
      }

      if (optionsChanged)
      {
        CameraDropdown.options = optionsList;
      }

      // Update the dropdown value.
      if (camHandle.ActiveCameraID > 0)
      {
        for (int i = 0; i < optionsList.Count; ++i)
        {
          optId = CamStringToID(optionsList[i].text);
          if (camHandle.ActiveCameraID == optId)
          {
            CameraDropdown.value = i;
            break;
          }
        }
      }
      else
      {
        CameraDropdown.value = 0;
      }
    }
  }
}