using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Editor
{
  class BuildVersion : IPreprocessBuildWithReport
  {
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
      try
      {
        using (Process proc = new Process())
        {
          proc.StartInfo.UseShellExecute = false;
          proc.StartInfo.CreateNoWindow = false;
          proc.StartInfo.FileName = "git";
          proc.StartInfo.Arguments = "describe";
          proc.StartInfo.RedirectStandardOutput = true;
          proc.StartInfo.RedirectStandardError = true;
          if (proc.Start())
          {
            proc.WaitForExit();
            string version = proc.StandardOutput.ReadLine();
            UnityEngine.Debug.Log($"Setting build version: ${version}");
            PlayerSettings.bundleVersion = version;
          }
        }
      }
      catch (System.Exception e)
      {
        UnityEngine.Debug.LogWarning($"Failed to resolve build version using git: {e.Message}");
      }
    }
  }
}
