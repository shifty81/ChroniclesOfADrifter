using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the collision detection system
/// Player can move around a terrain with walls and obstacles
/// </summary>
public class CollisionDemoScene : Scene
{
    private ChunkManager? chunkManager;
    
    public override void OnLoad()
    {
        Console.WriteLine("[CollisionDemo] Loading collision demo scene...");
        
        // Initialize chunk manager and terrain
        chunkManager = new ChunkManager();
        var terrainGenerator = new TerrainGenerator(seed: 42);
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Generate initial chunks
        for (int chunkX = -2; chunkX <= 2; chunkX++)
        {
            chunkManager.GetChunk(chunkX);
        }
        
        // Add systems (order matters!)
        World.AddSystem(new PlayerInputSystem());
        var movementSystem = new MovementSystem();
        World.AddSystem(movementSystem);
        
        var collisionSystem = new CollisionSystem();
        collisionSystem.SetChunkManager(chunkManager);
        World.AddSystem(collisionSystem);
        
        World.AddSystem(new CameraSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create player entity with collision
        var player = World.CreateEntity();
        World.AddComponent(player, new PlayerComponent { Speed = 200.0f });
        World.AddComponent(player, new PositionComponent(960, 200)); // Start in air above terrain
        World.AddComponent(player, new VelocityComponent());
        World.AddComponent(player, new SpriteComponent(0, 32, 32));
        
        // Add collision component to player using standard player dimensions
        World.AddComponent(player, new CollisionComponent(
            width: GameConstants.PlayerCollisionWidth,
            height: GameConstants.PlayerCollisionHeight,
            offsetX: 0,
            offsetY: 0,
            isStatic: false,
            checkTerrain: true,  // Check collision with terrain
            checkEntities: true, // Check collision with other entities
            layer: CollisionLayer.Player,
            collidesWith: CollisionLayer.Enemy | CollisionLayer.Terrain | CollisionLayer.Default
        ));
        
        // Create camera that follows player
        var camera = World.CreateEntity();
        var cameraComponent = new CameraComponent(1920, 1080)
        {
            Zoom = 1.0f,
            FollowSpeed = 8.0f
        };
        World.AddComponent(camera, cameraComponent);
        World.AddComponent(camera, new PositionComponent(960, 540));
        CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 8.0f);
        
        // Create some static obstacle entities
        CreateObstacle(World, 800, 400, 64, 64);
        CreateObstacle(World, 1100, 400, 64, 64);
        CreateObstacle(World, 950, 600, 128, 32);
        
        // Create some enemy entities
        CreateEnemy(World, 700, 350);
        CreateEnemy(World, 1200, 350);
        
        Console.WriteLine("[CollisionDemo] Demo scene loaded!");
        Console.WriteLine("[CollisionDemo] Use WASD or Arrow keys to move");
        Console.WriteLine("[CollisionDemo] Player will collide with terrain blocks and obstacles");
        Console.WriteLine("[CollisionDemo] Player can slide along walls for smooth movement");
        Console.WriteLine("[CollisionDemo] Try moving into solid blocks - collision prevents passage!");
    }
    
    private void CreateObstacle(World world, float x, float y, float width, float height)
    {
        var obstacle = world.CreateEntity();
        world.AddComponent(obstacle, new PositionComponent(x, y));
        world.AddComponent(obstacle, new VelocityComponent(0, 0));
        world.AddComponent(obstacle, new SpriteComponent(2, (int)width, (int)height));
        world.AddComponent(obstacle, new CollisionComponent(
            width: width,
            height: height,
            isStatic: true,  // Static obstacles don't move
            checkTerrain: false,
            checkEntities: false,
            layer: CollisionLayer.Default
        ));
    }
    
    private void CreateEnemy(World world, float x, float y)
    {
        var enemy = world.CreateEntity();
        world.AddComponent(enemy, new PositionComponent(x, y));
        world.AddComponent(enemy, new VelocityComponent(0, 0));
        world.AddComponent(enemy, new SpriteComponent(1, 24, 24));
        world.AddComponent(enemy, new CollisionComponent(
            width: 24,
            height: 24,
            isStatic: false,
            checkTerrain: true,
            checkEntities: true,
            layer: CollisionLayer.Enemy,
            collidesWith: CollisionLayer.Player | CollisionLayer.Enemy | CollisionLayer.Terrain
        ));
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[CollisionDemo] Unloading collision demo scene...");
    }
    
    public override void Update(float deltaTime)
    {
        // Update chunk loading based on player position
        if (chunkManager != null)
        {
            var playerEntities = World.GetEntitiesWithComponent<PlayerComponent>();
            foreach (var playerEntity in playerEntities)
            {
                var pos = World.GetComponent<PositionComponent>(playerEntity);
                if (pos != null)
                {
                    chunkManager.UpdateChunks(pos.X);
                }
            }
        }
        
        // Update all systems
        base.Update(deltaTime);
    }
}
