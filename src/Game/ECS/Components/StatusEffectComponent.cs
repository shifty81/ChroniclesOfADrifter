namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of status effects that can be applied to entities
/// </summary>
public enum StatusEffectType
{
    Poison,     // Damage over time (nature/swamp)
    Burning,    // Fire damage over time
    Bleeding,   // Physical damage over time
    Frozen,     // Slows movement speed
    Stunned     // Prevents movement and attacks
}

/// <summary>
/// Individual status effect instance
/// </summary>
public class StatusEffect
{
    public StatusEffectType Type { get; set; }
    public float DamagePerSecond { get; set; }
    public float Duration { get; set; }
    public float RemainingTime { get; set; }
    public float SpeedMultiplier { get; set; }
    public bool PreventsActions { get; set; }

    public StatusEffect(StatusEffectType type, float duration, float damagePerSecond = 0f)
    {
        Type = type;
        Duration = duration;
        RemainingTime = duration;
        DamagePerSecond = damagePerSecond;
        SpeedMultiplier = 1.0f;
        PreventsActions = false;

        // Set type-specific defaults
        switch (type)
        {
            case StatusEffectType.Poison:
                DamagePerSecond = damagePerSecond > 0 ? damagePerSecond : 2f;
                break;
            case StatusEffectType.Burning:
                DamagePerSecond = damagePerSecond > 0 ? damagePerSecond : 3f;
                break;
            case StatusEffectType.Bleeding:
                DamagePerSecond = damagePerSecond > 0 ? damagePerSecond : 1.5f;
                break;
            case StatusEffectType.Frozen:
                SpeedMultiplier = 0.3f;
                break;
            case StatusEffectType.Stunned:
                SpeedMultiplier = 0f;
                PreventsActions = true;
                break;
        }
    }

    /// <summary>
    /// Check if the effect has expired
    /// </summary>
    public bool IsExpired => RemainingTime <= 0;
}

/// <summary>
/// Component tracking active status effects on an entity
/// </summary>
public class StatusEffectComponent : IComponent
{
    public List<StatusEffect> ActiveEffects { get; set; }

    public StatusEffectComponent()
    {
        ActiveEffects = new List<StatusEffect>();
    }

    /// <summary>
    /// Apply a new status effect (replaces existing of same type if longer)
    /// </summary>
    public void ApplyEffect(StatusEffect effect)
    {
        // Replace if same type exists with shorter remaining time
        var existing = ActiveEffects.Find(e => e.Type == effect.Type);
        if (existing != null)
        {
            if (effect.RemainingTime > existing.RemainingTime)
            {
                ActiveEffects.Remove(existing);
                ActiveEffects.Add(effect);
            }
        }
        else
        {
            ActiveEffects.Add(effect);
        }
    }

    /// <summary>
    /// Remove all effects of a specific type
    /// </summary>
    public void RemoveEffect(StatusEffectType type)
    {
        ActiveEffects.RemoveAll(e => e.Type == type);
    }

    /// <summary>
    /// Check if entity has a specific effect type
    /// </summary>
    public bool HasEffect(StatusEffectType type)
    {
        return ActiveEffects.Any(e => e.Type == type);
    }

    /// <summary>
    /// Get the combined speed multiplier from all active effects
    /// </summary>
    public float GetSpeedMultiplier()
    {
        float multiplier = 1.0f;
        foreach (var effect in ActiveEffects)
        {
            multiplier *= effect.SpeedMultiplier;
        }
        return multiplier;
    }

    /// <summary>
    /// Check if any effect prevents actions
    /// </summary>
    public bool IsIncapacitated()
    {
        return ActiveEffects.Any(e => e.PreventsActions);
    }
}
