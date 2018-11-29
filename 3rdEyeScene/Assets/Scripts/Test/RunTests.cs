using System;
using UnityEngine;

public class RunTests : MonoBehaviour
{
  public delegate void TestMethod();
  
  void Start()
  {
    // TODO: use reflection.
    FrameTransforms frameTest = new FrameTransforms();

    TestMethod[] tests = new TestMethod[]
    {
      frameTest.FrameConsistency,
      frameTest.Conversion
    };

    int executed = 0;
    int succeeded = 0;
    int failed = 0;
    foreach (TestMethod test in tests)
    {
      try
      {
        ++executed;
        test();
        ++succeeded;
      }
      catch (Exception e)
      {
        Debug.LogException(e);
        ++failed;
      }
    }

    Debug.Log(string.Format("Testing complete -- {0}", (failed > 0) ? "FAILED" : "SUCCEEDED"));
    Debug.Log(string.Format("Executed: {0}", executed));
    Debug.Log(string.Format("Succeded: {0}", succeeded));
    Debug.Log(string.Format("Failed  : {0}", failed));
  }
}
