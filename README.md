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
- DirectX 12 / Vulkan backend
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

- **[Roadmap](ROADMAP.md)** - Development roadmap with 2D world generation plans
- **[Architecture](docs/ARCHITECTURE.md)** - System design and technical overview
- **[Terrain Generation](docs/TERRAIN_GENERATION.md)** - 2D procedural terrain with biomes and caves
- **[Vegetation System](docs/VEGETATION_SYSTEM.md)** - Tree and flora generation with biome-specific placement
- **[Animation System](docs/ANIMATION_SYSTEM.md)** - Sprite animation and character customization
- **[Camera System](docs/CAMERA_SYSTEM.md)** - 2D camera with following, zoom, and bounds
- **[Camera Features](docs/CAMERA_FEATURES.md)** - Parallax scrolling and look-ahead systems
- **[Sprite Assets](docs/SPRITE_ASSETS.md)** - Sprite creation guidelines and specifications
- **[Procedural Generation](docs/PROCEDURAL_GENERATION.md)** - Dungeon generation algorithms
- **[Lua Scripting](docs/LUA_SCRIPTING.md)** - Scripting API and examples
- **[C++/C# Integration](docs/CPP_CSHARP_INTEGRATION.md)** - Interop patterns and best practices
- **[Build Setup](docs/BUILD_SETUP.md)** - Build instructions and development workflow

## 🚀 Quick Start

### Prerequisites
- Visual Studio 2022 (v17.8+) or .NET 9 SDK
- CMake 3.20+
- C++ compiler (GCC/Clang on Linux, MSVC on Windows)

### Building

```bash
# Clone the repository
git clone https://github.com/shifty81/ChroniclesOfADrifter.git
cd ChroniclesOfADrifter

# Build C++ engine
mkdir build && cd build
cmake ..
cmake --build . --config Release

# Build C# game
cd ../src/Game
dotnet build -c Release

# Run the game
dotnet run
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
- [x] **Parallax scrolling system for depth illusion (NEW!)**
- [x] **Camera look-ahead based on player velocity (NEW!)**
- [x] **2D Terrain Generation System**
  - [x] Chunk-based world (32×30 blocks per chunk)
  - [x] Perlin noise terrain generation
  - [x] 3 biomes (Plains, Desert, Forest)
  - [x] 20-layer underground system with ores
  - [x] Cave generation
  - [x] Dynamic chunk loading/unloading
- [x] **Vegetation Generation System (NEW!)**
  - [x] Biome-specific vegetation (trees, grass, bushes, cacti, flowers)
  - [x] Forest biome: 60% coverage with oak/pine trees
  - [x] Plains biome: 30% coverage with scattered vegetation
  - [x] Desert biome: 5% coverage with cacti and palm trees
  - [x] Noise-based procedural placement
  - [x] Non-blocking vegetation (grass, flowers) vs blocking (trees)

### 🔄 Next Steps
- [ ] Implement C++ rendering engine (DirectX 12)
- [ ] Add actual sprite assets (high-resolution character sprites)
- [ ] Expand biome system to 8+ types
- [ ] Implement block digging/mining system
- [ ] Add collision detection system
- [ ] Create crafting system
- [ ] Implement combat mechanics
- [ ] Add weather and time systems
- [ ] Create UI framework

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
