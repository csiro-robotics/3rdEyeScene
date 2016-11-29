
namespace Tes.Maths
{
  /// <summary>
  /// Unity/TES quaternion conversion.
  /// </summary>
  public static class QuaternionExt
  {
    /// <summary>
    /// Convert a TES quaternion to a Unity quaternion.
    /// </summary>
    /// <returns>The equivalent Unity quaternion.</returns>
    /// <param name="q">The quaternion to convert.</param>
    public static UnityEngine.Quaternion ToUnity(this Quaternion q)
    {
      return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
    }

    /// <summary>
    /// Convert an Unity quaternion to a TES quaternion.
    /// </summary>
    /// <returns>The TES quaternion equivalent.</returns>
    /// <param name="uq">The Unity quaternion to convert.</param>
    public static Quaternion FromUnity(UnityEngine.Quaternion uq)
    {
      return new Quaternion(uq.x, uq.y, uq.z, uq.w);
    }
  }
}
