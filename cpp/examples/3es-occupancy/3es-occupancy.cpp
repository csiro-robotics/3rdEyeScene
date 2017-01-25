//
// author Kazys Stepanas
//
#include "3es-occupancy.h"

#include "3esvector3.h"

#include <3esservermacros.h>

#include "occupancyloader.h"
#include "occupancymesh.h"
#include "p2p.h"

#include <csignal>
#include <cstddef>
#include <fstream>
#include <sstream>
#include <unordered_set>

// Forced bug ideas to show now 3es highlights the issue(s).
// 1. Skip inserting the sample voxel key assuming that the ray will do so.
// 2. call integrateMiss() instead of integrateHit().
// 3. no trajectory data.

using namespace tes;

TES_SERVER_DECL(g_tesServer);

namespace
{
  bool quit = false;

  void onSignal(int arg)
  {
    if (arg == SIGINT || arg == SIGTERM)
    {
      quit = true;
    }
  }


  struct Options
  {
    std::string cloudFile;
    std::string trajectoryFile;
    uint64_t pointLimit;
    float resolution;
    float probHit;
    float probMiss;
    unsigned batchSize;
    bool noRays;
    bool quiet;

    inline Options()
      : pointLimit(0)
      , resolution(0.1f)
      , probHit(0.7f)
      , probMiss(0.49f)
      , batchSize(1000)
      , noRays(false)
      , quiet(false)
    {}
  };

  typedef std::unordered_set<octomap::OcTreeKey, octomap::OcTreeKey::KeyHash> KeySet;


  bool optionValue(const char *arg, int argc, char *argv[], std::string &value)
  {
    if (*arg == '=')
    {
      ++arg;
    }
    value = arg;
    return true;
  }

  template <typename NUMERIC>
  bool optionValue(const char *arg, int argc, char *argv[], NUMERIC &value)
  {
    std::string strValue;
    if (optionValue(arg, argc, argv, strValue))
    {
      std::istringstream instr(strValue);
      instr >> value;
      return !instr.fail();
    }

    return false;
  }


  void shiftToSet(UnorderedKeySet &dst, UnorderedKeySet &src, const octomap::OcTreeKey &key)
  {
    auto iter = src.find(key);
    if (iter != src.end())
    {
      src.erase(iter);
    }
    dst.insert(key);
  }

#ifdef TES_ENABLE
  void renderVoxels(const UnorderedKeySet &keys, const octomap::OcTree &map, const tes::Colour &colour, uint16_t category)
  {
    // Convert to voxel centres.
    if (!keys.empty())
    {
      std::vector<Vector3f> centres(keys.size());
      size_t index = 0;
      for (auto key : keys)
      {
        centres[index++] = p2p(map.keyToCoord(key));
      }

      // Render slightly smaller than the actual voxel size.
      TES_VOXELS(*g_tesServer, colour, 0.95f * float(map.getResolution()),
                 centres.data()->v, unsigned(centres.size()), sizeof(*centres.data()),
                 0u, category);
    }
  }
#endif // TES_ENABLE
}


