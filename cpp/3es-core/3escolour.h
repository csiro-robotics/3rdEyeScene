//
// author Kazys Stepanas
//
// Copyright (c) Kazys Stepanas 2014
//
#ifndef _3ESCOLOUR_H_
#define _3ESCOLOUR_H_

#include "3es-core.h"

#include <cstdint>

namespace tes
{
  /// A 32-bit integer colour class.
  ///
  /// Storage is designed to allow colours to be written as unsigned
  /// hexadecimal integers as 0xRRGGBBAA regardless of the target Endian.
  class _3es_coreAPI Colour
  {
  public:
    /// Channel index enumeration.
    enum Channels
    {
#if TES_IS_BIG_ENDIAN
      R = 0,
      G = 1,
      B = 2,
      A = 3
#else  // TES_IS_BIG_ENDIAN
      A = 0,  ///< Alpha channel index.
      B = 1,  ///< Blue channel index.
      G = 2,  ///< Green channel index.
      R = 3   ///< Red channel index.
#endif // TES_IS_BIG_ENDIAN
    };
    union
    {
      uint32_t c; ///< Encoded colour value.

      struct
      {
#if TES_IS_BIG_ENDIAN
        uint8_t r;
        uint8_t g;
        uint8_t b;
        uint8_t a;
#else  // TES_IS_BIG_ENDIAN
        uint8_t a;  ///< Alpha channel.
        uint8_t b;  ///< Blue channel.
        uint8_t g;  ///< Green channel.
        uint8_t r;  ///< Red channel.
#endif // TES_IS_BIG_ENDIAN
      };
      uint8_t rgba[4];  ///< Indexed channels.
    };

    /// Construct a colour with the given numeric value.
    /// @param c The integer colour representation: 0xRRGGBBAA.
    Colour(uint32_t c = 0xffffffffu);

    /// Copy constructor.
    /// @param other The colour to copy.
    Colour(const Colour &other);

    /// Partial copy constructor with new alpha value.
    /// @param other The colour to copy RGB channels from.
    /// @param a The new alpha channel value.
    Colour(const Colour &other, uint8_t a);
    /// Partial copy constructor with new alpha value.
    /// @param other The colour to copy RGB channels from.
    /// @param a The new alpha channel value.
    Colour(const Colour &other, int a);
    /// Partial copy constructor with new alpha value.
    /// @param other The colour to copy RGB channels from.
    /// @param a The new alpha channel value.
    Colour(const Colour &other, float a);

    /// Explicit byte based RGBA colour channel initialisation constructor.
    /// @param r Red channel value [0, 255].
    /// @param g Red channel value [0, 255].
    /// @param b Red channel value [0, 255].
    /// @param a Red channel value [0, 255].
    explicit Colour(uint8_t r, uint8_t g, uint8_t b, uint8_t a = 255u);

    /// Integer based RGBA colour channel initialisation constructor.
    /// @param r Red channel value [0, 255].
    /// @param g Red channel value [0, 255].
    /// @param b Red channel value [0, 255].
    /// @param a Red channel value [0, 255].
    Colour(int r, int g, int b, int a = 255);

    /// Floating point RGBA colour channel initialisation constructor.
    /// @param r Red channel value [0, 1].
    /// @param g Red channel value [0, 1].
    /// @param b Red channel value [0, 1].
    /// @param a Red channel value [0, 1].
    Colour(float r, float g, float b, float a = 1.0f);

    /// Get red channel in floating point form.
    /// @return Red channel [0, 1].
    float rf() const;
    /// Get green channel in floating point form.
    /// @return Green channel [0, 1].
    float gf() const;
    /// Get blue channel in floating point form.
    /// @return Blue channel [0, 1].
    float bf() const;
    /// Get alpha channel in floating point form.
    /// @return Alpha channel [0, 1].
    float af() const;

    /// Set red channel from a floating point value.
    /// @param f Channel value [0, 1].
    void setRf(float f);
    /// Set green channel from a floating point value.
    /// @param f Channel value [0, 1].
    void setGf(float f);
    /// Set blue channel from a floating point value.
    /// @param f Channel value [0, 1].
    void setBf(float f);
    /// Set alpha channel from a floating point value.
    /// @param f Channel value [0, 1].
    void setAf(float f);

    /// Set a channel in floating point form.
    /// @param f Channel value [0, 1].
    /// @param index The target channel [0, 3]. Best to use @c Channels.
    void setf(float f, int index);
    /// Get a channel in floating point form.
    /// @param index The target channel [0, 3]. Best to use @c Channels.
    /// @return The channel value [0, 1].
    float getf(int index) const;

    /// Lighten or darken a colour by @p factor.
    /// Works in HSV space, multiplying the V value by @p factor and clamping the result [0, 1].
    /// @return The adjusted colour.
    Colour adjust(float factor) const;

