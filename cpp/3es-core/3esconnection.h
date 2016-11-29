//
// author: Kazys Stepanas
//
#ifndef _3ESCONNECTION_H_
#define _3ESCONNECTION_H_

#include "3es-core.h"

#include <cstddef>
#include <cstdint>

namespace tes
{
  class Resource;
  class Shape;
  struct ServerInfoMessage;

  /// Defines the interfaces for a client connection.
  class _3es_coreAPI Connection
  {
  public:
    /// Virtual destructor.
    virtual ~Connection() {}

    /// Close the socket connection.
    virtual void close() = 0;

    /// Activate/deactivate the connection. Messages are ignored while inactive.
    /// @param enable The active state to set.
    virtual void setActive(bool active) = 0;

    /// Check if currently active.
    /// @return True while active.
    virtual bool active() const = 0;

    /// Address string for the connection. The string depends on
    /// the connection type.
    /// @return The connection end point address.
    virtual const char *address() const = 0;
    /// Get the connection port.
    /// @return The connection end point port.
    virtual uint16_t port() const = 0;
    /// Is the connection active and valid?
    /// @return True while connected.
    virtual bool isConnected() const = 0;

    /// Sends a create message for the given shape.
    /// @param shape The shape details.
    /// @return The number of bytes queued for transfer for this message, or negative on error.
    ///   The negative value may be less than -1 and still indicate the successful transfer size.
    virtual int create(const Shape &shape) = 0;

    /// Sends a destroy message for the given shape.
    /// @param shape The shape details.
    /// @return The number of bytes queued for transfer for this message, or negative on error.
    ///   The negative value may be less than -1 and still indicate the successful transfer size.
    virtual int destroy(const Shape &shape) = 0;

    /// Sends an update message for the given shape.
    /// @param shape The shape details.
    /// @return The number of bytes queued for transfer for this message, or negative on error.
    ///   The negative value may be less than -1 and still indicate the successful transfer size.
    virtual int update(const Shape &shape) = 0;

    /// Sends a message marking the end of the current frame (and start of a new frame).
    ///
    /// @param dt Indicates the time passed since over this frame (seconds).
    /// @param flush True to allow clients to flush transient objects, false to instruct clients
    ///   to preserve such objects.
    /// @return The number of bytes queued for transfer for this message, or negative on error.
    ///   The negative value may be less than -1 and still indicate the successful transfer size.
    virtual int updateFrame(float dt, bool flush = true) = 0;

        /// Update any pending amortised data transfers (e.g., mesh transfer).
    /// @param byteLimit Limit the packet payload size to approximately this
    /// amount of data.
    /// @return The number of bytes queued for transfer for this message, or negative on error.
    ///   The negative value may be less than -1 and still indicate the successful transfer size.
    virtual int updateTransfers(unsigned byteLimit) = 0;

    /// Add a resource to this connection.
    ///
    /// The resource is either added with a reference count of 1, or the resource
    /// reference count is incremented. The @p resource pointer must remain valid until
    /// the reference count returns to zero. A newly added resource is pushed into the
    /// resource queue for transfer.
    ///
    /// @param resource The resource to reference.
    /// @return The resource reference count after adjustment.
    virtual unsigned referenceResource(const Resource *resource) = 0;

    /// Release a resource within this connection.
    ///
    /// If found, the resource has its reference count reduced. A destroy message is sent for
    /// the resource if the count becomes zero.
    ///
    /// @param resource The resource to release.
    /// @return The resource reference count after adjustment.
    virtual unsigned releaseResource(const Resource *resource) = 0;

    /// Send server details to the client.
    virtual bool sendServerInfo(const ServerInfoMessage &info) = 0;

    /// Send pre-prepared message data to all connections.
    /// @param data Data buffer to send.
    /// @param byteCount Number of bytes to send.
    virtual int send(const uint8_t *data, int byteCount) = 0;

    /// @overload
    inline int send(const int8_t *data, int byteCount)
    {
      return send((const uint8_t*)data, byteCount);
    }
  };
}

#endif // _3ESCONNECTION_H_
