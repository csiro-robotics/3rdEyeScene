
namespace Tes.Maths
{
  /// <summary>
  /// Unity/TES Vector3 conversion.
  /// </summary>
  public static class Vector3Ext
  {
    /// <summary>
    /// Convert a TES vector to a Unity vector.
    /// </summary>
    /// <returns>The equivalent Unity vector.</returns>
    /// <param name="v">The vector to convert.</param>
    public static UnityEngine.Vector3 ToUnity(this Vector3 v)
    {
      return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// Convert a TES vector3 to a Unity vector4.
    /// </summary>
    /// <returns>The equivalent Unity vector.</returns>
    /// <param name="v">The vector to convert.</param>
    /// <param name="w">The W component for the returned vector.</param>
    public static UnityEngine.Vector4 ToUnity(this Vector3 v, float w)
    {
      return new UnityEngine.Vector4(v.X, v.Y, v.Z, w);
    }

    /// <summary>
    /// Convert an array of TES vectors to unity vectors.
    /// </summary>
    /// <param name="vecs">The array of vectors to convert.</param>
    /// <returns>The converted array.</returns>
    public static UnityEngine.Vector3[] ToUnity(Vector3[] vecs)
    {
      UnityEngine.Vector3[] uvecs = new UnityEngine.Vector3[vecs.Length];
      for (int i = 0; i < vecs.Length; ++i)
      {
        uvecs[i] = ToUnity(vecs[i]);
      }
      return uvecs;
    }

    /// <summary>
    /// Convert an Unity vector to a TES vector3.
    /// </summary>
    /// <returns>The TES vector equivalent.</returns>
    /// <param name="uv">The Unity vector to convert.</param>
    public static Vector3 FromUnity(UnityEngine.Vector3 uv)
    {
      return new Vector3(uv.x, uv.y, uv.z);
    }

    /// <summary>
    /// Convert an Unity vector to a TES vector3.
    /// </summary>
    /// <remarks>
    /// The W component is dropped.
    /// </remarks>
    /// <returns>The TES vector equivalent.</returns>
    /// <param name="uv">The Unity vector to convert.</param>
    public static Vector3 FromUnity(UnityEngine.Vector4 uv)
    {
      return new Vector3(uv.x, uv.y, uv.z);
    }

    /// <summary>
    /// Convert an array of unity vectors to TES vectors.
    /// </summary>
    /// <param name="uvecs">The array of vectors to convert.</param>
    /// <returns>The converted array.</returns>
    public static Vector3[] FromUnity(UnityEngine.Vector3[] uvecs)
    {
      if (uvecs != null)
      {
        Vector3[] tesVectors = new Vector3[uvecs.Length];
        for (int i = 0; i < uvecs.Length; ++i)
        {
          tesVectors[i] = FromUnity(uvecs[i]);
        }
        return tesVectors;
      }
      return null;
    }
  }
}
