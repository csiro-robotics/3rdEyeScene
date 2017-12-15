//
// author: Kazys Stepanas
//
#ifndef _3ESCOLLATEDPACKET_H_
#define _3ESCOLLATEDPACKET_H_

#include "3es-core.h"

#include "3esconnection.h"
#include "3espacketheader.h"

namespace tes
{
  struct CollatedPacketMessage;
  struct CollatedPacketZip;
  class PacketWriter;

  /// A utility class which generates a @c MtCollatedPacket message by appending multiple
  /// other messages. Compression may optionally be applied.
  ///
  /// Typical usage:
  /// - Instantiate the packet.
  /// - Reset the packet.
  /// - For each constituent message
  ///   - Generate the packet using @p PacketWriter.
  ///   - Finalise the message.
  ///   - Call @c add(const PacketWriter &)
  /// - Call @c finalise() on the collated packet.
  /// - Send the collated packet.
  /// - Call @c reset()
  ///
  /// The @c CollatedPacket also extends the @c Connection class in order to support
  /// multi-threaded packet generation and synchronisation. While the @c Connection
  /// and @c Connection implementations are required to be thread-safe, they
  /// cannot guarantee packets area correctly collated by thread. Thus a
  /// @c CollatedPacket can be used per thread to collate messages for each thread.
  /// The packet content can then be sent as a single transaction.
  ///
  /// By supporting the @c Connection methods, a @p CollatedPacket can be used in place
  /// of a 'server' argument with various server utility macros and functions.
  ///
  /// By default, a @c CollatedPacket has is limited to supporting @c MaxPacketSize bytes.
  /// This allows a single packet with a single @c PacketHeader and collated packet
  /// message with optional compression. However, when a collated packet is used for
  /// transaction collation (as described in the multi-threaded case), it may require
  /// collation of larger data sizes. In this case, the @c CollatedPacket(unsigned, unsigned)
  /// constructor can be used to specify a larger collation buffer limit (the buffer resizes as
  /// required). Such large, collated packets are sent using
  /// @c Server::send(const CollatedPacket &). Internally, the method may either send
  /// the packet as is (if small enough), or extract and reprocess each collated packet.
  class _3es_coreAPI CollatedPacket : public Connection
  {
  public:
    /// Byte count overhead added by using a @p CollatedPacket.
    /// This is the sum of @p PacketHeader, @c CollatedPacketMessage and the @c PacketWriter::CrcType.
    static const size_t Overhead;
    /// Initial cursor position in the write buffer.
    /// This is the sum of @p PacketHeader, @c CollatedPacketMessage.
    static const unsigned InitialCursorOffset;
    /// The default packet size limit for a @c CollatedPacketMessage.
    static const uint16_t MaxPacketSize;

  public:
    /// Initialise a collated packet. This sets the initial packet size limited
    /// by @c MaxPacketSize, and compression options.
    ///
    /// @param compress True to compress data as written.
    /// @param bufferSize The initial bufferSize
    /// @bug Specifying a buffer size too close to 0xffff (even correctly accounting for
    ///   the expected overhead) results in dropped packets despite the network layer
    ///   not reporting errors. Likely I'm missing some overhead detail. For now, use
    ///   a lower packet size.
    CollatedPacket(bool compress, uint16_t bufferSize = 0xff00u);

    /// Initialise a collated packet allowing packet sizes large than @p MaxPacketSize.
    /// This is intended for collating messages to be send as a group in a thread-safe
    /// fashion. The maximum packet size may exceed the normal send limit. As such
    /// compression is not allowed to better support splitting.
    ///
    /// @param bufferSize The initial bufferSize
    /// @param maxPacketSize The maximum packet size.
    CollatedPacket(unsigned bufferSize, unsigned maxPacketSize);

    /// Destructor.
    ~CollatedPacket();

    /// Is compression enabled. Required ZLIB.
    /// @return True if compression is enabled.
    bool compressionEnabled() const;

    /// Return the capacity of the collated packet.
    ///
    /// This defaults to 64 * 1024 - 1 (the maximum for a 16-bit unsigned integer),
    /// when using the constructor: @c CollatedPacket(bool, unsigned). It may be
    /// larger when using the @c CollatedPacket(unsigned, unsigned) constructor.
    /// See that constructor and class notes for details.
    ///
    /// @return The maximum packet capacity or 0xffffffffu if the packet size is variable.
    unsigned maxPacketSize() const;

    /// Reset the collated packet, dropping any existing data.
    void reset();

    /// Add the packet data in @p packet to the collation buffer.
    ///
    /// The method will fail (return -1) when the @c maxPacketSize() has been reached.
    /// In this case, the packet should be sent and reset before trying again.
    /// The method will also fail if the packet has already been finalised using
    /// @c finalise().
    ///
    /// @param packet The packet data to add.
    /// @return The <tt>packet.packetSize()</tt> on success, or -1 on failure.
    int add(const PacketWriter &packet);

