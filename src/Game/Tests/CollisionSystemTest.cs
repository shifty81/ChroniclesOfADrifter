using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the collision detection system
/// </summary>
public static class CollisionSystemTest
{
    public static void Run()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Collision System Test");
        Console.WriteLine("===========================================\n");
        
        TestAABBCollisionDetection();
        TestEntityToEntityCollision();
        TestTerrainCollision();
        TestCollisionLayers();
        TestSlidingCollision();
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Collision Tests Passed! ✓");
        Console.WriteLine("===========================================");
    }
    
    private static void TestAABBCollisionDetection()
    {
        Console.WriteLine("[Test 1] Testing AABB collision detection...");
        
        var world = new World();
        var collisionSystem = new CollisionSystem();
        world.AddSystem(collisionSystem);
        
        // Create two entities with collision boxes
        var entity1 = world.CreateEntity();
        world.AddComponent(entity1, new PositionComponent(100, 100));
        world.AddComponent(entity1, new VelocityComponent(0, 0));
        world.AddComponent(entity1, new CollisionComponent(32, 32, checkTerrain: false, checkEntities: true));
        
        var entity2 = world.CreateEntity();
        world.AddComponent(entity2, new PositionComponent(120, 120)); // Overlapping
        world.AddComponent(entity2, new VelocityComponent(0, 0));
        world.AddComponent(entity2, new CollisionComponent(32, 32, checkTerrain: false, checkEntities: true, isStatic: true));
        
        // Get collision components to check bounds
        var collision1 = world.GetComponent<CollisionComponent>(entity1);
        var collision2 = world.GetComponent<CollisionComponent>(entity2);
        var pos1 = world.GetComponent<PositionComponent>(entity1);
        var pos2 = world.GetComponent<PositionComponent>(entity2);
        
        var bounds1 = collision1!.GetBounds(pos1!.X, pos1.Y);
        var bounds2 = collision2!.GetBounds(pos2!.X, pos2.Y);
        
        // Check if boxes overlap
        bool overlapping = bounds1.left < bounds2.right &&
                          bounds1.right > bounds2.left &&
                          bounds1.top < bounds2.bottom &&
                          bounds1.bottom > bounds2.top;
        
        Console.WriteLine($"  Entity 1 bounds: ({bounds1.left:F1}, {bounds1.top:F1}) to ({bounds1.right:F1}, {bounds1.bottom:F1})");
        Console.WriteLine($"  Entity 2 bounds: ({bounds2.left:F1}, {bounds2.top:F1}) to ({bounds2.right:F1}, {bounds2.bottom:F1})");
        Console.WriteLine($"  Overlapping: {overlapping}");
        
        if (!overlapping)
        {
            throw new Exception("AABB collision detection failed - boxes should overlap");
        }
        
        Console.WriteLine("✓ AABB collision detection working\n");
    }
    
    private static void TestEntityToEntityCollision()
    {
        Console.WriteLine("[Test 2] Testing entity-to-entity collision...");
        
        var world = new World();
        var movementSystem = new MovementSystem();
        var collisionSystem = new CollisionSystem();
        world.AddSystem(movementSystem);
        world.AddSystem(collisionSystem);
        
        // Create a moving entity
        var movingEntity = world.CreateEntity();
        world.AddComponent(movingEntity, new PositionComponent(100, 100));
        world.AddComponent(movingEntity, new VelocityComponent(50, 0)); // Moving right
        world.AddComponent(movingEntity, new CollisionComponent(32, 32, checkTerrain: false, checkEntities: true));
        
        // Create a static obstacle
        var obstacle = world.CreateEntity();
        world.AddComponent(obstacle, new PositionComponent(150, 100));
        world.AddComponent(obstacle, new VelocityComponent(0, 0));
        world.AddComponent(obstacle, new CollisionComponent(32, 32, checkTerrain: false, checkEntities: true, isStatic: true));
        
        var movingPos = world.GetComponent<PositionComponent>(movingEntity);
        float originalX = movingPos!.X;
        
        Console.WriteLine($"  Moving entity starting position: ({movingPos.X:F1}, {movingPos.Y:F1})");
        Console.WriteLine($"  Obstacle position: (150.0, 100.0)");
        
        // Update for 1 second - entity should collide with obstacle
        collisionSystem.Update(world, 1.0f);
        
        Console.WriteLine($"  Moving entity position after collision: ({movingPos.X:F1}, {movingPos.Y:F1})");
        
        // Entity should be stopped by collision, not at the obstacle position
        if (movingPos.X >= 150)
        {
            throw new Exception("Entity-to-entity collision failed - entity passed through obstacle");
        }
        
        Console.WriteLine("✓ Entity-to-entity collision working\n");
    }
    
    private static void TestTerrainCollision()
    {
        Console.WriteLine("[Test 3] Testing terrain collision...");
        
        var world = new World();
        var movementSystem = new MovementSystem();
        var collisionSystem = new CollisionSystem();
        world.AddSystem(movementSystem);
        world.AddSystem(collisionSystem);
        
        // Create chunk manager and generate terrain
        var chunkManager = new ChunkManager();
        var terrainGenerator = new TerrainGenerator(seed: 12345);
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Generate a chunk with terrain
        var chunk = chunkManager.GetChunk(0);
        
        // Set chunk manager in collision system
        collisionSystem.SetChunkManager(chunkManager);
        
        // Find a solid block position
        int solidBlockX = -1;
        int solidBlockY = -1;
        
        for (int x = 0; x < Chunk.CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
            {
                var tile = chunk.GetTile(x, y);
                if (tile.IsSolid())
                {
                    solidBlockX = x;
                    solidBlockY = y;
                    break;
                }
            }
            if (solidBlockX >= 0)
                break;
        }
        
        if (solidBlockX < 0)
        {
            throw new Exception("Could not find a solid block in generated terrain");
        }
        
        Console.WriteLine($"  Found solid block at: ({solidBlockX}, {solidBlockY})");
        
        // Create entity trying to move into solid block
        float blockWorldX = solidBlockX * 32.0f + 16.0f; // Center of block
        float blockWorldY = solidBlockY * 32.0f + 16.0f;
        
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(blockWorldX - 50, blockWorldY));
        world.AddComponent(entity, new VelocityComponent(100, 0)); // Moving toward block
        world.AddComponent(entity, new CollisionComponent(16, 16, checkTerrain: true, checkEntities: false));
        
        var pos = world.GetComponent<PositionComponent>(entity);
        float startX = pos!.X;
        
        Console.WriteLine($"  Entity starting position: ({startX:F1}, {blockWorldY:F1})");
        Console.WriteLine($"  Solid block center: ({blockWorldX:F1}, {blockWorldY:F1})");
        
        // Update collision system
        collisionSystem.Update(world, 1.0f);
        
        Console.WriteLine($"  Entity position after collision: ({pos.X:F1}, {pos.Y:F1})");
        
        // Entity should be stopped before reaching block center
        if (Math.Abs(pos.X - blockWorldX) < 5.0f)
        {
            throw new Exception("Terrain collision failed - entity moved into solid block");
        }
        
        Console.WriteLine("✓ Terrain collision working\n");
    }
    
    private static void TestCollisionLayers()
    {
        Console.WriteLine("[Test 4] Testing collision layers...");
        
        var world = new World();
        var collisionSystem = new CollisionSystem();
        world.AddSystem(collisionSystem);
        
        // Create player entity
        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(100, 100));
        world.AddComponent(player, new VelocityComponent(50, 0));
        world.AddComponent(player, new CollisionComponent(32, 32, 
            checkTerrain: false, 
            checkEntities: true,
            layer: CollisionLayer.Player,
            collidesWith: CollisionLayer.Enemy | CollisionLayer.Terrain));
        
        // Create enemy (should collide with player)
        var enemy = world.CreateEntity();
        world.AddComponent(enemy, new PositionComponent(150, 100));
        world.AddComponent(enemy, new VelocityComponent(0, 0));
        world.AddComponent(enemy, new CollisionComponent(32, 32,
            checkTerrain: false,
            checkEntities: true,
            isStatic: true,
            layer: CollisionLayer.Enemy));
        
        // Create projectile (should NOT collide with player due to layer filtering)
        var projectile = world.CreateEntity();
        world.AddComponent(projectile, new PositionComponent(100, 100));
        world.AddComponent(projectile, new VelocityComponent(0, 0));
        world.AddComponent(projectile, new CollisionComponent(8, 8,
            checkTerrain: false,
            checkEntities: true,
            isStatic: true,
            layer: CollisionLayer.Projectile));
        
        var playerPos = world.GetComponent<PositionComponent>(player);
        float startX = playerPos!.X;
        
        Console.WriteLine($"  Player layer: {CollisionLayer.Player}");
        Console.WriteLine($"  Player collides with: {CollisionLayer.Enemy | CollisionLayer.Terrain}");
        Console.WriteLine($"  Enemy at (150, 100) - should collide");
        Console.WriteLine($"  Projectile at (100, 100) - should NOT collide due to layer filtering");
        
        // Update collision
        collisionSystem.Update(world, 1.0f);
        
        Console.WriteLine($"  Player position after: ({playerPos.X:F1}, {playerPos.Y:F1})");
        
        // Player should be stopped by enemy but not by projectile
        if (playerPos.X >= 150)
        {
            throw new Exception("Collision layer filtering failed - player should collide with enemy");
        }
        
        Console.WriteLine("✓ Collision layer filtering working\n");
    }
    
    private static void TestSlidingCollision()
    {
        Console.WriteLine("[Test 5] Testing sliding collision (moving along walls)...");
        
        var world = new World();
        var collisionSystem = new CollisionSystem();
        world.AddSystem(collisionSystem);
        
        // Create chunk manager with a wall
        var chunkManager = new ChunkManager();
        var chunk = chunkManager.GetChunk(0);
        
        // Create a vertical wall at x=5
        for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
        {
            chunk.SetTile(5, y, TileType.Stone);
        }
        
        collisionSystem.SetChunkManager(chunkManager);
        
        // Create entity trying to move diagonally into wall
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(4 * 32.0f, 10 * 32.0f));
        world.AddComponent(entity, new VelocityComponent(50, 50)); // Moving right and down
        world.AddComponent(entity, new CollisionComponent(16, 16, checkTerrain: true, checkEntities: false));
        
        var pos = world.GetComponent<PositionComponent>(entity);
        float startX = pos!.X;
        float startY = pos.Y;
        
        Console.WriteLine($"  Entity starting position: ({startX:F1}, {startY:F1})");
        Console.WriteLine($"  Moving diagonally (50, 50) toward wall at x=160");
        
        // Update collision
        collisionSystem.Update(world, 0.5f);
        
        Console.WriteLine($"  Entity position after collision: ({pos.X:F1}, {pos.Y:F1})");
        
        // Entity should slide down along the wall (Y should increase, X should be blocked)
        if (pos.Y <= startY)
        {
            throw new Exception("Sliding collision failed - entity should slide along wall");
        }
        
        if (pos.X > 5 * 32.0f)
        {
            throw new Exception("Sliding collision failed - entity passed through wall");
        }
        
        Console.WriteLine($"  Successfully slid along wall: Y changed by {pos.Y - startY:F1}, X blocked at wall");
        Console.WriteLine("✓ Sliding collision working\n");
    }
}
