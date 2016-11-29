#==============================================================================
# Helpers for 3es CMake configuration.
#==============================================================================
cmake_minimum_required(VERSION 3.0)
cmake_policy(SET CMP0020 OLD)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED TRUE)

include(CMakeParseArguments)

set(CMAKE_DEBUG_POSTFIX "d")
#set(CMAKE_MINSIZEREL_POSTFIX "m")
#set(CMAKE_RELEASE_POSTFIX "")
#set(CMAKE_RELWITHDEBINFO_POSTFIX "i")

if(TES_SINGLE_OUTPUT_DIR)
  set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib")
  set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
  set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
  foreach(CONF Debug;Release;MinSizeRel;RelWithDebInfo)
    string(TOUPPER ${CONF} CONFU)
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${CONFU} "${CMAKE_BINARY_DIR}/lib")
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_${CONFU} "${CMAKE_BINARY_DIR}/bin")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_${CONFU} "${CMAKE_BINARY_DIR}/bin")
  endforeach(CONF)
endif(TES_SINGLE_OUTPUT_DIR)

if(APPLE)
  add_compile_options("-Wno-logical-op-parentheses")
endif(APPLE)

#------------------------------------------------------------------------------
# Define TES_IS_BIG_ENDIAN if not already defined. Set to 1 if the current
# platform is big endian, zero otherwise.
#------------------------------------------------------------------------------
macro(tes_endian)
  if(NOT DEFINED TES_IS_BIG_ENDIAN)
    include(TestBigEndian)
    TEST_BIG_ENDIAN(IS_BIG_ENDIAN)
    if(IS_BIG_ENDIAN)
      set(TES_IS_BIG_ENDIAN 1 CACHE INTERNAL "Big endian target?")
    else(IS_BIG_ENDIAN)
      set(TES_IS_BIG_ENDIAN 0 CACHE INTERNAL "Big endian target?")
    endif(IS_BIG_ENDIAN)
  endif(NOT DEFINED TES_IS_BIG_ENDIAN)
endmacro(tes_endian)

#------------------------------------------------------------------------------
# Define TES_32 and TES_64 according to pointer size.
# - TES_32 set if 32-bit pointers, and TES_64 cleared
# - TES_64 set if 64-bit pointers, and TES_32 cleared
#------------------------------------------------------------------------------
macro(tes_32_64)
  if("${CMAKE_SIZEOF_VOID_P}" EQUAL "8")
    set(TES_32 0)
    set(TES_64 1)
  else("${CMAKE_SIZEOF_VOID_P}" EQUAL "8")
    set(TES_32 1)
    set(TES_64 0)
  endif("${CMAKE_SIZEOF_VOID_P}" EQUAL "8")
endmacro(tes_32_64)

#------------------------------------------------------------------------------
# Find and include ZLib if not disabled.
#
# Set TES_ZLIB if active and found.
#------------------------------------------------------------------------------
macro(tes_zlib)
  set(TES_ZLIB 0)
  if(NOT DEFINED TES_ZLIB_OFF OR NOT TES_ZLIB_OFF)
    find_package(ZLIB)
    if(ZLIB_FOUND)
      set(TES_ZLIB 1)
      include_directories("${ZLIB_INCLUDE_DIRS}")
    endif(ZLIB_FOUND)
  endif(NOT DEFINED TES_ZLIB_OFF OR NOT TES_ZLIB_OFF)
endmacro(tes_zlib)

#------------------------------------------------------------------------------
# tes_configure_file(target config_header [SHARED] VAR export_var)
#
# Setup configuration header for TARGET.
# The config_header is provided without the .in suffix. E.g., 3es-core.h rather than
# 3es-core.h.in
#
# Add SHARED when configuring a shared library.
#
# Add VAR export_var to export the full configured header file path to export_var
#
# The file 3es-config.h.in is provided as a sample base header to configure.
#------------------------------------------------------------------------------
function(tes_config_file TARGET CONFIG_HEADER)
  CMAKE_PARSE_ARGUMENTS(CONFIG_FILE "SHARED" "VAR" "" ${ARGN})

  string(REGEX REPLACE "[- =]" "_" TARGET_IDENTIFIER ${TARGET})
  string(REGEX REPLACE "^([0-9])(.*)$" "_\\1\\2" TARGET_IDENTIFIER ${TARGET_IDENTIFIER})
  set(TARGET_SHARED 0)
  if (CONFIG_FILE_SHARED)
    set(TARGET_SHARED 1)
  endif(CONFIG_FILE_SHARED)
  
  # Overwrite PROJECT name to remove illegal characters.
  string(TOUPPER ${TARGET_IDENTIFIER} TARGET_HEADER_GUARD)
  configure_file("${CONFIG_HEADER}.in" "${CMAKE_CURRENT_BINARY_DIR}/${CONFIG_HEADER}")
  if(CONFIG_FILE_VAR)
    set(${CONFIG_FILE_VAR} "${CMAKE_CURRENT_BINARY_DIR}/${CONFIG_HEADER}" PARENT_SCOPE)
  endif(CONFIG_FILE_VAR)
