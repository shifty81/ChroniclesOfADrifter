namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Category of an achievement
/// </summary>
public enum AchievementCategory
{
    Combat,
    Exploration,
    Crafting,
    Farming,
    Social,
    Survival,
    Collection,
    Progression
}

/// <summary>
/// Represents a single achievement definition
/// </summary>
public class AchievementDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public AchievementCategory Category { get; set; }
    public int RequiredCount { get; set; }
    public int XPReward { get; set; }

    public AchievementDefinition(string id, string name, string description,
        AchievementCategory category, int requiredCount, int xpReward)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        RequiredCount = requiredCount;
        XPReward = xpReward;
    }
}

/// <summary>
/// Progress record for a single achievement
/// </summary>
public class AchievementProgress
{
    public string AchievementId { get; set; }
    public int CurrentCount { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }

    public AchievementProgress(string achievementId)
    {
        AchievementId = achievementId;
        CurrentCount = 0;
        IsUnlocked = false;
        UnlockedAt = null;
    }

    public bool Increment(int amount, int required)
    {
        if (IsUnlocked) return false;
        CurrentCount += amount;
        if (CurrentCount >= required)
        {
            CurrentCount = required;
            IsUnlocked = true;
            UnlockedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }
}

/// <summary>
/// Component that tracks player achievements and progress
/// </summary>
public class AchievementComponent : IComponent
{
    public Dictionary<string, AchievementProgress> Progress { get; private set; }
    public List<string> RecentlyUnlocked { get; private set; }
    public int TotalXPEarned { get; private set; }

    // Stat counters
    public int EnemiesDefeated { get; set; }
    public int BossesDefeated { get; set; }
    public int CraftsCompleted { get; set; }
    public int CropsHarvested { get; set; }
    public int ChunksExplored { get; set; }
    public int NPCsSpokenTo { get; set; }
    public int DeathCount { get; set; }
    public int ItemsCollected { get; set; }

    public AchievementComponent()
    {
        Progress = new Dictionary<string, AchievementProgress>();
        RecentlyUnlocked = new List<string>();
        TotalXPEarned = 0;
    }

    public void InitializeAchievement(AchievementDefinition def)
    {
        if (!Progress.ContainsKey(def.Id))
            Progress[def.Id] = new AchievementProgress(def.Id);
    }

    /// <summary>
    /// Increment progress for an achievement; returns true if newly unlocked
    /// </summary>
    public bool IncrementProgress(string achievementId, int amount, int required)
    {
        if (!Progress.TryGetValue(achievementId, out var prog)) return false;
        bool unlocked = prog.Increment(amount, required);
        if (unlocked)
            RecentlyUnlocked.Add(achievementId);
        return unlocked;
    }

    public void AddXPEarned(int amount)
    {
        TotalXPEarned += amount;
    }

    public void ClearRecentlyUnlocked()
    {
        RecentlyUnlocked.Clear();
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var p in Progress.Values)
            if (p.IsUnlocked) count++;
        return count;
    }
}
