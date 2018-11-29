using System;
using System.IO;

namespace Tes.IO
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
  ///
  /// Note the stream is designed to be filled to capacity before reading. Thus the capacity
  /// denotes the number of bytes available.
  /// </remarks>
  class ReusableMemoryStream : Stream
  {
    /// <summary>
    /// Create a compression buffer with the given initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity in bytes.</param>
    public ReusableMemoryStream(int capacity)
    {
      _buffer = new byte[capacity];
    }

    /// <summary>
    /// Is this a readable stream?
    /// </summary>
    /// <returns><c>true</c></returns>
    public override bool CanRead
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Is this a seekable stream.?
    /// </summary>
    /// <returns><c>true</c></returns>
    public override bool CanSeek
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Is this a writable stream?
    /// </summary>
    /// <returns><c>true</c></returns>
    public override bool CanWrite
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Query the stream length in bytes.
    /// </summary>
    /// <returns>The stream length in bytes.</returns>
    /// <remarks>This is equivalent to the capacity set on construction.</remarks>
    public override long Length
    {
      get
      {
        return _buffer.Length;
      }
    }

    /// <summary>
    /// Get/set the current read/write position (byte offset).
    /// </summary>
    /// <returns>The current read/write byte offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the position set is out of range of the internal buffer <see cref="Length"/>.
    /// </exception>
    public override long Position
    {
      get
      {
        return _position;
      }

      set
      {
        if (value >= _buffer.Length || value < 0)
        {
          throw new ArgumentOutOfRangeException("Position", value, "Buffer overrun");
        }
        _position = value;
      }
    }

    /// <summary>
    /// Access the internal buffer directly.
    /// </summary>
    /// <returns>The internal byte buffer.</returns>
    public byte[] GetBuffer()
    {
      return _buffer;
    }

    /// <summary>
    /// Nominally close the stream (see remarks).
    /// </summary>
    /// <remarks>
    /// This method does not close the stream, only calling <see cref="Flush"/> instead. This is
    /// the key behavioural difference between this stream and the normal <c>MemoryStream</c> allowing
    /// this stream to be re-used.
    /// </remarks>
    public override void Close()
    {
      // Flush, but leave open.
      Flush();
    }

    /// <summary>
    /// Ignored: the position is preserved for reading.
    /// </summary>
    /// <remarks>
    /// Since the internal position is 
    /// </remarks>
    public override void Flush()
    {
      // Noop. We need to preserve the position for reading.
    }

    /// <summary>
    /// Read bytes from the stream.
    /// </summary>
    /// <param name="buffer">Buffer to read into.</param>
    /// <param name="offset">Offset into <paramref name="buffer"/> to write at.</param>
    /// <param name="count">Number of bytes to read from the stream.</param>
    /// <returns>The number of bytes read.</returns>
    /// <remarks>This progresses the <see cref="Position"/> by the number of bytes read.</remarks>
    public override int Read(byte[] buffer, int offset, int count)
    {
      long available = _buffer.LongLength - _position;
      if (available > int.MaxValue)
      {
        available = int.MaxValue;
      }
      count = (count <= available) ? count : (int)available;
      Array.Copy(_buffer, _position, buffer, offset, count);
      _position += count;
      return count;
    }

    /// <summary>
    /// Seek to a position in the stream, adjusting the <see cref="Position"/>.
    /// </summary>
    /// <param name="offset">Offset from <paramref name="origin"/> to seek to.</param>
    /// <param name="origin">Seeking reference position.</param>
    /// <returns>The <see cref="Position"/> after seeking.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          Position = offset;
          break;
        case SeekOrigin.End:
          Position = _buffer.LongLength + offset;
          break;
        case SeekOrigin.Current:
          Position = _position + offset;
          break;
      }
      return Position;
    }

    /// <summary>
    /// Resizes the internal buffer, affecting <see cref="Length"/>.
    /// </summary>
    /// <param name="value">The new byte length to set the buffer size to.</param>
    public override void SetLength(long value)
    {
      if (_buffer.LongLength != value)
      {
        if ((int)value == value)
        {
          Array.Resize<byte>(ref _buffer, (int)value);
        }
        else
        {
          byte[] newBuffer = new byte[value];
          Array.Copy(_buffer, newBuffer, value);
          _buffer = newBuffer;
        }
      }
    }

    /// <summary>
    /// Write bytes to the stream.
    /// </summary>
    /// <param name="buffer">Buffer to write from.</param>
    /// <param name="offset">Offset into <paramref name="buffer"/> to read from.</param>
    /// <param name="count">Number of bytes to write to the stream.</param>
    /// <returns>The number of bytes written.</returns>
    /// <remarks>This progresses the <see cref="Position"/> by the number of bytes written.</remarks>
    public override void Write(byte[] buffer, int offset, int count)
    {
      if (_position + count > _buffer.Length)
      {
        SetLength(Math.Max(_position + count, _buffer.LongLength * 2));;
      }
      Array.Copy(buffer, offset, _buffer, _position, count);
      _position += count;
    }

    private byte[] _buffer;
    private long _position;
  }
}
