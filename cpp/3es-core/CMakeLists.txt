set(TES_CORE_ONLY Off)
# Project setup if this is being built as a stand alone project.
if(NOT ${CMAKE_PROJECT_NAME})
  project(3es-core)

  cmake_minimum_required(VERSION 3.5)

  set(CMAKE_CXX_STANDARD 14)
  set(CMAKE_CXX_STANDARD_REQUIRED TRUE)

  set_property(GLOBAL PROPERTY USE_FOLDERS ON)

  if(CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
    target_compile_options(3es-core PRIVATE "-Wno-logical-op-parentheses")
  endif(CMAKE_CXX_COMPILER_ID STREQUAL "Clang")

  set(TES_CORE_ONLY On)
endif(NOT ${CMAKE_PROJECT_NAME})

option(TES_ASSERT_ENABLE_DEBUG "Enable assertions in debug builds." ON)
option(TES_ASSERT_ENABLE_RELEASE "Enable assertions in release builds." ON)

option(TES_ZLIB_OFF "Disable ZLIB usage even if found? Intended for testing." OFF)
set(TES_SOCKETS "custom" CACHE STRING "Select the TCP socket implementation. The 'custom' implementaiton is based on Berkley sockets or Winsock2.")
set_property(CACHE TES_SOCKETS PROPERTY STRINGS custom Qt)

message(STATUS "Using ${TES_SOCKETS} sockets")

set(TES_ZLIB 0)
if(NOT DEFINED TES_ZLIB_OFF OR NOT TES_ZLIB_OFF)
  find_package(ZLIB)
  if(ZLIB_FOUND)
    set(TES_ZLIB 1)
  endif(ZLIB_FOUND)
endif(NOT DEFINED TES_ZLIB_OFF OR NOT TES_ZLIB_OFF)

# Set TES_32 and TES_64 to 1 or 0 depending on CMAKE_SIZEOF_VOID_P denoting 32- or 64-bit pointers.
math(EXPR TES_32 "(${CMAKE_SIZEOF_VOID_P} & 4) >> 2")
math(EXPR TES_64 "(${CMAKE_SIZEOF_VOID_P} & 8) >> 3")

if(NOT DEFINED TES_IS_BIG_ENDIAN)
  include(TestBigEndian)
  TEST_BIG_ENDIAN(IS_BIG_ENDIAN)
  if(IS_BIG_ENDIAN)
    set(TES_IS_BIG_ENDIAN 1 CACHE INTERNAL "Big endian target?")
  else(IS_BIG_ENDIAN)
    set(TES_IS_BIG_ENDIAN 0 CACHE INTERNAL "Big endian target?")
  endif(IS_BIG_ENDIAN)
endif(NOT DEFINED TES_IS_BIG_ENDIAN)

list(APPEND DOXYGEN_INPUT_LIST "${CMAKE_CURRENT_LIST_DIR}" "${CMAKE_CURRENT_LIST_DIR}/tcp")
if(TES_SOCKETS STREQUAL "custom")
  list(APPEND DOXYGEN_INPUT_LIST "${CMAKE_CURRENT_LIST_DIR}/tcp")
elseif(TES_SOCKETS STREQUAL "Qt")
  find_package(Qt5Network)
else()
  message(FATAL_ERROR "Unknown socket implementation specificed: ${TES_SOCKETS}")
endif()

configure_file(3es-core.in.h "${CMAKE_CURRENT_BINARY_DIR}/3es-core.h")
set(PUBLIC_HEADERS "${CMAKE_CURRENT_BINARY_DIR}/3es-core.h")

include(sourcelist.cmake)

set(DOXYGEN_INPUT_LIST "${DOXYGEN_INPUT_LIST}" PARENT_SCOPE)

add_library(3es-core ${PUBLIC_HEADERS} ${SOURCES} ${PRIVATE_HEADERS} ${PRIVATE_SOURCES})

# Library setup.
include(GenerateExportHeader)
generate_export_header(3es-core
      EXPORT_MACRO_NAME _3es_coreAPI
      NO_EXPORT_MACRO_NAME _3es_coreHIDDEN
      EXPORT_FILE_NAME 3es-core-export.h
      STATIC_DEFINE TES_STATIC
      )

# set_target_properties(3es-core PROPERTIES FOLDER core)

# Setup target versioning.
set_target_properties(3es-core PROPERTIES
  # Build version.
  VERSION ${TES_VERSION}
  # API version.
  SOVERSION 0 # API version.
  # Compatibility versioning. Referenced via the COMPATIBLE_INTERFACE_STRING property.
  INTERFACE_3escore_MAJOR_VERSION 0
)
# Help enforce version compability by ensuring all link dependencies expect the same INTERFACE_3escore_MAJOR_VERSION
set_property(TARGET 3es-core APPEND PROPERTY COMPATIBLE_INTERFACE_STRING INTERFACE_3escore_MAJOR_VERSION)

# Include directories
target_include_directories(3es-core
  PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_BINARY_DIR}>
    $<BUILD_INTERFACE:${CMAKE_CURRENT_LIST_DIR}>
    $<INSTALL_INTERFACE:include>
)

