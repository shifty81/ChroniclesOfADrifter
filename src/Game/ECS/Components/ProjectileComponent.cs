namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Type of projectile
/// </summary>
public enum ProjectileType
{
    Arrow,
    FireBolt,
    IceShard,
    PoisonDart,
    Rock
}

/// <summary>
/// Component for projectile entities
/// </summary>
public class ProjectileComponent : IComponent
{
    public ProjectileType Type { get; set; }
    public float Damage { get; set; }
    public float Speed { get; set; }
    public float DirectionX { get; set; }
    public float DirectionY { get; set; }
    public float Lifetime { get; set; }
    public float RemainingLifetime { get; set; }
    public float Range { get; set; }
    public int OwnerId { get; set; }
    public bool IsPlayerOwned { get; set; }
    public StatusEffectType? AppliesEffect { get; set; }
    public float EffectDuration { get; set; }

    public ProjectileComponent(
        ProjectileType type,
        float damage,
        float speed,
        float dirX,
        float dirY,
        int ownerId,
        bool isPlayerOwned,
        float lifetime = 3f)
    {
        Type = type;
        Damage = damage;
        Speed = speed;
        DirectionX = dirX;
        DirectionY = dirY;
        OwnerId = ownerId;
        IsPlayerOwned = isPlayerOwned;
        Lifetime = lifetime;
        RemainingLifetime = lifetime;
        Range = speed * lifetime;
        AppliesEffect = null;
        EffectDuration = 0f;

        // Set type-specific effects
        switch (type)
        {
            case ProjectileType.FireBolt:
                AppliesEffect = StatusEffectType.Burning;
                EffectDuration = 3f;
                break;
            case ProjectileType.IceShard:
                AppliesEffect = StatusEffectType.Frozen;
                EffectDuration = 2f;
                break;
            case ProjectileType.PoisonDart:
                AppliesEffect = StatusEffectType.Poison;
                EffectDuration = 5f;
                break;
        }
    }

    /// <summary>
    /// Check if the projectile has expired
    /// </summary>
    public bool IsExpired => RemainingLifetime <= 0;
}

/// <summary>
/// Component for entities capable of ranged attacks
/// </summary>
public class RangedCombatComponent : IComponent
{
    public ProjectileType ProjectileType { get; set; }
    public float ProjectileDamage { get; set; }
    public float ProjectileSpeed { get; set; }
    public float AttackCooldown { get; set; }
    public float TimeSinceLastRangedAttack { get; set; }
    public float ProjectileLifetime { get; set; }
    public int AmmoCount { get; set; }
    public bool InfiniteAmmo { get; set; }

    public RangedCombatComponent(
        ProjectileType type = ProjectileType.Arrow,
        float damage = 8f,
        float speed = 300f,
        float cooldown = 1.0f,
        float lifetime = 3f)
    {
        ProjectileType = type;
        ProjectileDamage = damage;
        ProjectileSpeed = speed;
        AttackCooldown = cooldown;
        TimeSinceLastRangedAttack = cooldown;
        ProjectileLifetime = lifetime;
        AmmoCount = 20;
        InfiniteAmmo = false;
    }

    /// <summary>
    /// Check if ranged attack is ready
    /// </summary>
    public bool CanAttack()
    {
        return TimeSinceLastRangedAttack >= AttackCooldown && (InfiniteAmmo || AmmoCount > 0);
    }

    /// <summary>
    /// Consume ammo for an attack
    /// </summary>
    public bool ConsumeAmmo()
    {
        if (InfiniteAmmo) return true;
        if (AmmoCount <= 0) return false;
        AmmoCount--;
        return true;
    }
}
