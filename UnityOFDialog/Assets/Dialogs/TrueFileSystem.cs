using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dialogs
{
	/// <summary>
	/// An implementation of the <see cref="FileSystemModel" /> representing the local
	/// file system.
	/// </summary>
	/// <remarks>
	/// <see cref="Roots" /> are attained via DriveInfo.GetDrives() and include the
	/// <see cref="Home" /> directory.
	/// </remarks>
	public class TrueFileSystem : FileSystemModel
	{
		/// <summary>
		/// Attempts to convert <paramref name="directory"/> into a DriveInfo reference.
		/// </summary>
		/// <param name="directory">The directory to attempt to represent as a drive.</param>
		/// <returns>The drive representation of <paramref name="directory"/> or null on failure.</returns>
		/// <remarks>
		/// This works for any <paramref name="directory"/> for which the RootDirectory
		/// is the same as the <paramref name="directory"/>.
		/// For example, under Windows, C:\ will be converted to the C drive.
		/// <remarks>
		public DriveInfo AsDrive(DirectoryInfo directory)
		{
			if (directory != null && directory.Root == directory)
			{
				// Looks like a drive.
				foreach (DriveInfo drive in DriveInfo.GetDrives())
				{
					if (drive.RootDirectory == directory)
					{
						return drive;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the current user's home directory.
		/// </summary>
		/// <returns>The current user's home directory if available. A null entry otherwise.</returns>
		/// <remarks>
		/// This is based on Environment.SpecialFolder.Personal
		/// </remarks>
		public FileSystemEntry Home
		{
			get
			{
				DirectoryInfo dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
				if (dir.Exists)
				{
					FileSystemEntry e = CreateEntry(dir);
					e.SubType = "home";
					return e;
				}
				return FileSystemEntry.NullEntry;
			}
		}

		/// <summary>
		/// Converts a string path into a <see cref="FileSystemEntry"/>
		/// </summary>
		/// <param name="path">The path to convert.</param>
		/// <returns>The equivalent <see cref="FileSystemEntry"/> on success a null entry on failure.</returns>
		public FileSystemEntry FromPath(string path)
		{
			if (path != null)
			{
				if (Directory.Exists(path))
				{
					// TODO: resolve drive items.
					DirectoryInfo dir = new DirectoryInfo(path);
					DriveInfo drive = AsDrive(dir);
					if (drive != null)
					{
						return CreateEntry(drive);
					}
					UnityEngine.Debug.Log(string.Format("Path: {0} => {1}", path, new DirectoryInfo(path)));
					return CreateEntry(new DirectoryInfo(path));
				}
				if (File.Exists(path))
				{
					return CreateEntry(new FileInfo(path));
				}
			}
			return FileSystemEntry.NullEntry;
		}

		/// <summary>
		/// Converts <see cref="DriveInfo"/> into a <see cref="FileSystemEntry"/>
		/// </summary>
		/// <param name="info">The drive to convert.</param>
		/// <returns>The <see cref="FileSystemEntry"/> representation of <paramref name="info"/></returns>
		public static FileSystemEntry CreateEntry(DriveInfo info)
		{
			return new FileSystemEntry
			{
				Name = info.RootDirectory.Name,
				FullName = info.RootDirectory.FullName,
				Type = FileItemType.Drive,
				SubType = string.Empty,
				ParentPath = string.Empty,
				Detail = info
			};
		}

    public static bool IsDrive(DirectoryInfo directory)
    {
      if (directory != null && string.Compare(directory.Root.FullName, directory.FullName) == 0)
      {
        // Looks like a drive.
        return true;
      }

      return false;
    }

    /// <summary>
    /// Converts <see cref="DirectoryInfo"/> into a <see cref="FileSystemEntry"/>
    /// </summary>
    /// <param name="info">The directory to convert.</param>
    /// <returns>The <see cref="FileSystemEntry"/> representation of <paramref name="info"/></returns>
    public static FileSystemEntry CreateEntry(DirectoryInfo info)
		{
      if (IsDrive(info))
      {
        return new FileSystemEntry
        {
          Name = info.Name,
          FullName = info.FullName,
          Type = FileItemType.Drive,
          SubType = string.Empty,
          ParentPath = string.Empty,
          Detail = info
        };
      }

      return new FileSystemEntry
			{
				Name = info.Name,
				FullName = info.FullName,
				Type = FileItemType.Directory,
				SubType = string.Empty,
				ParentPath = (info.Parent != null) ? info.Parent.FullName : string.Empty,
				Detail = info
			};
		}

		/// <summary>
		/// Converts <see cref="FileInfo"/> into a <see cref="FileSystemEntry"/>
		/// </summary>
		/// <param name="info">The file to convert.</param>
		/// <returns>The <see cref="FileSystemEntry"/> representation of <paramref name="info"/></returns>
		public static FileSystemEntry CreateEntry(FileInfo info)
		{
			return new FileSystemEntry
			{
				Name = info.Name,
				FullName = info.FullName,
				Type = FileItemType.File,
				SubType = info.Extension,
				ParentPath = info.Directory.FullName,
				Detail = info
			};
		}

		/// <summary>
		/// Enemerates the file system roots.
		/// </summary>
		/// <returns>An enumeration of the file system root entry points.</returns>
		/// <remarks>
		/// This inclues the <see cref="Home"/> directory and the <see cref="DriveInfo.GetDrives()"/>
		/// </remarks>
		public IEnumerable<FileSystemEntry> GetRoots()
		{
			FileSystemEntry e = Home;
			if (e.Type != FileItemType.Null)
			{
				yield return e;
			}

      // Can't use DriveInfo as it's not implemented on Windows in Mono 2. Maybe when Unity Mono is upgraded.
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      for (char driveLetter = 'A'; driveLetter <= 'Z'; ++driveLetter)
      {
        DirectoryInfo dir = new DirectoryInfo(string.Format(@"{0}:\", driveLetter));
        if (dir.Exists)
        {
          yield return CreateEntry(dir);
        }
      }
#else
      yield return CreateEntry(new DirectoryInfo("/"));
#endif

#if DISABLED
      foreach (DriveInfo drive in DriveInfo.GetDrives())
			{
				yield return CreateEntry(drive);
			}
#endif // 
    }

    /// <summary>
    /// Lists the children for <paramref name="parent"/> with optional file filter.
    /// </summary>
    /// <param name="parent">The parent file system entry (e.g., drive or directory).</param>
    /// <param name="filter">Optional filter expression. Null for none.</param>
    /// <returns>An array of child items. May be empty, but not null.</returns>
    /// <remarks>
    /// The filter is only applied to file item types, not to directories.
    /// </remarks>
    public FileSystemEntry[] ListChildren(FileSystemEntry parent, Regex filter)
		{
			if (parent.Type == FileItemType.Null)
			{
				// List the root items.
				List<FileSystemEntry> rootItems = new List<FileSystemEntry>();
				foreach (FileSystemEntry entry in GetRoots())
				{
//					if (filter == null || filter.IsMatch(entry.Name))
					{
						rootItems.Add(entry);
					}
				}
				return rootItems.ToArray();
			}

			DriveInfo drive;
			DirectoryInfo directory;
			if ((drive = parent.Detail as DriveInfo) != null)
			{
				directory = drive.RootDirectory;
			}
			else
			{
				directory = parent.Detail as DirectoryInfo;
			}

			if (directory != null)
			{
				DirectoryInfo[] subDirs = directory.GetDirectories();
				FileInfo[] files = directory.GetFiles();

				List<FileSystemEntry> children = new List<FileSystemEntry>(subDirs.Length + files.Length);
				for (int i = 0; i < subDirs.Length; ++i)
				{
					DirectoryInfo dir = subDirs[i];
					children.Add(CreateEntry(dir));
				}

				for (int i = 0; i < files.Length; ++i)
				{
					FileInfo file = files[i];
					if (filter == null || filter.IsMatch(file.Name))
					{
						children.Add(CreateEntry(file));
					}
				}

				return children.ToArray();
			}

			return null;
		}

		/// <summary>
		/// Get the parent for <paramref name="entry"/>
		/// </summary>
		/// <param name="entry">The entry to resolve a parent for.</param>
		/// <returns>The parent of <paramref name="entry"/> or a null entry on failure.</returns>
		public FileSystemEntry GetParent(FileSystemEntry entry)
		{
			DirectoryInfo directory;
			FileInfo file;

			if ((directory = entry.Detail as DirectoryInfo) != null)
			{
				if (directory.Root == directory)
				{
					return FileSystemEntry.NullEntry;
				}

				directory = directory.Parent;
				DriveInfo drive = AsDrive(directory);
				if (drive != null)
				{
					// Looks like a drive.
					return CreateEntry(drive);
				}

				// Didn't resolve a drive.
				return CreateEntry(directory);
			}

			if ((file = entry.Detail as FileInfo) != null)
			{
				return CreateEntry(file.Directory);
			}

			return FileSystemEntry.NullEntry;
		}
	}
}
