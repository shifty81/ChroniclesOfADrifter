using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test suite for the status effect system
/// </summary>
public static class StatusEffectTest
{
    public static void Run()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Status Effect System Tests");
        Console.WriteLine("===========================================\n");

        TestApplyEffect();
        TestDamageOverTime();
        TestEffectExpiry();
        TestFrozenSpeed();
        TestStunnedIncapacitation();
        TestEffectReplacement();
        TestMultipleEffects();

        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Status Effect Tests Completed!");
        Console.WriteLine("===========================================\n");
    }

    private static void TestApplyEffect()
    {
        Console.WriteLine("[Test] Apply Status Effect");

        var world = new World();
        var entity = world.CreateEntity();
        world.AddComponent(entity, new HealthComponent(100f));

        StatusEffectSystem.ApplyEffect(world, entity, StatusEffectType.Poison, 5f);

        var effects = world.GetComponent<StatusEffectComponent>(entity);
        bool hasPoison = effects?.HasEffect(StatusEffectType.Poison) ?? false;

        Console.WriteLine($"  Has poison effect: {hasPoison}");
        Console.WriteLine($"  Active effects count: {effects?.ActiveEffects.Count}");

        if (hasPoison && effects?.ActiveEffects.Count == 1)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestDamageOverTime()
    {
        Console.WriteLine("[Test] Damage Over Time");

        var world = new World();
        var system = new StatusEffectSystem();
        system.Initialize(world);

        var entity = world.CreateEntity();
        world.AddComponent(entity, new HealthComponent(100f));

        // Apply poison (2 dps by default)
        StatusEffectSystem.ApplyEffect(world, entity, StatusEffectType.Poison, 5f);

        // Simulate 1 second
        system.Update(world, 1.0f);

        var health = world.GetComponent<HealthComponent>(entity);
        float expectedHealth = 100f - 2f; // 2 dps * 1 second

        Console.WriteLine($"  Health after 1s of poison: {health?.CurrentHealth:F1} (expected ~{expectedHealth:F1})");

        if (health != null && Math.Abs(health.CurrentHealth - expectedHealth) < 0.1f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestEffectExpiry()
    {
        Console.WriteLine("[Test] Effect Expiry");

        var world = new World();
        var system = new StatusEffectSystem();
        system.Initialize(world);

        var entity = world.CreateEntity();
        world.AddComponent(entity, new HealthComponent(100f));

        // Apply short-duration effect
        StatusEffectSystem.ApplyEffect(world, entity, StatusEffectType.Burning, 2f);

        var effects = world.GetComponent<StatusEffectComponent>(entity);

        // Simulate 3 seconds (should expire after 2)
        system.Update(world, 3.0f);

        bool stillHasEffect = effects?.HasEffect(StatusEffectType.Burning) ?? false;

        Console.WriteLine($"  Effect active after 3s (2s duration): {stillHasEffect}");

        if (!stillHasEffect)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestFrozenSpeed()
    {
        Console.WriteLine("[Test] Frozen Speed Reduction");

        var effects = new StatusEffectComponent();
        var frozen = new StatusEffect(StatusEffectType.Frozen, 3f);
        effects.ApplyEffect(frozen);

        float speedMultiplier = effects.GetSpeedMultiplier();

        Console.WriteLine($"  Speed multiplier while frozen: {speedMultiplier}");

        if (Math.Abs(speedMultiplier - 0.3f) < 0.01f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestStunnedIncapacitation()
    {
        Console.WriteLine("[Test] Stunned Incapacitation");

        var effects = new StatusEffectComponent();
        var stun = new StatusEffect(StatusEffectType.Stunned, 2f);
        effects.ApplyEffect(stun);

        bool incapacitated = effects.IsIncapacitated();
        float speed = effects.GetSpeedMultiplier();

        Console.WriteLine($"  Incapacitated: {incapacitated}");
        Console.WriteLine($"  Speed multiplier: {speed}");

        if (incapacitated && speed == 0f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestEffectReplacement()
    {
        Console.WriteLine("[Test] Effect Replacement - Longer replaces shorter");

        var effects = new StatusEffectComponent();

        // Apply short poison
        effects.ApplyEffect(new StatusEffect(StatusEffectType.Poison, 3f));
        float firstRemaining = effects.ActiveEffects[0].RemainingTime;

        // Apply longer poison (should replace)
        effects.ApplyEffect(new StatusEffect(StatusEffectType.Poison, 8f));
        float secondRemaining = effects.ActiveEffects[0].RemainingTime;

        Console.WriteLine($"  First poison duration: {firstRemaining}s");
        Console.WriteLine($"  After longer poison applied: {secondRemaining}s");
        Console.WriteLine($"  Total active effects: {effects.ActiveEffects.Count}");

        if (effects.ActiveEffects.Count == 1 && secondRemaining == 8f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestMultipleEffects()
    {
        Console.WriteLine("[Test] Multiple Different Effects");

        var effects = new StatusEffectComponent();
        effects.ApplyEffect(new StatusEffect(StatusEffectType.Poison, 5f));
        effects.ApplyEffect(new StatusEffect(StatusEffectType.Frozen, 3f));
        effects.ApplyEffect(new StatusEffect(StatusEffectType.Bleeding, 4f));

        Console.WriteLine($"  Active effects: {effects.ActiveEffects.Count}");
        Console.WriteLine($"  Has Poison: {effects.HasEffect(StatusEffectType.Poison)}");
        Console.WriteLine($"  Has Frozen: {effects.HasEffect(StatusEffectType.Frozen)}");
        Console.WriteLine($"  Has Bleeding: {effects.HasEffect(StatusEffectType.Bleeding)}");
        Console.WriteLine($"  Speed multiplier: {effects.GetSpeedMultiplier()}");

        bool allPresent = effects.ActiveEffects.Count == 3 &&
                          effects.HasEffect(StatusEffectType.Poison) &&
                          effects.HasEffect(StatusEffectType.Frozen) &&
                          effects.HasEffect(StatusEffectType.Bleeding);

        if (allPresent)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
}
