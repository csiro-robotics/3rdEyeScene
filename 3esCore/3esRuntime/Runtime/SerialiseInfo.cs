
namespace Tes.Runtime
{
  /// <summary>
  /// Information for <see cref="MessageHandler.Serialise(System.IO.BinaryWriter, ref SerialiseInfo)"/>.
  /// </summary>
  public struct SerialiseInfo
  {
    /// <summary>
    /// Number of transient objects processed.
    /// </summary>
    public uint TransientCount;
    /// <summary>
    /// Number of persistent objects processed.
    /// </summary>
    public uint PersistentCount;
  }

}
