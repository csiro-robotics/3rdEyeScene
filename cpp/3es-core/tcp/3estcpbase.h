#ifndef _3ESTCPBASE_H
#define _3ESTCPBASE_H

#include "3es-core.h"

struct timeval;

namespace tes
{
  namespace tcpbase
  {
    enum SocketError
    {

    };

    int _3es_coreAPI create();
    void _3es_coreAPI close(int socket);

    bool _3es_coreAPI setReceiveTimeout(int socket, unsigned timeoutMs);
    unsigned _3es_coreAPI getReceiveTimeout(int socket);

    bool _3es_coreAPI setSendTimeout(int socket, unsigned timeoutMs);
    unsigned _3es_coreAPI getSendTimeout(int socket);

    void _3es_coreAPI enableBlocking(int socket);
    void _3es_coreAPI disableBlocking(int socket);

    void _3es_coreAPI timevalFromMs(timeval &tv, unsigned milliseconds);

    void _3es_coreAPI dumpSocketOptions(int socket);

    unsigned short _3es_coreAPI getSocketPort(int socket);

    bool _3es_coreAPI isConnected(int socket);

    void _3es_coreAPI setNoDelay(int socket, bool noDelay);

    bool _3es_coreAPI noDelay(int socket);

    bool _3es_coreAPI checkSend(int socket, int ret, bool reportDisconnect = false);

    bool _3es_coreAPI checkRecv(int socket, int ret, bool reportDisconnect = false);

    int _3es_coreAPI getSendBufferSize(int socket);

    bool _3es_coreAPI setSendBufferSize(int socket, int bufferSize);

    int _3es_coreAPI getReceiveBufferSize(int socket);

    bool _3es_coreAPI setReceiveBufferSize(int socket, int bufferSize);
  }
}

#endif // _3ESTCPBASE_H
