using System;
using System.IO;
using System.Runtime.InteropServices;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// A system control message defining some global information about the server.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct ServerInfoMessage
  {
    /// <summary>
    /// Number of bytes reserved in the stream.
    /// </summary>
    public const int ReservedBytes = 31;

    /// <summary>
    /// Specifies the time unit in a <see cref="ControlMessageID.EndFrame"/>
    /// <see cref="ControlMessage"/>.
    /// </summary>
    /// <remarks>
    /// This value specifies the number of milliseconds microseconds associated with each
    /// time value in a <see cref="ControlMessageID.EndFrame"/> message. For example,
    /// consider a <c>TimeUnit</c> of 1000us, and a <see cref="ControlMessageID.EndFrame"/>
    /// value of 33. This means that the frame represents 33 <c>TimeUnit</c> increments
    /// of time, or 33 * 1000us = 33000ms = 33ms.
    ///
    /// The default <c>TimeUnit</c> is 1000us (1 millisecond).
    /// </remarks>
    public ulong TimeUnit;
    /// <summary>
    /// The default end of frame time delta.
    /// </summary>
    /// <remarks>
    /// This value is used for any <see cref="ControlMessageID.EndFrame"/> message
    /// which has a zero time delta. The value is used as if it were the value
    /// specified in the <see cref="ControlMessageID.EndFrame"/> message, thus it
    /// must be multiplied by the <see cref="TimeUnit"/>.
    /// </remarks>
    public uint DefaultFrameTime;
    /// <summary>
    /// Specifies the coordinate frame for the server. Written as a single byte.
    /// </summary>
    public CoordinateFrame CoordinateFrame;
    /// <summary>
    /// Reserved for future use. The reserved byte count is set in <see cref="ReservedBytes"/>.
    /// </summary>
    public fixed byte Reserved[ReservedBytes];

    /// <summary>
    /// Instantiates the default server info message.
    /// </summary>
    /// <remarks>
    /// Uses the following defaults:
    /// <list type="table">
    /// <listheader><term>Datum</term><description>Value</description></listheader>
    /// <item><term><see cref="TimeUnit"/></term><description>1000 microseconds</description></item>
    /// <item><term><see cref="DefaultFrameTime"/></term><description>33 (milliseconds)</description></item>
    /// <item><term><see cref="CoordinateFrame"/></term><description><see cref="CoordinateFrame.XYZ">XYZ</see></description></item>
    /// <item><term><see cref="Reserved"/></term><description>0</description></item>
    /// </list>
    /// </remarks>
    public static ServerInfoMessage Default
    {
      get
      {
        return new ServerInfoMessage
        {
          TimeUnit = 1000ul,
          DefaultFrameTime = 33u,
          CoordinateFrame = CoordinateFrame.XYZ
        };
      }
    }

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      bool ok = true;
      TimeUnit = reader.ReadUInt64();
      DefaultFrameTime = reader.ReadUInt32();
      try
      {
        CoordinateFrame = (CoordinateFrame)reader.ReadByte();
      }
      catch (InvalidCastException)
      {
        ok = false;
      }
      fixed (byte* reserved = Reserved)
      {
        for (int i = 0; i < ReservedBytes; ++i)
        {
          reserved[i] = reader.ReadByte();
        }
      }
      return ok;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(TimeUnit), true);
      packet.WriteBytes(BitConverter.GetBytes(DefaultFrameTime), true);
      packet.WriteBytes(new byte[] { (byte)CoordinateFrame }, true);
      fixed (byte* reserved = Reserved)
      {
        // Can this be done without allocating a new array?
        byte[] reservedArray = new byte[ReservedBytes];

        for (int i = 0; i < ReservedBytes; ++i)
        {
          reservedArray[i] = reserved[i];
        }
        packet.WriteBytes(reservedArray, false);
      }
      return true;
    }
  }
}

