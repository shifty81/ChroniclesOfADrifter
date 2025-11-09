namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Quest type determines the nature of the quest
/// </summary>
public enum QuestType
{
    Combat,        // Defeat enemies or bosses
    Gathering,     // Collect resources or items
    Delivery,      // Deliver items to NPCs
    Social,        // Build relationships or help NPCs
    Exploration,   // Discover locations or dungeons
    Farming,       // Grow or harvest crops
    Crafting,      // Create specific items
    Story          // Main story progression
}

/// <summary>
/// Quest status tracking
/// </summary>
public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed
}

/// <summary>
/// Individual quest data
/// </summary>
public class Quest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public QuestType Type { get; set; }
    public QuestStatus Status { get; set; }
    
    // Progress tracking
    public int CurrentProgress { get; set; }
    public int RequiredProgress { get; set; }
    
    // Rewards
    public int GoldReward { get; set; }
    public int ExperienceReward { get; set; }
    public Dictionary<TileType, int> ItemRewards { get; set; }
    public string? UnlockAbility { get; set; }
    public string? UnlockArea { get; set; }
    
    // Requirements
    public string? RequiredAbility { get; set; }
    public int RequiredLevel { get; set; }
    
    public Quest(string id, string name, string description, QuestType type)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Status = QuestStatus.NotStarted;
        CurrentProgress = 0;
        RequiredProgress = 1;
        ItemRewards = new Dictionary<TileType, int>();
    }
    
    /// <summary>
    /// Check if quest is complete
    /// </summary>
    public bool IsComplete()
    {
        return CurrentProgress >= RequiredProgress;
    }
    
    /// <summary>
    /// Update quest progress
    /// </summary>
    public void UpdateProgress(int amount = 1)
    {
        CurrentProgress = Math.Min(CurrentProgress + amount, RequiredProgress);
        
        if (IsComplete() && Status == QuestStatus.Active)
        {
            Status = QuestStatus.Completed;
        }
    }
}

/// <summary>
/// Component for tracking quests on an entity (typically player)
/// </summary>
public class QuestComponent : IComponent
{
    public List<Quest> ActiveQuests { get; private set; }
    public List<Quest> CompletedQuests { get; private set; }
    public int MaxActiveQuests { get; set; } = 10;
    
    public QuestComponent()
    {
        ActiveQuests = new List<Quest>();
        CompletedQuests = new List<Quest>();
    }
    
    /// <summary>
    /// Accept a new quest
    /// </summary>
    public bool AcceptQuest(Quest quest)
    {
        if (ActiveQuests.Count >= MaxActiveQuests)
            return false;
            
        if (ActiveQuests.Any(q => q.Id == quest.Id) || CompletedQuests.Any(q => q.Id == quest.Id))
            return false;
            
        quest.Status = QuestStatus.Active;
        ActiveQuests.Add(quest);
        return true;
    }
    
    /// <summary>
    /// Complete and claim rewards for a quest
    /// </summary>
    public Quest? CompleteQuest(string questId)
    {
        var quest = ActiveQuests.FirstOrDefault(q => q.Id == questId && q.IsComplete());
        if (quest != null)
        {
            ActiveQuests.Remove(quest);
            CompletedQuests.Add(quest);
            return quest;
        }
        return null;
    }
    
    /// <summary>
    /// Get a quest by ID
    /// </summary>
    public Quest? GetQuest(string questId)
    {
        return ActiveQuests.FirstOrDefault(q => q.Id == questId) 
            ?? CompletedQuests.FirstOrDefault(q => q.Id == questId);
    }
    
    /// <summary>
    /// Check if a quest is completed
    /// </summary>
    public bool HasCompletedQuest(string questId)
    {
        return CompletedQuests.Any(q => q.Id == questId);
    }
}
