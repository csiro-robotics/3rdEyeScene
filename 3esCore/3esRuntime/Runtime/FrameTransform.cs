using Tes.Net;
using UnityEngine;

namespace Tes.Runtime
{
  /// <summary>
  /// A utility class for transforming between a selected <see cref="CoordinateFrame"/> and the Unity coordinate frame.
  /// </summary>
  /// <remarks>
  /// The primary purpose of this object is to manage conversions from the server's
  /// <see cref="CoordinateFrame"/> to the Unity frame.
  /// </remarks>
  public static class FrameTransform
  {
    /// <summary>
    /// Set <paramref name="transform"/> to reflect a rotation from <paramref name="frame"/> to
    /// the Unity coordinate frame.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="frame">The source <see cref="CoordinateFrame"/>.</param>
    public static void SetFrameRotation(Transform transform, CoordinateFrame frame)
    {
      Matrix4x4 m = ToUnityTransforms[(int)frame];
      //Vector3 side = new Vector3();
			Vector3 up = new Vector3();
			Vector3 fwd = new Vector3();
      Vector3 scale = Vector3.one;

      SetFrameRotation(ref m, frame);
      //side = m.GetColumn(0);
			up = m.GetColumn(1);
			fwd = m.GetColumn(2);

      if ((int)frame < (int)CoordinateFrame.LeftHanded)
      {
        scale.x = -1.0f;
      }
      transform.localRotation = Quaternion.LookRotation(fwd, up);
      transform.localScale = scale;
    }


    /// <summary>
    /// Convert a vector from the specified coordinate frame to the Unity frame.
    /// </summary>
    /// <param name="v">The vector to convert</param>
    /// <param name="frame">The target coordinate frame.</param>
    /// <returns>The converted vector.</returns>
    /// <remarks>
    /// The Unity frame is <see cref="CoordinateFrame.XZY"/>.
    /// </remarks>
    public static Vector3 RemoteToUnity(Vector3 v, CoordinateFrame frame)
    {
      // MultiplyPoint() or MultiplyVector() is irrelevant. We only have rotation.
			return ToUnityTransforms[(int)frame].MultiplyPoint(v);
    }


    /// <summary>
    /// Convert a vector from the Unity frame to the specified coordinate fame.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <param name="frame">The source coordinate frame.</param>
    /// <returns>The converted vector.</returns>
    /// <remarks>
    /// The Unity frame is <see cref="CoordinateFrame.XZY"/>.
    /// </remarks>
    public static Vector3 UnityToRemote(Vector3 v, CoordinateFrame frame)
    {
      return ToUnityTransforms[(int)frame].inverse.MultiplyPoint(v);
    }

    /// <summary>
    /// Set <paramref name="transform"/> to reflect a rotation from <paramref name="frame"/> to
    /// the Unity coordinate frame.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="frame">The source <see cref="CoordinateFrame"/>.</param>
    public static void SetFrameRotation(ref Matrix4x4 transform, CoordinateFrame frame)
    {
      transform = ToUnityTransforms[(int)frame];
    }

    /// <summary>
    /// Set <paramref name="transform"/> to reflect a rotation to <paramref name="frame"/> from
    /// the Unity coordinate frame.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="frame">The target <see cref="CoordinateFrame"/>.</param>
    public static void SetFrameRotationInverse(ref Matrix4x4 transform, CoordinateFrame frame)
    {
      SetFrameRotation(ref transform, frame);
      transform = transform.inverse;
    }

    /// <summary>
    /// Returns true if the given matrix represents a transformation from left to right or right to left handedness.
    /// </summary>
    /// <returns><c>true</c> when <paramref name="transform"/> switches handedness.</returns>
		/// <param name="transform">The rigid body transformation matrix to check.</param>
    /// <remarks>
    /// This only works for matrices representing a set of orthogonal basis vectors.
    /// </remarks>
    public static bool EffectsHandChange(Matrix4x4 transform)
    {
			Vector3 x, y, z, xy;
      float dot;
			x = transform.GetColumn(0);
			y = transform.GetColumn(1);
			z = transform.GetColumn(2);
			xy = Vector3.Cross(x, y);
      // We are transforming from a right to left or left to right handed system when the
      // direction of the X/Y cross does not match the Z vector.
			dot = Vector3.Dot(xy, z);
      return dot < 0;
		}

    /// <summary>
    /// Builds a rotation matrix.
    /// </summary>
    /// <returns>The rotation matrix.</returns>
    /// <param name="r0">Row index zero.</param>
    /// <param name="r1">Row index one.</param>
    /// <param name="r2">Row index two.</param>
    private static Matrix4x4 BuildRotation(Vector3 r0, Vector3 r1, Vector3 r2)
    {
      Matrix4x4 m = new Matrix4x4();
			m.SetRow(0, new Vector4(r0.x, r0.y, r0.z, 0));
			m.SetRow(1, new Vector4(r1.x, r1.y, r1.z, 0));
			m.SetRow(2, new Vector4(r2.x, r2.y, r2.z, 0));
      m.SetRow(3, new Vector4(0, 0, 0, 1));
      return m;
    }


    /// <summary>
    /// Transformation matrices from various <see cref="T:CoordinateFrame" /> values.
    /// </summary>
    private static Matrix4x4[] ToUnityTransforms =
    {
      // CoordinateFrame.XYZ:
      // Right handed
      BuildRotation(new Vector3(1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0)),
      // CoordinateFrame.XZ_Y:
      // Right handed
      BuildRotation(new Vector3(1, 0, 0),
                    new Vector3(0, -1, 0),
                    new Vector3(0, 0, 1)),
      // CoordinateFrame.YX_Z:
      // Right handed
      BuildRotation(new Vector3(0, 1, 0),
                    new Vector3(0, 0, -1),
                    new Vector3(1, 0, 0)),
      // CoordinateFrame.YZX:
      // Right handed
      BuildRotation(new Vector3(0, 1, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 0, 1)),
      // CoordinateFrame.ZXY:
      // Right handed
      BuildRotation(new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 0, 0)),
      // CoordinateFrame.ZY_X:
      // Right handed
      BuildRotation(new Vector3(0, 0, 1),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 1, 0)),
      // CoordinateFrame.XY_Z:
      // Left handed
      BuildRotation(new Vector3(1, 0, 0),
                    new Vector3(0, 0, -1),
                    new Vector3(0, 1, 0)),
      // CoordinateFrame.XZY:
      // Left handed
      Matrix4x4.identity,
      // CoordinateFrame.YXZ:
      // Left handed
      BuildRotation(new Vector3(0, 1, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 0)),
      // CoordinateFrame.YZ_X:
      // Left handed
      BuildRotation(new Vector3(0, 1, 0),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 0, 1)),
      // CoordinateFrame.ZX_Y:
      // Left handed
      BuildRotation(new Vector3(0, 0, 1),
                    new Vector3(0, -1, 0),
                    new Vector3(1, 0, 0)),
      // CoordinateFrame.ZYX:
      // Left handed
      BuildRotation(new Vector3(0, 0, 1),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0))
    };
  }
}
