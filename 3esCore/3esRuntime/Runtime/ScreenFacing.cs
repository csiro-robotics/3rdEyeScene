using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tes
{
  /// <summary>
  /// A utility object which orients other objects to face the camera. Intended for text objects.
  /// </summary>
  /// <remarks>
  /// The <code>Update()</code> works on many objects rather than attaching this script to one object
  /// for performance reasons. Because of this, only one object with the behaviour is required.
  /// 
  /// The first instance of this object started is registered as the primary instance.
  /// The static calls are routed to this instance until it is destroyed.
  /// </remarks>
  public class ScreenFacing : MonoBehaviour
  {
    private static ScreenFacing _instance = null;

    [SerializeField]
    private bool _addSelf = false;
    [SerializeField]
    private Vector3 _selfForward = Vector3.forward;
    [SerializeField]
    private Vector3 _selfUp = Vector3.up;

    /// <summary>
    /// Routs to <see cref="Add(GameObject, Vector3, Vector3)"/> on the primary instance.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    /// <param name="forward">The vector to face to the camera.</param>
    /// <param name="up">The object's up vector.</param>
    public static void AddToManager(GameObject obj, Vector3 forward, Vector3 up)
    {
      if (_instance != null)
      {
        _instance.Add(obj, forward, up);
      }
    }

    /// <summary>
    /// Routs to <see cref="Remove(GameObject)"/> on the primary instance.
    /// </summary>
    /// <param name="obj">The object to remove.</param>
    public static void RemoveFromManager(GameObject obj)
    {
      if (_instance != null)
      {
        _instance.Remove(obj);
      }
    }

    /// <summary>
    /// Routs to <see cref="Add(GameObject, Vector3, Vector3)"/> on the primary instance.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    /// <param name="forward">The vector to face to the camera.</param>
    /// <param name="up">The object's up vector.</param>
    public void Add(GameObject obj, Vector3 forward, Vector3 up)
    {
      Remove(obj);
      _objects.Add(new ObjectReference
      {
        Reference = new WeakReference(obj),
        Forward = forward,
        Up = up
      });
    }

    /// <summary>
    /// Routs to <see cref="Remove(GameObject)"/> on the primary instance.
    /// </summary>
    /// <param name="obj">The object to remove.</param>
    public void Remove(GameObject obj)
    {
      for (int i = 0; i < _objects.Count; ++i)
      {
        if (_objects[i].Object == obj)
        {
          _objects.RemoveAt(i);
          break;
        }
      }
    }

    /// <summary>
    /// Register as the facing manager instance. Handle <see cref="_addSelf"/>.
    /// </summary>
    void Start()
    {
      if (_instance == null)
      {
        _instance = this;
      }

      if (_addSelf)
      {
        Add(gameObject, _selfForward, _selfUp);
      }
    }

    /// <summary>
    /// Ensure this is no longer the registered instance.
    /// </summary>
    void OnDestroy()
    {
      if (_instance == this)
      {
        _instance = null;
      }
    }

    /// <summary>
    /// Update all registered objects to face the main camera.
    /// </summary>
    void Update()
    {
      if (Camera.main == null)
      {
        return;
      }

      Plane nearClipPlane = new Plane(Camera.main.transform.forward, Camera.main.transform.position + Camera.main.transform.forward * Camera.main.nearClipPlane);
      Quaternion baseRotation = Quaternion.identity, targetRotation = Quaternion.identity;
      ObjectReference objRef;
      GameObject obj;
      Vector3 target, separation;

      for (int i = 0; i < _objects.Count; ++i)
      {
        objRef = _objects[i];
        obj = objRef.Object;
        if (obj != null)
        {
          if (objRef.Up != Vector3.zero)
          {
            baseRotation.SetLookRotation(objRef.Forward, objRef.Up);
          }
          else
          {
            baseRotation.SetLookRotation(objRef.Forward);
          }

          // Project object position onto the near clip plane.
          target = obj.transform.position - nearClipPlane.GetDistanceToPoint(obj.transform.position) * nearClipPlane.normal;
          separation = target - obj.transform.position;
          if (separation.sqrMagnitude > 1e-3)
          {
            targetRotation.SetLookRotation(separation.normalized);
            obj.transform.rotation = targetRotation * baseRotation;
            //if (objRef.Up != Vector3.zero)
            //{
            //  obj.transform.up = objRef.Up;
            //}
          }
        }
        else
        {
          // Dead object. Remove it.
          _objects.RemoveAt(i);
          --i;
        }
      }
    }

    /// <summary>
    /// Structure used to manage registered objects.
    /// </summary>
    private struct ObjectReference
    {
      /// <summary>
      /// Weak reference to the target <c>GameObject</c>
      /// </summary>
      public WeakReference Reference;
      /// <summary>
      /// Target's axis to face to the camera.
      /// </summary>
      public Vector3 Forward;
      /// <summary>
      /// Target's axis to face up the screen.
      /// </summary>
      public Vector3 Up;
      /// <summary>
      /// Helper to resolve <see cref="Reference"/> to a real object.
      /// </summary>
      public GameObject Object { get { return Reference.Target as GameObject; } }
    }
    private List<ObjectReference> _objects = new List<ObjectReference>();
  }
}