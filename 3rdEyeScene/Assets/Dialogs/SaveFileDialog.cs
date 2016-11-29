using System;
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

    //
    // Methods
    //
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

    protected override bool ValidateFiles(List<string> filenames)
    {
      // Ensure extensions are added as required.
      base.ValidateFiles(filenames);
      if (filenames == null || filenames.Count == 0)
      { 
        return true;
      }

      if (File.Exists(filenames[0]))
      {
        if (OverwritePrompt)
        {
          // Show overwrite confirmation dialog.
          _validatingPath = filenames[0];
          MessageBox.Show(OnValidateClose, "Overwrite existing file?", "Overwrite", MessageBoxButtons.YesNo, ConfirmUI);
          return false;
        }
      }
      else if (CreatePrompt)
      {
        // Show creation confirmation dialog.
        _validatingPath = filenames[0];
        MessageBox.Show(OnValidateClose, "Create new file?", "Create", MessageBoxButtons.YesNo, ConfirmUI);
        return false;
      }
      return true;
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
