using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Tes.Runtime;

namespace Editor
{ 
  /// <summary>
  /// Prints a warning message when building a player against the debug versions of 3esRuntime libraries.
  /// </summary>
  public static class DebugValidator
  {
    [PostProcessBuild()]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuildTarget)
    {
      if (Info.IsDebug)
      {
        Debug.LogWarning("Warning: player has been built against the Debug versions of the 3esRuntime libraries.");
      }
    }
  }
}
