namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// NPC role determines their behavior and interactions
/// </summary>
public enum NPCRole
{
    Villager,      // Generic townsperson
    Merchant,      // Sells items
    Questgiver,    // Provides quests
    Farmer,        // Works on farms
    Guard,         // Town guard
    Innkeeper,     // Runs an inn
    Blacksmith,    // Crafts and sells tools/weapons
    Healer         // Restores health
}

/// <summary>
/// Schedule entry for NPC daily routine
/// </summary>
public class ScheduleEntry
{
    public float StartHour { get; set; }
    public float EndHour { get; set; }
    public float LocationX { get; set; }
    public float LocationY { get; set; }
    public string Activity { get; set; }
    
    public ScheduleEntry(float startHour, float endHour, float x, float y, string activity)
    {
        StartHour = startHour;
        EndHour = endHour;
        LocationX = x;
        LocationY = y;
        Activity = activity;
    }
    
    public bool IsActiveAt(float currentHour)
    {
        return currentHour >= StartHour && currentHour < EndHour;
    }
}

/// <summary>
/// Component representing a non-player character with daily routines
/// </summary>
public class NPCComponent : IComponent
{
    public string Name { get; set; }
    public NPCRole Role { get; set; }
    public List<ScheduleEntry> Schedule { get; private set; }
    public string CurrentActivity { get; set; }
    
    // Dialogue
    public List<string> GreetingDialogue { get; private set; }
    public List<string> RandomDialogue { get; private set; }
    
    // Shop inventory (for merchants)
    public Dictionary<TileType, int> ShopInventory { get; private set; }
    public Dictionary<TileType, int> ShopPrices { get; private set; }
    
    // Quest offerings (for questgivers)
    public List<Quest> AvailableQuests { get; private set; }
    
    public NPCComponent(string name, NPCRole role)
    {
        Name = name;
        Role = role;
        Schedule = new List<ScheduleEntry>();
        GreetingDialogue = new List<string>();
        RandomDialogue = new List<string>();
        ShopInventory = new Dictionary<TileType, int>();
        ShopPrices = new Dictionary<TileType, int>();
        AvailableQuests = new List<Quest>();
        CurrentActivity = "idle";
    }
    
    /// <summary>
    /// Get the scheduled activity for a given time
    /// </summary>
    public ScheduleEntry? GetScheduleAt(float currentHour)
    {
        return Schedule.FirstOrDefault(s => s.IsActiveAt(currentHour));
    }
    
    /// <summary>
    /// Add a schedule entry
    /// </summary>
    public void AddSchedule(float startHour, float endHour, float x, float y, string activity)
    {
        Schedule.Add(new ScheduleEntry(startHour, endHour, x, y, activity));
    }
    
    /// <summary>
    /// Add dialogue options
    /// </summary>
    public void AddGreeting(string dialogue)
    {
        GreetingDialogue.Add(dialogue);
    }
    
    public void AddDialogue(string dialogue)
    {
        RandomDialogue.Add(dialogue);
    }
    
    /// <summary>
    /// Get a random greeting
    /// </summary>
    public string GetGreeting()
    {
        if (GreetingDialogue.Count == 0)
            return $"Hello! I'm {Name}.";
            
        return GreetingDialogue[Random.Shared.Next(GreetingDialogue.Count)];
    }
    
    /// <summary>
    /// Get random dialogue
    /// </summary>
    public string GetRandomDialogue()
    {
        if (RandomDialogue.Count == 0)
            return "It's a nice day, isn't it?";
            
        return RandomDialogue[Random.Shared.Next(RandomDialogue.Count)];
    }
}
