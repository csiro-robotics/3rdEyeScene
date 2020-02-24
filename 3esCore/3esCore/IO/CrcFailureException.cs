
namespace Tes.IO
{
   /// <summary>
  /// Exception for failure to validate the CRC.
  /// </summary>
 public class CrcFailureException : TesIOException
  {
    /// <summary>
    /// Constructor: message only.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public CrcFailureException(string message) : base(message) {}
    /// <summary>
    /// Constructor: message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception to propagate.</param>
    public CrcFailureException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}