int populateMap(const Options &opt)
{
  printf("Loading points from %s with trajectory %s \n", opt.cloudFile.c_str(), opt.trajectoryFile.c_str());

  OccupancyLoader loader;
  if (!loader.open(opt.cloudFile.c_str(), opt.trajectoryFile.c_str()))
  {
    fprintf(stderr, "Error loading cloud %s with trajectory %s \n", opt.cloudFile.c_str(), opt.trajectoryFile.c_str());
    TES_SERVER_STOP(*g_tesServer);
    return -2;
  }

  octomap::KeyRay rayKeys;
  octomap::OcTree map(opt.resolution);
  octomap::OcTreeKey key;
  tes::Vector3f origin, sample;
  tes::Vector3f voxel, ext(opt.resolution);
  double timestamp;
  uint64_t pointCount = 0;
  size_t keyIndex;
  // Update map visualisation every N samples.
  const size_t rayBatchSize = opt.batchSize;
#ifdef TES_ENABLE
  char timeStrBuffer[256];
  double timebase = -1;
  double firstBatchTimestamp = -1;
  double lastTimestamp = -1;
  // Keys of voxels touched in the current batch.
  UnorderedKeySet becomeOccupied;
  UnorderedKeySet becomeFree;
  UnorderedKeySet touchedFree;
  UnorderedKeySet touchedOccupied;
  std::vector<Vector3f> rays;
  OccupancyMesh mapMesh(RES_MapMesh, map);
#endif // TES_ENABLE

  map.setProbHit(opt.probHit);
  map.setProbMiss(opt.probMiss);

  // Prevent ready saturation to free.
  map.setClampingThresMin(0.01);
  //printf("min: %g\n", map.getClampingThresMinLog());

  TES_POINTCLOUDSHAPE(*g_tesServer, TES_COLOUR(SteelBlue), &mapMesh, RES_Map, CAT_Map);
  // Ensure mesh is created for later update.
  TES_SERVER_UPDATE(*g_tesServer, 0.0f);

  printf("Populating map");
  while (loader.nextPoint(sample, origin, &timestamp))
  {
    ++pointCount;
    TES_STMT(rays.push_back(origin));
    TES_STMT(rays.push_back(sample));
    if (firstBatchTimestamp < 0)
    {
      firstBatchTimestamp = timestamp;
    }
    // Compute free ray.
    map.computeRayKeys(p2p(origin), p2p(sample), rayKeys);
    // Draw intersected voxels.
    const size_t rayKeyCount = rayKeys.size();
    keyIndex = 0;
    for (auto key : rayKeys)
    {
      if (octomap::OcTree::NodeType *node = map.search(key))
      {
        // Existing node. 
        const bool initiallyOccupied = map.isNodeOccupied(node);
        map.integrateMiss(node);
        if (initiallyOccupied && !map.isNodeOccupied(node))
        {
          // Node became free.
#ifdef TES_ENABLE
          shiftToSet(becomeFree, becomeOccupied, key);
#endif // TES_ENABLE
        }
      }
      else
      {
        // New node.
        map.updateNode(key, false, true);
      }
      voxel = p2p(map.keyToCoord(key));
      // Collate for render.
      TES_STMT(touchedFree.insert(key));
      ++keyIndex;
    }

    // Update the sample node.
    key = map.coordToKey(p2p(sample));
    if (octomap::OcTree::NodeType *node = map.search(key))
    {
      // Existing node. 
      const bool initiallyOccupied = map.isNodeOccupied(node);
      map.integrateHit(node);
      if (!initiallyOccupied && map.isNodeOccupied(node))
      {
        // Node became occupied.
        TES_STMT(shiftToSet(becomeOccupied, becomeFree, key));
      }
    }
    else
    {
      // New node.
      map.updateNode(key, true, true);
      // Collate for render.
      TES_STMT(shiftToSet(becomeOccupied, becomeFree, key));
    }
    TES_STMT(shiftToSet(touchedOccupied, touchedFree, key));

    if (pointCount % rayBatchSize == 0 || quit)
    {
      //// Collapse the map.
      //map.isNodeCollapsible()
#ifdef TES_ENABLE
      double elapsedTime = (lastTimestamp >= 0) ? timestamp - lastTimestamp : timestamp - firstBatchTimestamp;
      // Handle time jumps back.
      elapsedTime = std::max(elapsedTime, 0.0);
      // Cull large time differences.
      elapsedTime = std::min(elapsedTime, 1.0);
      timebase = (timebase >= 0) ? timebase : firstBatchTimestamp;
      lastTimestamp = timestamp;
      firstBatchTimestamp = -1;

      sprintf(timeStrBuffer, "%g", timestamp - timebase);
      TES_TEXT2D_SCREEN(*g_tesServer, TES_COLOUR(White), timeStrBuffer, 0u, CAT_Info, Vector3f(0.05f, 0.1f, 0.0f));
      if (!rays.empty())
      {
        // Draw sample lines.
        if (!opt.noRays)
        {
          TES_LINES(*g_tesServer, TES_COLOUR(DarkOrange),
                    rays.data()->v, unsigned(rays.size()), sizeof(*rays.data()),
                    0u, CAT_Rays);
        }
        rays.clear();
      }
      // Render touched voxels in bulk.
      renderVoxels(touchedFree, map, tes::Colour::Colours[tes::Colour::MediumSpringGreen], CAT_FreeCells);
      renderVoxels(touchedOccupied, map, tes::Colour::Colours[tes::Colour::Turquoise], CAT_OccupiedCells);
      //TES_SERVER_UPDATE(*g_tesServer, 0.0f);

      // Ensure touchedOccupied does not contain newly occupied nodes for mesh update.
      for (auto key : becomeOccupied)
      {
        auto search = touchedOccupied.find(key);
        if (search != touchedOccupied.end())
        {
          touchedOccupied.erase(search);
        }
      }

      // Render changes to the map.
      mapMesh.update(becomeOccupied, becomeFree, touchedOccupied);

      touchedFree.clear();
      touchedOccupied.clear();
      becomeOccupied.clear();
      becomeFree.clear();
      TES_SERVER_UPDATE(*g_tesServer, float(elapsedTime));
      if (opt.pointLimit && pointCount >= opt.pointLimit || quit)
      {
        break;
      }
#endif // TES_ENABLE

      if (!opt.quiet)
      {
        printf("\r%g        ", lastTimestamp - timebase);
      }
    }
  }

  TES_SERVER_UPDATE(*g_tesServer, 0.0f);

  if (!opt.quiet)
  {
    printf("\n");
  }

  printf("Processed %" PRIu64 " points.\n", pointCount);

  // Save the occupancy map.
  printf("Saving map");
  map.writeBinary("map.bt");

  return 0;
}


