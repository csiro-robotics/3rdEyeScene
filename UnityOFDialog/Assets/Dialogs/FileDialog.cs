using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Dialogs
{
  /// <summary>
  /// The base class for the <see cref="OpenFileDialog"/> and <see cref="SaveFileDialog"/>.
  /// </summary>
  /// <remarks>
  /// This models the <c>System.Windows.Forms.SaveFileDialog</c> in general operation.
  /// An exact API match is not possible due to engine constraints - for example, blocking is not
  /// possible - and some features are not yet implemented.
  /// 
  /// Due to the Unity engine differences from Windows Forms, the dialog must be provided
  /// with a seperate object which represents the user interface. For flexibility, the dialog
  /// also supports an interface class to describe the file system. The interfaces used
  /// are <see cref="FileDialogView"/> and <see cref="FileSystemModel"/> respectively.
  /// 
  /// The <see cref="FileDialogView"/> must be implemented by a <see cref="UnityEngine.MonoBehaviour"/>
  /// object which implements the user interface objects. The default implementation is <see cref="FileDialogUI"/>
  /// which uses the Unity uGUI system. A default implementation object is also provided, but the dialog UI
  /// may be attached to any compatible UI object.
  /// 
  /// The <see cref="FileSystem"/> class implements the <see cref="FileSystemModel"/> as a reflection of
  /// the local file system. However, a logical file system may be provided, such as a virtual, in game
  /// file system, by providing an alternative implementation.
  /// 
  /// The <see cref="OpenFileDialog"/> and <see cref="SaveFileDialog"/> classes accept a view and file system
  /// model on construction and pass it to this class.
  /// </remarks>
  public class FileDialog : CommonDialog, FileDialogViewObserver
  {
    /// <summary>
    /// Defines the file dialog type.
    /// </summary>
    protected enum FileDialogType
    {
      /// <summary>
      /// Open an existing file for reading.
      /// </summary>
      OpenFileDialog,
      /// <summary>
      /// Create or open a file for writing.
      /// </summary>
      SaveFileDialog
    }

    /// <summary>
    /// The object defining the displayed file system.
    /// </summary>
    public FileSystemModel FileSystem { get; protected set; }
    /// <summary>
    /// The object used to show the user interface.
    /// </summary>
    public FileDialogView UI { get; protected set; }

    /// <summary>
    /// Automatically add the extension to the selected file if none provided?
    /// </summary>
    public bool AddExtension { get; set; }

    /// <summary>
    /// Multiselect file mode?
    /// </summary>
    internal bool BMultiselect
    {
      get { return UI != null ? UI.Multiselect : false; }
      set { if (UI != null) UI.Multiselect = value; }
    }

    //[DefaultValue(false)]
    //public virtual bool CheckFileExists { get; set; }

    //[DefaultValue(true)]
    //public virtual bool CheckPathExists { get; set; }

    /// <summary>
    /// Maintains the custom file filter - e.g., *.exe
    /// </summary>
    internal string CustomFilter
    {
      get
      {
        return _customFilterString;
      }

      set
      {
        _customFilterString = value;
        _activeFilter = new Regex(value.Replace(".", @"\.").Replace("*", ".*"));
      }
    }

//    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//    public FileDialogCustomPlacesCollection CustomPlaces
//    {
//      get;
//    }

    /// <summary>
    /// The default extension to add if none provided. Requires <see cref="AddExtension"/>.
    /// </summary>
    /// <remarks>
    /// Specified without the leading '.' character.
    /// </remarks>
    [DefaultValue("")]
    public string DefaultExt { get; set; }

    [DefaultValue(true)]
    public bool DereferenceLinks { get; set; }

    internal virtual string DialogTitle { get { return ""; } }

    [DefaultValue("")]
    public string FileName
    {
      get
      {
        return _fileNames.Count > 0 ? _fileNames[0] : "";
      }

      set
      {
        _fileNames.Clear();
        _fileNames.Add(value);
      }
    }

    [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string[] FileNames { get { return _fileNames.ToArray(); } }

    //internal string FileTypeLabel { set; }

    /// <summary>
    /// Identifies the available file filters.
    /// </summary>
    /// <remarks>
    /// Filters are supplied in the following format:
    /// <code>
    ///   "Display1|Filter1|Display2|Filter2|..."
    /// </code>
    /// 
    /// The display item is what the user sees, while the filter is what is logically applied.
    /// For example, below is a filter for exectuable, text or all files:
    /// <code>
    ///   "Executable Files (*.exe)|*.exe|Text Files (*.txt)|*.txt|All files (*.*)|*.*"
    /// </code>
    /// 
    /// The filter string may contain a list of semicolon separated items. Below is an example
    /// or a JPEG image filter supporting two JPEG extensions:
    /// <code>
    ///   "JPEG Files (*.jpeg;*.jpg)|*.jpeg;*.jpg|All files (*.*)|*.*"
    /// </code>
    /// </remarks>
    [DefaultValue("")]//, Localizable(true)]
    public string Filter
    {
      get
      {
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        bool first = true;
        foreach (FilterEntry filter in _filters)
        {
          if (!first)
          {
            str.Append("|");
          }
          first = false;
          str.Append(filter.Display);
          str.Append("|");
          str.Append(filter.Extension);
        }

        return str.ToString();
      }

      set
      {
        _filters.Clear();
        string[] parts = value.Split(new char[] { '|' });
        string rexstr;
        for (int i = 1; i < parts.Length; i += 2)
        {
          FilterEntry filter = new FilterEntry { Display = parts[i-1], Extension = parts[i] };
          rexstr = parts[i].Replace(".", @"\.").Replace('?', '.').Replace("*", ".*").Replace(";", "|");
          filter.Expression = new Regex(rexstr);
          _filters.Add(filter);
        }
      }
    }

    /// <summary>
    /// Index of the active file filter.
    /// </summary>
    public int FilterIndex
    {
      get { return UI != null ? UI.FileFilterIndex : 0; }
      set { if (UI != null) UI.FileFilterIndex = value; }
    }

    /// <summary>
    /// The starting directory (if any).
    /// </summary>
    public string InitialDirectory { get; set; }

    //protected int Options { get { return 0; } }

    //internal virtual bool ReadOnlyChecked { get; set; }

    //public bool RestoreDirectory { get; set; }

    //internal string SearchSaveLabel { get; set; }

    //public bool ShowHelp { get; set; }

    //internal virtual bool ShowReadOnly { get; set; }

    // [DefaultValue(false)]
    // public bool SupportMultiDottedExtensions { get; set; }

    /// <summary>
    /// The title text.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="type">The dialog type.</param>
    /// <param name="fileSystem">File system model to represent.</param>
    /// <param name="dialogView">The dialog UI.</param>
    protected FileDialog(FileDialogType type, FileSystemModel fileSystem, FileDialogView dialogView)
    {
      DialogType = type;
      FileSystem = fileSystem;
      UI = dialogView;
      DialogPanel = dialogView.UI;
      UI.Observer = this;
      if (DialogType == FileDialogType.OpenFileDialog)
      {
        UI.ConfirmButtonText = "Open";
      }
      else
      {
        UI.ConfirmButtonText = "Save";
      }
    }

    /// <summary>
    /// Called when the dialog is shown.
    /// </summary>
    /// <remarks>
    /// Dialog confirm button text yet to be localised.
    /// </remarks>
    protected override void OnShow()
    {
      UI.Title = Title;
      FileSystemEntry entry = FileSystem.FromPath(InitialDirectory);
      if (entry.Type == FileItemType.Null)
      {
        entry = FileSystem.Home;
      }
      if (Filter.Length == 0)
      {
        Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
      }
      UI.ShowLinks(FileSystem.GetRoots());
      UI.ShowItems(entry, FileSystem.ListChildren(entry, _filters[0].Expression));
      UI.FileFilter = Filter;
      FilterIndex = 0;
    }

    /// <summary>
    /// Reset the dialog for reuse.
    /// </summary>
    public override void Reset()
    {
      base.Reset();
      _fileNames.Clear();
    }

    /// <summary>
    /// Callback from the UI for when the UI has completed.
    /// </summary>
    /// <param name="e">Callback event. Cancel is true when cancelling, false otherwise.</param>
    /// <remarks>
    /// Calls through to <see cref="Close"/>
    /// </remarks>
    public virtual void OnFileDialogDone(CancelEventArgs e)
    {
      if (e.Cancel)
      {
        Close(DialogResult.Cancel);
      }
      else
      {
        // Update file names from the UI.
        _fileNames.Clear();
        foreach (string filename in UI.Filenames)
        {
          _fileNames.Add(filename);
        }

        if (_fileNames.Count > 0 && ValidateFiles(_fileNames))
        { 
          Close(DialogResult.OK);
        }
      }
      // if (FileOk != null)
      // {
      //   FileOk(this, e);
      // }
    }

    /// <summary>
    /// Callback handling UI changes.
    /// </summary>
    /// <param name="args">Details of the change.</param>
    public virtual void OnFileDialogChange(FileDialogViewEventArgs args)
    {
      if (args.Target == "Location")
      {
        // Update the browser location.
        FileSystemEntry location = FileSystem.FromPath(args.Value as string);
        UI.ShowItems(location, FileSystem.ListChildren(location, _activeFilter));
      }
      else if (args.Target == "FileFilter")
      {
        string restr = UI.FileFilter;
        restr = restr.Replace(".", @"\.").Replace('?', '.').Replace("*", ".*");
        _activeFilter = new Regex(restr);
        // Refresh display.
        FileSystemEntry location = UI.CurrentLocation;
        NavigateTo(location, false);
      }
      else if (args.Target == "FileFilterIndex")
      {
        if (0 <= UI.FileFilterIndex && UI.FileFilterIndex < _filters.Count)
        {
          _activeFilter = _filters[UI.FileFilterIndex].Expression;
          FileSystemEntry location = UI.CurrentLocation;
          NavigateTo(location, false);
        }
      }
    }

    /// <summary>
    /// UI navigation callback. Navigates to the new location.
    /// </summary>
    /// <param name="target">Target to nagivate to.</param>
    /// <param name="toTargetsParent">True if we want to navigate to the parent of
    /// <paramref name="target"/> instead of <paramref name="target"/> itself.
    /// </param>
    public virtual void FileDialogNavigate(FileSystemEntry target, bool toTargetsParent)
    {
      if (toTargetsParent)
      {
        target = FileSystem.GetParent(target);
      }
      UI.ShowItems(target, FileSystem.ListChildren(target, _activeFilter));
    }

    /// <summary>
    /// UI callback to attempt to set an explicit path location.
    /// May change directories, may complete the dialog.
    /// </summary>
    /// <param name="fullPath">The path to attempt to set (file or directory).</param>
    /// <returns>True if the <paramref name="fullPath"/> can be handled.</returns>
    /// <remarks>
    /// Handles three main possibilities:
    /// <list type="table">
    /// <listheader><term>Specified</term><description>Action</description></listheader>
    /// <item>
    ///   <term>File specified (existing or not)</term>
    ///   <description>
    ///   Change to the directory of and select the file.
    ///   </description>
    /// </item>
    /// <item><term>Directry specified</term><description>Navigate to the directory.</description></item>
    /// <item><term>Filter specified</term><description>Apply the custom filter to the current directory.</description></item>
    /// </list>
    /// </remarks>
    public virtual bool SetFileDialogLocation(string fullPath)
    {
      // Three possibilities:
      // - file item specified (existing or not)
      // - directory specified
      // - filter specified

      if (string.IsNullOrEmpty(fullPath))
      {
        return false;
      }

      if (Directory.Exists(fullPath))
      {
        // Just a directory. Nagivate to that directory.
        NavigateTo(FileSystem.FromPath(fullPath));
        return true;
      }

      string dir = Path.GetDirectoryName(fullPath);
      string filename = Path.GetFileName(fullPath);

      bool dirOk = true;
      if (!string.IsNullOrEmpty(dir))
      {
        // We have a directory party. Navigate to that first.
        if (Directory.Exists(dir))
        {
          NavigateTo(FileSystem.FromPath(dir));
        }
        else
        {
          dirOk = false;
        }
      }

      // Good on the directory part. Now update the file name or filter part.
      if (dirOk && !string.IsNullOrEmpty(filename))
      {
        if (filename.Contains("*"))
        {
          // Custom filter expression.
          CustomFilter = filename;
          FileSystemEntry location = UI.CurrentLocation;
          NavigateTo(location, false);
          return true;
        }
        else
        {
          // File name specified. Complete the dialog if:
          // 1. This is a SaveFileDialog.
          // 2. This is an OpenFileDialog and the file exists.
          string fullFilePath = Path.Combine(UI.CurrentLocation.FullName, filename);
          bool completeDialog = false;
          _fileNames.Clear();

          if (ValidateCompletionPath(fullFilePath))
          {
            _fileNames.Add(fullFilePath);
            completeDialog = true;
          }
          return completeDialog;
        }
      }

      return false;
    }

    /// <summary>
    /// Validate the final list of files, adding extensions as required (<see cref="AddExtension"/>).
    /// </summary>
    /// <param name="filenames">The list of selected files to validate.</param>
    /// <returns>true if validation is OK and the dialog can complete.</returns>
    protected virtual bool ValidateFiles(List<string> filenames)
    {
      if (AddExtension && !string.IsNullOrEmpty(DefaultExt))
      {
        for (int i = 0; i < filenames.Count; ++i)
        {
          if (!File.Exists(filenames[i]))
          { 
            if (string.IsNullOrEmpty(Path.GetExtension(filenames[i])))
            {
              // Missing extension.
              filenames[i] = string.Format("{0}.{1}", filenames[i], DefaultExt);
            }
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Validate a single path for completion.
    /// </summary>
    /// <param name="fullFilePath">The path to validate.</param>
    /// <returns>True if the path is usable for completion.</returns>
    /// <remarks>
    /// Used by <see cref="SetFileDialogLocation"/>.
    /// 
    /// For open file dialogs, the path must exist.
    /// </remarks>
    protected virtual bool ValidateCompletionPath(string fullFilePath)
    {
      if (DialogType == FileDialogType.SaveFileDialog)
      {
        return true;
      }
      else if (DialogType == FileDialogType.OpenFileDialog)
      {
        if (File.Exists(fullFilePath))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Navigate to the given location.
    /// </summary>
    /// <param name="location">The target location.</param>
    /// <param name="updateLocation">True to update the CurrentLocation in the UI.</param>
    protected void NavigateTo(FileSystemEntry location, bool updateLocation = true)
    {
      if (location.Type != FileItemType.Null)
      {
        if (updateLocation)
        { 
          UI.CurrentLocation = location;
        }
        UI.ShowItems(UI.CurrentLocation, FileSystem.ListChildren(location, _activeFilter));
      }
    }

    /// <summary>
    /// The dialog type.
    /// </summary>
    protected FileDialogType DialogType { get; private set; }

    /// <summary>
    /// File filter details.
    /// </summary>
    protected struct FilterEntry
    {
      /// <summary>
      /// The string to display.
      /// </summary>
      public string Display;
      /// <summary>
      /// The extension string.
      /// </summary>
      public string Extension;
      /// <summary>
      /// The regular expression to filter with.
      /// </summary>
      public Regex Expression;
    }

    private List<string> _fileNames = new List<string>();
    private List<FilterEntry> _filters = new List<FilterEntry>();
    private string _customFilterString = "";
    private Regex _activeFilter = null;
  }
}
