using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags used with <see cref="MeshFinaliseMessage"/>.
  /// </summary>
  [Flags]
  public enum MeshBuildFlags
  {
    /// <summary>
    /// Zero value.
    /// </summary>
    Zero = 0,
    /// <summary>
    /// Calculate per vertex normals based on triangle normals.
    /// </summary>
    CalculateNormals = (1 << 0)
  }
}
