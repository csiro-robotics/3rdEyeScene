using System;
using UnityEngine;
using Tes.Runtime;
using Tes.Net;

namespace Tes.Main
{
  /// <summary>
  /// Manages the scene root object.
  /// </summary>
  /// <remarks>
  /// The primary purpose of this object is to manage conversions from the server's
  /// <see cref="CoordinateFrame"/> to the Unity frame. The scene has two roots;
  /// <see cref="Root"/> - an object which is always in the Unity coordinate frame - and
  /// <see cref="ServerRoot"/> - an object maintained in the server <see cref="CoordinateFrame"/>.
  /// </remarks>
  public class Scene
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    public Scene()
    {
      Root = new GameObject("Root");
      ServerRoot = new GameObject("ServerRoot");
      ServerRoot.transform.SetParent(Root.transform, true);
      SetFrameRotation(ServerRoot.transform, Frame);
    }

    /// <summary>
    /// Root object always in the Unity coordinate frame.
    /// </summary>
    public GameObject Root { get; protected set; }
    /// <summary>
    /// Root object in the server's <see cref="CoordinateFrame"/>.
    /// </summary>
    public GameObject ServerRoot { get; protected set; }

    /// <summary>
    /// The server's <see cref="CoordinateFrame"/>.
    /// </summary>
    public CoordinateFrame Frame
    {
      get { return _frame; }
      set
      {
        if (_frame != value)
        {
          _frame = value;
          SetFrameRotation(ServerRoot.transform, _frame);
        }
      }
    }

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

    /// <summary>
    /// Query the server's 'right' axis index. This is the first letter in the current
    /// <see cref="CoordinateFrame"/>.
    /// </summary>
    /// <remarks>
    /// 0: X, 1: Y, 2: Z
    /// </remarks>
    public int RightAxis
    {
      get
      {
        switch (Frame)
        {
        default:
        case CoordinateFrame.XYZ:
          return 0;
        case CoordinateFrame.XZ_Y:
          return 0;
        case CoordinateFrame.YX_Z:
          return 1;
        case CoordinateFrame.YZX:
          return 1;
        case CoordinateFrame.ZXY:
          return 2;
        case CoordinateFrame.ZY_X:
          return 2;
        case CoordinateFrame.XY_Z:
          return 0;
        case CoordinateFrame.XZY:
          return 0;
        case CoordinateFrame.YXZ:
          return 1;
        case CoordinateFrame.YZ_X:
          return 1;
        case CoordinateFrame.ZX_Y:
          return 2;
        case CoordinateFrame.ZYX:
          return 2;
        }
      }
    }

    /// <summary>
    /// Query the server's 'up' axis index. This is the last letter in the current
    /// <see cref="CoordinateFrame"/>.
    /// </summary>
    /// <remarks>
    /// 0: X, 1: Y, 2: Z
    /// </remarks>
    public int UpAxis
    {
      get
      {
        switch (Frame)
        {
        default:
        case CoordinateFrame.XYZ:
          return 2;
        case CoordinateFrame.XZ_Y:
          return 1;
        case CoordinateFrame.YX_Z:
          return 2;
        case CoordinateFrame.YZX:
          return 0;
        case CoordinateFrame.ZXY:
          return 1;
        case CoordinateFrame.ZY_X:
          return 0;
        case CoordinateFrame.XY_Z:
          return 2;
        case CoordinateFrame.XZY:
          return 1;
        case CoordinateFrame.YXZ:
          return 2;
        case CoordinateFrame.YZ_X:
          return 0;
        case CoordinateFrame.ZX_Y:
          return 1;
        case CoordinateFrame.ZYX:
          return 0;
        }
      }
    }

    /// <summary>
    /// Query the server's 'forward' axis index. This is the middle letter in the current
    /// <see cref="CoordinateFrame"/>.
    /// </summary>
    /// <remarks>
    /// 0: X, 1: Y, 2: Z
    /// </remarks>
    public int ForwardAxis
    {
      get
      {
        switch (Frame)
        {
        default:
        case CoordinateFrame.XYZ:
          return 1;
        case CoordinateFrame.XZ_Y:
          return 2;
        case CoordinateFrame.YX_Z:
          return 0;
        case CoordinateFrame.YZX:
          return 2;
        case CoordinateFrame.ZXY:
          return 0;
        case CoordinateFrame.ZY_X:
          return 1;
        case CoordinateFrame.XY_Z:
          return 1;
        case CoordinateFrame.XZY:
          return 2;
        case CoordinateFrame.YXZ:
          return 0;
        case CoordinateFrame.YZ_X:
          return 2;
        case CoordinateFrame.ZX_Y:
          return 0;
        case CoordinateFrame.ZYX:
          return 1;
        }
      }
    }


    private CoordinateFrame _frame = CoordinateFrame.XYZ;
  }
}
