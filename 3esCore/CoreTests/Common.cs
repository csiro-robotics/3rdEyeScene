//
// author Kazys Stepanas
//
using System.Collections.Generic;
using Tes.Maths;
using Tes.TestSupport;

namespace Tes.CoreTests
{
  public static class Common
  {
    public static void MakeHiResSphere(List<Vector3> vertices, List<int> indices, List<Vector3> normals)
    {
      MakeSphere(vertices, indices, normals, 5);
    }

  
    public static void MakeLowResSphere(List<Vector3> vertices, List<int> indices, List<Vector3> normals)
    {
      MakeSphere(vertices, indices, normals, 0);
    }

  
    public static void MakeSphere(List<Vector3> vertices, List<int> indices, List<Vector3> normals, int depth)
    {
      // Start with a unit sphere so we have normals precalculated.
      // Use a fine subdivision to ensure we need multiple data packets to transfer vertices.
      SphereTessellator.SphereSubdivision(vertices, indices, 1.0f, Vector3.Zero, depth);

      // Normals as vertices. Scale and offset.
      if (normals != null)
      {
        normals.Clear();
        for (int i = 0; i < vertices.Count; ++i)
        {
          normals.Add(vertices[i]);
        }
      }

      const float radius = 5.5f;
      Vector3 sphereCentre = new Vector3(0.5f, 0, -0.25f);
      for (int i = 0; i < vertices.Count; ++i)
      {
        vertices[i] = sphereCentre + vertices[i] * radius;
      }
    }
}
}
