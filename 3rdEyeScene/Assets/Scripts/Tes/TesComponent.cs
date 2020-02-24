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
  public Tes.Net.CoordinateFrame Frame = Tes.Net.CoordinateFrame.ZXY;
  public Material OpaqueInstancedLeftHandedMaterial;
  public Material WireframeInstancedLeftHandedMaterial;
  public Material TransparentInstancedLeftHandedMaterial;
  public Material OpaqueInstancedRightHandedMaterial;
  public Material WireframeInstancedRightHandedMaterial;
  public Material TransparentInstancedRightHandedMaterial;
  public Material OpaqueMeshMaterial;
  public Material OpaqueTwoSidedMeshMaterial;
  public Material TransparentMeshMaterial;
  public Material WireframeMeshMaterial;
  public Material PointsMaterial;
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

  public bool GenerateTextMesh(string text, ref Mesh mesh, ref Material material)
  {
    // TODO: (KS) generate a text mesh and bind material.
    return false;
  }

  protected override void Start()
  {
    base.Start();

    UpdateMaterials();

    if (Scene != null && Scene.Root != null)
    {
      Scene.Root.transform.parent = this.transform;
    }

    CategoriesHandler categories = new CategoriesHandler();
    MeshCache meshCache = new MeshCache();
    Handlers.Register(meshCache);
    Handlers.Register(categories);

    Handlers.Register(new CameraHandler());

    Handlers.Register(new ArrowHandler());
    Handlers.Register(new BoxHandler());
    Handlers.Register(new CapsuleHandler());
    Handlers.Register(new ConeHandler());
    Handlers.Register(new CylinderHandler());
    Handlers.Register(new PlaneHandler());
    Handlers.Register(new SphereHandler());
    Handlers.Register(new StarHandler());
    Handlers.Register(new MeshHandler());
    Handlers.Register(new MeshSetHandler(meshCache));
    Handlers.Register(new PointCloudHandler(meshCache));
    Handlers.Register(new PoseHandler());
    Handlers.Register(new Text2DHandler());
    Text3DHandler text3DHandler = new Text3DHandler();
    text3DHandler.CreateTextMeshHandler = this.GenerateTextMesh;
    Handlers.Register(text3DHandler);

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

    foreach (string loadPath in loadPaths)
    {
      string[] excludeList = new string[] {
        "3esCore.dll",
        "3esRuntime.dll",
        "host*.dll",
        "SharpCompress.dll",
        "System.*.dll"
      };
      Handlers.LoadPlugins(loadPath, Plugins, excludeList, new object[] {});
    }

    InitialiseHandlers();

    HandleCommandLineStart();
  }

  void UpdateMaterials()
  {
    // TODO: (KS) validate the mesh materials in left handed remote scenes.
    if (Tes.Net.CoordinateFrameUtil.LeftHanded(ServerInfo.CoordinateFrame))
    {
      Materials.Register(MaterialLibrary.OpaqueInstanced, OpaqueInstancedLeftHandedMaterial);
      Materials.Register(MaterialLibrary.WireframeInstanced, WireframeInstancedLeftHandedMaterial);
      Materials.Register(MaterialLibrary.TransparentInstanced, TransparentInstancedLeftHandedMaterial);
      Materials.Register(MaterialLibrary.OpaqueMesh, OpaqueMeshMaterial);
      Materials.Register(MaterialLibrary.OpaqueTwoSidedMesh, OpaqueTwoSidedMeshMaterial);
      Materials.Register(MaterialLibrary.TransparentMesh, TransparentMeshMaterial);
      Materials.Register(MaterialLibrary.WireframeMesh, WireframeMeshMaterial);
      Materials.Register(MaterialLibrary.Points, PointsMaterial);
      Materials.Register(MaterialLibrary.Voxels, VoxelsMaterial);
    }
    else
    {
      Materials.Register(MaterialLibrary.OpaqueInstanced, OpaqueInstancedRightHandedMaterial);
      Materials.Register(MaterialLibrary.WireframeInstanced, WireframeInstancedRightHandedMaterial);
      Materials.Register(MaterialLibrary.TransparentInstanced, TransparentInstancedRightHandedMaterial);
      Materials.Register(MaterialLibrary.OpaqueMesh, OpaqueMeshMaterial);
      Materials.Register(MaterialLibrary.OpaqueTwoSidedMesh, OpaqueTwoSidedMeshMaterial);
      Materials.Register(MaterialLibrary.TransparentMesh, TransparentMeshMaterial);
      Materials.Register(MaterialLibrary.WireframeMesh, WireframeMeshMaterial);
      Materials.Register(MaterialLibrary.Points, PointsMaterial);
      Materials.Register(MaterialLibrary.Voxels, VoxelsMaterial);
    }
  }

  void HandleCommandLineStart()
  {
    if (Options.Current.Mode == Options.RunMode.Play)
    {
      // Load the first anonymous argument.
      if (!OpenFile(Options.Current.Values["play"]))
      {
        Log.Warning($"Failed to play command line specified file: {Options.Current.Values["play"]}");
      }
    }
    else if (Options.Current.Mode == Options.RunMode.Normal && Options.Current.Connection != null)
    {
      Connect(Options.Current.Connection, true);
    }
  }

  void OnDestroy()
  {
    Reset();
    GpuBufferManager.Instance.Reset();
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
      saveDlg.AllowNative = UISettings.Instance.NativeDialogs;
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
      openDlg.AllowNative = UISettings.Instance.NativeDialogs;
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
    settingsList.Add(UISettings.Instance);
  }

  protected override void OnServerInfoUpdate()
  {
    Frame = ServerInfo.CoordinateFrame;
  }

  protected override void Update()
  {
    // Debug: Support switching frames in the editor.
    if (Scene.Frame != Frame)
    {
      Scene.Frame = Frame;
    }

    base.Update();
  }
}
