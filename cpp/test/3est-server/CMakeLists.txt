
get_target_property(3ESCORE_INCLUDE_DIRS 3es-core INCLUDE_DIRECTORIES)
add_executable(3est-server 3es-server-test.cpp)
target_link_libraries(3est-server 3es-core)
target_include_directories(3est-server PRIVATE "${3ESCORE_INCLUDE_DIRS}")
source_group("source" REGULAR_EXPRESSION ".*$")
