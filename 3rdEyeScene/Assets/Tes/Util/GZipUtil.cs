using System.IO;

namespace Tes.Util
{
  /// <summary>
  /// GZip utilitie methods.
  /// </summary>
  public static class GZipUtil
  {
    public const byte GZipID1 = 0x1F;
    public const byte GZipID2 = 0x8B;

    /// <summary>
    /// Check whether <paramref name="stream"/> contains a GZip header.
    /// This is not the same as checking for the <see cref="GZipStream"/> class.
    /// </summary>
    /// <param name="stream">The stream to check. Must support releative seeking.</param>
    /// <returns>True if a GZip header is found at the current position of <paramref name="stream"/>.</returns>
    public static bool IsGZipStream(Stream stream)
    {
      bool foundHeader = true;
      foundHeader = stream.ReadByte() == GZipID1 && foundHeader;
      foundHeader = stream.ReadByte() == GZipID2 && foundHeader;
      stream.Seek(-2, SeekOrigin.Current);
      stream.Flush();
      return foundHeader;
    }
  }
}
