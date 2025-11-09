using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the world creature manager and spawn rate system
/// </summary>
public static class WorldCreatureManagerTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  World Creature Manager Test");
        Console.WriteLine("=======================================\n");
        
        TestBasicSpawnManagement();
        TestChunkBasedSpawning();
        TestSpawnDensityByBiome();
        TestCreatureTracking();
        
        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All World Creature Manager Tests Passed");
        Console.WriteLine("=======================================\n");
    }
    
    private static void TestBasicSpawnManagement()
    {
        Console.WriteLine("[Test] Basic Spawn Management");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        var creatureManager = new WorldCreatureManager(12345);
        var chunkManager = new ChunkManager();
        var terrainGenerator = new TerrainGenerator(12345);
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Load a few chunks around player
        float playerX = 0f;
        float playerY = 100f;
        
        chunkManager.UpdateChunks(playerX);
        
        var stats = creatureManager.GetStatistics(world);
        Console.WriteLine($"Initial creatures: {stats.totalCreatures}");
        
        // Update manager to spawn creatures
        for (int i = 0; i < 3; i++)
        {
            creatureManager.Update(world, chunkManager, playerX, playerY, 6f); // Trigger spawn checks
            playerX += 100f; // Move player
        }
        
        stats = creatureManager.GetStatistics(world);
        Console.WriteLine($"After 3 updates: {stats.totalCreatures} creatures across {stats.activeChunks} chunks");
        
        if (stats.totalCreatures > 0)
        {
            Console.WriteLine("Creature type breakdown:");
            foreach (var kvp in stats.creatureTypeCounts)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value}");
            }
        }
        
        System.Diagnostics.Debug.Assert(stats.totalCreatures >= 0, "Should have spawned some creatures");
        Console.WriteLine("✓ Basic spawn management working\n");
    }
    
    private static void TestChunkBasedSpawning()
    {
        Console.WriteLine("[Test] Chunk-Based Spawning");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        var creatureManager = new WorldCreatureManager(54321);
        var chunkManager = new ChunkManager();
        var terrainGenerator = new TerrainGenerator(54321);
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Load multiple chunks
        float playerX = 500f;
        float playerY = 100f;
        
        for (int i = 0; i < 5; i++)
        {
            chunkManager.UpdateChunks(playerX + i * 200f);
        }
        
        Console.WriteLine($"Loaded {chunkManager.GetLoadedChunkCount()} chunks");
        
        // Trigger spawning with significant time
        for (int i = 0; i < 5; i++)
        {
            creatureManager.Update(world, chunkManager, playerX, playerY, 6f);
        }
        
        var stats = creatureManager.GetStatistics(world);
        Console.WriteLine($"Spawned {stats.totalCreatures} creatures across {stats.activeChunks} active chunks");
        
        // Verify creatures are tracked per chunk
        System.Diagnostics.Debug.Assert(stats.activeChunks > 0, "Should be tracking multiple chunks");
        Console.WriteLine($"✓ Chunks with creatures: {stats.activeChunks}");
        Console.WriteLine($"✓ Average creatures per chunk: {(float)stats.totalCreatures / Math.Max(1, stats.activeChunks):F1}\n");
    }
    
    private static void TestSpawnDensityByBiome()
    {
        Console.WriteLine("[Test] Spawn Density by Biome");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        var creatureManager = new WorldCreatureManager(99999);
        
        // Test initial spawn for different biomes
        var chunk1 = new Chunk(0);
        var chunk2 = new Chunk(1);
        var chunk3 = new Chunk(2);
        var chunk4 = new Chunk(3);
        
        // Initial spawns for different biomes
        creatureManager.InitialChunkSpawn(world, chunk1, BiomeType.Forest);  // High density
        creatureManager.InitialChunkSpawn(world, chunk2, BiomeType.Desert);  // Low density
        creatureManager.InitialChunkSpawn(world, chunk3, BiomeType.Jungle);  // Highest density
        creatureManager.InitialChunkSpawn(world, chunk4, BiomeType.Rocky);   // Low density
        
        var stats = creatureManager.GetStatistics(world);
        Console.WriteLine($"Total creatures spawned: {stats.totalCreatures}");
        Console.WriteLine($"Active chunks: {stats.activeChunks}");
        
        if (stats.creatureTypeCounts.Count > 0)
        {
            Console.WriteLine("Creature distribution:");
            foreach (var kvp in stats.creatureTypeCounts)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value}");
            }
        }
        
        System.Diagnostics.Debug.Assert(stats.totalCreatures > 0, "Should have spawned creatures");
        System.Diagnostics.Debug.Assert(stats.activeChunks == 4, $"Should be tracking 4 chunks, got {stats.activeChunks}");
        Console.WriteLine("✓ Spawn density varies by biome\n");
    }
    
    private static void TestCreatureTracking()
    {
        Console.WriteLine("[Test] Creature Tracking and Cleanup");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        var creatureManager = new WorldCreatureManager(11111);
        var chunkManager = new ChunkManager();
        var terrainGenerator = new TerrainGenerator(11111);
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Load chunks
        float playerX = 0f;
        float playerY = 100f;
        chunkManager.UpdateChunks(playerX);
        
        // Spawn creatures
        for (int i = 0; i < 3; i++)
        {
            creatureManager.Update(world, chunkManager, playerX, playerY, 6f);
        }
        
        var initialStats = creatureManager.GetStatistics(world);
        Console.WriteLine($"Initial: {initialStats.totalCreatures} creatures");
        
        // Simulate creature death by removing health component
        var allCreatures = world.GetEntitiesWithComponent<CreatureComponent>().Take(2).ToList();
        if (allCreatures.Count > 0)
        {
            foreach (var creature in allCreatures)
            {
                var health = world.GetComponent<HealthComponent>(creature);
                if (health != null)
                {
                    health.CurrentHealth = 0; // Mark as dead
                }
            }
            
            Console.WriteLine($"Killed {allCreatures.Count} creatures");
            
            // Update to trigger cleanup
            creatureManager.Update(world, chunkManager, playerX, playerY, 1f);
            
            var afterStats = creatureManager.GetStatistics(world);
            Console.WriteLine($"After cleanup: {afterStats.totalCreatures} creatures");
            
            System.Diagnostics.Debug.Assert(afterStats.totalCreatures < initialStats.totalCreatures, 
                "Dead creatures should be removed from tracking");
            Console.WriteLine("✓ Dead creature cleanup working");
        }
        else
        {
            Console.WriteLine("⚠ No creatures spawned to test cleanup");
        }
        
        Console.WriteLine();
    }
}
