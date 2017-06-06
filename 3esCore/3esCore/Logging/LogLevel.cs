namespace Tes.Logging
{
  /// <summary>
  /// Various log levels. Lower numbers are more important.
  /// /// </summary>
  public enum LogLevel
  {
    /// <summary>
    /// Exception or critical level logging.
    /// </summary>
    Critical,
    /// <summary>
    /// Error message logging.
    /// </summary>
    Error,
    /// <summary>
    /// Warning logging.
    /// </summary>
    Warning,
    /// <summary>
    /// General logging.
    /// </summary>
    Info,
    /// <summary>
    /// Low level diagnostic logging.
    /// </summary>
    Diagnostic
  }
}
