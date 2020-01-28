using System;

namespace Tes.Exception
{
  /// <summary>
  /// Thrown when a duplicate ID is used.
  /// </summary>
  public class DuplicateIDException : Exception
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public DuplicateIDException() {}
    /// <summary>
    /// Constructor setting the exception messsage to the given value.
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    public DuplicateIDException(string message) : base(message) {}
    /// <summary>
    /// Constructor setting the exception messsage and the inner exception
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    /// <param name="innerException">The inner exception to wrap.</param>
    public DuplicateIDException(string message, Exception innerException) : base(message, innerException) {}
  }
}
