project(3rd-eye-scene)

file(TO_CMAKE_PATH "$ENV{CMAKE_MODULE_PATH}" CMK_MOD_DIR)
set(CMAKE_MODULE_PATH "${CMAKE_MODULE_PATH};${CMAKE_CURRENT_LIST_DIR}/cmake;${CMK_MOD_DIR}")

file(TO_CMAKE_PATH "$ENV{CMAKE_PREFIX_PATH}" CMK_PREFIX_DIR)
set(CMAKE_PREFIX_PATH "${CMAKE_PREFIX_PATH};${CMK_PREFIX_DIR}")

cmake_minimum_required(VERSION 3.0)
include(3es)

option(TES_ZLIB_OFF "Disable ZLIB usage even if found? Intended for testing." OFF)
option(TES_SINGLE_OUTPUT_DIR "Collated binaries?" ON)
option(TES_DOXYGEN "Build Doxygen documentation if doxygen is available?" ON)

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# Enable PDB files in release builds for MSVC to facilitate debugging.
if(MSVC)
  set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /Zi")
  set(CMAKE_C_FLAGS_RELEASE "${CMAKE_C_FLAGS_RELEASE} /Zi")
  set(CMAKE_MODULE_LINKER_FLAGS_RELEASE "/debug ${CMAKE_MODULE_LINKER_FLAGS_RELEASE}")
  set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "/debug ${CMAKE_SHARED_LINKER_FLAGS_RELEASE}")
  set(CMAKE_EXE_LINKER_FLAGS_RELEASE "/debug ${CMAKE_EXE_LINKER_FLAGS_RELEASE}")
endif(MSVC)

add_subdirectory(3es-core)
add_subdirectory(examples)
add_subdirectory(test)

if(TES_DOXYGEN)
  list(APPEND DOXYGEN_IMAGE_PATH ${DOXYGEN_INPUT_LIST})
  list(APPEND DOXYGEN_IMAGE_PATH "3es-doc/images")
  list(APPEND DOXYGEN_IMAGE_PATH "3es-doc/images/ui")
  list(APPEND DOXYGEN_IMAGE_PATH "3es-doc/images/usecase")
  if(IS_DIRECTORY  "../3rdEyeScene/Assets/UI/Images")
    list(APPEND DOXYGEN_IMAGE_PATH "../3rdEyeScene/Assets/UI/Images")
  endif(IS_DIRECTORY  "../3rdEyeScene/Assets/UI/Images")
  tes_doxygen_target(cmake/doxyfile.in)
endif(TES_DOXYGEN)

include(InstallRequiredSystemLibraries)

#-------------------------------------------------------------------------------
# Marshalling DLLs to the build directory (not installation, Windows only)
#-------------------------------------------------------------------------------
# set(DEPENDENCY_PATHS)
# foreach(PPATH ${CMAKE_PREFIX_PATH})
#   list(APPEND DEPENDENCY_PATHS "${PPATH}")
#   list(APPEND DEPENDENCY_PATHS "${PPATH}/bin")
# endforeach(PPATH)
# marshal_exe_dependencies(3es-marshal 3est-server PATHS "" ${DEPENDENCY_PATHS})
