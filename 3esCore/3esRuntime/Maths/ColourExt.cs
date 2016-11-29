using System;

namespace Tes.Maths
{
  /// <summary>
  /// Unity/TES colour conversion.
  /// </summary>
  public static class ColourExt
  {
    /// <summary>
    /// Convert to a Unity colour.
    /// </summary>
    /// <returns>The equivalent Unity colour.</returns>
    /// <param name="c">The colour to convert.</param>
    public static UnityEngine.Color32 ToUnity32(this Colour c)
    {
      return new UnityEngine.Color32(c.R, c.G, c.B, c.A);
    }

    /// <summary>
    /// Set from a Unity colour.
    /// </summary>
    /// <param name="c">The TES colour to modify.</param>
    /// <param name="uc">The Unity colour to read colour values from..</param>
    public static void Set(this Colour c, UnityEngine.Color32 uc)
    {
      c.R = uc.r;
      c.G = uc.g;
      c.B = uc.b;
      c.A = uc.a;
    }

    /// <summary>
    /// Convert to a Unity colour.
    /// </summary>
    /// <returns>The equivalent Unity colour.</returns>
    /// <param name="c">The colour to convert.</param>
    public static UnityEngine.Color ToUnity(this Colour c)
    {
      const float div = 1.0f / 255.0f;
      return new UnityEngine.Color((float)c.R * div, (float)c.G * div, (float)c.B * div, (float)c.A * div);
    }

    /// <summary>
    /// Set from a Unity colour.
    /// </summary>
    /// <returns>The TES equivalent colour.</returns>
    /// <param name="uc">The Unity colour to read colour values from..</param>
    public static Colour FromUnity(UnityEngine.Color uc)
    {
      return new Colour((byte)(uc.r * 255.0f), (byte)(uc.g * 255.0f), (byte)(uc.b * 255.0f), (byte)(uc.a * 255.0f));
    }
  }
}
