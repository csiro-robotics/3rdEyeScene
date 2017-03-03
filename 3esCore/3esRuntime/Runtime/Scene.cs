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
      FrameTransform.SetFrameRotation(ServerRoot.transform, Frame);
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
          FrameTransform.SetFrameRotation(ServerRoot.transform, _frame);
        }
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
