namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for creature entities
/// Defines creature type and behavior characteristics
/// </summary>
public class CreatureComponent : IComponent
{
    public CreatureType Type { get; set; }
    public string CreatureName { get; set; }
    public float AgroRange { get; set; }  // Detection range for hostiles
    public bool IsHostile { get; set; }
    public int ExperienceValue { get; set; }  // XP given when defeated
    
    public CreatureComponent(CreatureType type, string name, bool isHostile = false, float agroRange = 200f, int xpValue = 10)
    {
        Type = type;
        CreatureName = name;
        IsHostile = isHostile;
        AgroRange = agroRange;
        ExperienceValue = xpValue;
    }
}

/// <summary>
/// Types of creatures that can spawn in the world
/// </summary>
public enum CreatureType
{
    // Passive surface creatures
    Rabbit,
    Deer,
    Bird,
    
    // Hostile surface creatures
    Goblin,
    Bandit,
    Wolf,
    
    // Underground creatures
    CaveBat,
    CaveSpider,
    Rat,
    
    // Deep underground creatures
    Skeleton,
    Zombie,
    IceElemental,
    
    // Boss creatures
    GoblinChief,
    Yeti,
    DragonBoss
}
