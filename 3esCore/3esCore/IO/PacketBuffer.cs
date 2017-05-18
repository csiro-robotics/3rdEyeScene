using System;
using System.IO;
using Tes.Buffers;

namespace Tes.IO
{
  /// <summary>
  /// Status values for a <see cref="PacketBuffer"/>
  /// </summary>
  public enum PacketBufferStatus
  {
    /// <summary>
    /// No data
    /// </summary>
    Empty,
    /// <summary>
    /// Data present in the packet, but not enough for a full message.
    /// </summary>
    Collating,
    /// <summary>
    /// The packet contains data for at least one full message. Call <see cref="PacketBuffer.PopPacket(out bool)"/>
    /// until the status is no longer <code>Available</code>.
    /// </summary>
    Available,
    /// <summary>
    /// Packet contains a single, complete message.
    /// </summary>
    /// <remarks>
    /// Only set when for packets when writing is finalised <see cref="PacketBuffer.FinalisePacket(bool)"/>
    /// or for a packet extracted using <see cref="PacketBuffer.PopPacket(out bool)"/>.
    /// </remarks>
    Complete,
    /// <summary>
    /// Packet error has occurred.
    /// </summary>
    Error,
    /// <summary>
    /// Calculated CRC does not match the read value.
    /// </summary>
    CrcError
  }

  /// <summary>
  /// Exception thrown when a <see cref="PacketBuffer"/> is in an invalid state for the current operation.
  /// </summary>
  public class InvalidPacketStatusException : Exception
  {
    /// <summary>
    /// The expected packet buffer status.
    /// </summary>
    public PacketBufferStatus Expected { get; protected set; }
    /// <summary>
    /// The current packet buffer status.
    /// </summary>
    public PacketBufferStatus Current { get; protected set; }

    /// <summary>
    /// Create a new invalid status exception.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="expected">Expected status.</param>
    /// <param name="current">Current status.</param>
    public InvalidPacketStatusException(string message, PacketBufferStatus expected, PacketBufferStatus current = PacketBufferStatus.Empty)
      : base(message)
    {
      Expected = expected;
      Current = current;
    }


    /// <summary>
    /// Create a new invalid status exception.
    /// </summary>
    /// <param name="expected">Expected status.</param>
    /// <param name="current">Current status.</param>
    public InvalidPacketStatusException(PacketBufferStatus expected, PacketBufferStatus current = PacketBufferStatus.Empty)
      : base(string.Format("Invalid packet status: {0}{1}", expected,
              (current != PacketBufferStatus.Empty) ? string.Format(" current {0}", current) : ""))
    {
      Expected = expected;
      Current = current;
    }
  }

