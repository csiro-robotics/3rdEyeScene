using System;

namespace Tes.Handlers
{
  /// <summary>
  /// A placeholder for a mesh resource. It needs only expose the resource ID.
  /// </summary>
  /// <remarks>
  /// This is used to create mesh associations when serialising shapes in the viewer.
  /// </remarks>
  public class MeshResourcePlaceholder : Shapes.MeshBase
  {
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>0</returns>
    public override uint VertexCount(int stream = 0) { return 0u; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>0</returns>
    public override uint IndexCount(int stream = 0) { return 0u; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <returns>0</returns>
    public override int IndexSize { get { return 0; } }

    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>null</returns>
    public override Maths.Vector3[] Vertices(int stream = 0) { return null; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>null</returns>
    public override ushort[] Indices2(int stream = 0) { return null; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>null</returns>
    public override int[] Indices4(int stream = 0) { return null; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>null</returns>
    public override Maths.Vector3[] Normals(int stream = 0) { return null; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>null</returns>
    public override Maths.Vector2[] UVs(int stream = 0) { return null; }
    /// <summary>
    /// Dummy method.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>null</returns>
    public override uint[] Colours(int stream = 0) { return null; }

    /// <summary>
    /// Instantiate a place holder mesh resource to reference the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The mesh resource ID of interest.</param>
    public MeshResourcePlaceholder(uint id)
    {
      ID = id;
    }
  }
}
