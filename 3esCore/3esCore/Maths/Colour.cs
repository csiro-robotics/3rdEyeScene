using System;

namespace Tes.Maths
{
  /// <summary>
  /// Predefined colour references. Index into <see cref="Colour.Colours"/>.
  /// </summary>
  public enum PredefinedColour : int
  {
    // Disable Xml comment warnings.
#pragma warning disable 1591
    // Blacks.
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
    MediumSlateBlue
    // Restore Xml comment warnings.
#pragma warning restore 1591
  }

  /// <summary>
  /// Enumerates the various available colour cycles.
  /// </summary>
  /// <remarks>
  /// Note: the colours cycles include sets which attempt to cater for various
  /// forms of colour blindness. These are not rigorously constructed and may
  /// not be as well suited as they are intended. Feel free to offer suggested
  /// improvements to these colours sets.
  /// </remarks>
  /// <seealso cref="Colour.Cycle(uint, ColourCycle)"/>
  public enum ColourCycle
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

  /// <summary>
  /// A utility class representing a colour as a 32-bit integer.
  /// </summary>
  /// <remarks>
  /// The <see cref="Value"/> is represents a colour channels in the form
  /// 0xRRGGBBAAu. Each channel is accessible via Properties.
  /// </remarks>
  public struct Colour
  {
    /// <summary>
    /// An enumeration of the various colour channels. 
    /// </summary>
    public enum Channel : byte
    {
      /// <summary>
      /// Blue
      /// </summary>
      B,
      /// <summary>
      /// Green
      /// </summary>
      G,
      /// <summary>
      /// Red
      /// </summary>
      R,
      /// <summary>
      /// Alpha
      /// </summary>
      A
    } 

    /// <summary>
    /// The 32-bit colour value in the form: 0xRRGGBBAAu.
    /// </summary>
    public uint Value;

    /// <summary>
    /// Gets or sets the red channel.
    /// </summary>
    /// <value>The red value.</value>
    public byte R
    {
      get { return GetChannel(Channel.R); }
      set { SetChannel(Channel.R, value); }
    }

    /// <summary>
    /// Gets or sets the green channel.
    /// </summary>
    /// <value>The green value.</value>
    public byte G
    {
      get { return GetChannel(Channel.G); }
      set { SetChannel(Channel.G, value); }
    }

    /// <summary>
    /// Gets or sets the blue channel.
    /// </summary>
    /// <value>The blue value.</value>
    public byte B
    {
      get { return GetChannel(Channel.B); }
      set { SetChannel(Channel.B, value); }
    }
  
    /// <summary>
    /// Gets or sets the alpha channel.
    /// </summary>
    /// <value>The alpha value.</value>
    public byte A
    {
      get { return GetChannel(Channel.A); }
      set { SetChannel(Channel.A, value); }
    }

    /// <summary>
    /// Initialises a colour from a 32-bit integer. Simply copies the value.
    /// </summary>
    /// <param name="value">The colour value.</param>
    public Colour(uint value)
    {
      Value = value;
    }

    /// <summary>
    /// Initialises a colour from individual colour values.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    public Colour(int r, int g, int b, int a = 255)
    {
      Value = ((uint)r << GetShift(Channel.R)) | ((uint)g << GetShift(Channel.G)) | ((uint)b << GetShift(Channel.B)) | ((uint)a << GetShift(Channel.A));
    }

    /// <summary>
    /// Returns the bit shift required to access the requested colour channel <paramref name="c"/>.
    /// </summary>
    /// <param name="c">The requested colour channel.</param>
    /// <returns>The bit shift required to move the requested into or out of byte zero.</returns>
    public static int GetShift(Channel c)
    {
      int shift = 0;
      switch (c)
      {
        case Channel.R: shift = 24; break;
        case Channel.G: shift = 16; break;
        case Channel.B: shift = 8; break;
        default:
        case Channel.A: shift = 0; break;
      }

      if (System.BitConverter.IsLittleEndian)
      {
        return shift;
      }

      // Adjust the shift for an Endian swap.
      shift = 24 - shift;
      return shift;
    } 

