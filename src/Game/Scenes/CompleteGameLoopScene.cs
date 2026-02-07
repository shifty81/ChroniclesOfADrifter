using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;
using ChroniclesOfADrifter.UI;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Complete game loop demonstration integrating all major systems:
/// - Procedural terrain generation with biomes
/// - Player movement and combat
/// - Mining and building
/// - Inventory and crafting
/// - Day/night cycle and weather
/// - Enemy spawning and AI
/// - Quest system
/// - UI framework
/// 
/// This scene showcases a playable slice of the complete game experience.
/// </summary>
public class CompleteGameLoopScene : Scene
{
    private ChunkManager? chunkManager;
    private TerrainGenerator? terrainGenerator;
    private StructureGenerator? structureGenerator;
    private TimeSystem? timeSystem;
    private WeatherSystem? weatherSystem;
    private SaveSystem? saveSystem;
    private LootDropSystem? lootDropSystem;
    private DeathSystem? deathSystem;
    private Entity playerEntity;
    private Entity cameraEntity;
    private float gameTime = 0f;
    private int enemiesDefeated = 0;
    private int resourcesGathered = 0;
    private int itemsCrafted = 0;
    
    public override void OnLoad()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          COMPLETE GAME LOOP DEMONSTRATION                        ║");
        Console.WriteLine("║          Chronicles of a Drifter - All Systems Integrated        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");
        
        // Initialize world generation
        InitializeWorldGeneration();
        
        // Initialize time and weather systems
        InitializeWorldSystems();
        
        // Add all core ECS systems
        InitializeECSSystems();
        
        // Create the player with all capabilities
        CreatePlayer();
        
        // Create the camera
        CreateCamera();
        
        // Generate initial terrain
        GenerateInitialTerrain();
        
        // Generate structures (villages, dungeons)
        GenerateWorldStructures();
        
        // Create parallax background
        CreateParallaxLayers();
        
        // Spawn initial creatures
        SpawnInitialCreatures();
        
        // Create boss encounter
        CreateBossEncounter();
        
        // Create NPCs and quests
        CreateNPCsAndQuests();
        
        // Give player starting items
        GiveStartingItems();
        
        // Create UI overlays
        CreateUIOverlays();
        
