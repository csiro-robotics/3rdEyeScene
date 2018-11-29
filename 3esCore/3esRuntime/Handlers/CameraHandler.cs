using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers
{
  /// <summary>
  /// Maintains camera objects which may be used to view the scene.
  /// </summary>
  /// <remarks>
  /// Camera objects represent predetermined views into the scene. A camera object is
  /// really just a reference transform which the scene camera may optionally follow.
  ///
  /// Camera objects are implicitly created when a message with a new camera ID arrives.
  /// A camera object is never destroyed and may only be updated with a new message.
  /// </remarks>
  public class CameraHandler : MessageHandler
  {
    /// <summary>
    /// Details of the camera settings.
    /// </summary>
    public class CameraInfo : MonoBehaviour
    {
      /// <summary>
      /// Camera ID.
      /// </summary>
      public byte ID;
      /// <summary>
      /// Near clip plane.
      /// </summary>
      public float Near;
      /// <summary>
      /// Far clip plane.
      /// </summary>
      public float Far;
      /// <summary>
      /// Field of view.
      /// </summary>
      public float FOV;
    }


    /// <summary>
    /// Constructor initialising the persistent and transient caches.
    /// </summary>
    public CameraHandler(CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _root.name = Name;
    }

    private int _activeCameraID = -1;
    /// <summary>
    /// Get/set the active camera ID [-1, 255].
    /// </summary>
    /// <remarks>
    /// Setting -1 restores the free fly camera mode as does setting an unknown camera ID.
    /// </remarks>
    public int ActiveCameraID
    {
      get { return _activeCameraID; }
      set
      {
        _activeCameraID = -1;
        if (0 <= value && value <= 255)
        {
          if (_cameras.ContainsKey((byte)value))
          {
            _activeCameraID = value;
          }
        }
      }
    }

    /// <summary>
    /// Allow the remote camera settings to set the local near clip and far clip and field of view
    /// settings?
    /// </summary>
    public bool AllowRemoteCameraSettings { get; set; }


    /// <summary>
    /// Query details of the active camera.
    /// </summary>
    public CameraInfo ActiveCamera
    {
      get
      {
        CameraInfo camera = null;
        if (_activeCameraID >= 0 && _cameras.TryGetValue((byte)_activeCameraID, out camera))
        {
          return camera;
        }

        return null;
      }
    }

    /// <summary>
    /// Lists the available camera IDs.
    /// </summary>
    public IEnumerable<int> AvailableCameraIDs
    {
      get
      {
        foreach (byte key in _cameras.Keys)
        {
          yield return (int)key;
        }
      }
    }

    /// <summary>
    /// Index into the camera handler to information about the camera matching <paramref name="id" />.
    /// </summary>
    /// <param name="id">ID for the camera of interest.</param>
    /// <value>Information about the requested camera or null if the ID is invalid.</value>
    public CameraInfo this[int id]
    {
      get
      {
        CameraInfo info;
        //// Special camera ID.
        //if (id == 0) id = 255;
        if (_cameras.TryGetValue((byte)id, out info))
        {
          return info;
        }
        return null;
      }
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Camera"; } }
    /// <summary>
    /// Routing ID.
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Net.RoutingID.Camera; } }

    /// <summary>
    /// Defines the scene root for all objects of this shape type.
    /// </summary>
    /// <value>The root for objects of this shape.</value>
    public GameObject Root { get { return null; } }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <param name="maintainTransient"></param>
    public override void BeginFrame(uint frameNumber, bool maintainTransient)
    {
    }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="frameNumber"></param>
    public override void EndFrame(uint frameNumber)
    {
    }

    /// <summary>
    /// Initialise the shape handler by initialising the shape scene root and
    /// fetching the default materials.
    /// </summary>
    /// <param name="root">The 3rd Eye Scene root object.</param>
    /// <param name="serverRoot">The server scene root (transformed into the server reference frame).</param>
    /// <param name="materials">Material library from which to resolve materials.</param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      // Note: cameras are maintained in Unity coordinate space, thus we do not add
      // it to the scene root.
      // We still make it a child of root to maintain Unity clean order consistency.
      _root.transform.SetParent(root.transform, true);
    }

    /// <summary>
    /// Clear all current objects.
    /// </summary>
    public override void Reset()
    {
      foreach (CameraInfo camera in _cameras.Values)
      {
        GameObject.Destroy(camera.gameObject);
      }
      _cameras.Clear();
    }

    /// <summary>
    /// The primary message handling function.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public override Error ReadMessage(PacketBuffer packet, BinaryReader reader)
    {
      CameraMessage msg = new CameraMessage();
      if (!msg.Read(reader))
      {
        return new Error(ErrorCode.MalformedMessage);
      }

      CameraInfo camera = null;
      if (_cameras.ContainsKey(msg.CameraID))
      {
        camera = _cameras[msg.CameraID];
      }
      else
      {
        GameObject obj = new GameObject(string.Format("Camera {0}", msg.CameraID));
        camera = obj.AddComponent<CameraInfo>();
        _cameras.Add(msg.CameraID, camera);
        obj.transform.SetParent(_root.transform, false);
        camera.ID = msg.CameraID;
      }

      camera.Near = msg.Near;
      camera.Far = msg.Far;
      camera.FOV = msg.FOV;

      Transform transform = camera.transform;
      Vector3 dir = FrameTransform.RemoteToUnity(new Vector3(msg.DirX, msg.DirY, msg.DirZ), ServerInfo.CoordinateFrame);
      Vector3 up = FrameTransform.RemoteToUnity(new Vector3(msg.UpX, msg.UpY, msg.UpZ), ServerInfo.CoordinateFrame);
      transform.localPosition = FrameTransform.RemoteToUnity(new Vector3(msg.X, msg.Y, msg.Z), ServerInfo.CoordinateFrame);
      transform.LookAt(transform.position + dir, up);

      return new Error();
    }

    /// <summary>
    /// Serialises the currently active objects in for playback from file.
    /// </summary>
    /// <param name="writer">The write to serialise to.</param>
    /// <param name="info">Statistics</param>
    /// <returns>An error code on failure.</returns>
    public override Error Serialise(BinaryWriter writer, ref SerialiseInfo info)
    {
      Error err = new Error();
      info.PersistentCount = info.TransientCount = 0;
      if (_cameras.Count > 0)
      {
        PacketBuffer packet = new PacketBuffer(256);
        CameraMessage msg = new CameraMessage();
        Vector3 v = Vector3.zero;
        msg.Reserved1 = 0;
        msg.Reserved2 = 0;
        foreach (CameraInfo camera in _cameras.Values)
        {
          ++info.PersistentCount;
          msg.CameraID = camera.ID;
          v = FrameTransform.UnityToRemote(camera.transform.localPosition, ServerInfo.CoordinateFrame);
          msg.X = v.x;
          msg.Y = v.y;
          msg.Z = v.z;
          v = FrameTransform.UnityToRemote(camera.transform.forward, ServerInfo.CoordinateFrame);
          msg.DirX = v.x;
          msg.DirY = v.y;
          msg.DirZ = v.z;
          v = FrameTransform.UnityToRemote(camera.transform.up, ServerInfo.CoordinateFrame);
          msg.UpX = v.x;
          msg.UpY = v.y;
          msg.UpZ = v.z;
          msg.Near = camera.Near;
          msg.Far = camera.Far;
          msg.FOV = camera.FOV;
          packet.Reset(RoutingID, 0);
          msg.Write(packet);
          packet.FinalisePacket();
          packet.ExportTo(writer);
        }
      }

      return err;
    }

    /// <summary>
    /// Empty
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="active"></param>
    public override void OnCategoryChange(ushort categoryId, bool active)
    {
    }

    /// <summary>
    /// Scene root for camera frames.
    /// </summary>
    private GameObject _root = new GameObject();
    private Dictionary<byte, CameraInfo> _cameras = new Dictionary<byte, CameraInfo>();
  }
}