    /// Lighten the colour by 1.5
    /// @return A lighter colour.
    inline Colour lighten() const { return adjust(1.5f); }

    /// Darken the colour by 0.5
    /// @return A darker colour.
    inline Colour darken() const { return adjust(0.5f); }

    /// Convert RGB to HSV form.
    /// @param[out] h The hue value [0, 360].
    /// @param[out] s The saturation value [0, 1].
    /// @param[out] v The colour value [0, 1].
    /// @param r Red channel.
    /// @param g Green channel.
    /// @param b Blue channel.
    static void rgbToHsv(float &h, float &s, float &v, const float r, const float g, const float b);

    /// Convert HSV to RGB form.
    /// @param[out] r Red channel [0, 1].
    /// @param[out] g Green channel [0, 1].
    /// @param[out] b Blue channel [0, 1].
    /// @param h The hue value [0, 360].
    /// @param s The saturation value [0, 1].
    /// @param v The colour value [0, 1].
    static void hsvToRgb(float &r, float &g, float &b, const float h, const float s, const float v);
    /// Convert HSV to RGB form.
    /// @param[out] r Red channel [0, 255].
    /// @param[out] g Green channel [0, 255].
    /// @param[out] b Blue channel [0, 255].
    /// @param h The hue value [0, 360].
    /// @param s The saturation value [0, 1].
    /// @param v The colour value [0, 1].
    static void hsvToRgb(uint8_t &r, uint8_t &g, uint8_t &b,
                         const float h, const float s, const float v);

    /// Assignment operator.
    /// @param other The colour value to assign.
    /// @return @c this.
    Colour &operator=(const Colour &other);

    //inline operator uint32_t() const { return c; }

    /// Precise equality operator.
    /// @param other The colour to compare to.
    /// @return True if this colour is precisely equal to @p other.
    bool operator==(const Colour &other) const;
    /// Precise inequality operator.
    /// @param other The colour to compare to.
    /// @return True if this colour is not precisely equal to @p other.
    bool operator!=(const Colour &other) const;

    /// Enumerates a set of predefined colours ("web safe" colours).
    enum Predefined
    {
      // Greys and blacks.
      Gainsboro,
      LightGrey,
      Silver,
      DarkGrey,
      Grey,
      DimGrey,
      LightSlateGrey,
      SlateGrey,
      DarkSlateGrey,
      Black,

      // Whites
      White,
      Snow,
      Honeydew,
      MintCream,
      Azure,
      AliceBlue,
      GhostWhite,
      WhiteSmoke,
      Seashell,
      Beige,
      OldLace,
      FloralWhite,
      Ivory,
      AntiqueWhite,
      Linen,
      LavenderBlush,
      MistyRose,

      // Pinks
      Pink,
      LightPink,
      HotPink,
      DeepPink,
      PaleVioletRed,
      MediumVioletRed,

      // Reds
      LightSalmon,
      Salmon,
      DarkSalmon,
      LightCoral,
      IndianRed,
      Crimson,
      FireBrick,
      DarkRed,
      Red,

      // Oranges
      OrangeRed,
      Tomato,
      Coral,
      DarkOrange,
      Orange,

      // Yellows
      Yellow,
      LightYellow,
      LemonChiffon,
      LightGoldenrodYellow,
      PapayaWhip,
      Moccasin,
      PeachPuff,
      PaleGoldenrod,
      Khaki,
      DarkKhaki,
      Gold,

      // Browns
      Cornsilk,
      BlanchedAlmond,
      Bisque,
      NavajoWhite,
      Wheat,
      BurlyWood,
      Tan,
      RosyBrown,
      SandyBrown,
      Goldenrod,
      DarkGoldenrod,
      Peru,
      Chocolate,
      SaddleBrown,
      Sienna,
      Brown,
      Maroon,

      // Greens
      DarkOliveGreen,
      Olive,
      OliveDrab,
      YellowGreen,
      LimeGreen,
      Lime,
      LawnGreen,
      Chartreuse,
      GreenYellow,
      SpringGreen,
      MediumSpringGreen,
      LightGreen,
      PaleGreen,
      DarkSeaGreen,
      MediumSeaGreen,
      SeaGreen,
      ForestGreen,
      Green,
      DarkGreen,

      // Cyans
      MediumAquamarine,
      Aqua,
      Cyan,
      LightCyan,
      PaleTurquoise,
      Aquamarine,
      Turquoise,
      MediumTurquoise,
      DarkTurquoise,
      LightSeaGreen,
      CadetBlue,
      DarkCyan,
      Teal,