        // Display game information
        DisplayGameInfo();
    }
    
    private void InitializeWorldGeneration()
    {
        Console.WriteLine("[GameLoop] Initializing world generation...");
        
        // Create terrain generator with a seed
        int worldSeed = 42069; // Fixed seed for reproducible worlds
        terrainGenerator = new TerrainGenerator(seed: worldSeed);
        chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Create structure generator
        structureGenerator = new StructureGenerator(worldSeed);
        
        // Store chunk manager as shared resource for all systems
        World.SetSharedResource("ChunkManager", chunkManager);
        
        Console.WriteLine($"  ✓ World seed: {worldSeed}");
        Console.WriteLine($"  ✓ Chunk size: 32x30 blocks (surface + 20 underground layers)");
        Console.WriteLine($"  ✓ 8 biomes available");
        Console.WriteLine($"  ✓ Structure generator initialized");
    }
    
    private void InitializeWorldSystems()
    {
        Console.WriteLine("[GameLoop] Initializing world systems...");
        
        // Time system with accelerated time (60x = 1 real second = 1 game minute)
        timeSystem = new TimeSystem();
        timeSystem.TimeScale = 60f;
        World.SetSharedResource("TimeSystem", timeSystem);
        
        // Weather system
        weatherSystem = new WeatherSystem(seed: 42069);
        World.SetSharedResource("WeatherSystem", weatherSystem);
        
        Console.WriteLine("  ✓ Time system: 60x speed (1 real sec = 1 game min)");
        Console.WriteLine("  ✓ Starting time: Game start");
        Console.WriteLine("  ✓ Weather system: Active");
    }
    
    private void InitializeECSSystems()
    {
        Console.WriteLine("[GameLoop] Initializing ECS systems...");
        
        // Save system
        saveSystem = new SaveSystem();
        World.AddSystem(saveSystem);
        
        // Input and movement
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new MovementSystem());
        
        // Camera systems
        World.AddSystem(new CameraSystem());
        World.AddSystem(new CameraLookAheadSystem());
        World.AddSystem(new ScreenShakeSystem());
        World.AddSystem(new CameraZoneSystem());
        World.AddSystem(new ParallaxSystem());
        
        // Combat and AI
        World.AddSystem(new ScriptSystem());
        World.AddSystem(new CombatSystem());
        
        // Status effects (before combat so effects are processed each frame)
        World.AddSystem(new StatusEffectSystem());
        
        // Projectile system (ranged combat)
        World.AddSystem(new ProjectileSystem());
        
        // Experience system (before loot/death so XP shows before loot messages)
        World.AddSystem(new ExperienceSystem());
        
        // Loot system (before death system)
        lootDropSystem = new LootDropSystem(seed: 42069);
        World.AddSystem(lootDropSystem);
        
        // Death system (after loot system)
        deathSystem = new DeathSystem();
        World.AddSystem(deathSystem);
        deathSystem.SetLootDropSystem(lootDropSystem);
        
        World.AddSystem(new CreatureSpawnSystem(seed: 42069));
        
        // Mining and building
        World.AddSystem(new MiningSystem());
        World.AddSystem(new BlockInteractionSystem());
        
        // Crafting and inventory
        World.AddSystem(new CraftingSystem());
        
        // Farming
        World.AddSystem(new FarmingSystem());
        
        // Swimming and water
        World.AddSystem(new SwimmingSystem());
        
        // Quest and NPC
        World.AddSystem(new QuestSystem());
        World.AddSystem(new NPCSystem());
        
        // Boss encounters
        World.AddSystem(new BossSystem());
        
        // Lighting
        World.AddSystem(new LightingSystem());
        
        // Audio (framework)
        World.AddSystem(new AudioSystem());
        
        // Rendering
        World.AddSystem(new TerrainRenderingSystem());
        World.AddSystem(new AnimationSystem());
        
        // UI
        World.AddSystem(new UISystem());
        
        Console.WriteLine("  ✓ 34 core systems initialized");
    }
    
    private void CreatePlayer()
    {
        Console.WriteLine("[GameLoop] Creating player character...");
        
        playerEntity = World.CreateEntity();
        
        // Core components
        World.AddComponent(playerEntity, new PlayerComponent { Speed = 150.0f });
        World.AddComponent(playerEntity, new PositionComponent(500, 150)); // Spawn on surface
        World.AddComponent(playerEntity, new VelocityComponent());
        
        // Visuals
        World.AddComponent(playerEntity, new SpriteComponent(0, 32, 64));
        
        // Health and combat
        World.AddComponent(playerEntity, new HealthComponent(100));
        World.AddComponent(playerEntity, new CombatComponent(damage: 20f, range: 100f, cooldown: 0.5f));
        
        // Collision
        World.AddComponent(playerEntity, new CollisionComponent(28, 60, layer: CollisionLayer.Player));
        
        // Inventory (40 slots)
        World.AddComponent(playerEntity, new InventoryComponent(40));
        
        // Currency for trading
        World.AddComponent(playerEntity, new CurrencyComponent(50)); // Start with 50 gold
        
        // Quest tracking
        World.AddComponent(playerEntity, new QuestComponent());
        
        // Relationships with NPCs
        World.AddComponent(playerEntity, new RelationshipComponent());
        
        // Mining tool
        World.AddComponent(playerEntity, new ToolComponent(ToolType.Pickaxe, ToolMaterial.Wood));
        
        // Player light source (lantern)
        World.AddComponent(playerEntity, new LightSourceComponent(
            radius: 8.0f,
            intensity: 1.0f,
            type: LightSourceType.Player
        ));
        
        // Swimming capability
        World.AddComponent(playerEntity, new SwimmingComponent());
        
        // Respawn and death handling
        World.AddComponent(playerEntity, new RespawnComponent(
            respawnX: 500, 
            respawnY: 150, 
            deathPenaltyPercent: 10f, 
            invulnerabilityDuration: 2f
        ));
        
        // Experience and leveling
        World.AddComponent(playerEntity, new ExperienceComponent());
        
        Console.WriteLine("  ✓ Player created with full capabilities");
        Console.WriteLine("  ✓ Health: 100");
        Console.WriteLine("  ✓ Attack damage: 20");
        Console.WriteLine("  ✓ Inventory slots: 40");
        Console.WriteLine("  ✓ Starting gold: 50");
        Console.WriteLine("  ✓ Starting level: 1");
    }
    
    private void CreateCamera()
    {
        Console.WriteLine("[GameLoop] Creating camera...");
        
        cameraEntity = World.CreateEntity();
        
        var cameraComponent = new CameraComponent(1920, 1080)
        {
            Zoom = 1.0f,
            FollowSpeed = 8.0f
        };
        
        World.AddComponent(cameraEntity, cameraComponent);
        World.AddComponent(cameraEntity, new PositionComponent(500, 150));
        World.AddComponent(cameraEntity, new ScreenShakeComponent());
        
        CameraSystem.SetFollowTarget(World, cameraEntity, playerEntity, followSpeed: 8.0f);
        
        // Enable look-ahead
        CameraLookAheadSystem.EnableLookAhead(World, cameraEntity,
            lookAheadDistance: 120.0f,
            lookAheadSpeed: 4.0f,
            offsetScale: 0.2f);
        
        Console.WriteLine("  ✓ Camera configured with smooth following and look-ahead");
    }
    
    private void GenerateInitialTerrain()
    {
        Console.WriteLine("[GameLoop] Generating initial terrain...");
        
        var playerPos = World.GetComponent<PositionComponent>(playerEntity);
        if (playerPos != null && chunkManager != null)
        {
            chunkManager.UpdateChunks(playerPos.X);
            int chunkCount = chunkManager.GetLoadedChunkCount();
            Console.WriteLine($"  ✓ Generated {chunkCount} initial chunks");
            Console.WriteLine($"  ✓ Player spawned in procedurally generated world");
        }
    }
    
    private void GenerateWorldStructures()
    {
        Console.WriteLine("[GameLoop] Generating world structures...");
        
        if (structureGenerator == null || chunkManager == null) return;
        
        // Generate a village near spawn (east of player start)
        structureGenerator.GenerateVillage(chunkManager, 700, 5);
        
        // Generate a dungeon underground
        structureGenerator.GenerateDungeon(chunkManager, 400, 15);
        
        Console.WriteLine("  ✓ Village generated east of spawn");
        Console.WriteLine("  ✓ Underground dungeon generated");
    }
    
    private void CreateParallaxLayers()
    {
        Console.WriteLine("[GameLoop] Creating parallax background layers...");
        
        // Sky layer (static)
        ParallaxSystem.CreateParallaxLayer(World, "Sky",
            parallaxFactor: 0.0f,
            zOrder: -150,
            visualType: ParallaxVisualType.Sky,
            color: ConsoleColor.DarkBlue,
            density: 0.5f);
        
        // Clouds (auto-scrolling)
        ParallaxSystem.CreateParallaxLayer(World, "Clouds",
            parallaxFactor: 0.15f,
            zOrder: -100,
            visualType: ParallaxVisualType.Clouds,
            color: ConsoleColor.Gray,
            density: 0.25f,
            autoScrollX: 3.0f);
        
        // Mountains
        ParallaxSystem.CreateParallaxLayer(World, "Mountains",
            parallaxFactor: 0.3f,
            zOrder: -75,
            visualType: ParallaxVisualType.Mountains,
            color: ConsoleColor.DarkCyan,
            density: 0.5f);
        
        // Mist
        ParallaxSystem.CreateParallaxLayer(World, "Mist",
            parallaxFactor: 0.5f,
            zOrder: -25,
            visualType: ParallaxVisualType.Mist,
            color: ConsoleColor.DarkGray,
            density: 0.1f,
            autoScrollX: 1.5f);
        
        Console.WriteLine("  ✓ 4 parallax layers created for depth effect");
    }
    
    private void SpawnInitialCreatures()
    {
        Console.WriteLine("[GameLoop] Spawning initial creatures...");
        
        // Spawn some enemies at various positions
        SpawnGoblin(600, 150);
        SpawnGoblin(800, 150);
        SpawnGoblin(400, 150);
        SpawnGoblin(900, 150);
        SpawnGoblin(300, 150);
        
        Console.WriteLine("  ✓ 5 goblin enemies spawned");
    }
    
    private void SpawnGoblin(float x, float y)
    {
        var goblin = World.CreateEntity();
        
        World.AddComponent(goblin, new CreatureComponent(
            type: CreatureType.Goblin,
            name: "Goblin",
            isHostile: true,
            agroRange: 200f,
            xpValue: 10
        ));
        
        World.AddComponent(goblin, new PositionComponent(x, y));
        World.AddComponent(goblin, new VelocityComponent());
        World.AddComponent(goblin, new SpriteComponent(1, 32, 32));
        World.AddComponent(goblin, new HealthComponent(30));
        World.AddComponent(goblin, new CollisionComponent(28, 28, layer: CollisionLayer.Enemy));
        World.AddComponent(goblin, new CombatComponent(damage: 10f, range: 50f, cooldown: 1.5f));
        
        // Add loot drops
        World.AddComponent(goblin, LootDropComponent.CreateGoblinLoot());
        
        // Add Lua AI script
        World.AddComponent(goblin, new ScriptComponent("scripts/lua/goblin_ai.lua"));
    }
    
    private void CreateBossEncounter()
    {
        Console.WriteLine("[GameLoop] Creating boss encounter...");
        
        // Create the Forest Guardian boss east of the player start
        float bossX = 1500f;
        float bossY = 150f;
        float arenaX = 1300f;
        float arenaY = 50f;
        float arenaWidth = 400f;
        float arenaHeight = 200f;
        
        var bossEntity = BossSystem.CreateBoss(
            World,
            BossType.ForestGuardian,
            "Ancient Forest Guardian",
            bossX, bossY,
            arenaX, arenaY, arenaWidth, arenaHeight
        );
        
        // Configure boss rewards
        var boss = World.GetComponent<BossComponent>(bossEntity);
        if (boss != null)
        {
            boss.GoldReward = 500;
            boss.ExperienceReward = 200;
            boss.ItemDrops[TileType.GoldOre] = 5;
            boss.ItemDrops[TileType.IronOre] = 10;
            boss.AbilityReward = AbilityType.SwordSpin;
            boss.UnlockArea = "Deep Forest";
        }
        
        // Override health for this specific boss
        var bossHealth = World.GetComponent<HealthComponent>(bossEntity);
        if (bossHealth != null)
        {
            bossHealth.MaxHealth = 300;
            bossHealth.CurrentHealth = 300;
        }
        
        // Add sprite for boss
        World.AddComponent(bossEntity, new SpriteComponent(4, 64, 64));
        
        Console.WriteLine("  ✓ Forest Guardian boss created at eastern forest");
        Console.WriteLine($"  ✓ Boss arena: ({arenaX}, {arenaY}) {arenaWidth}x{arenaHeight}");
        Console.WriteLine("  ✓ Rewards: 500 gold, 200 XP, Sword Spin ability");
    }
    
    private void CreateNPCsAndQuests()
    {
        Console.WriteLine("[GameLoop] Creating NPCs and quests...");
        
        // Create a merchant NPC
        var merchant = World.CreateEntity();
        var merchantNPC = new NPCComponent("Trader Grimm", NPCRole.Merchant);
        merchantNPC.AddGreeting("Greetings, traveler! Care to do some trading?");
        merchantNPC.AddDialogue("I have many wares if you have coin.");
        merchantNPC.AddDialogue("The goblins have been troublesome lately...");
        
        World.AddComponent(merchant, merchantNPC);
        World.AddComponent(merchant, new PositionComponent(1000, 150));
        World.AddComponent(merchant, new SpriteComponent(2, 32, 64));
        World.AddComponent(merchant, new CollisionComponent(28, 60, layer: CollisionLayer.Enemy)); // Use Enemy as NPC layer
        
        // Create a quest giver
        var questGiver = World.CreateEntity();
        var questNPC = new NPCComponent("Elder Sage", NPCRole.Questgiver); // Note: lowercase 'g' in Questgiver
        questNPC.AddGreeting("Welcome, brave adventurer!");
        questNPC.AddDialogue("The land needs heroes like you.");
        
        World.AddComponent(questGiver, questNPC);
        World.AddComponent(questGiver, new PositionComponent(200, 150));
        World.AddComponent(questGiver, new SpriteComponent(3, 32, 64));
        World.AddComponent(questGiver, new CollisionComponent(28, 60, layer: CollisionLayer.Enemy)); // Use Enemy as NPC layer
        
        // Give player an initial quest
        var playerQuests = World.GetComponent<QuestComponent>(playerEntity);
        if (playerQuests != null)
        {
            var quest = new Quest(
                id: "goblin_threat",
                name: "Goblin Threat",
                description: "Defeat 5 goblins terrorizing the area",
                type: QuestType.Combat
            )
            {
                RequiredProgress = 5,
                GoldReward = 100,
                ExperienceReward = 50
            };
            playerQuests.AcceptQuest(quest);
        }
        
        Console.WriteLine("  ✓ 2 NPCs created (Merchant and Quest Giver)");
        Console.WriteLine("  ✓ Initial quest assigned: Defeat 5 goblins");
    }
    
    private void GiveStartingItems()
    {
        Console.WriteLine("[GameLoop] Giving player starting items...");
        
        var inventory = World.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null)
        {
            // Give some basic resources (using TileType enum)
            inventory.AddItem(TileType.Wood, 20);
            inventory.AddItem(TileType.Stone, 15);
            inventory.AddItem(TileType.CoalOre, 10);
            inventory.AddItem(TileType.CopperOre, 5);
            
            // Give a few iron for tools
            inventory.AddItem(TileType.IronOre, 3);
            
            Console.WriteLine("  ✓ Starting items added to inventory:");
            Console.WriteLine("    - Wood x20");
            Console.WriteLine("    - Stone x15");
            Console.WriteLine("    - Coal Ore x10");
            Console.WriteLine("    - Copper Ore x5");
            Console.WriteLine("    - Iron Ore x3");
        }
    }
    
    private void CreateUIOverlays()
    {
        Console.WriteLine("[GameLoop] Creating UI overlays...");
        
        // Note: UI creation is simplified here - in a full game, these would be
        // created and managed by the UI system with proper keyboard input handling
        
        Console.WriteLine("  ✓ UI system active:");
        Console.WriteLine("    - Press 'I' for inventory (when implemented with UI)");
        Console.WriteLine("    - Press 'C' for crafting (when implemented with UI)");
        Console.WriteLine("    - Press 'ESC' to close UI");
    }
    
    private void DisplayGameInfo()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                     GAME SYSTEMS ACTIVE                          ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  WORLD                                                           ║");
        Console.WriteLine("║  • Procedural terrain with 8 biomes                              ║");
        Console.WriteLine("║  • 20-layer underground system                                   ║");
        Console.WriteLine("║  • Day/night cycle (accelerated 60x)                             ║");
        Console.WriteLine("║  • Dynamic weather system                                        ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  GAMEPLAY                                                        ║");
        Console.WriteLine("║  • Mining and building system                                    ║");
        Console.WriteLine("║  • Crafting with 8+ recipes                                      ║");
        Console.WriteLine("║  • Combat with enemies                                           ║");
        Console.WriteLine("║  • Quest system with objectives                                  ║");
        Console.WriteLine("║  • NPC interaction and trading                                   ║");
        Console.WriteLine("║  • Swimming mechanics                                            ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  VISUALS                                                         ║");
        Console.WriteLine("║  • Smooth camera following with look-ahead                       ║");
        Console.WriteLine("║  • Parallax scrolling backgrounds                                ║");
        Console.WriteLine("║  • Screen shake effects                                          ║");
        Console.WriteLine("║  • Dynamic lighting (surface and underground)                    ║");
        Console.WriteLine("║  • Fog of war for unexplored areas                               ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║                        CONTROLS                                  ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  MOVEMENT                                                        ║");
        Console.WriteLine("║  • WASD or Arrow Keys - Move player                              ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  COMBAT                                                          ║");
        Console.WriteLine("║  • SPACE - Attack nearby enemies                                 ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  MINING & BUILDING                                               ║");
        Console.WriteLine("║  • Left Click - Mine blocks                                      ║");
        Console.WriteLine("║  • Right Click - Place blocks                                    ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  CAMERA                                                          ║");
        Console.WriteLine("║  • +/- Keys - Zoom in/out                                        ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  UI                                                              ║");
        Console.WriteLine("║  • I Key - Toggle inventory                                      ║");
        Console.WriteLine("║  • C Key - Toggle crafting                                       ║");
        Console.WriteLine("║  • ESC - Close UI/Exit                                           ║");
        Console.WriteLine("║                                                                  ║");
        Console.WriteLine("║  SAVE/LOAD                                                       ║");
        Console.WriteLine("║  • F5 - Quick Save                                               ║");
        Console.WriteLine("║  • F9 - Quick Load                                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");
        
        Console.WriteLine("Press Q or ESC to exit the demo\n");
    }
    
    public override void Update(float deltaTime)
    {
        gameTime += deltaTime;
        
        // Handle save/load keyboard shortcuts
        HandleSaveLoadInput();
        
        // Update time system
        if (timeSystem != null)
        {
            timeSystem.Update(deltaTime);
            
            // Update weather (simplified - no biome check since GetBiomeAt is private)
            if (weatherSystem != null)
            {
                // Default to Plains biome for weather updates
                weatherSystem.Update(BiomeType.Plains, deltaTime);
            }
        }
        
        // Update all ECS systems
        base.Update(deltaTime);
        
        // Update chunk loading based on player position
        if (chunkManager != null)
        {
            var playerPos = World.GetComponent<PositionComponent>(playerEntity);
            if (playerPos != null)
            {
                chunkManager.UpdateChunks(playerPos.X);
            }
        }
        
        // Track stats for demo purposes (every 5 seconds)
        if (gameTime % 5f < deltaTime)
        {
            DisplayStats();
        }
    }
    
    private void HandleSaveLoadInput()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            
            // F5 - Quick Save
            if (key.Key == ConsoleKey.F5)
            {
                if (saveSystem != null)
                {
                    Console.WriteLine("\n[Save] Quick saving...");
                    if (saveSystem.QuickSave(gameTime))
                    {
                        Console.WriteLine("[Save] Game saved successfully!");
                    }
                    else
                    {
                        Console.WriteLine("[Save] Failed to save game.");
                    }
                }
            }
            // F9 - Quick Load
            else if (key.Key == ConsoleKey.F9)
            {
                if (saveSystem != null)
                {
                    Console.WriteLine("\n[Load] Quick loading...");
                    if (saveSystem.QuickLoad(out float loadedGameTime))
                    {
                        gameTime = loadedGameTime;
                        Console.WriteLine("[Load] Game loaded successfully!");
                        
                        // Update camera to follow player's new position
                        if (cameraEntity.Id != 0)
                        {
                            var playerPos = World.GetComponent<PositionComponent>(playerEntity);
                            var cameraPos = World.GetComponent<PositionComponent>(cameraEntity);
                            if (playerPos != null && cameraPos != null)
                            {
                                cameraPos.X = playerPos.X;
                                cameraPos.Y = playerPos.Y;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Load] Failed to load game. No save found or save is corrupted.");
                    }
                }
            }
        }
    }
    
    private void DisplayStats()
    {
        if (timeSystem == null) return;
        
        var playerHealth = World.GetComponent<HealthComponent>(playerEntity);
        var playerInventory = World.GetComponent<InventoryComponent>(playerEntity);
        var playerQuests = World.GetComponent<QuestComponent>(playerEntity);
        var playerXP = World.GetComponent<ExperienceComponent>(playerEntity);
        
        Console.WriteLine("\n--- Game Stats ---");
        Console.WriteLine($"Game Time: {timeSystem.CurrentHour:D2}:{timeSystem.CurrentMinute:D2} - {timeSystem.CurrentPhase}");
        
        if (weatherSystem != null)
        {
            Console.WriteLine($"Weather: {weatherSystem.CurrentWeather} ({weatherSystem.CurrentIntensity})");
        }
        
        if (playerHealth != null)
        {
            Console.WriteLine($"Health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
        }
        
        if (playerXP != null)
        {
            Console.WriteLine($"Level: {playerXP.Level} ({playerXP.CurrentXP}/{playerXP.GetXPForNextLevel()} XP)");
        }
        
        if (playerInventory != null)
        {
            int usedSlots = playerInventory.GetAllItems().Count;
            Console.WriteLine($"Inventory: {usedSlots}/40 slots used");
        }
        
        if (playerQuests != null)
        {
            var activeQuests = playerQuests.ActiveQuests;
            Console.WriteLine($"Active Quests: {activeQuests.Count}");
            foreach (var quest in activeQuests)
            {
                Console.WriteLine($"  • {quest.Name}: {quest.CurrentProgress}/{quest.RequiredProgress}");
            }
        }
        
        if (chunkManager != null)
        {
            Console.WriteLine($"Loaded Chunks: {chunkManager.GetLoadedChunkCount()}");
        }
        
        Console.WriteLine("------------------\n");
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("\n[GameLoop] Unloading complete game loop demo...");
        Console.WriteLine($"Total game time: {gameTime:F1} seconds");
        Console.WriteLine($"Enemies defeated: {enemiesDefeated}");
        Console.WriteLine($"Resources gathered: {resourcesGathered}");
        Console.WriteLine($"Items crafted: {itemsCrafted}");
        Console.WriteLine("\nThank you for playing the demo!");
    }
}
