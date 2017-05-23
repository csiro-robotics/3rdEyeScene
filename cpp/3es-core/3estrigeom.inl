//
// author: Kazys Stepanas
//
#include "3estrigeom.h"

#include "3esplanegeom.h"

#include <algorithm>
#ifndef _USE_MATH_DEFINES
#define _USE_MATH_DEFINES
#endif // _USE_MATH_DEFINES
#include <cmath>

template <typename real>
real clamp(real val, real min, real max)
{
  return (val < min) ? min : ((val > max) ? max : val);
}

namespace tes
{
  namespace trigeom
  {
    namespace detail
    {
      template <typename T>
      inline T abs(T a)
      {
        return a >= 0 ? a : -a;
      }


      template <typename T>
      inline bool aabbAxisTestX01(T a, T b, T fa, T fb,
                                  const Vector3<T> &v0, const Vector3<T> &v2,
                                  const Vector3<T> &halfExtents)
      {
        const T p0 = a * v0.y - b * v0.z;
        const T p2 = a * v2.y - b * v2.z;
        const T minval = (p0 < p2) ? p0 : p2;
        const T maxval = (p0 < p2) ? p2 : p0;
        const T rad = fa * halfExtents.y + fb * halfExtents.z;
        return !(minval > rad || maxval < -rad);
      }


      template <typename T>
      inline bool aabbAxisTestX2(T a, T b, T fa, T fb,
                                 const Vector3<T> &v0, const Vector3<T> &v1,
                                 const Vector3<T> &halfExtents)
      {
        const T p0 = a * v0.y - b * v0.z;
        const T p1 = a * v1.y - b * v1.z;
        const T minval = (p0 < p1) ? p0 : p1;
        const T maxval = (p0 < p1) ? p1 : p0;
        const T rad = fa * halfExtents.y + fb * halfExtents.z;
        return !(minval > rad || maxval < -rad);
      }


      template <typename T>
      inline bool aabbAxisTestY02(T a, T b, T fa, T fb,
                                  const Vector3<T> &v0, const Vector3<T> &v2,
                                  const Vector3<T> &halfExtents)
      {
        const T p0 = -a * v0.x + b * v0.z;
        const T p2 = -a * v2.x + b * v2.z;
        const T minval = (p0 < p2) ? p0 : p2;
        const T maxval = (p0 < p2) ? p2 : p0;
        const T rad = fa * halfExtents.x + fb * halfExtents.z;
        return !(minval > rad || maxval < -rad);
      }


      template <typename T>
      inline bool aabbAxisTestY1(T a, T b, T fa, T fb,
                                 const Vector3<T> &v0, const Vector3<T> &v1,
                                 const Vector3<T> &halfExtents)
      {
        const T p0 = -a * v0.x + b * v0.z;
        const T p1 = -a * v1.x + b * v1.z;
        const T minval = (p0 < p1) ? p0 : p1;
        const T maxval = (p0 < p1) ? p1 : p0;
        const T rad = fa * halfExtents.x + fb * halfExtents.z;
        return !(minval > rad || maxval < -rad);
      }


      template <typename T>
      inline bool aabbAxisTestZ12(T a, T b, T fa, T fb,
                                  const Vector3<T> &v1, const Vector3<T> &v2,
                                  const Vector3<T> &halfExtents)
      {
        const T p1 = a * v1.x - b * v1.y;
        const T p2 = a * v2.x - b * v2.y;
        const T minval = (p2 < p1) ? p2 : p1;
        const T maxval = (p2 < p1) ? p1 : p2;
        const T rad = fa * halfExtents.x + fb * halfExtents.y;
        return !(minval > rad || maxval < -rad);
      }


