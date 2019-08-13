using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages startup based on command line arguments, loading either the main scene or test scene(s).
/// </summary>
public class Startup : MonoBehaviour
{
  void Start()
  {
    Options opt = Options.Current;

    bool quit = opt.ParseFailure;
    switch (opt.Mode)
    {
      default:
      case Options.RunMode.Normal:
        break;
      case Options.RunMode.Version:
        Debug.LogWarning($"version: {Application.version}");
        Console.WriteLine($"version: {Application.version}");
        quit = true;
        break;
      case Options.RunMode.Help:
        Options.ShowUsage();
        quit = true;
        break;
      case Options.RunMode.Test:
        if (!quit)
        {
          if (opt.Values.ContainsKey("test"))
          {
            string testScene = opt.Values["test"];
            if (string.IsNullOrEmpty(testScene))
            {
              Debug.LogError("No test scene specified");
              Application.Quit();
            }
            SceneManager.LoadScene(string.Format("Test/{0}", testScene));
          }
          else
          {
            Debug.LogWarning("No test scene specified");
          }
        }
        break;
    }

    if (quit)
    {
      Application.Quit();
    }

// #if UNITY_STANDALONE
//     Application.productName = $"{Application.productName} {Application.version}";
// #endif // UNITY_STANDALONE
  }
}
