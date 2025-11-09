# How to Play Chronicles of a Drifter

Chronicles of a Drifter is fully playable through various demo modes that showcase different game systems. This guide will help you experience the complete game.

## Quick Start

```bash
# Build the game
./build.sh        # Linux/macOS
# or
build.bat         # Windows

# Run the game
cd src/Game
dotnet run -c Release
```

## Game Modes

### 1. Main Playable Demo (Default)
**Command:** `dotnet run -c Release`

This is the primary game mode featuring:
- Player character with WASD/Arrow movement
- Combat against 5 goblin enemies
- Health system and damage mechanics
- Attack with SPACE key
- Parallax scrolling backgrounds (clouds, mountains)
- Camera following with smooth movement
- Zoom in/out with +/- keys
- FPS: ~40-45 FPS in console rendering mode

**Controls:**
- **WASD** or **Arrow Keys** - Move your character
- **SPACE** - Attack nearby enemies
- **+/-** - Zoom camera in/out
- **Q** or **ESC** - Quit game

### 2. Visual/Graphical Demo
**Command:** `dotnet run -c Release -- visual`

Experience the game with high-performance graphical rendering:
- Opens a graphical window (1280x720)
- Tile-based rendering like Zelda: A Link to the Past
- Runs at 5000-6000 FPS
- Vibrant colored terrain (grass, water, rocks, paths)
- Smooth player movement
- Perfect for testing rendering performance

**Controls:**
- **WASD** or **Arrow Keys** - Move
- **+/-** - Zoom
- **Q** or **ESC** - Quit

### 3. Terrain Generation Demo
**Command:** `dotnet run -c Release -- terrain`

Explore procedurally generated 2D terrain:
- 8 distinct biomes (Plains, Forest, Desert, Snow, Swamp, Rocky, Jungle, Beach)
- 20-layer underground system with ores and caves
- Dynamic chunk loading as you explore
- Water bodies (rivers, lakes, oceans)
- Trees and vegetation
- Console visualization with ASCII characters

**What You'll See:**
- `@` - Your character
- `▲` - Mountains/hills
- `~` - Water
- `♠`,`Ψ`,`*` - Trees and vegetation
- `█` - Stone/underground
- `▓` - Deep stone
- `#` - Different biome types
- `C` - Cave openings
- `I` - Iron ore

### 4. Mining & Building Demo
**Command:** `dotnet run -c Release -- mining`

Full mining and construction experience:
- Mine blocks with **M** key (hold down)
- Place blocks/torches with **P** key
- Inventory system (40 slots)
- Tool progression (wood, stone, iron, steel)
- Underground lighting and fog of war
- Personal lantern (8-block radius)
- Place torches for permanent light
- Resource collection system

**Starting Equipment:**
- Stone Pickaxe
- Lantern
- 10 Torches

**Controls:**
- **WASD/Arrows** - Move
- **Hold M** - Mine blocks near you
- **P** - Place blocks/torches
- **1-9** - Select inventory slot
- **Q/ESC** - Quit

### 5. Hybrid Gameplay Demo
**Command:** `dotnet run -c Release -- hybrid`

Experience the full RPG life simulation:
- **Quest system** - Multiple quests to complete
- **NPC interaction** - Merchant and questgiver NPCs
- **Farming system** - Plant and harvest crops
- **Boss battles** - Fight the Ancient Forest Guardian
- **Combat progression** - Track goblin defeats
- **Social system** - Build relationships with villagers
- **Currency system** - Buy and sell items

**Active Quests:**
1. **Goblin Threat** - Defeat 5 goblins (Combat quest)
2. **First Harvest** - Plant and harvest 3 wheat crops (Farming quest)
3. **New in Town** - Introduce yourself to 3 villagers (Social quest)

### 6. Creature Spawning Demo
**Command:** `dotnet run -c Release -- creatures`

Watch the creature AI and spawning system:
- Goblin AI with Lua scripting
- Patrol and attack behaviors
- Distance-based spawning
- Biome-specific creatures
- Automatic cleanup of dead creatures

### 7. Collision Detection Demo
**Command:** `dotnet run -c Release -- collision`

Test the collision system:
- AABB collision detection
- Entity-to-entity collisions
- Entity-to-terrain collisions
- Sliding collision response
- Multiple collision layers

### 8. Crafting System Demo
**Command:** `dotnet run -c Release -- crafting`

Explore the crafting mechanics:
- Recipe-based crafting
- Material requirements
- 8+ craftable items
- Categories: Tools, Building, Lighting
- Inventory integration

