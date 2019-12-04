using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System;

namespace Dialogs
{
  /// <summary>
  /// The default implementation for the <see cref="FileDialogView"/> used to provide a GUI to the user.
  /// </summary>
  /// <remarks>
  /// The default implementation is modeled on the Windows file browser, which is very similarly
  /// to the Ubuntu Unity browser. This script is attached to the GUI object and various members
  /// must be correctly bound to their respective GUI object. The system as a whole uses the uGUI
  /// system.
  /// </remarks>
  public class FileDialogUI : MonoBehaviour, FileDialogView
  {
    public float DoubleClickWindow = 0.3f;
    [SerializeField]
    protected RectTransform _ui;
    [SerializeField]
    protected Text _titleText;
    [SerializeField]
    protected InputField _locationInput;
    [SerializeField]
    protected InputField _filenameInput;
    [SerializeField]
    protected Dropdown _filterDropdown;
    [SerializeField]
    protected Button _confirmButton;
    [SerializeField]
    protected Button _cancelButton;
    /// <summary>
    /// The game objects to clone in order to create icon representations for a drive or file.
    /// The array caters for different display modes. See <see cref="FileDisplayMode"/>.
    /// </summary>
    [SerializeField]
    protected GameObject[] _iconItems = new GameObject[Enum.GetNames(typeof(FileDisplayMode)).Length];
    [SerializeField]
    protected ScrollRect _locationView;
    [SerializeField]
    protected ScrollRect _filesView;
    [SerializeField]
    protected FileDialogIcons _iconSet;
    [SerializeField]
    protected FileDisplayMode _displayMode = FileDisplayMode.Large;
    public FileDisplayMode DisplayMode { get { return _displayMode; } set { _displayMode = value; } }

    /// <summary>
    /// Does the file view expand horizontally? True for <see cref="DisplayMode.Small" />.
    /// </summary>
    /// <value></value>
    public bool HorizontalExpandingFileView { get { return DisplayMode == FileDisplayMode.Small; } }

    [SerializeField]
    protected int[] _iconModeWidths = new int[] { 0, 340, 300 };

    /// <summary>
    /// Controls the width of the icon item in <see cref="FileDisplayMode.List"/>.
    /// </summary>
    public int[] ListModeItemWidths { get { return _iconModeWidths; } }
    /// <summary>
    /// Set the display width for icons when using <paramref name="mode"/>.
    /// </summary>
    /// <param name="mode">The display mode to set the width for.</param>
    /// <param name="width">The display width to use.</param>
    void SetListmodeItemWidth(FileDisplayMode mode, int width) { _iconModeWidths[(int)mode] = width; }
    /// <summary>
    /// Get the display width for icons when using <paramref name="mode"/>.
    /// </summary>
    /// <param name="mode">The display mode to query the width for.</param>
    /// <returns>The display width for the selected mode.</returns>
    int GetListmodeItemWidth(FileDisplayMode mode) { return _iconModeWidths[(int)mode]; }

    [SerializeField]
    protected bool _autoGridLayoutSizing = true;
    /// <summary>
    /// Adjust the scroll rect layout sizing to match the item size? Only affects grid layout.
    /// </summary>
    public bool AutoGridLayoutSizing
    {
      get { return _autoGridLayoutSizing; }
      set { _autoGridLayoutSizing = value; }
    }
    [SerializeField]
    protected bool _autoScrollSensitivity = true;
    /// <summary>
    /// Automatically adjust the scrolling sensitivity to match the item size?
    /// </summary>
    public bool AutoScrollSensitivity
    {
      get { return _autoScrollSensitivity; }
      set { _autoScrollSensitivity = value; }
    }

    #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    private KeyCode[] _addSelectionKeys = new KeyCode[] { KeyCode.LeftCommand, KeyCode.RightCommand };
    #else  // UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    private KeyCode[] _addSelectionKeys = new KeyCode[] { KeyCode.LeftControl, KeyCode.RightControl };
    #endif // UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    public KeyCode[] AddSelectionKeys { get { return _addSelectionKeys; } }
    private KeyCode[] _extendSelectionKeys = new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift };
    public KeyCode[] ExtendSelectionKeys { get { return _extendSelectionKeys; } }

