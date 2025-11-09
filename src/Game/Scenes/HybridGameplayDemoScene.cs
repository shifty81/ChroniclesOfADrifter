using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.World;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the hybrid action-adventure and life simulation gameplay
/// </summary>
public class HybridGameplayDemoScene : Scene
{
    private Entity playerEntity;
    private Entity merchantNPC;
    private Entity questgiverNPC;
    private Entity boss;
    private List<Entity> farmPlots;
    
    public override void OnLoad()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  HYBRID GAMEPLAY DEMO - Action Adventure + Life Sim     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
        
        farmPlots = new List<Entity>();
        
        // Initialize core systems
        InitializeSystems();
        
        // Create player with full capabilities
        CreatePlayer();
        
        // Create NPCs
        CreateNPCs();
        
        // Create farm plots
        CreateFarmPlots();
        
        // Create a boss encounter
        CreateBossEncounter();
        
        // Give player starting quests
        GiveStartingQuests();
        
        // Demonstrate the game loop
        RunGameplayDemo();
    }
    
    private void InitializeSystems()
    {
        // Add gameplay systems
        World.AddSystem(new QuestSystem());
        World.AddSystem(new NPCSystem());
        World.AddSystem(new FarmingSystem());
        World.AddSystem(new BossSystem());
        
        Console.WriteLine("[Demo] All systems initialized\n");
    }
    
    private void CreatePlayer()
    {
        playerEntity = World.CreateEntity();
        
        // Basic components
        World.AddComponent(playerEntity, new PlayerComponent());
        World.AddComponent(playerEntity, new PositionComponent(100, 100));
        World.AddComponent(playerEntity, new HealthComponent(100, 100));
        World.AddComponent(playerEntity, new CollisionComponent(32, 64, CollisionLayer.Player));
        
        // Combat capabilities
        World.AddComponent(playerEntity, new CombatComponent
        {
            AttackDamage = 20,
            AttackRange = 50f,
            AttackCooldown = 1.0f
        });
        
        // Inventory and economy
        World.AddComponent(playerEntity, new InventoryComponent(40));
        World.AddComponent(playerEntity, new CurrencyComponent(100)); // Start with 100 gold
        
        // Quest tracking
        World.AddComponent(playerEntity, new QuestComponent());
        
        // Relationship tracking
        World.AddComponent(playerEntity, new RelationshipComponent());
        
        // Abilities
        var abilities = new AbilityComponent(100);
        World.AddComponent(playerEntity, abilities);
        
        Console.WriteLine("[Demo] Player created with full capabilities\n");
    }
    
    private void CreateNPCs()
    {
        // Create a merchant
        merchantNPC = World.CreateEntity();
        var merchant = new NPCComponent("Merchant Tom", NPCRole.Merchant);
        merchant.AddGreeting("Welcome to my shop!");
        merchant.AddDialogue("I've got the finest goods in town.");
        
        // Add shop inventory
        merchant.ShopInventory[TileType.Wood] = 50;
        merchant.ShopPrices[TileType.Wood] = 10;
        merchant.ShopInventory[TileType.Stone] = 30;
        merchant.ShopPrices[TileType.Stone] = 15;
        
        // Add schedule
        merchant.AddSchedule(8.0f, 18.0f, 120, 100, "shopkeeping");
        merchant.AddSchedule(18.0f, 22.0f, 150, 150, "resting");
        
        World.AddComponent(merchantNPC, merchant);
        World.AddComponent(merchantNPC, new PositionComponent(120, 100));
        
        // Create a questgiver
        questgiverNPC = World.CreateEntity();
        var questgiver = new NPCComponent("Elder Sarah", NPCRole.Questgiver);
        questgiver.AddGreeting("Greetings, adventurer!");
        questgiver.AddDialogue("The village needs your help.");
        
        World.AddComponent(questgiverNPC, questgiver);
        World.AddComponent(questgiverNPC, new PositionComponent(100, 120));
        
        Console.WriteLine("[Demo] NPCs created: Merchant Tom and Elder Sarah\n");
    }
    
    private void CreateFarmPlots()
    {
        // Create 3 farm plots
        for (int i = 0; i < 3; i++)
        {
            var plotEntity = World.CreateEntity();
            var plot = new FarmPlotComponent(50 + i * 10, 50);
            plot.Till(); // Pre-till for demo
            World.AddComponent(plotEntity, plot);
            farmPlots.Add(plotEntity);
        }
        
        Console.WriteLine("[Demo] Created 3 farm plots\n");
    }
    
    private void CreateBossEncounter()
    {
        boss = BossSystem.CreateBoss(
            World,
            BossType.ForestGuardian,
            "Ancient Forest Guardian",
            200, 200,
            150, 150, 100, 100 // Arena bounds
        );
        
        // Configure boss rewards
        var bossComponent = World.GetComponent<BossComponent>(boss);
        if (bossComponent != null)
        {
            bossComponent.GoldReward = 500;
            bossComponent.ItemDrops[TileType.IronOre] = 10;
            bossComponent.AbilityReward = AbilityType.Hookshot;
            bossComponent.UnlockArea = "ancient_forest";
        }
        
        Console.WriteLine("[Demo] Boss encounter created: Ancient Forest Guardian\n");
    }
    
    private void GiveStartingQuests()
    {
        var questComponent = World.GetComponent<QuestComponent>(playerEntity);
        if (questComponent == null) return;
        
        // Combat quest
        var combatQuest = new Quest(
            "defeat_goblins",
            "Goblin Threat",
            "Defeat 5 goblins terrorizing the village",
            QuestType.Combat
        )
        {
            RequiredProgress = 5,
            GoldReward = 100,
            ExperienceReward = 50
        };
        questComponent.AcceptQuest(combatQuest);
        
        // Farming quest
        var farmQuest = new Quest(
            "grow_wheat",
            "First Harvest",
            "Plant and harvest 3 wheat crops",
            QuestType.Farming
        )
        {
            RequiredProgress = 3,
            GoldReward = 50
        };
        farmQuest.ItemRewards[TileType.Gold] = 1;
        questComponent.AcceptQuest(farmQuest);
        
        // Social quest
        var socialQuest = new Quest(
            "meet_villagers",
            "New in Town",
            "Introduce yourself to 3 villagers",
            QuestType.Social
        )
        {
            RequiredProgress = 3,
            GoldReward = 30
        };
        questComponent.AcceptQuest(socialQuest);
        
        Console.WriteLine("[Demo] Starting quests given to player\n");
    }
    
    private void RunGameplayDemo()
    {
        Console.WriteLine("═══════════════ GAMEPLAY DEMONSTRATION ═══════════════\n");
        
        // 1. Show quest log
        Console.WriteLine("1. QUEST LOG:");
        var questComponent = World.GetComponent<QuestComponent>(playerEntity);
        if (questComponent != null)
        {
            foreach (var quest in questComponent.ActiveQuests)
            {
                Console.WriteLine($"   [{quest.Type}] {quest.Name}");
                Console.WriteLine($"   {quest.Description}");
                Console.WriteLine($"   Progress: {quest.CurrentProgress}/{quest.RequiredProgress}\n");
            }
        }
        
        // 2. Demonstrate farming
        Console.WriteLine("\n2. FARMING DEMONSTRATION:");
        var inventory = World.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null && farmPlots.Count > 0)
        {
            // Give player seeds
            inventory.AddItem(TileType.Grass, 3); // Placeholder for wheat seeds
            
            // Plant crops
            for (int i = 0; i < farmPlots.Count; i++)
            {
                FarmingSystem.PlantCrop(World, 50 + i * 10, 50, FarmingSystem.CropTypes.Wheat, playerEntity);
            }
            
            Console.WriteLine("   Crops planted!");
            
            // Water crops
            for (int i = 0; i < farmPlots.Count; i++)
            {
                FarmingSystem.WaterPlot(World, 50 + i * 10, 50);
            }
            Console.WriteLine("   Crops watered!\n");
        }
        
        // 3. Demonstrate NPC interaction
        Console.WriteLine("3. NPC INTERACTION:");
        var merchant = World.GetComponent<NPCComponent>(merchantNPC);
        if (merchant != null)
        {
            Console.WriteLine($"   Player approaches {merchant.Name}");
            Console.WriteLine($"   {merchant.Name}: \"{merchant.GetGreeting()}\"");
            
            // Build relationship
            var relationships = World.GetComponent<RelationshipComponent>(playerEntity);
            if (relationships != null)
            {
                relationships.Interact(merchant.Name, 10);
                Console.WriteLine($"   Relationship with {merchant.Name}: {relationships.GetLevel(merchant.Name)}\n");
            }
        }
        
        // 4. Demonstrate shopping
        Console.WriteLine("4. ECONOMIC SYSTEM:");
        var currency = World.GetComponent<CurrencyComponent>(playerEntity);
        if (currency != null && merchant != null)
        {
            Console.WriteLine($"   Player gold: {currency.Gold}");
            bool purchased = NPCSystem.BuyFromMerchant(World, playerEntity, merchant, TileType.Wood);
            if (purchased)
            {
                Console.WriteLine($"   Remaining gold: {currency.Gold}\n");
            }
        }
        
        // 5. Demonstrate ability system
        Console.WriteLine("5. ABILITY SYSTEM:");
        var abilities = World.GetComponent<AbilityComponent>(playerEntity);
        if (abilities != null)
        {
            // Unlock an ability
            abilities.UnlockAbility(AbilityType.Dash);
            Console.WriteLine("   Unlocked: Dash ability");
            
            var unlockedAbilities = abilities.GetUnlockedAbilities().ToList();
            Console.WriteLine($"   Total abilities unlocked: {unlockedAbilities.Count}");
            
            var accessibleAreas = abilities.GetAccessibleAreas();
            Console.WriteLine($"   Accessible areas: {accessibleAreas.Count}\n");
        }
        
        // 6. Demonstrate boss encounter
        Console.WriteLine("6. BOSS ENCOUNTER:");
        var bossComponent = World.GetComponent<BossComponent>(boss);
        var bossHealth = World.GetComponent<HealthComponent>(boss);
        if (bossComponent != null && bossHealth != null)
        {
            Console.WriteLine($"   Encountered: {bossComponent.BossName}");
            Console.WriteLine($"   Boss Health: {bossHealth.CurrentHealth}/{bossHealth.MaxHealth}");
            Console.WriteLine($"   Rewards: {bossComponent.GoldReward} gold");
            if (bossComponent.AbilityReward.HasValue)
            {
                Console.WriteLine($"   Ability Reward: {bossComponent.AbilityReward.Value}");
            }
            Console.WriteLine();
        }
        
        // 7. Show hybrid integration
        Console.WriteLine("7. HYBRID GAMEPLAY INTEGRATION:");
        Console.WriteLine("   ✓ Combat earns gold for farm upgrades");
        Console.WriteLine("   ✓ Farming provides resources to sell");
        Console.WriteLine("   ✓ NPC relationships unlock quests");
        Console.WriteLine("   ✓ Quests grant abilities for exploration");
        Console.WriteLine("   ✓ Boss defeats unlock new areas");
        Console.WriteLine("   ✓ Abilities gate dungeon access\n");
        
        Console.WriteLine("═══════════════════════════════════════════════════════\n");
    }
    
    public override void Update(float deltaTime)
    {
        World.Update(deltaTime);
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[Demo] Hybrid gameplay demo completed\n");
    }
}
