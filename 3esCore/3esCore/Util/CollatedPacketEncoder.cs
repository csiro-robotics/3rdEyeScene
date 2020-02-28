using System;
using System.IO;
using System.IO.Compression;
using Tes.IO;
using Tes.Net;
using Tes.Shapes;

namespace Tes.Util
{
  /// <summary>
  /// A utility class for encoding <see cref="CollatedPacketMessage"/> packets, including compression.
  /// </summary>
  /// <remarks>
  /// Typical usage:
  /// <list type="bullet">
  /// <item>Instantiate the packet encoder.</item>
  /// <item>Reset the encoder.</item>
  /// <item>For each constituent message:</item>
  /// <list type="bullet">
  /// <item>Generate the packet using <see cref="PacketBuffer"/></item>
  /// <item>Finalise the packet</item>
  /// <item>Call <see cref="Add(PacketBuffer)"/></item>
  /// </list>
  /// <item>Call <see cref="FinaliseEncoding()"/> on the encoder</item>
  /// <item>Send the encoded packet.</item>
  /// <item>Reset the encoder.</item>
  /// </list>
  ///
  /// See <see cref="CollatedPacketDecoder"/> for notes on why <code>System.IO.Compression</code>
  /// is not used.
  ///
  /// Derives the <see cref="IConnection"/> interface for compatibility.
  /// </remarks>
  public class CollatedPacketEncoder : IConnection
  {
    /// <summary>
    /// Byte count overhead added by using a collated packet.
    /// </summary>
    /// <remarks>
    /// This is the sum of <see cref="PacketHeader"/>, <see cref="CollatedPacketMessage"/>
    /// and the <see cref="Crc16"/> value type.
    /// </remarks>
    public static int Overhead { get { return PacketHeader.Size + CollatedPacketMessage.Size + Crc16.CrcSize; } }
    /// <summary>
    /// The default packet size limit for a <see cref="CollatedPacketMessage"/>.
    /// </summary>
    public static ushort MaxPacketSize { get { return (ushort)0xffffu; } }

    /// <summary>
    /// Create an encoder with the target initial buffer size.
    /// </summary>
    /// <param name="compress">True to compress collated data.</param>
    /// <param name="initialBufferSize">The initial buffer size (bytes).</param>
    public CollatedPacketEncoder(bool compress, int initialBufferSize = 64 * 1024)
    {
      CollatedBytes = 0;
      _collationStream = _dataStream = new CompressionBuffer(initialBufferSize);
      _packet = new PacketBuffer();

      // Prime the output stream, writing the initial header.
      PacketHeader header = PacketHeader.Create((ushort)RoutingID.CollatedPacket, 0);

      CollatedPacketMessage msg = new CollatedPacketMessage();
      msg.Flags = (ushort)((compress) ? CollatedPacketFlag.GZipCompressed : 0u);
      msg.Reserved = 0;
      msg.UncompressedBytes = 0;

      NetworkWriter writer = new NetworkWriter(_dataStream);
      header.Write(writer);
      msg.Write(writer);

      _resetPosition = (int)_dataStream.Position;

      CompressionEnabled = compress;
      if (compress)
      {
        _collationStream = new GZipStream(_dataStream, CompressionLevel.Fastest);
      }
    }

    /// <summary>
    /// True if created with compression.
    /// </summary>
    public bool CompressionEnabled { get; protected set; }
    /// <summary>
    /// Direct access to the internal buffer bytes.
    /// </summary>
    /// <remarks>
    /// Intended to aid in serialisation of completed packets. For example:
    /// <code>
    ///   writer.Send(packet.Buffer, 0, packet.Count)
    /// </code>
    ///
    /// Use with care.
    /// </remarks>
    public byte[] Buffer { get { return _dataStream.BaseStream.GetBuffer(); } }
    /// <summary>
    /// The total number of bytes written to <see cref="Buffer"/>.
    /// </summary>
    public int Count { get { return (int)_dataStream.Position; } }
    /// <summary>
    /// The number of bytes written to <see cref="Buffer"/> excluding the <see cref="Overhead"/>.
    /// </summary>
    public int CollatedBytes { get; protected set; }

