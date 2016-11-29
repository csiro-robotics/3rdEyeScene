using System;

namespace Tes.Maths
{
  /// <summary>
  /// Rotation conversion methods.
  /// </summary>
  /// <remarks>
  /// These should be extension methods, however, using Mono 2.0 (for Unity compatibility) doesn't support this.
  /// </remarks>
  public static class Rotation
  {
    /// <summary>
    /// Convert a 3x3 rotation matrix to the equivalent quaternion rotation. Scaling is lost.
    /// </summary>
    /// <returns>The equivalent quaternion rotation.</returns>
    /// <param name="m">The rotation matrix.</param>
    public static Quaternion ToQuaternion(this Matrix3 m)
    {
      // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
      // article "CSLQuaternion Calculus and Fast Animation".
      Quaternion q = new Quaternion();
      float trace = m[0, 0] + m[1, 1] + m[2, 2];
      float root;
      int[] next = new int[] { 1, 2, 0 };
      int i = 0, j, k;

      if (trace >= 0.0f)
      {
        // |w| > 1/2, may as well choose w > 1/2
        root = (float)Math.Sqrt(trace + 1.0f);  // 2w
        q.W = 0.5f * root;
        root = 0.5f / root;  // 1/(4w)
        q.X = (m[2, 1] - m[1, 2]) * root;
        q.Y = (m[0, 2] - m[2, 0]) * root;
        q.Z = (m[1, 0] - m[0, 1]) * root;
      }
      else
      {
        // |w| <= 1/2
        if (m[1, 1] > m[0, 0])
        {
          i = 1;
        }
        if (m[2, 2] > m[i, i])
        {
          i = 2;
        }
        j = next[i];
        k = next[j];

        root = (float)Math.Sqrt(m[i, i] - m[j, j] - m[k, k] + 1.0f);
        q[i] = 0.5f * root;
        root = 0.5f / root;
        q.W = (m[k, j] - m[j, k]) * root;
        q[j] = (m[j, i] + m[i, j]) * root;
        q[k] = (m[k, i] + m[i, k]) * root;
      }

      q.Normalise();
      return q;
    }

    /// <summary>
    /// Convert the rotation component of a 4x4 transformation matrix to the equivalent quaternion rotation.
    /// Scaling and translation are lost.
    /// </summary>
    /// <returns>The equivalent quaternion rotation.</returns>
    /// <param name="m">The transformation matrix.</param>
    public static Quaternion ToQuaternion(this Matrix4 m)
    {
      if (Math.Abs(m.Scale.X - 1.0f) > 1e3 ||
          Math.Abs(m.Scale.Y - 1.0f) > 1e3 ||
          Math.Abs(m.Scale.Z - 1.0f) > 1e3)
      {
        // Scale is "significant". Remove scale.
        m.RemoveScale();
      }

      // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
      // article "CSLQuaternion Calculus and Fast Animation".
      Quaternion q = new Quaternion();
      float trace = m[0, 0] + m[1, 1] + m[2, 2];
      float root;
      int[] next = new int[] { 1, 2, 0 };
      int i = 0, j, k;

      if (trace >= 0.0f)
      {
        // |w| > 1/2, may as well choose w > 1/2
        root = (float)Math.Sqrt(trace + 1.0f);  // 2w
        q.W = 0.5f * root;
        root = 0.5f / root;  // 1/(4w)
        q.X = (m[2, 1] - m[1, 2]) * root;
        q.Y = (m[0, 2] - m[2, 0]) * root;
        q.Z = (m[1, 0] - m[0, 1]) * root;
      }
      else
      {
        // |w| <= 1/2
        if (m[1, 1] > m[0, 0])
        {
          i = 1;
        }
        if (m[2, 2] > m[i, i])
        {
          i = 2;
        }
        j = next[i];
        k = next[j];

        root = (float)Math.Sqrt(m[i, i] - m[j, j] - m[k, k] + 1.0f);
        q[i] = 0.5f * root;
        root = 0.5f / root;
        q.W = (m[k, j] - m[j, k]) * root;
        q[j] = (m[j, i] + m[i, j]) * root;
        q[k] = (m[k, i] + m[i, k]) * root;
      }

      q.Normalise();
      return q;
    }

    /// <summary>
    /// Convert a quaterion rotation to the equivalent 3x3 rotation matrix.
    /// </summary>
    /// <returns>The equivalent 3x3 rotation matrix.</returns>
    /// <param name="q">The rotation quaternion.</param>
    public static Matrix3 ToMatrix3(Quaternion q)
    {
      Matrix3 m = new Matrix3();
      float tx = q.X + q.X;
      float ty = q.Y + q.Y;
      float tz = q.Z + q.Z;
      float twx = tx * q.W;
      float twy = ty * q.W;
      float twz = tz * q.W;
      float txx = tx * q.X;
      float txy = ty * q.X;
      float txz = tz * q.X;
      float tyy = ty * q.Y;
      float tyz = tz * q.Y;
      float tzz = tz * q.Z;

      m[0, 0] = 1.0f - (tyy + tzz);
      m[0, 1] = txy - twz;
      m[0, 2] = txz + twy;
      m[1, 0] = txy + twz;
      m[1, 1] = 1.0f - (txx + tzz);
      m[1, 2] = tyz - twx;
      m[2, 0] = txz - twy;
      m[2, 1] = tyz + twx;
      m[2, 2] = 1.0f - (txx + tyy);

      return m;
    }

    /// <summary>
    /// Convert a quaternion rotation to the equivalent 4x4 rotation matrix (no scaling or translation).
    /// </summary>
    /// <returns>The equivalent 4x4 transformation matrix.</returns>
    /// <param name="q">The rotation quaternion.</param>
    public static Matrix4 ToMatrix4(this Quaternion q)
    {
      Matrix4 m = Matrix4.Identity;
      float tx = q.X + q.X;
      float ty = q.Y + q.Y;
      float tz = q.Z + q.Z;
      float twx = tx * q.W;
      float twy = ty * q.W;
      float twz = tz * q.W;
      float txx = tx * q.X;
      float txy = ty * q.X;
      float txz = tz * q.X;
      float tyy = ty * q.Y;
      float tyz = tz * q.Y;
      float tzz = tz * q.Z;

      m[0, 0] = 1.0f - (tyy + tzz);
      m[0, 1] = txy - twz;
      m[0, 2] = txz + twy;
      m[1, 0] = txy + twz;
      m[1, 1] = 1.0f - (txx + tzz);
      m[1, 2] = tyz - twx;
      m[2, 0] = txz - twy;
      m[2, 1] = tyz + twx;
      m[2, 2] = 1.0f - (txx + tyy);

      return m;
    }
  }
}

