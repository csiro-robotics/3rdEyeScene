using Tes.IO;
using Tes.Net;
using Tes.Maths;
using System;
using System.Collections.Generic;
using System.IO;

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
      : base((ushort)ShapeID.MeshSet, id, category)
    {
    }

    /// <summary>
    /// Enumerate the mesh resource.
    /// </summary>
    public override IEnumerable<Resource> Resources
    {
      get
      {
        foreach (Part part in _parts)
        {
          yield return part.Resource;
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
    public MeshResource PartResource(int index) { return _parts[index].Resource; }

    /// <summary>
    /// Request the transformation for a part.
    /// </summary>
    /// <param name="index">The part index.</param>
    /// <returns>The requested part transformation matrix.</returns>
    public Matrix4 PartTransformAt(int index) { return _parts[index].Transform; }

    /// <summary>
    /// Request the colour for a part.
    /// </summary>
    /// <param name="index">The part index.</param>
    /// <returns>The requested part colour tint.</returns>
    public Colour PartColourAt(int index) { return _parts[index].Colour; }

    /// <summary>
    /// Add a part to the mesh set.
    /// </summary>
    /// <param name="part">The mesh resource to add.</param>
    /// <param name="transform">The local transform for <paramref name="part"/>.</param>
    /// <returns>This</returns>
    public MeshSet AddPart(MeshResource part, Matrix4 transform)
    {
      return AddPart(part, transform, new Colour(255, 255, 255));
    }

    /// <summary>
    /// Add a part to the mesh set with colour tint.
    /// </summary>
    /// <param name="part">The mesh resource to add.</param>
    /// <param name="transform">The local transform for <paramref name="part"/>.</param>
    /// <param name="colour">The part colour tint for <paramref name="part"/>.</param>
    /// <returns>This</returns>
    public MeshSet AddPart(MeshResource part, Matrix4 transform, Colour colour)
    {
      _parts.Add(new Part
      {
        Resource = part,
        Transform = transform,
        Colour = colour
      });
      return this;
    }

    /// <summary>
    /// Add a part to the mesh set with an identity local transformation.
    /// </summary>
    /// <param name="part">The mesh resource to add.</param>
    /// <returns>This</returns>
    public MeshSet AddPart(MeshResource part) { return AddPart(part, Matrix4.Identity, new Colour(255, 255, 255)); }

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
        uint partId = (_parts[i].Resource != null) ? (uint)_parts[i].Resource.ID : 0;

        partAttributes.SetFromTransform(_parts[i].Transform);
        partAttributes.Color = _parts[i].Colour.Value;

        packet.WriteBytes(BitConverter.GetBytes(partId), true);
        partAttributes.Write(packet);
      }

      return true;
    }

    /// <summary>
    /// Read the contents of a create message for a <c>MeshSet</c>.
    /// </summary>
    /// <param name="reader">Stream to read from</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Reads additional data about the mesh parts contained in the mesh. All parts are represented
    /// by a <see cref="PlaceholderMesh"/> as real mesh data cannot be resolved here.
    /// </remarks>
    public override bool ReadCreate(BinaryReader reader)
    {
      if (!_data.Read(reader))
      {
        return false;
      }

      // Read part details.
      // Note: we can only create placeholder meshes here for referencing resource IDs elsewhere.
      int partCount = reader.ReadUInt16();
      _parts.Clear();

      if (partCount > 0)
      {
        uint meshId;
        ObjectAttributes partAttributes = new ObjectAttributes();

        for (int i = 0; i < partCount; ++i)
        {
          meshId = reader.ReadUInt32();

          if (!partAttributes.Read(reader))
          {
            return false;
          }

          _parts.Add(new Part
          {
            Resource = new PlaceholderMesh(meshId),
            Transform = partAttributes.GetTransform(),
            Colour = new Colour(partAttributes.Colour)
          });
        }
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

      copy._parts = new List<Part>(_parts.Count);

      for (int i = 0; i < _parts.Count; ++i)
      {
        copy._parts.Add(_parts[i]);
      }
    }

    protected struct Part
    {
      public MeshResource Resource;
      public Matrix4 Transform;
      public Colour Colour;
    };

    /// <summary>
    /// List of parts.
    /// </summary>
    protected List<Part> _parts = new List<Part>();
  }
}
