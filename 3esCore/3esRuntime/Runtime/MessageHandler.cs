using System;
using System.IO;
using Tes.IO;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// Delegate used to check if a category is enabled.
  /// </summary>
  /// <param name="categoryID">Category ID to query.</param>
  /// <returns>True if enabled.</returns>
  public delegate bool CategoryCheckDelegate(ushort categoryID);

  /// <summary>
  /// This class defines the interface for any message handler class.
  /// Messages are routed by the <see cref="RoutingID"/>.
  /// </summary>
  /// <remarks>
  /// </remarks>
  public abstract class MessageHandler
  {
    /// <summary>
    /// Flags modifying the normal operating behaviour of a message handler.
    /// </summary>
    [Flags]
    public enum ModeFlags
    {
      /// <summary>
      /// Ignore messages for transient objects. Do not create new transient objects.
      /// </summary>
      /// <remarks>
      /// Normally used to reduce object creation during multi-frame stepping.
      /// </remarks>
      IgnoreTransient = (1 << 0)
    }

    /// <summary>
    /// Current mode control value.
    /// </summary>
    /// <remarks>
    /// Derived classes should respect the <see cref="ModeFlags"/> values as much as possible.
    /// </remarks>
    public ModeFlags Mode { get; set; }

    /// <summary>
    /// Delegate used to check if a category is enabled.
    /// </summary>
    public CategoryCheckDelegate CategoryCheck { get; protected set; }

    /// <summary>
    /// Function used as a <see cref="CategoryCheck"/> to always be true.
    /// </summary>
    /// <param name="categoryID">The category ID to check.</param>
    /// <returns><c>true</c></returns>
    /// <remarks>
    /// Used for handlers which ignore category settings.
    /// </remarks>
    private bool CheckTrue(ushort categoryID) { return true; }

    /// <summary>
    /// Create a message handler with the given delegate to assign to <see cref="CategoryCheck"/>.
    /// </summary>
    /// <param name="categoryCheck">The delegate to invoke for category enable checks.</param>
    public MessageHandler(CategoryCheckDelegate categoryCheck)
    {
      CategoryCheck = categoryCheck;
      if (CategoryCheck == null)
      {
        CategoryCheck = CheckTrue;
      }
    }

    /// <summary>
    /// A reference name for the handler. Used for debugging and logging.
    /// </summary>
    /// <value>A debug name for the handler.</value>
    public abstract string Name { get; }

    /// <summary>
    /// Returns the unique ID for the message handler. This identifies the type of
    /// handled and in some cases, such as Renderers, the type of object handled.
    /// ID ranges are described in the <see cref="Tes.Net.RoutingID"/> enumeration.
    /// </summary>
    /// <value>The unique routing ID for the message handler.</value>
    public abstract ushort RoutingID { get; }

    /// <summary>
    /// Stored server information.
    /// </summary>
    /// <remarks>
    /// Normally updated in <see cref="UpdateServerInfo(Net.ServerInfoMessage)"/>.
    /// </remarks>
    public Net.ServerInfoMessage ServerInfo
    {
      get; protected set;
    }

    /// <summary>
    /// Called to initialise the handler with various 3rd Eye Scene components.
    /// </summary>
    /// <remarks>
    /// Message handlers which instantiate 3D objects should only instantiate
    /// objects under either the <paramref name="root"/> or the <paramref name="serverRoot"/>.
    /// Use <paramref name="root"/> when objects are to be maintained in the Unity coordinate frame.
    /// Use <paramref name="serverRoot"/> when objects are to be maintained in the server coordinate frame.
    /// </remarks>
    /// <param name="root">The 3rd Eye Scene root object. This is always at the origin and in Unity's coordinate frame.</param>
    /// <param name="serverRoot">The scene root, which applies a transform to the server coordinate frame.</param>
    /// <param name="materials">Maintains available materials and shaders.</param>
    public virtual void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials) { }

    public virtual void AddCamera(Camera camera) {}

    /// <summary>
    /// Called on all handlers whenever the server info changes.
    /// </summary>
    /// <param name="info">Server information.</param>
    public virtual void UpdateServerInfo(Net.ServerInfoMessage info) { ServerInfo = info; }

    /// <summary>
    /// Clear all data in the handler. This resets it to the default, initialised state.
    /// For example, this method may be called to clear the scene.
    /// </summary>
    public virtual void Reset() { }

    /// <summary>
    /// Called at the start of a new frame, before processing new messages.
    /// </summary>
    /// <remarks>
    /// In practice, this method is called when the <see cref="Tes.Net.ControlMessageID.EndFrame"/>
    /// message arrives, just prior to processing all messages for the completed frame.
    /// </remarks>
    /// <param name="frameNumber">A monotonic frame counter.</param>
    /// <param name="maintainTransient">True to prevent flushing of transient objects.</param>
    public virtual void BeginFrame(uint frameNumber, bool maintainTransient) { }

    /// <summary>
    /// Called at the end of a frame. In practice, this is likely to be called
    /// at the same time as <see cref="BeginFrame(uint, bool)"/>.
    /// </summary>
    /// <remarks>
    /// This method is called when the <see cref="Tes.Net.ControlMessageID.EndFrame"/>
    /// message arrives, after processing all messages for the completed frame.
    /// </remarks>
    /// <param name="frameNumber">A monotonic frame counter.</param>
    public virtual void EndFrame(uint frameNumber) { }

    /// <summary>
    /// Render the current objects frame.
    /// </summary>
    /// <param name="categoryMask">A bit field representing the state of the first 64 categories.</>
    /// <param name="tesToWorld">The transform from the 3es scene frame into the Unity world frame.</>
    /// <param name="cameraToWorld">The transform from the render camera frame into the Unity world frame.</>
    public virtual void Render(ulong categoryMask, Matrix4x4 tesSceneToUnity, Matrix4x4 primaryCameraTransform) { }

    /// <summary>
    /// Called when a message arrives with a message routing ID matching
    /// <see cref="RoutingID"/>.
    /// </summary>
    /// <remarks>
    /// The main message header has already been decoded into the <paramref name="packet"/>.
    /// The handler is to decode the remaining message data from <paramref name="reader"/>
    /// and effect the appropriate actions.
    /// </remarks>
    /// <returns>An <see cref="Error"/> object with a code of <see cref="ErrorCode.OK"/>
    /// on success. On failure the error code indicates the reason for the failure.</returns>
    /// <param name="packet">The packet object being read.</param>
    /// <param name="reader">A binary reader which is initialised to the payload location in
    /// <paramref name="packet"/>.</param>
    public abstract Error ReadMessage(PacketBuffer packet, BinaryReader reader);

    /// <summary>
    /// Serialise the current state of the handler to the given stream.
    /// </summary>
    /// <param name="writer">The steam to serialise the current state to.</param>
    /// <remarks>
    /// See <see cref="Serialise(BinaryWriter, ref SerialiseInfo)"/>
    /// </remarks>
    public Error Serialise(BinaryWriter writer)
    {
      SerialiseInfo info = new SerialiseInfo();
      return Serialise(writer, ref info);
    }

    /// <summary>
    /// Serialise the current state of the handler to the given stream.
    /// </summary>
    /// <remarks>
    /// This is in direct support of recording incoming data streams. In
    /// particular, this supports when recording is requested after the first
    /// data packet has come through. This call must write the required messages
    /// to restore the handler from its initial state to its exact current state.
    ///
    /// For example, the sphere rendering shape handler will simulate sphere
    /// creation packets for the existing spheres.
    /// </remarks>
    /// <param name="writer">The steam to serialise the current state to.</param>
    /// <param name="info">Passed to attain information about serialisation.</param>
    public abstract Error Serialise(BinaryWriter writer, ref SerialiseInfo info);

    /// <summary>
    /// Called whenever a category's active state changes. All start active.
    /// </summary>
    /// <param name="categoryId">The category changing state.</param>
    /// <param name="active">Active or not?</param>
    public abstract void OnCategoryChange(ushort categoryId, bool active);
  }
}

