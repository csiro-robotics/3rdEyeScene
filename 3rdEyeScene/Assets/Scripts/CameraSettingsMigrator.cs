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
    if (edl != null)
    {
      RenderSettings renset = RenderSettings.Instance;
      edl.EdlOn = renset.EdlShader;
      edl.EdlScale = renset.EdlLinearScale;
      edl.EdlExpScale = renset.EdlExponentialScale;
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
