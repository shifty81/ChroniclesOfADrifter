# Vegetation Generation System

The vegetation generation system adds natural flora to the terrain, creating a more immersive and varied world. Vegetation is biome-specific and uses procedural generation for realistic placement.

## Overview

The vegetation system generates:
- **Trees**: Oak, Pine, Palm
- **Ground cover**: Tall grass, Bushes, Flowers
- **Desert flora**: Cacti

Vegetation is placed on the surface layer of terrain and rendered on top of ground tiles.

## Architecture

### Components

#### TileType Enum (TileComponent.cs)
Vegetation types added to the `TileType` enum:
- `TreeOak` - Oak tree (Forest/Plains)
- `TreePine` - Pine tree (generic)
- `TreePalm` - Palm tree (Desert oases)
- `TallGrass` - Decorative grass (non-blocking)
- `Bush` - Bush/shrub (blocking)
- `Cactus` - Desert cactus (blocking)
- `Flower` - Small flower (non-blocking)

#### Chunk Storage
Vegetation is stored separately from terrain blocks in a dedicated layer:
```csharp
// In Chunk.cs
private TileType?[] vegetation;  // One slot per X column (32 total)
```

This design allows:
- Vegetation to overlay terrain without replacing it
- Easy removal of vegetation without affecting terrain
- Future collision detection (vegetation can be walked through or blocks movement)

### VegetationGenerator Class

The `VegetationGenerator` class handles procedural vegetation placement.

#### Key Methods

**GenerateVegetation(Chunk chunk, BiomeType[] biomeMap)**
- Main entry point for vegetation generation
- Processes each X column in the chunk
- Places vegetation based on biome and noise values

**DetermineVegetation(BiomeType biome, float probability)**
- Determines which vegetation type to place
- Uses biome-specific rules
- Returns null if no vegetation should be placed

**Biome-specific methods**
- `DetermineForestVegetation()` - 60% coverage
- `DeterminePlainsVegetation()` - 30% coverage  
- `DetermineDesertVegetation()` - 5% coverage

## Biome Vegetation Profiles

### Forest Biome
**Coverage**: 60% of surface tiles
```
Trees:       30% (70% oak, 30% pine)
Bushes:      15%
Tall Grass:  10%
Flowers:      5%
```

Dense forest with lots of trees, bushes, and undergrowth. Creates a lush, green environment.

### Plains Biome
**Coverage**: 30% of surface tiles
```
Trees:       10% (oak only)
Tall Grass:  10%
Bushes:       5%
Flowers:      5%
```

Scattered trees and grass with open areas. Provides good visibility and movement space.

### Desert Biome
**Coverage**: 5% of surface tiles
```
Cacti:       3%
Palm Trees:  2%
```

Sparse vegetation reflecting the harsh desert environment. Cacti are most common, with occasional palm trees near water sources.

## Procedural Generation

### Noise-Based Placement

Vegetation placement uses Simplex noise for natural-looking distribution:

```csharp
float vegetationNoise = SimplexNoise.Noise.CalcPixel1D(worldX, VEGETATION_FREQUENCY);
float probability = vegetationNoise / 255.0f;
```

- **Frequency**: 0.15 (creates natural-looking clusters)
- **Probability**: 0.0 to 1.0 range from noise
- **Biome rules**: Applied on top of noise for final placement

### Surface Detection

Vegetation is only placed on valid surface blocks:
- Grass (Forest, Plains)
- Sand (Desert)
- Dirt
- Snow (future biomes)

The system automatically detects the surface height for each column.

## Rendering

### Console Rendering

Vegetation is rendered with Unicode characters and colors:

| Type      | Char | Color      |
|-----------|------|------------|
| TreeOak   | ♣    | DarkGreen  |
| TreePine  | ♠    | DarkGreen  |
| TreePalm  | Ψ    | Green      |
| TallGrass | "    | Green      |
| Bush      | ♠    | DarkGreen  |
| Cactus    | ‡    | Green      |
| Flower    | ✿    | Magenta    |

### Rendering Priority

Vegetation is rendered on top of terrain:
1. Background terrain (dirt, stone, etc.)
2. Surface blocks (grass, sand)
3. **Vegetation layer** ← Rendered here
4. Entities (player, NPCs)

## Collision Detection

### Current Implementation
- `TallGrass` and `Flower`: Non-solid (walkable)
- All other vegetation: Solid (blocks movement)

### Future Enhancement
The vegetation system is designed to support:
- Player collision with trees and bushes
- Tools to remove vegetation (axe for trees)
- Vegetation drops (wood, seeds, etc.)
- Regrowth mechanics

## Integration with Terrain Generation

Vegetation is generated after terrain in `TerrainGenerator.GenerateChunk()`:

