using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System;
using System.Collections.Generic;
using Tes.IO;
using Tes.Net;
using Tes.Server;

public class TesServer : MonoBehaviour
{
  public enum Ternary
  {
    Any,
    False,
    True
  }

  /// <summary>
  /// Used to assign colours to objects.
  /// </summary>
  /// <remarks>
  /// First match is used, so order is important.
  /// </remarks>
  [Serializable]
  public struct ColourLookup
  {
    public string Name;
    public Color32 Colour;
    public Ternary IsStatic;
    public Ternary IsTrigger;
    public string Layer;
    public string Tag;
  }

  public static TesServer Instance { get; protected set; }

  public Color32 DefaultColour = new Color32(128, 128, 128, 255);
  public ColourLookup[] Colours = new ColourLookup[]
  {
    new ColourLookup { Name = "Static Triggers", Colour = new Color32(0, 150, 150, 90), IsTrigger = Ternary.True, IsStatic = Ternary.True },
    new ColourLookup { Name = "Triggers", Colour = new Color32(0, 150, 192, 90), IsTrigger = Ternary.True },
    new ColourLookup { Name = "Player", Colour = new Color32(160, 255, 128, 255), Tag = "Player" },
    new ColourLookup { Name = "Static", Colour = new Color32(192, 192, 192, 255), IsStatic = Ternary.True },
  };

  public string[] Categories = new string[]
  {
    "Category00",
    "Category01",
    "Category02",
    "Category03",
    "Category04",
    "Category05",
    "Category06",
    "Category07",
    "Category08",
    "Category09",
    "Category10",
    "Category11",
    "Category12",
    "Category13",
    "Category14",
    "Category15",
    "Category16",
    "Category17",
    "Category18",
    "Category19",
    "Category20",
    "Category21",
    "Category22",
    "Category23",
    "Category24",
    "Category25",
    "Category26",
    "Category27",
    "Category28",
    "Category29",
    "Category30",
    "Category31"
  };

  [SerializeField]
  public ServerSettings Settings = ServerSettings.Default;

  public bool TrackMainCamera = true;

  public TcpServer Server
  {
    get { return _server; }
  }

  public static Tes.Maths.Vector3 ToTes(Vector3 v)
  {
    return new Tes.Maths.Vector3(v.x, v.y, v.z);
  }

  public static Quaternion FromTes(Tes.Maths.Quaternion q)
  {
    return new Quaternion(q.X, q.Y, q.Z, q.W);
  }

  public static Tes.Maths.Quaternion ToTes(Quaternion q)
  {
    return new Tes.Maths.Quaternion(q.x, q.y, q.z, q.w);
  }

  public static Tes.Maths.Colour ToTes(Color c)
  {
    return new Tes.Maths.Colour((int)(c.r * 255.0f), (int)(c.g * 255.0f), (int)(c.b * 255.0f), (int)(c.a * 255.0f));
  }

  public static Tes.Maths.Colour ToTes(Color32 c)
  {
    return new Tes.Maths.Colour(c.r, c.g, c.b, c.a);
  }

#if UNITY_EDITOR
  public void AttachViews()
  {
    // Attach physics view to all physics objects.
    foreach (Collider collider in Resources.FindObjectsOfTypeAll(typeof(Collider)))
    {
      if (collider.GetComponent<TesPhysicsView>() == null)
      {
        // Add views only to prefabs or objects with no prefab.
        switch (PrefabUtility.GetPrefabType(collider.gameObject))
        {
        case PrefabType.None:
        case PrefabType.Prefab:
          collider.gameObject.AddComponent<TesPhysicsView>();
          break;
        }
      }
    }
  }


  public void RemoveViews()
  {
    // Attach physics view to all physics objects.
    foreach (TesPhysicsView physicsView in Resources.FindObjectsOfTypeAll(typeof(TesPhysicsView)))
    {
      TesPhysicsView.DestroyImmediate(physicsView, true);
    }
  }
#endif // UNITY_EDITOR


