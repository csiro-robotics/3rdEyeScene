using System;

namespace Tes.Maths
{
  /// <summary>
  /// Floating point number utility functions.
  /// </summary>
  public static class FloatUtil
  {
    /// <summary>
    /// Bitwise conversion of a single precision floating point value to a 32-bit integer.
    /// </summary>
    /// <returns><paramref name="val"/> as an integer where the bits remain unchanged.</returns>
    /// <param name="val">The floating point value to convert.</param>
    public static unsafe int ToInt32Bits(float val)
    {
      return *(int*)&val;
    }

  
    /// <summary>
    /// Bitwise conversion of a 32-bit integer to a single precision floating point value.
    /// </summary>
    /// <returns><paramref name="val"/> as a float where the bits remain unchanged.</returns>
    /// <param name="val">The integer value to convert.</param>
    public static unsafe float FromInt32Bits(int val)
    {
      return *(float*)&val;
    }
  }
}
