using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Ability system
/// </summary>
public static class AbilitySystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Ability System Test");
        Console.WriteLine("=======================================\n");

        TestAbilityRegistration();
        TestEnergyRegeneration();
        TestAbilityUnlockAndUse();
        TestCooldownEnforcement();
        TestAbilityEffects();
        TestDisplayAbilities();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Ability System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static (World world, Entity player, AbilitySystem system) SetupPlayer()
    {
        var world = new World();
        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(100f, 100f));
        world.AddComponent(player, new VelocityComponent(0f, 0f));
        world.AddComponent(player, new HealthComponent(100f));
        world.AddComponent(player, new PlayerComponent());
        world.AddComponent(player, new AbilityComponent(100));

        var system = new AbilitySystem();
        system.Initialize(world);

        return (world, player, system);
    }

    private static void TestAbilityRegistration()
    {
        Console.WriteLine("[Test] Ability Registration (seed defaults)");
        Console.WriteLine("----------------------------------------");

        var (world, player, _) = SetupPlayer();
        var abilityComp = world.GetComponent<AbilityComponent>(player)!;

        var all = abilityComp.GetAllAbilities().ToList();
        System.Diagnostics.Debug.Assert(all.Count > 0, "Should have seeded default abilities");

        Console.WriteLine($"  Registered {all.Count} abilities:");
        foreach (var a in all)
            Console.WriteLine($"    {a.Name,-20} unlocked={a.IsUnlocked}  energy={a.EnergyCost}  cd={a.Cooldown:F1}s");

        // Dash should be present
        System.Diagnostics.Debug.Assert(all.Any(a => a.Type == AbilityType.Dash), "Dash should be registered");
        System.Diagnostics.Debug.Assert(all.Any(a => a.Type == AbilityType.MagicBolt), "MagicBolt should be registered");

        Console.WriteLine("✓ Ability registration working\n");
    }

    private static void TestEnergyRegeneration()
    {
        Console.WriteLine("[Test] Energy Regeneration");
        Console.WriteLine("----------------------------------------");

        var (world, player, system) = SetupPlayer();
        var abilityComp = world.GetComponent<AbilityComponent>(player)!;

        // Drain energy manually
        abilityComp.CurrentEnergy = 0;
        int before = abilityComp.CurrentEnergy;

        // Update for 3 seconds (10 energy/sec → 30 energy)
        system.Update(world, 3f);

        System.Diagnostics.Debug.Assert(abilityComp.CurrentEnergy > before,
            "Energy should have regenerated");
        System.Diagnostics.Debug.Assert(abilityComp.CurrentEnergy <= abilityComp.MaxEnergy,
            "Energy should not exceed max");

        Console.WriteLine($"  Energy after 3s: {abilityComp.CurrentEnergy}/{abilityComp.MaxEnergy}");
        Console.WriteLine("✓ Energy regeneration working\n");
    }

    private static void TestAbilityUnlockAndUse()
    {
        Console.WriteLine("[Test] Ability Unlock and Use");
        Console.WriteLine("----------------------------------------");

        var (world, player, _) = SetupPlayer();
        var abilityComp = world.GetComponent<AbilityComponent>(player)!;

        // Not unlocked initially
        System.Diagnostics.Debug.Assert(!abilityComp.HasAbility(AbilityType.Dash),
            "Dash should not be unlocked initially");

        AbilitySystem.UnlockAbility(world, player, AbilityType.Dash);
        System.Diagnostics.Debug.Assert(abilityComp.HasAbility(AbilityType.Dash),
            "Dash should be unlocked now");

        bool used = AbilitySystem.ActivateAbility(world, player, AbilityType.Dash, 0f);
        System.Diagnostics.Debug.Assert(used, "Should be able to use Dash after unlocking");

        Console.WriteLine("  Dash unlocked and used successfully.");
        Console.WriteLine("✓ Ability unlock and use working\n");
    }

    private static void TestCooldownEnforcement()
    {
        Console.WriteLine("[Test] Cooldown Enforcement");
        Console.WriteLine("----------------------------------------");

        var (world, player, _) = SetupPlayer();

        AbilitySystem.UnlockAbility(world, player, AbilityType.SwordSpin);

        bool first  = AbilitySystem.ActivateAbility(world, player, AbilityType.SwordSpin, 0f);
        bool second = AbilitySystem.ActivateAbility(world, player, AbilityType.SwordSpin, 0f); // same time

        System.Diagnostics.Debug.Assert(first, "First use should succeed");
        System.Diagnostics.Debug.Assert(!second, "Second use at same time should fail (cooldown)");

        // After cooldown (5 seconds), should work again
        bool third = AbilitySystem.ActivateAbility(world, player, AbilityType.SwordSpin, 6f);
        System.Diagnostics.Debug.Assert(third, "Should work after cooldown expires");

        Console.WriteLine("  Cooldown enforced correctly.");
        Console.WriteLine("✓ Cooldown enforcement working\n");
    }

    private static void TestAbilityEffects()
    {
        Console.WriteLine("[Test] Ability Effects");
        Console.WriteLine("----------------------------------------");

        var (world, player, _) = SetupPlayer();
        var velocity = world.GetComponent<VelocityComponent>(player)!;

        AbilitySystem.UnlockAbility(world, player, AbilityType.Dash);
        float speedBefore = MathF.Sqrt(velocity.VX * velocity.VX + velocity.VY * velocity.VY);

        AbilitySystem.ActivateAbility(world, player, AbilityType.Dash, 0f);

        float speedAfter = MathF.Sqrt(velocity.VX * velocity.VX + velocity.VY * velocity.VY);
        System.Diagnostics.Debug.Assert(speedAfter > speedBefore, "Dash should increase velocity");
        Console.WriteLine($"  Speed before dash: {speedBefore:F1}, after: {speedAfter:F1}");

        // TorchLight test – creates or toggles LightSourceComponent
        AbilitySystem.UnlockAbility(world, player, AbilityType.TorchLight);
        AbilitySystem.ActivateAbility(world, player, AbilityType.TorchLight, 1f);

        var light = world.GetComponent<LightSourceComponent>(player);
        System.Diagnostics.Debug.Assert(light != null, "TorchLight should add LightSourceComponent");
        System.Diagnostics.Debug.Assert(light.Radius > 0, "Torch should have positive radius when on");

        Console.WriteLine($"  Torch light radius: {light.Radius}");
        Console.WriteLine("✓ Ability effects working\n");
    }

    private static void TestDisplayAbilities()
    {
        Console.WriteLine("[Test] Display Abilities");
        Console.WriteLine("----------------------------------------");

        var (world, player, _) = SetupPlayer();

        AbilitySystem.UnlockAbility(world, player, AbilityType.Dash);
        AbilitySystem.UnlockAbility(world, player, AbilityType.MagicBolt);

        // Should not throw
        AbilitySystem.DisplayAbilities(world, player);

        Console.WriteLine("✓ Display abilities working\n");
    }
}
