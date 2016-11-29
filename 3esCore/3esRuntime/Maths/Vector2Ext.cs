
namespace Tes.Maths
{
  /// <summary>
  /// Unity/TES Vector2 conversion.
  /// </summary>
  public static class Vector2Ext
  {
    /// <summary>
    /// Convert a TES vector to a Unity vector.
    /// </summary>
    /// <returns>The equivalent Unity vector.</returns>
    /// <param name="v">The vector to convert.</param>
    public static UnityEngine.Vector2 ToUnity(this Vector2 v)
    {
      return new UnityEngine.Vector2(v.X, v.Y);
    }

    /// <summary>
    /// Convert a TES vector2 to a Unity vector3.
    /// </summary>
    /// <returns>The equivalent Unity vector.</returns>
    /// <param name="v">The vector to convert.</param>
    /// <param name="z">The Z component for the returned vector.</param>
    public static UnityEngine.Vector3 ToUnity(this Vector2 v, float z)
    {
      return new UnityEngine.Vector3(v.X, v.Y, z);
    }

    /// <summary>
    /// Convert an Unity vector to a TES vector2.
    /// </summary>
    /// <param name="v">The TES vector to modify.</param>
    /// <param name="uv">The Unity vector to convert.</param>
    public static void Set(this Vector2 v, UnityEngine.Vector2 uv)
    {
      v.X = uv.x;
      v.Y = uv.y;
    }

    /// <summary>
    /// Convert an Unity vector3 to a TES vector2.
    /// </summary>
    /// <remarks>
    /// The Z component is dropped.
    /// </remarks>
    /// <returns>The TES vector equivalent.</returns>
    /// <param name="uv">The Unity vector to convert.</param>
    public static Vector2 FromUnity(UnityEngine.Vector3 uv)
    {
      return new Vector2(uv.x, uv.y);
    }
  }
}
