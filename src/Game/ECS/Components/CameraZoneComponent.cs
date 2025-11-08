namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component defining a camera zone - an area with specific camera behavior
/// </summary>
public class CameraZoneComponent : IComponent
{
    /// <summary>
    /// Zone name for debugging
    /// </summary>
    public string Name { get; set; } = "Unnamed Zone";
    
    /// <summary>
    /// Zone bounds - minimum X
    /// </summary>
    public float MinX { get; set; }
    
    /// <summary>
    /// Zone bounds - maximum X
    /// </summary>
    public float MaxX { get; set; }
    
    /// <summary>
    /// Zone bounds - minimum Y
    /// </summary>
    public float MinY { get; set; }
    
    /// <summary>
    /// Zone bounds - maximum Y
    /// </summary>
    public float MaxY { get; set; }
    
    /// <summary>
    /// Camera follow speed in this zone
    /// </summary>
    public float FollowSpeed { get; set; } = 5.0f;
    
    /// <summary>
    /// Camera zoom level in this zone
    /// </summary>
    public float Zoom { get; set; } = 1.0f;
    
    /// <summary>
    /// Whether to enable camera look-ahead in this zone
    /// </summary>
    public bool EnableLookAhead { get; set; } = true;
    
    /// <summary>
    /// Look-ahead distance in this zone
    /// </summary>
    public float LookAheadDistance { get; set; } = 100.0f;
    
    /// <summary>
    /// Priority (higher priority zones override lower ones if overlapping)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Whether this zone is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Transition speed when entering/exiting zone (0 = instant, higher = smoother)
    /// </summary>
    public float TransitionSpeed { get; set; } = 2.0f;
    
    public CameraZoneComponent(string name, float minX, float maxX, float minY, float maxY)
    {
        Name = name;
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
    }
    
    /// <summary>
    /// Check if a point is inside this zone
    /// </summary>
    public bool ContainsPoint(float x, float y)
    {
        return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
    }
}
