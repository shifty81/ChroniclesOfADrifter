namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component representing a parallax layer for creating depth illusion in 2D scenes
/// </summary>
public class ParallaxLayerComponent : IComponent
{
    /// <summary>
    /// Parallax factor - controls how fast this layer moves relative to camera
    /// 0.0 = static (no movement)
    /// < 1.0 = moves slower than camera (background)
    /// 1.0 = moves with camera (gameplay layer)
    /// > 1.0 = moves faster than camera (foreground)
    /// </summary>
    public float ParallaxFactor { get; set; } = 1.0f;
    
    /// <summary>
    /// Automatic horizontal scroll speed (for things like clouds)
    /// </summary>
    public float AutoScrollX { get; set; } = 0.0f;
    
    /// <summary>
    /// Automatic vertical scroll speed
    /// </summary>
    public float AutoScrollY { get; set; } = 0.0f;
    
    /// <summary>
    /// Rendering order (lower values rendered first/behind)
    /// </summary>
    public int ZOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this layer is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// Layer name for debugging
    /// </summary>
    public string Name { get; set; } = "Unnamed Layer";
    
    /// <summary>
    /// Accumulated time for auto-scroll calculations
    /// </summary>
    public float AccumulatedTime { get; set; } = 0.0f;
    
    public ParallaxLayerComponent(string name, float parallaxFactor, int zOrder)
    {
        Name = name;
        ParallaxFactor = parallaxFactor;
        ZOrder = zOrder;
    }
}
