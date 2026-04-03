namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of particle effects
/// </summary>
public enum ParticleEffectType
{
    BlockBreak,
    CombatHit,
    WeatherRain,
    WeatherSnow,
    SpellEffect,
    LevelUp,
    ItemPickup,
    Dust,
    Sparks,
    Healing
}

/// <summary>
/// Represents a single particle in the system
/// </summary>
public class Particle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float Lifetime { get; set; }
    public float MaxLifetime { get; set; }
    public float Size { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }
    public float Gravity { get; set; }
    public float FadeRate { get; set; }
    public float ShrinkRate { get; set; }

    /// <summary>
    /// Whether this particle is still alive
    /// </summary>
    public bool IsAlive => Lifetime > 0f;

    /// <summary>
    /// Progress from 0 (just spawned) to 1 (about to die)
    /// </summary>
    public float Progress => 1f - (Lifetime / MaxLifetime);

    public Particle()
    {
        A = 1f;
        FadeRate = 1f;
        ShrinkRate = 0f;
    }
}

/// <summary>
/// Component that enables an entity to emit particles
/// </summary>
public class ParticleEmitterComponent : IComponent
{
    /// <summary>
    /// Type of particle effect this emitter produces
    /// </summary>
    public ParticleEffectType EffectType { get; set; }

    /// <summary>
    /// Active particles managed by this emitter
    /// </summary>
    public List<Particle> Particles { get; private set; }

    /// <summary>
    /// Maximum number of particles this emitter can have active
    /// </summary>
    public int MaxParticles { get; set; }

    /// <summary>
    /// Whether this emitter is currently active and spawning
    /// </summary>
    public bool IsEmitting { get; set; }

    /// <summary>
    /// Whether this emitter loops continuously
    /// </summary>
    public bool IsLooping { get; set; }

    /// <summary>
    /// Particles spawned per second when emitting
    /// </summary>
    public float EmissionRate { get; set; }

    /// <summary>
    /// Time accumulator for emission timing
    /// </summary>
    public float EmissionTimer { get; set; }

    /// <summary>
    /// Particle lifetime range (min)
    /// </summary>
    public float MinLifetime { get; set; }

    /// <summary>
    /// Particle lifetime range (max)
    /// </summary>
    public float MaxLifetime { get; set; }

    /// <summary>
    /// Minimum initial speed of particles
    /// </summary>
    public float MinSpeed { get; set; }

    /// <summary>
    /// Maximum initial speed of particles
    /// </summary>
    public float MaxSpeed { get; set; }

    /// <summary>
    /// Minimum particle size
    /// </summary>
    public float MinSize { get; set; }

    /// <summary>
    /// Maximum particle size
    /// </summary>
    public float MaxSize { get; set; }

    /// <summary>
    /// Gravity applied to particles (positive = downward)
    /// </summary>
    public float Gravity { get; set; }

    /// <summary>
    /// Color range for particles (start color)
    /// </summary>
    public float StartR { get; set; }
    public float StartG { get; set; }
    public float StartB { get; set; }

    /// <summary>
    /// Color range for particles (end color)
    /// </summary>
    public float EndR { get; set; }
    public float EndG { get; set; }
    public float EndB { get; set; }

    /// <summary>
    /// Emission angle range in radians (for directional emission)
    /// </summary>
    public float MinAngle { get; set; }
    public float MaxAngle { get; set; }

    /// <summary>
    /// Spread radius around emitter position
    /// </summary>
    public float SpreadRadius { get; set; }

    /// <summary>
    /// Duration for burst effects (non-looping). 0 = single burst
    /// </summary>
    public float BurstDuration { get; set; }

    /// <summary>
    /// Elapsed time for burst effects
    /// </summary>
    public float ElapsedTime { get; set; }

    /// <summary>
    /// Number of particles to emit in a single burst
    /// </summary>
    public int BurstCount { get; set; }

    public ParticleEmitterComponent()
    {
        Particles = new List<Particle>();
        MaxParticles = 100;
        IsEmitting = false;
        IsLooping = false;
        EmissionRate = 20f;
        EmissionTimer = 0f;
        MinLifetime = 0.5f;
        MaxLifetime = 1.5f;
        MinSpeed = 20f;
        MaxSpeed = 80f;
        MinSize = 2f;
        MaxSize = 6f;
        Gravity = 0f;
        StartR = 1f; StartG = 1f; StartB = 1f;
        EndR = 1f; EndG = 1f; EndB = 1f;
        MinAngle = 0f;
        MaxAngle = MathF.PI * 2f;
        SpreadRadius = 0f;
        BurstDuration = 0f;
        ElapsedTime = 0f;
        BurstCount = 0;
    }

