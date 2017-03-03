using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages startup based on command line arguments, loading either the main scene or test scene(s).
/// </summary>
public class Startup : MonoBehaviour
{
  void Start()
  {
    Options opt = new Options();
    if (opt.Values.ContainsKey("test"))
    {
      string testScene = opt.Values["test"];
      if (string.IsNullOrEmpty(testScene))
      {
        Debug.LogError("No test scene specified");
        Application.Quit();
      }
      SceneManager.LoadScene(string.Format("Test/{0}", testScene));
      return;
    }
  }
}