      template <typename T>
      inline bool aabbAxisTestZ0(T a, T b, T fa, T fb,
                                 const Vector3<T> &v0, const Vector3<T> &v1,
                                 const Vector3<T> &halfExtents)
      {
        const T p0 = a * v0.x - b * v0.y;
        const T p1 = a * v1.x - b * v1.y;
        const T minval = (p0 < p1) ? p0 : p1;
        const T maxval = (p0 < p1) ? p1 : p0;
        const T rad = fa * halfExtents.x + fb * halfExtents.y;
        return !(minval > rad || maxval < -rad);
      }

      template <typename T>
      void findMinMax(T x0, T x1, T x2, T &minval, T &maxval)
      {
        minval = maxval = x0;
        minval = (x1 < minval) ? x1 : minval;
        maxval = (x1 > maxval) ? x1 : maxval;
        minval = (x2 < minval) ? x2 : minval;
        maxval = (x2 > maxval) ? x2 : maxval;
      }


      template <typename T>
      bool planeBoxOverlap(const Vector3<T> &normal, const Vector3<T> &vert, const Vector3<T> &maxbox)  // -NJMP-
      {
        Vector3<T> vmin, vmax;

        vmin.x = (normal.x > 0.0f) ? -maxbox.x - vert.x : maxbox.x - vert.x; // -NJMP-
        vmax.x = (normal.x > 0.0f) ? maxbox.x - vert.x : -maxbox.x - vert.x; // -NJMP-

        vmin.y = (normal.y > 0.0f) ? -maxbox.y - vert.y : maxbox.y - vert.y; // -NJMP-
        vmax.y = (normal.y > 0.0f) ? maxbox.y - vert.y : -maxbox.y - vert.y; // -NJMP-

        vmin.z = (normal.z > 0.0f) ? -maxbox.z - vert.z : maxbox.z - vert.z; // -NJMP-
        vmax.z = (normal.z > 0.0f) ? maxbox.z - vert.z : -maxbox.z - vert.z; // -NJMP-

        return normal.dot(vmin) <= 0 && normal.dot(vmax) >= 0;
      }
    }

    //--------------------------------------------------------------------------
    // Template functions
    //--------------------------------------------------------------------------
    template <typename T>
    inline Vector3<T> centreT(const Vector3<T> tri[3])
    {
      return (T(1) / T(3)) * (tri[0] + tri[1] + tri[2]);
    }


    template <typename T>
    inline Vector3<T> normalT(const Vector3<T> &v0, const Vector3<T> &v1, const Vector3<T> &v2)
    {
      return (v1 - v0).cross(v2 - v0).normalised();
    }


    template <typename T>
    inline bool isDegenerateT(const Vector3<T> &tri0, const Vector3<T> &tri1, const Vector3<T> &tri2, T epsilon)
    {
      return (tri1 - tri0).cross(tri2 - tri0).magnitudeSquared() < (epsilon * epsilon);
    }


    template <typename T>
    inline Vector4<T> planeT(const Vector3<T> tri[3])
    {
      return planegeom::fromNormalAndPoint(normalT(tri[0], tri[1], tri[2]), tri[0]);
    }


    template <typename T>
    inline bool isPointInsideT(const Vector3<T> &point, const Vector3<T> tri[3])
    {
      // From http://www.blackpawn.com/texts/pointinpoly/ based on Realtime Collision Detection.
      // Compute vectors
      const Vector3<T> v0 = tri[2] - tri[0];
      const Vector3<T> v1 = tri[1] - tri[0];
      const Vector3<T> v2 = point - tri[0];

      // Compute dot products
      const T dot00 = v0.dot(v0);
      const T dot01 = v0.dot(v1);
      const T dot02 = v0.dot(v2);
      const T dot11 = v1.dot(v1);
      const T dot12 = v1.dot(v2);

      // Compute barycentric coordinates
      const T invDenom = T(1) / (dot00 * dot11 - dot01 * dot01);
      const T u = (dot11 * dot02 - dot01 * dot12) * invDenom;
      const T v = (dot00 * dot12 - dot01 * dot02) * invDenom;

      // Check if point is in triangle
      return (u >= T(0)) && (v >= T(0)) && (u + v < T(1));
    }