# Need to explicitly define some compile flags because the target name starts with a number.
if(BUILD_SHARED_LIBS)
  target_compile_definitions(3es-core PRIVATE -D_3es_core_EXPORTS)
else(BUILD_SHARED_LIBS)
  target_compile_definitions(3es-core PUBLIC -DTES_STATIC)
endif(BUILD_SHARED_LIBS)

if(ZLIB_FOUND AND NOT TES_ZLIB_OFF)
  target_include_directories(3es-core PRIVATE "${ZLIB_INCLUDE_DIRS}")
  target_link_libraries(3es-core ${ZLIB_LIBRARIES})
endif(ZLIB_FOUND AND NOT TES_ZLIB_OFF)

if(MSVC)
  # Windows build requires Winsock2.
  target_link_libraries(3es-core Ws2_32.lib)
endif(MSVC)

if(TES_SOCKETS STREQUAL "Qt")
  target_link_libraries(3es-core Qt5::Network)
  target_include_directories(3es-core PRIVATE "${Qt5Network_INCLUDE_DIRS}")
endif()

# Setup installation.
set(CONFIG_SUFFIX -config-targets)
if(TES_CORE_ONLY)
  set(CONFIG_SUFFIX -config)
endif(TES_CORE_ONLY)

# Binary installation
install(TARGETS 3es-core EXPORT ${CMAKE_PROJECT_NAME}${CONFIG_SUFFIX}
  LIBRARY DESTINATION lib
  ARCHIVE DESTINATION lib
  RUNTIME DESTINATION bin
  INCLUDES DESTINATION include
)

# Install PDB files (MSVC) in part to avoid unsupressable linker warnings.
if(BUILD_SHARED_LIBS)
  install(FILES $<TARGET_PDB_FILE:3es-core> DESTINATION bin OPTIONAL)
endif(BUILD_SHARED_LIBS)

# Header installation
install(FILES ${PUBLIC_HEADERS} DESTINATION include/3es-core COMPONENT Devel)

export(EXPORT ${CMAKE_PROJECT_NAME}${CONFIG_SUFFIX}
  FILE "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}${CONFIG_SUFFIX}.cmake"
  NAMESPACE 3es::
)
install(EXPORT ${CMAKE_PROJECT_NAME}${CONFIG_SUFFIX}
  FILE ${CMAKE_PROJECT_NAME}${CONFIG_SUFFIX}.cmake
  NAMESPACE 3es::
  DESTINATION ${ConfigPackageLocation}
)

source_group("source" FILES "${CMAKE_CURRENT_BINARY_DIR}/3es-core.h")
source_group("source" REGULAR_EXPRESSION ".*$")
source_group("source\\private" REGULAR_EXPRESSION "/private/.*$")
source_group("source\\qt" REGULAR_EXPRESSION "/qt/.*$")
source_group("source\\shapes" REGULAR_EXPRESSION "/shapes/.*$")
source_group("source\\tcp" REGULAR_EXPRESSION "/tcp/.*$")
