# Chronicles of a Drifter

A 2D top-down action RPG built with a custom **C++/.NET 9/Lua voxel game engine**, inspired by The Legend of Zelda: A Link to the Past.

## 🎮 Game Concept

Chronicles of a Drifter features:
- **Procedurally generated world** with interconnected scenes
- **Extensive crafting system** for equipment and upgrades
- **Randomized loot** with varied weapon attributes
- **Dynamic weather** and day/night cycles
- **Home base building** with modular construction
- **Satisfying combat** with DoT effects and responsive feedback

## 🛠️ Technology Stack

### C++ Core Engine
- Performance-critical systems (rendering, physics, audio)
- **DirectX 12 renderer** (Windows, high-performance) ✅ **IMPLEMENTED**
- **DirectX 11 renderer** (Windows, broad compatibility) ✅ **IMPLEMENTED**
- **SDL2 renderer** (cross-platform, default) ✅ **IMPLEMENTED**
- Abstracted rendering backend for flexibility
- Cross-platform foundation

### .NET 9 (C#) Game Logic
- Entity Component System (ECS) architecture
- Scene management
- Gameplay systems
- UI framework

### Lua Scripting
- Enemy AI behaviors
- Weapon effects
- Quest logic
- Runtime-editable content

## 📁 Project Structure

```
ChroniclesOfADrifter/
├── docs/              # Comprehensive documentation
├── src/
│   ├── Engine/        # C++ native engine
│   └── Game/          # C# game logic
├── scripts/lua/       # Lua game scripts
├── assets/            # Game assets (sprites, sounds)
└── tests/             # Unit and integration tests
```

## 📚 Documentation

### Getting Started
- **[Build Setup](docs/BUILD_SETUP.md)** - Build instructions and local development workflow
- **[Visual Studio 2022 Setup](docs/VS2022_SETUP.md)** - VS2022 debugging guide with mixed-mode C++/C# support
- **[Scale Reference](docs/SCALE_REFERENCE.md)** - Game scale constants and design guidelines (Player is 2.5 blocks tall)
- **[Roadmap](ROADMAP.md)** - Development roadmap with 2D world generation plans

### Systems Documentation
- **[Architecture](docs/ARCHITECTURE.md)** - System design and technical overview
- **[Terrain Generation](docs/TERRAIN_GENERATION.md)** - 2D procedural terrain with biomes and caves
- **[Vegetation System](docs/VEGETATION_SYSTEM.md)** - Tree and flora generation with biome-specific placement
- **[Water System](docs/WATER_SYSTEM.md)** - River, lake, and ocean generation
- **[Mining & Building System](docs/MINING_BUILDING_SYSTEM.md)** - Block mining, resource collection, and building mechanics
- **[Crafting System](docs/CRAFTING_SYSTEM.md)** - Recipe-based item crafting and material processing
- **[Swimming Mechanics](docs/SWIMMING_MECHANICS.md)** - Swimming, breath management, and water flow physics
- **[Collision Detection](docs/COLLISION_SYSTEM.md)** - AABB collision detection, entity and terrain collision, sliding response
- **[Animation System](docs/ANIMATION_SYSTEM.md)** - Sprite animation and character customization
- **[Camera System](docs/CAMERA_SYSTEM.md)** - 2D camera with following, zoom, and bounds
- **[Camera Features](docs/CAMERA_FEATURES.md)** - Parallax scrolling and look-ahead systems
- **[Cinematic Camera](docs/CINEMATIC_CAMERA.md)** - Cinematic camera movements for cutscenes with easing functions

