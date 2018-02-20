//
// author: Kazys Stepanas
//
#ifndef _3ESMESH_H_
#define _3ESMESH_H_

#include "3es-core.h"
#include "3esmatrix4.h"
#include "3esshape.h"
#include "3esintarg.h"

#include <cstdint>

namespace tes
{
  class MeshResource;

  /// Represents a mesh shape. Requires a @c MeshResource parts to get represent mesh topology.
  /// The shape never owns the @c MeshResource parts and they must outlive the shape.
  class _3es_coreAPI MeshSet : public Shape
  {
  public:
    /// Create a shape with a @c partCount parts. Use @c setPart() to populate.
    /// @param partCount The number of parts to the mesh.
    /// @param id The unique mesh shape ID, zero for transient (not recommended for mesh shapes).
    /// @param category The mesh shape category.
    MeshSet(uint32_t id = 0u, uint16_t category = 0u, const IntArg &partCount = 0);
    /// Create a shape with a single @p part with transform matching the shape transform.
    /// @param part The mesh part.
    /// @param id The unique mesh shape ID, zero for transient (not recommended for mesh shapes).
    /// @param category The mesh shape category.
    MeshSet(const MeshResource *part, uint32_t id = 0u, uint16_t category = 0u);

    /// Destructor.
    ~MeshSet();

    inline const char *type() const override { return "meshSet"; }

    /// Get the number of parts to this shape.
    /// @return The number of parts this shape has.
    int partCount() const;
    /// Set the part at the given index.
    /// @param index The part index to set. Must be in the range <tt>[0, partCount())</tt>.
    /// @param part The mesh data to set at @p index.
    /// @param transform The transform for this part, relative to this shape's transform.
    ///     This transform may not be updated after the shape is sent to a client.
    void setPart(int index, const MeshResource *part, const Matrix4f &transform);
    /// Fetch the part at the given @p index.
    /// @param index The part index to fetch. Must be in the range <tt>[0, partCount())</tt>.
    /// @return The mesh at the given index.
    const MeshResource *partAt(int index) const;
    /// Fetch the transform for the part at the given @p index.
    /// @param index The part transform to fetch. Must be in the range <tt>[0, partCount())</tt>.
    /// @return The transform for the mesh at the given index.
    const Matrix4f &partTransform(int index) const;

    /// Overridden to include the number of mesh parts, their IDs and transforms.
    bool writeCreate(PacketWriter &stream) const override;

    /// Reads the @c CreateMessage and details about the mesh parts.
    ///
    /// Sucessfully reading the message modifies the data in this shape such
    /// that the parts (@c partAt()) are only dummy resources
    /// (@c MeshPlaceholder). This identifies the resource IDs, but the data
    /// must be resolved separately.
    ///
    /// @param stream The stream to read from.
    /// @return @c true on success.
    bool readCreate(PacketReader &stream) override;

    /// Enumerate the mesh resources for this shape.
    /// @todo Add material resources.
    int enumerateResources(const Resource **resources, int capacity, int fetchOffset = 0) const override;

    /// Clone the mesh shape. @c MeshResource objects are shared.
    /// @return The cloned shape.
    Shape *clone() const override;

  protected:
    void onClone(MeshSet *copy) const;

  private:
    void cleanupParts();

    const MeshResource **_parts;
    Matrix4f *_transforms;
    int _partCount;
    bool _ownParts;
  };

  inline int MeshSet::partCount() const { return _partCount; }

  inline void MeshSet::setPart(int index, const MeshResource *part, const Matrix4f &transform)
  {
    _parts[index] = part;
    _transforms[index] = transform;
  }

  inline const MeshResource *MeshSet::partAt(int index) const { return _parts[index]; }

  inline const Matrix4f &MeshSet::partTransform(int index) const { return _transforms[index]; }
}

#endif // _3ESMESH_H_