    template <typename T>
    inline Vector3<T> nearestPointT(const Vector3<T> &point, const Vector3<T> tri[3])
    {
      Vector3<T> edge0 = tri[1] - tri[0];
      Vector3<T> edge1 = tri[2] - tri[0];
      Vector3<T> v0 = tri[0] - point;

      T a = edge0.dot(edge0);
      T b = edge0.dot(edge1);
      T c = edge1.dot(edge1);
      T d = edge0.dot(v0);
      T e = edge1.dot(v0);

      T det = a*c - b*b;
      T s = b*e - c*d;
      T t = b*d - a*e;

      if (s + t < det)
      {
        if (s < 0)
        {
          if (t < 0)
          {
            if (d < 0)
            {
              s = clamp<T>(-d / a, 0, 1);
              t = 0;
            }
            else
            {
              s = 0;
              t = clamp<T>(-e / c, 0, 1);
            }
          }
          else
          {
            s = 0;
            t = clamp<T>(-e / c, 0, 1);
          }
        }
        else if (t < 0)
        {
          s = clamp<T>(-d / a, 0, 1);
          t = 0;
        }
        else
        {
          T invDet = 1 / det;
          s *= invDet;
          t *= invDet;
        }
      }
      else
      {
        if (s < 0)
        {
          T tmp0 = b + d;
          T tmp1 = c + e;
          if (tmp1 > tmp0)
          {
            T numer = tmp1 - tmp0;
            T denom = a - 2 * b + c;
            s = clamp<T>(numer / denom, 0, 1);
            t = 1 - s;
          }
          else
          {
            t = clamp<T>(-e / c, 0, 1);
            s = 0;
          }
        }
        else if (t < 0)
        {
          if (a + d > b + e)
          {
            T numer = c + e - b - d;
            T denom = a - 2 * b + c;
            s = clamp<T>(numer / denom, 0, 1);
            t = 1 - s;
          }
          else
          {
            s = clamp<T>(-e / c, 0, 1);
            t = 0;
          }
        }
        else
        {
          T numer = c + e - b - d;
          T denom = a - 2 * b + c;
          s = clamp<T>(numer / denom, 0, 1);
          t = 1 - s;
        }
      }

      return tri[0] + s * edge0 + t * edge1;
    }


    template <typename T>
    bool intersectRayT(T *hitTime, const Vector3<T> &v0, const Vector3<T> &v1, const Vector3<T> &v2, const Vector3<T> &origin, const Vector3<T> &dir, const T epsilon)
    {
      // From https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
      Vector3<T> e0, e1;  //Edge0, Edge1
      Vector3<T> P, Q, TT;
      T det, inv_det, u, v;
      T t;

      //Find vectors for two edges sharing v0
      e0 = v1 - v0;
      e1 = v2 - v0;
      //Begin calculating determinant - also used to calculate u parameter
      P = dir.cross(e1);
      //if determinant is near zero, ray lies in plane of triangle or ray is parallel to plane of triangle
      det = e0.dot(P);
      //NOT CULLING
      if (det > -epsilon && det < epsilon) return false;
      inv_det = T(1) / det;

      //calculate distance from v0 to ray origin
      TT = origin - v0;

      //Calculate u parameter and test bound
      u = TT.dot(P) * inv_det;
      //The intersection lies outside of the triangle
      if (u < T(0) || u > T(1)) return false;

      //Prepare to test v parameter
      Q = TT.cross(e0);

      //Calculate V parameter and test bound
      v = dir.dot(Q) * inv_det;
      //The intersection lies outside of the triangle
      if (v < T(0) || u + v  > T(1)) return false;

      t = e1.dot(Q) * inv_det;

      if (t > epsilon)
      { //ray intersection
        *hitTime = t;
        return true;
      }

      // No hit, no win
      return false;
    }


