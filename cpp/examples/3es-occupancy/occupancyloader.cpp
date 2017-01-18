//
// author Kazys Stepanas
//
#include "occupancyloader.h"

#ifdef _MSC_VER
// std::equal with parameters that may be unsafe warning under Visual Studio.
#pragma warning(disable : 4996)
#endif // _MSC_VER

#include <string>
#include <fstream>

#include <liblas/liblas.hpp>
#include <liblas/factory.hpp>
#include <liblas/reader.hpp>

struct TrajectoryPoint
{
  double timestamp;
  tes::Vector3f position;
};

struct OccupancyLoaderDetail
{
  liblas::ReaderFactory *lasReaderFactory;
  liblas::Reader *sampleReader;
  liblas::Reader *trajectoryReader;
  std::string sampleFilePath;
  std::string trajectoryFilePath;
  std::ifstream sampleFile;
  std::ifstream trajectoryFile;
  TrajectoryPoint trajectoryBuffer[2];

  inline OccupancyLoaderDetail()
    : lasReaderFactory(nullptr)
    , sampleReader(nullptr)
    , trajectoryReader(nullptr)
  {
    memset(&trajectoryBuffer, 0, sizeof(trajectoryBuffer));
  }

  inline ~OccupancyLoaderDetail()
  {
    // Factory must be destroyed last.
    delete trajectoryReader;
    delete sampleReader;
    delete lasReaderFactory;
  }
};

namespace
{
  std::string getFileExtension(const std::string &file)
  {
    size_t lastDot = file.find_last_of(".");
    if (lastDot != std::string::npos)
    {
      return file.substr(lastDot + 1);
    }

    return "";
  }

  bool openLasFile(std::ifstream &in, const std::string &fileName)
  {
    const std::string ext = getFileExtension(fileName);
    if (ext.compare("laz") == 0 || ext.compare("las") == 0)
    {
      return liblas::Open(in, fileName);
    }

    // Extension omitted.
    // Try for a LAZ file (compressed), then a LAS file.
    if (liblas::Open(in, fileName + ".laz"))
    {
      return true;
    }
    return liblas::Open(in, fileName + ".las");
  }
}

OccupancyLoader::OccupancyLoader()
  : _imp(new OccupancyLoaderDetail)
{
  _imp->lasReaderFactory = new liblas::ReaderFactory;
}


OccupancyLoader::~OccupancyLoader()
{
  delete _imp;
}


bool OccupancyLoader::open(const char *sampleFilePath, const char *trajectoryFilePath)
{
  close();

  _imp->sampleFilePath = sampleFilePath;
  _imp->trajectoryFilePath = trajectoryFilePath;

  openLasFile(_imp->sampleFile, _imp->sampleFilePath);
  openLasFile(_imp->trajectoryFile, _imp->trajectoryFilePath);

  if (!sampleFileIsOpen() || !trajectoryFileIsOpen())
  {
    close();
    return false;
  }

  _imp->sampleReader = new liblas::Reader(_imp->lasReaderFactory->CreateWithStream(_imp->sampleFile));
  _imp->trajectoryReader = new liblas::Reader(_imp->lasReaderFactory->CreateWithStream(_imp->trajectoryFile));

  // Prime the trajectory buffer.
  bool trajectoryPrimed = true;
  for (int i = 0; i < 2; ++i)
  {
    if (_imp->trajectoryReader->ReadNextPoint())
    {
      const liblas::Point &p = _imp->trajectoryReader->GetPoint();
      _imp->trajectoryBuffer[i].timestamp = p.GetTime();
      _imp->trajectoryBuffer[i].position = tes::Vector3d(p.GetX(), p.GetY(), p.GetZ());
    }
    else
    {
      trajectoryPrimed = false;
    }
  }

  if (!trajectoryPrimed)
  {
    close();
    return false;
  }

  return true;
}


void OccupancyLoader::close()
{
  delete _imp->trajectoryReader;
  delete _imp->sampleReader;
  _imp->sampleReader = _imp->trajectoryReader = nullptr;
  _imp->sampleFile.close();
  _imp->trajectoryFile.close();
  _imp->sampleFilePath.clear();
  _imp->trajectoryFilePath.clear();
}


bool OccupancyLoader::sampleFileIsOpen() const
{
  return _imp->sampleFile.is_open();
}


bool OccupancyLoader::trajectoryFileIsOpen() const
{
  return _imp->trajectoryFile.is_open();
}


bool OccupancyLoader::nextPoint(tes::Vector3f &sample, tes::Vector3f &origin, double *timestampOut)
{
  if (_imp->sampleReader && _imp->sampleReader->ReadNextPoint())
  {
    const liblas::Point &p = _imp->sampleReader->GetPoint();
    const double timestamp = p.GetTime();
    if (*timestampOut)
    {
      *timestampOut = timestamp;
    }
    origin = tes::Vector3f(0.0f);
    sample = tes::Vector3d(p.GetX(), p.GetY(), p.GetZ());
    sampleTrajectory(origin, timestamp);
    return true;
  }
  return false;
}


bool OccupancyLoader::sampleTrajectory(tes::Vector3f &position, double timestamp)
{
  if (_imp->trajectoryReader)
  {
    while ((timestamp < _imp->trajectoryBuffer[0].timestamp || timestamp > _imp->trajectoryBuffer[1].timestamp)
            && _imp->trajectoryReader->ReadNextPoint())
    {
      const liblas::Point &p = _imp->trajectoryReader->GetPoint();
      _imp->trajectoryBuffer[0] = _imp->trajectoryBuffer[1];
      _imp->trajectoryBuffer[1].timestamp = p.GetTime();
      _imp->trajectoryBuffer[1].position = tes::Vector3d(p.GetX(), p.GetY(), p.GetZ());
    }

    if (_imp->trajectoryBuffer[0].timestamp <= timestamp &&
        timestamp <= _imp->trajectoryBuffer[1].timestamp &&
        _imp->trajectoryBuffer[0].timestamp != _imp->trajectoryBuffer[1].timestamp)
    {
      float lerp = float((timestamp - _imp->trajectoryBuffer[0].timestamp) / (_imp->trajectoryBuffer[1].timestamp - _imp->trajectoryBuffer[0].timestamp));
      position = _imp->trajectoryBuffer[0].position + lerp * (_imp->trajectoryBuffer[1].position - _imp->trajectoryBuffer[0].position);
      return true;
    }
  }
  return false;
}
