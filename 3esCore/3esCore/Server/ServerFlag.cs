using System;

namespace Tes.Server
{
  /// <summary>
  /// Defines server option flags.
  /// </summary>
  [Flags, Serializable]
  public enum ServerFlag
  {
    /// <summary>
    /// When set, the control message marking the end of a frame is always sent naked. That is, it
    /// is uncompressed and not collated with other packets.
    /// </summary>
    NakedFrameMessage = (1 << 0),
    ///<summary>
    /// Set to collate outgoing messages into larger packets.
    /// </summary>
    Collate = (1 << 1),
    /// <summary>
    /// Set to compress collated outgoing packets using GZip compression.
    /// Has no effect if <code>Collate</code> is not set.
    /// </summary>
    Compress = (1 << 2),
  }
}
