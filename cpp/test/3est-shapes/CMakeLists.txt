
set(SOURCES
  3est-shapes.cpp
)

add_executable(3est-shapes ${SOURCES})
set_target_properties(3est-shapes PROPERTIES DEBUG_POSTFIX ${CMAKE_DEBUG_POSTFIX})
target_link_libraries(3est-shapes 3es-core GTest::Main)
target_include_directories(3est-shapes
  PRIVATE
    $<TARGET_PROPERTY:3es-core,INCLUDE_DIRECTORIES>
    $<TARGET_PROPERTY:GTest::Main,INTERFACE_INCLUDE_DIRECTORIES>
)
source_group(TREE "${CMAKE_CURRENT_LIST_DIR}" PREFIX source FILES ${SOURCES})
