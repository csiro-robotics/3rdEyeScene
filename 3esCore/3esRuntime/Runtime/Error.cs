
namespace Tes.Runtime
{
  /// <summary>
  /// This class encapsulated error code values returned by various processing functions.
  /// </summary>
  /// <remarks>
  /// 3rd Eye Scene uses error code in many places instead of exceptions for performance
  /// reasons.
  /// </remarks>
  public class Error
  {
    /// <summary>
    /// The primary error code value. Normally correlates to a value in <see cref="ErrorCode"/>.
    /// </summary>
    public int Code;
    /// <summary>
    /// An additional value for the error code.
    /// </summary>
    public long Value;
    
    /// <summary>
    /// Create an error where <see cref="Success"/> is <c>true</c>.
    /// </summary>
    public Error() : this((int)ErrorCode.OK, 0) {}

    /// <summary>
    /// Create an error with the given code.
    /// </summary>
    /// <param name="code">The error code.</param>
    public Error(int code) : this(code, 0) {}

    /// <summary>
    /// Create an error with the given code and value.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="value">The error value associated with the code.</param>
    public Error(int code, long value)
    {
      Code = code;
      Value = value;
    }

    /// <summary>
    /// Create an error with an <see cref="ErrorCode"/>
    /// </summary>
    /// <param name="code">The error code.</param>
    public Error(ErrorCode code) : this((int)code, 0) {}

    /// <summary>
    /// Create an error with an <see cref="ErrorCode"/> and value.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="value">The error value associated with the code.</param>
    public Error(ErrorCode code, long value) : this((int)code, value) { }

    /// <summary>
    /// Check the error code for success.
    /// </summary>
    /// <returns>True if the <see cref="Code"/> is zero.</returns>
    public bool Success { get { return Code == (int)ErrorCode.OK; } }

    /// <summary>
    /// Check the error code for failure.
    /// </summary>
    /// <returns>True if the <see cref="Code"/> is non zero.</returns>
    public bool Failed { get { return Code != (int)ErrorCode.OK; } }

  }
}

