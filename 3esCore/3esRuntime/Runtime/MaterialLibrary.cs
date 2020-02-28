using System.Collections.Generic;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// The <see cref="MaterialLibrary"/> provides a way to register and
  /// access Unity materials by name.
  /// </summary>
  /// <remarks>
  /// Material requirements:
  /// - Must support rendering from a compute buffer containing float3 vertices.
  /// - Keyword blocks (preprocessor style blocks):
  ///   - WITH_COLOURS_UINT to support a per vertex uint32 colour (RGBA with high byte R, low byte A) stream.
  ///   - WITH_NORMALS to support a per vertex float3 normal stream. This enables lighting.
  ///   - WITH_UVS to support a per vertex float2 UV stream. This feature is not used yet.
  /// - Material properties:
  ///   - [_Color] global colour to apply
  ///   - [_Tint] additional global colour tint to apply. Multiplied by _Color.
  ///   - [_BackColor] global back face colour if rendering double sided.
  ///   - [_PointSize] point size for point cloud materials.
  ///   - [_PointHighlighting] point highlight factor.
  ///   - [_LeftHanded] 0/1 marks whether the server coordinate frame is left handled (1) or right handled (0).
  /// <remarks>
  public class MaterialLibrary
  {
    public static string OpaqueInstanced { get { return "OpaqueInstanced"; } }
    public static string WireframeInstanced { get { return "WireframeInstanced"; } }
    public static string TransparentInstanced { get { return "TransparentInstanced"; } }
    public static string OpaqueMesh { get { return "OpaqueMesh"; } }
    public static string OpaqueTwoSidedMesh { get { return "OpaqueTwoSidedMesh"; } }
    public static string TransparentMesh { get { return "TransparentMesh"; } }
    public static string WireframeMesh { get { return "WireframeMesh"; } }
    public static string Points { get { return "Points"; } }

    /// <summary>
    /// The name of a default material for rendering geometry shader based voxels.
    /// </summary>
    public static string Voxels { get { return "voxels"; } }

    /// <summary>
    /// Default pixel size used to render points.
    /// </summary>
    public int DefaultPointSize { get; set; }

    /// <summary>
    /// Fetch or register a material under <paramref name="key"/>. Will replace
    /// an existing material under <paramref name="key"/>
    /// </summary>
    /// <param name="key">The material name/key.</param>
    /// <value>A Unity material object.</value>
    public Material this [string key]
    {
      get
      {
        Material mat;
        if (_map.TryGetValue(key, out mat))
        {
          return mat;
        }
        return null;
      }

      set { _map[key] = value; }
    }

    /// <summary>
    /// Checks if the library has a material registered under the given <paramref name="key"/>
    /// </summary>
    /// <param name="key">The material key to check for.</param>
    public bool Contains(string key)
    {
      return _map.ContainsKey(key);
    }

    /// <summary>
    /// Register a material under <paramref name="key"/>. Replaces any existing material
    /// under that key.
    /// </summary>
    /// <param name="key">The key to register under.</param>
    /// <param name="material">A Unity material to register.</param>
    public void Register(string key, Material material)
    {
      if (material != null)
      {
        _map[key] = material;
      }
      else if (_map.ContainsKey(key))
      {
        _map.Remove(key);
      }
    }

    /// <summary>
    /// Setup <paramref name="material"/> to render the given <paramref name="mesh"/>.
    /// </summary>
    /// <param name="material"></param>
    /// <param name="mesh"></param>
    public static void SetupMaterial(Material material, RenderMesh mesh)
    {
      if (mesh.HasNormals)
      {
        material.EnableKeyword("WITH_NORMALS");
      }

      // if (mesh.HasUVs)
      // {
      //   material.EnableKeyword("WITH_UVS");
      // }

      if (mesh.HasColours)
      {
        material.EnableKeyword("WITH_COLOURS_UINT");
      }
    }

    /// <summary>
    /// The material map.
    /// </summary>
    protected Dictionary<string, Material> _map = new Dictionary<string, Material>();
  }
}
