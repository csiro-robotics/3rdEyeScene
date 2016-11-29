using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags for MeshShape rendering
  /// </summary>
  [Flags]
  public enum MeshShapeFlag : ushort
  {
    /// <summary>
    /// Calculate normals and rendering with lighting enabled?
    /// </summary>
    CalculateNormals = ObjectFlag.User
  }
}
