using System;

namespace Tes.Net
{
  public enum DataStreamType
  {
    None,
    Int8,
    UInt8,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float32,
    Float64,
    PackedFloat16,
    PackedFloat32,

    // Aliases matching .Net types.
    SByte = Int8,
    Byte = UInt8,
    Single = Float32,
    Double = Float64
  }

  public static class DataStreamTypeInfo
  {
    public static int SizeoOf(DataStreamType type)
    {
      switch (type)
      {
        case DataStreamType.None:
          return 0;
        case DataStreamType.Int8:
          return 1;
        case DataStreamType.UInt8:
          return 1;
        case DataStreamType.Int16:
          return 2;
        case DataStreamType.UInt16:
          return 2;
        case DataStreamType.Int32:
          return 4;
        case DataStreamType.UInt32:
          return 4;
        case DataStreamType.Int64:
          return 8;
        case DataStreamType.UInt64:
          return 8;
        case DataStreamType.Float32:
          return 4;
        case DataStreamType.Float64:
          return 8;
        case DataStreamType.PackedFloat16:
          return 2;
        case DataStreamType.PackedFloat32:
          return 4;
        default:
          break;
      }

      return 0;
    }
  }
}
