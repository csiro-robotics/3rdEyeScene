using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Net;

namespace Tes.Shapes
{
  /// <summary>
  /// A <see cref="Shape"/> which renders a set of points as in a point cloud.
  /// </summary>
  /// <remarks>
  /// The points are contained in a <see cref="MeshResource"/> (e.g., <see cref="PointCloud"/>)
  /// and may be shared between <see cref="PointCloudShape"/> shapes. The mesh resource should
  /// have a <see cref="MeshResource.DrawType"/> of <see cref="Tes.Net.MeshDrawType.Points"/>,
  /// or the behaviour may be undefined.
  ///
  /// The <see cref="PointCloudShape"/> shape supports the view into the <see cref="MeshResource"/>
  /// by having its own set of indices (see <see cref="SetIndices(uint[])"/>).
  /// </remarks>
  public class PointCloudShape : Shape
  {
    /// <summary>
    /// The point cloud resource.
    /// </summary>
    public MeshResource PointCloud { get; protected set; }
    /// <summary>
    /// Point render scale request. Zero for the default.
    /// </summary>
    public float PointScale { get; set; }

    /// <summary>
    /// Default constructor for an empty, transient cloud.
    /// </summary>
    public PointCloudShape() : base((ushort)Tes.Net.ShapeID.PointCloud)
    {
      IsComplex = true;
    }

    /// <summary>
    /// Create a new point cloud shape.
    /// </summary>
    /// <param name="cloud">The cloud mesh resource.</param>
    /// <param name="id">The shape ID. Zero for transient.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pointScale">Point scale override. Zero for default.</param>
    public PointCloudShape(MeshResource cloud, uint id = 0, ushort category = 0, float pointScale = 0)
      : base((ushort)Tes.Net.ShapeID.PointCloud, id, category)
    {
      PointCloud = cloud;
      PointScale = pointScale;
      IsComplex = true;
    }

    /// <summary>
    /// The number of explicit indices provided.
    /// </summary>
    /// <seealso cref="SetIndices(uint[])"/>
    public uint IndexCount { get { return (_indices != null) ? (uint)_indices.Length : 0u; } }
    /// <summary>
    /// Access one of the explicitly set index values.
    /// </summary>
    /// <seealso cref="SetIndices(uint[])"/>
    /// <exception cref="NullReferenceException">Thrown when indices are not set.</exception>
    public uint Index(uint at) { return _indices[at]; }
    /// <summary>
    /// Sets an array of explicit indices used to limit the points displayed.
    /// </summary>
    /// <param name="indices">The index array referencing the points to display.</param>
    /// <remarks>
    /// By default, the entire point cloud resource is rendered. By assigning indices,
    /// the displayed points are limited to those referenced in the given array.
    /// </remarks>
    public PointCloudShape SetIndices(uint[] indices) { _indices = indices; return this; }
    public PointCloudShape SetIndices(int[] indices)
    {
      if (indices == null)
      {
        _indices = null;
        return this;
      }

      if (_indices == null || _indices.Length != indices.Length)
      {
        _indices = new uint[indices.Length];
      }

      Array.Copy(indices, _indices, indices.Length);
      return this;
    }

    /// <summary>
    /// Enumerate the shape's resources.
    /// </summary>
    public override IEnumerable<Resource> Resources
    {
      get
      {
        if (PointCloud != null)
        {
          yield return PointCloud;
        }
      }
    }

    /// <summary>
    /// Writes the standard create message plus the number of indices used to restrict cloud viewing (if any - UInt32).
    /// </summary>
    /// <param name="packet">The packet to write to.</param>
    /// <returns>True on success.</returns>
    public override bool WriteCreate(PacketBuffer packet)
    {
      if (!base.WriteCreate(packet))
      {
        return false;
      }

      // Write the mesh ID and number of indices in this cloud.
      uint valueU32 = PointCloud.ID;
      packet.WriteBytes(BitConverter.GetBytes(valueU32), true);
      valueU32 = IndexCount;
      packet.WriteBytes(BitConverter.GetBytes(valueU32), true);
      packet.WriteBytes(BitConverter.GetBytes(PointScale), true);
      return true;
    }

