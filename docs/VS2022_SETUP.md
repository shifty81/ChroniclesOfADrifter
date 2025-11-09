# Visual Studio 2022 Setup and Debugging Guide

This guide explains how to use Visual Studio 2022 for debugging Chronicles of a Drifter with mixed-mode debugging support (C++ and C#).

## Prerequisites

- **Visual Studio 2022** (Version 17.8 or later)
- Required Workloads:
  - Desktop development with C++
  - .NET desktop development
  - Game development with C++ (optional, but recommended)
- **.NET 9 SDK** (9.0 or later)
- **SDL2** development libraries (for Windows)

## Opening the Solution

1. Clone the repository:
   ```bash
   git clone https://github.com/shifty81/ChroniclesOfADrifter.git
   cd ChroniclesOfADrifter
   ```

2. Open `ChroniclesOfADrifter.sln` in Visual Studio 2022

## Solution Structure

The solution contains two projects:

### 1. ChroniclesEngine (C++ DLL)
- **Location**: `src/Engine/`
- **Type**: C++ Dynamic Library (.dll)
- **Purpose**: Native game engine with rendering, physics, and audio
- **Platform**: x64
- **Configurations**: Debug, Release

### 2. ChroniclesOfADrifter (C# Executable)
- **Location**: `src/Game/`
- **Type**: .NET 9 Console Application
- **Purpose**: Game logic, ECS, scene management, and Lua scripting
- **Platform**: x64
- **Configurations**: Debug, Release
- **Startup Project**: Set as default

## Building the Solution

### First Time Setup

1. Right-click on the solution in Solution Explorer
2. Select **Build Solution** (or press `Ctrl+Shift+B`)
3. The C++ engine will build first, then the C# game project
4. Output DLL will be placed in `build/bin/`

### Build Order

Visual Studio automatically builds in the correct order:
1. **ChroniclesEngine** (C++ DLL) builds first
2. **ChroniclesOfADrifter** (C# EXE) builds second and copies the DLL

### Build Configurations

#### Debug Configuration
- **C++ Engine**: Compiled with debug symbols, no optimization, assertions enabled
- **C# Game**: Compiled with debug symbols, JIT optimizations disabled
- **Use for**: Development, debugging, testing
- **Performance**: Slower but easier to debug

#### Release Configuration
- **C++ Engine**: Full optimizations, minimal debug info
- **C# Game**: Full optimizations, tiered compilation
- **Use for**: Performance testing, profiling, distribution
- **Performance**: Fast but harder to debug

## Debugging

### Mixed-Mode Debugging (C++ and C#)

The solution is configured for **mixed-mode debugging**, allowing you to:
- Set breakpoints in both C++ and C# code
- Step through native and managed code seamlessly
- Inspect variables across the interop boundary

#### How to Start Debugging

1. Set **ChroniclesOfADrifter** as the startup project (right-click → Set as Startup Project)
2. Ensure **Debug** configuration is selected
3. Press **F5** or click **Debug → Start Debugging**
4. The game will launch with the debugger attached

#### Setting Breakpoints

- **C# Code**: Click in the left margin of any C# file
- **C++ Code**: Click in the left margin of any C++ file
- **Conditional Breakpoints**: Right-click breakpoint → Conditions

#### Debug Profiles

The C# project includes multiple launch profiles for different renderers:

1. **ChroniclesOfADrifter** (Default)
   - Uses default renderer settings

2. **ChroniclesOfADrifter (SDL2 Renderer)**
   - Forces SDL2 renderer (cross-platform)
   - Environment: `CHRONICLES_RENDERER=sdl2`

3. **ChroniclesOfADrifter (DirectX 11)**
   - Uses DirectX 11 renderer (Windows)
   - Environment: `CHRONICLES_RENDERER=dx11`

4. **ChroniclesOfADrifter (DirectX 12)**
   - Uses DirectX 12 renderer (Windows, requires compatible GPU)
   - Environment: `CHRONICLES_RENDERER=dx12`

To switch profiles:
1. Click the dropdown next to the Start button
2. Select the desired profile

### Common Debugging Scenarios

#### Debugging C# Game Logic

1. Open any C# file in `src/Game/`
2. Set breakpoints in methods you want to debug
3. Press **F5** to start debugging
4. The debugger will stop at your breakpoints

#### Debugging C++ Engine Code

1. Open any C++ file in `src/Engine/`
2. Set breakpoints in functions you want to debug
3. Press **F5** to start debugging
4. When C# calls into C++, the debugger will stop at C++ breakpoints

#### Debugging Lua Scripts

Lua scripts are loaded at runtime from `scripts/lua/`. Currently, Lua debugging requires:
- Adding print statements in Lua scripts
- Checking console output in Visual Studio's Output window

### Debugging Tools

#### Watch Windows
- **Watch 1-4**: Monitor specific variables
- **Autos**: Shows variables in current scope
- **Locals**: Shows all local variables

#### Call Stack Window
- View the complete call stack (C# and C++)
- Double-click to navigate to any frame

#### Immediate Window
- Execute code while debugging
- Evaluate expressions on-the-fly
- Modify variable values

#### Output Window
- Console output from the game
- Debug messages
- Build errors and warnings

## Common Issues and Solutions

### Issue: "Unable to load DLL 'ChroniclesEngine.dll'"

**Cause**: The C++ DLL is not being copied to the C# output directory.

**Solution**:
1. Rebuild the C++ project: Right-click **ChroniclesEngine** → Rebuild
2. Rebuild the C# project: Right-click **ChroniclesOfADrifter** → Rebuild
3. Verify the DLL exists in `build/bin/ChroniclesEngine.dll`

### Issue: "Breakpoints won't be hit in C++ code"

**Cause**: Mixed-mode debugging may not be enabled.

**Solution**:
1. Right-click **ChroniclesOfADrifter** project → Properties
2. Go to **Debug** → **General**
3. Check **Enable native code debugging**
4. Or verify `launchSettings.json` has `"nativeDebugging": true`

### Issue: "SDL2.lib not found"

**Cause**: SDL2 development libraries are not installed or not in the library path.

**Solution**:
1. Install SDL2 development libraries
2. Update the project's library directories:
   - Right-click **ChroniclesEngine** → Properties
   - Configuration Properties → Linker → General
   - Add SDL2 library path to **Additional Library Directories**

### Issue: "Platform target mismatch"

**Cause**: C++ and C# projects are targeting different platforms (x86 vs x64).

**Solution**:
- Ensure both projects are set to **x64** platform
- Check Configuration Manager: Build → Configuration Manager

## Performance Profiling

### Using Visual Studio Profiler

1. **Debug** → **Performance Profiler** (Alt+F2)
2. Select profiling tools:
   - **CPU Usage**: Find performance bottlenecks
   - **Memory Usage**: Track allocations and leaks
   - **.NET Object Allocation**: Analyze managed memory
3. Click **Start** and run your scenario
4. Stop profiling and analyze the results

### GPU Debugging (DirectX)

For DirectX 11/12 debugging:

1. **Debug** → **Graphics** → **Start Graphics Debugging**
2. Run the game and capture frames
3. Analyze draw calls, shaders, and GPU performance

Alternatively, use external tools:
- **PIX** for detailed DirectX 12 debugging
- **RenderDoc** for cross-API graphics debugging

## Advanced Debugging

### Attach to Running Process

If the game is already running:

1. **Debug** → **Attach to Process** (Ctrl+Alt+P)
2. Find **ChroniclesOfADrifter.exe**
3. Check **Attach to**: Both Managed and Native
4. Click **Attach**

### Exception Settings

Configure when the debugger breaks on exceptions:

1. **Debug** → **Windows** → **Exception Settings** (Ctrl+Alt+E)
2. Check/uncheck exception types:
   - **CLR Exceptions**: C# exceptions
   - **C++ Exceptions**: Native exceptions
   - **Win32 Exceptions**: System exceptions

### Edit and Continue

**C# Edit and Continue** is supported:
- Modify code while debugging
- Changes apply without restarting

**C++ Edit and Continue** has limitations:
- Some code changes require restart
- Enable in project properties if needed

## Recommended Extensions

- **Visual Studio Spell Checker**: Catches typos in comments
- **CodeMaid**: Code cleanup and organization
- **Productivity Power Tools**: Enhanced IDE features
- **ReSharper** (optional): Advanced C# refactoring

## Keyboard Shortcuts

| Action                  | Shortcut         |
|-------------------------|------------------|
| Start Debugging         | F5               |
| Start Without Debugging | Ctrl+F5          |
| Stop Debugging          | Shift+F5         |
| Restart Debugging       | Ctrl+Shift+F5    |
| Step Over               | F10              |
| Step Into               | F11              |
| Step Out                | Shift+F11        |
| Continue                | F5               |
| Toggle Breakpoint       | F9               |
| Build Solution          | Ctrl+Shift+B     |
| Clean Solution          | (menu only)      |
| Quick Search            | Ctrl+Q           |
| Go to File              | Ctrl+, (comma)   |

## Tips and Best Practices

1. **Build Frequently**: Build after small changes to catch errors early
2. **Use Breakpoints Wisely**: Too many breakpoints slow down debugging
3. **Check Output Window**: Console logs provide valuable runtime info
4. **Profile Regularly**: Use the profiler to find performance issues
5. **Version Control**: Commit working code frequently
6. **Clean Build**: If issues persist, try **Build → Clean Solution** then rebuild

## Additional Resources

- [Visual Studio Debugging Documentation](https://docs.microsoft.com/en-us/visualstudio/debugger/)
- [Mixed-Mode Debugging](https://docs.microsoft.com/en-us/visualstudio/debugger/how-to-debug-managed-and-native-code)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [CMake in Visual Studio](https://docs.microsoft.com/en-us/cpp/build/cmake-projects-in-visual-studio)

## Support

For issues specific to Visual Studio 2022 setup:
- Check [BUILD_SETUP.md](BUILD_SETUP.md) for general build instructions
- Open an issue on GitHub with [VS2022 Debugging] in the title
- Include error messages and steps to reproduce

---

**Happy Debugging!** 🐛🔍
