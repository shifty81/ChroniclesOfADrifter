namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component tracking player experience, level, and stat progression
/// </summary>
public class ExperienceComponent : IComponent
{
    /// <summary>
    /// Current experience points
    /// </summary>
    public int CurrentXP { get; set; }
    
    /// <summary>
    /// Current player level
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Maximum level
    /// </summary>
    public int MaxLevel { get; set; } = 50;
    
    /// <summary>
    /// Total XP earned across all levels
    /// </summary>
    public int TotalXPEarned { get; set; }
    
    // Stat bonuses from leveling
    
    /// <summary>
    /// Bonus attack damage from levels
    /// </summary>
    public float AttackBonus { get; set; }
    
    /// <summary>
    /// Bonus defense (damage reduction) from levels
    /// </summary>
    public float DefenseBonus { get; set; }
    
    /// <summary>
    /// Bonus max health from levels
    /// </summary>
    public float MaxHealthBonus { get; set; }
    
    /// <summary>
    /// Bonus speed from levels
    /// </summary>
    public float SpeedBonus { get; set; }
    
    /// <summary>
    /// Stat points available to spend (not used yet, reserved for future)
    /// </summary>
    public int StatPointsAvailable { get; set; }
    
    public ExperienceComponent()
    {
    }
    
    /// <summary>
    /// Gets the XP required to reach the next level.
    /// Uses a scaling formula: 50 * level^1.5
    /// </summary>
    public int GetXPForNextLevel()
    {
        if (Level >= MaxLevel) return int.MaxValue;
        return (int)(50 * Math.Pow(Level, 1.5));
    }
    
    /// <summary>
    /// Gets the XP required for a specific level
    /// </summary>
    public int GetXPForLevel(int level)
    {
        if (level <= 1) return 0;
        return (int)(50 * Math.Pow(level - 1, 1.5));
    }
    
    /// <summary>
    /// Gets the progress toward the next level as a percentage (0-1)
    /// </summary>
    public float GetLevelProgress()
    {
        if (Level >= MaxLevel) return 1.0f;
        int xpNeeded = GetXPForNextLevel();
        if (xpNeeded <= 0) return 1.0f;
        return Math.Min(1.0f, (float)CurrentXP / xpNeeded);
    }
    
    /// <summary>
    /// Adds XP and returns the number of levels gained
    /// </summary>
    public int AddXP(int amount)
    {
        if (amount <= 0 || Level >= MaxLevel) return 0;
        
        CurrentXP += amount;
        TotalXPEarned += amount;
        
        int levelsGained = 0;
        
        while (Level < MaxLevel && CurrentXP >= GetXPForNextLevel())
        {
            CurrentXP -= GetXPForNextLevel();
            Level++;
            levelsGained++;
            ApplyLevelUpBonuses();
        }
        
        return levelsGained;
    }
    
    /// <summary>
    /// Applies stat bonuses when leveling up
    /// </summary>
    private void ApplyLevelUpBonuses()
    {
        // +2 attack per level
        AttackBonus += 2f;
        
        // +1 defense per level
        DefenseBonus += 1f;
        
        // +10 max health per level
        MaxHealthBonus += 10f;
        
        // +5 speed every 5 levels
        if (Level % 5 == 0)
        {
            SpeedBonus += 5f;
        }
        
        // +1 stat point per level
        StatPointsAvailable++;
    }
}