    template <typename T>
    bool intersectTrianglesT(const Vector3<T> &a0, const Vector3<T> &a1, const Vector3<T> &a2,
                             const Vector3<T> &b0, const Vector3<T> &b1, const Vector3<T> &b2,
                             const T epsilon)
    {
      // From: Thomas Moller, "A Fast Triangle-Triangle Intersection test"
      // http://web.stanford.edu/class/cs277/resources/papers/Moller1997b.pdf
      // 1. Build triangle planes
      Vector4<T> planeA, planeB;
      Vector3<T> lineD;
      T dist[3], proj[3];
      T signs[3];
      // Line limits
      T ta1, ta2, tb1, tb2;
      int refinds[3];
      bool overlapA, overlapB;

      planeA = planegeom::fromNormalAndPoint(normalT(a0, a1, a2), a0);
      planeB = planegeom::fromNormalAndPoint(normalT(b0, b1, b2), b0);

      // d[V1[i]] = N[2].V1[i] + d[2]
      dist[0] = planeB.xyz().dot(a0) + planeB.w;
      dist[1] = planeB.xyz().dot(a1) + planeB.w;
      dist[2] = planeB.xyz().dot(a2) + planeB.w;

      signs[0] = std::copysign(T(1), dist[0]);
      signs[1] = std::copysign(T(1), dist[1]);
      signs[2] = std::copysign(T(1), dist[2]);

      // Special zero epsilon case. Don't intersect if we share vertices without intersection.
      // In this case we'll either all the non-touching vertices will be on one side or the other,
      // while the touching vertices always have a positive sign (because dist == zero).
      if (epsilon == 0)
      {
        // Zero epsilon. Early out with the exact same triangle.
        if (dist[0] == 0 && dist[1] == 0 && dist[2] == 0)
        {
          return true;
        }

        if (signs[0] == signs[1] && dist[2] == 0 ||
            signs[1] == signs[2] && dist[0] == 0 ||
            signs[2] == signs[0] && dist[1] == 0)
        {
          // Touching triangles. Not an intersection with zero epsilon.
          return false;
        }
      }

      // Check non-intersection cases: all points on the same side of the plane.
      if (signs[0] == signs[1] && signs[0] == signs[2] && signs[1] == signs[2])
      {
        // All points on the same side of the plane => no intersection.
        return false;
      }

      // Triangle coplanar check.
      if (dist[0] <= -epsilon || dist[0] >= epsilon ||
          dist[1] <= -epsilon || dist[1] >= epsilon ||
          dist[2] <= -epsilon || dist[2] >= epsilon)
      {
        // Not coplanar.
        // Solution is a line-intersection test of L = O + tD.
        lineD = planeA.xyz().cross(planeB.xyz());  // D
                                                   // O is arbitrary, so is implicitly dropped without altering the result.
                                                   // Project vertices only L.
                                                   // p[V1[i]] = D.(V1[i] - O)
        proj[0] = lineD.dot(a0);
        proj[1] = lineD.dot(a1);
        proj[2] = lineD.dot(a2);

        // Choose indices to isolate the point on one side of the plane.
        if (signs[0] != signs[1] && signs[0] != signs[2])
        {
          refinds[0] = 1;
          refinds[1] = 0;
          refinds[2] = 2;
        }
        else if (signs[1] != signs[0] && signs[1] != signs[2])
        {
          refinds[0] = 0;
          refinds[1] = 1;
          refinds[2] = 2;
        }
        else
        {
          refinds[0] = 0;
          refinds[1] = 2;
          refinds[2] = 1;
        }

        // t[1] = p[V1[0]] + (p[V1[1]] - p[V1[0]]) * ( d[V1[0]] / (d[V1[0]] - d[V1[1]]) )
        ta1 = proj[refinds[0]] + (proj[refinds[1]] - proj[refinds[0]]) * (dist[refinds[0]] / (dist[refinds[0]] - dist[refinds[1]]));

        // t[2] = p[V1[2]] + (p[V1[1]] - p[V1[2]]) * ( d[V1[2]] / (d[V1[2]] - d[V1[1]]) )
        ta2 = proj[refinds[2]] + (proj[refinds[1]] - proj[refinds[2]]) * (dist[refinds[2]] / (dist[refinds[2]] - dist[refinds[1]]));

        // Now we make the same calculation for triangle 2 on 1.
        dist[0] = planeA.xyz().dot(b0) + planeA.w;
        dist[1] = planeA.xyz().dot(b1) + planeA.w;
        dist[2] = planeA.xyz().dot(b2) + planeA.w;

        proj[0] = lineD.dot(b0);
        proj[1] = lineD.dot(b1);
        proj[2] = lineD.dot(b2);

        signs[0] = std::copysign(T(1), dist[0]);
        signs[1] = std::copysign(T(1), dist[1]);
        signs[2] = std::copysign(T(1), dist[2]);

        // Check non-intersection cases: all points on the same side of the plane.
        if (signs[0] == signs[1] && signs[0] == signs[2] && signs[1] == signs[2])
        {
          // All points on the same side of the plane => no intersection.
          return false;
        }
        // Choose indices to isolate the point on one side of the plane.
        if (signs[0] != signs[1] && signs[0] != signs[2])
        {
          refinds[0] = 1;
          refinds[1] = 0;
          refinds[2] = 2;
        }
        else if (signs[1] != signs[0] && signs[1] != signs[2])
        {
          refinds[0] = 0;
          refinds[1] = 1;
          refinds[2] = 2;
        }
        else
        {
          refinds[0] = 0;
          refinds[1] = 2;
          refinds[2] = 1;
        }

        // t[1] = p[V1[0]] + (p[V1[1]] - p[V1[0]]) * ( d[V1[0]] / (d[V1[0]] - d[V1[1]]) )
        tb1 = proj[refinds[0]] + (proj[refinds[1]] - proj[refinds[0]]) * (dist[refinds[0]] / (dist[refinds[0]] - dist[refinds[1]]));

        // t[2] = p[V1[2]] + (p[V1[1]] - p[V1[2]]) * ( d[V1[2]] / (d[V1[2]] - d[V1[1]]) )
        tb2 = proj[refinds[2]] + (proj[refinds[1]] - proj[refinds[2]]) * (dist[refinds[2]] / (dist[refinds[2]] - dist[refinds[1]]));

        // Intersect if ta1/2 and tb1/2 intervals overlap.
        if (ta1 <= tb1 && tb1 <= ta2 || ta1 <= tb2 && tb2 <= ta2 ||
            tb1 <= ta1 && ta1 <= tb2 || tb1 <= ta2 && ta2 <= tb2)
        {
          return true;
        }
      }
      else
      {
        // (Near) coplanar. Paper is vague on how to do this, so this is custom.
        // Iterate each triangle edge in A.
        //  - project each vertex in B on the edge of A
        //  - no intersection if V from B is outside of the edge.
        // This doesn't handle containment, so we flip the test around and test B to A.
        // We have an overlap if either test passes.
        overlapA = overlapB = true;
        lineD = a1 - a0;
        ta2 = lineD.magnitudeSquared() + epsilon * epsilon;
        proj[0] = lineD.dot(b0 - a0);
        proj[1] = lineD.dot(b1 - a0);
        proj[2] = lineD.dot(b2 - a0);
        overlapA = (proj[0] * proj[0] <= ta2 ||
                    proj[1] * proj[1] <= ta2 ||
                    proj[2] * proj[2] <= ta2) && overlapA;
        lineD = a2 - a1;
        ta2 = lineD.magnitudeSquared() + epsilon * epsilon;
        proj[0] = lineD.dot(b0 - a1);
        proj[1] = lineD.dot(b1 - a1);
        proj[2] = lineD.dot(b2 - a1);
        overlapA = (proj[0] * proj[0] <= ta2 ||
                    proj[1] * proj[1] <= ta2 ||
                    proj[2] * proj[2] <= ta2) && overlapA;
        lineD = a0 - a2;
        ta2 = lineD.magnitudeSquared() + epsilon * epsilon;
        proj[0] = lineD.dot(b0 - a2);
        proj[1] = lineD.dot(b1 - a2);
        proj[2] = lineD.dot(b2 - a2);
        overlapA = (proj[0] * proj[0] <= ta2 ||
                    proj[1] * proj[1] <= ta2 ||
                    proj[2] * proj[2] <= ta2) && overlapA;

        // Reverse the overlap test.
        lineD = b1 - b0;
        ta2 = lineD.magnitudeSquared() + epsilon * epsilon;
        proj[0] = lineD.dot(a0 - b0);
        proj[1] = lineD.dot(a1 - b0);
        proj[2] = lineD.dot(a2 - b0);
        overlapB = (proj[0] * proj[0] <= ta2 ||
                    proj[1] * proj[1] <= ta2 ||
                    proj[2] * proj[2] <= ta2) && overlapB;
        lineD = b2 - b1;
        ta2 = lineD.magnitudeSquared() + epsilon * epsilon;
        proj[0] = lineD.dot(a0 - b1);
        proj[1] = lineD.dot(a1 - b1);
        proj[2] = lineD.dot(a2 - b1);
        overlapB = (proj[0] * proj[0] <= ta2 ||
                    proj[1] * proj[1] <= ta2 ||
                    proj[2] * proj[2] <= ta2) && overlapB;
        lineD = b0 - b2;
        ta2 = lineD.magnitudeSquared() + epsilon * epsilon;
        proj[0] = lineD.dot(a0 - b2);
        proj[1] = lineD.dot(a1 - b2);
        proj[2] = lineD.dot(a2 - b2);
        overlapB = (proj[0] * proj[0] <= ta2 ||
                    proj[1] * proj[1] <= ta2 ||
                    proj[2] * proj[2] <= ta2) && overlapB;

        return overlapA || overlapB;
      }

      return false;
    }

