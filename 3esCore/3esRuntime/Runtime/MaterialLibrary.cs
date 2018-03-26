using System.Collections.Generic;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// The <see cref="MaterialLibrary"/> provides a way to register and
  /// access Unity materials by name.
  /// </summary>
  public class MaterialLibrary
  {
    /// <summary>
    /// The name of a default material, supporting per vertex colour and lighting.
    /// </summary>
    public static string VertexColourLit { get { return "vertexLit"; } }
    /// <summary>
    /// The name of a default material, supporting per vertex colour with no lighting.
    /// </summary>
    public static string VertexColourUnlit { get { return "vertexUnlit"; } }
    /// <summary>
    /// The name of a default material, supporting per vertex colour and lighting with culling disabled.
    /// </summary>
    public static string VertexColourLitTwoSided { get { return "vertexLitTwoSided"; } }
    /// <summary>
    /// The name of a default material, supporting per vertex colour with no lighting and culling disabled.
    /// </summary>
    public static string VertexColourUnlitTwoSided { get { return "vertexUnlitTwoSided"; } }
    /// <summary>
    /// The name of a default wireframe triangle rendering material.
    /// </summary>
    public static string WireframeTriangles { get { return "wireframe"; } }
    /// <summary>
    /// The name of a default material, supporting per vertex colour with no lighting.
    /// </summary>
    public static string VertexColourTransparent { get { return "vertexTransparent"; } }
    /// <summary>
    /// The name of a default material for rendering unlit points. per vertex colour with no lighting.
    /// </summary>
    public static string PointsLit { get { return "pointsLit"; } }
    /// <summary>
    /// The name of a default material for rendering unlit points.
    /// </summary>
    public static string PointsUnlit { get { return "pointsUnlit"; } }

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
    /// The material map.
    /// </summary>
    protected Dictionary<string, Material> _map = new Dictionary<string, Material>();
  }
}