      // Blues
      LightSteelBlue,
      PowderBlue,
      LightBlue,
      SkyBlue,
      LightSkyBlue,
      DeepSkyBlue,
      DodgerBlue,
      CornflowerBlue,
      SteelBlue,
      RoyalBlue,
      Blue,
      MediumBlue,
      DarkBlue,
      Navy,
      MidnightBlue,

      // Purples
      Lavender,
      Thistle,
      Plum,
      Violet,
      Orchid,
      Fuchsia,
      Magenta,
      MediumOrchid,
      MediumPurple,
      BlueViolet,
      DarkViolet,
      DarkOrchid,
      DarkMagenta,
      Purple,
      Indigo,
      DarkSlateBlue,
      SlateBlue,
      MediumSlateBlue,

      PredefinedCount
    };

    /// Enumerates the various available colour cycles.
    ///
    /// Note: the colours cycles include sets which attempt to cater for various
    /// forms of colour blindness. These are not rigorously constructed and may
    /// not be as well suited as they are intended. Feel free to offer suggested
    /// improvements to these colours sets.
    ///
    /// @see @c colourCycle()
    enum ColourCycle
    {
      /// Standard colour set.
      StandardCycle,
      /// A colour set which attempts to cater for Deuteranomaly colour blindness.
      DeuteranomalyCycle,
      /// A colour set which attempts to cater for Protanomaly colour blindness.
      ProtanomalyCycle,
      /// A colour set which attempts to cater for Tritanomaly colour blindness.
      TritanomalyCycle,
      /// A small grey scale colour set.
      GreyCycle,
      /// Defines the number of available colour sets.
      CycleCount
    };

    /// The set of colours matching the @c Predefined enumeration.
    static const Colour Colours[PredefinedCount];

    /// A set of colours which can be cycled to highlight components.
    ///
    /// The colour set is initialised to try and provide sufficient contrast between
    /// each colour. Each element should be indexed into @p Colours.
    static const int *ColourCycles[CycleCount];

    /// Number of colours in each @p ColourCycles entry.
    static const int CycleCounts[CycleCount];

    /// A utility function for colour cycling.
    ///
    /// This function returns a colour from the @c ColourCycle, resolved to its actual
    /// @c Colours element. The intended use is to periodically call this function with a
    /// monotonic, increasing @p number value. This is then correctly clamped to the
    /// @c ColourCycle range.
    ///
    /// @param number Any numeric value. It is clamped and wrapped to be in range (using modulus).
    /// @param cycle The colour cycle to request a colour from.
    /// @return A colour from the cycle.
    static const Colour &cycle(unsigned number, ColourCycle cycle = StandardCycle);
  };


  inline Colour::Colour(uint32_t c)
    : c(c)
  {
  }


  inline Colour::Colour(const Colour &other)
    : c(other.c)
  {
  }


  inline Colour::Colour(const Colour &other, uint8_t a)
    : c(other.c)
  {
    this->a = a;
  }


  inline Colour::Colour(const Colour &other, int a)
    : c(other.c)
  {
    this->a = a;
  }


  inline Colour::Colour(const Colour &other, float a)
    : c(other.c)
  {
    setAf(a);
  }


  inline Colour::Colour(uint8_t r, uint8_t g, uint8_t b, uint8_t a)
    : r(r), g(g), b(b), a(a)
  {
  }


  inline Colour::Colour(int r, int g, int b, int a)
    : r(uint8_t(r))
    , g(uint8_t(g))
    , b(uint8_t(b))
    , a(uint8_t(a))
  {
  }


  inline Colour::Colour(float r, float g, float b, float a)
  {
    setRf(r);
    setGf(g);
    setBf(b);
    setAf(a);
  }

  inline float Colour::rf() const
  {
    return getf(0);
  }


  inline float Colour::gf() const
  {
    return getf(1);
  }


  inline float Colour::bf() const
  {
    return getf(2);
  }


  inline float Colour::af() const
  {
    return getf(3);
  }


  inline void Colour::setRf(float f)
  {
    setf(f, 0);
  }


  inline void Colour::setGf(float f)
  {
    setf(f, 1);
  }


  inline void Colour::setBf(float f)
  {
    setf(f, 2);
  }


  inline void Colour::setAf(float f)
  {
    setf(f, 3);
  }


  inline void Colour::setf(float f, int index)
  {
    rgba[index] = uint8_t(f * 255.0f);
  }


  inline float Colour::getf(int index) const
  {
    return rgba[index] / 255.0f;
  }


  inline Colour &Colour::operator=(const Colour &other)
  {
    c = other.c;
    return *this;
  }


  inline bool Colour::operator==(const Colour &other) const
  {
    return c == other.c;
  }


  inline bool Colour::operator!=(const Colour &other) const
  {
    return c != other.c;
  }
}

#endif // _3ESCOLOUR_H_
