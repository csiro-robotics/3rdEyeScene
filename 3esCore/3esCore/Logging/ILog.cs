
namespace Tes.Logging
{
  /// <summary>
  /// Log interface supporting level based and categorised logging.
  /// </summary>
  /// <remarks>
  /// Category names are resolved using <see cref="T:LogCategories" />.
  /// </remarks>
  public interface ILog
  {
    /// <summary>
    /// Log a message with the given level and category.
    /// </summary>
    /// <param name="level">The message log level.</param>
    /// <param name="category">Category for the log message.</param>
    /// <param name="message">The log message. Formatting follows <c>string.Format()</c></param>
    /// <param name="args">Additional format arguments.</param>
    void Log(LogLevel level, int category, string message, params object[] args);

    /// <summary>
    /// Log a message with the given level in the default category.
    /// </summary>
    /// <param name="level">The message log level.</param>
    /// <param name="message">The log message. Formatting follows <c>string.Format()</c></param>
    /// <param name="args">Additional format arguments.</param>
    void Log(LogLevel level, string message, params object[] args);

    /// <summary>
    /// Log an information message in the given category.
    /// </summary>
    /// <param name="category">Category for the log message.</param>
    /// <param name="message">The log message. Formatting follows <c>string.Format()</c></param>
    /// <param name="args">Additional format arguments.</param>
    void Log(int category, string message, params object[] args);

    /// <summary>
    /// Log an information message in the default category.
    /// </summary>
    /// <param name="message">The log message. Formatting follows <c>string.Format()</c></param>
    /// <param name="args">Additional format arguments.</param>
    void Log(string message, params object[] args);

		/// <summary>
		/// Log details of an exception associating the output with a category.
		/// </summary>
		/// <param name="category">Category for the log message.</param>
		/// <param name="e">The exception to log details of.</param>
		void Log(int category, System.Exception e);

		/// <summary>
		/// Log details of an exception.
		/// </summary>
		/// <param name="e">The exception to log details of.</param>
		void Log(System.Exception e);
  }
}
