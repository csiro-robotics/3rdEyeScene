using System;
using Tes.IO;
using Tes.Maths;
using Tes.Net;

namespace Tes.Shapes
{
  public class MultiShape : Shape
  {
    /// <summary>
    /// Maximum number of shapes in a multi-shape. Limited by packet size.
    /// </summary>
    public static readonly int ShapeCountLimit = 1024;

    public MultiShape(Shape[] shapes, Vector3 position, Quaternion rotation, Vector3 scale)
      : base(shapes[0].RoutingID, shapes[0].ID, shapes[0].Category)
    {
      if (shapes.Length > ShapeCountLimit)
      {
        // TODO: (KS) custom exception.
        throw new Exception($"Multi-shape limit exceeded: {shapes.Length} > {ShapeCountLimit}");
      }

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
      : this(shapes, position, rotation, Vector3.One) {}

    public MultiShape(Shape[] shapes, Vector3 position)
      : this(shapes, position, Quaternion.Identity, Vector3.One) {}

    public MultiShape(Shape[] shapes)
      : this(shapes, Vector3.Zero, Quaternion.Identity, Vector3.One) {}

    public override bool WriteCreate(PacketBuffer packet)
    {
      if (!base.WriteCreate(packet))
      {
        return false;
      }

      // Write multi-shape details.
      UInt16 itemCount = (UInt16)_shapes.Length;
      packet.WriteBytes(BitConverter.GetBytes(itemCount), true);
      // Write the multi-shape attributes.
      for (int i = 0; i < _shapes.Length; ++i)
      {
        if (!_shapes[i].GetAttributes().Write(packet))
        {
          return false;
        }
      }

      return true;
    }

    private Shape[] _shapes = null;
  }
}