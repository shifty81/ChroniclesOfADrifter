using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing terrain generation with biomes
/// </summary>
public class TerrainDemoScene : Scene
{
    private ChunkManager? chunkManager;
    private TerrainGenerator? terrainGenerator;
    private Entity playerEntity;
    
    public override void OnLoad()
    {
        Console.WriteLine("[TerrainDemo] Loading terrain generation demo...");
        
        // Add systems
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        
        // Initialize terrain generation
        terrainGenerator = new TerrainGenerator(seed: 12345);
        chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        Console.WriteLine("[TerrainDemo] Terrain generator initialized with seed: 12345");
        
        // Create player entity at spawn point (world coordinates)
        playerEntity = World.CreateEntity();
        World.AddComponent(playerEntity, new PositionComponent(100, 5)); // X=100, Y=5 (near surface)
        World.AddComponent(playerEntity, new VelocityComponent());
        World.AddComponent(playerEntity, new SpriteComponent(0, 24, 24));
        World.AddComponent(playerEntity, new PlayerComponent { Speed = 100.0f });
        World.AddComponent(playerEntity, new HealthComponent(100));
        
        Console.WriteLine($"[TerrainDemo] Player spawned at ({100}, {5})");
        
        // Create camera
        var cameraEntity = World.CreateEntity();
        var cameraComponent = new CameraComponent(1920, 1080)
        {
            X = 100,
            Y = 5,
            Zoom = 1.0f,
            FollowSpeed = 5.0f
        };
        World.AddComponent(cameraEntity, cameraComponent);
        World.AddComponent(cameraEntity, new PositionComponent(100, 5));
        
        // Set camera to follow player
        CameraSystem.SetFollowTarget(World, cameraEntity, playerEntity, followSpeed: 5.0f);
        
        Console.WriteLine("[TerrainDemo] Camera created and following player");
        
        // Pre-generate initial chunks around player spawn
        var playerPos = World.GetComponent<PositionComponent>(playerEntity);
        if (playerPos != null)
        {
            chunkManager.UpdateChunks(playerPos.X);
        }
        
        Console.WriteLine($"[TerrainDemo] Generated {chunkManager.GetLoadedChunkCount()} initial chunks");
        Console.WriteLine("[TerrainDemo] Scene loaded! Use WASD or Arrow keys to move");
    }
    
    public override void Update(float deltaTime)
    {
        if (World == null || chunkManager == null) return;
        
        // Update chunks based on player position
        var playerPos = World.GetComponent<PositionComponent>(playerEntity);
        if (playerPos != null)
        {
            chunkManager.UpdateChunks(playerPos.X);
        }
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[TerrainDemo] Unloading terrain demo...");
    }
    
    /// <summary>
    /// Gets the chunk manager (for rendering)
    /// </summary>
    public ChunkManager? GetChunkManager()
    {
        return chunkManager;
    }
}
