using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Handles 3D text shapes.
  /// </summary>
  public class Text3DHandler : ShapeHandler
  {
    /// <summary>
    /// No solid mesh.
    /// </summary>
    public override Mesh SolidMesh { get { return null; } }
    /// <summary>
    /// Irrelevant.
    /// </summary>
    public override Mesh WireframeMesh { get { return null; } }

    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public Text3DHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      if (Root != null)
      {
        Root.name = Name;
      }
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Text3D"; } }

    /// <summary>
    /// <see cref="ShapeID.Text3D"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Text3D; } }

    /// <summary>
    /// Create an object for 3D text.
    /// </summary>
    /// <returns></returns>
    protected override GameObject CreateObject()
    {
      GameObject obj = new GameObject();
      obj.AddComponent<TextMesh>();
      //obj.AddComponent<MeshRenderer>();
      obj.AddComponent<ShapeComponent>();
      return obj;
    }

    /// <summary>
    /// Initialise the shape handler by initialising the shape scene root and
    /// fetching the default materials.
    /// </summary>
    /// <param name="root">The 3rd Eye Scene root object.</param>
    /// <param name="serverRoot">The server scene root (transformed into the server reference frame).</param>
    /// <param name="materials">Material library from which to resolve materials.</param>
    public override void Initialise(GameObject root, GameObject serverRoot, MaterialLibrary materials)
    {
      // Keep in Unity frame.
      Root.transform.SetParent(root.transform, false);
      if (Root.GetComponent<ScreenFacing>() == null)
      {
        Root.AddComponent<ScreenFacing>();
      }
    }

    /// <summary>
    /// Encode transform attributes for text.
    /// </summary>
    /// <param name="attr"></param>
    /// <param name="obj"></param>
    /// <param name="comp"></param>
    protected override void EncodeAttributes(ref ObjectAttributes attr, GameObject obj, ShapeComponent comp)
    {
      Transform transform = obj.transform;
      // Convert position to Unity position.
      Vector3 pos = FrameTransform.UnityToRemote(obj.transform.position, ServerInfo.CoordinateFrame);
      attr.X = pos.x;
      attr.Y = pos.y;
      attr.Z = pos.z;
      attr.RotationX = transform.localRotation.x;
      attr.RotationY = transform.localRotation.y;
      attr.RotationZ = transform.localRotation.z;
      attr.RotationW = transform.localRotation.w;
      attr.ScaleX = 1.0f;
      attr.ScaleY = 1.0f;
      attr.ScaleZ = 12.0f;

      TextMesh text = obj.GetComponent<TextMesh>();
      if (text != null)
      {
        attr.ScaleZ = text.fontSize;
      }
      if (comp != null)
      {
        attr.Colour = ShapeComponent.ConvertColour(comp.Colour);
      }
      else
      {
        attr.Colour = 0xffffffu;
      }
    }

    /// <summary>
    /// Handle additional <see cref="CreateMessage"/> data.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(GameObject obj, CreateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      // Convert position to Unity position.
      obj.transform.localPosition = FrameTransform.RemoteToUnity(obj.transform.localPosition, ServerInfo.CoordinateFrame);

      // Read the text in the buffer.
      int textLength = reader.ReadUInt16();
      if (textLength > 0)
      {
        TextMesh textMesh = obj.GetComponent<TextMesh>();
        if (textMesh == null)
        {
          textMesh = obj.AddComponent<TextMesh>();
        }

        byte[] textBytes = reader.ReadBytes(textLength);
        textMesh.text = System.Text.Encoding.UTF8.GetString(textBytes);
        ShapeComponent shapeComp = obj.GetComponent<ShapeComponent>();
        if (shapeComp != null)
        {
          textMesh.fontSize = (int)msg.Attributes.ScaleZ;
          textMesh.color = shapeComp.Colour;
        }
      }

      if ((msg.Flags & (ushort)Text3DFlag.SceenFacing) != 0)
      {
        // Need to use -forward for text.
        ScreenFacing.AddToManager(obj, -Vector3.forward, Vector3.up);
      }

      return base.PostHandleMessage(obj, msg, packet, reader);
    }

    /// <summary>
    /// Handle additional <see cref="UpdateMessage"/> data.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(GameObject obj, UpdateMessage msg, PacketBuffer packet, BinaryReader reader)
    {
      TextMesh textMesh = obj.GetComponent<TextMesh>();
      ShapeComponent shapeComp = obj.GetComponent<ShapeComponent>();
      if (textMesh != null && shapeComp != null)
      {
        textMesh.fontSize = (int)msg.Attributes.ScaleZ;
        textMesh.color = shapeComp.Colour;
      }

      // Convert position to Unity position.
      obj.transform.localPosition = FrameTransform.RemoteToUnity(obj.transform.localPosition, ServerInfo.CoordinateFrame);

      if (shapeComp != null && (msg.Flags & (ushort)Text3DFlag.SceenFacing) != (shapeComp.ObjectFlags & (ushort)Text3DFlag.SceenFacing))
      {
        // Screen facing flag changed.
        if ((msg.Flags & (ushort)Text3DFlag.SceenFacing) != 0)
        { 
          // Need to use -forward for text.
          ScreenFacing.AddToManager(obj, -Vector3.forward, Vector3.up);
        }
        else
        {
          // Need to use -forward for text.
          ScreenFacing.RemoveFromManager(obj);
        }
      }
      return new Error();
    }

    /// <summary>
    /// Creates a text shape for serialisation.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeComponent shapeComponent)
    {
      TextMesh text = shapeComponent.GetComponent<TextMesh>();
      if (text != null)
      {
        Shapes.Shape shape = new Shapes.Text3D(text.text);
        ConfigureShape(shape, shapeComponent);
        return shape;
      }
      return null;
    }
  }
}
