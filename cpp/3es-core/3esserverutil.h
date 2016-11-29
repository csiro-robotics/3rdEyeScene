//
// author: Kazys Stepanas
//
#ifndef _3ESSERVERUTIL_H_
#define _3ESSERVERUTIL_H_

#include "3es-core.h"

#include <3espacketwriter.h>

namespace tes
{
  template <class MESSAGE, class Server, unsigned BufferSize = 256>
  int sendMessage(Server &server, uint16_t routingId, uint16_t messageId, const MESSAGE &message)
  {
    uint8_t buffer[BufferSize];
    PacketWriter writer(buffer, BufferSize);
    writer.reset(routingId, messageId);
    if (message.write(writer) && writer.finalise())
    {
      return server.send(writer.data(), writer.packetSize());
    }

    return -1;
  }
}

#endif // _3ESSERVERUTIL_H_
