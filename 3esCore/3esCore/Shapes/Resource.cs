using Tes.IO;

namespace Tes
{
  /// <summary>
  /// Progress tracking for <see cref="Resource.Transfer"/>
  /// </summary>
  public struct TransferProgress
  {
    /// <summary>
    /// Progress marker.
    /// </summary>
    public long Progress;
    /// <summary>
    /// Phase marker (additional progress tracking).
    /// </summary>
    public int Phase;
    /// <summary>
    /// Completion flag. Stop when true.
    /// </summary>
    public bool Complete;
    /// <summary>
    /// Error flag. Halt when true.
    /// </summary>
    public bool Failed;

    /// <summary>
    /// Reset progress state.
    /// </summary>
    public void Reset()
    {
      Progress = 0;
      Phase = 0;
      Complete = Failed = false;
    }
  }

  /// <summary>
  /// An interface for tracking shared resources.
  /// </summary>
  /// <remarks>
  /// A shared resource represents data referenced by multiple other objects or shapes.
  /// Mesh data and materials are good examples of shared resources. Shared resources are
  /// transferred on the connection as required, and the destruction message sent some time
  /// after the resource is no longer referenced.
  /// 
  /// Typically calling pattern on the resource is:
  /// <list type="bullet">
  /// <item><see cref="Create(PacketBuffer)"/></item>
  /// <item><see cref="Transfer(PacketBuffer, int, ref TransferProgress)"/> until it returns zero (or an error).</item>
  /// <item><see cref="Destroy(PacketBuffer)"/> when no longer required.</item>
  /// </list>
  /// </remarks>
  public interface Resource
  {
    /// <summary>
    /// Unique ID for the shared resource. Only unique amongst resources with the same
    /// <see cref="TypeID"/>.
    /// </summary>
    uint ID { get; }

    /// <summary>
    /// Type ID of the resource. This matches the routing ID for the handler.
    /// </summary>
    ushort TypeID { get; }

    /// <summary>
    /// Send initial creation message for this resource.
    /// </summary>
    /// <param name="packet">Packet to populate with the create message.</param>
    /// <returns>Zero on success.</returns>
    int Create(PacketBuffer packet);

    /// <summary>
    /// Send destruction message for this resource.
    /// </summary>
    /// <param name="packet">Packet to populate with the destruction message.</param>
    /// <returns>Zero on success.</returns>
    int Destroy(PacketBuffer packet);

    /// <summary>
    /// Update transfer of the shared resource.
    /// </summary>
    /// <param name="packet">The packet buffer in which to compose the transfer message.</param>
    /// <param name="byteLimit">An advisory byte limit used to restrict how much data should be sent (in bytes).</param>
    /// <param name="progress">Track the transfer progress between calls.</param>
    /// <remarks>
    /// Supports amortised transfer via the <paramref name="progress"/> argument.
    /// On first call, this is the default initialised structure (zero). On subsequent
    /// calls it is the last returned value unless <c>Failed</c> was true.
    /// 
    /// The semantics of this value are entirely dependent on the internal implementation.
    /// </remarks>
    void Transfer(PacketBuffer packet, int byteLimit, ref TransferProgress progress);
  }


  /// <summary>
  /// Additional methods for <see cref="Resource"/>.
  /// </summary>
  public static class ResourceUtil
  {
    /// <summary>
    /// Generates an unique key for a <see cref="Resource"/>. This is unique among all resources.
    /// </summary>
    /// <remarks>
    /// The key is simply a combination of the <see cref="Resource.TypeID"/> in the high 32 bits,
    /// and the <see cref="Resource.ID"/> in the low 32 bits.
    /// </remarks>
    /// <returns>The unique resource key.</returns>
    /// <param name="resource">The resource to generate a key for.</param>
    static ulong UniqueKey(this Resource resource)
    {
      return (ulong)resource.TypeID << 32 | (ulong)resource.ID;
    }
  }
}
