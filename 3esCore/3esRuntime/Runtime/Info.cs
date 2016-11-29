namespace Tes.Runtime
{
  /// <summary>
  /// Information about the way the library was built.
  /// </summary>
  /// <remarks>
  /// Primarily used to generate a warning when finalising a Unity Third Eye Scene build against the
  /// debug version of this library.
  /// </remarks>
  public static class Info
  {
    /// <summary>
    /// Is this a debug build of this library?
    /// </summary>
    public static bool IsDebug
    {
      get
      {
#if DEBUG
        return true;
#else  // DEBUG
        return false;
#endif // DEBUG
      }
    }
  }
}
