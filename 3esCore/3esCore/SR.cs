#pragma warning disable 1591
namespace Tes
{
  /// <summary>
  /// Replacement for SR, translated error codes. No translation provided.
  /// </summary>
  internal class SR
  {
    public const string ArgumentException_BufferNotFromPool = "Argument exception: buffer not from pool";

    internal static string GetString(string p)
    {
      return p;
    }

    internal static string Format(string a, params object[] args)
    {
      return string.Format(a, args);
    }
  }
}
