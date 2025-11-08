# 2D Terrain Generation System

This document describes the 2D terrain generation system implemented for Chronicles of a Drifter.

> **Scale Reference**: The player character is approximately 2.5 blocks tall. See [SCALE_REFERENCE.md](SCALE_REFERENCE.md) for comprehensive scale guidelines when generating terrain features.

## Overview

The terrain generation system creates procedurally generated 2D worlds with:
- Height-mapped surface terrain using Perlin noise
- 20-layer underground system (0-20)
- Multiple biomes with distinct surface and underground characteristics
- Cave pocket generation
- Ore vein distribution by depth

## Architecture

### Chunk System

The world is divided into horizontal chunks:
- **Size**: 32 blocks wide × 30 blocks tall
- **Vertical Layers**: 10 surface + 20 underground
- **Dynamic Loading**: Chunks load/unload based on player position

```
Chunk Structure:
  Y=0-9   : Surface and air (variable height)
  Y=10-29 : Underground layers
  Y=29    : Bedrock (unbreakable)
```

### Coordinate Systems

1. **World Coordinates**: Absolute position (X can be any value, Y=0-29)
2. **Chunk Coordinates**: Which chunk (X/32)
3. **Local Coordinates**: Position within chunk (X%32, Y)

### Terrain Generation

#### Surface Terrain
- Uses SimplexNoise (Perlin noise) for natural height variation
- Height range: 4-9 blocks (adjustable)
- Frequency: 0.03 (controls smoothness)

#### Biomes
Three biomes with smooth transitions:
- **Plains**: Grass surface, dirt topsoil, standard underground
- **Desert**: Sand surface, sand topsoil, sandstone underground  
- **Forest**: Grass surface, dirt topsoil, standard underground

Biome selection uses low-frequency noise (0.005) for large-scale zones.

#### Underground Layers

```
Layers 0-3   : Topsoil (Dirt/Sand) - Easy to dig
Layers 4-8   : Stone - Copper ore common
Layers 9-14  : Stone - Iron ore uncommon
Layers 15-19 : Deep Stone - Gold ore rare
Layer 20     : Bedrock - Unbreakable
```

#### Cave Generation
- Uses noise threshold (>200/255)
- Frequency: 0.08 (smaller caves)
- Random pockets throughout underground

#### Ore Distribution
Ores are placed using 2D noise at specific depth ranges:
- **Copper**: Y=10-17 (shallow), threshold >230
- **Iron**: Y=14-23 (medium), threshold >240  
- **Gold**: Y=20-27 (deep), threshold >245

## Usage

### Running Tests

```bash
cd src/Game
dotnet run test
```

This runs comprehensive tests including:
1. Chunk creation and management
2. Terrain generation
3. Biome distribution
4. Ore generation
5. Visual cross-section output

### Terrain Demo

```bash
cd src/Game
dotnet run terrain
```

Interactive demo with:
- WASD/Arrow keys to move
- Real-time chunk loading
- Terrain rendering in console
- FPS counter and debug info

## Code Structure

```
src/Game/
├── World/
│   ├── Chunk.cs              # 32×30 tile grid storage
│   ├── ChunkManager.cs       # Chunk loading/unloading
│   └── TerrainGenerator.cs   # Noise-based generation
├── ECS/Components/
│   └── TileComponent.cs      # Tile types and properties
├── Rendering/
│   └── TerrainConsoleRenderer.cs  # Console visualization
├── Scenes/
│   └── TerrainDemoScene.cs   # Interactive demo scene
└── Tests/
    └── TerrainGenerationTest.cs  # Test suite
```

## API Examples

### Generate a Single Chunk

```csharp
var generator = new TerrainGenerator(seed: 12345);
var chunk = new Chunk(chunkX: 0);
generator.GenerateChunk(chunk);
```

### Use Chunk Manager

```csharp
var chunkManager = new ChunkManager();
chunkManager.SetTerrainGenerator(new TerrainGenerator());

// Get tile at world coordinates
var tile = chunkManager.GetTile(worldX: 50, worldY: 10);

// Update chunks based on player position
chunkManager.UpdateChunks(playerWorldX);
```

### Render Terrain

```csharp
var renderer = new TerrainConsoleRenderer();
renderer.Render(world, chunkManager, fps);
```

## Tile Types

| Type | Symbol | Color | Description |
|------|--------|-------|-------------|
| Air | ` ` | Black | Empty space |
| Grass | `#` | Green | Surface grass |
| Dirt | `=` | DarkYellow | Topsoil |
| Stone | `█` | Gray | Underground stone |
| DeepStone | `▓` | DarkGray | Deep underground |
| Bedrock | `■` | Black | Unbreakable floor |
| Sand | `≈` | Yellow | Desert surface |
| Water | `~` | Blue | Water blocks |
| Snow | `*` | White | Snow biome |
| CopperOre | `C` | DarkCyan | Copper deposits |
| IronOre | `I` | DarkRed | Iron deposits |
| GoldOre | `G` | DarkYellow | Gold deposits |

## Performance

- Chunk generation: <30ms per chunk (target)
- Memory: ~4KB per chunk (32×30×1 byte)
- Render distance: 2 chunks (configurable)
- Target FPS: 60

## Configuration

Key parameters in `TerrainGenerator.cs`:
```csharp
SURFACE_FREQUENCY = 0.03f;   // Surface terrain smoothness
BIOME_FREQUENCY = 0.005f;    // Biome zone size
CAVE_FREQUENCY = 0.08f;      // Cave pocket density
```

## Future Enhancements (Phase 2)

- [ ] More biome types (8+ total)
- [ ] Tree and vegetation generation
- [ ] Water bodies (rivers, lakes)
- [ ] Block modification system
- [ ] Underground lighting
- [ ] Structure generation (ruins, dungeons)
- [ ] Multithreaded chunk generation

## Testing

The test suite validates:
- Chunk coordinate conversion
- Terrain height variation
- Biome transitions
- Cave generation
- Ore distribution
- Cross-section visualization

Run tests with: `dotnet run test`

## References

- [ROADMAP.md](../../ROADMAP.md) - Full development roadmap
- [SimplexNoise Library](https://www.nuget.org/packages/SimplexNoise/) - Noise generation
- Inspired by: Terraria, Starbound, Minecraft
