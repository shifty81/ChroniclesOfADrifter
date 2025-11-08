# Water Generation System

## Overview

The Water Generation System adds dynamic water bodies to the procedurally generated 2D terrain in Chronicles of a Drifter. Water bodies include rivers, lakes, and oceans that integrate naturally with the existing biome and terrain systems.

## Water Types

### Rivers
- **Depth**: 2 blocks deep (shallow)
- **Appearance**: Narrow, meandering water channels
- **Biome Distribution**: Appear in most biomes except Desert and Snow
- **Frequency**: Occasional (controlled by noise threshold)
- **Purpose**: Connect different water bodies, provide water sources

### Lakes
- **Depth**: 3 blocks deep (medium)
- **Appearance**: Wider water bodies in natural depressions
- **Biome Distribution**: Common in Swamp, Forest, and Plains biomes
- **Frequency**: Occasional
- **Purpose**: Natural water features, exploration landmarks

### Oceans
- **Depth**: 5 blocks deep (deep water)
- **Appearance**: Large expanses of water
- **Biome Distribution**: Primarily in Beach biomes
- **Frequency**: Rare
- **Purpose**: Major water features, coastal areas

## Technical Implementation

### WaterGenerator Class

Located in `src/Game/World/WaterGenerator.cs`, this class handles all water body generation.

```csharp
public class WaterGenerator
{
    // Noise frequencies for different water types
    private const float RIVER_FREQUENCY = 0.005f;
    private const float LAKE_FREQUENCY = 0.008f;
    private const float OCEAN_FREQUENCY = 0.002f;
    
    // Generation thresholds (0-1 range)
    private const float RIVER_THRESHOLD = 0.60f;
    private const float LAKE_THRESHOLD = 0.65f;
    private const float OCEAN_THRESHOLD = 0.70f;
}
```

### Generation Pipeline

Water generation occurs in the terrain generation pipeline:

1. **Terrain Generation**: Base terrain with surface and underground layers
2. **Water Generation** ← New step
   - Check biome type for each column
   - Use Simplex noise to determine water placement
   - Generate water bodies based on thresholds
3. **Vegetation Generation**: Trees and plants (avoid water tiles)

### Noise-Based Generation

Water bodies use Simplex noise for natural, organic patterns:

```csharp
// River generation uses two noise layers for meandering patterns
float riverNoise1 = SimplexNoise.Noise.CalcPixel1D(worldX, RIVER_FREQUENCY);
float riverNoise2 = SimplexNoise.Noise.CalcPixel1D(worldX + 5000, RIVER_FREQUENCY * 1.5f);
float combinedNoise = (riverNoise1 + riverNoise2 * 0.5f) / (255.0f * 1.5f);

if (combinedNoise > RIVER_THRESHOLD)
{
    GenerateRiverColumn(chunk, localX);
}
```

### Biome-Specific Rules

Different biomes have different water generation rules:

| Biome   | Rivers | Lakes | Oceans |
|---------|--------|-------|--------|
| Plains  | ✓      | ✓     | ✗      |
| Desert  | ✗      | ✗     | ✗      |
| Forest  | ✓      | ✓     | ✗      |
| Snow    | ✗      | ✗     | ✗      |
| Swamp   | ✓      | ✓     | ✗      |
| Rocky   | ✓      | ✗     | ✗      |
| Jungle  | ✓      | ✓     | ✗      |
| Beach   | ✓      | ✓     | ✓      |

## Water Properties

### Tile Type
Water uses the existing `TileType.Water` enum value, which has these properties:

- **Character**: `~` (tilde)
- **Color**: Blue (ConsoleColor.Blue)
- **Solid**: No (players can swim through)
- **Mineable**: Yes (can be removed)

### Future Properties (To Be Implemented)
- **Flow**: Water should flow downward
- **Transparency**: Semi-transparent rendering
- **Animation**: Water surface animation
- **Physics**: Swimming mechanics, drowning
- **Interaction**: Water blocks lava, extinguishes fire

## Testing

### Water Generation Test

Run with: `dotnet run water-test`

The test generates 10 chunks and reports:
- Total water tiles generated
- Count of rivers, lakes, and oceans
- Water body locations
- Visual cross-section showing water placement

Example output:
```
[Test 2] Scanning for water tiles...
  Total water tiles: 69
  River water: 36 tiles
  Lake water: 33 tiles
  Ocean water: 0 tiles
  Unique water bodies: 29
✓ Water generation is working!
```

