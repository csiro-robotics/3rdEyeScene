include(3es)

tes_32_64()
tes_endian()

tes_config_file(3es-sphere-view 3es-sphere-view.h VAR CONFIG_HEADER SHARED)
include_directories("${CMAKE_CURRENT_BINARY_DIR}")
set(SOURCES "${CONFIG_HEADER}")

list(APPEND SOURCES
  3es-sphere-view.h.in
  3es-sphere-view.cpp
)

tes_include_target(3es-core)
add_executable(3es-sphere-view ${SOURCES})
target_link_libraries(3es-sphere-view 3es-core)

tes_group_source_by_dir(${SOURCES})
