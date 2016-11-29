
find_package(ZLIB)
if(ZLIB_FOUND)
  list(APPEND PACKAGES ZLIB)
  set(TES_ZLIB 1)
endif(ZLIB_FOUND)

add_subdirectory(3est-bandwidth)
add_subdirectory(3est-server)

set_target_properties(3est-bandwidth PROPERTIES FOLDER test)
set_target_properties(3est-server PROPERTIES FOLDER test)
