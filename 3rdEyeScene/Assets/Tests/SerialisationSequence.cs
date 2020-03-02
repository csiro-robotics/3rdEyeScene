using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Tes.Handlers.Shape2D;
using Tes.Handlers.Shape3D;
using Tes.Logging;
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
public class SerialisationSequence
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

  public ushort TestPort { get; set; }  =35035;
  public float ConnectWaitTime { get; set; } = 5.0f;
  public float StreamWaitTime { get; set; } = 1.0f;
  public CoordinateFrame ServerCoordinateFrame { get; set; } = CoordinateFrame.XYZ;

  // public TesComponent TesComponent { get { return _tes; } }
  public MeshResource SampleMesh { get { return _sampleMesh; } }

  public SerialisationSequence()
  {
    var tesCandidates = GameObject.FindObjectsOfType<TesComponent>();
    Debug.Assert(tesCandidates != null);
    Debug.Assert(tesCandidates.Length == 1);
    _tes = tesCandidates[0];

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

    _specialShapeValidation.Add(typeof(MeshShape), ValidateMeshShape);
    _specialShapeValidation.Add(typeof(MeshSet), ValidateMeshSet);
    _specialShapeValidation.Add(typeof(PointCloudShape), ValidateCloud);
    _specialShapeValidation.Add(typeof(Text3D), ValidateText3D);

    _specialValidation.Add(typeof(Text2D), ValidateText2D);

    _postCreationFunctions.Add(typeof(Cone), (Shape shape) =>
    {
      Cone cone = (Cone)shape;
      cone.Length = 2.0f;
      cone.Angle = 15.0f / 180.0f * Mathf.PI;
    });
  }

  public bool CanStart()
  {
#if !TRUE_THREADS
    Debug.LogError("3rd Eye Scene must be configured to use TRUE_THREADS in order to support the network communication required by this test.");
    return false;
#else  // !TRUE_THREADS
    return _tes != null;
#endif // !TRUE_THREADS
  }

  public IEnumerator Run()
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
    _tes.Connect(new IPEndPoint(IPAddress.Loopback, TestPort), true);
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
      connected = _tes.Connected && server.ConnectionCount > 0;
    } while (!connected && (elapsedTime < ConnectWaitTime || attempt++ < minConnectAttempts));

    Debug.Assert(_tes.Connected);
    Debug.Assert(server.ConnectionCount > 0);
    Debug.Log("Connected");

    //--------------------------------------------------------
    // Create scene.
    if (!CreateShapes(server, shapes))
    {
      Debug.LogError("Shape creation failed.");
      Debug.Assert(false);
      yield break;
    }

    Debug.Assert(server.UpdateTransfers(0) >= 0);
    Debug.Assert(server.UpdateFrame(Time.deltaTime) >= 0);
    Debug.Log("Shapes created.");
    yield return null;

    // Delay a frame to ensure data propagation (in case we have script execution order issues).
    Debug.Assert(server.UpdateFrame(Time.deltaTime) >= 0);;
    Debug.Log("Delayed");
    yield return null;

    //--------------------------------------------------------
    // Validate client scene.
    ++validationCount;
    if (!ValidateScene(shapes))
    {
      Debug.LogError(string.Format("Scene validation {0} failed.", validationCount));
      Debug.Assert(false);
      yield break;
    }
    yield return null;

    //--------------------------------------------------------
    // Serialise client scene file in the Unity Temp directory.
    string sceneFile1 = Path.GetFullPath(Path.Combine("Temp", "test-scene01.3es"));
    string sceneFile2 = Path.GetFullPath(Path.Combine("Temp", "test-scene02.3es"));
    // Note: compression is very slow in debug builds.
    // TODO: re-enable compression. The GZipStream we use seems to generate incomplete streams on larger streams
    // possibly due to usage issues here.
    // Debug.Assert(_tes.SerialiseScene(sceneFile1, true));
    Debug.Assert(_tes.SerialiseScene(sceneFile1, false));
    Debug.Log("Serialised scene");
    yield return null;

    //--------------------------------------------------------
    // Disconnect.
    _tes.Disconnect();
    // Wait for disconnect.
    yield return new WaitForSeconds(0.5f);

    // server.ConnectionMonitor.MonitorConnections();
    // server.ConnectionMonitor.CommitConnections(null);
    // Debug.Log($"connections: {server.ConnectionCount}");
    // Debug.Assert(server.ConnectionCount == 0);

    //server.Destroy();
    server.ConnectionMonitor.Stop();
    server = null;
    Debug.Log("Disconnected");
    yield return null;

    //--------------------------------------------------------
    // Reset and load the scene.
    Debug.Assert(_tes.OpenFile(sceneFile1));
    Debug.Log("Restored scene 1");
    yield return null;

    // Give the stream thread a chance to read.
    yield return new WaitForSeconds(StreamWaitTime);

    //--------------------------------------------------------
    // Validate client scene.
    ++validationCount;
    if (!ValidateScene(shapes))
    {
      Debug.LogError(string.Format("Scene validation {0} failed.", validationCount));
      Debug.Assert(false);
      yield break;
    }
    yield return null;

    //--------------------------------------------------------
    // Serialise again.
    // TODO: push a keyframe message instead and validate against that.
    Debug.Assert(_tes.SerialiseScene(sceneFile2, false));
    Debug.Log("Serialised scene again");
    yield return null;

    //--------------------------------------------------------
    // Reset and load the scene.
    Debug.Assert(_tes.OpenFile(sceneFile2));
    Debug.Log("Restored scene 2");
    yield return null;

    // Give the stream thread a chance to read.
    yield return new WaitForSeconds(StreamWaitTime);

    //--------------------------------------------------------
    // Validate client scene.
    ++validationCount;
    if (!ValidateScene(shapes))
    {
      Debug.LogError(string.Format("Scene validation {0} failed.", validationCount));
      Debug.Assert(false);
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
      ShapeDelegate postCreate = null;
      uint objId = 0;
      foreach (ConstructorInfo constructor in simpleConstructors)
      {
        object obj = constructor.Invoke(new object[] { ++objId, (ushort)0 });
        shape = (Shape)obj;// constructor.Invoke(new object[] { ++objId, (ushort)0 });
        shape.Position = new Tes.Maths.Vector3((float)objId);
        shape.Rotation = Tes.Maths.Quaternion.AxisAngle(new Tes.Maths.Vector3(1, 1, 0).Normalised, (objId * 24.0f) / 180.0f * Mathf.PI);
        shape.Scale = new Tes.Maths.Vector3(0.5f, 0.1f, 0.1f);
        shape.Colour = Tes.Maths.Colour.Cycle(objId).Value;

        if (_postCreationFunctions.TryGetValue(shape.GetType(), out postCreate))
        {
          postCreate(shape);
        }

        server.Create(shape);
        shapes.Add(shape);
      }

      // Now make explicit instantiations.

      // Tessellate a sphere for mesh tests.
      List<Vector3> verts = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();

      Tes.Tessellate.Sphere.SubdivisionSphere(verts, normals, indices, Vector3.zero, 0.42f, 5);
      shape = CreateMesh(++objId, verts, normals, indices);
      shape.Position = new Tes.Maths.Vector3((float)objId);
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
    mesh.Position = new Tes.Maths.Vector3((float)id);

    return mesh;
  }

  Shape CreateMeshSet(uint id, MeshResource mesh)
  {
    MeshSet meshSet = new MeshSet(id, 0);
    meshSet.AddPart(mesh);
    meshSet.Position = new Tes.Maths.Vector3((float)id);
    return meshSet;
  }

  Shape CreateCloud(uint id, MeshResource mesh)
  {
    PointCloudShape cloud = new PointCloudShape(mesh, id);
    cloud.ID = id;
    cloud.Position = new Tes.Maths.Vector3((float)id);
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

  /// <summary>
  /// Validate that the given list of <paramref name="shapes"/> is correctly represented in the Unity scene.
  /// </summary>
  /// <param name="shapes">The list of shapes to validate.</param>
  /// <returns>True on success, false if there are inconsistencies between <paramref name="shapes"/> and the scene.
  /// </returns>
  bool ValidateScene(List<Shape> shapes)
  {
    bool ok = true;
    ValidationFlag validationFlags = 0;
    foreach (Shape referenceShape in shapes)
    {
      // Find the associated handler.
      MessageHandler handler = FindHandlerFor(referenceShape);
      if (handler != null)
      {
        bool validated = false;
        Shape sceneShape = ExtractSceneRepresentation(referenceShape, handler);
        if (sceneShape != null)
        {
          if (!_validationFlags.TryGetValue(referenceShape.GetType(), out validationFlags))
          {
            validationFlags = ValidationFlag.Default;
          }
          validated = ValidateShape(sceneShape, referenceShape, validationFlags, handler);
        }
        else if (ValidateWithoutReference(referenceShape, handler))
        {
          validated = true;
        }
        else
        {
          Debug.LogError(string.Format("Failed to validate shape {0}", referenceShape.GetType().Name));
        }

        ok = ok && validated;
      }
      else
      {
        ok = false;
        Debug.LogError(string.Format("Failed to find validation handler for shape {0}", referenceShape.GetType().Name));
      }
    }
    // Not implemented.
    return ok;
  }

  MessageHandler FindHandlerFor(Shape shape)
  {
    foreach (var handler in _tes.Handlers.Handlers)
    {
      if (handler.RoutingID == shape.RoutingID)
      {
        return handler;
      }
    }

    return null;
  }

  /// <summary>
  /// Extract the representation of <paramref name="shape"/> in the current Unity scene.
  /// </summary>
  /// <param name="shape">The reference shape.</param>
  /// <param name="handler">The handler which should contain the scene representation.</param>
  /// <returns></returns>
  Shape ExtractSceneRepresentation(Shape shape, MessageHandler handler)
  {
    Tes.Handlers.ShapeHandler shapeHandler = handler as Tes.Handlers.ShapeHandler;
    if (shapeHandler == null)
    {
      return null;
    }

    // Extract a serialised version of the shape from the handler.
    return shapeHandler.CreateSerialisationShapeFor(shape.ID);
  }

  bool ValidateShape(Shape shape, Shape referenceShape, ValidationFlag flags, MessageHandler handler)
  {
    bool ok = true;
    // Validate core data.
    if (shape.ID != referenceShape.ID)
    {
      Debug.LogError($"{handler.Name} {shape.ID} does not match reference ID {referenceShape.ID}.");
      ok = false;
    }

    if (shape.Category != referenceShape.Category)
    {
      Debug.LogError($"{handler.Name} {shape.ID} category mismatch. Category {shape.Category} expect {referenceShape.ID}.");
      ok = false;
    }

    if (shape.Flags != shape.Flags)
    {
      Debug.LogError($"{handler.Name} {shape.ID} flags mismatch. Flags {shape.Flags} expect ${referenceShape.Flags}");
      ok = false;
    }

    // Validate position.
    if ((flags & ValidationFlag.Position) != 0)
    {
      if (shape.Position != referenceShape.Position)
      {
        Debug.LogError($"{handler.Name} {shape.ID} position mismatch. Position {shape.Position} expect {referenceShape.Position}");
        ok = false;
      }
    }

    if ((flags & ValidationFlag.Rotation) != 0)
    {
      if (shape.Rotation != referenceShape.Rotation)
      {
        Debug.LogError($"{handler.Name} {shape.ID} rotation mismatch. Rotation {shape.Rotation} expect {referenceShape.Rotation}");
        ok = false;
      }
    }

    if ((flags & ValidationFlag.Scale) != 0)
    {
      if (shape.Scale != referenceShape.Scale)
      {
        Debug.LogError($"{handler.Name} {shape.ID} scale mismatch. Scale {shape.Scale} expect {referenceShape.Scale}");
        ok = false;
      }
    }

    if ((flags & ValidationFlag.Colour) != 0)
    {
      if (shape.Colour != referenceShape.Colour)
      {
        Debug.LogError($"{handler.Name} {shape.ID} colour mismatch. Colour {shape.Colour} expect {referenceShape.Colour}");
        ok = false;
      }
    }

    // Special validation.
    SpecialShapeValidation specialValidation;
    if (_specialShapeValidation.TryGetValue(shape.GetType(), out specialValidation))
    {
      if (!specialValidation(shape, referenceShape, handler))
      {
        ok = false;
      }
    }

    return ok;
  }

  bool ValidateWithoutReference(Shape shape, MessageHandler handler)
  {
    SpecialValidation specialValidation;
    if (_specialValidation.TryGetValue(shape.GetType(), out specialValidation))
    {
      return specialValidation(shape, handler);
    }

    Log.Warning("No special validation for {0}", shape.GetType().Name);
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

  bool ValidateMeshShape(Shape shape, Shape referenceShape, MessageHandler handler)
  {
    MeshHandler meshHandler = (MeshHandler)handler;
    MeshHandler.MeshEntry meshEntry = meshHandler.ShapeCache.GetShapeData<MeshHandler.MeshEntry>(shape.ID);
    MeshShape meshShapeReference = (MeshShape)referenceShape;
    bool ok = true;

    ok = ValidateVectors("Vertex", meshEntry.Mesh.Vertices, meshShapeReference.Vertices) && ok;
    if (meshEntry.Mesh.HasNormals)
    {
      Vector3[] normals = meshEntry.Mesh.Normals;
      if (meshShapeReference.Normals.Length == 1)
      {
        // Single uniform normal will have been expanded. Extract just the first normal.
        normals = new Vector3[] { meshEntry.Mesh.Normals[0] };
      }
      ok = ValidateVectors("Normal", normals, meshShapeReference.Normals) && ok;
    }
    else
    {
      if (meshShapeReference.Normals != null && meshShapeReference.Normals.Length > 0)
      {
        Debug.LogError("Missing normals.");
        ok = false;
      }
    }
    if (meshEntry.Mesh.IndexCount >0)
    {
      ok = ValidateIndices("Index", meshEntry.Mesh.Indices, meshShapeReference.Indices) && ok;
    }
    else
    {
      if (meshShapeReference.Indices != null && meshShapeReference.Indices.Length > 0)
      {
        Debug.LogError("Missing indices.");
        ok = false;
      }
    }

    return ok;
  }

  bool ValidateMeshSet(Shape shape, Shape referenceShape, MessageHandler handler)
  {
    MeshSetHandler meshSetHandler = (MeshSetHandler)handler;
    MeshSetHandler.PartSet parts = meshSetHandler.ShapeCache.GetShapeData<MeshSetHandler.PartSet>(shape.ID);
    Tes.Handlers.MeshCache meshCache = (Tes.Handlers.MeshCache)_tes.GetHandler((ushort)RoutingID.Mesh);
    MeshSet meshSetReference = (MeshSet)referenceShape;
    bool ok = true;

    // Validate the number of parts.
    if (parts.MeshIDs.Length != meshSetReference.PartCount)
    {
      Debug.LogError("Part count mismatch.");
      return false;
    }

    // Validate each part.
    for (int i = 0; i < meshSetReference.PartCount; ++i)
    {
      MeshResource referenceMesh = meshSetReference.PartResource(i);
      if (parts.MeshIDs[i] != referenceMesh.ID)
      {
        Debug.LogError($"Part resource mismatch. {parts.MeshIDs[i]} != {meshSetReference.PartResource(i).ID}");
        ok = false;
        continue;
      }

      // Resolve the mesh resource from the cache.
      Tes.Handlers.MeshCache.MeshDetails meshDetails = meshCache.GetEntry(parts.MeshIDs[i]);

      if (meshDetails == null)
      {
        Debug.LogError($"Unable to resolve mesh resource {parts.MeshIDs[i]}");
        ok = false;
        continue;
      }

      // Validate mesh content.
      ok = ValidateVectors("Vertex", meshDetails.Mesh.Vertices, referenceMesh.Vertices()) && ok;
      if (meshDetails.Mesh.HasNormals)
      {
        Vector3[] normals = meshDetails.Mesh.Normals;
        if (referenceMesh.Normals().Length == 1)
        {
          // Single uniform normal will have been expanded. Extract just the first normal.
          normals = new Vector3[] { meshDetails.Mesh.Normals[0] };
        }
        ok = ValidateVectors("Normal", normals, referenceMesh.Normals()) && ok;
      }
      else
      {
        if (referenceMesh.Normals() != null && referenceMesh.Normals().Length > 0)
        {
          Debug.LogError("Missing normals.");
          ok = false;
        }
      }
      if (meshDetails.Mesh.IndexCount > 0)
      {
        ok = ValidateIndices("Index", meshDetails.Mesh.Indices, referenceMesh.Indices4()) && ok;
      }
      else
      {
        if (referenceMesh.Indices4() != null && referenceMesh.Indices4().Length > 0)
        {
          Debug.LogError("Missing indices.");
          ok = false;
        }
      }
    }

    return ok;
  }

  bool ValidateCloud(Shape shape, Shape referenceShape, MessageHandler handler)
  {
    PointCloudHandler cloudHandler = (PointCloudHandler)handler;
    PointsComponent pointsData = cloudHandler.ShapeCache.GetShapeData<PointsComponent>(shape.ID);
    PointCloudShape cloudReference = (PointCloudShape)referenceShape;

    if (pointsData == null)
    {
      Debug.LogError("Unable to resolve point cloud data.");
      return false;
    }

    bool ok = true;

    // Only validate vertices.
    ok = ValidateVectors("Point", pointsData.Mesh.Mesh.Vertices, cloudReference.PointCloud.Vertices()) && ok;

    return ok;
  }

  bool ValidateText3D(Shape shape, Shape referenceShape, MessageHandler handler)
  {
    Text3D text3D = (Text3D)shape;
    Text3D text3DReference = (Text3D)referenceShape;

    bool ok = true;
    if (string.Compare(text3D.Text, text3DReference.Text) != 0)
    {
      Debug.LogError($"Text mismatch : {text3D.Text} != {text3DReference.Text}");
      ok = false;
    }
    return ok;
  }

  bool ValidateText2D(Shape shape, MessageHandler handler)
  {
    Text2DHandler textHandler = (Text2DHandler)handler;
    Text2D text2DReference = (Text2D)textHandler.CreateSerialisationShapeFor(shape.ID);
    Text2D text2D = (Text2D)shape;

    bool ok = true;
    if (string.Compare(text2D.Text, text2DReference.Text) != 0)
    {
      Debug.LogError($"Text mismatch : {text2D.Text} != {text2DReference.Text}");
      ok = false;
    }
    return ok;
  }

  private TesComponent _tes = null;
  private MeshResource _sampleMesh = null;
  private Dictionary<Type, ValidationFlag> _validationFlags = new Dictionary<Type, ValidationFlag>();

  delegate bool SpecialShapeValidation(Shape shape, Shape referenceShape, MessageHandler handler);
  private Dictionary<Type, SpecialShapeValidation> _specialShapeValidation =
    new Dictionary<Type, SpecialShapeValidation>();

  delegate bool SpecialValidation(Shape shape, MessageHandler handler);
  private Dictionary<Type, SpecialValidation> _specialValidation = new Dictionary<Type, SpecialValidation>();

  delegate void ShapeDelegate(Shape shape);
  private Dictionary<Type, ShapeDelegate> _postCreationFunctions = new Dictionary<Type, ShapeDelegate>();
}
