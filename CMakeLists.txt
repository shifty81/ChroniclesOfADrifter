cmake_minimum_required(VERSION 3.20)
project(ChroniclesEngine VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Output directories
# Ensure consistent output paths across all configurations (no Release/Debug subfolder)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

# For multi-config generators (Visual Studio, Xcode), place all configs in the same directory
foreach(CONFIG ${CMAKE_CONFIGURATION_TYPES})
    string(TOUPPER ${CONFIG} CONFIG_UPPER)
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_${CONFIG_UPPER} ${CMAKE_BINARY_DIR}/bin)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_${CONFIG_UPPER} ${CMAKE_BINARY_DIR}/lib)
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${CONFIG_UPPER} ${CMAKE_BINARY_DIR}/lib)
endforeach()

# Engine library
add_library(ChroniclesEngine SHARED
    src/Engine/ChroniclesEngine.cpp
    src/Engine/ChroniclesEngine.h
)

target_compile_definitions(ChroniclesEngine PRIVATE ENGINE_EXPORTS)

target_include_directories(ChroniclesEngine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/src/Engine
)

# Platform-specific settings
if(WIN32)
    target_compile_definitions(ChroniclesEngine PRIVATE 
        _WIN32
        UNICODE
        _UNICODE
    )
endif()

# Compiler warnings
if(MSVC)
    target_compile_options(ChroniclesEngine PRIVATE /W4)
else()
    target_compile_options(ChroniclesEngine PRIVATE -Wall -Wextra -Wpedantic)
endif()

# Installation rules
install(TARGETS ChroniclesEngine
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES src/Engine/ChroniclesEngine.h
    DESTINATION include
)
