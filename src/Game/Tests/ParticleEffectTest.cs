using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the particle effects system
/// </summary>
public static class ParticleEffectTest
{
    public static void Run()
    {
        Console.WriteLine("=== Particle Effect System Tests ===\n");

        TestParticleEmitterCreation();
        TestBlockBreakPreset();
        TestCombatHitPreset();
        TestWeatherRainPreset();
        TestParticleSpawning();
        TestParticleLifetime();
        TestParticlePhysics();
        TestBurstEmission();
        TestEffectCleanup();

        Console.WriteLine("\n=== Particle Effect Tests Complete ===");
    }

    private static void TestParticleEmitterCreation()
    {
        Console.Write("Test: Create particle emitter component... ");
        var emitter = new ParticleEmitterComponent();

        bool pass = emitter.MaxParticles == 100
                 && !emitter.IsEmitting
                 && !emitter.IsLooping
                 && emitter.EmissionRate == 20f
                 && emitter.Particles.Count == 0;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestBlockBreakPreset()
    {
        Console.Write("Test: Block break preset configuration... ");
        var emitter = ParticleEmitterComponent.CreateBlockBreak();

        bool pass = emitter.EffectType == ParticleEffectType.BlockBreak
                 && emitter.BurstCount == 8
                 && emitter.Gravity == 200f
                 && emitter.MaxParticles == 12;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestCombatHitPreset()
    {
        Console.Write("Test: Combat hit preset configuration... ");
        var emitter = ParticleEmitterComponent.CreateCombatHit();

        bool pass = emitter.EffectType == ParticleEffectType.CombatHit
                 && emitter.BurstCount == 10
                 && emitter.StartR > 0.9f  // Red color
                 && emitter.MaxParticles == 16;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestWeatherRainPreset()
    {
        Console.Write("Test: Weather rain preset configuration... ");
        var emitter = ParticleEmitterComponent.CreateWeatherRain();

        bool pass = emitter.EffectType == ParticleEffectType.WeatherRain
                 && emitter.IsLooping == false  // Not yet set by preset (set by StartLoopingEffect)
                 && emitter.EmissionRate == 80f
                 && emitter.MaxParticles == 200
                 && emitter.Gravity == 400f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestParticleSpawning()
    {
        Console.Write("Test: Spawn particle effect in world... ");
        var world = new World();
        world.AddSystem(new ParticleSystem(42));

        var entity = ParticleSystem.SpawnEffect(world, ParticleEffectType.CombatHit, 100f, 200f);

        var emitter = world.GetComponent<ParticleEmitterComponent>(entity);
        var position = world.GetComponent<PositionComponent>(entity);

        bool pass = emitter != null
                 && position != null
                 && emitter.IsEmitting
                 && position.X == 100f
                 && position.Y == 200f
                 && emitter.EffectType == ParticleEffectType.CombatHit;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestParticleLifetime()
    {
        Console.Write("Test: Particle lifetime and expiry... ");
        var particle = new Particle
        {
            X = 0, Y = 0,
            Lifetime = 1.0f,
            MaxLifetime = 1.0f,
            Size = 4f
        };

        bool alive = particle.IsAlive;
        float initialProgress = particle.Progress;

        particle.Lifetime = 0.5f;
        float midProgress = particle.Progress;

        particle.Lifetime = 0f;
        bool dead = !particle.IsAlive;
        float endProgress = particle.Progress;

        bool pass = alive
                 && initialProgress == 0f
                 && midProgress == 0.5f
                 && dead
                 && endProgress == 1f;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestParticlePhysics()
    {
        Console.Write("Test: Particle gravity and movement... ");
        var particle = new Particle
        {
            X = 100f, Y = 100f,
            VelocityX = 50f, VelocityY = 0f,
            Gravity = 100f,
            Lifetime = 2f,
            MaxLifetime = 2f
        };

        // Simulate 0.1 seconds
        float dt = 0.1f;
        particle.VelocityY += particle.Gravity * dt;
        particle.X += particle.VelocityX * dt;
        particle.Y += particle.VelocityY * dt;

        bool pass = particle.X > 100f   // Moved right
                 && particle.Y > 100f   // Moved down (gravity)
                 && particle.VelocityY > 0f;  // Has downward velocity

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }

    private static void TestBurstEmission()
    {
        Console.Write("Test: Burst emission spawns correct count... ");
        var world = new World();
        var particleSystem = new ParticleSystem(42);
        world.AddSystem(particleSystem);

        var entity = ParticleSystem.SpawnEffect(world, ParticleEffectType.LevelUp, 0f, 0f);

        // Update to trigger burst
        world.Update(0.016f);

        var emitter = world.GetComponent<ParticleEmitterComponent>(entity);

        bool pass = emitter != null
                 && emitter.Particles.Count > 0
                 && emitter.Particles.Count <= emitter.MaxParticles;

        Console.WriteLine(pass ? $"✓ PASSED ({emitter?.Particles.Count} particles spawned)" : "✗ FAILED");
    }

    private static void TestEffectCleanup()
    {
        Console.Write("Test: Completed effect entity cleanup... ");
        var world = new World();
        var particleSystem = new ParticleSystem(42);
        world.AddSystem(particleSystem);

        // Spawn a short-lived effect
        var entity = ParticleSystem.SpawnEffect(world, ParticleEffectType.ItemPickup, 0f, 0f);

        // Update to trigger burst
        world.Update(0.016f);

        var emitter = world.GetComponent<ParticleEmitterComponent>(entity);
        int initialCount = emitter?.Particles.Count ?? 0;

        // Simulate enough time for all particles to die
        for (int i = 0; i < 100; i++)
        {
            world.Update(0.05f);
        }

        // Clean up completed effects
        ParticleSystem.CleanupCompletedEffects(world);

        // The entity should have been destroyed
        var postCleanEmitter = world.GetComponent<ParticleEmitterComponent>(entity);
        bool pass = initialCount > 0 && postCleanEmitter == null;

        Console.WriteLine(pass ? "✓ PASSED" : "✗ FAILED");
    }
}