    public FileDialogViewController Controller { get; set; }

    public RectTransform UI { get { return _ui; } }

    public bool Multiselect { get; set; }

    /// <summary>
    /// The dialog title.
    /// </summary>
    /// <value>The curent dialog title text.</value>
    public string Title
    {
      get { return _titleText != null ? _titleText.text : ""; }
      set
      {
        if (_titleText != null)
        {
          _titleText.text = value;
        }
      }
    }

    /// <summary>
    /// User display of the current location.
    /// </summary>
    /// <value>The current location text.</value>
    public string Location
    {
      get { return _locationInput != null ? _locationInput.text : ""; }
      set
      {
        if (_locationInput != null)
        {
          _locationInput.text = value;
          NotifyChange("Location", value);
        }
      }
    }

    /// <summary>
    /// The current file name.
    /// </summary>
    /// <value>The current filename text.</value>
    public string Filename
    {
      get
      {
        if (_selectedFiles.Count > 0)
        {
          return _selectedFiles[0].Entry.FullName;
        }
        // For file open dialogs return the current input text.
        if (!string.IsNullOrEmpty(FilenameDisplay))
        {
          return Path.Combine(Location, FilenameDisplay);
        }
        return string.Empty;
      }
    }

    public IEnumerable<string> Filenames
    {
      get
      {
        if (_selectedFiles.Count > 0)
        {
          for (int i = 0; i < _selectedFiles.Count; ++i)
          {
            yield return _selectedFiles[i].Entry.FullName;
          }
        }
        else
        {
          string singleFile = Filename;
          if (!string.IsNullOrEmpty(singleFile))
          {
            yield return singleFile;
          }
        }
      }
    }

    /// <summary>
    /// The current file name display.
    /// </summary>
    /// <value>The current filename text.</value>
    public string FilenameDisplay
    {
      get { return _filenameInput != null ? _filenameInput.text : ""; }
      set
      {
        if (_filenameInput != null)
        {
          _filenameInput.text = value;
        }
      }
    }

    /// <summary>
    /// Access the file type filter.
    /// </summary>
    /// <value>The current file type filter text.</value>
    public string FileFilter
    {
      get
      {
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        for (int i = 0; i < _fileFilterDescriptions.Length; ++i)
        {
          if (i != 0)
          {
            str.Append("|");
          }
          str.Append(_fileFilterDescriptions[i]);
          str.Append("|");
          str.Append(_fileFilters[i]);
        }
        return str.ToString();
      }

      set
      {
        // Parse the input text. Expected format is like:
        // Text files (*.txt)|*.txt|All files (*.*)|*.*
        string[] filters = value.Split(new char[] { '|' });
        _fileFilters = new string[filters.Length / 2];
        _fileFilterDescriptions = new string[filters.Length / 2];
        // An even numer is required.
        for (int i = 0; i + 1 < filters.Length; i += 2)
        {
          _fileFilterDescriptions[i / 2] = filters[i];
          _fileFilters[i / 2] = filters[i + 1];
        }

        if (_filterDropdown != null)
        {
          _filterDropdown.ClearOptions();
          List<string> options = new List<string>();
          for (int i = 0; i + 1 < filters.Length; i += 2)
          {
            options.Add(filters[i]);
          }
          _filterDropdown.AddOptions(options);
          _filterDropdown.value = 0;
        }

        NotifyChange("FileFilter", value);
      }
    }

    public int FileFilterIndex
    {
      get { return _filterDropdown != null ? _filterDropdown.value : 0; }
      set
      {
        if (_filterDropdown != null)
        {
          _filterDropdown.value = value;
          NotifyChange("FileFilterIndex", value);
          // Clear the custom filter. This will notify the active filter change as well.
          CustomFileFilter = string.Empty;
        }
      }
    }

