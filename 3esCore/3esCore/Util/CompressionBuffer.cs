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

    public override bool CanRead
    {
      get
      {
        return _buffer.CanRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return _buffer.CanSeek;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return _buffer.CanWrite;
      }
    }

    public override long Length
    {
      get
      {
        return _buffer.Length;
      }
    }

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

    public override void Close()
    {
      // Flush, but leave open.
      Flush();
    }

    public override void Flush()
    {
      _buffer.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return _buffer.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return _buffer.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      _buffer.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      _buffer.Write(buffer, offset, count);
    }

    MemoryStream _buffer;
  }
}
