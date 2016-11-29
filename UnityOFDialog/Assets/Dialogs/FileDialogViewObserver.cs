using System;
using System.ComponentModel;

namespace Dialogs
{
  public class FileDialogViewEventArgs : EventArgs
  {
    public string Target { get; protected set; }
    public object Value { get; protected set; }

    public FileDialogViewEventArgs(string target, object value)
    {
      Target = target;
      Value = value;
    }
  }

  public interface FileDialogViewObserver
  {
    void OnFileDialogChange(FileDialogViewEventArgs args);
    void OnFileDialogDone(CancelEventArgs args);
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
