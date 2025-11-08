using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the creature spawning system
/// </summary>
public static class CreatureSpawnTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Creature Spawn System Test");
        Console.WriteLine("=======================================\n");
        
        RunBasicSpawnTest();
        RunBiomeSpawnTest();
        RunDepthSpawnTest();
        RunSpawnPointTest();
        
        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Creature Spawn Tests Completed");
        Console.WriteLine("=======================================\n");
    }
    
    private static void RunBasicSpawnTest()
    {
        Console.WriteLine("[Test] Basic Creature Spawning");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        CreatureSpawnSystem spawnSystem = new CreatureSpawnSystem(12345);
        
        // Test spawning different creature types
        var rabbit = spawnSystem.SpawnCreature(world, CreatureType.Rabbit, 100f, 100f);
        var goblin = spawnSystem.SpawnCreature(world, CreatureType.Goblin, 200f, 100f);
        var bat = spawnSystem.SpawnCreature(world, CreatureType.CaveBat, 300f, 100f);
        
        // Verify creatures were created
        var rabbitCreature = world.GetComponent<CreatureComponent>(rabbit);
        var goblinCreature = world.GetComponent<CreatureComponent>(goblin);
        var batCreature = world.GetComponent<CreatureComponent>(bat);
        
        System.Diagnostics.Debug.Assert(rabbitCreature != null, "Rabbit creature component missing");
        System.Diagnostics.Debug.Assert(!rabbitCreature.IsHostile, "Rabbit should not be hostile");
        System.Diagnostics.Debug.Assert(goblinCreature != null, "Goblin creature component missing");
        System.Diagnostics.Debug.Assert(goblinCreature.IsHostile, "Goblin should be hostile");
        System.Diagnostics.Debug.Assert(batCreature != null, "Bat creature component missing");
        
        // Verify components
        System.Diagnostics.Debug.Assert(world.GetComponent<PositionComponent>(rabbit) != null, "Rabbit missing position");
        System.Diagnostics.Debug.Assert(world.GetComponent<VelocityComponent>(rabbit) != null, "Rabbit missing velocity");
        System.Diagnostics.Debug.Assert(world.GetComponent<HealthComponent>(rabbit) != null, "Rabbit missing health");
        System.Diagnostics.Debug.Assert(world.GetComponent<CollisionComponent>(rabbit) != null, "Rabbit missing collision");
        
        // Verify hostile creatures have combat component
        System.Diagnostics.Debug.Assert(world.GetComponent<CombatComponent>(goblin) != null, "Goblin missing combat component");
        
        Console.WriteLine("✓ Spawned Rabbit (passive)");
        Console.WriteLine("✓ Spawned Goblin (hostile)");
        Console.WriteLine("✓ Spawned Cave Bat (hostile)");
        Console.WriteLine("✓ All components correctly assigned");
        Console.WriteLine();
    }
    
    private static void RunBiomeSpawnTest()
    {
        Console.WriteLine("[Test] Biome-Based Spawning");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        CreatureSpawnSystem spawnSystem = new CreatureSpawnSystem(12345);
        
        // Test spawning in different biomes at surface level (depth 2)
        Console.WriteLine("Testing surface spawns in different biomes...");
        
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Plains, 2, 100f, 100f, 3);
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Forest, 2, 500f, 100f, 3);
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Desert, 2, 900f, 100f, 3);
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Snow, 2, 1300f, 100f, 3);
        
        var creatures = world.GetEntitiesWithComponent<CreatureComponent>().ToList();
        System.Diagnostics.Debug.Assert(creatures.Count >= 12, $"Expected at least 12 creatures, got {creatures.Count}");
        
        Console.WriteLine($"✓ Spawned {creatures.Count} creatures across 4 biomes");
        
        // Count creature types
        int passive = 0;
        int hostile = 0;
        foreach (var entity in creatures)
        {
            var creature = world.GetComponent<CreatureComponent>(entity);
            if (creature != null)
            {
                if (creature.IsHostile) hostile++;
                else passive++;
            }
        }
        
        Console.WriteLine($"  - Passive creatures: {passive}");
        Console.WriteLine($"  - Hostile creatures: {hostile}");
        Console.WriteLine();
    }
    
    private static void RunDepthSpawnTest()
    {
        Console.WriteLine("[Test] Depth-Based Spawning");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        CreatureSpawnSystem spawnSystem = new CreatureSpawnSystem(12345);
        
        // Test spawning at different depths
        Console.WriteLine("Testing spawns at different depths...");
        
        // Surface (depth 2)
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Plains, 2, 100f, 100f, 2);
        int surfaceCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
        
        // Shallow underground (depth 6)
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Plains, 6, 100f, 300f, 2);
        int shallowCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
        
        // Deep underground (depth 12)
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Plains, 12, 100f, 500f, 2);
        int deepCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
        
        // Very deep underground (depth 17)
        spawnSystem.SpawnByBiomeAndDepth(world, BiomeType.Plains, 17, 100f, 700f, 2);
        int veryDeepCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
        
        System.Diagnostics.Debug.Assert(surfaceCount >= 2, "Surface spawns failed");
        System.Diagnostics.Debug.Assert(shallowCount > surfaceCount, "Shallow underground spawns failed");
        System.Diagnostics.Debug.Assert(deepCount > shallowCount, "Deep underground spawns failed");
        System.Diagnostics.Debug.Assert(veryDeepCount > deepCount, "Very deep spawns failed");
        
        Console.WriteLine($"✓ Surface creatures: {surfaceCount}");
        Console.WriteLine($"✓ Shallow underground: {shallowCount - surfaceCount} new creatures");
        Console.WriteLine($"✓ Deep underground: {deepCount - shallowCount} new creatures");
        Console.WriteLine($"✓ Very deep: {veryDeepCount - deepCount} new creatures");
        Console.WriteLine($"✓ Total creatures: {veryDeepCount}");
        Console.WriteLine();
    }
    
    private static void RunSpawnPointTest()
    {
        Console.WriteLine("[Test] Spawn Point System");
        Console.WriteLine("----------------------------------------");
        
        World world = new World();
        CreatureSpawnSystem spawnSystem = new CreatureSpawnSystem(12345);
        world.AddSystem(spawnSystem);
        
        // Create a spawn point
        Entity spawnPoint = world.CreateEntity();
        world.AddComponent(spawnPoint, new PositionComponent(500f, 200f));
        world.AddComponent(spawnPoint, new SpawnPointComponent(CreatureType.Goblin, 100f, 3, 1f));
        
        Console.WriteLine("Created spawn point for Goblins (max: 3, respawn: 1s)");
        
        // Update to trigger spawns
        int initialCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
        
        for (int i = 0; i < 5; i++)
        {
            world.Update(0.5f); // 0.5 seconds per update
            int currentCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
            Console.WriteLine($"  After {(i + 1) * 0.5f}s: {currentCount} creatures");
        }
        
        int finalCount = world.GetEntitiesWithComponent<CreatureComponent>().Count();
        System.Diagnostics.Debug.Assert(finalCount >= 3, $"Expected at least 3 creatures from spawn point, got {finalCount}");
        
        Console.WriteLine($"✓ Spawn point correctly spawned {finalCount} creatures");
        Console.WriteLine();
    }
}
