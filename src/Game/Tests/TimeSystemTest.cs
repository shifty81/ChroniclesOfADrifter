using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the day/night cycle and time system
/// </summary>
public static class TimeSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Day/Night Cycle Time System Test");
        Console.WriteLine("=======================================\n");
        
        RunBasicTimeProgressionTest();
        RunPhaseTransitionTest();
        RunAmbientLightTest();
        RunAtmosphereTintTest();
        RunSpawnMultiplierTest();
        RunTimeManipulationTest();
        
        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Time System Tests Passed");
        Console.WriteLine("=======================================\n");
    }
    
    private static void RunBasicTimeProgressionTest()
    {
        Console.WriteLine("[Test] Basic Time Progression");
        Console.WriteLine("----------------------------------------");
        
        // Create time system starting at 8:00 AM with 60x speed
        var timeSystem = new TimeSystem(startHour: 8, timeScale: 60f);
        
        // Verify initial state
        System.Diagnostics.Debug.Assert(timeSystem.CurrentHour == 8, "Initial hour should be 8");
        System.Diagnostics.Debug.Assert(timeSystem.CurrentMinute == 0, "Initial minute should be 0");
        System.Diagnostics.Debug.Assert(timeSystem.DayCount == 0, "Day count should start at 0");
        Console.WriteLine($"✓ Initial time: {timeSystem.GetTimeString()}");
        
        // Advance 1 real second (= 1 game minute at 60x speed)
        timeSystem.Update(1.0f);
        System.Diagnostics.Debug.Assert(timeSystem.CurrentHour == 8, "Hour should still be 8");
        System.Diagnostics.Debug.Assert(timeSystem.CurrentMinute == 1, "Minute should be 1");
        Console.WriteLine($"✓ After 1 second: {timeSystem.GetTimeString()} (1 minute passed)");
        
        // Advance 60 real seconds (= 1 game hour)
        timeSystem.Update(60.0f);
        System.Diagnostics.Debug.Assert(timeSystem.CurrentHour == 9, "Hour should be 9");
        System.Diagnostics.Debug.Assert(timeSystem.CurrentMinute == 1, "Minute should be 1");
        Console.WriteLine($"✓ After 61 seconds: {timeSystem.GetTimeString()} (1 hour 1 min passed)");
        
        // Test day rollover - advance to next day
        timeSystem.SetTime(23);
        timeSystem.Update(60.0f * 60.0f); // Advance 1 hour
        System.Diagnostics.Debug.Assert(timeSystem.CurrentHour == 0, "Hour should roll over to 0");
        System.Diagnostics.Debug.Assert(timeSystem.DayCount == 1, "Day count should be 1");
        Console.WriteLine($"✓ Day rollover works: {timeSystem.GetTimeString()}, Day {timeSystem.DayCount}");
        
        Console.WriteLine();
    }
    
    private static void RunPhaseTransitionTest()
    {
        Console.WriteLine("[Test] Day Phase Transitions");
        Console.WriteLine("----------------------------------------");
        
        var timeSystem = new TimeSystem(startHour: 0, timeScale: 1f);
        
        // Test all phases
        var testCases = new[]
        {
            (Hour: 3, Expected: DayPhase.Night, Name: "3 AM - Night"),
            (Hour: 5, Expected: DayPhase.Dawn, Name: "5 AM - Dawn Start"),
            (Hour: 6, Expected: DayPhase.Dawn, Name: "6 AM - Dawn"),
            (Hour: 7, Expected: DayPhase.Day, Name: "7 AM - Day Start"),
            (Hour: 12, Expected: DayPhase.Day, Name: "12 PM - Noon"),
            (Hour: 17, Expected: DayPhase.Dusk, Name: "5 PM - Dusk Start"),
            (Hour: 18, Expected: DayPhase.Dusk, Name: "6 PM - Dusk"),
            (Hour: 19, Expected: DayPhase.Night, Name: "7 PM - Night Start"),
            (Hour: 23, Expected: DayPhase.Night, Name: "11 PM - Night")
        };
        
        foreach (var testCase in testCases)
        {
            timeSystem.SetTime(testCase.Hour);
            DayPhase actual = timeSystem.CurrentPhase;
            System.Diagnostics.Debug.Assert(actual == testCase.Expected, 
                $"Expected {testCase.Expected} at {testCase.Hour}:00, got {actual}");
            Console.WriteLine($"✓ {testCase.Name}: {actual}");
        }
        
        // Test phase progress
        timeSystem.SetTime(6);
        float progress = timeSystem.PhaseProgress;
        System.Diagnostics.Debug.Assert(progress >= 0f && progress <= 1f, "Phase progress should be 0-1");
        Console.WriteLine($"✓ Phase progress at 6 AM (Dawn): {progress:P0}");
        
        Console.WriteLine();
    }
    
    private static void RunAmbientLightTest()
    {
        Console.WriteLine("[Test] Ambient Light Calculation");
        Console.WriteLine("----------------------------------------");
        
        var timeSystem = new TimeSystem(startHour: 0, timeScale: 1f);
        
        var testCases = new[]
        {
            (Hour: 3, ExpectedMin: 0.15f, ExpectedMax: 0.25f, Name: "Night (3 AM)"),
            (Hour: 5, ExpectedMin: 0.15f, ExpectedMax: 0.30f, Name: "Dawn Start (5 AM)"),
            (Hour: 6, ExpectedMin: 0.40f, ExpectedMax: 0.80f, Name: "Dawn Middle (6 AM)"),
            (Hour: 7, ExpectedMin: 0.95f, ExpectedMax: 1.00f, Name: "Day Start (7 AM)"),
            (Hour: 12, ExpectedMin: 0.95f, ExpectedMax: 1.00f, Name: "Noon (12 PM)"),
            (Hour: 17, ExpectedMin: 0.95f, ExpectedMax: 1.00f, Name: "Dusk Start (5 PM)"),
            (Hour: 18, ExpectedMin: 0.40f, ExpectedMax: 0.80f, Name: "Dusk Middle (6 PM)"),
            (Hour: 19, ExpectedMin: 0.15f, ExpectedMax: 0.25f, Name: "Night Start (7 PM)"),
            (Hour: 22, ExpectedMin: 0.15f, ExpectedMax: 0.25f, Name: "Night (10 PM)")
        };
        
        foreach (var testCase in testCases)
        {
            timeSystem.SetTime(testCase.Hour);
            float light = timeSystem.GetAmbientLightLevel();
            System.Diagnostics.Debug.Assert(light >= testCase.ExpectedMin && light <= testCase.ExpectedMax,
                $"{testCase.Name}: Expected light between {testCase.ExpectedMin} and {testCase.ExpectedMax}, got {light}");
            Console.WriteLine($"✓ {testCase.Name}: {light:P0} light");
        }
        
        // Verify smooth transitions
        timeSystem.SetTime(6); // Dawn
        float light1 = timeSystem.GetAmbientLightLevel();
        timeSystem.Update(30f * 60f); // Advance 30 minutes
        float light2 = timeSystem.GetAmbientLightLevel();
        System.Diagnostics.Debug.Assert(light2 > light1, "Light should increase during dawn");
        Console.WriteLine($"✓ Light increases during dawn: {light1:P0} -> {light2:P0}");
        
        Console.WriteLine();
    }
    
    private static void RunAtmosphereTintTest()
    {
        Console.WriteLine("[Test] Atmospheric Tint");
        Console.WriteLine("----------------------------------------");
        
        var timeSystem = new TimeSystem(startHour: 0, timeScale: 1f);
        
        var testTimes = new[] { 6, 12, 18, 22 }; // Dawn, Day, Dusk, Night
        
        foreach (var hour in testTimes)
        {
            timeSystem.SetTime(hour);
            var (r, g, b, a) = timeSystem.GetAtmosphereTint();
            
            // Verify all values are in valid range
            System.Diagnostics.Debug.Assert(r >= 0f && r <= 1f, "Red should be 0-1");
            System.Diagnostics.Debug.Assert(g >= 0f && g <= 1f, "Green should be 0-1");
            System.Diagnostics.Debug.Assert(b >= 0f && b <= 1f, "Blue should be 0-1");
            System.Diagnostics.Debug.Assert(a >= 0f && a <= 1f, "Alpha should be 0-1");
            
            Console.WriteLine($"✓ {hour:D2}:00 ({timeSystem.CurrentPhase}): " +
                            $"R={r:F2} G={g:F2} B={b:F2} A={a:F2}");
        }
        
        // Verify dawn has warm colors (high red)
        timeSystem.SetTime(6);
        var dawnTint = timeSystem.GetAtmosphereTint();
        System.Diagnostics.Debug.Assert(dawnTint.r >= 0.8f, "Dawn should have warm colors (high red)");
        Console.WriteLine($"✓ Dawn has warm tint (R={dawnTint.r:F2})");
        
        // Verify night has cool colors (higher blue)
        timeSystem.SetTime(22);
        var nightTint = timeSystem.GetAtmosphereTint();
        System.Diagnostics.Debug.Assert(nightTint.b >= nightTint.r, "Night should have cool colors (blue > red)");
        Console.WriteLine($"✓ Night has cool tint (B={nightTint.b:F2} > R={nightTint.r:F2})");
        
        Console.WriteLine();
    }
    
    private static void RunSpawnMultiplierTest()
    {
        Console.WriteLine("[Test] Creature Spawn Multipliers");
        Console.WriteLine("----------------------------------------");
        
        var timeSystem = new TimeSystem(startHour: 0, timeScale: 1f);
        
        // Test hostile spawn multipliers
        timeSystem.SetTime(12); // Day
        float dayHostile = timeSystem.GetHostileSpawnMultiplier();
        timeSystem.SetTime(22); // Night
        float nightHostile = timeSystem.GetHostileSpawnMultiplier();
        System.Diagnostics.Debug.Assert(nightHostile > dayHostile, 
            "Night should have more hostile creatures than day");
        Console.WriteLine($"✓ Hostile spawn rate: Day={dayHostile:F1}x, Night={nightHostile:F1}x");
        
        // Test peaceful spawn multipliers
        timeSystem.SetTime(12); // Day
        float dayPeaceful = timeSystem.GetPeacefulSpawnMultiplier();
        timeSystem.SetTime(22); // Night
        float nightPeaceful = timeSystem.GetPeacefulSpawnMultiplier();
        System.Diagnostics.Debug.Assert(dayPeaceful > nightPeaceful,
            "Day should have more peaceful creatures than night");
        Console.WriteLine($"✓ Peaceful spawn rate: Day={dayPeaceful:F1}x, Night={nightPeaceful:F1}x");
        
        // Test all phases
        var phases = new[] { 6, 12, 18, 22 }; // Dawn, Day, Dusk, Night
        foreach (var hour in phases)
        {
            timeSystem.SetTime(hour);
            float hostile = timeSystem.GetHostileSpawnMultiplier();
            float peaceful = timeSystem.GetPeacefulSpawnMultiplier();
            Console.WriteLine($"✓ {hour:D2}:00 ({timeSystem.CurrentPhase}): " +
                            $"Hostile={hostile:F1}x, Peaceful={peaceful:F1}x");
        }
        
        Console.WriteLine();
    }
    
    private static void RunTimeManipulationTest()
    {
        Console.WriteLine("[Test] Time Manipulation");
        Console.WriteLine("----------------------------------------");
        
        var timeSystem = new TimeSystem(startHour: 8, timeScale: 60f);
        
        // Test time scale change
        timeSystem.TimeScale = 120f; // Double speed
        System.Diagnostics.Debug.Assert(timeSystem.TimeScale == 120f, "Time scale should be 120");
        Console.WriteLine($"✓ Time scale changed to {timeSystem.TimeScale}x");
        
        // Test SetTime
        timeSystem.SetTime(15);
        System.Diagnostics.Debug.Assert(timeSystem.CurrentHour == 15, "Hour should be 15");
        System.Diagnostics.Debug.Assert(timeSystem.CurrentMinute == 0, "Minute should be 0");
        Console.WriteLine($"✓ Set time to {timeSystem.GetTimeString()}");
        
        // Test IsDaytime and IsNighttime
        timeSystem.SetTime(12);
        System.Diagnostics.Debug.Assert(timeSystem.IsDaytime(), "12 PM should be daytime");
        System.Diagnostics.Debug.Assert(!timeSystem.IsNighttime(), "12 PM should not be nighttime");
        Console.WriteLine($"✓ 12:00 is daytime: {timeSystem.IsDaytime()}");
        
        timeSystem.SetTime(22);
        System.Diagnostics.Debug.Assert(!timeSystem.IsDaytime(), "10 PM should not be daytime");
        System.Diagnostics.Debug.Assert(timeSystem.IsNighttime(), "10 PM should be nighttime");
        Console.WriteLine($"✓ 22:00 is nighttime: {timeSystem.IsNighttime()}");
        
        // Test formatted strings
        timeSystem.SetTime(9);
        timeSystem.Update(30f * 60f); // Advance 30 minutes
        string timeStr = timeSystem.GetTimeString();
        string timeWithPhase = timeSystem.GetTimeStringWithPhase();
        System.Diagnostics.Debug.Assert(timeStr.Contains("09:30"), "Time string should be 09:30");
        Console.WriteLine($"✓ Time string: {timeStr}");
        Console.WriteLine($"✓ Time with phase: {timeWithPhase}");
        
        Console.WriteLine();
    }
}
