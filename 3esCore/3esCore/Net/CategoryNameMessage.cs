using System;
using System.IO;
using System.Text;
using Tes.IO;

namespace Tes.Net
{
  /// <summary>
  /// Message used to change/set a category name.
  /// </summary>
  /// <remarks>
  /// The category name is encoded using a two byte string byte count, followed by the
  /// UTF8 character bytes.
  /// </remarks>
  public struct CategoryNameMessage : IMessage
  {
    /// <summary>
    /// Category message ID.
    /// </summary>
    public static ushort MessageID { get { return (ushort)CategoryMessageID.Name; } }
    /// <summary>
    /// The category ID.
    /// </summary>
    public ushort CategoryID;
    /// <summary>
    /// The parent category ID. Zero for none (zero cannot be a parent category).
    /// </summary>
    public ushort ParentID;
    /// <summary>
    /// Start active?
    /// </summary>
    public bool DefaultActive;
    /// <summary>
    /// The name for the category. Encoded as a UTF8 prefixed by the character count in two bytes (no null terminator).
    /// </summary>
    public string Name;

    /// <summary>
    /// Read the message from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>True</returns>
    public bool Read(BinaryReader reader)
    {
      CategoryID = reader.ReadUInt16();
      ParentID = reader.ReadUInt16();
      DefaultActive = reader.ReadInt16() != 0;
      ushort textLength = reader.ReadUInt16();
      if (textLength != 0)
      { 
        byte[] bytes = reader.ReadBytes(textLength);
        Name = Encoding.UTF8.GetString(bytes);
      }
      else
      {
        Name = "";
      }
      return true;
    }

    /// <summary>
    /// Write the message to <paramref name="packet"/>
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True on success.</returns>
    public bool Write(PacketBuffer packet)
    {
      packet.WriteBytes(BitConverter.GetBytes(CategoryID), true);
      packet.WriteBytes(BitConverter.GetBytes(ParentID), true);
      ushort active = (ushort)(DefaultActive ? 1 : 0);
      packet.WriteBytes(BitConverter.GetBytes(active), true);
      byte[] text = Encoding.UTF8.GetBytes(Name);
      ushort length = (ushort)text.Length;
      packet.WriteBytes(BitConverter.GetBytes(length), true);
      if (length != 0)
      {
        packet.WriteBytes(text, false);
      }
      return true;
    }
  }
}
