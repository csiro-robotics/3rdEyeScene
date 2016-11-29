using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tes.Maths
{
  /// <summary>
  /// Integer utility functions.
  /// </summary>
  public static class IntUtil
  {
    /// <summary>
    /// A utility function calculating the next power of 2 greater than or equal to <paramref name="val"/>.
    /// </summary>
    /// <param name="val">The base value</param>
    /// <returns>The next power of 2 greater than or equal to <paramref name="val"/></returns>
    public static int NextPowerOf2(int val)
    {
      uint x = (uint)val;
      x--; // comment out to always take the next biggest power of two, even if x is already a power of two
      x |= (x >> 1);
      x |= (x >> 2);
      x |= (x >> 4);
      x |= (x >> 8);
      x |= (x >> 16);
      return (int)(x + 1);
    }
  }
}
