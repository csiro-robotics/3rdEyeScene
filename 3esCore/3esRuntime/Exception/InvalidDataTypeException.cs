namespace Tes.Exception
{
  /// <summary>
  /// Thrown when an invalid data type is requested.
  /// </summary>
  public class InvalidDataTypeException : System.Exception
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public InvalidDataTypeException() {}
    /// <summary>
    /// Constructor setting the exception messsage to the given value.
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    public InvalidDataTypeException(string message) : base(message) {}
    /// <summary>
    /// Constructor setting the exception messsage and the inner exception
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    /// <param name="innerException">The inner exception to wrap.</param>
    public InvalidDataTypeException(string message, System.Exception innerException) : base(message, innerException) {}
  }
}
