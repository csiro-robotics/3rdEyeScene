using System.IO;

namespace Tes.Util
{
  /// <summary>
  /// Implements a <c>MemoryStream</c> wrapper to support GZip compression.
  /// </summary>
  /// <remarks>
  /// This class is a workaround for how <c>GZipStream</c> needs to be finalised and
  /// closes its wrapped stream.
  /// 
  /// <c>GZipStream</c> compression works by wrapping another stream. The compression
  /// stream is not finalised until the <c>GZipStream</c> is closed. Doing so closes
  /// the stream wrapped by the zip stream. This makes it impossible to access the
  /// written byte count in the memory stream or to reuse the stream.
  /// 
  /// Most members are simply a pass through to the underlying <c>MemoryStream</c>. However
  /// the <see cref="Close()"/> function flushes the buffer, but leaves the <c>MemoryStream</c>
  /// open. In this way, the <c>MemoryStream</c> can continue to be used.
  /// </remarks>
  class CompressionBuffer : Stream
  {
    /// <summary>
    /// Create a compression buffer with the given initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity in bytes.</param>
    public CompressionBuffer(int capacity)
    {
      _buffer = new MemoryStream(capacity);
    }

    /// <summary>
    /// Access the underlying <c>MemoryStream</c> object.
    /// </summary>
    public MemoryStream BaseStream
    {
      get { return _buffer; }
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    public override bool CanRead
    {
      get
      {
        return _buffer.CanRead;
      }
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    public override bool CanSeek
    {
      get
      {
        return _buffer.CanSeek;
      }
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    public override bool CanWrite
    {
      get
      {
        return _buffer.CanWrite;
      }
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    public override long Length
    {
      get
      {
        return _buffer.Length;
      }
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    public override long Position
    {
      get
      {
        return _buffer.Position;
      }

      set
      {
        _buffer.Position = value;
      }
    }

    /// <summary>
    /// Flush the <see cref="BaseStream"/>, but leave open.
    /// </summary>
    /// <remarks>See class comments on why the stream is left open.</remarks>
    public override void Close()
    {
      // Flush, but leave open.
      Flush();
    }

    /// <summary>
    /// Flush the <see cref="BaseStream"/>.
    /// </summary>
    public override void Flush()
    {
      _buffer.Flush();
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    /// <param name="buffer">Buffer to read into.</param>
    /// <param name="offset">Offset into <paramref name="buffer"/> to write at.</param>
    /// <param name="count">Number of bytes to read from the stream.</param>
    /// <returns>The number of bytes read.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
      return _buffer.Read(buffer, offset, count);
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    /// <param name="offset">Offset from <paramref name="origin"/> to seek to.</param>
    /// <param name="origin">Seeking reference position.</param>
    /// <returns>The <see cref="Position"/> after seeking.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
      return _buffer.Seek(offset, origin);
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    /// <param name="value">The new byte length to set the buffer size to.</param>
    public override void SetLength(long value)
    {
      _buffer.SetLength(value);
    }

    /// <summary>
    /// Pass through to the <see cref="BaseStream"/>.
    /// </summary>
    /// <param name="buffer">Buffer to write from.</param>
    /// <param name="offset">Offset into <paramref name="buffer"/> to read from.</param>
    /// <param name="count">Number of bytes to write to the stream.</param>
    /// <returns>The number of bytes written.</returns>
    public override void Write(byte[] buffer, int offset, int count)
    {
      _buffer.Write(buffer, offset, count);
    }

    MemoryStream _buffer;
  }
}
