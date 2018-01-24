find_package(GTest QUIET)

add_subdirectory(3est-bandwidth)
add_subdirectory(3est-primitive-server)
add_subdirectory(3est-server)

set_target_properties(3est-bandwidth PROPERTIES FOLDER test)
set_target_properties(3est-primitive-server PROPERTIES FOLDER test)
set_target_properties(3est-server PROPERTIES FOLDER test)

# Add unit tests
if(GTEST_FOUND)
  add_subdirectory(3est-shapes)
else(GTEST_FOUND)
  message("Missing GTest. Unit tests will not be built.")
endif(GTEST_FOUND)
