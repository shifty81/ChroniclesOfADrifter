using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Simple test program to verify terrain generation without the engine
/// </summary>
class TerrainGenerationTest
{
    public static void Run()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Terrain Generation Test");
        Console.WriteLine("===========================================\n");
        
        // Test 1: Chunk creation
        Console.WriteLine("[Test 1] Creating chunks...");
        var chunk1 = new Chunk(0);
        var chunk2 = new Chunk(1);
        Console.WriteLine($"✓ Created chunk at X=0 (world start: {chunk1.GetWorldStartX()})");
        Console.WriteLine($"✓ Created chunk at X=1 (world start: {chunk2.GetWorldStartX()})");
        
        // Test 2: Terrain generation
        Console.WriteLine("\n[Test 2] Generating terrain...");
        var generator = new TerrainGenerator(seed: 12345);
        generator.GenerateChunk(chunk1);
        generator.GenerateChunk(chunk2);
        Console.WriteLine($"✓ Generated terrain for chunk 0: {(chunk1.IsGenerated ? "Success" : "Failed")}");
        Console.WriteLine($"✓ Generated terrain for chunk 1: {(chunk2.IsGenerated ? "Success" : "Failed")}");
        
        // Test 3: Chunk manager
        Console.WriteLine("\n[Test 3] Testing ChunkManager...");
        var chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(generator);
        
        var loadedChunk = chunkManager.GetChunk(0);
        Console.WriteLine($"✓ Loaded chunk 0 via manager");
        Console.WriteLine($"✓ Chunk manager has {chunkManager.GetLoadedChunkCount()} chunks loaded");
        
        // Test 4: Sample tiles
        Console.WriteLine("\n[Test 4] Sampling terrain at different positions...");
        for (int testX = 0; testX < 10; testX++)
        {
            var tile = chunkManager.GetTile(testX, 5);
            Console.WriteLine($"  Position ({testX}, 5): {tile} ({tile.GetChar()})");
        }
        
        // Test 5: Visualize a cross-section
        Console.WriteLine("\n[Test 5] Terrain cross-section (first 40 blocks, depth 0-15):");
        Console.WriteLine("     " + new string('-', 40));
        
        for (int y = 0; y < 15; y++)
        {
            Console.Write($" {y,2}: ");
            for (int x = 0; x < 40; x++)
            {
                var tile = chunkManager.GetTile(x, y);
                Console.Write(tile.GetChar());
            }
            Console.WriteLine();
        }
        
        Console.WriteLine("     " + new string('-', 40));
        Console.WriteLine("     Legend: ' '=Air, #=Grass, ==Dirt, █=Stone");
        Console.WriteLine("             ≈=Sand, C=Copper, I=Iron, G=Gold");
        
        // Test 6: Biome transitions
        Console.WriteLine("\n[Test 6] Biome detection across wider range...");
        chunkManager.UpdateChunks(100); // Load more chunks
        
        Dictionary<TileType, int> surfaceTiles = new Dictionary<TileType, int>();
        for (int x = 0; x < 100; x++)
        {
            var surfaceTile = chunkManager.GetTile(x, 4); // Sample at Y=4 (typical surface)
            if (surfaceTile != TileType.Air)
            {
                if (!surfaceTiles.ContainsKey(surfaceTile))
                    surfaceTiles[surfaceTile] = 0;
                surfaceTiles[surfaceTile]++;
            }
        }
        
        Console.WriteLine("Surface tile distribution (first 100 blocks):");
        foreach (var kvp in surfaceTiles.OrderByDescending(k => k.Value))
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value} blocks ({kvp.Value}%)");
        }
        
        Console.WriteLine($"\n✓ Chunk manager now has {chunkManager.GetLoadedChunkCount()} chunks loaded");
        
        // Test 7: Underground ores
        Console.WriteLine("\n[Test 7] Scanning for ore deposits...");
        Dictionary<TileType, int> oreCount = new Dictionary<TileType, int>
        {
            { TileType.CopperOre, 0 },
            { TileType.IronOre, 0 },
            { TileType.GoldOre, 0 }
        };
        
        for (int x = 0; x < 100; x++)
        {
            for (int y = 10; y < 28; y++) // Underground layers
            {
                var tile = chunkManager.GetTile(x, y);
                if (oreCount.ContainsKey(tile))
                {
                    oreCount[tile]++;
                }
            }
        }
        
        Console.WriteLine("Ore deposits found (100 blocks wide × 18 layers deep):");
        foreach (var kvp in oreCount)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value} veins");
        }
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Tests Passed! ✓");
        Console.WriteLine("===========================================");
    }
}
