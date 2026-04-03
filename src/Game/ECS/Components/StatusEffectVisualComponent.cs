namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of visual effects for status effects
/// </summary>
public enum StatusVisualType
{
    None,
    PoisonBubbles,
    FireAura,
    BleedDrops,
    IceCrystals,
    StunStars,
    HealingGlow,
    ShieldShimmer
}

/// <summary>
/// Component that adds visual indicators for active status effects on entities.
/// Creates visual feedback such as colored auras, floating icons, and pulsing effects.
/// </summary>
public class StatusEffectVisualComponent : IComponent
{
    /// <summary>
    /// Active visual effects and their timers
    /// </summary>
    public List<ActiveStatusVisual> ActiveVisuals { get; private set; } = new();

    /// <summary>
    /// Represents a single active status effect visual
    /// </summary>
    public class ActiveStatusVisual
    {
        public StatusVisualType VisualType { get; set; }
        public float Timer { get; set; }
        public float Duration { get; set; }
        public float Intensity { get; set; } = 1f;
        public float PulseSpeed { get; set; } = 3f;
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        /// <summary>
        /// Whether this visual is still active
        /// </summary>
        public bool IsActive => Duration <= 0 || Timer < Duration;

        /// <summary>
        /// Current pulse value (0 to 1, oscillating)
        /// </summary>
        public float PulseValue => (MathF.Sin(Timer * PulseSpeed) + 1f) * 0.5f;

        public ActiveStatusVisual(StatusVisualType type, float r, float g, float b, float duration = 0f)
        {
            VisualType = type;
            R = r;
            G = g;
            B = b;
            Duration = duration;
            Timer = 0f;
        }
    }

    /// <summary>
    /// Add a poison visual effect (green bubbles)
    /// </summary>
    public void AddPoisonVisual(float duration)
    {
        RemoveVisual(StatusVisualType.PoisonBubbles);
        ActiveVisuals.Add(new ActiveStatusVisual(StatusVisualType.PoisonBubbles, 0.2f, 0.8f, 0.1f, duration)
        {
            PulseSpeed = 4f,
            Intensity = 0.8f
        });
    }

    /// <summary>
    /// Add a fire aura visual effect (orange/red glow)
    /// </summary>
    public void AddFireVisual(float duration)
    {
        RemoveVisual(StatusVisualType.FireAura);
        ActiveVisuals.Add(new ActiveStatusVisual(StatusVisualType.FireAura, 1f, 0.4f, 0.1f, duration)
        {
            PulseSpeed = 6f,
            Intensity = 1f
        });
    }

    /// <summary>
    /// Add a bleed visual effect (red drops)
    /// </summary>
    public void AddBleedVisual(float duration)
    {
        RemoveVisual(StatusVisualType.BleedDrops);
        ActiveVisuals.Add(new ActiveStatusVisual(StatusVisualType.BleedDrops, 0.8f, 0.1f, 0.1f, duration)
        {
            PulseSpeed = 2f,
            Intensity = 0.7f
        });
    }

    /// <summary>
    /// Add an ice crystal visual effect (blue shimmer)
    /// </summary>
    public void AddFrozenVisual(float duration)
    {
        RemoveVisual(StatusVisualType.IceCrystals);
        ActiveVisuals.Add(new ActiveStatusVisual(StatusVisualType.IceCrystals, 0.4f, 0.7f, 1f, duration)
        {
            PulseSpeed = 2.5f,
            Intensity = 0.9f
        });
    }

    /// <summary>
    /// Add a stun visual effect (yellow stars)
    /// </summary>
    public void AddStunVisual(float duration)
    {
        RemoveVisual(StatusVisualType.StunStars);
        ActiveVisuals.Add(new ActiveStatusVisual(StatusVisualType.StunStars, 1f, 1f, 0.3f, duration)
        {
            PulseSpeed = 5f,
            Intensity = 1f
        });
    }

    /// <summary>
    /// Remove a visual effect by type
    /// </summary>
    public void RemoveVisual(StatusVisualType type)
    {
        ActiveVisuals.RemoveAll(v => v.VisualType == type);
    }

    /// <summary>
    /// Check if a visual type is active
    /// </summary>
    public bool HasVisual(StatusVisualType type)
    {
        return ActiveVisuals.Exists(v => v.VisualType == type && v.IsActive);
    }

    /// <summary>
    /// Clear all active visuals
    /// </summary>
    public void ClearAll()
    {
        ActiveVisuals.Clear();
    }
}
