using System.ComponentModel;
using System.IO;
using System.Collections.Generic;

namespace Dialogs
{
  /// <summary>
  /// Implementation of a dialog for selecting a file to save to.
  /// </summary>
  /// <remarks>
  /// General useage is:
  /// <code>
  /// void ShowSaveDialog(FileSystemModel model, FileDialogView ui)
  /// {
  ///   Dialogs.SaveFileDialog dialog = new SaveFileDialog(model, ui);
  ///   dialog.InitialDirectory = LastSaveLocation;
  ///   dialog.ShowDialog(delegate(Dialogs.CommonDialog dlg, Dialogs.DialogResult result)
  ///   {
  ///     if (result == Dialogs.DialogResult.OK)
  ///     {
  ///       LastSaveLocation = System.IO.Path.GetDirectoryName(dialog.FileName);
  ///       Debug.Log(string.Format("Save to: {0}", dialog.FileName));
  ///     }
  ///   });
  /// }
  /// </code>
  /// </remarks>
  public class SaveFileDialog : FileDialog
  {
    public SaveFileDialog(FileSystemModel fileSystem, FileDialogView ui)
      : this(fileSystem, ui, null)
    {
    }

    public SaveFileDialog(FileSystemModel fileSystem, FileDialogView ui, MessageBoxUI confirmUI)
      : base(FileDialogType.SaveFileDialog, fileSystem, ui)
    {
      BMultiselect = false;
      Title = "Save File";
      OverwritePrompt = true;
      ConfirmUI = confirmUI;
    }

    public MessageBoxUI ConfirmUI { get; protected set; }

    [DefaultValue(false)]
    public bool CreatePrompt { get; set; }

    //public bool DefaultExt { get; set; }

    [DefaultValue(true)]
    public bool OverwritePrompt { get; set; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SafeFileName
    {
      get { return FileName ?? ""; }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string[] SafeFileNames
    {
      get { return FileNames ?? new string[0]; }
    }

    public override bool CanShowNative
    {
      get
      {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN_DEPRECATED
        // Native dialogs only allowed for a TrueFileSystem and only supported for
        // for MacOS and Windows.
        return AllowNative && FileSystem is TrueFileSystem;
#else  // UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN_DEPRECATED
        return false;
#endif // UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN_DEPRECATED
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
      // TODO: set DefaultName.
      string path = SFB.StandaloneFileBrowser.SaveFilePanel(new SFB.BrowserParameters()
      {
        Title = Title,
        Directory = InitialDirectory,
        Extensions = extensionFilter,
        FilterIndex = FilterIndex,
        DefaultExt = DefaultExt,
        AddExtension = AddExtension
      });
      List<string> localFileNames = new List<string>();
      if (!string.IsNullOrEmpty(path))
      {
        // SFB generates URI paths. Convert from this.
        localFileNames.Add(new System.Uri(path).LocalPath);

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
        FileMode mode = FileMode.OpenOrCreate;
        return new FileStream(SafeFileName, mode);
      }

      return null;
    }

    public override void Reset()
    {
      base.Reset();
    }

    public override bool OnValidateFiles(IEnumerable<string> filenames)
    {
      bool validationOk = true;
      bool firstFile = true;
      bool fileExists = false;

      _fileNames.Clear();

      foreach (string filename in filenames)
      {
        // Add extension if required.
        string filenameWithDefaultExt = string.Format("{0}.{1}", filename, DefaultExt);
        bool existsAsIs = File.Exists(filename);
        bool existsAsWithDefaultExt = !string.IsNullOrEmpty(DefaultExt) && File.Exists(filenameWithDefaultExt);
        if (!existsAsIs && existsAsWithDefaultExt)
        {
          // Add the extension as required.
          _fileNames.Add(filenameWithDefaultExt);
          fileExists = firstFile;
        }
        else
        {
          _fileNames.Add(filename);
          fileExists = firstFile && existsAsIs;
        }

        if (!firstFile)
        {
          // Requires only one file name.
          validationOk = false;
        }

        firstFile = false;
      }

      if (validationOk && fileExists)
      {
        // In native mode, an overwrite prompt will already have been given.
        if (OverwritePrompt && !ShowingNative)
        {
          // Show overwrite confirmation dialog.
          _validatingPath = _fileNames[0];
          MessageBox.Show(OnValidateClose, "Overwrite existing file?", "Overwrite", MessageBoxButtons.YesNo, ConfirmUI);
          return false;
        }
      }
      // In native mode, a create prompt will have already been given.
      else if (CreatePrompt && !ShowingNative)
      {
        // Show creation confirmation dialog.
        _validatingPath = _fileNames[0];
        MessageBox.Show(OnValidateClose, "Create new file?", "Create", MessageBoxButtons.YesNo, ConfirmUI);
        return false;
      }

      return validationOk;
    }

    protected void OnValidateClose(CommonDialog dialog, DialogResult result)
    {
      if (result == DialogResult.Yes)
      {
        FileName = _validatingPath;
        Close(DialogResult.OK);
      }
      else if (result == DialogResult.Cancel)
      {
        Close(DialogResult.Cancel);
      }
    }

    protected string _validatingPath;
  }
}
