using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the Object Pool system
/// </summary>
public static class ObjectPoolSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Object Pool System Test");
        Console.WriteLine("=======================================\n");

        TestPoolInitialization();
        TestAcquireAndRelease();
        TestPoolExpansion();
        TestReleaseAll();
        TestMultiplePools();
        TestPoolStats();

        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Object Pool System Tests Passed");
        Console.WriteLine("=======================================\n");
    }

    // -----------------------------------------------------------------------

    private static void TestPoolInitialization()
    {
        Console.WriteLine("[Test] Pool Initialization");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var poolSystem = new ObjectPoolSystem();
        poolSystem.Initialize(world);

        System.Diagnostics.Debug.Assert(poolSystem.TotalPoolCount == 4,
            "Should have 4 built-in pools");
        System.Diagnostics.Debug.Assert(poolSystem.TotalAvailableEntities > 0,
            "Should have pre-allocated entities");
        System.Diagnostics.Debug.Assert(poolSystem.TotalActiveEntities == 0,
            "No entities should be active initially");

        Console.WriteLine($"  Pools: {poolSystem.TotalPoolCount}");
        Console.WriteLine($"  Available: {poolSystem.TotalAvailableEntities}");
        Console.WriteLine("✓ Pool initialization working\n");
    }

    private static void TestAcquireAndRelease()
    {
        Console.WriteLine("[Test] Acquire and Release");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var poolSystem = new ObjectPoolSystem();
        poolSystem.Initialize(world);

        int availableBefore = poolSystem.TotalAvailableEntities;

        var entity = poolSystem.Acquire(ObjectPoolSystem.PROJECTILE_POOL);
        System.Diagnostics.Debug.Assert(entity.HasValue, "Should acquire an entity");
        System.Diagnostics.Debug.Assert(poolSystem.TotalActiveEntities == 1,
            "One entity should be active");
        System.Diagnostics.Debug.Assert(poolSystem.TotalAvailableEntities == availableBefore - 1,
            "Available count should decrease by 1");

        // Verify the entity is marked active via PositionComponent
        var pos = world.GetComponent<PositionComponent>(entity!.Value);
        System.Diagnostics.Debug.Assert(pos != null && pos.IsActive, "Acquired entity should be active");

        poolSystem.Release(entity!.Value);
        System.Diagnostics.Debug.Assert(poolSystem.TotalActiveEntities == 0,
            "Active count should be zero after release");
        System.Diagnostics.Debug.Assert(poolSystem.TotalAvailableEntities == availableBefore,
            "Available count should restore after release");

        // Entity should be inactive again
        System.Diagnostics.Debug.Assert(!pos!.IsActive, "Released entity should be inactive");

        Console.WriteLine($"  Acquired and released entity {entity}.");
        Console.WriteLine("✓ Acquire/release cycle working\n");
    }

    private static void TestPoolExpansion()
    {
        Console.WriteLine("[Test] Pool Auto-Expansion");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var pool = new EntityPool(world, "TestPool", initialCapacity: 4);

        // Drain the pool
        var acquired = new List<Entity>();
        for (int i = 0; i < 4; i++)
        {
            var e = pool.Acquire();
            System.Diagnostics.Debug.Assert(e.HasValue, "Should acquire entity");
            acquired.Add(e!.Value);
        }

        int capacityBefore = pool.TotalCapacity;

        // One more acquire should trigger expansion
        var extra = pool.Acquire();
        System.Diagnostics.Debug.Assert(extra.HasValue, "Should acquire after expansion");
        System.Diagnostics.Debug.Assert(pool.TotalCapacity > capacityBefore,
            "Pool should have expanded");

        Console.WriteLine($"  Capacity before: {capacityBefore}, after expansion: {pool.TotalCapacity}");
        Console.WriteLine("✓ Pool auto-expansion working\n");
    }

    private static void TestReleaseAll()
    {
        Console.WriteLine("[Test] Release All");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var poolSystem = new ObjectPoolSystem();
        poolSystem.Initialize(world);

        // Acquire several entities
        var entities = new List<Entity>();
        for (int i = 0; i < 5; i++)
        {
            var e = poolSystem.Acquire(ObjectPoolSystem.PARTICLE_POOL);
            if (e.HasValue) entities.Add(e.Value);
        }

        System.Diagnostics.Debug.Assert(poolSystem.TotalActiveEntities == 5,
            "5 entities should be active");

        poolSystem.ReleaseAll();

        System.Diagnostics.Debug.Assert(poolSystem.TotalActiveEntities == 0,
            "All entities should be returned after ReleaseAll");

        Console.WriteLine("  Released 5 entities at once.");
        Console.WriteLine("✓ ReleaseAll working\n");
    }

    private static void TestMultiplePools()
    {
        Console.WriteLine("[Test] Multiple Named Pools");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var poolSystem = new ObjectPoolSystem();
        poolSystem.Initialize(world);

        var e1 = poolSystem.Acquire(ObjectPoolSystem.PROJECTILE_POOL);
        var e2 = poolSystem.Acquire(ObjectPoolSystem.PARTICLE_POOL);
        var e3 = poolSystem.Acquire(ObjectPoolSystem.LOOT_POOL);

        System.Diagnostics.Debug.Assert(e1.HasValue && e2.HasValue && e3.HasValue,
            "Should acquire from different pools");

        // Entities should be distinct
        System.Diagnostics.Debug.Assert(e1!.Value != e2!.Value, "Entities from different pools should differ");
        System.Diagnostics.Debug.Assert(e2!.Value != e3!.Value, "Entities from different pools should differ");

        Console.WriteLine($"  Acquired from 3 different pools: {e1}, {e2}, {e3}");

        // Release through global Release() which detects the owning pool
        poolSystem.Release(e1.Value);
        poolSystem.Release(e2.Value);
        poolSystem.Release(e3.Value);

        System.Diagnostics.Debug.Assert(poolSystem.TotalActiveEntities == 0,
            "All entities returned");

        Console.WriteLine("✓ Multiple pools working\n");
    }

    private static void TestPoolStats()
    {
        Console.WriteLine("[Test] Pool Statistics");
        Console.WriteLine("----------------------------------------");

        var world = new World();
        var poolSystem = new ObjectPoolSystem();
        poolSystem.Initialize(world);

        // Just ensure stats don't throw
        poolSystem.PrintStats();

        System.Diagnostics.Debug.Assert(poolSystem.TotalPoolCount > 0, "Should have pools");

        Console.WriteLine("✓ Pool statistics working\n");
    }
}
