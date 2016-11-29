using UnityEngine;
using UnityEngine.UI;

namespace Dialogs
{
	public class FileEntryComponent : MonoBehaviour
	{
		public Image Icon { get; set; }
		public Image Highlight { get; set; }
		public FileSystemEntry Entry { get; set; }
	}
}