    /// <summary>
    /// Reset the buffer to start again.
    /// </summary>
    public void Reset()
    {
      if (!_finalised)
      {
        FinaliseEncoding();
      }
      _finalised = false;
      CollatedBytes = 0;
      SetPayloadSize(0);
      SetUncompressedBytesSize(0);
      _dataStream.Seek(_resetPosition, SeekOrigin.Begin);
      if (CompressionEnabled)
      {
        _collationStream = new GZipStream(_dataStream, CompressionLevel.Fastest);
      }
    }

    /// <summary>
    /// Add the given packet.
    /// </summary>
    /// <param name="packet">The buffer to collate.</param>
    /// <returns>The number of (uncompressed) bytes added.</returns>
    public int Add(PacketBuffer packet)
    {
      if (_finalised)
      {
        // TODO: throw exception.
        return -1;
      }
      return Add(packet.Data, 0, packet.Cursor);
    }

    /// <summary>
    /// Add bytes from the given buffer.
    /// </summary>
    /// <param name="bytes">Buffer to add from.</param>
    /// <param name="offset">Offset to the first byte in <paramref name="bytes"/> to add.</param>
    /// <param name="length">Number of bytes from buffer to add.</param>
    /// <returns>The number of (uncompressed) bytes added.</returns>
    public int Add(byte[] bytes, int offset, int length)
    {
      if (_finalised)
      {
        // TODO: throw exception.
        return -1;
      }

      if (length <= 0)
      {
        return 0;
      }

      // Check total size capacity.
      if (Count + length + Overhead > MaxPacketSize)
      {
        // Too many bytes to collate.
        return -1;
      }

      _collationStream.Write(bytes, offset, length);
      CollatedBytes += length;
      return length;
    }

    /// <summary>
    /// Finalise the collated packet before sending.
    /// </summary>
    /// <returns>True on successful finalisation, false if already finalised</returns>
    public bool FinaliseEncoding()
    {
      if (_finalised)
      {
        return false;
      }

      // Finalise compression. Stream must be closed.
      _collationStream.Close();
      // Ensure a valid state.
      _collationStream = _dataStream;
      //SetPayloadSize((ushort)(Count + CollatedPacketMessage.Size));
      SetPayloadSize((ushort)(Count - PacketHeader.Size));
      // Update the payload and uncompressed sizes.
      SetUncompressedBytesSize((ushort)CollatedBytes);

      // Calculate the CRC
      ushort crc = Crc16.Crc.Calculate(_dataStream.BaseStream.GetBuffer(), (uint)_dataStream.Position);
      new NetworkWriter(_dataStream).Write(crc);
      _finalised = true;
      return true;
    }

    #region IConnection methods
    /// <summary>
    /// Identifies as "CollatedPacket".
    /// </summary>
    public string Address { get { return "CollatedPacket"; } }

    /// <summary>
    /// Always zero.
    /// </summary>
    public int Port { get { return 0; } }

    /// <summary>
    /// Always true.
    /// </summary>
    public bool Connected { get { return true; } }

    /// <summary>
    /// Ignored.
    /// </summary>
    public void Close() { }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="flush"></param>
    /// <returns></returns>
    public int UpdateFrame(float dt, bool flush) { return -1; }

    /// <summary>
    /// Send the create message for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns></returns>
    public int Create(Shape shape)
    {
      if (shape.WriteCreate(_packet))
      {
        _packet.FinalisePacket();
        int writeSize = Add(_packet);

        if (shape.IsComplex)
        {
          uint progress = 0;
          int res = 0;
          while ((res = shape.WriteData(_packet, ref progress)) >= 0)
          {
            if (!_packet.FinalisePacket())
            {
              return -1;
            }

            writeSize += Add(_packet);

            if (res == 0)
            {
              break;
            }
          }

          if (res < 0)
          {
            return res;
          }
        }
        // No resource handling in collated packets when using the IConnection interface..
        return writeSize;
      }
      return 0;
    }

    /// <summary>
    /// Send the create message for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns></returns>
    public int Destroy(Shape shape)
    {
      if (shape.WriteDestroy(_packet))
      {
        _packet.FinalisePacket();
        return Add(_packet);
      }
      return 0;
    }

