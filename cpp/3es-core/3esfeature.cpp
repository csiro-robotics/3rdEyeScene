//
// author: Kazys Stepanas
//
//
#include "3esfeature.h"

namespace tes
{
  uint64_t featureFlag(Feature feature)
  {
    return (1ull << feature);
  }


  Feature featureForFlag(uint64_t flag)
  {
    // Simple solution for now.
    uint64_t bit = 1u;
    for (int i = 0; i < TFeatureEnd; ++i, bit = bit << 1)
    {
      if (flag & bit)
      {
        return (Feature)i;
      }
    }

    return TFeatureInvalid;
  }

  bool checkFeature(Feature feature)
  {
    return checkFeatureFlag(featureFlag(feature));
  }


  bool checkFeatureFlag(uint64_t featureFlag)
  {
    switch (featureFlag)
    {
      case (1ull << TFeatureCompression):
#ifdef TES_ZLIB
        return true;
#endif // TES_ZLIB
        break;

      default:
        break;
    }

    return false;
  }


  bool checkFeatures(uint64_t featureFlags)
  {
    uint64_t bit = 1u;
    for (int i = 0; i < TFeatureEnd && featureFlags != 0ull; ++i, bit = bit << 1)
    {
      if (featureFlags & bit)
      {
        if (!checkFeatureFlag(bit))
        {
          return false;
        }
      }
      featureFlags &= ~bit;
    }

    return true;
  }
}
