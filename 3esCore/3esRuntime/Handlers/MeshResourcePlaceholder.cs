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
    public override uint VertexCount(int stream = 0) { return 0u; }
    public override uint IndexCount(int stream = 0) { return 0u; }
    public override int IndexSize { get { return 0; } }

    public override Maths.Vector3[] Vertices(int stream = 0) { return null; }
    public override ushort[] Indices2(int stream = 0) { return null; }
    public override int[] Indices4(int stream = 0) { return null; }
    public override Maths.Vector3[] Normals(int stream = 0) { return null; }
    public override Maths.Vector2[] UVs(int stream = 0) { return null; }
    public override uint[] Colours(int stream = 0) { return null; }

    public MeshResourcePlaceholder(uint id)
    {
      ID = id;
    }
  }
}
