//
// author: Kazys Stepanas
//
#ifndef _3ESDEBUG_H_
#define _3ESDEBUG_H_

#include "3es-core.h"

#if TES_ASSERT_ENABLE
#define TES_ASSERT2(x, msg) if (!(x)) { tes::assertionFailure("Assertion failed: " msg); }
#define TES_ASSERT(x) TES_ASSERT2(x, #x)
#else  // TES_ASSERT_ENABLE
#define TES_ASSERT(x)
#define TES_ASSERT2(x, msg)
#endif // TES_ASSERT_ENABLE

namespace tes
{
  /// Trigger a programmatic breakpoint. Behaviour varies between platforms.
  void _3es_coreAPI debugBreak();

  /// Called on assertion failures. Prints @p msg and triggers a programmatic breakpoint.
  /// @param msg The assertion message to display.
  void _3es_coreAPI assertionFailure(const char *msg = "");
}

#endif // _3ESDEBUG_H_
