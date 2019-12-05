
namespace Tes.IO
{
   /// <summary>
  /// Exception for when insufficient bytes are available from a data stream to complete a data packet.
  /// </summary>
 public class InsufficientDataException : TesIOException
  {
    /// <summary>
    /// Constructor: message only.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public InsufficientDataException(string message) : base(message) {}
    /// <summary>
    /// Constructor: message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception to propagate.</param>
    public InsufficientDataException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}