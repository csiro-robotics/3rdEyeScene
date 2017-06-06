using System;

namespace Tes.Net
{
  /// <summary>
  /// Flags for a <see cref="CollatedPacketMessage"/>
  /// </summary>
  [Flags]
  public enum CollatedPacketFlag
  {
    /// <summary>
    /// Data are compressed using GZIP compression.
    /// </summary>
    GZipCompressed = 1,
  }
}
 