using System;
using UnityEngine;

namespace Tes.Handlers.Shape3D
{
  /// <summary>
  /// Used to track details of objects from <see cref="PointCloudHandler"/>.
  /// </summary>
  public class PointsComponent
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

    public Material Material { get; set; }

    private Color32 _colour = Colour32.white;
    public Color32 Colour { get { return _colour; } set { _colour = value; } }

    ~PointsComponent()
    {
      Release();
    }

    public void Release()
    {
      if (_indexBuffer != null)
      {
        _indexBuffer.Release();
        _indexBuffer = null;
      }
    }

    /// <summary>
    /// The <see cref="MeshCache"/> resource ID from which to attain vertex data.
    /// </summary>
    public uint MeshID { get { return _meshID; } set { _meshID = value; } }
    [SerializeField]
    private uint _meshID;
    /// <summary>
    /// The mesh component once resolved from the mesh cache.
    /// </summary>
    /// <value></value>
    public MeshCache. MeshEntry Mesh { get; set; }
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
        int oldCount = (_indices != null) ? _indices.Length : 0;
        if (value != oldCount)
        {
          int[] oldIndices = _indices;
          _indices = new int[value];
          if (oldIndices != null && (oldCount > 0 || value > 0))
          {
            Array.Copy(oldIndices, _indices, _indices.Length);
          }

          if (_indexBuffer != null)
          {
            _indexBuffer.Release();
            _indexBuffer = null;
          }
          IndicesDirty = true;

          // if (value > 0)
          // {
          //   _indexBuffer = new ComputeBuffer(value, Marshal.Sizeof(typeof(int)));
          // }
        }
      }
    }

    #region First pass changes.
    public bool IndicesDirty { get; set; }
    public void UpdateIndexBuffer()
    {
      if (_indexBuffer == null)
      {
        _indexBuffer = new ComputeBuffer(value, Marshal.Sizeof(typeof(int)));
      }
      _indexBuffer.SetData(_indices);
      IndicesDirty = false;
    }
    #endregion

    /// <summary>
    /// Window of point indices limiting rendered points from the source mesh (optional).
    /// </summary>
    /// <remarks>
    /// May be null, in which case all vertices are used.
    /// </remarks>
    public int[] Indices { get { return _indices; } }
    public ComputeBuffer IndexBuffer { get { return _indexBuffer; } }
    /// <summary>
    /// Point render size override. Zero to use the default.
    /// </summary>
    public int PointSize { get { return _pointSize; } set { _pointSize = value; } }

    /// <summary>
    /// True if the mesh is dirty and needs updating.
    /// </summary>
    public bool MeshDirty { get; set; }

    [SerializeField]
    private int _pointSize;

    private int[] _indices = null;
    private ComputeBuffer _indexBuffer = null;
  }
}
