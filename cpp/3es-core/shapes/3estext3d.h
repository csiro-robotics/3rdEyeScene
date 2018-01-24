//
// author: Kazys Stepanas
//
#ifndef _3ESTEXT3D_H_
#define _3ESTEXT3D_H_

#include "3es-core.h"

#include "3esshape.h"

#include <cstdint>
#include <cstring>

namespace tes
{
  /// A shape 3D world position and perspective adjusted text, optionally screen facing.
  /// Expects UTF-8 encoding.
  ///
  /// FIXME: add rotation support to the text, identifying the orientation axes.
  class _3es_coreAPI Text3D : public Shape
  {
  public:
    static const Vector3f DefaultFacing;

    //Text3D(const char *text, uint16_t textLength, const V3Arg &pos = V3Arg(0, 0, 0), int fontSize = 12);
    //Text3D(const char *text, uint16_t textLength, const V3Arg &pos, const V3Arg &facing, int fontSize = 12);
    //Text3D(const char *text, uint16_t textLength, uint32_t id, const V3Arg &pos = V3Arg(0, 0, 0), int fontSize = 12);
    //Text3D(const char *text, uint16_t textLength, uint32_t id, const V3Arg &pos, const V3Arg &facing, int fontSize = 12);
    //Text3D(const char *text, uint16_t textLength, uint32_t id, uint16_t category, const V3Arg &pos = V3Arg(0, 0, 0), int fontSize = 12);
    //Text3D(const char *text, uint16_t textLength, uint32_t id, uint16_t category, const V3Arg &pos, const V3Arg &facing, int fontSize = 12);
    Text3D(const char *text, const V3Arg &pos = V3Arg(0, 0, 0), int fontSize = 12);
    Text3D(const char *text, const V3Arg &pos, const V3Arg &facing, int fontSize = 12);
    Text3D(const char *text, uint32_t id, const V3Arg &pos = V3Arg(0, 0, 0), int fontSize = 12);
    Text3D(const char *text, uint32_t id, const V3Arg &pos, const V3Arg &facing, int fontSize = 12);
    Text3D(const char *text, uint32_t id, uint16_t category, const V3Arg &pos = V3Arg(0, 0, 0), int fontSize = 12);
    /// Construct a box object.
    /// @param id The shape ID, unique among @c Arrow objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    Text3D(const char *text, uint32_t id, uint16_t category, const V3Arg &pos, const V3Arg &facing, int fontSize = 12);

    ~Text3D();

    inline const char *type() const override { return "text3D"; }

    bool screenFacing() const;
    Text3D &setScreenFacing(bool worldSpace);

    Text3D &setFacing(const V3Arg &toCamera);
    Vector3f facing() const;

    int fontSize() const;
    Text3D &setFontSize(int size);

    inline char *text() const { return _text; }
    inline uint16_t textLength() const { return _textLength; }

    Text3D &setText(const char *text, uint16_t textLength);

    virtual bool writeCreate(PacketWriter &stream) const override;

    bool readCreate(PacketReader &stream) override;

    Shape *clone() const override;

  protected:
    void onClone(Text3D *copy) const;

  private:
    char *_text;
    uint16_t _textLength;
  };


  //inline Text3D::Text3D(const char *text, uint16_t textLength, const V3Arg &pos, int fontSize)
  //  : Shape(SIdText3D)
  //  , _text(nullptr)
  //  , _textLength(0)
  //{
  //  setPosition(pos);
  //  setText(text, textLength);
  //  setFontSize(fontSize);
  //}


  //inline Text3D::Text3D(const char *text, uint16_t textLength, const V3Arg &pos, const V3Arg &facing, int fontSize)
  //  : Shape(SIdText3D)
  //  , _text(nullptr)
  //  , _textLength(0)
  //{
  //  setPosition(pos);
  //  setText(text, textLength);
  //  setFontSize(fontSize);
  //}


  //inline Text3D::Text3D(const char *text, uint16_t textLength, uint32_t id, const V3Arg &pos, int fontSize)
  //  : Shape(SIdText3D, id)
  //  , _text(nullptr)
  //  , _textLength(0)
  //{
  //  setPosition(pos);
  //  setText(text, textLength);
  //  setFontSize(fontSize);
  //}