endfunction(tes_config_file)

#------------------------------------------------------------------------------
# tes_include_target(target [target2 ...])
# Add the include directories of target using include_directories().
#------------------------------------------------------------------------------
macro(tes_include_target)
  foreach(TARGET ${ARGN})
    if(TARGET ${TARGET})
      get_target_property(_inc_dirs ${TARGET} INCLUDE_DIRECTORIES)
      if(_inc_dirs)
        include_directories("${_inc_dirs}")
      endif(_inc_dirs)
    endif(TARGET ${TARGET})
  endforeach(TARGET)
endmacro(tes_include_target)

#------------------------------------------------------------------------------
# tes_executable_suffix(target)
#
# Add CMAKE_<CONFIG>_POSTFIX to an executable target.
#
# CMake only adds POSTFIX items to library targets. This function can be used
# to modify an executable target's properties to include the current POSTFIX
# values.
#
# Ignored if TARGET is not a target, not an executable. Also does not
# modify OUTPUT_NAME_<CONFIG> property on target if already set.
#------------------------------------------------------------------------------
function(tes_executable_suffix TARGET)
  if(TARGET ${TARGET})
    get_target_property(TARGET_TYPE ${TARGET} TYPE)
    if(TARGET_TYPE STREQUAL "EXECUTABLE")
      if(CMAKE_BUILD_TYPE)
        # Single build config setup.
        get_target_property(TARGET_NAME ${TARGET} OUTPUT_NAME)
        if(NOT TARGET_NAME)
          string(TOUPPER ${CONFIG_NAME} CONFIGUP)
          set_property(TARGET ${TARGET} PROPERTY OUTPUT_NAME ${TARGET}${CMAKE_${CONFIGUP}_POSTFIX})
        endif(NOT TARGET_NAME)
      else(CMAKE_BUILD_TYPE)
        # Muti-build-config setup.
        foreach(CONFIG_NAME ${CMAKE_CONFIGURATION_TYPES})
          string(TOUPPER ${CONFIG_NAME} CONFIGUP)
          get_target_property(TARGET_NAME ${TARGET} OUTPUT_NAME_${CONFIGUP})
          if(NOT TARGET_NAME)
            set_property(TARGET ${TARGET} PROPERTY OUTPUT_NAME_${CONFIGUP} ${TARGET}${CMAKE_${CONFIGUP}_POSTFIX})
          endif(NOT TARGET_NAME)
        endforeach(CONFIG_NAME)
      endif(CMAKE_BUILD_TYPE)
    endif(TARGET_TYPE STREQUAL "EXECUTABLE")
  endif(TARGET ${TARGET})
endfunction(tes_executable_suffix)

#===============================================================================
# Filters a list of files to extract a list of directories containing those
# files. The result is written to ${OUT_VAR}.
#
# Usage:
#   tes_extract_directories(<out-var> file1 file2 ... fileN)
#===============================================================================
function(tes_extract_directories OUT_VAR)
  set(_dirs)
  foreach(_file ${ARGN})
    get_filename_component(_dir "${_file}" PATH)
    file(TO_CMAKE_PATH "${_dir}" _dir)
    list(APPEND _dirs "${_dir}")
  endforeach(_file)

  # Remove duplicates
  list(REMOVE_DUPLICATES _dirs)
  set(${OUT_VAR} "${_dirs}" PARENT_SCOPE)
endfunction(tes_extract_directories)

