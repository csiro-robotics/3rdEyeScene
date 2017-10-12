using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Dialogs
{
  /// <summary>
  /// Arguments used for property change notification on a <see cref="FileDialogUI"/>
  /// </summary>
  public class FileDialogViewEventArgs : EventArgs
  {
    /// <summary>
    /// The name of the property which has changed.
    /// </summary>
    public string Target { get; protected set; }
    /// <summary>
    /// The new property value.
    /// </summary>
    public object Value { get; protected set; }

    /// <summary>
    /// Simple constructor.
    /// </summary>
    /// <param name="target">The name of the changed property.</param>
    /// <param name="value">The new property value.</param>
    public FileDialogViewEventArgs(string target, object value)
    {
      Target = target;
      Value = value;
    }
  }

  /// <summary>
  /// Observer interfaces used by the <see cref="FileDialogUI"/> to validate and notify events.
  /// </summary>
  public interface FileDialogViewController
  {
    /// <summary>
    /// Validate the given list of files before confirmation.
    /// </summary>
    /// <param name="filenames">List of file paths to validate.</param>
    /// <returns>True when all files are valid and confirmation may be continue with a call to
    /// <see cref="OnFileDialogDone(CanceEventArgs, List&lt;string&gt;)"/>.</returns>
    /// <remarks>
    /// Called prior to <see cref="OnFileDialogDone(CancelEventArgs, List&lt;string&gt;)"/> which will only be called when this method
    /// returns <c>true</c>. Otherwise the UI will remain active.
    ///
    /// Implementations should cache the validated file list for the following call to
    /// <see cref="OnFileDialogDone(CanceEventArgs)"/>.
    /// </remarks>
    bool OnValidateFiles(IEnumerable<string> filenames);
    /// <summary>
    /// Called when a property of the <see cref="FileDialogUI"/> changes.
    /// </summary>
    /// <param name="args">Identifies changed property name and value.</param>
    void OnFileDialogChange(FileDialogViewEventArgs args);
    /// <summary>
    /// Called when the UI has completed with <paramref name="args"/> identifying confirmation or cancellation.
    /// </summary>
    /// <param name="args">Cancellation arguments.</param>
    /// <remarks>
    /// This is only if a call to <see cref="OnValidateFiles(IEnumerable&lt;string&gt;)"/> passes first.
    /// </remarks>
    void OnFileDialogDone(CancelEventArgs args);
    /// <summary>
    /// Called on user interaction with the UI to navigate to a new location, either <paramref name="target"/> or its parent.
    /// </summary>
    /// <param name="target">Identifies the new navigation location.</param>
    /// <param name="toTargetsParent">True if we want to navigate to the parent of
    ///   <paramref name="target"/> instead of <paramref name="target"/> itself.
    /// <remarks>
    /// From this call, the controlling observer should refresh the UI to match the <paramref name="target"/>
    /// location.
    /// </remarks>
    void FileDialogNavigate(FileSystemEntry target, bool toParentsTarget);
    /// <summary>
    /// Requests the location be updated to the given path based on user interaction.
    /// This may complete the dialog interaction if the full path is a valid item for
    /// completion and <paramref name="allowCompletion"/> is true.
    /// </summary>
    /// <param name="fullPath">The full path of the new location.</param>
    /// <remarks>
    /// This method is for when the user enters a location via text input.
    /// </remarks>
    bool SetFileDialogLocation(string fullPath);
  }
}
