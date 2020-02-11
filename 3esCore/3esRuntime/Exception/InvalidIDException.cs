namespace Tes.Exception
{
  /// <summary>
  /// Thrown when an invalid object ID is given.
  /// </summary>
  public class InvalidIDException : System.Exception
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public InvalidIDException() {}
    /// <summary>
    /// Constructor setting the exception messsage to the given value.
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    public InvalidIDException(string message) : base(message) {}
    /// <summary>
    /// Constructor setting the exception messsage and the inner exception
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    /// <param name="innerException">The inner exception to wrap.</param>
    public InvalidIDException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}
