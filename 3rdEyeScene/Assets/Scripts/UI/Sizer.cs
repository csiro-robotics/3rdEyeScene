using UnityEngine;

namespace UI
{
	public class Sizer : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _panel;
		public RectTransform Panel
		{
			get { return _panel; }
			set { _panel = value; }
		}
		[SerializeField]
		private float _minSize;
		public float MinSize
		{
			get { return _minSize; }
			set { _minSize = value; }
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update()
		{
			if (_dragging && _panel != null)
			{
				Vector3 dragPos = CursorPosition;
				float deltaY = dragPos.y - _lastMousePos.y;

				if (deltaY != 0)
				{
					Vector2 panelSize = _panel.sizeDelta;
					panelSize.y = Mathf.Max(panelSize.y + deltaY, _minSize);
					_panel.sizeDelta = panelSize;
				}

				_lastMousePos = dragPos;
			}
		}

		public void OnDragStart()
		{
			_lastMousePos = CursorPosition;
			_dragging = true;
		}

		public void OnDragEnd()
		{
			_dragging = false;
		}

		private Vector3 CursorPosition
		{
			get
			{
				return Input.mousePosition;
			}
		}

		private Vector3 _lastMousePos;
		private bool _dragging = false;
	}
}
