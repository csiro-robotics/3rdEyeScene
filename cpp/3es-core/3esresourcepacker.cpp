//
// author: Kazys Stepanas
//
#include "3esresourcepacker.h"

#include "3espacketwriter.h"
#include "3esresource.h"
#include "3estransferprogress.h"

#include <algorithm>
#include <cstdio>

using namespace tes;

ResourcePacker::ResourcePacker()
  : _resource(nullptr)
  , _progress(new TransferProgress)
  , _lastCompletedId(0)
  , _started(false)
{
  _progress->reset();
}


ResourcePacker::~ResourcePacker()
{
  cancel();
  delete _progress;
}


void ResourcePacker::transfer(const Resource *resource)
{
  cancel();
  _resource = resource;
}


void ResourcePacker::cancel()
{
  _progress->reset();
  _resource = nullptr;
  _started = false;
}


bool ResourcePacker::nextPacket(PacketWriter &packet, unsigned byteLimit)
{
  if (!_resource)
  {
    return false;
  }

  if (!_started)
  {
    _resource->create(packet);
    _started = true;
    return true;
  }

  if (_resource->transfer(packet, (int)byteLimit, *_progress) != 0)
  {
    _progress->failed = true;
    _resource = nullptr;
    _progress->reset();
    return false;
  }

  if (_progress->complete || _progress->failed)
  {
    _lastCompletedId = _resource->uniqueKey();
    _resource = nullptr;
    _progress->reset();
  }

  return true;
}


