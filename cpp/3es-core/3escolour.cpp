// 
// author Kazys Stepanas 
// 
// Copyright (c) Kazys Stepanas 2014 
// 
#include "3escolour.h" 

#include <algorithm>

using namespace tes;

Colour Colour::adjust(float factor) const
{
  float h, s, v;
  Colour c;
  rgbToHsv(h, s, v, rf(), gf(), bf());
  v = std::max(0.0f, std::min(v, 1.0f));
  c.a = this->a;
  hsvToRgb(c.r, c.g, c.b, h, s, v);
  return c;
}


void Colour::rgbToHsv(float &h, float &s, float &v,
                      const float r, const float g, const float b)
{
  const float cmin = std::min<float>(r, std::min<float>(g, b));
  const float cmax = std::max<float>(r, std::max<float>(g, b));
  const float delta = cmax - cmin;

  const float yellowToMagenta = (r == cmax && cmax) ? 1.0f : 0.0f;
  const float cyanToYellow = (g == cmax && cmax) ? 1.0f : 0.0f;
  const float magentaToCyan = (b == cmax && cmax) ? 1.0f : 0.0f;

  v = cmax;
  s = (cmax) ? delta / cmax : 0;
  h = (yellowToMagenta * ((g - b) / delta) +
    cyanToYellow * ((g - b) / delta) +
    magentaToCyan * ((g - b) / delta)) * 60.0f;
}


void Colour::hsvToRgb(float &r, float &g, float &b,
                      const float h, const float s, const float v)
{
  const float hSector = h / 60.0f; // sector 0 to 5
  const int sectorIndex = int(std::min<float>(std::max<float>(0.0f, std::floor(hSector)), 5.0f));
  const float f = hSector - sectorIndex;
  const float p = v * (1 - s);
  const float q = v * (1 - s * f);
  const float t = v * (1 - s * (1 - f));

  static const int vindex[] = { 0, 1, 1, 2, 2, 0 };
  static const int pindex[] = { 2, 2, 0, 0, 1, 1 };
  static const int qindex[] = { 3, 0, 3, 1, 3, 2 };
  static const int tindex[] = { 1, 3, 2, 3, 0, 3 };

  float rgb[4];
  rgb[vindex[sectorIndex]] = v;
  rgb[pindex[sectorIndex]] = p;
  rgb[qindex[sectorIndex]] = q;
  rgb[tindex[sectorIndex]] = t;

  // Handle achromatic here by testing s inline.
  r = (s) ? rgb[0] : v;
  g = (s) ? rgb[1] : v;
  b = (s) ? rgb[2] : v;
}


void Colour::hsvToRgb(uint8_t &r, uint8_t &g, uint8_t &b,
                      const float h, const float s, const float v)
{
  float rf, gf, bf;
  hsvToRgb(rf, gf, bf, h, s, v);
  r = (uint8_t)(rf * 255.0f);
  g = (uint8_t)(gf * 255.0f);
  b = (uint8_t)(bf * 255.0f);
}


const Colour &Colour::cycle(unsigned number, ColourCycle cycle)
{
  return Colours[ColourCycles[cycle][number % CycleCounts[cycle]]];
}


