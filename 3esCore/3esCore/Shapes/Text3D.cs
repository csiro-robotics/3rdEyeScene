using System;
using System.IO;
using System.Text;
using Tes.IO;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines 3D text to render in 3D coordinates.
  /// </summary>
  public class Text3D : Shape
  {
    /// <summary>
    /// The default text facing.
    /// </summary>
    public static Vector3 DefaultFacing  = new Vector3(0, -1, 0);

    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    public Text3D() : this(string.Empty, Vector3.Zero) { }

    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="pos">The text 3D position.</param>
    /// <param name="fontSize">Text size.</param>
    public Text3D(string text, Vector3 pos, int fontSize = 12)
      : base((ushort)Tes.Net.ShapeID.Text3D)
    {
      Position = pos;
      Text = text;
    }


    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="pos">The text 3D position.</param>
    /// <param name="fontSize">Text size.</param>
    public Text3D(string text, uint id, Vector3 pos, int fontSize = 12)
      : base((ushort)Tes.Net.ShapeID.Text3D, id)
    {
      Position = pos;
      Text = text;
    }

    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pos">The text 3D position.</param>
    /// <param name="fontSize">Text size.</param>
    public Text3D(string text, uint id, ushort category, Vector3 pos, int fontSize = 12)
      : base((ushort)Tes.Net.ShapeID.Text3D, id, category)
    {
      Position = pos;
      Text = text;
    }


    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    public Text3D(string text) : this(text, Vector3.Zero) { }
    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public Text3D(string text, uint id) : this(text, id, Vector3.Zero) { }
    /// <summary>
    /// Create 3D text to render.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Text3D(string text, uint id, ushort category) : this(text, id, category, Vector3.Zero) { }

    /// <summary>
    /// The size of the rendered text.
    /// </summary>
    /// <remarks>
    /// The units need to be better defined.
    /// </remarks>
    public int FontSize
    {
      get { return (int)ScaleZ; }
      set { ScaleZ = value; }
    }

    /// <summary>
    /// The text to render.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Does the text always face the camera? If so, the <see cref="Facing"/> is ignored.
    /// </summary>
    public bool ScreenFacing
    {
      get
      {
        return (_data.Flags & (ushort)Tes.Net.Text3DFlag.SceenFacing) != 0;
      }

      set
      {
        _data.Flags &= (ushort)~Tes.Net.Text3DFlag.SceenFacing;
        _data.Flags |= (ushort)((value) ? Tes.Net.Text3DFlag.SceenFacing : 0);
      }
    }

    /// <summary>
    /// The facing for the text (can be considered a normal vector).
    /// </summary>
    public Vector3 Facing
    {
      get
      {
        return Rotation * DefaultFacing;
      }

      set
      {
        ScreenFacing = false;
        Quaternion rot = new Quaternion();
        if (value.Dot(DefaultFacing) > -0.9998f)
        {
          rot.SetFromTo(DefaultFacing, value);
        }
        else
        {
          rot.SetAxisAngle(Vector3.AxisX, (float)Math.PI);
        }
        Rotation = rot;
      }
    }

    /// <summary>
    /// Overridden create message.
    /// </summary>
    /// <remarks>
    /// Includes the text written as a byte count, followed by a series of UTF-8 bytes. No
    /// null terminator is included.
    /// </remarks>
    /// <param name="packet">Packet to write to.</param>
    /// <returns>True on success.</returns>
    public override bool WriteCreate(PacketBuffer packet)
    {
      if (base.WriteCreate(packet))
      {
        byte[] text = Encoding.UTF8.GetBytes(Text);
        ushort length = (ushort)text.Length;
        packet.WriteBytes(BitConverter.GetBytes(length), true);
        if (length != 0)
        {
          packet.WriteBytes(text, false);
        }
        return true;
      }

      return false;
    }

    public override bool ReadCreate(BinaryReader reader)
    {
      if (!base.ReadCreate(reader))
      {
        return false;
      }

      ushort length = reader.ReadUInt16();
      if (length > 0)
      {
        byte[] text = reader.ReadBytes(length);
        Text = Encoding.UTF8.GetString(text);
      }

      return true;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      Text3D copy = new Text3D(Text);
      OnClone(copy);
      return copy;
    }

    /// <summary>
    /// Additional data copy on <see cref="Clone()"/>.
    /// </summary>
    /// <param name="copy">The cloned object.</param>
    protected void OnClone(Text3D copy)
    {
      base.OnClone(copy);
      copy.ScreenFacing = ScreenFacing;
    }
  }
}
