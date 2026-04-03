using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages particle emission, updates, and rendering.
/// Handles burst and continuous emission modes with physics simulation.
/// </summary>
public class ParticleSystem : ISystem
{
    private Random _random;

    public ParticleSystem(int seed = 0)
    {
        _random = seed == 0 ? new Random() : new Random(seed);
    }

    public void Initialize(World world)
    {
        Console.WriteLine("[ParticleSystem] Initialized");
    }

    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<ParticleEmitterComponent>())
        {
            var emitter = world.GetComponent<ParticleEmitterComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);

            if (emitter == null || position == null)
                continue;

            // Emit new particles
            if (emitter.IsEmitting)
            {
                EmitParticles(emitter, position.X, position.Y, deltaTime);
            }

            // Update existing particles
            UpdateParticles(emitter, deltaTime);

            // Render particles
            RenderParticles(emitter);
        }
    }

    /// <summary>
    /// Emit new particles based on emitter configuration
    /// </summary>
    private void EmitParticles(ParticleEmitterComponent emitter, float x, float y, float deltaTime)
    {
        // Handle burst emission
        if (emitter.BurstCount > 0 && emitter.ElapsedTime == 0f)
        {
            int count = Math.Min(emitter.BurstCount, emitter.MaxParticles - emitter.Particles.Count);
            for (int i = 0; i < count; i++)
            {
                SpawnParticle(emitter, x, y);
            }
            emitter.ElapsedTime += deltaTime;

            if (!emitter.IsLooping)
            {
                emitter.IsEmitting = false;
            }
            return;
        }

        // Handle continuous emission
        if (emitter.EmissionRate > 0 && emitter.IsLooping)
        {
            emitter.EmissionTimer += deltaTime;
            float interval = 1f / emitter.EmissionRate;

            while (emitter.EmissionTimer >= interval && emitter.Particles.Count < emitter.MaxParticles)
            {
                SpawnParticle(emitter, x, y);
                emitter.EmissionTimer -= interval;
            }
        }

        emitter.ElapsedTime += deltaTime;

        // Check if burst duration has expired
        if (!emitter.IsLooping && emitter.BurstDuration > 0 && emitter.ElapsedTime >= emitter.BurstDuration)
        {
            emitter.IsEmitting = false;
        }
    }

    /// <summary>
    /// Spawn a single particle at the given position
    /// </summary>
    private void SpawnParticle(ParticleEmitterComponent emitter, float x, float y)
    {
        var particle = new Particle();

        // Position with spread
        float spreadX = (_random.NextSingle() * 2f - 1f) * emitter.SpreadRadius;
        float spreadY = (_random.NextSingle() * 2f - 1f) * emitter.SpreadRadius;
        particle.X = x + spreadX;
        particle.Y = y + spreadY;

        // Velocity from angle and speed
        float angle = Lerp(emitter.MinAngle, emitter.MaxAngle, _random.NextSingle());
        float speed = Lerp(emitter.MinSpeed, emitter.MaxSpeed, _random.NextSingle());
        particle.VelocityX = MathF.Cos(angle) * speed;
        particle.VelocityY = MathF.Sin(angle) * speed;

        // Lifetime
        particle.Lifetime = Lerp(emitter.MinLifetime, emitter.MaxLifetime, _random.NextSingle());
        particle.MaxLifetime = particle.Lifetime;

        // Size
        particle.Size = Lerp(emitter.MinSize, emitter.MaxSize, _random.NextSingle());

        // Rotation
        particle.Rotation = _random.NextSingle() * MathF.PI * 2f;
        particle.RotationSpeed = (_random.NextSingle() * 2f - 1f) * 5f;

        // Color (start color)
        particle.R = emitter.StartR;
        particle.G = emitter.StartG;
        particle.B = emitter.StartB;
        particle.A = 1f;

        // Physics
        particle.Gravity = emitter.Gravity;
        particle.FadeRate = 1f;
        particle.ShrinkRate = 0.3f;

        emitter.Particles.Add(particle);
    }

    /// <summary>
    /// Update all particles in an emitter (physics, aging, removal)
    /// </summary>
    private void UpdateParticles(ParticleEmitterComponent emitter, float deltaTime)
    {
        for (int i = emitter.Particles.Count - 1; i >= 0; i--)
        {
            var particle = emitter.Particles[i];

            // Update lifetime
            particle.Lifetime -= deltaTime;
            if (!particle.IsAlive)
            {
                emitter.Particles.RemoveAt(i);
                continue;
            }

            // Apply gravity
            particle.VelocityY += particle.Gravity * deltaTime;

            // Update position
            particle.X += particle.VelocityX * deltaTime;
            particle.Y += particle.VelocityY * deltaTime;

            // Update rotation
            particle.Rotation += particle.RotationSpeed * deltaTime;

            // Fade alpha over lifetime
            float progress = particle.Progress;
            particle.A = 1f - (progress * particle.FadeRate);
            if (particle.A < 0f) particle.A = 0f;

            // Shrink over lifetime
            if (particle.ShrinkRate > 0f)
            {
                float scale = 1f - (progress * particle.ShrinkRate);
                if (scale < 0.1f) scale = 0.1f;
                // Size is reduced but we keep min
                // Size is set at spawn and shrinks via rendering
            }

            // Interpolate color
            particle.R = Lerp(emitter.StartR, emitter.EndR, progress);
            particle.G = Lerp(emitter.StartG, emitter.EndG, progress);
            particle.B = Lerp(emitter.StartB, emitter.EndB, progress);
        }
    }

    /// <summary>
    /// Render all particles in an emitter
    /// </summary>
    private static void RenderParticles(ParticleEmitterComponent emitter)
    {
        foreach (var particle in emitter.Particles)
        {
            if (!particle.IsAlive) continue;

            float progress = particle.Progress;
            float scale = 1f - (progress * particle.ShrinkRate);
            if (scale < 0.1f) scale = 0.1f;
            float renderSize = particle.Size * scale;

            EngineInterop.Renderer_DrawRect(
                particle.X - renderSize * 0.5f,
                particle.Y - renderSize * 0.5f,
                renderSize,
                renderSize,
                particle.R,
                particle.G,
                particle.B,
                particle.A
            );
        }
    }

    /// <summary>
    /// Trigger a burst particle effect at a specific position
    /// </summary>
    public static Entity SpawnEffect(World world, ParticleEffectType effectType, float x, float y)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new PositionComponent(x, y));

        var emitter = effectType switch
        {
            ParticleEffectType.BlockBreak => ParticleEmitterComponent.CreateBlockBreak(),
            ParticleEffectType.CombatHit => ParticleEmitterComponent.CreateCombatHit(),
            ParticleEffectType.WeatherRain => ParticleEmitterComponent.CreateWeatherRain(),
            ParticleEffectType.WeatherSnow => ParticleEmitterComponent.CreateWeatherSnow(),
            ParticleEffectType.SpellEffect => ParticleEmitterComponent.CreateSpellEffect(),
            ParticleEffectType.LevelUp => ParticleEmitterComponent.CreateLevelUp(),
            ParticleEffectType.ItemPickup => ParticleEmitterComponent.CreateItemPickup(),
            ParticleEffectType.Healing => ParticleEmitterComponent.CreateHealing(),
            _ => new ParticleEmitterComponent()
        };

        emitter.IsEmitting = true;
        world.AddComponent(entity, emitter);

        Console.WriteLine($"[ParticleSystem] Spawned {effectType} effect at ({x:F0}, {y:F0})");
        return entity;
    }

    /// <summary>
    /// Trigger a looping particle effect (e.g., weather) attached to an entity
    /// </summary>
    public static void StartLoopingEffect(World world, Entity entity, ParticleEffectType effectType)
    {
        var emitter = effectType switch
        {
            ParticleEffectType.WeatherRain => ParticleEmitterComponent.CreateWeatherRain(),
            ParticleEffectType.WeatherSnow => ParticleEmitterComponent.CreateWeatherSnow(),
            _ => new ParticleEmitterComponent { IsLooping = true }
        };

        emitter.IsEmitting = true;
        emitter.IsLooping = true;
        world.AddComponent(entity, emitter);
    }

    /// <summary>
    /// Stop a looping particle effect on an entity
    /// </summary>
    public static void StopEffect(World world, Entity entity)
    {
        var emitter = world.GetComponent<ParticleEmitterComponent>(entity);
        if (emitter != null)
        {
            emitter.IsEmitting = false;
        }
    }

    /// <summary>
    /// Clean up completed burst effects (entities with no remaining particles)
    /// </summary>
    public static void CleanupCompletedEffects(World world)
    {
        var toRemove = new List<Entity>();

        foreach (var entity in world.GetEntitiesWithComponent<ParticleEmitterComponent>())
        {
            var emitter = world.GetComponent<ParticleEmitterComponent>(entity);
            if (emitter != null && !emitter.IsEmitting && !emitter.IsLooping && emitter.Particles.Count == 0)
            {
                toRemove.Add(entity);
            }
        }

        foreach (var entity in toRemove)
        {
            world.DestroyEntity(entity);
        }
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
