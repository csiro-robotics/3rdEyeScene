using UnityEngine;
using Tes.Net;
using Tes.Handlers;

/// <summary>
/// A simple script responsible for ensuring camera settings are migrated to the required targets.
/// </summary>
/// <remarks>
/// Migrates EDL, clip plane and field of view settings.
/// </remarks>
public class CameraSettingsMigrator : MonoBehaviour
{
  public TesComponent TesComponent = null;

  void Update()
  {
    EdlCamera edl = GetComponent<EdlCamera>();
    RenderSettings renderSettings = RenderSettings.Instance;
    Camera sceneCamera = null;
    if (edl != null)
    {
      edl.EdlOn = renderSettings.EdlShader;
      edl.EdlScale = renderSettings.EdlLinearScale;
      edl.EdlExpScale = renderSettings.EdlExponentialScale;

      if (edl.EdlSourceCamera != null)
      {
        sceneCamera = edl.EdlSourceCamera.GetComponent<Camera>();
      }
    }

    Camera cam = GetComponent<Camera>();
    if (cam != null)
    {
      CameraSettings camset = CameraSettings.Instance;
      if (!camset.AllowRemoteSettings)
      {
        cam.fieldOfView = camset.FOV;
        cam.nearClipPlane = camset.NearClip;
        cam.farClipPlane = camset.FarClip;
      }

      if (sceneCamera == null)
      {
        sceneCamera = cam;
      }
    }

    // Update background settings.
    if (sceneCamera)
    {
      if (renderSettings.Background == RenderSettings.RenderBackground.Skybox)
      {
        // Using skybox.
        if ((sceneCamera.clearFlags & CameraClearFlags.Skybox) == 0)
        {
          sceneCamera.clearFlags |= CameraClearFlags.Skybox;
        }
      }
      else
      {
        // Solid background colour.
        if ((sceneCamera.clearFlags & CameraClearFlags.Skybox) != 0)
        {
          sceneCamera.clearFlags &= ~CameraClearFlags.Skybox;
        }

        sceneCamera.backgroundColor = RenderSettings.BackgroundColours[(int)renderSettings.Background];
      }
    }

    if (TesComponent != null)
    {
      CameraHandler camhandler = TesComponent.GetHandler((ushort)RoutingID.Camera) as CameraHandler;
      if (camhandler != null)
      {
        CameraSettings camset = CameraSettings.Instance;
        camhandler.AllowRemoteCameraSettings = camset.AllowRemoteSettings;
      }
    }
  }
}
