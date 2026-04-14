using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Minimap System
/// </summary>
public static class MinimapSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Minimap System Test");
        Console.WriteLine("=======================================\n");

        RunMinimapInitializationTest();
        RunChunkExplorationTest();
        RunPOITest();
        RunASCIIRenderTest();
        RunVisibilityToggleTest();
        RunPlayerPositionUpdateTest();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Minimap Tests Completed");
        Console.WriteLine("=======================================\n");
    }

    private static void RunMinimapInitializationTest()
    {
        Console.WriteLine("[Test] Minimap Initialization");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new MinimapComponent());
        world.AddComponent(player, new PositionComponent(0f, 0f));

        var minimap = world.GetComponent<MinimapComponent>(player);
        System.Diagnostics.Debug.Assert(minimap != null, "MinimapComponent should exist");
        System.Diagnostics.Debug.Assert(minimap!.ExploredChunks.Count == 0, "No chunks should be explored at start");
        System.Diagnostics.Debug.Assert(minimap.IsVisible, "Minimap should be visible by default");
        System.Diagnostics.Debug.Assert(minimap.VisibilityRadius == 5, "Default radius should be 5");

        Console.WriteLine("  ✅ Minimap initialized correctly\n");
    }

    private static void RunChunkExplorationTest()
    {
        Console.WriteLine("[Test] Chunk Exploration");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new MinimapComponent(visibilityRadius: 1));
        world.AddComponent(player, new PositionComponent(8f, 8f)); // chunk (0,0)

        var sys = new MinimapSystem();
        sys.Initialize(world);
        sys.Update(world, 0.016f);

        var minimap = world.GetComponent<MinimapComponent>(player)!;
        // With radius 1, we should explore a 3x3 = 9 chunk area
        System.Diagnostics.Debug.Assert(minimap.ExploredChunks.Count == 9, $"Should explore 9 chunks, got {minimap.ExploredChunks.Count}");
        System.Diagnostics.Debug.Assert(minimap.IsChunkExplored(0, 0), "Chunk (0,0) should be explored");
        System.Diagnostics.Debug.Assert(minimap.IsChunkExplored(-1, -1), "Chunk (-1,-1) should be explored");
        System.Diagnostics.Debug.Assert(!minimap.IsChunkExplored(5, 5), "Distant chunk should not be explored");

        Console.WriteLine($"  ✅ Explored {minimap.ExploredChunks.Count} chunks around player\n");
    }

    private static void RunPOITest()
    {
        Console.WriteLine("[Test] POI Markers");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new MinimapComponent());
        world.AddComponent(player, new PositionComponent(0f, 0f));

        MinimapSystem.AddPOI(world, player, 100f, 200f, MinimapTileType.Boss);
        MinimapSystem.AddPOI(world, player, 300f, 400f, MinimapTileType.Village);

        var minimap = world.GetComponent<MinimapComponent>(player)!;
        System.Diagnostics.Debug.Assert(minimap.POIMarkers.Count == 2, "Should have 2 POI markers");
        System.Diagnostics.Debug.Assert(minimap.POIMarkers[0].TileType == MinimapTileType.Boss, "First POI should be Boss");
        System.Diagnostics.Debug.Assert(minimap.POIMarkers[1].TileType == MinimapTileType.Village, "Second POI should be Village");

        MinimapSystem.RemovePOI(world, player, 100f, 200f);
        System.Diagnostics.Debug.Assert(minimap.POIMarkers.Count == 1, "Should have 1 POI after removal");

        Console.WriteLine("  ✅ POI markers add and remove correctly\n");
    }

    private static void RunASCIIRenderTest()
    {
        Console.WriteLine("[Test] ASCII Minimap Render");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new MinimapComponent(visibilityRadius: 2));
        world.AddComponent(player, new PositionComponent(8f, 8f));

        var sys = new MinimapSystem();
        sys.Initialize(world);
        sys.Update(world, 0.016f);

        var minimap = world.GetComponent<MinimapComponent>(player)!;
        string ascii = MinimapSystem.RenderASCII(minimap, radius: 4);

        System.Diagnostics.Debug.Assert(ascii.Contains("@"), "ASCII minimap should contain player '@'");
        System.Diagnostics.Debug.Assert(ascii.Contains("."), "ASCII minimap should contain explored '.'");
        System.Diagnostics.Debug.Assert(ascii.Contains("+"), "ASCII minimap should have border '+'");

        Console.WriteLine(ascii);
        Console.WriteLine("  ✅ ASCII minimap renders correctly\n");
    }

    private static void RunVisibilityToggleTest()
    {
        Console.WriteLine("[Test] Minimap Visibility Toggle");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new MinimapComponent());
        world.AddComponent(player, new PositionComponent(0f, 0f));

        var minimap = world.GetComponent<MinimapComponent>(player)!;
        System.Diagnostics.Debug.Assert(minimap.IsVisible, "Should start visible");

        MinimapSystem.ToggleVisibility(world, player);
        System.Diagnostics.Debug.Assert(!minimap.IsVisible, "Should be hidden after toggle");

        MinimapSystem.ToggleVisibility(world, player);
        System.Diagnostics.Debug.Assert(minimap.IsVisible, "Should be visible again after second toggle");

        Console.WriteLine("  ✅ Minimap visibility toggles correctly\n");
    }

    private static void RunPlayerPositionUpdateTest()
    {
        Console.WriteLine("[Test] Player Position Update");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new MinimapComponent());
        world.AddComponent(player, new PositionComponent(100f, 200f));

        var sys = new MinimapSystem();
        sys.Initialize(world);
        sys.Update(world, 0.016f);

        var minimap = world.GetComponent<MinimapComponent>(player)!;
        System.Diagnostics.Debug.Assert(Math.Abs(minimap.PlayerWorldX - 100f) < 0.01f, "Player X should be updated");
        System.Diagnostics.Debug.Assert(Math.Abs(minimap.PlayerWorldY - 200f) < 0.01f, "Player Y should be updated");

        Console.WriteLine($"  ✅ Player position tracked at ({minimap.PlayerWorldX}, {minimap.PlayerWorldY})\n");
    }
}