void usage(const Options &opt)
{
  printf("Usage:\n");
  printf("3es-occupancy [options] <cloud.las> <trajectory.las>\n");
  printf("\nGenerates an Octomap occupancy map from a LAS/LAZ based point cloud and accompanying trajectory file.\n\n");
  printf("The trajectory marks the scanner trajectory with timestamps loosely corresponding to cloud point timestamps. ");
  printf("Trajectory points are interpolated for each cloud point based on corresponding times in the trajectory.\n\n");
  printf("Third Eye Scene render commands are interspersed throughout the code to visualise the generation process\n\n");
  printf("Options:\n");
  printf("-b=<batch-size> (%u)\n", opt.batchSize);
  printf("  The number of points to process in each batch. Controls debug display.\n");
  printf("-h=<hit-probability> (%g)\n", opt.probHit);
  printf("  The occupancy probability due to a hit. Must be >= 0.5.\n");
  printf("-m=<miss-probability> (%g)\n", opt.probMiss);
  printf("  The occupancy probability due to a miss. Must be < 0.5.\n");
  printf("-p=<point-limit> (0)\n");
  printf("  The voxel resolution of the generated map.\n");
  printf("-q\n");
  printf("  Run in quiet mode. Suppresses progress messages.\n");
  printf("-r=<resolution> (%g)\n", opt.resolution);
  printf("  The voxel resolution of the generated map.\n");
  printf("--no-rays\n");
  printf("  Disable output of sample lines\n");
}

void initialiseDebugCategories()
{
  TES_CATEGORY(*g_tesServer, "Map", CAT_Map, 0, true);
  TES_CATEGORY(*g_tesServer, "Populate", CAT_Populate, 0, true);
  TES_CATEGORY(*g_tesServer, "Rays", CAT_Rays, CAT_Populate, true);
  TES_CATEGORY(*g_tesServer, "Free", CAT_FreeCells, CAT_Populate, false);
  TES_CATEGORY(*g_tesServer, "Occupied", CAT_OccupiedCells, CAT_Populate, true);
  TES_CATEGORY(*g_tesServer, "Info", CAT_Info, 0, true);
}

int main(int argc, char *argv[])
{
  Options opt;

  if (argc < 3)
  {
    usage(opt);
    return 0;
  }

  for (int i = 1; i < argc; ++i)
  {
    if (argv[i][0] == '-')
    {
      bool ok = true;
      switch (argv[i][1])
      {
      case 'b': // batch size
        ok = optionValue(argv[i] + 2, argc, argv, opt.batchSize);
        break;
      case 'h':
        ok = optionValue(argv[i] + 2, argc, argv, opt.probHit);
        break;
      case 'm':
        ok = optionValue(argv[i] + 2, argc, argv, opt.probMiss);
        break;
      case 'p': // point limit
        ok = optionValue(argv[i] + 2, argc, argv, opt.pointLimit);
        break;
      case 'q': // quiet
        opt.quiet = true;
        break;
      case 'r': // resolution
        ok = optionValue(argv[i] + 2, argc, argv, opt.resolution);
        break;
      case '-':  // Long option name.
      {
        if (std::string(&argv[i][2]).compare("no-lines") == 0)
        {
          opt.noRays = true;
        }
        break;
      }
      }

      if (!ok)
      {
        fprintf(stderr, "Failed to read %s option value.\n", argv[i]);
      }
    }
    else if (opt.cloudFile.empty())
    {
      opt.cloudFile = argv[i];
    }
    else if (opt.trajectoryFile.empty())
    {
      opt.trajectoryFile = argv[i];
    }
  }

  if (opt.cloudFile.empty())
  {
    fprintf(stderr, "Missing input cloud (-i)\n");
    return -1;
  }
  if (opt.trajectoryFile.empty())
  {
    fprintf(stderr, "Missing trajectory file (-t)\n");
    return -1;
  }

  // Initialise TES
  TES_SETTINGS(settings, tes::SF_Compress | tes::SF_Collate);
  // Initialise server info.
  TES_SERVER_INFO(info, tes::XYZ);
  // Create the server. Use tesServer declared globally above.
  TES_SERVER_CREATE(g_tesServer, settings, &info);

  // Start the server and wait for the connection monitor to start.
  TES_SERVER_START(*g_tesServer, tes::ConnectionMonitor::Asynchronous);
  TES_SERVER_START_WAIT(*g_tesServer, 1000);

#ifdef TES_ENABLE
  std::cout << "Starting with " << g_tesServer->connectionCount() << " connection(s)." << std::endl;
#endif // TES_ENABLE

  initialiseDebugCategories();

  int res = populateMap(opt);
  TES_SERVER_STOP(*g_tesServer);
  return res;
}
