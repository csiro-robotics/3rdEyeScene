using System;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Used to track details of objects from <see cref="PointCloudHandler"/>.
  /// </summary>
  public class PointsComponent : MonoBehaviour
  {
    /// <summary>
    /// Point attributes.
    /// </summary>
    [Flags]
    public enum PointAttributes : ushort
    {
      /// <summary>
      /// None
      /// </summary>
      None = 0,
      /// <summary>
      /// Per vertex normals.
      /// </summary>
      Normals = (1 << 0),
      /// <summary>
      /// Per vertex colours.
      /// </summary>
      Colours = (1 << 1)
    }

    /// <summary>
    /// The <see cref="MeshCache"/> resource ID from which to attain vertex data.
    /// </summary>
    public uint MeshID { get { return _meshID; } set { _meshID = value; } }
    [SerializeField]
    private uint _meshID;
    /// <summary>
    /// Number of indices used as a window into the mesh.
    /// </summary>
    /// <remarks>
    /// May be zero indicating the all mesh vertices are to be rendered.
    /// </remarks>
    public uint IndexCount
    {
      get { return (_indices != null) ? (uint)_indices.Length : 0u; }
      set
      {
        int[] oldIndices = _indices;
        _indices = new int[value];
        if (oldIndices != null)
        {
          Array.Copy(oldIndices, _indices, _indices.Length);
        }
      }
    }
    /// <summary>
    /// Window of point indices limiting rendered points from the source mesh (optional).
    /// </summary>
    /// <remarks>
    /// May be null, in which case all vertices are used.
    /// </remarks>
    public int[] Indices { get { return _indices; } }
    /// <summary>
    /// Point render size override. Zero to use the default.
    /// </summary>
    public int PointSize { get { return _pointSize; } set { _pointSize = value; } }
    [SerializeField]
    private int _pointSize;

    private int[] _indices = null;
  }
}
