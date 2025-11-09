# Chronicles of a Drifter - Build and Setup Guide

## Overview

This guide covers building and running Chronicles of a Drifter on your local development machine. The project is currently in active development with a focus on **local iteration and debugging** rather than production releases.

**For Visual Studio 2022 debugging setup**, see the **[Visual Studio 2022 Setup Guide](VS2022_SETUP.md)** which includes mixed-mode C++/C# debugging instructions.

### Quick Start for Local Development

The easiest way to build and run the game locally:

```bash
# Quick build (runs both C++ and C# builds)
./build.sh        # Linux/macOS
build.bat         # Windows

# Run the game
cd src/Game
dotnet run -c Release
```

**Note:** GitHub Actions and automated releases are not configured yet, as the project is still in the debugging and prototyping phase. Focus is on rapid local iteration.

## Prerequisites

### Required Software
- **Visual Studio 2022** (v17.8 or later)
  - Workloads:
    - Desktop development with C++
    - .NET desktop development
    - Game development with C++
- **.NET 9 SDK** (9.0 or later)
- **CMake** (3.20 or later)
- **Git** for version control

### Optional Tools
- **Visual Studio Code** for Lua script editing
- **RenderDoc** or **PIX** for graphics debugging
- **vcpkg** for C++ package management

## Project Structure

```
ChroniclesOfADrifter/
├── docs/                          # Documentation
│   ├── ARCHITECTURE.md
│   ├── PROCEDURAL_GENERATION.md
│   ├── LUA_SCRIPTING.md
│   └── CPP_CSHARP_INTEGRATION.md
├── src/
│   ├── Engine/                    # C++ Native Engine
│   │   ├── ChroniclesEngine.h
│   │   ├── ChroniclesEngine.cpp
│   │   ├── Renderer/
│   │   │   ├── Renderer.h
│   │   │   ├── Renderer.cpp
│   │   │   ├── D3D12Renderer.h
│   │   │   └── D3D12Renderer.cpp
│   │   ├── Input/
│   │   │   ├── Input.h
│   │   │   └── Input.cpp
│   │   ├── Audio/
│   │   │   ├── Audio.h
│   │   │   └── Audio.cpp
│   │   └── Physics/
│   │       ├── Physics.h
│   │       └── Physics.cpp
│   └── Game/                      # C# Game Logic
│       ├── ChroniclesOfADrifter.csproj
│       ├── Program.cs
│       ├── Engine/
│       │   ├── EngineInterop.cs
│       │   └── NativeEngine.cs
│       ├── ECS/
│       │   ├── Entity.cs
│       │   ├── Component.cs
│       │   ├── System.cs
│       │   └── World.cs
│       ├── Systems/
│       │   ├── MovementSystem.cs
│       │   ├── CombatSystem.cs
│       │   ├── RenderSystem.cs
│       │   └── ScriptingSystem.cs
│       ├── Components/
│       │   ├── Transform.cs
│       │   ├── Sprite.cs
│       │   ├── Health.cs
│       │   └── Velocity.cs
│       └── World/
│           ├── SceneManager.cs
│           ├── ProceduralGenerator.cs
│           └── WeatherSystem.cs
├── scripts/
│   └── lua/                       # Lua Scripts
│       ├── enemies/
│       ├── weapons/
│       └── quests/
├── assets/                        # Game Assets
│   ├── sprites/
│   ├── sounds/
│   ├── music/
│   └── data/
├── tests/                         # Unit Tests
│   ├── EngineTests/               # C++ Tests
│   └── GameTests/                 # C# Tests
├── build/                         # Build output (gitignored)
├── CMakeLists.txt                 # C++ build configuration
└── ChroniclesOfADrifter.sln       # Visual Studio solution
```

## Building the Project

### Step 1: Clone the Repository

```bash
git clone https://github.com/shifty81/ChroniclesOfADrifter.git
cd ChroniclesOfADrifter
```

### Step 2: Initialize Submodules (if any)

