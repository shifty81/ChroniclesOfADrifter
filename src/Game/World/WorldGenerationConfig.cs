namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Predefined world generation presets that control various terrain parameters
/// </summary>
public enum WorldPreset
{
    Normal,         // Standard balanced world
    LargeBiomes,    // Bigger biomes, less variation
    Amplified,      // Extreme terrain height variation
    Flat,           // Mostly flat terrain with minimal elevation
    Islands,        // Island-based world with lots of water
    Caves,          // More caves and underground features
    Dense,          // Dense vegetation and features
    Sparse          // Minimal vegetation and features
}

/// <summary>
/// Configuration for world generation based on selected preset
/// </summary>
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
    
    /// <summary>
    /// Create a config from a preset
    /// </summary>
    public static WorldGenerationConfig FromPreset(WorldPreset preset, int? seed = null)
    {
        int actualSeed = seed ?? Environment.TickCount;
        
        return preset switch
        {
            WorldPreset.Normal => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 1.0f,
                TerrainHeightMultiplier = 1.0f,
                CaveDensity = 1.0f,
                VegetationDensity = 1.0f,
                WaterLevel = 1.0f,
                OreDensity = 1.0f,
                EnableMiniBiomes = true,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.LargeBiomes => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 2.5f,           // 2.5x larger biomes
                TerrainHeightMultiplier = 1.0f,
                CaveDensity = 0.8f,
                VegetationDensity = 1.0f,
                WaterLevel = 1.0f,
                OreDensity = 1.0f,
                EnableMiniBiomes = true,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.Amplified => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 1.0f,
                TerrainHeightMultiplier = 2.0f,  // 2x height variation
                CaveDensity = 1.2f,
                VegetationDensity = 0.9f,
                WaterLevel = 0.8f,
                OreDensity = 1.1f,
                EnableMiniBiomes = true,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.Flat => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 1.0f,
                TerrainHeightMultiplier = 0.2f,  // Much flatter terrain
                CaveDensity = 0.5f,
                VegetationDensity = 1.0f,
                WaterLevel = 0.6f,
                OreDensity = 1.0f,
                EnableMiniBiomes = false,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.Islands => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 0.8f,
                TerrainHeightMultiplier = 0.6f,
                CaveDensity = 0.7f,
                VegetationDensity = 1.2f,        // More tropical vegetation
                WaterLevel = 2.0f,               // Much more water
                OreDensity = 0.8f,
                EnableMiniBiomes = true,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.Caves => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 1.0f,
                TerrainHeightMultiplier = 1.2f,
                CaveDensity = 2.5f,              // Much more caves
                VegetationDensity = 0.9f,
                WaterLevel = 1.0f,
                OreDensity = 1.5f,               // More ores in caves
                EnableMiniBiomes = true,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.Dense => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 1.0f,
                TerrainHeightMultiplier = 1.0f,
                CaveDensity = 1.0f,
                VegetationDensity = 2.0f,        // 2x vegetation
                WaterLevel = 1.2f,
                OreDensity = 1.3f,
                EnableMiniBiomes = true,
                EnableOreVeins = true,
                EnableBiomeBlending = true
            },
            
            WorldPreset.Sparse => new WorldGenerationConfig
            {
                Preset = preset,
                Seed = actualSeed,
                BiomeScale = 1.0f,
                TerrainHeightMultiplier = 0.8f,
                CaveDensity = 0.6f,
                VegetationDensity = 0.3f,        // Minimal vegetation
                WaterLevel = 0.7f,
                OreDensity = 0.8f,
                EnableMiniBiomes = false,
                EnableOreVeins = true,
                EnableBiomeBlending = false
            },
            
            _ => FromPreset(WorldPreset.Normal, actualSeed)
        };
    }
    
    /// <summary>
    /// Get a description of the preset
    /// </summary>
    public static string GetPresetDescription(WorldPreset preset)
    {
        return preset switch
        {
            WorldPreset.Normal => "Balanced world with standard biome sizes and features",
            WorldPreset.LargeBiomes => "Larger biomes with less frequent transitions",
            WorldPreset.Amplified => "Extreme terrain with dramatic height variations",
            WorldPreset.Flat => "Mostly flat terrain ideal for building",
            WorldPreset.Islands => "Island world with scattered landmasses and lots of ocean",
            WorldPreset.Caves => "Cave-heavy world with extensive underground systems",
            WorldPreset.Dense => "Dense vegetation and many natural features",
            WorldPreset.Sparse => "Minimal vegetation and features, more open space",
            _ => "Unknown preset"
        };
    }
    
    /// <summary>
    /// Create a custom config with specific parameters
    /// </summary>
    public static WorldGenerationConfig CreateCustom(
        int seed,
        float biomeScale = 1.0f,
        float heightMultiplier = 1.0f,
        float caveDensity = 1.0f,
        float vegetationDensity = 1.0f,
        float waterLevel = 1.0f,
        float oreDensity = 1.0f,
        bool enableMiniBiomes = true,
        bool enableOreVeins = true,
        bool enableBiomeBlending = true)
    {
        return new WorldGenerationConfig
        {
            Preset = WorldPreset.Normal,
            Seed = seed,
            BiomeScale = biomeScale,
            TerrainHeightMultiplier = heightMultiplier,
            CaveDensity = caveDensity,
            VegetationDensity = vegetationDensity,
            WaterLevel = waterLevel,
            OreDensity = oreDensity,
            EnableMiniBiomes = enableMiniBiomes,
            EnableOreVeins = enableOreVeins,
            EnableBiomeBlending = enableBiomeBlending
        };
    }
    
    /// <summary>
    /// Get all available presets
    /// </summary>
    public static IEnumerable<WorldPreset> GetAllPresets()
    {
        return Enum.GetValues<WorldPreset>();
    }
}
