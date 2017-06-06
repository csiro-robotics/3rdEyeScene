project(3rd-eye-scene)

file(TO_CMAKE_PATH "$ENV{CMAKE_MODULE_PATH}" CMK_MOD_DIR)
set(CMAKE_MODULE_PATH "${CMAKE_MODULE_PATH};${CMAKE_CURRENT_LIST_DIR}/cmake;${CMK_MOD_DIR}")

file(TO_CMAKE_PATH "$ENV{CMAKE_PREFIX_PATH}" CMK_PREFIX_DIR)
set(CMAKE_PREFIX_PATH "${CMAKE_PREFIX_PATH};${CMK_PREFIX_DIR}")

cmake_minimum_required(VERSION 3.0)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED TRUE)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_Debug "${CMAKE_BINARY_DIR}/Debug")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_Release "${CMAKE_BINARY_DIR}/Release")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RelWithDebInfo "${CMAKE_BINARY_DIR}/RelWithDebInfo")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_MinSizeRel "${CMAKE_BINARY_DIR}/MinSizeRel")

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

if(CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
  add_compile_options("-Wno-logical-op-parentheses")
endif(CMAKE_CXX_COMPILER_ID STREQUAL "Clang")

add_subdirectory(3es-core)
add_subdirectory(examples)
add_subdirectory(test)

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
