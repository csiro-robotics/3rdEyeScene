include(3es)

tes_32_64()
tes_endian()

tes_config_file(3est-bandwidth 3est-bandwidth.h VAR CONFIG_HEADER SHARED)
include_directories("${CMAKE_CURRENT_BINARY_DIR}")
set(SOURCES "${CONFIG_HEADER}")

list(APPEND SOURCES
  3est-bandwidth.h.in
  3est-bandwidth.cpp
)

tes_include_target(3es-core)
add_executable(3est-bandwidth ${SOURCES})
target_link_libraries(3est-bandwidth 3es-core)

tes_group_source_by_dir(${SOURCES})
