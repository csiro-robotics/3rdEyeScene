//
// author: Kazys Stepanas
//
#include "3estext3d.h"

using namespace tes;

const Vector3f Text3D::DefaultFacing(0, -1, 0);

Text3D::~Text3D()
{
  delete[] _text;
}


bool Text3D::writeCreate(PacketWriter &stream) const
{
  bool ok = true;
  stream.reset(routingId(), CreateMessage::MessageId);
  ok = _data.write(stream) && ok;

  // Write line count and lines.
  const uint16_t textLength = _textLength;
  ok = stream.writeElement(textLength) == sizeof(textLength) && ok;

  if (textLength)
  {
    // Don't write null terminator.
    ok = stream.writeArray(_text, textLength) == sizeof(*_text) * textLength && ok;
  }

  return ok;
}


Shape *Text3D::clone() const
{
  Text3D *copy = new Text3D(nullptr, (uint16_t)0);
  onClone(copy);
  return copy;
}


void Text3D::onClone(Text3D *copy) const
{
  Shape::onClone(copy);
  copy->setText(_text, _textLength);
}


Text3D &Text3D::setText(const char *text, uint16_t textLength)
{
  delete[] _text;
  _text = nullptr;
  _textLength = 0;
  if (text && textLength)
  {
    _text = new char[textLength + 1];
    _textLength = textLength;
#ifdef _MSC_VER
    strncpy_s(_text, _textLength + 1, text, textLength);
#else  // _MSC_VER
    strncpy(_text, text, textLength);
#endif // _MSC_VER
  }
  return *this;
}
