using System;
using System.Runtime.InteropServices;
using System.IO;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// A message identifying the properties of a camera.
  /// </summary>
  /// <remarks>
  /// The camera message may be used to represent the desired view. Remote viewing and
  /// playback may follow the primary camera at the user's request.
  /// </remarks>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct CameraMessage : IMessage
  {
    /// <summary>
    /// Reserved camera ID for recording the camera properties during playback.
    /// </summary>
    public static byte RecordedCameraID { get { return (byte)255u; } }

    /// <summary>
    /// ID of the camera.
    /// </summary>
    /// <remarks>
    /// 255 is reserved to record the view used while recording.
    /// </remarks>
    public byte CameraID;

    /// <summary>
    /// Padding/reserved. Must be zero.
    /// </summary>
    public byte Reserved1;

    /// <summary>
    /// Padding/reserved. Must be zero.
    /// </summary>
    public ushort Reserved2;

    /// <summary>
    /// Position X coordinate.
    /// </summary>
    public float X;

    /// <summary>
    /// Position Y coordinate.
    /// </summary>
    public float Y;

    /// <summary>
    /// Position Z coordinate.
    /// </summary>
    public float Z;

    /// <summary>
    /// Forward vector X value.
    /// </summary>
    public float DirX;

    /// <summary>
    /// Forward vector Y value.
    /// </summary>
    public float DirY;

    /// <summary>
    /// Forward vector Z value.
    /// </summary>
    public float DirZ;

    /// <summary>
    /// Up vector X value.
    /// </summary>
    public float UpX;

    /// <summary>
    /// Up vector Y value.
    /// </summary>
    public float UpY;

    /// <summary>
    /// Up vector Z value.
    /// </summary>
    public float UpZ;

    /// <summary>
    /// Near clip plane (optional). Zero or less implies an unspecified or unchanged value.
    /// </summary>
    public float Near;

    /// <summary>
    /// Far clip plane (optional). Zero or less implies an unspecified or unchanged value.
    /// </summary>
    public float Far;

    /// <summary>
    /// Horizontal field of view in degrees (optional). Zero or less implies an unspecified or unchanged value.
    /// </summary>
    public float FOV;

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      CameraID = reader.ReadByte();
      Reserved1 = reader.ReadByte();
      Reserved2 = reader.ReadUInt16();
      X = reader.ReadSingle();
      Y = reader.ReadSingle();
      Z = reader.ReadSingle();
      DirX = reader.ReadSingle();
      DirY = reader.ReadSingle();
      DirZ = reader.ReadSingle();
      UpX = reader.ReadSingle();
      UpY = reader.ReadSingle();
      UpZ = reader.ReadSingle();
      Near = reader.ReadSingle();
      Far = reader.ReadSingle();
      FOV = reader.ReadSingle();
      return true;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(new byte[] { CameraID }, true);
      packet.WriteBytes(new byte[] { Reserved1 }, true);
      packet.WriteBytes(BitConverter.GetBytes(Reserved2), true);
      packet.WriteBytes(BitConverter.GetBytes(X), true);
      packet.WriteBytes(BitConverter.GetBytes(Y), true);
      packet.WriteBytes(BitConverter.GetBytes(Z), true);
      packet.WriteBytes(BitConverter.GetBytes(DirX), true);
      packet.WriteBytes(BitConverter.GetBytes(DirY), true);
      packet.WriteBytes(BitConverter.GetBytes(DirZ), true);
      packet.WriteBytes(BitConverter.GetBytes(UpX), true);
      packet.WriteBytes(BitConverter.GetBytes(UpY), true);
      packet.WriteBytes(BitConverter.GetBytes(UpZ), true);
      packet.WriteBytes(BitConverter.GetBytes(Near), true);
      packet.WriteBytes(BitConverter.GetBytes(Far), true);
      packet.WriteBytes(BitConverter.GetBytes(FOV), true);
      return true;
    }
  }
}