const Colour Colour::Colours[PredefinedCount] =
{
  Colour(220, 220, 220),
  Colour(211, 211, 211),
  Colour(192, 192, 192),
  Colour(169, 169, 169),
  Colour(128, 128, 128),
  Colour(105, 105, 105),
  Colour(119, 136, 153),
  Colour(112, 128, 144),
  Colour(47, 79, 79),
  Colour(0, 0, 0),
  Colour(255, 255, 255),
  Colour(255, 250, 250),
  Colour(240, 255, 240),
  Colour(245, 255, 250),
  Colour(240, 255, 255),
  Colour(240, 248, 255),
  Colour(248, 248, 255),
  Colour(245, 245, 245),
  Colour(255, 245, 238),
  Colour(245, 245, 220),
  Colour(253, 245, 230),
  Colour(255, 250, 240),
  Colour(255, 255, 240),
  Colour(250, 235, 215),
  Colour(250, 240, 230),
  Colour(255, 240, 245),
  Colour(255, 228, 225),
  Colour(255, 192, 203),
  Colour(255, 182, 193),
  Colour(255, 105, 180),
  Colour(255, 20, 147),
  Colour(219, 112, 147),
  Colour(199, 21, 133),
  Colour(255, 160, 122),
  Colour(250, 128, 114),
  Colour(233, 150, 122),
  Colour(240, 128, 128),
  Colour(205, 92, 92),
  Colour(220, 20, 60),
  Colour(178, 34, 34),
  Colour(139, 0, 0),
  Colour(255, 0, 0),
  Colour(255, 69, 0),
  Colour(255, 99, 71),
  Colour(255, 127, 80),
  Colour(255, 140, 0),
  Colour(255, 165, 0),
  Colour(255, 255, 0),
  Colour(255, 255, 224),
  Colour(255, 250, 205),
  Colour(250, 250, 210),
  Colour(255, 239, 213),
  Colour(255, 228, 181),
  Colour(255, 218, 185),
  Colour(238, 232, 170),
  Colour(240, 230, 140),
  Colour(189, 183, 107),
  Colour(255, 215, 0),
  Colour(255, 248, 220),
  Colour(255, 235, 205),
  Colour(255, 228, 196),
  Colour(255, 222, 173),
  Colour(245, 222, 179),
  Colour(222, 184, 135),
  Colour(210, 180, 140),
  Colour(188, 143, 143),
  Colour(244, 164, 96),
  Colour(218, 165, 32),
  Colour(184, 134, 11),
  Colour(205, 133, 63),
  Colour(210, 105, 30),
  Colour(139, 69, 19),
  Colour(160, 82, 45),
  Colour(165, 42, 42),
  Colour(128, 0, 0),
  Colour(85, 107, 47),
  Colour(128, 128, 0),
  Colour(107, 142, 35),
  Colour(154, 205, 50),
  Colour(50, 205, 50),
  Colour(0, 255, 0),
  Colour(124, 252, 0),
  Colour(127, 255, 0),
  Colour(173, 255, 47),
  Colour(0, 255, 127),
  Colour(0, 250, 154),
  Colour(144, 238, 144),
  Colour(152, 251, 152),
  Colour(143, 188, 143),
  Colour(60, 179, 113),
  Colour(46, 139, 87),
  Colour(34, 139, 34),
  Colour(0, 128, 0),
  Colour(0, 100, 0),
  Colour(102, 205, 170),
  Colour(0, 255, 255),
  Colour(0, 255, 255),
  Colour(224, 255, 255),
  Colour(175, 238, 238),
  Colour(127, 255, 212),
  Colour(64, 224, 208),
  Colour(72, 209, 204),
  Colour(0, 206, 209),
  Colour(32, 178, 170),
  Colour(95, 158, 160),
  Colour(0, 139, 139),
  Colour(0, 128, 128),
  Colour(176, 196, 222),
  Colour(176, 224, 230),
  Colour(173, 216, 230),
  Colour(135, 206, 235),
  Colour(135, 206, 250),
  Colour(0, 191, 255),
  Colour(30, 144, 255),
  Colour(100, 149, 237),
  Colour(70, 130, 180),
  Colour(65, 105, 225),
  Colour(0, 0, 255),
  Colour(0, 0, 205),
  Colour(0, 0, 139),
  Colour(0, 0, 128),
  Colour(25, 25, 112),
  Colour(230, 230, 250),
  Colour(216, 191, 216),
  Colour(221, 160, 221),
  Colour(238, 130, 238),
  Colour(218, 112, 214),
  Colour(255, 0, 255),
  Colour(255, 0, 255),
  Colour(186, 85, 211),
  Colour(147, 112, 219),
  Colour(138, 43, 226),
  Colour(148, 0, 211),
  Colour(153, 50, 204),
  Colour(139, 0, 139),
  Colour(128, 0, 128),
  Colour(75, 0, 130),
  Colour(72, 61, 139),
  Colour(106, 90, 205),
  Colour(123, 104, 238),
};


