namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Special ability that can be unlocked
/// </summary>
public enum AbilityType
{
    // Movement abilities
    Dash,
    Swim,
    WallClimb,
    DoubleJump,
    Glide,
    
    // Combat abilities
    SwordSpin,
    ShieldBash,
    BowCharge,
    MagicBolt,
    
    // Utility abilities
    Hookshot,
    BombCraft,
    TorchLight,
    WaterBreathing,
    
    // Exploration abilities
    RevealSecrets,
    ReadAncientText,
    OpenLockedDoors,
    MineHardRocks
}

/// <summary>
/// Individual ability data
/// </summary>
public class Ability
{
    public AbilityType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsUnlocked { get; set; }
    public float Cooldown { get; set; }
    public float LastUsedTime { get; set; }
    public int EnergyCost { get; set; }
    
    // Area unlocks - which areas this ability grants access to
    public List<string> UnlockedAreas { get; set; }
    
    public Ability(AbilityType type, string name, string description, float cooldown = 1.0f, int energyCost = 0)
    {
        Type = type;
        Name = name;
        Description = description;
        IsUnlocked = false;
        Cooldown = cooldown;
        LastUsedTime = 0;
        EnergyCost = energyCost;
        UnlockedAreas = new List<string>();
    }
    
    /// <summary>
    /// Check if ability is ready to use
    /// </summary>
    public bool IsReady(float currentTime)
    {
        return IsUnlocked && (currentTime - LastUsedTime >= Cooldown);
    }
    
    /// <summary>
    /// Use the ability
    /// </summary>
    public bool Use(float currentTime)
    {
        if (!IsReady(currentTime))
            return false;
            
        LastUsedTime = currentTime;
        return true;
    }
}

/// <summary>
/// Component tracking player's unlocked abilities
/// </summary>
public class AbilityComponent : IComponent
{
    private Dictionary<AbilityType, Ability> abilities;
    public int CurrentEnergy { get; set; }
    public int MaxEnergy { get; set; }
    
    public AbilityComponent(int maxEnergy = 100)
    {
        abilities = new Dictionary<AbilityType, Ability>();
        MaxEnergy = maxEnergy;
        CurrentEnergy = maxEnergy;
        InitializeAbilities();
    }
    
    /// <summary>
    /// Initialize all possible abilities
    /// </summary>
    private void InitializeAbilities()
    {
        // Movement abilities
        abilities[AbilityType.Dash] = new Ability(
            AbilityType.Dash,
            "Dash",
            "Quickly dash forward to evade danger",
            cooldown: 2.0f,
            energyCost: 10
        );
        
        abilities[AbilityType.Swim] = new Ability(
            AbilityType.Swim,
            "Swim",
            "Swim through water without drowning",
            cooldown: 0f,
            energyCost: 0
        );
        
        // Combat abilities
        abilities[AbilityType.SwordSpin] = new Ability(
            AbilityType.SwordSpin,
            "Sword Spin",
            "Spin attack hitting all nearby enemies",
            cooldown: 3.0f,
            energyCost: 15
        );
        
        // Utility abilities
        abilities[AbilityType.Hookshot] = new Ability(
            AbilityType.Hookshot,
            "Hookshot",
            "Grapple to distant objects and pull yourself forward",
            cooldown: 1.0f,
            energyCost: 5
        );
        abilities[AbilityType.Hookshot].UnlockedAreas.Add("canyon_area");
        abilities[AbilityType.Hookshot].UnlockedAreas.Add("cliff_dungeon");
        
        abilities[AbilityType.BombCraft] = new Ability(
            AbilityType.BombCraft,
            "Bomb Crafting",
            "Craft and use bombs to destroy obstacles",
            cooldown: 0.5f,
            energyCost: 20
        );
        abilities[AbilityType.BombCraft].UnlockedAreas.Add("blocked_cave");
        
        // Exploration abilities
        abilities[AbilityType.RevealSecrets] = new Ability(
            AbilityType.RevealSecrets,
            "Reveal Secrets",
            "Reveal hidden paths and secret doors",
            cooldown: 5.0f,
            energyCost: 25
        );
        
        abilities[AbilityType.MineHardRocks] = new Ability(
            AbilityType.MineHardRocks,
            "Advanced Mining",
            "Mine through the hardest rocks and ores",
            cooldown: 0f,
            energyCost: 0
        );
        abilities[AbilityType.MineHardRocks].UnlockedAreas.Add("deep_caverns");
    }
    
    /// <summary>
    /// Unlock an ability
    /// </summary>
    public bool UnlockAbility(AbilityType type)
    {
        if (abilities.TryGetValue(type, out var ability))
        {
            ability.IsUnlocked = true;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Check if an ability is unlocked
    /// </summary>
    public bool HasAbility(AbilityType type)
    {
        return abilities.TryGetValue(type, out var ability) && ability.IsUnlocked;
    }
    
    /// <summary>
    /// Use an ability
    /// </summary>
    public bool UseAbility(AbilityType type, float currentTime)
    {
        if (!abilities.TryGetValue(type, out var ability))
            return false;
            
        if (!ability.IsReady(currentTime))
            return false;
            
        if (CurrentEnergy < ability.EnergyCost)
            return false;
            
        if (ability.Use(currentTime))
        {
            CurrentEnergy -= ability.EnergyCost;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get all unlocked abilities
    /// </summary>
    public IEnumerable<Ability> GetUnlockedAbilities()
    {
        return abilities.Values.Where(a => a.IsUnlocked);
    }
    
    /// <summary>
    /// Get all areas the player can access with current abilities
    /// </summary>
    public HashSet<string> GetAccessibleAreas()
    {
        var areas = new HashSet<string>();
        foreach (var ability in abilities.Values.Where(a => a.IsUnlocked))
        {
            foreach (var area in ability.UnlockedAreas)
            {
                areas.Add(area);
            }
        }
        return areas;
    }
    
    /// <summary>
    /// Restore energy over time
    /// </summary>
    public void RestoreEnergy(int amount)
    {
        CurrentEnergy = Math.Min(CurrentEnergy + amount, MaxEnergy);
    }
}
