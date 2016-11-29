using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	/// UI utilities.
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// Create a spacer UI element which serves to consume any remaining layout space.
		/// </summary>
		public static LayoutElement CreateSpacer(bool vertical = true)
		{
				GameObject spacerObj = new GameObject("Spacer");
				spacerObj.AddComponent<Canvas>();
				LayoutElement spacer = spacerObj.AddComponent<LayoutElement>();
				spacer.flexibleWidth = (vertical) ? 1 : 10000;
				spacer.flexibleHeight = (vertical) ? 10000 : 1;
				return spacer;
		}
	}
}