    /// <summary>
    /// Get the value of the requested colour channel.
    /// </summary>
    /// <param name="c">The requested channel.</param>
    /// <returns>The value of the requested colour channel.</returns>
    public byte GetChannel(Channel c)
    {
      return (byte)((Value >> GetShift(c)) & 0xFFu);
    }

    /// <summary>
    /// Set the value of the indicated colour channel.
    /// </summary>
    /// <param name="c">The requested channel.</param>
    /// <param name="val">The value for the indicates colour channel.</param>
    public void SetChannel(Channel c, byte val)
    {
      int shift = GetShift(c);
      Value &= ~(0xFFu << shift);
      Value |= (uint)val << shift;
    }

    /// <summary>
    /// Return a colour from one of the standard <see cref="ColourCycle"/> sets.
    /// </summary>
    /// <param name="number">An indexing value in the colour cycle. May be out of range as it is wrapped.</param>
    /// <param name="cycle">The colour cycle to use.</param>
    /// <returns>A colour from the selected cycle.</returns>
    /// <remarks>
    /// Using this method it is possible to use a monotonic <paramref name="number"/> as
    /// as indexing value. The value is wrapped to the <paramref name="cycle"/> length,
    /// guaranteeing valid colour from the set.
    /// </remarks>
    public static Colour Cycle(uint number, ColourCycle cycle = ColourCycle.StandardCycle)
    {
      int[] cycleArray = ColourCycles[(int)cycle];
      return Colours[cycleArray[number % cycleArray.Length]];
    }

