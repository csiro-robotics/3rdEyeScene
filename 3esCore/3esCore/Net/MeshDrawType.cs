namespace Tes.Net
{
  /// <summary>
  /// Defines mesh drawing topology.
  /// </summary>
  public enum MeshDrawType
  {
    /// <summary>
    /// Vertices and indices represent points.
    /// </summary>
    Points,
    /// <summary>
    /// Vertices/indices come in pairs, defining connected lines.
    /// </summary>
    Lines,
    /// <summary>
    /// Vertices/indices come in triples, defining connected triangles.
    /// </summary>
    Triangles,
    //LineLoop,
    //LineStrip,
    //TriangleStrip,
    //TriangleFan
  }
}
