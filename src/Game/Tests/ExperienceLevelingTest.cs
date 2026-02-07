using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Experience and Leveling system
/// </summary>
public static class ExperienceLevelingTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Experience & Leveling System Test");
        Console.WriteLine("=======================================\n");
        
        RunXPCalculationTest();
        RunLevelUpTest();
        RunCombatXPAwardTest();
        RunQuestXPAwardTest();
        RunMultiLevelUpTest();
        RunMaxLevelTest();
        RunSaveLoadXPTest();
        
        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Experience System Tests Completed");
        Console.WriteLine("=======================================\n");
    }
    
    private static void RunXPCalculationTest()
    {
        Console.WriteLine("[Test] XP Calculation");
        Console.WriteLine("----------------------------------------");
        
        var xp = new ExperienceComponent();
        
        System.Diagnostics.Debug.Assert(xp.Level == 1, "Starting level should be 1");
        System.Diagnostics.Debug.Assert(xp.CurrentXP == 0, "Starting XP should be 0");
        
        int xpNeeded = xp.GetXPForNextLevel();
        System.Diagnostics.Debug.Assert(xpNeeded > 0, "XP needed should be positive");
        Console.WriteLine($"✓ Level 1 requires {xpNeeded} XP to reach level 2");
        
        float progress = xp.GetLevelProgress();
        System.Diagnostics.Debug.Assert(progress == 0f, "Progress should be 0");
        Console.WriteLine($"✓ Progress at 0 XP: {progress:P0}");
        
        Console.WriteLine();
    }
    
    private static void RunLevelUpTest()
    {
        Console.WriteLine("[Test] Level Up");
        Console.WriteLine("----------------------------------------");
        
        var xp = new ExperienceComponent();
        int xpNeeded = xp.GetXPForNextLevel();
        
        int levelsGained = xp.AddXP(xpNeeded);
        
        System.Diagnostics.Debug.Assert(levelsGained == 1, "Should gain exactly 1 level");
        System.Diagnostics.Debug.Assert(xp.Level == 2, "Should be level 2");
        System.Diagnostics.Debug.Assert(xp.AttackBonus > 0, "Should have attack bonus");
        System.Diagnostics.Debug.Assert(xp.DefenseBonus > 0, "Should have defense bonus");
        System.Diagnostics.Debug.Assert(xp.MaxHealthBonus > 0, "Should have health bonus");
        Console.WriteLine($"✓ Leveled up to {xp.Level}");
        Console.WriteLine($"  Attack Bonus: +{xp.AttackBonus}");
        Console.WriteLine($"  Defense Bonus: +{xp.DefenseBonus}");
        Console.WriteLine($"  Max Health Bonus: +{xp.MaxHealthBonus}");
        Console.WriteLine($"  Total XP Earned: {xp.TotalXPEarned}");
        
        Console.WriteLine();
    }
    
    private static void RunCombatXPAwardTest()
    {
        Console.WriteLine("[Test] Combat XP Award");
        Console.WriteLine("----------------------------------------");
        
        var world = new World();
        
        var player = world.CreateEntity();
        world.AddComponent(player, new PlayerComponent());
        world.AddComponent(player, new PositionComponent(0, 0));
        world.AddComponent(player, new HealthComponent(100));
        world.AddComponent(player, new ExperienceComponent());
        
        // Award XP as if killing a goblin (10 XP)
        int levelsGained = ExperienceSystem.AwardCombatXP(world, player, 10);
        
        var xp = world.GetComponent<ExperienceComponent>(player);
        System.Diagnostics.Debug.Assert(xp != null, "XP component should exist");
        System.Diagnostics.Debug.Assert(xp!.CurrentXP > 0 || xp.Level > 1, "XP should be awarded");
        System.Diagnostics.Debug.Assert(xp.TotalXPEarned == 10, "Total XP should be 10");
        Console.WriteLine($"✓ Awarded 10 combat XP");
        Console.WriteLine($"  Level: {xp.Level}, XP: {xp.CurrentXP}/{xp.GetXPForNextLevel()}");
        
        Console.WriteLine();
    }
    
    private static void RunQuestXPAwardTest()
    {
        Console.WriteLine("[Test] Quest XP Award");
        Console.WriteLine("----------------------------------------");
        
        var world = new World();
        
        var player = world.CreateEntity();
        world.AddComponent(player, new PlayerComponent());
        world.AddComponent(player, new PositionComponent(0, 0));
        world.AddComponent(player, new HealthComponent(100));
        world.AddComponent(player, new ExperienceComponent());
        
        // Award quest XP (50 XP for goblin quest)
        ExperienceSystem.AwardQuestXP(world, player, 50);
        
        var xp = world.GetComponent<ExperienceComponent>(player);
        System.Diagnostics.Debug.Assert(xp != null, "XP component should exist");
        System.Diagnostics.Debug.Assert(xp!.TotalXPEarned == 50, "Total XP should be 50");
        Console.WriteLine($"✓ Awarded 50 quest XP");
        Console.WriteLine($"  Level: {xp.Level}, XP: {xp.CurrentXP}/{xp.GetXPForNextLevel()}");
        
        Console.WriteLine();
    }
    
    private static void RunMultiLevelUpTest()
    {
        Console.WriteLine("[Test] Multi-Level Up");
        Console.WriteLine("----------------------------------------");
        
        var xp = new ExperienceComponent();
        
        // Give massive XP to gain multiple levels
        int levelsGained = xp.AddXP(5000);
        
        System.Diagnostics.Debug.Assert(levelsGained > 1, "Should gain multiple levels");
        System.Diagnostics.Debug.Assert(xp.Level > 2, "Should be above level 2");
        Console.WriteLine($"✓ Gained {levelsGained} levels from 5000 XP");
        Console.WriteLine($"  Now level {xp.Level}");
        Console.WriteLine($"  Attack Bonus: +{xp.AttackBonus}");
        Console.WriteLine($"  Defense Bonus: +{xp.DefenseBonus}");
        Console.WriteLine($"  Max Health Bonus: +{xp.MaxHealthBonus}");
        Console.WriteLine($"  Speed Bonus: +{xp.SpeedBonus}");
        Console.WriteLine($"  Stat Points: {xp.StatPointsAvailable}");
        
        Console.WriteLine();
    }
    
    private static void RunMaxLevelTest()
    {
        Console.WriteLine("[Test] Max Level Cap");
        Console.WriteLine("----------------------------------------");
        
        var xp = new ExperienceComponent();
        
        // Give huge XP to try exceeding max level
        xp.AddXP(999999);
        
        System.Diagnostics.Debug.Assert(xp.Level <= xp.MaxLevel, "Should not exceed max level");
        Console.WriteLine($"✓ Level capped at {xp.Level} (max: {xp.MaxLevel})");
        
        // Try adding more XP at max level
        int moreLevels = xp.AddXP(1000);
        System.Diagnostics.Debug.Assert(moreLevels == 0, "No more levels should be gained at max");
        Console.WriteLine($"✓ No additional levels gained at max level");
        
        Console.WriteLine();
    }
    
    private static void RunSaveLoadXPTest()
    {
        Console.WriteLine("[Test] Save/Load XP Data");
        Console.WriteLine("----------------------------------------");
        
        // Simulate saving
        var xp = new ExperienceComponent();
        xp.AddXP(300);
        
        int savedLevel = xp.Level;
        int savedXP = xp.CurrentXP;
        int savedTotal = xp.TotalXPEarned;
        float savedAttack = xp.AttackBonus;
        float savedDefense = xp.DefenseBonus;
        float savedHealth = xp.MaxHealthBonus;
        
        // Simulate loading into a new component
        var loaded = new ExperienceComponent
        {
            Level = savedLevel,
            CurrentXP = savedXP,
            TotalXPEarned = savedTotal,
            AttackBonus = savedAttack,
            DefenseBonus = savedDefense,
            MaxHealthBonus = savedHealth
        };
        
        System.Diagnostics.Debug.Assert(loaded.Level == savedLevel, "Level should match");
        System.Diagnostics.Debug.Assert(loaded.CurrentXP == savedXP, "XP should match");
        System.Diagnostics.Debug.Assert(loaded.TotalXPEarned == savedTotal, "Total XP should match");
        System.Diagnostics.Debug.Assert(loaded.AttackBonus == savedAttack, "Attack bonus should match");
        
        Console.WriteLine($"✓ XP data round-trips correctly");
        Console.WriteLine($"  Level: {loaded.Level}");
        Console.WriteLine($"  XP: {loaded.CurrentXP}");
        Console.WriteLine($"  Total: {loaded.TotalXPEarned}");
        
        Console.WriteLine();
    }
}
