# CMake package documentation (https://cmake.org/cmake/help/v3.5/manual/cmake-packages.7.html) shows how to generate
# cmake package config files. While a <pacakge>-config.cmake file can be generated and used directly, the documentation
# recommends providing a wrapper such as this file. The purpose is to ensure other depdencies are found first and bound
# as required. That is, this is a good place to find external dependencies for the project libraries.
# https://cmake.org/cmake/help/v3.5/manual/cmake-packages.7.html#creating-a-package-configuration-file

# Find external dependencies here.
# find_package(LIBLAS)
# find_package(NABO)
#...

# Include the generated configuration file.
include("${CMAKE_CURRENT_LIST_DIR}/3es-config-targets.cmake")
