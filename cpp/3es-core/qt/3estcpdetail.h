
//
// author: Kazys Stepanas
//
#ifndef _3ESTCPDETAIL_H_
#define _3ESTCPDETAIL_H_

#include "3es-core.h"

#include <QTcpServer>
#include <QTcpSocket>

namespace tes
{
  struct TcpSocketDetail
  {
    QTcpSocket *socket;
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
    QTcpServer listenSocket;
  };
}

#endif // _3ESTCPDETAIL_H_
