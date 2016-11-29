using UnityEngine;
using System.Collections.Generic;

namespace Dialogs
{
	public interface FileIconSet
	{
		Sprite DefaultIcon { get; }
		IEnumerable<IconInfo> Icons { get; }
		Sprite GetIcon(FileSystemEntry entry);
	}
}
