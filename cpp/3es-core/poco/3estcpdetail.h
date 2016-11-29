
//
// author: Kazys Stepanas
//
#ifndef _3ESTCPDETAIL_H_
#define _3ESTCPDETAIL_H_

#include "3es-core.h"

#include <Poco/Net/TCPServer.h>
#include <Poco/Net/TCPSocket.h>

namespace tes
{
  struct TcpSocketDetail
  {
    Poco::Net::TcpSocket *socket;
    int readTimeout;
    int writeTimeout;

    inline TcpSocketDetail()
      : socket(nullptr)
      , readTimeout(~0u)
      , writeTimeout(~0u)
   {}

    inline ~TcpSocketDetail()
    {
      delete socket;
    }
  };

  struct TcpListenSocketDetail
  {
    Poco::Net::TcpServer *listenSocket;

    TcpListenSocketDetail() : listenSocket(nullptr) {}
    ~TcpListenSocketDetail()
    {
      delete listenSocket;
    }
  };
}

#endif // _3ESTCPDETAIL_H_
