//
// author: Kazys Stepanas
//
#ifndef _3ESTRIANGLE_H_
#define _3ESTRIANGLE_H_

#include "3esvector4.h"

namespace tes
{
  /// Geometry functions based around triangles.
  namespace trigeom
  {
    //--------------------------------------------------------------------------
    // Vector3f functions
    //--------------------------------------------------------------------------
    /// @overload
    Vector3f normal(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2);
    /// Calculate the triangle normal.
    /// Results are undefined for degenerate triangles.
    /// @param tri Triangle vertices.
    /// @return The triangle normal.
    Vector3f normal(const Vector3f tri[3]);

    /// @overload
    Vector4f plane(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2);
    /// Calculate a plane representation for the given triangle.
    ///
    /// This calculates the triangle plane in the resulting XYZ coordinates,
    /// and a plane distance value in W.
    ///
    /// Results are undefined for degenerate triangles.
    /// @param tri Triangle vertices.
    /// @return A representation of the triangle plane.
    Vector4f plane(const Vector3f tri[3]);

    /// @overload
    bool isDegenerate(const Vector3f &v0, const Vector3f &v1, const Vector3f &v2, float epsilon = 1e-6f);
    /// Check for a degenerate triangle This checks the magnitude of the cross product of the
    /// edges to be greater than @p epsilon for non-degenerate triangles.
    /// @param tri Triangle vertices.
    /// @param epsilon Error tolerance.
    /// @return True if the triangle is degenerate.
    bool isDegenerate(const Vector3f tri[3], float epsilon = 1e-6f);

    /// @overload
    bool isPointInside(const Vector3f &point, const Vector3f &v0, const Vector3f &v1, const Vector3f &v2);
    /// Check if a point lies inside a triangle, assuming they are on the same plane.
    /// Results are undefined for degenerate triangles.
    /// @param point The point to test. Assumed to be on the triangle plane.
    /// @param tri The triangle vertices.
    /// @return True if @p point lies inside the triangle.
    bool isPointInside(const Vector3f &point, const Vector3f tri[3]);

    /// @overload
    Vector3f nearestPoint(const Vector3f &point, const Vector3f &v0, const Vector3f &v1, const Vector3f &v2);
    /// Find a point on or within @p tri closest to @p point.
    /// The @p point need not be on the same plane as it is first projected onto that plane..
    /// Results are undefined for degenerate triangles.
    /// @param point The point of interest.
    /// @param tri The triangle vertices.
    /// @return The point on or within @p tri closest to @p point.
    Vector3f nearestPoint(const Vector3f &point, const Vector3f tri[3]);

    /// Performs a ray/triangle intersection test.
    ///
    /// When an intersection occurs, the @p hitTime is set to represent the 'time' of
    /// intersection along the ray @p dir. This is always positive and intersections backwards
    /// along the ray are ignored. The location of the intersection can be calculated as:
    /// @code{.unparsed}
    ///   Vector3f p = origin + hitTime * dir;
    /// @endcode
    ///
    /// So long as @p dir is normalised, the @p hitTime represents the distance long the ray
    /// at which intersection occurs.
    ///
    /// @param[out] hitTime Represents the intersection 'time'. Must not be null.
    /// @param v0 A triangle vertex.
    /// @param v1 A triangle vertex.
    /// @param v2 A triangle vertex.
    /// @param origin The ray origin.
    /// @param dir The ray direction. Need not be normalised, but is best to be.
    /// @param epsilon Intersection error tolerance.
    bool intersectRay(float *hitTime, const Vector3f &v0, const Vector3f &v1, const Vector3f &v2,
                      const Vector3f &origin, const Vector3f &dir, const float epsilon = 1e-6f);

    /// Triangle intersection test.
    ///
    /// As a special case, the triangles are not considered intersecting when they exactly
    /// touch (equal vertices) and epsilon is zero.
    bool intersectTriangles(const Vector3f &a0, const Vector3f &a1, const Vector3f &a2,
                            const Vector3f &b0, const Vector3f &b1, const Vector3f &b2,
                            const float epsilon = 1e-6f);

    /// Intersect a triangle with an axis aligned box.
    /// @param tri The triangle vertices.
    /// @param aabb The axis aligned box. Index zero is the minimum extents, index one the maximum.
    /// @return True if the triangle overlaps, lies inside or contains the box.
    bool intersectAABB(const Vector3f tri[3], const Vector3f aabb[2]);

    //--------------------------------------------------------------------------
    // Vector3d functions
    //--------------------------------------------------------------------------
    /// @overload
    Vector3d normal(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2);
    /// @overload
    Vector3d normal(const Vector3d tri[3]);

    /// @overload
    Vector4d plane(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2);
    /// @overload
    Vector4d plane(const Vector3d tri[3]);

    /// @overload
    bool isDegenerate(const Vector3d &v0, const Vector3d &v1, const Vector3d &v2, double epsilon = 1e-6);
    /// @overload
    bool isDegenerate(const Vector3d tri[3], double epsilon = 1e-6);

    /// @overload
    bool isPointInside(const Vector3d &point, const Vector3d &v0, const Vector3d &v1, const Vector3d &v2);
    /// @overload
    bool isPointInside(const Vector3d &point, const Vector3d tri[3]);

    /// @overload
    Vector3d nearestPoint(const Vector3d &point, const Vector3d &v0, const Vector3d &v1, const Vector3d &v2);
    /// @overload
    Vector3d nearestPoint(const Vector3d &point, const Vector3d tri[3]);

    /// @overload
    bool intersectRay(double *hitTime, const Vector3d &v0, const Vector3d &v1, const Vector3d &v2, const Vector3d &origin, const Vector3d &dir, const double epsilon = 1e-6);
    /// @overload
    bool intersectTriangles(const Vector3d &a0, const Vector3d &a1, const Vector3d &a2,
                            const Vector3d &b0, const Vector3d &b1, const Vector3d &b2,
                            const double epsilon = 1e-6);

    /// @overload
    bool intersectAABB(const Vector3d tri[3], const Vector3d aabb[2]);
  }
}

#include "3estrigeom.inl"

#endif  // _3ESTRIANGLE_H
