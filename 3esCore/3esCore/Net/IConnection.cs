using Tes.Shapes;

namespace Tes.Net
{
  /// <summary>
  /// Defines the interface for a TES connection.
  /// </summary>
  public interface IConnection
  {
    /// <summary>
    /// The connection end point address.
    /// </summary>
    string Address { get; }
    /// <summary>
    /// The connection port.
    /// </summary>
    int Port { get; }
    /// <summary>
    /// Reflects the connected state.
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    void Close();

    /// <summary>
    /// Sends a message marking the end of the current frame (and start of a new frame).
    /// </summary>
    /// <param name="dt">Indicates the time passed since over this frame (seconds).</param>
    /// <param name="flush">True to allow clients to flush transient options, false to clients
    ///   preserver such objects.</param>
    /// <returns>The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int UpdateFrame(float dt, bool flush = true);

    /// <summary>
    /// Sends a create message for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">The shape to create.</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int Create(Shape shape);

    /// <summary>
    /// Sends a destroy message for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">The shape to destroy.</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int Destroy(Shape shape);

    /// <summary>
    /// Sends an update message for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">The shape to update.</param>
    /// <returns>
    /// The number of bytes queued for transfer for this message, or negative on error.
    /// The negative value may be less than -1 and still indicate the successful transfer size.
    /// </returns>
    int Update(Shape shape);

    /// <summary>
    /// Sends the <see cref="ServerInfoMessage"/> structure to the connected client.
    /// </summary>
    /// <param name="info">The info message to send.</param>
    /// <returns>True on success.</returns>
    bool SendServerInfo(ServerInfoMessage info);

    /// <summary>
    /// Sends data on the client connection.
    /// </summary>
    /// <param name="data">The data buffer to send.</param>
    /// <param name="offset">An offset into <paramref name="data"/> at which to start sending.</param>
    /// <param name="length">The number of bytes to transfer.</param>
    /// <returns>The number of bytes transferred or -1 on failure.</returns>
    int Send(byte[] data, int offset, int length);

    /// <summary>
    /// Queries the reference count of the given resource.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    uint GetReferenceCount(Resource resource);

    /// <summary>
    /// Add a shared resource to the connection.
    /// </summary>
    /// <param name="resource">The resource object to add. Increments the reference count if
    /// already present.</param>
    /// <returns>The reference count for the resource after adding it.</returns>
    /// <remarks>
    /// <see cref="Shape"/> objects may have shared resources which must persist so long as any
    /// shape is referring to them. A prime example is <see cref="MeshSet"/> instances, each of
    /// which has at least one <see cref="MeshResource"/>. These parts may be referenced and shared by
    /// multiple <see cref="MeshSet"/> objects and are considered shared resources. Each part
    /// must persist so long as at least one shape is referencing it.
    /// 
    /// The concept is generalised to the <see cref="Resource"/> interface. Each call to
    /// <see cref="AddResource(Resource)"/> increases the reference counting of that resource and
    /// enables creation messages for that resource. Each call to <see cref="RemoveResource(Resource)"/>
    /// reduces the reference count. A destruction message is sent when a resource reaches a zero
    /// reference count.
    /// 
    /// Adding a shared resource adds it to the resource transfer queue. Data transfer may be amortised
    /// over the next series of updates and combined with the transfer of other shared resources.
    /// 
    /// Note: in order to differentiate shared resources, it is critical that each resource has an
    /// unique ID. This ID is used to identify the resource, not the object reference.
    /// </remarks>
    uint AddResource(Resource resource);

    /// <summary>
    /// Removes a shared resource.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    /// <remarks>
    /// The resource has its reference count decremented and is removed from the transfer queue.
    /// The destruction message is send to complete the removal, though there may be a delay before
    /// sending this message.
    /// </remarks>
    uint RemoveResource(Resource resource);
  }
}