  public void Start()
  {
    // Unity coordinate frame.
    _info.CoordinateFrame = Tes.Net.CoordinateFrame.XZY;
    _packet = new PacketBuffer(1024);

    if (Instance == null)
    {
      Instance = this;

      foreach (TesPhysicsView view in Resources.FindObjectsOfTypeAll(typeof(TesPhysicsView)))
      {
        Add(view);
      }
    }

    StopServer();

    // Manual cache management transmit on new connection.
    Settings.Flags &= ~ServerFlag.Collate;  // Not working
    _server = new TcpServer(Settings, _info);
    _server.ConnectionMonitor.Start(ConnectionMonitorMode.Asynchronous);
  }


  public void OnDisable()
  {
    if (Instance == this)
    {
      Instance = null;
    }
    _staticViews.Clear();
    _dynamicViews.Clear();

    StopServer();
  }


  public void OnDestroy()
  {
    // Make sure the server is cleaned up.
    StopServer();
  }


  public void Add(TesView view)
  {
    GameObject obj = view.GameObject;
    if (obj != null && view.Shape != null)
    {
      Color32 colour = LookupColour(obj, obj.GetComponent<Collider>());
      view.Shape.Colour = new Tes.Maths.Colour(colour.r, colour.g, colour.b, colour.a).Value;
      view.Shape.Transparent = colour.a < 255;
    }

    if (view.Dynamic)
    {
      _dynamicViews.Add(view);
    }
    else
    { 
      _staticViews.Add(view);
    }

    if (_server != null && view.Shape != null)
    {
      _server.Create(view.Shape);
    }
  }


  public void Remove(TesView view)
  {
    _staticViews.Remove(view);
    _dynamicViews.Remove(view);

    if (_server != null && view.Shape != null)
    {
      _server.Destroy(view.Shape);
    }
  }


  public virtual Color32 LookupColour(GameObject obj, Collider collider)
  {
    Color32 colour = DefaultColour;

    for (int i = 0; i < Colours.Length; ++i)
    {
      if (IsMatch(Colours[i], obj, collider))
      {
        colour = Colours[i].Colour;
        return colour;
      }
    }

    return colour;
  }


  public virtual bool IsMatch(ColourLookup colour, GameObject obj, Collider collider)
  {
    bool ok = true;
    ok = ok && (colour.IsStatic == Ternary.Any || (colour.IsStatic == Ternary.True && obj.isStatic || colour.IsStatic == Ternary.False && !obj.isStatic));
    ok = ok && (colour.IsTrigger == Ternary.Any || (colour.IsTrigger == Ternary.True && collider != null && collider.isTrigger || colour.IsTrigger == Ternary.False && (collider == null || !collider.isTrigger)));
    ok = ok && (string.IsNullOrEmpty(colour.Layer) || (LayerMask.NameToLayer(colour.Layer) & obj.layer) != 0);
    ok = ok && (string.IsNullOrEmpty(colour.Tag) || obj.CompareTag(colour.Tag));
    return ok;
  }


  /// <summary>
  /// Update the server connection.
  /// </summary>
  /// <remarks>
  /// Should probably use FixedUpdate(), but make sure it goes last.
  /// </remarks>
  void LateUpdate()
  {
    if (_server == null)
    {
      return;
    }

    // Update new connections.
    if (_server.ConnectionMonitor.Mode == ConnectionMonitorMode.Synchronous)
    {
      _server.ConnectionMonitor.MonitorConnections();
    }
    _server.ConnectionMonitor.CommitConnections(this.OnNewConnection);

    UpdateViews();

    if (TrackMainCamera)
    {
      UpdateCamera();
    }

    _server.UpdateTransfers(64 * 1024);
    _server.UpdateFrame(Time.deltaTime);
  }

  protected void OnNewConnection(IServer server, IConnection connection)
  {
    // Send cached objects.
    for (int i = 0; i < _staticViews.Count; ++i)
    {
      if (_staticViews[i].Shape != null)
      {
        connection.Create(_staticViews[i].Shape);
      }
    }

    for (int i = 0; i < _dynamicViews.Count; ++i)
    {
      if (_dynamicViews[i].Shape != null)
      {
        connection.Create(_dynamicViews[i].Shape);
      }
    }

    connection.UpdateFrame(0.0f, false);
  }