## Configuration

Water generation can be tuned by adjusting constants in `WaterGenerator.cs`:

### Frequency Constants
- Lower values = smoother, larger water bodies
- Higher values = more chaotic, smaller water bodies
- **RIVER_FREQUENCY**: 0.005 (very smooth rivers)
- **LAKE_FREQUENCY**: 0.008 (medium-sized lakes)
- **OCEAN_FREQUENCY**: 0.002 (very large ocean zones)

### Threshold Constants
- Lower values = more water
- Higher values = less water
- **RIVER_THRESHOLD**: 0.60 (fairly common)
- **LAKE_THRESHOLD**: 0.65 (occasional)
- **OCEAN_THRESHOLD**: 0.70 (rare)

### Depth Constants
- **RIVER_DEPTH**: 2 blocks
- **LAKE_DEPTH**: 3 blocks
- **OCEAN_DEPTH**: 5 blocks

## Integration with Other Systems

### Terrain Generator
The `TerrainGenerator` class creates a `WaterGenerator` instance and calls it:

```csharp
// Generate water bodies after terrain but before vegetation
waterGenerator.GenerateWater(chunk, biomeMap);
```

### Vegetation Generator
Vegetation should avoid water tiles (already implemented in vegetation system).

### Lighting System
Water tiles currently use ambient lighting. Future enhancements:
- Underwater lighting (darker)
- Light reflection on water surface
- Transparency effects

### Mining/Building System
Water can be mined/removed like other blocks, but:
- Future: Water should flow to fill empty spaces
- Future: Source blocks for infinite water

## Future Enhancements

### Phase 1: Basic Improvements
- [ ] Water flow mechanics (simple gravity-based)
- [ ] Animated water surface
- [ ] Splash effects when entering water

### Phase 2: Advanced Features
- [ ] Swimming mechanics for player
- [ ] Drowning mechanic
- [ ] Underwater breathing (oxygen system)
- [ ] Water current mechanics

### Phase 3: Complex Systems
- [ ] Fish and underwater creatures
- [ ] Boats and water transportation
- [ ] Fishing system
- [ ] Underwater caves and structures
- [ ] Water pollution/purification

### Phase 4: Polish
- [ ] Reflection rendering
- [ ] Transparency with depth-based opacity
- [ ] Wave animations
- [ ] Water sound effects
- [ ] Weather effects (rain fills water)

## Performance Considerations

### Memory
- Water tiles use the same memory as any other tile
- No additional per-tile data structures needed

### Generation Speed
- Water generation adds ~5% to chunk generation time
- Uses same noise functions as terrain (cached)
- O(n) complexity where n = chunk width

### Optimization Tips
1. Water generation is deterministic (same seed = same water)
2. Can skip water generation in dry biomes (Desert, Snow)
3. Noise calculations are already optimized by SimplexNoise library

## Examples

### Small River
```
  6: ###=====###
  7: #=========###
  8: ##~~~====####
  9: ##~~~=======##
 10: ##~~~========
 11: ###~~=========
 12: ###============
```

### Lake in Forest
```
  5: ♣##########♣
  6: #==========#
  7: #===~~~====#
  8: #===~~~====#
  9: #===~~~====#
 10: #==========♣
```

### Ocean in Beach Biome
```
  4: ≈≈≈≈≈≈≈≈
  5: ≈≈≈≈≈≈≈≈
  6: ~~~~~~~~~~
  7: ~~~~~~~~~~
  8: ~~~~~~~~~~
  9: ~~~~~~~~~~
 10: ~~~~~~~~~~
 11: ≈≈≈≈≈≈≈≈≈≈
```

## References

- [Terrain Generation](TERRAIN_GENERATION.md) - How terrain is generated
- [Vegetation System](VEGETATION_SYSTEM.md) - How vegetation interacts with water
- [Mining & Building](MINING_BUILDING_SYSTEM.md) - How water blocks can be mined

## Related Code Files

- `src/Game/World/WaterGenerator.cs` - Main water generation class
- `src/Game/World/TerrainGenerator.cs` - Calls water generation
- `src/Game/Tests/WaterGenerationTest.cs` - Water generation tests
- `src/Game/ECS/Components/TileComponent.cs` - Water tile definition

## Changelog

### 2025-11-08: Initial Implementation
- Created WaterGenerator class
- Implemented rivers, lakes, and oceans
- Added biome-specific water rules
- Created water generation tests
- Integrated with terrain generation pipeline
