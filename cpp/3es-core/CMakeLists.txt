
include(3es)

tes_zlib()
tes_32_64()
tes_endian()

tes_config_file(3es-core 3es-core.h VAR CONFIG_HEADER SHARED)
include_directories("${CMAKE_CURRENT_BINARY_DIR}" .)
set(PUBLIC_HEADERS "${CONFIG_HEADER}")

set(TES_SOCKETS "custom" CACHE STRING "Select the TCP socket implementation. The 'custom' implementaiton is based on Berkley sockets or Winsock2.")
set_property(CACHE TES_SOCKETS PROPERTY STRINGS custom POCO Qt)

message(STATUS "Using ${TES_SOCKETS} sockets")

list(APPEND DOXYGEN_INPUT_LIST "${CMAKE_CURRENT_LIST_DIR}" "${CMAKE_CURRENT_LIST_DIR}/tcp")
if(TES_SOCKETS STREQUAL "custom")
  list(APPEND DOXYGEN_INPUT_LIST "${CMAKE_CURRENT_LIST_DIR}/tcp")
# elseif(TES_SOCKETS STREQUAL "asio")
#   # Try resolve ASIO socket library.
#   find_package(ASIO QUIET)
#   if(NOT ASIO_FOUND)
#     # Failed. Include on local path.
#     set(ASIO_INCLUDE_DIR "${CMAKE_CURRENT_LIST_DIR}/../asio/include")
#     set(TES_ASIO 1)
#   endif(NOT ASIO_FOUND)
#   include_directories("${ASIO_INCLUDE_DIR}")
#   message(STATUS "ASIO_INCLUDE_DIR: ${ASIO_INCLUDE_DIR}")
elseif(TES_SOCKETS STREQUAL "POCO")
  find_package(Poco REQUIRED Net)
  include_directories(${Poco_INCLUDE_DIRS})
elseif(TES_SOCKETS STREQUAL "Qt")
  find_package(Qt5Network)
  include_directories(${Qt5Network_INCLUDE_DIRS})
else()
  message(FATAL_ERROR "Unknown socket implementation specificed: ${TES_SOCKETS}")
endif()

include(sourcelist.cmake)

set(DOXYGEN_INPUT_LIST "${DOXYGEN_INPUT_LIST}" PARENT_SCOPE)

add_library(3es-core SHARED ${PUBLIC_HEADERS} ${SOURCES} ${PRIVATE_HEADERS} ${PRIVATE_SOURCES})
tes_executable_suffix(3es-core)

if(ZLIB_FOUND)
  target_link_libraries(3es-core ${ZLIB_LIBRARIES})
endif(ZLIB_FOUND)

if(MSVC)
  # Windows build requires Winsock2.
  set(DEP_LIBS Ws2_32.lib)
  target_link_libraries(3es-core ${DEP_LIBS})
endif(MSVC)

if(TES_SOCKETS STREQUAL "POCO")
  target_link_libraries(3es-core ${Poco_LIBRARIES})
elseif(TES_SOCKETS STREQUAL "Qt")
  target_link_libraries(3es-core Qt5::Network)
endif()


tes_group_source_by_dir(${PUBLIC_HEADERS} ${SOURCES} ${PRIVATE_HEADERS} ${PRIVATE_SOURCES})

install(TARGETS 3es-core EXPORT 3es-core
        RUNTIME DESTINATION bin
        LIBRARY DESTINATION lib
        ARCHIVE DESTINATION lib
        INCLUDES DESTINATION include
        )
install(FILES ${PUBLIC_HEADERS} DESTINATION include)
install(EXPORT 3es-core DESTINATION lib/3es-core)
# FIXME: use BundleUtilities to copy the dependencies (ZLIB, Qt, etc).
# set(APPS 3es-core)
# set(DIRS)

# foreach(LIB ${ZLIB_LIBRARIES})
#   get_filename_component(LIBPATH "${ZLIB_LIBRARIES}" DIRECTORY)
#   list(APPEND DIRS "${LIBPATH}")
# endforeach(LIB)
# INSTALL(CODE "
# include(BundleUtilities)
# fixup_bundle(\"${APPS}\"   \"\"   \"${DIRS}\")
# ")