#------------------------------------------------------------------------------
# group_source_by_dir(file1 file2 ... fileN)
# Creates source filters to group files by subdirectory. This affects IDE
# environments such as Visual Studio.
#------------------------------------------------------------------------------
function(tes_group_source_by_dir)
  if (tes_group_source_by_dir_DBG)
    message("Files: ${ARGN}")
  endif (tes_group_source_by_dir_DBG)
  tes_extract_directories(_dirs "${ARGN}")

  if(_dirs)
    # Build source groups for IDE environments such as Visual Studio.
    foreach(_dir ${_dirs})
      if (tes_group_source_by_dir_DBG)
        message("_dir: ${_dir}")
      endif (tes_group_source_by_dir_DBG)
      if(_dir STREQUAL .)
        if (tes_group_source_by_dir_DBG)
          message("Group is .")
        endif (tes_group_source_by_dir_DBG)
        set(_group_re "${CMAKE_CURRENT_LIST_DIR}/[^/]+")
        set(_groupFilter)
      elseif(_dir STREQUAL "${CMAKE_CURRENT_BINARY_DIR}")
        set(_group_re ${_dir}/[^/]+)
        string(REPLACE "${CMAKE_CURRENT_BINARY_DIR}" "" _groupFilter "${_dir}")
        string(REPLACE "/" "\\" _groupFilter "${_groupFilter}")
        set(_groupFilter "${_groupFilter}")
      else(_dir STREQUAL .)
        set(_group_re ${_dir}/[^/]+)
        string(REPLACE "${CMAKE_CURRENT_LIST_DIR}" "" _groupFilter "${_dir}")
        string(REPLACE "/" "\\" _groupFilter "${_groupFilter}")
        set(_groupFilter "${_groupFilter}")
      endif(_dir STREQUAL .)

      # Strip "\.\" pattern
      string(REPLACE "\\.\\" "\\" _groupFilter "${_groupFilter}")
      # Ensure leading backslash.
      if(NOT "${_groupFilter}" MATCHES "^\\\\.*")
        set(_groupFilter "\\${_groupFilter}")
      endif(NOT "${_groupFilter}" MATCHES "^\\\\.*")

      if (tes_group_source_by_dir_DBG)
        message("Source${_groupFilter} -> ${_group_re}$")
      endif (tes_group_source_by_dir_DBG)
      source_group("Source${_groupFilter}" ${_group_re}$)
    endforeach(_dir)
  endif(_dirs)
  # Catch all
  source_group("Source" .*)
endfunction(tes_group_source_by_dir)


#------------------------------------------------------------------------------
# Configure a build target to configure Doxygen documentation.
#------------------------------------------------------------------------------
function(tes_doxygen_target DOXYFILE)
  CMAKE_PARSE_ARGUMENTS(GDT "INSTALL" "OUTDIR" "" "${ARGN}")

  # Only generate for the top level CMAKE_LISTS file
  find_package(Doxygen QUIET)
  
  if(NOT DOXYGEN_FOUND)
    message("Doxygen not found. Skipping.")
    return()
  endif(NOT DOXYGEN_FOUND)

  if(NOT DEFINED DOXYGEN_INPUT_LIST OR NOT DOXYGEN_INPUT_LIST)
    message(SEND_ERROR "Nothing for doxygen to process for project 3es-doc.")
    return()
  endif(NOT DEFINED DOXYGEN_INPUT_LIST OR NOT DOXYGEN_INPUT_LIST)

  if(DOXYGEN_FOUND)
    set(DOXYGEN_HTML_OUTPUT doc)

    # Break up doxygen input dirs.
    set(DOXYGEN_DIRS ${DOXYGEN_INPUT_LIST})
    set(DOXYGEN_INPUT_LIST)
    foreach(dir ${DOXYGEN_DIRS})
      set(DOXYGEN_INPUT_LIST "${DOXYGEN_INPUT_LIST} \\\n  ${dir}")
    endforeach(dir)
    set(DOXYGEN_DIRS ${DOXYGEN_IMAGE_PATH})
    set(DOXYGEN_IMAGE_PATH)
    foreach(dir ${DOXYGEN_DIRS})
      set(DOXYGEN_IMAGE_PATH "${DOXYGEN_IMAGE_PATH} \\\n  ${dir}")
    endforeach(dir)
    set(DOXYGEN_DIRS ${DOXYGEN_EXAMPLE_PATH})
    set(DOXYGEN_EXAMPLE_PATH)
    foreach(dir ${DOXYGEN_DIRS})
      set(DOXYGEN_EXAMPLE_PATH "${DOXYGEN_EXAMPLE_PATH} \\\n  ${dir}")
    endforeach(dir)

    configure_file("${DOXYFILE}" "${CMAKE_CURRENT_BINARY_DIR}/Doxyfile")
    get_filename_component(DOXYFILE_PATH "${DOXYFILE}" ABSOLUTE)
    add_custom_target(3es-doc
                      "${DOXYGEN_EXECUTABLE}" "${CMAKE_CURRENT_BINARY_DIR}/Doxyfile"
                      DEPENDS
                        "${DOXYFILE_PATH}"
                        "${CMAKE_CURRENT_BINARY_DIR}/Doxyfile"
                      )
    # Restrict build configurations. Default to restricting to Release.
    foreach(CONFIG DEBUG;MINSIZEREL;RELWITHDEBINFO)
      set_target_properties(${DOXYGEN_TARGET} PROPERTIES EXCLUDE_FROM_DEFAULT_BUILD_${CONFIG} True)
    endforeach(CONFIG)

    install(DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/doc DESTINATION doc ${ARG_COMPONENT} ${ARG_COMPONENT_NAME} CONFIGURATIONS Release)
  endif(DOXYGEN_FOUND)
endfunction(tes_doxygen_target)
