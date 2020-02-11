using System.IO;
using System.IO.Compression;
using Tes.Net;

namespace Tes.IO
{
  /// <summary>
  /// A utility class for decoding <see cref="CollatedPacketMessage"/> packets.
  /// </summary>
  /// <remarks>
  /// Unity does not support classes from <code>System.IO.Compression</code>, thus we
  /// can't use <code>GZipStream</code> from there directly. Whilst .Net Core source code is available
  /// and can be ported, the managed implementation supports only one, default compression level.
  /// The default .Net Core GZip implementation uses native ZLib, which cannot be supported herre to maintain
  /// higher platform independence. In the end we use a DotNetZip NuGet package.
  /// </remarks>
  public class CollatedPacketDecoder
  {
    /// <summary>
    /// True while we have more packets to decode/extract.
    /// </summary>
    public bool Decoding { get { return _packet != null; } }

    /// <summary>
    /// Sets the packet to decode.
    /// </summary>
    /// <param name="packet"></param>
    /// <remarks>
    /// The <paramref name="packet"/> need not be a collated packet, in which case it will be
    /// immediately returned on calling <see cref="Next()"/>, followed by <code>null</code>
    /// </remarks>
    public bool SetPacket(PacketBuffer packet)
    {
      _packet = packet;
      _packetStream = null;
      _streamReader = null;

      // Check for a collated packet.
      if (_packet != null && _packet.Header.RoutingID == (ushort)RoutingID.CollatedPacket)
      {
        _packetStream = _packet.CreateReadStream(true);
        CollatedPacketMessage msg = new CollatedPacketMessage();
        _streamReader = new NetworkReader(_packetStream);
        if (!msg.Read(_streamReader))
        {
          return false;
        }

        _targetBytes = msg.UncompressedBytes;
        _decodedBytes = 0;

        if ((msg.Flags & (ushort)CollatedPacketFlag.GZipCompressed) != 0)
        {
          _packetStream = new GZipStream(_packetStream, CompressionMode.Decompress);
          if (_packetStream != null)
          {
            _streamReader = new NetworkReader(_packetStream);
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Extracts the next packet from the collated buffer.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// For collated packets, this decodes and decompresses the next packet. For non-collated
    /// packets, this simply returns the packet given to <see cref="PacketBuffer"/>, then
    /// <code>null</code> on the following call.
    /// </remarks>
    public PacketBuffer Next()
    {
      PacketBuffer next = null;
      if (_packetStream != null)
      {
        PacketHeader header = new PacketHeader();
        // Read the header via the network reader to do the network byte order swap as required.
        if (_targetBytes - _decodedBytes > PacketHeader.Size && header.Read(_streamReader))
        {
          _decodedBytes += (uint)PacketHeader.Size;
          next = new PacketBuffer(header.PacketSize);
          next.WriteHeader(header);
          // Read payload data as it to preserve network byte order.
          next.Emplace(_packetStream, header.DataSize);
          next.FinalisePacket(true);
          _decodedBytes += (uint)header.DataSize;
          // Skip CRC if present.
          if ((header.Flags & (byte)PacketFlag.NoCrc) == 0)
          {
            // Skip CRC.
            _streamReader.ReadUInt16();
            _decodedBytes += 2;
          }
        }
      }
      else
      {
        // Not compressed.
        next = _packet;
        SetPacket(null);
      }

      // Check for failure or completion.
      if (next == null || _decodedBytes >= _targetBytes)
      {
        SetPacket(null);
      }

      return next;
    }

    /// <summary>
    /// The packet to decode.
    /// </summary>
    private PacketBuffer _packet = null;
    /// <summary>
    /// A stream into <see cref="_packet"/>. May be a GZIP stream reader.
    /// </summary>
    private Stream _packetStream = null;
    /// <summary>
    /// Wraps the <see cref="_packetStream"/> to ensure correct Endian decoding of data.
    /// </summary>
    private NetworkReader _streamReader = null;
    /// <summary>
    /// Number of bytes to decode from the packet.
    /// </summary>
    private uint _targetBytes = 0;
    /// <summary>
    /// Number of bytes decoded so far.
    /// </summary>
    private uint _decodedBytes = 0;
  }
}
