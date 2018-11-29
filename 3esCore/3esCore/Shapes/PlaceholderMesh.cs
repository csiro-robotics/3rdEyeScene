using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// An empty mesh object which may be used as a placeholder for resolving IDs.
  /// </summary>
  public class PlaceholderMesh : MeshBase
  {
    /// <summary>
    /// Default constructor. ID is set to zero.
    /// </summary>
    public PlaceholderMesh() { ID = 0; }

    /// <summary>
    /// Constructor setting the mesh ID. This is the ID to be generally used.
    /// </summary>
    /// <param name="id">ID for the placeholder mesh.</param>
    public PlaceholderMesh(uint id) { ID = id; }

    /// <summary>
    /// Not used (returns 4).
    /// </summary>
    public override int IndexSize => 4;

    /// <summary>
    /// Not used.
    /// </summary>
    /// <param name="stream">Ignored.</param>
    /// <returns>null</returns>
    public override uint[] Colours(int stream = 0)
    {
      return null;
    }

    /// <summary>
    /// Always zero.
    /// </summary>
    /// <param name="stream">Ignored.</param>
    /// <returns>0</returns>
    public override uint IndexCount(int stream = 0)
    {
      return 0u;
    }

    /// <summary>
    /// Not used.
    /// </summary>
    /// <param name="stream">Ignored.</param>
    /// <returns>null</returns>
    public override Vector3[] Normals(int stream = 0)
    {
      return null;
    }

    /// <summary>
    /// Not used.
    /// </summary>
    /// <param name="stream">Ignored.</param>
    /// <returns>null</returns>
    public override Vector2[] UVs(int stream = 0)
    {
      return null;
    }

    /// <summary>
    /// Always zero.
    /// </summary>
    /// <param name="stream">Ignored.</param>
    /// <returns>0</returns>
    public override uint VertexCount(int stream = 0)
    {
      return 0u;
    }

    /// <summary>
    /// Not used.
    /// </summary>
    /// <param name="stream">Ignored.</param>
    /// <returns>null</returns>
    public override Vector3[] Vertices(int stream = 0)
    {
      return null;
    }
  }
}