    /// <summary>
    /// Handle enter as dialog confirmation.
    /// </summary>
    void Update()
    {
      if (Input.GetButton("Cancel"))
      {
        OnCancel();
        return;
      }
      if (Input.GetKey(KeyCode.Return) && _lastInputFocus != null)
      {
        if (_filenameInput == _lastInputFocus && _filenameInput.text.Length > 0)
        {
          // Confirm the selection.
          if (OnFilenameInputChanged())
          {
            OnConfirm();
          }
        }
        else if (_locationInput == _lastInputFocus)
        {
          OnLocationInputChanged();
        }
      }
      _lastInputFocus = null;
      if (_filenameInput != null && _filenameInput.isFocused)
      {
        _lastInputFocus = _filenameInput;
      }
      if (_locationInput != null && _locationInput.isFocused)
      {
        _lastInputFocus = _locationInput;
      }
    }

    /// <summary>
    /// Event handler for when the file filter index is changed in the UI.
    /// </summary>
    public void OnFilterIndexChange()
    {
      NotifyChange("FileFilterIndex", FileFilterIndex);
      // Clear the custom filter. This will notify the active filter change as well.
      CustomFileFilter = string.Empty;
    }

    private string _customFileFilter = string.Empty;
    public string CustomFileFilter
    {
      get
      {
        return _customFileFilter;
      }

      set
      {
        _customFileFilter = value;
        NotifyChange("CustomFileFilter", value);
        NotifyChange("ActiveFileFilter", value);
      }
    }

    public string ActiveFileFilter
    {
      get
      {
        // Prefer the custom filter.
        string filter = CustomFileFilter;
        if (!string.IsNullOrEmpty(filter))
        {
          return filter;
        }
        int filterIndex = FileFilterIndex;
        if (filterIndex >= 0 && filterIndex < _fileFilters.Length)
        {
          return _fileFilters[filterIndex];
        }
        // Catchall
        return string.Empty;
      }
    }

    public void OnLocationInputChanged()
    {
      if (Controller != null && !SuppressEvents)
      {
        Controller.SetFileDialogLocation(Location);
      }
    }

    /// <summary>
    /// Handles user input to the filename field.
    /// </summary>
    /// <remarks>
    /// Input can be handled in one of three ways:
    /// <list type="bullet">
    /// <item><description>File name entry</description></item>
    /// <item><description>Directory change</description></item>
    /// <item><description>Custom filter</description></item>
    /// </list>
    /// </remarks>
    public bool OnFilenameInputChanged()
    {
      _selectedFiles.Clear();
      if (Controller != null && !SuppressEvents)
      {
        _pendingSelection = FilenameDisplay;
        return Controller.SetFileDialogLocation(FilenameDisplay);
      }
      return false;
    }

    /// <summary>
    /// Access the text for the confirmation button.
    /// </summary>
    /// <value>Current confirmation text.</value>
    public string ConfirmButtonText
    {
      get
      {
        if (_confirmButton != null && _confirmButton.GetComponentInChildren<Text>() != null)
        {
          return _confirmButton.GetComponentInChildren<Text>().text;
        }
        return string.Empty;
      }

      set
      {
        if (_confirmButton != null && _confirmButton.GetComponentInChildren<Text>() != null)
        {
          _confirmButton.GetComponentInChildren<Text>().text = value;
          NotifyChange("ConfirmButtonText", value);
        }
      }
    }
    /// <summary>
    /// Access the text for the cancel button.
    /// </summary>
    /// <value>Current cancel text.</value>
    public string CancelButtonText
    {
      get
      {
        if (_cancelButton != null && _cancelButton.GetComponent<Text>() != null)
        {
          return _cancelButton.GetComponent<Text>().text;
        }
        return string.Empty;
      }

      set
      {
        if (_cancelButton != null && _cancelButton.GetComponent<Text>() != null)
        {
          _cancelButton.GetComponent<Text>().text = value;
          NotifyChange("CancelButtonText", value);
        }
      }
    }

    /// <summary>
    /// Access the currently selected item.
    /// </summary>œ
    /// <value>Details of the current file.</value>
    public FileSystemEntry CurrentFile
    {
      get;
      set;
    }

    public FileSystemEntry CurrentLocation { get; set; }

