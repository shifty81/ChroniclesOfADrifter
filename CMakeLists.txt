cmake_minimum_required(VERSION 3.20)
project(ChroniclesEngine VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Enforce Windows-only builds with DirectX 11 as default
if(NOT WIN32)
    message(FATAL_ERROR "This project is configured for Windows only with DirectX 11 as the default renderer. "
                        "Cross-platform support with SDL2 is available but not the default configuration.")
endif()

# Find SDL2 (optional on Windows, DirectX 11 is the default)
find_package(SDL2)
if(SDL2_FOUND)
    include_directories(${SDL2_INCLUDE_DIRS})
    message(STATUS "SDL2 found: ${SDL2_INCLUDE_DIRS} (optional, DirectX 11 is default on Windows)")
else()
    message(STATUS "SDL2 not found. Using DirectX 11 as default renderer on Windows.")
endif()

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
)

# Add SDL2 renderer if SDL2 is found
if(SDL2_FOUND)
    list(APPEND ENGINE_SOURCES
        src/Engine/SDL2Renderer.cpp
        src/Engine/SDL2Renderer.h
    )
endif()

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

# Add SDL2 compile definition if available
if(SDL2_FOUND)
    target_compile_definitions(ChroniclesEngine PRIVATE HAS_SDL2)
endif()

target_include_directories(ChroniclesEngine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/src/Engine
)

# Link SDL2
if(SDL2_FOUND)
    target_link_libraries(ChroniclesEngine PRIVATE ${SDL2_LIBRARIES})
endif()

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
