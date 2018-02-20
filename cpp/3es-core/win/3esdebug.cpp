//
// author: Kazys Stepanas
//
#include "3esdebug.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

namespace tes
{
  void debugBreak()
  {
    DebugBreak();
  }


  void assertionFailure(const char *msg)
  {
    OutputDebugStringA(msg);
    OutputDebugStringA("\n");
    DebugBreak();
  }
}
