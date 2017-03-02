using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Tes.Handlers.Shape2D;
using Tes.Handlers.Shape3D;
using Tes.Net;
using Tes.Runtime;
using Tes.Server;
using Tes.Shapes;
using UnityEngine;

/// <summary>
/// Test scene serialisation and restoration
/// </summary>
/// <remarks>
/// <list type="numbered">
/// <item>Create a TES server.</item>
/// <item>Connect the <see cref="TesComponent"/> to the server.</item>
/// <item>Create a scene with at least one of each type of shape.</item>
/// <item>Serialise the client scene.</item>
/// <item>Validate the scene.</item>
/// <item>Disconnect the server.</item>
/// <item>Restore the serialised scene.</item>
/// <item>Validate the scene.</item>
/// <item>Serialise the client scene again.</item>
/// <item>Compare serialised files.</item>
/// </list>
/// </remarks>
public class SerialisationTest : MonoBehaviour
{
  [Flags]
  public enum ValidationFlag
  {
    Position = (1 << 0),
    Rotation = (1 << 1),
    Scale = (1 << 2),
    Colour = (1 << 3),
    // Original shape position must be converted to Unity's left-handed system. Requires position flag.
    PositionConverted = (1 << 4),
    RotationAsNormal = (1 << 5),

    Default = Position | Rotation | Scale | Colour
  }

  public TesComponent tes = null;
  public ushort TestPort = 35035;
  public float ConnectWaitTime = 5.0f;
  public float StreamWaitTime = 1.0f;
  public CoordinateFrame ServerCoordinateFrame = CoordinateFrame.XYZ;
  private MeshResource _sampleMesh = null;

  void Start()
  {
    _validationFlags = new Dictionary<Type, ValidationFlag>();
    // Planes use rotation as a normal.
    _validationFlags.Add(typeof(Tes.Shapes.Plane),
                         ValidationFlag.Default | ValidationFlag.RotationAsNormal);
    // Rotation independent, or special rotation shapes:
    _validationFlags.Add(typeof(Sphere),
                         ValidationFlag.Position | ValidationFlag.Scale | ValidationFlag.Colour);
    _validationFlags.Add(typeof(Star),
                         ValidationFlag.Position | ValidationFlag.Scale | ValidationFlag.Colour);
    _validationFlags.Add(typeof(Text3D),
                         ValidationFlag.Position | ValidationFlag.Scale | ValidationFlag.Colour | ValidationFlag.PositionConverted);

    _specialObjectValidation = new Dictionary<Type, SpecialObjectValidation>();
    _specialValidation = new Dictionary<Type, SpecialValidation>();

    _specialObjectValidation.Add(typeof(MeshShape), ValidateMesh);
    _specialObjectValidation.Add(typeof(MeshSet), ValidateMeshSet);
    _specialObjectValidation.Add(typeof(PointCloudShape), ValidateCloud);
    _specialObjectValidation.Add(typeof(Text3D), ValidateText3D);

    _specialValidation.Add(typeof(Text2D), ValidateText2D);

    if (CanStart())
    {
      StartCoroutine(TestRoutine());
    }
  }

  public bool CanStart()
  {
#if !TRUE_THREADS
    Debug.LogError("3rd Eye Scene must be configured to use TRUE_THREADS in order to support the network communication required by this test.");
    return false;
#else  // !TRUE_THREADS
    return tes != null;
#endif // !TRUE_THREADS
  }