  protected void UpdateViews()
  {
    for (int i = 0; i < _dynamicViews.Count; ++i)
    {
      if (_dynamicViews[i].UpdateView() && _dynamicViews[i].Shape != null)
      {
        _server.Update(_dynamicViews[i].Shape);
      }
    }
  }

  protected void UpdateCamera()
  {
    // Compose and send a camera update message.
    Camera camera = Camera.main;
    Transform camXForm = camera.transform;
    CameraMessage msg = new CameraMessage();
    msg.CameraID = 1; // Main.
    msg.X = camXForm.position.x;
    msg.Y = camXForm.position.y;
    msg.Z = camXForm.position.z;
    msg.DirX = camXForm.forward.x;
    msg.DirY = camXForm.forward.y;
    msg.DirZ = camXForm.forward.z;
    msg.UpX = camXForm.up.x;
    msg.UpY = camXForm.up.y;
    msg.UpZ = camXForm.up.z;
    msg.Far = camera.farClipPlane;
    msg.Near = camera.nearClipPlane;
    msg.FOV = camera.fieldOfView;
    msg.Reserved1 = 0;
    msg.Reserved2 = 0;

    // Write to packet and set.
    _packet.Reset((ushort)RoutingID.Camera, 0);
    msg.Write(_packet);
    if (_packet.FinalisePacket())
    {
      _server.Send(_packet);
    }
  }

  protected void StopServer()
  {
    if (_server != null)
    {
      // TODO: add close command.
      _server.ConnectionMonitor.Stop();
      _server.ConnectionMonitor.Join();
      _server = null;
    }
  }


  #region Shape helper methods
  // TODO: set shape categories by maintaining a category stack and assigning the top value to all shape calls.

