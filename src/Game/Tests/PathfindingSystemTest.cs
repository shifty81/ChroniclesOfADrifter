using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the A* Pathfinding system
/// </summary>
public static class PathfindingSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Pathfinding System Test (A*)");
        Console.WriteLine("=======================================\n");

        TestFindPathOpenField();
        TestFindPathAroundObstacle();
        TestFindPathBlockedGoal();
        TestFindPathSameCell();
        TestNPCPathTracking();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Pathfinding System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static void TestFindPathOpenField()
    {
        Console.WriteLine("[Test] A* Path in Open Field");
        Console.WriteLine("----------------------------------------");

        var world = new World();

        var path = PathfindingSystem.FindPath(world,
            startPos: (0f, 0f),
            goalPos:  (128f, 64f));  // 4 cells right, 2 cells down

        System.Diagnostics.Debug.Assert(path.Count > 0, "Should find path in open field");

        // First waypoint should be closer to start than goal
        var first = path[0];
        var last  = path[^1];

        Console.WriteLine($"  Path found: {path.Count} waypoints");
        Console.WriteLine($"  First wp: ({first.x:F0}, {first.y:F0})  Last wp: ({last.x:F0}, {last.y:F0})");

        // Last waypoint should be at/near goal cell centre
        float goalCentreX = (int)(128f / PathfindingSystem.CELL_SIZE) * PathfindingSystem.CELL_SIZE + PathfindingSystem.CELL_SIZE / 2f;
        float goalCentreY = (int)(64f  / PathfindingSystem.CELL_SIZE) * PathfindingSystem.CELL_SIZE + PathfindingSystem.CELL_SIZE / 2f;

        System.Diagnostics.Debug.Assert(Math.Abs(last.x - goalCentreX) < PathfindingSystem.CELL_SIZE,
            "Last waypoint should be at the goal cell");

        Console.WriteLine("✓ A* open-field path found correctly\n");
    }

    private static void TestFindPathAroundObstacle()
    {
        Console.WriteLine("[Test] A* Path Around Obstacle");
        Console.WriteLine("----------------------------------------");

        var world = new World();

        // Place a wall of static collision entities across the direct route (x=64)
        for (int y = 0; y < 5; y++)
        {
            var wall = world.CreateEntity();
            world.AddComponent(wall, new PositionComponent(64f, y * 32f));
            world.AddComponent(wall, new CollisionComponent(32f, 32f, isStatic: true));
        }

        var path = PathfindingSystem.FindPath(world,
            startPos: (16f, 64f),
            goalPos:  (160f, 64f));  // Must go around the wall

        System.Diagnostics.Debug.Assert(path.Count > 0, "Should find path around obstacle");
        Console.WriteLine($"  Path around wall: {path.Count} waypoints");

        // Path should be longer than a straight line (indicating it went around)
        System.Diagnostics.Debug.Assert(path.Count >= 4, "Path around wall should have extra waypoints");

        Console.WriteLine("✓ A* path-around-obstacle working\n");
    }

    private static void TestFindPathBlockedGoal()
    {
        Console.WriteLine("[Test] A* Blocked Goal");
        Console.WriteLine("----------------------------------------");

        var world = new World();

        // Completely surround the goal cell with walls
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                var wall = world.CreateEntity();
                world.AddComponent(wall, new PositionComponent(128f + dx * 32f, 64f + dy * 32f));
                world.AddComponent(wall, new CollisionComponent(32f, 32f, isStatic: true));
            }
        }

        var path = PathfindingSystem.FindPath(world,
            startPos: (0f, 0f),
            goalPos:  (128f, 64f));  // Goal is blocked

        // Either no path or a path that ends at the closest reachable point
        Console.WriteLine($"  Blocked goal result: {path.Count} waypoints (0 = no path found)");
        // Don't assert on specific result – just ensure it doesn't throw
        Console.WriteLine("✓ A* blocked-goal handled gracefully\n");
    }

    private static void TestFindPathSameCell()
    {
        Console.WriteLine("[Test] A* Same Start and Goal");
        Console.WriteLine("----------------------------------------");

        var world = new World();

        var path = PathfindingSystem.FindPath(world,
            startPos: (16f, 16f),
            goalPos:  (16f, 16f));

        Console.WriteLine($"  Same-cell result: {path.Count} waypoints");
        // May be 0 or 1 – should not crash
        Console.WriteLine("✓ A* same-cell handled gracefully\n");
    }

    private static void TestNPCPathTracking()
    {
        Console.WriteLine("[Test] NPC Path Tracking via System Update");
        Console.WriteLine("----------------------------------------");

        var world = new World();

        // Create an NPC with a schedule location
        var npc = world.CreateEntity();
        world.AddComponent(npc, new PositionComponent(0f, 0f));

        var npcComp = new NPCComponent("TestNPC", NPCRole.Villager);
        npcComp.AddSchedule(0f, 24f, 200f, 200f, "Wandering");
        world.AddComponent(npc, npcComp);

        // Create a player (so the system has a reference target)
        var player = world.CreateEntity();
        world.AddComponent(player, new PlayerComponent());
        world.AddComponent(player, new PositionComponent(300f, 300f));

        var pfSystem = new PathfindingSystem();
        pfSystem.Initialize(world);

        float startX = 0f;
        float startY = 0f;

        // Run several updates to allow path calculation and movement
        for (int i = 0; i < 20; i++)
            pfSystem.Update(world, 0.1f);

        var pos = world.GetComponent<PositionComponent>(npc)!;
        bool moved = pos.X != startX || pos.Y != startY;

        Console.WriteLine($"  NPC moved: {moved}  New pos: ({pos.X:F1}, {pos.Y:F1})");
        System.Diagnostics.Debug.Assert(moved, "NPC should have moved towards its schedule location");

        Console.WriteLine("✓ NPC path tracking working\n");
    }
}