static const int DefaultColourSet[] =
{
  Colour::Red,
  Colour::Green,
  Colour::Blue,
  Colour::MediumOrchid,
  Colour::Olive,
  Colour::Teal,
  Colour::Black,
  Colour::OrangeRed,
  Colour::Yellow,
  Colour::MediumAquamarine,
  Colour::Gainsboro,
  Colour::White,
  Colour::Pink,
  Colour::LightSalmon,
  Colour::Tomato,
  Colour::DarkOliveGreen,
  Colour::Aqua,
  Colour::LightSteelBlue,
  Colour::Silver,
  Colour::HotPink,
  Colour::Salmon,
  Colour::Coral,
  Colour::Wheat,
  Colour::Olive,
  Colour::PowderBlue,
  Colour::Thistle,
  Colour::DarkGrey,
  Colour::DeepPink,
  Colour::DarkSalmon,
  Colour::DarkOrange,
  Colour::Moccasin,
  Colour::BurlyWood,
  Colour::OliveDrab,
  Colour::Aquamarine,
  Colour::LightBlue,
  Colour::Plum,
  Colour::DimGrey,
  Colour::PaleVioletRed,
  Colour::LightCoral,
  Colour::Orange,
  Colour::PeachPuff,
  Colour::Tan,
  Colour::YellowGreen,
  Colour::Turquoise,
  Colour::SkyBlue,
  Colour::Violet,
  Colour::SlateGrey,
  Colour::MediumVioletRed,
  Colour::IndianRed,
  Colour::RosyBrown,
  Colour::LimeGreen,
  Colour::MediumTurquoise,
  Colour::DeepSkyBlue,
  Colour::Orchid,
  Colour::DarkSlateGrey,
  Colour::Crimson,
  Colour::Khaki,
  Colour::SandyBrown,
  Colour::Lime,
  Colour::DarkTurquoise,
  Colour::CornflowerBlue,
  Colour::Fuchsia,
  Colour::FireBrick,
  Colour::DarkKhaki,
  Colour::DarkGoldenrod,
  Colour::LawnGreen,
  Colour::LightSeaGreen,
  Colour::SteelBlue,
  Colour::MediumPurple,
  Colour::DarkRed,
  Colour::Gold,
  Colour::Peru,
  Colour::MediumSpringGreen,
  Colour::CadetBlue,
  Colour::RoyalBlue,
  Colour::BlueViolet,
  Colour::Chocolate,
  Colour::LightGreen,
  Colour::DarkCyan,
  Colour::DarkBlue,
  Colour::DarkViolet,
  Colour::SaddleBrown,
  Colour::DarkSeaGreen,
  Colour::MidnightBlue,
  Colour::Purple,
  Colour::Sienna,
  Colour::MediumSeaGreen,
  Colour::Indigo,
  Colour::Brown,
  Colour::SeaGreen,
  Colour::DarkSlateBlue,
  Colour::Maroon,
  Colour::DarkGreen,
  Colour::SlateBlue
};

const int DeuteranomalyColourSet[] =
{
  Colour::RoyalBlue,
  Colour::Yellow,
  Colour::Silver,
  Colour::Black,
  Colour::Blue,
  Colour::Khaki,
  Colour::Gainsboro,
  Colour::Beige,
  Colour::Navy,
  Colour::DarkKhaki,
  Colour::White,
  Colour::Grey,
  Colour::MidnightBlue,
  Colour::SlateGrey,
  Colour::Ivory,
  Colour::Gold,
  Colour::DarkSlateBlue,
  Colour::MediumSlateBlue
};

const int ProtanomalyColourSet[] =
{
  Colour::Blue,
  Colour::Yellow,
  Colour::Black,
  Colour::Silver,
  Colour::CornflowerBlue,
  Colour::Gainsboro,
  Colour::MediumSlateBlue,
  Colour::Khaki,
  Colour::Grey,
  Colour::DarkBlue,
  Colour::Beige,
  Colour::DarkKhaki,
  Colour::MidnightBlue,
  Colour::SlateGrey,
  Colour::RoyalBlue,
  Colour::Ivory,
  Colour::DarkSlateBlue,
};

const int TritanomalyColourSet[] =
{
  Colour::DeepSkyBlue,
  Colour::DeepPink,
  Colour::PaleTurquoise,
  Colour::Black,
  Colour::Crimson,
  Colour::LightSeaGreen,
  Colour::Gainsboro,
  Colour::Blue,
  Colour::DarkRed,
  Colour::Silver,
  Colour::Brown,
  Colour::DarkTurquoise,
  Colour::Grey,
  Colour::Maroon,
  Colour::Teal,
  Colour::SlateGrey,
  Colour::MidnightBlue,
  Colour::DarkSlateGrey,
};

const int GreyColourSet[] =
{
  Colour::Black,
  Colour::Silver,
  Colour::DarkSlateGrey,
  Colour::Grey,
  Colour::Gainsboro,
  Colour::SlateGrey,
};

const int Colour::CycleCounts[CycleCount] =
{
  sizeof(DefaultColourSet) / sizeof(DefaultColourSet[0]),
  sizeof(DeuteranomalyColourSet) / sizeof(DeuteranomalyColourSet[0]),
  sizeof(ProtanomalyColourSet) / sizeof(ProtanomalyColourSet[0]),
  sizeof(TritanomalyColourSet) / sizeof(TritanomalyColourSet[0]),
  sizeof(GreyColourSet) / sizeof(GreyColourSet[0]),
};

const int *Colour::ColourCycles[CycleCount] =
{
  DefaultColourSet,
  DeuteranomalyColourSet,
  ProtanomalyColourSet,
  TritanomalyColourSet,
  GreyColourSet
};