  IEnumerator TestRoutine()
  {
    List<Shape> shapes = new List<Shape>();
    float elapsedTime = 0;
    int minConnectAttempts = 10;
    int attempt = 0;
    int validationCount = 0;

    //--------------------------------------------------------
    // Initialise the server.
    IServer server = InitialiseServer();
    Debug.Log("Server initialised.");
    server.ConnectionMonitor.Start(ConnectionMonitorMode.Synchronous);
    yield return null;

    //--------------------------------------------------------
    // Connect sever.
    tes.Connect(new IPEndPoint(IPAddress.Loopback, TestPort), true);
    Debug.Log("Connecting...");

    elapsedTime = 0;
    attempt = 0;
    bool connected = false;
    do
    {
      server.ConnectionMonitor.MonitorConnections();
      server.ConnectionMonitor.CommitConnections(null);
      yield return null;
      elapsedTime += Time.deltaTime;
      connected = tes.Connected && server.ConnectionCount > 0;
    } while (!connected && (elapsedTime < ConnectWaitTime || attempt++ < minConnectAttempts));

    if (!tes.Connected || server.ConnectionCount == 0)
    {
      Debug.LogError("Connection failed.");
      yield break;
    }

    Debug.Log("Connected");

    //--------------------------------------------------------
    // Create scene.
    if (!CreateShapes(server, shapes))
    {
      Debug.LogError("Shape creation failed.");
      yield break;
    }
    server.UpdateTransfers(0);
    server.UpdateFrame(Time.deltaTime);
    Debug.Log("Shapes created.");
    yield return null;

    // Delay a frame to ensure data propagation (in case we have script execution order issues).
    server.UpdateFrame(Time.deltaTime);
    Debug.Log("Delay");
    yield return null;

    //--------------------------------------------------------
    // Validate client scene.
    if (!ValidateScene(++validationCount, shapes))
    {
      Debug.LogError(string.Format("Scene validation {0} failed.", validationCount));
      yield break;
    }
    yield return null;

    //--------------------------------------------------------
    // Serialise client scene.
    string sceneFile1 = Path.GetFullPath(Path.Combine("temp", "test-scene01.3es"));
    string sceneFile2 = Path.GetFullPath(Path.Combine("temp", "test-scene02.3es"));
    // Note: compression is very slow in debug builds.
    tes.SerialiseScene(sceneFile1, true);
    //tes.SerialiseScene(sceneFile1, false);
    Debug.Log("Serialised scene");
    yield return null;

    //--------------------------------------------------------
    // Disconnect.
    tes.Disconnect();
    //server.Destroy();
    server.ConnectionMonitor.Stop();
    server = null;
    Debug.Log("Disconnected");
    yield return null;

    //--------------------------------------------------------
    // Reset and load the scene.
    tes.OpenFile(sceneFile1);
    Debug.Log("Restored scene 1");
    yield return null;

    // Give the stream thread a chance to read.
    elapsedTime = 0;
    do
    {
      yield return null;
      elapsedTime += Time.deltaTime;
    } while (elapsedTime < StreamWaitTime && tes.CurrentFrame == 0);

    //--------------------------------------------------------
    // Validate client scene.
    if (!ValidateScene(++validationCount, shapes))
    {
      Debug.LogError(string.Format("Scene validation {0} failed.", validationCount));
      yield break;
    }
    yield return null;

    //--------------------------------------------------------
    // Serialise again.
    // TODO: push a snapshot message instead and validate against that.
    tes.SerialiseScene(sceneFile2, false);
    Debug.Log("Serialised scene again");
    yield return null;

    //--------------------------------------------------------
    // Reset and load the scene.
    tes.OpenFile(sceneFile2);
    Debug.Log("Restored scene 2");
    yield return null;

    // Give the stream thread a chance to read.
    elapsedTime = 0;
    do
    {
      yield return null;
      elapsedTime += Time.deltaTime;
    } while (elapsedTime < StreamWaitTime && tes.CurrentFrame == 0);

    //--------------------------------------------------------
    // Validate client scene.
    if (!ValidateScene(++validationCount, shapes))
    {
      Debug.LogError(string.Format("Scene validation {0} failed.", validationCount));
      yield break;
    }
    yield return null;

    Debug.Log("Test OK");
  }

  IServer InitialiseServer()
  {
    ServerSettings serverSettings = ServerSettings.Default;
    ServerInfoMessage info = ServerInfoMessage.Default;
    serverSettings.ListenPort = TestPort;
    info.CoordinateFrame = ServerCoordinateFrame;
    //serverSettings.Flags |= ServerFlag.Compress;

    return new TcpServer(serverSettings);
  }

  ConstructorInfo[] GenerateShapeConstructors(Type[] ignoreTypes)
  {
    List<ConstructorInfo> constructors = new List<ConstructorInfo>();
    Type shapeType = typeof(Shape);

    foreach (AssemblyName assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
    {
      Assembly asmembly = Assembly.Load(assemblyName.ToString());
      foreach (Type type in asmembly.GetTypes())
      {
        if (type.IsSubclassOf(shapeType) && (ignoreTypes == null || Array.IndexOf(ignoreTypes, type) < 0))
        {
          // Candidate type. Check for appropriate constructor.
          // We are looking for the ObjectID, CategoryID constructor.
          ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(uint), typeof(ushort) });
          if (constructor != null)
          {
            constructors.Add(constructor);
          }
        }
      }
    }

