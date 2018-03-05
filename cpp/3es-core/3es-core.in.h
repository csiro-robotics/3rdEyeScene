//
// Project configuration header. This is a generated header; do not modify
// it directly. Instead, modify the config.h.in version and run CMake again.
//
#ifndef _3ES_CORE_H_
#define _3ES_CORE_H_

#include "3es-core-export.h"
#include "3esmeta.h"

// Version setup.
#define TES_VERSION_MAJOR @TES_VERSION_MAJOR@
#define TES_VERSION_MINOR @TES_VERSION_MINOR@
#define TES_VERSION_PATCH @TES_VERSION_PATCH@
#define TES_VERSION "@TES_VERSION@"

// Force MSVC to define useful things like M_PI.
#ifndef _USE_MATH_DEFINES
#define _USE_MATH_DEFINES
#endif // _USE_MATH_DEFINES

// For MSVC to skip defining min/max as macros.
#ifndef NOMINMAX
#define NOMINMAX
#endif // NOMINMAX
#ifndef NOMINMAX
#define NOMINMAX
#endif // NOMINMAX

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

// Include standard headers to ensure we effect the configuration above.
#include <cmath>
// #include <cstddef>

// Define TES_ZLIB if in use
#cmakedefine TES_ZLIB

// Define the local Endian and the network Endian
#define TES_IS_BIG_ENDIAN @TES_IS_BIG_ENDIAN@
#define TES_IS_NETWORK_ENDIAN @TES_IS_BIG_ENDIAN@

// Define assertion usage.s
#cmakedefine TES_ASSERT_ENABLE_DEBUG
#cmakedefine TES_ASSERT_ENABLE_RELEASE

#if defined(NDEBUG) && defined(TES_ASSERT_ENABLE_RELEASE) || !defined(NDEBUG) && defined(TES_ASSERT_ENABLE_DEBUG)
#define TES_ASSERT_ENABLE 1
#endif // defined(NDEBUG) && defined(TES_ASSERT_ENABLE_RELEASE) || !defined(NDEBUG) && defined(TES_ASSERT_ENABLE_DEBUG)

// Define the word size (in bits)s
#cmakedefine TES_32
#cmakedefine TES_64

// Define a useful printf format string for size_t
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
