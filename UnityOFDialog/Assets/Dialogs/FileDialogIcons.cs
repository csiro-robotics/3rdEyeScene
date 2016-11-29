using UnityEngine;
using System.Collections.Generic;

namespace Dialogs
{
  /// <summary>
  /// Implements the <see cref="FileIconSet"/> and allows user configuration.
  /// </summary>
  /// <remarks>
  /// This script can be attached to an object to allow user configuration of the
  /// icon set.
  /// </remarks>
	public class FileDialogIcons : MonoBehaviour, FileIconSet
	{
    /// <summary>
    /// Default icon to show when no other icon can be resolved.
    /// </summary>
		[SerializeField]
		public Sprite _defaultIcon;
		public Sprite DefaultIcon { get { return _defaultIcon; } }

    /// <summary>
    /// The set of icons (user editable).
    /// </summary>
		[SerializeField]
		private IconInfo[] _icons = null;
		public IEnumerable<IconInfo> Icons { get { return _icons; } }

    /// <summary>
    /// Get the icon for <paramref name="entry"/>
    /// </summary>
    /// <param name="entry">The file entry to match an icon for.</param>
    /// <returns>An icon to use for <paramref name="entry"/>. May be the <see cref="DefaultIcon"/>.</returns>
    /// <remarks>
    /// Matching is made by Type, then SubType. SubType is only used where available.
    /// An icon with matching Type and no SubType matches any item of the type, regardless
    /// of its SubType.
    /// </remarks>
		public Sprite GetIcon(FileSystemEntry entry)
		{
			Sprite defaultIcon = _defaultIcon;

			for (int i = 0; i < _icons.Length; ++i)
			{
				if (_icons[i].Type == entry.Type)
				{
					if (_icons[i].SubType == entry.SubType)
					{
						return _icons[i].Icon;
					}
					else if (string.IsNullOrEmpty(_icons[i].SubType))
					{
						defaultIcon = _icons[i].Icon;
					}
				}
			}

			return defaultIcon;
		}
	}
}
