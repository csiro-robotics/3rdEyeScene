
add_subdirectory(3es-sphere-view)
set_target_properties(3es-sphere-view PROPERTIES FOLDER examples)

add_subdirectory(3es-occupancy)
if(TARGET 3es-occupancy)
  set_target_properties(3es-occupancy PROPERTIES FOLDER examples)
endif(TARGET 3es-occupancy)
