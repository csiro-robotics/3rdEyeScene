using System;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogs
{

	/// <summary>
	/// Provides an abstraction layer between the <see cref="FileDialog"/> and
	/// its GUI components.
	/// </summary>
	public interface FileDialogView
	{
    /// <summary>
    /// Dialog observer.
    /// </summary>
    FileDialogViewObserver Observer { get; set; }

		RectTransform UI { get; }

    /// <summary>
    /// Support multiple file selections?
    /// </summary>
    bool Multiselect { get; set; }
		/// <summary>
		/// The dialog title.
		/// </summary>
		/// <value>The curent dialog title text.</value>
		string Title { get; set; }
		/// <summary>
		/// User display of the current location.
		/// </summary>
		/// <value>The current location text.</value>
		string Location { get; set; }
		/// <summary>
		/// The full path for the currently selected file. Used as the final filename on confirmation.
		/// </summary>
		/// <value>The current filename text.</value>
		string Filename { get; }
		/// <summary>
		/// The full path for the currently selected files. Used as the final filenames on confirmation.
		/// </summary>
		/// <value>The current filename text.</value>
		IEnumerable<string> Filenames { get; }
		/// <summary>
		/// Access the file type filter string.
		/// </summary>
		/// <remarks>
		/// The filter string is formatted as per the .NET API for <see cref="System.Windows.Forms.FileDialog.Filter"/>.
		/// The filter must be broken down into its component values. The active filter is set via
		/// <see cref="FilterFilterIndex"/>
		/// </remarks>
		/// <value>The current file type filter text.</value>
		string FileFilter { get; set; }
		/// <summary>
		/// Access the custom file filter string. This is to support user filters outside of the
		/// <see cref="FileFilter"/> options.
		/// </summary>
		/// <remarks>
		/// Unlike the <see cref="FileFilter"/> this is a direct globbing expression.
		/// </remarks>
		/// <value>The current file custom file filter string.</value>
		string CustomFileFilter { get; set; }
		/// <summary>
		/// Retrieve the active file filter. This exposes the active globbing expression.
		/// </summary>
		/// <remarks>
		/// Returns an empty string when no specific filter is active.
		/// </remarks>
		/// <value>The active globbing expression, either custom or one from <see cref="FileFilter"/>
		string ActiveFileFilter { get; }
		/// <summary>
		/// Access the active filter filter index.
		/// </summary>
		/// <value>The index of the active file filter.</value>
		int FileFilterIndex { get; set; }
		/// <summary>
		/// Access the text for the confirmation button.
		/// </summary>
		/// <value>Current confirmation text.</value>
		string ConfirmButtonText { get; set; }
		/// <summary>
		/// Access the text for the cancel button.
		/// </summary>
		/// <value>Current cancel text.</value>
		string CancelButtonText { get; set; }
		/// <summary>
		/// Access the currently selected item.
		/// </summary>
		/// <value>Details of the current file.</value>
		FileSystemEntry CurrentFile { get; set; }
		FileSystemEntry CurrentLocation { get; set; }
		/// <summary>
		/// Called to update the links view to display the given items.
		/// For example, the <paramref name="locations"/> may specify a list of drives.
		/// </summary>
		/// <param name="locations">The locations items.</param>
		void ShowLinks(IEnumerable<FileSystemEntry> locations);
		/// <summary>
		/// Called to the main view to display the given items.
		/// </summary>
		/// <param name="items">The items to display.</param>
		void ShowItems(FileSystemEntry location, IEnumerable<FileSystemEntry> items);

    /// <summary>
    /// Called once finished setting up UI to finalise showing the UI.
    /// </summary>
    /// <remarks>
    /// Example usage is to set the initial focus.
    /// </remarks>
    void OnShow();
	}
}

