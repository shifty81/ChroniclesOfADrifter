using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Rendering;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing texture customization and world generation enhancements
/// </summary>
public class CustomizationDemoScene
{
    private World world;
    private TilesetManager tilesetManager;
    private TileCustomizationManager customizationManager;
    private TilesetVariantManager variantManager;
    private TerrainEnhancementSystem enhancementSystem;
    private WorldGenerationConfig worldConfig;
    
    private string[] availableThemes;
    private int currentThemeIndex = 0;
    private TilesetVariantType[] availableVariants;
    private int currentVariantIndex = 0;
    private WorldPreset[] availablePresets;
    private int currentPresetIndex = 0;
    
    private float simulatedTimeOfDay = 12.0f; // Start at noon
    
    public void Initialize()
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Texture Customization & World Generation Demo           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        
        world = new World();
        
        // Initialize tileset system
        tilesetManager = new TilesetManager();
        tilesetManager.CreateDefaultTileset();
        tilesetManager.LoadTilesetsFromDirectory("assets/tilesets");
        
        // Initialize customization
        customizationManager = new TileCustomizationManager(tilesetManager);
        variantManager = new TilesetVariantManager(customizationManager);
        
        // Initialize world generation
        int seed = Environment.TickCount;
        worldConfig = WorldGenerationConfig.FromPreset(WorldPreset.Normal, seed);
        enhancementSystem = new TerrainEnhancementSystem(seed);
        
        // Get available options
        availableThemes = customizationManager.GetAvailableThemes().ToArray();
        availableVariants = Enum.GetValues<TilesetVariantType>();
        availablePresets = WorldGenerationConfig.GetAllPresets().ToArray();
        
