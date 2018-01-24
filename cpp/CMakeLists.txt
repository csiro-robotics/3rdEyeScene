# Project setup and minimum cmake version.
cmake_minimum_required(VERSION 3.5)

# Setup project details.
project(3es)
set(TES_VERSION 0.7)
set(ConfigPackageLocation lib/cmake/${CMAKE_PROJECT_NAME})

# C++ standards setup.
set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED TRUE)

# Add cmake directory to the modules path.
set(CMAKE_MODULE_PATH "${CMAKE_MODULE_PATH};${CMAKE_CURRENT_LIST_DIR}/cmake")

# Ensure debug libraries are built with different named to release builds. This is to address issues such as MSVC
# having different debug and release runtime libraries. For a well setup API, one which hides resource allocation and
# ensures symmetrical deallcation occurs from the same allocator, this won't be a problem, but the consistency is
# useful.
set(CMAKE_DEBUG_POSTFIX "d")
# Marshall all binaries to the same directory. This is expecially useful on Windows when trying to run exectuables from
# this project with shared libraries. Otherwise those shared libraries aren't on the path. Note that other package
# binaries should be on the path already.
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")

# Add project level options here.
option(TES_BUILD_DOXYGEN "Build doxgen documentation?" OFF)
option(TES_BUILD_EXAMPLES "Build examples?" OFF)
option(TES_BUILD_TESTS "Build unit tests?" OFF)

# Allow the use of folders to group targets in supporting environments.
# For example Visual Studio solution folders.
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# Manage warnings, disabling ones we want to ignore.
# message("CMAKE_CXX_COMPILER_ID: ${CMAKE_CXX_COMPILER_ID}")
if(CMAKE_CXX_COMPILER_ID MATCHES ".*Clang.*")
  add_compile_options("-Wno-logical-op-parentheses")
endif(CMAKE_CXX_COMPILER_ID MATCHES ".*Clang.*")

# For Visual Studio, force PDB based debug information even in release builds.
# This has a small impact on the size of the of the DLLs, but provides debug information for release mode crashes.
# The PDBs can be kept for debugging specific releases, but do not need to be shipped as part of the runtime, unless
# shipping an SDK. The last point is to address MSVC linked warnings which are impossible to suppress without providing
# PDB files.
if(MSVC)
  set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /Zi")
  set(CMAKE_C_FLAGS_RELEASE "${CMAKE_C_FLAGS_RELEASE} /Zi")
  set(CMAKE_MODULE_LINKER_FLAGS_RELEASE "/debug ${CMAKE_MODULE_LINKER_FLAGS_RELEASE}")
  set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "/debug ${CMAKE_SHARED_LINKER_FLAGS_RELEASE}")
  set(CMAKE_EXE_LINKER_FLAGS_RELEASE "/debug ${CMAKE_EXE_LINKER_FLAGS_RELEASE}")
endif(MSVC)

# Include sub projects.
add_subdirectory(3es-core)
if(TES_BUILD_EXAMPLES)
  add_subdirectory(examples)
endif(TES_BUILD_EXAMPLES)

if(TES_BUILD_TESTS)
  # We can enable testing here and/or in the subdirectory, but doing it here allows us to run CTest from the build root.
  # To run the tests, we execute:
  #   CTest -C [Debug|Release|RelWithDebInfo|MinSizeRel] --output-on-failure
  # CTest normally shows only a very terse test ouput, but we make sure failed tests show all output by adding
  #   --output-on-failure
  # The full test output is always available in:
  #   <build>/Testing/Temporary/LastTest.log
  # Note: not all projects under tests actually add something for CTest.
  enable_testing()
  add_subdirectory(test)
endif(TES_BUILD_TESTS)

# Package configuration script configuration.
# Setup import scripts.
include(CMakePackageConfigHelpers)
write_basic_package_version_file(
  "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}-config-version.cmake"
  VERSION ${TES_VERSION}
  COMPATIBILITY AnyNewerVersion
)

# Installation of the package configuration file.
configure_file(cmake/${CMAKE_PROJECT_NAME}-config.cmake "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}-config.cmake" COPYONLY)
install(FILES cmake/${CMAKE_PROJECT_NAME}-config.cmake "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}-config-version.cmake"
  DESTINATION ${ConfigPackageLocation}
  COMPONENT Devel)

# Install MSVC runtime libraries. This will also affect the CPack installation.
include(InstallRequiredSystemLibraries)

# Doxygen setup.
if(TES_BUILD_DOXYGEN)
  # Include Doxygen helper functions. This also finds the Doxygen package.
  include(cmake/doxygen.cmake)

  if(DOXYGEN_FOUND)
    # Create a target to build the documentation.
    # Here we also setup various documentation variables passed through to the doxyfile configuration.
    # Each named argument below describes the Doxygen variable it sets.
    doxygen_create(
      # DOXYFILE cmake/doxyfile.in  # Doxyfile to configure.
      PROJECT ${CMAKE_PROJECT_NAME} # PROJECT_NAME
      VERSION ${TES_VERSION}        # PROJECT_NUMBER
      BRIEF "3rd Eye Scene documentation" # PROJECT_BRIEF
      OUTPUT_DIR html               # HTML_OUPTUT
      # CSS <style>.css             # HTML_STYLESHEET
      PUBLISHER "CSIRO"             # DOCSET_PUBLISHER_NAME
      PUBLISHER_ID au.csiro         # DOCSET_PUBLISHER_ID
      PROJECT_ID au.csiro.3es       # DOCSET_BUNDLE_ID, QHP_NAMESPACE, ECLIPSE_DOC_ID
      PATHS                         # INPUT (RECURSIVE is on)
        3es-doc  # Must come first.
        3es-core
      EXCLUDE_PATHS                 # EXCLUDE
        3es-core/private
      # Where to find source code examples.
      # EXAMPLE_PATHS <paths>        # EXAMPLE_PATH
      # Where to find images.
      IMAGE_PATHS                    # IMAGE_PATH
        3es-doc/images
        ../3rdEyeScene/Assets/UI/Images
      )

    # Setup installation of the generated documentation: source, destination.
    doxygen_install(${CMAKE_CURRENT_BINARY_DIR}/html 3es)
  endif(DOXYGEN_FOUND)
endif(TES_BUILD_DOXYGEN)
