using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing terrain generation with mining/digging mechanics
/// </summary>
public class MiningDemoScene : Scene
{
    private ChunkManager? chunkManager;
    private TerrainGenerator? terrainGenerator;
    private Entity playerEntity;
    private MiningSystem? miningSystem;
    
    public override void OnLoad()
    {
        Console.WriteLine("[MiningDemo] Loading mining demo scene...");
        
        // Add systems
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        
        // Add mining system
        miningSystem = new MiningSystem();
        World.AddSystem(miningSystem);
        
        // Initialize terrain generation
        terrainGenerator = new TerrainGenerator(seed: 12345);
        chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Store chunk manager as shared resource so mining system can access it
        World.SetSharedResource("ChunkManager", chunkManager);
        
        Console.WriteLine("[MiningDemo] Terrain generator initialized with seed: 12345");
        
        // Create player entity at spawn point (world coordinates)
        playerEntity = World.CreateEntity();
        World.AddComponent(playerEntity, new PositionComponent(500, 150)); // X=500, Y=150
        World.AddComponent(playerEntity, new VelocityComponent());
        World.AddComponent(playerEntity, new SpriteComponent(0, 24, 24));
        World.AddComponent(playerEntity, new PlayerComponent { Speed = 100.0f });
        World.AddComponent(playerEntity, new HealthComponent(100));
        
        // Add inventory for collecting resources
        var inventory = new InventoryComponent(maxSlots: 40);
        World.AddComponent(playerEntity, inventory);
        
        // Add a basic stone pickaxe to start
        var tool = new ToolComponent(ToolType.Pickaxe, ToolMaterial.Stone);
        World.AddComponent(playerEntity, tool);
        
        Console.WriteLine($"[MiningDemo] Player spawned at (500, 150) with Stone Pickaxe");
        
        // Create camera
        var cameraEntity = World.CreateEntity();
        var cameraComponent = new CameraComponent(1920, 1080)
        {
            X = 500,
            Y = 150,
            Zoom = 1.0f,
            FollowSpeed = 5.0f
        };
        World.AddComponent(cameraEntity, cameraComponent);
        World.AddComponent(cameraEntity, new PositionComponent(500, 150));
        
        // Set camera to follow player
        CameraSystem.SetFollowTarget(World, cameraEntity, playerEntity, followSpeed: 5.0f);
        
        Console.WriteLine("[MiningDemo] Camera created and following player");
        
        // Pre-generate initial chunks around player spawn
        var playerPos = World.GetComponent<PositionComponent>(playerEntity);
        if (playerPos != null)
        {
            chunkManager.UpdateChunks(playerPos.X);
        }
        
        Console.WriteLine($"[MiningDemo] Generated {chunkManager.GetLoadedChunkCount()} initial chunks");
        Console.WriteLine("[MiningDemo] Scene loaded!");
        Console.WriteLine("[MiningDemo] Controls:");
        Console.WriteLine("[MiningDemo]   - WASD or Arrow keys to move");
        Console.WriteLine("[MiningDemo]   - Hold 'M' key to mine blocks near you");
        Console.WriteLine("[MiningDemo]   - Currently equipped: Stone Pickaxe");
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
        
        // Display mining progress if mining
        if (miningSystem != null && miningSystem.IsMining())
        {
            float progress = miningSystem.GetMiningProgress();
            // This would be displayed visually in a real game
            // For console, we could print it periodically
        }
        
        // Display inventory every few seconds (for demo purposes)
        if (frameCounter++ % 300 == 0) // Every 5 seconds at 60 FPS
        {
            DisplayInventory();
        }
    }
    
    private int frameCounter = 0;
    
    private void DisplayInventory()
    {
        var inventory = World.GetComponent<InventoryComponent>(playerEntity);
        if (inventory == null) return;
        
        var items = inventory.GetAllItems();
        if (items.Count > 0)
        {
            Console.WriteLine("\n[MiningDemo] === Inventory ===");
            foreach (var item in items)
            {
                Console.WriteLine($"[MiningDemo]   {item.Key}: {item.Value}");
            }
            Console.WriteLine("[MiningDemo] ==================\n");
        }
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[MiningDemo] Unloading mining demo...");
        
        // Display final inventory
        Console.WriteLine("\n[MiningDemo] === Final Inventory ===");
        var inventory = World.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null)
        {
            var items = inventory.GetAllItems();
            foreach (var item in items)
            {
                Console.WriteLine($"[MiningDemo]   {item.Key}: {item.Value}");
            }
        }
        Console.WriteLine("[MiningDemo] =====================\n");
    }
    
    /// <summary>
    /// Gets the chunk manager (for rendering)
    /// </summary>
    public ChunkManager? GetChunkManager()
    {
        return chunkManager;
    }
}
