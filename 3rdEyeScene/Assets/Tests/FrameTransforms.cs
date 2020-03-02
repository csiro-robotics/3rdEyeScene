using System;
using NUnit.Framework;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class FrameTransforms
  {
    private static Vector3[] TestVectors = new Vector3[]
    {
      new Vector3(1, 0, 0),
      new Vector3(0, 2, 0),
      new Vector3(0, 0, 3),
      new Vector3(-1, 0, 0),
      new Vector3(0, -2, 0),
      new Vector3(0, 0, -3),
      new Vector3(1, 2, 3)
    };

  //  public void Transform()
  //  {
  //    GameObject toUnityObj = new GameObject();
  //    GameObject fromUnityObj = new GameObject();
  //
  //    foreach (CoordinateFrame frame in Enum.GetValues(typeof(CoordinateFrame)))
  //    {
  //      FrameTransform.SetFrameRotation(toUnityObj.transform, frame);
  //    }
  //    Debug.Log("ok");
  //  }

    [Test]
    public void FrameConsistency()
    {
      Matrix4x4 toUnity = new Matrix4x4();
      string message;
      bool shouldChangeHands;
      bool changedHands;
      bool ok = true;

      foreach (CoordinateFrame frame in Enum.GetValues(typeof(CoordinateFrame)))
      {
        //string FrameName = frame.ToString();
        FrameTransform.SetFrameRotation(ref toUnity, frame);
        shouldChangeHands = frame < CoordinateFrame.LeftHanded;
        changedHands = FrameTransform.EffectsHandChange(toUnity);
        ok = ok && shouldChangeHands == changedHands;

        message = string.Format("{0} R? {1} : Ok? {2}", frame.ToString(), shouldChangeHands, changedHands);
        if (shouldChangeHands == changedHands)
        {
          Debug.Log(message);
        }
        else
        {
          Debug.LogError(message);
        }
      }

      if (!ok)
      {
        throw new Exception("failed");
      }
    }

    [Test]
    public void Conversion()
    {
      GameObject parent = new GameObject("Parent");
      GameObject child = new GameObject("Child");
      Matrix4x4 toUnity = new Matrix4x4();
      Matrix4x4 toRemote = new Matrix4x4();
      Vector3 v, v1, v2;

      bool stepOk = true;
      bool ok = true;

      child.transform.SetParent(parent.transform);

      foreach (CoordinateFrame frame in Enum.GetValues(typeof(CoordinateFrame)))
      {
        //string FrameName = frame.ToString();
        FrameTransform.SetFrameRotation(ref toUnity, frame);
        FrameTransform.SetFrameRotationInverse(ref toRemote, frame);
        FrameTransform.SetFrameRotation(parent.transform, frame);

        for (int i = 0; i < TestVectors.Length; ++i)
        {
          v = TestVectors[i];
          child.transform.localPosition = v;
          v1 = toUnity.MultiplyPoint(v);
          v2 = toRemote.MultiplyPoint(v1);

          stepOk = ApproxEqual(v, v2);
          ok = ok && stepOk;

          if (!stepOk)
          {
            Debug.LogError(string.Format("Transformation {0} failed: {1} -> {2} -> {3}", frame, v, v1, v2));
          }

          v2 = child.transform.position;
          stepOk = ApproxEqual(v1, v2);
          ok = ok && stepOk;

          if (!stepOk)
          {
            Debug.LogError(string.Format("Transform/Matrix {0} disagreement: {1} : {2} <> {3}", frame, v, v2, v1));
          }
        }
      }

      GameObject.Destroy(parent);
      GameObject.Destroy(child);
      if (!ok)
      {
        throw new Exception("failed");
      }
    }

    private static bool ApproxEqual(Vector3 v0, Vector3 v1, float epsilon = 1e-3f)
    {
      return Mathf.Abs(v0.x - v1.x) <= epsilon
          && Mathf.Abs(v0.y - v1.y) <= epsilon
          && Mathf.Abs(v0.z - v1.z) <= epsilon;
    }
  }
}
