include(3es)

find_package(octomap)
if(NOT DEFINED OCTOMAP_INCLUDE_DIRS)
  message(STATUS "Octomap not found. Skipping 3es-occupancy example.")
  return()
endif(NOT DEFINED OCTOMAP_INCLUDE_DIRS)

find_package(LIBLAS)
find_package(LASZIP)

if(NOT LIBLAS_FOUND)
  message(STATUS "LIBLAS not found. Skipping 3es-occupancy example.")
endif(NOT LIBLAS_FOUND)

# Need to add boost link directories because of LIBLAS.
find_package(Boost COMPONENTS "thread")
link_directories("${Boost_LIBRARY_DIRS}")

tes_32_64()
tes_endian()

tes_config_file(3es-occupancy 3es-occupancy.h VAR CONFIG_HEADER SHARED)
include_directories("${CMAKE_CURRENT_BINARY_DIR}")
set(SOURCES "${CONFIG_HEADER}")

list(APPEND SOURCES
  3es-occupancy.h.in
  3es-occupancy.cpp
  occupancyloader.cpp
  occupancyloader.h
  occupancymesh.cpp
  occupancymesh.h
  p2p.h
)

include_directories(
  "${OCTOMAP_INCLUDE_DIRS}"
  "${LIBLAS_INCLUDE_DIR}"
  "${LASZIP_INCLUDE_DIR}"
  )

tes_include_target(3es-core)
add_executable(3es-occupancy ${SOURCES})
target_link_libraries(3es-occupancy 3es-core)
target_link_libraries(3es-occupancy
  ${OCTOMAP_LIBRARIES}
  ${LIBLAS_LIBRARIES}
  ${LASZIP_LIBRARIES}
)

tes_group_source_by_dir(${SOURCES})