    /// <summary>
    /// Called to update the links view to display the given items.
    /// For example, the <paramref name="locations"/> may specify a list of drives.
    /// </summary>
    /// <param name="locations">The locations items.</param>
    public void ShowLinks(IEnumerable<FileSystemEntry> locations)
    {
      int itemCount = PopulateScrollRect(_locationView, _iconItems[(int)FileDisplayMode.Large], locations, _iconSet,
                                         0, true);
      if (_locationView != null)
      {
        // Bind selection events. Can't be done easily in static code.
        for (int i = 0; i < _locationView.content.childCount; ++i)
        {
          FileEntryComponent itemUI = _locationView.content.GetChild(i).GetComponent<FileEntryComponent>();
          foreach (Button button in itemUI.GetComponentsInChildren<Button>())
          {
            button.onClick.AddListener(delegate() { this.OnSelectLocation(itemUI); });
          }
        }
      }

      if (itemCount != 0)
      {
        StartCoroutine(FixSizeAtEndOfFrame(_locationView, _iconItems[(int)FileDisplayMode.Large], true));
      }
    }

    /// <summary>
    /// Called to the main view to display the given items.
    /// </summary>
    /// <param name="items">The items to display.</param>
    public void ShowItems(FileSystemEntry location, IEnumerable<FileSystemEntry> items)
    {
      CurrentLocation = location;
      SuppressEvents = true;
      Location = location.FullName;
      SuppressEvents = false;
      FilenameDisplay = string.Empty;
      int itemCount = PopulateScrollRect(_filesView, _iconItems[(int)DisplayMode], items, _iconSet,
                                         _iconModeWidths[(int)DisplayMode], !HorizontalExpandingFileView);
      if (_filesView != null)
      {
        FileEntryComponent initialSelection = null;
        // Bind selection events. Can't be done easily in static code.
        for (int i = 0; i < _filesView.content.childCount; ++i)
        {
          FileEntryComponent itemUI = _filesView.content.GetChild(i).GetComponent<FileEntryComponent>();
          foreach (Button button in itemUI.GetComponentsInChildren<Button>())
          {
            button.onClick.AddListener(delegate() { this.OnSelectFile(itemUI); });
            if (!string.IsNullOrEmpty(_pendingSelection) &&
                itemUI.Entry.FullName.CompareTo(_pendingSelection) == 0)
            {
              initialSelection = itemUI;
            }
          }
        }

        // Make initial selection.
        if (initialSelection)
        {
          OnSelectFile(initialSelection);
        }
      }

      if (itemCount != 0 && isActiveAndEnabled)
      {
        // Performed delayed fixup of the scroll view sizing. We have to wait in order to get the item sizes right.
        StartCoroutine(FixSizeAtEndOfFrame(_filesView, _iconItems[(int)DisplayMode], !HorizontalExpandingFileView));
      }

      _pendingSelection = null;
    }

    public void OnShow()
    {
      if (_filenameInput != null)
      {
        // We only allow vertical scrolling in list view. Horizontal scrolling is the other way around.
        _filesView.vertical = !HorizontalExpandingFileView;
        _filesView.horizontal = HorizontalExpandingFileView;
        EventSystem.current.SetSelectedGameObject(_filenameInput.gameObject);
      }
    }

    public static void ClearScrollRect(ScrollRect scroll)
    {
      for (int i = 0; i < scroll.content.childCount; ++i)
      {
        GameObject.Destroy(scroll.content.GetChild(i).gameObject);
      }
      scroll.content.DetachChildren();
    }

