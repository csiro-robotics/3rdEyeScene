namespace Tes.Runtime
{
  /// <summary>
  /// Contains global configuration settings which affect the 3es runtime.
  /// </summary>
  public static class GlobalSettings
  {
    /// <summary>
    /// Controls the base rendering size for point primitives.
    /// </summary>
    /// <value></value>
    public static float PointSize { get; set; } = 1.0f;
  }
}