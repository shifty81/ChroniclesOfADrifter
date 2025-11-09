namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Boss enemy type with unique identifiers
/// </summary>
public enum BossType
{
    ForestGuardian,
    DesertWarlord,
    IceTitan,
    ShadowKing,
    DragonLord,
    AncientGolem,
    SwampBeast,
    VolcanoLord
}

/// <summary>
/// Boss phase - some bosses change behavior at health thresholds
/// </summary>
public enum BossPhase
{
    Phase1,
    Phase2,
    Phase3
}

/// <summary>
/// Component marking an entity as a boss enemy
/// </summary>
public class BossComponent : IComponent
{
    public BossType Type { get; set; }
    public string BossName { get; set; }
    public BossPhase CurrentPhase { get; set; }
    public bool IsDefeated { get; set; }
    
    // Phase transition thresholds (percentage of health)
    public float Phase2HealthThreshold { get; set; } = 0.66f;
    public float Phase3HealthThreshold { get; set; } = 0.33f;
    
    // Rewards on defeat
    public int GoldReward { get; set; }
    public int ExperienceReward { get; set; }
    public Dictionary<TileType, int> ItemDrops { get; private set; }
    public AbilityType? AbilityReward { get; set; }
    public string? UnlockArea { get; set; }
    
    // Boss arena
    public float ArenaX { get; set; }
    public float ArenaY { get; set; }
    public float ArenaWidth { get; set; }
    public float ArenaHeight { get; set; }
    
    public BossComponent(BossType type, string name)
    {
        Type = type;
        BossName = name;
        CurrentPhase = BossPhase.Phase1;
        IsDefeated = false;
        ItemDrops = new Dictionary<TileType, int>();
    }
    
    /// <summary>
    /// Update boss phase based on health percentage
    /// </summary>
    public void UpdatePhase(float healthPercentage)
    {
        if (healthPercentage <= Phase3HealthThreshold && CurrentPhase != BossPhase.Phase3)
        {
            CurrentPhase = BossPhase.Phase3;
        }
        else if (healthPercentage <= Phase2HealthThreshold && CurrentPhase == BossPhase.Phase1)
        {
            CurrentPhase = BossPhase.Phase2;
        }
    }
    
    /// <summary>
    /// Mark boss as defeated
    /// </summary>
    public void Defeat()
    {
        IsDefeated = true;
    }
    
    /// <summary>
    /// Check if entity is in boss arena
    /// </summary>
    public bool IsInArena(float x, float y)
    {
        return x >= ArenaX && x <= ArenaX + ArenaWidth &&
               y >= ArenaY && y <= ArenaY + ArenaHeight;
    }
}
