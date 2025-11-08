namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of tools used for mining/gathering
/// </summary>
public enum ToolType
{
    None,       // Bare hands
    Pickaxe,    // For mining stone and ores
    Axe,        // For chopping trees
    Shovel,     // For digging dirt and sand
}

/// <summary>
/// Tool material quality levels
/// </summary>
public enum ToolMaterial
{
    None = 0,       // Bare hands
    Wood = 1,       // Basic wooden tools
    Stone = 2,      // Stone tools
    Iron = 3,       // Iron tools
    Steel = 4,      // Steel tools (advanced)
}

/// <summary>
/// Component that tracks the currently equipped tool
/// </summary>
public class ToolComponent : ChroniclesOfADrifter.ECS.IComponent
{
    public ToolType Type { get; set; }
    public ToolMaterial Material { get; set; }
    
    /// <summary>
    /// Mining power - higher values mine faster
    /// </summary>
    public float MiningPower => Material switch
    {
        ToolMaterial.None => 0.5f,
        ToolMaterial.Wood => 1.0f,
        ToolMaterial.Stone => 1.5f,
        ToolMaterial.Iron => 2.5f,
        ToolMaterial.Steel => 4.0f,
        _ => 0.5f
    };
    
    public ToolComponent(ToolType type, ToolMaterial material)
    {
        Type = type;
        Material = material;
    }
    
    /// <summary>
    /// Creates a bare hands tool (no tool equipped)
    /// </summary>
    public static ToolComponent BareHands()
    {
        return new ToolComponent(ToolType.None, ToolMaterial.None);
    }
}
