//
// author: Kazys Stepanas
//
#ifndef _3ESRESOURCEPACKER_H_
#define _3ESRESOURCEPACKER_H_

#include "3es-core.h"

#include <cstdint>

namespace tes
{
  class Resource;
  struct TransferProgress;
  class PacketWriter;

  class _3es_coreAPI ResourcePacker
  {
  public:
    ResourcePacker();
    ~ResourcePacker();

    inline const Resource *resource() const { return _resource; }
    inline bool isNull() const { return _resource == nullptr; }
    void transfer(const Resource *resource);
    void cancel();

    inline uint64_t lastCompletedId() const { return _lastCompletedId; }

    bool nextPacket(PacketWriter &packet, unsigned byteLimit);

  private:
    const Resource *_resource;
    TransferProgress *_progress;
    uint64_t _lastCompletedId;
    bool _started;
  };
}

#endif // _3ESRESOURCEPACKER_H_
