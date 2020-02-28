using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers.Shape2D
{
  /// <summary>
  /// Handles 2D text. May be located in screen space, or 3D and projected into screen space.
  /// </summary>
  public class Text2DHandler : MessageHandler
  {
    /// <summary>
    /// Tracks a text object.
    /// </summary>
    public class TextEntry
    {
      /// <summary>
      /// Object id.
      /// </summary>
      public uint ID;
      /// <summary>
      /// Position coordinates. 2D or 3D depending on <see cref="WorldSpace"/>
      /// </summary>
      public Vector3 Position;
      /// <summary>
      /// Text colour.
      /// </summary>
      public Color32 Colour;
      /// <summary>
      /// <see cref="ObjectFlag"/> and <see cref="Text2DFlag"/>
      /// </summary>
      public ushort ObjectFlags;
      /// <summary>
      /// Object category.
      /// </summary>
      public ushort Category;
      /// <summary>
      /// The text to display.
      /// </summary>
      public string Text;

      /// <summary>
      /// True if located in 3D space and projected into screen space.
      /// </summary>
      /// <remarks>
      /// Tests <see cref="ObjectFlags"/> for <see cref="Text2DFlag.WorldSpace"/>
      /// </remarks>
      public bool WorldSpace { get { return (ObjectFlags & (ushort)Text2DFlag.WorldSpace) != 0; } }
    }

    /// <summary>
    /// Manages text rendering.
    /// </summary>
    public class Text2DManager : MonoBehaviour
    {
      public Text2DHandler TextHandler { get; set; }

      /// <summary>
      /// Enumerate the text list.
      /// </summary>
      public IEnumerable<TextEntry> Entries
      {
        get
        {
          foreach (TextEntry entry in _text)
          {
            yield return entry;
          }
        }
      }

      /// <summary>
      /// Add text to display.
      /// </summary>
      /// <param name="entry">Text details.</param>
      public void Add(TextEntry entry)
      {
        _text.Add(entry);
      }

      /// <summary>
      /// Check if there is a text entry matching the given <paramref name="id"/>
      /// </summary>
      /// <param name="id">The ID to search for</param>
      /// <returns>True if there is an entry present matching <paramref name="id"/>.</returns>
      public bool ContainsEntry(uint id)
      {
        for (int i = 0; i < _text.Count; ++i)
        {
          if (_text[i].ID == id)
          {
            _text.RemoveAt(i);
            return true;
          }
        }

        return false;
      }

      /// <summary>
      /// Update existing text display.
      /// </summary>
      /// <param name="entry">Updated text details.</param>
      public bool UpdateEntry(TextEntry entry)
      {
        for (int i = 0; i < _text.Count; ++i)
        {
          if (_text[i].ID == entry.ID)
          {
            TextEntry cur = _text[i];
            cur.Position = entry.Position;
            cur.Colour = entry.Colour;
            cur.ObjectFlags = entry.ObjectFlags;
            if (!string.IsNullOrEmpty(entry.Text))
            {
              cur.Text = entry.Text;
            }
            return true;
          }
        }

        return false;
      }

      /// <summary>
      /// Remove text.
      /// </summary>
      /// <param name="id">Object ID of the text to remove.</param>
      /// <returns>True if the corresponding entry was found and removed.</returns>
      public bool Remove(uint id)
      {
        for (int i = 0; i < _text.Count; ++i)
        {
          if (_text[i].ID == id)
          {
            _text.RemoveAt(i);
            return true;
          }
        }

        return false;
      }

      /// <summary>
      /// Clear all text.
      /// </summary>
      public void Clear()
      {
        _text.Clear();
      }

      /// <summary>
      /// Text rendering entry point.
      /// </summary>
      /// <remarks>
      /// Uses the old GUI system for its simplicity in rendering on screen text.
      /// </remarks>
      void OnGUI()
      {
        Vector3 position;
        Rect textRect = new Rect();
        GUIStyle style = new GUIStyle("label");
        // Render all the text.
        for (int i = 0; i < _text.Count; ++i)
        {
          TextEntry entry = _text[i];

          if (TextHandler.CategoriesState != null && !TextHandler.CategoriesState.IsActive(entry.Category))
          {
            continue;
          }

          if (entry.WorldSpace)
          {
            position = Camera.main.WorldToScreenPoint(entry.Position);
            position.y = Camera.main.pixelRect.height - position.y;
          }
          else
          {
            position = entry.Position;
            position.x *= Camera.main.pixelRect.width;
            position.y *= Camera.main.pixelRect.height;
          }

          if (TextRect(ref textRect, position, entry.Text, style))
          {
            GUI.Label(textRect, entry.Text, style);
          }
        }
      }

      /// <summary>
      /// Generate screen rect for the given text with the given screen position.
      /// </summary>
      /// <param name="rect">Calculated text rectangle.</param>
      /// <param name="position">Text position (screen space).</param>
      /// <param name="text">Text to render.</param>
      /// <param name="style">Rendering style.</param>
      /// <returns>True if <paramref name="rect"/> overlaps the screen and should be rendered.</returns>
      bool TextRect(ref Rect rect, Vector3 position, string text, GUIStyle style)
      {
        Vector2 size = style.CalcSize(new GUIContent(text));
        rect.xMin = position.x;
        rect.yMin = position.y;
        rect.xMax = position.x + size.x;
        rect.yMax = position.y + size.y;
        return rect.Overlaps(Camera.main.pixelRect);
      }

      private List<TextEntry> _text = new List<TextEntry>();
    }

    /// <summary>
    /// Root object implementation (irrelevant).
    /// </summary>
    public GameObject Root { get; protected set; }
    /// <summary>
    /// Persistent object root (irrelevant).
    /// </summary>
    public GameObject Persistent { get; protected set; }
    /// <summary>
    /// Transient object root (irrelevant).
    /// </summary>
    public GameObject Transient { get; protected set; }
    /// <summary>
    /// Persistent text manager object.
    /// </summary>
    public Text2DManager PersistentText { get { return Persistent.GetComponent<Text2DManager>(); } }
    /// <summary>
    /// Transient text manager object.
    /// </summary>
    public Text2DManager TransientText { get { return Transient.GetComponent<Text2DManager>(); } }

    /// <summary>
    /// Create a new handler.
    /// </summary>
    public Text2DHandler()
    {
      Root = new GameObject(Name);
      Persistent = new GameObject("Persistent " + Name);
      Text2DManager textManager = Persistent.AddComponent<Text2DManager>();
      textManager.TextHandler = this;
      Persistent.transform.SetParent(Root.transform, false);
      Transient = new GameObject("Transient " + Name);
      textManager = Transient.AddComponent<Text2DManager>();
      textManager.TextHandler = this;
      Transient.transform.SetParent(Root.transform, false);
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Text2D"; } }

    /// <summary>
    /// <see cref="ShapeID.Text2D"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Text2D; } }

    /// <summary>
    /// Start the frame by flushing transient objects.
    /// </summary>
    /// <param name="frameNumber">A monotonic frame number.</param>
    /// <param name="maintainTransient">True to disable transient flush.</param>
    public override void BeginFrame(uint frameNumber, bool maintainTransient)
    {
      if (!maintainTransient)
      {
        TransientText.Clear();
      }
    }

    /// <summary>
    /// Initialise.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="serverRoot"></param>
    /// <param name="materials"></param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      Root.transform.SetParent(root.transform, false);
    }

    /// <summary>
    /// Message handler.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public override Error ReadMessage(PacketBuffer packet, BinaryReader reader)
    {
      switch ((ObjectMessageID)packet.Header.MessageID)
      {
      default:
      case ObjectMessageID.Null:
        return new Error(ErrorCode.InvalidMessageID, packet.Header.MessageID);

      case ObjectMessageID.Create:
        // Read the create message details.
        CreateMessage create = new CreateMessage();
        if (!create.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, packet.Header.MessageID);
        }
        return HandleMessage(create, packet, reader);

      case ObjectMessageID.Update:
        // Read the create message details.
        UpdateMessage update = new UpdateMessage();
        if (!update.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, packet.Header.MessageID);
        }
        return HandleMessage(update, packet, reader);

      case ObjectMessageID.Destroy:
        // Read the create message details.
        DestroyMessage destroy = new DestroyMessage();
        if (!destroy.Read(reader))
        {
          return new Error(ErrorCode.InvalidContent, packet.Header.MessageID);
        }
        return HandleMessage(destroy);
      }
    }

    /// <summary>
    /// Reset, clearing all text objects.
    /// </summary>
    public override void Reset()
    {
      PersistentText.Clear();
      TransientText.Clear();
    }

    /// <summary>
    /// Serialisation.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="info">Statistics</param>
    /// <returns></returns>
    public override Error Serialise(BinaryWriter writer, ref SerialiseInfo info)
    {
      PacketBuffer packet = new PacketBuffer();
      info.TransientCount = info.PersistentCount = 0u;

      if (!Serialise(TransientText, packet, writer, ref info.TransientCount))
      {
        return new Error(ErrorCode.SerialisationFailure);
      }

      if (!Serialise(PersistentText, packet, writer, ref info.PersistentCount))
      {
        return new Error(ErrorCode.SerialisationFailure);
      }

      return new Error();
    }

    private static bool Serialise(Text2DManager textManager, PacketBuffer packet, BinaryWriter writer, ref uint count)
    {
      Shapes.Text2D shape = new Shapes.Text2D(null);
      uint progressMarker = 0;
      int dataResult = 1;
      bool shapeOk = true;

      foreach (TextEntry entry in textManager.Entries)
      {
        ++count;
        shape.ID = entry.ID;
        shape.Category = entry.Category;
        shape.Flags = entry.ObjectFlags;
        shape.SetPosition(entry.Position.x, entry.Position.y, entry.Position.z);
        shape.Text = entry.Text;
        shape.WriteCreate(packet);

        shapeOk = packet.FinalisePacket();
        if (shapeOk)
        {
          packet.ExportTo(writer);

          if (shape.IsComplex)
          {
            dataResult = 1;
            while (dataResult > 0 && shapeOk)
            {
              shape.WriteData(packet, ref progressMarker);
              shapeOk = packet.FinalisePacket();
              if (shapeOk)
              {
                packet.ExportTo(writer);
              }
            }

            shapeOk = dataResult == 0;
          }
        }

        if (!shapeOk)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Handle create messages.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected virtual Error HandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      TextEntry text = new TextEntry();
      text.ID = msg.ObjectID;
      text.ObjectFlags = msg.Flags;
      text.Category = msg.Category;
      text.Position = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      text.Colour = Maths.ColourExt.ToUnity32(new Maths.Colour(msg.Attributes.Colour));

      // Read the text.
      int textLength = reader.ReadUInt16();
      if (textLength > 0)
      {
        byte[] textBytes = reader.ReadBytes(textLength);
        text.Text = System.Text.Encoding.UTF8.GetString(textBytes);
      }

      if (msg.ObjectID == 0)
      {
        TransientText.Add(text);
      }
      else
      {
        if (PersistentText.ContainsEntry(text.ID))
        {
          // Object ID already present. Check for replace flag.
          if ((msg.Flags & (ushort)ObjectFlag.Replace) == 0)
          {
            // Not replace flag => error.
            return new Error(ErrorCode.DuplicateShape, msg.ObjectID);
          }

          // Replace.
          PersistentText.Remove(text.ID);
        }
        PersistentText.Add(text);
      }

      return new Error();
    }

    /// <summary>
    /// Handle update messages.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected virtual Error HandleMessage(UpdateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      TextEntry text = new TextEntry();
      text.ID = msg.ObjectID;
      text.ObjectFlags = msg.Flags;
      text.Position = new Vector3(msg.Attributes.X, msg.Attributes.Y, msg.Attributes.Z);
      text.Colour = Maths.ColourExt.ToUnity32(new Maths.Colour(msg.Attributes.Colour));

      // Read the text.
      int textLength = reader.ReadUInt16();
      if (textLength > 0)
      {
        byte[] textBytes = reader.ReadBytes(textLength);
        text.Text = System.Text.Encoding.Default.GetString(textBytes);
      }

      if (text.ID != 0)
      {
        PersistentText.UpdateEntry(text);
      }

      return new Error();
    }

    /// <summary>
    /// Handle destroy messages.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected virtual Error HandleMessage(DestroyMessage msg)
    {
      PersistentText.Remove(msg.ObjectID);
      return new Error();
    }
  }
}