  /// <summary>
  /// Destroy a persistent arrow.
  /// </summary>
  /// <param name="id"></param>
  public static void ArrowEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Arrow(id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update a persistent arrow.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="origin"></param>
  /// <param name="dir"></param>
  /// <param name="length"></param>
  /// <param name="radius"></param>
  public static void Arrow(uint id, Color colour, Vector3 origin, Vector3 dir, float length, float radius, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Arrow(id, ToTes(origin), ToTes(dir), length, radius);
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create a persistent arrow.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="origin"></param>
  /// <param name="target"></param>
  /// <param name="radius"></param>
  public static void Arrow(uint id, Color colour, Vector3 origin, Vector3 target, float radius = 0.05f)
  {
    target = target - origin;
    float len = target.magnitude;
    if (len > 1e-5f)
    {
      target *= 1.0f / len;
    }
    Arrow(id, colour, origin, target, len, radius);
  }

  /// <summary>
  /// Create a transient arrow.
  /// </summary>
  /// <param name="colour"></param>
  /// <param name="origin"></param>
  /// <param name="dir"></param>
  /// <param name="length"></param>
  /// <param name="radius"></param>
  public static void Arrow(Color colour, Vector3 origin, Vector3 dir, float length, float radius)
  {
    Arrow(0, colour, origin, dir, length, radius);
  }

  /// <summary>
  /// Create a transient arrow.
  /// </summary>
  /// <param name="colour"></param>
  /// <param name="origin"></param>
  /// <param name="target"></param>
  /// <param name="radius"></param>
  public static void Arrow(Color colour, Vector3 origin, Vector3 target, float radius = 0.05f)
  {
    Arrow(0u, colour, origin, target, radius);
  }

  /// <summary>
  /// Destroy a persistent box (including AABB).
  /// </summary>
  /// <param name="id"></param>
  public static void BoxEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Box(id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update a persistent axis aligned box.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="dimensions"></param>
  /// <param name="update"></param>
  public static void AABB(uint id, Color colour, Vector3 centre, Vector3 dimensions, bool update = false)
  {
    Box(id, colour, centre, dimensions, Quaternion.identity, update);
  }

  /// <summary>
  /// Create a transient axis aligned box.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="dimensions"></param>
  public static void AABB(Color colour, Vector3 centre, Vector3 dimensions)
  {
    Box(0, colour, centre, dimensions, Quaternion.identity);
  }

  /// <summary>
  /// Create or update a persistent oriented box.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="dimensions"></param>
  /// <param name="rotation"></param>
  public static void Box(uint id, Color colour, Vector3 centre, Vector3 dimensions, Quaternion rotation, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Box(id, ToTes(centre), ToTes(dimensions), ToTes(rotation));
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create a transient oriented box.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="dimensions"></param>
  /// <param name="rotation"></param>
  public static void Box(Color colour, Vector3 centre, Vector3 dimensions, Quaternion rotation)
  {
    Box(0, colour, centre, dimensions, rotation);
  }

  /// <summary>
  /// Destroy a persistent capsule.
  /// </summary>
  /// <param name="id"></param>
  public static void CapsuleEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Capsule(id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update a persistent capsule.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="up"></param>
  /// <param name="length"></param>
  /// <param name="radius"></param>
  public static void Capsule(uint id, Color colour, Vector3 centre, Vector3 up, float length, float radius, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Capsule(id, ToTes(centre), ToTes(up), length, radius);
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create a persistent capsule.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="target"></param>
  /// <param name="radius"></param>
  public static void Capsule(uint id, Color colour, Vector3 centre, float length, float radius)
  {
    Capsule(id, colour, centre, Vector3.up, length, radius);
  }

  /// <summary>
  /// Create a transient capsule.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="up"></param>
  /// <param name="length"></param>
  /// <param name="radius"></param>
  public static void Capsule(Color colour, Vector3 centre, Vector3 up, float length, float radius)
  {
    Capsule(0u, colour, centre, up, length, radius);
  }

  /// <summary>
  /// Create a transient capsule.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="target"></param>
  /// <param name="radius"></param>
  public static void Capsule(Color colour, Vector3 centre, float length, float radius)
  {
    Capsule(0, colour, centre, Vector3.up, length, radius);
  }

  /// <summary>
  /// Destroy a persistent cone.
  /// </summary>
  /// <param name="id"></param>
  public static void ConeEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Cone(id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update a persistent cone.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="apex"></param>
  /// <param name="dir"></param>
  /// <param name="angle">Angle in degrees.</param>
  /// <param name="length"></param>
  public static void Cone(uint id, Color colour, Vector3 apex, Vector3 dir, float angle, float length, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Cone(id, ToTes(apex), ToTes(dir), angle / 180.0f * Mathf.PI, length);
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create a transient cone.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="apex"></param>
  /// <param name="dir"></param>
  /// <param name="angle">Angle in degrees.</param>
  /// <param name="length"></param>
  public static void Cone(Color colour, Vector3 apex, Vector3 dir, float angle, float length)
  {
    Cone(0u, colour, apex, dir, angle, length);
  }

  /// <summary>
  /// Destroy a persistent cylinder.
  /// </summary>
  /// <param name="id"></param>
  public static void CylinderEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Cylinder(id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update a persistent cylinder.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="up"></param>
  /// <param name="length"></param>
  /// <param name="radius"></param>
  public static void Cylinder(uint id, Color colour, Vector3 centre, Vector3 up, float length, float radius, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Cylinder(id, ToTes(centre), ToTes(up), length, radius);
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create a persistent cylinder.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="target"></param>
  /// <param name="radius"></param>
  public static void Cylinder(uint id, Color colour, Vector3 centre, float length, float radius)
  {
    Cylinder(id, colour, centre, Vector3.up, length, radius);
  }

  /// <summary>
  /// Create a transient cylinder.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="up"></param>
  /// <param name="length"></param>
  /// <param name="radius"></param>
  public static void Cylinder(Color colour, Vector3 centre, Vector3 up, float length, float radius)
  {
    Cylinder(0u, colour, centre, up, length, radius);
  }

  /// <summary>
  /// Create a transient cylinder.
  /// </summary>
  /// <param name="colour"></param>
  /// <param name="target"></param>
  /// <param name="radius"></param>
  public static void Cylinder(Color colour, Vector3 centre, float length, float radius)
  {
    Cylinder(0, colour, centre, Vector3.up, length, radius);
  }

  /// <summary>
  /// Destroy a persistent sphere.
  /// </summary>
  /// <param name="id"></param>
  public static void SphereEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Sphere(id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update a persistent sphere.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="radius"></param>
  /// <param name="update"></param>
  public static void Sphere(uint id, Color colour, Vector3 centre, float radius, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Sphere(id, ToTes(centre), radius);
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create a transient sphere.
  /// </summary>
  /// <param name="colour"></param>
  /// <param name="centre"></param>
  /// <param name="radius"></param>
  public static void Sphere(Color colour, Vector3 centre, float radius)
  {
    Sphere(0, colour, centre, radius);
  }

  /// <summary>
  /// Destroy persistent 2D text. Works for screen and world space 2D text.
  /// </summary>
  /// <param name="id"></param>
  public static void Text2DEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Text2D("", id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update 2D world space text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  /// <param name="colour"></param>
  /// <param name="update"></param>
  public static void Text2DWorld(uint id, string text, Vector3 location, Color colour, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Text2D(text, id, ToTes(location));
      shape.InWorldSpace = true;
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create persistent 2D world space text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text2DWorld(uint id, string text, Vector3 location)
  {
    Text2DWorld(id, text, location, Color.white);
  }

  /// <summary>
  /// Create transient 2D world space text.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text2DWorld(string text, Vector3 location)
  {
    Text2DWorld(0, text, location);
  }

  /// <summary>
  /// Create or update persistent 2D screen space text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  /// <param name="colour"></param>
  /// <param name="update"></param>
  public static void Text2DScreen(uint id, string text, Vector2 location, Color colour, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Text2D(text, id, new Tes.Maths.Vector3(location.x, location.y, 0.0f));
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create persistent 2D screen space text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text2DScreen(uint id, string text, Vector2 location)
  {
    Text2DScreen(id, text, location, Color.white);
  }

  /// <summary>
  /// Create transient 2D screen space text.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text2DScreen(string text, Vector2 location)
  {
    Text2DScreen(0, text, location);
  }


  /// <summary>
  /// Destroy persistent 3D text.
  /// </summary>
  /// <param name="id"></param>
  public static void Text3DEnd(uint id)
  {
    if (Instance != null && id != 0)
    {
      var shape = new Tes.Shapes.Text3D("", id);
      Instance.Server.Destroy(shape);
    }
  }

  /// <summary>
  /// Create or update 3D text with fixed facing.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  /// <param name="facing"></param>
  /// <param name="colour"></param>
  /// <param name="update"></param>
  public static void Text3D(uint id, string text, Vector3 location, Vector3 facing, Color colour, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Text3D(text, id, ToTes(location));
      shape.Facing = ToTes(facing);
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create or update 3D billboard text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  /// <param name="colour"></param>
  /// <param name="update"></param>
  public static void Text3D(uint id, string text, Vector3 location, Color colour, bool update = false)
  {
    if (Instance != null)
    {
      var shape = new Tes.Shapes.Text3D(text, id, ToTes(location));
      shape.ScreenFacing = true;
      shape.Colour = ToTes(colour).Value;
      if (!update || id == 0)
      {
        Instance.Server.Create(shape);
      }
      else
      {
        Instance.Server.Update(shape);
      }
    }
  }

  /// <summary>
  /// Create persistent 3D billboard text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text3D(uint id, string text, Vector3 location)
  {
    Text3D(id, text, location, Color.white);
  }

  /// <summary>
  /// Create transient 3D billboard text.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text3D(string text, Vector3 location)
  {
    Text3D(0, text, location);
  }

  /// <summary>
  /// Create persistent 3D oriented text.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text3D(uint id, string text, Vector3 location, Vector3 facing)
  {
    Text3D(id, text, location, facing, Color.white);
  }

  /// <summary>
  /// Create transient 3D oriented text.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="location"></param>
  public static void Text3D(string text, Vector3 location, Vector3 facing)
  {
    Text3D(0, text, location, facing);
  }

  #endregion


  private List<TesView> _staticViews = new List<TesView>();
  private List<TesView> _dynamicViews = new List<TesView>();
  private ServerInfoMessage _info = ServerInfoMessage.Default;
  private TcpServer _server = null;
  private PacketBuffer _packet = null;
}