    template <typename T>
    bool intersectAABBT(const Vector3<T> tri[3], const Vector3<T> aabb[2])
    {
      using namespace trigeom::detail;
      // From : http://fileadmin.cs.lth.se/cs/Personal/Tomas_Akenine-Moller/code/tribox3.txt
      /*    use separating axis theorem to test overlap between triangle and box */
      /*    need to test for overlap in these directions: */
      /*    1) the {x,y,z}-directions (actually, since we use the AABB of the triangle */
      /*       we do not even need to test these) */
      /*    2) normal of the triangle */
      /*    3) crossproduct(edge from tri, {x,y,z}-directin) */
      /*       this gives 3x3=9 more tests */
      const Vector3<T> halfExtents = T(0.5) * (aabb[1] - aabb[0]);
      const Vector3<T> centre = T(0.5) * (aabb[1] + aabb[0]);
      Vector3<T> v0, v1, v2;
      Vector3<T> normal, e0, e1, e2;
      //   float axis[3];
      T minval, maxval, fex, fey, fez;   // -NJMP- "d" local variable removed
      bool intersect = true;

      /* move everything so that the box center is in (0,0,0) */
      v0 = tri[0] - centre;
      v1 = tri[1] - centre;
      v2 = tri[2] - centre;

      /* compute triangle edges */
      e0 = v1 - v0;
      e1 = v2 - v1;
      e2 = v0 - v2;

      /* Bullet 3:  */
      /*  test the 9 tests first (this was faster) */
      fex = detail::abs(e0.x);
      fey = detail::abs(e0.y);
      fez = detail::abs(e0.z);

      intersect = aabbAxisTestX01(e0.z, e0.y, fez, fey, v0, v2, halfExtents) && intersect;
      intersect = aabbAxisTestY02(e0.z, e0.x, fez, fex, v0, v2, halfExtents) && intersect;
      intersect = aabbAxisTestZ12(e0.y, e0.x, fey, fex, v1, v2, halfExtents) && intersect;

      fex = detail::abs(e1.x);
      fey = detail::abs(e1.y);
      fez = detail::abs(e1.z);
      intersect = aabbAxisTestX01(e1.z, e1.y, fez, fey, v0, v2, halfExtents) && intersect;
      intersect = aabbAxisTestY02(e1.z, e1.x, fez, fex, v0, v2, halfExtents) && intersect;
      intersect = aabbAxisTestZ0(e1.y, e1.x, fey, fex, v0, v1, halfExtents) && intersect;

      fex = detail::abs(e2.x);
      fey = detail::abs(e2.y);
      fez = detail::abs(e2.z);

      intersect = aabbAxisTestX2(e2.z, e2.y, fez, fey, v0, v1, halfExtents) && intersect;
      intersect = aabbAxisTestY1(e2.z, e2.x, fez, fex, v0, v1, halfExtents) && intersect;
      intersect = aabbAxisTestZ12(e2.y, e2.x, fey, fex, v1, v2, halfExtents) && intersect;

      /* Bullet 1: */
      /*  first test overlap in the {x,y,z}-directions */
      /*  find minval, maxval of the triangle each direction, and test for overlap in */
      /*  that direction -- this is equivalent to testing a minimal AABB around */
      /*  the triangle against the AABB */

      /* test in X-direction */
      findMinMax(v0.x, v1.x, v2.x, minval, maxval);
      intersect = (minval <= halfExtents.x && maxval >= -halfExtents.x) && intersect;

      /* test in Y-direction */
      findMinMax(v0.y, v1.y, v2.y, minval, maxval);
      intersect = (minval <= halfExtents.y && maxval >= -halfExtents.y) && intersect;

      /* test in Z-direction */
      findMinMax(v0.z, v1.z, v2.z, minval, maxval);
      intersect = (minval <= halfExtents.z && maxval >= -halfExtents.z) && intersect;

      /* Bullet 2: */
      /*  test if the box intersects the plane of the triangle */
      /*  compute plane equation of triangle: normal*x+d=0 */
      normal = e0.cross(e1);

      // -NJMP- (line removed here)
      intersect = planeBoxOverlap(normal, v0, halfExtents) && intersect; // -NJMP-
      return intersect;
    }

