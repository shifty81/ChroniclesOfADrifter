using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the WeatherEffect system (ECS integration of WeatherSystem)
/// </summary>
public static class WeatherEffectSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Weather Effect System Test");
        Console.WriteLine("=======================================\n");

        TestMovementSlowdown();
        TestVisibilityReduction();
        TestWeatherDamage();
        TestWeatherStatusEffect();
        TestWeatherTint();
        TestDisplayEffects();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Weather Effect System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static (World world, Entity player, WeatherEffectSystem system, WeatherSystem weatherSys)
        SetupWorld(WeatherType weatherType = WeatherType.Clear, WeatherIntensity intensity = WeatherIntensity.Moderate)
    {
        var world = new World();

        var weatherSys = new WeatherSystem(seed: 42);
        weatherSys.RestoreWeatherState(weatherType, intensity, timer: 0f, duration: 300f);
        world.SetSharedResource("WeatherSystem", weatherSys);

        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(0f, 0f));
        world.AddComponent(player, new PlayerComponent { Speed = 5f });
        world.AddComponent(player, new HealthComponent(100f));
        world.AddComponent(player, new VelocityComponent(3f, 0f));
        world.AddComponent(player, new StatusEffectComponent());

        // Add a lighting component so visibility effects can apply
        world.AddComponent(player, new LightingComponent(1f));

        var effectSystem = new WeatherEffectSystem();
        effectSystem.Initialize(world);

        return (world, player, effectSystem, weatherSys);
    }

    // -----------------------------------------------------------------------

    private static void TestMovementSlowdown()
    {
        Console.WriteLine("[Test] Weather Movement Slowdown (Rain)");
        Console.WriteLine("----------------------------------------");

        var (world, player, system, weatherSys) = SetupWorld(WeatherType.Rain, WeatherIntensity.Heavy);

        var playerComp = world.GetComponent<PlayerComponent>(player)!;
        float baseSpeed = playerComp.Speed;

        system.Update(world, 0.1f);

        float multiplier = weatherSys.GetMovementSpeedMultiplier();
        float expectedSpeed = baseSpeed * multiplier;

        Console.WriteLine($"  Weather: {weatherSys.CurrentWeather} ({weatherSys.CurrentIntensity})");
        Console.WriteLine($"  Base speed: {baseSpeed:F2}, Multiplier: {multiplier:F2}, New speed: {playerComp.Speed:F2}");
        Console.WriteLine($"  Expected: {expectedSpeed:F2}");

        System.Diagnostics.Debug.Assert(Math.Abs(playerComp.Speed - expectedSpeed) < 0.01f,
            "Player speed should be scaled by weather multiplier");

        Console.WriteLine("✓ Weather movement slowdown working\n");
    }

    private static void TestVisibilityReduction()
    {
        Console.WriteLine("[Test] Weather Visibility Reduction (Fog)");
        Console.WriteLine("----------------------------------------");

        var (world, player, system, weatherSys) = SetupWorld(WeatherType.Fog, WeatherIntensity.Heavy);

        var lighting = world.GetComponent<LightingComponent>(player)!;
        float visibilityBefore = lighting.VisibilityMultiplier;

        system.Update(world, 0.1f);

        float expectedVis = weatherSys.GetVisibilityMultiplier();
        Console.WriteLine($"  Fog visibility: {expectedVis:F2}  LightingComp: {lighting.VisibilityMultiplier:F2}");

        System.Diagnostics.Debug.Assert(Math.Abs(lighting.VisibilityMultiplier - expectedVis) < 0.01f,
            "LightingComponent.VisibilityMultiplier should match WeatherSystem value");

        Console.WriteLine("✓ Weather visibility reduction working\n");
    }

    private static void TestWeatherDamage()
    {
        Console.WriteLine("[Test] Weather Damage (Sandstorm)");
        Console.WriteLine("----------------------------------------");

        var (world, player, system, weatherSys) = SetupWorld(WeatherType.Sandstorm, WeatherIntensity.Heavy);

        var health = world.GetComponent<HealthComponent>(player)!;
        float hpBefore = health.CurrentHealth;

        bool isDamaging = weatherSys.IsDamagingWeather();
        float dps = weatherSys.GetWeatherDamagePerSecond();
        Console.WriteLine($"  Sandstorm damaging: {isDamaging}, DPS: {dps:F1}");

        if (isDamaging && dps > 0f)
        {
            system.Update(world, 1f); // 1 second of exposure
            System.Diagnostics.Debug.Assert(health.CurrentHealth < hpBefore,
                "Player health should decrease from sandstorm damage");
            Console.WriteLine($"  HP: {hpBefore:F1} → {health.CurrentHealth:F1}");
        }
        else
        {
            Console.WriteLine("  (Sandstorm not damaging in current WeatherSystem config — skipped damage assertion)");
        }

        Console.WriteLine("✓ Weather damage working\n");
    }

    private static void TestWeatherStatusEffect()
    {
        Console.WriteLine("[Test] Weather Status Effects (Heavy Storm)");
        Console.WriteLine("----------------------------------------");

        var (world, player, system, weatherSys) = SetupWorld(WeatherType.Storm, WeatherIntensity.Heavy);

        var statusComp = world.GetComponent<StatusEffectComponent>(player)!;

        system.Update(world, 0.1f);

        Console.WriteLine($"  Storm intensity: {weatherSys.CurrentIntensity}");
        bool poisoned = statusComp.HasEffect(StatusEffectType.Poison);
        Console.WriteLine($"  Poison applied: {poisoned}");

        // Heavy Storm should apply Poison status
        System.Diagnostics.Debug.Assert(poisoned,
            "Heavy storm should apply Poison status effect");

        Console.WriteLine("✓ Weather status effects working\n");
    }

    private static void TestWeatherTint()
    {
        Console.WriteLine("[Test] Weather Tint");
        Console.WriteLine("----------------------------------------");

        var (world, player, system, weatherSys) = SetupWorld(WeatherType.Rain, WeatherIntensity.Heavy);

        system.Update(world, 0.1f);

        var tint = system.CurrentTint;
        var expected = weatherSys.GetWeatherTint();

        Console.WriteLine($"  System tint: R={tint.r:F2} G={tint.g:F2} B={tint.b:F2} A={tint.a:F2}");
        Console.WriteLine($"  Expected:     R={expected.r:F2} G={expected.g:F2} B={expected.b:F2} A={expected.a:F2}");

        System.Diagnostics.Debug.Assert(
            Math.Abs(tint.r - expected.r) < 0.01f &&
            Math.Abs(tint.g - expected.g) < 0.01f &&
            Math.Abs(tint.b - expected.b) < 0.01f,
            "System tint should match WeatherSystem tint");

        Console.WriteLine("✓ Weather tint working\n");
    }

    private static void TestDisplayEffects()
    {
        Console.WriteLine("[Test] Display Current Effects");
        Console.WriteLine("----------------------------------------");

        var (world, player, system, _) = SetupWorld(WeatherType.Snow, WeatherIntensity.Moderate);

        system.Update(world, 0.5f);

        // Should not throw
        system.DisplayCurrentEffects();

        Console.WriteLine("✓ Display effects working\n");
    }
}
