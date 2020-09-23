using System;
using Tes.IO;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  public class MultiShape : Shape
  {
    /// <summary>
    /// Maximum number of shapes in a multi-shape packet. Limited by packet size.
    /// </summary>
    public static readonly int BlockCountLimit = 1024;
    /// <summary>
    /// Maximum number of shapes in a multi-shape.
    /// </summary>
    public static readonly int ShapeCountLimit = 0xffff;

    public MultiShape(Shape[] shapes, Vector3 position, Quaternion rotation, Vector3 scale)
      : base(shapes[0].RoutingID, shapes[0].ID, shapes[0].Category)
    {
      if (shapes.Length > ShapeCountLimit)
      {
        // TODO: (KS) custom exception.
        throw new Exception($"Multi-shape limit exceeded: {shapes.Length} > {ShapeCountLimit}");
      }

      IsComplex = true;

      _shapes = shapes;
      _data = shapes[0].Data.Clone();
      _data.Attributes.X = position.X;
      _data.Attributes.Y = position.Y;
      _data.Attributes.Z = position.Z;
      _data.Attributes.RotationX = rotation.X;
      _data.Attributes.RotationY = rotation.Y;
      _data.Attributes.RotationZ = rotation.Z;
      _data.Attributes.RotationW = rotation.W;
      _data.Attributes.ScaleX = scale.X;
      _data.Attributes.ScaleY = scale.Y;
      _data.Attributes.ScaleZ = scale.Z;
      _data.Flags |= (ushort)ObjectFlag.MultiShape;
    }

    public MultiShape(Shape[] shapes, Vector3 position, Quaternion rotation)
      : this(shapes, position, rotation, Vector3.One) { }

    public MultiShape(Shape[] shapes, Vector3 position)
      : this(shapes, position, Quaternion.Identity, Vector3.One) { }

    public MultiShape(Shape[] shapes)
      : this(shapes, Vector3.Zero, Quaternion.Identity, Vector3.One) { }

    public override bool WriteCreate(PacketBuffer packet)
    {
      if (!base.WriteCreate(packet))
      {
        return false;
      }

      // Write multi-shape details.
      UInt32 itemCount = (UInt32)_shapes.Length;
      UInt16 blockCount = (UInt16)Math.Min(itemCount, BlockCountLimit);
      packet.WriteBytes(BitConverter.GetBytes(itemCount), true);
      packet.WriteBytes(BitConverter.GetBytes(blockCount), true);

      bool writeDoublePrecision = (_data.Flags & (ushort)ObjectFlag.DoublePrecision) != 0;
      // Write the multi-shape attributes.
      for (int i = 0; i < blockCount; ++i)
      {
        if (!_shapes[i].GetAttributes().Write(packet, writeDoublePrecision))
        {
          return false;
        }
      }

      return true;
    }

    public override int WriteData(PacketBuffer packet, ref uint progressMarker)
    {
      if (_shapes.Length <= BlockCountLimit)
      {
        // Nothing more to write. Creation packet was enough.
        return 0;
      }

      DataMessage msg = new DataMessage();
      msg.ObjectID = ID;
      packet.Reset(RoutingID, DataMessage.MessageID);
      msg.Write(packet);

      UInt32 itemOffset = (progressMarker + (uint)BlockCountLimit);
      UInt32 remainingItems = (uint)_shapes.Length - itemOffset;
      UInt16 blockCount = (UInt16)(Math.Min(remainingItems, BlockCountLimit));

      packet.WriteBytes(BitConverter.GetBytes(blockCount), true);

      bool writeDoublePrecision = (_data.Flags & (ushort)ObjectFlag.DoublePrecision) != 0;
      for (uint i = 0; i < blockCount; ++i)
      {
        if (!_shapes[itemOffset + i].GetAttributes().Write(packet, writeDoublePrecision))
        {
          return -1;
        }
      }

      progressMarker += blockCount;

      if (remainingItems > blockCount)
      {
        // More to come.
        return 1;
      }

      // All done.
      return 0;
    }

    private Shape[] _shapes = null;
  }
}