    /// <summary>
    /// Predefined colour array.
    /// </summary>
    public static readonly Colour[] Colours = new Colour[]
    {
      new Colour(220, 220, 220),
      new Colour(211, 211, 211),
      new Colour(192, 192, 192),
      new Colour(169, 169, 169),
      new Colour(128, 128, 128),
      new Colour(105, 105, 105),
      new Colour(119, 136, 153),
      new Colour(112, 128, 144),
      new Colour(47, 79, 79),
      new Colour(0, 0, 0),
      new Colour(255, 255, 255),
      new Colour(255, 250, 250),
      new Colour(240, 255, 240),
      new Colour(245, 255, 250),
      new Colour(240, 255, 255),
      new Colour(240, 248, 255),
      new Colour(248, 248, 255),
      new Colour(245, 245, 245),
      new Colour(255, 245, 238),
      new Colour(245, 245, 220),
      new Colour(253, 245, 230),
      new Colour(255, 250, 240),
      new Colour(255, 255, 240),
      new Colour(250, 235, 215),
      new Colour(250, 240, 230),
      new Colour(255, 240, 245),
      new Colour(255, 228, 225),
      new Colour(255, 192, 203),
      new Colour(255, 182, 193),
      new Colour(255, 105, 180),
      new Colour(255, 20, 147),
      new Colour(219, 112, 147),
      new Colour(199, 21, 133),
      new Colour(255, 160, 122),
      new Colour(250, 128, 114),
      new Colour(233, 150, 122),
      new Colour(240, 128, 128),
      new Colour(205, 92, 92),
      new Colour(220, 20, 60),
      new Colour(178, 34, 34),
      new Colour(139, 0, 0),
      new Colour(255, 0, 0),
      new Colour(255, 69, 0),
      new Colour(255, 99, 71),
      new Colour(255, 127, 80),
      new Colour(255, 140, 0),
      new Colour(255, 165, 0),
      new Colour(255, 255, 0),
      new Colour(255, 255, 224),
      new Colour(255, 250, 205),
      new Colour(250, 250, 210),
      new Colour(255, 239, 213),
      new Colour(255, 228, 181),
      new Colour(255, 218, 185),
      new Colour(238, 232, 170),
      new Colour(240, 230, 140),
      new Colour(189, 183, 107),
      new Colour(255, 215, 0),
      new Colour(255, 248, 220),
      new Colour(255, 235, 205),
      new Colour(255, 228, 196),
      new Colour(255, 222, 173),
      new Colour(245, 222, 179),
      new Colour(222, 184, 135),
      new Colour(210, 180, 140),
      new Colour(188, 143, 143),
      new Colour(244, 164, 96),
      new Colour(218, 165, 32),
      new Colour(184, 134, 11),
      new Colour(205, 133, 63),
      new Colour(210, 105, 30),
      new Colour(139, 69, 19),
      new Colour(160, 82, 45),
      new Colour(165, 42, 42),
      new Colour(128, 0, 0),
      new Colour(85, 107, 47),
      new Colour(128, 128, 0),
      new Colour(107, 142, 35),
      new Colour(154, 205, 50),
      new Colour(50, 205, 50),
      new Colour(0, 255, 0),
      new Colour(124, 252, 0),
      new Colour(127, 255, 0),
      new Colour(173, 255, 47),
      new Colour(0, 255, 127),
      new Colour(0, 250, 154),
      new Colour(144, 238, 144),
      new Colour(152, 251, 152),
      new Colour(143, 188, 143),
      new Colour(60, 179, 113),
      new Colour(46, 139, 87),
      new Colour(34, 139, 34),
      new Colour(0, 128, 0),
      new Colour(0, 100, 0),
      new Colour(102, 205, 170),
      new Colour(0, 255, 255),
      new Colour(0, 255, 255),
      new Colour(224, 255, 255),
      new Colour(175, 238, 238),
      new Colour(127, 255, 212),
      new Colour(64, 224, 208),
      new Colour(72, 209, 204),
      new Colour(0, 206, 209),
      new Colour(32, 178, 170),
      new Colour(95, 158, 160),
      new Colour(0, 139, 139),
      new Colour(0, 128, 128),
      new Colour(176, 196, 222),
      new Colour(176, 224, 230),
      new Colour(173, 216, 230),
      new Colour(135, 206, 235),
      new Colour(135, 206, 250),
      new Colour(0, 191, 255),
      new Colour(30, 144, 255),
      new Colour(100, 149, 237),
      new Colour(70, 130, 180),
      new Colour(65, 105, 225),
      new Colour(0, 0, 255),
      new Colour(0, 0, 205),
      new Colour(0, 0, 139),
      new Colour(0, 0, 128),
      new Colour(25, 25, 112),
      new Colour(230, 230, 250),
      new Colour(216, 191, 216),
      new Colour(221, 160, 221),
      new Colour(238, 130, 238),
      new Colour(218, 112, 214),
      new Colour(255, 0, 255),
      new Colour(255, 0, 255),
      new Colour(186, 85, 211),
      new Colour(147, 112, 219),
      new Colour(138, 43, 226),
      new Colour(148, 0, 211),
      new Colour(153, 50, 204),
      new Colour(139, 0, 139),
      new Colour(128, 0, 128),
      new Colour(75, 0, 130),
      new Colour(72, 61, 139),
      new Colour(106, 90, 205),
      new Colour(123, 104, 238)
    };

