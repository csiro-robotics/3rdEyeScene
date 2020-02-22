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
    public static int NextPowerOf2(uint val)
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

    /// <summary>
    /// A utility function calculating the next power of 2 greater than or equal to <paramref name="val"/>.
    /// </summary>
    /// <param name="val">The base value</param>
    /// <returns>The next power of 2 greater than or equal to <paramref name="val"/></returns>
    public static int NextPowerOf2(int val)
    {
      return (int)NextPowerOf2((uint)val);
    }

    public static int ToBitIndex(uint v)
    {
      // TODO: (KS) lookup a bit twiddling hack for this.
      uint bitValue = 1u;
      for (int i = 0; i < 32; ++i, bitValue = bitValue << 1)
      {
        if ((v & bitValue) != 0)
        {
          return i;
        }
      }

      return -1;
    }

    public static int ToBitIndex(int v)
    {
      return ToBitIndex((uint)v);
    }
  }
}
