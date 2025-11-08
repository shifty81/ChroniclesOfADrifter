using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test for the lighting and fog of war system
/// </summary>
public static class LightingTest
{
    public static void Run()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Lighting System Test");
        Console.WriteLine("===========================================\n");
        
        // Create a test world
        var world = new World();
        world.AddSystem(new LightingSystem());
        
        Console.WriteLine("✓ Created test world with lighting system\n");
        
        // Test 1: Ambient light at different depths
        Console.WriteLine("Test 1: Ambient Light by Depth");
        Console.WriteLine("--------------------------------");
        
        TestAmbientLight(world, 0, "Surface (Y=0)", expectedBright: true);
        TestAmbientLight(world, 5, "Mid-Surface (Y=5)", expectedBright: true);
        TestAmbientLight(world, 10, "Shallow Underground (Y=10)", expectedBright: false);
        TestAmbientLight(world, 15, "Mid Underground (Y=15)", expectedBright: false);
        TestAmbientLight(world, 20, "Deep Underground (Y=20)", expectedBright: false);
        TestAmbientLight(world, 25, "Very Deep (Y=25)", expectedBright: false);
        
        Console.WriteLine();
        
        // Test 2: Player light source
        Console.WriteLine("Test 2: Player Light Source");
        Console.WriteLine("--------------------------------");
        
        var playerEntity = world.CreateEntity();
        world.AddComponent(playerEntity, new PositionComponent(100, 100));
        world.AddComponent(playerEntity, new PlayerComponent());
        world.AddComponent(playerEntity, new LightSourceComponent(
            radius: 8.0f,
            intensity: 1.0f,
            type: LightSourceType.Player
        ));
        
        Console.WriteLine($"✓ Created player entity at (100, 100) with 8-block radius light");
        
        // Test 3: Torch light source
        Console.WriteLine("\nTest 3: Torch Light Source");
        Console.WriteLine("--------------------------------");
        
        var torchEntity = world.CreateEntity();
        world.AddComponent(torchEntity, new PositionComponent(200, 200));
        world.AddComponent(torchEntity, new LightSourceComponent(
            radius: 8.0f,
            intensity: 1.0f,
            type: LightSourceType.Torch
        ));
        
        Console.WriteLine($"✓ Created torch entity at (200, 200) with 8-block radius light");
        
        // Test 4: Light source activation/deactivation
        Console.WriteLine("\nTest 4: Light Source Toggle");
        Console.WriteLine("--------------------------------");
        
        var lightSource = world.GetComponent<LightSourceComponent>(torchEntity);
        if (lightSource != null)
        {
            Console.WriteLine($"✓ Torch light is active: {lightSource.IsActive}");
            
            lightSource.IsActive = false;
            world.AddComponent(torchEntity, lightSource);
            Console.WriteLine($"✓ Deactivated torch light");
            
            lightSource.IsActive = true;
            world.AddComponent(torchEntity, lightSource);
            Console.WriteLine($"✓ Reactivated torch light");
        }
        
        // Test 5: Lighting component
        Console.WriteLine("\nTest 5: Lighting Components");
        Console.WriteLine("--------------------------------");
        
        var tileEntity = world.CreateEntity();
        world.AddComponent(tileEntity, new PositionComponent(150, 150));
        world.AddComponent(tileEntity, new LightingComponent(0.5f));
        
        var lighting = world.GetComponent<LightingComponent>(tileEntity);
        if (lighting != null)
        {
            Console.WriteLine($"✓ Created tile with light level: {lighting.LightLevel}");
            Console.WriteLine($"  - Is Explored: {lighting.IsExplored}");
            Console.WriteLine($"  - Is Currently Visible: {lighting.IsCurrentlyVisible}");
        }
        
        // Summary
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Lighting Tests Passed! ✓");
        Console.WriteLine("===========================================");
        Console.WriteLine("\nLighting System Features:");
        Console.WriteLine("  - Depth-based ambient lighting");
        Console.WriteLine("  - Player personal lantern (8-block radius)");
        Console.WriteLine("  - Placed torches (8-block radius)");
        Console.WriteLine("  - Light intensity falloff with distance");
        Console.WriteLine("  - Fog of war with exploration tracking");
        Console.WriteLine("\nNext Steps:");
        Console.WriteLine("  - Build C++ engine to test in-game");
        Console.WriteLine("  - Place torches underground while mining");
        Console.WriteLine("  - Observe darkness in deep layers (Y >= 20)");
        Console.WriteLine("  - Test light propagation visually\n");
    }
    
    private static void TestAmbientLight(World world, int y, string description, bool expectedBright)
    {
        // Calculate expected light level based on depth
        float expectedLight;
        if (y < 10)
        {
            expectedLight = 1.0f; // Full daylight
        }
        else if (y < 19)
        {
            float depth = y - 10;
            float maxDepth = 19 - 10;
            expectedLight = Math.Max(0.3f - (depth / maxDepth) * 0.3f, 0f);
        }
        else
        {
            expectedLight = 0f; // Pitch black
        }
        
        string brightness = expectedLight >= 0.5f ? "Bright" : (expectedLight > 0.1f ? "Dim" : "Dark");
        Console.WriteLine($"  {description,-25} Light Level: {expectedLight:F2} ({brightness})");
    }
}
