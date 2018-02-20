//
// author: Kazys Stepanas
//
#include "3esdebug.h"

#include <csignal>
#include <cstdio>

namespace tes
{
  void debugBreak()
  {
    std::raise(SIGINT);
  }


  void assertionFailure(const char *msg)
  {
    fprintf(stderr, "%s\n", msg);
    std::raise(SIGINT);
  }
}
