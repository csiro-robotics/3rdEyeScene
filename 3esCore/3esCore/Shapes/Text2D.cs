using System;
using System.IO;
using System.Text;
using Tes.IO;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines 2D text to render.
  /// </summary>
  /// <remarks>
  /// Can represent 2D screen located text, or 3D located text projected in to screen space.
  /// Use <see cref="InWorldSpace"/> <c>true</c> for 3D located text.
  /// 
  /// 2D located text ignores the position Z coordinate. X and Y range [0, 1], with (0, 0) being
  /// the top left corner and (1, 1) the bottom right.
  /// </remarks>
  public class Text2D : Shape
  {
    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    public Text2D() : this(string.Empty, Vector3.Zero) { }

    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="pos">The text position.</param>
    public Text2D(string text, Vector3 pos)
      : base((ushort)Tes.Net.ShapeID.Text2D)
    {
      Position = pos;
      Text = text;
    }

    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="pos">The text position.</param>
    public Text2D(string text, uint id, Vector3 pos)
      : base((ushort)Tes.Net.ShapeID.Text2D, id)
    {
      Position = pos;
      Text = text;
    }


    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pos">The text position.</param>
    public Text2D(string text, uint id, ushort category, Vector3 pos)
      : base((ushort)Tes.Net.ShapeID.Text2D, id, category)
    {
      Position = pos;
      Text = text;
    }

    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    public Text2D(string text) : this(text, Vector3.Zero) { }
    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public Text2D(string text, uint id) : this(text, id, Vector3.Zero) { }
    /// <summary>
    /// Create a 2D text shape.
    /// </summary>
    /// <param name="text">The text string to display.</param>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public Text2D(string text, uint id, ushort category) : this(text, id, category, Vector3.Zero) { }

    /// <summary>
    /// The text string to render.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Position in world coordinates (<c>true</c>) or screen coordinates (<c>false</c>).
    /// </summary>
    public bool InWorldSpace
    {
      get
      {
        return (_data.Flags & (ushort)Tes.Net.Text2DFlag.WorldSpace) != 0;
      }

      set
      {
        _data.Flags &= (ushort)~Tes.Net.Text2DFlag.WorldSpace;
        _data.Flags |= (ushort)((value) ? Tes.Net.Text2DFlag.WorldSpace : 0);
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

    /// <summary>
    /// Read create message and appended text string.
    /// </summary>
    /// <param name="reader">Stream to read from</param>
    /// <returns>True on success.</returns>
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
      Text2D copy = new Text2D(Text);
      OnClone(copy);
      return copy;
    }

    /// <summary>
    /// Additional data copy on <see cref="Clone()"/>.
    /// </summary>
    /// <param name="copy">The cloned object.</param>
    protected void OnClone(Text2D copy)
    {
      base.OnClone(copy);
      copy.InWorldSpace = InWorldSpace;
    }
  }
}
