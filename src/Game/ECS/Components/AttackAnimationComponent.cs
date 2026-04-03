namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of attack animations
/// </summary>
public enum AttackAnimationType
{
    None,
    MeleeSwing,
    MeleeThrust,
    BowDraw,
    SpellCast,
    ShieldBlock,
    Dodge
}

/// <summary>
/// Component that manages attack animation states for combat entities.
/// Tracks animation timing, arc direction, and visual feedback during attacks.
/// </summary>
public class AttackAnimationComponent : IComponent
{
    /// <summary>
    /// Current attack animation type
    /// </summary>
    public AttackAnimationType CurrentAttack { get; set; } = AttackAnimationType.None;

    /// <summary>
    /// Whether an attack animation is currently playing
    /// </summary>
    public bool IsPlaying { get; set; }

    /// <summary>
    /// Total duration of the current attack animation
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    /// Elapsed time in the current animation
    /// </summary>
    public float ElapsedTime { get; set; }

    /// <summary>
    /// Progress of the animation (0.0 to 1.0)
    /// </summary>
    public float Progress => Duration > 0 ? Math.Clamp(ElapsedTime / Duration, 0f, 1f) : 0f;

    /// <summary>
    /// Direction of the attack in radians
    /// </summary>
    public float AttackAngle { get; set; }

    /// <summary>
    /// Swing arc in radians (for melee swing animations)
    /// </summary>
    public float SwingArc { get; set; } = MathF.PI * 0.5f;

    /// <summary>
    /// Reach/range of the attack visualization
    /// </summary>
    public float AttackReach { get; set; } = 40f;

    /// <summary>
    /// Width of the attack arc visualization
    /// </summary>
    public float ArcWidth { get; set; } = 4f;

    /// <summary>
    /// Color of the attack visualization
    /// </summary>
    public float EffectR { get; set; } = 1f;
    public float EffectG { get; set; } = 1f;
    public float EffectB { get; set; } = 1f;
    public float EffectA { get; set; } = 0.8f;

    /// <summary>
    /// Whether to show a trailing afterimage effect
    /// </summary>
    public bool ShowTrail { get; set; } = true;

    /// <summary>
    /// Number of trail segments to render
    /// </summary>
    public int TrailSegments { get; set; } = 5;

    /// <summary>
    /// Start a melee swing attack animation
    /// </summary>
    public void PlayMeleeSwing(float angle, float duration = 0.25f)
    {
        CurrentAttack = AttackAnimationType.MeleeSwing;
        IsPlaying = true;
        Duration = duration;
        ElapsedTime = 0f;
        AttackAngle = angle;
        SwingArc = MathF.PI * 0.6f;
        EffectR = 1f; EffectG = 1f; EffectB = 1f;
    }

    /// <summary>
    /// Start a melee thrust attack animation
    /// </summary>
    public void PlayMeleeThrust(float angle, float duration = 0.2f)
    {
        CurrentAttack = AttackAnimationType.MeleeThrust;
        IsPlaying = true;
        Duration = duration;
        ElapsedTime = 0f;
        AttackAngle = angle;
        EffectR = 0.8f; EffectG = 0.9f; EffectB = 1f;
    }

    /// <summary>
    /// Start a bow draw animation
    /// </summary>
    public void PlayBowDraw(float angle, float duration = 0.4f)
    {
        CurrentAttack = AttackAnimationType.BowDraw;
        IsPlaying = true;
        Duration = duration;
        ElapsedTime = 0f;
        AttackAngle = angle;
        EffectR = 0.9f; EffectG = 0.7f; EffectB = 0.3f;
    }

    /// <summary>
    /// Start a spell cast animation
    /// </summary>
    public void PlaySpellCast(float angle, float duration = 0.5f)
    {
        CurrentAttack = AttackAnimationType.SpellCast;
        IsPlaying = true;
        Duration = duration;
        ElapsedTime = 0f;
        AttackAngle = angle;
        SwingArc = MathF.PI * 2f;
        EffectR = 0.3f; EffectG = 0.5f; EffectB = 1f;
    }

    /// <summary>
    /// Stop the current attack animation
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
        CurrentAttack = AttackAnimationType.None;
        ElapsedTime = 0f;
    }
}