    return constructors.ToArray();
  }

  bool CreateShapes(IServer server, List<Shape> shapes)
  {
    try
    {
      // Explicit instantiation for:
      // - MeshShape
      // - MeshSet
      // - PointCloudShape
      // - Text2D
      // - Text3D
      // ( Categories)
      Type[] explicitTypes = new Type[]
      {
        typeof(MeshShape),
        typeof(MeshSet),
        typeof(PointCloudShape),
        typeof(Text2D),
        typeof(Text3D)
      };

      ConstructorInfo[] simpleConstructors = GenerateShapeConstructors(explicitTypes);

      Shape shape = null;
      uint objId = 0;
      foreach (ConstructorInfo constructor in simpleConstructors)
      {
        object obj = constructor.Invoke(new object[] { ++objId, (ushort)0 });
        shape = (Shape)obj;// constructor.Invoke(new object[] { ++objId, (ushort)0 });
        shape.Position = new Tes.Maths.Vector3((float)objId);
        shape.Rotation = Tes.Maths.Quaternion.AxisAngle(new Tes.Maths.Vector3(1, 1, 0).Normalised, (objId * 24.0f) / 180.0f * Mathf.PI);
        shape.Scale = new Tes.Maths.Vector3(0.5f, 0.1f, 0.1f);
        shape.Colour = Tes.Maths.Colour.Colours[(int)(objId % Tes.Maths.Colour.Colours.Length)].Value;
        server.Create(shape);
        shapes.Add(shape);
      }

      // Now make explicit instantiations.

      // Tessellate a sphere for mesh tests.
      List<Vector3> verts = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();

      Tes.Tessellate.Sphere.SubdivisionSphere(verts, normals, indices, Vector3.zero, 0.42f, 4);
      shape = CreateMesh(++objId, verts, normals, indices);
      server.Create(shape);
      shapes.Add(shape);

      _sampleMesh = CreateMeshResource(1u, verts, normals, indices);
      shape = CreateMeshSet(++objId, _sampleMesh);
      server.Create(shape);
      shapes.Add(shape);

      shape = CreateCloud(++objId, _sampleMesh);
      server.Create(shape);
      shapes.Add(shape);

      shape = CreateText2D(++objId);
      server.Create(shape);
      shapes.Add(shape);

      shape = CreateText3D(++objId);
      server.Create(shape);
      shapes.Add(shape);
    }
    catch (Exception e)
    {
      Debug.LogException(e);
      return false;
    }

    return true;
  }

  MeshResource CreateMeshResource(uint id, List<Vector3> uverts, List<Vector3> unormals, List<int> indices)
  {
    SimpleMesh mesh = new SimpleMesh(id, MeshDrawType.Triangles,
      MeshComponentFlag.Vertex | MeshComponentFlag.Normal | MeshComponentFlag.Index);

    for (int i = 0; i < uverts.Count; ++i)
    {
      mesh.AddVertex(Tes.Maths.Vector3Ext.FromUnity(uverts[i]));
    }

    for (int i = 0; i < unormals.Count; ++i)
    {
      mesh.AddNormal(Tes.Maths.Vector3Ext.FromUnity(unormals[i]));
    }

    mesh.AddIndices(indices);

    return mesh;
  }

  Shape CreateMesh(uint id, List<Vector3> uverts, List<Vector3> unormals, List<int> indices)
  {
    // Convert to TES vector arrays.
    Tes.Maths.Vector3[] verts = Tes.Maths.Vector3Ext.FromUnity(uverts.ToArray());
    Tes.Maths.Vector3[] normals = Tes.Maths.Vector3Ext.FromUnity(unormals.ToArray());

    MeshShape mesh = new MeshShape(MeshDrawType.Triangles, verts,
        indices.ToArray(), new Tes.Maths.Vector3((float)id));
    mesh.ID = id;
    mesh.Normals = normals;

    return mesh;
  }

  Shape CreateMeshSet(uint id, MeshResource mesh)
  {
    MeshSet meshSet = new MeshSet(id, 0);
    meshSet.AddPart(mesh);
    return meshSet;
  }

  Shape CreateCloud(uint id, MeshResource mesh)
  {
    PointCloudShape cloud = new PointCloudShape(mesh, id);
    cloud.ID = id;
    return cloud;
  }

  Shape CreateText2D(uint id)
  {
    Text2D text = new Text2D(string.Format("Hello {0}", id), id, new Tes.Maths.Vector3(0.1f, 0.1f, 0));
    text.InWorldSpace = false;
    return text;
  }

  Shape CreateText3D(uint id)
  {
    Text3D text = new Text3D(string.Format("Hello {0}", id), id, new Tes.Maths.Vector3(id));
    text.ScreenFacing = true;
    return text;
  }

  bool ValidateScene(int validationCount, List<Shape> shapes)
  {
    bool ok = true;
    ValidationFlag validationFlags = 0;
    foreach (Shape shape in shapes)
    {
      // Find the associated handler.
      MessageHandler handler = FindHandlerFor(shape);
      if (handler != null)
      {
        bool validated = false;
        GameObject obj = FindObjectFor(shape, handler);
        if (obj != null)
        {
          if (!_validationFlags.TryGetValue(shape.GetType(), out validationFlags))
          {
            validationFlags = ValidationFlag.Default;
          }
          validated = ValidateAgainstObject(shape, obj, validationFlags);
        }
        else if (ValidateWithoutObject(shape, handler))
        {
          validated = true;
        }
        else
        {
          Debug.LogError(string.Format("Failed to validate shape {0}", shape.GetType().Name));
        }

        ok = ok && validated;
      }
      else
      {
        ok = false;
        Debug.LogError(string.Format("Failed to find validation handler for shape {0}", shape.GetType().Name));
      }
    }
    // Not implemented.
    return ok;
  }

  MessageHandler FindHandlerFor(Shape shape)
  {
    foreach (var handler in tes.Handlers.Handlers)
    {
      if (handler.RoutingID == shape.RoutingID)
      {
        return handler;
      }
    }

    return null;
  }

  GameObject FindObjectFor(Shape shape, MessageHandler handler)
  {
    Tes.Handlers.ShapeHandler shapeHandler = handler as Tes.Handlers.ShapeHandler;
    if (shapeHandler == null)
    {
      return null;
    }

    GameObject root = shapeHandler.Root;
    // Search for a child object with ShapeComponent.
    var shapeComponent = root.GetComponentInChildren<Tes.Handlers.ShapeComponent>();

    if (shapeComponent == null)
    {
      return null;
    }

    return shapeComponent.gameObject;
  }

  bool ValidateAgainstObject(Shape shape, GameObject obj, ValidationFlag flags)
  {
    // Validate position and rotation. Can't do scale due to varying semantics.
    var shapeComponent = obj.GetComponentInChildren<Tes.Handlers.ShapeComponent>();
    var xform = obj.transform;
    bool ok = true;

    if (shapeComponent.Category != shape.Category)
    {
      Debug.LogError(string.Format("{0} Category mismatch.", obj.name));
      ok = false;
    }
    if (shapeComponent.ObjectFlags != shape.Flags)
    {
      Debug.LogError(string.Format("{0} Flags mismatch", obj.name));
      ok = false;
    }

    // Convert to Unity position.
    if ((flags & ValidationFlag.Position) != 0)
    {
      Vector3 shapePos = Tes.Maths.Vector3Ext.ToUnity(shape.Position);
      if ((flags & ValidationFlag.PositionConverted) != 0)
      {
        shapePos = Tes.Main.Scene.RemoteToUnity(shapePos, ServerCoordinateFrame);
      }

      if (xform.localPosition != shapePos)
      {
        Debug.LogError(string.Format("{0} Position mismatch: {2} != {3}", obj.name, xform.localPosition, shapePos));
        ok = false;
      }
    }
    else if ((flags & ValidationFlag.PositionConverted) != 0)
    {

    }

    if ((flags & ValidationFlag.Rotation) != 0)
    {
      Quaternion shapeRot = Tes.Maths.QuaternionExt.ToUnity(shape.Rotation);
      if ((flags & ValidationFlag.RotationAsNormal) != 0)
      {
        // TODO:
        shapeRot = xform.localRotation;
      }

      if (xform.localRotation != shapeRot)
      {
        Debug.LogError(string.Format("{0} Rotation mismatch: {1} != {2}",
                        xform.localRotation, Tes.Maths.QuaternionExt.ToUnity(shape.Rotation)));
        ok = false;
      }
    }

    if ((flags & ValidationFlag.Colour) != 0)
    {
      Color32 c1 = shapeComponent.Colour;
      Color32 c2 = Tes.Handlers.ShapeComponent.ConvertColour(shape.Colour);
      if (c1.r != c2.r || c1.g != c2.g || c1.b != c2.b || c1.a != c2.a)
      {
        Debug.LogError(string.Format("{0} Colour mismatch: {1} != {2}", obj.name, c1, c2));
        ok = false;
      }
    }

    if (shape.ID != shapeComponent.ObjectID)
    {
      Debug.LogError(string.Format("{0} Shape ID mismatch: {1} != {2}", obj.name, shape.ID, shapeComponent.ObjectID));
      ok = false;
    }

    // Special validation.
    SpecialObjectValidation specialValidation;
    if (_specialObjectValidation.TryGetValue(shape.GetType(), out specialValidation))
    {
      if (!specialValidation(shape, obj))
      {
        ok = false;
      }
    }

    return ok;
  }

  bool ValidateWithoutObject(Shape shape, MessageHandler handler)
  {
    SpecialValidation specialValidation;
    if (_specialValidation.TryGetValue(shape.GetType(), out specialValidation))
    {
      return specialValidation(shape, handler);
    }

    Debug.LogWarning(string.Format("No special validation for {0}", shape.GetType().Name));
    return false;
  }

  static bool ValidateVectors(string context, Vector3[] uverts, Tes.Maths.Vector3[] tverts, float epsilon = 1e-3f)
  {
    bool ok = true;
    int count = uverts.Length;
    if (uverts.Length != tverts.Length)
    {
      count = Math.Min(uverts.Length, tverts.Length);
      ok = false;
      Debug.LogError(string.Format("{0} count mismatch: {1} != {2}", context, uverts.Length, tverts.Length));
    }

    Vector3 tvert;
    Vector3 separation;
    float diffSqr;
    for (int i = 0; i < count; ++i)
    {
      tvert = Tes.Maths.Vector3Ext.ToUnity(tverts[i]);
      separation = uverts[i] - tvert;
      diffSqr = separation.sqrMagnitude;
      if (diffSqr > epsilon * epsilon)
      {
        Debug.LogError(string.Format("{0} mismatch at index {1}: {2} != {3} (difference: {4})",
                       context, i, uverts[i], tvert, Mathf.Sqrt(diffSqr)));
        ok = false;
        break;
      }
    }

    return ok;
  }

  static bool ValidateIndices(string context, int[] uinds, int[] tinds)
  {
    bool ok = true;
    int count = uinds.Length;
    if (uinds.Length != tinds.Length)
    {
      count = Math.Min(uinds.Length, tinds.Length);
      ok = false;
      Debug.LogError(string.Format("{0} count mismatch: {1} != {2}", context, uinds.Length, tinds.Length));
    }

    for (int i = 0; i < count; ++i)
    {
      if (uinds[i] != tinds[i])
      {
        Debug.LogError(string.Format("{0} mismatch at index {1}", context, i));
        ok = false;
        break;
      }
    }

    return ok;
  }

  bool ValidateMesh(Shape shape, GameObject obj)
  {
    MeshHandler.MeshDataComponent meshData = obj.GetComponent<MeshHandler.MeshDataComponent>();
    if (meshData == null)
    {
      Debug.LogError("Missing mesh data object.");
      return false;
    }

    bool ok = true;

    ok = ValidateVectors("Vertex", meshData.Vertices, _sampleMesh.Vertices()) && ok;
    ok = ValidateVectors("Normal", meshData.Normals, _sampleMesh.Normals()) && ok;
    ok = ValidateIndices("Index", meshData.Indices, _sampleMesh.Indices4()) && ok;

    return ok;
  }

  bool ValidateMeshSet(Shape shape, GameObject obj)
  {
    bool ok = true;
    Tes.Handlers.MeshCache meshCache = (Tes.Handlers.MeshCache)tes.GetHandler((ushort)RoutingID.Mesh);

    // First the first *child* ShapeComponent. This identfies the mesh.
    var part = GetFirstChildComponent<Tes.Handlers.ShapeComponent>(obj);
    if (part == null)
    {
      Debug.LogError("Missing mesh set part resource.");
      return false;
    }

    // Find the resource.
    var meshDetails = meshCache.GetEntry(part.ObjectID);
    if (meshDetails == null)
    {
      Debug.LogError(string.Format("Missing mesh {0}", part.ObjectID));
      return false;
    }

    ok = ValidateVectors("Vertex", meshDetails.Builder.Vertices, _sampleMesh.Vertices()) && ok;
    ok = ValidateVectors("Normal", meshDetails.Builder.Normals, _sampleMesh.Normals()) && ok;
    ok = ValidateIndices("Index", meshDetails.Builder.Indices, _sampleMesh.Indices4()) && ok;

    return ok;
  }

  T GetFirstChildComponent<T>(GameObject obj, bool includeInactive = false) where T : MonoBehaviour
  {
    foreach (T comp in obj.GetComponentsInChildren<T>())
    {
      if (comp.gameObject != obj)
      {
        return comp;
      }
    }

    return null;
  }

  bool ValidateCloud(Shape shape, GameObject obj)
  {
    PointsComponent points = obj.GetComponent<PointsComponent>();
    if (points == null)
    {
      Debug.LogError("Missing mesh data object.");
      return false;
    }

    bool ok = true;

    Tes.Handlers.MeshCache meshCache = (Tes.Handlers.MeshCache)tes.GetHandler((ushort)RoutingID.Mesh);

    // Find the resource.
    var meshDetails = meshCache.GetEntry(points.MeshID);
    if (meshDetails == null)
    {
      Debug.LogError(string.Format("Missing mesh {0}", points.MeshID));
      return false;
    }


    ok = ValidateVectors("Point", meshDetails.Builder.Vertices, _sampleMesh.Vertices()) && ok;

    return ok;
  }

  bool ValidateText3D(Shape shape, GameObject obj)
  {
    Text3D text3D = (Text3D)shape;
    TextMesh textMesh = obj.GetComponent<TextMesh>();

    if (textMesh == null)
    {
      Debug.LogError("Missing text mesh.");
      return false;
    }

    bool ok = true;
    if (string.Compare(textMesh.text, text3D.Text) != 0)
    {
      Debug.LogError(string.Format("Text mismatch : {0} != {1}", textMesh.text, text3D.Text));
      ok = false;
    }
    return ok;
  }

  bool ValidateText2D(Shape shape, MessageHandler handler)
  {
    bool ok = true;
    Text2D text2D = (Text2D)shape;
    Text2DHandler textHandler = handler as Text2DHandler;
    if (textHandler == null)
    {
      Debug.LogError(string.Format("Wrong handler for Text2D: {0}", textHandler.Name));
      ok = false;
      // Nothing more to test.
      return false;
    }

    Text2DHandler.Text2DManager textManager = textHandler.PersistentText;
    Text2DHandler.TextEntry textEntry = null;
    foreach (var text in textManager.Entries)
    {
      if (text.ID == text2D.ID)
      {
        textEntry = text;
        break;
      }
    }

    if (textEntry == null)
    {
      Debug.LogError("Failed to find matching text entry.");
      ok = false;
      // Nothing more to test.
      return false;
    }

    if (textEntry.Category != text2D.Category)
    {
      Debug.LogError("Category mismatch.");
      ok = false;
    }

    if (textEntry.ObjectFlags != text2D.Flags)
    {
      Debug.LogError("Flags mismatch.");
      ok = false;
    }

    if (textEntry.Position != Tes.Maths.Vector3Ext.ToUnity(shape.Position))
    {
      Debug.LogError(string.Format("Position mismatch: {0} != {1}",
                      textEntry.Position, Tes.Maths.Vector3Ext.ToUnity(shape.Position)));
      ok = false;
    }

    Color32 c1 = textEntry.Colour;
    Color32 c2 = Tes.Handlers.ShapeComponent.ConvertColour(shape.Colour);
    if (c1.r != c2.r || c1.g != c2.g || c1.b != c2.b || c1.a != c2.a)
    {
      Debug.LogError("Colour mismatch.");
      ok = false;
    }

    if (string.Compare(textEntry.Text, text2D.Text) != 0)
    {
      Debug.LogError(string.Format("Text mismatch : {0} != {1}", textEntry.Text, text2D.Text));
      ok = false;
    }

    return ok;
  }

  private Dictionary<Type, ValidationFlag> _validationFlags = null;

  delegate bool SpecialObjectValidation(Shape shape, GameObject obj);
  private Dictionary<Type, SpecialObjectValidation> _specialObjectValidation = null;

  delegate bool SpecialValidation(Shape shape, MessageHandler handler);
  private Dictionary<Type, SpecialValidation> _specialValidation = null;
}
