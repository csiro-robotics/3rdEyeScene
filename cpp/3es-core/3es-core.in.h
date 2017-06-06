//
// Project configuration header. This is a generated header; do not modify
// it directly. Instead, modify the config.h.in version and run CMake again.
//
#ifndef _3ES_CORE_H_
#define _3ES_CORE_H_

#include "3es-core-export.h"
#include "3esmeta.h"

#ifndef _USE_MATH_DEFINES
#define _USE_MATH_DEFINES
#endif // _USE_MATH_DEFINES
#ifndef NOMINMAX
#define NOMINMAX
#endif // NOMINMAX
#ifndef NOMINMAX
#define NOMINMAX
#endif // NOMINMAX
#include <cmath>
#include <cstddef>

#cmakedefine TES_ZLIB

#ifdef _MSC_VER
// Avoid dubious security warnings for plenty of legitimate code
# ifndef _SCL_SECURE_NO_WARNINGS
#   define _SCL_SECURE_NO_WARNINGS
# endif // _SCL_SECURE_NO_WARNINGS
# ifndef _CRT_SECURE_NO_WARNINGS
#   define _CRT_SECURE_NO_WARNINGS
# endif // _CRT_SECURE_NO_WARNINGS
//#define _CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES 1
#endif // _MSC_VER

#define TES_IS_BIG_ENDIAN @TES_IS_BIG_ENDIAN@
#define TES_IS_NETWORK_ENDIAN @TES_IS_BIG_ENDIAN@

#cmakedefine TES_32
#cmakedefine TES_64

/// #def TES_ZU
/// Defines a printf format specifier suitable for use with size_t.
#ifdef TES_64
#if defined(_MSC_VER)
#define TES_ZU "%Iu"
#else
#define TES_ZU "%zu"
#endif
#else  // TES_64
#define TES_ZU "%u"
#endif // TES_64

#endif // _3ES_CORE_H_