    //--------------------------------------------------------------------------
    // Vector3f functions
    //--------------------------------------------------------------------------
    Vector3f centre(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2)
    {
      const Vector3f tri[3] = { v0, v1, v2 };
      return centreT(tri);
    }


    Vector3f centre(const Vector3f tri[3])
    {
      return centreT(tri);
    }


    Vector3f normal(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2)
    {
      return normalT(v0, v1, v2);
    }


    Vector3f normal(const Vector3f tri[3])
    {
      return normalT(tri[0], tri[1], tri[2]);
    }

    Vector4f plane(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2)
    {
      const Vector3f tri[3] = { v0, v1, v2 };
      return planeT(tri);
    }


    Vector4f plane(const Vector3f tri[3])
    {
      return planeT(tri);
    }


    bool isDegenerate(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2, float epsilon)
    {
      return isDegenerateT(v0, v1, v2, epsilon);
    }


    bool isDegenerate(const Vector3f tri[3], float epsilon)
    {
      return isDegenerateT(tri[0], tri[1], tri[2], epsilon);
    }


    bool isPointInside(const Vector3f &point, const Vector3f &v0, const Vector3f &v1, const Vector3f &v2)
    {
      const Vector3f tri[3] = { v0, v1, v2 };
      return isPointInsideT(point, tri);
    }


