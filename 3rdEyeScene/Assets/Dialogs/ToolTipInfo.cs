using UnityEngine;

namespace Dialogs
{
	/// <summary>
	/// A simple behaviour which exposes a tool tip string for other components to read.
	/// </summary>
	/// <remarks>
	/// TODO: add localisation support via a string ID.
	/// </remarks>
	public class ToolTipInfo : MonoBehaviour
	{
		[SerializeField]
		private string _toolTip;
		/// <summary>
		/// Get [set] the tool tip string.
		/// </summary>
		public string ToolTip
		{
			get { return _toolTip; }
			set { _toolTip = value; }
		}
	}
}
