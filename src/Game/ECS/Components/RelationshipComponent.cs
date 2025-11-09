namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Relationship level with NPCs
/// </summary>
public enum RelationshipLevel
{
    Stranger = 0,      // 0-99 points
    Acquaintance = 1,  // 100-249 points
    Friend = 2,        // 250-499 points
    GoodFriend = 3,    // 500-999 points
    BestFriend = 4     // 1000+ points
}

/// <summary>
/// Individual relationship with one NPC
/// </summary>
public class Relationship
{
    public string NPCName { get; set; }
    public int Points { get; set; }
    public RelationshipLevel Level { get; private set; }
    public int GiftsGivenToday { get; set; }
    public DateTime LastInteraction { get; set; }
    
    public Relationship(string npcName)
    {
        NPCName = npcName;
        Points = 0;
        Level = RelationshipLevel.Stranger;
        GiftsGivenToday = 0;
        LastInteraction = DateTime.MinValue;
    }
    
    /// <summary>
    /// Add relationship points and update level
    /// </summary>
    public void AddPoints(int amount)
    {
        Points += amount;
        Points = Math.Max(0, Points); // Can't go negative
        UpdateLevel();
    }
    
    /// <summary>
    /// Update relationship level based on points
    /// </summary>
    private void UpdateLevel()
    {
        if (Points >= 1000)
            Level = RelationshipLevel.BestFriend;
        else if (Points >= 500)
            Level = RelationshipLevel.GoodFriend;
        else if (Points >= 250)
            Level = RelationshipLevel.Friend;
        else if (Points >= 100)
            Level = RelationshipLevel.Acquaintance;
        else
            Level = RelationshipLevel.Stranger;
    }
    
    /// <summary>
    /// Reset daily gift counter
    /// </summary>
    public void ResetDailyGifts()
    {
        GiftsGivenToday = 0;
    }
}

/// <summary>
/// Component tracking relationships with all NPCs
/// </summary>
public class RelationshipComponent : IComponent
{
    private Dictionary<string, Relationship> relationships;
    public int MaxGiftsPerDay { get; set; } = 2;
    
    public RelationshipComponent()
    {
        relationships = new Dictionary<string, Relationship>();
    }
    
    /// <summary>
    /// Get or create relationship with an NPC
    /// </summary>
    public Relationship GetRelationship(string npcName)
    {
        if (!relationships.ContainsKey(npcName))
        {
            relationships[npcName] = new Relationship(npcName);
        }
        return relationships[npcName];
    }
    
    /// <summary>
    /// Give a gift to an NPC
    /// </summary>
    public bool GiveGift(string npcName, TileType item, int value)
    {
        var relationship = GetRelationship(npcName);
        
        if (relationship.GiftsGivenToday >= MaxGiftsPerDay)
            return false;
            
        relationship.AddPoints(value);
        relationship.GiftsGivenToday++;
        relationship.LastInteraction = DateTime.Now;
        return true;
    }
    
    /// <summary>
    /// Interact with an NPC (talk, etc.)
    /// </summary>
    public void Interact(string npcName, int points = 5)
    {
        var relationship = GetRelationship(npcName);
        relationship.AddPoints(points);
        relationship.LastInteraction = DateTime.Now;
    }
    
    /// <summary>
    /// Get relationship level with an NPC
    /// </summary>
    public RelationshipLevel GetLevel(string npcName)
    {
        return GetRelationship(npcName).Level;
    }
    
    /// <summary>
    /// Get all relationships
    /// </summary>
    public IReadOnlyDictionary<string, Relationship> GetAllRelationships()
    {
        return relationships;
    }
    
    /// <summary>
    /// Reset daily gift counters for all NPCs
    /// </summary>
    public void ResetDailyGifts()
    {
        foreach (var relationship in relationships.Values)
        {
            relationship.ResetDailyGifts();
        }
    }
}