    bool isPointInside(const Vector3f &point, const Vector3f tri[3])
    {
      return isPointInsideT(point, tri);
    }


    Vector3f nearestPoint(const Vector3f &point, const Vector3f &v0, const Vector3f &v1, const Vector3f &v2)
    {
      const Vector3f tri[3] = { v0, v1, v2 };
      return nearestPointT(point, tri);
    }


    Vector3f nearestPoint(const Vector3f &point, const Vector3f tri[3])
    {
      return nearestPointT(point, tri);
    }


    bool intersectRay(float *hitTime, const Vector3f &v0, const Vector3f &v1, const Vector3f &v2, const Vector3f &origin, const Vector3f &dir, const float epsilon)
    {
      return intersectRayT(hitTime, v0, v1, v2, origin, dir, epsilon);
    }


    bool intersectTriangles(const Vector3f &a0, const Vector3f &a1, const Vector3f &a2,
                            const Vector3f &b0, const Vector3f &b1, const Vector3f &b2,
                            const float epsilon)
    {
      return intersectTrianglesT(a0, a1, a2, b0, b1, b2, epsilon);
    }


    inline bool intersectAABB(const Vector3f tri[3], const Vector3f aabb[2])
    {
      return intersectAABBT(tri, aabb);
    }


    //--------------------------------------------------------------------------
    // Vector3d functions
    //--------------------------------------------------------------------------
    Vector3d centre(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2)
    {
      const Vector3d tri[3] = { v0, v1, v2 };
      return centreT(tri);
    }


