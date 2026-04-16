using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Enhanced Quest Tracker (QuestTrackerSystem)
/// </summary>
public static class EnhancedQuestTrackerTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Enhanced Quest Tracker Test");
        Console.WriteLine("=======================================\n");

        TestStarterQuestCreation();
        TestEnemyKillProgress();
        TestItemCollectProgress();
        TestExplorationProgress();
        TestQuestCompletion();
        TestMultipleQuestTracking();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Enhanced Quest Tracker Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static (World world, Entity player) SetupPlayer()
    {
        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(0f, 0f));
        world.AddComponent(player, new InventoryComponent());
        world.AddComponent(player, new CurrencyComponent(0));
        world.AddComponent(player, new QuestComponent());
        world.AddComponent(player, new ExperienceComponent());
        world.AddComponent(player, new AbilityComponent());
        return (world, player);
    }

    private static void DrainEvents(QuestTrackerSystem tracker, World world, int ticks = 3)
    {
        for (int i = 0; i < ticks; i++)
            tracker.Update(world, 0.016f);
    }

    // -----------------------------------------------------------------------

    private static void TestStarterQuestCreation()
    {
        Console.WriteLine("[Test] Starter Quest Creation");
        Console.WriteLine("----------------------------------------");

        var quests = QuestTrackerSystem.CreateStarterQuests();

        System.Diagnostics.Debug.Assert(quests.Count == 5,
            "Should create 5 starter quests");

        foreach (var q in quests)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(q.Id), "Quest must have ID");
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(q.Name), "Quest must have name");
            System.Diagnostics.Debug.Assert(q.RequiredProgress > 0, "Quest must require progress");
            System.Diagnostics.Debug.Assert(q.GoldReward >= 0, "Gold reward must be non-negative");
            System.Diagnostics.Debug.Assert(q.ExperienceReward >= 0, "XP reward must be non-negative");
            Console.WriteLine($"  ✓ {q.Name} ({q.Type}) — {q.RequiredProgress} steps, {q.GoldReward}g, {q.ExperienceReward} XP");
        }

        Console.WriteLine("✓ Starter quest creation working\n");
    }

    private static void TestEnemyKillProgress()
    {
        Console.WriteLine("[Test] Enemy Kill Progress");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var tracker = new QuestTrackerSystem();
        tracker.Initialize(world);

        var questComp = world.GetComponent<QuestComponent>(player)!;
        var killQuest = new Quest("kill_goblins", "Goblin Slayer", "Defeat 3 Goblins.", QuestType.Combat)
        {
            RequiredProgress = 3
        };
        questComp.AcceptQuest(killQuest);

        // Notify 2 goblin kills
        QuestTrackerSystem.NotifyEnemyKilled(CreatureType.Goblin, 2);
        DrainEvents(tracker, world);

        System.Diagnostics.Debug.Assert(killQuest.CurrentProgress == 2,
            $"Expected progress 2 but got {killQuest.CurrentProgress}");
        System.Diagnostics.Debug.Assert(!killQuest.IsComplete(), "Quest should not be complete yet");

        Console.WriteLine($"  Progress after 2 kills: {killQuest.CurrentProgress}/{killQuest.RequiredProgress}");

        // Notify the final kill
        QuestTrackerSystem.NotifyEnemyKilled(CreatureType.Goblin, 1);
        DrainEvents(tracker, world);

        System.Diagnostics.Debug.Assert(killQuest.Status == QuestStatus.Completed,
            "Quest should be completed after 3 kills");

        Console.WriteLine($"  Quest status: {killQuest.Status}");
        Console.WriteLine("✓ Enemy kill progress working\n");
    }

    private static void TestItemCollectProgress()
    {
        Console.WriteLine("[Test] Item Collection Progress");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var tracker = new QuestTrackerSystem();
        tracker.Initialize(world);

        var questComp = world.GetComponent<QuestComponent>(player)!;
        var gatherQuest = new Quest("gather_wood", "Lumberjack", "Collect 10 Wood.", QuestType.Gathering)
        {
            RequiredProgress = 10
        };
        questComp.AcceptQuest(gatherQuest);

        // Notify 10 wood collected
        QuestTrackerSystem.NotifyItemCollected(TileType.Wood, 10);
        DrainEvents(tracker, world);

        System.Diagnostics.Debug.Assert(gatherQuest.Status == QuestStatus.Completed,
            $"Expected Completed but got {gatherQuest.Status}");

        Console.WriteLine($"  Wood quest completed successfully.");
        Console.WriteLine("✓ Item collection progress working\n");
    }

    private static void TestExplorationProgress()
    {
        Console.WriteLine("[Test] Exploration Progress");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var tracker = new QuestTrackerSystem();
        tracker.Initialize(world);

        var questComp = world.GetComponent<QuestComponent>(player)!;
        var exploreQuest = new Quest("explore_biomes", "World Explorer",
            "Discover 2 new biome locations.", QuestType.Exploration)
        {
            RequiredProgress = 2
        };
        questComp.AcceptQuest(exploreQuest);

        QuestTrackerSystem.NotifyLocationDiscovered("Desert Biome");
        DrainEvents(tracker, world);
        System.Diagnostics.Debug.Assert(exploreQuest.CurrentProgress == 1, "Progress should be 1");

        QuestTrackerSystem.NotifyLocationDiscovered("Snow Biome");
        DrainEvents(tracker, world);
        System.Diagnostics.Debug.Assert(exploreQuest.Status == QuestStatus.Completed,
            "Quest should complete after 2 discoveries");

        Console.WriteLine("  Exploration quest completed after 2 discoveries.");
        Console.WriteLine("✓ Exploration progress working\n");
    }

    private static void TestQuestCompletion()
    {
        Console.WriteLine("[Test] Quest Completion Rewards");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var tracker = new QuestTrackerSystem();
        tracker.Initialize(world);

        var questComp = world.GetComponent<QuestComponent>(player)!;
        var currency = world.GetComponent<CurrencyComponent>(player)!;
        var xpComp = world.GetComponent<ExperienceComponent>(player)!;

        var rewardQuest = new Quest("reward_test", "Reward Test", "Harvest 1 crop.", QuestType.Farming)
        {
            RequiredProgress = 1,
            GoldReward = 100,
            ExperienceReward = 200
        };
        questComp.AcceptQuest(rewardQuest);

        QuestTrackerSystem.NotifyCropHarvested(TileType.Grass, 1);
        DrainEvents(tracker, world);

        System.Diagnostics.Debug.Assert(rewardQuest.Status == QuestStatus.Completed,
            "Quest should be completed");
        System.Diagnostics.Debug.Assert(currency.Gold >= 100,
            $"Player should have received gold reward (has {currency.Gold})");

        Console.WriteLine($"  Gold after reward: {currency.Gold}");
        Console.WriteLine($"  Quest moved to completed list: {questComp.HasCompletedQuest(rewardQuest.Id)}");
        Console.WriteLine("✓ Quest completion rewards working\n");
    }

    private static void TestMultipleQuestTracking()
    {
        Console.WriteLine("[Test] Multiple Simultaneous Quests");
        Console.WriteLine("----------------------------------------");

        var (world, player) = SetupPlayer();
        var tracker = new QuestTrackerSystem();
        tracker.Initialize(world);

        var questComp = world.GetComponent<QuestComponent>(player)!;

        var starterQuests = QuestTrackerSystem.CreateStarterQuests();
        foreach (var q in starterQuests)
            questComp.AcceptQuest(q);

        System.Diagnostics.Debug.Assert(questComp.ActiveQuests.Count == 5,
            "Should have 5 active quests");

        // Emit events for different quest types
        QuestTrackerSystem.NotifyItemCollected(TileType.TreeOak, 10);  // gather wood
        QuestTrackerSystem.NotifyEnemyKilled(CreatureType.Goblin, 5);  // kill goblins
        QuestTrackerSystem.NotifyLocationDiscovered("Jungle Biome");   // explore
        QuestTrackerSystem.NotifyCropHarvested(TileType.Grass, 5);     // farming
        QuestTrackerSystem.NotifyItemCrafted(TileType.WoodPlank, 3);   // crafting

        DrainEvents(tracker, world, ticks: 5);

        Console.WriteLine($"  Active quests remaining: {questComp.ActiveQuests.Count}");
        Console.WriteLine($"  Completed quests: {questComp.CompletedQuests.Count}");

        System.Diagnostics.Debug.Assert(questComp.CompletedQuests.Count > 0,
            "At least some quests should have completed");

        Console.WriteLine("✓ Multiple quest tracking working\n");
    }
}
