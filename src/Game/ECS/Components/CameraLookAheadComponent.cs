namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that enables camera look-ahead behavior based on target movement
/// </summary>
public class CameraLookAheadComponent : IComponent
{
    /// <summary>
    /// Distance in world units to look ahead
    /// </summary>
    public float LookAheadDistance { get; set; } = 100.0f;
    
    /// <summary>
    /// Speed at which the look-ahead offset adjusts
    /// </summary>
    public float LookAheadSpeed { get; set; } = 3.0f;
    
    /// <summary>
    /// Current look-ahead offset X
    /// </summary>
    public float CurrentOffsetX { get; set; } = 0.0f;
    
    /// <summary>
    /// Current look-ahead offset Y
    /// </summary>
    public float CurrentOffsetY { get; set; } = 0.0f;
    
    /// <summary>
    /// Whether look-ahead is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Minimum velocity threshold to activate look-ahead
    /// </summary>
    public float MinVelocityThreshold { get; set; } = 0.1f;
    
    /// <summary>
    /// Scale factor for the look-ahead offset (for subtle effect)
    /// </summary>
    public float OffsetScale { get; set; } = 0.1f;
    
    public CameraLookAheadComponent(float lookAheadDistance = 100.0f, float lookAheadSpeed = 3.0f)
    {
        LookAheadDistance = lookAheadDistance;
        LookAheadSpeed = lookAheadSpeed;
    }
}
