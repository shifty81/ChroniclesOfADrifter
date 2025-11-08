using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test for water body generation (rivers, lakes, oceans)
/// </summary>
public static class WaterGenerationTest
{
    public static void Run()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Water Generation Test");
        Console.WriteLine("===========================================\n");
        
        // Test 1: Create terrain with water
        Console.WriteLine("[Test 1] Generating chunks with water...");
        var terrainGen = new TerrainGenerator(12345); // Fixed seed for consistency
        
        // Generate multiple chunks to increase chance of water bodies
        var chunks = new List<Chunk>();
        for (int i = 0; i < 10; i++)
        {
            var chunk = new Chunk(i);
            terrainGen.GenerateChunk(chunk);
            chunks.Add(chunk);
        }
        Console.WriteLine($"✓ Generated {chunks.Count} chunks");
        
        // Test 2: Scan for water tiles
        Console.WriteLine("\n[Test 2] Scanning for water tiles...");
        int waterCount = 0;
        int riverWaterCount = 0;
        int lakeWaterCount = 0;
        int oceanWaterCount = 0;
        
        var waterPositions = new List<(int x, int y, string type)>();
        
        foreach (var chunk in chunks)
        {
            int startX = chunk.GetWorldStartX();
            
            for (int localX = 0; localX < Chunk.CHUNK_WIDTH; localX++)
            {
                int worldX = startX + localX;
                bool inWater = false;
                int waterDepth = 0;
                int waterStartY = -1;
                
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    var tileType = chunk.GetTile(localX, y);
                    
                    if (tileType == TileType.Water)
                    {
                        waterCount++;
                        if (!inWater)
                        {
                            inWater = true;
                            waterStartY = y;
                        }
                        waterDepth++;
                    }
                    else if (inWater)
                    {
                        // End of water column, classify it
                        string waterType = waterDepth switch
                        {
                            <= 2 => "River",
                            <= 3 => "Lake",
                            _ => "Ocean"
                        };
                        
                        if (waterType == "River") riverWaterCount += waterDepth;
                        else if (waterType == "Lake") lakeWaterCount += waterDepth;
                        else oceanWaterCount += waterDepth;
                        
                        waterPositions.Add((worldX, waterStartY, waterType));
                        
                        inWater = false;
                        waterDepth = 0;
                    }
                }
                
                // Handle water that extends to bottom of chunk
                if (inWater)
                {
                    string waterType = waterDepth switch
                    {
                        <= 2 => "River",
                        <= 3 => "Lake",
                        _ => "Ocean"
                    };
                    
                    if (waterType == "River") riverWaterCount += waterDepth;
                    else if (waterType == "Lake") lakeWaterCount += waterDepth;
                    else oceanWaterCount += waterDepth;
                    
                    waterPositions.Add((worldX, waterStartY, waterType));
                }
            }
        }
        
        Console.WriteLine($"  Total water tiles: {waterCount}");
        Console.WriteLine($"  River water: {riverWaterCount} tiles");
        Console.WriteLine($"  Lake water: {lakeWaterCount} tiles");
        Console.WriteLine($"  Ocean water: {oceanWaterCount} tiles");
        Console.WriteLine($"  Unique water bodies: {waterPositions.Count}");
        
        if (waterCount > 0)
        {
            Console.WriteLine("✓ Water generation is working!");
        }
        else
        {
            Console.WriteLine("⚠ No water generated (this is rare but possible with some seeds)");
        }
        
        // Test 3: Display a cross-section showing water
        Console.WriteLine("\n[Test 3] Terrain cross-section with water (first 80 blocks):");
        Console.WriteLine("     " + new string('-', 80));
        
        for (int y = 0; y < 15; y++)
        {
            Console.Write($"{y,3}: ");
            
            for (int x = 0; x < 80; x++)
            {
                int chunkIndex = x / Chunk.CHUNK_WIDTH;
                int localX = x % Chunk.CHUNK_WIDTH;
                
                if (chunkIndex >= chunks.Count)
                {
                    Console.Write(' ');
                    continue;
                }
                
                var tileType = chunks[chunkIndex].GetTile(localX, y);
                Console.Write(tileType.GetChar());
            }
            
            Console.WriteLine();
        }
        
        Console.WriteLine("     " + new string('-', 80));
        Console.WriteLine("     Legend: ' '=Air, #=Grass, ==Dirt, █=Stone, ~=Water");
        Console.WriteLine("             ≈=Sand, C=Copper, I=Iron, G=Gold");
        
        // Test 4: Show water body locations
        if (waterPositions.Count > 0)
        {
            Console.WriteLine("\n[Test 4] Water body locations (first 20):");
            var uniqueWaterBodies = waterPositions.Take(20).ToList();
            
            foreach (var (x, y, type) in uniqueWaterBodies)
            {
                Console.WriteLine($"  {type} at X={x}, Y={y}");
            }
        }
        
        // Test 5: Biome analysis with water
        Console.WriteLine("\n[Test 5] Analyzing biomes and water distribution...");
        var biomeWaterCounts = new Dictionary<string, int>();
        
        foreach (var chunk in chunks)
        {
            int startX = chunk.GetWorldStartX();
            
            for (int localX = 0; localX < Chunk.CHUNK_WIDTH; localX++)
            {
                // Find surface and check biome
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    var tileType = chunk.GetTile(localX, y);
                    
                    if (tileType == TileType.Water)
                    {
                        // Approximate biome based on nearby tiles
                        string biome = "Unknown";
                        
                        // Check surface tile above water if possible
                        if (y > 0)
                        {
                            var aboveType = chunk.GetTile(localX, y - 1);
                            if (aboveType == TileType.Sand) biome = "Beach/Desert";
                            else if (aboveType == TileType.Grass) biome = "Plains/Forest/Swamp";
                            else if (aboveType == TileType.Snow) biome = "Snow";
                        }
                        
                        if (!biomeWaterCounts.ContainsKey(biome))
                        {
                            biomeWaterCounts[biome] = 0;
                        }
                        biomeWaterCounts[biome]++;
                    }
                }
            }
        }
        
        foreach (var (biome, count) in biomeWaterCounts)
        {
            Console.WriteLine($"  {biome}: {count} water tiles");
        }
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Water Generation Test Complete! ✓");
        Console.WriteLine("===========================================");
    }
}
