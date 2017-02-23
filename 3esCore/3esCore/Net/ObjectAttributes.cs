using System;
using System.IO;
using Tes.IO;
using System.Runtime.InteropServices;
using Tes.Maths;

namespace Tes.Net
{
  /// <summary>
  /// Defines the core attributes for a 3D shape or object.
  /// </summary>
  /// <remarks>
  /// Defines the object's:
  /// <list type="bullet">
  /// <item>Colour</item>
  /// <item>Position/translation</item>
  /// <item>Quaternion rotation.</item>
  /// <item>Scale.</item>
  /// </list>
  /// 
  /// Semantics may vary for different shapes, especially scale interpretation.
  /// </remarks>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct ObjectAttributes
  {
    /// <summary>
    /// The encoded colour. See <see cref="Maths.Colour.Value"/>
    /// </summary>
    public uint Colour;
    /// <summary>
    /// The position X coordinate.
    /// </summary>
    public float X;
    /// <summary>
    /// The position X coordinate.
    /// </summary>
    public float Y;
    /// <summary>
    /// The position X coordinate.
    /// </summary>
    public float Z;
    /// <summary>
    /// Quaternion rotation X value.
    /// </summary>
    public float RotationX;
    /// <summary>
    /// Quaternion rotation Y value.
    /// </summary>
    public float RotationY;
    /// <summary>
    /// Quaternion rotation Z value.
    /// </summary>
    public float RotationZ;
    /// <summary>
    /// Quaternion rotation Z value (angle component).
    /// </summary>
    public float RotationW;
    /// <summary>
    /// Scale along the X axis.
    /// </summary>
    public float ScaleX;
    /// <summary>
    /// Scale along the Y axis.
    /// </summary>
    public float ScaleY;
    /// <summary>
    /// Scale along the Z axis.
    /// </summary>
    public float ScaleZ;

    /// <summary>
    /// Alias the <see cref="Colour"/> member.
    /// </summary>
    public uint Color { get { return Colour; } set { Colour = value; } }

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      Colour = reader.ReadUInt32();
      X = reader.ReadSingle();
      Y = reader.ReadSingle();
      Z = reader.ReadSingle();
      RotationX = reader.ReadSingle();
      RotationY = reader.ReadSingle();
      RotationZ = reader.ReadSingle();
      RotationW = reader.ReadSingle();
      ScaleX = reader.ReadSingle();
      ScaleY = reader.ReadSingle();
      ScaleZ = reader.ReadSingle();

      return true;
    }

    /// <summary>
    /// Write this message to <paramref name="packet"/>.
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(Colour), true);
      packet.WriteBytes(BitConverter.GetBytes(X), true);
      packet.WriteBytes(BitConverter.GetBytes(Y), true);
      packet.WriteBytes(BitConverter.GetBytes(Z), true);
      packet.WriteBytes(BitConverter.GetBytes(RotationX), true);
      packet.WriteBytes(BitConverter.GetBytes(RotationY), true);
      packet.WriteBytes(BitConverter.GetBytes(RotationZ), true);
      packet.WriteBytes(BitConverter.GetBytes(RotationW), true);
      packet.WriteBytes(BitConverter.GetBytes(ScaleX), true);
      packet.WriteBytes(BitConverter.GetBytes(ScaleY), true);
      packet.WriteBytes(BitConverter.GetBytes(ScaleZ), true);
      return true;
    }

    /// <summary>
    /// Initialise the attributes from a 4x4 transformation matrix.
    /// </summary>
    /// <remarks>
    /// Does not modify <see cref="Colour"/>.
    /// </remarks>
    /// <param name="transform">The transformation matrix.</param>
    public void SetFromTransform(Matrix4 transform)
    {
      Vector3 scale = transform.Scale;
      Vector3 trans = transform.Translation;
      Quaternion rot = Rotation.ToQuaternion(transform);
      X = trans.X;
      Y = trans.Y;
      Z = trans.Z;
      RotationX = rot.X;
      RotationY = rot.Y;
      RotationZ = rot.Z;
      RotationW = rot.W;
      ScaleX = scale.X;
      ScaleY = scale.Y;
      ScaleZ = scale.Z;
    }

    /// <summary>
    /// Extract a 4x4 transformation matrix from the stored attributes.
    /// </summary>
    /// <returns>The extracted transformation matrix.</returns>
    public Matrix4 GetTransform()
    {
      Matrix4 trans = Rotation.ToMatrix4(new Quaternion(RotationX, RotationY, RotationZ, RotationW));
      trans.Translation = new Vector3(X, Y, Z);
      trans.ApplyScaling(new Vector3(ScaleX, ScaleY, ScaleZ));
      return trans;
    }
  }
}
