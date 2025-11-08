namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that represents a light source (torch, player light, etc.)
/// Emits light in a radius to illuminate dark areas
/// </summary>
public class LightSourceComponent : IComponent
{
    /// <summary>
    /// The radius of light emitted by this source (in blocks)
    /// </summary>
    public float Radius { get; set; }
    
    /// <summary>
    /// The intensity of the light (0.0 to 1.0)
    /// 1.0 = full brightness, 0.0 = no light
    /// </summary>
    public float Intensity { get; set; }
    
    /// <summary>
    /// Whether this light source is currently active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// The type of light source
    /// </summary>
    public LightSourceType Type { get; set; }
    
    public LightSourceComponent(float radius, float intensity, LightSourceType type = LightSourceType.Torch)
    {
        Radius = radius;
        Intensity = Math.Clamp(intensity, 0f, 1f);
        IsActive = true;
        Type = type;
    }
}

/// <summary>
/// Types of light sources
/// </summary>
public enum LightSourceType
{
    /// <summary>
    /// Player's personal light (lantern/headlamp)
    /// </summary>
    Player,
    
    /// <summary>
    /// Placed torch on wall/ground
    /// </summary>
    Torch,
    
    /// <summary>
    /// Glowing ore or crystal
    /// </summary>
    GlowingOre,
    
    /// <summary>
    /// Ambient light on surface
    /// </summary>
    Sunlight
}
