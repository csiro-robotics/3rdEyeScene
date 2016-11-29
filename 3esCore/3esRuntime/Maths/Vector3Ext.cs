
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
  }
}
