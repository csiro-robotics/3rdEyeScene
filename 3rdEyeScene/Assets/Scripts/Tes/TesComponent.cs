using Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using Tes.Logging;
using Tes.Main;
using Tes.Runtime;
using Tes.Handlers;
using Tes.Handlers.Shape2D;
using Tes.Handlers.Shape3D;
using UnityEngine;

/// <summary>
/// Primary component for instantiating the 3rd Eye Scene managers.
/// There should be exactly one object in the scene with this component.
/// </summary>
public class TesComponent : Router
{
  public Material VertexColourLitMaterial;
  public Material VertexColourUnlitMaterial;
  public Material VertexColourLitTwoSidedMaterial;
  public Material VertexColourUnlitTwoSidedMaterial;
  public Material WireframeTriangles;
  public Material VertexColourTransparent;
  public Material PointsLitMaterial;
  public Material PointsUnlitMaterial;
  public Material VoxelsMaterial;
  public FileDialogUI FileDialogUI;

  [SerializeField]
  private InputSystem.InputStack _inputStack = null;
  public InputSystem.InputStack InputStack { get { return _inputStack; } }

  private PluginManager _plugins = new PluginManager();
  public PluginManager Plugins { get { return _plugins; } }

  public string LastBrowseLocation
  {
    get { return PlayerPrefs.GetString("TesBrowseLocation"); }
    set { PlayerPrefs.SetString("TesBrowseLocation", value); }
  }

  public static string FileFilter
  {
    get
    {
      return "3rd Eye Scene Files (*.3es)|*.3es|All Files (*.*)|*.*";
    }
  }

  public static void PrintBounds(Bounds bounds)
  {
    Debug.Log(string.Format("B: ({0}, {1}, {2}), ({3}, {4}, {5})",
      bounds.min.x, bounds.min.y, bounds.min.z,
      bounds.max.x, bounds.max.y, bounds.max.z));
  }

  protected override void Start()
  {
    base.Start();

    Materials.Register(MaterialLibrary.VertexColourLit, VertexColourLitMaterial);
    Materials.Register(MaterialLibrary.VertexColourUnlit, VertexColourUnlitMaterial);
    Materials.Register(MaterialLibrary.VertexColourLitTwoSided, VertexColourLitTwoSidedMaterial);
    Materials.Register(MaterialLibrary.VertexColourUnlitTwoSided, VertexColourUnlitTwoSidedMaterial);
    Materials.Register(MaterialLibrary.WireframeTriangles, WireframeTriangles);
    Materials.Register(MaterialLibrary.VertexColourTransparent, VertexColourTransparent);
    Materials.Register(MaterialLibrary.PointsLit, PointsLitMaterial);
    Materials.Register(MaterialLibrary.PointsUnlit, PointsUnlitMaterial);
    Materials.Register(MaterialLibrary.Voxels, VoxelsMaterial);

    if (Scene != null && Scene.Root != null)
    {
      Scene.Root.transform.parent = this.transform;
    }

    CategoriesHandler categories = new CategoriesHandler();
    MeshCache meshCache = new MeshCache();
    Handlers.Register(meshCache);
    Handlers.Register(categories);

    Handlers.Register(new CameraHandler(categories.IsActive));

    Handlers.Register(new ArrowHandler(categories.IsActive));
    Handlers.Register(new BoxHandler(categories.IsActive));
    Handlers.Register(new CapsuleHandler(categories.IsActive));
    Handlers.Register(new ConeHandler(categories.IsActive));
    Handlers.Register(new CylinderHandler(categories.IsActive));
    Handlers.Register(new PlaneHandler(categories.IsActive));
    Handlers.Register(new SphereHandler(categories.IsActive));
    Handlers.Register(new StarHandler(categories.IsActive));
    Handlers.Register(new MeshHandler(categories.IsActive));
    Handlers.Register(new MeshSetHandler(categories.IsActive, meshCache));
    Handlers.Register(new PointCloudHandler(categories.IsActive, meshCache));
    Handlers.Register(new Text2DHandler(categories.IsActive));
    Handlers.Register(new Text3DHandler(categories.IsActive));

    // Register handlers from plugins.
    string[] loadPaths = new string[]
      {
#if UNITY_EDITOR
      Path.Combine("Assets", "plugins")
#else  // UNITY_EDITOR
      "plugins",
      Path.Combine("3rd-Eye-Scene_Data", "Managed")
#endif // UNITY_EDITOR
    };

    CategoryCheckDelegate catDelegate = categories.IsActive;
    foreach (string loadPath in loadPaths)
    {
      Handlers.LoadPlugins(loadPath, Plugins, "3esRuntime.dll", new object[] { catDelegate });
    }

    InitialiseHandlers();

    categories.OnActivationChange += (ushort categoryId, bool active) =>
    {
      foreach (MessageHandler handler in Handlers.Handlers)
      {
        handler.OnCategoryChange(categoryId, active);
      }
    };

    LoadCommandLineFile();
  }

