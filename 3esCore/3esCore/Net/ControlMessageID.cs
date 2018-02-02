using System;

namespace Tes.Net
{
  /// <summary>
  /// Control message IDs for <see cref="ControlMessage"/> 
  /// </summary>
  public enum ControlMessageID : ushort
  {
    /// <summary>
    /// Invalid ID.
    /// </summary>
    Null = 0,
    /// <summary>
    /// Notifies the change of frame. Pending objects changes are applied.
    /// </summary>
    /// <remarks>
    /// <c>Value32</c> specifies the frame time delta in the server time units, or to use
    /// the default time delta when 0. <c>Value64</c> should always be zero, but is used
    /// internally during playback to identify the frame number.
    /// </remarks>
    EndFrame,
    /// <summary>
    /// Specifies the coordinate frame to adopt. <c>Value32</c> is one of <see cref="Tes.Net.CoordinateFrame"/>.
    /// </summary>
    CoordinateFrame,
    /// <summary>
    /// Reports the total number of frames in a recorded stream. <c>Value32</c> is an unsigned integer.
    /// </summary>
    /// <remarks>
    /// This is intended for use only at the start of a recorded file stream, not for use by live servers.
    /// </remarks>
    FrameCount,
    /// <summary>
    /// Forces a frame update without advancing the time. 
    /// </summary>
    /// <remarks>
    /// This message is primarily used at the start of a recording in order to display the shapes which have
    /// been loaded so far.
    /// </remarks>
    ForceFrameFlush,
    /// <summary>
    /// Reset the simulation state. All current objects and data are dropped and destroyed.
    /// </summary>
    /// <remarks>
    /// This is primarily intended for internal use in playback mode. The <c>Value32</c> is used to
    /// identify the frame number to which we are resetting.
    /// </remarks>
    Reset,
    /// <summary>
    /// Request a keframe. <c>Value32</c> is the frame number.
    /// </summary>
    /// <remarks>
    /// This is not for remote transmission, but supports snapping the scene in order
    /// to improve step-back updates.
    /// </remarks>
    Keyframe,
    /// <summary>
    /// Marks the end of the server stream. Clients may disconnect.
    /// </summary>
    End
  }


  /// <summary>
  /// Flags for <see cref="ControlMessageID.EndFrame"/> messages.
  /// </summary>
  [Flags]
  public enum EndFrameFlag
  {
    /// <summary>
    /// Indicates transient objects should be maintained and not flushed for this frame.
    /// </summary>
    Persist = (1 << 0)
  }
}

