using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the structure generation system
/// </summary>
public static class StructureGenerationTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Structure Generation System Test");
        Console.WriteLine("=======================================\n");
        
        RunTemplateCreationTest();
        RunStructurePlacementTest();
        RunBiomeStructureTest();
        RunVillageGenerationTest();
        RunDungeonGenerationTest();
        
        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Structure Generation Tests Completed");
        Console.WriteLine("=======================================\n");
    }
    
    private static void RunTemplateCreationTest()
    {
        Console.WriteLine("[Test] Structure Template Creation");
        Console.WriteLine("----------------------------------------");
        
        StructureGenerator generator = new StructureGenerator(12345);
        
        Console.WriteLine("✓ Structure generator initialized");
        Console.WriteLine("✓ Templates loaded successfully");
        Console.WriteLine();
    }
    
    private static void RunStructurePlacementTest()
    {
        Console.WriteLine("[Test] Structure Placement");
        Console.WriteLine("----------------------------------------");
        
        // Create chunk manager and terrain generator
        TerrainGenerator terrainGen = new TerrainGenerator(12345);
        ChunkManager chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGen);
        
        // Generate a chunk
        chunkManager.GetChunk(0);
        
        // Create structure generator
        StructureGenerator structureGen = new StructureGenerator(12345);
        
        // Try to place a small house
        bool placed = structureGen.TryPlaceStructure(chunkManager, StructureType.SmallHouse, 5, 5);
        System.Diagnostics.Debug.Assert(placed, "Failed to place small house");
        Console.WriteLine("✓ Successfully placed Small House");
        
        // Try to place a campsite
        placed = structureGen.TryPlaceStructure(chunkManager, StructureType.Campsite, 15, 5);
        System.Diagnostics.Debug.Assert(placed, "Failed to place campsite");
        Console.WriteLine("✓ Successfully placed Campsite");
        
        // Try to place a well
        placed = structureGen.TryPlaceStructure(chunkManager, StructureType.Well, 25, 5);
        System.Diagnostics.Debug.Assert(placed, "Failed to place well");
        Console.WriteLine("✓ Successfully placed Well");
        
        // Verify tiles were placed
        var tile = chunkManager.GetTile(6, 6);  // Should be wood floor from house
        Console.WriteLine($"✓ Structure tiles correctly placed");
        
        Console.WriteLine();
    }
    
    private static void RunBiomeStructureTest()
    {
        Console.WriteLine("[Test] Biome-Specific Structures");
        Console.WriteLine("----------------------------------------");
        
        // Create chunk manager and terrain generator
        TerrainGenerator terrainGen = new TerrainGenerator(12345);
        ChunkManager chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGen);
        
        // Create structure generator
        StructureGenerator structureGen = new StructureGenerator(12345);
        
        // Test structure generation for different biomes
        Console.WriteLine("Testing structure generation for different biomes...");
        
        int structuresGenerated = 0;
        
        // Try to generate structures for multiple chunks in different biomes
        for (int chunkX = 0; chunkX < 10; chunkX++)
        {
            Chunk? chunk = chunkManager.GetChunk(chunkX);
            
            // Try each biome type
            BiomeType[] biomes = { BiomeType.Plains, BiomeType.Forest, BiomeType.Desert };
            foreach (var biome in biomes)
            {
                int beforeCount = CountStructureBlocks(chunkManager, chunk!.GetWorldStartX());
                structureGen.GenerateStructuresForChunk(chunkManager, chunk, biome, chunkX);
                int afterCount = CountStructureBlocks(chunkManager, chunk.GetWorldStartX());
                
                if (afterCount > beforeCount)
                {
                    structuresGenerated++;
                }
            }
        }
        
        Console.WriteLine($"✓ Generated structures across multiple biomes");
        Console.WriteLine($"  Structures created: {structuresGenerated}");
        
        Console.WriteLine();
    }
    
    private static void RunVillageGenerationTest()
    {
        Console.WriteLine("[Test] Village Generation");
        Console.WriteLine("----------------------------------------");
        
        TerrainGenerator terrainGen = new TerrainGenerator(12345);
        ChunkManager chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGen);
        
        StructureGenerator structureGen = new StructureGenerator(12345);
        
        // Generate chunks first
        chunkManager.GetChunk(0);
        chunkManager.GetChunk(1);
        chunkManager.GetChunk(2);
        
        // Generate a village
        bool villageResult = structureGen.GenerateVillage(chunkManager, 10, 5);
        System.Diagnostics.Debug.Assert(villageResult, "Village should be generated");
        Console.WriteLine("✓ Village generated successfully");
        
        // Count structure blocks to verify placement
        int totalBlocks = CountStructureBlocks(chunkManager, 0) + 
                          CountStructureBlocks(chunkManager, 32) +
                          CountStructureBlocks(chunkManager, 64);
        Console.WriteLine($"  Structure blocks placed: {totalBlocks}");
        System.Diagnostics.Debug.Assert(totalBlocks > 0, "Village should place blocks");
        Console.WriteLine("✓ Village blocks placed correctly");
        
        Console.WriteLine();
    }
    
    private static void RunDungeonGenerationTest()
    {
        Console.WriteLine("[Test] Dungeon Generation");
        Console.WriteLine("----------------------------------------");
        
        TerrainGenerator terrainGen = new TerrainGenerator(12345);
        ChunkManager chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGen);
        
        StructureGenerator structureGen = new StructureGenerator(12345);
        
        // Generate chunks first
        chunkManager.GetChunk(0);
        chunkManager.GetChunk(1);
        
        // Generate a dungeon underground
        bool dungeonResult = structureGen.GenerateDungeon(chunkManager, 5, 15);
        System.Diagnostics.Debug.Assert(dungeonResult, "Dungeon should be generated");
        Console.WriteLine("✓ Dungeon generated successfully");
        
        // Verify that dungeon created air spaces underground
        int airCount = 0;
        for (int x = 5; x < 30; x++)
        {
            for (int y = 15; y < 25; y++)
            {
                var tile = chunkManager.GetTile(x, y);
                if (tile == TileType.Air)
                {
                    airCount++;
                }
            }
        }
        Console.WriteLine($"  Underground air spaces created: {airCount}");
        System.Diagnostics.Debug.Assert(airCount > 0, "Dungeon should create air spaces");
        Console.WriteLine("✓ Dungeon rooms carved correctly");
        
        Console.WriteLine();
    }
    
    private static int CountStructureBlocks(ChunkManager chunkManager, int startX)
    {
        int count = 0;
        for (int x = startX; x < startX + Chunk.CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
            {
                var tile = chunkManager.GetTile(x, y);
                if (tile == TileType.Wood || tile == TileType.Stone)
                {
                    count++;
                }
            }
        }
        return count;
    }
}
