#ifndef _3ES_OCCUPANCY_H_
#define _3ES_OCCUPANCY_H_

#include "3es-core.h"

#define TES_ENABLE
#ifdef TES_ENABLE
namespace tes { class Server; }
extern tes::Server *g_tesServer;
#endif // TES_ENABLE

#include "debugids.h"

#endif // _3ES_OCCUPANCY_H_
