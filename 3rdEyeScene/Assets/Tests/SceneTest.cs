using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
  public class SceneTest
  {
    [UnityTest]
    public IEnumerator Serialisation()
    {
      string sceneName = "3rdEyeScene";
      SceneManager.LoadScene(sceneName);
      // Allow the load command to be processed.
      yield return null;
      Debug.Assert(SceneManager.GetActiveScene().name == sceneName);

      SerialisationSequence sequence = new SerialisationSequence();
      Debug.Assert(sequence.CanStart());
      IEnumerator iter = sequence.Run();
      while (iter.MoveNext())
      {
        yield return iter.Current;
      }
      // yield return new WaitForSeconds(5.0f);
    }
  }
}