  void LoadCommandLineFile()
  {
    Options opt = new Options();
    if (opt.Anonymous.Count > 0)
    {
      // Load the first anonymous argument.
      if (!OpenFile(opt.Anonymous[0]))
      {
        Log.Warning("Failed to load command line specified file: {0}", opt.Anonymous[0]);
      }
    }
  }

  void OnApplicationQuit()
  {
    // Try improve shut down time with a partial reset. Let Unity deal with the objects.
    _quitting = true;
    Reset(true);
  }

  void OnDisable()
  {
    // Reset already effected in OnApplicationQuit().
    if (!_quitting)
    {
      Reset();
    }
  }

  public void RecordStop()
  {
    switch (Mode)
    {
    case RouterMode.Recording:
      StopRecording();
      break;
    case RouterMode.Idle: // For record on start.
    case RouterMode.Connected:
    case RouterMode.Connecting:
      SaveFileDialog saveDlg = new SaveFileDialog(new TrueFileSystem(), FileDialogUI, CommonDialogUIs.FindGlobalUI<MessageBoxUI>());
      saveDlg.InitialDirectory = LastBrowseLocation;
      saveDlg.Filter = FileFilter;
      saveDlg.DefaultExt = "3es";
      saveDlg.AddExtension = true;
      _inputStack.SetLayerEnabled("Dialogs", true);
      // Input
      saveDlg.ShowDialog(delegate(CommonDialog dialog, DialogResult result)
      {
        _inputStack.SetLayerEnabled("Dialogs", false);
        SaveFileDialog dlg = dialog as SaveFileDialog;
        if (result == DialogResult.OK && dlg != null)
        {
          string recordFile = dlg.FileName;
          LastBrowseLocation = System.IO.Path.GetDirectoryName(recordFile);
          if (!string.IsNullOrEmpty(recordFile))
          {
            StartRecording(recordFile);
          }
        }
      });
      break;
    default:
      Stop();
      break;
    }
  }

  public void PlayPause()
  {
    if (Mode == RouterMode.Idle)
    {
      OpenFileDialog openDlg = new OpenFileDialog(new TrueFileSystem(), FileDialogUI);
      openDlg.Multiselect = false;
      openDlg.InitialDirectory = LastBrowseLocation;
      openDlg.Filter = FileFilter;
      openDlg.DefaultExt = "3es";
      openDlg.AddExtension = true;
      _inputStack.SetLayerEnabled("Dialogs", true);
      openDlg.ShowDialog(delegate (CommonDialog dialog, DialogResult result)
      {
        _inputStack.SetLayerEnabled("Dialogs", false);
        OpenFileDialog dlg = dialog as OpenFileDialog;
        if (result == DialogResult.OK && dlg != null)
        {
          string openFile = dlg.FileName;
          LastBrowseLocation = System.IO.Path.GetDirectoryName(openFile);
          if (!string.IsNullOrEmpty(openFile))
          {
            OpenFile(openFile);
          }
        }
      });
    }
    else
    {
      TogglePause();
    }
  }

  public void BuildSettingsList(List<Settings> settingsList)
  {
    settingsList.Add(CameraSettings.Instance);
    settingsList.Add(RenderSettings.Instance);
    settingsList.Add(PlaybackSettings.Instance);
  }

  private void OnCategoryActiveChange(ushort categoryId, bool active)
  {
    foreach (MessageHandler handler in Handlers.Handlers)
    {
      handler.OnCategoryChange(categoryId, active);
    }
  }

  /// <summary>
  /// True while quitting the application. Avoids a double reset.
  /// </summary>
  private bool _quitting = false;
}