    /// <summary>
    /// Overridden to write the point cloud indices.
    /// </summary>
    /// <param name="packet">Packet to write to.</param>
    /// <param name="progressMarker">Number of indices written so far.</param>
    /// <returns>0 when complete, 1 to call again.</returns>
    public override int WriteData(PacketBuffer packet, ref uint progressMarker)
    {
      // Max items based on packet size of 0xffff, minus some overhead divide by index size.
      DataMessage msg = new DataMessage();
      const uint MaxItems = (uint)((0xffff - 256) / 4);
      packet.Reset(RoutingID, DataMessage.MessageID);

      msg.ObjectID = ID;
      msg.Write(packet);

      // Write indices for this view into the cloud.
      uint offset = progressMarker;
      uint count = IndexCount - progressMarker;
      if (count > MaxItems)
      {
        count = MaxItems;
      }

      // Use 32-bits for both values though count will never need greater than 16-bit.
      packet.WriteBytes(BitConverter.GetBytes(offset), true);
      packet.WriteBytes(BitConverter.GetBytes(count), true);

      if (count != 0)
      {
        for (uint i = offset; i < offset + count; ++i)
        {
          packet.WriteBytes(BitConverter.GetBytes(Index(i)), true);
        }
      }

      progressMarker += count;
      return (progressMarker < IndexCount) ? 1 : 0;
    }

    /// <summary>
    /// Read the contents of a create message for a <c>MeshSet</c>.
    /// </summary>
    /// <param name="reader">Stream to read from</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Reads additional data for the cloud and the cloud mesh ID. The cloud mesh is represented
    /// by a <see cref="PlaceholderMesh"/> as real mesh data cannot be resolved here.
    /// </remarks>
    public override bool ReadCreate(PacketBuffer packet, BinaryReader reader)
    {
      if (!base.ReadCreate(packet, reader))
      {
        return false;
      }

      uint cloudID = reader.ReadUInt32();
      uint indexCount = reader.ReadUInt32();
      PointScale = reader.ReadSingle();

      PointCloud = new PlaceholderMesh(cloudID);
      _indices = (indexCount > 0) ? new uint[indexCount] : null;

      return true;
    }

    /// <summary>
    /// Read additional index data for the cloud.
    /// </summary>
    /// <param name="packet">The buffer from which the reader reads.</param>
    /// <param name="reader">Stream to read from</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Reads additional index data if required.
    ///
    /// Fails if the <see cref="DataMessage.ObjectID"/> does not match <see cref="Shape.ID"/>.
    /// </remarks>
    public override bool ReadData(PacketBuffer packet, BinaryReader reader)
    {
      DataMessage msg = new DataMessage();
      if (!msg.Read(reader))
      {
        return false;
      }

      if (msg.ObjectID != ID)
      {
        return false;
      }

      uint offset = reader.ReadUInt32();
      uint itemCount = reader.ReadUInt32();

      if (offset + itemCount > 0)
      {
        if (_indices == null || offset + itemCount > _indices.Length)
        {
          return false;
        }
      }

      for (uint i = offset; i < offset + itemCount; ++i)
      {
        _indices[i] = reader.ReadUInt32();
      }

      return true;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      PointCloudShape copy = new PointCloudShape(null);
      OnClone(copy);
      return copy;
    }

    /// <summary>
    /// Actions to perform on cloning.
    /// </summary>
    /// <param name="copy">The cloned object.</param>
    /// <remarks>
    /// Clones the indices if any.
    /// </remarks>
    protected void OnClone(PointCloudShape copy)
    {
      base.OnClone(copy);
      copy.PointCloud = PointCloud;
      copy.PointScale = PointScale;
      if (_indices != null && _indices.Length != 0)
      {
        Array.Copy(_indices, copy._indices, _indices.Length);
      }
    }

    /// <summary>
    /// Optional indices.
    /// </summary>
    private uint[] _indices = null;
  }
}
