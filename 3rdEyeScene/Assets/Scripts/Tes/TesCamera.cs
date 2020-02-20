using System;
using Tes.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

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

    void Start()
    {
      Camera camera = GetComponent<Camera>();
      if (camera != null)
      {
        _opaqueBuffer = new CommandBuffer();
        _opaqueBuffer.name = $"{gameObject.name}-Opaque";
        _transparentBuffer = new CommandBuffer();
        _transparentBuffer.name = $"{gameObject.name}-Transparent";

        camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _opaqueBuffer);
        camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, _transparentBuffer);
      }
    }

    public void OnRenderObject()
    {
      _opaqueBuffer.Clear();
      _transparentBuffer.Clear();
      // AddLights();
      if (_thirdEyeScene != null)
      {
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
          CameraContext cameraContext = new CameraContext
          {
            CameraToWorldTransform = camera.transform.localToWorldMatrix,
            CameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera),
            OpaqueBuffer = _opaqueBuffer,
            TransparentBuffer = _transparentBuffer
          };
          _thirdEyeScene.Render(cameraContext);
        }
      }
    }

    private CommandBuffer _opaqueBuffer = null;
    private CommandBuffer _transparentBuffer = null;
  }
}