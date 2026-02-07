using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test suite for the ranged combat / projectile system
/// </summary>
public static class RangedCombatTest
{
    public static void Run()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Ranged Combat System Tests");
        Console.WriteLine("===========================================\n");

        TestRangedComponentSetup();
        TestFireProjectile();
        TestProjectileMovement();
        TestProjectileCollision();
        TestProjectileLifetime();
        TestAmmoConsumption();
        TestStatusEffectOnHit();

        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Ranged Combat Tests Completed!");
        Console.WriteLine("===========================================\n");
    }

    private static void TestRangedComponentSetup()
    {
        Console.WriteLine("[Test] Ranged Combat Component Setup");

        var ranged = new RangedCombatComponent(
            type: ProjectileType.Arrow,
            damage: 10f,
            speed: 300f,
            cooldown: 1.0f
        );

        Console.WriteLine($"  Projectile type: {ranged.ProjectileType}");
        Console.WriteLine($"  Damage: {ranged.ProjectileDamage}");
        Console.WriteLine($"  Speed: {ranged.ProjectileSpeed}");
        Console.WriteLine($"  Ammo: {ranged.AmmoCount}");
        Console.WriteLine($"  Can attack: {ranged.CanAttack()}");

        if (ranged.ProjectileType == ProjectileType.Arrow &&
            ranged.ProjectileDamage == 10f &&
            ranged.CanAttack())
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestFireProjectile()
    {
        Console.WriteLine("[Test] Fire Projectile");

        var world = new World();
        var system = new ProjectileSystem();
        system.Initialize(world);

        // Create player
        var player = world.CreateEntity();
        world.AddComponent(player, new PositionComponent(100f, 100f));
        world.AddComponent(player, new PlayerComponent());
        var ranged = new RangedCombatComponent(ProjectileType.Arrow, 10f, 300f, 1.0f);
        world.AddComponent(player, ranged);

        // Fire toward (200, 100)
        var projectileEntity = ProjectileSystem.FireProjectile(world, player, 200f, 100f, ranged, true);

        bool fired = projectileEntity != null;
        Console.WriteLine($"  Projectile created: {fired}");

        if (fired)
        {
            var projComp = world.GetComponent<ProjectileComponent>(projectileEntity!.Value);
            Console.WriteLine($"  Direction: ({projComp?.DirectionX:F2}, {projComp?.DirectionY:F2})");
            Console.WriteLine($"  Cooldown reset: {ranged.TimeSinceLastRangedAttack == 0}");
        }

        if (fired && ranged.TimeSinceLastRangedAttack == 0)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestProjectileMovement()
    {
        Console.WriteLine("[Test] Projectile Movement");

        var world = new World();
        var system = new ProjectileSystem();
        system.Initialize(world);

        // Create a projectile directly
        var projEntity = world.CreateEntity();
        world.AddComponent(projEntity, new PositionComponent(0f, 0f));
        world.AddComponent(projEntity, new ProjectileComponent(
            ProjectileType.Arrow, 10f, 100f, 1f, 0f, 0, true, 5f
        ));

        // Update for 1 second
        system.Update(world, 1.0f);

        var pos = world.GetComponent<PositionComponent>(projEntity);
        Console.WriteLine($"  Position after 1s: ({pos?.X:F1}, {pos?.Y:F1})");
        Console.WriteLine($"  Expected: (100.0, 0.0)");

        if (pos != null && Math.Abs(pos.X - 100f) < 0.1f && Math.Abs(pos.Y) < 0.1f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestProjectileCollision()
    {
        Console.WriteLine("[Test] Projectile-Enemy Collision");

        var world = new World();
        var system = new ProjectileSystem();
        system.Initialize(world);

        // Create enemy at (50, 0)
        var enemy = world.CreateEntity();
        world.AddComponent(enemy, new PositionComponent(50f, 0f));
        world.AddComponent(enemy, new HealthComponent(100f));
        world.AddComponent(enemy, new ScriptComponent("enemy"));

        // Create projectile heading toward enemy
        var projEntity = world.CreateEntity();
        world.AddComponent(projEntity, new PositionComponent(20f, 0f));
        world.AddComponent(projEntity, new ProjectileComponent(
            ProjectileType.Arrow, 15f, 100f, 1f, 0f, 0, true, 5f
        ));

        // Update until collision (projectile travels ~30px in 0.3s)
        system.Update(world, 0.3f);

        var health = world.GetComponent<HealthComponent>(enemy);
        Console.WriteLine($"  Enemy health after hit: {health?.CurrentHealth}/{health?.MaxHealth}");

        if (health != null && health.CurrentHealth < 100f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestProjectileLifetime()
    {
        Console.WriteLine("[Test] Projectile Lifetime Expiry");

        var world = new World();
        var system = new ProjectileSystem();
        system.Initialize(world);

        // Create projectile with 2s lifetime
        var projEntity = world.CreateEntity();
        world.AddComponent(projEntity, new PositionComponent(0f, 0f));
        world.AddComponent(projEntity, new ProjectileComponent(
            ProjectileType.Rock, 5f, 50f, 1f, 0f, 0, true, 2f
        ));

        // Verify exists before expiry
        var proj = world.GetComponent<ProjectileComponent>(projEntity);
        bool existsBefore = proj != null;

        // Update for 3 seconds (past lifetime)
        system.Update(world, 3.0f);

        // Projectile should be destroyed
        var projAfter = world.GetComponent<ProjectileComponent>(projEntity);
        bool existsAfter = projAfter != null;

        Console.WriteLine($"  Exists before expiry: {existsBefore}");
        Console.WriteLine($"  Exists after expiry: {existsAfter}");

        if (existsBefore && !existsAfter)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestAmmoConsumption()
    {
        Console.WriteLine("[Test] Ammo Consumption");

        var ranged = new RangedCombatComponent(ProjectileType.Arrow, 10f, 300f, 0.5f);
        ranged.AmmoCount = 3;

        Console.WriteLine($"  Starting ammo: {ranged.AmmoCount}");

        bool shot1 = ranged.ConsumeAmmo();
        bool shot2 = ranged.ConsumeAmmo();
        bool shot3 = ranged.ConsumeAmmo();
        bool shot4 = ranged.ConsumeAmmo(); // Should fail

        Console.WriteLine($"  Shot 1: {shot1}, ammo: {ranged.AmmoCount}");
        Console.WriteLine($"  Shot 4 (empty): {shot4}");

        if (shot1 && shot2 && shot3 && !shot4 && ranged.AmmoCount == 0)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestStatusEffectOnHit()
    {
        Console.WriteLine("[Test] Projectile Status Effect on Hit");

        var world = new World();
        var system = new ProjectileSystem();
        system.Initialize(world);

        // Create enemy
        var enemy = world.CreateEntity();
        world.AddComponent(enemy, new PositionComponent(30f, 0f));
        world.AddComponent(enemy, new HealthComponent(100f));
        world.AddComponent(enemy, new ScriptComponent("enemy"));

        // Create fire bolt projectile (applies Burning)
        var projEntity = world.CreateEntity();
        world.AddComponent(projEntity, new PositionComponent(10f, 0f));
        world.AddComponent(projEntity, new ProjectileComponent(
            ProjectileType.FireBolt, 12f, 100f, 1f, 0f, 0, true, 5f
        ));

        // Move projectile to hit enemy
        system.Update(world, 0.2f);

        var effects = world.GetComponent<StatusEffectComponent>(enemy);
        bool hasBurning = effects?.HasEffect(StatusEffectType.Burning) ?? false;

        Console.WriteLine($"  Enemy has Burning effect: {hasBurning}");

        if (hasBurning)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
}
