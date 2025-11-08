namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that tracks the lighting level at a tile's position
/// Used for fog of war and darkness visualization
/// </summary>
public class LightingComponent : IComponent
{
    /// <summary>
    /// The current light level at this position (0.0 = pitch black, 1.0 = full brightness)
    /// </summary>
    public float LightLevel { get; set; }
    
    /// <summary>
    /// Whether this tile has been explored/discovered by the player
    /// Once explored, tiles remain visible but dimmed when not lit
    /// </summary>
    public bool IsExplored { get; set; }
    
    /// <summary>
    /// Whether this tile is currently visible to the player
    /// (within light radius and line of sight)
    /// </summary>
    public bool IsCurrentlyVisible { get; set; }
    
    public LightingComponent(float lightLevel = 0f)
    {
        LightLevel = Math.Clamp(lightLevel, 0f, 1f);
        IsExplored = false;
        IsCurrentlyVisible = false;
    }
}
