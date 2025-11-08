# Underground Lighting and Fog of War System

## Overview

This implementation adds a comprehensive underground lighting and fog of war system to Chronicles of a Drifter, enhancing the underground exploration experience with dynamic lighting, darkness, and strategic torch placement.

## Features Implemented

### 1. Core Lighting Components

#### LightSourceComponent
- Represents light-emitting entities (player lantern, torches, etc.)
- Properties:
  - `Radius`: Light emission distance in blocks (float)
  - `Intensity`: Light brightness 0.0-1.0 (float)
  - `IsActive`: Whether light is currently on (bool)
  - `Type`: Type of light source (enum: Player, Torch, GlowingOre, Sunlight)

#### LightingComponent
- Tracks lighting and visibility state for tiles/entities
- Properties:
  - `LightLevel`: Current brightness 0.0 (dark) to 1.0 (bright)
  - `IsExplored`: Whether player has discovered this tile (permanent)
  - `IsCurrentlyVisible`: Whether tile is currently lit and visible

### 2. Lighting System

#### Ambient Light Calculation
- **Surface (Y < 10)**: Full daylight (1.0 brightness)
- **Shallow Underground (Y 10-19)**: Dim light (0.3 to 0.0 gradient)
- **Deep Underground (Y >= 20)**: Pitch black (0.0 brightness)

#### Light Propagation
- Distance-based radial falloff from light sources
- Light intensity = `source_intensity * (1.0 - distance / radius)`
- Checks 8-block radius around each light source
- Multiple light sources combine (takes maximum light level)

#### Fog of War
- Tiles start unexplored and invisible
- Once lit and visible, tiles become "explored" (permanent)
- Explored tiles remain dimly visible even when not lit
- Currently lit tiles shown at full brightness

### 3. Torch System

#### Torch Block Type
- New `TileType.Torch` added to tile enum
- Visual representation: '☼' character in yellow
- Properties:
  - Hardness: 0.5 seconds to break
  - Non-solid (doesn't block movement)
  - Emits light: 8-block radius, 1.0 intensity

#### Torch Placement
- Press 'P' key to place blocks/torches from inventory
- Player starts with 10 torches
- Placing a torch creates a `LightSourceComponent` entity at that position
- Torch entities persist and emit light permanently

#### Torch Mining
- Breaking a torch returns it to inventory
- Automatically removes the associated light source entity
- Light immediately stops emitting when torch destroyed

### 4. Player Lighting

#### Personal Lantern
- Player has built-in 8-block radius light source
- Follows player movement automatically
- Always active (can be toggled if desired)
- Allows basic underground navigation

### 5. Visual Rendering

#### TerrainConsoleRenderer Updates
- `ApplyLighting()`: Adjusts tile colors based on light level
- `GetAmbientLightForDepth()`: Calculates depth-based ambient light
- `GetLightFromNearbyLightSources()`: Checks for player/torch lights
- `AdjustColorBrightness()`: Dims colors in low light
  - Light < 0.1: Black (pitch dark)
  - Light < 0.5: Dark variants of colors
  - Light >= 0.5: Normal full colors

## Technical Implementation

### Architecture
```
LightingSystem (ISystem)
├── Initialize() - Gets ChunkManager reference
└── Update() - Called every frame
    ├── ResetLightLevels() - Clear previous frame's lighting
    ├── ApplyAmbientLight() - Add depth-based lighting
    ├── PropagateLightFromSource() - Add light from each source
    └── UpdateVisibilityAndExploration() - Update fog of war
```

### Performance Considerations
- Light calculation runs every frame
- O(n) complexity where n = number of light sources
- Each light source checks 8-block radius (max ~200 blocks)
- Spatial optimization possible but not critical for current scale

## Usage

### Mining Demo
```bash
dotnet run -- mining
```

Controls:
- WASD/Arrows: Move player
- Hold 'M': Mine blocks
- Press 'P': Place blocks/torches
- Press 1-9: Select block type from inventory

### Lighting Test
```bash
dotnet run -- lighting-test
```

Runs automated tests verifying:
- Depth-based ambient lighting
- Light source creation/toggling
- Lighting component state management

## Testing Results

All tests pass successfully:
- ✓ Surface lighting (Y < 10): Bright (1.0)
- ✓ Shallow underground (Y 10-19): Dim (0.3-0.0)
- ✓ Deep underground (Y >= 20): Dark (0.0)
- ✓ Player light source creation
- ✓ Torch light source creation
- ✓ Light source activation/deactivation
- ✓ Lighting component state tracking

## Security

CodeQL scan completed: **0 vulnerabilities found**

## Future Enhancements

Potential improvements:
1. **Spatial indexing**: Use grid-based lookup for faster light queries
2. **Light blocking**: Solid blocks could cast shadows
3. **Colored lighting**: Different light sources with color tinting
4. **Dynamic day/night**: Surface ambient light changes over time
5. **Light sources**: Glowing ores, lava, magical items
6. **Optimization**: Only recalculate lighting when sources move/change
7. **Visual effects**: Flickering torches, light bloom

## Integration Notes

### Required for Future Work
When the C++ rendering engine is built:
- Lighting data can be passed to shaders for GPU-accelerated rendering
- Light levels stored in chunk data for serialization
- Fog of war state saved with world data

### Compatibility
- Works with existing terrain generation
- Compatible with mining/building system
- Integrates with ECS architecture
- Console renderer supports lighting immediately
- Ready for 3D rendering when engine complete

## Files Changed

- `src/Game/ECS/Components/LightSourceComponent.cs` (NEW)
- `src/Game/ECS/Components/LightingComponent.cs` (NEW)
- `src/Game/ECS/Components/TileComponent.cs` (MODIFIED - added Torch type)
- `src/Game/ECS/Systems/LightingSystem.cs` (NEW)
- `src/Game/ECS/Systems/BlockInteractionSystem.cs` (MODIFIED - torch placement)
- `src/Game/Rendering/TerrainConsoleRenderer.cs` (MODIFIED - lighting rendering)
- `src/Game/Scenes/MiningDemoScene.cs` (MODIFIED - added lighting system)
- `src/Game/Tests/LightingTest.cs` (NEW)
- `src/Game/Program.cs` (MODIFIED - added lighting-test command)

## Conclusion

The underground lighting and fog of war system successfully enhances the mining and exploration experience by:
- Adding strategic depth through light management
- Creating atmospheric underground environments
- Encouraging torch placement for navigation
- Providing clear visual feedback about explored areas
- Supporting future rendering improvements

The implementation is complete, tested, secure, and ready for use.