## Complete Feature List

### Core Systems ✅
- [x] Entity Component System (ECS)
- [x] Lua scripting for AI
- [x] Player movement and input
- [x] Camera system with smooth following
- [x] Parallax scrolling
- [x] Screen shake effects
- [x] Camera zones

### World Generation ✅
- [x] Procedural terrain generation
- [x] 8 biomes with unique characteristics
- [x] 20-layer underground system
- [x] Cave generation
- [x] Water bodies (rivers, lakes, oceans)
- [x] Trees and vegetation
- [x] Dynamic chunk loading/unloading
- [x] Multithreaded generation

### Gameplay Systems ✅
- [x] Combat system
- [x] Health and damage
- [x] Mining and building
- [x] Inventory management (40 slots)
- [x] Crafting system
- [x] Tool progression
- [x] Collision detection
- [x] Swimming mechanics
- [x] Lighting and fog of war
- [x] Creature spawning
- [x] Weather system
- [x] Day/night cycle
- [x] Quest system
- [x] NPC interactions
- [x] Farming system
- [x] Boss encounters

### Rendering ✅
- [x] Console rendering (ASCII art)
- [x] Visual/graphical rendering (SDL2)
- [x] DirectX 11 support (Windows)
- [x] DirectX 12 support (Windows)
- [x] High FPS (5000+ in visual mode)

## Tips for Playing

### Combat Tips
- Goblins have 30 HP, you deal 15 damage per hit
- Your attack range is 100 pixels
- Attack cooldown is 0.3 seconds
- Move strategically to avoid multiple enemies

### Mining Tips
- Different blocks require different tool tiers
- Deeper layers have better resources
- Use torches to light up dark areas
- Your lantern has an 8-block radius
- Hold M key to continuously mine

### Exploration Tips
- Different biomes have unique resources
- Caves often contain valuable ores
- Water bodies can be dangerous (swimming)
- Stay near the surface initially
- Chunk loading is automatic as you move

### Farming Tips
- Till the ground first
- Plant seeds in tilled soil
- Water crops regularly
- Harvest when fully grown
- Complete farming quests for rewards

## Performance Notes

- **Console Mode**: ~40-45 FPS (rendering overhead from console output)
- **Visual Mode**: 5000-6000 FPS (hardware accelerated)
- **Terrain Demo**: ~40-60 FPS (with chunk generation)
- **Mining Demo**: Smooth 60 FPS with lighting effects

## Renderer Selection (Windows Only)

By default, the game uses SDL2 renderer (cross-platform). On Windows, you can select different renderers:

```bash
# DirectX 11 (broad hardware support)
set CHRONICLES_RENDERER=dx11
dotnet run -c Release

# DirectX 12 (high performance)
set CHRONICLES_RENDERER=dx12
dotnet run -c Release
```

## Troubleshooting

### Game won't build
- Make sure you have .NET 9 SDK installed
- Make sure you have CMake 3.20+ installed
- On Linux: Install SDL2 with `sudo apt-get install libsdl2-dev`

### Low FPS in console mode
- This is expected due to console rendering overhead
- Use `visual` mode for better performance

### Crashes on startup
- Check that the C++ engine built successfully
- Verify SDL2 is installed (Linux)
- Try running with `--no-logo` flag

## Next Steps

After playing through the demos:
1. Try different demo modes to see all systems
2. Explore different biomes in terrain mode
3. Dig deep underground in mining mode
4. Complete quests in hybrid mode
5. Test combat in the main playable demo

## Development Status

**Current Phase:** Implementation Complete ✅

All major systems are implemented and working:
- ✅ Core engine (C++ with DirectX 11/12 + SDL2)
- ✅ Game logic (.NET 9 C#)
- ✅ Terrain generation with biomes
- ✅ Combat and AI systems
- ✅ Mining and crafting
- ✅ Full gameplay loop

The game is **fully playable** through the various demo modes. Each demo showcases different aspects of the complete game experience.

## More Information

- **Architecture**: See `docs/ARCHITECTURE.md`
- **Build Instructions**: See `docs/BUILD_SETUP.md`
- **Terrain System**: See `docs/TERRAIN_GENERATION.md`
- **Mining System**: See `docs/MINING_BUILDING_SYSTEM.md`
- **Crafting System**: See `docs/CRAFTING_SYSTEM.md`
- **Full Roadmap**: See `ROADMAP.md`

---

**Chronicles of a Drifter** - A modern 2D action RPG with classic Zelda inspiration
