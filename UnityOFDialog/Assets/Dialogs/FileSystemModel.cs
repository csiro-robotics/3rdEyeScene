using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dialogs
{
  /// <summary>
  /// Represents an entry in the file system. This may be a file,
  /// drive, directory or favourite.
  /// </summary>
  /// <remarks>
  /// The <see cref="FileItemType.Null">Null</see> <see cref="Type"/> is used to
  /// denote an invalid entry. Other values will generally also be null, but the
  /// <see cref="Type"/> is used as the primary identification.
  /// </remarks>
  public struct FileSystemEntry
  {
    /// <summary>
    /// The short display name for the entry.
    /// </summary>
    public string Name;
    /// <summary>
    /// The fully qualified name for the entry. This equates to the full path name.
    /// </summary>
    public string FullName;
    /// <summary>
    /// The entry type.
    /// </summary>
    public FileItemType Type;
    /// <summary>
    /// The sub type is mostly used to identify the extension for file objects.
    /// </summary>
    public string SubType;
    /// <summary>
    /// Full path to the parent.
    /// </summary>
    public string ParentPath;
    /// <summary>
    /// Detail object. This is user defined.
    /// </summary>
    public object Detail;

    /// <summary>
    /// Used to generate a null entry object.
    /// </summary>
    /// <value>A null entry.</value>
    public static FileSystemEntry NullEntry
    {
      get
      {
        return new FileSystemEntry
        {
          Name = null,
          FullName = null,
          Type = FileItemType.Null,
          SubType = null,
          Detail = null
        };
      }
    }
  }

  /// <summary>
  /// This interface is used to traverse and iterate a file system.
  /// </summary>
  /// <remarks>
  /// The results may represent the real file system, or a virtual file system,
  /// as the use case dictates. The results may also be filtered or unfiltered.
  /// </remarks>
  public interface FileSystemModel
  {
    /// <summary>
    /// Get the home directory. This is used as the starting directory.
    /// </summary>
    /// <remarks>The home directory semantics vary between platforms.</remarks>
    /// <value>The home directory for this file system.</value>
    FileSystemEntry Home { get; }
    /// <summary>
    /// Resolve a file system entry from a full path specification.
    /// </summary>
    /// <param name="path">The full path name to resolve.</param>
    /// <returns>The corresponding file system entry. Null entry on failure.</returns>
    FileSystemEntry FromPath(string path);
    /// <summary>
    /// Defines the root locations in the file system. This will generally include
    /// drives and favourites.
    /// </summary>
    /// <returns>An enumeration of the file system root entries.</returns>
    IEnumerable<FileSystemEntry> GetRoots();
    /// <summary>
    /// Enumerate the children for a given entry. Results are empty or null for
    /// childless entries.
    /// </summary>
    /// <param name="parent">The parent item to retrieve children for.</param>
    /// <param name="filter">Optional regular expression filter matched against the entry Name
    ///   field. A child entry must match the expression to be included in the result. May be null
    ///    to return all children.</param>
    /// <returns>The child entries, a zero length array if there are no children
    /// (an empty directory) or null if there cannot be any children (files have
    /// no children).</returns>
    /// <remarks>
    /// The method must support a null entry <paramref name="parent"/> type by listing
    /// the root items.
    /// </remarks>
    FileSystemEntry[] ListChildren(FileSystemEntry parent, Regex filter);
    /// <summary>
    /// Resolve the parent for a given item.
    /// </summary>
    /// <param name="entry">The entry to get the parent of.</param>
    /// <returns>The parent item on success or a null entry on failure.</returns>
    /// <remarks>
    /// Note that when traversing the hierarchy, the individual objects retrieved may
    /// be different, but alias the same logical file.
    /// </remarks>
    FileSystemEntry GetParent(FileSystemEntry entry);
  }
}
