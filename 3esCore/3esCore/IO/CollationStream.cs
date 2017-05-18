using System;
using System.IO;

namespace Tes.IO
{
  /// <summary>
  /// A stream implementation which uses a <see cref="T:IO.CollatedPacketEncoder"/>  to compress
  /// data before writing to another stream.
  /// </summary>
  /// <remarks>
  /// This is intended for compressing <see cref="T:PacketBuffer"/> data to a <c>FileStream</c>.
  /// Compression is implemented in chunks using the <see cref="T:IO.CollatedPacketEncoder"/>
  /// and is implicitly enabled. No data other than <see cref="T:PacketBuffer"/> data should be
  /// written to this stream.
  /// 
  /// Compression may be temporarily disabled by setting <see cref="CompressionEnabled"/> to 
  /// <c>false</c>. This will flush the current buffer then cause <see cref="Write(byte[], int, int)"/>
  /// calls to write directly to the <see cref="BaseStream"/>.
  /// </remarks>
  public class CollationStream : Stream
  {
    /// <summary>
    /// Createa a collation stream wrapping <paramref name="baseStream"/>.
    /// </summary>
    /// <param name="baseStream">The stream to write into after collation.</param>
    /// <param name="compress">Enable compression? Otherwise we collated without compression.</param>
    public CollationStream(Stream baseStream, bool compress = true)
    {
      BaseStream = baseStream;
      _collator = new CollatedPacketEncoder(compress);
    }

    /// <summary>
    /// Toggle compression. Initialised to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// While disabled, bytes are written directly to the <see cref="BaseStream"/>
    /// without collation or comrpession.
    /// </remarks>
    public bool CompressionEnabled
    {
      get { return _compressionEnabled; }
      set
      {
        if (value != _compressionEnabled)
        {
          FlushCollatedPacket();
          _compressionEnabled = value;
        }
      }
    }

    /// <summary>
    /// The underlying stream to write to.
    /// </summary>
    public Stream BaseStream { get; protected set; }

    /// <summary>
    /// <c>false</c>
    /// </summary>
    public override bool CanRead { get { return false; } }

    /// <summary>
    /// <c>false</c>
    /// </summary>
    public override bool CanSeek { get { return false; } }

    /// <summary>
    /// <c>false</c>
    /// </summary>
    public override bool CanWrite { get { return true; } }

    /// <summary>
    /// Returns the number of bytes written plus the oustanding buffered bytes.
    /// </summary>
    public override long Length { get { return Position; } }

    /// <summary>
    /// Get returns the number of bytes written plus the oustanding buffered bytes.
    /// Set is not supported (exception).
    /// </summary>
    public override long Position
    {
      get
      {
        return BaseStream.Position + (_collator.CollatedBytes > 0 ? _collator.Count : 0);
      }

      set
      {
        throw new NotSupportedException();
      }
    }

    /// <summary>
    /// Flush the collation buffer and the underlying stream.
    /// </summary>
    public override void Flush()
    {
      FlushCollatedPacket();
      BaseStream.Flush();
    }

    /// <summary>
    /// Not suypported.
    /// </summary>
    /// <returns>N/A</returns>
    /// <param name="buffer">Ignored.</param>
    /// <param name="offset">Ignored.</param>
    /// <param name="count">Ignored.</param>
    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <returns>N/A</returns>
    /// <param name="offset">Ignored.</param>
    /// <param name="origin">Ignored.</param>
    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="value">Ignored.</param>
    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Writes and compresses data to the collation buffer. Periodically flushes the
    /// collation buffer and writes to the <see cref="BaseStream"/>.
    /// </summary>
    /// <param name="buffer">The buffer to write data from.</param>
    /// <param name="offset">Offset to the first byte to write.</param>
    /// <param name="count">Number of bytes to write.</param>
    /// <remarks>
    /// This method should only be used to write data from a <see cref="T:PacketBuffer"/>.
    /// </remarks>
    public override void Write(byte[] buffer, int offset, int count)
    {
      if (_collator.CollatedBytes + count >= CollatedPacketEncoder.MaxPacketSize)
      {
        // Additional bytes would be too much. Flush collated
        FlushCollatedPacket();
      }
      int added = _collator.Add(buffer, offset, count);
      // Check if we failed to add to the packet. We'll send the data by itself afterwards.
      if (added == -1)
      {
        // Failed to add. Flush the packet, then try again. If that fails we write uncompressed.
        FlushCollatedPacket();
        added = _collator.Add(buffer, offset, count);
        if (added == -1)
        {
          // Cannot collate this data. Write uncompressed.
          BaseStream.Write(buffer, offset, count);
        }
      }
    }

    /// <summary>
    /// Flush the collation buffer, writing its contents to the <see cref="BaseStream"/> and reset.
    /// </summary>
    private void FlushCollatedPacket()
    {
      if (_collator.CollatedBytes > 0 && _collator.FinaliseEncoding())
      {
        int byteCount = _collator.Count;
        BaseStream.Write(_collator.Buffer, 0, byteCount);
        _collator.Reset();
      }
    }

    private CollatedPacketEncoder _collator;
    private bool _compressionEnabled = true;
  }
}
