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
  public class FileDialog : CommonDialog, FileDialogViewController
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

    /// <summary>
    /// Can a native dialog be shown?
    /// </summary>
    /// <remarks>
    /// The <see cref="FileDialog"/> does not support native dialogs without further
    /// specialisation.
    /// </remarks>
    public override bool CanShowNative
    {
      get { return false; }
    }

    /// <summary>
    /// True if currently using a native dialog UI, rather than a Unity UI.
    /// </summary>
    /// <remarks>
    /// It is up to derived classes to manage this flag from <see cref="OnShowNative()"/>.
    /// </remarks>
    public bool ShowingNative { get; protected set; }

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
        _activeFilter = GlobToRegex(value);
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
          for (int i = 0; i < filter.Extensions.Length; ++i)
          {
            if (i > 0)
            {
              str.Append(",");
            }
            str.Append("*.");
            str.Append(filter.Extensions[i]);
          }
        }

        return str.ToString();
      }

      set
      {
        _filters.Clear();
        string[] parts = value.Split(new char[] { '|' });
        string[] extsPart;
        string[] exts;
        for (int i = 1; i < parts.Length; i += 2)
        {
          extsPart = parts[i].Split(new char[] { ',' });
          exts = new string[extsPart.Length];
          for (int e = 0; e < extsPart.Length; ++e)
          {
            // Strip any leading "*." or ".".
            if (extsPart[e].StartsWith("*."))
            {
              exts[e] = extsPart[e].Substring(2);
            }
            else if (extsPart[e].StartsWith("."))
            {
              exts[e] = extsPart[e].Substring(1);
            }
            else
            {
              exts[e] = extsPart[e];
            }
          }
          FilterEntry filter = new FilterEntry { Display = parts[i-1], Extensions = exts };
          filter.Expression = GlobToRegex(parts[i]);
          _filters.Add(filter);
        }

        if (UI != null)
        {
          UI.FileFilter = Filter;
        }
      }
    }

    /// <summary>
    /// Index of the active file filter.
    /// </summary>
    /// <remarks>
    /// This is a 1 based index, making the index of the first filter 1, not 0.
    /// </remarks>
    public int FilterIndex
    {
      get { return UI != null ? UI.FileFilterIndex : 0; }
      set { if (UI != null) { UI.FileFilterIndex = value; } }
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
      UI.Controller = this;
      if (DialogType == FileDialogType.OpenFileDialog)
      {
        UI.ConfirmButtonText = "Open";
      }
      else
      {
        UI.ConfirmButtonText = "Save";
      }
    }

    protected override void OnShowNative()
    {
      throw new System.NotSupportedException("FileDialog class does not support native dialogs without further specialisation.");
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
      UI.ShowLinks(FileSystem.GetRoots());
      if (_filters.Count > 0)
      {
        int filterIndexZeroBase = (FilterIndex > 0) ? FilterIndex - 1 : 0;
        UI.ShowItems(entry, FileSystem.ListChildren(entry, _filters[filterIndexZeroBase].Expression));
      }
      else
      {
        UI.ShowItems(entry, FileSystem.ListChildren(entry, new Regex(".*")));
      }
      UI.OnShow();
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
    /// Calls through to <see cref="Close"/> which must consider the state of <see cref="CanShowNative"/>
    /// to determine whether a native dialog was displayed. The default implementation does so.
    ///
    /// The <see cref="_fileNames" member will have been populated with the list of selected file names.
    /// </remarks>
    public virtual void OnFileDialogDone(CancelEventArgs e)
    {
      // Look for confirmed close conditions.
      if (!e.Cancel)
      {
        Close(DialogResult.OK);
      }
      else
      {
        Close(DialogResult.Cancel);
      }
      ShowingNative = false;
    }

    /// <summary>
    /// Validate the given list of files before confirmation.
    /// </summary>
    /// <param name="filenames">List of file paths to validate.</param>
    /// <returns>True when all files are valid and confirmation may be continue with a call to
    /// <see cref="OnFileDialogDone(CanceEventArgs)"/>.</returns>
    /// <remarks>
    /// Called prior to <see cref="OnFileDialogDone(CancelEventArgs)"/> which will only be called when this method
    /// returns <c>true</c>. Otherwise the UI will remain active.
    ///
    /// The default implementation returns <c>false</c> if <paramref name="filenames" is empty,
    /// or one of the files does not exist. The logic considers adding the <see cref="DefaultExt"/>
    /// as part of the file exists check. Valid files are cache in the <see cref="_fileNames"/> member.
    /// </remarks>
    public virtual bool OnValidateFiles(IEnumerable<string> filenames)
    {
      bool validationOk = true;
      _fileNames.Clear();
      foreach (string filename in filenames)
      {
        if (File.Exists(filename))
        {
          _fileNames.Add(filename);
        }
        else
        {
          if (AddExtension && !string.IsNullOrEmpty(DefaultExt) && string.IsNullOrEmpty(Path.GetExtension(filename)))
          {
            // Missing extension.
            string testFile = string.Format("{0}.{1}", filename, DefaultExt);
            if (File.Exists(testFile))
            {
              _fileNames.Add(testFile);
            }
            else
            {
              validationOk = false;
            }
          }
        }
      }

      return validationOk && _fileNames.Count > 0;
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
        _activeFilter = GlobToRegex(UI.ActiveFileFilter);
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
        // Just a directory. Navigate to that directory.
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
    /// Build the native dialog filter pattern based on the <see cref="Filter"/>
    /// and <see cref="FilterIndex"/>.
    /// </summary>
    /// <returns>The native dialog filter filter pattern.</returns>
    /// <remarks>
    /// Only the native Windows API of <c>StandAloneFilterBrowser</c> supports a filter index.
    /// To get around this, we ensure that on other platforms the selected filter it appears first in the list.
    /// This changes the sorting order, but ensures it is selected by default.
    /// </remarks>
    protected SFB.ExtensionFilter[] BuildNativeFilter()
    {
      if (_filters.Count == 0)
      {
        return null;
      }

      SFB.ExtensionFilter[] extFilter = new SFB.ExtensionFilter[_filters.Count];

#if UNITY_STANDALONE_WINDOWS
      for (int i = 0; i < _filters.Count; ++i)
      {
        extFilter[i] = new SFB.ExtensionFilter(_filters[i].Display, _filters[i].Extension);
      }
#else  // UNITY_STANDALONE_WINDOWS
      int insertIndex = 0;

      if (FilterIndex > 0)
      {
        extFilter[insertIndex++] = new SFB.ExtensionFilter(_filters[FilterIndex - 1].Display, _filters[FilterIndex - 1].Extensions);
      }

      for (int i = 0; i < _filters.Count; ++i)
      {
        if (i + 1 != FilterIndex)
        {
          extFilter[insertIndex++] = new SFB.ExtensionFilter(_filters[i].Display, _filters[i].Extensions);
        }
      }
#endif // UNITY_STANDALONE_WINDOWS

      return extFilter;
    }

    /// <summary>
    /// Convert a globbing expression to a regular expression.
    /// </summary>
    /// <param name="glob">The globbing string.</param>
    /// <returns>An equivalent regular expression.</returns>
    public static Regex GlobToRegex(string glob)
    {
      System.Text.StringBuilder rexstr = new System.Text.StringBuilder();
      // To build the filtering regular expression, we convert strings of the form:
      //    *.txt,*.log,*.blah
      // into the regular expression form:
      //    (.*\.txt)|(.*\.log)|(.*\.blah)
      // For this we draw on the filter.Extensions member we just build.
      string[] globs = glob.Split(new char[] { ',' });

      for (int i = 0; i < globs.Length; ++ i)
      {
        if (i > 0)
        {
          rexstr.Append("|");
        }
        rexstr.Append("(");
        rexstr.Append(globs[i].Replace(".", @"\.").Replace("*", ".*").Replace("?", "."));
        rexstr.Append(")");
      }

      return new Regex(rexstr.ToString());
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
      /// The extension strings. Only the extenions themselves are stored (i.e., no "*.").
      /// </summary>
      public string[] Extensions;
      /// <summary>
      /// The regular expression to filter with.
      /// </summary>
      public Regex Expression;
    }

    protected List<FilterEntry> Filters { get { return _filters; } }

    protected List<string> _fileNames = new List<string>();
    protected List<FilterEntry> _filters = new List<FilterEntry>();
    private string _customFilterString = "";
    private Regex _activeFilter = null;
  }
}
