using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Interfaces for TES messages.
  /// </summary>
  /// <remarks>
  /// Defines a <see cref="Write(PacketBuffer)"/> method which can be used with
  /// <see cref="Server.ServerUtil"/>.
  /// </remarks>
  public interface IMessage
  {
    /// <summary>
    /// Write the message content to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The buffer to write to.</param>
    /// <returns>True on successfully writing the message.</returns>
    /// <remarks>
    /// Before calling, the packet must have the correct <c>RoutingID</c> and <c>MessageID</c> set,
    /// normally by calling <see cref="PacketBuffer.Reset(ushort, ushort)"/>.
    /// 
    /// Data must be written in the correct Endian format. Typically, data may be written as follows:
    /// <code lang="csharp">
    ///   uint ui = 42;
    ///   float f = 4.2f;
    ///   writer.WriteBytes(BitConverter.GetBytes(ui), true);
    ///   writer.WriteBytes(BitConverter.GetBytes(f), true);
    /// </code>
    /// </remarks>
    bool Write(PacketBuffer writer);
  }
}
