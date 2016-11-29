using System;
using System.Collections.Generic;
using Tes.Maths;

namespace Tes.Shapes
{
  /// <summary>
  /// A simple implementation of the <see cref="PointCloud"/> interface.
  /// </summary>
  /// <remarks>
  /// This implementation stores point cloud positions, normals and colours in separate
  /// lists/arrays. Normals and colours are optional.
  /// 
  /// Note: when adding normals and/or colours it is essentially that the user maintain
  /// the correct number of normals/points, matching the <see cref="PointCount"/>. Otherwise,
  /// undefined behaviour may ensue.
  /// </remarks>
  public class PointCloud : MeshBase
  {
    /// <summary>
    /// Construct a new point cloud.
    /// </summary>
    /// <param name="id">The cloud ID. Shared with other <see cref="MeshResource"/> objects.</param>
    /// <param name="reserve">Number of point vertices to reserve memroy for.</param>
    public PointCloud(uint id, int reserve = 0)
    {
      ID = id;
      MeshDrawType = Net.MeshDrawType.Points;
      if (reserve > 0)
      {
        _points = new List<Vector3>(reserve * 3);
      }
      else
      {
        _points = new List<Vector3>();
      }
      CalculateNormals = false;
    }

    /// <summary>
    /// Zero.
    /// </summary>
    public override int IndexSize { get { return 0; } }

    /// <summary>
    /// The number of points in the cloud.
    /// </summary>
    public uint PointCount { get { return (uint)_points.Count; } }
    /// <summary>
    /// Access the point vertex array (read only).
    /// </summary>
    public Vector3[] Points { get { return _points.ToArray(); } }
    /// <summary>
    /// Access the point normals if present (read only).
    /// </summary>
    /// <remarks>
    /// Null if not using point normals.
    /// </remarks>
    public Vector3[] PointNormals { get { return (_normals != null) ? _normals.ToArray() : null; } }
    /// <summary>
    /// Access the point colours if present (read only).
    /// </summary>
    /// <remarks>
    /// Null if unavailable.
    /// </remarks>
    public uint[] PointColours { get { return (_colours != null) ? _colours.ToArray() : null; } }

    /// <summary>
    /// Overridden to access to the vertex count.
    /// </summary>
    /// <param name="stream">Unused</param>
    /// <returns>The point count.</returns>
    public override uint VertexCount(int stream = 0) { return PointCount; }
    /// <summary>
    /// Override.
    /// </summary>
    /// <param name="stream">Not used</param>
    /// <returns>0</returns>
    public override uint IndexCount(int stream = 0) { return 0u; }
    /// <summary>
    /// Exposes the points as vertices.
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>The point vertex array.</returns>
    public override Vector3[] Vertices(int stream = 0) { return Points; }
    /// <summary>
    /// override
    /// </summary>
    /// <param name="stream">Not used.</param>
    /// <returns>null</returns>
    public override ushort[] Indices2(int stream = 0) { return null; }
    /// <summary>
    /// Override
    /// </summary>
    /// <param name="stream">Not used.</param>
    /// <returns>null</returns>
    public override int[] Indices4(int stream = 0) { return null; }
    /// <summary>
    /// Override
    /// </summary>
    /// <param name="stream">Ignored</param>
    /// <returns>The point normals.</returns>
    public override Vector3[] Normals(int stream = 0) { return PointNormals; }
    /// <summary>
    /// Override
    /// </summary>
    /// <param name="stream">Not used.</param>
    /// <returns>null</returns>
    public override Vector2[] UVs(int stream = 0) { return null; }
    /// <summary>
    /// Override
    /// </summary>
    /// <param name="stream">Not used.</param>
    /// <returns>The point colours.</returns>
    public override uint[] Colours(int stream = 0) { return PointColours; }

    /// <summary>
    /// Adds a point to the cloud.
    /// </summary>
    /// <param name="pos">The point position.</param>
    public void AddPoint(Vector3 pos)
    {
      Components |= MeshComponentFlag.Vertex;
      _points.Add(pos);
    }

    /// <summary>
    /// Add a set of points to the cloud.
    /// </summary>
    /// <param name="positions">The points to add.</param>
    public void AddPoints(IEnumerable<Vector3> positions)
    {
      Components |= MeshComponentFlag.Vertex;
      _points.AddRange(positions);
    }

    /// <summary>
    /// Add a point normal to the cloud.
    /// </summary>
    /// <param name="normal">The point normals</param>
    public void AddNormal(Vector3 normal)
    {
      Components |= MeshComponentFlag.Normal;
      if (_normals == null)
      {
        _normals = new List<Vector3>(_points.Count);
      }
      _normals.Add(normal);
    }

    /// <summary>
    /// Adds a set of point normals.
    /// </summary>
    /// <param name="normals">The point normals to add.</param>
    public void AddNormals(IEnumerable<Vector3> normals)
    {
      Components |= MeshComponentFlag.Normal;
      if (_normals == null)
      {
        _normals = new List<Vector3>(_points.Count);
      }
      _normals.AddRange(normals);
    }

    /// <summary>
    /// Add a point colour.
    /// </summary>
    /// <param name="colour">The colour value.</param>
    public void AddColour(uint colour)
    {
      Components |= MeshComponentFlag.Colour;
      if (_colours == null)
      {
        _colours = new List<uint>(_points.Count);
      }
      _colours.Add(colour);
    }

    /// <summary>
    /// Add a set of point colours.
    /// </summary>
    /// <param name="colours">The colour values.</param>
    public void AddColours(IEnumerable<uint> colours)
    {
      Components |= MeshComponentFlag.Colour;
      if (_colours == null)
      {
        _colours = new List<uint>(_points.Count);
      }
      _colours.AddRange(colours);
    }

    /// <summary>
    /// Point vertices.
    /// </summary>
    private List<Vector3> _points = null;
    /// <summary>
    /// Point normals.
    /// </summary>
    private List<Vector3> _normals = null;
    /// <summary>
    /// Point colours.
    /// </summary>
    private List<uint> _colours = null;
  }
}

