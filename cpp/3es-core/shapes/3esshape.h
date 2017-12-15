//
// author: Kazys Stepanas
//
#ifndef _3ESSHAPE_H_
#define _3ESSHAPE_H_

#include "3es-core.h"

#include "3escolour.h"
#include "3esmessages.h"
#include "3esv3arg.h"
#include "3esquaternionarg.h"

#include <cstdint>

#ifdef WIN32
#pragma warning(push)
#pragma warning(disable : 4251)
#endif // WIN32

namespace tes
{
  class PacketWriter;
  class Resource;

  /// A base class for encapsulating a shape which is to be represented remotely.
  ///
  /// Simple shapes only need a @c writeCreate() call to be fully represented,
  /// after which @c writeUpdate() may move the object. Complex shapes required
  /// additional data to be fully represented and the @c writeCreate() packet
  /// stream may not be large enough to hold all the data. Such complex shapes
  /// will have @c writeData() called, with a changing progress marker.
  /// Complex shapes return @c true from @c isComplex().
  ///
  /// Note that a shape which is not complex may override the @c writeCreate()
  /// method and add additional data. Complex shapes are only required when
  /// this is not sufficient and the additional data may overflow the packet
  /// buffer.
  class _3es_coreAPI Shape
  {
  public:
    Shape(uint16_t routingId, uint32_t id = 0);
    /// Construct a box object.
    /// @param id The shape ID, unique among @c Arrow objects, or zero for a transient shape.
    /// @param category The category grouping for the shape used for filtering.
    Shape(uint16_t routingId, uint32_t id, uint16_t category);
    virtual inline ~Shape() {}

    uint16_t routingId() const;

    uint32_t id() const;
    Shape &setId(uint32_t id);
    uint16_t category() const;
    Shape &setCategory(uint16_t category);

    /// Sets the wireframe flag value for this shape. Only before sending create.
    /// Not all shapes will respect the flag.
    /// @return @c *this.
    Shape &setWireframe(bool wire);
    /// Returns true if the wireframe flag is set.
    /// @return True if wireframe flag is set.
    bool isWireframe() const;

    /// Sets the transparent flag value for this shape. Only before sending create.
    /// Not all shapes will respect the flag.
    /// @return @c *this.
    Shape &setTransparent(bool transparent);
    /// Returns true if the transparent flag is set.
    /// @return True if transparent flag is set.
    bool isTransparent() const;

    /// Sets the two sided shader flag value for this shape. Only before sending create.
    /// Not all shapes will respect the flag.
    /// @return @c *this.
    Shape &setTwoSided(bool twoSided);
    /// Returns true if the two sided shader flag is set.
    /// @return True if two sided flag is set.
    bool isTwoSided() const;

    /// Set the full set of @c ObjectFlag values.
    /// This affects attributes such as @c isTwoSided() and @c isWireframe().
    /// @param flags New flag values to write.
    /// @return @c *this.
    Shape &setFlags(uint16_t flags);
    /// Retrieve the full set of @c ObjectFlag values.
    /// @return Active flag set.
    uint16_t flags() const;

    Shape &setPosition(const V3Arg &pos);
    Vector3f position() const;

    Shape &setPosX(float p);
    Shape &setPosY(float p);
    Shape &setPosZ(float p);

    Shape &setRotation(const QuaternionArg &rot);
    Quaternionf rotation() const;

    Shape &setScale(const V3Arg &scale);
    Vector3f scale() const;

    Shape &setColour(const Colour &colour);
    Colour colour() const;

    /// Update the attributes of this shape to match @p other.
    /// Used in maintaining cached copies of shapes. The shapes should
    /// already represent the same object.
    ///
    /// Not all attributes need to be updated. Only attributes which may be updated
    /// via an @c UpdateMessage for this shape need be copied.
    ///
    /// The default implementation copies only the @c ObjectAttributes.
    ///
    /// @param other The shape to update data from.
    virtual void updateFrom(const Shape &other);

    /// Writes the create message to @p stream.
    ///
    /// Simple shapes will write all data in the create message. More
    /// complex shapes may have additional data which is required to
    /// create object (e.g., point clouds). In the latter case, only
    /// enough data to initialise the object need be written.
    virtual bool writeCreate(PacketWriter &stream) const;

    /// Called only for complex shapes to write additional creation data.
    ///
    /// @param stream The data stream to write to.
    /// @param[in,out] progressMarker Indicates data transfer progress.
    ///   Initially zero, the @c Shape manages its own semantics.
    /// @return Indicates completion progress. 0 indicates completion,
    ///   1 indicates more data are available and more calls should be made.
    ///   -1 indicates an error. No more calls should be made.
    virtual inline int writeData(PacketWriter &stream, unsigned &progressMarker) const { return 0; }

    bool writeUpdate(PacketWriter &stream) const;
    bool writeDestroy(PacketWriter &stream) const;

    /// Is this a complex shape? Complex shapes have @c writeData() called.
    /// @return True if complex, false if simple.
    virtual inline bool isComplex() const { return false; }

    /// Enumerate the resources used by this shape. Resources are most commonly used by
    /// mesh shapes to expose the mesh data, where the shape simply positions the mesh.
    ///
    /// The function is called to fetch the shape's resources into @p resources,
    /// up to the given @p capacity. Repeated calls may be used to fetch all resources
    /// into a smaller array by using the @p fetchOffset parameter as a marker indicating
    /// how many items have already been fetched. Regardless, data are always written to
    /// @p resources starting at index zero.
    ///
    /// This function may also be called with a @c nullptr for @p resources and/or
    /// a zero @p capacity. In this case the return value indicates the number of
    /// resources used by the shape.
    ///
    /// @param resources The array to populate with this shape's resources.
    /// @param capacity The element count capacity of @p resources.
    /// @param fetchOffset An offset used to fetch resources into an array too small to
    ///   hold all available resources. It is essentially the running sum of resources
    ///   fetched so far.
    /// @return The number of items added to @p resources when @p resources and @p capacity
    ///   are non zero. When @p resources is null or @p capacity zero, the return value
    ///   indicates the total number of resources used by the shape.
    virtual int enumerateResources(const Resource **resources, int capacity, int fetchOffset = 0) const;

