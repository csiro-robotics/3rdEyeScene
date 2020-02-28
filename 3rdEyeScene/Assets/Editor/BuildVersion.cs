using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;

namespace Editor
{
  class BuildVersion : IPreprocessBuildWithReport
  {
    public int callbackOrder { get { return 0; } }

    private static string ProductName = "3rd Eye Scene";
    private static string VersionString = "";

    public void OnPreprocessBuild(BuildReport report)
    {
      try
      {
        using (Process proc = new Process())
        {
          proc.StartInfo.UseShellExecute = false;
          proc.StartInfo.CreateNoWindow = false;
          proc.StartInfo.FileName = "git";
          proc.StartInfo.Arguments = "describe --tags";
          proc.StartInfo.RedirectStandardOutput = true;
          proc.StartInfo.RedirectStandardError = true;
          if (proc.Start())
          {
            proc.WaitForExit();
            string version = proc.StandardOutput.ReadLine();
            UnityEngine.Debug.Log($"Setting build version: ${version}");
            // Cache the current name/version to restore later.
            ProductName = PlayerSettings.productName;
            VersionString = PlayerSettings.bundleVersion;
            // Set the version information.
            PlayerSettings.productName = $"3rd Eye Scene {version}";
            PlayerSettings.bundleVersion = version;
          }
        }
      }
      catch (System.Exception e)
      {
        UnityEngine.Debug.LogWarning($"Failed to resolve build version using git: {e.Message}");
      }
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
      // Restore the cached strings.
      PlayerSettings.productName = ProductName;
      PlayerSettings.bundleVersion = VersionString;
    }
  }
}
