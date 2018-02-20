//
// author: Kazys Stepanas
//
#ifndef _3ESINTARG_H_
#define _3ESINTARG_H_

#include "3es-core.h"

#include "3esdebug.h"

#include <cstddef>

namespace tes
{
  /// A helper structure for handling integer arguments of various types without generating compiler warnings.
  ///
  /// This is intended primarily for size_t arguments from std::vectors::size() calls
  /// passed to things like the @c MesShape or @c SimpleMesh. The argument may be given as @c int,
  /// @c unsigned, or @c size_t and converted to the same. A conversion which will
  /// lose information generates a runtime error.
  ///
  /// Cast operators are supplied to support casting down to the specific required type.
  struct IntArg
  {
    /// Constructor from @c int.
    /// @param s The argument value.
    inline IntArg(int s) : _value(s) {}
    /// Constructor from @c unsigned.
    /// @param s The argument value.
    inline IntArg(unsigned s) : _value(s) {}

#ifdef TES_64
    /// Constructor from @c size_t.
    /// @param s The argument value.
    inline IntArg(size_t s) : _value(s) {}
#endif // TES_64

    /// Boolean cast operator.
    /// @return @c true if @c _value is non-zero.
    inline operator bool() const { return _value != 0; }

    /// Convert to @c int. Raised an error if @c _value is too large.
    /// @return The size as an integer.
    inline operator int() const { return i(); }

    /// Convert to @c int. Raised an error if @c _value is too large.
    /// @return The size as an unsigned integer.
    inline operator unsigned() const { return u(); }

#ifdef TES_64
    /// Convert to @c size_t.
    /// @return The size as an size_t integer.
    inline operator size_t() const { return _value; }
#endif // TES_64

    /// Shorthand cast function to @c int.
    /// @return The value as @c int.
    inline int i() const { TES_ASSERT(_value <= 0x7FFFFFFFu); return int(_value); }
    /// Shorthand cast function to @c unsigned.
    /// @return The value as @c unsigned.
    inline unsigned u() const { TES_ASSERT(_value <= 0xFFFFFFFFu); return unsigned(_value); }
    /// Shorthand cast function to @c size_t.
    /// @return The value as @c size_t.
    inline size_t st() const { return _value; }

    /// The stored size value.
    size_t _value;
  };
}

#ifndef DOXYGEN_SHOULD_SKIP_THIS

#define _TES_INTARG_BOOL_OP2(INT, OP) \
  inline bool operator OP(INT a, const tes::IntArg &b) { return a OP static_cast<INT>(b); } \
  inline bool operator OP(const tes::IntArg &a, INT b) { return static_cast<INT>(a) OP b; }
//#define _TES_INTARG_ARITH_OP2(INT, OP) \
//  inline INT operator OP(INT a, const tes::IntArg &b) { return a OP static_cast<INT>(b); } \
//  inline INT operator OP(const tes::IntArg &a, INT b) { return static_cast<INT>(a) OP b; }

// Comparison operators for @c IntArg.
#define TES_INTARG_OPERATORS(INT) \
  _TES_INTARG_BOOL_OP2(INT, <) \
  _TES_INTARG_BOOL_OP2(INT, <=) \
  _TES_INTARG_BOOL_OP2(INT, >) \
  _TES_INTARG_BOOL_OP2(INT, >=) \
  _TES_INTARG_BOOL_OP2(INT, ==) \
  _TES_INTARG_BOOL_OP2(INT, !=)

  //inline bool operator<(INT a, const tes::IntArg &b) { return a < static_cast<INT>(b); } \
  //inline bool operator<=(INT a, const tes::IntArg &b) { return a <= static_cast<INT>(b); } \
  //inline bool operator>(INT a, const tes::IntArg &b) { return a > static_cast<INT>(b); } \
  //inline bool operator>=(INT a, const tes::IntArg &b) { return a >= static_cast<INT>(b); } \
  //inline bool operator<(const tes::IntArg &a, INT b) { return static_cast<INT>(a) < b; } \
  //inline bool operator<=(const tes::IntArg &a, INT b) { return static_cast<INT>(a) <= b; } \
  //inline bool operator>(const tes::IntArg &a, INT b) { return static_cast<INT>(a) > b; } \
  //inline bool operator>=(const tes::IntArg &a, INT b) { return static_cast<INT>(a) >= b; } \
  //inline bool operator==(INT a, const tes::IntArg &b) { return a == static_cast<INT>(b); } \
  //inline bool operator!=(INT a, const tes::IntArg &b) { return a != static_cast<INT>(b); } \
  //inline bool operator==(const tes::IntArg &a, INT b) { return static_cast<INT>(a) == b; } \
  //inline bool operator!=(const tes::IntArg &a, INT b) { return static_cast<INT>(a) != b; } \
  //inline INT operator+(INT a, tes::IntArg &)

TES_INTARG_OPERATORS(int);
TES_INTARG_OPERATORS(unsigned);

#ifdef TES_64
TES_INTARG_OPERATORS(size_t);
#endif // TES_64
#endif // DOXYGEN_SHOULD_SKIP_THIS

#endif // _3ESINTARG_H_
