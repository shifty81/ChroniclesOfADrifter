# Cross-Platform Support Implementation

## Overview
This document describes the cross-platform support implementation that enables Chronicles of a Drifter to run on both Windows and Linux/Unix platforms.

## Problem Statement
The original implementation was Windows-only with DirectX 11 as the default renderer. When attempting to run on Linux, the game would throw a `System.DllNotFoundException` because:
1. CMakeLists.txt explicitly prevented non-Windows builds
2. No native engine library was built for Linux
3. Settings system always defaulted to DirectX 11

## Solution

### 1. CMake Build System Changes
**File:** `CMakeLists.txt`

**Changes:**
- Removed Windows-only restriction that prevented building on other platforms
- Made SDL2 a required dependency on Linux/Unix (optional on Windows)
- Added `SDL2_DEFAULT_RENDERER` compile definition for non-Windows platforms
- Removed unused `ReflectionExample.cpp` from build (had compilation issues)

**Platform Detection:**
```cmake
if(WIN32)
    # Windows: DirectX 11 is default, SDL2 is optional
    find_package(SDL2)
else()
    # Linux/Unix: SDL2 is required
    find_package(SDL2 REQUIRED)
endif()
```

### 2. C++ Reflection System Fix
**File:** `src/Engine/Reflection.h`

**Issue:** `TypeRegistrar` class was non-copyable due to `unique_ptr` member, but macro usage attempted to copy temporary objects.

**Solution:** 
- Explicitly deleted copy constructor and copy assignment
- Added move constructor and move assignment with `noexcept`
- Added null check in destructor to handle moved-from objects

### 3. Build Script Updates
**File:** `build.sh`

**Changes:**
- Added platform detection (Windows/Linux/macOS/Unix)
- Added SDL2 prerequisite check for non-Windows platforms
- Provides clear error messages with installation instructions for SDL2

### 4. Settings System Platform Awareness
**File:** `src/Game/Engine/SettingsManager.cs`

**Changes:**
- Made renderer backend default platform-aware using `OperatingSystem.IsWindows()`
- Windows defaults to "dx11"
- Linux/Unix defaults to "sdl2"

**Implementation:**
```csharp
private static string GetDefaultRenderer()
{
    return OperatingSystem.IsWindows() ? "dx11" : "sdl2";
}
```

### 5. Game Initialization Flow
**File:** `src/Game/Program.cs`

**Changes:**
- Added `InitializeSettings()` helper method to centralize settings loading
- Updated all demo entry points to load and apply settings before engine initialization
- Updated console messages to show actual platform and active renderer

**Benefits:**
- Settings are loaded automatically before engine starts
- Correct renderer backend is selected based on platform
- Users see which renderer is actually being used

### 6. C++ Engine Platform Detection
**File:** `src/Engine/ChroniclesEngine.cpp`

**Existing Logic (Enhanced):**
The engine already had logic to detect platform and fall back to appropriate renderer:
```cpp
#ifdef _WIN32
    // Windows: Default to DirectX 11
    result = Chronicles::RendererBackend::DirectX11;
#elif defined(HAS_SDL2)
    // Non-Windows: Default to SDL2
    result = Chronicles::RendererBackend::SDL2;
#endif
```

### 7. Documentation Updates
**File:** `README.md`

**Changes:**
- Updated platform configuration section to clearly state cross-platform support
- Added Linux-specific prerequisites (SDL2 development libraries)
- Added Linux build instructions
- Updated technology stack section to clarify platform-specific defaults
- Added installation commands for SDL2 on various Linux distributions

## Platform-Specific Behavior

### Windows
- **Default Renderer:** DirectX 11
- **Optional Renderers:** DirectX 12, SDL2
- **Build Tools:** Visual Studio 2022 or CMake
- **Native Library:** `ChroniclesEngine.dll`

### Linux/Unix
- **Default Renderer:** SDL2
- **Prerequisites:** SDL2 development libraries
- **Build Tools:** CMake with GCC/Clang
- **Native Library:** `libChroniclesEngine.so`

## Renderer Selection Priority

1. **Environment Variable:** `CHRONICLES_RENDERER` (highest priority)
2. **Settings File:** `settings.json`
3. **Platform Default:** 
   - Windows: `dx11`
   - Linux/Unix: `sdl2`

## Testing Results

All test suites pass successfully on Linux with SDL2 renderer:
- ✅ Terrain generation test
- ✅ Camera features test
- ✅ Collision system test
- ✅ Crafting system test
- ✅ Vegetation generation test
- ✅ Settings system test
- ✅ Swimming system test
- ✅ Lighting system test

## Build Instructions

### Linux
```bash
# Install dependencies
sudo apt-get install libsdl2-dev  # Ubuntu/Debian
# or
sudo dnf install SDL2-devel       # Fedora
# or
sudo pacman -S sdl2               # Arch

# Clone and build
git clone https://github.com/shifty81/ChroniclesOfADrifter.git
cd ChroniclesOfADrifter
./build.sh

# Run
cd src/Game
dotnet run -c Release
```

### Windows
```bash
# Clone and build
git clone https://github.com/shifty81/ChroniclesOfADrifter.git
cd ChroniclesOfADrifter
build.bat

# Run
cd src\Game
dotnet run -c Release
```

## Known Limitations

1. **Graphical Demos:** Some graphical demos (visual, editor) may not work in headless environments without a display server
2. **DirectX on Linux:** DirectX renderers are Windows-only and will not work on Linux
3. **Performance:** SDL2 renderer may have different performance characteristics compared to DirectX

## Future Improvements

1. **macOS Support:** Add Metal renderer support for macOS
2. **Vulkan Renderer:** Add Vulkan renderer for better cross-platform performance
3. **Headless Mode:** Add headless mode for server-side simulations
4. **CI/CD:** Add automated builds for multiple platforms

## Migration Guide for Users

### Existing Windows Users
No changes required. The game will continue to use DirectX 11 by default.

### New Linux Users
Simply install SDL2 development libraries and build. The game will automatically use SDL2.

### Changing Renderer
Edit `settings.json`:
```json
{
  "renderer": {
    "backend": "sdl2",  // or "dx11", "dx12"
    ...
  }
}
```

Or set environment variable:
```bash
export CHRONICLES_RENDERER=sdl2
```

## Conclusion

Chronicles of a Drifter now provides full cross-platform support with minimal user intervention. The build system automatically detects the platform and selects appropriate defaults, while still allowing users to override renderer selection when needed.
