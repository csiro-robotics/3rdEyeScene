// 
// author: Kazys Stepanas
// 
#include "3esmessages.h" 

#include <cstring>

void tes::initDefaultServerInfo(ServerInfoMessage *info)
{
  memset(info, 0, sizeof(*info));
  info->timeUnit = 1000ull;
  info->defaultFrameTime = 33ull;
  info->coordinateFrame = 0;
}
