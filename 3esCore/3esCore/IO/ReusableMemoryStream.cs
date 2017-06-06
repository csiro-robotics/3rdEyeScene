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

    public override bool CanRead
    {
      get
      {
        return true;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return true;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return true;
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

    public byte[] GetBuffer()
    {
      return _buffer;
    }

    public override void Close()
    {
      // Flush, but leave open.
      Flush();
    }

    public override void Flush()
    {
      // Noop. We need to preserve the position for reading.
    }

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
