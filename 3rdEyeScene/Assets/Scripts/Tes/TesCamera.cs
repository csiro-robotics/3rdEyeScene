using System;
using UnityEngine;

namespace Tes
{
  /// <summary>
  /// A bridging component between a scene camera and the <see cref="TesComponent"/>. Ensures the 3es scene is rendered.
  /// </summary>
  public class TesCamera : MonoBehaviour
  {
    [SerializeField]
    private TesComponent _thirdEyeScene;
    public TesComponent ThirdEyeScene
    {
      get { return _thirdEyeScene; }
      set { _thirdEyeScene = value; }
    }

    public void OnRenderObject()
    {
      if (_thirdEyeScene != null)
      {
      //  _thirdEyeScene.Render(transform.localToWorldMatrix);
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
          foreach (var handler in _thirdEyeScene.Handlers.Handlers)
          {
            handler.AddCamera(camera);
          }
        }
      }
    }
  }
}