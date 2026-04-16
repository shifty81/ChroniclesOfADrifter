using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Relationship system
/// </summary>
public static class RelationshipSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Relationship System Test");
        Console.WriteLine("=======================================\n");

        TestRelationshipLevels();
        TestGiftMechanic();
        TestDailyGiftLimit();
        TestTradeReputation();
        TestDialogueTiers();
        TestDisplayRelationships();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Relationship System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static (World world, Entity player) SetupPlayer()
    {
        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(0f, 0f));
        world.AddComponent(player, new InventoryComponent(40));
        world.AddComponent(player, new CurrencyComponent(100));
        world.AddComponent(player, new RelationshipComponent());
        return (world, player);
    }

    private static void TestRelationshipLevels()
    {
        Console.WriteLine("[Test] Relationship Level Progression");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var rel = world.GetComponent<RelationshipComponent>(player)!;

        var r = rel.GetRelationship("TestNPC");

        System.Diagnostics.Debug.Assert(r.Level == RelationshipLevel.Stranger, "Should start as Stranger");
        r.AddPoints(100);
        System.Diagnostics.Debug.Assert(r.Level == RelationshipLevel.Acquaintance, "100 pts → Acquaintance");
        r.AddPoints(150);
        System.Diagnostics.Debug.Assert(r.Level == RelationshipLevel.Friend, "250 pts → Friend");
        r.AddPoints(250);
        System.Diagnostics.Debug.Assert(r.Level == RelationshipLevel.GoodFriend, "500 pts → GoodFriend");
        r.AddPoints(500);
        System.Diagnostics.Debug.Assert(r.Level == RelationshipLevel.BestFriend, "1000 pts → BestFriend");

        Console.WriteLine("  Stranger → Acquaintance → Friend → GoodFriend → BestFriend ✓");
        Console.WriteLine("✓ Relationship level progression working\n");
    }

    private static void TestGiftMechanic()
    {
        Console.WriteLine("[Test] Gift Mechanic");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var inventory = world.GetComponent<InventoryComponent>(player)!;
        inventory.AddItem(TileType.Gold, 5);

        bool success = RelationshipSystem.GiveGift(world, player, "Blacksmith", TileType.Gold, 50);

        System.Diagnostics.Debug.Assert(success, "Gift should succeed when item is in inventory");

        var rel = world.GetComponent<RelationshipComponent>(player)!;
        System.Diagnostics.Debug.Assert(rel.GetRelationship("Blacksmith").Points == 50,
            "Relationship should gain 50 pts from gift");
        System.Diagnostics.Debug.Assert(!inventory.HasItem(TileType.Gold, 5),
            "Item should be consumed from inventory");

        Console.WriteLine($"  Blacksmith relationship: {rel.GetRelationship("Blacksmith").Level} ({rel.GetRelationship("Blacksmith").Points} pts)");
        Console.WriteLine("✓ Gift mechanic working\n");
    }

    private static void TestDailyGiftLimit()
    {
        Console.WriteLine("[Test] Daily Gift Limit");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var inventory = world.GetComponent<InventoryComponent>(player)!;
        inventory.AddItem(TileType.Gold, 10);

        // Give 2 gifts (max per day)
        RelationshipSystem.GiveGift(world, player, "Merchant", TileType.Gold, 30);
        RelationshipSystem.GiveGift(world, player, "Merchant", TileType.Gold, 30);

        // 3rd gift should fail
        bool third = RelationshipSystem.GiveGift(world, player, "Merchant", TileType.Gold, 30);
        System.Diagnostics.Debug.Assert(!third, "3rd gift in same day should be rejected");

        Console.WriteLine("  Third gift correctly rejected (daily limit = 2).");
        Console.WriteLine("✓ Daily gift limit working\n");
    }

    private static void TestTradeReputation()
    {
        Console.WriteLine("[Test] Trade Reputation Modifier");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var rel = world.GetComponent<RelationshipComponent>(player)!;

        // Stranger → 0.0 reputation
        float stranger = RelationshipSystem.GetTradeReputation(world, player, "Trader");
        System.Diagnostics.Debug.Assert(Math.Abs(stranger) < 0.01f, "Stranger should have 0 reputation");

        // BestFriend → 1.0 reputation
        rel.GetRelationship("Trader").AddPoints(1000);
        float bestFriend = RelationshipSystem.GetTradeReputation(world, player, "Trader");
        System.Diagnostics.Debug.Assert(Math.Abs(bestFriend - 1.0f) < 0.01f,
            $"BestFriend should have 1.0 reputation but got {bestFriend}");

        Console.WriteLine($"  Stranger rep: {stranger:F2}, BestFriend rep: {bestFriend:F2}");
        Console.WriteLine("✓ Trade reputation modifier working\n");
    }

    private static void TestDialogueTiers()
    {
        Console.WriteLine("[Test] Dialogue Tier Unlocks");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();

        string strangerTier = RelationshipSystem.GetDialogueTier(world, player, "NPC1");
        Console.WriteLine($"  Stranger tier: {strangerTier}");
        System.Diagnostics.Debug.Assert(strangerTier.Contains("greeting", StringComparison.OrdinalIgnoreCase),
            "Stranger should only have greeting dialogue");

        var rel = world.GetComponent<RelationshipComponent>(player)!;
        rel.GetRelationship("NPC1").AddPoints(1000);

        string bestFriendTier = RelationshipSystem.GetDialogueTier(world, player, "NPC1");
        Console.WriteLine($"  BestFriend tier: {bestFriendTier}");
        System.Diagnostics.Debug.Assert(bestFriendTier.Contains("quest", StringComparison.OrdinalIgnoreCase),
            "BestFriend should unlock secret quest");

        Console.WriteLine("✓ Dialogue tiers working\n");
    }

    private static void TestDisplayRelationships()
    {
        Console.WriteLine("[Test] Display Relationships");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();

        // Talk to a few NPCs
        RelationshipSystem.TalkToNPC(world, player, "Guard", 10);
        RelationshipSystem.TalkToNPC(world, player, "Innkeeper", 15);

        RelationshipSystem.DisplayRelationships(world, player);

        var rel = world.GetComponent<RelationshipComponent>(player)!;
        System.Diagnostics.Debug.Assert(rel.GetAllRelationships().Count == 2, "Should have 2 NPC relationships");

        Console.WriteLine("✓ Display relationships working\n");
    }
}
