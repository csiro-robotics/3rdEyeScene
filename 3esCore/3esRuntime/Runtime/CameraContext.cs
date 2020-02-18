using UnityEngine;
using UnityEngine.Rendering;

namespace Tes.Runtime
{
  public struct CameraContext
  {
    public Matrix4x4 CameraToWorldTransform;
    public Matrix4x4 TesSceneToWorldTransform;
    public CommandBuffer OpaqueBuffer;
    public CommandBuffer TransparentBuffer;
  }
}