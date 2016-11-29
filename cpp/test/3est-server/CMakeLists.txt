include(3es)

tes_32_64()
tes_endian()

tes_config_file(3est-server 3es-server-test.h VAR CONFIG_HEADER SHARED)
include_directories("${CMAKE_CURRENT_BINARY_DIR}")
set(SOURCES "${CONFIG_HEADER}")

list(APPEND SOURCES
  3es-server-test.h.in
  3es-server-test.cpp
)

tes_include_target(3es-core)
add_executable(3est-server ${SOURCES})
target_link_libraries(3est-server 3es-core)

tes_group_source_by_dir(${SOURCES})
