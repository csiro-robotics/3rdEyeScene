using UnityEngine;

namespace Tes.Tessellate
{
  /// <summary>
  /// A hashable vector for vertices on a sphere.
  /// </summary>
  /// <remarks>
  /// Not a very good hash.
  /// </remarks>
  public struct SphereVector3Hash
  {
    /// <summary>
    /// The vector being hashed.
    /// </summary>
    public Vector3 Value;
    
    /// <summary>
    /// Create a new hashed vector.
    /// </summary>
    /// <param name="value">The vertex coordinate. Exected to be unit length.</param>
    public SphereVector3Hash(Vector3 value)
    {
      Value = value;
    }

    /// <summary>
    /// Generate the hash code.
    /// </summary>
    /// <returns>The hash code for <see cref="Value"/>.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        const float quant = (float)0xffffu;
        // Decompose the vector into two rotation angles.
        float alpha = Mathf.Sin(Value.z);
        float beta = Mathf.Cos(Value.x);

        // Convert angles to quantised integers (32-bit).
        int alphaKey = (ushort)(alpha / (2.0f * Mathf.PI) * quant); 
        int betaKey = (ushort)(beta / (2.0f * Mathf.PI) * quant); 
        return (alphaKey) | ((betaKey) << 16);
      }
    }
  }
}
