using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Integrated playable scene that combines all game systems into a cohesive experience
/// Includes: Terrain, Combat, Mining, Crafting, Weather, Time, Creatures, etc.
/// </summary>
public class IntegratedPlayableScene : Scene
{
    private Entity _playerEntity;
    private Entity _cameraEntity;
    private TerrainGenerator? _terrainGenerator;
    private ChunkManager? _chunkManager;
    private WorldCreatureManager? _creatureManager;
    private WeatherSystem? _weatherSystem;
    private TimeSystem? _timeSystem;
    
    public override void OnLoad()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Chronicles of a Drifter - Integrated Playable       ║");
        Console.WriteLine("║         Full Game Experience with All Systems            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
        
        // Initialize all game systems
        InitializeSystems();
        
        // Create terrain and world
        InitializeTerrain();
        
        // Create player with full capabilities
        CreatePlayer();
        
        // Create camera
        CreateCamera();
        
        // Initialize time and weather
        InitializeTimeAndWeather();
        
        // Spawn initial creatures
        InitializeCreatures();
        
        Console.WriteLine("\n[IntegratedGame] Game loaded successfully!");
        Console.WriteLine("[IntegratedGame] Use WASD/Arrows to move");
        Console.WriteLine("[IntegratedGame] Press M to mine blocks");
        Console.WriteLine("[IntegratedGame] Press P to place blocks");
        Console.WriteLine("[IntegratedGame] Press Space to attack");
        Console.WriteLine("[IntegratedGame] Press C to open crafting menu (console)");
        Console.WriteLine("[IntegratedGame] Press I to view inventory (console)");
        Console.WriteLine("[IntegratedGame] Press +/- to zoom");
        Console.WriteLine("[IntegratedGame] Press Q or ESC to quit\n");
    }
    
    private void InitializeSystems()
    {
        Console.WriteLine("[IntegratedGame] Initializing game systems...");
        
        // Core gameplay systems
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new CameraLookAheadSystem());
        World.AddSystem(new ScreenShakeSystem());
        World.AddSystem(new ParallaxSystem());
        
        // Combat and AI systems
        World.AddSystem(new ScriptSystem());
        World.AddSystem(new CombatSystem());
        World.AddSystem(new CreatureSpawnSystem());
        
        // World interaction systems
        World.AddSystem(new MiningSystem());
        World.AddSystem(new CraftingSystem());
        World.AddSystem(new LightingSystem());
        World.AddSystem(new CollisionSystem());
        World.AddSystem(new SwimmingSystem());
        
        // Rendering system (visual mode)
        World.AddSystem(new TerrainRenderingSystem());
        World.AddSystem(new RenderingSystem());
        
