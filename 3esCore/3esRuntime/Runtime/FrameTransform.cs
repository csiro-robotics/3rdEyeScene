using Tes.Net;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// A utility class for transforming between a selected <see cref="CoordinateFrame"/> and the Unity coordinate frame.
  /// </summary>
  /// <remarks>
  /// The primary purpose of this object is to manage conversions from the server's
  /// <see cref="CoordinateFrame"/> to the Unity frame.
  /// </remarks>
  public static class FrameTransform
  {
    /// <summary>
    /// Set <paramref name="transform"/> to reflect a rotation from <paramref name="frame"/> to
    /// the Unity coordinate frame.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="frame">The source <see cref="CoordinateFrame"/>.</param>
    public static void SetFrameRotation(Transform transform, CoordinateFrame frame)
    {
      // Note: right handed frames require the left/right axis be negated.
      // We use a scale of -1 to do so.
      switch (frame)
      {
      default:
      case CoordinateFrame.XYZ:
        // Right handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case CoordinateFrame.XZ_Y:
        // Right handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(0, -1, 0));
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case CoordinateFrame.YX_Z:
        // Right handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), new Vector3(1, 0, 0));
        transform.localScale = new Vector3(-1, 1, -1);
        break;
      case CoordinateFrame.YZX:
        // Right handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(1, 0, 0));
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case CoordinateFrame.ZXY:
        // Right handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(1, 0, 0), new Vector3(0, 1, 0));
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case CoordinateFrame.ZY_X:
        // Right handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        transform.localScale = new Vector3(-1, 1, 1);
        break;
      case CoordinateFrame.XY_Z:
        // Left handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), new Vector3(0, 1, 0));
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case CoordinateFrame.XZY:
        // Left handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(0, 1, 0));
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case CoordinateFrame.YXZ:
        // Left handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, 1, 0), new Vector3(1, 0, 0));
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case CoordinateFrame.YZ_X:
        // Left handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(1, 0, 0));
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case CoordinateFrame.ZX_Y:
        // Left handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(1, 0, 0), new Vector3(0, -1, 0));
        transform.localScale = new Vector3(1, 1, 1);
        break;
      case CoordinateFrame.ZYX:
        // Left handed
        transform.localRotation = Quaternion.LookRotation(new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        transform.localScale = new Vector3(1, 1, 1);
        break;
      }
    }


    /// <summary>
    /// Convert a vector from the specified coordinate frame to the Unity frame.
    /// </summary>
    /// <param name="v">The vector to convert</param>
    /// <param name="frame">The target coordinate frame.</param>
    /// <returns>The converted vector.</returns>
    /// <remarks>
    /// The Unity frame is <see cref="CoordinateFrame.XZY"/>.
    /// </remarks>
    public static Vector3 RemoteToUnity(Vector3 v, CoordinateFrame frame)
    {
      // Note: right handed frames require the left/right axis be negated.
      // We use a scale of -1 to do so.
      switch (frame)
      {
      case CoordinateFrame.XYZ:
        // Right handed
        return new Vector3(-v.x, v.z, v.y);
      case CoordinateFrame.XZ_Y:
        // Right handed
        return new Vector3(-v.x, -v.y, v.z);
      case CoordinateFrame.YX_Z:
        // Right handed
        return new Vector3(-v.y, -v.z, v.x);
      case CoordinateFrame.YZX:
        // Right handed
        return new Vector3(-v.y, v.x, v.z);
      case CoordinateFrame.ZXY:
        // Right handed
        return new Vector3(-v.z, v.y, v.x);
      case CoordinateFrame.ZY_X:
        // Right handed
        return new Vector3(-v.z, -v.x, v.y);
      case CoordinateFrame.XY_Z:
        // Left handed
        return new Vector3(v.x, -v.z, v.y);
      default:
      case CoordinateFrame.XZY:
        // Left handed
        return v;
      case CoordinateFrame.YXZ:
        // Left handed
        return new Vector3(v.y, v.z, v.x);
      case CoordinateFrame.YZ_X:
        // Left handed
        return new Vector3(v.y, -v.x, v.z);
      case CoordinateFrame.ZX_Y:
        // Left handed
        return new Vector3(v.z, -v.y, v.x);
      case CoordinateFrame.ZYX:
        // Left handed
        return new Vector3(v.z, v.x, v.y);
      }
    }


    /// <summary>
    /// Convert a vector from the Unity frame to the specified coordinate fame.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <param name="frame">The source coordinate frame.</param>
    /// <returns>The converted vector.</returns>
    /// <remarks>
    /// The Unity frame is <see cref="CoordinateFrame.XZY"/>.
    /// </remarks>
    public static Vector3 UnityToRemote(Vector3 v, CoordinateFrame frame)
    {
      // Note: right handed frames require the left/right axis be negated.
      // We use a scale of -1 to do so.
      switch (frame)
      {
      case CoordinateFrame.XYZ:
        // Right handed
        return new Vector3(-v.x, v.z, v.y);
      case CoordinateFrame.XZ_Y:
        // Right handed
        return new Vector3(-v.x, -v.y, v.z);
      case CoordinateFrame.YX_Z:
        // Right handed
        return new Vector3(v.z, -v.x, -v.y);
      case CoordinateFrame.YZX:
        // Right handed
        return new Vector3(v.y, -v.x, v.z);
      case CoordinateFrame.ZXY:
        // Right handed
        return new Vector3(v.z, v.y, -v.x);
      case CoordinateFrame.ZY_X:
        // Right handed
        return new Vector3(-v.y, v.z, -v.x);
      case CoordinateFrame.XY_Z:
        // Left handed
        return new Vector3(v.x, v.z, -v.y);
      default:
      case CoordinateFrame.XZY:
        // Left handed
        return v;
      case CoordinateFrame.YXZ:
        // Left handed
        return new Vector3(v.z, v.x, v.y);
      case CoordinateFrame.YZ_X:
        // Left handed
        return new Vector3(-v.y, v.x, v.z);
      case CoordinateFrame.ZX_Y:
        // Left handed
        return new Vector3(v.z, -v.y, v.x);
      case CoordinateFrame.ZYX:
        // Left handed
        return new Vector3(v.y, v.z, v.x);
      }
    }


    /// <summary>
    /// Set <paramref name="transform"/> to reflect a rotation from <paramref name="frame"/> to
    /// the Unity coordinate frame.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="frame">The source <see cref="CoordinateFrame"/>.</param>
    public static void SetFrameRotation(ref Matrix4x4 transform, CoordinateFrame frame)
    {
      switch (frame)
      {
      case CoordinateFrame.XYZ:
        // Right handed
        transform.SetColumn(0, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, 1, 0));
        transform.SetColumn(2, new Vector4(0, 1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.XZ_Y:
        // Right handed
        transform.SetColumn(0, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(1, new Vector4(0, -1, 0, 0));
        transform.SetColumn(2, new Vector4(0, 0, 1, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.YX_Z:
        // Right handed
        transform.SetColumn(0, new Vector4(0, -1, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, -1, 0));
        transform.SetColumn(2, new Vector4(1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.YZX:
        // Right handed
        transform.SetColumn(0, new Vector4(0, -1, 0, 0));
        transform.SetColumn(1, new Vector4(1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 0, 1, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZXY:
        // Right handed
        transform.SetColumn(0, new Vector4(0, 0, -1, 0));
        transform.SetColumn(1, new Vector4(0, 1, 0, 0));
        transform.SetColumn(2, new Vector4(1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZY_X:
        // Right handed
        transform.SetColumn(0, new Vector4(0, 0, -1, 0));
        transform.SetColumn(1, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.XY_Z:
        // Left handed
        transform.SetColumn(0, new Vector4(1, 0, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, -1, 0));
        transform.SetColumn(2, new Vector4(0, 1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      default:
      case CoordinateFrame.XZY:
        // Left handed
        transform = Matrix4x4.identity;
        break;
      case CoordinateFrame.YXZ:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 1, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, 1, 0));
        transform.SetColumn(2, new Vector4(1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.YZ_X:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 1, 0, 0));
        transform.SetColumn(1, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 0, 1, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZX_Y:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 0, 1, 0));
        transform.SetColumn(1, new Vector4(0, -1, 0, 0));
        transform.SetColumn(2, new Vector4(1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZYX:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 0, 1, 0));
        transform.SetColumn(1, new Vector4(1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      }
    }

    /// <summary>
    /// Set <paramref name="transform"/> to reflect a rotation to <paramref name="frame"/> from
    /// the Unity coordinate frame.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="frame">The target <see cref="CoordinateFrame"/>.</param>

    public static void SetFrameRotationInverse(ref Matrix4x4 transform, CoordinateFrame frame)
    {
      //SetFrameRotation(ref transform, frame);
      //transform = transform.inverse;
      switch (frame)
      {
      case CoordinateFrame.XYZ:
        // Right handed
        transform.SetColumn(0, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, 1, 0));
        transform.SetColumn(2, new Vector4(0, 1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.XZ_Y:
        // Right handed
        transform.SetColumn(0, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(1, new Vector4(0, -1, 0, 0));
        transform.SetColumn(2, new Vector4(0, 0, 1, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.YX_Z:
        // Right handed
        transform.SetColumn(0, new Vector4(0, 0, 1, 0));
        transform.SetColumn(1, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, -1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.YZX:
        // Right handed
        transform.SetColumn(0, new Vector4(0, 1, 0, 0));
        transform.SetColumn(1, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 0, 1, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZXY:
        // Right handed
        transform.SetColumn(0, new Vector4(0, 0, 1, 0));
        transform.SetColumn(1, new Vector4(0, 1, 0, 0));
        transform.SetColumn(2, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZY_X:
        // Right handed
        transform.SetColumn(0, new Vector4(0, -1, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, 1, 0));
        transform.SetColumn(2, new Vector4(-1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.XY_Z:
        // Left handed
        transform.SetColumn(0, new Vector4(1, 0, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, 1, 0));
        transform.SetColumn(2, new Vector4(0, -1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      default:
      case CoordinateFrame.XZY:
        // Left handed
        transform = Matrix4x4.identity;
        break;
      case CoordinateFrame.YXZ:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 0, 1, 0));
        transform.SetColumn(1, new Vector4(1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 1, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.YZ_X:
        // Left handed
        transform.SetColumn(0, new Vector4(0, -1, 0, 0));
        transform.SetColumn(1, new Vector4(1, 0, 0, 0));
        transform.SetColumn(2, new Vector4(0, 0, 1, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZX_Y:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 0, 1, 0));
        transform.SetColumn(1, new Vector4(0, -1, 0, 0));
        transform.SetColumn(2, new Vector4(1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      case CoordinateFrame.ZYX:
        // Left handed
        transform.SetColumn(0, new Vector4(0, 1, 0, 0));
        transform.SetColumn(1, new Vector4(0, 0, 1, 0));
        transform.SetColumn(2, new Vector4(1, 0, 0, 0));
        transform.SetColumn(3, new Vector4(0, 0, 0, 1));
        break;
      }
    }
  }
}
