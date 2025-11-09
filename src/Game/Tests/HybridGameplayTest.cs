using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the hybrid gameplay systems
/// </summary>
public class HybridGameplayTest
{
    public static void Run()
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║     HYBRID GAMEPLAY SYSTEMS TEST                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝\n");
        
        TestQuestSystem();
        TestRelationshipSystem();
        TestFarmingSystem();
        TestAbilitySystem();
        TestEconomicIntegration();
        
        Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║     ALL TESTS PASSED ✓                             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝\n");
    }
    
    private static void TestQuestSystem()
    {
        Console.WriteLine("═══ Testing Quest System ═══");
        
        var world = new World();
        var player = world.CreateEntity();
        var quests = new QuestComponent();
        world.AddComponent(player, quests);
        
        // Test quest creation and acceptance
        var quest = new Quest("test_quest", "Test Quest", "A test quest", QuestType.Combat)
        {
            RequiredProgress = 5,
            GoldReward = 100
        };
        
        Assert(quests.AcceptQuest(quest), "Should accept quest");
        Assert(quests.ActiveQuests.Count == 1, "Should have 1 active quest");
        
        // Test quest progress
        quest.UpdateProgress(3);
        Assert(quest.CurrentProgress == 3, "Progress should be 3");
        Assert(!quest.IsComplete(), "Quest should not be complete");
        
        quest.UpdateProgress(2);
        Assert(quest.IsComplete(), "Quest should be complete");
        Assert(quest.Status == QuestStatus.Completed, "Status should be Completed");
        
        // Test quest completion
        var completedQuest = quests.CompleteQuest("test_quest");
        Assert(completedQuest != null, "Should return completed quest");
        Assert(quests.ActiveQuests.Count == 0, "Should have no active quests");
        Assert(quests.CompletedQuests.Count == 1, "Should have 1 completed quest");
        
        Console.WriteLine("✓ Quest system tests passed\n");
    }
    
    private static void TestRelationshipSystem()
    {
        Console.WriteLine("═══ Testing Relationship System ═══");
        
        var world = new World();
        var player = world.CreateEntity();
        var relationships = new RelationshipComponent();
        world.AddComponent(player, relationships);
        
        // Test relationship creation
        var relationship = relationships.GetRelationship("TestNPC");
        Assert(relationship != null, "Should create relationship");
        Assert(relationship!.Level == RelationshipLevel.Stranger, "Should start as Stranger");
        
        // Test relationship progression
        relationships.Interact("TestNPC", 50);
        relationship = relationships.GetRelationship("TestNPC");
        Assert(relationship.Points == 50, "Should have 50 points");
        
        relationships.Interact("TestNPC", 60);
        Assert(relationship.Points == 110, "Should have 110 points");
        Assert(relationship.Level == RelationshipLevel.Acquaintance, "Should be Acquaintance");
        
        // Test gift giving
        bool giftGiven = relationships.GiveGift("TestNPC", TileType.GoldOre, 50);
        Assert(giftGiven, "Should accept gift");
        Assert(relationship.Points == 160, "Should have 160 points");
        Assert(relationship.GiftsGivenToday == 1, "Should have given 1 gift today");
        
        Console.WriteLine("✓ Relationship system tests passed\n");
    }
    
    private static void TestFarmingSystem()
    {
        Console.WriteLine("═══ Testing Farming System ═══");
        
        var world = new World();
        var plotEntity = world.CreateEntity();
        var plot = new FarmPlotComponent(10, 10);
        world.AddComponent(plotEntity, plot);
        
        // Test tilling
        plot.Till();
        Assert(plot.IsTilled, "Plot should be tilled");
        
        // Test planting
        var cropType = FarmingSystem.CropTypes.Wheat;
        bool planted = plot.PlantCrop(cropType);
        Assert(planted, "Should plant crop");
        Assert(plot.CurrentCrop != null, "Should have a crop");
        Assert(plot.CurrentCrop!.Stage == CropStage.Seed, "Should start as seed");
        
        // Test watering
        plot.Water();
        Assert(plot.CurrentCrop!.IsWatered, "Crop should be watered");
        
        // Test growth
        plot.AdvanceDay();
        Assert(plot.CurrentCrop.DaysGrowing == 1, "Should have grown 1 day");
        
        // Advance to harvest
        for (int i = 0; i < cropType.GrowthDays - 1; i++)
        {
            plot.Water();
            plot.AdvanceDay();
        }
        
        Assert(plot.CurrentCrop.IsHarvestable(), "Crop should be harvestable");
        
        // Test harvest
        var (success, item, quantity) = plot.HarvestCrop();
        Assert(success, "Should harvest successfully");
        Assert(quantity >= cropType.MinYield && quantity <= cropType.MaxYield, "Yield should be in range");
        Assert(plot.CurrentCrop == null, "Plot should be empty after harvest");
        
        Console.WriteLine("✓ Farming system tests passed\n");
    }
    
    private static void TestAbilitySystem()
    {
        Console.WriteLine("═══ Testing Ability System ═══");
        
        var world = new World();
        var player = world.CreateEntity();
        var abilities = new AbilityComponent(100);
        world.AddComponent(player, abilities);
        
        // Test ability unlocking
        bool unlocked = abilities.UnlockAbility(AbilityType.Dash);
        Assert(unlocked, "Should unlock Dash");
        Assert(abilities.HasAbility(AbilityType.Dash), "Should have Dash");
        
        // Test ability usage
        bool used = abilities.UseAbility(AbilityType.Dash, 0f);
        Assert(used, "Should use ability");
        Assert(abilities.CurrentEnergy < 100, "Energy should be consumed");
        
        // Test cooldown
        bool usedAgain = abilities.UseAbility(AbilityType.Dash, 0f);
        Assert(!usedAgain, "Should not use ability on cooldown");
        
        // Test area unlocking
        abilities.UnlockAbility(AbilityType.Hookshot);
        var areas = abilities.GetAccessibleAreas();
        Assert(areas.Count > 0, "Should have accessible areas");
        Assert(areas.Contains("canyon_area"), "Should access canyon area");
        
        Console.WriteLine("✓ Ability system tests passed\n");
    }
    
    private static void TestEconomicIntegration()
    {
        Console.WriteLine("═══ Testing Economic Integration ═══");
        
        var world = new World();
        var player = world.CreateEntity();
        
        var currency = new CurrencyComponent(100);
        var inventory = new InventoryComponent(40);
        world.AddComponent(player, currency);
        world.AddComponent(player, inventory);
        
        // Test currency operations
        currency.AddGold(50);
        Assert(currency.Gold == 150, "Should have 150 gold");
        
        bool removed = currency.RemoveGold(30);
        Assert(removed, "Should remove gold");
        Assert(currency.Gold == 120, "Should have 120 gold");
        
        // Test insufficient funds
        bool removedTooMuch = currency.RemoveGold(200);
        Assert(!removedTooMuch, "Should not remove more gold than available");
        
        // Test merchant interaction
        var merchantNPC = world.CreateEntity();
        var merchant = new NPCComponent("Test Merchant", NPCRole.Merchant);
        merchant.ShopInventory[TileType.Wood] = 10;
        merchant.ShopPrices[TileType.Wood] = 20;
        world.AddComponent(merchantNPC, merchant);
        
        bool purchased = NPCSystem.BuyFromMerchant(world, player, merchant, TileType.Wood);
        Assert(purchased, "Should purchase item");
        Assert(currency.Gold == 100, "Should have spent 20 gold");
        Assert(inventory.HasItem(TileType.Wood, 1), "Should have item in inventory");
        Assert(merchant.ShopInventory[TileType.Wood] == 9, "Merchant stock should decrease");
        
        // Test selling
        bool sold = NPCSystem.SellToMerchant(world, player, TileType.Wood, 15);
        Assert(sold, "Should sell item");
        Assert(currency.Gold == 115, "Should have received 15 gold");
        Assert(!inventory.HasItem(TileType.Wood, 1), "Should not have item anymore");
        
        Console.WriteLine("✓ Economic integration tests passed\n");
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Test failed: {message}");
        }
    }
}