### Development Resources
- **[Sprite Assets](docs/SPRITE_ASSETS.md)** - Sprite creation guidelines and specifications
- **[Procedural Generation](docs/PROCEDURAL_GENERATION.md)** - Dungeon generation algorithms
- **[Lua Scripting](docs/LUA_SCRIPTING.md)** - Scripting API and examples
- **[C++/C# Integration](docs/CPP_CSHARP_INTEGRATION.md)** - Interop patterns and best practices

## 🚀 Quick Start

### Prerequisites
- Visual Studio 2022 (v17.8+) or .NET 9 SDK
- CMake 3.20+
- C++ compiler (GCC/Clang on Linux, MSVC on Windows)
- SDL2 development libraries (Linux: `libsdl2-dev`, Windows: included)

### Building Locally

**Note:** This project is currently in active development. Focus is on **local iteration and debugging** rather than production releases. There are no GitHub Actions or CI/CD pipelines configured yet.

#### Quick Build (Recommended for Local Development)

```bash
# Clone the repository
git clone https://github.com/shifty81/ChroniclesOfADrifter.git
cd ChroniclesOfADrifter

# Run the automated build script
./build.sh        # Linux/macOS
# or
build.bat         # Windows

# Run the game (default: SDL2 renderer)
cd src/Game
dotnet run -c Release

# Or use DirectX 11 renderer on Windows (broad hardware support)
set CHRONICLES_RENDERER=dx11  # Windows Command Prompt
# or
$env:CHRONICLES_RENDERER="dx11"  # Windows PowerShell
dotnet run -c Release

# Or use DirectX 12 renderer on Windows (high performance)
set CHRONICLES_RENDERER=dx12  # Windows Command Prompt
# or
$env:CHRONICLES_RENDERER="dx12"  # Windows PowerShell
dotnet run -c Release
```

#### Using Visual Studio 2022 (Recommended for Debugging)

**For the best debugging experience with mixed-mode C++/C# debugging:**

1. Open `ChroniclesOfADrifter.sln` in Visual Studio 2022
2. Select **Build → Build Solution** (Ctrl+Shift+B)
3. Press **F5** to start debugging with breakpoints in both C++ and C# code

See the **[Visual Studio 2022 Setup Guide](docs/VS2022_SETUP.md)** for detailed instructions.

#### Manual Build

```bash
# Build C++ engine
mkdir build && cd build
cmake ..
cmake --build . --config Release

# Build C# game
cd ../src/Game
dotnet build -c Release

# Run the game
dotnet run -c Release
```

### Running the Demo

The current implementation includes:
- **ECS Demo**: Player entity with WASD/Arrow key movement
- **Lua Scripting Demo**: Goblin AI controlled by Lua script

The demo will run for ~5 seconds showing the game loop in action with console output from the Lua-controlled goblin AI.

See [BUILD_SETUP.md](docs/BUILD_SETUP.md) for detailed instructions.

## 🎯 Current Status: Implementation Phase

This repository contains the **initial implementation** of Chronicles of a Drifter. The following has been completed:

### ✅ Completed
- [x] Project structure defined
- [x] Architecture documentation
- [x] Procedural generation algorithm specifications
- [x] Lua scripting API design
- [x] C++/C# integration patterns
- [x] Build system configuration
- [x] **Entity Component System (ECS) implementation**
- [x] **Player movement with keyboard input**
- [x] **Lua scripting integration with NLua**
- [x] **Scene management system**
- [x] **Example AI scripts (Goblin patrol)**
- [x] **Sprite animation system with frame-by-frame support**
- [x] **Character customization system with clothing layers**
- [x] **High-resolution sprite support (64x64, 128x128)**
- [x] **Character creator with multiple customization options**
- [x] **Clothing color customization system**
- [x] **Armor/clothing visibility system**
- [x] **2D Camera system with smooth following and zoom**
- [x] **Parallax scrolling system for depth illusion**
- [x] **Camera look-ahead based on player velocity**
- [x] **2D Terrain Generation System**
  - [x] Chunk-based world (32×30 blocks per chunk)
  - [x] Perlin noise terrain generation
  - [x] 8 biomes (Plains, Desert, Forest, Snow, Swamp, Rocky, Jungle, Beach)
  - [x] Temperature/moisture-based biome distribution
  - [x] 20-layer underground system with ores
  - [x] Cave generation
  - [x] Dynamic chunk loading/unloading
- [x] **Vegetation Generation System**
  - [x] Biome-specific vegetation (trees, grass, bushes, cacti, flowers)
  - [x] Forest biome: 60% coverage with oak/pine trees
  - [x] Plains biome: 30% coverage with scattered vegetation
  - [x] Desert biome: 5% coverage with cacti and palm trees
  - [x] Snow biome: 30% coverage with pine trees
  - [x] Swamp biome: 40% coverage with oak trees and reeds
  - [x] Rocky biome: 10% coverage with hardy plants
  - [x] Jungle biome: 70% coverage with dense vegetation
  - [x] Beach biome: 15% coverage with palm trees
  - [x] Noise-based procedural placement
  - [x] Non-blocking vegetation (grass, flowers) vs blocking (trees)
- [x] **Mining and Building System**
  - [x] Block mining with tool requirements
  - [x] Inventory system for resource collection (40 slots)
  - [x] Tool progression (wood, stone, iron, steel)
  - [x] Block hardness and mining speed mechanics
  - [x] Resource drops from mined blocks
  - [x] Block placement from inventory
  - [x] Interactive mining demo scene
- [x] **Water Body Generation**
  - [x] Rivers with meandering patterns (2 blocks deep)
  - [x] Lakes in natural depressions (3 blocks deep)
  - [x] Ocean zones in beach biomes (5 blocks deep)
  - [x] Biome-specific water placement rules
  - [x] Noise-based natural water patterns
  - [x] Water generation test suite
- [x] **Advanced Camera System**
  - [x] Multi-layer parallax backgrounds (Sky, Clouds, Mountains, Stars, Mist)
  - [x] Screen shake effects for combat feedback (light, medium, heavy)
  - [x] Camera zones with dynamic behavior per area
  - [x] Smooth zoom transitions
  - [x] Camera look-ahead based on velocity
  - [x] Cinematic camera movements for cutscenes with easing functions
- [x] **Underground Lighting and Fog of War**
  - [x] Depth-based ambient lighting (bright surface, dark underground)
  - [x] Player personal lantern (8-block radius)
  - [x] Torch placement system (8-block radius per torch)
  - [x] Light intensity falloff with distance
  - [x] Fog of war with exploration tracking
  - [x] Dynamic lighting for all light sources
- [x] **Collision Detection System**
  - [x] AABB (Axis-Aligned Bounding Box) collision detection
  - [x] Entity-to-terrain collision with ChunkManager integration
  - [x] Entity-to-entity collision detection
  - [x] Collision layer system for filtering (Player, Enemy, Projectile, etc.)
  - [x] Sliding collision response (smooth wall sliding)
  - [x] Static vs. dynamic entity support
  - [x] Comprehensive collision test suite
  - [x] Interactive collision demo scene
- [x] **Crafting System**
  - [x] Recipe-based crafting with materials
  - [x] 8 initial recipes (wood planks, wood blocks, bricks, torches, etc.)
  - [x] Crafting categories (Tools, Building, Lighting)
  - [x] Inventory integration for crafting
  - [x] Craftable recipes viewer
  - [x] Comprehensive crafting system tests
  - [x] Interactive crafting demo scene
- [x] **Swimming and Water Mechanics**
  - [x] Swimming component with breath management
  - [x] Water flow system with different body types (River, Lake, Ocean)
  - [x] Drowning mechanics when out of breath
  - [x] Swim speed reduction in water
  - [x] Water flow affects entity movement
  - [x] Comprehensive swimming system tests
- [x] **Day/Night Cycle and Time System**
  - [x] 24-hour in-game day with configurable time scale (60x default)
  - [x] Four day phases (Dawn, Day, Dusk, Night) with smooth transitions
  - [x] Dynamic ambient lighting based on time of day
  - [x] Atmospheric color tinting (warm dawn/dusk, cool night)
  - [x] Creature spawn rate multipliers by time of day
  - [x] Integration with lighting system for surface/underground
  - [x] Time manipulation and query API
  - [x] Comprehensive time system tests

### 🔄 Next Steps
- [x] **Implement C++ rendering engine (DirectX 12)** (COMPLETED - see [DIRECTX12_RENDERER.md](docs/DIRECTX12_RENDERER.md))
- [ ] Add actual sprite assets (high-resolution character sprites)
- [x] Add weather and time systems (COMPLETED)
- [ ] Create UI framework for crafting and inventory
- [x] Implement multithreaded chunk generation (COMPLETED)
- [ ] Enhance combat mechanics with weapon crafting

## 🎨 Game Features

### Character Customization
- **Sprite Animation System** with frame-by-frame animations
- **Character Creator** with extensive customization options
  - 6 skin tones (pale to dark)
  - 7 hair styles (short, long, ponytail, bald, curly, braided, spiky)
  - 4 body types (slim, average, athletic, heavy)
- **Layered Clothing System**
  - Multiple clothing categories (shirts, pants, boots, gloves, hats)
  - 5+ styles per category
  - Dynamic color customization with primary and secondary colors
  - 8 preset color palettes (Earth Tones, Forest, Ocean, Crimson, Royal, Neutral, Midnight, Desert)
- **Armor System**
  - Armor overrides clothing visibility when equipped
  - Clothing automatically reappears when armor is removed
- **High-Resolution Sprites**
  - Support for 64x64 and 128x128 per-frame sprites
  - Smooth animations at 6-8 frames per second
  - Scalable rendering for different resolutions

### Procedural Generation
- **BSP Algorithm** for structured dungeons
- **Random Walkers** for organic caves
- **Cellular Automata** for natural terrain
- **Hybrid approach** for Zelda-like dungeons

### Crafting & Loot
- Material gathering from the world
- Equipment creation and upgrading
- Randomized weapon attributes
- Varied weapon types (swords, guns, magic)

### World Systems
- Scene-based world structure
- Hidden dungeon entrances
- Dynamic weather effects
- Day/night cycle with gameplay impact
- Seasonal progression

### Combat
- Responsive melee combat
- Ranged weapons with spread
- Status effects (bleeding, burning, poison)
- Damage-over-time mechanics

## 🤝 Contributing

This is currently in the planning phase. Contributions are welcome once the core systems are implemented.

## 📄 License

MIT License - See [LICENSE](LICENSE) for details

## 🙏 Acknowledgments

- Inspired by The Legend of Zelda: A Link to the Past
- Built with modern C++20, .NET 9, and Lua
- Procedural generation techniques from roguelike development

## 📬 Contact

For questions or discussions about the project architecture:
- GitHub Issues: [Report a bug or request a feature](https://github.com/shifty81/ChroniclesOfADrifter/issues)
- Discussions: [Join the conversation](https://github.com/shifty81/ChroniclesOfADrifter/discussions)

---

**Chronicles of a Drifter** - A modern 2D action RPG with classic Zelda inspiration
