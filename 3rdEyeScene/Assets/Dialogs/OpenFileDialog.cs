using System;
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

    //
    // Methods
    //
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