    /// Add bytes to the packet. Use with care as the @p buffer should always
    /// start with a valid @c PacketHeader in network byte order.
    /// @param buffer The data to add.
    /// @param byteCount The number of bytes in @p buffer.
    /// @return The <tt>packet.packetSize()</tt> on success, or -1 on failure.
    int add(const uint8_t *buffer, uint16_t byteCount);

    /// Finalises the collated packet for sending. This includes completing
    /// compression and calculating the CRC.
    /// @return True on successful finalisation, false when already finalised.
    bool finalise();

    /// Access the internal buffer pointer.
    /// @param[out] Set to the number of used bytes in the collated buffer, including
    ///     the CRC when the packet has been finalised.
    /// @return The internal buffer pointer.
    const uint8_t *buffer(unsigned &byteCount) const;

    /// Return the number of bytes that have been collated. This excludes the @c PacketHeader
    /// and @c CollatedPacketMessage, but will include the CRC once finalised.
    unsigned collatedBytes() const;

    //-------------------------------------------
    // Connection methods.
    //-------------------------------------------

    /// Ignored for @c CollatedPacket.
    void close() override;

    /// Enable/disable the connection. While disabled, messages are ignored.
    /// @param active The active state to set.
    void setActive(bool active) override;

    /// Check if currently active.
    /// @return True while active.
    bool active() const override;

    /// Identifies the collated packet.
    /// @return Always "CollatedPacket".
    const char *address() const override;

    /// Not supported.
    /// @return Zero.
    uint16_t port() const override;

    /// Always connected.
    /// @return True.
    bool isConnected() const override;

    /// Collated the create message for @p shape.
    /// @param shape The shape of interest.
    /// @return The number of bytes added, or -1 on failure (as per @c add()).
    int create(const Shape &shape) override;

    /// Collated the update message for @p shape.
    /// @param shape The shape of interest.
    /// @return The number of bytes added, or -1 on failure (as per @c add()).
    int destroy(const Shape &shape) override;

    /// Collated the destroy message for @p shape.
    /// @param shape The shape of interest.
    /// @return The number of bytes added, or -1 on failure (as per @c add()).
    int update(const Shape &shape) override;

    /// Not supported.
    /// @param byteLimit Ignored.
    /// @return -1.
    int updateTransfers(unsigned byteLimit) override;

    /// Not supported.
    /// @param dt Ignored.
    /// @param flush Ignored.
    int updateFrame(float dt, bool flush = true) override;

    /// Not supported.
    /// @return 0;
    unsigned referenceResource(const Resource *resource) override;

    /// Not supported.
    /// @return 0;
    unsigned releaseResource(const Resource *resource) override;

    /// Collated the create message for @p shape.
    /// @param shape The shape of interest.
    /// @return The number of bytes added, or -1 on failure (as per @c add()).
    bool sendServerInfo(const ServerInfoMessage &info) override;

    /// Aliased to @p add().
    /// @param buffer The data to add.
    /// @param bufferSize The number of bytes in @p buffer.
    /// @return The <tt>packet.packetSize()</tt> on success, or -1 on failure.
    int send(const uint8_t *data, int byteCount) override;

  private:
    /// Initialise the buffer.
    /// @param compress Enable compression?
    /// @param bufferSize Initial buffer size.
    /// @param maxPacketSize Maximum buffer size.
    void init(bool compress, unsigned bufferSize, unsigned maxPacketSize);

    /// Expand the internal buffer size by @p expandBy bytes up to @c maxPacketSize().
    /// @param expandBy Minimum number of bytes to expand by.
    static void expand(unsigned expandBy, uint8_t *&buffer, unsigned &bufferSize, unsigned currentDataCount, unsigned maxPacketSize);

    CollatedPacketZip *_zip;  ///< Present and used when compression is enabled.
    uint8_t *_buffer;         ///< Internal buffer.
    /// Buffer used to finalise collation. Deflating may not be successful, so we can try and fail with this buffer.
    uint8_t *_finalBuffer;
    unsigned _bufferSize;     ///< current size of @c _buffer.
    unsigned _finalBufferSize;///< current size of @c _finalBuffer.
    unsigned _finalPacketCursor;  ///< End of data in @c _finalBuffer
    unsigned _cursor;         ///< Current write position in @c _buffer.
    unsigned _maxPacketSize;  ///< Maximum @p _bufferSize.
    bool _finalised;          ///< Finalisation flag.
    bool _active;             ///< For @c Connection::active().
  };


  inline bool CollatedPacket::compressionEnabled() const
  {
    return _zip != nullptr;
  }


  inline unsigned CollatedPacket::maxPacketSize() const
  {
    return _maxPacketSize;
  }


  inline unsigned CollatedPacket::collatedBytes() const
  {
    return _cursor;
  }
}

#endif // _3ESCOLLATEDPACKET_H_
