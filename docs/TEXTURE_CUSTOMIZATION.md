# Texture Customization and World Generation Enhancements

This document describes the texture/tileset customization system and world generation enhancements added to Chronicles of a Drifter.

## Table of Contents

1. [Texture Customization](#texture-customization)
2. [Tileset Variants](#tileset-variants)
3. [World Generation Enhancements](#world-generation-enhancements)
4. [World Generation Presets](#world-generation-presets)
5. [Usage Examples](#usage-examples)
6. [Demo Scene](#demo-scene)

---

## Texture Customization

The `TileCustomizationManager` provides runtime customization of tile appearances, including:

- **Color Themes**: Predefined color schemes for different visual styles
- **Color Overrides**: Custom color assignments for individual tile types
- **Tile-to-Tileset Mapping**: Map game tile types to tileset definitions

### Available Themes

- **Default**: Standard game colors
- **Zelda**: Inspired by A Link to the Past color palette
- **Dark**: Darker, desaturated colors for moody atmosphere
- **Bright**: Vibrant, saturated colors for cheerful look
- **Volcanic**: Red/orange tones for volcanic/hellish environments

### Usage Example

```csharp
var tilesetManager = new TilesetManager();
tilesetManager.CreateDefaultTileset();

var customization = new TileCustomizationManager(tilesetManager);

// Apply a theme
customization.ApplyTheme("Zelda");

// Set custom color override
customization.SetColorOverride(TileType.Grass, 1.0f, 0.0f, 0.0f); // Red grass

// Get tile color with overrides applied
var color = customization.GetTileColor(TileType.Grass);

// Clear specific override
customization.ClearColorOverride(TileType.Grass);

// Clear all overrides
customization.ClearAllColorOverrides();
```

---

## Tileset Variants

The `TilesetVariantManager` enables automatic tile appearance changes based on:

- **Seasons**: Spring, Summer, Fall, Winter
- **Time of Day**: Dawn, Day, Dusk, Night
- **Weather**: Rainy, Snowy, Foggy

### Variant Types

| Variant | Description |
|---------|-------------|
| Spring | Vibrant greens, more flowers |
| Summer | Warm, bright colors |
| Fall | Orange and brown tones |
| Winter | Cold, desaturated colors with snow |
| Dawn | Cool morning light |
| Day | Standard colors |
| Dusk | Warm evening light |
| Night | Dark, desaturated colors |
| Rainy | Darker, saturated colors |
| Snowy | White overlay effect |
| Foggy | Desaturated, misty colors |

### Usage Example

```csharp
var variantManager = new TilesetVariantManager(customizationManager);

// Apply specific variant
variantManager.ApplyVariant(TilesetVariantType.Winter);

// Apply based on time of day (0.0 to 24.0)
variantManager.ApplyTimeVariant(18.0f); // 6 PM = Dusk

// Apply based on season (day of year)
variantManager.ApplySeasonalVariant(45); // Day 45 = Spring

// Apply based on weather
variantManager.ApplyWeatherVariant("rain");

// Reset to default
variantManager.ResetToDefault();
```

---

## World Generation Enhancements

The `TerrainEnhancementSystem` adds advanced features to world generation:

### Features

1. **Biome Blending**
   - Smooth transitions between biomes
   - Natural-looking boundaries using noise
   - Gradual color/tile transitions

2. **Ore Vein Clustering**
   - Realistic ore distribution in clusters/veins
   - Spherical clusters with probability-based placement
   - Depth-based ore type selection

3. **Mini-Biome Features**
   - Small unique features within larger biomes
   - Examples: Oasis in desert, clearing in forest
   - Rare but add variety and interest

4. **Enhanced Cave Generation**
   - Biome-specific cave styles
   - Large caverns, tight tunnels, ice caves, water-filled caves
   - Mushroom caves in jungle biome

5. **Enhanced Terrain Height**
   - Biome-specific height variations
   - Mountain biomes are taller
   - Beach and swamp biomes are flatter
   - Amplified terrain option

### Mini-Biome Types

| Biome | Mini-Feature | Description |
|-------|--------------|-------------|
| Desert | Oasis | Water source with palm trees |
| Forest | Clearing | Open area with flowers |
| Snow | Ice Lake | Frozen lake |
| Rocky | Boulder | Large rock formation |

### Cave Styles

| Biome | Cave Style | Description |
|-------|------------|-------------|
| Desert | Large Caverns | Open, spacious caves |
| Snow | Ice Caves | Frozen underground |
| Swamp | Water Filled | Flooded caves |
| Jungle | Mushroom Caves | Cave vegetation |
| Rocky | Tight Tunnels | Narrow passages |

### Usage Example

```csharp
var enhancement = new TerrainEnhancementSystem(seed);

// Blend between biomes
var blendedBiome = enhancement.BlendBiomes(
    BiomeType.Plains, 
    BiomeType.Forest, 
    0.5f // 50% blend
);

// Generate ore cluster
var oreCluster = enhancement.GenerateOreCluster(
    centerX: 100, 
    centerY: 10, 
    oreType: TileType.IronOre
);

// Get appropriate ore for depth
var random = new Random();
var oreType = enhancement.GetOreTypeForDepth(15, random);

// Generate mini-biome feature
var miniBiome = enhancement.GenerateMiniBiomeFeature(
    worldX: 1000, 
    biome: BiomeType.Desert
);

// Get cave style for biome
var caveStyle = enhancement.GetCaveStyleForBiome(BiomeType.Snow);

// Calculate enhanced surface height
int height = enhancement.CalculateEnhancedSurfaceHeight(
    worldX: 0, 
    biome: BiomeType.Rocky, 
    amplified: true
);
```

---

## World Generation Presets

The `WorldGenerationConfig` class provides predefined world presets with customizable parameters:

### Available Presets

| Preset | Description | Key Features |
|--------|-------------|--------------|
| **Normal** | Balanced standard world | 1.0x all parameters |
| **Large Biomes** | Bigger biomes | 2.5x biome scale |
| **Amplified** | Extreme terrain | 2.0x height variation |
| **Flat** | Mostly flat terrain | 0.2x height, minimal elevation |
| **Islands** | Island-based world | 2.0x water level, scattered land |
| **Caves** | Cave-heavy world | 2.5x cave density, 1.5x ore density |
| **Dense** | Dense vegetation | 2.0x vegetation density |
| **Sparse** | Minimal features | 0.3x vegetation, 0.6x caves |

### Configuration Parameters

```csharp
public class WorldGenerationConfig
{
    public WorldPreset Preset { get; set; }
    public int Seed { get; set; }
    public float BiomeScale { get; set; }
    public float TerrainHeightMultiplier { get; set; }
    public float CaveDensity { get; set; }
    public float VegetationDensity { get; set; }
    public float WaterLevel { get; set; }
    public float OreDensity { get; set; }
    public bool EnableMiniBiomes { get; set; }
    public bool EnableOreVeins { get; set; }
    public bool EnableBiomeBlending { get; set; }
}
```

### Usage Example

```csharp
// Create from preset
var config = WorldGenerationConfig.FromPreset(WorldPreset.Amplified, seed: 12345);

// Get preset description
string desc = WorldGenerationConfig.GetPresetDescription(WorldPreset.Islands);
// Returns: "Island world with scattered landmasses and lots of ocean"

// Create custom configuration
var customConfig = WorldGenerationConfig.CreateCustom(
    seed: 99999,
    biomeScale: 1.5f,
    heightMultiplier: 1.2f,
    caveDensity: 0.8f,
    vegetationDensity: 1.5f
);

// Get all available presets
var allPresets = WorldGenerationConfig.GetAllPresets();
```

---

## Usage Examples

### Complete Integration Example

```csharp
// Initialize all systems
var tilesetManager = new TilesetManager();
tilesetManager.LoadTilesetsFromDirectory("assets/tilesets");

var customization = new TileCustomizationManager(tilesetManager);
var variants = new TilesetVariantManager(customization);

int seed = 12345;
var worldConfig = WorldGenerationConfig.FromPreset(WorldPreset.Normal, seed);
var enhancement = new TerrainEnhancementSystem(seed);

// Apply theme and variant
customization.ApplyTheme("Zelda");
variants.ApplyVariant(TilesetVariantType.Summer);

// Use in terrain generation
var terrainGen = new TerrainGenerator(seed);
// ... integrate enhancement features during generation ...
```

### Dynamic Time/Weather Updates

```csharp
// Update visuals based on game time
public void UpdateVisuals(float gameTime, string currentWeather)
{
    // Update time-based variant
    variantManager.ApplyTimeVariant(gameTime);
    
    // Update weather-based variant if weather changed
    if (currentWeather != lastWeather)
    {
        variantManager.ApplyWeatherVariant(currentWeather);
        lastWeather = currentWeather;
    }
}
```

---

## Demo Scene

An interactive demo scene is included to showcase all features:

### Running the Demo

```csharp
var demo = new CustomizationDemoScene();
demo.Run();
```

### Demo Controls

| Key | Action |
|-----|--------|
| **T** | Cycle color themes |
| **V** | Cycle tileset variants |
| **P** | Cycle world generation presets |
| **D** | Demonstrate time-based variants |
| **W** | Demonstrate weather-based variants |
| **O** | Demonstrate ore clustering |
| **M** | Demonstrate mini-biome features |
| **C** | Demonstrate custom color overrides |
| **I** | Show current configuration info |
| **H** | Show help |
| **ESC** | Exit demo |

### Demo Features

The demo showcases:

1. **Theme Switching**: Cycle through all available color themes
2. **Variant Effects**: See how seasons, time, and weather affect appearance
3. **Ore Clustering**: Visualize ore vein generation
4. **Mini-Biomes**: Examples of special features in each biome
5. **Custom Colors**: Live demonstration of color override system
6. **World Presets**: Compare different world generation settings

---

## Testing

Comprehensive tests are included in `CustomizationEnhancementTests.cs`:

```bash
# Run tests
dotnet test

# Or run from program
ChroniclesOfADrifter.Tests.CustomizationEnhancementTests.RunAllTests();
```

Tests cover:
- TileCustomizationManager functionality
- TilesetVariantManager seasonal/time/weather variants
- TerrainEnhancementSystem biome blending and ore clustering
- WorldGenerationConfig preset creation and validation

---

## Performance Considerations

1. **Color Overrides**: Minimal performance impact, applied at render time
2. **Biome Blending**: Uses noise calculations, consider caching results
3. **Ore Clustering**: Generated per-chunk, amortized cost
4. **Variant Updates**: Only update when time/weather changes, not every frame

---

## Future Enhancements

Potential additions:

- [ ] User-created custom themes saved to JSON
- [ ] Texture packs with actual image files
- [ ] Animated tile variants (flowing water, swaying grass)
- [ ] Biome-specific music/sound themes
- [ ] World generation preview before creating world
- [ ] More mini-biome types (hot springs, meteor craters, etc.)
- [ ] Advanced cave features (underground lakes, lava pools)

---

## See Also

- [Terrain Generation Documentation](TERRAIN_GENERATION.md)
- [Tileset System Documentation](../assets/tilesets/README.md)
- [Map Editor Guide](MAP_EDITOR.md)
- [World Visualization](WORLD_VISUALIZATION_FIX.md)
