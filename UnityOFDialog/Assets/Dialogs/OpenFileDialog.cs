using System.ComponentModel;
using System.IO;
using System.Collections.Generic;

namespace Dialogs
{
  /// <summary>
  /// Implementation of a dialog for selecting a file to read from.
  /// </summary>
  /// <remarks>
  /// General useage is:
  /// <code>
  /// void ShowOpenDialog(FileSystemModel model, FileDialogView ui)
  /// {
  ///   Dialogs.OpenFileDialog dialog = new OpenFileDialog(model, ui);
  ///   dialog.InitialDirectory = LastOpenLocation;
  ///   dialog.ShowDialog(delegate(Dialogs.CommonDialog dlg, Dialogs.DialogResult result)
  ///   {
  ///     if (result == Dialogs.DialogResult.OK)
  ///     {
  ///       LastOpenLocation = System.IO.Path.GetDirectoryName(dialog.FileName);
  ///       Debug.Log(string.Format("Open: {0}", dialog.FileName));
  ///     }
  ///   });
  /// }
  /// </code>
  /// </remarks>
  public class OpenFileDialog : FileDialog
  {
    public OpenFileDialog(FileSystemModel fileSystem, FileDialogView ui)
      : base(FileDialogType.OpenFileDialog, fileSystem, ui)
    {
      Title = "Open File";
    }

    public bool Multiselect
    {
      get { return BMultiselect; }
      set { BMultiselect = value; }
    }

    public string SafeFileName
    {
      get { return FileName ?? ""; }
    }

    public string[] SafeFileNames
    {
      get { return FileNames ?? new string[0]; }
    }

    public override bool CanShowNative
    {
      get
      {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        // Native dialogs only allowed for a TrueFileSystem and only supported for
        // for MacOS and Windows.
        return AllowNative && FileSystem is TrueFileSystem;
#else  // UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        return false;
#endif // UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
      }
    }

    //
    // Methods
    //

    /// <summary>
    /// Called to show the native dialog.
    /// </summary>
    protected override void OnShowNative()
    {
      ShowingNative = true;
      SFB.ExtensionFilter[] extensionFilter = BuildNativeFilter();

      string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel(new SFB.BrowserParameters()
      {
        Title = Title,
        Directory = InitialDirectory,
        Extensions = extensionFilter,
        Multiselect = Multiselect,
        FilterIndex = FilterIndex,
        DefaultExt = DefaultExt,
        AddExtension = AddExtension
      });
      List<string> localFileNames = new List<string>();
      if (paths != null)
      {
        foreach (string path in paths)
        {
          // SFB generates URI paths. Convert from this.
          if (!string.IsNullOrEmpty(path))
          {
            localFileNames.Add(new System.Uri(path).LocalPath);
          }
        }
      }

      if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
      {
        if (OnValidateFiles(localFileNames))
        {
          // We have a result.
          OnFileDialogDone(new CancelEventArgs(false));
          ShowingNative = false;
          return;
        }
      }
      // No result.
      OnFileDialogDone(new CancelEventArgs(true));
      ShowingNative = false;
    }

    public Stream OpenFile()
    {
      if (!string.IsNullOrEmpty(SafeFileName))
      {
        FileMode mode = FileMode.Open;
        return new FileStream(SafeFileName, mode);
      }

      return null;
    }

    public override void Reset()
    {
      base.Reset();
    }
  }
}
