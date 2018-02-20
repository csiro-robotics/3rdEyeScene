//
// author: Kazys Stepanas
//
#ifndef _3ESINTARG_H_
#define _3ESINTARG_H_

#include "3es-core.h"

#include "3esdebug.h"

#include <cstddef>
#include <limits>

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
  template <typename INT>
  struct IntArgT
  {
    typedef INT ValueType;

    /// Constructor from @c int.
    /// @param s The argument value.
    inline IntArgT(int ii) : i(ii) { TES_ASSERT(std::numeric_limits<INT>::min() <= ii); TES_ASSERT(ii <= std::numeric_limits<INT>::max()); }
    /// Constructor from @c unsigned.
    /// @param s The argument value.
    inline IntArgT(unsigned ii) : i(ii) { TES_ASSERT(std::numeric_limits<INT>::min() <= ii); TES_ASSERT(ii <= std::numeric_limits<INT>::max()); }

#ifdef TES_64
    /// Constructor from @c size_t.
    /// @param s The argument value.
    inline IntArgT(size_t ii) : i(ii) { TES_ASSERT(std::numeric_limits<INT>::min() <= ii); TES_ASSERT(ii <= std::numeric_limits<INT>::max()); }
#endif // TES_64

    /// Boolean cast operator.
    /// @return @c true if @c i is non-zero.
    inline operator bool() const { return i != 0; }

    /// Convert to @c int. Raised an error if @c i is too large.
    /// @return The size as an integer.
    inline operator INT() const { return i; }

    /// The stored size value.
    INT i;
  };

  template struct _3es_coreAPI IntArgT<int>;
  typedef IntArgT<int> IntArg;
  template struct _3es_coreAPI IntArgT<unsigned>;
  typedef IntArgT<unsigned> UIntArg;
#ifdef TES_64
  template struct _3es_coreAPI IntArgT<size_t>;
  typedef IntArgT<size_t> SizeTArg;
#else  // TES_64
  typedef UIntArg SizeTArg;
#endif // TES_64
}

#ifndef DOXYGEN_SHOULD_SKIP_THIS

#define _TES_INTARG_BOOL_OP_SELF(INTARG, OP) \
  inline bool operator OP(const INTARG &a, const INTARG &b) { return static_cast<INTARG::ValueType>(a) OP static_cast<INTARG::ValueType>(b); }
#define _TES_INTARG_ARITH_OP_SELF(INTARG, OP) \
 inline INTARG::ValueType operator OP(const INTARG &a, const INTARG &b) { return static_cast<INTARG::ValueType>(a) OP static_cast<INTARG::ValueType>(b); }

#define _TES_INTARG_BOOL_OP(INT, INTARG, OP) \
  inline bool operator OP(INT a, const INTARG &b) { return a OP static_cast<INT>(b); } \
  inline bool operator OP(const INTARG &a, INT b) { return static_cast<INT>(a) OP b; }
// #define _TES_INTARG_ARITH_OP(INT, INTARG, OP) \
//  inline INT operator OP(INT a, const INTARG &b) { return a OP static_cast<INT>(b); } \
//  inline INT operator OP(const INTARG &a, INT b) { return static_cast<INT>(a) OP b; }

// Comparison operators for @c IntArg and similar utilities.
#define TES_INTARG_OPERATORS(INT, INTARG) \
  _TES_INTARG_BOOL_OP(INT, INTARG, <) \
  _TES_INTARG_BOOL_OP(INT, INTARG, <=) \
  _TES_INTARG_BOOL_OP(INT, INTARG, >) \
  _TES_INTARG_BOOL_OP(INT, INTARG, >=) \
  _TES_INTARG_BOOL_OP(INT, INTARG, ==) \
  _TES_INTARG_BOOL_OP(INT, INTARG, !=)
  // _TES_INTARG_ARITH_OP(INT, INTARG, +) \
  // _TES_INTARG_ARITH_OP(INT, INTARG, -) \
  // _TES_INTARG_ARITH_OP(INT, INTARG, *) \
  // _TES_INTARG_ARITH_OP(INT, INTARG, /)

#define TES_INTARG_OPERATORS_SELF(INTARG) \
  _TES_INTARG_BOOL_OP_SELF(INTARG, <) \
  _TES_INTARG_BOOL_OP_SELF(INTARG, <=) \
  _TES_INTARG_BOOL_OP_SELF(INTARG, >) \
  _TES_INTARG_BOOL_OP_SELF(INTARG, >=) \
  _TES_INTARG_BOOL_OP_SELF(INTARG, ==) \
  _TES_INTARG_BOOL_OP_SELF(INTARG, !=) \
  _TES_INTARG_ARITH_OP_SELF(INTARG, +) \
  _TES_INTARG_ARITH_OP_SELF(INTARG, -) \
  _TES_INTARG_ARITH_OP_SELF(INTARG, *) \
  _TES_INTARG_ARITH_OP_SELF(INTARG, /)

TES_INTARG_OPERATORS(int, tes::IntArg);
TES_INTARG_OPERATORS_SELF(tes::IntArg);
TES_INTARG_OPERATORS(unsigned, tes::UIntArg);
TES_INTARG_OPERATORS_SELF(tes::UIntArg);
TES_INTARG_OPERATORS(size_t, tes::SizeTArg);
TES_INTARG_OPERATORS_SELF(tes::SizeTArg);

#endif // DOXYGEN_SHOULD_SKIP_THIS

#endif // _3ESINTARG_H_
