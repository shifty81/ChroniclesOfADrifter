using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Visual demo scene showing graphical rendering with terrain generation
/// Demonstrates Zelda: A Link to the Past style tile-based rendering with procedural world
/// Press F1 or ~ to enable in-game editor
/// </summary>
public class VisualDemoScene : Scene
{
    private Entity _playerEntity;
    private Entity _cameraEntity;
    private ChunkManager? chunkManager;
    private TerrainGenerator? terrainGenerator;
    private InGameEditor? inGameEditor;
    
    public override void OnLoad()
    {
        Console.WriteLine("[VisualDemo] Loading visual demo scene...");
        
        // Initialize terrain generation
        terrainGenerator = new TerrainGenerator(seed: 67890);
        chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Store chunk manager as shared resource
        World.SetSharedResource("ChunkManager", chunkManager);
        
        Console.WriteLine("[VisualDemo] Terrain generator initialized with seed: 67890");
        
        // Add systems
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new LightingSystem()); // Add lighting system
        World.AddSystem(new TerrainRenderingSystem()); // Use terrain rendering instead of basic visual rendering
        
        // Create camera entity
        _cameraEntity = World.CreateEntity();
        var camera = new CameraComponent(1280, 720)
        {
            Zoom = 1.0f,
            FollowSpeed = 5.0f
        };
        World.AddComponent(_cameraEntity, camera);
        World.AddComponent(_cameraEntity, new PositionComponent(640, 150)); // Start at player position
        
        // Create player entity (yellow square representing Link) on surface
        _playerEntity = World.CreateEntity();
        World.AddComponent(_playerEntity, new PositionComponent(640, 150)); // Spawn on surface
        World.AddComponent(_playerEntity, new VelocityComponent());
        World.AddComponent(_playerEntity, new SpriteComponent(0, 32, 32));
        World.AddComponent(_playerEntity, new PlayerComponent { Speed = 200.0f });
        World.AddComponent(_playerEntity, new HealthComponent(100));
        
        // Add player light source
        World.AddComponent(_playerEntity, new LightSourceComponent(
            radius: 8.0f,
            intensity: 1.0f,
            type: LightSourceType.Player
        ));
        
        // Set camera to follow player
        CameraSystem.SetFollowTarget(World, _cameraEntity, _playerEntity, followSpeed: 5.0f);
        
        // Set camera bounds (larger world for exploration - procedurally generated)
        CameraSystem.SetBounds(World, _cameraEntity,
            minX: 0, maxX: 5000,
            minY: 0, maxY: 960); // 30 blocks * 32 pixels
        
        // Pre-generate initial chunks around player spawn
        var playerPos = World.GetComponent<PositionComponent>(_playerEntity);
        if (playerPos != null && chunkManager != null)
        {
            chunkManager.UpdateChunks(playerPos.X);
            Console.WriteLine($"[VisualDemo] Generated {chunkManager.GetLoadedChunkCount()} initial chunks");
        }
        
        // Initialize in-game editor
        inGameEditor = new InGameEditor(World, chunkManager);
        
        Console.WriteLine("[VisualDemo] Visual demo scene loaded!");
        Console.WriteLine("[VisualDemo] A graphical window should appear with procedurally generated terrain!");
        Console.WriteLine("[VisualDemo] Explore the world with varied biomes, caves, and vegetation!");
        Console.WriteLine("[VisualDemo] Use WASD or Arrow keys to move");
        Console.WriteLine("[VisualDemo] Use +/- keys to zoom in/out");
        Console.WriteLine("[VisualDemo] Press F1 or ~ to toggle in-game editor");
        Console.WriteLine("[VisualDemo] Press Q or ESC to quit");
    }
    
    public override void Update(float deltaTime)
    {
        // Update in-game editor first
        inGameEditor?.Update(deltaTime);
        
        World.Update(deltaTime);
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[VisualDemo] Unloading visual demo scene...");
    }
}
