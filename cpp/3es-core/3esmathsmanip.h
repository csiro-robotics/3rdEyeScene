// Copyright (c) 2018
// Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// ABN 41 687 119 230
//
// Author: Kazys Stepanas
#ifndef _3ESMATHSMANIP_H_
#define _3ESMATHSMANIP_H_

#include "3es-core.h"

#include <iostream>
#include <iomanip>

// IO stream manipulators supporting maths type streaming.

namespace tes
{
  /// @ingroup tesiostream
  /// Accepted values for @c ::matmode.
  enum MatMode
  {
    /// Display all matrix elements inline.
    MM_Inline,
    /// Insert newlines after every row.
    MM_Block
  };

  /// @ingroup tesiostream
  /// Display mode W component in @c Vector4 and @c Quaternion types.
  enum WMode
  {
    /// W is displayed last to match memory layout (default).
    WM_Last,
    /// W component is displayed first.
    WM_First
  };

  int _3es_coreAPI getMatMode(std::ostream &o);
  int _3es_coreAPI getQuatWMode(std::ostream &o);
  int _3es_coreAPI getV4WMode(std::ostream &o);
}

/// @ingroup tesiostream
/// Set the @c tes::MatMode for a stream affecting @c tes::Matrix3 and @c tes::Matrix4 output.
/// @param o The stream to set the mode for.
/// @param mode The mode to set. See @c tes::MatMode
/// @return @c o
std::ostream & _3es_coreAPI matmode(std::ostream &o, int mode);
/// @ingroup tesiostream
/// Set the @c tes::WMode used to display @c tes::Vector4 in a stream.
/// @param o The stream to set the mode for.
/// @param mode The mode to set. See @c tes::WMode
/// @return @c o
std::ostream & _3es_coreAPI v4wmode(std::ostream &o, int mode);
/// @ingroup tesiostream
/// Set the @c tes::WMode used to display @c tes::Quaternion in a stream.
/// @param o The stream to set the mode for.
/// @param mode The mode to set. See @c tes::WMode
/// @return @c o
std::ostream & _3es_coreAPI quatwmode(std::ostream &o, int mode);

#endif // _3ESMATHSMANIP_H_
