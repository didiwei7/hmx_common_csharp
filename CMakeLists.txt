cmake_minimum_required(VERSION 3.18)

project(all_csharp LANGUAGES CSharp)

add_subdirectory(plc_conn)

set_target_properties(plc_conn     PROPERTIES RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/bin)