    /// <summary>
    /// A predefined set of colours which attempts to distinguish consecutive colours.
    /// </summary>
    public static readonly int[] DefaultColourSet = new int[]
    {
      (int)PredefinedColour.Red,
      (int)PredefinedColour.Green,
      (int)PredefinedColour.Blue,
      (int)PredefinedColour.MediumOrchid,
      (int)PredefinedColour.Olive,
      (int)PredefinedColour.Teal,
      (int)PredefinedColour.Black,
      (int)PredefinedColour.OrangeRed,
      (int)PredefinedColour.Yellow,
      (int)PredefinedColour.MediumAquamarine,
      (int)PredefinedColour.Gainsboro,
      (int)PredefinedColour.White,
      (int)PredefinedColour.Pink,
      (int)PredefinedColour.LightSalmon,
      (int)PredefinedColour.Tomato,
      (int)PredefinedColour.DarkOliveGreen,
      (int)PredefinedColour.Aqua,
      (int)PredefinedColour.LightSteelBlue,
      (int)PredefinedColour.Silver,
      (int)PredefinedColour.HotPink,
      (int)PredefinedColour.Salmon,
      (int)PredefinedColour.Coral,
      (int)PredefinedColour.Wheat,
      (int)PredefinedColour.Olive,
      (int)PredefinedColour.PowderBlue,
      (int)PredefinedColour.Thistle,
      (int)PredefinedColour.DarkGrey,
      (int)PredefinedColour.DeepPink,
      (int)PredefinedColour.DarkSalmon,
      (int)PredefinedColour.DarkOrange,
      (int)PredefinedColour.Moccasin,
      (int)PredefinedColour.BurlyWood,
      (int)PredefinedColour.OliveDrab,
      (int)PredefinedColour.Aquamarine,
      (int)PredefinedColour.LightBlue,
      (int)PredefinedColour.Plum,
      (int)PredefinedColour.DimGrey,
      (int)PredefinedColour.PaleVioletRed,
      (int)PredefinedColour.LightCoral,
      (int)PredefinedColour.Orange,
      (int)PredefinedColour.PeachPuff,
      (int)PredefinedColour.Tan,
      (int)PredefinedColour.YellowGreen,
      (int)PredefinedColour.Turquoise,
      (int)PredefinedColour.SkyBlue,
      (int)PredefinedColour.Violet,
      (int)PredefinedColour.SlateGrey,
      (int)PredefinedColour.MediumVioletRed,
      (int)PredefinedColour.IndianRed,
      (int)PredefinedColour.RosyBrown,
      (int)PredefinedColour.LimeGreen,
      (int)PredefinedColour.MediumTurquoise,
      (int)PredefinedColour.DeepSkyBlue,
      (int)PredefinedColour.Orchid,
      (int)PredefinedColour.DarkSlateGrey,
      (int)PredefinedColour.Crimson,
      (int)PredefinedColour.Khaki,
      (int)PredefinedColour.SandyBrown,
      (int)PredefinedColour.Lime,
      (int)PredefinedColour.DarkTurquoise,
      (int)PredefinedColour.CornflowerBlue,
      (int)PredefinedColour.Fuchsia,
      (int)PredefinedColour.FireBrick,
      (int)PredefinedColour.DarkKhaki,
      (int)PredefinedColour.DarkGoldenrod,
      (int)PredefinedColour.LawnGreen,
      (int)PredefinedColour.LightSeaGreen,
      (int)PredefinedColour.SteelBlue,
      (int)PredefinedColour.MediumPurple,
      (int)PredefinedColour.DarkRed,
      (int)PredefinedColour.Gold,
      (int)PredefinedColour.Peru,
      (int)PredefinedColour.MediumSpringGreen,
      (int)PredefinedColour.CadetBlue,
      (int)PredefinedColour.RoyalBlue,
      (int)PredefinedColour.BlueViolet,
      (int)PredefinedColour.Chocolate,
      (int)PredefinedColour.LightGreen,
      (int)PredefinedColour.DarkCyan,
      (int)PredefinedColour.DarkBlue,
      (int)PredefinedColour.DarkViolet,
      (int)PredefinedColour.SaddleBrown,
      (int)PredefinedColour.DarkSeaGreen,
      (int)PredefinedColour.MidnightBlue,
      (int)PredefinedColour.Purple,
      (int)PredefinedColour.Sienna,
      (int)PredefinedColour.MediumSeaGreen,
      (int)PredefinedColour.Indigo,
      (int)PredefinedColour.Brown,
      (int)PredefinedColour.SeaGreen,
      (int)PredefinedColour.DarkSlateBlue,
      (int)PredefinedColour.Maroon,
      (int)PredefinedColour.DarkGreen,
      (int)PredefinedColour.SlateBlue
    };

