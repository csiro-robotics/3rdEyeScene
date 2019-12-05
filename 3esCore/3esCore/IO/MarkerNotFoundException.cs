
namespace Tes.IO
{
  /// <summary>
  /// Exception for when the <see cref="PacketHeader.PacketMarker" /> pattern cannot be found in a data stream.
  /// </summary>
  public class MarkerNotFoundException : TesIOException
  {
    /// <summary>
    /// Constructor: message only.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public MarkerNotFoundException(string message) : base(message) {}
    /// <summary>
    /// Constructor: message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception to propagate.</param>
    public MarkerNotFoundException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}