        DisplayInfo();
        ShowCommands();
    }
    
    public void Run()
    {
        Initialize();
        
        Console.WriteLine("\nDemo running. Press ESC to exit...\n");
        
        bool running = true;
        while (running)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                running = HandleInput(key);
            }
            
            Thread.Sleep(100);
        }
        
        Console.WriteLine("\nDemo ended.");
    }
    
    private bool HandleInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Escape:
                return false;
            
            case ConsoleKey.T: // Cycle themes
                currentThemeIndex = (currentThemeIndex + 1) % availableThemes.Length;
                customizationManager.ApplyTheme(availableThemes[currentThemeIndex]);
                Console.WriteLine($"\n→ Applied theme: {availableThemes[currentThemeIndex]}");
                ShowTileColors();
                break;
            
            case ConsoleKey.V: // Cycle variants
                currentVariantIndex = (currentVariantIndex + 1) % availableVariants.Length;
                variantManager.ApplyVariant(availableVariants[currentVariantIndex]);
                Console.WriteLine($"\n→ Applied variant: {availableVariants[currentVariantIndex]}");
                ShowTileColors();
                break;
            
            case ConsoleKey.P: // Cycle world presets
                currentPresetIndex = (currentPresetIndex + 1) % availablePresets.Length;
                worldConfig = WorldGenerationConfig.FromPreset(
                    availablePresets[currentPresetIndex], 
                    worldConfig.Seed);
                Console.WriteLine($"\n→ World Preset: {availablePresets[currentPresetIndex]}");
                Console.WriteLine($"  {WorldGenerationConfig.GetPresetDescription(availablePresets[currentPresetIndex])}");
                ShowWorldConfig();
                break;
            
            case ConsoleKey.D: // Show demo of time-based variants
                DemonstrateTimeVariants();
                break;
            
            case ConsoleKey.W: // Show demo of weather variants
                DemonstrateWeatherVariants();
                break;
            
            case ConsoleKey.O: // Show ore clustering demo
                DemonstrateOreClustering();
                break;
            
            case ConsoleKey.M: // Show mini-biome demo
                DemonstrateMiniBiomes();
                break;
            
            case ConsoleKey.C: // Show custom color override
                DemonstrateCustomColors();
                break;
            
            case ConsoleKey.H: // Show help
                ShowCommands();
                break;
            
            case ConsoleKey.I: // Show current info
                DisplayInfo();
                break;
        }
        
        return true;
    }
    
    private void ShowCommands()
    {
        Console.WriteLine("\n═══ Commands ═══");
        Console.WriteLine("  T - Cycle color themes (Default, Zelda, Dark, Bright, Volcanic)");
        Console.WriteLine("  V - Cycle tileset variants (Spring, Summer, Fall, Winter, etc.)");
        Console.WriteLine("  P - Cycle world generation presets");
        Console.WriteLine("  D - Demonstrate time-based variants");
        Console.WriteLine("  W - Demonstrate weather-based variants");
        Console.WriteLine("  O - Demonstrate ore clustering");
        Console.WriteLine("  M - Demonstrate mini-biome features");
        Console.WriteLine("  C - Demonstrate custom color overrides");
        Console.WriteLine("  I - Show current configuration info");
        Console.WriteLine("  H - Show this help");
        Console.WriteLine("  ESC - Exit demo\n");
    }
    
    private void DisplayInfo()
    {
        Console.WriteLine("\n═══ Current Configuration ═══");
        Console.WriteLine($"Theme: {customizationManager.GetCurrentTheme()}");
        Console.WriteLine($"Variant: {variantManager.GetCurrentVariant()}");
        Console.WriteLine($"World Preset: {worldConfig.Preset}");
        Console.WriteLine($"Seed: {worldConfig.Seed}");
        ShowTileColors();
        ShowWorldConfig();
    }
    
    private void ShowTileColors()
    {
        Console.WriteLine("\n--- Tile Colors ---");
        var tileTypes = new[] { 
            TileType.Grass, TileType.Dirt, TileType.Stone, 
            TileType.Water, TileType.Sand, TileType.TreeOak 
        };
        
        foreach (var tileType in tileTypes)
        {
            var color = customizationManager.GetTileColor(tileType);
            Console.WriteLine($"  {tileType,-12}: RGB({color.r:F2}, {color.g:F2}, {color.b:F2})");
        }
    }
    
    private void ShowWorldConfig()
    {
        Console.WriteLine("\n--- World Generation Config ---");
        Console.WriteLine($"  Biome Scale: {worldConfig.BiomeScale:F1}x");
        Console.WriteLine($"  Height Multiplier: {worldConfig.TerrainHeightMultiplier:F1}x");
        Console.WriteLine($"  Cave Density: {worldConfig.CaveDensity:F1}x");
        Console.WriteLine($"  Vegetation Density: {worldConfig.VegetationDensity:F1}x");
        Console.WriteLine($"  Water Level: {worldConfig.WaterLevel:F1}x");
        Console.WriteLine($"  Ore Density: {worldConfig.OreDensity:F1}x");
        Console.WriteLine($"  Mini-biomes: {(worldConfig.EnableMiniBiomes ? "Enabled" : "Disabled")}");
        Console.WriteLine($"  Ore Veins: {(worldConfig.EnableOreVeins ? "Enabled" : "Disabled")}");
        Console.WriteLine($"  Biome Blending: {(worldConfig.EnableBiomeBlending ? "Enabled" : "Disabled")}");
    }
    
    private void DemonstrateTimeVariants()
    {
        Console.WriteLine("\n═══ Time-Based Variants Demo ═══");
        var times = new[] { 
            (6.0f, "Dawn"), 
            (12.0f, "Noon"), 
            (18.0f, "Dusk"), 
            (23.0f, "Night") 
        };
        
        foreach (var (time, name) in times)
        {
            Console.WriteLine($"\n→ {name} ({time}:00)");
            variantManager.ApplyTimeVariant(time);
            Thread.Sleep(500);
            ShowTileColors();
            Thread.Sleep(1500);
        }
        
        Console.WriteLine("\n✓ Time variant demo complete");
    }
    
    private void DemonstrateWeatherVariants()
    {
        Console.WriteLine("\n═══ Weather-Based Variants Demo ═══");
        var weathers = new[] { "clear", "rain", "snow", "fog" };
        
        foreach (var weather in weathers)
        {
            Console.WriteLine($"\n→ Weather: {weather}");
            variantManager.ApplyWeatherVariant(weather);
            Thread.Sleep(500);
            ShowTileColors();
            Thread.Sleep(1500);
        }
        
        Console.WriteLine("\n✓ Weather variant demo complete");
    }
    
    private void DemonstrateOreClustering()
    {
        Console.WriteLine("\n═══ Ore Clustering Demo ═══");
        
        var random = new Random();
        var oreTypes = new[] { 
            TileType.CoalOre, 
            TileType.IronOre, 
            TileType.GoldOre 
        };
        
        foreach (var oreType in oreTypes)
        {
            Console.WriteLine($"\n→ Generating {oreType} cluster");
            var cluster = enhancementSystem.GenerateOreCluster(0, 10, oreType);
            Console.WriteLine($"  Cluster size: {cluster.Count} blocks");
            Console.WriteLine($"  Coverage area: ~{cluster.Count / 2} block radius");
            Thread.Sleep(1000);
        }
        
        Console.WriteLine("\n--- Ore Distribution by Depth ---");
        for (int depth = 5; depth <= 19; depth += 3)
        {
            var oreType = enhancementSystem.GetOreTypeForDepth(depth, random);
            Console.WriteLine($"  Depth {depth,2}: {oreType?.ToString() ?? "Stone"}");
        }
        
        Console.WriteLine("\n✓ Ore clustering demo complete");
    }
    
    private void DemonstrateMiniBiomes()
    {
        Console.WriteLine("\n═══ Mini-Biome Features Demo ═══");
        
        var biomes = new[] { 
            BiomeType.Desert, 
            BiomeType.Forest, 
            BiomeType.Snow, 
            BiomeType.Rocky 
        };
        
        foreach (var biome in biomes)
        {
            Console.WriteLine($"\n→ {biome} mini-biome features:");
            
            // Try to generate a mini-biome (they're rare, so try multiple positions)
            MiniBiomeFeature? feature = null;
            for (int attempt = 0; attempt < 100 && feature == null; attempt++)
            {
                feature = enhancementSystem.GenerateMiniBiomeFeature(
                    attempt * 1000, biome);
            }
            
            if (feature != null)
            {
                Console.WriteLine($"  Type: {feature.Type}");
                Console.WriteLine($"  Radius: {feature.Radius} blocks");
                Console.WriteLine($"  Tile: {feature.TileType}");
                Console.WriteLine($"  Vegetation: {feature.VegetationType}");
            }
            else
            {
                Console.WriteLine($"  (Mini-biomes are rare - none generated in sample)");
            }
            
            var caveStyle = enhancementSystem.GetCaveStyleForBiome(biome);
            Console.WriteLine($"  Cave style: {caveStyle}");
            
            Thread.Sleep(1000);
        }
        
        Console.WriteLine("\n✓ Mini-biome demo complete");
    }
    
    private void DemonstrateCustomColors()
    {
        Console.WriteLine("\n═══ Custom Color Override Demo ═══");
        
        // Show original grass color
        Console.WriteLine("\n→ Original grass color:");
        var originalColor = customizationManager.GetTileColor(TileType.Grass);
        Console.WriteLine($"  RGB({originalColor.r:F2}, {originalColor.g:F2}, {originalColor.b:F2})");
        Thread.Sleep(1000);
        
        // Apply custom red grass
        Console.WriteLine("\n→ Applying red grass override:");
        customizationManager.SetColorOverride(TileType.Grass, 1.0f, 0.0f, 0.0f);
        var redColor = customizationManager.GetTileColor(TileType.Grass);
        Console.WriteLine($"  RGB({redColor.r:F2}, {redColor.g:F2}, {redColor.b:F2})");
        Thread.Sleep(1500);
        
        // Apply custom blue grass
        Console.WriteLine("\n→ Applying blue grass override:");
        customizationManager.SetColorOverride(TileType.Grass, 0.0f, 0.0f, 1.0f);
        var blueColor = customizationManager.GetTileColor(TileType.Grass);
        Console.WriteLine($"  RGB({blueColor.r:F2}, {blueColor.g:F2}, {blueColor.b:F2})");
        Thread.Sleep(1500);
        
        // Clear override
        Console.WriteLine("\n→ Clearing override, reverting to original:");
        customizationManager.ClearColorOverride(TileType.Grass);
        var revertedColor = customizationManager.GetTileColor(TileType.Grass);
        Console.WriteLine($"  RGB({revertedColor.r:F2}, {revertedColor.g:F2}, {revertedColor.b:F2})");
        
        Console.WriteLine("\n✓ Custom color demo complete");
    }
}
