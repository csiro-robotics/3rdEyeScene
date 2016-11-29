using UnityEngine;

/// <summary>
/// Exposes the physics object to the 3rd Eye Scene view.
/// </summary>
public class TesPhysicsView : MonoBehaviour, TesView
{
  private static uint _nextId = 1;
  public static uint NextId()
  {
    uint id = _nextId++;
    if (_nextId == 0) ++_nextId;
    return id;
  }

  private static uint _nextMeshId = 1;
  public static uint NextMeshId()
  {
    uint id = _nextMeshId++;
    if (_nextMeshId == 0) ++_nextMeshId;
    return id;
  }

  public bool UpdatePosition { get; protected set; }
  public bool UpdateRotation { get; protected set; }
  public bool UpdateScale { get; protected set; }

  public Tes.Shapes.Shape Shape { get; protected set; }
  public GameObject GameObject { get { return gameObject; } }

  public bool Dynamic { get { return _dynamic; } protected set { _dynamic = value; } }

  /// <summary>
  /// When true, do not use the <see cref="TesServer.LookupColour(GameObject, Collider)"/> method.
  /// </summary>
  public bool ExplicitColour { get; set; }

  public static Vector3 FromTes(Tes.Maths.Vector3 v)
  {
    return new Vector3(v.X, v.Y, v.Z);
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

  public static Tes.Maths.Colour ToTes(Color32 c)
  {
    return new Tes.Maths.Colour(c.r, c.g, c.b, c.a);
  }

  public Vector3 PositionOffset { get; set; }

  public Vector3 ShapeScale
  {
    get
    {
      Vector3 scale = transform.lossyScale;
      BoxCollider box = null;
      CapsuleCollider capsule = null;
      SphereCollider sphere = null;
      Collider collider = GetComponent<Collider>();
      if ((box = collider as BoxCollider) != null)
      {
        scale.x *= box.size.x;
        scale.y *= box.size.y;
        scale.z *= box.size.z;
      }
      else if ((capsule = collider as CapsuleCollider) != null)
      {
        scale.y *= capsule.height;
        scale.x *= capsule.radius;
        scale.z *= capsule.radius;
      }
      else if ((sphere = collider as SphereCollider) != null)
      {
        UpdateRotation = false;
        UpdateScale = false;
        scale.x *= sphere.radius;
        scale.y *= sphere.radius;
        scale.z *= sphere.radius;
      }

      return scale;
    }
  }

  void Start()
  {
    Dynamic = false;
    if (Shape == null)
    {
      // Resolve the appropriate shape.
      Collider collider = GetComponent<Collider>();
      UpdatePosition = UpdateRotation = UpdateScale = false;
      if (collider != null)
      {
        BoxCollider box = null;
        CapsuleCollider capsule = null;
        MeshCollider mesh = null;
        SphereCollider sphere = null;
        TerrainCollider terrain = null;
        WheelCollider wheel = null;

        // Note: colour is set using LookupColour() in TesServer.Add()

        // FIXME: isStatic doesn't do well to highlight static collision geometry.
        // Mark everything as dynamic for now.
        //if (!collider.gameObject.isStatic)
        //{
          UpdatePosition = UpdateRotation = UpdateScale = true;
        //}

        Tes.Maths.Vector3 scale = ToTes(transform.lossyScale);
        Tes.Maths.Vector3 pos = ToTes(transform.position);
        if ((box = collider as BoxCollider) != null)
        {
          pos += ToTes(PositionOffset);
          scale.X *= box.size.x;
          scale.Y *= box.size.y;
          scale.Z *= box.size.z;
          Shape = new Tes.Shapes.Box(NextId(), pos, scale, ToTes(transform.rotation));
        }
        else if ((capsule = collider as CapsuleCollider) != null)
        {
          UpdateScale = false;
          Shape = new Tes.Shapes.Capsule(NextId(), ToTes(transform.position), Tes.Maths.Vector3.AxisY, scale.Y * capsule.height, scale.X * capsule.radius);
          _shapeRotation = FromTes(Shape.Rotation);
          Shape.Rotation = ToTes(transform.rotation * _shapeRotation);
        }
        else if ((mesh = collider as MeshCollider) != null)
        {
          var meshShape = new Tes.Shapes.MeshSet(NextId());
          Shape = meshShape;
          meshShape.AddPart(new TesMeshWrapper(mesh.sharedMesh));
          meshShape.Position = ToTes(transform.position);
          meshShape.Rotation = ToTes(transform.rotation);
          meshShape.Scale = scale;
        }
        else if ((sphere = collider as SphereCollider) != null)
        {
          UpdateRotation = false;
          UpdateScale = false;
          Shape = new Tes.Shapes.Sphere(NextId(), ToTes(transform.localPosition), transform.lossyScale.x * sphere.radius);
        }
        else if ((terrain = collider as TerrainCollider) != null)
        {
          Debug.LogWarning(string.Format("TerrainCollider not yet supported ({0}).", gameObject.name));
        }
        else if ((wheel = collider as WheelCollider) != null)
        {
          Debug.LogWarning(string.Format("WheelCollider not yet supported ({0}).", gameObject.name));
        }
        else
        {
          Debug.LogError(string.Format("Unsupported collider type for object {0}: {1}.", gameObject.name, collider.GetType().Name));
        }
      }
    }

    Dynamic = UpdatePosition || UpdateRotation || UpdateScale;
  }

  void OnEnable()
  {
    if (TesServer.Instance)
    {
      TesServer.Instance.Add(this);
    }
  }

  void OnDisable()
  {
    RemoveView();
  }

  void OnDestroy()
  {
    RemoveView();
  }

  void RemoveView()
  {
    if (TesServer.Instance)
    {
      TesServer.Instance.Remove(this);
    }
  }

  public bool UpdateView()
  {
    if (Shape == null)
    {
      return false;
    }

    bool requireUpdate = false;
    Tes.Maths.Vector3 tv3;
    Tes.Maths.Quaternion tq;
    if (UpdatePosition)
    {
      tv3 = ToTes(transform.position + PositionOffset);
      requireUpdate = requireUpdate || Shape.Position != tv3;
      Shape.Position = tv3;
    }

    if (UpdateRotation)
    {
      tq = ToTes(transform.rotation * _shapeRotation);
      requireUpdate = requireUpdate || Shape.Rotation != tq;
      Shape.Rotation = tq;
    }

    if (UpdateScale)
    {
      tv3 = ToTes(ShapeScale);
      requireUpdate = requireUpdate || Shape.Scale != tv3;
      Shape.Scale = tv3;
    }

    return requireUpdate;
  }

  /// <summary>
  /// Additional rotation for the shape. E.g., orient capsules.
  /// </summary>
  private Quaternion _shapeRotation = Quaternion.identity;
  /// <summary>
  /// Underpins <see cref="Dynamic"/>
  /// </summary>
  [SerializeField]
  private bool _dynamic = true;
}
