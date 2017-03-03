//
// author Kazys Stepanas
//
#include "3es-occupancy.h"

#include <3esvector3.h>

struct OccupancyLoaderDetail;

class OccupancyLoader
{
public:
  OccupancyLoader();
  ~OccupancyLoader();

  bool open(const char *sampleFilePath, const char *trajectoryFilePath);

  void close();

  bool sampleFileIsOpen() const;
  bool trajectoryFileIsOpen() const;

  bool nextPoint(tes::Vector3f &sample, tes::Vector3f &origin, double *timestamp = nullptr);

private:
  /// Sample the trajectory at the given timestamp.
  ///
  /// This reads the trajectory to the segment which covers @p timestamp and
  /// linearly interpolates a @p position at this time.
  ///
  /// @param[out] position Set to the trajectory position on success.
  /// @param timestamp The desired sample time.
  /// @return True on success, false when @p timestamp is out of range.
  bool sampleTrajectory(tes::Vector3f &position, double timestamp);

  OccupancyLoaderDetail *_imp;
};