```bash
git submodule update --init --recursive
```

### Step 3: Build C++ Engine

#### Using CMake (Command Line)

```bash
# Create build directory
mkdir build
cd build

# Configure
cmake .. -G "Visual Studio 17 2022" -A x64

# Build
cmake --build . --config Release
```

#### Using Visual Studio 2022

1. Open `ChroniclesOfADrifter.sln`
2. Right-click on the `ChroniclesEngine` project
3. Select **Build**
4. The DLL will be output to `build/Release/ChroniclesEngine.dll`

### Step 4: Build C# Game Logic

#### Using Command Line

```bash
cd src/Game
dotnet build -c Release
```

#### Using Visual Studio 2022

1. In the Solution Explorer, right-click on `ChroniclesOfADrifter` C# project
2. Select **Build**

### Step 5: Run the Game

```bash
cd src/Game/bin/Release/net9.0
./ChroniclesOfADrifter.exe
```

Or press **F5** in Visual Studio 2022 to debug.

## Development Workflow

### C++ Development

1. **Make changes** to C++ code in `src/Engine/`
2. **Rebuild** the C++ project
3. The DLL will be automatically copied to the C# output directory
4. **Restart** the C# application to load the new DLL

### C# Development

1. **Make changes** to C# code in `src/Game/`
2. **Rebuild** the C# project (hot reload supported in some cases)
3. **Run** the application

### Lua Script Development

1. **Edit** Lua scripts in `scripts/lua/`
2. **No rebuild required** - scripts are loaded at runtime
3. Use **hot-reload** feature to see changes immediately (if implemented)

## Configuration

### Debug vs Release Builds

#### Debug Build (Development)
- Slower performance
- Full debugging symbols
- Assertions enabled
- Verbose logging

```bash
cmake --build . --config Debug
dotnet build -c Debug
```

#### Release Build (Production)
- Optimized performance
- Minimal symbols
- Assertions disabled
- Error-only logging

```bash
cmake --build . --config Release
dotnet build -c Release
```

### Engine Configuration

Edit `assets/data/config.json`:

```json
{
  "window": {
    "width": 1920,
    "height": 1080,
    "fullscreen": false,
    "vsync": true
  },
  "renderer": {
    "backend": "DirectX12",
    "msaa": 4,
    "anisotropicFiltering": 16
  },
  "audio": {
    "masterVolume": 1.0,
    "musicVolume": 0.7,
    "sfxVolume": 0.8
  },
  "gameplay": {
    "difficulty": "normal",
    "autoSave": true,
    "autosaveInterval": 300
  }
}
```

## Testing

### Running Unit Tests

#### C++ Tests (Google Test)

```bash
cd build
ctest --output-on-failure
```

#### C# Tests (XUnit)

```bash
cd tests/GameTests
dotnet test
```

### Running Integration Tests

```bash
dotnet test --filter "Category=Integration"
```

## Debugging

### Visual Studio 2022 Mixed-Mode Debugging