  //inline Text3D::Text3D(const char *text, uint16_t textLength, uint32_t id, const V3Arg &pos, const V3Arg &facing, int fontSize)
  //  : Shape(SIdText3D, id)
  //  , _text(nullptr)
  //  , _textLength(0)
  //{
  //  setPosition(pos);
  //  setText(text, textLength);
  //  setFontSize(fontSize);
  //}


  //inline Text3D::Text3D(const char *text, uint16_t textLength, uint32_t id, uint16_t category, const V3Arg &pos, int fontSize)
  //  : Shape(SIdText3D, id, category)
  //  , _text(nullptr)
  //  , _textLength(0)
  //{
  //  setPosition(pos);
  //  setText(text, textLength);
  //  setFontSize(fontSize);
  //}


  //inline Text3D::Text3D(const char *text, uint16_t textLength, uint32_t id, uint16_t category, const V3Arg &pos, const V3Arg &facing, int fontSize)
  //  : Shape(SIdText3D, id, category)
  //  , _text(nullptr)
  //  , _textLength(0)
  //{
  //  setPosition(pos);
  //  setText(text, textLength);
  //  setFontSize(fontSize);
  //}


  inline Text3D::Text3D(const char *text, const V3Arg &pos, int fontSize)
    : Shape(SIdText3D)
    , _text(nullptr)
    , _textLength(0)
  {
    setPosition(pos);
    setText(text, (uint16_t)strlen(text));
    setFontSize(fontSize);
  }


  inline Text3D::Text3D(const char *text, const V3Arg &pos, const V3Arg &facing, int fontSize)
    : Shape(SIdText3D)
    , _text(nullptr)
    , _textLength(0)
  {
    setPosition(pos);
    setText(text, (uint16_t)strlen(text));
    setFontSize(fontSize);
  }


  inline Text3D::Text3D(const char *text, uint32_t id, const V3Arg &pos, int fontSize)
    : Shape(SIdText3D, id)
    , _text(nullptr)
    , _textLength(0)
  {
    setPosition(pos);
    setText(text, (uint16_t)strlen(text));
    setFontSize(fontSize);
  }


  inline Text3D::Text3D(const char *text, uint32_t id, const V3Arg &pos, const V3Arg &facing, int fontSize)
    : Shape(SIdText3D, id)
    , _text(nullptr)
    , _textLength(0)
  {
    setPosition(pos);
    setText(text, (uint16_t)strlen(text));
    setFontSize(fontSize);
  }


  inline Text3D::Text3D(const char *text, uint32_t id, uint16_t category, const V3Arg &pos, int fontSize)
    : Shape(SIdText3D, id, category)
    , _text(nullptr)
    , _textLength(0)
  {
    setPosition(pos);
    setText(text, (uint16_t)strlen(text));
    setFontSize(fontSize);
  }


  inline Text3D::Text3D(const char *text, uint32_t id, uint16_t category, const V3Arg &pos, const V3Arg &facing, int fontSize)
    : Shape(SIdText3D, id, category)
    , _text(nullptr)
    , _textLength(0)
  {
    setPosition(pos);
    setText(text, (uint16_t)strlen(text));
    setFontSize(fontSize);
  }


  inline bool Text3D::screenFacing() const
  {
    return (_data.flags & Text3DFScreenFacing) != 0;
  }


  inline Text3D &Text3D::setScreenFacing(bool worldSpace)
  {
    _data.flags &= ~Text3DFScreenFacing;
    _data.flags |= Text3DFScreenFacing * !!worldSpace;
    return *this;
  }


  inline Text3D &Text3D::setFacing(const V3Arg &toCamera)
  {
    setScreenFacing(false);
    Quaternionf rot;
    if (toCamera.v3.dot(DefaultFacing) > -0.9998f)
    {
      rot = Quaternionf(DefaultFacing, toCamera);
    }
    else
    {
      rot.setAxisAngle(Vector3f::axisx, float(M_PI));
    }
    setRotation(rot);
    return *this;
  }


  inline Vector3f Text3D::facing() const
  {
    Quaternionf rot = rotation();
    return rot * DefaultFacing;
  }


  inline int Text3D::fontSize() const
  {
    return (int)_data.attributes.scale[2];
  }


  inline Text3D &Text3D::setFontSize(int size)
  {
    _data.attributes.scale[2] = (float)size;
    return *this;
  }
}

#endif // _3ESTEXT3D_H_
