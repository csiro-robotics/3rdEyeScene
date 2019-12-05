
namespace Tes.IO
{
  /// <summary>
  /// Decoding a data packet has failed.
  /// </summary>
  public class DecodeException : TesIOException
  {
    /// <summary>
    /// Constructor: message only.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public DecodeException(string message) : base(message) {}
    /// <summary>
    /// Constructor: message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception to propagate.</param>
    public DecodeException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}