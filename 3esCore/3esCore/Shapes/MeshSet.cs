using Tes.IO;
using Tes.Net;
using Tes.Maths;
using System;
using System.Collections.Generic;

namespace Tes.Shapes
{
  /// <summary>
  /// Defines a set of <see cref="MeshResource"/> references remote rendering.
  /// </summary>
  public class MeshSet : Shape
  {
    /// <summary>
    /// Construct an empty mesh set.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    public MeshSet(uint id = 0) : this(id, 0) { }

    /// <summary>
    /// Construct an empty mesh set.
    /// </summary>
    /// <param name="id">The shape ID. Zero for transient shapes.</param>
    /// <param name="category">Category to which the shape belongs.</param>
    public MeshSet(uint id, ushort category)
      : base((ushort)ShapeID.Mesh, id, category)
    {
    }

    /// <summary>
    /// Enumerate the mesh resource.
    /// </summary>
    public override IEnumerable<Resource> Resources
    {
      get
      {
        foreach (MeshResource res in _parts)
        {
          yield return res;
        }
      }
    }

    /// <summary>
    /// Queries the number of mesh resource parts.
    /// </summary>
    public int PartCount { get { return _parts.Count; } }
    /// <summary>
    /// Access a mesh resource path.
    /// </summary>
    /// <param name="index">The part index.</param>
    /// <returns>The requested part.</returns>
    public MeshResource PartAt(int index) { return _parts[index]; }

    /// <summary>
    /// Request the transformation for a part.
    /// </summary>
    /// <param name="index">The part index.</param>
    /// <returns>The requested part transformation matrix.</returns>
    public Matrix4 PartTransformAt(int index) { return _transforms[index]; }

    /// <summary>
    /// Add a part to the mesh set.
    /// </summary>
    /// <param name="part">The mesh resource to add.</param>
    /// <param name="transform">The local transform for <paramref name="part"/>.</param>
    /// <returns>This</returns>
    public MeshSet AddPart(MeshResource part, Matrix4 transform)
    {
      _parts.Add(part);
      _transforms.Add(transform);
      return this;
    }
    /// <summary>
    /// Add a part to the mesh set with an identity local transformation.
    /// </summary>
    /// <param name="part">The mesh resource to add.</param>
    /// <returns>This</returns>
    public MeshSet AddPart(MeshResource part) { return AddPart(part, Matrix4.Identity); }

    /// <summary>
    /// Override to write part details.
    /// </summary>
    /// <param name="packet">Packet buffer to write to.</param>
    /// <returns>true on success.</returns>
    public override bool WriteCreate(PacketBuffer packet)
    {
      if (!base.WriteCreate(packet))
      {
        return false;
      }

      packet.WriteBytes(BitConverter.GetBytes((ushort)_parts.Count), true);

      ObjectAttributes partAttributes = new ObjectAttributes();
      for (int i = 0; i < _parts.Count; ++i)
      {
        MeshResource part = _parts[i];
        Matrix4 transform = _transforms[i];

        partAttributes.SetFromTransform(transform);
        partAttributes.Color = 0xffffffffu;

        packet.WriteBytes(BitConverter.GetBytes((uint)part.ID), true);
        partAttributes.Write(packet);
      }

      return true;
    }

    /// <summary>
    /// Clone this shape.
    /// </summary>
    /// <returns>A deep copy of this object.</returns>
    public override object Clone()
    {
      MeshSet copy = new MeshSet();
      OnClone(copy);
      return copy;
    }

    /// <summary>
    /// Overridden to copy mesh parts.
    /// </summary>
    /// <param name="copy">The clone object.</param>
    protected void OnClone(MeshSet copy)
    {
      base.OnClone(copy);
      copy._parts = new List<MeshResource>(_parts.Count);
      copy._transforms = new List<Matrix4>(_transforms.Count);

      for (int i = 0; i < _parts.Count; ++i)
      {
        copy._parts.Add(_parts[i]);
      }

      for (int i = 0; i < _transforms.Count; ++i)
      {
        copy._transforms.Add(_transforms[i]);
      }
    }

    /// <summary>
    /// List of parts.
    /// </summary>
    protected List<MeshResource> _parts = new List<MeshResource>();
    /// <summary>
    /// Transforms corresponding to <see cref="_parts"/>.
    /// </summary>
    protected List<Matrix4> _transforms = new List<Matrix4>();
  }
}
