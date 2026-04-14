using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Achievement System
/// </summary>
public static class AchievementSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Achievement System Test");
        Console.WriteLine("=======================================\n");

        RunAchievementInitializationTest();
        RunCombatAchievementTest();
        RunExplorationAchievementTest();
        RunCraftingAchievementTest();
        RunFarmingAchievementTest();
        RunSocialAchievementTest();
        RunSurvivalAchievementTest();
        RunCollectionAchievementTest();
        RunProgressionAchievementTest();
        RunMultipleUnlockTest();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Achievement Tests Completed");
        Console.WriteLine("=======================================\n");
    }

    private static void RunAchievementInitializationTest()
    {
        Console.WriteLine("[Test] Achievement Initialization");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());

        AchievementSystem.InitializePlayer(world, player);

        var ach = world.GetComponent<AchievementComponent>(player);
        System.Diagnostics.Debug.Assert(ach != null, "Achievement component should exist");
        System.Diagnostics.Debug.Assert(ach!.Progress.Count > 0, "Progress slots should be initialized");
        System.Diagnostics.Debug.Assert(ach.GetUnlockedCount() == 0, "No achievements should be unlocked at start");

        Console.WriteLine($"  ✅ {ach.Progress.Count} achievements initialized, none unlocked\n");
    }

    private static void RunCombatAchievementTest()
    {
        Console.WriteLine("[Test] Combat Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        // First kill -> "First Blood"
        AchievementSystem.OnEnemyDefeated(world, player);
        var ach = world.GetComponent<AchievementComponent>(player)!;
        new AchievementSystem().Update(world, 0f);

        System.Diagnostics.Debug.Assert(ach.EnemiesDefeated == 1, "Should have 1 enemy defeated");
        System.Diagnostics.Debug.Assert(ach.Progress["first_blood"].IsUnlocked, "'First Blood' should be unlocked");
        System.Diagnostics.Debug.Assert(!ach.Progress["warrior"].IsUnlocked, "'Warrior' should not yet be unlocked");

        // Defeat 49 more for Warrior
        for (int i = 0; i < 49; i++)
            AchievementSystem.OnEnemyDefeated(world, player);
        new AchievementSystem().Update(world, 0f);

        System.Diagnostics.Debug.Assert(ach.EnemiesDefeated == 50, "Should have 50 enemies defeated");
        System.Diagnostics.Debug.Assert(ach.Progress["warrior"].IsUnlocked, "'Warrior' should be unlocked at 50 kills");

        Console.WriteLine("  ✅ Combat achievements unlock at correct thresholds\n");
    }

    private static void RunExplorationAchievementTest()
    {
        Console.WriteLine("[Test] Exploration Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        for (int i = 0; i < 10; i++)
            AchievementSystem.OnChunkExplored(world, player);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["explorer"].IsUnlocked, "'Explorer' should unlock at 10 chunks");
        System.Diagnostics.Debug.Assert(!ach.Progress["adventurer"].IsUnlocked, "'Adventurer' should not yet be unlocked");

        Console.WriteLine("  ✅ Exploration achievements unlock correctly\n");
    }

    private static void RunCraftingAchievementTest()
    {
        Console.WriteLine("[Test] Crafting Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        AchievementSystem.OnCraftCompleted(world, player);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["apprentice_smith"].IsUnlocked, "'Apprentice Smith' should unlock on first craft");

        Console.WriteLine("  ✅ Crafting achievements unlock correctly\n");
    }

    private static void RunFarmingAchievementTest()
    {
        Console.WriteLine("[Test] Farming Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        AchievementSystem.OnCropHarvested(world, player);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["green_thumb"].IsUnlocked, "'Green Thumb' should unlock on first harvest");

        Console.WriteLine("  ✅ Farming achievements unlock correctly\n");
    }

    private static void RunSocialAchievementTest()
    {
        Console.WriteLine("[Test] Social Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        for (int i = 0; i < 5; i++)
            AchievementSystem.OnNPCSpokenTo(world, player);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["socialite"].IsUnlocked, "'Socialite' should unlock after 5 NPCs");

        Console.WriteLine("  ✅ Social achievements unlock correctly\n");
    }

    private static void RunSurvivalAchievementTest()
    {
        Console.WriteLine("[Test] Survival Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        AchievementSystem.OnPlayerDied(world, player);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["survivor"].IsUnlocked, "'Survivor' should unlock on first death");

        Console.WriteLine("  ✅ Survival achievements unlock correctly\n");
    }

    private static void RunCollectionAchievementTest()
    {
        Console.WriteLine("[Test] Collection Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        AchievementSystem.OnItemCollected(world, player, 100);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["hoarder"].IsUnlocked, "'Hoarder' should unlock at 100 items");
        System.Diagnostics.Debug.Assert(!ach.Progress["pack_rat"].IsUnlocked, "'Pack Rat' should not unlock yet");

        Console.WriteLine("  ✅ Collection achievements unlock correctly\n");
    }

    private static void RunProgressionAchievementTest()
    {
        Console.WriteLine("[Test] Progression Achievements");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        AchievementSystem.OnLevelReached(world, player, 5);
        new AchievementSystem().Update(world, 0f);

        var ach = world.GetComponent<AchievementComponent>(player)!;
        System.Diagnostics.Debug.Assert(ach.Progress["level5"].IsUnlocked, "'Seasoned' should unlock at level 5");
        System.Diagnostics.Debug.Assert(!ach.Progress["level10"].IsUnlocked, "'Veteran' should not unlock yet");

        AchievementSystem.OnLevelReached(world, player, 10);
        new AchievementSystem().Update(world, 0f);
        System.Diagnostics.Debug.Assert(ach.Progress["level10"].IsUnlocked, "'Veteran' should unlock at level 10");

        Console.WriteLine("  ✅ Progression achievements unlock correctly\n");
    }

    private static void RunMultipleUnlockTest()
    {
        Console.WriteLine("[Test] Multiple Achievement Unlocks & XP Reward");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new AchievementComponent());
        AchievementSystem.InitializePlayer(world, player);

        // Defeat 50 enemies (unlocks "First Blood" and "Warrior")
        for (int i = 0; i < 50; i++)
            AchievementSystem.OnEnemyDefeated(world, player);

        var sys = new AchievementSystem();
        sys.Update(world, 0f); // Flush notifications

        var ach = world.GetComponent<AchievementComponent>(player)!;
        int unlocked = ach.GetUnlockedCount();
        System.Diagnostics.Debug.Assert(unlocked >= 2, "Should have at least 2 achievements unlocked");
        System.Diagnostics.Debug.Assert(ach.TotalXPEarned > 0, "Should have earned XP from achievements");

        Console.WriteLine($"  ✅ {unlocked} achievements unlocked, {ach.TotalXPEarned} XP earned\n");
    }
}
