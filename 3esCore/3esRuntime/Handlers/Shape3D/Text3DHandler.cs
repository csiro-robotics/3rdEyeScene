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
  public class Text3DHandler : ShapeHandler, IShapeData
  {
    public struct TextShapeData : IShapeData
    {
      public Mesh Mesh;
      public Material Material;
      public string Text;
      public int FontSize;
      public bool ScreenFacing;
    }

    public delegate bool CreateTextMeshDelegate(string text, ref Mesh mesh, ref Material material);

    public CreateTextMeshDelegate CreateTextMeshHandler;

    /// <summary>
    /// Create the shape handler.
    /// </summary>
    /// <param name="categoryCheck"></param>
    public Text3DHandler(Runtime.CategoryCheckDelegate categoryCheck)
      : base(categoryCheck)
    {
      _shapeCache.AddShapeDataType<TextShapeData>();
      _transientCache.AddShapeDataType<TextShapeData>();
    }

    /// <summary>
    /// Handler name.
    /// </summary>
    public override string Name { get { return "Text3D"; } }

    /// <summary>
    /// <see cref="ShapeID.Text3D"/>
    /// </summary>
    public override ushort RoutingID { get { return (ushort)Tes.Net.ShapeID.Text3D; } }

    public override void Render(CameraContext cameraContext)
    {
      Render(cameraContext, _transientCache);
      Render(cameraContext, _shapeCache);
    }

    protected void Render(CameraContext cameraContext, ShapeCache shapeCache)
    {
      // TODO: (KS) verify material setup.
      // TODO: (KS) incorporate the 3es scene transform.
      // TODO: (KS) handle multiple cameras (code only tailored to one).
      Vector3 cameraPosition = (Vector3)cameraContext.CameraToWorldTransform.GetColumn(3);
      int sideAxis = CoordinateFrameUtil.AxisIndex(ServerInfo.CoordinateFrame, 0);
      int forwardAxis = CoordinateFrameUtil.AxisIndex(ServerInfo.CoordinateFrame, 1);
      int upAxis = CoordinateFrameUtil.AxisIndex(ServerInfo.CoordinateFrame, 2);
      // Set flipAxis if up axis is negative. We will negate again for right handed base systems.
      bool flipAxis = !CoordinateFrameUtil.LeftHanded(ServerInfo.CoordinateFrame);

      // TODO: (KS) May have the negation the wrong way around for getting into Unity's left handed frame.
      if (CoordinateFrameUtil.LeftHanded(ServerInfo.CoordinateFrame))
      {
        flipAxis = !flipAxis;
      }

      // Walk the items in the shape cache.
      foreach (int shapeIndex in shapeCache.ShapeIndices)
      {
        // TODO: (KS) check category enabled.
        // if (shapeCache.GetShapeDataByIndex<CreateMessage>(shapeIndex).Category)

        // Get transform and text data.
        Matrix4x4 transform = shapeCache.GetShapeTransformByIndex(shapeIndex);
        TextShapeData textData = shapeCache.GetShapeDataByIndex<TextShapeData>(shapeIndex);

        if (textData.Mesh == null || textData.Material == null)
        {
          // No mesh/material. Try instantiate via the delegate.
          if (CreateTextMeshHandler != null)
          {
            CreateTextMeshHandler(textData.Text, ref textData.Mesh, ref textData.Material);
          }

          if (textData.Mesh == null || textData.Material == null)
          {
            continue;
          }

          // Newly creates mesh/material. Store the changes.
          shapeCache.SetShapeDataByIndex<TextShapeData>(shapeIndex, textData);
        }

        if (textData.ScreenFacing)
        {
          Vector3 textPosition = (Vector3)transform.GetColumn(3);
          Vector3 toCamera = cameraPosition - textPosition;
          // Remove any height component from the camera. This depends on the server settings.
          toCamera[upAxis] = 0;

          if (toCamera.sqrMagnitude > 1e-3f)
          {
            toCamera = toCamera.normalized;
            Vector3 up = Vector3.zero;
            up[upAxis] = 1.0f;
            Vector3 side = Vector3.Cross(toCamera, up);
            // Build new rotation axes using toCamera for forward and a new Up axis.
            transform.SetColumn(sideAxis, new Vector4(side.x, side.y, side.z));
            transform.SetColumn(forwardAxis, new Vector4(toCamera.x, toCamera.y, toCamera.z));
            transform.SetColumn(upAxis, new Vector4(up.x, up.y, up.z));
            transform.SetColumn(3, new Vector4(textPosition.x, textPosition.y, textPosition.z, 1.0f));

            // Write the transform back to the shape cache. This maintains consistency close to the camera.
            shapeCache.SetShapeTransformByIndex(shapeIndex, transform);
          }
          // else too close to the camera to build a rotation.
        }

        // This probably needs to be applied before screen facing.
        transform = cameraContext.TesSceneToWorldTransform * transform;
        cameraContext.OpaqueBuffer.DrawMesh(textData.Mesh, transform, textData.Material);

        // TODO: (KS) resolve procedural rendering without a game object. Consider TextMeshPro.
        // TODO: (KS) select opaque layer.
        // Graphics.DrawMesh(textData.Mesh, transform, material, 0);
      }
    }

    /// <summary>
    /// Handle additional <see cref="CreateMessage"/> data.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <param name="cache"></param>
    /// <param name="shapeIndex"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(CreateMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      if (shapeIndex >= 0)
      {
        // Read the text in the buffer.
        TextShapeData textData = new TextShapeData();
        int textLength = reader.ReadUInt16();
        textData.Mesh = null;
        textData.Text = string.Empty;
        if (textLength > 0)
        {
          byte[] textBytes = reader.ReadBytes(textLength);
          textData.Text = System.Text.Encoding.UTF8.GetString(textBytes);
        }

        textData.FontSize = (int)msg.Attributes.ScaleZ;

        if ((msg.Flags & (ushort)Text3DFlag.ScreenFacing) != 0)
        {
          // Need to use -forward for text.
          // ScreenFacing.AddToManager(obj, -Vector3.forward, Vector3.up);
          textData.ScreenFacing = true;
        }
        cache.SetShapeDataByIndex(shapeIndex, textData);
      }

      return base.PostHandleMessage(msg, packet, reader, cache, shapeIndex);
    }

    /// <summary>
    /// Handle additional <see cref="UpdateMessage"/> data.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="msg"></param>
    /// <param name="packet"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected override Error PostHandleMessage(UpdateMessage msg, PacketBuffer packet, BinaryReader reader,
                                               ShapeCache cache, int shapeIndex)
    {
      TextShapeData textData = cache.GetShapeDataByIndex<TextShapeData>(shapeIndex);

      // textData.Mesh.fontSize = (int)msg.Attributes.ScaleZ;
      // textData.Mesh.color = Maths.ColourExt.ToUnity32(new Maths.Colour(msg.Attributes.Colour));
      textData.ScreenFacing = (msg.Flags & (ushort)Text3DFlag.ScreenFacing) != 0;

      cache.SetShapeDataByIndex(shapeIndex, textData);

      return base.PostHandleMessage(msg, packet, reader, cache, shapeIndex);
    }

    /// <summary>
    /// Creates a text shape for serialisation.
    /// </summary>
    /// <param name="shapeComponent">The component to create a shape for.</param>
    /// <returns>A shape instance suitable for configuring to generate serialisation messages.</returns>
    protected override Shapes.Shape CreateSerialisationShape(ShapeCache cache, int shapeIndex, CreateMessage shape)
    {
      TextShapeData textData = cache.GetShapeDataByIndex<TextShapeData>(shapeIndex);
      // Note: initialise position to zero. SetAttributes() below will overwrite this.
      var textShape = new Shapes.Text3D(textData.Text, shape.ObjectID, shape.Category, Maths.Vector3.Zero);
      textShape.SetAttributes(shape.Attributes);
      return textShape;
    }
  }
}