        Console.WriteLine("[IntegratedGame] ✓ All systems initialized");
    }
    
    private void InitializeTerrain()
    {
        Console.WriteLine("[IntegratedGame] Initializing terrain generation...");
        
        int seed = 12345; // Use consistent seed for reproducible world
        _terrainGenerator = new TerrainGenerator(seed);
        _chunkManager = new ChunkManager(useAsyncGeneration: true);
        _chunkManager.SetTerrainGenerator(_terrainGenerator);
        
        // Register chunk manager with world for other systems to access
        World.RegisterSharedResource("ChunkManager", _chunkManager);
        World.RegisterSharedResource("TerrainGenerator", _terrainGenerator);
        
        // Generate initial chunks around spawn point
        float spawnX = 500f;
        float spawnY = 5f; // Near surface
        int chunksToLoad = 7;
        
        for (int i = -chunksToLoad/2; i <= chunksToLoad/2; i++)
        {
            int chunkX = (int)(spawnX / ChunkManager.CHUNK_WIDTH) + i;
            _chunkManager.LoadChunkAt(chunkX);
        }
        
        Console.WriteLine($"[IntegratedGame] ✓ Generated {chunksToLoad} initial chunks");
    }
    
    private void CreatePlayer()
    {
        Console.WriteLine("[IntegratedGame] Creating player character...");
        
        _playerEntity = World.CreateEntity();
        
        // Position player at spawn point (near surface)
        float spawnX = 500f * GameConstants.BlockSize;
        float spawnY = 5f * GameConstants.BlockSize;
        World.AddComponent(_playerEntity, new PositionComponent(spawnX, spawnY));
        World.AddComponent(_playerEntity, new VelocityComponent());
        
        // Player characteristics
        World.AddComponent(_playerEntity, new PlayerComponent { Speed = 200.0f });
        World.AddComponent(_playerEntity, new SpriteComponent(0, 32, 32));
        World.AddComponent(_playerEntity, new HealthComponent(100));
        
        // Combat capabilities
        World.AddComponent(_playerEntity, new CombatComponent(damage: 20f, range: 80f, cooldown: 0.5f));
        
        // Collision
        World.AddComponent(_playerEntity, new CollisionComponent(
            GameConstants.PlayerCollisionWidth, 
            GameConstants.PlayerCollisionHeight,
            layer: CollisionLayer.Player
        ));
        
        // Mining and building
        var inventory = new InventoryComponent(40); // 40 slots
        World.AddComponent(_playerEntity, inventory);
        
        // Give player starting items
        inventory.AddItem(TileType.Torch, 20);
        inventory.AddItem(TileType.Wood, 50);
        inventory.AddItem(TileType.Stone, 30);
        
        var miningComponent = new MiningComponent
        {
            ToolType = ToolType.Stone,
            MiningSpeed = 2.0f,
            MiningRange = 96f
        };
        World.AddComponent(_playerEntity, miningComponent);
        
        // Lighting (player has a lantern)
        World.AddComponent(_playerEntity, new LightSourceComponent(
            radius: 8,
            intensity: 1.0f,
            color: (1.0f, 0.9f, 0.7f)
        ));
        
        // Swimming capability
        World.AddComponent(_playerEntity, new SwimmingComponent
        {
            MaxBreath = 100f,
            CurrentBreath = 100f,
            SwimSpeed = 0.6f
        });
        
        Console.WriteLine("[IntegratedGame] ✓ Player created with full capabilities");
    }
    
    private void CreateCamera()
    {
        Console.WriteLine("[IntegratedGame] Creating camera...");
        
        _cameraEntity = World.CreateEntity();
        var camera = new CameraComponent(1920, 1080)
        {
            Zoom = 1.5f, // Zoom in for better terrain visibility
            FollowSpeed = 8.0f
        };
        World.AddComponent(_cameraEntity, camera);
        
        var playerPos = World.GetComponent<PositionComponent>(_playerEntity);
        if (playerPos != null)
        {
            World.AddComponent(_cameraEntity, new PositionComponent(playerPos.X, playerPos.Y));
        }
        
        // Enable camera features
        World.AddComponent(_cameraEntity, new ScreenShakeComponent());
        CameraSystem.SetFollowTarget(World, _cameraEntity, _playerEntity, followSpeed: 8.0f);
        
        // Enable look-ahead
        CameraLookAheadSystem.EnableLookAhead(World, _cameraEntity,
            lookAheadDistance: 100.0f,
            lookAheadSpeed: 3.0f,
            offsetScale: 0.15f);
        
        // Create parallax layers for depth
        CreateParallaxLayers();
        
        Console.WriteLine("[IntegratedGame] ✓ Camera created with parallax layers");
    }
    
    private void CreateParallaxLayers()
    {
        // Background sky
        ParallaxSystem.CreateParallaxLayer(World, "Sky",
            parallaxFactor: 0.0f,
            zOrder: -150,
            visualType: ParallaxVisualType.Sky,
            color: ConsoleColor.DarkBlue,
            density: 0.5f);
        
        // Clouds
        ParallaxSystem.CreateParallaxLayer(World, "Clouds",
            parallaxFactor: 0.2f,
            zOrder: -100,
            visualType: ParallaxVisualType.Clouds,
            color: ConsoleColor.Gray,
            density: 0.3f,
            autoScrollX: 2.0f);
        
        // Distant mountains
        ParallaxSystem.CreateParallaxLayer(World, "Mountains",
            parallaxFactor: 0.4f,
            zOrder: -75,
            visualType: ParallaxVisualType.Mountains,
            color: ConsoleColor.DarkCyan,
            density: 0.6f);
    }
    
    private void InitializeTimeAndWeather()
    {
        Console.WriteLine("[IntegratedGame] Initializing time and weather systems...");
        
        // Initialize time system (start at 8 AM, 60x speed)
        _timeSystem = new TimeSystem(startHour: 8, timeScale: 60f);
        World.RegisterSharedResource("TimeSystem", _timeSystem);
        
        // Initialize weather system
        _weatherSystem = new WeatherSystem(12345);
        World.RegisterSharedResource("WeatherSystem", _weatherSystem);
        
        Console.WriteLine("[IntegratedGame] ✓ Time system initialized (8:00 AM, 60x speed)");
        Console.WriteLine("[IntegratedGame] ✓ Weather system initialized");
    }
    
    private void InitializeCreatures()
    {
        Console.WriteLine("[IntegratedGame] Initializing creature spawning...");
        
        _creatureManager = new WorldCreatureManager(12345);
        World.RegisterSharedResource("WorldCreatureManager", _creatureManager);
        
        // Spawn a few initial creatures near player for testing
        SpawnInitialCreatures();
        
        Console.WriteLine("[IntegratedGame] ✓ Creature manager initialized");
    }
    
    private void SpawnInitialCreatures()
    {
        var playerPos = World.GetComponent<PositionComponent>(_playerEntity);
        if (playerPos == null) return;
        
        // Spawn 3 goblins at various positions around player
        float[] offsets = { -200f, 200f, 0f };
        
        for (int i = 0; i < 3; i++)
        {
            float offsetX = offsets[i];
            float offsetY = -150f + (i * 75f);
            
            var goblin = World.CreateEntity();
            World.AddComponent(goblin, new PositionComponent(playerPos.X + offsetX, playerPos.Y + offsetY));
            World.AddComponent(goblin, new VelocityComponent());
            World.AddComponent(goblin, new SpriteComponent(1, 24, 24));
            World.AddComponent(goblin, new HealthComponent(30));
            World.AddComponent(goblin, new CombatComponent(damage: 5f, range: 75f, cooldown: 1.0f));
            World.AddComponent(goblin, new CollisionComponent(24, 24, layer: CollisionLayer.Enemy));
            World.AddComponent(goblin, new ScriptComponent("scripts/lua/enemies/goblin_ai.lua"));
            World.AddComponent(goblin, new CreatureComponent 
            { 
                CreatureType = "Goblin",
                IsHostile = true,
                SpawnBiome = Biome.Plains
            });
        }
    }
    
    public override void Update(float deltaTime)
    {
        // Update time system
        _timeSystem?.Update(deltaTime);
        
        // Update weather system
        var playerPos = World.GetComponent<PositionComponent>(_playerEntity);
        if (playerPos != null && _terrainGenerator != null && _weatherSystem != null)
        {
            // Get current biome at player position
            int chunkX = (int)(playerPos.X / GameConstants.BlockSize) / ChunkManager.CHUNK_WIDTH;
            var biome = _terrainGenerator.GetBiomeAt(chunkX * ChunkManager.CHUNK_WIDTH + ChunkManager.CHUNK_WIDTH / 2);
            _weatherSystem.Update(biome, deltaTime);
        }
        
        // Update creature manager
        if (_chunkManager != null && _creatureManager != null && playerPos != null)
        {
            _creatureManager.Update(World, _chunkManager, playerPos.X, playerPos.Y, deltaTime);
        }
        
        // Update chunk loading based on player position
        if (_chunkManager != null && playerPos != null)
        {
            int playerChunkX = (int)(playerPos.X / GameConstants.BlockSize) / ChunkManager.CHUNK_WIDTH;
            int loadDistance = 3; // Load 3 chunks in each direction
            
            for (int dx = -loadDistance; dx <= loadDistance; dx++)
            {
                _chunkManager.LoadChunkAt(playerChunkX + dx);
            }
            
            // Unload distant chunks
            _chunkManager.UnloadDistantChunks(playerChunkX, unloadDistance: 5);
        }
        
        // Update all ECS systems
        World.Update(deltaTime);
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[IntegratedGame] Unloading integrated playable scene...");
    }
}