    /// <summary>
    /// Send the create message for <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns></returns>
    public int Update(Shape shape)
    {
      if (shape.WriteUpdate(_packet))
      {
        _packet.FinalisePacket();
        return Add(_packet);
      }
      return 0;
    }

    /// <summary>
    /// Sends the <see cref="ServerInfoMessage"/> structure to the connected client.
    /// </summary>
    /// <param name="info">The info message to send.</param>
    /// <returns>True on success.</returns>
    public bool SendServerInfo(ServerInfoMessage info)
    {
      _packet.Reset((ushort)RoutingID.ServerInfo, 0);
      if (info.Write(_packet))
      {
        _packet.FinalisePacket();
        return Add(_packet) > 0;
      }
      return false;
    }

    /// <summary>
    /// Sends data on the client connection. Aliases <see cref="Add(byte[], int, int)"/>.
    /// </summary>
    /// <param name="bytes">The data buffer to send.</param>
    /// <param name="offset">An offset into <paramref name="bytes"/> at which to start sending.</param>
    /// <param name="length">The number of bytes to transfer.</param>
    /// <param name="allowCollation">Ignored</param>
    /// <returns>The number of bytes transferred or -1 on failure.</returns>
    public int Send(byte[] bytes, int offset, int length, bool allowCollation)
    {
      return Add(bytes, offset, length);
    }

    /// <summary>
    /// Not supported for packet collation.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns>0</returns>
    public uint GetReferenceCount(Resource resource)
    {
      return 0;
    }

    /// <summary>
    /// Not supported for packet collation. Throws <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="resource"></param>
    public uint AddResource(Resource resource)
    {
      throw new NotSupportedException("Resources not supported for collated packets.");
    }

    /// <summary>
    /// Not supported for packet collation. Throws <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="resource"></param>
    public uint RemoveResource(Resource resource)
    {
      throw new NotSupportedException("Resources not supported for collated packets.");
    }

    #endregion

    /// <summary>
    /// Sets the <see cref="PacketHeader.PayloadSize"/> value.
    /// </summary>
    /// <param name="size">The packet payload size (compressed).</param>
    protected void SetPayloadSize(ushort size)
    {
      // Write to the payload size bytes.
      int offset = PacketHeader.PayloadSizeOffset;
      WriteHeaderData(offset, BitConverter.GetBytes(Endian.ToNetwork(size)));
    }

    /// <summary>
    /// Sets the <see cref="CollatedPacketMessage.UncompressedBytes"/> value.
    /// </summary>
    /// <param name="size">The number of uncompressed in the payload.</param>
    protected void SetUncompressedBytesSize(uint size)
    {
      // Write to the uncompressed size bytes.
      int offset = PacketHeader.Size + CollatedPacketMessage.UncompressedBytesOffset;
      WriteHeaderData(offset, BitConverter.GetBytes(Endian.ToNetwork(size)));
    }

    /// <summary>
    /// Write data into the header section at the given offset.
    /// </summary>
    /// <param name="dstOffset">Buffer offset to write at.</param>
    /// <param name="bytes">Bytes to write.</param>
    protected void WriteHeaderData(int dstOffset, byte[] bytes)
    {
      long restorePos = _dataStream.Position;
      _dataStream.Seek(dstOffset, SeekOrigin.Begin);
      _dataStream.Write(bytes, 0, bytes.Length);
      _dataStream.Seek(restorePos, SeekOrigin.Begin);
    }

    /// <summary>
    /// Buffer stream to write into
    /// </summary>
    /// <remarks>
    /// Matches <see cref="_dataStream"/> when uncompressed. Otherwise it wraps
    /// <see cref="_dataStream"/> in a GZip stream.
    /// </remarks>
    private Stream _collationStream = null;
    /// <summary>
    /// Internal memory buffer.
    /// </summary>
    private CompressionBuffer _dataStream = null;
    /// <summary>
    /// Buffer used in methods implementing the <see cref="IConnection"/> interface.
    /// </summary>
    private PacketBuffer _packet = null;
    /// <summary>
    /// Position to seek to on reset.
    /// </summary>
    private int _resetPosition = 0;
    /// <summary>
    /// Finalisation flag.
    /// </summary>
    private bool _finalised = false;
  }
}