    protected int PopulateScrollRect(ScrollRect scroll, GameObject template, IEnumerable<FileSystemEntry> items,
                                     FileIconSet icons, int targetWidth, bool fillHorizontal)
    {
      if (scroll == null)
      {
        return 0;
      }

      ClearScrollRect(scroll);

      if (template == null)
      {
        return 0;
      }

      // We modify the template width because it's the the simplest way to implement this feature.
      if (targetWidth > 0)
      {
        // Resize the icon.
        RectTransform rect = template.transform as RectTransform;
        if (rect != null)
        {
          var sd = rect.sizeDelta;
          sd.x = targetWidth;
          rect.sizeDelta = sd;
        }
      }

      GridLayoutGroup layoutGrid = scroll.content.GetComponent<GridLayoutGroup>();
      if (layoutGrid != null)
      {
        layoutGrid.startAxis = (fillHorizontal) ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
      }

      // Create and position each item.
      int itemCount = 0;
      // Dodgy way to simulate many items for testing.
      foreach (FileSystemEntry item in items)
      {
        // Create item
        GameObject itemUI = GameObject.Instantiate(template);
        Text itemText = itemUI.GetComponentInChildren<Text>();
        FileEntryComponent entry = itemUI.GetComponent<FileEntryComponent>();
        Sprite sprite = (icons != null) ? icons.GetIcon(item) : null;
        ToolTipInfo toolTip = itemUI.GetComponentInChildren<ToolTipInfo>();

        // We are only allowed to modify an empty tool tip.
        if (toolTip != null && toolTip.ToolTip.Length > 0)
        {
          toolTip = null;
        }

        // Ensure tag
        if (entry == null)
        {
          entry = itemUI.AddComponent<FileEntryComponent>();
        }

        entry.Entry = item;

        // We expect the item to have two images; an icon image and an disabled highlight.
        // We can expect that the highlight is disabled, to hide it for now, and must come
        // first because of Unity uGUI draw order/hierarchy constraints.
        // For simplicity, we assume the first image is the highlight, the second image is
        // the icon. The icon image is optional.
        foreach (Image image in itemUI.GetComponentsInChildren<Image>())
        {
          if (entry.Highlight == null)
          {
            entry.Highlight = image;
          }
          else if (entry.Icon == null)
          {
            // All done if we are here.
            entry.Icon = image;
            break;
          }
        }

        // Set text.
        if (itemText != null)
        {
          itemText.text = item.Name;
          if (toolTip != null)
          {
            toolTip.ToolTip = item.Name;
          }
        }

        if (entry.Highlight)
        {
          entry.Highlight.enabled = false;
        }

        // Set image
        if (entry.Icon != null && sprite != null)
        {
          entry.Icon.sprite = sprite;
        }

        itemUI.transform.SetParent(scroll.content, false);
        ++itemCount;
      }

      return itemCount;
    }

    /// <summary>
    /// A coroutine which calls <see cref="FixSize"/> at the end of the current frame.
    /// </summary>
    /// <remarks>
    /// This addresses layout issues where the size of a scroll view may not have been calculated yet
    /// and we must wait until the end of frame to have valid values for <see cref="FixSize"/>.
    ///
    /// This method also ensures that the content grid size matches the item size and the scrolling
    /// sensitivity is suitable.
    /// </remarks>
    /// <param name="scroll">The scroll view.</param>
    /// <param name="template">The template item used to populate the scroll view.</param>
    /// <param name="verticalExpanding">Scroll view expands in the vertical axis.</param>
    /// <returns></returns>
    protected IEnumerator FixSizeAtEndOfFrame(ScrollRect scroll, GameObject template, bool verticalExpanding)
    {
      // Fix up the scrolling sensitivity and spacing grid size (if using a grid).
      RectTransform templateRect = (template) ? template.transform as RectTransform : null;
      if (AutoGridLayoutSizing)
      {
        CheckLayoutSpacing(scroll, templateRect, verticalExpanding);
      }
      if (AutoScrollSensitivity)
      {
        CheckScrollSensitivity(scroll, templateRect, verticalExpanding);
      }
      yield return new WaitForEndOfFrame();
      FixSize(scroll, templateRect, verticalExpanding);
    }

