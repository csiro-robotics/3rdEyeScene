using System;

namespace Tes.IO
{
  /// <summary>
  /// This class is used to calculate a 2-byte/16-bit CRC value for a memory buffer.
  /// </summary>
  /// <remarks>
  /// General usage is to use the <see cref="Crc"/> static property to access the default
  /// calculator (see below).
  /// 
  /// <code lang="C#">
  /// ushort calcCrc(byte[] buffer)
  /// {
  ///   return Crc16.Crc.Calculate(buffer);
  /// }
  /// </code>
  /// </remarks>
  public class Crc16
  {
    /// <summary>
    /// The default calculator.
    /// </summary>
    private static Crc16 _crc = new Crc16();

    /// <summary>
    /// Access the default calculator.
    /// </summary>
    public static Crc16 Crc { get { return _crc; } }

    /// <summary>
    /// The size of the calculated CRC in bytes.
    /// </summary>
    public static int CrcSize { get { return 2; } }

    /// <summary>
    /// Creates a CRC calculator.
    /// </summary>
    public Crc16() : this((ushort)0xFFFFu, (ushort)0u, (ushort)0x1021u) { }

    /// <summary>
    /// Creates a CRC calculator with the given CRC seed values.
    /// </summary>
    /// <param name="initial">The initial CRC starting value.</param>
    /// <param name="final">The final XOR value applied to the CRC.</param>
    /// <param name="polynomial">The polynomial use dto initialise the CRC table.</param>
    public Crc16(ushort initial, ushort final, ushort polynomial)
    {
      _initialRemainder = initial;
      _finalXorValue = final;
      _crcTable = new ushort[256];
      _width = 0;
      _width = (ushort)(BitConverter.GetBytes(_width).Length * 8);
      _topBit = (ushort)(1 << (_width - 1));
      InitialiseTable(polynomial);
    }

    /// <summary>
    /// Calculate a CRC for the given buffer.
    /// </summary>
    /// <param name="buffer">The buffer to operate on.</param>
    /// <returns>The CRC value.</returns>
    public ushort Calculate(byte[] buffer)
    {
      return Calculate(buffer, 0u, (uint)buffer.Length);
    }


    /// <summary>
    /// Calculate a CRC for part of the given buffer (from the start).
    /// </summary>
    /// <param name="buffer">The buffer to operate on.</param>
    /// <param name="bufferLength">The number of bytes to calculate the CRC for. 
    /// Must be less than <c>buffer.Length</c></param>
    /// <returns>The CRC value.</returns>
    public ushort Calculate(byte[] buffer, uint bufferLength)
    {
      return Calculate(buffer, 0u, bufferLength);
    }

    /// <summary>
    /// Calculate a CRC for part of the given buffer.
    /// </summary>
    /// <remarks>
    /// The <paramref name="startIndex"/> must be in range of the <paramref name="buffer"/>.
    /// The sum of the <paramref name="startIndex"/> and the <paramref name="numberOfBytes"/>
    /// must be less than or equal to <c>buffer.Length</c>.
    /// </remarks>
    /// <param name="buffer">The buffer to operate on.</param>
    /// <param name="startIndex">CRC calculation starts at this byte into the <paramref name="buffer"/>.</param>
    /// <param name="numberOfBytes">The number of bytes to calculate the CRC for.</param> 
    /// <returns>The CRC value.</returns>
    public ushort Calculate(byte[] buffer, uint startIndex, uint numberOfBytes)
    {
      byte data;
      ushort remainder = _initialRemainder;

      unchecked
      {
        // Divide the message by the polynomial, a byte at a time.
        uint endIndex = startIndex + numberOfBytes;
        for (uint pos = startIndex; pos < endIndex; ++pos)
        {
          data = (byte)(buffer[pos] ^ (remainder >> (_width - 8)));
          remainder = (ushort)(_crcTable[data] ^ (remainder << 8));
        }

        // The final remainder is the CRC.
        return (ushort)(remainder ^ _finalXorValue);
      }
    }

    /// <summary>
    /// Initialises the CRC polynomial table.
    /// </summary>
    /// <param name="polynomial">Initialisation value.</param>
    private void InitialiseTable(ushort polynomial)
    {
      ushort remainder;

      try
      {
        unchecked
        {
          // Compute the remainder of each possible dividend.
          for (int dividend = 0; dividend < _crcTable.Length; ++dividend)
          {
            // Start with the dividend followed by zeros.
            remainder = (ushort)(dividend << (_width - 8));

            // Perform modulo-2 division, a bit at a time.
            for (byte bit = 8; bit > 0; --bit)
            {
              // Try to divide the current data bit.
              if ((remainder & _topBit) != 0)
              {
                remainder = (ushort)((remainder << 1) ^ polynomial);
              }
              else
              {
                remainder = (ushort)(remainder << 1);
              }
            }

            // Store the result into the table.
            _crcTable[dividend] = remainder;
          }
        }
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Write(e);
      }
    }

    /// <summary>
    /// CRC seed value.
    /// </summary>
    private ushort _initialRemainder;
    /// <summary>
    /// CRC final XOR value.
    /// </summary>
    private ushort _finalXorValue;
    /// <summary>
    /// Calculation table.
    /// </summary>
    private ushort[] _crcTable;
    /// <summary>
    /// CRC bit width (16).
    /// </summary>
    private ushort _width;
    /// <summary>
    /// The highest order bit in the CRC type.
    /// </summary>
    private ushort _topBit;
  }
}
