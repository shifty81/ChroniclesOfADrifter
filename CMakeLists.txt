cmake_minimum_required(VERSION 3.20)
project(ChroniclesEngine VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Find SDL2
find_package(SDL2 REQUIRED)
include_directories(${SDL2_INCLUDE_DIRS})

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

# Source files
set(ENGINE_SOURCES
    src/Engine/ChroniclesEngine.cpp
    src/Engine/ChroniclesEngine.h
    src/Engine/IRenderer.h
    src/Engine/SDL2Renderer.cpp
    src/Engine/SDL2Renderer.h
)

# Add DirectX renderers on Windows
if(WIN32)
    list(APPEND ENGINE_SOURCES
        src/Engine/D3D11Renderer.cpp
        src/Engine/D3D11Renderer.h
        src/Engine/D3D12Renderer.cpp
        src/Engine/D3D12Renderer.h
    )
endif()

# Engine library
add_library(ChroniclesEngine SHARED ${ENGINE_SOURCES})

target_compile_definitions(ChroniclesEngine PRIVATE ENGINE_EXPORTS)

target_include_directories(ChroniclesEngine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/src/Engine
)

# Link SDL2
target_link_libraries(ChroniclesEngine PRIVATE ${SDL2_LIBRARIES})

# Platform-specific settings
if(WIN32)
    target_compile_definitions(ChroniclesEngine PRIVATE 
        _WIN32
        UNICODE
        _UNICODE
    )
    
    # Link DirectX libraries on Windows
    target_link_libraries(ChroniclesEngine PRIVATE
        d3d11.lib
        d3d12.lib
        dxgi.lib
        d3dcompiler.lib
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
