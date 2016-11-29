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
    /// See <code><see cref="System.IO.Compression.GZipStream">GZipStream</see></code>.
    /// </summary>
    GZipCompressed = 1,
  }
}
 