  /// <summary>
  /// The <see cref="Tes.IO.PacketBuffer"/> is used to collate incoming network data
  /// and extract a <see cref="Tes.IO.PacketHeader"/> and payload.
  /// </summary>
  /// <remarks>
  /// A <code>PacketBuffer</code> can be used in two ways: input and output
  ///
  /// An input packet is used to collate data coming from a network or file stream.
  /// Bytes are read from the stream and added to the <code>PacketBuffer</code> by
  /// calling <see cref="Append(byte[], int)"/>. Individual messages are extracted using
  /// <see cref="PopPacket(out bool)"/>, which creates a new <see cref="PacketBuffer"/>
  /// containing data for a single message. Data are read from this message by
  /// obtaining a <code>Stream</code> from <see cref="CreateReadStream(bool)"/>.
  /// This is summarised below:
  ///
  /// <list type="bullet">
  /// <item>Add data by calling <see cref="Append(byte[], int)"/> until it returns <code>true</code>.</item>
  /// <item>Extract messages by calling <see cref="PopPacket(out bool)"/></item>
  /// <item>Read from extracted messages by creating a stream using <see cref="CreateReadStream(bool)"/></item>
  /// </list>
  ///
  /// Output packets are used to generate content for writing to a network or file
  /// stream. An output packet first requires a <see cref="PacketHeader"/>, which is
  /// added to the buffer via <see cref="WriteHeader(PacketHeader)"/>. Message data are then written
  /// to the buffer by calling <see cref="WriteBytes(byte[], bool, int, int)"/>. Note that data should only be added to the
  /// buffer in network byte order (Big Endian). This conversion must occur before calling
  /// <see cref="WriteBytes(byte[], bool, int, int)"/>.
  ///
  /// Once all data are written, the packet is finalised calling <see cref="FinalisePacket(bool)"/> and
  /// may be exported by calling one of the export methods: <see cref="ExportTo(BinaryWriter)"/>. Once exported
  /// a <code>PacketBuffer</code> may be reused by calling <see cref="Reset()"/> and starting the
  /// process from the beginning. Alternatively, the existing header may be preserved by calling
  /// <see cref="Reset(ushort, ushort)"/> and updating only the <see cref="PacketHeader.RoutingID"/>.
  ///
  /// The output process is summarised below:
  ///
  /// <list type="bullet">
  /// <item>Create and write a new header to the packet using <see cref="WriteHeader(PacketHeader)"/></item>
  /// <item>Write message data in network byte order using <see cref="WriteBytes(byte[], bool, int, int)"/></item>
  /// <item>Finalise the packet size and CRC using <see cref="FinalisePacket(bool)"/></item>
  /// <item>Export the packet using <see cref="ExportTo(BinaryWriter)"/></item>
  /// <item>Optionally reset and reuse the packet with <see cref="Reset(ushort, ushort)"/></item>
  /// </list>
  /// </remarks>
  public class PacketBuffer : IDisposable
  {
    /// <summary>
    /// Packet status.
    /// </summary>
    public PacketBufferStatus Status { get; protected set; }
    /// <summary>
    /// Is the header valid?
    /// </summary>
    public bool ValidHeader { get; protected set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PacketBuffer() : this(1024) {}

    /// <summary>
    /// Create a packet with the given initial buffer size.
    /// </summary>
    /// <param name="initialBufferSize">The initial buffer size (bytes)</param>
    /// <param name="useBufferPool">Allow the use of the <see cref="T:ArrayPool"/>?</param>
    public PacketBuffer(int initialBufferSize, bool useBufferPool = true)
    {
      if (initialBufferSize <= 0)
      {
        initialBufferSize = 1024;
      }
      if (useBufferPool)
      {
        _internalBuffer = ArrayPool<byte>.Shared.Rent(initialBufferSize);
        _rentedBuffer = true;
      }
      else
      {
        _internalBuffer = new byte[initialBufferSize];
        _rentedBuffer = false;
      }
      _cursor = _currentByteCount = 0;
      ValidHeader = false;
    }

    /// <summary>
    /// Returns the internal buffer pack to the <see cref="T:ArrayPool"/> when using a rented buffer.
    /// </summary>
    /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:Tes.IO.PacketBuffer"/>. The
    /// <see cref="Dispose"/> method leaves the <see cref="T:Tes.IO.PacketBuffer"/> in an unusable state. After calling
    /// <see cref="Dispose"/>, you must release all references to the <see cref="T:Tes.IO.PacketBuffer"/> so the garbage
    /// collector can reclaim the memory that the <see cref="T:Tes.IO.PacketBuffer"/> was occupying.</remarks>
    public void Dispose()
    {
      if (_rentedBuffer && _internalBuffer != null)
      {
        ArrayPool<byte>.Shared.Return(_internalBuffer);
        _internalBuffer = null;
        _rentedBuffer = false;
      }
    }

    /// <summary>
    /// The current packet header. Only relevant when <see cref="ValidHeader"/> is <c>true</c>.
    /// </summary>
    public PacketHeader Header { get { return _header; } }
    /// <summary>
    /// Returns the number of byte available in the buffer.
    /// </summary>
    public int Count { get { return _currentByteCount; } }

    /// <summary>
    /// Direct access to the internal buffer bytes.
    /// </summary>
    /// <remarks>
    /// Intended to aid in serialisation of completed packets. For example:
    /// <code>
    ///   writer.Send(packet.Data, packet.Cursor, packet.Count)
    /// </code>
    ///
    /// Use with care.
    /// </remarks>
    public byte[] Data { get { return _internalBuffer; } }
    /// <summary>
    /// Marks the start of the next packet in the buffer.
    /// </summary>
    public int Cursor { get { return _cursor; } }

    /// <summary>
    /// Tracks the number of dropped bytes.
    /// </summary>
    /// <remarks>
    /// This value is adjusted when the header marker cannot be found an identifies how many bytes are
    /// removed from the packet before finding valid header marker. The value continually accumulates
    /// but may be cleared by users of the packet buffer.
    /// </remarks>
    public int DroppedByteCount { get; set; }

    #region Output usage
    /// <summary>
    /// Full reset the packet.
    /// </summary>
    public void Reset()
    {
      _cursor = _currentByteCount = 0;
      ValidHeader = false;
      Status = PacketBufferStatus.Empty;
    }

    /// <summary>
    /// Reset the packet while maintaining the current header with a new <paramref name="routingId"/>
    /// </summary>
    /// <param name="routingId">The routing ID for the header.</param>
    /// <param name="messageId">The message ID for the router to handle.</param>
    public void Reset(ushort routingId, ushort messageId)
    {
      if (!ValidHeader)
      {
        Reset();
        PacketHeader header = PacketHeader.Default;
        header.RoutingID = routingId;
        header.MessageID = messageId;
        WriteHeader(header);
      }
      else
      {
        _cursor = _currentByteCount = 0;
        _header.RoutingID = routingId;
        _header.MessageID = messageId;
        _header.Flags = 0;
        _header.PayloadOffset = 0;
        _header.PayloadSize = 0;
        WriteHeader(_header);
      }
    }

    /// <summary>
    /// Resets the buffer and writes a new header to the packet.
    /// </summary>
    /// <param name="header"></param>
    /// <remarks>
    /// Intended for use in packet export only.
    ///
    /// The <see cref="Header"/> is set to match <paramref name="header"/>,
    /// <see cref="ValidHeader"/> becomes <code>true</code> and the status
    /// changes to <see cref="PacketBufferStatus.Collating"/>. The given
    /// <paramref name="header"/> is immediately written to the packet buffer.
    ///
    /// Message data may then be written to the buffer and the packet completed by
    /// calling <see cref="FinalisePacket(bool)"/>, which fixes the packet size.
    /// </remarks>
    public void WriteHeader(PacketHeader header)
    {
      Reset();
      _header = header;
      ValidHeader = true;
      Status = PacketBufferStatus.Collating;
      WriteBytes(BitConverter.GetBytes(_header.Marker), true);
      WriteBytes(BitConverter.GetBytes(_header.VersionMajor), true);
      WriteBytes(BitConverter.GetBytes(_header.VersionMinor), true);
      WriteBytes(BitConverter.GetBytes(_header.RoutingID), true);
      WriteBytes(BitConverter.GetBytes(_header.MessageID), true);
      // Size and offset are only place holders and are corrected in FinalisePacket()
      WriteBytes(BitConverter.GetBytes(_header.PayloadSize), true);
      // BitConverter upgrades bytes to shorts.
      //WriteBytes(BitConverter.GetBytes(_header.PayloadOffset));
      //WriteBytes(BitConverter.GetBytes(_header.Reserved));
      // Single byte values.
      WriteBytes(new byte[] { _header.PayloadOffset }, true);
      WriteBytes(new byte[] { _header.Flags }, true);
    }

    /// <summary>
    /// Write data into the packet.
    /// </summary>
    /// <param name="bytes">Data to write. Bytes are converted to network Endian (Big Endian)</param>
    /// <param name="toNetworkEndian">When <code>true</code>, <paramref name="bytes"/> are converted
    /// to network Endian (Big Endian), otherwise <paramref name="bytes"/> are written as is.</param>
    /// <param name="offset">Offset from the start of <paramref name="bytes"/> to start writing from.</param>
    /// <param name="length">The number of bytes to write. Zero for all bytes from <paramref name="offset"/>
    /// to the end of <paramref name="bytes"/>.</param>
    /// <remarks>
    /// The buffer size is increased if required.
    /// </remarks>
    public void WriteBytes(byte[] bytes, bool toNetworkEndian, int offset = 0, int length = 0)
    {
      EnsureBufferCapacity(_cursor + _currentByteCount + bytes.Length);
      length = Math.Max(0, (length > 0) ? length : bytes.Length - offset);
      Array.Copy((toNetworkEndian) ? Endian.ToNetwork(bytes) : bytes, offset, _internalBuffer, _cursor + _currentByteCount, length);
      _currentByteCount += bytes.Length;
    }

    /// <summary>
    /// Finalise an output packet.
    /// </summary>
    /// <param name="addCrc">True to calculate and add the CRC.</param>
    /// <remarks>
    /// Finalises an output packet by updating the packet size, calculating the
    /// CRC and writing the CRC to the buffer. The buffer is ready for <see cref="ExportTo(BinaryWriter)"/>
    /// </remarks>
    public bool FinalisePacket(bool addCrc = true)
    {
      if (!ValidHeader)
      {
        return false;
      }

      // Calculate the packet CRC.
      // Fix up the payload size.
      _header.PayloadSize = (ushort)(Math.Max(0, _currentByteCount - PacketHeader.Size));
      if (_header.PayloadSize != _currentByteCount - PacketHeader.Size)
      {
        // Payload is too large.
        return false;
      }
      byte[] sizeBytes = BitConverter.GetBytes(Endian.ToNetwork(_header.PayloadSize));
      for (int i = 0; i < sizeBytes.Length; ++i)
      {
        _internalBuffer[i + PacketHeader.PayloadSizeOffset] = sizeBytes[i];
      }

      // Calculate the CRC.
      if (addCrc)
      {
        ushort crc = Crc16.Crc.Calculate(_internalBuffer, (uint)_currentByteCount);
        // Don't do Endian swap here. WriteBytes does it.
        WriteBytes(BitConverter.GetBytes(crc), true);
      }
      else
      {
        // Add the NoCrc flag.
        _internalBuffer[PacketHeader.FlagsOffset] |= (byte)PacketFlag.NoCrc;
      }
      Status = PacketBufferStatus.Complete;
      return true;
    }

    /// <summary>
    /// Exports the packet contents to the given <code>BinaryWriter</code>.
    /// </summary>
    /// <param name="writer">The <code>BinaryWriter</code> to export available bytes to.</param>
    /// <returns>The number of bytes written</returns>
    /// <remarks>
    /// Export does not validate the packet status, exporting available bytes as is.
    /// </remarks>
    /// <exception cref="InvalidPacketStatusException">Thrown when the status is not <see cref="PacketBufferStatus.Complete"/>.</exception>
    public int ExportTo(BinaryWriter writer)
    {
      if (Status != PacketBufferStatus.Complete)
      {
        throw new InvalidPacketStatusException(PacketBufferStatus.Complete, Status);
      }
      if (_currentByteCount > 0)
      {
        writer.Write(_internalBuffer, _cursor, _currentByteCount - _cursor);
      }
      return _currentByteCount;
    }

    /// <summary>
    /// Exports the packet contents to the given <code>Stream</code>.
    /// </summary>
    /// <param name="stream">The <code>Stream</code> to export available bytes to.</param>
    /// <returns>The number of bytes written</returns>
    /// <remarks>
    /// Export does not validate the packet status, exporting available bytes as is.
    /// </remarks>
    /// <exception cref="InvalidPacketStatusException">Thrown when the status is not <see cref="PacketBufferStatus.Complete"/>.</exception>
    public int ExportTo(Stream stream)
    {
      if (Status != PacketBufferStatus.Complete)
      {
        throw new InvalidPacketStatusException(PacketBufferStatus.Complete, Status);
      }
      if (_currentByteCount > 0)
      {
        stream.Write(_internalBuffer, _cursor, _currentByteCount - _cursor);
      }
      return _currentByteCount;
    }

    #endregion

    #region Input usage

    /// <summary>
    /// Peek at two bytes in the message content.
    /// </summary>
    /// <param name="offset">Offset from the packet start to peek at.</param>
    /// <returns>The peeked value or zero when <paramref name="offset"/> is out of range.</returns>
    /// <remarks>
    /// Two bytes are read from the message at the given offset and converted from network to local Endian.
    /// </remarks>
    public ushort PeekUInt16(int offset)
    {
      if (0 <= offset && offset < _currentByteCount)
      {
        return Endian.FromNetwork(BitConverter.ToUInt16(_internalBuffer, offset));
      }
      return 0;
    }

    /// <summary>
    /// Peek at four bytes in the message content.
    /// </summary>
    /// <param name="offset">Offset from the packet start to peek at.</param>
    /// <returns>The peeked value or zero when <paramref name="offset"/> is out of range.</returns>
    /// <remarks>
    /// Four bytes are read from the message at the given offset and converted from network to local Endian.
    /// </remarks>
    public uint PeekUInt32(int offset)
    {
      if (0 <= offset && offset < _currentByteCount)
      {
        return Endian.FromNetwork(BitConverter.ToUInt32(_internalBuffer, offset));
      }
      return 0;
    }

    /// <summary>
    /// Peek at four bytes in the message content.
    /// </summary>
    /// <param name="offset">Offset from the packet start to peek at.</param>
    /// <returns>The peeked value or zero when <paramref name="offset"/> is out of range.</returns>
    /// <remarks>
    /// Four bytes are read from the message at the given offset and converted from network to local Endian.
    /// </remarks>
    public ulong PeekUInt64(int offset)
    {
      if (0 <= offset && offset < _currentByteCount)
      {
        return Endian.FromNetwork(BitConverter.ToUInt64(_internalBuffer, offset));
      }
      return 0;
    }

    /// <summary>
    /// Peek at a number of bytes in the message content.
    /// </summary>
    /// <param name="offset">Offset from the packet start to peek at.</param>
    /// <param name="bytes">The byte stream to write to.</param>
    /// <param name="byteCount">The number of bytes to read.</param>
    /// <returns>True when <paramref name="offset"/> the <c>byteCount</c> are both in range.</returns>
    /// <remarks>
    /// The read bytes are left in network Endian.
    /// </remarks>
    public bool PeekBytes(int offset, byte[] bytes, int byteCount)
    {
      if (byteCount > 0 && 0 <= offset && offset + bytes.Length < _currentByteCount)
      {
        Array.Copy(_internalBuffer, offset, bytes, 0, byteCount);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Instantiates a memory stream which reads the bytes in this packet buffer.
    /// </summary>
    /// <returns>The read only memory stream.</returns>
    /// <param name="skipHeader">True to skip the header bytes, moving on to the payload.
    /// Includes respecting the payload offset.</param>
    public Stream CreateReadStream(bool skipHeader)
    {
      int skipBytes = 0;
      if (skipHeader)
      {
        skipBytes += PacketHeader.Size + Header.PayloadOffset;
      }

      return new MemoryStream(_internalBuffer, skipBytes, _currentByteCount - skipBytes, false);
    }

    /// <summary>
    /// Appends data to the internal buffer to complete the current message.
    /// Returns a <see cref="PacketBuffer"/> with a completed message if
    /// <paramref name="bytes"/> completes the current message.
    /// </summary>
    /// <param name="bytes">The data stream to append.</param>
    /// <param name="available">The number of bytes from <paramref name="bytes"/> to append.
    /// This allows the given buffer to be larger than the available data.</param>
    /// <returns>True if there is a completed packet available.</returns>
    /// <remarks>
    /// As data bytes are appended, the buffer code searches for a valid header,
    /// consuming all bytes until a valid header is found - i.e., bytes before the
    /// valid header are lost.
    ///
    /// Once a valid header is found, <code>Append()</code> waits for sufficient bytes
    /// to complete the packet as specified in the validated header. At this point
    /// <code>Append()</code> returns <code>true</code>. Available packets
    /// may be extracted by calling <see cref="PopPacket(out bool)"/> until that method
    /// returns null.
    ///
    /// Calling <code>Append()</code> will change the <see cref="PacketBuffer.Status"/>
    /// as follows:
    /// <list type="table">
    /// <listheader><term>Status</term><description>Conditions</description></listheader>
    /// <item>
    ///   <term><code>Empty</code></term>
    ///   <description>
    ///     The append call did not add any data, or an invalid header has been consumed
    ///     leaving the buffer empty.
    ///   </description>
    /// </item>
    /// <item>
    ///   <term><code>Collating</code></term>
    ///   <description>
    ///     Bytes have been appended, but no complete packet is available yet.
    ///     The header may be valid: check <see cref="ValidHeader"/>
    ///   </description>
    /// </item>
    /// <item>
    ///   <term><code>Available</code></term>
    ///   <description>
    ///     A header has been validated and sufficient bytes are present to extract
    ///     a packet. Call <see cref="PopPacket(out bool)"/>.
    ///   </description>
    /// </item>
    /// </list>"
    /// </remarks>
    public bool Append(byte[] bytes, int available)
    {
      // Reset the read cursor
      ResetCursor();
      EnsureBufferCapacity(_cursor + _currentByteCount + available);
      Array.Copy(bytes, 0, _internalBuffer, _currentByteCount, available);
      _currentByteCount += available;
      if (!ValidHeader)
      {
        ValidateHeader();
      }
      Status = (_currentByteCount > 0) ? PacketBufferStatus.Collating : PacketBufferStatus.Empty;

      if (ValidHeader)
      {
        int crcSize = ((_header.Flags & (byte)PacketFlag.NoCrc) == 0) ? Crc16.CrcSize : 0;
        if (_currentByteCount >= _header.PacketSize + crcSize)
        {
          Status = PacketBufferStatus.Available;
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// An overload supporting reading bytes from a <code>Stream</code> instead of a byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="available">The number of bytes available to read from the stream.</param>
    /// <returns></returns>
    /// <remarks>
    /// See <see cref="Append(byte[], int)"/>.
    /// </remarks>
    public bool Append(Stream stream, int available)
    {
      ResetCursor();
      EnsureBufferCapacity(_cursor + _currentByteCount + available);
      _currentByteCount += stream.Read(_internalBuffer, _currentByteCount, available);
      if (!ValidHeader)
      {
        ValidateHeader();
      }
      Status = (_currentByteCount > 0) ? PacketBufferStatus.Collating : PacketBufferStatus.Empty;

      if (ValidHeader)
      {
        int crcSize = ((_header.Flags & (byte)PacketFlag.NoCrc) == 0) ? Crc16.CrcSize : 0;
        if (_currentByteCount >= _header.PacketSize + crcSize)
        {
          Status = PacketBufferStatus.Available;
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Pops the next available packet from the internal buffer.
    /// </summary>
    /// <param name="crcOk">Exposes the results of the CRC check. Always true when a valid packet
    /// is returned. Also true when no packet is available. Only false when a completed packet is
    /// available, but the CRC fails.</param>
    /// <returns>The next available packet or null if not available or the CRC check fails.</returns>
    /// <remarks>
    /// A <code>PacketBuffer</code> accumulating packets using <see cref="Append(byte[], int)"/> may
    /// contain at least one completed packet. This method is used to extract each
    /// packet as a self contained <code>PacketBuffer</code>. When completed packets
    /// are available, this method returns each available completed packet as a single,
    /// self contained item.
    ///
    /// The bytes making up the completed packet are removed from this packet buffer
    /// while excess bytes are preserved.
    ///
    /// A null packet buffer may also be returned when there is enough data for a packet,
    /// but the CRC check fails. This case is detected when null is returned as <paramref name="crcOk"/>
    /// is <code>false</code> (failed CRC) as opposed to null returned and <paramref name="crcOk"/>
    /// <code>true</code> (no packet).
    /// </remarks>
    public PacketBuffer PopPacket(out bool crcOk)
    {
      crcOk = true;
      if (!ValidHeader)
      {
        // Incomplete header data.
        return null;
      }

      int totalPacketSize = _header.PacketSize;
      if ((_header.Flags & (byte)PacketFlag.NoCrc) == 0)
      {
        totalPacketSize += Crc16.CrcSize;
      }
      if (_currentByteCount < totalPacketSize)
      {
        // Not enough data to validate yet.
        return null;
      }

      PacketBuffer completedPacket = null;
      crcOk = CheckCrc(totalPacketSize);
      // Create the packet if CRC is OK.
      if (crcOk)
      {
        completedPacket = ExtractCompletedPacket(totalPacketSize, crcOk);
      }
      else
      {
        // Failed CRC. Consume the packet.
        _cursor += totalPacketSize;
        _currentByteCount -= totalPacketSize;
      }

      // Try validate the header for the next message.
      ValidHeader = false;
      ValidateHeader();

      // Consume the packet data regardless of CRC.
      return completedPacket;
    }

    /// <summary>
    /// Emplace bytes within the buffer, returning true if this completes the packet.
    /// </summary>
    /// <remarks>
    /// This is equivalent to <see cref="Append(byte[], int)"/>, except this object represents
    /// the completed packet.
    /// </remarks>
    /// <param name="bytes">The data stream to append.</param>
    /// <param name="available">The number of bytes from <paramref name="bytes"/> to append.
    /// This allows the given buffer to be larger than the available data.</param>
    /// <returns>The number of bytes added from <paramref name="bytes"/>.</returns>
    public int Emplace(byte[] bytes, int available)
    {
      return Emplace(bytes, 0, available);
    }

    /// <summary>
    /// Emplace bytes within the buffer, returning true if this completes the packet.
    /// </summary>
    /// <remarks>
    /// This is equivalent to <see cref="Append(byte[], int)"/>, except this object represents
    /// the completed packet.
    /// </remarks>
    /// <param name="bytes">The data stream to append.</param>
    /// <param name="offset">Byte offset into <paramref name="bytes"/> to start reading from.</param>
    /// <param name="length">The number of bytes from <paramref name="bytes"/> to append.
    /// This allows the given buffer to be larger than the available data.</param>
    /// <returns>The number of bytes added from <paramref name="bytes"/>.</returns>
    public int Emplace(byte[] bytes, int offset, int length)
    {
      ResetCursor();
      EnsureBufferCapacity(_cursor + _currentByteCount + length);
      Array.Copy(bytes, offset, _internalBuffer, _currentByteCount, length);
      _currentByteCount += length;
      CompleteEmplace(length);
      return length;
    }

    /// <summary>
    /// Emplace bytes from a stream.
    /// </summary>
    /// <remarks>
    /// This is equivalent to <see cref="Emplace(byte[], int)"/> except for the data source.
    /// </remarks>
    /// <param name="stream">The stream to read bytes from.</param>
    /// <param name="available">The number of bytes to read from <paramref name="stream"/>.</param>
    /// <returns>The number of bytes added from <paramref name="stream"/>.</returns>
    public int Emplace(Stream stream, int available)
    {
      ResetCursor();
      EnsureBufferCapacity(_cursor + _currentByteCount + available);
      int addedBytes = stream.Read(_internalBuffer, _currentByteCount, available);
      _currentByteCount += addedBytes;
      CompleteEmplace(available);
      return addedBytes;
    }

    #endregion

    #region Input internal

    /// <summary>
    /// Complete an <see cref="Append(byte[], int)"/> operation, checking for a valid header and/or completed packet.
    /// </summary>
    /// <returns>True if the appended data completes the packet currently being assembled.</returns>
    /// <param name="appendedByteCount">The number of bytes that were just appended to the internal buffer.</param>
    private bool CompleteAppend(int appendedByteCount)
    {
      if (!ValidHeader)
      {
        if (_currentByteCount == appendedByteCount)
        {
          // This is the first data in the buffer. Validate that we have the start of a message.
          ValidateHeader();
        }
      }

      // Header may have become valid.
      if (ValidHeader)
      {
        int crcSize = ((_header.Flags & (byte)PacketFlag.NoCrc) == 0) ? Crc16.CrcSize : 0;
        if (_currentByteCount >= _header.PacketSize + crcSize)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Complete an <see cref="Emplace(byte[], int)"/> operation, checking for a valid header and/or completed packet.
    /// </summary>
    /// <returns>True if the emplaced data completes the packet currently being assembled.</returns>
    /// <param name="appendedByteCount">The number of bytes that were just appended to the internal buffer.</param>
    private bool CompleteEmplace(int appendedByteCount)
    {
      if (!ValidHeader)
      {
        if (_currentByteCount == appendedByteCount)
        {
          // This is the first data in the buffer. Validate that we have the start of a message.
          ValidateHeader();
        }
      }

      // May have become valid.
      if (ValidHeader)
      {
        int totalPacketSize;
        bool crcOk;
        if (CheckPacketCompletion(out totalPacketSize, out crcOk))
        {
          // Enough data for a packet. Set CRC status.
          Status = (crcOk) ? PacketBufferStatus.Complete : PacketBufferStatus.CrcError;
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Check the buffer contents looking for a valid header.
    /// </summary>
    /// <remarks>
    /// If there are sufficient bytes for a header, the buffer is searched for the expected
    /// <see cref="PacketHeader.Marker"/> followed by sufficient bytes to complete the <see cref="PacketHeader"/>
    /// and <see cref="ValidHeader"/> is set to <code>true</code>. Note how this consumes bytes
    /// before the marker.
    ///
    /// Bytes are also consumed when the marker cannot be found, appropriately adjusting
    /// <see cref="DroppedByteCount"/>.
    /// </remarks>
    private void ValidateHeader()
    {
      // First check that we have enough bytes for the header.
      // Loop through available bytes in case we have bad data to consume.
      int offset = 0;
      ValidHeader = false;
      UInt32 marker = 0;
      while (!ValidHeader && _currentByteCount - offset >= PacketHeader.Size)
      {
        marker = Endian.FromNetwork(BitConverter.ToUInt32(_internalBuffer, _cursor + offset));
        if (marker == PacketHeader.PacketMarker)
        {
          // We have enough to extract a header.
          NetworkReader reader = new NetworkReader(new System.IO.MemoryStream(_internalBuffer, _cursor + offset, _currentByteCount - offset, false));
          if (_header.Read(reader))
          {
            ValidHeader = true;
          }
          else
          {
            ++offset;
          }
        }
        else
        {
          ++offset;
        }
      }

      // Consume bad bytes.
      if (offset != 0)
      {
        _cursor += offset;
        _currentByteCount -= offset;
        DroppedByteCount += offset;
      }

      //if (_currentByteCount >= _header.PacketSize)
      //{
      //  Status = PacketBufferStatus.Complete;
      //}
      if (ValidHeader)
      {
        Status = PacketBufferStatus.Collating;
      }
      else // if (ValidHeader)
      {
        Status = PacketBufferStatus.Empty;
      }
    }

    /// <summary>
    /// Checks if we have a completed packet, returning true if we have sufficient data for
    /// a full packet.
    /// </summary>
    /// <param name="totalPacketSize">Set to the total size of the packet including header, payload and CRC.</param>
    /// <param name="crcOk">Set to <c>true</c> if the CRC is OK, <c>false</c> if the CRC failed, or the packet
    ///   is incomplete.</param>
    /// <returns><c>true</c> when there is sufficient data to complete a packet and it should be consumed.</returns>
    private bool CheckPacketCompletion(out int totalPacketSize, out bool crcOk)
    {
      totalPacketSize = _header.PacketSize;
      if ((_header.Flags & (byte)PacketFlag.NoCrc) == 0)
      {
        totalPacketSize += Crc16.CrcSize;
      }
      crcOk = false;
      if (ValidHeader && _currentByteCount >= totalPacketSize)
      {
        crcOk = CheckCrc(totalPacketSize);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Extract a completed packet from the buffer.
    /// </summary>
    /// <remarks>
    /// A valid packet has to already be identified and validated.
    /// </remarks>
    /// <param name="packetTotalSize">Number of bytes to extract.</param>
    /// <param name="crcOk">Has CRC validation passed?</param>
    /// <param name="invalidate">Invalidate the buffer state afterwards to allow searching for
    ///   a new packet?</param>
    /// <returns>The extracted packet.</returns>
    private PacketBuffer ExtractCompletedPacket(int packetTotalSize, bool crcOk, bool invalidate = true)
    {
      // Create a copy of the current buffer.
      PacketBuffer packet = new PacketBuffer(packetTotalSize);
      packet._header = _header;
      _header = new PacketHeader();
      // Copy to the new packet.
      Array.Copy(_internalBuffer, _cursor, packet._internalBuffer, 0, packetTotalSize);
      // Move the read cursor.
      _cursor += packetTotalSize;
      // Consume the bytes.
      _currentByteCount -= packetTotalSize;

      packet._currentByteCount = packetTotalSize;
      packet.Status = (crcOk) ? PacketBufferStatus.Complete : PacketBufferStatus.CrcError;

      if (invalidate)
      {
        // Look for a new completed packet.
        ValidHeader = false;
      }
      return packet;
    }

    /// <summary>
    /// Calculates the CRC for <paramref name="packetSize"/> and checks it against
    /// what is in the buffer.
    /// </summary>
    /// <param name="packetSize">The number of bytes in the pending packet including the CRC.</param>
    /// <returns><c>true</c>, the calculated CRC matches the reported CRC</returns>
    /// <remarks>
    /// The expected <paramref name="packetSize"/> includes the two CRC bytes at the
    /// very end of the buffer.
    /// </remarks>
    private bool CheckCrc(int packetSize)
    {
      // Check the CRC flag.
      byte packetFlags = _internalBuffer[_cursor + PacketHeader.FlagsOffset];
      if ((packetFlags & (byte)PacketFlag.NoCrc) == 0)
      {
        ushort crc = Crc16.Crc.Calculate(_internalBuffer, (uint)_cursor, (uint)(packetSize - Crc16.CrcSize));
        byte[] crcBytes = new byte[2];
        crcBytes[0] = _internalBuffer[_cursor + packetSize - 2];
        crcBytes[1] = _internalBuffer[_cursor + packetSize - 1];
        ushort packetCrc = Endian.FromNetwork(BitConverter.ToUInt16(crcBytes, 0));
        return packetCrc == crc;
      }
      return true;
    }

    #endregion

    #region Buffer management

    /// <summary>
    /// Move available bytes in the buffer such that the read cursor is zero.
    /// </summary>
    private void ResetCursor()
    {
      if (_cursor > 0 && _currentByteCount > 0)
      {
        // Move the available bytes.
        Array.Copy(_internalBuffer, _cursor, _internalBuffer, 0, _currentByteCount);
      }
      _cursor = 0;
    }

    /// <summary>
    /// Ensure the internal buffer has sufficient capacity to hold <paramref name="required"/> bytes
    /// </summary>
    /// <param name="required">The number of bytes required</param>
    /// <remarks>
    /// The buffer increases in size using powers of 2.
    /// </remarks>
    private void EnsureBufferCapacity(int required)
    {
      if (required > _internalBuffer.Length)
      {
        int newSize = Maths.IntUtil.NextPowerOf2(required);
        if (_rentedBuffer)
        {
          // Rented buffer. Return the buffer and ask for a buffer one.
          byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
          Array.Copy(_internalBuffer, newBuffer, _internalBuffer.Length);
          ArrayPool<byte>.Shared.Return(_internalBuffer);
          _internalBuffer = newBuffer;
        }
        else
        {
          // Need to resize the internal buffer.
          Array.Resize(ref _internalBuffer, newSize);
        }
      }
    }

    #endregion

    /// <summary>
    /// Used to decode packet headers.
    /// </summary>
    private PacketHeader _header = new PacketHeader();
    /// <summary>
    /// Internal byte buffer.
    /// </summary>
    private byte[] _internalBuffer;
    /// <summary>
    /// True if the <see cref="_internalBuffer"/> has been rented from <see cref="T:ArrayPool" /> and needs
    /// to be returned.
    /// </summary>
    private bool _rentedBuffer;
    /// <summary>
    /// Read cursor, tracking the start of valid bytes. Reset on <see cref="Append(byte[], int)"/>
    /// or <see cref="Emplace(byte[], int)"/>.
    /// </summary>
    private int _cursor;
    /// <summary>
    /// Number of bytes added to the buffer.
    /// </summary>
    private int _currentByteCount;
  }
}
