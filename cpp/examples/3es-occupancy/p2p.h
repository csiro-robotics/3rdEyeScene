//
// author Kazys Stepanas
//
#ifndef P2P_H
#define P2P_H

#include "3es-occupancy.h"

#include <octomap/octomap.h>
#include "3esvector3.h"

#include <cstddef>

inline tes::Vector3f p2p(const octomap::point3d &p)
{
  return tes::Vector3f(p.x(), p.y(), p.z());
}

inline octomap::point3d p2p(const tes::Vector3f &p)
{
  return octomap::point3d(p.x, p.y, p.z);
}

inline const tes::Vector3f *p2pArray(const octomap::point3d *points)
{
  static_assert(sizeof(tes::Vector3f) == sizeof(octomap::point3d), "tes::Vector3f size does not match octomap::point3d size.");
  return reinterpret_cast<const tes::Vector3f *>(points);
}

inline const octomap::point3d *p2pArray(const tes::Vector3f *points)
{
  static_assert(sizeof(tes::Vector3f) == sizeof(octomap::point3d), "tes::Vector3f size does not match octomap::point3d size.");
  return reinterpret_cast<const octomap::point3d *>(points);
}

#endif  // P2P_H
