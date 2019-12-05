namespace Tes.IO
{
  /// <summary>
  /// A general IO exception from 3rd Eye Scene and base class for specialised exceptions.
  /// </summary>
  public class TesIOException : System.IO.IOException
  {
    /// <summary>
    /// Constructor: message only.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public TesIOException(string message) : base(message) {}
    /// <summary>
    /// Constructor: message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception to propagate.</param>
    public TesIOException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}