    Vector3d centre(const Vector3d tri[3])
    {
      return centreT(tri);
    }


    Vector3d normal(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2)
    {
      return normalT(v0, v1, v2);
    }


    Vector3d normal(const Vector3d tri[3])
    {
      return normalT(tri[0], tri[1], tri[2]);
    }

    Vector4d plane(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2)
    {
      const Vector3d tri[3] = { v0, v1, v2 };
      return planeT(tri);
    }


    Vector4d plane(const Vector3d tri[3])
    {
      return planeT(tri);
    }


    bool isDegenerate(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2, double epsilon)
    {
      return isDegenerateT(v0, v1, v2, epsilon);
    }


    bool isDegenerate(const Vector3d tri[3], double epsilon)
    {
      return isDegenerateT(tri[0], tri[1], tri[2], epsilon);
    }


    bool isPointInside(const Vector3d &point, const Vector3d &v0, const Vector3d &v1, const Vector3d &v2)
    {
      const Vector3d tri[3] = { v0, v1, v2 };
      return isPointInsideT(point, tri);
    }


    bool isPointInside(const Vector3d &point, const Vector3d tri[3])
    {
      return isPointInsideT(point, tri);
    }


    Vector3d nearestPoint(const Vector3d &point, const Vector3d &v0, const Vector3d &v1, const Vector3d &v2)
    {
      const Vector3d tri[3] = { v0, v1, v2 };
      return nearestPointT(point, tri);
    }


    Vector3d nearestPoint(const Vector3d &point, const Vector3d tri[3])
    {
      return nearestPointT(point, tri);
    }


    bool intersectRay(double *hitTime, const Vector3d &v0, const Vector3d &v1, const Vector3d &v2, const Vector3d &origin, const Vector3d &dir, const double epsilon)
    {
     return intersectRayT(hitTime, v0, v1, v2, origin, dir, epsilon);
    }


    bool intersectTriangles(const Vector3d &a0, const Vector3d &a1, const Vector3d &a2,
                            const Vector3d &b0, const Vector3d &b1, const Vector3d &b2,
                            const double epsilon)
    {
      return intersectTrianglesT(a0, a1, a2, b0, b1, b2, epsilon);
    }


    inline bool intersectAABB(const Vector3d tri[3], const Vector3d aabb[2])
    {
      return intersectAABBT(tri, aabb);
    }
  }
}
