using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the creature spawning system
/// </summary>
public class CreatureSpawnDemoScene : Scene
{
    private ChunkManager? chunkManager;
    private TerrainGenerator? terrainGenerator;
    private CreatureSpawnSystem? spawnSystem;
    
    public override void OnLoad()
    {
        Console.WriteLine("[CreatureSpawnDemo] Loading creature spawn demo scene...");
        
        // World is already created by base Scene constructor
        
        // Create terrain generator and chunk manager
        int seed = 12345;
        terrainGenerator = new TerrainGenerator(seed);
        chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Create creature spawn system
        spawnSystem = new CreatureSpawnSystem(seed);
        
        // Add core systems
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new ScriptSystem());  // ScriptSystem creates its own ScriptEngine
        var collisionSystem = new CollisionSystem();
        collisionSystem.SetChunkManager(chunkManager);
        World.AddSystem(collisionSystem);
        World.AddSystem(spawnSystem);
        World.AddSystem(new CombatSystem());
        
        // Create player
        Entity player = World.CreateEntity();
        World.AddComponent(player, new PositionComponent(500f, 200f));
        World.AddComponent(player, new VelocityComponent());
        World.AddComponent(player, new PlayerComponent());
        World.AddComponent(player, new HealthComponent(100f));
        World.AddComponent(player, new CombatComponent(25f, 75f, 0.5f));
        World.AddComponent(player, new CollisionComponent(32f, 48f, 0, 0, false, true, true, CollisionLayer.Player, CollisionLayer.All));
        World.AddComponent(player, new SpriteComponent(0, 64, 64));  // TextureId 0 = placeholder
        
        // Create camera following player
        Entity camera = World.CreateEntity();
        var cameraComp = new CameraComponent(1920, 1080);
        cameraComp.FollowTarget = player;
        cameraComp.FollowSpeed = 5.0f;
        World.AddComponent(camera, cameraComp);
        World.AddComponent(camera, new PositionComponent(500f, 200f));
        
        // Create spawn points in different biomes
        CreateSpawnPoints();
        
        // Spawn some initial creatures manually for testing
        SpawnInitialCreatures();
        
        Console.WriteLine("[CreatureSpawnDemo] Scene loaded successfully");
        Console.WriteLine("[CreatureSpawnDemo] Use WASD to move player");
        Console.WriteLine("[CreatureSpawnDemo] Creatures will spawn and behave based on their AI");
    }
    
    private void CreateSpawnPoints()
    {
        if (spawnSystem == null) return;
        
        // Plains spawn point - passive animals
        Entity plainsSpawn = World.CreateEntity();
        World.AddComponent(plainsSpawn, new PositionComponent(300f, 150f));
        World.AddComponent(plainsSpawn, new SpawnPointComponent(CreatureType.Rabbit, 200f, 5, 20f));
        
        // Forest spawn point - mix of passive and hostile
        Entity forestSpawn = World.CreateEntity();
        World.AddComponent(forestSpawn, new PositionComponent(800f, 150f));
        World.AddComponent(forestSpawn, new SpawnPointComponent(CreatureType.Deer, 250f, 3, 25f));
        
        Entity wolfSpawn = World.CreateEntity();
        World.AddComponent(wolfSpawn, new PositionComponent(1000f, 150f));
        World.AddComponent(wolfSpawn, new SpawnPointComponent(CreatureType.Wolf, 300f, 2, 30f));
        
        // Goblin spawn point
        Entity goblinSpawn = World.CreateEntity();
        World.AddComponent(goblinSpawn, new PositionComponent(1500f, 150f));
        World.AddComponent(goblinSpawn, new SpawnPointComponent(CreatureType.Goblin, 250f, 4, 25f));
        
        Console.WriteLine("[CreatureSpawnDemo] Created spawn points at various locations");
    }
    
    private void SpawnInitialCreatures()
    {
        if (spawnSystem == null) return;
        
        // Spawn some creatures immediately for demonstration
        spawnSystem.SpawnCreature(World, CreatureType.Rabbit, 400f, 150f);
        spawnSystem.SpawnCreature(World, CreatureType.Rabbit, 450f, 180f);
        spawnSystem.SpawnCreature(World, CreatureType.Deer, 700f, 150f);
        spawnSystem.SpawnCreature(World, CreatureType.Bird, 600f, 120f);
        spawnSystem.SpawnCreature(World, CreatureType.Wolf, 1100f, 160f);
        spawnSystem.SpawnCreature(World, CreatureType.Goblin, 1400f, 150f);
        spawnSystem.SpawnCreature(World, CreatureType.Goblin, 1500f, 180f);
        
        // Spawn underground creatures at various depths
        spawnSystem.SpawnByBiomeAndDepth(World, BiomeType.Plains, 5, 500f, 400f, 2);  // Shallow underground
        spawnSystem.SpawnByBiomeAndDepth(World, BiomeType.Forest, 10, 800f, 500f, 2); // Deep underground
        spawnSystem.SpawnByBiomeAndDepth(World, BiomeType.Snow, 17, 1200f, 600f, 1);  // Very deep
        
        Console.WriteLine("[CreatureSpawnDemo] Spawned initial creatures for testing");
    }
    
    public override void Update(float deltaTime)
    {
        if (World == null) return;
        
        // Update chunk loading based on player position
        var playerEntities = World.GetEntitiesWithComponent<PlayerComponent>();
        var playerList = playerEntities.ToList();
        if (playerList.Count > 0)
        {
            var playerPos = World.GetComponent<PositionComponent>(playerList[0]);
            if (playerPos != null && chunkManager != null)
            {
                chunkManager.UpdateChunks(playerPos.X);
            }
        }
        
        // Update all systems
        World.Update(deltaTime);
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[CreatureSpawnDemo] Unloading scene...");
    }
}
