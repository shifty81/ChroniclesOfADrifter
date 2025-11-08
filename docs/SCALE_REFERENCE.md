# Game Scale and Constants Reference

## Overview

This document defines the fundamental scale and proportions used throughout Chronicles of a Drifter. All measurements and procedural generation should reference these constants to maintain consistent scale and proper proportions.

## Core Scale Reference

### Player Character Dimensions

**The player character is approximately 2.5 blocks tall.** This is the foundational measurement for the entire game world.

```
Player Height: 2.5 blocks (80 pixels at 32px/block)
Player Width: 0.8 blocks (26 pixels at 32px/block)
Block Size: 32 pixels (at 1:1 rendering scale)
```

Visual reference:
```
┌────┐
│    │  Block 3 (headroom for doors/tunnels)
├────┤
│    │  Block 2
│ 🧍 │
│    │  Block 1
├────┤
│    │  Block 0 (ground)
└────┘
```

### Why This Matters

The player scale affects everything in the game:
- **Terrain Generation**: Cave heights, cliff faces, terrain features
- **Structure Design**: Door sizes, room heights, building proportions
- **Vegetation**: Tree heights, bush sizes, grass scale
- **Enemy Design**: NPC and monster sizes relative to the player
- **Object Placement**: Item sizes, furniture, obstacles

## Scale Constants

All scale constants are defined in `src/Game/GameConstants.cs`:

### Player Dimensions

```csharp
// Height and width in blocks
PlayerHeightInBlocks = 2.5f
PlayerWidthInBlocks = 0.8f

// Collision box dimensions (pixels at 1:1 scale)
PlayerCollisionHeight = 80f  // 2.5 blocks * 32 px/block
PlayerCollisionWidth = 26f   // ~0.8 blocks * 32 px/block
```

### World Dimensions

```csharp
BlockSize = 32              // Pixels per block at 1:1 scale
ChunkWidth = 32             // Blocks per chunk horizontally
ChunkHeight = 30            // Blocks per chunk vertically
```

### Architectural Guidelines

```csharp
MinDoorHeight = 3.0f        // Blocks (player + headroom)
MinTunnelHeight = 3.0f      // Blocks (comfortable passage)
StructureCeilingHeight = 4.0f  // Blocks (spacious feel)
```

### Vegetation Scale

```csharp
MinTreeHeight = 4.0f        // Blocks (clearly taller than player)
MaxTreeHeight = 8.0f        // Blocks (variety in forest)
```

## Scale Guidelines by Feature

### Terrain Generation

When generating terrain features:

- **Caves and Tunnels**: Minimum 3 blocks high (96 pixels)
  - Main passages: 4-5 blocks high
  - Small crawlspaces: 1.5-2 blocks high (crouching/special movement)

- **Cliff Faces**: 8-20 blocks tall
  - Should feel imposing relative to player
  - Create sense of verticality

- **Hills and Mountains**: 10-50 blocks tall
  - Vary height for visual interest
  - Player can climb in ~2.5 block increments

### Structure Design

When creating buildings or dungeons:

- **Doors**: 3 blocks tall × 1.5 blocks wide
  - Allows comfortable player passage
  - Room for visual decoration (frame, arch)

- **Windows**: 1-2 blocks tall
  - Positioned 1-2 blocks above floor
  - Width varies by style (1-3 blocks)

- **Room Ceilings**: 4 blocks high (standard)
  - Grand halls: 6-8 blocks high
  - Cramped spaces: 2.5-3 blocks high

- **Stairs**: 0.5 blocks rise per step
  - 5 steps = 2.5 blocks (player height)
  - Comfortable climbing ratio

### Vegetation and Objects

When placing or generating props:

- **Trees**:
  - Small trees: 4-5 blocks (forest variety)
  - Medium trees: 6-7 blocks (common)
  - Large trees: 8-10 blocks (landmarks)
  - Ancient/special trees: 10-15 blocks

- **Bushes and Shrubs**: 0.5-1.5 blocks tall
  - Below player eye level
  - Provide cover but allow combat

- **Grass**: 0.2-0.4 blocks tall
  - Visual detail
  - Non-blocking decoration

- **Furniture**:
  - Tables: 1.2 blocks high
  - Chairs: 0.8 blocks high (seat), 1.5 blocks total
  - Beds: 0.5 blocks high, 2.5 blocks long
  - Chests: 1 block tall, 1.5 blocks wide

### Enemy and NPC Sizing

Scale enemies relative to player for visual communication:

