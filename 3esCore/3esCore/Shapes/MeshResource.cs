using System;
using System.Collections.Generic;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// Represents a mesh part or object. These are visualised via <see cref="MeshSet"/>,
  /// which may contain several <code>Mesh</code> parts.
  /// </summary>
  /// <remarks>
  /// The <see cref="Resource.TypeID"/> for Mesh is <see cref="Tes.Net.RoutingID.Mesh"/>.
  /// </remarks>
  public interface MeshResource : Resource
  {
    /// <summary>
    /// Root transform for the mesh. This defines the origin.
    /// </summary>
    Matrix4 Transform { get; }
    /// <summary>
    /// A global tint colour applied to the mesh.
    /// </summary>
    uint Tint { get; }
    /// <summary>
    /// The <see cref="DrawType"/> of the mesh.
    /// </summary>
    byte DrawType { get; }

    /// <summary>
    /// Defines the byte size used by indices in this mesh.
    /// </summary>
    /// <value>The size of each index value in bytes.</value>
    /// <remarks>
    /// Must be 0, 2 or 4. When 2 the <see cref="Indices2"/> property
    /// must be supported, and <see cref="Indices4"/> is optional. When
    /// 4, <see cref="Indices4"/> must be supported and <see cref="Indices2"/>
    /// is optional. When zero, neither property is supported: the mesh does not
    /// support indices.
    /// </remarks>
    int IndexSize { get; }

    /// <summary>
    /// Exposes the number of vertices in the mesh.
    /// </summary>
    /// <returns>The number of vertices in this mesh.</returns>
    /// <param name="stream">For future use. Must be zero.</param>
    uint VertexCount(int stream = 0);

    /// <summary>
    /// Exposes the number of indices in the mesh.
    /// </summary>
    /// <returns>The number of indices in this mesh.</returns>
    /// <param name="stream">For future use. Must be zero.</param>
    uint IndexCount(int stream = 0);

    /// <summary>
    /// Supports iteration of the vertices of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    Vector3[] Vertices(int stream = 0);

    /// <summary>
    /// Supports iteration of the indices of the mesh when using two byte indices.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    ushort[] Indices2(int stream = 0);
    /// <summary>
    /// Supports iteration of the indices of the mesh when using four byte indices.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    int[] Indices4(int stream = 0);

    /// <summary>
    /// Supports iteration of the normal of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    Vector3[] Normals(int stream = 0);

    /// <summary>
    /// Supports iteration of the UV coordinates of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    Vector2[] UVs(int stream = 0);

    /// <summary>
    /// Supports iteration of per vertex colours of the mesh.
    /// </summary>
    /// <param name="stream">For future use. Must be zero.</param>
    uint[] Colours(int stream = 0);
  }
}
