//
// author: Kazys Stepanas
//
// This file contains utility macros and defines mostly used to avoid compiler
// warnings.
//
#ifndef _3ESMETA_H_
#define _3ESMETA_H_

// Do not include 3es-core for now. That would be circular.

#ifdef __clang__
#define TES_FALLTHROUGH [[clang::fallthrough]]
#endif // __clang__


// Fall back definitions.
#ifndef TES_FALLTHROUGH
/// Use this macro at the end of a switch statement case which is to fall through without a break.
#define TES_FALLTHROUGH
#endif // TES_FALLTHROUGH

#endif // _3ESMETA_H_