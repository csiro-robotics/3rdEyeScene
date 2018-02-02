using System;
using Tes.IO;
using Tes.Net;

namespace Tes.Server
{
  public static class ServerUtil
  {
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
