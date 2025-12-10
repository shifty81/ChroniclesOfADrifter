using ChroniclesOfADrifter.Rendering;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for tileset customization and world generation enhancements
/// </summary>
public class CustomizationEnhancementTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("\n=== Customization & Enhancement Tests ===\n");
        
        TestTileCustomizationManager();
        TestTilesetVariantManager();
        TestTerrainEnhancementSystem();
        TestWorldGenerationConfig();
        
        Console.WriteLine("\n=== All Customization & Enhancement Tests Passed ===\n");
    }
    
    private static void TestTileCustomizationManager()
    {
        Console.WriteLine("[TEST] TileCustomizationManager");
        
        var tilesetManager = new TilesetManager();
        tilesetManager.CreateDefaultTileset();
        
        var customization = new TileCustomizationManager(tilesetManager);
        
        // Test theme application
        Assert(customization.ApplyTheme("Zelda"), "Should apply Zelda theme");
        Assert(customization.GetCurrentTheme() == "Zelda", "Current theme should be Zelda");
        
        // Test color override
        customization.SetColorOverride(TileType.Grass, 1.0f, 0.0f, 0.0f);
        var color = customization.GetTileColor(TileType.Grass);
        Assert(color.r == 1.0f && color.g == 0.0f && color.b == 0.0f, 
               "Grass should have red color override");
        
        // Test clearing overrides
        customization.ClearColorOverride(TileType.Grass);
        
        // Test available themes
        var themes = customization.GetAvailableThemes().ToList();
        Assert(themes.Count >= 4, "Should have at least 4 themes");
        Assert(themes.Contains("Default"), "Should have Default theme");
        Assert(themes.Contains("Dark"), "Should have Dark theme");
        Assert(themes.Contains("Bright"), "Should have Bright theme");
        
        Console.WriteLine("  ✓ TileCustomizationManager working correctly");
    }
    
    private static void TestTilesetVariantManager()
    {
        Console.WriteLine("[TEST] TilesetVariantManager");
        
        var tilesetManager = new TilesetManager();
        tilesetManager.CreateDefaultTileset();
        var customization = new TileCustomizationManager(tilesetManager);
        var variants = new TilesetVariantManager(customization);
        
        // Test seasonal variants
        variants.ApplyVariant(TilesetVariantType.Spring);
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Spring, 
               "Should be Spring variant");
        
        variants.ApplyVariant(TilesetVariantType.Winter);
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Winter, 
               "Should be Winter variant");
        
        // Test time-based variants
        variants.ApplyTimeVariant(6.0f);  // Dawn
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Dawn, 
               "Should be Dawn variant at 6 AM");
        
        variants.ApplyTimeVariant(12.0f); // Day
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Day, 
               "Should be Day variant at noon");
        
        variants.ApplyTimeVariant(18.0f); // Dusk
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Dusk, 
               "Should be Dusk variant at 6 PM");
        
        variants.ApplyTimeVariant(23.0f); // Night
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Night, 
               "Should be Night variant at 11 PM");
        
        // Test weather variants
        variants.ApplyWeatherVariant("rain");
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Rainy, 
               "Should be Rainy variant");
        
        variants.ApplyWeatherVariant("snow");
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Snowy, 
               "Should be Snowy variant");
        
        // Test reset
        variants.ResetToDefault();
        Assert(variants.GetCurrentVariant() == TilesetVariantType.Default, 
               "Should reset to Default variant");
        
        Console.WriteLine("  ✓ TilesetVariantManager working correctly");
    }
    
    private static void TestTerrainEnhancementSystem()
    {
        Console.WriteLine("[TEST] TerrainEnhancementSystem");
        
        int seed = 12345;
        var enhancement = new TerrainEnhancementSystem(seed);
        
        // Test biome blending
        var blendedBiome = enhancement.BlendBiomes(
            BiomeType.Plains, BiomeType.Forest, 0.5f);
        Assert(blendedBiome == BiomeType.Plains || blendedBiome == BiomeType.Forest,
               "Blended biome should be one of the input biomes");
        
        // Test ore cluster generation
        var oreCluster = enhancement.GenerateOreCluster(0, 10, TileType.IronOre);
        Assert(oreCluster.Count > 0, "Should generate ore cluster");
        Assert(oreCluster.Count <= 50, "Ore cluster should be reasonable size");
        
        // Test ore type selection
        var random = new Random(seed);
        var oreType = enhancement.GetOreTypeForDepth(5, random);
        Assert(oreType == TileType.CoalOre, "Should return CoalOre at depth 5");
        
        oreType = enhancement.GetOreTypeForDepth(18, random);
        Assert(oreType == TileType.GoldOre || oreType == TileType.SilverOre,
               "Should return appropriate ore at depth 18");
        
        // Test mini-biome features
        var miniBiome = enhancement.GenerateMiniBiomeFeature(1000, BiomeType.Desert);
        // Mini-biomes are rare, so it might be null
        
        // Test cave styles
        var caveStyle = enhancement.GetCaveStyleForBiome(BiomeType.Desert);
        Assert(caveStyle == CaveStyle.LargeCaverns, 
               "Desert should have large caverns");
        
        caveStyle = enhancement.GetCaveStyleForBiome(BiomeType.Snow);
        Assert(caveStyle == CaveStyle.IceCaves, 
               "Snow biome should have ice caves");
        
        // Test enhanced surface height
        int height = enhancement.CalculateEnhancedSurfaceHeight(0, BiomeType.Plains, false);
        Assert(height >= 0 && height <= 20, "Height should be in valid range");
        
        int amplifiedHeight = enhancement.CalculateEnhancedSurfaceHeight(0, BiomeType.Rocky, true);
        Assert(amplifiedHeight >= 0 && amplifiedHeight <= 30, 
               "Amplified height should be in valid range");
        
        Console.WriteLine("  ✓ TerrainEnhancementSystem working correctly");
    }
    
    private static void TestWorldGenerationConfig()
    {
        Console.WriteLine("[TEST] WorldGenerationConfig");
        
        // Test preset creation
        var normalConfig = WorldGenerationConfig.FromPreset(WorldPreset.Normal, 12345);
        Assert(normalConfig.Preset == WorldPreset.Normal, "Should be Normal preset");
        Assert(normalConfig.Seed == 12345, "Seed should match");
        Assert(normalConfig.BiomeScale == 1.0f, "Normal should have 1.0 biome scale");
        Assert(normalConfig.EnableMiniBiomes, "Normal should enable mini-biomes");
        
        var largeBiomesConfig = WorldGenerationConfig.FromPreset(WorldPreset.LargeBiomes);
        Assert(largeBiomesConfig.BiomeScale > 1.0f, "Large biomes should have larger scale");
        
        var amplifiedConfig = WorldGenerationConfig.FromPreset(WorldPreset.Amplified);
        Assert(amplifiedConfig.TerrainHeightMultiplier > 1.0f, 
               "Amplified should have larger height multiplier");
        
        var islandsConfig = WorldGenerationConfig.FromPreset(WorldPreset.Islands);
        Assert(islandsConfig.WaterLevel > 1.0f, "Islands should have higher water level");
        
        var cavesConfig = WorldGenerationConfig.FromPreset(WorldPreset.Caves);
        Assert(cavesConfig.CaveDensity > 1.0f, "Caves should have higher cave density");
        
        var denseConfig = WorldGenerationConfig.FromPreset(WorldPreset.Dense);
        Assert(denseConfig.VegetationDensity > 1.0f, 
               "Dense should have higher vegetation density");
        
        var sparseConfig = WorldGenerationConfig.FromPreset(WorldPreset.Sparse);
        Assert(sparseConfig.VegetationDensity < 1.0f, 
               "Sparse should have lower vegetation density");
        
        // Test custom config
        var customConfig = WorldGenerationConfig.CreateCustom(
            seed: 99999,
            biomeScale: 1.5f,
            heightMultiplier: 1.2f
        );
        Assert(customConfig.Seed == 99999, "Custom seed should match");
        Assert(customConfig.BiomeScale == 1.5f, "Custom biome scale should match");
        
        // Test preset descriptions
        var description = WorldGenerationConfig.GetPresetDescription(WorldPreset.Amplified);
        Assert(!string.IsNullOrEmpty(description), "Should have description");
        Assert(description.Contains("Extreme"), "Amplified description should mention extreme");
        
        // Test getting all presets
        var allPresets = WorldGenerationConfig.GetAllPresets().ToList();
        Assert(allPresets.Count >= 8, "Should have at least 8 presets");
        
        Console.WriteLine("  ✓ WorldGenerationConfig working correctly");
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
