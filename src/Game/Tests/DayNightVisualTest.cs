using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Day/Night Visual Cycle System
/// </summary>
public static class DayNightVisualTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Day/Night Visual Cycle Test");
        Console.WriteLine("=======================================\n");

        RunComponentInitializationTest();
        RunPresetBlendTest();
        RunSystemUpdateTest();
        RunPhaseNamingTest();
        RunBrightnessRangeTest();
        RunFogDensityRangeTest();
        RunFullDayCycleTest();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Day/Night Visual Tests Completed");
        Console.WriteLine("=======================================\n");
    }

    private static void RunComponentInitializationTest()
    {
        Console.WriteLine("[Test] DayNightVisual Component Initialization");
        Console.WriteLine("----------------------------------------");

        var dnv = new DayNightVisualComponent();
        System.Diagnostics.Debug.Assert(dnv.Presets.Count > 0, "Should have default presets");
        System.Diagnostics.Debug.Assert(dnv.CurrentBrightness >= 0f && dnv.CurrentBrightness <= 1f, "Brightness should be in [0,1]");
        System.Diagnostics.Debug.Assert(dnv.CurrentFogDensity >= 0f && dnv.CurrentFogDensity <= 1f, "FogDensity should be in [0,1]");
        System.Diagnostics.Debug.Assert(dnv.CurrentPhaseName.Length > 0, "Phase name should not be empty");

        Console.WriteLine($"  Default presets: {dnv.Presets.Count}");
        foreach (var p in dnv.Presets)
            Console.WriteLine($"    {p.StartHour:00.0}h -> {p.Name} (brightness={p.Brightness:F2}, fog={p.FogDensity:F2})");

        Console.WriteLine("  ✅ DayNightVisualComponent initializes correctly\n");
    }

    private static void RunPresetBlendTest()
    {
        Console.WriteLine("[Test] Preset Blend Factors");
        Console.WriteLine("----------------------------------------");

        var dnv = new DayNightVisualComponent();

        // At noon exactly, we should be AT the Noon preset or just after
        var (noonFrom, noonTo, noonT) = dnv.GetBlendFactors(12f);
        if (noonFrom.Name != "Noon" && noonTo.Name != "Noon")
            throw new InvalidOperationException($"Expected Noon preset at hour 12 but got from={noonFrom.Name}, to={noonTo.Name}");
        if (noonT < 0f || noonT > 1f)
            throw new InvalidOperationException("Blend t must be in [0,1]");

        Console.WriteLine($"  Noon blend:     from={noonFrom.Name}, to={noonTo.Name}, t={noonT:F3}");

        // Midnight
        var (midFrom, midTo, midT) = dnv.GetBlendFactors(0f);
        if (midT < 0f || midT > 1f)
            throw new InvalidOperationException("Midnight blend t must be in [0,1]");

        Console.WriteLine($"  Midnight blend: from={midFrom.Name}, to={midTo.Name}, t={midT:F3}");
        Console.WriteLine("  ✅ Blend factors are computed correctly\n");
    }

    private static void RunSystemUpdateTest()
    {
        Console.WriteLine("[Test] DayNightVisual System Update");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var entity = DayNightVisualSystem.CreateDayNightEntity(world);

        var sys = new DayNightVisualSystem();
        sys.Initialize(world);
        sys.Update(world, 0.016f);

        var dnv = world.GetComponent<DayNightVisualComponent>(entity)!;
        System.Diagnostics.Debug.Assert(dnv.CurrentBrightness >= 0f && dnv.CurrentBrightness <= 1f,
            "Brightness should be in valid range after update");
        System.Diagnostics.Debug.Assert(dnv.CurrentFogDensity >= 0f && dnv.CurrentFogDensity <= 1f,
            "FogDensity should be in valid range after update");

        string desc = DayNightVisualSystem.DescribeState(dnv);
        Console.WriteLine($"  State: {desc}");
        System.Diagnostics.Debug.Assert(desc.Contains("Phase:"), "Description should contain phase");
        System.Diagnostics.Debug.Assert(desc.Contains("Brightness:"), "Description should contain brightness");

        Console.WriteLine("  ✅ DayNightVisualSystem updates state correctly\n");
    }

    private static void RunPhaseNamingTest()
    {
        Console.WriteLine("[Test] Phase Name Changes Through Day");
        Console.WriteLine("----------------------------------------");

        var dnv = new DayNightVisualComponent();
        var sys = new DayNightVisualSystem();

        var hoursAndExpected = new (float hour, string expectedPhase)[]
        {
            (0f,  "Midnight"),
            (5f,  "Dawn"),
            (7f,  "Morning"),
            (12f, "Noon"),
            (17f, "Dusk"),
            (21f, "Night"),
        };

        foreach (var (hour, expected) in hoursAndExpected)
        {
            var (from, _, _) = dnv.GetBlendFactors(hour);
            Console.WriteLine($"  Hour {hour:00.0}: phase = {from.Name}");
            System.Diagnostics.Debug.Assert(from.Name == expected,
                $"At hour {hour}, expected '{expected}' but got '{from.Name}'");
        }

        Console.WriteLine("  ✅ Phase names are correct at key hours\n");
    }

    private static void RunBrightnessRangeTest()
    {
        Console.WriteLine("[Test] Brightness Range Over Full Day");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var entity = DayNightVisualSystem.CreateDayNightEntity(world);
        var dnv = world.GetComponent<DayNightVisualComponent>(entity)!;

        float minBrightness = float.MaxValue;
        float maxBrightness = float.MinValue;

        for (int h = 0; h < 24; h++)
        {
            var (from, to, t) = dnv.GetBlendFactors(h);
            float b = from.Brightness + (to.Brightness - from.Brightness) * t;
            minBrightness = Math.Min(minBrightness, b);
            maxBrightness = Math.Max(maxBrightness, b);
        }

        System.Diagnostics.Debug.Assert(minBrightness < 0.5f, "Minimum brightness (night) should be below 0.5");
        System.Diagnostics.Debug.Assert(maxBrightness >= 0.9f, "Maximum brightness (day) should be 0.9+");

        Console.WriteLine($"  Brightness range: {minBrightness:F2} (night) - {maxBrightness:F2} (day)");
        Console.WriteLine("  ✅ Brightness spans a wide range over the day/night cycle\n");
    }

    private static void RunFogDensityRangeTest()
    {
        Console.WriteLine("[Test] Fog Density During Night vs Day");
        Console.WriteLine("----------------------------------------");

        var dnv = new DayNightVisualComponent();

        // Noon should have low fog
        var (noonFrom, noonTo, noonT) = dnv.GetBlendFactors(12f);
        float noonFog = noonFrom.FogDensity + (noonTo.FogDensity - noonFrom.FogDensity) * noonT;

        // Midnight should have high fog
        var (midFrom, midTo, midT) = dnv.GetBlendFactors(0f);
        float midFog = midFrom.FogDensity + (midTo.FogDensity - midFrom.FogDensity) * midT;

        System.Diagnostics.Debug.Assert(noonFog < midFog, "Night fog should be denser than noon fog");
        Console.WriteLine($"  Noon fog: {noonFog:F2}, Midnight fog: {midFog:F2}");
        Console.WriteLine("  ✅ Fog density correctly higher at night\n");
    }

    private static void RunFullDayCycleTest()
    {
        Console.WriteLine("[Test] Full Day Cycle Visual Summary");
        Console.WriteLine("----------------------------------------");

        var dnv = new DayNightVisualComponent();

        Console.WriteLine("  Hour | Phase      | Brightness | Fog  | Ambient");
        Console.WriteLine("  -----|------------|------------|------|--------");

        for (int h = 0; h < 24; h += 2)
        {
            var (from, to, t) = dnv.GetBlendFactors(h);
            float brightness = from.Brightness + (to.Brightness - from.Brightness) * t;
            float fog = from.FogDensity + (to.FogDensity - from.FogDensity) * t;
            var ambient = AmbientColor.Lerp(from.AmbientLight, to.AmbientLight, t);

            System.Diagnostics.Debug.Assert(brightness >= 0f && brightness <= 1f, "Brightness out of range");
            System.Diagnostics.Debug.Assert(fog >= 0f && fog <= 1f, "Fog out of range");

            Console.WriteLine($"  {h:00}:00 | {from.Name,-10} | {brightness:F2}       | {fog:F2} | {ambient}");
        }

        Console.WriteLine("  ✅ Full day cycle produces valid values at all hours\n");
    }
}
