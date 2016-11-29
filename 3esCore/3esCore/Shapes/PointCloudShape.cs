using System;
using System.Collections.Generic;
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
    /// Point render size request. Zero for the default.
    /// </summary>
    public byte PointSize { get; set; }

    /// <summary>
    /// Create a new point cloud shape.
    /// </summary>
    /// <param name="cloud">The cloud mesh resource.</param>
    /// <param name="id">The shape ID. Zero for transient.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    /// <param name="pointSize">Point size override. Zero for default.</param>
    public PointCloudShape(MeshResource cloud, uint id = 0, ushort category = 0, byte pointSize = 0)
      : base((ushort)Tes.Net.ShapeID.PointCloud, id, category)
    {
      PointCloud = cloud;
      PointSize = pointSize;
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
    public void SetIndices(uint[] indices) { _indices = indices; }

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
      packet.WriteBytes(new byte[] { PointSize }, false);
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
      copy.PointSize = PointSize;
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
