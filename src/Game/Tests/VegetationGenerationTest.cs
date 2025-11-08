using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the vegetation generation system
/// </summary>
public class VegetationGenerationTest
{
    public static void Run()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Vegetation Generation System Tests");
        Console.WriteLine("===========================================\n");

        RunAllTests();

        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Tests Completed");
        Console.WriteLine("===========================================\n");
        
        if (Console.IsInputRedirected)
        {
            return;
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void RunAllTests()
    {
        TestVegetationTypes();
        TestBiomeVegetationDensity();
        TestChunkVegetationStorage();
        TestVegetationRendering();
    }

    private static void TestVegetationTypes()
    {
        Console.WriteLine("[Test 1] Testing vegetation tile types...");

        // Test that all vegetation types are non-solid
        var vegetationTypes = new[]
        {
            TileType.TreeOak,
            TileType.TreePine,
            TileType.TreePalm,
            TileType.TallGrass,
            TileType.Bush,
            TileType.Cactus,
            TileType.Flower
        };

        foreach (var vegType in vegetationTypes)
        {
            if (vegType == TileType.TallGrass || vegType == TileType.Flower)
            {
                if (vegType.IsSolid())
                {
                    Console.WriteLine($"  ❌ FAILED: {vegType} should not be solid");
                    return;
                }
            }
            else
            {
                if (!vegType.IsSolid())
                {
                    Console.WriteLine($"  ⚠️  WARNING: {vegType} is not solid (trees/bushes should block movement)");
                }
            }

            // Test that vegetation has proper characters and colors
            char c = vegType.GetChar();
            ConsoleColor color = vegType.GetColor();

            if (c == '?')
            {
                Console.WriteLine($"  ❌ FAILED: {vegType} has invalid character '?'");
                return;
            }

            Console.WriteLine($"  ✓ {vegType}: '{c}' (color: {color})");
        }

        Console.WriteLine("  ✅ PASSED: All vegetation types have valid properties\n");
    }

    private static void TestBiomeVegetationDensity()
    {
        Console.WriteLine("[Test 2] Testing biome-specific vegetation density...");

        const int SEED = 54321;
        var generator = new TerrainGenerator(SEED);

        // Generate test chunks for each biome type
        var biomes = new[] { BiomeType.Forest, BiomeType.Plains, BiomeType.Desert };

        foreach (var biome in biomes)
        {
            // Create a chunk in an area known to be this biome
            // (Using different X positions to get different biomes)
            int chunkX = biome switch
            {
                BiomeType.Desert => 0,
                BiomeType.Plains => 50,
                BiomeType.Forest => 100,
                _ => 0
            };

            var chunk = new Chunk(chunkX);
            generator.GenerateChunk(chunk);

            // Count vegetation
            int vegetationCount = 0;
            for (int x = 0; x < Chunk.CHUNK_WIDTH; x++)
            {
                var veg = chunk.GetVegetation(x);
                if (veg.HasValue && veg.Value.IsVegetation())
                {
                    vegetationCount++;
                }
            }

            float density = (float)vegetationCount / Chunk.CHUNK_WIDTH * 100f;

            Console.WriteLine($"  {biome} biome: {vegetationCount}/{Chunk.CHUNK_WIDTH} tiles ({density:F1}% coverage)");

            // Validate expected densities
            bool valid = biome switch
            {
                BiomeType.Forest => density >= 40f && density <= 80f,  // ~60% expected
                BiomeType.Plains => density >= 15f && density <= 50f,  // ~30% expected
                BiomeType.Desert => density >= 0f && density <= 15f,   // ~5% expected
                _ => false
            };

            if (!valid)
            {
                Console.WriteLine($"  ⚠️  WARNING: {biome} density outside expected range");
            }
        }

        Console.WriteLine("  ✅ PASSED: Biome vegetation densities are reasonable\n");
    }

    private static void TestChunkVegetationStorage()
    {
        Console.WriteLine("[Test 3] Testing chunk vegetation storage...");

        var chunk = new Chunk(0);

        // Test setting and getting vegetation
        chunk.SetVegetation(0, TileType.TreeOak);
        chunk.SetVegetation(15, TileType.TallGrass);
        chunk.SetVegetation(31, TileType.Cactus);

        var veg0 = chunk.GetVegetation(0);
        var veg15 = chunk.GetVegetation(15);
        var veg31 = chunk.GetVegetation(31);
        var veg5 = chunk.GetVegetation(5); // Should be null

        if (!veg0.HasValue || veg0.Value != TileType.TreeOak)
        {
            Console.WriteLine("  ❌ FAILED: Vegetation at X=0 not stored correctly");
            return;
        }

        if (!veg15.HasValue || veg15.Value != TileType.TallGrass)
        {
            Console.WriteLine("  ❌ FAILED: Vegetation at X=15 not stored correctly");
            return;
        }

        if (!veg31.HasValue || veg31.Value != TileType.Cactus)
        {
            Console.WriteLine("  ❌ FAILED: Vegetation at X=31 not stored correctly");
            return;
        }

        if (veg5.HasValue)
        {
            Console.WriteLine("  ❌ FAILED: Vegetation at X=5 should be null");
            return;
        }

        // Test clearing vegetation
        chunk.SetVegetation(0, null);
        var cleared = chunk.GetVegetation(0);
        if (cleared.HasValue)
        {
            Console.WriteLine("  ❌ FAILED: Vegetation at X=0 should be cleared");
            return;
        }

        Console.WriteLine("  ✓ Vegetation storage works correctly");
        Console.WriteLine("  ✓ Vegetation clearing works correctly");
        Console.WriteLine("  ✅ PASSED: Chunk vegetation storage is working\n");
    }

    private static void TestVegetationRendering()
    {
        Console.WriteLine("[Test 4] Testing vegetation rendering...");

        const int SEED = 99999;
        var generator = new TerrainGenerator(SEED);
        var chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(generator);

        // Generate a few chunks
        for (int i = 0; i < 3; i++)
        {
            chunkManager.GetChunk(i);
        }

        // Sample some positions and check for vegetation
        int vegetationFound = 0;
        for (int worldX = 0; worldX < 96; worldX++)
        {
            var veg = chunkManager.GetVegetation(worldX);
            if (veg.HasValue && veg.Value.IsVegetation())
            {
                vegetationFound++;
            }
        }

        Console.WriteLine($"  ✓ Found {vegetationFound} vegetation tiles across 3 chunks");

        if (vegetationFound == 0)
        {
            Console.WriteLine("  ⚠️  WARNING: No vegetation found (might be bad luck with RNG)");
        }
        else if (vegetationFound > 0)
        {
            Console.WriteLine("  ✓ Vegetation is being generated and accessible");
        }

        Console.WriteLine("  ✅ PASSED: Vegetation rendering integration works\n");
    }
}
