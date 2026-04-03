namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of environmental animations
/// </summary>
public enum EnvironmentalAnimationType
{
    None,
    WaterRipple,
    LavaFlow,
    TorchFlicker,
    GrassWave,
    LeafFall,
    Sparkle,
    Steam,
    Smoke
}

/// <summary>
/// Component for environmental animations that add visual life to the world.
/// Applied to terrain tiles, decoration entities, and environmental effects.
/// </summary>
public class EnvironmentalAnimationComponent : IComponent
{
    /// <summary>
    /// Type of environmental animation
    /// </summary>
    public EnvironmentalAnimationType AnimType { get; set; } = EnvironmentalAnimationType.None;

    /// <summary>
    /// Animation speed multiplier
    /// </summary>
    public float Speed { get; set; } = 1f;

    /// <summary>
    /// Amplitude of the animation effect (e.g., wave height, flicker intensity)
    /// </summary>
    public float Amplitude { get; set; } = 1f;

    /// <summary>
    /// Phase offset for variation between instances
    /// </summary>
    public float PhaseOffset { get; set; }

    /// <summary>
    /// Time accumulator for animation progress
    /// </summary>
    public float Timer { get; set; }

    /// <summary>
    /// Current visual offset X (computed each frame)
    /// </summary>
    public float OffsetX { get; set; }

    /// <summary>
    /// Current visual offset Y (computed each frame)
    /// </summary>
    public float OffsetY { get; set; }

    /// <summary>
    /// Current scale modifier (1.0 = normal)
    /// </summary>
    public float ScaleModifier { get; set; } = 1f;

    /// <summary>
    /// Current alpha modifier (1.0 = fully opaque)
    /// </summary>
    public float AlphaModifier { get; set; } = 1f;

    /// <summary>
    /// Current color tint modifiers
    /// </summary>
    public float TintR { get; set; } = 1f;
    public float TintG { get; set; } = 1f;
    public float TintB { get; set; } = 1f;

    /// <summary>
    /// Whether this animation is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creates a water ripple animation
    /// </summary>
    public static EnvironmentalAnimationComponent CreateWaterRipple(float phaseOffset = 0f)
    {
        return new EnvironmentalAnimationComponent
        {
            AnimType = EnvironmentalAnimationType.WaterRipple,
            Speed = 2f,
            Amplitude = 2f,
            PhaseOffset = phaseOffset,
            TintR = 0.4f, TintG = 0.6f, TintB = 0.9f
        };
    }

    /// <summary>
    /// Creates a lava flow animation
    /// </summary>
    public static EnvironmentalAnimationComponent CreateLavaFlow(float phaseOffset = 0f)
    {
        return new EnvironmentalAnimationComponent
        {
            AnimType = EnvironmentalAnimationType.LavaFlow,
            Speed = 0.5f,
            Amplitude = 1.5f,
            PhaseOffset = phaseOffset,
            TintR = 1f, TintG = 0.4f, TintB = 0.1f
        };
    }

    /// <summary>
    /// Creates a torch flicker animation
    /// </summary>
    public static EnvironmentalAnimationComponent CreateTorchFlicker(float phaseOffset = 0f)
    {
        return new EnvironmentalAnimationComponent
        {
            AnimType = EnvironmentalAnimationType.TorchFlicker,
            Speed = 8f,
            Amplitude = 0.3f,
            PhaseOffset = phaseOffset,
            TintR = 1f, TintG = 0.8f, TintB = 0.3f
        };
    }

    /// <summary>
    /// Creates a grass wave animation
    /// </summary>
    public static EnvironmentalAnimationComponent CreateGrassWave(float phaseOffset = 0f)
    {
        return new EnvironmentalAnimationComponent
        {
            AnimType = EnvironmentalAnimationType.GrassWave,
            Speed = 1.5f,
            Amplitude = 3f,
            PhaseOffset = phaseOffset,
            TintR = 0.4f, TintG = 0.8f, TintB = 0.3f
        };
    }

    /// <summary>
    /// Creates a leaf fall animation
    /// </summary>
    public static EnvironmentalAnimationComponent CreateLeafFall(float phaseOffset = 0f)
    {
        return new EnvironmentalAnimationComponent
        {
            AnimType = EnvironmentalAnimationType.LeafFall,
            Speed = 1f,
            Amplitude = 4f,
            PhaseOffset = phaseOffset,
            TintR = 0.6f, TintG = 0.8f, TintB = 0.2f
        };
    }

    /// <summary>
    /// Creates a sparkle effect animation
    /// </summary>
    public static EnvironmentalAnimationComponent CreateSparkle(float phaseOffset = 0f)
    {
        return new EnvironmentalAnimationComponent
        {
            AnimType = EnvironmentalAnimationType.Sparkle,
            Speed = 3f,
            Amplitude = 1f,
            PhaseOffset = phaseOffset,
            TintR = 1f, TintG = 1f, TintB = 0.8f
        };
    }
}