    /// <summary>
    /// Creates a block break particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateBlockBreak()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.BlockBreak,
            MaxParticles = 12,
            BurstCount = 8,
            MinLifetime = 0.3f,
            MaxLifetime = 0.8f,
            MinSpeed = 40f,
            MaxSpeed = 120f,
            MinSize = 2f,
            MaxSize = 5f,
            Gravity = 200f,
            StartR = 0.6f, StartG = 0.5f, StartB = 0.3f,
            EndR = 0.4f, EndG = 0.3f, EndB = 0.2f,
            MinAngle = -MathF.PI,
            MaxAngle = 0f
        };
    }

    /// <summary>
    /// Creates a combat hit particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateCombatHit()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.CombatHit,
            MaxParticles = 16,
            BurstCount = 10,
            MinLifetime = 0.2f,
            MaxLifetime = 0.5f,
            MinSpeed = 60f,
            MaxSpeed = 150f,
            MinSize = 1f,
            MaxSize = 4f,
            Gravity = 50f,
            StartR = 1.0f, StartG = 0.2f, StartB = 0.1f,
            EndR = 0.8f, EndG = 0.1f, EndB = 0.0f
        };
    }

    /// <summary>
    /// Creates a rain weather particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateWeatherRain()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.WeatherRain,
            MaxParticles = 200,
            IsLooping = true,
            EmissionRate = 80f,
            MinLifetime = 0.5f,
            MaxLifetime = 1.0f,
            MinSpeed = 200f,
            MaxSpeed = 300f,
            MinSize = 1f,
            MaxSize = 3f,
            Gravity = 400f,
            StartR = 0.5f, StartG = 0.6f, StartB = 0.8f,
            EndR = 0.3f, EndG = 0.4f, EndB = 0.7f,
            MinAngle = MathF.PI * 0.4f,
            MaxAngle = MathF.PI * 0.6f,
            SpreadRadius = 400f
        };
    }

    /// <summary>
    /// Creates a snow weather particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateWeatherSnow()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.WeatherSnow,
            MaxParticles = 150,
            IsLooping = true,
            EmissionRate = 40f,
            MinLifetime = 2.0f,
            MaxLifetime = 4.0f,
            MinSpeed = 10f,
            MaxSpeed = 30f,
            MinSize = 2f,
            MaxSize = 5f,
            Gravity = 30f,
            StartR = 0.9f, StartG = 0.95f, StartB = 1.0f,
            EndR = 0.7f, EndG = 0.75f, EndB = 0.8f,
            SpreadRadius = 400f
        };
    }

    /// <summary>
    /// Creates a spell effect particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateSpellEffect()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.SpellEffect,
            MaxParticles = 30,
            BurstCount = 15,
            MinLifetime = 0.5f,
            MaxLifetime = 1.2f,
            MinSpeed = 30f,
            MaxSpeed = 80f,
            MinSize = 2f,
            MaxSize = 6f,
            Gravity = -20f,
            StartR = 0.3f, StartG = 0.5f, StartB = 1.0f,
            EndR = 0.1f, EndG = 0.2f, EndB = 0.8f
        };
    }

    /// <summary>
    /// Creates a level up particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateLevelUp()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.LevelUp,
            MaxParticles = 40,
            BurstCount = 30,
            MinLifetime = 0.8f,
            MaxLifetime = 2.0f,
            MinSpeed = 40f,
            MaxSpeed = 100f,
            MinSize = 3f,
            MaxSize = 7f,
            Gravity = -40f,
            StartR = 1.0f, StartG = 0.9f, StartB = 0.2f,
            EndR = 1.0f, EndG = 1.0f, EndB = 0.6f,
            MinAngle = -MathF.PI,
            MaxAngle = 0f
        };
    }

    /// <summary>
    /// Creates an item pickup particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateItemPickup()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.ItemPickup,
            MaxParticles = 8,
            BurstCount = 6,
            MinLifetime = 0.3f,
            MaxLifetime = 0.6f,
            MinSpeed = 20f,
            MaxSpeed = 50f,
            MinSize = 2f,
            MaxSize = 4f,
            Gravity = -30f,
            StartR = 0.2f, StartG = 1.0f, StartB = 0.3f,
            EndR = 0.1f, EndG = 0.8f, EndB = 0.2f,
            MinAngle = -MathF.PI,
            MaxAngle = 0f
        };
    }

    /// <summary>
    /// Creates a healing particle emitter preset
    /// </summary>
    public static ParticleEmitterComponent CreateHealing()
    {
        return new ParticleEmitterComponent
        {
            EffectType = ParticleEffectType.Healing,
            MaxParticles = 20,
            BurstCount = 12,
            MinLifetime = 0.6f,
            MaxLifetime = 1.5f,
            MinSpeed = 15f,
            MaxSpeed = 40f,
            MinSize = 3f,
            MaxSize = 6f,
            Gravity = -50f,
            StartR = 0.2f, StartG = 1.0f, StartB = 0.5f,
            EndR = 0.4f, EndG = 1.0f, EndB = 0.7f,
            MinAngle = -MathF.PI * 0.75f,
            MaxAngle = -MathF.PI * 0.25f
        };
    }
}