```csharp
// 1. Generate terrain tiles (ground, stone, ores)
for (int localX = 0; localX < Chunk.CHUNK_WIDTH; localX++) {
    // ... generate terrain column
}

// 2. Generate vegetation on surface
vegetationGenerator.GenerateVegetation(chunk, biomeMap);

// 3. Mark chunk as generated
chunk.SetGenerated();
```

This ensures vegetation is placed on fully-generated terrain.

## Testing

### Unit Tests (VegetationGenerationTest.cs)

The test suite validates:
1. **Vegetation Types**: All types have valid chars and colors
2. **Biome Density**: Each biome has appropriate vegetation coverage
3. **Chunk Storage**: Vegetation can be set and retrieved correctly
4. **Rendering Integration**: Vegetation is accessible through ChunkManager

Run tests with:
```bash
dotnet run -- vegetation-test
```

### Visual Testing

Run the terrain demo to see vegetation in action:
```bash
dotnet run -- terrain
```

Look for:
- Dense forests (♣ ♠) with scattered bushes
- Open plains (#) with occasional trees
- Sparse desert (≈) with rare cacti (‡)

## Performance Considerations

### Memory Usage
- **Per chunk**: 32 bytes (32 nullable TileType values)
- **100 chunks**: ~3.2 KB (negligible)

### Generation Speed
- Vegetation generation adds <5ms per chunk
- Uses same noise seed as terrain for consistency
- No additional noise calculations (reuses terrain noise)

### Optimization Opportunities
1. Batch vegetation rendering
2. Cull off-screen vegetation
3. LOD for distant vegetation (future)

## Future Enhancements

### Planned Features
1. **More vegetation types**: Mushrooms, reeds, seaweed
2. **Seasonal variation**: Color changes, leaf fall
3. **Growth system**: Saplings grow into trees over time
4. **Biome-specific trees**: Jungle trees, swamp willows
5. **Underground fungi**: Glowing mushrooms in caves
6. **Harvestable plants**: Berry bushes, herbs

### Advanced Features
1. **Wind animation**: Trees sway in the breeze
2. **Fire spread**: Vegetation burns and spreads fire
3. **Dynamic vegetation**: Trampled grass, chopped trees
4. **Ecosystem simulation**: Plants affect animal spawning

## API Reference

### Chunk Methods
```csharp
// Get vegetation at local X coordinate
TileType? GetVegetation(int localX)

// Set vegetation at local X coordinate  
void SetVegetation(int localX, TileType? type)
```

### ChunkManager Methods
```csharp
// Get vegetation at world X coordinate
TileType? GetVegetation(int worldX)

// Set vegetation at world X coordinate
void SetVegetation(int worldX, TileType? type)
```

### TileType Extensions
```csharp
// Check if a tile is vegetation
bool IsVegetation(this TileType type)

// Check if vegetation blocks movement
bool IsSolid(this TileType type)

// Get display character
char GetChar(this TileType type)

// Get display color
ConsoleColor GetColor(this TileType type)
```

## Examples

### Clearing Vegetation
```csharp
// Remove tree at world position X=50
chunkManager.SetVegetation(50, null);
```

### Checking for Vegetation
```csharp
int worldX = 100;
var veg = chunkManager.GetVegetation(worldX);

if (veg.HasValue && veg.Value.IsVegetation())
{
    Console.WriteLine($"Found {veg.Value} at X={worldX}");
}
```

### Custom Vegetation Placement
```csharp
// Plant a forest manually
for (int x = 0; x < 32; x++)
{
    if (x % 3 == 0)  // Every 3rd tile
    {
        chunk.SetVegetation(x, TileType.TreeOak);
    }
}
```

## Troubleshooting

### No Vegetation Visible
1. Check biome - Desert has only 5% coverage
2. Verify chunk is generated: `chunk.IsGenerated`
3. Ensure vegetation rendering is enabled
4. Check camera viewport includes surface layer

### Wrong Vegetation for Biome
1. Verify biome detection: `GetBiomeAt(worldX)`
2. Check seed consistency between terrain and vegetation
3. Validate noise frequency (should be 0.15)

### Performance Issues
1. Reduce render distance if needed
2. Check for excessive chunk generation
3. Profile vegetation generation time

## Credits

The vegetation system was inspired by:
- **Terraria**: Biome-specific vegetation
- **Minecraft**: Surface decoration
- **Starbound**: Procedural flora placement

## Changelog

### Version 1.0 (Current)
- Initial vegetation system implementation
- 7 vegetation types (trees, grass, bushes, etc.)
- Biome-specific generation rules
- Noise-based procedural placement
- Console rendering with Unicode characters
- Unit test suite