    /// <summary>
    /// Adjust the grid layout of <paramref name="scroll"/> (if present) to exactly
    /// match the <paramref name="templateRect"/> size.
    /// </summary>
    /// <param name="scroll">The scroll view.</param>
    /// <param name="template">The template item transform used to populate the scroll view.</param>
    protected void CheckLayoutSpacing(ScrollRect scroll, RectTransform templateRect, bool verticalExpanding)
    {
      if (templateRect != null && scroll != null)
      {
        GridLayoutGroup gridLayout = scroll.content.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
          gridLayout.cellSize = new Vector2(templateRect.rect.width, templateRect.rect.height);
        }
      }
    }

    /// <summary>
    /// Adjust the scroll sensitivity of <param name="scroll"/> to match the sizing of
    /// <paramref name="templateRect"/> size.
    /// </summary>
    /// <param name="scroll">The scroll view.</param>
    /// <param name="template">The template item transform used to populate the scroll view.</param>
    /// <param name="vertical">True to address vertical scrolling, false for horizontal.</param>
    protected void CheckScrollSensitivity(ScrollRect scroll, RectTransform templateRect, bool vertical)
    {
      if (templateRect != null && scroll != null)
      {
        if (vertical)
        {
          scroll.scrollSensitivity = templateRect.rect.height;
        }
        else
        {
          // Horizontal scrolling needs to be negative to get the direction right on the mouse wheel.
          scroll.scrollSensitivity = -templateRect.rect.width;
        }
      }
    }

    /// <summary>
    /// Fixes the size of a file item ScrollRect to match its content.
    /// </summary>
    /// <remarks>
    ///  The height of the scroll view is adjusted to match the number of child items.
    /// A grid layout is assumed where the number of columns is determined by the scroll view
    /// width divided by the template item width. The number of rows required is calculated to
    /// set the content width as rows * template height.
    ///  </remarks>
    /// <param name="scroll">The scroll view.</param>
    /// <param name="templateRect">The template item used to populate the scroll view. Used to determine
    /// <param name="verticalExpanding">Scroll view expands in the vertical axis.</param>
    ///  the per item width/height.</param>
    protected static void FixSize(ScrollRect scroll, RectTransform templateRect, bool verticalExpanding)
    {
      // Change the height to suit the number of items.
      RectTransform contentRect = scroll.content.transform as RectTransform;
      RectTransform scrollRect = scroll.transform as RectTransform;
      if (!templateRect || !contentRect || !scrollRect)
      {
        return;
      }

      if (verticalExpanding)
      {
        if (templateRect.rect.width <= 0)
        {
          return;
        }

        float scrollWidth = contentRect.rect.width;

        // Account for the vertical scroll bar if hidden.
        if (scroll.verticalScrollbar != null && !scroll.verticalScrollbar.gameObject.activeSelf)
        {
          scrollWidth -= scroll.verticalScrollbar.GetComponent<RectTransform>().rect.width;
        }

        int columnCount = Mathf.FloorToInt(scrollWidth / templateRect.rect.width);
        if (columnCount == 0)
        {
          columnCount = 1;
        }

        // Review: This assumes a lot about the content layout.
        int itemCount = contentRect.childCount;
        int rowCount = (itemCount + columnCount - 1) / columnCount;
        Vector2 offset = contentRect.offsetMax;
        offset.y = 0;
        contentRect.offsetMax = offset;
        offset = contentRect.offsetMin;
        offset.y = -rowCount * templateRect.rect.height;
        contentRect.offsetMin = offset;
      }
      else
      {
        if (templateRect.rect.height <= 0)
        {
          return;
        }

        float scrollHeight = scrollRect.rect.height;

        // Account for the vertical scroll bar if hidden.
        if (scroll.horizontalScrollbar != null && !scroll.horizontalScrollbar.gameObject.activeSelf)
        {
          scrollHeight -= scroll.horizontalScrollbar.GetComponent<RectTransform>().rect.height;
        }

        int rowCount = Mathf.FloorToInt(scrollHeight / templateRect.rect.height);
        if (rowCount == 0)
        {
          rowCount = 1;
        }

        // Review: This assumes a lot about the content layout.
        int itemCount = contentRect.childCount;
        // We take two off columnCount to get the right width. Not sure how this works, but it does.
        const int columnCountOffset = 2;
        int columnCount = (itemCount + rowCount - 1) / rowCount - columnCountOffset;
        if (columnCount < 0)
        {
          columnCount = 0;
        }
        Vector2 offset = contentRect.offsetMax;
        offset.x = columnCount * templateRect.rect.width;
        offset.y = 0;
        contentRect.offsetMax = offset;
        offset = contentRect.offsetMin;
        offset.x = 0;
        offset.y = -scrollHeight;
        contentRect.offsetMin = offset;
      }
    }

    /// <summary>
    /// Browse up a directory if possible.
    /// </summary>
    public void GoUp()
    {
      if (Controller != null)
      {
        Controller.FileDialogNavigate(CurrentLocation, true);
      }
    }

    /// <summary>
    /// Confirm button press event.
    /// </summary>
    public void OnConfirm()
    {
      // Open a directory if there is just a directory selected.
      if (_selectedFiles.Count == 1 && _selectedFiles[0].Entry.Type == FileItemType.Directory)
      {
        CurrentLocation = _selectedFiles[0].Entry;
        Location = _selectedFiles[0].Entry.FullName;
        return;
      }

      List<string> filenames = new List<string>();
      foreach (string filename in Filenames)
      {
        filenames.Add(filename);
      }

      if (ValidateFiles(filenames))
      {
        // Confirmed selection.
        NotifyDone(new CancelEventArgs(false));
      }
    }

    /// <summary>
    /// Cancel button press event.
    /// </summary>
    public void OnCancel()
    {
      NotifyDone(new CancelEventArgs(true));
    }

    /// <summary>
    /// Identifies what the state of attempting to select an item is.
    /// </summary>
    protected enum SelectResult
    {
      /// <summary>
      /// Item has been selected and is the only selection.
      /// </summary>
      Select,
      /// <summary>
      /// Item has been added to the selection. There are multiple selected items.
      /// </summary>
      SelectAdd,
      /// <summary>
      /// The selection has been extended to include the new item.
      /// </summary>
      SelectExtend,
      /// <summary>
      /// The item has been unselected. There may or may not be additional selected items.
      /// </summary>
      Unselect,
      /// <summary>
      /// Item was already selected and we have an activation event (double click).
      /// </summary>
      Activate
    }

    protected bool AddSelectionActive
    {
      get
      {
        return Input.GetKey(_addSelectionKeys[0]) || Input.GetKey(_addSelectionKeys[1]);
      }
    }

    protected bool ExtendSelectionActive
    {
      get
      {
        return Input.GetKey(_extendSelectionKeys[0]) || Input.GetKey(_extendSelectionKeys[1]);
      }
    }

    /// <summary>
    /// Resolves selection logic.
    /// </summary>
    /// <param name="item">The item to (de)select.</param>
    /// <param name="lastSelectTime">The last time something was selected. Updated to now if item is selected.</param>
    /// <param name="selectionList">Optional: the list of active selections. Only required for multi-selection support.</param>
    /// <param name="primarySelection">The primary selected item.</param>
    /// <param name="activationWindow">The time window for double click style activation (seconds)</param>
    /// <returns>The selection result.</returns>
    /// <remarks>
    /// Handles multi or single selection. Requires <paramref name="selectionList"/> for multi-select support.
    /// </remarks>
    protected SelectResult ProcessSelection(FileEntryComponent item, ref float lastSelectTime,
                                            List<FileEntryComponent> selectionList,
                                            ref FileEntryComponent primarySelection,
                                            float activationWindow, bool multiselect)
    {
      float selectDelta = Time.time - lastSelectTime;
      int selectionIndex = (selectionList != null) ? selectionList.IndexOf(item) : -1;
      bool alreadySelected = primarySelection == item || selectionIndex >= 0;
      if (selectDelta <= activationWindow && alreadySelected)
      {
        // Already selected, and we are in the activation windows. Activate.
        return SelectResult.Activate;
      }

      SelectResult sres = SelectResult.Select;
      if (selectionList != null)
      {
        if (multiselect && AddSelectionActive)
        {
          // Add selection mode: toggle selection of item.
          // Try remove from the current selection.
          if (selectionIndex >= 0)
          {
            item.Highlight.enabled = false;
            _selectedFiles.RemoveAt(selectionIndex);
            if (primarySelection == item)
            {
              if (selectionList.Count != 0)
              {
                primarySelection = selectionList[selectionList.Count - 1];
              }
              else
              {
                primarySelection = null;
              }
            }
            // Removed. We are done here.
            return SelectResult.Unselect;
          }
          // else don't remove from the current selection.
          sres = SelectResult.SelectAdd;
        }
        // TODO: Extend selection mode.
        //else if (multiselect && ExtendSelectionActive)
        //{
        //  sres = SelectResult.SelectExtend;
        //}
        else
        {
          // Clear the current selection. We'll replace it with this item.
          foreach (FileEntryComponent entry in _selectedFiles)
          {
            if (entry.Highlight != null)
            {
              entry.Highlight.enabled = false;
            }
          }

          _selectedFiles.Clear();
        }

        sres = SelectResult.Select;
        selectionList.Add(item);
        if (primarySelection != null)
        {
          primarySelection = item;
        }
      }

      primarySelection = item;

      // Highlight the item.
      if (item.Highlight != null)
      {
        item.Highlight.enabled = true;
      }

      lastSelectTime = Time.time;
      return sres;
    }

    public void OnSelectFile(FileEntryComponent item)
    {
      switch (ProcessSelection(item, ref _lastSelectTime, _selectedFiles, ref _primarySelectedFile, DoubleClickWindow, Multiselect))
      {
      case SelectResult.Activate:
        if (item.Entry.Type == FileItemType.File)
        {
          // Confirm.
          List<string> filenames = new List<string>();
          foreach (string filename in Filenames)
          {
            filenames.Add(filename);
          }

          if (ValidateFiles(filenames))
          {
            NotifyDone(new CancelEventArgs(false));
          }
        }
        else
        {
          CurrentLocation = item.Entry;
          Location = item.Entry.FullName;
        }
        break;
      default:
        // Update file name.
        UpdateFilenameDisplayFromSelection();
        NotifyChange("Filename", Filename);
        // NotifyChange("Filenames", Filename);
        break;
      }
    }

    protected void UpdateFilenameDisplayFromSelection()
    {
      StringBuilder str = new StringBuilder();
      bool first = true;
      // string newFileName = string.Empty;
      foreach (FileEntryComponent entry in _selectedFiles)
      {
        if (entry.Entry.Type == FileItemType.File)
        {
          if (!first)
          {
            str.Append(" ");
          }
          str.Append(entry.Entry.Name);
          first = false;
        }
      }

      FilenameDisplay = str.ToString();;
    }

    public void OnSelectLocation(FileEntryComponent item)
    {
      if (_selectedLocation != null && _selectedLocation.Highlight != null)
      {
        _selectedLocation.Highlight.enabled = false;
      }

      _selectedLocation = item;
      if (_selectedLocation != null)
      {
        CurrentLocation = item.Entry;
        Location = item.Entry.FullName;
        if (_selectedLocation.Highlight != null)
        {
          _selectedLocation.Highlight.enabled = true;
        }
      }
    }

    protected void NotifyChange(string target, object value)
    {
      if (Controller != null && !SuppressEvents)
      {
        Controller.OnFileDialogChange(new FileDialogViewEventArgs(target, value));
      }
    }

    protected bool ValidateFiles(List<string> filenames)
    {
      if (Controller != null && !SuppressEvents)
      {
        return Controller.OnValidateFiles(filenames);
      }

      return true;
    }

    protected void NotifyDone(CancelEventArgs args)
    {
      if (Controller != null && !SuppressEvents)
      {
        Controller.OnFileDialogDone(args);
      }
    }

    private string[] _fileFilters = null;
    private string[] _fileFilterDescriptions = null;
    private List<FileEntryComponent> _selectedFiles = new List<FileEntryComponent>();
    private FileEntryComponent _primarySelectedFile = null;
    private FileEntryComponent _selectedLocation = null;
    private float _lastSelectTime;
    protected bool SuppressEvents { get; set; }
    private InputField _lastInputFocus = null;
    /// <summary>
    /// Pending file selection to resolve in <see cref="ShowItems(FileSystemEntry, IEnumerable{FileSystemEntry})"/>.
    /// </summary>
    /// <remarks>
    /// Supports entering a full file name in the file name input box.
    /// </remarks>
    private string _pendingSelection = null;
  }
}
