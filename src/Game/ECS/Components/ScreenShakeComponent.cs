namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for screen shake effects on camera
/// </summary>
public class ScreenShakeComponent : IComponent
{
    /// <summary>
    /// Current shake intensity (0 = no shake)
    /// </summary>
    public float Intensity { get; set; } = 0f;
    
    /// <summary>
    /// How long the shake should last (seconds)
    /// </summary>
    public float Duration { get; set; } = 0f;
    
    /// <summary>
    /// How much time has elapsed
    /// </summary>
    public float ElapsedTime { get; set; } = 0f;
    
    /// <summary>
    /// Current shake offset X
    /// </summary>
    public float OffsetX { get; set; } = 0f;
    
    /// <summary>
    /// Current shake offset Y
    /// </summary>
    public float OffsetY { get; set; } = 0f;
    
    /// <summary>
    /// Shake frequency (how fast it shakes)
    /// </summary>
    public float Frequency { get; set; } = 25f;
    
    /// <summary>
    /// Whether shake is currently active
    /// </summary>
    public bool IsActive => ElapsedTime < Duration && Intensity > 0f;
    
    /// <summary>
    /// Trigger a new screen shake effect
    /// </summary>
    public void Trigger(float intensity, float duration, float frequency = 25f)
    {
        Intensity = intensity;
        Duration = duration;
        Frequency = frequency;
        ElapsedTime = 0f;
    }
    
    /// <summary>
    /// Reset the screen shake
    /// </summary>
    public void Reset()
    {
        Intensity = 0f;
        Duration = 0f;
        ElapsedTime = 0f;
        OffsetX = 0f;
        OffsetY = 0f;
    }
}
