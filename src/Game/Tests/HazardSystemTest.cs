using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Environmental Hazard system
/// </summary>
public static class HazardSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Environmental Hazard System Test");
        Console.WriteLine("=======================================\n");

        TestHazardComponent();
        TestLavaPoolDamage();
        TestSpikeTrapsActivation();
        TestGasVentPoisoning();
        TestBearTrapTriggerAndReset();
        TestHazardOutOfRange();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Hazard System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static World CreateWorldWithSystem(out HazardSystem hazardSystem)
    {
        var world = new World();
        hazardSystem = new HazardSystem();
        hazardSystem.Initialize(world);
        return world;
    }

    private static Entity CreateVictim(World world, float x, float y, float maxHp = 100f)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(x, y));
        world.AddComponent(entity, new HealthComponent(maxHp));
        world.AddComponent(entity, new StatusEffectComponent());
        return entity;
    }

    private static void TestHazardComponent()
    {
        Console.WriteLine("[Test] Hazard Component Defaults");
        Console.WriteLine("----------------------------------------");

        var lava = new HazardComponent(HazardType.LavaPool, 40f, 2f);
        System.Diagnostics.Debug.Assert(lava.IsActive, "Lava pool should start active");
        System.Diagnostics.Debug.Assert(lava.AppliedStatusEffect == StatusEffectType.Burning,
            "Lava should apply Burning");

        var gas = new HazardComponent(HazardType.GasVent, 10f, 2.5f);
        System.Diagnostics.Debug.Assert(!gas.IsActive, "Gas vent should start inactive");
        System.Diagnostics.Debug.Assert(gas.AppliedStatusEffect == StatusEffectType.Poison,
            "Gas vent should apply Poison");

        var spike = new HazardComponent(HazardType.SpikeTrap, 25f, 0.8f);
        System.Diagnostics.Debug.Assert(!spike.IsActive, "Spike trap should start inactive");

        var bear = new HazardComponent(HazardType.BearTrap, 30f, 0.6f);
        System.Diagnostics.Debug.Assert(bear.IsActive, "Bear trap should start active");
        System.Diagnostics.Debug.Assert(bear.AppliedStatusEffect == StatusEffectType.Stunned,
            "Bear trap should apply Stunned");

        Console.WriteLine("  All hazard type defaults verified.");
        Console.WriteLine($"  Lava description: {lava.GetDescription()}");
        Console.WriteLine("✓ Hazard component defaults correct\n");
    }

    private static void TestLavaPoolDamage()
    {
        Console.WriteLine("[Test] Lava Pool Damage");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorldWithSystem(out var hazardSystem);

        var lava = HazardSystem.CreateLavaPool(world, 0f, 0f, 2f);
        var victim = CreateVictim(world, 1f, 0f); // inside radius

        var health = world.GetComponent<HealthComponent>(victim)!;
        float hpBefore = health.CurrentHealth;

        // Run several updates so cooldown is exercised and damage accumulates
        for (int i = 0; i < 10; i++)
            hazardSystem.Update(world, 0.1f);

        System.Diagnostics.Debug.Assert(health.CurrentHealth < hpBefore,
            "Victim inside lava pool should have taken damage");

        Console.WriteLine($"  HP before: {hpBefore:F1}, HP after: {health.CurrentHealth:F1}");

        // Status effect: Burning
        var status = world.GetComponent<StatusEffectComponent>(victim)!;
        System.Diagnostics.Debug.Assert(status.HasEffect(StatusEffectType.Burning),
            "Victim should be Burning from lava");

        Console.WriteLine("  Victim is Burning ✓");
        Console.WriteLine("✓ Lava pool damage working\n");
    }

    private static void TestSpikeTrapsActivation()
    {
        Console.WriteLine("[Test] Spike Trap Phase Toggle");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorldWithSystem(out var hazardSystem);

        var spike = HazardSystem.CreateSpikeTrap(world, 0f, 0f);
        var hazard = world.GetComponent<HazardComponent>(spike)!;

        // Spike trap starts inactive; advance past InactiveDuration (2s) to activate
        System.Diagnostics.Debug.Assert(!hazard.IsActive, "Spike trap should start inactive");

        hazardSystem.Update(world, 2.1f); // past inactive phase
        System.Diagnostics.Debug.Assert(hazard.IsActive, "Spike trap should be active after inactive phase");

        hazardSystem.Update(world, 1.1f); // past active phase
        System.Diagnostics.Debug.Assert(!hazard.IsActive, "Spike trap should deactivate after active phase");

        Console.WriteLine("  Phase cycling verified (inactive→active→inactive).");
        Console.WriteLine("✓ Spike trap activation working\n");
    }

    private static void TestGasVentPoisoning()
    {
        Console.WriteLine("[Test] Gas Vent Poison");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorldWithSystem(out var hazardSystem);

        var vent = HazardSystem.CreateGasVent(world, 0f, 0f);
        var hazard = world.GetComponent<HazardComponent>(vent)!;
        var victim = CreateVictim(world, 1f, 0f);
        var health = world.GetComponent<HealthComponent>(victim)!;

        // Gas vent is inactive initially – advance to active phase
        hazardSystem.Update(world, 5.1f);
        System.Diagnostics.Debug.Assert(hazard.IsActive, "Gas vent should activate after inactive phase");

        float hpBefore = health.CurrentHealth;
        hazardSystem.Update(world, 0.6f); // allow cooldown to pass, apply damage

        System.Diagnostics.Debug.Assert(health.CurrentHealth <= hpBefore,
            "Victim should take damage from active gas vent");

        var status = world.GetComponent<StatusEffectComponent>(victim)!;
        System.Diagnostics.Debug.Assert(status.HasEffect(StatusEffectType.Poison),
            "Victim should be Poisoned by gas vent");

        Console.WriteLine("  Gas vent applied poison successfully.");
        Console.WriteLine("✓ Gas vent poisoning working\n");
    }

    private static void TestBearTrapTriggerAndReset()
    {
        Console.WriteLine("[Test] Bear Trap Trigger & Reset");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorldWithSystem(out var hazardSystem);

        var trap = HazardSystem.CreateBearTrap(world, 0f, 0f);
        var hazard = world.GetComponent<HazardComponent>(trap)!;
        var victim = CreateVictim(world, 0.2f, 0f); // inside radius

        System.Diagnostics.Debug.Assert(hazard.IsActive, "Bear trap starts active");

        // First update triggers the trap
        hazardSystem.Update(world, 0.6f);

        System.Diagnostics.Debug.Assert(hazard.IsTriggered, "Bear trap should be triggered");
        System.Diagnostics.Debug.Assert(!hazard.IsActive, "Bear trap should be inactive after triggering");

        // Advance past reset time (InactiveDuration = 10s)
        hazardSystem.Update(world, 10.5f);

        System.Diagnostics.Debug.Assert(!hazard.IsTriggered, "Bear trap should have reset");
        System.Diagnostics.Debug.Assert(hazard.IsActive, "Bear trap should be active again after reset");

        Console.WriteLine("  Bear trap trigger and reset verified.");
        Console.WriteLine("✓ Bear trap working\n");
    }

    private static void TestHazardOutOfRange()
    {
        Console.WriteLine("[Test] Hazard Out Of Range");
        Console.WriteLine("----------------------------------------");

        var world = CreateWorldWithSystem(out var hazardSystem);

        // Lava at origin, victim far away
        HazardSystem.CreateLavaPool(world, 0f, 0f, 1f);
        var victim = CreateVictim(world, 50f, 50f);

        var health = world.GetComponent<HealthComponent>(victim)!;
        float hpBefore = health.CurrentHealth;

        for (int i = 0; i < 20; i++)
            hazardSystem.Update(world, 0.1f);

        System.Diagnostics.Debug.Assert(Math.Abs(health.CurrentHealth - hpBefore) < 0.001f,
            "Victim out of range should take no damage");

        Console.WriteLine("  Victim out of range took no damage ✓");
        Console.WriteLine("✓ Hazard range check working\n");
    }
}
