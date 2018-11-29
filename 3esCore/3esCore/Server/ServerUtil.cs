using System;
using Tes.IO;
using Tes.Net;

namespace Tes.Server
{
  /// <summary>
  /// Server side helper and utility functions.
  /// </summary>
  public static class ServerUtil
  {
    /// <summary>
    /// Helper for sending an arbitrary message via <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection">The client or clients to send to.</param>
    /// <param name="routingId">The message <see cref="RoutingID"/>.</param>
    /// <param name="messageId">The message ID associated with the <paramref name="routingId"/>.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>The number of bytes written, or -1 on failure.</returns>
    public static int SendMessage(IServer connection, ushort routingId, ushort messageId, IMessage message)
    {
      PacketBuffer buffer = new PacketBuffer(1024);

      buffer.Reset(routingId, messageId);
      if (message.Write(buffer) && buffer.FinalisePacket())
      {
        return connection.Send(buffer);
      }

      return -1;
    }
  }
}