- **Small Enemies**: 0.5-1.5 blocks (goblins, rats, small creatures)
- **Human-sized**: 2-3 blocks (bandits, guards, most humanoids)
- **Large Enemies**: 4-6 blocks (ogres, trolls, elite warriors)
- **Boss Enemies**: 8-16 blocks (dragons, giants, major threats)

Rule of thumb: Players should immediately understand threat level by size comparison.

## Code Examples

### Creating Properly Scaled Player

```csharp
using ChroniclesOfADrifter;

var player = World.CreateEntity();
World.AddComponent(player, new PositionComponent(x, y));
World.AddComponent(player, new PlayerComponent { 
    Speed = GameConstants.DefaultPlayerSpeed 
});

// Use standard player collision dimensions
World.AddComponent(player, new CollisionComponent(
    width: GameConstants.PlayerCollisionWidth,
    height: GameConstants.PlayerCollisionHeight,
    layer: CollisionLayer.Player,
    collidesWith: CollisionLayer.All
));
```

### Procedural Door Generation

```csharp
using ChroniclesOfADrifter;

// Ensure door is tall enough for player
float doorHeight = GameConstants.MinDoorHeight * GameConstants.BlockSize; // 96 pixels
float doorWidth = 1.5f * GameConstants.BlockSize; // 48 pixels

// Generate door at position
GenerateDoor(worldX, worldY, doorWidth, doorHeight);
```

### Procedural Tree Generation

```csharp
using ChroniclesOfADrifter;

// Random tree height in appropriate range
float treeHeightBlocks = Random.Range(
    GameConstants.MinTreeHeight,
    GameConstants.MaxTreeHeight
);

float treeHeightPixels = treeHeightBlocks * GameConstants.BlockSize;
PlaceTree(worldX, worldY, treeHeightPixels);
```

### Cave Generation with Proper Clearance

```csharp
using ChroniclesOfADrifter;

// Ensure cave passage is navigable
float minCaveHeight = GameConstants.MinTunnelHeight * GameConstants.BlockSize;

if (caveSegmentHeight < minCaveHeight)
{
    caveSegmentHeight = minCaveHeight;
}

GenerateCaveSegment(startX, startY, endX, endY, caveSegmentHeight);
```

## Testing Scale

When implementing new features, always test with player scale in mind:

1. **Visual Test**: Can you see the player clearly against the feature?
2. **Navigation Test**: Can the player move through/around it comfortably?
3. **Proportion Test**: Does it look right relative to the 2.5-block-tall player?
4. **Gameplay Test**: Is it clear what the player can/cannot interact with?

## Common Mistakes to Avoid

❌ **Don't hardcode dimensions** - Use `GameConstants` instead
```csharp
// Bad
var collision = new CollisionComponent(28, 80);

// Good
var collision = new CollisionComponent(
    GameConstants.PlayerCollisionWidth,
    GameConstants.PlayerCollisionHeight
);
```

❌ **Don't ignore player scale in procedural generation**
```csharp
// Bad - player can't fit through
float caveHeight = Random.Range(1.0f, 2.0f) * BlockSize;

// Good - player can navigate
float caveHeight = Random.Range(
    GameConstants.MinTunnelHeight,
    GameConstants.MinTunnelHeight + 3.0f
) * GameConstants.BlockSize;
```

❌ **Don't create scale inconsistencies**
```csharp
// Bad - arbitrary values
float doorHeight = 100; // pixels, but why 100?
float windowHeight = 45; // pixels, arbitrary

// Good - based on player scale
float doorHeight = GameConstants.MinDoorHeight * GameConstants.BlockSize;
float windowHeight = 1.5f * GameConstants.BlockSize; // 1.5 blocks
```

## Related Documentation

- [BUILD_SETUP.md](BUILD_SETUP.md) - Build instructions and scale constants
- [TERRAIN_GENERATION.md](TERRAIN_GENERATION.md) - Terrain generation with proper scale
- [PROCEDURAL_GENERATION.md](PROCEDURAL_GENERATION.md) - Dungeon generation guidelines
- [SPRITE_ASSETS.md](SPRITE_ASSETS.md) - Creating properly scaled sprite assets

## Summary

**Remember: The player is 2.5 blocks tall. Everything else scales from there.**

Use `GameConstants.cs` for all scale-related values. This ensures consistency across:
- Procedural generation
- Asset creation
- Collision detection
- UI and camera systems
- Level design tools

When in doubt, place a player character next to your feature and verify it looks proportional and feels right to navigate.
