using Tes.Maths;

namespace Tes.Maths
{
  /// <summary>
  /// Unity/TES matrix conversion.
  /// </summary>
  public static class Matrix3Ext
  {
    /// <summary>
    /// Convert a TES matrix to a Unity matrix.
    /// </summary>
    /// <returns>The equivalent Unity matrix.</returns>
    /// <param name="mat">The matrix to convert.</param>
    public static UnityEngine.Matrix4x4 ToUnity(this Matrix4 mat)
    {
      UnityEngine.Matrix4x4 umat = UnityEngine.Matrix4x4.identity;
      for (int i = 0; i < 3; ++i)
      {
        umat.SetColumn(i, mat.GetAxis(i).ToUnity(0.0f));
      }
      return umat;
    }

    /// <summary>
    /// Convert an Unity matrix to a TES matrix.
    /// </summary>
    /// <remarks>
    /// Translation of <paramref name="umat"/> is dropped.
    /// </remarks>
    /// <returns>The TES matrix equivalent.</returns>
    /// <param name="umat">The Unity matrix to convert.</param>
    public static Matrix3 FromUnity(UnityEngine.Matrix4x4 umat)
    {
      Matrix3 mat = new Matrix3();
      Vector3 v = new Vector3();
      for (int i = 0; i < 3; ++i)
      {
        v = Vector3Ext.FromUnity(umat.GetColumn(i));
        mat.SetAxis(i, v);
      }
      return mat;
    }
  }
}
