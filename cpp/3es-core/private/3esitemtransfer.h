//
// author: Kazys Stepanas
//
#ifndef _3ESITEMTRANSFER_H_
#define _3ESITEMTRANSFER_H_

#include "3es-core.h"

namespace tes
{
  class Connection;
  class PacketWriter;

  class ItemTransfer
  {
  public:
    enum TransferProgress
    {
      Idle,
      Complete,
      Started,
      InProgress
    };

    virtual ~ItemTransfer() {}

    virtual bool isNull() const = 0;
    virtual void cancel() = 0;
    virtual TransferProgress transferUpdate(Connection &connection, PacketWriter &writer, unsigned byteLimit, unsigned &bytesTransfered) = 0;
  };
}

#endif // _3ESITEMTRANSFER_H_
