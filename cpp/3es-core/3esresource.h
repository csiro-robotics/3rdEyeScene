//
// author: Kazys Stepanas
//
#ifndef _3ESRESOURCE_H_
#define _3ESRESOURCE_H_

#include "3es-core.h"

#include <cstdint>

namespace tes
{
  class PacketReader;
  class PacketWriter;
  struct TransferProgress;

  /// The @p Resource base class defines an interface for any resource used by @p Shape
  /// objects such as @c MeshSet. Resources are uniquely identified by a combination of
  /// their @p typeId() and @p id(). The @p typeId() essentially acts as a class type ID -
  /// e.g., mesh or material - while the @p id() is unique amongst objects of that type.
  ///
  /// The IDs are most significantly used in reference counting resource usage and to
  /// identify which resources require transfer to each client. A @c Resource is transferred
  /// to a client when first referenced and a destroy message sent when the last shape
  /// using that resource is destroyed.
  ///
  /// The transfer of a @c Resource to a client follows this lifecycle:
  /// -# Generate and send a creation message by calling @c create().
  /// -# Call @c transfer() and send each packet until the @p TransferProgress is
  ///     complete or fails.
  /// -# Call @c destroy() after the last shape referencing the @c Resource is destroyed.
  ///
  /// These steps are followed for each connected client all these functions must be
  /// reentrant.
  class _3es_coreAPI Resource
  {
  public:
    /// Virtual destructor (empty).
    virtual ~Resource();

    /// The resource ID. Unique among resources of the same @c typeId().
    /// @return The resource's ID.
    virtual uint32_t id() const = 0;

    /// The resource type ID. This corresponds to the routing ID (see @c MessageTypeIDs).
    /// May be loosely used for casting.
    /// @return The type ID.
    virtual uint16_t typeId() const = 0;

    /// Returns a unique key for this resource, based on the @c typeId() and @c id().
    inline uint64_t uniqueKey() const { return ((uint64_t)typeId() << 32) | (uint64_t)id(); }

    /// Clone the resource. Ideally this should perform a limited, shallow copy and expose
    /// shared resource data. For example, a @c MeshResource may wrap an existing mesh
    /// object pointer and the clone operation simply copies the wrapped pointer.
    /// Obviously, the existing mesh object must outlive the resource use.
    /// @return A (preferably shallow) copy of this resource.
    virtual Resource *clone() const = 0;

    /// Generate a creation packet to send to a connected client.
    ///
    /// Note that any implementation must @c PacketWriter::reset() the packet before
    /// writing to it, but should not @c PacketWriter::finalise() the packet.
    ///
    /// @param packet A packet to populate and send.
    /// @return Zero on success, an error code otherwise.
    virtual int create(PacketWriter &packet) const = 0;

    /// Generate a destruction packet to send to a connected client.
    ///
    /// Note that any implementation must @c PacketWriter::reset() the packet before
    /// writing to it, but should not @c PacketWriter::finalise() the packet.
    ///
    /// @param packet A packet to populate and send.
    /// @return Zero on success, an error code otherwise.
    virtual int destroy(PacketWriter &packet) const = 0;

    /// Populate a packet with additional resource data to send to a connected client.
    ///
    /// This function is called repeatedly to transfer the resource data, possibly over several
    /// update cycles. The @p progress is used to track which data have been transferred already.
    /// The semantics of the @c TransferProgress @p progress and @p phase values are entirely
    /// dependent on a @p transfer() function implementation.
    ///
    /// Once the last packet is populated, the @p progress.complete flag must be set and no
    /// further calls to @p transfer() are made for a client (unless the resource is destroyed
    /// then referenced again). On error, the @p progress.failed flag may be sent, which also
    /// halts transfer.
    ///
    /// Implementations should respect the @p byteLimit and keep packet sizes below this limit.
    ///
    /// Note that any implementation must @c PacketWriter::reset() the packet before
    /// writing to it, but should not @c PacketWriter::finalise() the packet.
    ///
    /// @param packet A packet to populate and send.
    /// @param byteLimit A nominal byte limit on how much data a single @p transfer() call may add.
    /// @param[in,out] progress A progress marker tracking how much has already been transferred, and
    ///     updated to indicate what has been added to @p packet.
    /// @return Zero on success, an error code otherwise.
    virtual int transfer(PacketWriter &packet, int byteLimit, TransferProgress &progress) const = 0;

    virtual bool readCreate(PacketReader &packet) = 0;
    virtual bool readTransfer(int messageType, PacketReader &packet) = 0;
  };
}

#endif // _3ESRESOURCE_H_