    /// Deep copy clone.
    /// @return A deep copy.
    virtual Shape *clone() const;

  protected:
    /// Called when @p copy is created from this object to copy appropriate attributes to @p copy.
    ///
    /// The general use case is for a subclass to override @c clone(), creating the correct
    /// concrete type, then call @c onClone() to copy data. The advantage is that @p onClone()
    /// can recursively call up the class hierarchy.
    /// @param copy The newly cloned object to copy data to. Must not be null.
    void onClone(Shape *copy) const;

    void init(uint32_t id, uint16_t cat = 0, uint16_t flags = 0);

    uint16_t _routingId;
    CreateMessage _data;
  };


  inline Shape::Shape(uint16_t routingId, uint32_t id)
    : _routingId(routingId)
  {
    init(id);
  }


  inline Shape::Shape(uint16_t routingId, uint32_t id, uint16_t category)
    : _routingId(routingId)
  {
    init(id, category);
  }


  inline void Shape::init(uint32_t id, uint16_t cat, uint16_t flags)
  {
    _data.id = id;
    _data.category = cat;
    _data.flags = flags;
    _data.reserved = 0u;
    _data.attributes.colour = 0xffffffffu;
    _data.attributes.position[0] = _data.attributes.position[1] = _data.attributes.position[2] = 0;
    _data.attributes.rotation[0] = _data.attributes.rotation[1] = _data.attributes.rotation[2] = 0;
    _data.attributes.rotation[3] = _data.attributes.scale[0] = _data.attributes.scale[1] = _data.attributes.scale[2] = 1;
  }


  inline uint16_t Shape::routingId() const
  {
    return _routingId;
  }


  inline uint32_t Shape::id() const
  {
    return _data.id;
  }


  inline Shape &Shape::setId(uint32_t id)
  {
    _data.id = id;
    return *this;
  }


  inline uint16_t Shape::category() const
  {
    return _data.category;
  }


  inline Shape &Shape::setCategory(uint16_t category)
  {
    _data.category = category;
    return *this;
  }


  inline Shape &Shape::setWireframe(bool wire)
  {
    _data.flags &= ~OFWire;
    _data.flags |= OFWire * !!wire;
    return *this;
  }


  inline bool Shape::isWireframe() const
  {
    return (_data.flags & OFWire) != 0;
  }


  inline Shape &Shape::setTransparent(bool transparent)
  {
    _data.flags &= ~OFTransparent;
    _data.flags |= OFTransparent * !!transparent;
    return *this;
  }


  inline bool Shape::isTransparent() const
  {
    return (_data.flags & OFTransparent) != 0;
  }


  inline Shape &Shape::setTwoSided(bool twoSided)
  {
    _data.flags &= ~OFTwoSided;
    _data.flags |= OFTwoSided * !!twoSided;
    return *this;
  }


  inline bool Shape::isTwoSided() const
  {
    return (_data.flags & OFTwoSided) != 0;
  }


  inline Shape &Shape::setFlags(uint16_t flags)
  {
    _data.flags = flags;
    return *this;
  }


  inline uint16_t Shape::flags() const
  {
    return _data.flags;
  }


  inline Shape &Shape::setPosition(const V3Arg &pos)
  {
    _data.attributes.position[0] = pos[0];
    _data.attributes.position[1] = pos[1];
    _data.attributes.position[2] = pos[2];
    return *this;
  }


  inline Vector3f Shape::position() const
  {
    return Vector3f(_data.attributes.position[0], _data.attributes.position[1], _data.attributes.position[2]);
  }


  inline Shape &Shape::setPosX(float p)
  {
    _data.attributes.position[0] = p;
    return *this;
  }


  inline Shape &Shape::setPosY(float p)
  {
    _data.attributes.position[1] = p;
    return *this;
  }


  inline Shape &Shape::setPosZ(float p)
  {
    _data.attributes.position[2] = p;
    return *this;
  }


  inline Shape &Shape::setRotation(const QuaternionArg &rot)
  {
    _data.attributes.rotation[0] = rot[0];
    _data.attributes.rotation[1] = rot[1];
    _data.attributes.rotation[2] = rot[2];
    _data.attributes.rotation[3] = rot[3];
    return *this;
  }


  inline Quaternionf Shape::rotation() const
  {
    return Quaternionf(_data.attributes.rotation[0], _data.attributes.rotation[1], _data.attributes.rotation[2], _data.attributes.rotation[3]);
  }


  inline Shape &Shape::setScale(const V3Arg &scale)
  {
    _data.attributes.scale[0] = scale[0];
    _data.attributes.scale[1] = scale[1];
    _data.attributes.scale[2] = scale[2];
    return *this;
  }


  inline Vector3f Shape::scale() const
  {
    return Vector3f(_data.attributes.scale[0], _data.attributes.scale[1], _data.attributes.scale[2]);
  }


  inline Shape &Shape::setColour(const Colour &colour)
  {
    _data.attributes.colour = colour.c;
    return *this;
  }


  inline Colour Shape::colour() const
  {
    return Colour(_data.attributes.colour);
  }
}

#ifdef WIN32
#pragma warning(pop)
#endif // WIN32

#endif // _3ESSHAPE_H_