    /// <summary>
    /// A colour set which attempts to be deuteranomoly colour blind friendly.
    /// </summary>
    public static readonly int[] DeuteranomalyColourSet = new int[]
    {
      (int)PredefinedColour.RoyalBlue,
      (int)PredefinedColour.Yellow,
      (int)PredefinedColour.Silver,
      (int)PredefinedColour.Black,
      (int)PredefinedColour.Blue,
      (int)PredefinedColour.Khaki,
      (int)PredefinedColour.Gainsboro,
      (int)PredefinedColour.Beige,
      (int)PredefinedColour.Navy,
      (int)PredefinedColour.DarkKhaki,
      (int)PredefinedColour.White,
      (int)PredefinedColour.Grey,
      (int)PredefinedColour.MidnightBlue,
      (int)PredefinedColour.SlateGrey,
      (int)PredefinedColour.Ivory,
      (int)PredefinedColour.Gold,
      (int)PredefinedColour.DarkSlateBlue,
      (int)PredefinedColour.MediumSlateBlue
    };

    /// <summary>
    /// A colour set which attempts to be protanomoly colour blind friendly.
    /// </summary>
    public static readonly int[] ProtanomalyColourSet = new int[]
    {
      (int)PredefinedColour.Blue,
      (int)PredefinedColour.Yellow,
      (int)PredefinedColour.Black,
      (int)PredefinedColour.Silver,
      (int)PredefinedColour.CornflowerBlue,
      (int)PredefinedColour.Gainsboro,
      (int)PredefinedColour.MediumSlateBlue,
      (int)PredefinedColour.Khaki,
      (int)PredefinedColour.Grey,
      (int)PredefinedColour.DarkBlue,
      (int)PredefinedColour.Beige,
      (int)PredefinedColour.DarkKhaki,
      (int)PredefinedColour.MidnightBlue,
      (int)PredefinedColour.SlateGrey,
      (int)PredefinedColour.RoyalBlue,
      (int)PredefinedColour.Ivory,
      (int)PredefinedColour.DarkSlateBlue,
    };

    /// <summary>
    /// A colour set which attempts to be trianomoly colour blind friendly.
    /// </summary>
    public static readonly int[] TritanomalyColourSet = new int[]
    {
      (int)PredefinedColour.DeepSkyBlue,
      (int)PredefinedColour.DeepPink,
      (int)PredefinedColour.PaleTurquoise,
      (int)PredefinedColour.Black,
      (int)PredefinedColour.Crimson,
      (int)PredefinedColour.LightSeaGreen,
      (int)PredefinedColour.Gainsboro,
      (int)PredefinedColour.Blue,
      (int)PredefinedColour.DarkRed,
      (int)PredefinedColour.Silver,
      (int)PredefinedColour.Brown,
      (int)PredefinedColour.DarkTurquoise,
      (int)PredefinedColour.Grey,
      (int)PredefinedColour.Maroon,
      (int)PredefinedColour.Teal,
      (int)PredefinedColour.SlateGrey,
      (int)PredefinedColour.MidnightBlue,
      (int)PredefinedColour.DarkSlateGrey,
    };

    /// <summary>
    /// A greyscale colour set.
    /// </summary>
    public static readonly int[] GreyColourSet = new int[]
    {
      (int)PredefinedColour.Black,
      (int)PredefinedColour.Silver,
      (int)PredefinedColour.DarkSlateGrey,
      (int)PredefinedColour.Grey,
      (int)PredefinedColour.Gainsboro,
      (int)PredefinedColour.SlateGrey,
    };

    /// <summary>
    /// Encapsulates the various standard colour cycles into an array.
    /// </summary>
    public static readonly int[][] ColourCycles = new int[][]
    {
      DefaultColourSet,
      DeuteranomalyColourSet,
      ProtanomalyColourSet,
      TritanomalyColourSet,
      GreyColourSet
    };
  }
}