1. Open `ChroniclesOfADrifter.sln`
2. Set `ChroniclesOfADrifter` (C#) as the startup project
3. Right-click project → **Properties** → **Debug**
4. Check **Enable native code debugging**
5. Set breakpoints in both C++ and C# code
6. Press **F5** to start debugging

### Common Issues

#### DLL Not Found
```
System.DllNotFoundException: Unable to load DLL 'ChroniclesEngine.dll'
```

**Solution**: Ensure `ChroniclesEngine.dll` is in the same directory as the C# executable, or update the DLL path in `EngineInterop.cs`.

#### Version Mismatch
```
System.BadImageFormatException: An attempt was made to load a program with an incorrect format.
```

**Solution**: Ensure both C++ and C# projects target the same architecture (x64).

#### Lua Scripts Not Loading
```
Error: Script file not found: enemies/goblin.lua
```

**Solution**: Check that Lua scripts are copied to the output directory. Add to `.csproj`:

```xml
<ItemGroup>
  <None Include="..\..\scripts\lua\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Performance Profiling

### Using Visual Studio Profiler

1. **Debug** → **Performance Profiler**
2. Select profiling tools:
   - CPU Usage
   - Memory Usage
   - .NET Object Allocation
3. Click **Start**
4. Run your game scenario
5. Stop profiling and analyze results

### Using PIX (for DirectX 12)

1. Download PIX from Microsoft
2. Launch PIX
3. Select **GPU Capture**
4. Target `ChroniclesOfADrifter.exe`
5. Capture a frame
6. Analyze rendering performance

## Continuous Integration

### GitHub Actions Example

`.github/workflows/build.yml`:

```yaml
name: Build

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Build C++ Engine
      run: |
        mkdir build
        cd build
        cmake .. -G "Visual Studio 17 2022" -A x64
        cmake --build . --config Release
    
    - name: Build C# Game
      run: |
        cd src/Game
        dotnet build -c Release
    
    - name: Run Tests
      run: |
        cd tests/GameTests
        dotnet test
```

## Packaging for Distribution

### Creating a Release Build

```bash
# Build everything in Release mode
cmake --build build --config Release
dotnet publish src/Game/ChroniclesOfADrifter.csproj -c Release -r win-x64 --self-contained true

# Package files
cd src/Game/bin/Release/net9.0/win-x64/publish
```

### Files to Include
- `ChroniclesOfADrifter.exe`
- `ChroniclesEngine.dll`
- `assets/` folder
- `scripts/` folder
- `README.md`
- `LICENSE`

### Installer Creation

Use **Inno Setup** or **WiX Toolset** to create an installer.

## Game Scale and Constants

### Player Character Scale Reference

**The player character is approximately 2.5 blocks tall.** This is the fundamental scale reference for all game content:

```csharp
// From GameConstants.cs
public const float PlayerHeightInBlocks = 2.5f;
public const float PlayerWidthInBlocks = 0.8f;
public const int BlockSize = 32; // pixels at 1:1 scale
```

### Scale Guidelines for Content Creation

When generating or creating game content, use the player scale as reference:

- **Doors**: Minimum 3 blocks tall (player height + headroom)
- **Cave/Tunnel Passages**: Minimum 3 blocks high for comfortable navigation
- **Trees**: 4-8 blocks tall for visual variety and proper scale
- **Building Ceilings**: ~4 blocks high for spacious indoor areas
- **NPC Characters**: Scale relative to player (2.5 blocks for human-sized)

### Using Scale Constants in Code

All scale constants are centralized in `src/Game/GameConstants.cs`:

```csharp
using ChroniclesOfADrifter;

// When creating player collision box
var collision = new CollisionComponent(
    width: GameConstants.PlayerCollisionWidth,
    height: GameConstants.PlayerCollisionHeight
);

// When generating doorways
if (doorHeight < GameConstants.MinDoorHeight * GameConstants.BlockSize)
{
    doorHeight = GameConstants.MinDoorHeight * GameConstants.BlockSize;
}

// When placing trees procedurally
float treeHeight = Random.Range(
    GameConstants.MinTreeHeight, 
    GameConstants.MaxTreeHeight
) * GameConstants.BlockSize;
```

This ensures consistent scale throughout the game world and makes procedural generation feel natural and properly proportioned.

## Additional Resources

- [DirectX 12 Programming Guide](https://docs.microsoft.com/en-us/windows/win32/direct3d12/directx-12-programming-guide)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [CMake Documentation](https://cmake.org/documentation/)
- [Lua 5.4 Reference Manual](https://www.lua.org/manual/5.4/)

## Getting Help

- **Issues**: Report bugs on GitHub Issues
- **Discussions**: Ask questions on GitHub Discussions
- **Discord**: Join our community Discord server
- **Wiki**: Check the project wiki for more guides

## License

This project is licensed under the MIT License - see the LICENSE file for details.
