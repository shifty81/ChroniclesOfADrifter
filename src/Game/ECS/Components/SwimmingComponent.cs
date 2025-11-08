namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that indicates an entity can swim in water
/// </summary>
public class SwimmingComponent : IComponent
{
    /// <summary>
    /// Whether the entity is currently in water
    /// </summary>
    public bool IsInWater { get; set; }
    
    /// <summary>
    /// Swimming speed multiplier (1.0 = normal speed, 0.5 = half speed)
    /// </summary>
    public float SwimSpeed { get; set; } = 0.7f;
    
    /// <summary>
    /// How long the entity can stay underwater before needing air (in seconds)
    /// 0 = infinite (can breathe underwater)
    /// </summary>
    public float MaxBreathTime { get; set; } = 10.0f;
    
    /// <summary>
    /// Current remaining breath time
    /// </summary>
    public float CurrentBreath { get; set; }
    
    /// <summary>
    /// Damage taken per second when out of breath underwater
    /// </summary>
    public float DrowningDamage { get; set; } = 2.0f;
    
    /// <summary>
    /// Whether this entity can breathe underwater
    /// </summary>
    public bool CanBreatheUnderwater { get; set; } = false;
    
    public SwimmingComponent(float swimSpeed = 0.7f, float maxBreathTime = 10.0f)
    {
        SwimSpeed = swimSpeed;
        MaxBreathTime = maxBreathTime;
        CurrentBreath = maxBreathTime;
        IsInWater = false;
    }
}
